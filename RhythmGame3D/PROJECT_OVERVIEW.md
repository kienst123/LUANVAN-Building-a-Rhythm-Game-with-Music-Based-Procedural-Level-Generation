# RhythmGame3D - Complete Project Documentation

## 📋 Tổng Quan Dự Án

### Thông Tin Cơ Bản
- **Tên dự án**: RhythmGame3D - Building a Rhythm Game with Music-Based Procedural Level Generation
- **Engine**: Unity 2021.3.45f2 Personal
- **Platform**: macOS (có thể build cho Windows/Linux)
- **Ngôn ngữ**: C# (.NET Framework)
- **Thể loại**: 4-Key Rhythm Game (giống osu!mania)
- **Hoàn thành**: 70%

### Mô Tả
Game nhịp điệu 3D với 4 lane, người chơi ấn phím D-F-J-K để hit notes theo nhịp nhạc. Game có khả năng tự động generate beatmap từ file nhạc hoặc load beatmap .osu có sẵn.

---

## 🎯 Tính Năng Chính

### 1. **3D Menu System** (Beat Saber Style)
- Main menu 3D với tunnel background animation
- Audio visualizer 64 bars phản ứng theo nhạc nền
- 3 panels: BEATMAP, SETTINGS, EXIT
- Camera parallax effects khi di chuyển chuột

### 2. **Beatmap Selector 3D**
- File browser chọn nhạc (.mp3, .ogg, .wav)
- Auto-generate beatmap với 3 độ khó:
  - **EASY**: 40+ notes, khoảng cách 1.0 beat
  - **NORMAL**: 70+ notes, khoảng cách 0.75 beat
  - **HARD**: 120+ notes, khoảng cách 0.5 beat
- Random lane placement (không theo thứ tự)
- Hiển thị số notes và độ dài bài hát

### 3. **Settings Manager 3D**
- 3 volume sliders: Master, Music, SFX
- Keyboard controls: +/- điều chỉnh, 1-2-3 chuyển slider
- Visual feedback với thanh trượt 3D
- Auto-save vào PlayerPrefs

### 4. **Gameplay System**
- 4-lane gameplay (D, F, J, K keys)
- Note spawning với random colors (4 màu)
- Judgment system: Perfect/Great/Good/Miss
- Combo system với multiplier
- Health system (tăng khi hit, giảm khi miss)
- Empty press penalty (giảm HP khi ấn sai)

### 5. **Visual Effects**
- **Tunnel Background**: 
  - 20 vertical lines (purple)
  - 30 horizontal rings (cyan)
  - 3 hexagon layers
  - 30 floating particles
  - Brightness tăng theo combo
  - Pulse animation khi Perfect hit
- **Note Effects**:
  - Trail renderer
  - Hit particles
  - Emission glow
  - Random colors per note

### 6. **Results Screen 3D**
- Hiển thị sau khi hết nhạc:
  - Score (điểm số)
  - Accuracy (độ chính xác %)
  - Max Combo
  - Grade (S/A/B/C/D/F)
  - Judgment counts (Perfect/Great/Good/Miss)
- 2 buttons: RETRY (chơi lại) và MENU (về menu)

### 7. **Audio System**
- Music playback với sync chính xác
- Audio offset compensation
- Hit sound effects
- Combo break sound
- Volume controls cho từng kênh

---

## 🏗️ Kiến Trúc Hệ Thống

### Core Architecture

```
RhythmGame3D/
├── Core/
│   ├── GameManager3D.cs          # Main game controller
│   └── AudioManager3D.cs          # Audio management
│
├── Beatmap/
│   ├── BeatmapData.cs            # Beatmap data structure
│   ├── BeatmapParser.cs          # Parse .osu files
│   └── HitObject.cs              # Note data structure
│
├── Gameplay/
│   ├── NoteSpawner3D.cs          # Spawn notes
│   ├── NoteController3D.cs       # Individual note behavior
│   ├── InputManager3D.cs         # Keyboard input handling
│   ├── JudgmentSystem.cs         # Timing judgment
│   └── GameplayTunnelBackground.cs # Visual tunnel effects
│
├── UI/
│   ├── ModernUIManager3D.cs      # In-game UI
│   ├── ResultsScreen3D.cs        # End game results
│   └── Menu3D/
│       ├── MainMenu3DManager.cs   # Main menu controller
│       ├── MenuButton3D.cs        # Interactive 3D buttons
│       ├── BeatmapSelector3D.cs   # Beatmap selection
│       ├── SettingsManager3D.cs   # Settings panel
│       ├── BeatmapStorage.cs      # Data transfer between scenes
│       └── AudioVisualizer3D.cs   # Menu audio visualizer
│
└── Visual/
    └── TunnelBackground3D.cs      # Menu tunnel background
```

### Data Flow

```
MainMenu Scene:
1. User clicks BEATMAP button
2. BeatmapSelector3D opens
3. User selects difficulty (Easy/Normal/Hard)
4. User clicks SELECT → file browser opens
5. User chooses .mp3 file
6. System loads AudioClip and gets duration
7. Auto-generate beatmap based on difficulty
8. Store in BeatmapStorage.currentBeatmap
9. User clicks PLAY
10. Load GameScene

GameScene:
1. GameManager3D.Initialize()
2. Load beatmap from BeatmapStorage
3. NoteSpawner3D.LoadBeatmap()
4. Start music playback
5. Spawn notes based on song time
6. InputManager3D detects key presses
7. JudgmentSystem calculates timing
8. Update score, combo, health
9. When song ends → ResultsScreen3D
10. User clicks RETRY or MENU
```

---

## 📦 Chi Tiết Từng Component

### 1. GameManager3D.cs
**Vai trò**: Controller chính của gameplay scene

**Nhiệm vụ**:
- Initialize tất cả systems
- Load beatmap từ BeatmapStorage
- Quản lý game state (playing, paused, ended)
- Track statistics (perfect count, great count, etc.)
- Update UI mỗi frame
- Detect song end và show results

**Key Methods**:
```csharp
void Initialize()                    // Setup all systems
void LoadBeatmap(string path)        // Load beatmap from file
void StartSong()                     // Begin gameplay
void OnJudgmentReceived(result)      // Handle hit/miss
void OnComboChanged(combo)           // Update combo effects
void OnSongEnd()                     // Show results screen
void UpdateUI()                      // Update score/accuracy/time
```

**State Variables**:
```csharp
private BeatmapData currentBeatmap;
private bool isPlaying;
private float songTime;
private float currentHealth;
private int perfectCount, greatCount, goodCount, missCount;
private int maxCombo;
private int totalNotes;
```

---

### 2. NoteSpawner3D.cs
**Vai trò**: Quản lý việc spawn notes

**Cơ chế hoạt động**:
1. Nhận BeatmapData từ GameManager
2. Theo dõi song time
3. Khi note.time - spawnDistance ≤ songTime → spawn note
4. Tạo instance từ prefab (TapNote hoặc LongNote)
5. Set màu random từ noteColors array
6. Track active notes để cleanup

**Key Features**:
- Spawn distance: 50 units
- Note speed: 20 units/second
- 4 colors: Cyan, Pink, Yellow, Green
- Prevent spawn overlap

**Code Example**:
```csharp
void SpawnNote(HitObject hitObject)
{
    // 1. Validate lane
    if (hitObject.lane < 0 || hitObject.lane >= 4) return;
    
    // 2. Choose prefab
    GameObject prefab = tapNotePrefab;
    
    // 3. Instantiate
    GameObject noteObj = Instantiate(prefab, transform);
    NoteController3D controller = noteObj.GetComponent<NoteController3D>();
    
    // 4. Calculate position
    float xPos = lanes[hitObject.lane].position.x;
    Vector3 spawnPos = new Vector3(xPos, 0.5f, spawnDistance);
    noteObj.transform.position = spawnPos;
    
    // 5. Random color
    Color randomColor = noteColors[Random.Range(0, 4)];
    
    // 6. Initialize
    controller.Initialize(hitObject, noteSpeed, hitPosition);
    controller.SetColor(randomColor);
    
    // 7. Track
    activeNotes.Add(controller);
    laneNotes[hitObject.lane].Add(controller);
}
```

---

### 3. NoteController3D.cs
**Vai trò**: Điều khiển từng note riêng lẻ

**Lifecycle**:
1. **Initialize**: Nhận HitObject, speed, hit position
2. **SetColor**: Apply màu random
3. **Update**: Di chuyển về phía camera
4. **OnHit**: Xử lý khi hit thành công
5. **OnMiss**: Xử lý khi miss
6. **Destroy**: Cleanup sau khi hit/miss

**Visual Components**:
- MeshRenderer: Main note body
- TrailRenderer: Trailing effect
- ParticleSystem: Hit explosion
- Material: Emission glow

**Movement Logic**:
```csharp
void Update()
{
    if (!isMoving) return;
    
    // Move towards camera
    transform.position += Vector3.back * speed * Time.deltaTime;
    
    // Check if missed
    if (transform.position.z < hitPositionZ - 2f && !isHit)
    {
        OnMiss();
    }
}
```

---

### 4. InputManager3D.cs
**Vai trò**: Xử lý input và phát hiện hits

**Input Mapping**:
```
Lane 0: D key
Lane 1: F key
Lane 2: J key
Lane 3: K key
```

**Hit Detection Logic**:
```csharp
void Update()
{
    // Check each lane
    for (int lane = 0; lane < 4; lane++)
    {
        if (Input.GetKeyDown(laneKeys[lane]))
        {
            HandleLanePress(lane);
        }
    }
}

void HandleLanePress(int lane)
{
    // 1. Get closest note in lane
    NoteController3D note = noteSpawner.GetClosestNoteInLane(
        lane, 
        hitPosition, 
        maxHitDistance
    );
    
    // 2. If note found
    if (note != null)
    {
        // Calculate timing difference
        float timeDiff = Mathf.Abs(note.hitObject.time - songTime);
        
        // Judge timing
        JudgmentResult result = judgmentSystem.Judge(timeDiff, note);
        
        // Apply result
        note.OnHit(result.judgment);
        noteSpawner.RemoveNote(note);
    }
    else
    {
        // Empty press - penalty
        OnEmptyPress?.Invoke();
    }
}
```

---

### 5. JudgmentSystem.cs
**Vai trò**: Đánh giá timing và tính điểm

**Timing Windows** (milliseconds):
```csharp
Perfect: ≤ 50ms   → 300 points + combo
Great:   ≤ 100ms  → 200 points + combo
Good:    ≤ 150ms  → 100 points + combo
Miss:    > 150ms  → 0 points, break combo
```

**Scoring Formula**:
```csharp
int CalculateScore(string judgment)
{
    int baseScore = GetBaseScore(judgment); // 300/200/100/0
    int comboBonus = Mathf.Min(combo, 100); // Max +100
    return baseScore + comboBonus;
}
```

**Accuracy Calculation**:
```csharp
float accuracy = (perfect * 300 + great * 200 + good * 100) 
                 / (totalNotes * 300) * 100;
```

---

### 6. BeatmapSelector3D.cs
**Vai trò**: Chọn nhạc và tự động generate beatmap

**Auto-Generation Algorithm**:

```csharp
IEnumerator GenerateBeatmapCoroutine(string musicPath)
{
    // 1. Load AudioClip để lấy duration
    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(
        "file://" + musicPath, 
        AudioType.UNKNOWN))
    {
        yield return www.SendWebRequest();
        
        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
        float songDuration = clip.length; // VD: 154 giây
        
        // 2. Tính số notes dựa trên độ khó
        float bpm = 120f;
        float beatDuration = 60f / bpm; // 0.5s
        
        float noteSpacing;
        switch (currentDifficulty)
        {
            case 0: noteSpacing = 1.0f;  break;  // Easy
            case 1: noteSpacing = 0.75f; break;  // Normal
            case 2: noteSpacing = 0.5f;  break;  // Hard
        }
        
        float timePerNote = beatDuration * noteSpacing;
        int noteCount = Mathf.FloorToInt(songDuration / timePerNote);
        // Easy (154s): 154 / 0.5 = 308 notes
        // Normal (154s): 154 / 0.375 = 410 notes
        // Hard (154s): 154 / 0.25 = 616 notes
        
        // 3. Generate notes với random lanes
        int lastLane = -1;
        
        for (int i = 0; i < noteCount; i++)
        {
            // Random lane (tránh spam 1 lane)
            int lane;
            do {
                lane = Random.Range(0, 4);
            } while (lane == lastLane);
            
            lastLane = lane;
            
            // Calculate position và time
            int xPos = 64 + (lane * 128); // osu!mania standard
            int noteTime = Mathf.RoundToInt(i * timePerNote * 1000);
            
            // Create HitObject
            HitObject note = new HitObject(xPos, 192, noteTime, 1, 0);
            beatmap.AddHitObject(note);
        }
        
        // 4. Store để GameScene sử dụng
        BeatmapStorage.currentBeatmap = beatmap;
        BeatmapStorage.currentMusicPath = musicPath;
    }
}
```

**Key Features**:
- Tự động phát hiện độ dài bài hát
- Generate notes đều đặn theo beat
- Random lane nhưng tránh spam
- Chỉ tạo tap notes (không có long notes)
- Số lượng notes tỷ lệ với độ dài bài

---

### 7. GameplayTunnelBackground.cs
**Vai trò**: Visual effects tunnel trong gameplay

**Components**:

1. **Vertical Lines** (20 lines):
```csharp
for (int i = 0; i < 20; i++)
{
    float angle = i * 18f; // 360° / 20 = 18°
    Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
    Vector3 start = direction * 5f;
    Vector3 end = direction * 100f;
    
    LineRenderer line = CreateLine();
    line.SetPositions(new Vector3[] { start, end });
    line.startColor = purple;
    line.endColor = purple;
}
```

2. **Horizontal Rings** (30 rings):
```csharp
for (int i = 0; i < 30; i++)
{
    float z = 10f + i * 3f; // Every 3 units
    LineRenderer ring = CreateCircle(z, radius: 8f, segments: 40);
    ring.startColor = cyan;
}
```

3. **Hexagons** (3 layers):
```csharp
for (int layer = 0; layer < 3; layer++)
{
    float z = -2f + layer * 1f;
    float scale = 1f + layer * 0.3f;
    GameObject hexagon = CreateHexagon(z, scale);
}
```

4. **Particles** (30 floating dots):
```csharp
ParticleSystem.MainModule main = particles.main;
main.maxParticles = 30;
main.startSpeed = new MinMaxCurve(0.5f, 2f);
main.startSize = new MinMaxCurve(0.1f, 0.3f);
```

**Dynamic Effects**:

```csharp
// Brightness tăng theo combo
public void SetIntensity(float intensity)
{
    float brightness = Mathf.Lerp(0.3f, 1.0f, intensity);
    
    foreach (LineRenderer line in verticalLines)
        line.material.SetColor("_EmissionColor", color * brightness);
}

// Pulse khi Perfect hit
public void PulseOnBeat()
{
    StartCoroutine(PulseCoroutine());
}

IEnumerator PulseCoroutine()
{
    foreach (GameObject hex in hexagons)
    {
        Vector3 original = hex.transform.localScale;
        hex.transform.localScale = original * 1.2f;
        yield return new WaitForSeconds(0.1f);
        hex.transform.localScale = original;
    }
}
```

---

### 8. ResultsScreen3D.cs
**Vai trò**: Hiển thị kết quả sau khi hết nhạc

**UI Layout**:
```
┌─────────────────────────────┐
│         RESULTS             │ (Title, fontSize: 12)
├─────────────────────────────┤
│    SCORE: 48,600            │ (fontSize: 6, yellow)
│    ACCURACY: 77.51%         │ (fontSize: 4, white)
│    MAX COMBO: 29x           │ (fontSize: 4, cyan)
├─────────────────────────────┤
│            B                │ (Grade, fontSize: 10)
├─────────────────────────────┤
│    PERFECT: 15              │ (cyan)
│    GREAT: 8                 │ (green)
│    GOOD: 3                  │ (yellow)
│    MISS: 4                  │ (red)
├─────────────────────────────┤
│   [RETRY]      [MENU]       │ (Buttons)
└─────────────────────────────┘
```

**Grade System**:
```csharp
string CalculateGrade(float accuracy)
{
    if (accuracy >= 95f) return "S";  // Gold
    if (accuracy >= 90f) return "A";  // Bright green
    if (accuracy >= 80f) return "B";  // Cyan
    if (accuracy >= 70f) return "C";  // Yellow
    if (accuracy >= 60f) return "D";  // Orange
    return "F";                       // Red
}
```

**Show Results**:
```csharp
public void ShowResults(
    int finalScore,       // From JudgmentSystem.totalScore
    float accuracy,       // From JudgmentSystem.accuracy
    int maxCombo,         // Tracked in GameManager3D
    int perfect,          // Counted in GameManager3D
    int great,
    int good,
    int miss)
{
    gameObject.SetActive(true);
    
    scoreText.text = $"SCORE: {finalScore:N0}";
    accuracyText.text = $"ACCURACY: {accuracy:F2}%";
    maxComboText.text = $"MAX COMBO: {maxCombo}x";
    
    perfectCountText.text = $"PERFECT: {perfect}";
    greatCountText.text = $"GREAT: {great}";
    goodCountText.text = $"GOOD: {good}";
    missCountText.text = $"MISS: {miss}";
    
    string grade = CalculateGrade(accuracy);
    gradeText.text = grade;
    gradeText.color = GetGradeColor(grade);
}
```

---

### 9. MainMenu3DManager.cs
**Vai trò**: Quản lý main menu 3D

**Menu Structure**:
```
Main Menu
├── Tunnel Background (animated)
├── Audio Visualizer (64 bars)
├── Title Text "RHYTHM GAME 3D"
└── Buttons:
    ├── BEATMAP → Opens BeatmapSelector3D panel
    ├── SETTINGS → Opens SettingsManager3D panel
    └── EXIT → Quit application

Panels System:
- Only 1 panel active at a time
- BACK button returns to main buttons
- Smooth transitions
```

**Panel Creation**:
```csharp
void CreateBeatmapPanel3D()
{
    GameObject panel = CreatePanel3D("BeatmapPanel");
    
    // Add BeatmapSelector3D component
    BeatmapSelector3D selector = panel.AddComponent<BeatmapSelector3D>();
    selector.menuManager = this;
    
    // Create BACK button
    CreateBackButton(panel);
    
    panel.SetActive(false);
    beatmapPanel = panel;
}
```

**Button Click Handling**:
```csharp
void OnBeatmapButtonClicked()
{
    // Hide main buttons
    foreach (var btn in mainButtons)
        btn.SetActive(false);
    
    // Show beatmap panel
    beatmapPanel.SetActive(true);
    
    Debug.Log("[MainMenu3D] Opened Beatmap Panel");
}
```

---

### 10. BeatmapStorage.cs
**Vai trò**: Static storage để transfer data giữa scenes

**Why needed?**
- Unity scenes không share instance variables
- BeatmapSelector3D (MainMenu) → GameManager3D (GameScene)
- Cần lưu beatmap và music path

**Implementation**:
```csharp
public static class BeatmapStorage
{
    public static BeatmapData currentBeatmap { get; set; }
    public static string currentMusicPath { get; set; }
    
    public static void Clear()
    {
        currentBeatmap = null;
        currentMusicPath = null;
    }
}
```

**Usage**:
```csharp
// In BeatmapSelector3D (MainMenu scene):
BeatmapStorage.currentBeatmap = generatedBeatmap;
BeatmapStorage.currentMusicPath = selectedMusicFile;
SceneManager.LoadScene("GameScene");

// In GameManager3D (GameScene):
if (BeatmapStorage.currentBeatmap != null)
{
    currentBeatmap = BeatmapStorage.currentBeatmap;
    LoadMusicFromFile(BeatmapStorage.currentMusicPath);
}
```

---

## 🔧 Cấu Hình & Settings

### Unity Project Settings

**Quality Settings**:
```
Anti Aliasing: 4x MSAA
VSync: Disabled (để có FPS cao)
Shadow Quality: Medium
Texture Quality: Full Resolution
```

**Audio Settings**:
```
DSP Buffer Size: Best Latency
Sample Rate: 48000 Hz
Virtual Voice Count: 512
Real Voice Count: 32
```

**Player Settings**:
```
Color Space: Linear
Graphics API: Metal (macOS)
Scripting Backend: Mono
API Compatibility: .NET Framework
```

### Performance Optimization

**Target Performance**:
- FPS: 60+ (stable)
- Input latency: < 10ms
- Audio sync accuracy: ±5ms

**Optimization Techniques**:
1. Object pooling cho particles
2. Material instances để tránh batching breaks
3. Coroutines cho async operations
4. Cached component references
5. Minimal Update() operations

---

## 🎨 Visual Design

### Color Palette

**Menu Colors**:
```csharp
Background: (0, 0, 0) Black
Primary: (0, 0.94, 1) Cyan
Secondary: (1, 0, 1) Magenta
Accent: (1, 1, 0) Yellow
Text: (1, 1, 1) White
```

**Note Colors** (Random):
```csharp
Color 1: (0, 0.94, 1) Cyan
Color 2: (1, 0.3, 0.8) Pink
Color 3: (1, 1, 0.3) Yellow
Color 4: (0.3, 1, 0.5) Green
```

**Judgment Colors**:
```csharp
Perfect: (0, 1, 1) Cyan
Great: (0, 1, 0) Green
Good: (1, 1, 0) Yellow
Miss: (1, 0, 0) Red
```

### UI Typography

**Font Sizes** (3D Text):
```
Title: 8-12 units
Subtitle: 4-6 units
Body: 2.5-3 units
Button Text: 2.5-3 units
Info Text: 2-2.5 units
```

---

## 🎮 Gameplay Mechanics

### Difficulty Progression

| Difficulty | Note Spacing | Notes per Minute | Skill Required |
|------------|--------------|------------------|----------------|
| Easy       | 1.0 beat     | 120 NPM          | Beginner       |
| Normal     | 0.75 beat    | 160 NPM          | Intermediate   |
| Hard       | 0.5 beat     | 240 NPM          | Advanced       |

### Scoring System

**Base Points**:
- Perfect: 300 points
- Great: 200 points
- Good: 100 points
- Miss: 0 points

**Combo Bonus**:
- +1 point per combo (max +100)

**Example Score Calculation**:
```
Song: 100 notes
Perfect: 70 notes = 70 × 300 = 21,000
Great: 20 notes = 20 × 200 = 4,000
Good: 8 notes = 8 × 100 = 800
Miss: 2 notes = 2 × 0 = 0

Base Score: 25,800
Combo Bonus: ~1,500 (average)
Total: ~27,300 points
Accuracy: (70×300 + 20×200 + 8×100) / (100×300) = 85.33%
Grade: B
```

### Health System

**Mechanics**:
```
Starting Health: 100%
Perfect hit: +2% HP
Great hit: +1% HP
Good hit: +0.5% HP
Miss: -5% HP
Empty press: -2% HP

Game Over: HP ≤ 0%
```

---

## 📂 File Formats

### .osu Beatmap Format (Supported)

```ini
[General]
AudioFilename: song.mp3
Mode: 3

[Metadata]
Title: Song Title
Artist: Artist Name
Creator: Mapper Name

[Difficulty]
HPDrainRate: 5
CircleSize: 4
OverallDifficulty: 7
ApproachRate: 5

[TimingPoints]
0,500,4,1,0,100,1,0

[HitObjects]
64,192,1000,1,0
192,192,1500,1,0
320,192,2000,128,0:2500
```

**HitObject Format**:
```
x,y,time,type,hitSound[:endTime]

x: Lane position (64, 192, 320, 448 for 4K)
y: Always 192 (ignored in mania)
time: Hit time in milliseconds
type: 1 = tap note, 128 = long note
hitSound: Sound effect (usually 0)
endTime: For long notes only
```

---

## 🐛 Known Issues & Limitations

### Current Limitations

1. **Auto-Generated Beatmaps**:
   - Không có audio analysis
   - Notes không sync với beat thật
   - Không có patterns phức tạp
   - Chỉ có tap notes (no long notes)

2. **File Browser**:
   - Chỉ hoạt động trong Unity Editor
   - Build runtime cần custom file picker
   - Không có file preview

3. **Performance**:
   - Particle system có thể lag trên máy yếu
   - Tunnel background CPU intensive
   - Nhiều notes cùng lúc có thể drop FPS

4. **Audio**:
   - Không hỗ trợ .flac, .wav lớn
   - Không có audio effects (echo, reverb)
   - Volume controls không có fade

### Bug Fixes Applied

✅ **Long Note Display Bug**:
- Problem: Long notes hiển thị quá dài (20+ units)
- Solution: Removed long notes from auto-generation

✅ **Text Size Too Small**:
- Problem: Không đọc được text trong 3D menu
- Solution: Tăng font size từ 1.5-4 lên 2.5-8

✅ **Panel Background Blocking View**:
- Problem: Blue panel che tunnel background
- Solution: `bgPanel.SetActive(false)`

✅ **Beatmap Not Found**:
- Problem: GameScene không nhận beatmap từ menu
- Solution: Created BeatmapStorage static class

✅ **Sequential Lane Pattern**:
- Problem: Notes theo thứ tự 0-1-2-3 (quá dễ)
- Solution: Random lane với anti-spam logic

---

## 🚀 Future Improvements

### Planned Features (30% remaining)

#### 1. **Audio Analysis System**
```csharp
// Beat detection từ waveform
public class AudioAnalyzer
{
    public List<float> DetectBeats(AudioClip clip)
    {
        // FFT analysis
        // Peak detection
        // BPM calculation
        return beatTimestamps;
    }
}
```

#### 2. **Advanced Beatmap Generator**
- Onset detection (phát hiện note starts trong nhạc)
- Energy level mapping (độ mạnh → difficulty)
- Melody tracking (theo giai điệu)
- Pattern generation (jacks, stairs, streams)

#### 3. **Long Notes Support**
- Hold detection với key down/up
- Visual feedback khi holding
- Score multiplier khi hold perfect
- Miss nếu release sớm

#### 4. **Multiplayer Mode**
- Local split-screen
- Online leaderboards
- Ghost replay system
- Real-time battles

#### 5. **More Visual Effects**
- Screen shake khi miss
- Color flash khi full combo
- Slow motion khi critical moment
- Custom note skins

#### 6. **Enhanced UI**
- Song search & filter
- Beatmap preview
- Difficulty stars rating
- Achievement system

#### 7. **Modifiers**
- Speed up/down (1.5x, 2x)
- No fail mode
- Hidden (notes disappear)
- Sudden death (1 miss = game over)

---

## 📊 Project Statistics

### Code Metrics

```
Total Scripts: 25 files
Lines of Code: ~5,000 lines
C# Classes: 25
Coroutines: 3
Events/Delegates: 8
```

### File Structure

```
Assets/
├── Scenes/ (2 scenes)
│   ├── MainMenu.unity
│   └── GameScene.unity
│
├── Scripts/ (25 .cs files)
│   ├── Core/ (2 files, ~800 lines)
│   ├── Beatmap/ (3 files, ~600 lines)
│   ├── Gameplay/ (5 files, ~1,500 lines)
│   ├── UI/ (10 files, ~2,000 lines)
│   └── Visual/ (2 files, ~500 lines)
│
├── Prefabs/ (5 prefabs)
│   ├── TapNote.prefab
│   ├── LongNote.prefab
│   ├── HitParticle.prefab
│   ├── Lane.prefab
│   └── MenuButton3D.prefab
│
├── Materials/ (15 materials)
│   ├── NoteMaterial.mat
│   ├── TunnelLine.mat
│   ├── HexagonGlow.mat
│   └── ...
│
└── Sounds/ (8 audio files)
    ├── HitSound.wav
    ├── PerfectSound.wav
    ├── ComboBreak.wav
    └── ...
```

---

## 🎓 Learning Resources

### Unity Concepts Used

1. **Scene Management**
   - SceneManager.LoadScene()
   - DontDestroyOnLoad()
   - Static classes for data transfer

2. **Audio**
   - AudioSource.Play/Stop/Pause
   - AudioClip loading via UnityWebRequest
   - Volume mixing

3. **Input System**
   - Input.GetKeyDown()
   - Event-driven input handling
   - Timing window detection

4. **UI**
   - TextMeshPro for 3D text
   - Canvas WorldSpace rendering
   - Interactive 3D buttons với raycasting

5. **Visual Effects**
   - LineRenderer for tunnel
   - ParticleSystem for explosions
   - Material emission glow
   - Coroutines for animations

6. **Data Structures**
   - Lists, Dictionaries
   - Custom classes (BeatmapData, HitObject)
   - Static storage patterns

7. **Design Patterns**
   - Singleton (GameManager)
   - Observer (Events/Delegates)
   - Object Pool (Particles)
   - State Machine (Game states)

---

## 🎯 How to Play

### Controls

**Gameplay**:
```
D key - Lane 0 (leftmost)
F key - Lane 1
J key - Lane 2
K key - Lane 3 (rightmost)

SPACE - Start/Pause
R - Restart song
ESC - Stop and return to menu
```

**Menu**:
```
Mouse - Hover over buttons
Left Click - Select/Click buttons
+/- keys - Adjust volume (in Settings)
1/2/3 keys - Switch volume slider (in Settings)
```

### Gameplay Tips

1. **Timing is Key**:
   - Perfect window: ±50ms
   - Watch for judgment text feedback
   - Listen to hit sounds

2. **Maintain Combo**:
   - Combo increases score multiplier
   - One miss breaks combo
   - Focus on consistency over speed

3. **Choose Right Difficulty**:
   - Easy: Học cách chơi
   - Normal: Practice accuracy
   - Hard: Speed & reaction training

4. **Health Management**:
   - Don't spam keys (empty press penalty)
   - Perfect hits heal more
   - Miss costs significant HP

---

## 📝 Development Timeline

### Version History

**v0.1 (Initial)** - Complete RhythmGame3D Project
- Basic 4K gameplay
- Note spawning & judgment
- .osu file support
- Simple UI

**v0.5** - UI Improvements
- Hiding panel backgrounds
- 3D menu system
- Tunnel backgrounds

**v0.6** - Feature Additions
- Difficulty selection (Easy/Normal/Hard)
- Auto beatmap generation
- Text size improvements

**v0.7 (Current - 70%)** - Major Updates
- Results screen với grade system
- Random lane generation
- Judgment tracking
- Long note fixes
- Full song duration support
- Audio duration detection
- Enhanced visual feedback

**v1.0 (Planned - 100%)**
- Audio analysis integration
- Long notes support
- Advanced patterns
- Leaderboards
- Achievement system

---

## 🤝 Credits & References

### Inspired By
- **osu!mania** - Gameplay mechanics & beatmap format
- **Beat Saber** - Visual style & 3D UI concept
- **Cytus II** - Judgment system & scoring

### Technologies Used
- **Unity 2021.3 LTS** - Game engine
- **TextMeshPro** - Advanced text rendering
- **C# .NET Framework** - Programming language
- **UnityWebRequest** - Audio file loading

### Assets & Resources
- All code written from scratch
- Materials created using Unity Standard Shader
- Sound effects generated using synthesizers
- No external packages required

---

## 📞 Support & Documentation

### Getting Help

**Documentation**:
- This file (PROJECT_OVERVIEW.md)
- Code comments inline
- Unity tooltips in Inspector

**Debugging**:
- Console logs với [ComponentName] prefix
- Error handling với try-catch
- Validation checks throughout

**Common Issues**:

Q: Notes không spawn?
A: Kiểm tra beatmap có hitObjects không, và song time có tăng không

Q: Audio không play?
A: Kiểm tra AudioSource component và file path

Q: Input không hoạt động?
A: Verify key bindings và InputManager enabled

Q: Performance lag?
A: Giảm particle count, disable tunnel background

---

## 🎬 Conclusion

**Project Status**: 70% Complete

**Achieved Goals**:
✅ Core gameplay hoàn chỉnh
✅ 3D menu system đẹp mắt
✅ Auto-generation beatmap
✅ Results screen với statistics
✅ Multiple difficulty levels
✅ Random lane patterns
✅ Health & combo systems

**Remaining Work** (30%):
⏳ Audio analysis integration
⏳ Long notes mechanics
⏳ Advanced pattern generation
⏳ Online features
⏳ Polish & optimization

**Key Achievements**:
- Fully functional 4K rhythm game
- Clean, modular code architecture
- Smooth 60+ FPS performance
- Intuitive 3D UI/UX
- Comprehensive documentation

---

**Last Updated**: December 12, 2025
**Version**: 0.7 (70%)
**Repository**: https://github.com/kienst123/LUANVAN-Building-a-Rhythm-Game-with-Music-Based-Procedural-Level-Generation
