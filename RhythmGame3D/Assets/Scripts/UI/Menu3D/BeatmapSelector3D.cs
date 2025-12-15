using UnityEngine;
using TMPro;
using RhythmGame3D.Beatmap;
using RhythmGame3D.UI;
using System.IO;
using System.Collections;

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
        
        [Header("ML Generation")]
        public bool useMLGeneration = true; // ✅ BẬT ML! Python test THÀNH CÔNG!
        public float mlTimeout = 420f; // 7 phút timeout (ML full song mất ~3-6 phút, changed from 180f)
        
        private string selectedMusicFile = "";
        private BeatmapData generatedBeatmap;
        private int currentDifficulty = 1; // 0=Easy, 1=Normal, 2=Hard (mặc định Normal)
        private PythonMLBridge mlBridge;
        private bool isGenerating = false;
        
        void Start()
        {
            SetupUI();
            
            // Initialize ML bridge
            mlBridge = new PythonMLBridge();
            mlBridge.OnProgress += OnMLProgress;
            mlBridge.OnComplete += OnMLComplete;
            mlBridge.OnError += OnMLError;
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
            if (useMLGeneration && !isGenerating)
            {
                // Thử dùng ML generation trước
                StartCoroutine(GenerateBeatmapML(musicPath));
            }
            else
            {
                // Fallback: Simple generation
                StartCoroutine(GenerateBeatmapCoroutine(musicPath));
            }
        }
        
        System.Collections.IEnumerator GenerateBeatmapCoroutine(string musicPath)
        {
            // Load audio clip
            string fileUrl = "file://" + musicPath;
            
            #if UNITY_2017_1_OR_NEWER
            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.UNKNOWN))
            {
                yield return www.SendWebRequest();
                
                float songDuration = 90f; // Default fallback
                
                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null)
                    {
                        songDuration = clip.length;
                        Debug.Log($"[BeatmapSelector3D] Audio duration from clip: {songDuration:F2}s");
                    }
                }
                else
                {
                    Debug.LogWarning($"[BeatmapSelector3D] Could not load audio, using default duration: {songDuration}s");
                }
            #else
            WWW www = new WWW(fileUrl);
            yield return www;
            
            float songDuration = 90f;
            if (www.GetAudioClip() != null)
            {
                songDuration = www.GetAudioClip().length;
            }
            #endif
            
            // Simple beatmap generation
            generatedBeatmap = new BeatmapData();
            generatedBeatmap.audioFilename = Path.GetFileName(musicPath);
            generatedBeatmap.title = Path.GetFileNameWithoutExtension(musicPath);
            generatedBeatmap.artist = "Unknown";
            generatedBeatmap.creator = "Auto-Generated";
            
            Debug.Log($"[BeatmapSelector3D] Song duration: {songDuration:F2}s");
            
            // Generate simple pattern (every beat)
            float bpm = 120f;
            float beatDuration = 60f / bpm; // 0.5 giây mỗi beat
            
            // KHOẢNG CÁCH GIỮA CÁC NOTE DỰA TRÊN ĐỘ KHÓ
            float noteSpacing;
            switch (currentDifficulty)
            {
                case 0: // EASY
                    noteSpacing = 1.0f; // 1 beat spacing (0.5s)
                    break;
                case 1: // NORMAL
                    noteSpacing = 0.75f; // 0.75 beat spacing (0.375s)
                    break;
                case 2: // HARD
                    noteSpacing = 0.5f; // 0.5 beat spacing (0.25s)
                    break;
                default:
                    noteSpacing = 0.75f;
                    break;
            }
            
            // TÍNH SỐ NOTE DỰA TRÊN ĐỘ DÀI BÀI HÁT
            float timePerNote = beatDuration * noteSpacing;
            int noteCount = Mathf.FloorToInt(songDuration / timePerNote);
            
            Debug.Log($"[BeatmapSelector3D] Generating {noteCount} notes for {songDuration:F2}s song ({GetDifficultyName()})");
            
            // ĐỂ TRÁNH 2 NOTE CÙNG LANE LIÊN TIẾP, dùng lastLane
            int lastLane = -1;
            
            for (int i = 0; i < noteCount; i++)
            {
                // RANDOM LANE (0-3), nhưng tránh trùng với note trước
                int lane;
                int maxAttempts = 10;
                int attempts = 0;
                
                do
                {
                    lane = Random.Range(0, 4);
                    attempts++;
                } while (lane == lastLane && attempts < maxAttempts);
                
                lastLane = lane;
                
                int xPos = 64 + (lane * 128); // osu!mania 4K positions: 64, 192, 320, 448
                int noteTime = Mathf.RoundToInt(i * beatDuration * noteSpacing * 1000); // Convert to milliseconds
                
                // KHÔNG TẠO LONG NOTE trong auto-generate (chỉ tap notes)
                // Long note phức tạp và cần beatmap .osu thật để hoạt động tốt
                int noteType = 1; // Chỉ tạo tap note (type 1)
                
                HitObject note = new HitObject(xPos, 192, noteTime, noteType, 0);
                
                generatedBeatmap.AddHitObject(note);
            }
            
            // Store for gameplay
            BeatmapStorage.currentBeatmap = generatedBeatmap;
            BeatmapStorage.currentMusicPath = musicPath;
            
            Debug.Log($"[BeatmapSelector3D] Generated beatmap with {noteCount} notes ({GetDifficultyName()})");
            
            // Cập nhật info text với thông tin độ khó
            if (infoText != null)
            {
                infoText.text = $"Ready to play!\nDifficulty: {GetDifficultyName()}\nNotes: {noteCount}\nDuration: ~{songDuration:F0}s\n\nClick PLAY to start";
                infoText.color = new Color(0.7f, 0.7f, 0.7f);
            }
            
            #if UNITY_2017_1_OR_NEWER
            }
            #endif
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
        
        // ========== ML GENERATION METHODS ==========
        
        /// <summary>
        /// Generate beatmap sử dụng ML model (BEaRT)
        /// </summary>
        System.Collections.IEnumerator GenerateBeatmapML(string musicPath)
        {
            isGenerating = true;
            
            Debug.Log($"[BeatmapSelector3D] Starting ML generation for: {Path.GetFileName(musicPath)}");
            
            // Update UI - Đang generate
            if (infoText != null)
            {
                infoText.text = "Generating beatmap with AI...\nPlease wait (30-60 seconds)";
                infoText.color = new Color(1f, 1f, 0f); // Yellow
            }
            
            // Map Unity difficulty (0-2) → ML difficulty (0.0-1.0)
            float mlDifficulty = currentDifficulty switch
            {
                0 => 0.33f, // Easy
                1 => 0.50f, // Normal
                2 => 0.75f, // Hard
                _ => 0.50f
            };
            
            // Temporary output path
            string tempDir = Path.Combine(Application.temporaryCachePath, "ML_Beatmaps");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
            
            string outputFileName = $"{Path.GetFileNameWithoutExtension(musicPath)}_{GetDifficultyName()}.osu";
            string outputPath = Path.Combine(tempDir, outputFileName);
            
            Debug.Log($"[BeatmapSelector3D] ML output path: {outputPath}");
            
            // Start ML generation
            bool mlSuccess = false;
            yield return mlBridge.GenerateBeatmap(musicPath, mlDifficulty, outputPath, mlTimeout);
            
            // Check if ML generation succeeded
            if (File.Exists(outputPath))
            {
                Debug.Log($"[BeatmapSelector3D] ML generation successful! Parsing beatmap...");
                
                // Parse generated .osu file
                try
                {
                    generatedBeatmap = BeatmapParser.ParseBeatmap(outputPath);
                    
                    // Override audio filename with original path
                    generatedBeatmap.audioFilename = Path.GetFileName(musicPath);
                    
                    // Store for gameplay
                    BeatmapStorage.currentBeatmap = generatedBeatmap;
                    BeatmapStorage.currentMusicPath = musicPath;
                    
                    mlSuccess = true;
                    
                    // Update UI - Success
                    if (infoText != null)
                    {
                        int noteCount = generatedBeatmap.hitObjects.Count;
                        infoText.text = $"✅ AI Beatmap Generated!\nDifficulty: {GetDifficultyName()}\nNotes: {noteCount}\n\nClick PLAY to start";
                        infoText.color = new Color(0f, 1f, 0.5f); // Green
                    }
                    
                    Debug.Log($"[BeatmapSelector3D] ✅ ML beatmap parsed: {generatedBeatmap.hitObjects.Count} notes");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[BeatmapSelector3D] Failed to parse ML beatmap: {e.Message}");
                    mlSuccess = false;
                }
            }
            
            // Fallback nếu ML fail
            if (!mlSuccess)
            {
                Debug.LogWarning($"[BeatmapSelector3D] ML generation failed, using simple generation");
                
                if (infoText != null)
                {
                    infoText.text = "AI generation failed\nUsing simple generation...";
                    infoText.color = new Color(1f, 0.5f, 0f); // Orange
                }
                
                yield return new WaitForSeconds(1f);
                
                // Fallback to simple generation
                yield return GenerateBeatmapCoroutine(musicPath);
            }
            
            isGenerating = false;
        }
        
        /// <summary>
        /// ML progress callback
        /// </summary>
        void OnMLProgress(string stage, string message)
        {
            Debug.Log($"[ML Progress] {stage}: {message}");
            
            if (infoText != null)
            {
                string displayMessage = stage switch
                {
                    "import" => "Loading AI libraries...",
                    "validate" => "Validating input...",
                    "download" => "Downloading AI model...",
                    "load_model" => "Loading neural network...",
                    "generate" => "AI is analyzing music...",
                    "convert" => "Converting to beatmap...",
                    "verify" => "Verifying output...",
                    "complete" => "Generation complete!",
                    _ => message
                };
                
                infoText.text = $"🤖 AI Generation\n{displayMessage}\n\nPlease wait...";
                infoText.color = new Color(0f, 0.94f, 1f); // Cyan
            }
        }
        
        /// <summary>
        /// ML complete callback
        /// </summary>
        void OnMLComplete(string outputPath)
        {
            Debug.Log($"[ML Complete] Beatmap saved to: {outputPath}");
        }
        
        /// <summary>
        /// ML error callback
        /// </summary>
        void OnMLError(string errorMessage)
        {
            Debug.LogError($"[ML Error] {errorMessage}");
            
            if (infoText != null)
            {
                infoText.text = $"❌ AI generation failed:\n{errorMessage}\n\nUsing simple generation...";
                infoText.color = new Color(1f, 0f, 0f); // Red
            }
        }
    }
}
