using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using RhythmGame3D.Core;
using RhythmGame3D.Gameplay;
using RhythmGame3D.UI;

namespace RhythmGame3D.Editor
{
    public class SceneSetup : EditorWindow
    {
        [MenuItem("RhythmGame/Auto Setup Scene")]
        public static void AutoSetupScene()
        {
            if (!EditorUtility.DisplayDialog("Auto Setup Scene",
                "This will create all GameObjects, UI, and assign all references automatically.\n\n" +
                "Make sure you have:\n" +
                "- TapNote and LongNote prefabs in Assets/Prefabs/\n\n" +
                "Continue?", "Yes", "Cancel"))
            {
                return;
            }

            Debug.Log("=== Starting Auto Scene Setup ===");

            // Step 1: Create Materials
            CreateMaterials();

            // Step 2: Setup Camera
            SetupCamera();

            // Step 3: Create GameManager
            GameObject gameManager = CreateGameManager();

            // Step 4: Create Canvas and UI
            GameObject canvas = CreateUI();

            // Step 5: Assign all references
            AssignReferences(gameManager, canvas);

            // Step 6: Save scene
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("=== Scene Setup Complete! ===");
            EditorUtility.DisplayDialog("Success!", 
                "Scene setup complete!\n\n" +
                "Next steps:\n" +
                "1. Assign Audio Clip to GameManager AudioSource\n" +
                "2. Set Beatmap Path in GameManager3D\n" +
                "3. Press Play to test!", "OK");
        }

        static void CreateMaterials()
        {
            Debug.Log("Creating Materials...");

            System.IO.Directory.CreateDirectory("Assets/Materials");

            // NoteMaterial
            string noteMatPath = "Assets/Materials/NoteMaterial.mat";
            Material noteMat = new Material(Shader.Find("Standard"));
            noteMat.SetFloat("_Mode", 3);
            noteMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            noteMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            noteMat.SetInt("_ZWrite", 0);
            noteMat.DisableKeyword("_ALPHATEST_ON");
            noteMat.DisableKeyword("_ALPHABLEND_ON");
            noteMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            noteMat.renderQueue = 3000;

            Color cyan = new Color(100f / 255f, 180f / 255f, 1f, 1f);
            noteMat.SetColor("_Color", cyan);
            noteMat.EnableKeyword("_EMISSION");
            noteMat.SetColor("_EmissionColor", cyan);
            noteMat.SetFloat("_Metallic", 0f);
            noteMat.SetFloat("_Glossiness", 0.5f);

            AssetDatabase.CreateAsset(noteMat, noteMatPath);

            // LaneMaterial
            string laneMatPath = "Assets/Materials/LaneMaterial.mat";
            Material laneMat = new Material(Shader.Find("Standard"));
            laneMat.SetFloat("_Mode", 3);
            laneMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            laneMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            laneMat.SetInt("_ZWrite", 0);
            laneMat.DisableKeyword("_ALPHATEST_ON");
            laneMat.DisableKeyword("_ALPHABLEND_ON");
            laneMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            laneMat.renderQueue = 3000;

            Color white = new Color(1f, 1f, 1f, 80f / 255f);
            laneMat.SetColor("_Color", white);
            laneMat.SetFloat("_Metallic", 0f);
            laneMat.SetFloat("_Glossiness", 0.2f);

            AssetDatabase.CreateAsset(laneMat, laneMatPath);
            AssetDatabase.SaveAssets();

            Debug.Log("Materials created!");
        }

        static void SetupCamera()
        {
            Debug.Log("Setting up Camera...");
            Camera cam = Camera.main;
            if (cam)
            {
                cam.transform.position = new Vector3(0, 3, -5);
                cam.transform.rotation = Quaternion.Euler(15, 0, 0);
                cam.fieldOfView = 60;

                if (!cam.GetComponent<CameraController3D>())
                {
                    cam.gameObject.AddComponent<CameraController3D>();
                }
            }
        }

        static GameObject CreateGameManager()
        {
            Debug.Log("Creating GameManager...");
            GameObject existing = GameObject.Find("GameManager");
            if (existing) DestroyImmediate(existing);

            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager3D>();
            gm.AddComponent<NoteSpawner3D>();
            gm.AddComponent<InputManager3D>();
            gm.AddComponent<JudgmentSystem>();
            gm.AddComponent<AudioManager3D>();
            gm.AddComponent<AudioSource>();

            return gm;
        }

        static GameObject CreateUI()
        {
            Debug.Log("Creating UI...");
            
            GameObject oldCanvas = GameObject.Find("Canvas");
            if (oldCanvas) DestroyImmediate(oldCanvas);

            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasGO.AddComponent<GraphicRaycaster>();
            canvasGO.AddComponent<ModernUIManager3D>();

            if (!GameObject.Find("EventSystem"))
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            CreateTopRightPanel(canvasGO);
            CreateJudgmentPanel(canvasGO);
            CreateBottomPanel(canvasGO);
            CreateGameOverPanel(canvasGO);
            CreatePausePanel(canvasGO);

            return canvasGO;
        }

        static void CreateTopRightPanel(GameObject canvas)
        {
            GameObject panel = new GameObject("TopRightPanel");
            panel.transform.SetParent(canvas.transform, false);
            
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-150, -50);
            rt.sizeDelta = new Vector2(300, 100);

            GameObject scoreText = new GameObject("ScoreText");
            scoreText.transform.SetParent(panel.transform, false);
            TextMeshProUGUI score = scoreText.AddComponent<TextMeshProUGUI>();
            score.text = "000000";
            score.fontSize = 48;
            score.color = new Color(1f, 215f / 255f, 0f);
            score.alignment = TextAlignmentOptions.Right;
            score.fontStyle = FontStyles.Bold;
            RectTransform scoreRT = score.rectTransform;
            scoreRT.anchorMin = new Vector2(0, 1);
            scoreRT.anchorMax = new Vector2(1, 1);
            scoreRT.pivot = new Vector2(0.5f, 1);
            scoreRT.anchoredPosition = Vector2.zero;
            scoreRT.sizeDelta = new Vector2(0, 50);

            GameObject comboText = new GameObject("ComboText");
            comboText.transform.SetParent(panel.transform, false);
            TextMeshProUGUI combo = comboText.AddComponent<TextMeshProUGUI>();
            combo.text = "0x";
            combo.fontSize = 56;
            combo.color = new Color(100f / 255f, 220f / 255f, 1f);
            combo.alignment = TextAlignmentOptions.Right;
            combo.fontStyle = FontStyles.Bold;
            RectTransform comboRT = combo.rectTransform;
            comboRT.anchorMin = new Vector2(0, 0);
            comboRT.anchorMax = new Vector2(1, 0);
            comboRT.pivot = new Vector2(0.5f, 0);
            comboRT.anchoredPosition = new Vector2(0, -60);
            comboRT.sizeDelta = new Vector2(0, 60);
        }

        static void CreateJudgmentPanel(GameObject canvas)
        {
            GameObject panel = new GameObject("JudgmentPanel");
            panel.transform.SetParent(canvas.transform, false);
            
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 50);
            rt.sizeDelta = new Vector2(400, 200);

            panel.AddComponent<CanvasGroup>();

            GameObject judgmentText = new GameObject("JudgmentText");
            judgmentText.transform.SetParent(panel.transform, false);
            TextMeshProUGUI judgment = judgmentText.AddComponent<TextMeshProUGUI>();
            judgment.text = "Perfect";
            judgment.fontSize = 72;
            judgment.color = Color.white;
            judgment.alignment = TextAlignmentOptions.Center;
            judgment.fontStyle = FontStyles.Bold;
            judgment.enableAutoSizing = true;
            judgment.fontSizeMin = 36;
            judgment.fontSizeMax = 72;
            RectTransform judgmentRT = judgment.rectTransform;
            judgmentRT.anchorMin = Vector2.zero;
            judgmentRT.anchorMax = Vector2.one;
            judgmentRT.pivot = new Vector2(0.5f, 0.5f);
            judgmentRT.anchoredPosition = Vector2.zero;
            judgmentRT.sizeDelta = Vector2.zero;

            GameObject earlyLateText = new GameObject("EarlyLateText");
            earlyLateText.transform.SetParent(panel.transform, false);
            TextMeshProUGUI earlyLate = earlyLateText.AddComponent<TextMeshProUGUI>();
            earlyLate.text = "Early";
            earlyLate.fontSize = 32;
            earlyLate.color = new Color(100f / 255f, 1f, 100f / 255f);
            earlyLate.alignment = TextAlignmentOptions.Center;
            RectTransform earlyLateRT = earlyLate.rectTransform;
            earlyLateRT.anchorMin = new Vector2(0, 0);
            earlyLateRT.anchorMax = new Vector2(1, 0);
            earlyLateRT.pivot = new Vector2(0.5f, 0);
            earlyLateRT.anchoredPosition = new Vector2(0, -60);
            earlyLateRT.sizeDelta = new Vector2(0, 40);
        }

        static void CreateBottomPanel(GameObject canvas)
        {
            GameObject panel = new GameObject("BottomPanel");
            panel.transform.SetParent(canvas.transform, false);
            
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 40);
            rt.sizeDelta = new Vector2(0, 80);

            GameObject accuracyText = new GameObject("AccuracyText");
            accuracyText.transform.SetParent(panel.transform, false);
            TextMeshProUGUI accuracy = accuracyText.AddComponent<TextMeshProUGUI>();
            accuracy.text = "100.00%";
            accuracy.fontSize = 36;
            accuracy.color = new Color(1f, 215f / 255f, 0f);
            accuracy.alignment = TextAlignmentOptions.Center;
            accuracy.fontStyle = FontStyles.Bold;
            RectTransform accRT = accuracy.rectTransform;
            accRT.anchorMin = new Vector2(0.5f, 0.5f);
            accRT.anchorMax = new Vector2(0.5f, 0.5f);
            accRT.pivot = new Vector2(0.5f, 0.5f);
            accRT.anchoredPosition = Vector2.zero;
            accRT.sizeDelta = new Vector2(300, 40);

            GameObject healthBar = new GameObject("HealthBar");
            healthBar.transform.SetParent(panel.transform, false);
            Slider slider = healthBar.AddComponent<Slider>();
            RectTransform sliderRT = slider.GetComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0, 0.5f);
            sliderRT.anchorMax = new Vector2(1, 0.5f);
            sliderRT.pivot = new Vector2(0.5f, 0.5f);
            sliderRT.anchoredPosition = new Vector2(0, -30);
            sliderRT.sizeDelta = new Vector2(0, 20);
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;
            slider.interactable = false;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(healthBar.transform, false);
            RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = Color.green;
            RectTransform fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;

            slider.fillRect = fillRT;

            GameObject timeText = new GameObject("TimeText");
            timeText.transform.SetParent(panel.transform, false);
            TextMeshProUGUI time = timeText.AddComponent<TextMeshProUGUI>();
            time.text = "0:00 / 3:00";
            time.fontSize = 24;
            time.color = new Color(1f, 1f, 1f, 180f / 255f);
            time.alignment = TextAlignmentOptions.Right;
            RectTransform timeRT = time.rectTransform;
            timeRT.anchorMin = new Vector2(1, 0.5f);
            timeRT.anchorMax = new Vector2(1, 0.5f);
            timeRT.pivot = new Vector2(1, 0.5f);
            timeRT.anchoredPosition = new Vector2(-20, 0);
            timeRT.sizeDelta = new Vector2(200, 30);
        }

        static void CreateGameOverPanel(GameObject canvas)
        {
            GameObject panel = new GameObject("GameOverPanel");
            panel.transform.SetParent(canvas.transform, false);
            
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            
            // Semi-transparent background
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.9f);
            
            // Content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(panel.transform, false);
            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.pivot = new Vector2(0.5f, 0.5f);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(600, 700);
            
            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(content.transform, false);
            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "GAME OVER";
            title.fontSize = 72;
            title.color = new Color(1f, 0.3f, 0.3f);
            title.alignment = TextAlignmentOptions.Center;
            title.fontStyle = FontStyles.Bold;
            RectTransform titleRT = title.rectTransform;
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.anchoredPosition = new Vector2(0, -20);
            titleRT.sizeDelta = new Vector2(0, 80);
            
            // Score
            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(content.transform, false);
            TextMeshProUGUI score = scoreObj.AddComponent<TextMeshProUGUI>();
            score.text = "000000";
            score.fontSize = 56;
            score.color = new Color(1f, 215f / 255f, 0f);
            score.alignment = TextAlignmentOptions.Center;
            score.fontStyle = FontStyles.Bold;
            RectTransform scoreRT = score.rectTransform;
            scoreRT.anchorMin = new Vector2(0, 1);
            scoreRT.anchorMax = new Vector2(1, 1);
            scoreRT.pivot = new Vector2(0.5f, 1);
            scoreRT.anchoredPosition = new Vector2(0, -120);
            scoreRT.sizeDelta = new Vector2(0, 60);
            
            // Accuracy
            GameObject accObj = new GameObject("AccuracyText");
            accObj.transform.SetParent(content.transform, false);
            TextMeshProUGUI acc = accObj.AddComponent<TextMeshProUGUI>();
            acc.text = "0.00%";
            acc.fontSize = 40;
            acc.color = Color.white;
            acc.alignment = TextAlignmentOptions.Center;
            RectTransform accRT = acc.rectTransform;
            accRT.anchorMin = new Vector2(0, 1);
            accRT.anchorMax = new Vector2(1, 1);
            accRT.pivot = new Vector2(0.5f, 1);
            accRT.anchoredPosition = new Vector2(0, -200);
            accRT.sizeDelta = new Vector2(0, 50);
            
            // Stats
            GameObject statsObj = new GameObject("StatsText");
            statsObj.transform.SetParent(content.transform, false);
            TextMeshProUGUI stats = statsObj.AddComponent<TextMeshProUGUI>();
            stats.text = "Perfect: 0\nGreat: 0\nGood: 0\nMiss: 0\nMax Combo: 0";
            stats.fontSize = 32;
            stats.color = new Color(0.8f, 0.8f, 0.8f);
            stats.alignment = TextAlignmentOptions.Center;
            RectTransform statsRT = stats.rectTransform;
            statsRT.anchorMin = new Vector2(0, 0.5f);
            statsRT.anchorMax = new Vector2(1, 0.5f);
            statsRT.pivot = new Vector2(0.5f, 0.5f);
            statsRT.anchoredPosition = new Vector2(0, 0);
            statsRT.sizeDelta = new Vector2(0, 200);
            
            // Restart Button
            GameObject restartBtn = new GameObject("RestartButton");
            restartBtn.transform.SetParent(content.transform, false);
            RectTransform restartRT = restartBtn.AddComponent<RectTransform>();
            restartRT.anchorMin = new Vector2(0.5f, 0);
            restartRT.anchorMax = new Vector2(0.5f, 0);
            restartRT.pivot = new Vector2(0.5f, 0);
            restartRT.anchoredPosition = new Vector2(0, 100);
            restartRT.sizeDelta = new Vector2(300, 60);
            
            Button restart = restartBtn.AddComponent<Button>();
            Image restartImg = restartBtn.AddComponent<Image>();
            restartImg.color = new Color(0.2f, 0.8f, 0.2f);
            
            GameObject restartTextObj = new GameObject("Text");
            restartTextObj.transform.SetParent(restartBtn.transform, false);
            TextMeshProUGUI restartText = restartTextObj.AddComponent<TextMeshProUGUI>();
            restartText.text = "RESTART";
            restartText.fontSize = 36;
            restartText.color = Color.white;
            restartText.alignment = TextAlignmentOptions.Center;
            restartText.fontStyle = FontStyles.Bold;
            RectTransform restartTextRT = restartText.rectTransform;
            restartTextRT.anchorMin = Vector2.zero;
            restartTextRT.anchorMax = Vector2.one;
            restartTextRT.sizeDelta = Vector2.zero;
            
            // Exit Button
            GameObject exitBtn = new GameObject("ExitButton");
            exitBtn.transform.SetParent(content.transform, false);
            RectTransform exitRT = exitBtn.AddComponent<RectTransform>();
            exitRT.anchorMin = new Vector2(0.5f, 0);
            exitRT.anchorMax = new Vector2(0.5f, 0);
            exitRT.pivot = new Vector2(0.5f, 0);
            exitRT.anchoredPosition = new Vector2(0, 20);
            exitRT.sizeDelta = new Vector2(300, 60);
            
            Button exit = exitBtn.AddComponent<Button>();
            Image exitImg = exitBtn.AddComponent<Image>();
            exitImg.color = new Color(0.8f, 0.2f, 0.2f);
            
            GameObject exitTextObj = new GameObject("Text");
            exitTextObj.transform.SetParent(exitBtn.transform, false);
            TextMeshProUGUI exitText = exitTextObj.AddComponent<TextMeshProUGUI>();
            exitText.text = "EXIT";
            exitText.fontSize = 36;
            exitText.color = Color.white;
            exitText.alignment = TextAlignmentOptions.Center;
            exitText.fontStyle = FontStyles.Bold;
            RectTransform exitTextRT = exitText.rectTransform;
            exitTextRT.anchorMin = Vector2.zero;
            exitTextRT.anchorMax = Vector2.one;
            exitTextRT.sizeDelta = Vector2.zero;
            
            // Hide panel initially
            panel.SetActive(false);
        }

        static void CreatePausePanel(GameObject canvas)
        {
            GameObject panel = new GameObject("PausePanel");
            panel.transform.SetParent(canvas.transform, false);
            
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            
            // Semi-transparent background
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.85f);
            
            // Content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(panel.transform, false);
            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.pivot = new Vector2(0.5f, 0.5f);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(500, 400);
            
            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(content.transform, false);
            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "PAUSED";
            title.fontSize = 72;
            title.color = new Color(1f, 0.84f, 0f);
            title.alignment = TextAlignmentOptions.Center;
            title.fontStyle = FontStyles.Bold;
            RectTransform titleRT = title.rectTransform;
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.anchoredPosition = new Vector2(0, -20);
            titleRT.sizeDelta = new Vector2(0, 80);
            
            // Hint text
            GameObject hintObj = new GameObject("HintText");
            hintObj.transform.SetParent(content.transform, false);
            TextMeshProUGUI hint = hintObj.AddComponent<TextMeshProUGUI>();
            hint.text = "Press ESC or P to resume";
            hint.fontSize = 20;
            hint.color = new Color(0.7f, 0.7f, 0.7f);
            hint.alignment = TextAlignmentOptions.Center;
            RectTransform hintRT = hint.rectTransform;
            hintRT.anchorMin = new Vector2(0, 1);
            hintRT.anchorMax = new Vector2(1, 1);
            hintRT.pivot = new Vector2(0.5f, 1);
            hintRT.anchoredPosition = new Vector2(0, -110);
            hintRT.sizeDelta = new Vector2(0, 30);
            
            // Resume Button
            GameObject resumeBtn = new GameObject("ResumeButton");
            resumeBtn.transform.SetParent(content.transform, false);
            RectTransform resumeRT = resumeBtn.AddComponent<RectTransform>();
            resumeRT.anchorMin = new Vector2(0.5f, 0.5f);
            resumeRT.anchorMax = new Vector2(0.5f, 0.5f);
            resumeRT.pivot = new Vector2(0.5f, 0.5f);
            resumeRT.anchoredPosition = new Vector2(0, 20);
            resumeRT.sizeDelta = new Vector2(300, 70);
            
            Button resume = resumeBtn.AddComponent<Button>();
            Image resumeImg = resumeBtn.AddComponent<Image>();
            resumeImg.color = new Color(0.3f, 0.9f, 0.3f);
            
            GameObject resumeTextObj = new GameObject("Text");
            resumeTextObj.transform.SetParent(resumeBtn.transform, false);
            TextMeshProUGUI resumeText = resumeTextObj.AddComponent<TextMeshProUGUI>();
            resumeText.text = "RESUME";
            resumeText.fontSize = 36;
            resumeText.color = Color.white;
            resumeText.alignment = TextAlignmentOptions.Center;
            resumeText.fontStyle = FontStyles.Bold;
            RectTransform resumeTextRT = resumeText.rectTransform;
            resumeTextRT.anchorMin = Vector2.zero;
            resumeTextRT.anchorMax = Vector2.one;
            resumeTextRT.sizeDelta = Vector2.zero;
            
            // Return to Menu Button
            GameObject menuBtn = new GameObject("ReturnToMenuButton");
            menuBtn.transform.SetParent(content.transform, false);
            RectTransform menuRT = menuBtn.AddComponent<RectTransform>();
            menuRT.anchorMin = new Vector2(0.5f, 0.5f);
            menuRT.anchorMax = new Vector2(0.5f, 0.5f);
            menuRT.pivot = new Vector2(0.5f, 0.5f);
            menuRT.anchoredPosition = new Vector2(0, -80);
            menuRT.sizeDelta = new Vector2(300, 70);
            
            Button menu = menuBtn.AddComponent<Button>();
            Image menuImg = menuBtn.AddComponent<Image>();
            menuImg.color = new Color(0.9f, 0.3f, 0.3f);
            
            GameObject menuTextObj = new GameObject("Text");
            menuTextObj.transform.SetParent(menuBtn.transform, false);
            TextMeshProUGUI menuText = menuTextObj.AddComponent<TextMeshProUGUI>();
            menuText.text = "RETURN TO MENU";
            menuText.fontSize = 32;
            menuText.color = Color.white;
            menuText.alignment = TextAlignmentOptions.Center;
            menuText.fontStyle = FontStyles.Bold;
            RectTransform menuTextRT = menuText.rectTransform;
            menuTextRT.anchorMin = Vector2.zero;
            menuTextRT.anchorMax = Vector2.one;
            menuTextRT.sizeDelta = Vector2.zero;
            
            // Hide panel initially
            panel.SetActive(false);
        }

        static void AssignReferences(GameObject gameManager, GameObject canvas)
        {
            Debug.Log("Assigning all references...");

            Material laneMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/LaneMaterial.mat");
            GameObject tapNotePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TapNote.prefab");
            GameObject longNotePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/LongNote.prefab");

            var gmScript = gameManager.GetComponent<GameManager3D>();
            var spawner = gameManager.GetComponent<NoteSpawner3D>();
            var input = gameManager.GetComponent<InputManager3D>();
            var judgment = gameManager.GetComponent<JudgmentSystem>();
            var audioMgr = gameManager.GetComponent<AudioManager3D>();
            var audio = gameManager.GetComponent<AudioSource>();
            var uiManager = canvas.GetComponent<ModernUIManager3D>();

            if (spawner)
            {
                SerializedObject so = new SerializedObject(spawner);
                so.FindProperty("tapNotePrefab").objectReferenceValue = tapNotePrefab;
                so.FindProperty("longNotePrefab").objectReferenceValue = longNotePrefab;
                so.FindProperty("laneMaterial").objectReferenceValue = laneMat;
                so.FindProperty("judgmentSystem").objectReferenceValue = judgment;
                so.ApplyModifiedProperties();
            }

            if (input)
            {
                SerializedObject so = new SerializedObject(input);
                so.FindProperty("noteSpawner").objectReferenceValue = spawner;
                so.FindProperty("judgmentSystem").objectReferenceValue = judgment;
                so.ApplyModifiedProperties();
            }

            if (gmScript)
            {
                SerializedObject so = new SerializedObject(gmScript);
                so.FindProperty("noteSpawner").objectReferenceValue = spawner;
                so.FindProperty("inputManager").objectReferenceValue = input;
                so.FindProperty("judgmentSystem").objectReferenceValue = judgment;
                so.FindProperty("uiManager").objectReferenceValue = uiManager;
                so.FindProperty("audioManager").objectReferenceValue = audioMgr;
                so.FindProperty("musicSource").objectReferenceValue = audio;
                so.FindProperty("audioOffset").floatValue = 0.1f;
                so.ApplyModifiedProperties();
            }

            if (uiManager)
            {
                SerializedObject so = new SerializedObject(uiManager);
                so.FindProperty("scoreText").objectReferenceValue = canvas.transform.Find("TopRightPanel/ScoreText").GetComponent<TextMeshProUGUI>();
                so.FindProperty("comboText").objectReferenceValue = canvas.transform.Find("TopRightPanel/ComboText").GetComponent<TextMeshProUGUI>();
                so.FindProperty("judgmentText").objectReferenceValue = canvas.transform.Find("JudgmentPanel/JudgmentText").GetComponent<TextMeshProUGUI>();
                so.FindProperty("earlyLateText").objectReferenceValue = canvas.transform.Find("JudgmentPanel/EarlyLateText").GetComponent<TextMeshProUGUI>();
                so.FindProperty("judgmentGroup").objectReferenceValue = canvas.transform.Find("JudgmentPanel").GetComponent<CanvasGroup>();
                so.FindProperty("accuracyText").objectReferenceValue = canvas.transform.Find("BottomPanel/AccuracyText").GetComponent<TextMeshProUGUI>();
                so.FindProperty("healthBar").objectReferenceValue = canvas.transform.Find("BottomPanel/HealthBar").GetComponent<Slider>();
                so.FindProperty("timeText").objectReferenceValue = canvas.transform.Find("BottomPanel/TimeText").GetComponent<TextMeshProUGUI>();
                
                // Game Over Panel references
                GameObject gameOverPanel = canvas.transform.Find("GameOverPanel").gameObject;
                so.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
                so.FindProperty("gameOverScoreText").objectReferenceValue = gameOverPanel.transform.Find("Content/ScoreText").GetComponent<TextMeshProUGUI>();
                so.FindProperty("gameOverAccuracyText").objectReferenceValue = gameOverPanel.transform.Find("Content/AccuracyText").GetComponent<TextMeshProUGUI>();
                so.FindProperty("gameOverStatsText").objectReferenceValue = gameOverPanel.transform.Find("Content/StatsText").GetComponent<TextMeshProUGUI>();
                so.FindProperty("restartButton").objectReferenceValue = gameOverPanel.transform.Find("Content/RestartButton").GetComponent<Button>();
                so.FindProperty("exitButton").objectReferenceValue = gameOverPanel.transform.Find("Content/ExitButton").GetComponent<Button>();
                
                so.ApplyModifiedProperties();
            }
            
            // Setup PauseManager
            GameObject pausePanel = canvas.transform.Find("PausePanel").gameObject;
            PauseManager pauseMgr = canvas.AddComponent<PauseManager>();
            
            SerializedObject pauseSO = new SerializedObject(pauseMgr);
            pauseSO.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            pauseSO.FindProperty("resumeButton").objectReferenceValue = pausePanel.transform.Find("Content/ResumeButton").GetComponent<Button>();
            pauseSO.FindProperty("returnToMenuButton").objectReferenceValue = pausePanel.transform.Find("Content/ReturnToMenuButton").GetComponent<Button>();
            pauseSO.ApplyModifiedProperties();
            
            Debug.Log("PauseManager setup!");
            
            // Setup button listeners
            if (gmScript && uiManager)
            {
                Button restartBtn = canvas.transform.Find("GameOverPanel/Content/RestartButton").GetComponent<Button>();
                Button exitBtn = canvas.transform.Find("GameOverPanel/Content/ExitButton").GetComponent<Button>();
                
                restartBtn.onClick.AddListener(gmScript.RestartGame);
                exitBtn.onClick.AddListener(gmScript.ExitGame);
                
                Debug.Log("Button listeners setup!");
            }

            Debug.Log("All references assigned!");
        }
    }
}
