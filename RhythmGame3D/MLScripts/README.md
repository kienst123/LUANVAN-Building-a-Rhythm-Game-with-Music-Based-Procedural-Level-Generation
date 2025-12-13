# ML Scripts - Setup Guide

Hướng dẫn cài đặt môi trường Python để chạy ML beatmap generation.

## 📋 Yêu Cầu

- Python 3.8 hoặc mới hơn
- pip (Python package manager)
- 2-4GB RAM khả dụng
- 1-2GB dung lượng ổ cứng (cho model và dependencies)

## 🚀 Cài Đặt Nhanh (macOS)

### 1. Kiểm tra Python

```bash
python3 --version
```

Nếu chưa có Python, cài đặt qua Homebrew:
```bash
brew install python3
```

### 2. Tạo Virtual Environment (Khuyến nghị)

```bash
cd RhythmGame3D/MLScripts
python3 -m venv venv
source venv/bin/activate
```

### 3. Cài đặt Dependencies

```bash
pip install beatlearning torch librosa huggingface_hub
```

Hoặc dùng requirements file:
```bash
pip install -r requirements.txt
```

### 4. Test Script

```bash
python generate_beatmap.py test.mp3 0.5 output.osu
```

## 📦 Dependencies Chi Tiết

### Core Libraries:
- **beatlearning**: BEaRT model và tokenizer
- **torch**: PyTorch (ML framework)
- **librosa**: Audio analysis
- **huggingface_hub**: Download model từ HuggingFace

### Tự động install khi cài beatlearning:
- numpy
- scipy
- soundfile
- tqdm

## 🔧 Troubleshooting

### Lỗi: "No module named 'beatlearning'"

```bash
pip install beatlearning
```

### Lỗi: "torch not found"

```bash
# CPU version (nhẹ hơn)
pip install torch --index-url https://download.pytorch.org/whl/cpu

# GPU version (nếu có CUDA)
pip install torch
```

### Lỗi: "librosa requires ffmpeg"

```bash
brew install ffmpeg
```

### Lỗi: "Permission denied"

```bash
chmod +x generate_beatmap.py
```

## 🎮 Unity Integration

Script này được gọi từ Unity thông qua PythonMLBridge.cs:

```csharp
// Trong Unity
PythonMLBridge bridge = new PythonMLBridge();
yield return bridge.GenerateBeatmap(
    audioPath: "path/to/song.mp3",
    difficulty: 0.5f,
    outputPath: "path/to/output.osu"
);
```

## 📊 Performance

### Thời gian generation (ước tính):

- **CPU (M1/M2 Mac)**: 20-40 giây
- **CPU (Intel Mac)**: 30-60 giây
- **GPU (CUDA)**: 10-20 giây

### First run:
- Lần đầu chạy sẽ download model (~500MB) từ HuggingFace
- Model được cache ở `ml_cache/` để lần sau dùng lại

## 🔍 Testing Script Standalone

Trước khi integrate vào Unity, test script độc lập:

```bash
# Test với file MP3
python generate_beatmap.py "/path/to/song.mp3" 0.33 "output_easy.osu"
python generate_beatmap.py "/path/to/song.mp3" 0.50 "output_normal.osu"
python generate_beatmap.py "/path/to/song.mp3" 0.75 "output_hard.osu"
```

## 📝 Output Format

Script tạo file .osu với format:

```ini
[General]
AudioFilename: song.mp3
Mode: 3

[Metadata]
Title: Song Name
Artist: Unknown Artist
Creator: BeatLearning AI
Version: Normal

[Difficulty]
HPDrainRate: 5
CircleSize: 4
OverallDifficulty: 5

[HitObjects]
64,192,1000,1,0
192,192,1500,1,0
...
```

## 🎯 Difficulty Mapping

| Unity Difficulty | Value | Python Difficulty | OD |
|-----------------|-------|-------------------|-----|
| Easy            | 0     | 0.33              | 3   |
| Normal          | 1     | 0.50              | 5   |
| Hard            | 2     | 0.75              | 7   |

## 💡 Tips

1. **Cache Model**: Model được download 1 lần, lưu ở `ml_cache/`
2. **Use SSD**: Generation nhanh hơn trên SSD
3. **Close Apps**: Đóng app khác để tăng RAM khả dụng
4. **Fixed Seed**: Script dùng seed=42 để kết quả consistent

## 🔄 Updating Model

Để update model mới:

```bash
rm -rf ml_cache/
python generate_beatmap.py <audio> <diff> <output>
# Sẽ download model mới
```

## 📞 Support

Nếu gặp lỗi, check:
1. Python version >= 3.8
2. All dependencies installed
3. Audio file tồn tại và đúng format
4. Đủ dung lượng ổ cứng (1GB+)
5. Đủ RAM (2GB+)

## 🎓 Documentation

- BeatLearning: https://github.com/sedthh/BeatLearning
- PyTorch: https://pytorch.org
- HuggingFace: https://huggingface.co/sedthh/BeatLearning
