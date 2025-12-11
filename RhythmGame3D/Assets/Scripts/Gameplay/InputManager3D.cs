using UnityEngine;

namespace RhythmGame3D.Gameplay
{
    /// <summary>
    /// Handles player input for 4-key rhythm game (D, F, J, K)
    /// </summary>
    public class InputManager3D : MonoBehaviour
    {
        [Header("Key Bindings")]
        public KeyCode[] laneKeys = new KeyCode[] {
            KeyCode.D,  // Lane 0
            KeyCode.F,  // Lane 1
            KeyCode.J,  // Lane 2
            KeyCode.K   // Lane 3
        };
        
        [Header("References")]
        public NoteSpawner3D noteSpawner;
        public JudgmentSystem judgmentSystem;
        
        [Header("Settings")]
        public float hitWindow = 0.18f;  // Maximum timing window for any hit
        public float emptyPressHealthPenalty = 2f;  // Health lost for pressing with no note nearby
        
        // Event for empty press (no note)
        public System.Action OnEmptyPress;
        
        private float currentSongTime;
        
        void Update()
        {
            // Check input for each lane
            for (int lane = 0; lane < laneKeys.Length; lane++)
            {
                if (Input.GetKeyDown(laneKeys[lane]))
                {
                    OnLaneInput(lane);
                }
            }
        }
        
        /// <summary>
        /// Handle input for a specific lane
        /// </summary>
        void OnLaneInput(int lane)
        {
            if (noteSpawner == null || judgmentSystem == null)
            {
                Debug.LogWarning("[InputManager3D] Missing references!");
                return;
            }
            
            // Get closest note in this lane
            NoteController3D note = noteSpawner.GetClosestNoteInLane(lane, 0f, 10f);
            
            if (note == null)
            {
                Debug.Log($"[InputManager3D] No note found in lane {lane} - EMPTY PRESS PENALTY");
                
                // Fire empty press event (for health penalty)
                OnEmptyPress?.Invoke();
                
                return;
            }
            
            // Check timing
            float timingDiff = note.GetTimingDifference(currentSongTime);
            
            if (Mathf.Abs(timingDiff) <= hitWindow)
            {
                // Within hit window - judge it
                JudgmentResult result = judgmentSystem.Judge(timingDiff);
                
                // Mark note as hit
                note.OnHit(result.judgment);
                
                // Remove from spawner tracking
                noteSpawner.RemoveNote(note);
                
                Debug.Log($"[InputManager3D] Lane {lane} hit: {result.judgment}");
            }
            else
            {
                Debug.Log($"[InputManager3D] Lane {lane} - note too far ({timingDiff * 1000f:F1}ms) - EMPTY PRESS PENALTY");
                
                // Note exists but too far away - treat as empty press
                OnEmptyPress?.Invoke();
            }
        }
        
        /// <summary>
        /// Update current song time (call from GameManager)
        /// </summary>
        public void UpdateSongTime(float time)
        {
            currentSongTime = time;
        }
        
        /// <summary>
        /// Enable/disable input
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            this.enabled = enabled;
        }
    }
}
