using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;
using RhythmGame3D.Beatmap;

namespace RhythmGame3D.UI
{
    /// <summary>
    /// Manages beatmap selection and import
    /// </summary>
    public class BeatmapSelector : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button importButton;
        public Button backButton;
        public TMP_Dropdown difficultyDropdown;
        public TextMeshProUGUI selectedFileText;
        public TextMeshProUGUI infoText;
        
        [Header("References")]
        public MenuManager menuManager;
        
        [Header("Selected Beatmap")]
        public string selectedMusicFile = "";
        public int selectedDifficulty = 0; // 0=Easy, 1=Normal, 2=Hard
        private float musicDuration = 180f; // Default 3 minutes, will be updated when loading audio
        
        // Static property to pass data to gameplay scene
        public static BeatmapData currentBeatmap { get; private set; }
        public static string currentMusicPath { get; private set; }
        
        void Start()
        {
            // Setup button listeners
            if (importButton != null)
                importButton.onClick.AddListener(OnImportClicked);
            
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
            
            // Setup difficulty dropdown
            if (difficultyDropdown != null)
            {
                difficultyDropdown.ClearOptions();
                difficultyDropdown.AddOptions(new System.Collections.Generic.List<string> { "Easy", "Normal", "Hard" });
                difficultyDropdown.value = 0;
                difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
            }
            
            UpdateUI();
        }
        
        /// <summary>
        /// Import button clicked - Open file browser
        /// </summary>
        void OnImportClicked()
        {
            Debug.Log("[BeatmapSelector] Opening file browser...");
            
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            // Windows file dialog
            string path = UnityEditor.EditorUtility.OpenFilePanel("Select Music File", "", "mp3,ogg,wav");
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            // macOS file dialog
            string path = UnityEditor.EditorUtility.OpenFilePanel("Select Music File", "", "mp3,ogg,wav");
#else
            string path = "";
#endif
            
            if (!string.IsNullOrEmpty(path))
            {
                selectedMusicFile = path;
                Debug.Log($"[BeatmapSelector] Selected file: {path}");
                
                // Generate beatmap using the selected music
                GenerateBeatmap(path);
                
                UpdateUI();
            }
        }
        
        /// <summary>
        /// Difficulty dropdown changed
        /// </summary>
        void OnDifficultyChanged(int value)
        {
            selectedDifficulty = value;
            Debug.Log($"[BeatmapSelector] Difficulty changed to: {GetDifficultyName(value)}");
            
            // Regenerate beatmap if music file is already selected
            if (!string.IsNullOrEmpty(selectedMusicFile))
            {
                Debug.Log("[BeatmapSelector] Regenerating beatmap with new difficulty...");
                GenerateBeatmap(selectedMusicFile);
            }
            
            UpdateUI();
        }
        
        /// <summary>
        /// Back button clicked
        /// </summary>
        void OnBackClicked()
        {
            if (menuManager != null)
                menuManager.ShowMainPanel();
        }
        
        /// <summary>
        /// Update UI displays
        /// </summary>
        void UpdateUI()
        {
            if (selectedFileText != null)
            {
                if (string.IsNullOrEmpty(selectedMusicFile))
                {
                    selectedFileText.text = "No file selected";
                    selectedFileText.color = Color.gray;
                }
                else
                {
                    string fileName = Path.GetFileName(selectedMusicFile);
                    selectedFileText.text = $"File: {fileName}";
                    selectedFileText.color = Color.white;
                }
            }
            
            if (infoText != null)
            {
                if (string.IsNullOrEmpty(selectedMusicFile))
                {
                    infoText.text = "Click IMPORT MUSIC to select a music file\nThen choose difficulty and click Play!";
                    infoText.color = Color.gray;
                }
                else if (currentBeatmap == null)
                {
                    infoText.text = "Generating beatmap...";
                    infoText.color = Color.yellow;
                }
                else
                {
                    string diffName = GetDifficultyName(selectedDifficulty);
                    int noteCount = currentBeatmap.hitObjects.Count;
                    
                    // Calculate notes per second based on difficulty
                    float notesPerSecond;
                    switch (selectedDifficulty)
                    {
                        case 0: notesPerSecond = 1.0f; break;
                        case 1: notesPerSecond = 3.0f; break;
                        case 2: notesPerSecond = 5.0f; break;
                        default: notesPerSecond = 1.0f; break;
                    }
                    
                    infoText.text = $"<color=#FFD700>Difficulty: {diffName}</color>\n" +
                                   $"<color=#00FF00>Notes: {noteCount}</color>\n" +
                                   $"<color=#00FFFF>Density: {notesPerSecond:F1} notes/sec</color>\n" +
                                   $"<color=#FFFF00>Ready to play!</color>";
                    infoText.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// Get difficulty name from index
        /// </summary>
        string GetDifficultyName(int difficulty)
        {
            switch (difficulty)
            {
                case 0: return "Easy";
                case 1: return "Normal";
                case 2: return "Hard";
                default: return "Normal";
            }
        }
        
        /// <summary>
        /// Check if beatmap is ready
        /// </summary>
        public bool IsBeatmapReady()
        {
            return !string.IsNullOrEmpty(selectedMusicFile) && currentBeatmap != null;
        }
        
        /// <summary>
        /// Generate beatmap from music file
        /// </summary>
        void GenerateBeatmap(string musicPath)
        {
            Debug.Log($"[BeatmapSelector] Generating beatmap for: {musicPath}");
            
            // Store music path
            currentMusicPath = musicPath;
            
            // Load audio to get duration, then create beatmap
            StartCoroutine(LoadAudioAndGenerateBeatmap(musicPath));
        }
        
        /// <summary>
        /// Load audio file to get duration, then generate beatmap
        /// </summary>
        System.Collections.IEnumerator LoadAudioAndGenerateBeatmap(string musicPath)
        {
            // Convert to file:// URL format
            string url = "file://" + musicPath;
            
            // Determine audio type from extension
            AudioType audioType = AudioType.MPEG;
            string ext = System.IO.Path.GetExtension(musicPath).ToLower();
            if (ext == ".ogg")
                audioType = AudioType.OGGVORBIS;
            else if (ext == ".wav")
                audioType = AudioType.WAV;
            
            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                yield return www.SendWebRequest();
                
                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                    musicDuration = clip.length;
                    
                    Debug.Log($"[BeatmapSelector] Audio loaded! Duration: {musicDuration:F2} seconds ({musicDuration/60:F2} minutes)");
                    
                    // Now create beatmap with actual duration
                    BeatmapData beatmap = CreateSimpleBeatmap(musicPath);
                    currentBeatmap = beatmap;
                    
                    Debug.Log($"[BeatmapSelector] Beatmap generated with {beatmap.hitObjects.Count} notes for {musicDuration:F2}s");
                    
                    UpdateUI();
                }
                else
                {
                    Debug.LogError($"[BeatmapSelector] Failed to load audio: {www.error}");
                    Debug.LogWarning("[BeatmapSelector] Using default duration of 3 minutes");
                    
                    // Fallback: use default duration
                    musicDuration = 180f;
                    BeatmapData beatmap = CreateSimpleBeatmap(musicPath);
                    currentBeatmap = beatmap;
                    
                    UpdateUI();
                }
            }
        }
        
        /// <summary>
        /// Create a simple test beatmap
        /// (Replace with BeatLearning later)
        /// </summary>
        BeatmapData CreateSimpleBeatmap(string musicPath)
        {
            BeatmapData beatmap = new BeatmapData();
            
            beatmap.audioFilename = System.IO.Path.GetFileName(musicPath);
            beatmap.title = System.IO.Path.GetFileNameWithoutExtension(musicPath);
            beatmap.artist = "Unknown";
            beatmap.creator = "BeatmapSelector";
            beatmap.version = GetDifficultyName(selectedDifficulty);
            beatmap.circleSize = 4; // 4 lanes
            beatmap.overallDifficulty = selectedDifficulty * 3 + 3; // 3, 6, or 9
            
            // Generate simple pattern based on difficulty
            // Easy: 1 note per second (58 notes in 60s)
            // Normal: 3 notes per second (174 notes in 60s)
            // Hard: 5 notes per second (290 notes in 60s)
            float notesPerSecond;
            switch (selectedDifficulty)
            {
                case 0: // Easy
                    notesPerSecond = 1.0f;
                    break;
                case 1: // Normal
                    notesPerSecond = 3.0f;
                    break;
                case 2: // Hard
                    notesPerSecond = 5.0f;
                    break;
                default:
                    notesPerSecond = 1.0f;
                    break;
            }
            
            float interval = 1.0f / notesPerSecond;
            
            // Generate for full song duration (actual audio length)
            float duration = musicDuration;
            
            Debug.Log($"[BeatmapSelector] Generating beatmap - Duration: {duration:F2}s, Difficulty: {GetDifficultyName(selectedDifficulty)}, Notes/sec: {notesPerSecond}");
            
            // Smart pattern generation
            System.Random random = new System.Random();
            int[] laneCount = new int[4]; // Track notes per lane for balance
            int lastLane = -1;
            int secondLastLane = -1;
            
            int noteCount = 0;
            for (float time = 2f; time < duration; time += interval)
            {
                int lane;
                
                // Choose lane with smart logic
                if (selectedDifficulty == 0) // Easy - Simple patterns
                {
                    // Avoid 2 consecutive same lanes
                    do
                    {
                        lane = random.Next(0, 4);
                    } while (lane == lastLane);
                }
                else if (selectedDifficulty == 1) // Normal - More variety
                {
                    // Avoid 3 consecutive same lanes
                    do
                    {
                        // Weighted random - prefer lanes with fewer notes
                        int totalNotes = noteCount;
                        if (totalNotes > 0)
                        {
                            // Find least used lane
                            int minCount = int.MaxValue;
                            for (int i = 0; i < 4; i++)
                                if (laneCount[i] < minCount)
                                    minCount = laneCount[i];
                            
                            // Create weighted list
                            System.Collections.Generic.List<int> weightedLanes = new System.Collections.Generic.List<int>();
                            for (int i = 0; i < 4; i++)
                            {
                                // Add lane multiple times if it's used less
                                int weight = (minCount + 2) - (laneCount[i] - minCount);
                                for (int w = 0; w < weight; w++)
                                    weightedLanes.Add(i);
                            }
                            
                            lane = weightedLanes[random.Next(weightedLanes.Count)];
                        }
                        else
                        {
                            lane = random.Next(0, 4);
                        }
                    } while (lane == lastLane && lane == secondLastLane);
                }
                else // Hard - Complex patterns
                {
                    // Advanced patterns: jacks, stairs, streams
                    float patternRoll = (float)random.NextDouble();
                    
                    if (patternRoll < 0.15f && lastLane != -1) // 15% Jack (same lane twice)
                    {
                        lane = lastLane;
                    }
                    else if (patternRoll < 0.30f && lastLane != -1) // 15% Stair pattern
                    {
                        // Move to adjacent lane
                        if (lastLane == 0) lane = 1;
                        else if (lastLane == 3) lane = 2;
                        else lane = lastLane + (random.Next(0, 2) == 0 ? -1 : 1);
                    }
                    else // 70% Random with balance
                    {
                        do
                        {
                            // Balanced random
                            int totalNotes = noteCount;
                            if (totalNotes > 8)
                            {
                                // Find least used lane
                                int minCount = laneCount.Min();
                                int maxCount = laneCount.Max();
                                
                                // If imbalance is too high, force balance
                                if (maxCount - minCount > 3)
                                {
                                    // Pick from lanes with min count
                                    var minLanes = new System.Collections.Generic.List<int>();
                                    for (int i = 0; i < 4; i++)
                                        if (laneCount[i] == minCount)
                                            minLanes.Add(i);
                                    lane = minLanes[random.Next(minLanes.Count)];
                                }
                                else
                                {
                                    lane = random.Next(0, 4);
                                }
                            }
                            else
                            {
                                lane = random.Next(0, 4);
                            }
                        } while (lane == lastLane && lane == secondLastLane);
                    }
                }
                
                laneCount[lane]++;
                int xPos = lane * 128; // osu!mania 4K: 0, 128, 256, 384
                
                HitObject note = new HitObject(
                    x: xPos,
                    y: 192, // Standard y position for mania
                    time: (int)(time * 1000), // Convert to milliseconds
                    type: 1, // Normal note (not LN)
                    hitSound: 0 // No custom hit sound
                );
                
                beatmap.hitObjects.Add(note);
                
                // Update lane history
                secondLastLane = lastLane;
                lastLane = lane;
                noteCount++;
            }
            
            Debug.Log($"[BeatmapSelector] Created simple beatmap with {noteCount} notes, interval: {interval}s");
            
            return beatmap;
        }
    }
}
