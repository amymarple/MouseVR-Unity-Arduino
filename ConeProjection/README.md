# Cone Projection System

This folder contains a custom projection mapping system for correcting visual distortions in cone-shaped VR projection screens. This advanced rendering technique enables accurate visual presentation in cylindrical or conical projection setups commonly used in rodent VR systems.

## 📂 Folder Structure

```
ConeProjection/
├── Scripts/
│   ├── MakeConeProjection.cs    # Main projection controller with optimization
│   ├── MakeCubeMap.cs           # Cube map generation from camera
│   └── ProjectionOptimizer.cs   # Gradient descent projection calibration
├── Shaders/
│   ├── ConeProjection.compute           # GPU compute shader for cone mapping
│   ├── CubeMapReadoutShader_LEFT.shader # Left eye projection shader
│   └── CubeMapReadoutShader_RIGHT.shader # Right eye projection shader
├── Materials/
│   ├── LeftProjectionMaterial.mat   # Left eye material
│   └── RightProjectionMaterial.mat  # Right eye material
└── RenderTexturesForConeProjection/
    ├── CubeMapLeft.renderTexture        # Cube map render target
    └── EquirectangularLeft.renderTexture # Equirectangular render target
```

## 🎯 Purpose

When projecting VR content onto curved or conical screens, standard perspective projection creates severe visual distortions. This system:

1. **Captures the 3D scene** using cube map rendering
2. **Computes ray-cone intersections** to determine what the mouse sees from inside the cone
3. **Applies geometric corrections** using GPU compute shaders
4. **Optimizes projection parameters** to match physical projector placement

This ensures that the virtual environment appears geometrically correct from the mouse's perspective, even on a curved projection surface.

## 🔧 System Components

### Scripts

#### **MakeConeProjection.cs**
**Main projection controller and calibration system**

**Key Features:**
- Real-time projection parameter adjustment (10 DOF)
- Gradient descent optimization for calibration
- Compute shader integration for GPU acceleration
- UV marker-based alignment system

**Configurable Parameters:**
```csharp
projector_distance    // Distance from origin to projector (meters)
projector_theta_1     // Horizontal angle (degrees)
projector_theta_2     // Vertical tilt adjustment (degrees)
projector_phi         // Azimuth rotation (degrees)
projector_tau         // Roll/twist angle (degrees)
projector_scale       // Zoom/magnification factor
projector_height      // Height above ground plane (meters)

cone_distance_to_top_row    // Cone apex distance (meters)
cone_dinstance_to_second_row // Cone base distance (meters)
cone_half_angle             // Cone opening half-angle (degrees)
```

**Optimization System:**
- **Marker-based calibration**: Place world markers at known 3D positions on cone surface
- **UV correspondence**: Match markers to their 2D projector coordinates
- **Automatic parameter tuning**: Gradient descent minimizes reprojection error
- **Real-time feedback**: Visual overlay shows calibration accuracy

**Usage:**
```csharp
// Attach to Camera GameObject
// Assign ComputeShader, CubeMap, Material, and output_normals in Inspector
// Configure projection parameters
// Call optimizationStep() to auto-calibrate
```

---

#### **MakeCubeMap.cs**
**Environment capture using cube map rendering**

**Purpose:** Converts the Unity camera view into a cube map texture that can be sampled from any direction.

**Key Features:**
- Real-time cube map generation (6 faces)
- Executes in Edit Mode for setup
- Provides omnidirectional view for projection mapping

**Technical Details:**
- Renders 63 faces (full coverage)
- MonoOrStereoscopicEye.Left for left eye rendering
- Updates every frame for dynamic scenes

---

#### **ProjectionOptimizer.cs**
**Gradient descent optimization for projection calibration**

**Purpose:** Automatically finds optimal projection parameters by minimizing the error between expected and actual marker positions.

**Algorithm:**
1. **Forward projection**: World markers → Expected UV coordinates
2. **Loss computation**: Sum of squared distances between expected and actual UV
3. **Gradient calculation**: Analytical derivatives w.r.t. all 10 parameters
4. **Parameter update**: Gradient descent with weighted learning rates

**Key Features:**
- Analytical gradient computation (no numerical approximation)
- Importance weighting for critical markers
- Automatic learning rate scheduling
- 1M iteration optimization with periodic logging

**Gradient Weights:**
```csharp
gradient_weights = {
    1f,    // theta_1 (horizontal angle)
    1f,    // theta_2 (vertical tilt)
    1f,    // phi (azimuth)
    10.0f, // tau (roll) - more sensitive
    10.0f, // distance - more sensitive
    1f,    // height
    10f,   // scale - more sensitive
    1f,    // cone_half_angle
    1f,    // distance_to_top
    0f     // distance_to_bottom - fixed
};
```

---

### Shaders

#### **ConeProjection.compute**
**GPU compute shader for ray-cone intersection**

**Algorithm:**
1. For each pixel (u,v) in projector space:
   - Construct ray from projector origin through pixel
   - Solve quadratic equation for ray-cone intersection
   - Compute 3D point on cone surface
   - Calculate surface normal and lighting
   - Handle edge cases (no intersection, behind cone)

**Mathematical Foundation:**
```
Ray: P = X + λY  (X = projector position, Y = ray direction)
Cone: x² + y² = (z·tan(α))²  (α = half-angle)

Quadratic: aλ² + bλ + c = 0
a = Y.x² + Y.y² - (tan α)²·Y.z²
b = 2(X·Y)x,y - 2(tan α)²·(X·Y)z
c = X.x² + X.y² - (tan α)²·X.z²
```

**Output:**
- RGB: 3D point on cone surface (scaled)
- Alpha: cos(angle) for lighting/fade effects

---

#### **CubeMapReadoutShader_LEFT/RIGHT.shader**
**Final rendering shaders with cube map sampling**

**Purpose:** Samples the cube map using the computed 3D directions to display the correct texture on the cone projection.

**Key Features:**
- Mouse position-based alignment
- Height-based fadeout (configurable min/max)
- Alignment image overlay (for calibration)
- Separate shaders for left/right eye rendering

**Properties:**
```glsl
_MainTex           // Normal map from compute shader
_Cube              // Cube map texture
_MousePositionLEFT // Mouse position for dynamic adjustment
_AlignmentImageLEFT // Calibration overlay toggle
_MaxHeight         // Upper fade boundary
_MinHeight         // Lower fade boundary
_fadeOut           // Fade intensity
```

---

## 🚀 Setup & Usage

### 1. Initial Setup

```
1. Create two GameObjects:
   - "CubemapCamera" (for environment capture)
   - "ProjectionCamera" (for final projection)

2. Attach Scripts:
   - MakeCubeMap.cs → CubemapCamera
   - MakeConeProjection.cs → ProjectionCamera

3. Create RenderTextures:
   - CubeMapLeft (Cube type, 1024x1024)
   - EquirectangularLeft (2D, 1980x1020)

4. Assign in Inspector:
   - MakeCubeMap: CubeMap = CubeMapLeft
   - MakeConeProjection:
     - ComputeShader = ConeProjection.compute
     - CubeMap = CubeMapLeft
     - Material = LeftProjectionMaterial
     - output_normals = EquirectangularLeft
```

### 2. Calibration Process

```
1. Physical Markers:
   - Place calibration markers on cone surface
   - Measure 3D positions (angle, height)
   - Record in worldPointData array

2. UV Markers:
   - Create GameObjects at corresponding projector coordinates
   - Scale by 9.0 (Unity units)
   - Assign to UVMarkers array

3. Optimization:
   - Set learning_rate (start with 0.001)
   - Set importance weights (higher for critical points)
   - Call optimizationStep() in Inspector
   - Monitor loss value (should decrease)

4. Fine-tuning:
   - Manually adjust parameters for final tweaks
   - Check estimatedUVMarkers overlay
   - Verify alignment across entire cone surface
```

### 3. Runtime Usage

Once calibrated, the system runs automatically:
- CubemapCamera captures environment
- Compute shader projects onto cone
- Shader renders corrected output
- Mouse sees geometrically accurate VR world

---

## 📊 Technical Specifications

**Resolution:** 1980x1020 pixels (custom aspect ratio)
**Compute Threads:** 16x16 blocks (GPU optimized)
**Optimization:** 1M iterations max (early stopping available)
**Update Rate:** Real-time (Unity frame rate)

**Supported Projector Types:**
- Standard DLP/LCD projectors
- Short-throw projectors
- Arbitrary mounting angles

**Supported Screen Geometries:**
- Cones (arbitrary half-angle)
- Cylinders (90° half-angle)
- Truncated cones

---

## 🎓 Scientific Applications

This projection system is critical for:

1. **Accurate Visual Stimuli:** Ensures grating frequencies, object sizes, and distances are perceived correctly
2. **Spatial Navigation:** Prevents geometric distortions that could confuse place cell recordings
3. **Optokinetic Testing:** Maintains constant velocity across visual field
4. **Depth Cue Research:** Preserves perspective and parallax cues

**Without correction:** Objects appear stretched, velocities non-uniform, distances distorted
**With correction:** Geometrically accurate presentation matching real-world optics

---

## 🔬 Mathematical Background

### Projection Transform

The system implements a full perspective projection with arbitrary projector placement:

**Basis Vectors:**
```
U = horizontal projector axis (right)
V = vertical projector axis (up)
F = forward projector axis (optical axis)
X = projector position in world space
```

**Ray Construction:**
```
Y(u,v) = β·(aspect·(u-0.5)·U + (v-0.5)·V) + F
Ray = X + λY(u,v)
```

**Optimization Objective:**
```
L = Σ importance[i] · ||UV_estimated[i] - UV_actual[i]||²
```

Minimized using analytical gradient descent with respect to all 10 projection parameters.

---

## 💡 Customization

### Different Screen Geometries

**Cylinder:**
```csharp
cone_half_angle = 90f;  // Vertical walls
```

**Narrow Cone:**
```csharp
cone_half_angle = 30f;  // Steeper cone
```

**Truncated Cone:**
```csharp
cone_distance_to_top_row = 2.0f;     // Top radius
cone_dinstance_to_second_row = 3.0f; // Height difference
```

### Performance Optimization

**Lower Resolution:**
```csharp
x_resolution = 1280;
y_resolution = 720;
```

**Reduce Iterations:**
```csharp
for (int step_number = 0; step_number < 100000; step_number++)
```

---

## 📚 References

This projection correction technique is based on principles from:

- **Computer Graphics:** Perspective projection and ray tracing
- **Camera Calibration:** Zhang's method and bundle adjustment
- **VR Systems:** Curved display warping and pre-distortion

Similar techniques used in:
- Planetarium dome projection
- Flight simulator curved screens
- Immersive cave systems (CAVE)

Adapted specifically for rodent VR trackball systems with conical projection screens.

---

## 🔗 Dependencies

**Required Unity Components:**
- Compute Shader support (Unity 2018.1+)
- RenderTexture API
- Camera.RenderToCubemap()

**Required Scripts:**
- None (standalone system)

**Recommended Resolution:** 1920x1080 minimum projector output

---

## ⚠️ Troubleshooting

**Black output:**
- Check compute shader assignment
- Verify RenderTexture formats (ARGBFloat)
- Ensure cube map is populated

**Distorted projection:**
- Recalibrate using marker system
- Check cone parameters (half-angle, distances)
- Verify projector mounting matches parameters

**Low performance:**
- Reduce x_resolution/y_resolution
- Optimize compute shader thread groups
- Disable real-time optimization during experiments

---

**When describing this system:**

✅ "Developed custom GPU-accelerated projection mapping system using compute shaders and ray-tracing for geometrically accurate VR display on curved screens"

✅ "Implemented automatic calibration using gradient descent optimization with 10 degrees of freedom for arbitrary projector placement"

✅ "Created analytical gradient computation for real-time parameter optimization minimizing reprojection error across cone surface"
