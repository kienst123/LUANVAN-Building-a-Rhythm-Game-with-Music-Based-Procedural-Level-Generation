using UnityEngine;
using TMPro;

namespace RhythmGame3D.UI.Menu3D
{
    /// <summary>
    /// 3D version of settings manager for menu panel
    /// Uses 3D text and interactive sliders
    /// </summary>
    public class SettingsManager3D : MonoBehaviour
    {
        [Header("UI Elements (3D)")]
        public TextMeshPro titleText;
        public TextMeshPro masterVolumeText;
        public TextMeshPro musicVolumeText;
        public TextMeshPro sfxVolumeText;
        
        [Header("Slider Visuals")]
        public GameObject masterSliderBar;
        public GameObject musicSliderBar;
        public GameObject sfxSliderBar;
        
        // PlayerPrefs keys
        private const string MASTER_VOLUME_KEY = "MasterVolume";
        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string SFX_VOLUME_KEY = "SFXVolume";
        
        private float masterVolume = 1f;
        private float musicVolume = 0.7f;
        private float sfxVolume = 0.8f;
        
        void Start()
        {
            SetupUI();
            LoadSettings();
            UpdateUI();
        }
        
        void SetupUI()
        {
            // Create title
            if (titleText == null)
            {
                GameObject titleObj = new GameObject("Title");
                titleObj.transform.parent = transform;
                titleObj.transform.localPosition = new Vector3(0f, 5f, 0f);
                titleText = titleObj.AddComponent<TextMeshPro>();
                titleText.text = "SETTINGS";
                titleText.fontSize = 8; // TĂNG từ 4 lên 8
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.color = new Color(0f, 0.94f, 1f);
                titleText.fontStyle = TMPro.FontStyles.Bold;
            }
            
            // Create volume labels and sliders - TĂNG KHOẢNG CÁCH
            CreateVolumeControl("MASTER VOLUME", ref masterVolumeText, ref masterSliderBar, new Vector3(0f, 2.5f, 0f));
            CreateVolumeControl("MUSIC VOLUME", ref musicVolumeText, ref musicSliderBar, new Vector3(0f, 0.5f, 0f));
            CreateVolumeControl("SFX VOLUME", ref sfxVolumeText, ref sfxSliderBar, new Vector3(0f, -1.5f, 0f));
            
            // Create instruction text
            GameObject instrObj = new GameObject("Instructions");
            instrObj.transform.parent = transform;
            instrObj.transform.localPosition = new Vector3(0f, -4f, 0f);
            TextMeshPro instrText = instrObj.AddComponent<TextMeshPro>();
            instrText.text = "Use +/- keys to adjust volume\n1-2-3 to switch between controls";
            instrText.fontSize = 2f; // TĂNG từ 1.2f lên 2f
            instrText.alignment = TextAlignmentOptions.Center;
            instrText.color = new Color(0.6f, 0.6f, 0.6f);
            instrText.rectTransform.sizeDelta = new Vector2(12f, 4f);
        }
        
        void CreateVolumeControl(string label, ref TextMeshPro text, ref GameObject sliderBar, Vector3 position)
        {
            // Create label
            GameObject labelObj = new GameObject($"{label}_Label");
            labelObj.transform.parent = transform;
            labelObj.transform.localPosition = position + new Vector3(-4f, 0f, 0f);
            TextMeshPro labelText = labelObj.AddComponent<TextMeshPro>();
            labelText.text = label;
            labelText.fontSize = 2.5f; // TĂNG từ 1.5f lên 2.5f
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.color = Color.white;
            labelText.fontStyle = TMPro.FontStyles.Bold;
            
            // Create value text
            GameObject valueObj = new GameObject($"{label}_Value");
            valueObj.transform.parent = transform;
            valueObj.transform.localPosition = position + new Vector3(4.5f, 0f, 0f); // DI CHUYỂN RA XA HƠN
            text = valueObj.AddComponent<TextMeshPro>();
            text.text = "100%";
            text.fontSize = 2.5f; // TĂNG từ 1.5f lên 2.5f
            text.alignment = TextAlignmentOptions.Right;
            text.color = new Color(0f, 1f, 0.5f);
            text.fontStyle = TMPro.FontStyles.Bold;
            
            // Create slider bar background
            GameObject barBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barBg.name = $"{label}_BarBg";
            barBg.transform.parent = transform;
            barBg.transform.localPosition = position + new Vector3(0f, -0.4f, 0f);
            barBg.transform.localScale = new Vector3(4f, 0.2f, 0.1f);
            
            MeshRenderer bgRenderer = barBg.GetComponent<MeshRenderer>();
            Material bgMat = new Material(Shader.Find("Standard"));
            bgMat.color = new Color(0.2f, 0.2f, 0.3f);
            bgRenderer.material = bgMat;
            Destroy(barBg.GetComponent<Collider>());
            
            // Create slider bar fill
            sliderBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sliderBar.name = $"{label}_BarFill";
            sliderBar.transform.parent = transform;
            sliderBar.transform.localPosition = position + new Vector3(0f, -0.4f, -0.05f);
            sliderBar.transform.localScale = new Vector3(4f, 0.2f, 0.1f);
            
            MeshRenderer fillRenderer = sliderBar.GetComponent<MeshRenderer>();
            Material fillMat = new Material(Shader.Find("Standard"));
            fillMat.EnableKeyword("_EMISSION");
            fillMat.color = new Color(0f, 0.94f, 1f);
            fillMat.SetColor("_EmissionColor", new Color(0f, 0.94f, 1f) * 0.5f);
            fillRenderer.material = fillMat;
            Destroy(sliderBar.GetComponent<Collider>());
        }
        
        void Update()
        {
            HandleInput();
        }
        
        void HandleInput()
        {
            float delta = 0f;
            
            // Volume adjustment
            if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
                delta = 0.01f;
            else if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.Underscore) || Input.GetKey(KeyCode.KeypadMinus))
                delta = -0.01f;
            
            if (delta != 0f)
            {
                // Quick switch with number keys
                if (Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Keypad1))
                {
                    masterVolume = Mathf.Clamp01(masterVolume + delta);
                    SaveSettings();
                }
                else if (Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Keypad2))
                {
                    musicVolume = Mathf.Clamp01(musicVolume + delta);
                    SaveSettings();
                }
                else if (Input.GetKey(KeyCode.Alpha3) || Input.GetKey(KeyCode.Keypad3))
                {
                    sfxVolume = Mathf.Clamp01(sfxVolume + delta);
                    SaveSettings();
                }
                else
                {
                    // Default: adjust master volume
                    masterVolume = Mathf.Clamp01(masterVolume + delta);
                    SaveSettings();
                }
                
                UpdateUI();
            }
        }
        
        void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
            sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);
            
            // Apply to AudioListener
            AudioListener.volume = masterVolume;
            
            Debug.Log($"[SettingsManager3D] Loaded settings - Master: {masterVolume:P0}, Music: {musicVolume:P0}, SFX: {sfxVolume:P0}");
        }
        
        void SaveSettings()
        {
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
            PlayerPrefs.Save();
            
            // Apply to AudioListener
            AudioListener.volume = masterVolume;
        }
        
        void UpdateUI()
        {
            // Update text
            if (masterVolumeText != null)
                masterVolumeText.text = $"{masterVolume:P0}";
            
            if (musicVolumeText != null)
                musicVolumeText.text = $"{musicVolume:P0}";
            
            if (sfxVolumeText != null)
                sfxVolumeText.text = $"{sfxVolume:P0}";
            
            // Update slider bars
            if (masterSliderBar != null)
            {
                Vector3 scale = masterSliderBar.transform.localScale;
                scale.x = 4f * masterVolume;
                masterSliderBar.transform.localScale = scale;
                
                Vector3 pos = masterSliderBar.transform.localPosition;
                pos.x = -2f + (2f * masterVolume);
                masterSliderBar.transform.localPosition = pos;
            }
            
            if (musicSliderBar != null)
            {
                Vector3 scale = musicSliderBar.transform.localScale;
                scale.x = 4f * musicVolume;
                musicSliderBar.transform.localScale = scale;
                
                Vector3 pos = musicSliderBar.transform.localPosition;
                pos.x = -2f + (2f * musicVolume);
                musicSliderBar.transform.localPosition = pos;
            }
            
            if (sfxSliderBar != null)
            {
                Vector3 scale = sfxSliderBar.transform.localScale;
                scale.x = 4f * sfxVolume;
                sfxSliderBar.transform.localScale = scale;
                
                Vector3 pos = sfxSliderBar.transform.localPosition;
                pos.x = -2f + (2f * sfxVolume);
                sfxSliderBar.transform.localPosition = pos;
            }
        }
        
        /// <summary>
        /// Get current volumes for other systems
        /// </summary>
        public float GetMasterVolume() => masterVolume;
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;
    }
}
