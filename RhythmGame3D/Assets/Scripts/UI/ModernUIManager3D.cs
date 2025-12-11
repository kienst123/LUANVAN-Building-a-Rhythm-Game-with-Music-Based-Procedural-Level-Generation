using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace RhythmGame3D.UI
{
    /// <summary>
    /// Modern UI Manager for 3D Rhythm Game
    /// Minimalist design similar to Web osu!mania
    /// </summary>
    public class ModernUIManager3D : MonoBehaviour
    {
        [Header("Score Display")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI comboText;
        
        [Header("Judgment Display")]
        public TextMeshProUGUI judgmentText;
        public TextMeshProUGUI earlyLateText;
        public CanvasGroup judgmentGroup;
        
        [Header("Bottom Info")]
        public TextMeshProUGUI accuracyText;
        public Slider healthBar;
        public TextMeshProUGUI timeText;
        
        [Header("Game Over Panel")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI gameOverScoreText;
        public TextMeshProUGUI gameOverAccuracyText;
        public TextMeshProUGUI gameOverStatsText;
        public Button restartButton;
        public Button exitButton;
        
        [Header("Settings")]
        public float judgmentFadeDuration = 0.5f;
        public float comboScalePulse = 1.2f;
        
        private Coroutine judgmentCoroutine;
        private int currentScore = 0;
        private int currentCombo = 0;
        private float currentAccuracy = 100f;
        private float currentHealth = 100f;
        
        void Awake()
        {
            InitializeUI();
        }
        
        void InitializeUI()
        {
            if (scoreText != null) scoreText.text = "000000";
            if (comboText != null) comboText.text = "0x";
            if (accuracyText != null) accuracyText.text = "100.00%";
            if (judgmentText != null) judgmentText.text = "";
            if (earlyLateText != null) earlyLateText.text = "";
            
            // Hide game over panel at start
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            
            if (healthBar != null)
            {
                healthBar.value = 1f;
            }
            
            if (judgmentGroup != null)
            {
                judgmentGroup.alpha = 0f;
            }
            
            // Setup game over button listeners
            SetupGameOverButtons();
        }
        
        /// <summary>
        /// Setup game over button listeners
        /// </summary>
        void SetupGameOverButtons()
        {
            // Find GameManager3D in scene
            var gameManager = FindObjectOfType<RhythmGame3D.Core.GameManager3D>();
            
            if (gameManager == null)
            {
                Debug.LogWarning("[ModernUIManager3D] GameManager3D not found! Buttons won't work.");
                return;
            }
            
            // Setup Restart button
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => gameManager.RestartGame());
                Debug.Log("[ModernUIManager3D] Restart button listener added");
            }
            
            // Setup Exit button
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(() => gameManager.ExitGame());
                Debug.Log("[ModernUIManager3D] Exit button listener added");
            }
        }
        
        /// <summary>
        /// Update score display
        /// </summary>
        public void UpdateScore(int score)
        {
            currentScore = score;
            if (scoreText != null)
            {
                scoreText.text = score.ToString("D6");  // 6 digits with leading zeros
            }
        }
        
        /// <summary>
        /// Update combo display
        /// </summary>
        public void UpdateCombo(int combo)
        {
            currentCombo = combo;
            if (comboText != null)
            {
                comboText.text = combo > 0 ? $"{combo}x" : "";
                
                // Pulse animation on combo milestones
                if (combo > 0 && combo % 50 == 0)
                {
                    StartCoroutine(PulseCombo());
                }
            }
        }
        
        /// <summary>
        /// Show judgment result
        /// </summary>
        public void ShowJudgment(string judgment, float timingDifference)
        {
            if (judgmentText == null) return;
            
            // Set judgment text
            judgmentText.text = judgment;
            
            // Set color based on judgment
            Color judgmentColor = GetJudgmentColor(judgment);
            judgmentText.color = judgmentColor;
            
            // Show early/late indicator
            if (earlyLateText != null && judgment != "Miss")
            {
                if (Mathf.Abs(timingDifference) < 0.005f) // Ignore very small differences (±5ms)
                {
                    earlyLateText.text = "";
                }
                else if (timingDifference < 0)
                {
                    earlyLateText.text = "EARLY";
                    earlyLateText.color = new Color(0.3f, 0.7f, 1f);  // Light Blue
                }
                else
                {
                    earlyLateText.text = "LATE";
                    earlyLateText.color = new Color(1f, 0.5f, 0.3f);  // Orange
                }
            }
            else if (earlyLateText != null)
            {
                earlyLateText.text = ""; // Clear on Miss
            }
            
            // Animate judgment
            if (judgmentCoroutine != null)
                StopCoroutine(judgmentCoroutine);
            judgmentCoroutine = StartCoroutine(AnimateJudgment());
        }
        
        /// <summary>
        /// Update accuracy display
        /// </summary>
        public void UpdateAccuracy(float accuracy)
        {
            currentAccuracy = accuracy;
            if (accuracyText != null)
            {
                accuracyText.text = $"{accuracy:F2}%";
                
                // Color based on accuracy
                if (accuracy >= 95f)
                    accuracyText.color = new Color(1f, 0.84f, 0f);      // Gold
                else if (accuracy >= 90f)
                    accuracyText.color = new Color(0.4f, 1f, 0.4f);     // Green
                else if (accuracy >= 80f)
                    accuracyText.color = new Color(1f, 1f, 0.4f);       // Yellow
                else
                    accuracyText.color = Color.white;
            }
        }
        
        /// <summary>
        /// Update health bar
        /// </summary>
        public void UpdateHealth(float normalizedHealth)
        {
            // normalizedHealth is 0-1
            normalizedHealth = Mathf.Clamp01(normalizedHealth);
            currentHealth = normalizedHealth * 100f;
            
            if (healthBar != null)
            {
                healthBar.value = normalizedHealth;  // Slider expects 0-1
                
                // Color gradient: red -> yellow -> green
                Color healthColor;
                if (normalizedHealth > 0.5f)
                {
                    // 50%-100%: yellow to green
                    float t = (normalizedHealth - 0.5f) / 0.5f;
                    healthColor = Color.Lerp(Color.yellow, Color.green, t);
                }
                else
                {
                    // 0%-50%: red to yellow
                    float t = normalizedHealth / 0.5f;
                    healthColor = Color.Lerp(Color.red, Color.yellow, t);
                }
                
                // Update fill color
                if (healthBar.fillRect != null)
                {
                    Image fillImage = healthBar.fillRect.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        fillImage.color = healthColor;
                    }
                }
                
                Debug.Log($"[ModernUIManager3D] Health updated: {normalizedHealth:F2} ({currentHealth:F1}%), Color: {healthColor}");
            }
        }
        
        /// <summary>
        /// Update time display
        /// </summary>
        public void UpdateTime(float currentTime, float totalTime)
        {
            if (timeText != null)
            {
                string currentStr = FormatTime(currentTime);
                string totalStr = FormatTime(totalTime);
                timeText.text = $"{currentStr} / {totalStr}";
            }
        }
        
        string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes}:{secs:D2}";
        }
        
        Color GetJudgmentColor(string judgment)
        {
            switch (judgment)
            {
                case "Perfect":
                    return new Color(1f, 0.84f, 0f);     // Gold
                case "Great":
                    return new Color(0.4f, 1f, 0.4f);    // Green
                case "Good":
                    return new Color(0.4f, 0.7f, 1f);    // Blue
                case "Miss":
                    return new Color(1f, 0.3f, 0.3f);    // Red
                default:
                    return Color.white;
            }
        }
        
        IEnumerator AnimateJudgment()
        {
            if (judgmentGroup == null) yield break;
            
            // Fade in quickly
            judgmentGroup.alpha = 1f;
            judgmentGroup.transform.localScale = Vector3.one * 1.2f;
            
            // Wait a bit
            yield return new WaitForSeconds(0.1f);
            
            // Scale down
            float elapsed = 0f;
            while (elapsed < judgmentFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / judgmentFadeDuration;
                judgmentGroup.alpha = 1f - t;
                judgmentGroup.transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, t);
                yield return null;
            }
            
            judgmentGroup.alpha = 0f;
        }
        
        IEnumerator PulseCombo()
        {
            if (comboText == null) yield break;
            
            float duration = 0.15f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = Mathf.Lerp(comboScalePulse, 1f, t);
                comboText.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            
            comboText.transform.localScale = Vector3.one;
        }
        
        /// <summary>
        /// Show game over panel with final stats
        /// </summary>
        public void ShowGameOver(int finalScore, float finalAccuracy, int maxCombo, int perfects, int greats, int goods, int misses, int early = 0, int late = 0)
        {
            if (gameOverPanel == null)
            {
                Debug.LogError("[ModernUIManager3D] Game Over Panel not assigned!");
                return;
            }
            
            // Show panel
            gameOverPanel.SetActive(true);
            
            // Update texts
            if (gameOverScoreText != null)
                gameOverScoreText.text = finalScore.ToString("D6");
            
            if (gameOverAccuracyText != null)
                gameOverAccuracyText.text = $"{finalAccuracy:F2}%";
            
            if (gameOverStatsText != null)
            {
                gameOverStatsText.text = $"Perfect: {perfects}\n" +
                                        $"Great: {greats}\n" +
                                        $"Good: {goods}\n" +
                                        $"Miss: {misses}\n" +
                                        $"Max Combo: {maxCombo}\n" +
                                        $"\n" +
                                        $"<color=#5DADE2>Early: {early}</color>  " +
                                        $"<color=#F39C12>Late: {late}</color>";
            }
            
            Debug.Log("[ModernUIManager3D] Game Over displayed");
        }
    }
}
