# Mouse VR - Unity & Arduino Integration for Behavioral Neuroscience

A complete Virtual Reality system for rodent behavioral experiments, integrating Unity 3D with Arduino-controlled hardware for real-time motion tracking, reward delivery, and event logging.

![Project Status](https://img.shields.io/badge/status-active-success.svg)
![Unity](https://img.shields.io/badge/Unity-2020+-blue.svg)
![Arduino](https://img.shields.io/badge/Arduino-Compatible-green.svg)
![C#](https://img.shields.io/badge/C%23-8.0+-purple.svg)

## 🎯 Project Overview

This project implements a traditional VR environment for behavioral neuroscience research, where a mouse on a trackball can navigate virtual environments. This approach is based on the pioneering trackball-based VR method described by Harvey et al. (2009), and was used as a comparison system in recent head-mounted VR headset research published in ***Nature Methods*** (*Isaacson, ***Chang**, et al., 2025 - [MouseGoggles](https://github.com/sn-lab/MouseGoggles)).

**Note:** This repository contains the Arduino firmware and Unity scripts for the VR system. The hardware setup (including the trackball, projection screen, optical sensors, and mechanical components) follows the design principles described in Harvey et al. (2009). For detailed hardware construction and assembly instructions, please refer to the original publication.

The system features:

- **Real-time motion tracking** using dual ADNS3080 optical flow sensors
- **Bidirectional Arduino-Unity communication** via serial protocol
- **Automated reward/punishment delivery** (water rewards & air puffs)
- **Comprehensive event logging** for experimental data analysis
- **Customizable VR environments** with trigger zones and stimulus presentation

### Projector-Based VR Setup

![Projector VR Setup](Documentation/images/projectorVR3.png)

*Traditional projector-based VR system with trackball for mouse navigation*

![Mouse in VR Setup](Documentation/images/real_mouse_example.png)

*Mouse navigating in the virtual environment. All animal procedures complied with relevant ethical regulations and were performed after approval by the Institutional Animal Care and Use Committee of Cornell University (protocol number 2015-0029).*

### System Architecture

```
┌─────────────────────┐
│   Unity VR Engine   │
│  ┌───────────────┐  │
│  │  Event Logger │  │
│  └───────┬───────┘  │
│  ┌───────▼───────┐  │
│  │Arduino Interface│ │
│  └───────┬───────┘  │
│  ┌───────▼───────┐  │
│  │Player Controller│ │
│  └───────────────┘  │
└──────────┬──────────┘
           │ Serial (115200 baud)
           │
┌──────────▼──────────┐
│   Arduino Board     │
│  ┌───────────────┐  │
│  │ ADNS3080 #1   │  │ ◄── Trackball Motion (X/Y)
│  │ ADNS3080 #2   │  │ ◄── Trackball Motion (X/Y)
│  │ Lick Sensor   │  │ ◄── Digital Input
│  │ Water Solenoid│  │ ──► Reward Delivery
│  │ Air Puff      │  │ ──► Punishment Delivery
│  └───────────────┘  │
└─────────────────────┘
```

## 🚀 Features

### Unity Components

- **ArduinoInterface.cs**: Manages serial communication, sensor data parsing, and hardware control
- **PlayerController.cs**: Translates trackball motion into VR navigation with keyboard fallback
- **Event.cs**: Data structure for timestamped experimental events
- **EventLogger.cs**: Persistent CSV logging of all experimental events for analysis

### Arduino Firmware

- **VR_sketch.ino**: Full production firmware with motion tracking, lick detection, and reward/punishment control
- **TrackballController.ino**: Basic trackball motion tracking only
- **reward_test.ino**: Serial-controlled water delivery testing
- **air_puff_test.ino**: Air puff solenoid validation

### Key Capabilities

- ✅ **Sub-millisecond latency** tracking using SPI communication with optical sensors
- ✅ **Dual sensor fusion** for robust X/Y motion detection
- ✅ **Time-synchronized event logging** (position, rotation, rewards, stimuli)
- ✅ **Configurable movement scaling** and velocity thresholds
- ✅ **Automated reward contingencies** based on position and behavior
- ✅ **Safe zone detection** for reward delivery control

## 📋 Hardware Requirements

### Arduino Setup
- **Arduino Due** or compatible board (115200 baud serial)
- **2x ADNS3080 optical flow sensors** (trackball motion)
- **Lick sensor** (capacitive or IR beam-break)
- **Water solenoid valve** (5V/12V with relay)
- **Air puff solenoid** (20-25 PSI recommended)

### Pin Configuration
```cpp
PIN_MOUSECAM_CS_1    = 10    // Sensor 1 chip select
PIN_MOUSECAM_RESET_1 = 11    // Sensor 1 reset
PIN_MOUSECAM_CS_2    = 4     // Sensor 2 chip select  
PIN_MOUSECAM_RESET_2 = 5     // Sensor 2 reset
LICK_PIN             = 2     // Lick sensor input
WATER_PIN            = 8     // Water solenoid output
AIRPUFF_PIN          = 7     // Air puff solenoid output
```

### Unity Setup
- **Unity 2020.x or later**
- **Universal Render Pipeline (URP)** (optional, for performance)
- **Character Controller** component
- **Serial port access** (configure COM port in Inspector)

## 🛠️ Installation & Setup

### 1. Arduino Firmware Upload

```bash
# Open Arduino IDE
# Select Board: Arduino Due (Programming Port)
# Select Port: (your COM port)
# Open: ArduinoSketches/VR_sketch.ino
# Click Upload
```

### 2. Unity Project Integration

1. Copy all scripts from `UnityScripts/` into your Unity project's `Assets/Scripts/` folder
2. Create an empty GameObject named "ArduinoInterface"
3. Attach the following scripts:
   - `ArduinoInterface.cs` → ArduinoInterface GameObject
   - `EventLogger.cs` → ArduinoInterface GameObject
   - `PlayerController.cs` → Player/Camera GameObject

4. Configure in Inspector:
   ```
   ArduinoInterface:
     - Serial Port Name: "COM7" (adjust for your system)
     - Back Scale: 0.1 (motion sensitivity)
     
   PlayerController:
     - Arduino Interface: (drag ArduinoInterface GameObject)
     - Event Logger: (drag ArduinoInterface GameObject)
     - Forward Velocity Scale Factor: 10.16
   ```

### 3. Serial Communication Protocol

The system uses a simple command-response protocol:

**Commands (Unity → Arduino):**
- `'h'` - Request motion data (returns 24 bytes: x1, y1, x2, y2, lick_count, dt)
- `'w'` - Trigger water reward (60ms pulse)
- `'a'` - Trigger air puff (20ms pulse)

**Data Structure:**
```cpp
struct MotionAndLickData {
  int32_t x_1;          // Sensor 1 X displacement
  int32_t y_1;          // Sensor 1 Y displacement  
  int32_t x_2;          // Sensor 2 X displacement
  int32_t y_2;          // Sensor 2 Y displacement
  int32_t lick_count;   // Number of licks detected
  int32_t dt;           // Time delta (milliseconds)
};
```

## 📊 Event Logging

All experimental events are logged to CSV with timestamps:

```csv
code,time,data
RESET,0.123,
POSITION,0.456,1.23,0.5,4.56
Rotated,0.789,0,45.2,0
[Rx,Ry,Rz],1.234,150,25,-10
WATER,2.345,
AIRPUFF,3.456,
TELEPORT,4.567,
```

**Event Types:**
- `RESET` - Player position reset
- `POSITION` - Player world coordinates (x, y, z)
- `Rotated` - Player rotation (Euler angles)
- `[Rx,Ry,Rz]` - Raw trackball rotation rates
- `WATER` - Water reward delivered
- `AIRPUFF` - Air puff delivered
- `TELEPORT` - Player teleported

**Access logs:** `C:/Users/[username]/OneDrive/Documents/UnityEvents/`

## 🎮 Usage

### Testing Hardware
1. Upload `reward_test.ino` to test water delivery
2. Upload `air_puff_test.ino` to test air puff system
3. Upload `TrackballController.ino` to test motion tracking only

### Running Experiments
1. Upload `VR_sketch.ino` (full system)
2. Launch Unity scene
3. Configure experiment parameters in EventLogger Inspector:
   - `mouse_ID`: Subject identifier
   - `experiment_name`: Experiment type/name
4. Press Play - data logging begins automatically
5. Press assigned `saver` key (default: 'S') to manually save during session
6. Data auto-saves on application quit

### Keyboard Controls (Debug)
- **Arrow Keys**: Manual movement (when trackball inactive)
- **Reset Key**: Return player to origin
- **Saver Key**: Force event log save

## 📁 Repository Structure

```
MouseVR-GitHub-Portfolio/
├── UnityScripts/
│   ├── ArduinoInterface.cs      # Serial communication & sensor management
│   ├── PlayerController.cs      # VR navigation & movement control
│   ├── Event.cs                 # Event data structure
│   └── EventLogger.cs           # CSV data persistence
│
├── UnityScripts_Tasks/          # Behavioral paradigm implementations
│   ├── README.md               # Detailed task documentation
│   ├── CoreTriggerScripts/     # Basic trigger zone scripts
│   │   ├── SafeZoneScript.cs   # Reward zone detection
│   │   ├── TeleportScript.cs   # Virtual cliff teleportation
│   │   ├── WallStripeTriggerScript.cs  # Reward cue trigger
│   │   └── WallLightTriggerScipt.cs    # Punishment cue trigger
│   ├── LoomingStimulus/        # Predator threat simulation
│   │   ├── looming.cs          # Automated looming paradigm
│   │   ├── Loomingdarkening.cs # Looming with darkness
│   │   ├── LoomingStimulusControlScript.cs  # Physics-based tracking
│   │   └── SpawnZone_LoomingStimulus_ControlScript.cs  # Spawn control
│   ├── Optomotor/              # Optokinetic reflex testing
│   │   ├── optomotor_task.cs   # Grating stimulus control
│   │   ├── optomotor_task1.cs  # Alternative paradigm
│   │   ├── single_frequency_task.cs  # Single frequency test
│   │   ├── multple_freq_2.cs   # Multiple frequency test
│   │   └── Velocity.cs         # Velocity control script
│   └── VisualCliff/            # Depth perception testing
│       └── visualcliff.cs      # Cliff paradigm controller
│
├── ConeProjection/             # Advanced projection mapping system
│   ├── README.md               # Comprehensive technical documentation
│   ├── Scripts/
│   │   ├── MakeConeProjection.cs    # Projection controller & optimization
│   │   ├── MakeCubeMap.cs           # Cube map generation
│   │   └── ProjectionOptimizer.cs   # Gradient descent calibration
│   ├── Shaders/
│   │   ├── ConeProjection.compute   # GPU ray-cone intersection
│   │   ├── CubeMapReadoutShader_LEFT.shader
│   │   └── CubeMapReadoutShader_RIGHT.shader
│   ├── Materials/               # Projection materials
│   └── RenderTexturesForConeProjection/  # Render targets
│
├── ArduinoSketches/
│   ├── VR_sketch.ino           # Full production firmware
│   ├── TrackballController.ino # Motion tracking only
│   ├── reward_test.ino         # Water delivery test
│   └── air_puff_test.ino       # Air puff test
│
├── Documentation/
│   └── images/                 # Setup photos and diagrams
│
├── Assets/
│   ├── Materials/              # Custom materials/shaders
│   └── Prefabs/                # Reusable GameObjects
│
├── .gitignore                  # Unity/Arduino gitignore
└── README.md                   # This file
```

## 🔧 Customization

### Adjusting Motion Sensitivity
```csharp
// In ArduinoInterface.cs Inspector
back_scale = 0.1f;          // Lower = less sensitive
scale_ratio = 1.0f;         // Overall multiplier
```

### Modifying Reward Timing
```cpp
// In VR_sketch.ino
#define WATER_OPEN_TIME 60      // milliseconds
#define AIRPUFF_OPEN_TIME 20    // milliseconds
```

### Adding Custom Events
```csharp
// In your trigger script
eventLogger.Add(new Event("CUSTOM_EVENT", dataArray));
```

## 🎓 Applications

This system was designed for behavioral neuroscience research, including:
- **Visuomotor integration** experiments
- **Looming stimulus** response studies
- **Spatial navigation** tasks
- **Operant conditioning** paradigms
- **Virtual cliff** avoidance tasks

## 🧪 Experimental Paradigms (UnityScripts_Tasks/)

The [UnityScripts_Tasks/](UnityScripts_Tasks/) folder contains complete implementations of classical behavioral neuroscience paradigms. See the [detailed README](UnityScripts_Tasks/README.md) for comprehensive documentation.

### Available Paradigms

#### 🦅 Looming Stimulus (Predator Threat Response)
**Location:** [LoomingStimulus/](UnityScripts_Tasks/LoomingStimulus/)

- **looming.cs** - Automated looming paradigm with controlled trials (30 trials, 3 speeds, randomized approach angles)
- **Loomingdarkening.cs** - Looming with environmental darkening effect
- **LoomingStimulusControlScript.cs** - Physics-based predator tracking with optimal interception algorithm
- **SpawnZone_LoomingStimulus_ControlScript.cs** - Probabilistic spawning with habituation control

**Scientific Basis:** Models innate defensive responses to overhead threats (Schiff et al., 1962; De Franceschi et al., 2016)

**Key Features:**
- Randomized approach angles (±35° azimuth, 25-30° elevation)
- Variable approach speeds (125, 250, 500 units/s)
- Automatic trial management and event logging
- Air puff delivery on collision

#### 👁️ Optomotor Response (Optokinetic Reflex)
**Location:** [Optomotor/](UnityScripts_Tasks/Optomotor/)

- **optomotor_task.cs** - Multi-frequency grating paradigm (7 spatial frequencies, 70 trials)
- **optomotor_task1.cs** - Alternative optomotor implementation
- **single_frequency_task.cs** - Single frequency validation
- **multple_freq_2.cs** - Multiple frequency testing protocol
- **Velocity.cs** - Velocity control utilities

**Scientific Basis:** Tests visual motion processing and tracking reflexes

**Key Features:**
- Spatial frequencies: 1°, 2°, 4°, 6°, 8°, 12°, 24° gratings
- Bidirectional rotation (clockwise/counterclockwise)
- Randomized trial order to prevent adaptation
- Automated stimulus-gray period alternation (2s grating, 4s gray)

#### 🏔️ Virtual Cliff (Depth Perception)
**Location:** [VisualCliff/](UnityScripts_Tasks/VisualCliff/)

- **visualcliff.cs** - Cliff avoidance testing (currently scaffolded for customization)

**Scientific Basis:** Gibson & Walk's classic visual cliff paradigm (1960)

**Note:** This script provides a template for implementing custom cliff avoidance protocols

#### 🎯 Core Trigger Scripts
**Location:** [CoreTriggerScripts/](UnityScripts_Tasks/CoreTriggerScripts/)

**SafeZoneScript.cs** - Reward zone detection
- Tracks player entry/exit from designated safe zones
- Enables operant conditioning with spatial contingencies

**TeleportScript.cs** - Virtual cliff teleportation
- Teleports player on boundary crossing
- Automatically deactivates looming stimuli on reset

**WallStripeTriggerScript.cs** - Positive cue (reward)
- Delivers water reward when player exits striped wall trigger zone
- Used for visual discrimination learning

**WallLightTriggerScipt.cs** - Aversive cue (punishment)
- Delivers air puff when player exits illuminated wall trigger zone
- Used for avoidance learning paradigms

### Experimental Design Features

**Automated Trial Management:**
- Pseudorandom stimulus presentation
- Built-in habituation periods
- Automatic trial counting and session termination
- Real-time event logging (stimulus parameters, trial numbers, outcomes)

**Data Logging:**
All task scripts integrate with EventLogger to record:
- Trial numbers and parameters
- Stimulus characteristics (speed, direction, frequency)
- Behavioral outcomes (approach/avoidance, reaction times)
- Continuous position and rotation data

**Customization:**
Each paradigm includes configurable parameters:
- Trial counts and durations
- Stimulus speeds and frequencies
- Randomization schemes
- Reward/punishment contingencies

## 🎨 Cone Projection System (ConeProjection/)

The [ConeProjection/](ConeProjection/) folder contains an advanced GPU-accelerated projection mapping system for correcting visual distortions on curved projection screens. See the [detailed technical README](ConeProjection/README.md) for comprehensive documentation.

### Overview

Standard projectors create severe geometric distortions when displaying on cylindrical or conical screens. This system uses compute shaders and ray-tracing to ensure the mouse perceives geometrically accurate visual stimuli.

**Technical Approach:**
1. Capture 360° environment using cube map rendering
2. Calculate ray-cone intersections on GPU (compute shader)
3. Apply geometric corrections for arbitrary projector placement
4. Optimize 10 projection parameters using gradient descent

### Key Components

**MakeConeProjection.cs** - Main projection controller
- 10 DOF parameter optimization (position, orientation, scale, cone geometry)
- Real-time calibration using marker-based correspondence
- Analytical gradient descent (1M iterations)
- GPU compute shader integration

**ProjectionOptimizer.cs** - Calibration algorithm
- Minimizes reprojection error: `L = Σ ||UV_estimated - UV_actual||²`
- Importance-weighted gradient descent
- Automatic parameter tuning from physical calibration markers

**ConeProjection.compute** - GPU ray-tracing shader
- Solves quadratic ray-cone intersection: `aλ² + bλ + c = 0`
- 1980x1020 resolution at real-time frame rates
- Handles edge cases (occlusion, invalid rays)

**CubeMapReadoutShader_LEFT/RIGHT.shader** - Final rendering
- Samples cube map using corrected directions
- Height-based fadeout for screen boundaries
- Separate left/right eye rendering

### Calibration Parameters

```csharp
// Projector Placement
projector_distance    // Distance from origin (meters)
projector_theta_1     // Horizontal angle (degrees)
projector_theta_2     // Vertical tilt (degrees)
projector_phi         // Azimuth rotation (degrees)
projector_tau         // Roll angle (degrees)
projector_height      // Height above ground (meters)
projector_scale       // Zoom/magnification

// Screen Geometry
cone_distance_to_top_row     // Distance to cone apex (meters)
cone_dinstance_to_second_row // Distance to cone base (meters)
cone_half_angle              // Cone opening half-angle (degrees)
```

### Scientific Importance

**Without Correction:**
- Grating frequencies appear non-uniform
- Object sizes distorted (closer to edges)
- Velocities non-constant across visual field
- Depth cues unreliable

**With Correction:**
- Geometrically accurate stimulus presentation
- Uniform visual angles and velocities
- Accurate depth perception cues
- Reliable spatial frequency measurements

**Applications:**
- Optokinetic reflex testing (uniform grating motion)
- Place cell recordings (accurate spatial layouts)
- Visual cliff experiments (depth cue preservation)
- Any paradigm requiring precise visual geometry

### Mathematical Foundation

**Ray-Cone Intersection:**
```
Cone equation: x² + y² = (z·tan(α))²
Ray equation:  P = X + λY

Quadratic solution yields intersection point
Surface normal computed for lighting
Cube map sampled at intersection direction
```

**Optimization:**
- 10-parameter nonlinear least squares
- Analytical Jacobian (no finite differences)
- Weighted gradient descent (sensitive parameters prioritized)
- Converges to sub-pixel accuracy

### Setup & Usage

1. **Physical Setup:**
   - Measure projector position and orientation
   - Place calibration markers on cone surface
   - Record 3D marker positions (angle, height)

2. **Unity Configuration:**
   - Attach `MakeConeProjection.cs` to camera
   - Create cube map render texture
   - Assign compute shader and materials
   - Configure initial parameter estimates

3. **Calibration:**
   - Mark corresponding UV positions in projector space
   - Set learning rate and importance weights
   - Run optimization (monitor loss convergence)
   - Validate with visual overlay

4. **Runtime:**
   - System automatically applies corrections
   - No performance overhead (GPU accelerated)
   - Works with dynamic scenes

**Performance:** Real-time at 1920x1080 on modern GPUs

## 📝 Technical Notes

- **Serial Baud Rate**: 115200 (configurable in both Unity and Arduino)
- **Update Rate**: ~1000 Hz on Arduino, Unity frame-rate dependent (~60-90 Hz)
- **Motion Resolution**: ADNS3080 provides up to 6469 counts per inch
- **Sensor Configuration**: "Sensitive mode" enabled (0x19) for low-velocity tracking

## 🤝 Contributing

This is a portfolio showcase project, but suggestions and improvements are welcome! Feel free to:
- Report issues with hardware integration
- Suggest protocol improvements
- Share your own behavioral paradigms

## 📄 License

This project is provided as-is for educational and research purposes. When using this code in academic work, please cite appropriately.

## 📚 References

This traditional trackball-based VR system implements methods based on:

1. **Harvey, C. D., Collman, F., Dombeck, D. A., & Tank, D. W.** (2009). Intracellular dynamics of hippocampal place cells during virtual navigation. *Nature, 461*(7266), 941-946. https://doi.org/10.1038/nature08499

This code was used in comparative analysis with head-mounted VR systems in:

2. **Isaacson, M., Chang, H., Berkowitz, L., Huang, Z.-J., Murphy, G. G., & Dombeck, D. A.** (2025). MouseGoggles: an immersive virtual reality headset for mouse neuroscience and behavior. *Nature Methods, 22*, 380-385. https://doi.org/10.1038/s41592-024-02540-y  
   - **GitHub Repository**: [MouseGoggles](https://github.com/sn-lab/MouseGoggles)

## 📧 Contact

**Hongyu Chang**  
Department of Neurobiology & Behavior, Cornell University  
hc997@cornell.edu  
[LinkedIn](www.linkedin.com/in/hongyu-chang-925987113)

**Ian Ellwood, PhD** (Principal Investigator)  
Department of Neurobiology & Behavior, Cornell University  
ite2@cornell.edu

---

## 🔗 Related Resources

- [ADNS3080 Datasheet](https://www.tindie.com/products/citizenresistor/adns-3080-optical-flow-sensor/)
- [Unity Serial Communication](https://docs.unity3d.com/ScriptReference/SerialPort.html)
- [Arduino Due Documentation](https://www.arduino.cc/en/Main/ArduinoBoardDue)

---

**⭐ If you find this project useful, please consider giving it a star!**
