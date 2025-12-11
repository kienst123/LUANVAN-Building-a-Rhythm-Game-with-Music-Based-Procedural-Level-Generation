# 🎮 Rhythm Game 3D

**3D osu!mania style rhythm game built with Unity**

Minimalist clean design with modern features:
- ✨ 3D perspective view (notes fall towards camera)
- 🎯 4-key gameplay (D, F, J, K)
- 🎨 Particles, trails, and visual effects
- 📊 Perfect/Great/Good/Miss judgment system
- 🎵 Reads .osu beatmap files
- 💯 Real-time accuracy and combo tracking

---

## 🚀 Quick Start

### 1. Open Project in Unity
- Unity version: **2021.3+ recommended**
- Open folder: `RhythmGame3D`

### 2. Create Game Scene
1. Create new scene: `Assets/Scenes/GameScene.unity`
2. Add empty GameObject `GameManager`
3. Add components to GameManager:
   - `GameManager3D.cs`
   - `NoteSpawner3D.cs`
   - `InputManager3D.cs`
   - `JudgmentSystem.cs`

### 3. Setup Camera
1. Main Camera:
   - Add `CameraController3D.cs`
   - Position: (0, 3, -5)
   - Rotation: (15, 0, 0)
   - FOV: 60

### 4. Create Note Prefabs
**Tap Note Prefab:**
1. Create Cube → Name: "TapNote"
2. Scale: (1, 0.5, 1)
3. Add `NoteController3D.cs`
4. Add TrailRenderer (optional)
5. Add ParticleSystem (child object, optional)
6. Save as prefab: `Assets/Prefabs/TapNote.prefab`

**Long Note Prefab:**
- Same as Tap Note but name: "LongNote"
- NoteController3D will auto-create tail

### 5. Create Materials
**Note Material:**
```
Assets/Materials/NoteMaterial.mat
- Shader: Standard
- Rendering Mode: Transparent
- Albedo: Cyan (100, 180, 255)
- Emission: Enabled, Color: Cyan
```

**Lane Material:**
```
Assets/Materials/LaneMaterial.mat
- Shader: Standard
- Rendering Mode: Transparent
- Albedo: White (alpha 0.3)
```

### 6. Setup UI
1. Create Canvas (Screen Space - Overlay)
2. Add panels and TextMeshPro elements:
   - Top Right: Score, Combo
   - Center: Judgment text, Early/Late text
   - Bottom: Accuracy, Health bar, Time
3. Add `ModernUIManager3D.cs` to Canvas
4. Assign UI elements in Inspector

### 7. Connect References
In GameManager Inspector:
- Note Spawner → NoteSpawner3D component
- Input Manager → InputManager3D component
- Judgment System → JudgmentSystem component
- UI Manager → ModernUIManager3D component
- Music Source → Add AudioSource component

In NoteSpawner3D Inspector:
- Tap Note Prefab → TapNote prefab
- Long Note Prefab → LongNote prefab
- Lane Material → LaneMaterial

### 8. Add Beatmap
1. Copy .osu file to `Assets/Beatmaps/`
2. Copy audio file (mp3/ogg) to same folder
3. In GameManager Inspector:
   - Beatmap File Path → Full path to .osu file
4. In Music Source Inspector:
   - Audio Clip → Assign audio file

### 9. Play!
- Press **Play** in Unity
- Press **Space** to start song
- Keys: **D F J K** for lanes 0-3
- Press **R** to restart
- Press **ESC** to stop

---

## 🎹 Controls

| Key | Action |
|-----|--------|
| D | Lane 0 (leftmost) |
| F | Lane 1 |
| J | Lane 2 |
| K | Lane 3 (rightmost) |
| Space | Start/Pause |
| R | Restart |
| ESC | Stop |

---

## 📁 Project Structure

```
RhythmGame3D/
├── Assets/
│   ├── Scripts/
│   │   ├── Beatmap/
│   │   │   ├── BeatmapData.cs       # Beatmap data structure
│   │   │   ├── BeatmapParser.cs     # Parse .osu files
│   │   │   └── HitObject.cs         # Note data
│   │   ├── Core/
│   │   │   ├── GameManager3D.cs     # Main game controller
│   │   │   └── CameraController3D.cs # Camera setup
│   │   ├── Gameplay/
│   │   │   ├── NoteSpawner3D.cs     # Spawns notes in 3D
│   │   │   ├── NoteController3D.cs  # Individual note behavior
│   │   │   ├── InputManager3D.cs    # Handle D/F/J/K input
│   │   │   └── JudgmentSystem.cs    # Timing & scoring
│   │   └── UI/
│   │       └── ModernUIManager3D.cs # UI display
│   ├── Prefabs/
│   │   ├── TapNote.prefab
│   │   └── LongNote.prefab
│   ├── Materials/
│   │   ├── NoteMaterial.mat
│   │   └── LaneMaterial.mat
│   ├── Scenes/
│   │   └── GameScene.unity
│   └── Beatmaps/
│       └── (your .osu files here)
└── ProjectSettings/
```

---

## ⚙️ Configuration

### Timing Windows (in JudgmentSystem)
- Perfect: ±40ms
- Great: ±80ms
- Good: ±120ms
- Miss: ±180ms

### Scoring
- Perfect: 300 points
- Great: 200 points
- Good: 100 points
- Miss: 0 points

### Lane Settings (in NoteSpawner3D)
- Lane Count: 4
- Lane Width: 1.5 units
- Spawn Distance: 40 units (Z-axis)
- Hit Position: 0 units (Z-axis)
- Approach Time: 2 seconds

### Camera Settings (in CameraController3D)
- Distance: 5 units behind hit position
- Height: 3 units above lanes
- Look Down Angle: 15 degrees
- Field of View: 60 degrees

---

## 🎨 Customization

### Change Note Colors
Edit in `NoteController3D.cs`:
```csharp
public Color noteColor = new Color(0.4f, 0.7f, 1f);  // Cyan
```

### Change Lane Colors
Edit in `NoteSpawner3D.cs`:
```csharp
public Color[] laneColors = new Color[] {
    new Color(0.8f, 0.8f, 0.8f, 0.3f),  // Lane 0
    new Color(0.4f, 0.7f, 1f, 0.3f),    // Lane 1
    new Color(0.4f, 0.7f, 1f, 0.3f),    // Lane 2
    new Color(0.8f, 0.8f, 0.8f, 0.3f)   // Lane 3
};
```

### Change Key Bindings
Edit in `InputManager3D.cs`:
```csharp
public KeyCode[] laneKeys = new KeyCode[] {
    KeyCode.D,  // Lane 0
    KeyCode.F,  // Lane 1
    KeyCode.J,  // Lane 2
    KeyCode.K   // Lane 3
};
```

---

## 🐛 Troubleshooting

**Notes not appearing?**
- Check prefabs assigned in NoteSpawner3D
- Check beatmap file path is correct
- Check camera can see Z=0 to Z=40 range

**No audio playing?**
- Assign AudioClip to Music Source
- Check audio file format (mp3/ogg/wav)
- Audio loading from path not implemented - use Inspector

**Input not working?**
- Check InputManager3D references set
- Check keys D/F/J/K not used by other components

**Timing feels off?**
- Adjust `audioOffset` in GameManager3D
- Positive value = delay audio
- Negative value = advance audio

---

## 📝 TODO / Future Features

- [ ] Results screen after song ends
- [ ] Audio loading from file at runtime
- [ ] More visual effects (bloom, glow)
- [ ] Difficulty selector
- [ ] Replay system
- [ ] Online leaderboards
- [ ] Custom note skins
- [ ] Video background support

---

## 🔗 Based On

- **osu!mania** - Rhythm game mode
- **Web-Osu-Mania** - UI design inspiration

---

## 📄 License

MIT License - Free to use and modify

---

**Made with ❤️ using Unity 3D**
