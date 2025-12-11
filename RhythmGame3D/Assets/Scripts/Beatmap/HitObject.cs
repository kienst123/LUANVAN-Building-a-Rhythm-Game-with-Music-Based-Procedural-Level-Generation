using UnityEngine;

namespace RhythmGame3D.Beatmap
{
    /// <summary>
    /// Đại diện cho một note trong beatmap (osu!mania)
    /// </summary>
    [System.Serializable]
    public class HitObject
    {
        // Vị trí X (cho 4K mania: 0, 128, 256, 384)
        public int x;
        
        // Vị trí Y (không dùng trong mania)
        public int y;
        
        // Thời điểm note xuất hiện (ms)
        public int time;
        
        // Loại note (1 = circle/tap, 128 = long note)
        public int type;
        
        // Hit sound
        public int hitSound;
        
        // End time cho long note (ms)
        public int endTime;
        
        // Lane index (0-3 cho 4K mania)
        public int lane;
        
        // Có phải long note không
        public bool isLongNote;
        
        // Constructor
        public HitObject(int x, int y, int time, int type, int hitSound)
        {
            this.x = x;
            this.y = y;
            this.time = time;
            this.type = type;
            this.hitSound = hitSound;
            this.isLongNote = (type & 128) != 0;
            
            // Tính lane từ position X (4K mania)
            // osu!mania 4K: lane 0 = 64, lane 1 = 192, lane 2 = 320, lane 3 = 448
            this.lane = Mathf.FloorToInt(x / 128f);
            this.lane = Mathf.Clamp(lane, 0, 3);
        }
        
        public void SetEndTime(int endTime)
        {
            this.endTime = endTime;
        }
        
        public override string ToString()
        {
            if (isLongNote)
                return $"LN: Lane {lane}, Time {time}-{endTime}ms";
            else
                return $"Note: Lane {lane}, Time {time}ms";
        }
    }
}
