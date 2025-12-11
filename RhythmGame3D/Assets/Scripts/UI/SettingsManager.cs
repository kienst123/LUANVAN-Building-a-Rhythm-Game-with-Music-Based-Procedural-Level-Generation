using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RhythmGame3D.UI
{
    /// <summary>
    /// Manages game settings (audio volumes)
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [Header("Volume Sliders")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        
        [Header("Volume Text")]
        public TextMeshProUGUI masterVolumeText;
        public TextMeshProUGUI musicVolumeText;
        public TextMeshProUGUI sfxVolumeText;
        
        [Header("Buttons")]
        public Button backButton;
        
        [Header("References")]
        public MenuManager menuManager;
        
        // PlayerPrefs keys
        private const string MASTER_VOLUME_KEY = "MasterVolume";
        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string SFX_VOLUME_KEY = "SFXVolume";
        
        void Start()
        {
            // Load saved settings
            LoadSettings();
            
            // Setup slider listeners
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            
            // Setup back button
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
            
            // Update UI
            UpdateUI();
        }
        
        /// <summary>
        /// Load settings from PlayerPrefs
        /// </summary>
        void LoadSettings()
        {
            float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
            float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);
            
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = masterVol;
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = musicVol;
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = sfxVol;
            
            // Apply to AudioListener
            AudioListener.volume = masterVol;
            
            Debug.Log($"[SettingsManager] Loaded settings - Master: {masterVol:F2}, Music: {musicVol:F2}, SFX: {sfxVol:F2}");
        }
        
        /// <summary>
        /// Save settings to PlayerPrefs
        /// </summary>
        void SaveSettings()
        {
            if (masterVolumeSlider != null)
                PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolumeSlider.value);
            
            if (musicVolumeSlider != null)
                PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolumeSlider.value);
            
            if (sfxVolumeSlider != null)
                PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolumeSlider.value);
            
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Master volume changed
        /// </summary>
        void OnMasterVolumeChanged(float value)
        {
            AudioListener.volume = value;
            UpdateUI();
            SaveSettings();
        }
        
        /// <summary>
        /// Music volume changed
        /// </summary>
        void OnMusicVolumeChanged(float value)
        {
            // Music volume will be applied in GameManager when loading
            UpdateUI();
            SaveSettings();
        }
        
        /// <summary>
        /// SFX volume changed
        /// </summary>
        void OnSFXVolumeChanged(float value)
        {
            // SFX volume will be applied in AudioManager when loading
            UpdateUI();
            SaveSettings();
        }
        
        /// <summary>
        /// Update UI text displays
        /// </summary>
        void UpdateUI()
        {
            if (masterVolumeText != null && masterVolumeSlider != null)
                masterVolumeText.text = $"{(masterVolumeSlider.value * 100):F0}%";
            
            if (musicVolumeText != null && musicVolumeSlider != null)
                musicVolumeText.text = $"{(musicVolumeSlider.value * 100):F0}%";
            
            if (sfxVolumeText != null && sfxVolumeSlider != null)
                sfxVolumeText.text = $"{(sfxVolumeSlider.value * 100):F0}%";
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
        /// Get saved music volume (for use in GameManager)
        /// </summary>
        public static float GetMusicVolume()
        {
            return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
        }
        
        /// <summary>
        /// Get saved SFX volume (for use in AudioManager)
        /// </summary>
        public static float GetSFXVolume()
        {
            return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);
        }
    }
}
