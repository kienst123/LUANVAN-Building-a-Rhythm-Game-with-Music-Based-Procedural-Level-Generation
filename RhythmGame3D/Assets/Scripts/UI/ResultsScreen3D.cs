using UnityEngine;
using TMPro;
using RhythmGame3D.UI.Menu3D;

namespace RhythmGame3D.UI
{
    /// <summary>
    /// Màn hình kết quả sau khi hoàn thành bài hát
    /// Hiển thị điểm số, accuracy, combo, và nút Retry/Menu
    /// </summary>
    public class ResultsScreen3D : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshPro titleText;
        public TextMeshPro scoreText;
        public TextMeshPro accuracyText;
        public TextMeshPro maxComboText;
        public TextMeshPro gradeText;
        public TextMeshPro perfectCountText;
        public TextMeshPro greatCountText;
        public TextMeshPro goodCountText;
        public TextMeshPro missCountText;
        
        public MenuButton3D retryButton;
        public MenuButton3D menuButton;
        
        private Canvas canvas;
        private Camera mainCamera;
        
        void Start()
        {
            mainCamera = Camera.main;
            SetupUI();
            gameObject.SetActive(false); // Ẩn ban đầu
        }
        
        void SetupUI()
        {
            // Tạo Canvas 3D
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;
            
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1920, 1080);
            canvasRect.position = new Vector3(0, 0, 10f);
            canvasRect.localScale = Vector3.one * 0.01f; // Scale nhỏ lại
            
            // Background panel (mờ đen)
            GameObject bgPanel = new GameObject("Background");
            bgPanel.transform.SetParent(transform);
            bgPanel.transform.localPosition = new Vector3(0, 0, -1f);
            bgPanel.transform.localScale = new Vector3(100f, 100f, 1f);
            
            GameObject bgMesh = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bgMesh.transform.SetParent(bgPanel.transform);
            bgMesh.transform.localPosition = Vector3.zero;
            bgMesh.transform.localScale = Vector3.one;
            
            MeshRenderer bgRenderer = bgMesh.GetComponent<MeshRenderer>();
            Material bgMat = new Material(Shader.Find("Standard"));
            bgMat.color = new Color(0f, 0f, 0f, 0.85f);
            bgRenderer.material = bgMat;
            Destroy(bgMesh.GetComponent<Collider>());
            
            // Title "RESULTS"
            CreateTitle();
            
            // Score texts
            CreateScoreTexts();
            
            // Judgment counts
            CreateJudgmentCounts();
            
            // Buttons
            CreateButtons();
        }
        
        void CreateTitle()
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(transform);
            titleObj.transform.localPosition = new Vector3(0f, 8f, 0f);
            
            titleText = titleObj.AddComponent<TextMeshPro>();
            titleText.text = "RESULTS";
            titleText.fontSize = 12;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0f, 0.94f, 1f);
            titleText.fontStyle = TMPro.FontStyles.Bold;
        }
        
        void CreateScoreTexts()
        {
            // Score
            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(transform);
            scoreObj.transform.localPosition = new Vector3(0f, 5f, 0f);
            
            scoreText = scoreObj.AddComponent<TextMeshPro>();
            scoreText.text = "SCORE: 0";
            scoreText.fontSize = 6;
            scoreText.alignment = TextAlignmentOptions.Center;
            scoreText.color = Color.yellow;
            scoreText.fontStyle = TMPro.FontStyles.Bold;
            
            // Accuracy
            GameObject accObj = new GameObject("Accuracy");
            accObj.transform.SetParent(transform);
            accObj.transform.localPosition = new Vector3(0f, 3f, 0f);
            
            accuracyText = accObj.AddComponent<TextMeshPro>();
            accuracyText.text = "ACCURACY: 0.00%";
            accuracyText.fontSize = 4;
            accuracyText.alignment = TextAlignmentOptions.Center;
            accuracyText.color = Color.white;
            
            // Max Combo
            GameObject comboObj = new GameObject("MaxCombo");
            comboObj.transform.SetParent(transform);
            comboObj.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            
            maxComboText = comboObj.AddComponent<TextMeshPro>();
            maxComboText.text = "MAX COMBO: 0x";
            maxComboText.fontSize = 4;
            maxComboText.alignment = TextAlignmentOptions.Center;
            maxComboText.color = Color.cyan;
            
            // Grade (S, A, B, C, D, F)
            GameObject gradeObj = new GameObject("Grade");
            gradeObj.transform.SetParent(transform);
            gradeObj.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            
            gradeText = gradeObj.AddComponent<TextMeshPro>();
            gradeText.text = "C";
            gradeText.fontSize = 10;
            gradeText.alignment = TextAlignmentOptions.Center;
            gradeText.color = Color.white;
            gradeText.fontStyle = TMPro.FontStyles.Bold;
        }
        
        void CreateJudgmentCounts()
        {
            float startY = -3f;
            float spacing = 1.2f;
            
            // Perfect
            GameObject perfectObj = new GameObject("PerfectCount");
            perfectObj.transform.SetParent(transform);
            perfectObj.transform.localPosition = new Vector3(0f, startY, 0f);
            
            perfectCountText = perfectObj.AddComponent<TextMeshPro>();
            perfectCountText.text = "PERFECT: 0";
            perfectCountText.fontSize = 3;
            perfectCountText.alignment = TextAlignmentOptions.Center;
            perfectCountText.color = new Color(0f, 1f, 1f); // Cyan
            
            // Great
            GameObject greatObj = new GameObject("GreatCount");
            greatObj.transform.SetParent(transform);
            greatObj.transform.localPosition = new Vector3(0f, startY - spacing, 0f);
            
            greatCountText = greatObj.AddComponent<TextMeshPro>();
            greatCountText.text = "GREAT: 0";
            greatCountText.fontSize = 3;
            greatCountText.alignment = TextAlignmentOptions.Center;
            greatCountText.color = new Color(0f, 1f, 0f); // Green
            
            // Good
            GameObject goodObj = new GameObject("GoodCount");
            goodObj.transform.SetParent(transform);
            goodObj.transform.localPosition = new Vector3(0f, startY - spacing * 2, 0f);
            
            goodCountText = goodObj.AddComponent<TextMeshPro>();
            goodCountText.text = "GOOD: 0";
            goodCountText.fontSize = 3;
            goodCountText.alignment = TextAlignmentOptions.Center;
            goodCountText.color = new Color(1f, 1f, 0f); // Yellow
            
            // Miss
            GameObject missObj = new GameObject("MissCount");
            missObj.transform.SetParent(transform);
            missObj.transform.localPosition = new Vector3(0f, startY - spacing * 3, 0f);
            
            missCountText = missObj.AddComponent<TextMeshPro>();
            missCountText.text = "MISS: 0";
            missCountText.fontSize = 3;
            missCountText.alignment = TextAlignmentOptions.Center;
            missCountText.color = new Color(1f, 0f, 0f); // Red
        }
        
        void CreateButtons()
        {
            // RETRY button
            GameObject retryObj = CreateButton("RETRY", new Vector3(-3f, -7.5f, 0f), new Color(0f, 1f, 0.5f));
            retryButton = retryObj.GetComponent<MenuButton3D>();
            retryButton.AddClickListener(OnRetryClicked);
            
            // MENU button
            GameObject menuObj = CreateButton("MENU", new Vector3(3f, -7.5f, 0f), new Color(1f, 0.5f, 0f));
            menuButton = menuObj.GetComponent<MenuButton3D>();
            menuButton.AddClickListener(OnMenuClicked);
        }
        
        GameObject CreateButton(string text, Vector3 position, Color buttonColor)
        {
            GameObject buttonObj = new GameObject($"Button_{text}");
            buttonObj.transform.SetParent(transform);
            buttonObj.transform.localPosition = position;
            
            // Add BoxCollider
            BoxCollider collider = buttonObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(5f, 1.5f, 0.5f);
            
            // Create button mesh
            GameObject buttonMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buttonMesh.name = "ButtonMesh";
            buttonMesh.transform.SetParent(buttonObj.transform);
            buttonMesh.transform.localPosition = Vector3.zero;
            buttonMesh.transform.localScale = new Vector3(5f, 1.5f, 0.5f);
            
            Destroy(buttonMesh.GetComponent<BoxCollider>());
            
            // Setup material
            MeshRenderer renderer = buttonMesh.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.EnableKeyword("_EMISSION");
            mat.color = buttonColor * 0.5f;
            mat.SetColor("_EmissionColor", buttonColor * 0.3f);
            renderer.material = mat;
            
            // Create text
            GameObject textObj = new GameObject("ButtonText");
            textObj.transform.SetParent(buttonObj.transform);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.3f);
            
            TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
            textMesh.text = text;
            textMesh.fontSize = 3;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.color = Color.white;
            textMesh.fontStyle = TMPro.FontStyles.Bold;
            
            // Create glow light
            GameObject lightObj = new GameObject("GlowLight");
            lightObj.transform.SetParent(buttonObj.transform);
            lightObj.transform.localPosition = Vector3.zero;
            
            Light glowLight = lightObj.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.range = 6f;
            glowLight.intensity = 0.5f;
            glowLight.color = buttonColor;
            
            // Add MenuButton3D component
            MenuButton3D buttonScript = buttonObj.AddComponent<MenuButton3D>();
            buttonScript.buttonRenderer = renderer;
            buttonScript.buttonText = textMesh;
            buttonScript.glowLight = glowLight;
            
            return buttonObj;
        }
        
        public void ShowResults(int finalScore, float accuracy, int maxCombo, int perfect, int great, int good, int miss)
        {
            gameObject.SetActive(true);
            
            // Update texts
            scoreText.text = $"SCORE: {finalScore:N0}";
            accuracyText.text = $"ACCURACY: {accuracy:F2}%";
            maxComboText.text = $"MAX COMBO: {maxCombo}x";
            
            perfectCountText.text = $"PERFECT: {perfect}";
            greatCountText.text = $"GREAT: {great}";
            goodCountText.text = $"GOOD: {good}";
            missCountText.text = $"MISS: {miss}";
            
            // Calculate grade
            string grade = CalculateGrade(accuracy);
            gradeText.text = grade;
            gradeText.color = GetGradeColor(grade);
            
            Debug.Log($"[ResultsScreen3D] Showing results: Score={finalScore}, Acc={accuracy:F2}%, Grade={grade}");
        }
        
        string CalculateGrade(float accuracy)
        {
            if (accuracy >= 95f) return "S";
            if (accuracy >= 90f) return "A";
            if (accuracy >= 80f) return "B";
            if (accuracy >= 70f) return "C";
            if (accuracy >= 60f) return "D";
            return "F";
        }
        
        Color GetGradeColor(string grade)
        {
            switch (grade)
            {
                case "S": return new Color(1f, 0.84f, 0f); // Gold
                case "A": return new Color(0f, 1f, 0.5f); // Bright green
                case "B": return new Color(0f, 1f, 1f); // Cyan
                case "C": return new Color(1f, 1f, 0f); // Yellow
                case "D": return new Color(1f, 0.5f, 0f); // Orange
                case "F": return new Color(1f, 0f, 0f); // Red
                default: return Color.white;
            }
        }
        
        void OnRetryClicked()
        {
            Debug.Log("[ResultsScreen3D] RETRY clicked - Reloading scene");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        
        void OnMenuClicked()
        {
            Debug.Log("[ResultsScreen3D] MENU clicked - Loading MainMenu");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
