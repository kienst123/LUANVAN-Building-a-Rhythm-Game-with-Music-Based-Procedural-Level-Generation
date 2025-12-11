using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace RhythmGame3D.UI
{
    /// <summary>
    /// Manages the main menu navigation and panels
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainPanel;
        public GameObject beatmapPanel;
        public GameObject settingsPanel;
        
        [Header("Main Menu Buttons")]
        public Button playButton;
        public Button beatmapButton;
        public Button settingsButton;
        public Button quitButton;
        
        [Header("Settings")]
        public string gameplaySceneName = "GameScene";
        
        void Start()
        {
            // Setup button listeners
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);
            
            if (beatmapButton != null)
                beatmapButton.onClick.AddListener(OnBeatmapClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
            
            // Show main panel by default
            ShowMainPanel();
        }
        
        /// <summary>
        /// Play button - Load gameplay scene
        /// </summary>
        void OnPlayClicked()
        {
            // Check if beatmap is selected
            if (BeatmapSelector.currentBeatmap == null || string.IsNullOrEmpty(BeatmapSelector.currentMusicPath))
            {
                Debug.LogWarning("[MenuManager] No beatmap selected! Redirecting to beatmap selection...");
                
                // Show notification panel
                StartCoroutine(ShowNoBeatmapNotification());
                
                return;
            }
            
            Debug.Log("[MenuManager] Loading gameplay scene...");
            SceneManager.LoadScene(gameplaySceneName);
        }
        
        /// <summary>
        /// Show notification that no beatmap is selected
        /// </summary>
        System.Collections.IEnumerator ShowNoBeatmapNotification()
        {
            // Create temporary notification panel
            GameObject notificationPanel = CreateNotificationPanel(
                "NO BEATMAP SELECTED",
                "Please select a music file first!\n\nClick OK to go to Beatmap Selection.",
                "OK"
            );
            
            // Wait for user to click OK
            bool clicked = false;
            Button okButton = notificationPanel.GetComponentInChildren<Button>();
            if (okButton != null)
            {
                okButton.onClick.AddListener(() => clicked = true);
            }
            
            // Wait for click
            while (!clicked)
            {
                yield return null;
            }
            
            // Destroy notification
            Destroy(notificationPanel);
            
            // Redirect to beatmap panel
            ShowBeatmapPanel();
        }
        
        /// <summary>
        /// Create a simple notification panel
        /// </summary>
        GameObject CreateNotificationPanel(string title, string message, string buttonText)
        {
            // Find canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MenuManager] No Canvas found!");
                return null;
            }
            
            // Create panel
            GameObject panel = new GameObject("NotificationPanel");
            panel.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.sizeDelta = Vector2.zero;
            
            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.8f);
            
            // Content box
            GameObject content = new GameObject("Content");
            content.transform.SetParent(panel.transform, false);
            
            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.pivot = new Vector2(0.5f, 0.5f);
            contentRT.sizeDelta = new Vector2(500, 300);
            
            Image contentBg = content.AddComponent<Image>();
            contentBg.color = new Color(0.2f, 0.2f, 0.3f);
            
            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(content.transform, false);
            
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1);
            titleRT.anchorMax = new Vector2(0.5f, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.anchoredPosition = new Vector2(0, -20);
            titleRT.sizeDelta = new Vector2(450, 50);
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.3f, 0.3f); // Red
            
            // Message
            GameObject messageObj = new GameObject("Message");
            messageObj.transform.SetParent(content.transform, false);
            
            RectTransform messageRT = messageObj.AddComponent<RectTransform>();
            messageRT.anchorMin = new Vector2(0.5f, 0.5f);
            messageRT.anchorMax = new Vector2(0.5f, 0.5f);
            messageRT.pivot = new Vector2(0.5f, 0.5f);
            messageRT.anchoredPosition = new Vector2(0, 20);
            messageRT.sizeDelta = new Vector2(450, 120);
            
            TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = message;
            messageText.fontSize = 20;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.color = Color.white;
            
            // OK Button
            GameObject buttonObj = new GameObject("OKButton");
            buttonObj.transform.SetParent(content.transform, false);
            
            RectTransform buttonRT = buttonObj.AddComponent<RectTransform>();
            buttonRT.anchorMin = new Vector2(0.5f, 0);
            buttonRT.anchorMax = new Vector2(0.5f, 0);
            buttonRT.pivot = new Vector2(0.5f, 0);
            buttonRT.anchoredPosition = new Vector2(0, 20);
            buttonRT.sizeDelta = new Vector2(200, 50);
            
            Image buttonImg = buttonObj.AddComponent<Image>();
            buttonImg.color = new Color(0.2f, 0.7f, 0.2f);
            
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImg;
            
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform buttonTextRT = buttonTextObj.AddComponent<RectTransform>();
            buttonTextRT.anchorMin = Vector2.zero;
            buttonTextRT.anchorMax = Vector2.one;
            buttonTextRT.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI buttonTextTMP = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonTextTMP.text = buttonText;
            buttonTextTMP.fontSize = 24;
            buttonTextTMP.fontStyle = FontStyles.Bold;
            buttonTextTMP.alignment = TextAlignmentOptions.Center;
            buttonTextTMP.color = Color.white;
            
            return panel;
        }
        
        /// <summary>
        /// Beatmap button - Show beatmap selection panel
        /// </summary>
        void OnBeatmapClicked()
        {
            Debug.Log("[MenuManager] Opening beatmap panel...");
            ShowBeatmapPanel();
        }
        
        /// <summary>
        /// Settings button - Show settings panel
        /// </summary>
        void OnSettingsClicked()
        {
            Debug.Log("[MenuManager] Opening settings panel...");
            ShowSettingsPanel();
        }
        
        /// <summary>
        /// Quit button - Exit game
        /// </summary>
        void OnQuitClicked()
        {
            Debug.Log("[MenuManager] Quitting game...");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        /// <summary>
        /// Show main menu panel
        /// </summary>
        public void ShowMainPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (beatmapPanel != null) beatmapPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }
        
        /// <summary>
        /// Show beatmap selection panel
        /// </summary>
        public void ShowBeatmapPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (beatmapPanel != null) beatmapPanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }
        
        /// <summary>
        /// Show settings panel
        /// </summary>
        public void ShowSettingsPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (beatmapPanel != null) beatmapPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }
    }
}
