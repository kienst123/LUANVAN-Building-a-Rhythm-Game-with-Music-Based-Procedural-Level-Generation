# Kế Hoạch Tích Hợp ML vào RhythmGame3D

## 📊 Phân Tích Notebooks BeatLearning

Tôi đã đọc và phân tích 3 notebooks ML của bạn:

### 1. **dataset.ipynb** - Chuẩn bị dữ liệu huấn luyện
**Chức năng**:
- Chuyển đổi file .osz → định dạng IBF (Intermediate Beatmap Format)
- Tạo tokenizer với config QuaverBEaRT
- Tạo dataset với augmentation (tăng cường dữ liệu)
- Kết hợp nhiều beatmap với cùng file MP3

**Code chính**:
```python
converter = OsuBeatmapConverter()
tokenizer = BEaRTTokenizer(QuaverBEaRT())
dataset = BEaRTDataset(tokenizer, augment=True)
dataset.add(ibfs, mp3, offsets=[0.0])
```

---

### 2. **finetune.ipynb** - Huấn luyện mô hình
**Chức năng**:
- Khởi tạo mô hình BEaRT transformer
- Load checkpoint cũ (nếu có) để tiếp tục training
- Training với BEaRTTrainer
- Hỗ trợ CUDA (GPU) hoặc CPU
- Lưu checkpoints định kỳ
- TensorBoard để theo dõi metrics

**Code chính**:
```python
# Tạo mô hình
model_config = QuaverBEaRT()
tokenizer = BEaRTTokenizer(model_config)
model = BEaRT(tokenizer)

# Load dữ liệu
with open(dataset_train, "rb") as f:
    train_data = pickle.load(f)

# Huấn luyện
trainer = BEaRTTrainer(training_run_name, model, train_data, test_data, training_config)
for epoch in range(0, trainer.config.num_epochs):
    trainer.train(epoch)
    trainer.test(epoch)
```

**Thông tin quan trọng**:
- Mô hình đã được train sẵn và host trên HuggingFace
- Checkpoint: `sedthh/BeatLearning/quaver_beart_v1.pt`
- Không cần train lại, có thể dùng luôn!

---

### 3. **generate_osu.ipynb** - Tạo beatmap mới (QUAN TRỌNG NHẤT!)
**Chức năng**:
- Load mô hình đã train từ HuggingFace
- Generate beatmap từ file MP3
- Hỗ trợ điều chỉnh độ khó (difficulty)
- Xuất ra file .osu hoặc .osz

**Code chính**:
```python
# 1. Tải mô hình từ HuggingFace
from huggingface_hub import hf_hub_download

checkpoint = hf_hub_download(
    repo_id="sedthh/BeatLearning",
    filename="quaver_beart_v1.pt"
)

# 2. Khởi tạo mô hình
tokenizer = BEaRTTokenizer(QuaverBEaRT())
model = BEaRT(tokenizer)
model.load(checkpoint)
model.to(device)

# 3. Generate beatmap
ibf = model.generate(
    audio_file=mp3_path,           # Đường dẫn file MP3
    audio_start=0.0,               # Thời điểm bắt đầu (giây)
    audio_end=None,                # Thời điểm kết thúc (None = toàn bộ)
    use_tracks=["LEFT"],           # Chỉ dùng LEFT cho osu!mania
    difficulty=0.5,                # Độ khó: 0.0 (dễ) đến 1.0 (khó)
    beams=[2] * 8,                 # Cấu hình beam search
    max_beam_width=256,            # Độ chính xác (thấp hơn = nhanh hơn)
    temperature=0.1,               # Độ ngẫu nhiên (thấp = ổn định hơn)
    random_seed=69420              # Seed để tái tạo kết quả
)

# 4. Chuyển đổi IBF → file .osz
converter = OsuBeatmapConverter()
converter.generate(ibf, output_path, meta={
    "title": "Tên bài hát",
    "artist": "Tên ca sĩ",
    "difficulty_name": "normal",
    "overall_difficulty": int(7 * difficulty),
    "creator": "BeatLearning AI"
})
```

**🎯 PHÁT HIỆN QUAN TRỌNG**:
```
Parameter difficulty của mô hình: 0.0 - 1.0
Độ khó trong Unity:
  - Easy   → 0.33 (1/3)
  - Normal → 0.50 (1/2)
  - Hard   → 0.75 (3/4)

→ KHỚP HOÀN HẢO! Không cần chuyển đổi phức tạp!
```

---

## 🔧 Phương Án Tích Hợp

### **Phương Án 1: Python Subprocess** (KHUYẾN NGHỊ)

**Cách hoạt động**:
- Unity gọi Python script qua Process
- Python chạy mô hình ML → tạo file .osu
- Unity đọc file .osu → parse thành BeatmapData
- Load vào game để chơi

**Ưu điểm**:
✅ Dễ implement, sử dụng code notebook có sẵn
✅ Không cần chuyển đổi mô hình
✅ Có thể test ngay lập tức
✅ Tạo beatmap động từ bất kỳ bài hát nào

**Nhược điểm**:
❌ Cần bundle Python environment khi build Unity
❌ Thời gian generate: 10-60 giây (người chơi phải đợi)
❌ Phải xử lý dependencies: PyTorch, librosa, beatlearning
❌ Tốn CPU/RAM khi inference

**Implementation**:

```csharp
// Trong BeatmapSelector3D.cs
IEnumerator GenerateBeatmapML(string musicPath)
{
    // 1. Map độ khó Unity → ML
    float mlDifficulty = currentDifficulty switch {
        0 => 0.33f, // Easy
        1 => 0.50f, // Normal
        2 => 0.75f, // Hard
        _ => 0.50f
    };
    
    // 2. Chuẩn bị paths
    string pythonScript = Path.Combine(Application.streamingAssetsPath, "generate_beatmap.py");
    string tempOsuPath = Path.GetTempFileName() + ".osu";
    
    // 3. Tạo ProcessStartInfo
    ProcessStartInfo psi = new ProcessStartInfo
    {
        FileName = "python3",
        Arguments = $"\"{pythonScript}\" \"{musicPath}\" {mlDifficulty} \"{tempOsuPath}\"",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };
    
    // 4. Chạy Python script
    Process process = Process.Start(psi);
    
    // 5. Đợi hoàn thành (với timeout)
    float timeout = 120f; // 2 phút
    float elapsed = 0f;
    
    while (!process.HasExited && elapsed < timeout)
    {
        yield return new WaitForSeconds(0.5f);
        elapsed += 0.5f;
        
        // Update loading UI
        UpdateLoadingProgress(elapsed / timeout);
    }
    
    // 6. Kiểm tra kết quả
    if (process.HasExited && process.ExitCode == 0)
    {
        // Parse file .osu
        currentBeatmap = BeatmapParser.ParseBeatmap(tempOsuPath);
        
        // Lưu vào storage
        BeatmapStorage.currentBeatmap = currentBeatmap;
        BeatmapStorage.currentMusicPath = musicPath;
        
        UpdateInfoText();
        Debug.Log("[ML] Beatmap generated successfully!");
    }
    else
    {
        // Fallback: dùng generation đơn giản
        Debug.LogWarning("[ML] Generation failed, using simple algorithm");
        yield return GenerateBeatmapSimple(musicPath);
    }
}
```

**Python Script** (`generate_beatmap.py`):
```python
#!/usr/bin/env python3
import sys
import os
from huggingface_hub import hf_hub_download
from beatlearning.tokenizers import BEaRTTokenizer
from beatlearning.configs import QuaverBEaRT
from beatlearning.models import BEaRT
from beatlearning.converters import OsuBeatmapConverter

def generate_beatmap(audio_path, difficulty, output_path):
    """
    Generate beatmap từ MP3 sử dụng BEaRT model
    
    Args:
        audio_path: Đường dẫn file MP3
        difficulty: Độ khó (0.0 - 1.0)
        output_path: Đường dẫn output file .osu
    """
    try:
        # 1. Load model
        print(f"Loading model from HuggingFace...")
        checkpoint = hf_hub_download(
            repo_id="sedthh/BeatLearning",
            filename="quaver_beart_v1.pt",
            cache_dir="./ml_cache"
        )
        
        tokenizer = BEaRTTokenizer(QuaverBEaRT())
        model = BEaRT(tokenizer)
        model.load(checkpoint)
        print(f"Model loaded successfully!")
        
        # 2. Generate beatmap
        print(f"Generating beatmap for {audio_path} (difficulty={difficulty})...")
        ibf = model.generate(
            audio_file=audio_path,
            audio_start=0.0,
            audio_end=None,
            use_tracks=["LEFT"],
            difficulty=float(difficulty),
            beams=[2] * 8,
            max_beam_width=256,
            temperature=0.1,
            random_seed=42
        )
        print(f"Generation complete!")
        
        # 3. Convert to .osu
        print(f"Converting to .osu format...")
        converter = OsuBeatmapConverter()
        
        # Extract metadata
        song_name = os.path.splitext(os.path.basename(audio_path))[0]
        diff_name = ["easy", "normal", "hard"][int(difficulty * 3)]
        
        converter.generate(ibf, output_path, meta={
            "title": song_name,
            "artist": "Unknown Artist",
            "difficulty_name": diff_name,
            "overall_difficulty": int(7 * difficulty),
            "creator": "BeatLearning AI"
        })
        
        print(f"Beatmap saved to {output_path}")
        return 0
        
    except Exception as e:
        print(f"ERROR: {str(e)}", file=sys.stderr)
        return 1

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("Usage: python generate_beatmap.py <audio_path> <difficulty> <output_path>")
        sys.exit(1)
    
    audio_path = sys.argv[1]
    difficulty = float(sys.argv[2])
    output_path = sys.argv[3]
    
    exit_code = generate_beatmap(audio_path, difficulty, output_path)
    sys.exit(exit_code)
```

---

### **Phương Án 2: Pre-generation** (ĐƠN GIẢN NHẤT CHO DEMO)

**Cách hoạt động**:
- Trước khi bảo vệ, generate sẵn 3-5 beatmaps cho demo songs
- Parse các file .osu này vào định dạng BeatmapData của Unity
- Lưu dưới dạng JSON hoặc ScriptableObject
- Trong game, load trực tiếp từ bộ nhớ

**Ưu điểm**:
✅ Không cần Python runtime
✅ Playback tức thì (0 giây)
✅ Hoàn hảo cho demo/bảo vệ luận văn
✅ Có thể chọn kết quả ML tốt nhất để show

**Nhược điểm**:
❌ Không thực sự động (dynamic)
❌ Giới hạn số bài hát
❌ Không thể generate beatmap mới trong game

**Implementation**:

**Bước 1**: Generate beatmaps offline (Python):
```python
# generate_demo_beatmaps.py
songs = [
    ("demo_songs/song1.mp3", "Song 1", "Artist 1"),
    ("demo_songs/song2.mp3", "Song 2", "Artist 2"),
    ("demo_songs/song3.mp3", "Song 3", "Artist 3"),
]

difficulties = [
    ("easy", 0.33),
    ("normal", 0.50),
    ("hard", 0.75)
]

for song_path, title, artist in songs:
    for diff_name, diff_value in difficulties:
        output_path = f"demo_beatmaps/{title}_{diff_name}.osu"
        
        ibf = model.generate(
            audio_file=song_path,
            difficulty=diff_value,
            beams=[2] * 8,
            max_beam_width=256,
            temperature=0.1
        )
        
        converter.generate(ibf, output_path, meta={
            "title": title,
            "artist": artist,
            "difficulty_name": diff_name,
            "overall_difficulty": int(7 * diff_value)
        })
```

**Bước 2**: Parse .osu files vào Unity:
```csharp
// PreloadedBeatmaps.cs
[CreateAssetMenu(fileName = "PreloadedBeatmaps", menuName = "RhythmGame/Preloaded Beatmaps")]
public class PreloadedBeatmaps : ScriptableObject
{
    [System.Serializable]
    public class PreloadedSong
    {
        public string songName;
        public string artistName;
        public AudioClip audioClip;
        public BeatmapData easyBeatmap;
        public BeatmapData normalBeatmap;
        public BeatmapData hardBeatmap;
    }
    
    public List<PreloadedSong> songs = new List<PreloadedSong>();
    
    public BeatmapData GetBeatmap(int songIndex, int difficulty)
    {
        if (songIndex < 0 || songIndex >= songs.Count)
            return null;
            
        PreloadedSong song = songs[songIndex];
        return difficulty switch {
            0 => song.easyBeatmap,
            1 => song.normalBeatmap,
            2 => song.hardBeatmap,
            _ => song.normalBeatmap
        };
    }
}
```

**Bước 3**: Trong BeatmapSelector3D:
```csharp
public PreloadedBeatmaps preloadedBeatmaps;
private int currentSongIndex = 0;

void ShowPreloadedSongs()
{
    // Hiển thị list các bài hát có sẵn
    for (int i = 0; i < preloadedBeatmaps.songs.Count; i++)
    {
        var song = preloadedBeatmaps.songs[i];
        CreateSongButton(song.songName, song.artistName, i);
    }
}

void OnSongSelected(int songIndex)
{
    currentSongIndex = songIndex;
    var song = preloadedBeatmaps.songs[songIndex];
    
    // Update UI
    songTitleText.text = song.songName;
    artistText.text = song.artistName;
    
    // Load beatmap theo difficulty
    currentBeatmap = preloadedBeatmaps.GetBeatmap(songIndex, currentDifficulty);
    
    // Store
    BeatmapStorage.currentBeatmap = currentBeatmap;
    BeatmapStorage.currentMusicPath = song.audioClip.name;
}
```

---

### **Phương Án 3: REST API Service** (NÂNG CAO)

**Cách hoạt động**:
- Chạy Python server (Flask/FastAPI) riêng
- Server host mô hình ML
- Unity gửi HTTP request với file MP3
- Server generate và trả về beatmap JSON
- Unity parse JSON thành BeatmapData

**Ưu điểm**:
✅ Tách biệt ML server với game
✅ Có thể chạy trên server mạnh/GPU
✅ Dễ update model mà không cần build lại game
✅ Có thể xử lý nhiều request đồng thời

**Nhược điểm**:
❌ Cần kết nối mạng
❌ Phức tạp triển khai server
❌ Không thích hợp cho offline

**Implementation** (tóm tắt):

```python
# server.py (FastAPI)
from fastapi import FastAPI, UploadFile
import uvicorn

app = FastAPI()

@app.post("/generate")
async def generate_beatmap(audio: UploadFile, difficulty: float):
    # Save uploaded file
    audio_path = f"temp/{audio.filename}"
    with open(audio_path, "wb") as f:
        f.write(await audio.read())
    
    # Generate
    ibf = model.generate(audio_path, difficulty=difficulty)
    beatmap_json = convert_to_json(ibf)
    
    return {"beatmap": beatmap_json}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
```

```csharp
// Unity client
IEnumerator GenerateBeatmapAPI(string musicPath, float difficulty)
{
    // Prepare form data
    WWWForm form = new WWWForm();
    byte[] audioData = File.ReadAllBytes(musicPath);
    form.AddBinaryData("audio", audioData, Path.GetFileName(musicPath), "audio/mpeg");
    form.AddField("difficulty", difficulty.ToString());
    
    // Send request
    using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:8000/generate", form))
    {
        yield return www.SendWebRequest();
        
        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;
            currentBeatmap = JsonUtility.FromJson<BeatmapData>(json);
        }
    }
}
```

---

## 🎯 Khuyến Nghị Cho Luận Văn (30% Còn Lại)

### **Phương Án Kết Hợp** (HYBRID):

#### **1. Cho Demo/Bảo Vệ** → Dùng **Phương Án 2 (Pre-generation)**

**Tại sao?**
- ✅ Không có latency, chơi mượt mà
- ✅ Không lo lỗi kỹ thuật khi demo
- ✅ Showcase được kết quả ML tốt nhất
- ✅ Giảng viên thấy rõ chất lượng beatmap

**Cách làm**:
1. Chọn 3-5 bài hát demo (đa dạng thể loại)
2. Generate 3 độ khó cho mỗi bài (Easy/Normal/Hard)
3. Parse vào Unity ScriptableObject
4. Tạo UI chọn bài hát đẹp mắt
5. **Thời gian**: 2-3 ngày

#### **2. Cho Implementation Đầy Đủ** → Thêm **Phương Án 1 (Python Subprocess)**

**Tại sao?**
- ✅ Cho thấy tích hợp ML thực sự
- ✅ Chứng minh hệ thống hoạt động động
- ✅ Người chơi có thể thử với bài hát riêng
- ✅ Nội dung luận văn phong phú hơn

**Cách làm**:
1. Viết Python script wrapper (generate_beatmap.py)
2. Implement subprocess call trong Unity
3. Thêm loading screen với progress bar
4. Xử lý fallback nếu ML fail
5. **Thời gian**: 3-5 ngày

---

## 📋 Roadmap Hoàn Thành (30%)

### **Tuần 1: Pre-generation Setup** (10%)

**Day 1-2**: Generate Demo Beatmaps
- [ ] Chạy generate_osu.ipynb cho 3-5 bài hát
- [ ] Tạo Easy/Normal/Hard cho mỗi bài
- [ ] Verify chất lượng beatmaps
- [ ] Chọn kết quả tốt nhất

**Day 3**: Unity Integration
- [ ] Tạo PreloadedBeatmaps ScriptableObject
- [ ] Parse .osu files vào BeatmapData
- [ ] Import audio clips
- [ ] Test loading trong game

**Day 4**: UI Update
- [ ] Redesign BeatmapSelector3D cho preloaded songs
- [ ] Hiển thị danh sách bài hát
- [ ] Preview info (title, artist, duration)
- [ ] Polish visual

---

### **Tuần 2: Python Subprocess** (10%)

**Day 1**: Python Script
- [ ] Viết generate_beatmap.py
- [ ] Test standalone (ngoài Unity)
- [ ] Xử lý error cases
- [ ] Optimize generation time

**Day 2**: Unity Integration
- [ ] Implement subprocess call
- [ ] Parse stdout/stderr
- [ ] Handle timeouts
- [ ] Test với nhiều bài hát

**Day 3**: Loading UI
- [ ] Tạo loading screen
- [ ] Progress bar (fake hoặc real)
- [ ] Cancel button
- [ ] Error messages

**Day 4**: Fallback System
- [ ] Detect ML failure
- [ ] Fallback to simple generation
- [ ] Log errors
- [ ] User feedback

---

### **Tuần 3: Testing & Documentation** (10%)

**Day 1-2**: Testing
- [ ] Test cả 2 phương án
- [ ] Test nhiều bài hát
- [ ] Test các độ khó
- [ ] Performance testing
- [ ] Edge cases

**Day 3**: Documentation
- [ ] Update PROJECT_OVERVIEW.md
- [ ] Viết phần ML integration
- [ ] Hướng dẫn setup Python environment
- [ ] Troubleshooting guide

**Day 4**: Video & Presentation
- [ ] Record demo gameplay
- [ ] Tạo slides bảo vệ
- [ ] Chuẩn bị câu trả lời Q&A
- [ ] Final polish

---

## 🔍 So Sánh Phương Án

| Tiêu chí | Pre-generation | Python Subprocess | REST API |
|----------|----------------|-------------------|----------|
| **Độ khó implement** | ⭐⭐ Dễ | ⭐⭐⭐ Trung bình | ⭐⭐⭐⭐⭐ Khó |
| **Thời gian hoàn thành** | 2-3 ngày | 3-5 ngày | 7-10 ngày |
| **Latency** | 0ms | 10-60s | 5-30s |
| **Dependencies** | Không | Python + ML libs | Server + Network |
| **Độ động** | ❌ Tĩnh | ✅ Động | ✅ Động |
| **Phù hợp demo** | ✅✅✅ Rất tốt | ⚠️ Phải đợi | ⚠️ Cần mạng |
| **Phù hợp luận văn** | ✅✅ Tốt | ✅✅✅ Rất tốt | ✅✅ Tốt |
| **Build deployment** | ✅ Dễ | ⚠️ Khó | ✅ Dễ (client) |

---

## 💡 Câu Hỏi Thường Gặp

### Q1: Mô hình ML mất bao lâu để generate?
**A**: Trên CPU: 30-60 giây. Trên GPU: 10-20 giây.

### Q2: Chất lượng ML beatmap tốt hơn random generation?
**A**: Rất nhiều! ML đã học từ hàng nghìn beatmaps thật, nên:
- Notes sync với beat/melody
- Patterns hợp lý, chơi được
- Difficulty chuẩn hơn
- Giống osu!mania thật

### Q3: Có cần GPU không?
**A**: Không bắt buộc. CPU cũng chạy được, chỉ chậm hơn.

### Q4: Python subprocess có work trong Unity build không?
**A**: Có, nhưng cần bundle Python environment:
- macOS: Dùng Python system hoặc bundle Miniconda
- Windows: Bundle Python embeddable
- Linux: Yêu cầu user cài Python

### Q5: Nếu ML generation fail thì sao?
**A**: Có fallback system:
```csharp
try {
    beatmap = GenerateML();
} catch {
    beatmap = GenerateSimple(); // Dùng random cũ
}
```

### Q6: Có thể deploy lên mobile không?
**A**: Khó! ML models quá nặng cho mobile. Khuyên dùng:
- Pre-generation cho mobile
- Hoặc cloud API

---

## 🎬 Kết Luận

### **Khuyến Nghị Cuối Cùng**

Cho luận văn của bạn (còn 30%), tôi đề xuất:

#### **📅 Timeline 2-3 tuần:**

**Tuần 1** (Cơ bản - 10%):
- Pre-generate 3-5 beatmaps chất lượng cao
- Integrate vào Unity với UI đẹp
- Test kỹ để demo

**Tuần 2** (Nâng cao - 10%):
- Implement Python subprocess
- Thêm loading UI
- Fallback system

**Tuần 3** (Hoàn thiện - 10%):
- Testing toàn diện
- Documentation
- Chuẩn bị bảo vệ

#### **🎯 Kết Quả Mong Đợi:**

✅ **Demo mượt mà**: Pre-generated beatmaps chơi tức thì
✅ **Tính năng đầy đủ**: Python subprocess cho dynamic generation
✅ **Luận văn chất lượng**: Showcase cả lý thuyết và thực hành ML
✅ **Ấn tượng giảng viên**: Thấy rõ ứng dụng AI vào game

---

## 📞 Câu Hỏi Cho Bạn

Bạn muốn tôi bắt đầu với phương án nào?

1. **🚀 Bắt đầu ngay**: Implement Pre-generation (2-3 ngày)
2. **🔥 Full implementation**: Làm cả Pre-gen + Subprocess (1 tuần)
3. **📚 Chi tiết hơn**: Giải thích thêm về ML model
4. **🎨 Cải thiện khác**: Suggest thêm tính năng khác

Hãy cho tôi biết bạn muốn đi theo hướng nào! 🎮
