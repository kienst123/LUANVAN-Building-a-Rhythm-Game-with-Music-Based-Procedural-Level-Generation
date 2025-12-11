using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RhythmGame3D.Beatmap
{
    /// <summary>
    /// Parse beatmap files từ osu! format (.osu)
    /// </summary>
    public class BeatmapParser
    {
        public static BeatmapData ParseBeatmap(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Beatmap file not found: {filePath}");
                return null;
            }

            BeatmapData beatmap = new BeatmapData();
            string[] lines = File.ReadAllLines(filePath);
            
            string currentSection = "";
            
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                
                // Skip empty lines and comments
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
                    continue;
                
                // Check for section headers
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    continue;
                }
                
                // Parse sections
                switch (currentSection)
                {
                    case "General":
                        ParseGeneral(trimmedLine, beatmap);
                        break;
                    case "Metadata":
                        ParseMetadata(trimmedLine, beatmap);
                        break;
                    case "Difficulty":
                        ParseDifficulty(trimmedLine, beatmap);
                        break;
                    case "TimingPoints":
                        ParseTimingPoint(trimmedLine, beatmap);
                        break;
                    case "HitObjects":
                        ParseHitObject(trimmedLine, beatmap);
                        break;
                }
            }
            
            // Sort hit objects by time
            beatmap.SortHitObjects();
            
            Debug.Log($"Beatmap parsed: {beatmap}");
            return beatmap;
        }
        
        private static void ParseGeneral(string line, BeatmapData beatmap)
        {
            string[] parts = line.Split(':');
            if (parts.Length < 2) return;
            
            string key = parts[0].Trim();
            string value = parts[1].Trim();
            
            switch (key)
            {
                case "AudioFilename":
                    beatmap.audioFilename = value;
                    break;
                case "AudioLeadIn":
                    beatmap.audioLeadIn = int.Parse(value);
                    break;
                case "PreviewTime":
                    beatmap.previewTime = int.Parse(value);
                    break;
            }
        }
        
        private static void ParseMetadata(string line, BeatmapData beatmap)
        {
            string[] parts = line.Split(':');
            if (parts.Length < 2) return;
            
            string key = parts[0].Trim();
            string value = string.Join(":", parts.Skip(1)).Trim();
            
            switch (key)
            {
                case "Title":
                    beatmap.title = value;
                    break;
                case "Artist":
                    beatmap.artist = value;
                    break;
                case "Creator":
                    beatmap.creator = value;
                    break;
                case "Version":
                    beatmap.version = value;
                    break;
            }
        }
        
        private static void ParseDifficulty(string line, BeatmapData beatmap)
        {
            string[] parts = line.Split(':');
            if (parts.Length < 2) return;
            
            string key = parts[0].Trim();
            string value = parts[1].Trim();
            
            try
            {
                switch (key)
                {
                    case "HPDrainRate":
                        beatmap.hpDrainRate = float.Parse(value);
                        break;
                    case "CircleSize":
                        beatmap.circleSize = float.Parse(value);
                        beatmap.keyCount = (int)beatmap.circleSize;
                        break;
                    case "OverallDifficulty":
                        beatmap.overallDifficulty = float.Parse(value);
                        break;
                    case "ApproachRate":
                        beatmap.approachRate = float.Parse(value);
                        break;
                    case "SliderMultiplier":
                        beatmap.sliderMultiplier = float.Parse(value);
                        break;
                    case "SliderTickRate":
                        beatmap.sliderTickRate = float.Parse(value);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to parse difficulty value: {key} = {value}, Error: {e.Message}");
            }
        }
        
        private static void ParseTimingPoint(string line, BeatmapData beatmap)
        {
            string[] parts = line.Split(',');
            if (parts.Length < 8) return;
            
            try
            {
                int time = int.Parse(parts[0]);
                float beatLength = float.Parse(parts[1]);
                int meter = int.Parse(parts[2]);
                int sampleSet = int.Parse(parts[3]);
                int sampleIndex = int.Parse(parts[4]);
                int volume = int.Parse(parts[5]);
                bool uninherited = parts[6] == "1";
                int effects = int.Parse(parts[7]);
                
                TimingPoint tp = new TimingPoint(time, beatLength, meter, sampleSet, 
                                                sampleIndex, volume, uninherited, effects);
                beatmap.AddTimingPoint(tp);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to parse timing point: {line}, Error: {e.Message}");
            }
        }
        
        private static void ParseHitObject(string line, BeatmapData beatmap)
        {
            string[] parts = line.Split(',');
            if (parts.Length < 5) return;
            
            try
            {
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                int time = int.Parse(parts[2]);
                int type = int.Parse(parts[3]);
                int hitSound = int.Parse(parts[4]);
                
                HitObject hitObject = new HitObject(x, y, time, type, hitSound);
                
                // Parse long note end time if exists
                if (hitObject.isLongNote && parts.Length > 5)
                {
                    string[] endTimeParts = parts[5].Split(':');
                    if (endTimeParts.Length > 0)
                    {
                        int endTime = int.Parse(endTimeParts[0]);
                        hitObject.SetEndTime(endTime);
                    }
                }
                
                beatmap.AddHitObject(hitObject);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to parse hit object: {line}, Error: {e.Message}");
            }
        }
    }
}
