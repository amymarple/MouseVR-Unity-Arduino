# Contributing to Mouse VR

Thank you for your interest in contributing! This is primarily a portfolio showcase project, but improvements and suggestions are welcome.

## How to Contribute

### Reporting Issues
- **Hardware Problems**: Include Arduino board model, sensor specifications, wiring diagram
- **Software Bugs**: Include Unity version, OS, console error messages
- **Documentation**: Suggest clarifications or corrections

### Suggesting Enhancements
- **New Features**: Describe use case and behavioral paradigm
- **Performance Improvements**: Include benchmarks and profiling data
- **Hardware Support**: Specify new sensor/actuator models

### Code Contributions

#### Arduino Firmware
- Follow Arduino style guide
- Test on Arduino Due (or specify compatible boards)
- Comment all hardware-specific timing requirements
- Avoid blocking delays in main loop

#### Unity Scripts
- Follow C# naming conventions (PascalCase for public, camelCase for private)
- Add XML documentation comments for public methods
- Maintain compatibility with Unity 2020.x+
- Avoid external dependencies when possible

#### Documentation
- Use clear, concise language
- Include code examples where appropriate
- Add diagrams for complex concepts
- Test all installation steps

## Pull Request Process

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/your-feature-name`
3. **Make changes**:
   - Write clear commit messages
   - Update README/documentation if needed
   - Test on actual hardware (if applicable)
4. **Submit PR**:
   - Describe changes and motivation
   - Reference any related issues
   - Include test results

## Development Setup

### Testing Arduino Code
```bash
# Compile without uploading
arduino-cli compile --fqbn arduino:sam:arduino_due_x VR_sketch.ino

# Upload to board
arduino-cli upload -p COM7 --fqbn arduino:sam:arduino_due_x VR_sketch.ino
```

### Testing Unity Scripts
- Create minimal test scene
- Verify no console errors
- Test with both hardware and keyboard fallback

## Code Style

### Arduino
```cpp
// Constants in UPPER_CASE
#define SENSOR_PIN 10

// Functions in camelCase
void readSensorData() {
    // ...
}

// Descriptive variable names
int sensorValue = 0;  // Good
int sv = 0;           // Bad
```

### C# (Unity)
```csharp
// Public fields PascalCase
public float BackScale = 0.1f;

// Private fields camelCase
private bool firstRead = true;

// Methods PascalCase
public void DeliverWater() {
    // ...
}
```

## Areas for Contribution

### High Priority
- [ ] Mac/Linux COM port detection
- [ ] Unity Package Manager (.unitypackage) distribution
- [ ] Video tutorials/demos
- [ ] Additional behavioral paradigms (examples)

### Medium Priority
- [ ] Support for other Arduino boards (Mega, Teensy)
- [ ] Alternative sensor options (MPU6050, etc.)
- [ ] Real-time data visualization in Unity
- [ ] Configuration file support (JSON/XML)

### Documentation
- [ ] Hardware assembly video
- [ ] Wiring photographs
- [ ] Example experimental protocols
- [ ] Performance benchmarking results

## Questions?

Open an issue with the "question" label, or contact [your contact method].

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
