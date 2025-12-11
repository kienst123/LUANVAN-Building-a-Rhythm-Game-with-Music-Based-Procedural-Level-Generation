using UnityEngine;

namespace RhythmGame3D.Core
{
    /// <summary>
    /// Manages all sound effects for the rhythm game
    /// </summary>
    public class AudioManager3D : MonoBehaviour
    {
        [Header("Hit Sounds")]
        [Tooltip("Sound when hitting with Perfect timing")]
        public AudioClip perfectHitSound;
        
        [Tooltip("Sound when hitting with Great timing")]
        public AudioClip greatHitSound;
        
        [Tooltip("Sound when hitting with Good timing")]
        public AudioClip goodHitSound;
        
        [Tooltip("Sound when missing a note")]
        public AudioClip missSound;
        
        [Header("Combo Sounds")]
        [Tooltip("Sound when combo breaks")]
        public AudioClip comboBreakSound;
        
        [Header("UI Sounds")]
        [Tooltip("Sound when clicking buttons")]
        public AudioClip buttonClickSound;
        
        [Tooltip("Sound when game over")]
        public AudioClip gameOverSound;
        
        [Header("Countdown Sounds")]
        [Tooltip("Countdown sounds: 3, 2, 1, Go")]
        public AudioClip countdown3Sound;
        public AudioClip countdown2Sound;
        public AudioClip countdown1Sound;
        public AudioClip countdownGoSound;
        
        [Header("Settings")]
        [Range(0f, 1f)]
        [Tooltip("Master volume for all sound effects")]
        public float masterVolume = 1f;
        
        [Range(0f, 1f)]
        [Tooltip("Volume for hit sounds")]
        public float hitSoundVolume = 0.8f;
        
        [Range(0f, 1f)]
        [Tooltip("Volume for UI sounds")]
        public float uiSoundVolume = 0.6f;
        
        [Header("Audio Sources")]
        [Tooltip("Audio source for hit sounds (will auto-create if null)")]
        public AudioSource hitAudioSource;
        
        [Tooltip("Audio source for UI sounds (will auto-create if null)")]
        public AudioSource uiAudioSource;
        
        void Awake()
        {
            // Create audio sources if not assigned
            if (hitAudioSource == null)
            {
                GameObject hitSourceGO = new GameObject("HitAudioSource");
                hitSourceGO.transform.SetParent(transform);
                hitAudioSource = hitSourceGO.AddComponent<AudioSource>();
                hitAudioSource.playOnAwake = false;
            }
            
            if (uiAudioSource == null)
            {
                GameObject uiSourceGO = new GameObject("UIAudioSource");
                uiSourceGO.transform.SetParent(transform);
                uiAudioSource = uiSourceGO.AddComponent<AudioSource>();
                uiAudioSource.playOnAwake = false;
            }
        }
        
        /// <summary>
        /// Play hit sound based on judgment
        /// </summary>
        public void PlayHitSound(string judgment)
        {
            AudioClip clip = null;
            
            switch (judgment)
            {
                case "Perfect":
                    clip = perfectHitSound;
                    break;
                case "Great":
                    clip = greatHitSound;
                    break;
                case "Good":
                    clip = goodHitSound;
                    break;
                case "Miss":
                    clip = missSound;
                    break;
            }
            
            if (clip != null && hitAudioSource != null)
            {
                hitAudioSource.PlayOneShot(clip, masterVolume * hitSoundVolume);
            }
        }
        
        /// <summary>
        /// Play combo break sound
        /// </summary>
        public void PlayComboBreakSound()
        {
            if (comboBreakSound != null && uiAudioSource != null)
            {
                uiAudioSource.PlayOneShot(comboBreakSound, masterVolume * uiSoundVolume);
            }
        }
        
        /// <summary>
        /// Play button click sound
        /// </summary>
        public void PlayButtonClickSound()
        {
            if (buttonClickSound != null && uiAudioSource != null)
            {
                uiAudioSource.PlayOneShot(buttonClickSound, masterVolume * uiSoundVolume);
            }
        }
        
        /// <summary>
        /// Play game over sound
        /// </summary>
        public void PlayGameOverSound()
        {
            if (gameOverSound != null && uiAudioSource != null)
            {
                uiAudioSource.PlayOneShot(gameOverSound, masterVolume * uiSoundVolume);
            }
        }
        
        /// <summary>
        /// Play countdown sound (3, 2, 1, Go)
        /// </summary>
        public void PlayCountdownSound(int count)
        {
            AudioClip clip = null;
            
            switch (count)
            {
                case 3:
                    clip = countdown3Sound;
                    break;
                case 2:
                    clip = countdown2Sound;
                    break;
                case 1:
                    clip = countdown1Sound;
                    break;
                case 0:
                    clip = countdownGoSound;
                    break;
            }
            
            if (clip != null && uiAudioSource != null)
            {
                uiAudioSource.PlayOneShot(clip, masterVolume * uiSoundVolume);
                Debug.Log($"[AudioManager3D] Playing countdown: {count}");
            }
        }
        
        /// <summary>
        /// Set master volume for all sounds
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
        }
        
        /// <summary>
        /// Set hit sound volume
        /// </summary>
        public void SetHitSoundVolume(float volume)
        {
            hitSoundVolume = Mathf.Clamp01(volume);
        }
        
        /// <summary>
        /// Set UI sound volume
        /// </summary>
        public void SetUISoundVolume(float volume)
        {
            uiSoundVolume = Mathf.Clamp01(volume);
        }
    }
}
