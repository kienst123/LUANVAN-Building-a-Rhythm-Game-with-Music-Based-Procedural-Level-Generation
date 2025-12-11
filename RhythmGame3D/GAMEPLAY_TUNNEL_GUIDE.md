# Gameplay Tunnel Background Setup Guide

## Tính năng mới
Đã thêm **Tunnel Background** vào GameScene để tạo hiệu ứng tunnel neon giống như trong MainMenu!

## Tự động setup
Tunnel background sẽ **TỰ ĐỘNG** được tạo khi bắt đầu game! Không cần làm gì thêm.

## Tính năng đặc biệt

### 1. Dynamic Intensity (Cường độ động)
- Tunnel sáng hơn khi **combo cao**
- Độ sáng tăng dần từ combo 0 → 100
- Tạo cảm giác "power up" khi chơi tốt

### 2. Beat Pulse (Nhấp theo nhịp)
- Hexagon **phóng to/thu nhỏ** khi đánh Perfect
- Tạo feedback thị giác cho người chơi
- Đồng bộ với gameplay

### 3. Scrolling Animation
- Grid và particles di chuyển về phía camera
- Tạo cảm giác tốc độ
- Loop vô tận

## Cấu hình trong Inspector

Nếu muốn tùy chỉnh, chọn **GameplayTunnelBackground** object và điều chỉnh:

### Tunnel Settings
- **Primary Color**: Màu cyan (#00F0FF) - Đường thẳng dọc
- **Secondary Color**: Màu magenta (#FF00E5) - Vòng tròn ngang  
- **Scroll Speed**: 2.0 - Tốc độ cuộn tunnel
- **Rotation Speed**: 5.0 - Tốc độ quay hexagon

### Grid Settings
- **Vertical Lines**: 20 - Số đường thẳng dọc
- **Ring Count**: 30 - Số vòng tròn
- **Ring Spacing**: 5.0 - Khoảng cách giữa các vòng
- **Tunnel Radius**: 25.0 - Bán kính tunnel

### Hexagon Layers
- **Hexagon Count**: 3 - Số lớp hexagon
- **Hexagon Size**: 15.0 - Kích thước hexagon

### Particles
- **Particle Count**: 30 - Số particle bay
- **Particle Speed**: 5.0 - Tốc độ bay của particle

## Hiệu ứng combo

- **Combo 0-25**: Tunnel tối, màu bình thường
- **Combo 25-50**: Bắt đầu sáng hơn
- **Combo 50-75**: Sáng rõ rệt
- **Combo 75-100+**: Cực sáng, hiệu ứng mạnh nhất!

## API cho lập trình viên

```csharp
// Trong GameManager3D hoặc script khác:

// Điều chỉnh cường độ tunnel (0-1)
tunnelBackground.SetIntensity(0.5f);

// Tạo hiệu ứng pulse
tunnelBackground.PulseOnBeat();
```

## Tối ưu hóa

Tunnel background được tối ưu cho performance:
- Sử dụng LineRenderer thay vì mesh phức tạp
- Particles có giới hạn số lượng
- Không có collision detection
- Update chỉ vị trí, không tạo/xóa object

## Troubleshooting

**Q: Không thấy tunnel?**
A: Kiểm tra Camera Position - phải ở Z < 0 để nhìn thấy tunnel

**Q: Tunnel quá sáng/tối?**  
A: Điều chỉnh Primary Color và Secondary Color trong Inspector

**Q: FPS giảm?**
A: Giảm Particle Count và Vertical Lines

**Q: Tunnel không pulse khi đánh Perfect?**
A: Kiểm tra GameManager3D đã gọi `tunnelBackground.PulseOnBeat()` trong `OnJudgmentReceived()`

## So sánh với Menu Background

| Feature | Menu Background | Gameplay Background |
|---------|----------------|---------------------|
| Auto-create | ✅ | ✅ |
| Scrolling | ✅ | ✅ |
| Hexagons | ✅ | ✅ |
| Particles | ✅ | ✅ |
| **Dynamic Intensity** | ❌ | ✅ |
| **Beat Pulse** | ❌ | ✅ |
| **Combo Reactive** | ❌ | ✅ |

Gameplay tunnel có thêm tính năng tương tác với game!
