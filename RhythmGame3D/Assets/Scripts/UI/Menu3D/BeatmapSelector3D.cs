using UnityEngine;
using TMPro;
using RhythmGame3D.Beatmap;
using RhythmGame3D.UI;
using System.IO;

namespace RhythmGame3D.UI.Menu3D
{
    /// <summary>
    /// 3D version of beatmap selector for menu panel
    /// Uses 3D text and buttons instead of Canvas UI
    /// </summary>
    public class BeatmapSelector3D : MonoBehaviour
    {
        [Header("UI Elements (3D)")]
        public TextMeshPro titleText;
        public TextMeshPro selectedFileText;
        public TextMeshPro infoText;
        public TextMeshPro difficultyText; // THÊM text hiển thị độ khó
        public MenuButton3D selectButton;
        public MenuButton3D playButton;
        public MenuButton3D easyButton;    // THÊM nút Easy
        public MenuButton3D normalButton;  // THÊM nút Normal
        public MenuButton3D hardButton;    // THÊM nút Hard
        
        [Header("References")]
        public MainMenu3DManager menuManager;
        
        private string selectedMusicFile = "";
        private BeatmapData generatedBeatmap;
        private int currentDifficulty = 1; // 0=Easy, 1=Normal, 2=Hard (mặc định Normal)
        
        void Start()
        {
            SetupUI();
        }
        
        void SetupUI()
        {
            // Create title if not exists
            if (titleText == null)
            {
                GameObject titleObj = new GameObject("Title");
                titleObj.transform.parent = transform;
                titleObj.transform.localPosition = new Vector3(0f, 5f, 0f);
                titleText = titleObj.AddComponent<TextMeshPro>();
                titleText.text = "SELECT BEATMAP";
                titleText.fontSize = 8; // TĂNG từ 4 lên 8
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.color = new Color(0f, 0.94f, 1f);
                titleText.fontStyle = TMPro.FontStyles.Bold;
            }
            
            // Create selected file text
            if (selectedFileText == null)
            {
                GameObject fileObj = new GameObject("SelectedFile");
                fileObj.transform.parent = transform;
                fileObj.transform.localPosition = new Vector3(0f, 2.5f, 0f);
                selectedFileText = fileObj.AddComponent<TextMeshPro>();
                selectedFileText.text = "No file selected";
                selectedFileText.fontSize = 3; // TĂNG từ 2 lên 3
                selectedFileText.alignment = TextAlignmentOptions.Center;
                selectedFileText.color = Color.white;
                selectedFileText.rectTransform.sizeDelta = new Vector2(15f, 3f);
            }
            
            // Create info text
            if (infoText == null)
            {
                GameObject infoObj = new GameObject("Info");
                infoObj.transform.parent = transform;
                infoObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                infoText = infoObj.AddComponent<TextMeshPro>();
                infoText.text = "Click SELECT to choose music file\n(.mp3, .ogg, .wav)";
                infoText.fontSize = 2.5f; // TĂNG từ 1.5f lên 2.5f
                infoText.alignment = TextAlignmentOptions.Center;
                infoText.color = new Color(0.7f, 0.7f, 0.7f);
                infoText.rectTransform.sizeDelta = new Vector2(12f, 4f);
            }
            
            // Create difficulty text
            if (difficultyText == null)
            {
                GameObject diffObj = new GameObject("DifficultyLabel");
                diffObj.transform.parent = transform;
                diffObj.transform.localPosition = new Vector3(0f, -1.2f, 0f);
                difficultyText = diffObj.AddComponent<TextMeshPro>();
                difficultyText.text = "DIFFICULTY:";
                difficultyText.fontSize = 2.5f;
                difficultyText.alignment = TextAlignmentOptions.Center;
                difficultyText.color = new Color(1f, 1f, 0f);
                difficultyText.fontStyle = TMPro.FontStyles.Bold;
            }
            
            // Create difficulty buttons
            if (easyButton == null)
            {
                GameObject easyObj = CreateDifficultyButton("EASY", new Vector3(-4f, -2.5f, 0f), new Color(0f, 1f, 0f));
                easyButton = easyObj.GetComponent<MenuButton3D>();
                easyButton.AddClickListener(() => OnDifficultyChanged(0));
            }
            
            if (normalButton == null)
            {
                GameObject normalObj = CreateDifficultyButton("NORMAL", new Vector3(0f, -2.5f, 0f), new Color(1f, 1f, 0f));
                normalButton = normalObj.GetComponent<MenuButton3D>();
                normalButton.AddClickListener(() => OnDifficultyChanged(1));
            }
            
            if (hardButton == null)
            {
                GameObject hardObj = CreateDifficultyButton("HARD", new Vector3(4f, -2.5f, 0f), new Color(1f, 0f, 0f));
                hardButton = hardObj.GetComponent<MenuButton3D>();
                hardButton.AddClickListener(() => OnDifficultyChanged(2));
            }
            
            // Create SELECT button
            if (selectButton == null)
            {
                GameObject selectObj = CreateButton("SELECT", new Vector3(0f, -4f, 0f));
                selectButton = selectObj.GetComponent<MenuButton3D>();
                selectButton.AddClickListener(OnSelectClicked);
            }
            
            // Create PLAY button
            if (playButton == null)
            {
                GameObject playObj = CreateButton("PLAY", new Vector3(0f, -5.5f, 0f));
                playButton = playObj.GetComponent<MenuButton3D>();
                playButton.AddClickListener(OnPlayClicked);
            }
            
            UpdateDifficultyButtonColors(); // Highlight button mặc định
        }
        
        GameObject CreateButton(string text, Vector3 position)
        {
            GameObject buttonObj = new GameObject($"Button_{text}");
            buttonObj.transform.parent = transform;
            buttonObj.transform.localPosition = position;
            
            // Add BoxCollider - TĂNG SIZE
            BoxCollider collider = buttonObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(5f, 1.2f, 0.5f); // TĂNG từ 3x0.6x0.3
            
            // Create button mesh - TĂNG SIZE
            GameObject buttonMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buttonMesh.name = "ButtonMesh";
            buttonMesh.transform.parent = buttonObj.transform;
            buttonMesh.transform.localPosition = Vector3.zero;
            buttonMesh.transform.localScale = new Vector3(5f, 1.2f, 0.5f); // TĂNG từ 3x0.6x0.3
            
            Destroy(buttonMesh.GetComponent<BoxCollider>());
            
            // Setup material
            MeshRenderer renderer = buttonMesh.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.EnableKeyword("_EMISSION");
            mat.color = new Color(0.1f, 0.1f, 0.3f, 0.8f);
            renderer.material = mat;
            
            // Create text - TĂNG FONT SIZE
            GameObject textObj = new GameObject("ButtonText");
            textObj.transform.parent = buttonObj.transform;
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.3f);
            
            TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
            textMesh.text = text;
            textMesh.fontSize = 2.5f; // TĂNG từ 1.5f lên 2.5f
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.color = Color.white;
            textMesh.fontStyle = TMPro.FontStyles.Bold;
            
            // Create glow light
            GameObject lightObj = new GameObject("GlowLight");
            lightObj.transform.parent = buttonObj.transform;
            lightObj.transform.localPosition = Vector3.zero;
            
            Light glowLight = lightObj.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.range = 6f; // TĂNG từ 4f
            glowLight.intensity = 0.5f; // TĂNG từ 0.3f
            glowLight.color = new Color(0f, 0.94f, 1f, 1f);
            
            // Add MenuButton3D component
            MenuButton3D buttonScript = buttonObj.AddComponent<MenuButton3D>();
            buttonScript.buttonRenderer = renderer;
            buttonScript.buttonText = textMesh;
            buttonScript.glowLight = glowLight;
            
            return buttonObj;
        }
        
        GameObject CreateDifficultyButton(string text, Vector3 position, Color baseColor)
        {
            GameObject buttonObj = new GameObject($"DiffButton_{text}");
            buttonObj.transform.parent = transform;
            buttonObj.transform.localPosition = position;
            
            // Add BoxCollider - NHỎ HƠN NÚT CHÍNH
            BoxCollider collider = buttonObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(3.5f, 1f, 0.4f);
            
            // Create button mesh
            GameObject buttonMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buttonMesh.name = "ButtonMesh";
            buttonMesh.transform.parent = buttonObj.transform;
            buttonMesh.transform.localPosition = Vector3.zero;
            buttonMesh.transform.localScale = new Vector3(3.5f, 1f, 0.4f);
            
            Destroy(buttonMesh.GetComponent<BoxCollider>());
            
            // Setup material với màu sắc riêng
            MeshRenderer renderer = buttonMesh.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.EnableKeyword("_EMISSION");
            mat.color = baseColor * 0.3f; // Màu tối hơn khi chưa chọn
            renderer.material = mat;
            
            // Create text
            GameObject textObj = new GameObject("ButtonText");
            textObj.transform.parent = buttonObj.transform;
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.25f);
            
            TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
            textMesh.text = text;
            textMesh.fontSize = 2f;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.color = Color.white;
            textMesh.fontStyle = TMPro.FontStyles.Bold;
            
            // Create glow light
            GameObject lightObj = new GameObject("GlowLight");
            lightObj.transform.parent = buttonObj.transform;
            lightObj.transform.localPosition = Vector3.zero;
            
            Light glowLight = lightObj.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.range = 5f;
            glowLight.intensity = 0.3f;
            glowLight.color = baseColor;
            
            // Add MenuButton3D component
            MenuButton3D buttonScript = buttonObj.AddComponent<MenuButton3D>();
            buttonScript.buttonRenderer = renderer;
            buttonScript.buttonText = textMesh;
            buttonScript.glowLight = glowLight;
            
            return buttonObj;
        }
        
        void OnDifficultyChanged(int difficulty)
        {
            currentDifficulty = difficulty;
            Debug.Log($"[BeatmapSelector3D] Difficulty changed to: {GetDifficultyName()}");
            UpdateDifficultyButtonColors();
            
            // Nếu đã có file được chọn, tạo lại beatmap với độ khó mới
            if (!string.IsNullOrEmpty(selectedMusicFile))
            {
                GenerateBeatmap(selectedMusicFile);
            }
        }
        
        void UpdateDifficultyButtonColors()
        {
            // Easy button
            if (easyButton != null)
            {
                Color easyColor = new Color(0f, 1f, 0f);
                Material easyMat = easyButton.buttonRenderer.material;
                easyMat.color = currentDifficulty == 0 ? easyColor : easyColor * 0.3f;
                easyMat.SetColor("_EmissionColor", currentDifficulty == 0 ? easyColor * 0.5f : Color.black);
                easyButton.glowLight.intensity = currentDifficulty == 0 ? 0.8f : 0.2f;
            }
            
            // Normal button
            if (normalButton != null)
            {
                Color normalColor = new Color(1f, 1f, 0f);
                Material normalMat = normalButton.buttonRenderer.material;
                normalMat.color = currentDifficulty == 1 ? normalColor : normalColor * 0.3f;
                normalMat.SetColor("_EmissionColor", currentDifficulty == 1 ? normalColor * 0.5f : Color.black);
                normalButton.glowLight.intensity = currentDifficulty == 1 ? 0.8f : 0.2f;
            }
            
            // Hard button
            if (hardButton != null)
            {
                Color hardColor = new Color(1f, 0f, 0f);
                Material hardMat = hardButton.buttonRenderer.material;
                hardMat.color = currentDifficulty == 2 ? hardColor : hardColor * 0.3f;
                hardMat.SetColor("_EmissionColor", currentDifficulty == 2 ? hardColor * 0.5f : Color.black);
                hardButton.glowLight.intensity = currentDifficulty == 2 ? 0.8f : 0.2f;
            }
        }
        
        string GetDifficultyName()
        {
            switch (currentDifficulty)
            {
                case 0: return "EASY";
                case 1: return "NORMAL";
                case 2: return "HARD";
                default: return "NORMAL";
            }
        }
        
        void OnSelectClicked()
        {
            Debug.Log("[BeatmapSelector3D] Opening file browser...");
            
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.OpenFilePanel("Select Music File", "", "mp3,ogg,wav");
            
            if (!string.IsNullOrEmpty(path))
            {
                selectedMusicFile = path;
                string fileName = Path.GetFileName(path);
                
                if (selectedFileText != null)
                {
                    selectedFileText.text = $"Selected: {fileName}";
                    selectedFileText.color = new Color(0f, 1f, 0.5f);
                }
                
                if (infoText != null)
                {
                    infoText.text = $"Ready to play!\nPath: {path}\n\nClick PLAY to start";
                }
                
                // Generate beatmap
                GenerateBeatmap(path);
                
                Debug.Log($"[BeatmapSelector3D] Selected: {fileName}");
            }
#else
            if (infoText != null)
            {
                infoText.text = "File browser only available in Editor\nUse pre-loaded beatmaps in build";
            }
#endif
        }
        
        void GenerateBeatmap(string musicPath)
        {
            // Simple beatmap generation
            generatedBeatmap = new BeatmapData();
            generatedBeatmap.audioFilename = Path.GetFileName(musicPath);
            generatedBeatmap.title = Path.GetFileNameWithoutExtension(musicPath);
            generatedBeatmap.artist = "Unknown";
            generatedBeatmap.creator = "Auto-Generated";
            
            // Generate simple pattern (every beat)
            float bpm = 120f;
            float beatDuration = 60f / bpm;
            
            // SỐ NOTE DỰA TRÊN ĐỘ KHÓ
            int noteCount;
            float noteSpacing; // Khoảng cách giữa các note
            switch (currentDifficulty)
            {
                case 0: // EASY
                    noteCount = 40;
                    noteSpacing = 1.0f; // 1 beat spacing
                    break;
                case 1: // NORMAL
                    noteCount = 70;
                    noteSpacing = 0.75f; // 0.75 beat spacing
                    break;
                case 2: // HARD
                    noteCount = 120;
                    noteSpacing = 0.5f; // 0.5 beat spacing
                    break;
                default:
                    noteCount = 70;
                    noteSpacing = 0.75f;
                    break;
            }
            
            for (int i = 0; i < noteCount; i++)
            {
                int lane = i % 4; // Cycle through 4 lanes (0-3)
                int xPos = 64 + (lane * 128); // osu!mania 4K positions: 64, 192, 320, 448
                int noteTime = Mathf.RoundToInt(i * beatDuration * noteSpacing * 1000); // Convert to milliseconds
                
                // HARD có nhiều long note hơn
                bool isLongNote = false;
                if (currentDifficulty == 2)
                {
                    isLongNote = (i % 6 == 0); // Mỗi note thứ 6 là long note
                }
                else if (currentDifficulty == 1)
                {
                    isLongNote = (i % 8 == 0); // Mỗi note thứ 8 là long note
                }
                else
                {
                    isLongNote = (i % 12 == 0); // Mỗi note thứ 12 là long note (Easy ít long note)
                }
                
                int noteType = isLongNote ? 128 : 1; // 128 = long note, 1 = tap
                
                HitObject note = new HitObject(xPos, 192, noteTime, noteType, 0);
                
                // Set end time for long notes
                if (note.isLongNote)
                {
                    note.endTime = noteTime + Mathf.RoundToInt(beatDuration * 2 * 1000);
                }
                
                generatedBeatmap.AddHitObject(note);
            }
            
            // Store for gameplay
            BeatmapStorage.currentBeatmap = generatedBeatmap;
            BeatmapStorage.currentMusicPath = musicPath;
            
            Debug.Log($"[BeatmapSelector3D] Generated beatmap with {noteCount} notes ({GetDifficultyName()})");
            
            // Cập nhật info text với thông tin độ khó
            if (infoText != null)
            {
                infoText.text = $"Ready to play!\nDifficulty: {GetDifficultyName()}\nNotes: {noteCount}\n\nClick PLAY to start";
                infoText.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
        
        void OnPlayClicked()
        {
            if (string.IsNullOrEmpty(selectedMusicFile))
            {
                if (infoText != null)
                {
                    infoText.text = "Please select a music file first!";
                    infoText.color = Color.red;
                }
                return;
            }
            
            Debug.Log("[BeatmapSelector3D] Loading gameplay scene...");
            
            // Load gameplay scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}
