# Unity Scripts - Experimental Tasks

This folder contains Unity C# scripts implementing various behavioral neuroscience paradigms.

## 📂 Contents

### Core Task Scripts

#### 🏔️ Virtual Cliff / Teleport System
**`TeleportScript.cs`**
- Implements virtual cliff paradigm (Gibson & Walk, 1960)
- Teleports player when crossing invisible boundary
- Tests depth perception and avoidance learning
- Automatically deactivates looming stimulus on teleport

**Usage:** Attach to invisible trigger collider at cliff edge

---

#### 🎯 Safe Zone Detection
**`SafeZoneScript.cs`**
- Detects when player enters/exits reward zones
- Updates `playerIsInSafeZone` flag in PlayerController
- Used for operant conditioning (reward delivery contingencies)
- Requires player to stop in zone to receive water

**Usage:** Attach to invisible trigger collider marking safe area

---

#### 🚦 Visual Discrimination Cues

**`WallStripeTriggerScript.cs`**
- Striped pattern → Water reward delivery
- Positive reinforcement cue

**`WallLightTriggerScipt.cs`**
- Illuminated pattern → Air puff punishment
- Aversive stimulus cue

**Usage:** Attach to wall/object colliders (set as Trigger)  
**Scientific Purpose:** Classical conditioning, cue discrimination learning

---

### 🦅 Looming Stimulus System

#### `LoomingStimulus/LoomingStimulusControlScript.cs`
**Simulates overhead predator threat**

**Key Features:**
- Physics-based player tracking and interception
- Randomized spawn position (elevation 25-30°, azimuth ±35°)
- Optimal heading calculation accounting for player velocity
- Delivers air puff on collision

**Algorithm:**
```csharp
// Interception calculation
Vector3 D = (player.position - stimulus.position).normalized;
Vector3 v = player.velocity.normalized;
Vector3 v_perp = v - Dot(v, D) * D;
float sin_th = v_perp.magnitude * (player.speed / stimulus.speed);
Vector3 optimal_velocity = (sqrt(1 - sin_th²) * D + sin_th * v) * speed;
```

**Scientific Context:**
- Models innate defensive responses (De Franceschi et al., 2016)
- Classic looming paradigm (Schiff et al., 1962)
- Used to study predator avoidance in rodents

---

#### `LoomingStimulus/SpawnZone_LoomingStimulus_ControlScript.cs`
**Manages looming stimulus spawning**

**Features:**
- Habituation period: `min_safe_crossings = 2` (no threats initially)
- Probabilistic spawning: `spawnChance = 0.01` per second
- Single stimulus at a time (prevents overwhelming)
- Resets on zone exit

**Experimental Design:**
1. Mouse completes 2+ safe trials (habituation)
2. Looming stimulus becomes possible in spawn zone
3. Probabilistic appearance prevents predictability
4. Stimulus tracks mouse until collision or zone exit

---

## 🎓 Scientific Applications

### Behavioral Paradigms Implemented

| Paradigm | Script | Classical Reference |
|----------|--------|---------------------|
| Visual Cliff | TeleportScript | Gibson & Walk (1960) |
| Looming Threat | LoomingStimulusControl | Schiff et al. (1962) |
| Safe Zone Navigation | SafeZoneScript | Operant conditioning |
| Cue Discrimination | WallStripe/WallLight | Pavlov (1927) |

### Research Applications

**Depth Perception:**
- Virtual cliff avoidance
- Edge detection studies

**Threat Detection:**
- Innate defensive behaviors
- Freezing/fleeing responses
- Predator avoidance learning

**Associative Learning:**
- Classical conditioning (cue-outcome)
- Operant conditioning (action-reward)
- Discrimination learning (stripe vs light)

**Spatial Navigation:**
- Goal-directed movement
- Reward zone learning
- Path optimization

---

## 🔧 Implementation Guide

### 1. Safe Zone Setup
```
GameObject: SafeZone (invisible BoxCollider)
├── Is Trigger: ✓
└── Script: SafeZoneScript
    └── Player: [Drag Player GameObject]
```

### 2. Virtual Cliff Setup
```
GameObject: CliffEdge (invisible BoxCollider)
├── Is Trigger: ✓
└── Script: TeleportScript
    ├── Player: [Drag Player GameObject]
    ├── Teleport Vector: (-2, 1, 0)  ← Start position
    └── Looming Stimulus: [Drag LoomingStimulus GameObject]
```

### 3. Looming Stimulus Setup
```
GameObject: LoomingStimulus (Sphere with collider)
├── Is Trigger: ✓
├── Mesh Renderer: Dark/black material
└── Script: LoomingStimulusControlScript
    ├── Player: [Drag Player GameObject]
    ├── Start Distance: 40
    ├── Speed: 1.0
    ├── Th Range: (25, 30)
    └── Ph Range: (-35, 35)

GameObject: SpawnZone (invisible BoxCollider)
├── Is Trigger: ✓
└── Script: SpawnZone_LoomingStimulus_ControlScript
    ├── Player: [Drag Player GameObject]
    ├── Looming Stimulus: [Drag LoomingStimulus GameObject]
    ├── Spawn Chance: 0.01
    └── Min Safe Crossings: 2
```

### 4. Visual Cue Setup
```
GameObject: StripedWall (BoxCollider)
├── Is Trigger: ✓
├── Material: Striped pattern
└── Script: WallStripeTriggerScript
    └── Player: [Drag Player GameObject]

GameObject: LightWall (BoxCollider)
├── Is Trigger: ✓
├── Material: Illuminated
└── Script: WallLightTriggerScipt
    └── Player: [Drag Player GameObject]
```

---

## 📊 Event Logging

All task events are automatically logged via EventLogger:

**Logged Events:**
- `TELEPORT` - Cliff crossing
- `WATER` - Reward delivery (safe zone or striped wall)
- `AIRPUFF` - Punishment delivery (light wall or looming collision)
- `POSITION` - Continuous position tracking
- `Rotated` - Heading changes

**Analysis Metrics:**
- Cliff avoidance rate
- Looming stimulus escape latency
- Safe zone dwell time
- Cue discrimination accuracy
- Path trajectories

---

## 💡 Customization

### Adjust Difficulty

**Easier:**
```csharp
// SafeZone larger
// Looming slower: speed = 0.5f
// Lower spawn chance: spawnChance = 0.005f
// More habituation: min_safe_crossings = 5
```

**Harder:**
```csharp
// SafeZone smaller
// Looming faster: speed = 2.0f
// Higher spawn chance: spawnChance = 0.02f
// Less habituation: min_safe_crossings = 0
```

### Add New Cues

```csharp
public class NewCueTriggerScript : MonoBehaviour {
    public GameObject player;
    
    void OnTriggerEnter(Collider other) {
        if (other.gameObject == player) {
            // Your custom action
            player.GetComponent<PlayerController>().customAction();
        }
    }
}
```

---

## 🎯 Resume Highlights

**When describing these tasks:**

✅ "Implemented physics-based predator simulation with real-time player tracking 
   and optimal interception algorithms"

✅ "Designed modular trigger system enabling rapid development of behavioral 
   paradigms (cliff avoidance, threat response, discrimination learning)"

✅ "Built probabilistic spawning system with habituation period for controlled 
   threat exposure in neuroscience experiments"

✅ "Created event-driven architecture logging all behavioral responses for 
   data analysis (reaction times, trajectories, learning curves)"

---

## 📚 References

- **Visual Cliff:** Gibson, E. J., & Walk, R. D. (1960). The "visual cliff." *Scientific American, 202*(4), 64-71.

- **Looming Stimulus:** Schiff, W., Caviness, J. A., & Gibson, J. J. (1962). Persistent fear responses in rhesus monkeys to the optical stimulus of "looming." *Science, 136*(3520), 982-983.

- **Mouse Looming:** De Franceschi, G., Vivattanasarn, T., Saleem, A. B., & Solomon, S. G. (2016). Vision guides selection of freeze or flight defense strategies in mice. *Current Biology, 26*(16), 2150-2154.

- **Classical Conditioning:** Pavlov, I. P. (1927). *Conditioned reflexes: An investigation of the physiological activity of the cerebral cortex.*

---

## 🔗 Dependencies

**Required Components:**
- `PlayerController.cs` (in ../UnityScripts/)
- `EventLogger.cs` (in ../UnityScripts/)
- CharacterController (Unity built-in)

**Optional Enhancement:**
- `ConeProjection/` system (in ../ConeProjection/) - For geometrically accurate projection on curved screens

**Unity Version:** 2020.x or later

---

## 🎨 Integration with Cone Projection System

For experiments requiring precise visual geometry (especially optomotor and visual cliff), consider using the [ConeProjection system](../ConeProjection/README.md) to correct distortions on curved projection screens.

**Benefits for Task Paradigms:**

- **Optomotor Tasks**: Ensures uniform grating velocity across visual field
- **Visual Cliff**: Preserves depth cues and perspective accuracy
- **Looming Stimulus**: Maintains accurate angular expansion rates
- **Spatial Navigation**: Accurate distance and scale perception

See the [ConeProjection README](../ConeProjection/README.md) for setup and calibration instructions.
