using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace RhythmGame3D.UI
{
    /// <summary>
    /// Manages game pause/resume functionality
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject pausePanel;
        public Button resumeButton;
        public Button returnToMenuButton;
        
        [Header("Settings")]
        public string mainMenuSceneName = "MainMenu";
        public KeyCode pauseKey = KeyCode.Escape;
        public KeyCode alternatePauseKey = KeyCode.P;
        
        private bool isPaused = false;
        
        void Start()
        {
            // Setup button listeners
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);
            
            if (returnToMenuButton != null)
                returnToMenuButton.onClick.AddListener(ReturnToMenu);
            
            // Hide pause panel at start
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }
        
        void Update()
        {
            // Check for pause key press
            if (Input.GetKeyDown(pauseKey) || Input.GetKeyDown(alternatePauseKey))
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }
        }
        
        /// <summary>
        /// Pause the game
        /// </summary>
        public void Pause()
        {
            if (isPaused) return;
            
            isPaused = true;
            Time.timeScale = 0f; // Freeze game time
            
            if (pausePanel != null)
                pausePanel.SetActive(true);
            
            // Pause music
            AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
            foreach (var source in audioSources)
            {
                if (source.isPlaying)
                    source.Pause();
            }
            
            Debug.Log("[PauseManager] Game paused");
        }
        
        /// <summary>
        /// Resume the game
        /// </summary>
        public void Resume()
        {
            if (!isPaused) return;
            
            isPaused = false;
            Time.timeScale = 1f; // Restore normal time
            
            if (pausePanel != null)
                pausePanel.SetActive(false);
            
            // Resume music
            AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
            foreach (var source in audioSources)
            {
                source.UnPause();
            }
            
            Debug.Log("[PauseManager] Game resumed");
        }
        
        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMenu()
        {
            Debug.Log("[PauseManager] Returning to main menu...");
            
            // Restore time scale before loading new scene
            Time.timeScale = 1f;
            
            // Load main menu
            SceneManager.LoadScene(mainMenuSceneName);
        }
        
        /// <summary>
        /// Check if game is currently paused
        /// </summary>
        public bool IsPaused()
        {
            return isPaused;
        }
        
        void OnDestroy()
        {
            // Ensure time scale is restored when scene unloads
            Time.timeScale = 1f;
        }
    }
}
