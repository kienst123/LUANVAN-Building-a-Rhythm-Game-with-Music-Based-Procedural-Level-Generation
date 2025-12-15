using UnityEngine;
using UnityEngine.SceneManagement;
using RhythmGame3D.UI;

namespace RhythmGame3D.UI.Menu3D
{
    /// <summary>
    /// Main manager for 3D menu scene
    /// Sets up background, buttons, camera, and visualizer
    /// </summary>
    public class MainMenu3DManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera menuCamera;
        [SerializeField] private AudioSource menuMusic;
        
        [Header("Prefabs (Optional - will be created if null)")]
        [SerializeField] private GameObject buttonPrefab;
        
        [Header("Settings")]
        [SerializeField] private string gameplaySceneName = "GameScene";
        [SerializeField] private AudioClip menuMusicClip;
        
        [Header("UI Panels (will be created if not assigned)")]
        [SerializeField] private GameObject beatmapPanel3D;
        [SerializeField] private GameObject settingsPanel3D;
        
        private NeonBackgroundManager backgroundManager;
        private MenuCamera3D cameraController;
        private AudioVisualizer3D audioVisualizer;
        private MenuButton3D[] menuButtons;
        
        void Start()
        {
            SetupCamera();
            SetupBackground();
            SetupMenuButtons();
            SetupAudioVisualizer();
            SetupMenuMusic();
            SetupPanels();
            
            // FORCE ẨN PANELS SAU KHI SETUP XONG (delay 1 frame)
            StartCoroutine(HidePanelsNextFrame());
        }
        
        System.Collections.IEnumerator HidePanelsNextFrame()
        {
            yield return null; // Đợi 1 frame
            
            // Tìm và ẩn TẤT CẢ panels
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Panel3D") || obj.name.Contains("BeatmapPanel") || obj.name.Contains("SettingsPanel"))
                {
                    obj.SetActive(false);
                    Debug.Log($"[MainMenu3D] FORCE HIDE: {obj.name}");
                }
            }
            
            if (beatmapPanel3D != null)
            {
                beatmapPanel3D.SetActive(false);
                Debug.Log("[MainMenu3D] FORCE HIDE BeatmapPanel3D");
            }
            if (settingsPanel3D != null)
            {
                settingsPanel3D.SetActive(false);
                Debug.Log("[MainMenu3D] FORCE HIDE SettingsPanel3D");
            }
        }
        
        /// <summary>
        /// Setup 3D panels for beatmap and settings
        /// </summary>
        void SetupPanels()
        {
            // Find and hide any existing panels first
            GameObject existingBeatmap = GameObject.Find("BeatmapPanel3D");
            if (existingBeatmap != null)
            {
                Destroy(existingBeatmap);
                Debug.Log("[MainMenu3D] Destroyed existing BeatmapPanel3D");
            }
            
            GameObject existingSettings = GameObject.Find("SettingsPanel3D");
            if (existingSettings != null)
            {
                Destroy(existingSettings);
                Debug.Log("[MainMenu3D] Destroyed existing SettingsPanel3D");
            }
            
            // Create beatmap panel if not assigned (closer and larger)
            if (beatmapPanel3D == null)
            {
                beatmapPanel3D = CreateBeatmapPanel3D(new Vector3(0f, 0f, 8f));
                beatmapPanel3D.SetActive(false);
            }
            else
            {
                beatmapPanel3D.SetActive(false); // Force hide
            }
            
            // Create settings panel if not assigned (closer and larger)
            if (settingsPanel3D == null)
            {
                settingsPanel3D = CreateSettingsPanel3D(new Vector3(0f, 0f, 8f));
                settingsPanel3D.SetActive(false);
            }
            else
            {
                settingsPanel3D.SetActive(false); // Force hide
            }
            
            Debug.Log("[MainMenu3D] Panels setup complete - all hidden");
        }
        
        /// <summary>
        /// Create beatmap selector panel with functional UI
        /// </summary>
        GameObject CreateBeatmapPanel3D(Vector3 position)
        {
            GameObject panel = new GameObject("BeatmapPanel3D");
            panel.transform.position = position;
            
            // Add BeatmapSelector3D component
            BeatmapSelector3D selector = panel.AddComponent<BeatmapSelector3D>();
            selector.menuManager = this;
            
            // Add back button (DƯỚI CÙNG)
            GameObject backButton = CreateMenuButton("BACK", Vector3.zero);
            backButton.transform.parent = panel.transform;
            backButton.transform.localPosition = new Vector3(0f, -8f, 0f); // Dời xuống từ -5f → -8f
            backButton.transform.localScale = Vector3.one * 1.0f;
            
            MenuButton3D backScript = backButton.GetComponent<MenuButton3D>();
            backScript.AddClickListener(() => {
                panel.SetActive(false);
                ShowMainMenu();
            });
            
            return panel;
        }
        
        /// <summary>
        /// Create settings panel with functional UI
        /// </summary>
        GameObject CreateSettingsPanel3D(Vector3 position)
        {
            GameObject panel = new GameObject("SettingsPanel3D");
            panel.transform.position = position;
            
            // Add SettingsManager3D component
            SettingsManager3D settings = panel.AddComponent<SettingsManager3D>();
            
            // Add back button (DƯỚI CÙNG)
            GameObject backButton = CreateMenuButton("BACK", Vector3.zero);
            backButton.transform.parent = panel.transform;
            backButton.transform.localPosition = new Vector3(0f, -5.5f, 0f); // Dời xuống từ -5f → -5.5f (Settings gần hơn)
            backButton.transform.localScale = Vector3.one * 1.0f;
            
            MenuButton3D backScript = backButton.GetComponent<MenuButton3D>();
            backScript.AddClickListener(() => {
                panel.SetActive(false);
                ShowMainMenu();
            });
            
            return panel;
        }
        
        /// <summary>
        /// Create a simple 3D panel
        /// </summary>
        GameObject CreatePanel3D(string name, Vector3 position)
        {
            GameObject panel = new GameObject(name);
            panel.transform.position = position;
            
            // Create background panel (LARGER for better visibility)
            GameObject bgPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bgPanel.name = "Background";
            bgPanel.transform.parent = panel.transform;
            bgPanel.transform.localPosition = Vector3.zero;
            bgPanel.transform.localScale = new Vector3(0.2f, 12f, 9f); // Bigger!
            
            // ẨN BACKGROUND PANEL ĐI!
            bgPanel.SetActive(false);
            
            // Setup material with transparency (giữ lại code cho sau nếu cần)
            MeshRenderer renderer = bgPanel.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3); // Transparent mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            
            mat.color = new Color(0.05f, 0.05f, 0.2f, 0.5f); // Darker, more visible
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0f, 0.5f, 0.8f) * 0.5f);
            renderer.material = mat;
            
            // Remove collider
            Destroy(bgPanel.GetComponent<Collider>());
            
            // Add title text (LARGER font)
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.parent = panel.transform;
            titleObj.transform.localPosition = new Vector3(-0.5f, 4.5f, 0f);
            titleObj.transform.localRotation = Quaternion.identity;
            
            TMPro.TextMeshPro title = titleObj.AddComponent<TMPro.TextMeshPro>();
            title.text = name.Replace("Panel3D", "").ToUpper();
            title.fontSize = 6; // Bigger font!
            title.alignment = TMPro.TextAlignmentOptions.Center;
            title.color = new Color(0f, 0.94f, 1f);
            title.fontStyle = TMPro.FontStyles.Bold;
            
            // Add info text (LARGER font)
            GameObject infoObj = new GameObject("InfoText");
            infoObj.transform.parent = panel.transform;
            infoObj.transform.localPosition = new Vector3(-0.5f, 0f, 0f);
            infoObj.transform.localRotation = Quaternion.identity;
            
            TMPro.TextMeshPro info = infoObj.AddComponent<TMPro.TextMeshPro>();
            info.text = "This panel is under construction.\n\nClick BACK to return to main menu.";
            info.fontSize = 2.5f; // Bigger font!
            info.alignment = TMPro.TextAlignmentOptions.Center;
            info.color = Color.white;
            info.rectTransform.sizeDelta = new Vector2(8f, 4f);
            
            // Add back button (LARGER)
            GameObject backButton = CreateMenuButton("BACK", Vector3.zero);
            backButton.transform.parent = panel.transform;
            backButton.transform.localPosition = new Vector3(-0.5f, -4f, 0f);
            backButton.transform.localScale = Vector3.one * 0.7f; // Bigger button!
            
            MenuButton3D backScript = backButton.GetComponent<MenuButton3D>();
            backScript.AddClickListener(() => {
                panel.SetActive(false);
                ShowMainMenu();
            });
            
            return panel;
        }
        
        /// <summary>
        /// Setup main camera with controller
        /// </summary>
        void SetupCamera()
        {
            if (menuCamera == null)
            {
                menuCamera = Camera.main;
                
                if (menuCamera == null)
                {
                    GameObject camObj = new GameObject("MainCamera");
                    menuCamera = camObj.AddComponent<Camera>();
                    camObj.tag = "MainCamera";
                }
            }
            
            // Add camera controller
            cameraController = menuCamera.gameObject.GetComponent<MenuCamera3D>();
            if (cameraController == null)
            {
                cameraController = menuCamera.gameObject.AddComponent<MenuCamera3D>();
            }
            
            // Setup camera settings
            menuCamera.backgroundColor = new Color(0.04f, 0.04f, 0.12f, 1f); // Dark blue
            menuCamera.clearFlags = CameraClearFlags.SolidColor;
            
            Debug.Log("[MainMenu3D] Camera setup complete");
        }
        
        /// <summary>
        /// Setup 3D neon background
        /// </summary>
        void SetupBackground()
        {
            GameObject bgObj = new GameObject("NeonBackground");
            backgroundManager = bgObj.AddComponent<NeonBackgroundManager>();
            
            Debug.Log("[MainMenu3D] Background setup complete");
        }
        
        /// <summary>
        /// Setup 3D menu buttons
        /// </summary>
        void SetupMenuButtons()
        {
            string[] buttonNames = { "PLAY", "BEATMAP", "SETTINGS", "QUIT" };
            
            // Center the buttons vertically
            Vector3[] buttonPositions = {
                new Vector3(0f, 3f, 5f),   // PLAY - top
                new Vector3(0f, 1f, 5f),   // BEATMAP
                new Vector3(0f, -1f, 5f),  // SETTINGS
                new Vector3(0f, -3f, 5f)   // QUIT - bottom
            };
            
            menuButtons = new MenuButton3D[buttonNames.Length];
            
            for (int i = 0; i < buttonNames.Length; i++)
            {
                GameObject buttonObj = CreateMenuButton(buttonNames[i], buttonPositions[i]);
                MenuButton3D button = buttonObj.GetComponent<MenuButton3D>();
                menuButtons[i] = button;
                
                // Add click listeners
                int index = i; // Capture for lambda
                switch (index)
                {
                    case 0: // PLAY
                        button.AddClickListener(OnPlayClicked);
                        break;
                    case 1: // BEATMAP
                        button.AddClickListener(OnBeatmapClicked);
                        break;
                    case 2: // SETTINGS
                        button.AddClickListener(OnSettingsClicked);
                        break;
                    case 3: // QUIT
                        button.AddClickListener(OnQuitClicked);
                        break;
                }
            }
            
            Debug.Log("[MainMenu3D] Created 4 menu buttons");
        }
        
        /// <summary>
        /// Create a single 3D menu button
        /// </summary>
        GameObject CreateMenuButton(string text, Vector3 position)
        {
            GameObject buttonObj = new GameObject($"Button_{text}");
            buttonObj.transform.position = position;
            
            // Add BoxCollider FIRST (before MenuButton3D for proper initialization)
            BoxCollider collider = buttonObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(4f, 0.8f, 0.3f);
            
            // Create button mesh (rounded cube)
            GameObject buttonMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buttonMesh.name = "ButtonMesh";
            buttonMesh.transform.parent = buttonObj.transform;
            buttonMesh.transform.localPosition = Vector3.zero;
            buttonMesh.transform.localScale = new Vector3(4f, 0.8f, 0.3f);
            
            // Remove the cube's collider (we already have one on parent)
            Destroy(buttonMesh.GetComponent<BoxCollider>());
            
            // Setup material
            MeshRenderer renderer = buttonMesh.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.EnableKeyword("_EMISSION");
            mat.color = new Color(0.1f, 0.1f, 0.3f, 0.8f);
            renderer.material = mat;
            
            // Create text
            GameObject textObj = new GameObject("ButtonText");
            textObj.transform.parent = buttonObj.transform;
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.2f);
            
            TMPro.TextMeshPro textMesh = textObj.AddComponent<TMPro.TextMeshPro>();
            textMesh.text = text;
            textMesh.fontSize = 2;
            textMesh.alignment = TMPro.TextAlignmentOptions.Center;
            textMesh.color = Color.white;
            
            // Create glow light
            GameObject lightObj = new GameObject("GlowLight");
            lightObj.transform.parent = buttonObj.transform;
            lightObj.transform.localPosition = Vector3.zero;
            
            Light glowLight = lightObj.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.range = 5f;
            glowLight.intensity = 0.5f;
            glowLight.color = new Color(0f, 0.94f, 1f, 1f);
            
            // Add MenuButton3D component and assign references IMMEDIATELY
            MenuButton3D buttonScript = buttonObj.AddComponent<MenuButton3D>();
            buttonScript.buttonRenderer = renderer;
            buttonScript.buttonText = textMesh;
            buttonScript.glowLight = glowLight;
            
            Debug.Log($"[MainMenu3D] Created button '{text}' with all references assigned");
            
            return buttonObj;
        }
        
        /// <summary>
        /// Setup audio visualizer
        /// </summary>
        void SetupAudioVisualizer()
        {
            GameObject vizObj = new GameObject("AudioVisualizer");
            vizObj.transform.position = new Vector3(5f, 0f, 10f);
            
            audioVisualizer = vizObj.AddComponent<AudioVisualizer3D>();
            
            // Will auto-find audio source
            
            Debug.Log("[MainMenu3D] Audio visualizer setup complete");
        }
        
        /// <summary>
        /// Setup menu background music
        /// </summary>
        void SetupMenuMusic()
        {
            if (menuMusic == null)
            {
                GameObject musicObj = new GameObject("MenuMusic");
                menuMusic = musicObj.AddComponent<AudioSource>();
            }
            
            menuMusic.loop = true;
            menuMusic.playOnAwake = false;
            menuMusic.volume = 0.3f;
            
            if (menuMusicClip != null)
            {
                menuMusic.clip = menuMusicClip;
                menuMusic.Play();
            }
            
            Debug.Log("[MainMenu3D] Menu music setup complete");
        }
        
        /// <summary>
        /// Button click handlers
        /// </summary>
        void OnPlayClicked()
        {
            // Check if BeatmapSelector exists
            var beatmapSelector = FindObjectOfType<BeatmapSelector>();
            
            // Check if beatmap is selected
            if (beatmapSelector == null || BeatmapSelector.currentBeatmap == null || string.IsNullOrEmpty(BeatmapSelector.currentMusicPath))
            {
                Debug.LogWarning("[MainMenu3D] No beatmap selected! Please implement beatmap selection first.");
                // For now, just load the scene anyway for testing
                // TODO: Show 3D notification panel
            }
            
            Debug.Log("[MainMenu3D] Loading gameplay scene...");
            SceneManager.LoadScene(gameplaySceneName);
        }
        
        void OnBeatmapClicked()
        {
            Debug.Log("[MainMenu3D] Opening beatmap selection...");
            
            // Hide main menu buttons
            HideMainMenu();
            
            // Show beatmap panel
            if (beatmapPanel3D != null)
            {
                beatmapPanel3D.SetActive(true);
                
                // Move camera CLOSER to panel for better view
                if (cameraController != null)
                {
                    cameraController.MoveTo(new Vector3(0f, 0f, -8f), new Vector3(0f, 0f, 0f));
                }
            }
            
            // Try to find and show old BeatmapSelector if exists
            var beatmapSelector = FindObjectOfType<BeatmapSelector>();
            if (beatmapSelector != null)
            {
                beatmapSelector.gameObject.SetActive(true);
            }
        }
        
        void OnSettingsClicked()
        {
            Debug.Log("[MainMenu3D] Opening settings...");
            
            // Hide main menu buttons
            HideMainMenu();
            
            // Show settings panel
            if (settingsPanel3D != null)
            {
                settingsPanel3D.SetActive(true);
                
                // Move camera CLOSER to panel for better view
                if (cameraController != null)
                {
                    cameraController.MoveTo(new Vector3(0f, 0f, -8f), new Vector3(0f, 0f, 0f));
                }
            }
            
            // Try to find and show old SettingsManager if exists
            var settingsManager = FindObjectOfType<SettingsManager>();
            if (settingsManager != null)
            {
                settingsManager.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// Hide main menu buttons
        /// </summary>
        void HideMainMenu()
        {
            if (menuButtons != null)
            {
                foreach (var button in menuButtons)
                {
                    if (button != null)
                    {
                        button.gameObject.SetActive(false);
                    }
                }
            }
        }
        
        /// <summary>
        /// Show main menu buttons
        /// </summary>
        void ShowMainMenu()
        {
            if (menuButtons != null)
            {
                foreach (var button in menuButtons)
                {
                    if (button != null)
                    {
                        button.gameObject.SetActive(true);
                    }
                }
            }
            
            // Reset camera
            if (cameraController != null)
            {
                cameraController.ResetToDefault();
            }
        }
        
        void OnQuitClicked()
        {
            Debug.Log("[MainMenu3D] Quitting application...");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
