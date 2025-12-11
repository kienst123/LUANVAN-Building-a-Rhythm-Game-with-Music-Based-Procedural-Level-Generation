# 🎮 3D Main Menu Setup Guide

## Overview
Transform your rhythm game menu from 2D Canvas UI to stunning 3D "Neon Rhythm Arena" style!

## Features
✨ **Neon Tunnel Background** - Infinite scrolling grid with animated rings
🎯 **3D Interactive Buttons** - Hover effects, glow, and smooth animations  
📹 **Dynamic Camera** - Mouse parallax and gentle idle movement
🎵 **Audio Visualizer** - Real-time spectrum analyzer with 64 bars
💡 **Bloom & Glow** - Neon aesthetic with emissive materials

---

## Quick Setup (5 Minutes)

### Step 1: Create New Scene or Modify MainMenu
1. Open `MainMenu` scene (or create new scene)
2. Delete old Canvas UI (if exists)
3. Create empty GameObject: `MenuManager3D`
4. Add component: `MainMenu3DManager`

### Step 2: Auto Setup
The `MainMenu3DManager` will automatically create:
- ✅ Neon background with tunnel
- ✅ 4 menu buttons (Play, Beatmap, Settings, Quit)
- ✅ Camera with parallax effect
- ✅ Audio visualizer

### Step 3: Optional - Add Post Processing
For best visual quality:

1. Install **Post Processing** package:
   - Window → Package Manager
   - Search "Post Processing"
   - Install

2. Add to Camera:
   - Select Main Camera
   - Add Component: `Post-process Layer`
   - Add Component: `Post-process Volume`
   - Enable "Is Global"

3. Create Profile:
   - Create → Post-processing Profile
   - Add effects:
     - ✅ Bloom (Intensity: 5, Threshold: 0.5)
     - ✅ Color Grading (Temperature: -10, Tint: 10)
     - ✅ Vignette (Intensity: 0.3)

---

## Manual Setup (Detailed)

### Background Setup
```csharp
GameObject bgObj = new GameObject("NeonBackground");
NeonBackgroundManager bg = bgObj.AddComponent<NeonBackgroundManager>();
```

**Customization:**
- `gridScrollSpeed`: Speed of tunnel movement (default: 2)
- `rotationSpeed`: Speed of hexagon rotation (default: 5)
- `primaryColor`: Main neon color (default: Cyan)
- `secondaryColor`: Accent color (default: Magenta)

### Camera Setup
```csharp
Camera cam = Camera.main;
MenuCamera3D camController = cam.gameObject.AddComponent<MenuCamera3D>();
```

**Customization:**
- `enableParallax`: Toggle mouse parallax (default: true)
- `parallaxAmount`: Strength of parallax effect (default: 2)
- `enableIdleAnimation`: Toggle idle movement (default: true)

### Create 3D Buttons
```csharp
GameObject buttonObj = new GameObject("PlayButton");
MenuButton3D button = buttonObj.AddComponent<MenuButton3D>();
button.SetText("PLAY");
button.AddClickListener(() => {
    SceneManager.LoadScene("GameScene");
});
```

### Audio Visualizer
```csharp
GameObject vizObj = new GameObject("AudioVisualizer");
AudioVisualizer3D viz = vizObj.AddComponent<AudioVisualizer3D>();
viz.SetAudioSource(yourAudioSource);
```

---

## Color Palette

**Neon Rhythm Arena Theme:**
```
Background:     #0A0A1F (Dark blue-purple)
Primary Neon:   #00F0FF (Cyan)
Secondary Neon: #FF00E5 (Magenta)  
Accent:         #FFFF33 (Yellow)
UI Glass:       rgba(255,255,255,0.1)
```

---

## Customization Examples

### Change Colors
```csharp
// In MainMenu3DManager or NeonBackgroundManager Inspector
primaryColor = new Color(1f, 0.3f, 0.3f); // Red theme
secondaryColor = new Color(0.3f, 1f, 0.3f); // Green theme
```

### Adjust Button Layout
```csharp
// In MainMenu3DManager.SetupMenuButtons()
Vector3[] buttonPositions = {
    new Vector3(0f, 3f, 0f),   // Top
    new Vector3(-5f, 0f, 0f),  // Left
    new Vector3(5f, 0f, 0f),   // Right
    new Vector3(0f, -3f, 0f)   // Bottom
};
```

### Change Visualizer Style
```csharp
// In AudioVisualizer3D Inspector
barCount = 128;           // More bars
radius = 15f;            // Larger circle
rotateVisualizer = true; // Enable rotation
```

---

## Performance Tips

### For Low-End Devices:
```csharp
// Reduce particle count
NeonBackgroundManager.particleCount = 20;

// Reduce visualizer bars
AudioVisualizer3D.barCount = 32;

// Disable post-processing
// (Remove Post-process Layer component)
```

### For High-End Devices:
```csharp
// Increase quality
NeonBackgroundManager.particleCount = 100;
AudioVisualizer3D.barCount = 128;

// Enable additional effects
// Add more rotating rings
// Add trail renderers
```

---

## Troubleshooting

### Buttons Not Clickable
**Problem:** Raycast not hitting buttons  
**Solution:** 
- Make sure Main Camera has tag "MainCamera"
- Verify BoxCollider is on button GameObject
- Check camera is not behind buttons

### No Audio Visualization
**Problem:** Bars not moving  
**Solution:**
- Assign AudioSource with music playing
- Make sure music clip is assigned
- Check audioSource.isPlaying is true

### Dark Scene
**Problem:** Everything too dark  
**Solution:**
- Add directional light (low intensity: 0.2)
- Enable emissive materials on objects
- Add post-processing bloom

### Low Performance
**Problem:** Frame drops  
**Solution:**
- Reduce barCount in AudioVisualizer3D
- Reduce particleCount in NeonBackgroundManager
- Disable camera idle animation
- Disable post-processing

---

## Advanced Features

### Add Title Logo
```csharp
GameObject titleObj = new GameObject("GameTitle");
TextMeshPro title = titleObj.AddComponent<TextMeshPro>();
title.text = "RHYTHM GAME 3D";
title.fontSize = 8;
title.alignment = TextAlignmentOptions.Center;
titleObj.transform.position = new Vector3(0, 5, 0);

// Add rotation animation
titleObj.AddComponent<RotateAnimation>();
```

### Add Panel Transitions
```csharp
// Smooth camera movement between panels
MenuCamera3D cam = Camera.main.GetComponent<MenuCamera3D>();

// Move to beatmap panel
cam.MoveTo(new Vector3(10f, 0f, -15f), Vector3.zero);

// Return to main menu
cam.ResetToDefault();
```

---

## Next Steps

1. ✅ Run the scene - everything should work!
2. 🎨 Customize colors to match your game theme
3. 🎵 Add background music (assign in MenuMusic AudioSource)
4. 🌟 Add post-processing for bloom effect
5. 📱 Test on target platform and optimize

---

## Support

If you encounter any issues:
1. Check Unity Console for error messages
2. Verify all components are assigned in Inspector
3. Make sure Main Camera exists and is tagged correctly
4. Check that BeatmapSelector exists in scene (for Play button)

**Enjoy your stunning 3D menu!** 🎮✨
