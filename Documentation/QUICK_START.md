# Quick Start Guide

Get your Mouse VR system running in 15 minutes!

## Prerequisites

- ✅ Arduino IDE installed
- ✅ Unity 2020.x or later installed  
- ✅ Hardware assembled (see [HARDWARE_SETUP.md](HARDWARE_SETUP.md))
- ✅ USB cable connected Arduino ↔ PC

## Step 1: Upload Arduino Firmware (5 min)

1. **Open Arduino IDE**
2. **Install SPI Library** (if not already installed)
   - Sketch → Include Library → Manage Libraries
   - Search "SPI" → Install
3. **Open firmware:**
   ```
   File → Open → ArduinoSketches/VR_sketch.ino
   ```
4. **Configure board:**
   - Tools → Board → Arduino Due (Programming Port)
   - Tools → Port → [Select your COM port]
5. **Upload:**
   - Click "Upload" button (→)
   - Wait for "Done uploading" message

**Test:** Open Serial Monitor (115200 baud), type `h` - you should see binary data response

## Step 2: Unity Project Setup (5 min)

### Option A: New Unity Project
1. Create new 3D URP project
2. Copy scripts:
   ```
   MouseVR-GitHub-Portfolio/UnityScripts/*.cs
   → YourUnityProject/Assets/Scripts/
   ```

### Option B: Existing Project
1. Copy scripts to your `Assets/Scripts/` folder

### Common Steps:
3. **Create GameObjects:**
   - GameObject → Create Empty → Name: "ArduinoInterface"
   - GameObject → 3D Object → Capsule → Name: "Player"

4. **Attach Scripts:**
   - Select `ArduinoInterface` → Add Component → `ArduinoInterface`
   - Select `ArduinoInterface` → Add Component → `EventLogger`
   - Select `Player` → Add Component → `Character Controller`
   - Select `Player` → Add Component → `PlayerController`

5. **Configure Inspector:**

   **ArduinoInterface:**
   ```
   Serial Port Name: COM7  (change to your port)
   Back Scale: 0.1
   Event Logger: [Drag ArduinoInterface GameObject here]
   ```

   **EventLogger:**
   ```
   Mouse ID: test_mouse_01
   Experiment Name: quick_start_test
   ```

   **PlayerController:**
   ```
   Arduino Interface: [Drag ArduinoInterface GameObject]
   Event Logger: [Drag ArduinoInterface GameObject]
   Running: ✓ (checked)
   Rotation: ✓ (checked)
   Forward Velocity Scale Factor: 10.16
   ```

6. **Add Camera:**
   - Select `Player`
   - Drag `Main Camera` onto `Player` (make it child)
   - Set Camera position: (0, 1, 0)

## Step 3: First Test Run (5 min)

1. **Close Serial Monitor** (important!)
2. **Press Play** in Unity
3. **Check Console:**
   - Should see: "SerialPortOpened!"
   - Should NOT see errors about COM port

4. **Test Trackball:**
   - Roll trackball forward → Player should move forward
   - Rotate trackball left/right → Camera should rotate

5. **Test Keyboard Fallback:**
   - Arrow keys should move player (if trackball inactive)

6. **Test Logging:**
   - Press `S` key (saver) → Check console for save confirmation
   - Open: `C:/Users/[YourUsername]/OneDrive/Documents/UnityEvents/`
   - Verify CSV file created

## Troubleshooting

### ❌ "Access to COM port denied"
**Fix:** Close Arduino Serial Monitor, close other Unity instances

### ❌ "SerialPort not found"
**Fix:** 
1. Check Device Manager (Windows) → Ports (COM & LPT)
2. Update `serial_port_name` in Inspector
3. Try different USB cable/port

### ❌ Player not moving
**Fix:**
1. Check `running` is enabled in PlayerController
2. Verify Arduino is sending data (check `percent_arduino_reads_succesful` in Inspector should be ~0.99)
3. Increase `back_scale` value (try 0.5)

### ❌ Erratic movement
**Fix:**
1. Clean trackball surface
2. Lower `back_scale` value (try 0.05)
3. Check sensor alignment (2-3mm gap)

### ❌ No events logging
**Fix:**
1. Verify EventLogger attached to same GameObject as ArduinoInterface
2. Check output path exists
3. Ensure `save_on_quit` is checked

## Next Steps

✅ **Working?** Congratulations! Now:
1. Test water reward: Call `PlayerController.deliverWater()` from console
2. Test air puff: Call `PlayerController.deliverAirpuff()` from console
3. Build your VR environment (add walls, objects, triggers)
4. Implement experimental logic (safe zones, reward contingencies)

📖 **Learn More:**
- [SOFTWARE_ARCHITECTURE.md](SOFTWARE_ARCHITECTURE.md) - Understand the code
- [HARDWARE_SETUP.md](HARDWARE_SETUP.md) - Advanced hardware configuration
- [README.md](../README.md) - Full documentation

## Quick Reference

### Unity Inspector Variables

| Variable | Default | Purpose |
|----------|---------|---------|
| `back_scale` | 0.1 | Motion sensitivity (lower = slower) |
| `forward_velocity_scale_factor` | 10.16 | Speed multiplier |
| `stop_speed_threshold` | 0.1 | Minimum speed for "stopped" |
| `running` | true | Enable forward motion |
| `rotation` | true | Enable rotation |
| `strafing` | false | Enable side-to-side motion |

### Arduino Commands (Serial)

| Command | Action |
|---------|--------|
| `'h'` | Request sensor data |
| `'w'` | Deliver water (60ms) |
| `'a'` | Trigger air puff (20ms) |

### Common COM Ports

- **Windows:** COM3, COM4, COM5, COM7
- **Mac:** /dev/cu.usbmodem*
- **Linux:** /dev/ttyACM0

### File Locations

**Event Logs:**
```
Windows: C:/Users/[User]/OneDrive/Documents/UnityEvents/
Mac: ~/Documents/UnityEvents/
Linux: ~/Documents/UnityEvents/
```

**Arduino Sketches:**
```
Windows: Documents/Arduino/
Mac: ~/Documents/Arduino/
Linux: ~/Arduino/
```

## Testing Checklist

Before running experiments, verify:

- [ ] Arduino uploads without errors
- [ ] Serial port opens in Unity (check console)
- [ ] Trackball motion moves player
- [ ] Camera rotation responds to trackball
- [ ] Events save to CSV file
- [ ] Water solenoid triggers correctly
- [ ] Air puff solenoid triggers correctly
- [ ] Lick sensor registers behavior
- [ ] No console errors during 5-minute test run

## Getting Help

**Common Issues:**
1. Serial communication → Check COM port, close Serial Monitor
2. Performance → Reduce scene complexity, use URP
3. Sensor noise → Clean trackball, check wiring
4. Logging errors → Verify file path exists, check write permissions

**Still stuck?** Check the full [README.md](../README.md) for detailed troubleshooting.

---

**🎉 Success?** You're ready to build behavioral experiments!

Remember to:
- Save your Unity scene
- Commit your project to Git (exclude Library/ folder)
- Document your experimental parameters
- Back up event logs regularly
