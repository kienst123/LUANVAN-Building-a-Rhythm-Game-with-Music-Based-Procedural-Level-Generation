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
        
        // Slider backgrounds for mouse detection
        private GameObject masterSliderBg;
        private GameObject musicSliderBg;
        private GameObject sfxSliderBg;
        
        // Mouse interaction
        private bool isDragging = false;
        private GameObject activeSlider = null;
        
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
                titleObj.transform.localPosition = new Vector3(0f, 3f, 0f); // Hạ xuống từ 5f
                titleText = titleObj.AddComponent<TextMeshPro>();
                titleText.text = "SETTINGS";
                titleText.fontSize = 12; // TĂNG từ 8 lên 12
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.color = new Color(0f, 0.94f, 1f);
                titleText.fontStyle = TMPro.FontStyles.Bold;
            }
            
            // Create volume labels and sliders - KÉO GẦN NHAU HƠN
            CreateVolumeControl("MASTER VOLUME", ref masterVolumeText, ref masterSliderBar, ref masterSliderBg, new Vector3(0f, 1.2f, 0f));
            CreateVolumeControl("MUSIC VOLUME", ref musicVolumeText, ref musicSliderBar, ref musicSliderBg, new Vector3(0f, -0.5f, 0f));
            CreateVolumeControl("SFX VOLUME", ref sfxVolumeText, ref sfxSliderBar, ref sfxSliderBg, new Vector3(0f, -2.2f, 0f));
        }
        
        void CreateVolumeControl(string label, ref TextMeshPro text, ref GameObject sliderBar, ref GameObject sliderBg, Vector3 position)
        {
            // Create value text (BÊN TRÁI - SÁT ĐẦU TRÁI SLIDER)
            GameObject valueObj = new GameObject($"{label}_Value");
            valueObj.transform.parent = transform;
            valueObj.transform.localPosition = position + new Vector3(-3.2f, -0.5f, 0f); // Cùng độ cao với slider (-0.5f)
            text = valueObj.AddComponent<TextMeshPro>();
            text.text = "100%";
            text.fontSize = 3.5f;
            text.alignment = TextAlignmentOptions.Right; // Căn phải để sát slider
            text.color = new Color(0f, 1f, 0.5f);
            text.fontStyle = TMPro.FontStyles.Bold;
            
            // Create label (BÊN PHẢI - SÁT ĐẦU PHẢI SLIDER)
            GameObject labelObj = new GameObject($"{label}_Label");
            labelObj.transform.parent = transform;
            labelObj.transform.localPosition = position + new Vector3(3.2f, -0.5f, 0f); // Cùng độ cao với slider (-0.5f)
            TextMeshPro labelText = labelObj.AddComponent<TextMeshPro>();
            labelText.text = label;
            labelText.fontSize = 3.5f;
            labelText.alignment = TextAlignmentOptions.Left; // Căn trái để sát slider
            labelText.color = Color.white;
            labelText.fontStyle = TMPro.FontStyles.Bold;
            
            // Create slider bar background (KEEP COLLIDER for mouse detection)
            sliderBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sliderBg.name = $"{label}_BarBg";
            sliderBg.transform.parent = transform;
            sliderBg.transform.localPosition = position + new Vector3(0f, -0.5f, 0f); // Hạ xuống 1 chút
            sliderBg.transform.localScale = new Vector3(6f, 0.3f, 0.1f); // TĂNG từ 4f lên 6f width, 0.2f lên 0.3f height
            
            MeshRenderer bgRenderer = sliderBg.GetComponent<MeshRenderer>();
            Material bgMat = new Material(Shader.Find("Standard"));
            bgMat.color = new Color(0.2f, 0.2f, 0.3f);
            bgRenderer.material = bgMat;
            // Keep collider for mouse interaction!
            
            // Create slider bar fill
            sliderBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sliderBar.name = $"{label}_BarFill";
            sliderBar.transform.parent = transform;
            sliderBar.transform.localPosition = position + new Vector3(0f, -0.5f, -0.05f); // Match background position
            sliderBar.transform.localScale = new Vector3(6f, 0.3f, 0.1f); // TĂNG từ 4f lên 6f width, 0.2f lên 0.3f height
            
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
            HandleMouseInput();
            HandleKeyboardInput();
        }
        
        void HandleMouseInput()
        {
            // Mouse click detection
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, 100f))
                {
                    // Check which slider was clicked
                    if (hit.collider.gameObject == masterSliderBg)
                    {
                        isDragging = true;
                        activeSlider = masterSliderBg;
                        UpdateVolumeFromMouse(hit.point, ref masterVolume);
                    }
                    else if (hit.collider.gameObject == musicSliderBg)
                    {
                        isDragging = true;
                        activeSlider = musicSliderBg;
                        UpdateVolumeFromMouse(hit.point, ref musicVolume);
                    }
                    else if (hit.collider.gameObject == sfxSliderBg)
                    {
                        isDragging = true;
                        activeSlider = sfxSliderBg;
                        UpdateVolumeFromMouse(hit.point, ref sfxVolume);
                    }
                }
            }
            
            // Mouse drag
            if (Input.GetMouseButton(0) && isDragging && activeSlider != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, 100f))
                {
                    if (activeSlider == masterSliderBg)
                        UpdateVolumeFromMouse(hit.point, ref masterVolume);
                    else if (activeSlider == musicSliderBg)
                        UpdateVolumeFromMouse(hit.point, ref musicVolume);
                    else if (activeSlider == sfxSliderBg)
                        UpdateVolumeFromMouse(hit.point, ref sfxVolume);
                }
            }
            
            // Mouse release
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                activeSlider = null;
            }
        }
        
        void UpdateVolumeFromMouse(Vector3 worldPoint, ref float volume)
        {
            // Convert world position to local position
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            
            // Slider is 6 units wide, centered at x=0, so range is [-3, 3]
            float normalizedX = (localPoint.x + 3f) / 6f;
            volume = Mathf.Clamp01(normalizedX);
            
            SaveSettings();
            UpdateUI();
        }
        
        void HandleKeyboardInput()
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
            
            // Update slider bars (6 units wide now)
            if (masterSliderBar != null)
            {
                Vector3 scale = masterSliderBar.transform.localScale;
                scale.x = 6f * masterVolume;
                masterSliderBar.transform.localScale = scale;
                
                Vector3 pos = masterSliderBar.transform.localPosition;
                pos.x = -3f + (3f * masterVolume);
                masterSliderBar.transform.localPosition = pos;
            }
            
            if (musicSliderBar != null)
            {
                Vector3 scale = musicSliderBar.transform.localScale;
                scale.x = 6f * musicVolume;
                musicSliderBar.transform.localScale = scale;
                
                Vector3 pos = musicSliderBar.transform.localPosition;
                pos.x = -3f + (3f * musicVolume);
                musicSliderBar.transform.localPosition = pos;
            }
            
            if (sfxSliderBar != null)
            {
                Vector3 scale = sfxSliderBar.transform.localScale;
                scale.x = 6f * sfxVolume;
                sfxSliderBar.transform.localScale = scale;
                
                Vector3 pos = sfxSliderBar.transform.localPosition;
                pos.x = -3f + (3f * sfxVolume);
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
