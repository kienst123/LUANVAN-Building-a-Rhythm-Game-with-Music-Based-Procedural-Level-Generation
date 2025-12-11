using System.Collections.Generic;
using UnityEngine;

namespace RhythmGame3D.Beatmap
{
    /// <summary>
    /// Chứa toàn bộ thông tin của một beatmap
    /// </summary>
    [System.Serializable]
    public class BeatmapData
    {
        // Metadata
        public string title;
        public string artist;
        public string creator;
        public string version;  // Difficulty name (Easy, Normal, Hard)
        public string audioFilename;
        
        // Timing
        public int previewTime;
        public float sliderMultiplier;
        public float sliderTickRate;
        
        // Difficulty settings
        public float hpDrainRate;
        public float circleSize;  // Số keys (4 cho 4K mania)
        public float overallDifficulty;
        public float approachRate;
        
        // Timing Points
        public List<TimingPoint> timingPoints = new List<TimingPoint>();
        
        // Hit Objects (notes)
        public List<HitObject> hitObjects = new List<HitObject>();
        
        // Audio
        public int audioLeadIn;
        
        // Number of keys (4K mania)
        public int keyCount = 4;
        
        public BeatmapData()
        {
            timingPoints = new List<TimingPoint>();
            hitObjects = new List<HitObject>();
        }
        
        public void AddHitObject(HitObject hitObject)
        {
            hitObjects.Add(hitObject);
        }
        
        public void AddTimingPoint(TimingPoint timingPoint)
        {
            timingPoints.Add(timingPoint);
        }
        
        public void SortHitObjects()
        {
            hitObjects.Sort((a, b) => a.time.CompareTo(b.time));
        }
        
        public override string ToString()
        {
            return $"{title} - {artist} [{version}]\n" +
                   $"Keys: {keyCount}, Notes: {hitObjects.Count}, " +
                   $"OD: {overallDifficulty}, HP: {hpDrainRate}";
        }
    }
    
    /// <summary>
    /// Timing point (BPM, time signature)
    /// </summary>
    [System.Serializable]
    public class TimingPoint
    {
        public int time;              // Thời điểm (ms)
        public float beatLength;      // Độ dài một beat (ms)
        public int meter;             // Time signature
        public int sampleSet;
        public int sampleIndex;
        public int volume;
        public bool uninherited;      // true = red line (BPM change)
        public int effects;
        
        public float bpm => 60000f / beatLength;
        
        public TimingPoint(int time, float beatLength, int meter, int sampleSet, 
                          int sampleIndex, int volume, bool uninherited, int effects)
        {
            this.time = time;
            this.beatLength = beatLength;
            this.meter = meter;
            this.sampleSet = sampleSet;
            this.sampleIndex = sampleIndex;
            this.volume = volume;
            this.uninherited = uninherited;
            this.effects = effects;
        }
    }
}
