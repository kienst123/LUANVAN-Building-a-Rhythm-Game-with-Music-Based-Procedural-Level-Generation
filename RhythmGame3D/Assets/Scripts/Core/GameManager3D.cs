using UnityEngine;
using UnityEngine.SceneManagement;
using RhythmGame3D.Beatmap;
using RhythmGame3D.Gameplay;
using RhythmGame3D.UI;
using RhythmGame3D.UI.Menu3D;

namespace RhythmGame3D.Core
{
    /// <summary>
    /// Main game manager for 3D rhythm game
    /// Coordinates all systems
    /// </summary>
    public class GameManager3D : MonoBehaviour
    {
        [Header("References")]
        public NoteSpawner3D noteSpawner;
        public InputManager3D inputManager;
        public JudgmentSystem judgmentSystem;
        public ModernUIManager3D uiManager;
        public AudioManager3D audioManager;
        public AudioSource musicSource;
        
        [Header("Visual Effects")]
        public GameplayTunnelBackground tunnelBackground;
        
        [Header("Results Screen")]
        public ResultsScreen3D resultsScreen;
        
        [Header("Beatmap")]
        public string beatmapFilePath;
        
        [Header("Settings")]
        public float audioOffset = 0.1f;  // Audio delay compensation
        
        [Header("Health System")]
        public float maxHealth = 100f;
        public float healthGainPerHit = 1f;
        public float healthLossPerMiss = 5f;
        public float healthLossPerEmptyPress = 2f;  // Penalty for pressing with no note
        
        // State
        private BeatmapData currentBeatmap;
        private bool isPlaying = false;
        private float songTime = 0f;
        private float currentHealth = 100f;
        
        // Judgment tracking
        private int perfectCount = 0;
        private int greatCount = 0;
        private int goodCount = 0;
        private int missCount = 0;
        private int maxCombo = 0;
        private int totalNotes = 0;
        
        void Start()
        {
            Initialize();
        }
        
        void Initialize()
        {
            // Setup tunnel background if not assigned
            if (tunnelBackground == null)
            {
                GameObject tunnelObj = new GameObject("GameplayTunnelBackground");
                tunnelBackground = tunnelObj.AddComponent<GameplayTunnelBackground>();
                Debug.Log("[GameManager3D] Created tunnel background");
            }
            
            // Setup results screen if not assigned
            if (resultsScreen == null)
            {
                GameObject resultsObj = new GameObject("ResultsScreen3D");
                resultsScreen = resultsObj.AddComponent<ResultsScreen3D>();
                Debug.Log("[GameManager3D] Created results screen");
            }
            
            // Subscribe to judgment events
            if (judgmentSystem != null)
            {
                judgmentSystem.OnJudgment += OnJudgmentReceived;
                judgmentSystem.OnComboChange += OnComboChanged;
            }
            
            // Subscribe to input events
            if (inputManager != null)
            {
                inputManager.OnEmptyPress += OnEmptyPress;
            }
            
            // Check if beatmap was passed from menu (check both old and new systems)
            if (BeatmapStorage.currentBeatmap != null)
            {
                Debug.Log($"[GameManager3D] Loading beatmap from 3D menu selection");
                currentBeatmap = BeatmapStorage.currentBeatmap;
                
                // Count total notes
                totalNotes = currentBeatmap.hitObjects.Count;
                Debug.Log($"[GameManager3D] Total notes: {totalNotes}");
                
                // Load into spawner
                if (noteSpawner != null)
                {
                    noteSpawner.LoadBeatmap(currentBeatmap);
                }
                
                // Load music from path
                if (!string.IsNullOrEmpty(BeatmapStorage.currentMusicPath))
                {
                    LoadMusicFromFile(BeatmapStorage.currentMusicPath);
                }
                
                Debug.Log($"[GameManager3D] Beatmap loaded from 3D menu!");
            }
            else if (BeatmapSelector.currentBeatmap != null)
            {
                Debug.Log($"[GameManager3D] Loading beatmap from old menu selection");
                currentBeatmap = BeatmapSelector.currentBeatmap;
                
                // Count total notes
                totalNotes = currentBeatmap.hitObjects.Count;
                Debug.Log($"[GameManager3D] Total notes: {totalNotes}");
                
                // Load into spawner
                if (noteSpawner != null)
                {
                    noteSpawner.LoadBeatmap(currentBeatmap);
                }
                
                // Load music from path
                if (!string.IsNullOrEmpty(BeatmapSelector.currentMusicPath))
                {
                    LoadMusicFromFile(BeatmapSelector.currentMusicPath);
                }
                
                Debug.Log($"[GameManager3D] Beatmap loaded from old menu!");
            }
            // Otherwise, load beatmap if path specified
            else if (!string.IsNullOrEmpty(beatmapFilePath))
            {
                LoadBeatmap(beatmapFilePath);
            }
            else
            {
                Debug.LogWarning("[GameManager3D] No beatmap loaded!");
            }
            
            Debug.Log("[GameManager3D] Initialized");
        }
        
        /// <summary>
        /// Load beatmap from file
        /// </summary>
        public void LoadBeatmap(string filePath)
        {
            Debug.Log($"[GameManager3D] Loading beatmap: {filePath}");
            
            currentBeatmap = BeatmapParser.ParseBeatmap(filePath);
            
            if (currentBeatmap == null)
            {
                Debug.LogError("[GameManager3D] Failed to load beatmap!");
                return;
            }
            
            // Count total notes
            totalNotes = currentBeatmap.hitObjects.Count;
            Debug.Log($"[GameManager3D] Total notes: {totalNotes}");
            
            // Load into spawner
            if (noteSpawner != null)
            {
                noteSpawner.LoadBeatmap(currentBeatmap);
            }
            
            // Load audio
            LoadAudio();
            
            Debug.Log($"[GameManager3D] Beatmap loaded: {currentBeatmap}");
        }
        
        /// <summary>
        /// Load audio file
        /// </summary>
        void LoadAudio()
        {
            if (musicSource == null || currentBeatmap == null)
                return;
            
            // Get beatmap directory
            string beatmapDir = System.IO.Path.GetDirectoryName(beatmapFilePath);
            string audioPath = System.IO.Path.Combine(beatmapDir, currentBeatmap.audioFilename);
            
            if (!System.IO.File.Exists(audioPath))
            {
                Debug.LogError($"[GameManager3D] Audio file not found: {audioPath}");
                return;
            }
            
            // Load audio (note: needs special handling for runtime loading)
            Debug.Log($"[GameManager3D] Audio path: {audioPath}");
            Debug.LogWarning("[GameManager3D] Audio loading from file not implemented. Assign AudioClip manually in Inspector.");
        }
        
        /// <summary>
        /// Load music from file path (runtime loading)
        /// </summary>
        void LoadMusicFromFile(string filePath)
        {
            if (musicSource == null)
            {
                Debug.LogError("[GameManager3D] No music AudioSource!");
                return;
            }
            
            Debug.Log($"[GameManager3D] Loading music from: {filePath}");
            
            // Use UnityWebRequest to load audio at runtime
            StartCoroutine(LoadAudioClip(filePath));
        }
        
        /// <summary>
        /// Coroutine to load audio clip from file
        /// </summary>
        System.Collections.IEnumerator LoadAudioClip(string path)
        {
            // Convert to file:// URL format
            string url = "file://" + path;
            
            // Determine audio type from extension
            AudioType audioType = AudioType.MPEG;
            string ext = System.IO.Path.GetExtension(path).ToLower();
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
                    musicSource.clip = clip;
                    Debug.Log($"[GameManager3D] Music loaded! Duration: {clip.length}s");
                    
                    // Auto-start after loading
                    StartSong();
                }
                else
                {
                    Debug.LogError($"[GameManager3D] Failed to load audio: {www.error}");
                }
            }
        }
        
        /// <summary>
        /// Start playing the song
        /// </summary>
        public void StartSong()
        {
            if (currentBeatmap == null)
            {
                Debug.LogError("[GameManager3D] No beatmap loaded!");
                return;
            }
            
            if (musicSource != null && musicSource.clip != null)
            {
                musicSource.Play();
            }
            
            isPlaying = true;
            songTime = -audioOffset;
            
            // Reset health
            currentHealth = maxHealth;
            if (uiManager != null)
            {
                uiManager.UpdateHealth(1f);  // Full health at start
            }
            
            if (judgmentSystem != null)
                judgmentSystem.ResetStats();
            
            if (noteSpawner != null)
                noteSpawner.ResetSpawner();
            
            Debug.Log("[GameManager3D] Song started!");
        }
        
        /// <summary>
        /// Stop the song
        /// </summary>
        public void StopSong()
        {
            if (!isPlaying)
            {
                Debug.Log("[GameManager3D] Already stopped");
                return;
            }
            
            if (musicSource != null)
                musicSource.Stop();
            
            isPlaying = false;
            songTime = 0f;
            
            // Disable input
            if (inputManager != null)
                inputManager.SetInputEnabled(false);
            
            Debug.LogWarning($"[GameManager3D] Song stopped! Final score: {judgmentSystem.totalScore}, Health: {currentHealth:F1}");
        }
        
        /// <summary>
        /// Pause/Resume
        /// </summary>
        public void TogglePause()
        {
            if (musicSource != null)
            {
                if (musicSource.isPlaying)
                    musicSource.Pause();
                else
                    musicSource.UnPause();
            }
        }
        
        void Update()
        {
            if (!isPlaying) return;
            
            // Update song time
            if (musicSource != null && musicSource.isPlaying)
            {
                songTime = musicSource.time - audioOffset;
            }
            else
            {
                songTime += Time.deltaTime;
            }
            
            // Update systems
            if (noteSpawner != null)
                noteSpawner.UpdateSpawning(songTime);
            
            if (inputManager != null)
                inputManager.UpdateSongTime(songTime);
            
            // Update UI
            UpdateUI();
            
            // Check for song end
            if (musicSource != null && !musicSource.isPlaying && songTime > 1f)
            {
                OnSongEnd();
            }
        }
        
        /// <summary>
        /// Update UI displays
        /// </summary>
        void UpdateUI()
        {
            if (uiManager == null || judgmentSystem == null) return;
            
            uiManager.UpdateScore(judgmentSystem.totalScore);
            uiManager.UpdateAccuracy(judgmentSystem.accuracy);
            
            if (musicSource != null && musicSource.clip != null)
            {
                uiManager.UpdateTime(songTime, musicSource.clip.length);
            }
        }
        
        /// <summary>
        /// Handle judgment received
        /// </summary>
        void OnJudgmentReceived(JudgmentResult result)
        {
            // Track judgment counts
            if (result.judgment == "Perfect")
                perfectCount++;
            else if (result.judgment == "Great")
                greatCount++;
            else if (result.judgment == "Good")
                goodCount++;
            else if (result.judgment == "Miss")
                missCount++;
            
            // Pulse tunnel on perfect hits
            if (result.judgment == "Perfect" && tunnelBackground != null)
            {
                tunnelBackground.PulseOnBeat();
            }
            
            // Play hit sound
            if (audioManager != null)
            {
                audioManager.PlayHitSound(result.judgment);
            }
            
            if (uiManager != null)
            {
                uiManager.ShowJudgment(result.judgment, result.timingDifference);
            }
            
            // Update health based on judgment
            if (result.isHit)
            {
                // Gain health on hit (Perfect, Great, Good)
                if (result.judgment == "Perfect")
                    currentHealth += healthGainPerHit * 2f;  // Perfect = +2 health
                else if (result.judgment == "Great")
                    currentHealth += healthGainPerHit;       // Great = +1 health
                else
                    currentHealth += healthGainPerHit * 0.5f; // Good = +0.5 health
            }
            else
            {
                // Lose health on miss
                currentHealth -= healthLossPerMiss;
            }
            
            // Clamp health between 0 and max
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            
            // Update UI
            if (uiManager != null)
            {
                float normalizedHealth = currentHealth / maxHealth;
                uiManager.UpdateHealth(normalizedHealth);  // Normalized 0-1
                Debug.Log($"[GameManager3D] Health: {currentHealth:F1}/{maxHealth} ({normalizedHealth:P0}) - {result.judgment}");
            }
            
            // Check game over
            if (currentHealth <= 0f)
            {
                Debug.LogWarning("[GameManager3D] Health depleted! Game Over!");
                GameOver();
            }
        }
        
        /// <summary>
        /// Handle combo change
        /// </summary>
        void OnComboChanged(int combo)
        {
            // Track max combo
            if (combo > maxCombo)
            {
                maxCombo = combo;
            }
            
            // Play combo break sound if combo dropped to 0 from a higher value
            if (combo == 0 && judgmentSystem != null && judgmentSystem.maxCombo > 0)
            {
                if (audioManager != null)
                {
                    audioManager.PlayComboBreakSound();
                }
            }
            
            if (uiManager != null)
            {
                uiManager.UpdateCombo(combo);
            }
            
            // Update tunnel intensity based on combo
            if (tunnelBackground != null)
            {
                float intensity = Mathf.Clamp01(combo / 100f); // Max intensity at 100 combo
                tunnelBackground.SetIntensity(intensity);
            }
        }
        
        /// <summary>
        /// Handle empty press (player pressed key with no note nearby)
        /// </summary>
        void OnEmptyPress()
        {
            // Apply health penalty
            currentHealth -= healthLossPerEmptyPress;
            
            // Clamp health between 0 and max
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            
            // Update UI
            if (uiManager != null)
            {
                float normalizedHealth = currentHealth / maxHealth;
                uiManager.UpdateHealth(normalizedHealth);
                Debug.Log($"[GameManager3D] EMPTY PRESS! Health: {currentHealth:F1}/{maxHealth} ({normalizedHealth:P0}) [-{healthLossPerEmptyPress}]");
            }
            
            // Check game over
            if (currentHealth <= 0f)
            {
                Debug.LogWarning("[GameManager3D] Health depleted from empty presses! Game Over!");
                GameOver();
            }
        }
        
        /// <summary>
        /// Called when song ends
        /// </summary>
        void OnSongEnd()
        {
            isPlaying = false;
            
            Debug.Log("[GameManager3D] Song ended!");
            Debug.Log(judgmentSystem.ToString());
            
            // Show results screen
            if (resultsScreen != null && judgmentSystem != null)
            {
                int finalScore = judgmentSystem.totalScore;
                float accuracy = judgmentSystem.accuracy;
                
                resultsScreen.ShowResults(
                    finalScore,
                    accuracy,
                    maxCombo,
                    perfectCount,
                    greatCount,
                    goodCount,
                    missCount
                );
                
                Debug.Log($"[GameManager3D] Results: Score={finalScore}, Acc={accuracy:F2}%, MaxCombo={maxCombo}, P/G/G/M={perfectCount}/{greatCount}/{goodCount}/{missCount}");
            }
        }
        
        /// <summary>
        /// Keyboard shortcuts
        /// </summary>
        void LateUpdate()
        {
            // Space = Start/Pause
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!isPlaying)
                    StartSong();
                else
                    TogglePause();
            }
            
            // R = Restart
            if (Input.GetKeyDown(KeyCode.R))
            {
                StopSong();
                StartSong();
            }
            
            // Escape = Stop
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopSong();
            }
        }
        
        /// <summary>
        /// Handle game over
        /// </summary>
        void GameOver()
        {
            // Stop game IMMEDIATELY
            isPlaying = false;
            
            // Stop and freeze music
            if (musicSource != null)
            {
                musicSource.Stop();
            }
            
            // Disable input immediately
            if (inputManager != null)
            {
                inputManager.SetInputEnabled(false);
            }
            
            // Stop spawning notes
            if (noteSpawner != null)
            {
                noteSpawner.StopSpawning();
            }
            
            // Clear all active notes to freeze score
            if (noteSpawner != null)
            {
                noteSpawner.ClearAllNotes();
            }
            
            Debug.LogWarning($"[GameManager3D] GAME OVER! Final Score: {judgmentSystem.totalScore}, Accuracy: {judgmentSystem.accuracy:F2}%");
            
            // Play game over sound
            if (audioManager != null)
            {
                audioManager.PlayGameOverSound();
            }
            
            // Show game over UI with final stats (score is frozen now)
            if (uiManager != null && judgmentSystem != null)
            {
                uiManager.ShowGameOver(
                    judgmentSystem.totalScore,
                    judgmentSystem.accuracy,
                    judgmentSystem.maxCombo,
                    judgmentSystem.perfectCount,
                    judgmentSystem.greatCount,
                    judgmentSystem.goodCount,
                    judgmentSystem.missCount,
                    judgmentSystem.earlyCount,
                    judgmentSystem.lateCount
                );
            }
        }
        
        /// <summary>
        /// Restart the game (reload current scene)
        /// </summary>
        public void RestartGame()
        {
            if (audioManager != null)
            {
                audioManager.PlayButtonClickSound();
            }
            
            Debug.Log("[GameManager3D] Restarting game...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        /// <summary>
        /// Exit game or return to menu
        /// </summary>
        public void ExitGame()
        {
            if (audioManager != null)
            {
                audioManager.PlayButtonClickSound();
            }
            
            Debug.Log("[GameManager3D] Exiting game...");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        void OnDestroy()
        {
            // Unsubscribe events
            if (judgmentSystem != null)
            {
                judgmentSystem.OnJudgment -= OnJudgmentReceived;
                judgmentSystem.OnComboChange -= OnComboChanged;
            }
        }
    }
}
