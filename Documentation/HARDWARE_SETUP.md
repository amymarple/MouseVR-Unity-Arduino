# Hardware Setup Guide

## Bill of Materials

### Core Components
| Component | Quantity | Purpose | Notes |
|-----------|----------|---------|-------|
| Arduino Due | 1 | Main microcontroller | 84 MHz ARM Cortex-M3 |
| ADNS3080 Optical Sensor | 2 | Motion tracking | Mounted on trackball |
| Trackball | 1 | Input device | Standard 1-inch diameter |
| Water Solenoid Valve | 1 | Reward delivery | 12V, normally closed |
| Air Puff Solenoid | 1 | Punishment delivery | 12V, normally closed |
| Lick Sensor | 1 | Behavioral input | Capacitive or IR beam-break |
| 5V/12V Relay Module | 2 | Solenoid control | Protects Arduino outputs |
| Power Supply | 1 | 12V DC | 2A minimum |
| USB Cable | 1 | Arduino-PC connection | USB Type-B |

### Optional Components
- 3D printed trackball mount
- Optical breadboard for stable mounting
- Shielded cables for sensor connections
- Barrel jack adapter for Arduino power

## Physical Assembly

### 1. Trackball Mount

The trackball should be mounted with the two ADNS3080 sensors positioned orthogonally (90° apart) to capture independent X and Y motion components.

```
        Top View:
        
         [Sensor 1]
              |
              |
    ──────────●────────── Trackball (rotates freely)
              |
              |
         [Sensor 2]
```

**Mounting Tips:**
- Distance from sensor to trackball surface: 2-3mm optimal
- Ensure sensors are level and perpendicular to trackball surface
- Use M3 screws for secure sensor mounting
- Allow trackball to spin freely without friction

### 2. Arduino Wiring Diagram

```
Arduino Due                    Components
───────────────────────────────────────────

Digital Pin 10 ──────────► ADNS3080 #1 CS
Digital Pin 11 ──────────► ADNS3080 #1 RST
MOSI ────────────────────► ADNS3080 #1 MOSI
MISO ────────────────────► ADNS3080 #1 MISO
SCK ─────────────────────► ADNS3080 #1 SCK

Digital Pin 4 ───────────► ADNS3080 #2 CS
Digital Pin 5 ───────────► ADNS3080 #2 RST
MOSI ────────────────────► ADNS3080 #2 MOSI
MISO ────────────────────► ADNS3080 #2 MISO
SCK ─────────────────────► ADNS3080 #2 SCK

Digital Pin 2 ───────────► Lick Sensor Signal
Digital Pin 7 ───────────► Air Puff Relay (IN)
Digital Pin 8 ───────────► Water Relay (IN)

GND ─────────────────────► Common Ground (all components)
5V ──────────────────────► Sensors VCC
VIN (12V) ───────────────► Relay Module VCC
```

### 3. Sensor Calibration

**ADNS3080 Setup:**
1. Focus: Sensors auto-focus, but verify 2-3mm gap to surface
2. Lighting: Works in ambient light; avoid direct overhead lights
3. Surface: Trackball should have texture/pattern for tracking
4. Testing: Use `TrackballController.ino` to verify raw data

**Expected Values:**
- Idle trackball: dx = 0, dy = 0
- Forward roll: sensor 1 shows positive displacement
- Side rotation: sensor 2 shows positive displacement

### 4. Power Considerations

**Power Budget:**
- Arduino Due: ~200mA
- 2x ADNS3080: ~50mA each
- Relay modules: ~70mA each (inactive)
- **Total: ~440mA at 5V**

**Solenoid Power:**
- Water solenoid: 12V @ 0.5A (60ms pulses)
- Air puff solenoid: 12V @ 0.3A (20ms pulses)
- **Use separate 12V supply for relays/solenoids**

### 5. Lick Sensor Options

#### Option A: Capacitive Touch Sensor
```
Arduino Pin 2 ────► Touch Sensor OUT
Touch Sensor VCC ──► 5V
Touch Sensor GND ──► GND
```
- **Pros:** No moving parts, easy to clean
- **Cons:** May need adjustment for sensitivity

#### Option B: IR Beam-Break
```
Arduino Pin 2 ────► IR Receiver OUT
IR LED (+) ────────► 5V (via 220Ω resistor)
IR Receiver VCC ───► 5V
Common GND ────────► GND
```
- **Pros:** Precise, no direct contact needed
- **Cons:** Requires alignment, sensitive to ambient IR

## Troubleshooting

### Sensor Issues
**Problem:** No motion detected
- Check SPI wiring (MOSI, MISO, SCK, CS)
- Verify sensor is powered (5V on VCC)
- Ensure 2-3mm distance from trackball surface
- Try the other sensor to isolate hardware failure

**Problem:** Erratic motion data
- Clean trackball surface (fingerprints affect tracking)
- Check for loose wiring/connections
- Verify ground is common between Arduino and sensors
- Adjust `back_scale` parameter in Unity (lower value)

### Communication Issues
**Problem:** Unity can't connect to Arduino
- Check COM port number (Device Manager on Windows)
- Verify Arduino is powered and programmed
- Close Serial Monitor (conflicts with Unity serial access)
- Try different USB cable/port

**Problem:** Data corruption/garbled
- Verify baud rate (115200) matches in both systems
- Check USB cable quality (use shielded cable if possible)
- Reduce cable length if >2 meters

### Solenoid Issues
**Problem:** Solenoids not activating
- Verify relay module is powered (12V on VCC)
- Check relay indicator LED lights when triggered
- Measure voltage at solenoid terminals (should be 12V)
- Ensure solenoid is connected to relay NC→COM or NO→COM appropriately

**Problem:** Weak water flow
- Increase `WATER_OPEN_TIME` in Arduino code
- Check water reservoir level
- Verify solenoid is normally-closed type
- Check for air bubbles in water line

## Safety Notes

⚠️ **Important Safety Considerations:**

1. **Electrical Safety:**
   - Never connect/disconnect components while powered
   - Use proper gauge wire for solenoid power (16-18 AWG)
   - Insulate all exposed connections

2. **Animal Safety:**
   - Water reward: Use distilled or filtered water
   - Air puff: Limit to 20-25 PSI, verify with regulator
   - Test all stimuli on yourself first
   - Monitor animal welfare during experiments

3. **Equipment Protection:**
   - Use relay modules to protect Arduino from solenoid back-EMF
   - Add flyback diodes across solenoids if using transistor drivers
   - Keep water away from electronics (use drip-proof enclosure)

## Maintenance

**Weekly:**
- Clean trackball with alcohol wipe
- Check sensor alignment
- Test solenoid function

**Monthly:**
- Inspect all cable connections
- Clean water delivery system
- Verify sensor calibration

**As Needed:**
- Replace water reservoir
- Recalibrate lick sensor sensitivity
- Update firmware for bug fixes
