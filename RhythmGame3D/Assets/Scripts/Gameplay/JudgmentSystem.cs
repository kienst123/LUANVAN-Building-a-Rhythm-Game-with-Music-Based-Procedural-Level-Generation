using UnityEngine;

namespace RhythmGame3D.Gameplay
{
    /// <summary>
    /// Judgment result for a note hit
    /// </summary>
    public class JudgmentResult
    {
        public string judgment;          // "Perfect", "Great", "Good", "Miss"
        public float timingDifference;   // seconds (positive = late, negative = early)
        public int score;                // Points earned
        public bool isHit;               // Successfully hit or missed
        
        public JudgmentResult(string judgment, float timingDiff, int score, bool hit)
        {
            this.judgment = judgment;
            this.timingDifference = timingDiff;
            this.score = score;
            this.isHit = hit;
        }
    }
    
    /// <summary>
    /// Judgment system for rhythm game
    /// Calculates accuracy, combo, score based on hit timing
    /// </summary>
    public class JudgmentSystem : MonoBehaviour
    {
        [Header("Timing Windows (seconds)")]
        public float perfectWindow = 0.040f;  // ±40ms
        public float greatWindow = 0.080f;    // ±80ms
        public float goodWindow = 0.120f;     // ±120ms
        public float missWindow = 0.180f;     // ±180ms
        
        [Header("Scoring")]
        public int perfectScore = 300;
        public int greatScore = 200;
        public int goodScore = 100;
        public int missScore = 0;
        
        // Stats
        public int totalScore { get; private set; }
        public int combo { get; private set; }
        public int maxCombo { get; private set; }
        public float accuracy { get; private set; }
        
        // Judgment counts
        public int perfectCount { get; private set; }
        public int greatCount { get; private set; }
        public int goodCount { get; private set; }
        public int missCount { get; private set; }
        
        // Early/Late counts
        public int earlyCount { get; private set; }
        public int lateCount { get; private set; }
        
        private int totalNotes = 0;
        private int totalPossibleScore = 0;
        
        // Events
        public System.Action<JudgmentResult> OnJudgment;
        public System.Action<int> OnComboChange;
        
        void Awake()
        {
            ResetStats();
        }
        
        /// <summary>
        /// Judge a note hit
        /// </summary>
        public JudgmentResult Judge(float timingDifference)
        {
            totalNotes++;
            
            float absDiff = Mathf.Abs(timingDifference);
            string judgment;
            int score;
            bool isHit = true;
            
            // Determine judgment
            if (absDiff <= perfectWindow)
            {
                judgment = "Perfect";
                score = perfectScore;
                perfectCount++;
                combo++;
            }
            else if (absDiff <= greatWindow)
            {
                judgment = "Great";
                score = greatScore;
                greatCount++;
                combo++;
            }
            else if (absDiff <= goodWindow)
            {
                judgment = "Good";
                score = goodScore;
                goodCount++;
                combo++;
            }
            else
            {
                judgment = "Miss";
                score = missScore;
                missCount++;
                combo = 0;
                isHit = false;
            }
            
            // Track early/late (only for hits, not misses)
            if (isHit && absDiff > 0.005f) // Ignore very small differences (±5ms)
            {
                if (timingDifference < 0)
                    earlyCount++;
                else
                    lateCount++;
            }
            
            // Update max combo
            if (combo > maxCombo)
                maxCombo = combo;
            
            // Update score
            totalScore += score;
            totalPossibleScore += perfectScore;
            
            // Calculate accuracy
            if (totalPossibleScore > 0)
            {
                accuracy = (float)totalScore / totalPossibleScore * 100f;
            }
            
            // Create result
            JudgmentResult result = new JudgmentResult(judgment, timingDifference, score, isHit);
            
            // Fire events
            OnJudgment?.Invoke(result);
            OnComboChange?.Invoke(combo);
            
            Debug.Log($"[JudgmentSystem] {judgment} ({timingDifference * 1000f:F1}ms), Combo: {combo}, Acc: {accuracy:F2}%");
            
            return result;
        }
        
        /// <summary>
        /// Judge a miss (note passed without hit)
        /// </summary>
        public JudgmentResult JudgeMiss()
        {
            return Judge(missWindow + 0.1f);  // Beyond miss window
        }
        
        /// <summary>
        /// Reset all statistics
        /// </summary>
        public void ResetStats()
        {
            totalScore = 0;
            combo = 0;
            maxCombo = 0;
            accuracy = 100f;
            
            perfectCount = 0;
            greatCount = 0;
            goodCount = 0;
            missCount = 0;
            
            earlyCount = 0;
            lateCount = 0;
            
            totalNotes = 0;
            totalPossibleScore = 0;
        }
        
        /// <summary>
        /// Get grade based on accuracy
        /// </summary>
        public string GetGrade()
        {
            if (accuracy >= 95f) return "SS";
            if (accuracy >= 90f) return "S";
            if (accuracy >= 80f) return "A";
            if (accuracy >= 70f) return "B";
            if (accuracy >= 60f) return "C";
            return "D";
        }
        
        /// <summary>
        /// Get statistics summary
        /// </summary>
        public override string ToString()
        {
            return $"Score: {totalScore}\n" +
                   $"Accuracy: {accuracy:F2}%\n" +
                   $"Max Combo: {maxCombo}\n" +
                   $"Perfect: {perfectCount}, Great: {greatCount}, Good: {goodCount}, Miss: {missCount}\n" +
                   $"Early: {earlyCount}, Late: {lateCount}";
        }
    }
}
