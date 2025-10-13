# Software Architecture

## System Overview

The Mouse VR system consists of two main software components that communicate via serial protocol:

1. **Arduino Firmware**: Low-level hardware control and sensor polling
2. **Unity Application**: VR environment rendering, game logic, and data logging

## Arduino Firmware Architecture

### Main Components

```cpp
// Data Structures
struct MotionAndLickData {
  int32_t x_1, y_1;      // Sensor 1 motion
  int32_t x_2, y_2;      // Sensor 2 motion  
  int32_t lick_count;    // Behavioral events
  int32_t dt;            // Time delta
};

struct MD {
  byte motion;           // Motion detected flag
  char dx, dy;           // Delta X/Y
  byte squal;            // Surface quality
  word shutter;          // Shutter time
  byte max_pix;          // Max pixel value
};
```

### Update Loop Flow

```
┌─────────────────────────────────┐
│       Arduino Main Loop         │
└────────────┬────────────────────┘
             │
             ├──► Read Sensor 1 (SPI)
             │    └─► Accumulate X1, Y1
             │
             ├──► Read Sensor 2 (SPI)
             │    └─► Accumulate X2, Y2
             │
             ├──► Check Lick Sensor
             │    └─► Increment lick_count
             │
             ├──► Process Serial Commands
             │    ├─► 'h': Send data packet
             │    ├─► 'w': Open water valve
             │    └─► 'a': Trigger air puff
             │
             ├──► Manage Solenoid Timers
             │    ├─► Close water after 60ms
             │    └─► Close air after 20ms
             │
             └──► Loop (repeat at ~1000 Hz)
```

### Timing Considerations

- **Sensor polling**: ~1ms per sensor (SPI at 20 MHz)
- **Serial write**: ~1ms for 24-byte packet at 115200 baud
- **Loop frequency**: ~1000 Hz (1ms period)
- **Solenoid precision**: ±1ms timing accuracy

## Unity Application Architecture

### Class Hierarchy

```
GameObject: ArduinoInterface
├── Component: ArduinoInterface
│   ├── Manages SerialPort
│   ├── Parses sensor data
│   ├── Exposes Rx, Ry, Rz rotation rates
│   └── Methods:
│       ├── deliverWater()
│       └── deliverAirpuff()
│
└── Component: EventLogger
    ├── Manages event list
    ├── Saves to CSV
    └── Methods:
        ├── Add(Event e)
        └── saveEvents()

GameObject: Player
└── Component: PlayerController
    ├── References ArduinoInterface
    ├── References EventLogger
    ├── CharacterController integration
    └── Movement calculation
```

### Unity Update Cycle

```
┌─────────────────────────────────┐
│      Unity Update() Loop        │
│         (~60-90 Hz)             │
└────────────┬────────────────────┘
             │
    ┌────────▼──────────┐
    │ ArduinoInterface  │
    │      Update()     │
    └────────┬──────────┘
             │
             ├──► Request data: Serial.Write("h")
             │
             ├──► Read response (24 bytes)
             │
             ├──► Parse into offsets array
             │
             ├──► Calculate Rx, Ry, Rz
             │    └─► Apply scaling factors
             │
             └──► Rotate camera GameObject
             
    ┌────────▼──────────┐
    │ PlayerController  │
    │      Update()     │
    └────────┬──────────┘
             │
             ├──► Get Rx, Ry, Rz from ArduinoInterface
             │
             ├──► Calculate movement vectors
             │    ├─► Forward: Rx * scale
             │    ├─► Strafe: Rz * scale
             │    └─► Rotation: Ry * scale
             │
             ├──► Apply CharacterController.Move()
             │
             ├──► Log events to EventLogger
             │    ├─► Position (30 Hz)
             │    ├─► Rotation (on change)
             │    └─► [Rx, Ry, Rz] (every frame)
             │
             └──► Check trigger conditions
                  ├─► In safe zone + stopped?
                  └─► Deliver reward
```

### Data Flow Diagram

```
┌───────────────┐
│   Trackball   │ Physical motion
└───────┬───────┘
        │
┌───────▼───────┐
│  ADNS3080 #1  │ dx1, dy1
│  ADNS3080 #2  │ dx2, dy2
└───────┬───────┘
        │ SPI (20 MHz)
┌───────▼───────┐
│  Arduino Due  │ Accumulates offsets
└───────┬───────┘
        │ Serial (115200 baud)
        │ Every ~16ms (60 Hz Unity request)
        │
┌───────▼────────────────────────┐
│     ArduinoInterface.cs        │
│  ┌──────────────────────────┐  │
│  │ read_ints(6)             │  │
│  │ [x1, y1, x2, y2, lick, dt]│ │
│  └────────────┬──────────────┘  │
│               │                 │
│  ┌────────────▼──────────────┐  │
│  │ Calculate rotation rates  │  │
│  │ Rx = (x2 * scale)         │  │
│  │ Ry = (x1 * scale)         │  │
│  │ Rz = (y2+y1)/2 * scale    │  │
│  └────────────┬──────────────┘  │
└───────────────┼─────────────────┘
                │
┌───────────────▼─────────────────┐
│     PlayerController.cs         │
│  ┌──────────────────────────┐   │
│  │ forwardMovement = Rx*dt  │   │
│  │ rotationMovement = Ry*dt │   │
│  │ strafeMovement = Rz*dt   │   │
│  └────────────┬──────────────┘  │
│               │                 │
│  ┌────────────▼──────────────┐  │
│  │ CharacterController.Move()│  │
│  └────────────┬──────────────┘  │
└───────────────┼─────────────────┘
                │
┌───────────────▼─────────────────┐
│      Unity Transform            │
│  Player position & rotation     │
│  updated in 3D world            │
└─────────────────────────────────┘
```

## Communication Protocol

### Command Structure

**Unity → Arduino:**
| Command | Byte Value | Response | Purpose |
|---------|-----------|----------|---------|
| Request data | `'h'` (0x68) | 24 bytes | Poll sensor data |
| Water reward | `'w'` (0x77) | None | Trigger reward |
| Air puff | `'a'` (0x61) | None | Trigger punishment |

**Arduino → Unity (response to 'h'):**
```
Byte Position | Data Type | Description
──────────────┼───────────┼─────────────────────
0-3           | int32_t   | x_1 (sensor 1 X)
4-7           | int32_t   | y_1 (sensor 1 Y)
8-11          | int32_t   | x_2 (sensor 2 X)
12-15         | int32_t   | y_2 (sensor 2 Y)
16-19         | int32_t   | lick_count
20-23         | int32_t   | dt (milliseconds)
```

### Error Handling

**Arduino Side:**
```cpp
// Buffer overflow protection
if (bytesToRead <= 6*4*4) {
    stream.Write("h");  // Safe to request
} else {
    Debug.Log("buffer too full");  // Skip this frame
}
```

**Unity Side:**
```csharp
// First read discarded (initialization)
if (firstOffsetRead) {
    read_ints(6);
    firstOffsetRead = false;
    return (false, empty_array);
}

// Verify sufficient data available
if (stream.BytesToRead >= 6*4) {
    int[] data = read_ints(6);
    return (true, data);
}
```

## Event Logging System

### Event Class Design

```csharp
public class Event {
    public string code;        // Event identifier
    public float time;         // Unity Time.time (seconds)
    public float[] data;       // Optional event data
    
    // Multiple constructors for convenience
    Event(string code)                        // Simple event
    Event(string code, float data)            // Single value
    Event(string code, float[] data)          // Array data
    Event(string code, int data)              // Integer conversion
}
```

### Logging Strategy

**High-frequency events (60-90 Hz):**
- `[Rx, Ry, Rz]` - Raw rotation rates (every frame)

**Medium-frequency events (30 Hz):**
- `POSITION` - Player coordinates (throttled)

**Low-frequency events (on-change):**
- `Rotated` - Euler angle changes
- `WATER` - Reward delivery
- `AIRPUFF` - Punishment delivery
- `TELEPORT` - Position reset
- `RESET` - Experiment reset

**CSV Output Format:**
```csv
code,time,data
[Rx,Ry,Rz],0.016,150.2,25.3,-10.1
POSITION,0.033,1.23,0.5,4.56
Rotated,0.789,0,45.2,0
WATER,2.345,
```

## Performance Optimization

### Arduino Optimization
- **SPI Speed**: 20 MHz (maximum for ADNS3080)
- **Integer arithmetic**: Avoids float operations
- **Minimal serial writes**: Only on request
- **No `Serial.println()`**: Removes string formatting overhead

### Unity Optimization
- **Exponential smoothing**: `smoothed_Rx = 0.1*new + 0.9*old`
- **Throttled logging**: Position logged at 30 Hz, not 60-90 Hz
- **Buffer management**: Checks `BytesToRead` before parsing
- **Direct array writes**: No string concatenation in hot path

### Latency Analysis

```
Motion Event → Unity Response: ~16-20ms total

Breakdown:
- Trackball motion occurs: T = 0ms
- Arduino reads sensors: T = 1ms (next loop iteration)
- Data accumulates: T = 1-16ms (until Unity requests)
- Serial transmission: T = +1ms (24 bytes @ 115200 baud)
- Unity parsing: T = +0.1ms
- CharacterController update: T = +0.5ms
- Render frame: T = +5-10ms (depends on scene complexity)

Total: 16-20ms (acceptable for behavioral experiments)
```

## Extensibility

### Adding New Events
```csharp
// In any Unity script with EventLogger reference:
eventLogger.Add(new Event("CUSTOM_EVENT", myDataArray));
```

### Adding New Arduino Commands
```cpp
// In Arduino loop():
if(a == 'x'){  // New command
    // Your custom action
    customFunction();
}
```

### Adding New Sensors
```cpp
// Define new pins
#define NEW_SENSOR_PIN 3

// Add to data structure
struct ExtendedData {
    // ... existing fields
    int32_t new_sensor_value;
};

// Read in loop
motionAndLickData.new_sensor_value = analogRead(NEW_SENSOR_PIN);
```

## Debugging Tools

### Arduino Debug
```cpp
// Enable serial debugging (WARNING: interferes with Unity communication)
#ifdef DEBUG_MODE
    Serial.print("X1: "); Serial.println(motionAndLickData.x_1);
#endif
```

### Unity Debug
```csharp
// Inspector monitoring
public float percent_arduino_reads_succesful;  // Should be ~0.99

// Console logging
if (data_available) {
    Debug.Log($"Motion: X={offsets[0]}, Y={offsets[1]}");
}
```

## Known Limitations

1. **Buffer overflow**: At >90 Hz Unity request rate, serial buffer fills
   - Mitigation: Throttle requests, check `bytesToRead`

2. **Sensor saturation**: Very fast trackball motion may exceed sensor range
   - Mitigation: Adjust lighting, trackball surface texture

3. **COM port conflicts**: Unity and Serial Monitor can't share port
   - Mitigation: Always close Serial Monitor before running Unity

4. **Float precision**: Event times may drift over long sessions (hours)
   - Mitigation: Use `Time.time` for relative timing, log session start time
