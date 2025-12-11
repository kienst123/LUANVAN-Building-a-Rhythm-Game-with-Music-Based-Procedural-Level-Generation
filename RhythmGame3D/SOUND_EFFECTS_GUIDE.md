# 🔊 Sound Effects Guide - RhythmGame3D

## ✅ Đã Hoàn Thành

### 1. AudioManager3D Script
- ✅ Tạo `AudioManager3D.cs` với quản lý tất cả sound effects
- ✅ Tích hợp vào `GameManager3D`
- ✅ Auto Setup Script đã cập nhật

### 2. Sound Effects Đã Tích Hợp

#### Hit Sounds (Khi đánh note)
- `perfectHitSound` - Âm thanh khi Perfect
- `greatHitSound` - Âm thanh khi Great  
- `goodHitSound` - Âm thanh khi Good
- `missSound` - Âm thanh khi Miss

#### UI Sounds
- `buttonClickSound` - Khi nhấn Restart/Exit
- `gameOverSound` - Khi hết máu
- `comboBreakSound` - Khi combo về 0

#### Countdown Sounds (Tùy chọn)
- `countdown3Sound` - "3"
- `countdown2Sound` - "2"
- `countdown1Sound` - "1"
- `countdownGoSound` - "Go!"

### 3. Volume Controls
- `masterVolume` (0-1) - Âm lượng tổng
- `hitSoundVolume` (0-1) - Âm lượng hit sounds
- `uiSoundVolume` (0-1) - Âm lượng UI sounds

---

## 📥 Bước Tiếp Theo: Download/Tạo Sound Files

### Tùy Chọn 1: Download Miễn Phí (Khuyến nghị)

#### A. Freesound.org (Chất lượng cao)
1. Truy cập: https://freesound.org/
2. Tìm kiếm các từ khóa:
   - "click" → Button click
   - "game over" → Game over sound
   - "hit" hoặc "tap" → Hit sounds
   - "combo break" → Combo break
   - "countdown" → Countdown sounds

#### B. Kenney.nl (UI Sounds Pack)
1. Truy cập: https://kenney.nl/assets/interface-sounds
2. Download "Interface Sounds" pack (miễn phí)
3. Bao gồm rất nhiều button clicks và UI sounds

#### C. Mixkit (Free Game Sounds)
1. Truy cập: https://mixkit.co/free-sound-effects/game/
2. Download các sound effects game miễn phí

### Tùy Chọn 2: Tạo Sound Với Bfxr (Nhanh)

1. Truy cập: https://sfxr.me/
2. Chọn preset hoặc randomize
3. Điều chỉnh parameters
4. Export as .wav
5. Làm cho mỗi loại sound (Perfect, Great, Good, Miss)

**Gợi ý tạo Hit Sounds:**
- **Perfect**: Pitch cao, reverb nhiều, âm "sáng"
- **Great**: Pitch trung bình, reverb vừa
- **Good**: Pitch thấp hơn, reverb ít
- **Miss**: Âm "tối", pitch thấp hoặc không có âm

---

## 🎵 Cách Thêm Sounds Vào Unity

### Bước 1: Tạo Thư Mục
```
Assets/
  Sounds/
    HitSounds/
      Perfect.wav
      Great.wav
      Good.wav
      Miss.wav
    UI/
      ButtonClick.wav
      GameOver.wav
      ComboBreak.wav
    Countdown/  (tùy chọn)
      3.wav
      2.wav
      1.wav
      Go.wav
```

### Bước 2: Import Sound Files
1. Kéo thả các file .wav/.mp3 vào folder `Assets/Sounds/`
2. Unity sẽ tự động import

### Bước 3: Cấu Hình Import Settings
1. Chọn sound file trong Project window
2. Trong Inspector:
   - **Load Type**: Decompress On Load (cho sounds ngắn)
   - **Compression Format**: Vorbis (balance giữa chất lượng và dung lượng)
   - **Quality**: 70-100%
   - **Sample Rate Setting**: Preserve Sample Rate
3. Click "Apply"

### Bước 4: Assign Vào AudioManager
1. Chạy `RhythmGame > Auto Setup Scene` (nếu chưa)
2. Chọn GameObject `GameManager` trong Hierarchy
3. Tìm component `Audio Manager 3D` trong Inspector
4. Kéo thả sound files từ Project vào các field tương ứng:
   - Hit Sounds → Perfect Hit Sound, Great Hit Sound, etc.
   - UI Sounds → Button Click Sound, Game Over Sound, etc.

### Bước 5: Điều Chỉnh Volume
1. Trong `Audio Manager 3D` component:
   - `Master Volume`: 1.0 (100%)
   - `Hit Sound Volume`: 0.8 (80%)
   - `UI Sound Volume`: 0.6 (60%)
2. Test và điều chỉnh theo ý thích

---

## 🧪 Testing

### Test Hit Sounds
1. Play game
2. Space để bắt đầu
3. Đánh notes → Nghe Perfect/Great/Good sounds
4. Bỏ notes → Nghe Miss sound (nếu có)

### Test Combo Break
1. Hit một vài notes để có combo
2. Miss 1 note → Nghe combo break sound

### Test UI Sounds
1. Để hết máu → Nghe game over sound
2. Nhấn Restart/Exit → Nghe button click

---

## 📝 Gợi Ý Sound Characteristics

### Hit Sounds (Theo thứ tự tốt → xấu)

| Judgment | Pitch | Reverb | Volume | Đặc điểm |
|----------|-------|--------|--------|----------|
| Perfect  | Cao   | Nhiều  | 100%   | Sắc, trong, "bling" |
| Great    | Trung | Vừa    | 90%    | Tròn, ấm |
| Good     | Thấp  | Ít     | 80%    | Nhẹ, nhàn |
| Miss     | Rất thấp | Không | 70% | "Ugh", thất vọng |

### UI Sounds

| Sound | Đặc điểm |
|-------|----------|
| Button Click | Ngắn, sắc, "click" rõ ràng |
| Game Over | Dài hơn, buồn, dramatic |
| Combo Break | "Shatter" hoặc "break" |

---

## 🎨 Ví Dụ Sound Pack Miễn Phí

### Từ osu! Community
- Nhiều skin osu!mania có hit sounds chất lượng
- **Lưu ý**: Check license trước khi dùng!
- File thường là: `normal-hitnormal.wav`, `normal-hitclap.wav`

### Tự Tạo Đơn Giản
```
Perfect: Cao độ C6 (1047 Hz), reverb 50%
Great:   Cao độ G5 (784 Hz), reverb 30%
Good:    Cao độ E5 (659 Hz), reverb 10%
Miss:    Cao độ C4 (262 Hz), no reverb
```

---

## 🚀 Quick Start (Nếu Muốn Test Nhanh)

### Không Có Sounds Ngay Bây Giờ?
Code đã sẵn sàng! Sounds sẽ không play nếu AudioClip = null, nhưng game vẫn chạy bình thường.

### Có Sound Samples Sẵn?
1. Kéo thả vào `Assets/Sounds/`
2. Assign vào AudioManager
3. Play và test!

---

## 🔧 Troubleshooting

### Sound không play?
- ✅ Check AudioClip đã assign chưa
- ✅ Check Master Volume > 0
- ✅ Check Audio Listener có trong scene (thường ở Main Camera)
- ✅ Check Console có error không

### Sound bị delay?
- ✅ Đổi Load Type → Decompress On Load
- ✅ Giảm Sample Rate xuống 22050 Hz

### Sound quá to/nhỏ?
- ✅ Điều chỉnh Hit Sound Volume / UI Sound Volume
- ✅ Hoặc chỉnh Master Volume

---

## 📌 Notes

- **Không bắt buộc phải có tất cả sounds**: Game vẫn chạy nếu một số sounds bị thiếu
- **Countdown sounds**: Optional, có thể implement sau
- **File format**: .wav (không nén) hoặc .mp3 (nén) đều được
- **File size**: Nên < 100KB cho mỗi sound clip (sounds ngắn)

---

**Chúc may mắn! 🎮🎵**
