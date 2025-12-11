using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using RhythmGame3D.UI;
using UnityEngine.SceneManagement;

namespace RhythmGame3D.Editor
{
    public class MainMenuSetup : EditorWindow
    {
        [MenuItem("RhythmGame/Create Main Menu Scene")]
        public static void CreateMainMenuScene()
        {
            if (!EditorUtility.DisplayDialog("Create Main Menu",
                "This will create a new Main Menu scene with:\n" +
                "- Gradient background (cyan)\n" +
                "- Title text\n" +
                "- Play, Beatmap, Settings buttons\n" +
                "- Settings panel with volume sliders\n" +
                "- Beatmap import panel\n\n" +
                "Continue?", "Yes", "Cancel"))
            {
                return;
            }

            Debug.Log("=== Creating Main Menu Scene ===");

            // Create new scene or clear current
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "MainMenu";

            // Create camera
            CreateCamera();

            // Create Canvas
            GameObject canvas = CreateCanvas();

            // Create Background
            CreateBackground(canvas);

            // Create Title
            CreateTitle(canvas);

            // Create Main Panel with buttons
            GameObject mainPanel = CreateMainPanel(canvas);

            // Create Beatmap Panel
            GameObject beatmapPanel = CreateBeatmapPanel(canvas);

            // Create Settings Panel
            GameObject settingsPanel = CreateSettingsPanel(canvas);

            // Create MenuManager
            GameObject menuManager = CreateMenuManager(canvas, mainPanel, beatmapPanel, settingsPanel);

            // Save scene
            string scenePath = "Assets/Scenes/MainMenu.unity";
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log("=== Main Menu Scene Created! ===");
            EditorUtility.DisplayDialog("Success!",
                $"Main Menu scene created at:\n{scenePath}\n\n" +
                "Next steps:\n" +
                "1. Add MainMenu scene to Build Settings\n" +
                "2. Set as first scene (index 0)\n" +
                "3. Add GameScene to Build Settings\n" +
                "4. Test menu navigation!", "OK");
        }

        static void CreateCamera()
        {
            GameObject cam = new GameObject("Main Camera");
            Camera camera = cam.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.2f, 0.6f, 0.8f);
            cam.AddComponent<AudioListener>();
            cam.tag = "MainCamera";
        }

        static GameObject CreateCanvas()
        {
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            canvasGO.AddComponent<GraphicRaycaster>();

            // Event System
            if (!GameObject.Find("EventSystem"))
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            return canvasGO;
        }

        static void CreateBackground(GameObject canvas)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvas.transform, false);

            RectTransform rt = bg.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = bg.AddComponent<Image>();
            // Gradient from light cyan to blue
            img.color = new Color(0.5f, 0.85f, 0.95f); // Light cyan

            // Add wave pattern (optional)
            GameObject wave = new GameObject("WavePattern");
            wave.transform.SetParent(bg.transform, false);
            RectTransform waveRT = wave.AddComponent<RectTransform>();
            waveRT.anchorMin = Vector2.zero;
            waveRT.anchorMax = Vector2.one;
            waveRT.sizeDelta = Vector2.zero;
            Image waveImg = wave.AddComponent<Image>();
            waveImg.color = new Color(1, 1, 1, 0.1f);
        }

        static void CreateTitle(GameObject canvas)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.75f);
            rt.anchorMax = new Vector2(0.5f, 0.75f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(800, 200);

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "GAME MENU\nMUSIC PACK";
            title.fontSize = 60;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = Color.white;

            // Outline
            title.outlineWidth = 0.3f;
            title.outlineColor = new Color(0, 0, 0, 0.5f);
        }

        static GameObject CreateMainPanel(GameObject canvas)
        {
            GameObject panel = new GameObject("MainPanel");
            panel.transform.SetParent(canvas.transform, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.4f);
            rt.anchorMax = new Vector2(0.5f, 0.4f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 350);

            // Play Button (Green/Cyan)
            CreateMenuButton(panel, "PlayButton", "PLAY", new Vector2(0, 100), new Color(0.3f, 0.95f, 0.7f));

            // Beatmap Button (Pink/Magenta)
            CreateMenuButton(panel, "BeatmapButton", "BEATMAP", new Vector2(0, 0), new Color(1f, 0.3f, 0.7f));

            // Settings Button (Purple)
            CreateMenuButton(panel, "SettingsButton", "SETTINGS", new Vector2(0, -100), new Color(0.7f, 0.3f, 0.95f));

            // Quit Button (small, at bottom)
            GameObject quitBtn = CreateMenuButton(panel, "QuitButton", "Quit", new Vector2(0, -170), new Color(0.5f, 0.5f, 0.5f));
            quitBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
            quitBtn.GetComponentInChildren<TextMeshProUGUI>().fontSize = 24;

            return panel;
        }

        static GameObject CreateMenuButton(GameObject parent, string name, string text, Vector2 position, Color color)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent.transform, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(350, 70);

            Image img = btnObj.AddComponent<Image>();
            img.color = color;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = colors;

            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = 36;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;

            return btnObj;
        }

        static GameObject CreateBeatmapPanel(GameObject canvas)
        {
            GameObject panel = new GameObject("BeatmapPanel");
            panel.transform.SetParent(canvas.transform, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Semi-transparent background
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

            // Content panel
            GameObject content = new GameObject("Content");
            content.transform.SetParent(panel.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.pivot = new Vector2(0.5f, 0.5f);
            contentRT.sizeDelta = new Vector2(600, 500);

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = new Color(0.2f, 0.2f, 0.3f);

            // Title
            CreatePanelTitle(content, "BEATMAP SELECTION");

            // Import button
            CreateMenuButton(content, "ImportButton", "IMPORT MUSIC", new Vector2(0, 100), new Color(0.3f, 0.7f, 1f));

            // Difficulty dropdown
            CreateDifficultyDropdown(content);

            // Selected file text
            CreateInfoText(content, "SelectedFileText", "No file selected", new Vector2(0, -50));

            // Info text
            CreateInfoText(content, "InfoText", "Click Import to select a music file", new Vector2(0, -120));

            // Back button
            CreateMenuButton(content, "BackButton", "BACK", new Vector2(0, -200), new Color(0.5f, 0.5f, 0.5f));

            panel.SetActive(false);
            return panel;
        }

        static GameObject CreateSettingsPanel(GameObject canvas)
        {
            GameObject panel = new GameObject("SettingsPanel");
            panel.transform.SetParent(canvas.transform, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Semi-transparent background
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

            // Content panel
            GameObject content = new GameObject("Content");
            content.transform.SetParent(panel.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.pivot = new Vector2(0.5f, 0.5f);
            contentRT.sizeDelta = new Vector2(600, 500);

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = new Color(0.2f, 0.2f, 0.3f);

            // Title
            CreatePanelTitle(content, "SETTINGS");

            // Volume sliders
            CreateVolumeSlider(content, "MasterVolumeSlider", "Master Volume", new Vector2(0, 100));
            CreateVolumeSlider(content, "MusicVolumeSlider", "Music Volume", new Vector2(0, 20));
            CreateVolumeSlider(content, "SFXVolumeSlider", "SFX Volume", new Vector2(0, -60));

            // Back button
            CreateMenuButton(content, "BackButton", "BACK", new Vector2(0, -180), new Color(0.5f, 0.5f, 0.5f));

            panel.SetActive(false);
            return panel;
        }

        static void CreatePanelTitle(GameObject parent, string text)
        {
            GameObject titleObj = new GameObject("PanelTitle");
            titleObj.transform.SetParent(parent.transform, false);

            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -20);
            rt.sizeDelta = new Vector2(0, 60);

            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = text;
            title.fontSize = 40;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = new Color(1f, 0.84f, 0f);
        }

        static void CreateDifficultyDropdown(GameObject parent)
        {
            GameObject dropdownObj = new GameObject("DifficultyDropdown");
            dropdownObj.transform.SetParent(parent.transform, false);

            RectTransform rt = dropdownObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 20);
            rt.sizeDelta = new Vector2(400, 50);

            Image img = dropdownObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.3f);

            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
            
            // Create Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(dropdownObj.transform, false);
            
            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.offsetMin = new Vector2(10, 6);
            labelRT.offsetMax = new Vector2(-25, -7);
            
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "Easy";
            labelText.fontSize = 24;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;
            
            dropdown.captionText = labelText;
            
            // Create Arrow
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(dropdownObj.transform, false);
            
            RectTransform arrowRT = arrowObj.AddComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.pivot = new Vector2(0.5f, 0.5f);
            arrowRT.anchoredPosition = new Vector2(-15, 0);
            arrowRT.sizeDelta = new Vector2(20, 20);
            
            TextMeshProUGUI arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
            arrowText.text = "▼";
            arrowText.fontSize = 20;
            arrowText.alignment = TextAlignmentOptions.Center;
            arrowText.color = Color.white;
            
            // Create Template
            GameObject templateObj = new GameObject("Template");
            templateObj.transform.SetParent(dropdownObj.transform, false);
            
            RectTransform templateRT = templateObj.AddComponent<RectTransform>();
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(1, 0);
            templateRT.pivot = new Vector2(0.5f, 1);
            templateRT.anchoredPosition = new Vector2(0, 2);
            templateRT.sizeDelta = new Vector2(0, 150);
            
            Image templateImg = templateObj.AddComponent<Image>();
            templateImg.color = new Color(0.15f, 0.15f, 0.2f);
            
            ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            
            // Viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(templateObj.transform, false);
            
            RectTransform viewportRT = viewportObj.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportRT.pivot = new Vector2(0, 1);
            
            Image viewportMask = viewportObj.AddComponent<Image>();
            viewportMask.color = Color.white;
            Mask mask = viewportObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            
            scrollRect.viewport = viewportRT;
            
            // Content
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            
            RectTransform contentRT = contentObj.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0, 50);
            
            scrollRect.content = contentRT;
            
            // Item
            GameObject itemObj = new GameObject("Item");
            itemObj.transform.SetParent(contentObj.transform, false);
            
            RectTransform itemRT = itemObj.AddComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(1, 0.5f);
            itemRT.pivot = new Vector2(0.5f, 0.5f);
            itemRT.sizeDelta = new Vector2(0, 50);
            
            Toggle itemToggle = itemObj.AddComponent<Toggle>();
            
            Image itemBg = itemObj.AddComponent<Image>();
            itemBg.color = new Color(0.2f, 0.2f, 0.3f);
            
            itemToggle.targetGraphic = itemBg;
            itemToggle.isOn = true;
            
            // Item Background
            GameObject itemBgObj = new GameObject("Item Background");
            itemBgObj.transform.SetParent(itemObj.transform, false);
            
            RectTransform itemBgRT = itemBgObj.AddComponent<RectTransform>();
            itemBgRT.anchorMin = Vector2.zero;
            itemBgRT.anchorMax = Vector2.one;
            itemBgRT.sizeDelta = Vector2.zero;
            
            Image itemBgImage = itemBgObj.AddComponent<Image>();
            itemBgImage.color = new Color(0.4f, 0.6f, 1f, 0.3f);
            
            // Item Label
            GameObject itemLabelObj = new GameObject("Item Label");
            itemLabelObj.transform.SetParent(itemObj.transform, false);
            
            RectTransform itemLabelRT = itemLabelObj.AddComponent<RectTransform>();
            itemLabelRT.anchorMin = Vector2.zero;
            itemLabelRT.anchorMax = Vector2.one;
            itemLabelRT.offsetMin = new Vector2(10, 2);
            itemLabelRT.offsetMax = new Vector2(-10, -2);
            
            TextMeshProUGUI itemLabel = itemLabelObj.AddComponent<TextMeshProUGUI>();
            itemLabel.text = "Easy";
            itemLabel.fontSize = 24;
            itemLabel.alignment = TextAlignmentOptions.Center;
            itemLabel.color = Color.white;
            
            dropdown.template = templateRT;
            dropdown.itemText = itemLabel;
            
            templateObj.SetActive(false);
        }

        static void CreateInfoText(GameObject parent, string name, string text, Vector2 position)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(500, 60);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.gray;
        }

        static void CreateVolumeSlider(GameObject parent, string name, string label, Vector2 position)
        {
            // Container
            GameObject container = new GameObject(name + "Container");
            container.transform.SetParent(parent.transform, false);

            RectTransform containerRT = container.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 0.5f);
            containerRT.anchorMax = new Vector2(0.5f, 0.5f);
            containerRT.pivot = new Vector2(0.5f, 0.5f);
            containerRT.anchoredPosition = position;
            containerRT.sizeDelta = new Vector2(500, 50);

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0.5f);
            labelRT.anchorMax = new Vector2(0, 0.5f);
            labelRT.pivot = new Vector2(0, 0.5f);
            labelRT.anchoredPosition = Vector2.zero;
            labelRT.sizeDelta = new Vector2(150, 30);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 20;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.color = Color.white;

            // Slider
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(container.transform, false);

            RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0.35f, 0.5f);
            sliderRT.anchorMax = new Vector2(0.85f, 0.5f);
            sliderRT.pivot = new Vector2(0.5f, 0.5f);
            sliderRT.sizeDelta = new Vector2(0, 20);

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.sizeDelta = Vector2.zero;

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRT = fill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.7f, 1f);

            slider.fillRect = fillRT;

            // Handle Slide Area
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRT = handleArea.AddComponent<RectTransform>();
            handleAreaRT.anchorMin = Vector2.zero;
            handleAreaRT.anchorMax = Vector2.one;
            handleAreaRT.sizeDelta = Vector2.zero;

            // Handle
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform handleRT = handle.AddComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(20, 20);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;

            slider.handleRect = handleRT;

            // Value text
            GameObject valueObj = new GameObject("ValueText");
            valueObj.transform.SetParent(container.transform, false);

            RectTransform valueRT = valueObj.AddComponent<RectTransform>();
            valueRT.anchorMin = new Vector2(1, 0.5f);
            valueRT.anchorMax = new Vector2(1, 0.5f);
            valueRT.pivot = new Vector2(1, 0.5f);
            valueRT.anchoredPosition = Vector2.zero;
            valueRT.sizeDelta = new Vector2(60, 30);

            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = "100%";
            valueText.fontSize = 20;
            valueText.alignment = TextAlignmentOptions.Right;
            valueText.color = new Color(1f, 0.84f, 0f);
        }

        static GameObject CreateMenuManager(GameObject canvas, GameObject mainPanel, GameObject beatmapPanel, GameObject settingsPanel)
        {
            GameObject manager = new GameObject("MenuManager");

            MenuManager menuMgr = manager.AddComponent<MenuManager>();

            // Assign references
            SerializedObject so = new SerializedObject(menuMgr);
            so.FindProperty("mainPanel").objectReferenceValue = mainPanel;
            so.FindProperty("beatmapPanel").objectReferenceValue = beatmapPanel;
            so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            so.FindProperty("playButton").objectReferenceValue = mainPanel.transform.Find("PlayButton").GetComponent<Button>();
            so.FindProperty("beatmapButton").objectReferenceValue = mainPanel.transform.Find("BeatmapButton").GetComponent<Button>();
            so.FindProperty("settingsButton").objectReferenceValue = mainPanel.transform.Find("SettingsButton").GetComponent<Button>();
            so.FindProperty("quitButton").objectReferenceValue = mainPanel.transform.Find("QuitButton").GetComponent<Button>();
            so.ApplyModifiedProperties();

            // Add SettingsManager to settings panel
            GameObject settingsContent = settingsPanel.transform.Find("Content").gameObject;
            SettingsManager settingsMgr = settingsContent.AddComponent<SettingsManager>();

            SerializedObject settingsSO = new SerializedObject(settingsMgr);
            settingsSO.FindProperty("masterVolumeSlider").objectReferenceValue = settingsContent.transform.Find("MasterVolumeSliderContainer/MasterVolumeSlider").GetComponent<Slider>();
            settingsSO.FindProperty("musicVolumeSlider").objectReferenceValue = settingsContent.transform.Find("MusicVolumeSliderContainer/MusicVolumeSlider").GetComponent<Slider>();
            settingsSO.FindProperty("sfxVolumeSlider").objectReferenceValue = settingsContent.transform.Find("SFXVolumeSliderContainer/SFXVolumeSlider").GetComponent<Slider>();
            settingsSO.FindProperty("masterVolumeText").objectReferenceValue = settingsContent.transform.Find("MasterVolumeSliderContainer/ValueText").GetComponent<TextMeshProUGUI>();
            settingsSO.FindProperty("musicVolumeText").objectReferenceValue = settingsContent.transform.Find("MusicVolumeSliderContainer/ValueText").GetComponent<TextMeshProUGUI>();
            settingsSO.FindProperty("sfxVolumeText").objectReferenceValue = settingsContent.transform.Find("SFXVolumeSliderContainer/ValueText").GetComponent<TextMeshProUGUI>();
            settingsSO.FindProperty("backButton").objectReferenceValue = settingsContent.transform.Find("BackButton").GetComponent<Button>();
            settingsSO.FindProperty("menuManager").objectReferenceValue = menuMgr;
            settingsSO.ApplyModifiedProperties();

            // Add BeatmapSelector to beatmap panel
            GameObject beatmapContent = beatmapPanel.transform.Find("Content").gameObject;
            BeatmapSelector beatmapSel = beatmapContent.AddComponent<BeatmapSelector>();

            SerializedObject beatmapSO = new SerializedObject(beatmapSel);
            beatmapSO.FindProperty("importButton").objectReferenceValue = beatmapContent.transform.Find("ImportButton").GetComponent<Button>();
            beatmapSO.FindProperty("backButton").objectReferenceValue = beatmapContent.transform.Find("BackButton").GetComponent<Button>();
            beatmapSO.FindProperty("difficultyDropdown").objectReferenceValue = beatmapContent.transform.Find("DifficultyDropdown").GetComponent<TMP_Dropdown>();
            beatmapSO.FindProperty("selectedFileText").objectReferenceValue = beatmapContent.transform.Find("SelectedFileText").GetComponent<TextMeshProUGUI>();
            beatmapSO.FindProperty("infoText").objectReferenceValue = beatmapContent.transform.Find("InfoText").GetComponent<TextMeshProUGUI>();
            beatmapSO.FindProperty("menuManager").objectReferenceValue = menuMgr;
            beatmapSO.ApplyModifiedProperties();

            return manager;
        }
    }
}
