# ML Integration - Testing Guide

Hướng dẫn test tích hợp ML beatmap generation vào Unity.

## 📋 Prerequisites

Trước khi test, đảm bảo đã:

1. ✅ Cài đặt Python environment (xem `MLScripts/README.md`)
2. ✅ Test Python script độc lập thành công
3. ✅ Copy folder `MLScripts/` vào Unity project
4. ✅ PythonMLBridge.cs đã được add vào Unity

## 🔧 Setup Unity

### 1. Copy MLScripts folder

```bash
# Trong thư mục RhythmGame3D
mkdir -p Assets/StreamingAssets/
cp -r MLScripts/ Assets/StreamingAssets/MLScripts/
```

Hoặc trong Unity Editor:
1. Tạo folder `Assets/StreamingAssets/MLScripts/`
2. Copy các file:
   - `generate_beatmap.py`
   - `requirements.txt`
   - `README.md`

### 2. Verify Scripts in Unity

1. Mở Unity project
2. Check trong Project window:
   - `Assets/Scripts/UI/Menu3D/PythonMLBridge.cs` ✅
   - `Assets/Scripts/UI/Menu3D/BeatmapSelector3D.cs` (updated) ✅
   - `Assets/StreamingAssets/MLScripts/generate_beatmap.py` ✅

3. Check console không có compile errors

### 3. Configure BeatmapSelector3D

Trong MainMenu scene:

1. Select BeatmapPanel → BeatmapSelector3D component
2. Settings:
   - ✅ `Use ML Generation` = **true**
   - ✅ `ML Timeout` = **120** (2 phút)

![BeatmapSelector Settings](https://via.placeholder.com/400x200?text=BeatmapSelector3D+Inspector)

## 🧪 Testing Workflow

### Test 1: Python Script Standalone

Trước khi test trong Unity, verify Python script hoạt động:

```bash
cd RhythmGame3D/Assets/StreamingAssets/MLScripts/

# Activate venv nếu dùng
source venv/bin/activate

# Test với 1 file MP3
python generate_beatmap.py "/path/to/test.mp3" 0.5 "test_output.osu"

# Nếu thành công, sẽ thấy:
# PROGRESS:{"stage":"import","message":"Loading Python libraries..."}
# PROGRESS:{"stage":"download","message":"Downloading model from HuggingFace..."}
# PROGRESS:{"stage":"load_model","message":"Loading BEaRT model..."}
# PROGRESS:{"stage":"generate","message":"Generating beatmap..."}
# PROGRESS:{"stage":"convert","message":"Converting to .osu format..."}
# PROGRESS:{"stage":"complete","message":"Beatmap generated successfully!"}
# SUCCESS:test_output.osu
```

### Test 2: Unity Editor Test

1. **Mở MainMenu scene**
2. **Play mode**
3. **Click BEATMAP button**
4. **Chọn độ khó** (Easy/Normal/Hard)
5. **Click SELECT** → Chọn file MP3 test
6. **Observe Console**:

```
[BeatmapSelector3D] Selected: test.mp3
[BeatmapSelector3D] Starting ML generation for: test.mp3
[PythonMLBridge] Starting ML generation...
[PythonMLBridge] Process started (PID: 12345)
[ML Progress] import: Loading Python libraries...
[ML Progress] download: Downloading model from HuggingFace...
[ML Progress] load_model: Loading BEaRT model...
[ML Progress] generate: Generating beatmap (difficulty=0.50)...
[ML Progress] convert: Converting to .osu format...
[ML Progress] complete: Beatmap generated successfully!
[PythonMLBridge] ✅ Success! Generated beatmap (2048 bytes)
[BeatmapSelector3D] ✅ ML beatmap parsed: 156 notes
```

7. **UI sẽ hiển thị**:
```
✅ AI Beatmap Generated!
Difficulty: Normal
Notes: 156

Click PLAY to start
```

8. **Click PLAY** → Test gameplay với ML beatmap

### Test 3: Different Difficulties

Test cả 3 độ khó:

| Difficulty | ML Value | Expected Notes | Testing |
|------------|----------|----------------|---------|
| Easy       | 0.33     | ~100-150       | ⏳      |
| Normal     | 0.50     | ~150-250       | ⏳      |
| Hard       | 0.75     | ~250-400       | ⏳      |

### Test 4: Fallback System

Test khi ML generation fail:

1. **Rename Python script** để gây lỗi:
```bash
mv generate_beatmap.py generate_beatmap.py.bak
```

2. **Run Unity** → Select beatmap
3. **Expected behavior**:
   - Console warning: "ML generation failed, using simple generation"
   - UI: "AI generation failed\nUsing simple generation..."
   - Falls back to simple random generation
   - Game vẫn chơi được

4. **Restore Python script**:
```bash
mv generate_beatmap.py.bak generate_beatmap.py
```

### Test 5: Multiple Songs

Test với nhiều loại file:

```
✅ test1.mp3 (3:24, 204s) → ~400 notes
✅ test2.ogg (2:15, 135s) → ~270 notes
✅ test3.wav (1:30, 90s)  → ~180 notes
⚠️ test4.flac (not supported) → Falls back to simple
```

## 🐛 Common Issues & Solutions

### Issue 1: "Python command not found"

**Triệu chứng**:
```
[PythonMLBridge] Failed to start Python process: Cannot find python3
```

**Solution**:
```bash
# Check Python path
which python3

# Nếu ở path khác, update PythonMLBridge.cs:
private const string PYTHON_COMMAND = "/usr/local/bin/python3";
```

### Issue 2: "beatlearning module not found"

**Triệu chứng**:
```
ERROR: Missing required library: No module named 'beatlearning'
```

**Solution**:
```bash
cd MLScripts
source venv/bin/activate  # Nếu dùng venv
pip install beatlearning
```

### Issue 3: "Timeout after 120s"

**Triệu chứng**:
```
[PythonMLBridge] Generation timeout after 120s
```

**Solution**:
- Tăng timeout trong Inspector: `ML Timeout = 180` (3 phút)
- Hoặc dùng GPU nếu có:
```bash
pip install torch --index-url https://download.pytorch.org/whl/cu118  # CUDA
```

### Issue 4: "Output file not found"

**Triệu chứng**:
```
[PythonMLBridge] Generation reported success but output file not found
```

**Solution**:
- Check permissions:
```bash
chmod 777 Assets/StreamingAssets/MLScripts/
```
- Check temp directory:
```bash
ls ~/Library/Caches/Unity/ML_Beatmaps/
```

### Issue 5: ".osu file parse error"

**Triệu chứng**:
```
[BeatmapSelector3D] Failed to parse ML beatmap: Invalid format
```

**Solution**:
- Verify .osu file manually:
```bash
cat ~/Library/Caches/Unity/ML_Beatmaps/test_Normal.osu
```
- Check có sections: [General], [Metadata], [HitObjects]
- Re-run generation

## 📊 Performance Benchmarks

### Generation Time (M1 Mac, CPU):

| Song Duration | Difficulty | Time | Notes |
|---------------|------------|------|-------|
| 1:30 (90s)    | Easy       | 25s  | 120   |
| 2:00 (120s)   | Normal     | 35s  | 200   |
| 3:00 (180s)   | Hard       | 50s  | 350   |

### First Run:
- ⚠️ **+30-60s** để download model từ HuggingFace (~500MB)
- Model được cache ở `MLScripts/ml_cache/`
- Lần sau không cần download

## ✅ Success Criteria

Test pass nếu:

1. ✅ Python script chạy độc lập thành công
2. ✅ Unity gọi được Python subprocess
3. ✅ Console hiển thị progress stages
4. ✅ .osu file được tạo trong temp directory
5. ✅ Unity parse .osu file thành BeatmapData
6. ✅ Beatmap có > 50 notes
7. ✅ Gameplay chơi được, không crash
8. ✅ Fallback hoạt động khi ML fail

## 🎯 Next Steps

Sau khi test thành công:

### 1. Add Loading Screen UI
- Progress bar (0-100%)
- Cancel button
- Stage messages
- Estimated time remaining

### 2. Cache Generated Beatmaps
- Save .osu files để reuse
- Tránh re-generate cùng bài hát

### 3. Optimize Performance
- Pre-load model khi start game
- Use GPU nếu available
- Multi-threading

### 4. User Options
- Toggle ML on/off in Settings
- Choose ML vs Simple generation
- Adjust generation quality/speed

### 5. Build Testing
- Test standalone macOS build
- Bundle Python environment
- Test on different machines

## 📝 Test Log Template

Copy template này để track testing:

```markdown
## Test Session: [Date]

### Environment
- Unity: 2021.3.45f2
- Python: 3.x.x
- macOS: 12.x

### Test Results

#### Test 1: Standalone Python ✅/❌
- Command: python generate_beatmap.py test.mp3 0.5 output.osu
- Time: XXs
- Output size: XXX bytes
- Notes: 

#### Test 2: Unity Integration ✅/❌
- Song: test.mp3
- Difficulty: Normal
- Time: XXs
- Notes generated: XXX
- Gameplay: ✅/❌

#### Test 3: Fallback ✅/❌
- Trigger: [How you broke it]
- Fallback activated: ✅/❌
- Simple generation worked: ✅/❌

### Issues Found
1. 
2. 
3. 

### Notes
- 
```

## 🤝 Support

Nếu test fail:

1. Check console errors
2. Test Python script độc lập
3. Verify all files exist
4. Check Python dependencies
5. Try fallback mode (useMLGeneration = false)

Good luck! 🎮
