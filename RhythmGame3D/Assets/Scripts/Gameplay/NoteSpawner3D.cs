using System.Collections.Generic;
using UnityEngine;
using RhythmGame3D.Beatmap;

namespace RhythmGame3D.Gameplay
{
    /// <summary>
    /// Spawns notes in 3D space (osu!mania style)
    /// Notes move from far (positive Z) towards camera (Z = 0)
    /// </summary>
    public class NoteSpawner3D : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject tapNotePrefab;
        public GameObject longNotePrefab;
        
        [Header("Lane Configuration")]
        public int laneCount = 4;
        public float laneWidth = 1.5f;
        public float laneLength = 50f;
        
        [Header("Spawn Settings")]
        public float spawnDistance = 40f;  // Z position where notes spawn
        public float hitPosition = 0f;     // Z position where notes should be hit
        public float approachTime = 2f;    // Time for note to reach hit position (seconds)
        
        [Header("Visual")]
        public Material laneMaterial;
        public Color[] laneColors = new Color[] { 
            new Color(0.8f, 0.8f, 0.8f, 0.3f),  // Lane 0 - Light gray
            new Color(0.4f, 0.7f, 1f, 0.3f),     // Lane 1 - Light blue
            new Color(0.4f, 0.7f, 1f, 0.3f),     // Lane 2 - Light blue
            new Color(0.8f, 0.8f, 0.8f, 0.3f)   // Lane 3 - Light gray
        };
        
        [Header("Note Colors")]
        public Color[] noteColors = new Color[] {
            new Color(1f, 0.2f, 0.2f, 1f),      // Bright Red
            new Color(0.2f, 1f, 0.2f, 1f),      // Bright Green
            new Color(0.3f, 0.6f, 1f, 1f),      // Bright Blue
            new Color(1f, 1f, 0.2f, 1f)         // Bright Yellow
        };
        
        [Header("References")]
        public JudgmentSystem judgmentSystem;  // For triggering miss judgments
        
        // Private data
        private Transform[] lanes;
        private BeatmapData currentBeatmap;
        private List<HitObject> notesToSpawn;
        private int nextNoteIndex = 0;
        
        // Active notes tracking
        public List<NoteController3D> activeNotes = new List<NoteController3D>();
        public Dictionary<int, List<NoteController3D>> laneNotes = new Dictionary<int, List<NoteController3D>>();
        
        // Calculated speed
        private float noteSpeed;
        
        void Awake()
        {
            // Initialize lane notes dictionary
            for (int i = 0; i < laneCount; i++)
            {
                laneNotes[i] = new List<NoteController3D>();
            }
            
            // Calculate note speed based on approach time
            noteSpeed = (spawnDistance - hitPosition) / approachTime;
            
            Debug.Log($"[NoteSpawner3D] Initialized. Speed: {noteSpeed} units/sec");
        }
        
        void Start()
        {
            CreateLanes();
        }
        
        /// <summary>
        /// Creates 4 lanes in 3D space
        /// </summary>
        void CreateLanes()
        {
            lanes = new Transform[laneCount];
            float startX = -(laneCount - 1) * laneWidth / 2f;
            
            for (int i = 0; i < laneCount; i++)
            {
                GameObject laneObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                laneObj.name = $"Lane_{i}";
                laneObj.transform.parent = transform;
                
                // Position: centered at X, extending along Z axis
                float xPos = startX + (i * laneWidth);
                laneObj.transform.position = new Vector3(xPos, 0, laneLength / 2f);
                laneObj.transform.localScale = new Vector3(laneWidth * 0.95f, 0.1f, laneLength);
                
                // Apply material and color
                MeshRenderer renderer = laneObj.GetComponent<MeshRenderer>();
                if (laneMaterial != null)
                {
                    renderer.material = laneMaterial;
                }
                renderer.material.color = laneColors[i % laneColors.Length];
                
                // Remove collider (not needed)
                Destroy(laneObj.GetComponent<Collider>());
                
                lanes[i] = laneObj.transform;
            }
            
            Debug.Log($"[NoteSpawner3D] Created {laneCount} lanes");
        }
        
        /// <summary>
        /// Load beatmap data
        /// </summary>
        public void LoadBeatmap(BeatmapData beatmap)
        {
            currentBeatmap = beatmap;
            notesToSpawn = new List<HitObject>(beatmap.hitObjects);
            nextNoteIndex = 0;
            
            Debug.Log($"[NoteSpawner3D] Loaded beatmap: {beatmap.hitObjects.Count} notes");
        }
        
        /// <summary>
        /// Update spawning (call from GameManager with current song time)
        /// </summary>
        public void UpdateSpawning(float currentTime)
        {
            if (notesToSpawn == null || nextNoteIndex >= notesToSpawn.Count)
                return;
            
            // Spawn notes that should appear now
            while (nextNoteIndex < notesToSpawn.Count)
            {
                HitObject hitObject = notesToSpawn[nextNoteIndex];
                float spawnTime = (hitObject.time / 1000f) - approachTime;
                
                if (currentTime >= spawnTime)
                {
                    SpawnNote(hitObject);
                    nextNoteIndex++;
                }
                else
                {
                    break;
                }
            }
        }
        
        /// <summary>
        /// Called when a note is missed (passed hit position without being hit)
        /// </summary>
        void OnNoteMissed(NoteController3D note)
        {
            Debug.Log($"[NoteSpawner3D] OnNoteMissed called for lane {note.hitObject.lane}");
            
            // Trigger miss judgment
            if (judgmentSystem != null)
            {
                judgmentSystem.JudgeMiss();
                Debug.Log($"[NoteSpawner3D] Called JudgeMiss() for lane {note.hitObject.lane}");
            }
            else
            {
                Debug.LogError("[NoteSpawner3D] JudgmentSystem is NULL! Cannot trigger miss!");
            }
            
            // Remove the note from tracking
            RemoveNote(note);
        }
        
        /// <summary>
        /// Spawn a single note
        /// </summary>
        void SpawnNote(HitObject hitObject)
        {
            // Validate lane
            if (hitObject.lane < 0 || hitObject.lane >= laneCount)
            {
                Debug.LogWarning($"Invalid lane {hitObject.lane} for note at {hitObject.time}ms");
                return;
            }
            
            // Choose prefab
            GameObject prefab = hitObject.isLongNote ? longNotePrefab : tapNotePrefab;
            if (prefab == null)
            {
                Debug.LogError("Note prefab not assigned!");
                return;
            }
            
            // Instantiate
            GameObject noteObj = Instantiate(prefab, transform);
            NoteController3D controller = noteObj.GetComponent<NoteController3D>();
            
            if (controller == null)
            {
                Debug.LogError("Note prefab missing NoteController3D component!");
                Destroy(noteObj);
                return;
            }
            
            // Calculate spawn position
            float xPos = lanes[hitObject.lane].position.x;
            Vector3 spawnPos = new Vector3(xPos, 0.5f, spawnDistance);
            noteObj.transform.position = spawnPos;
            
            // Choose random color from noteColors array
            Color randomColor = noteColors[Random.Range(0, noteColors.Length)];
            
            // Initialize note controller with random color
            controller.Initialize(hitObject, noteSpeed, hitPosition);
            controller.SetColor(randomColor);
            
            // Subscribe to miss callback
            controller.onMissed += OnNoteMissed;
            
            // Track note
            activeNotes.Add(controller);
            laneNotes[hitObject.lane].Add(controller);
            
            Debug.Log($"[NoteSpawner3D] Spawned {(hitObject.isLongNote ? "LN" : "note")} at lane {hitObject.lane}, time {hitObject.time}ms");
        }
        
        /// <summary>
        /// Remove note from tracking
        /// </summary>
        public void RemoveNote(NoteController3D note)
        {
            activeNotes.Remove(note);
            
            if (note.hitObject != null && laneNotes.ContainsKey(note.hitObject.lane))
            {
                laneNotes[note.hitObject.lane].Remove(note);
            }
        }
        
        /// <summary>
        /// Get closest note in specific lane
        /// </summary>
        public NoteController3D GetClosestNoteInLane(int lane, float hitZ, float maxDistance)
        {
            if (!laneNotes.ContainsKey(lane) || laneNotes[lane].Count == 0)
                return null;
            
            NoteController3D closest = null;
            float minDistance = maxDistance;
            
            foreach (var note in laneNotes[lane])
            {
                if (note.isHit) continue;
                
                float distance = Mathf.Abs(note.transform.position.z - hitZ);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = note;
                }
            }
            
            return closest;
        }
        
        /// <summary>
        /// Get first unhit note in lane
        /// </summary>
        public NoteController3D GetNoteAtLane(int lane)
        {
            if (!laneNotes.ContainsKey(lane) || laneNotes[lane].Count == 0)
                return null;
            
            foreach (var note in laneNotes[lane])
            {
                if (!note.isHit)
                    return note;
            }
            
            return null;
        }
        
        /// <summary>
        /// Reset spawner
        /// </summary>
        public void ResetSpawner()
        {
            nextNoteIndex = 0;
            ClearAllNotes();
        }
        
        /// <summary>
        /// Stop spawning new notes (for game over)
        /// </summary>
        public void StopSpawning()
        {
            // Set index to end to prevent new spawns
            if (notesToSpawn != null)
            {
                nextNoteIndex = notesToSpawn.Count;
            }
            
            Debug.Log("[NoteSpawner3D] Stopped spawning notes");
        }
        
        /// <summary>
        /// Clear all active notes
        /// </summary>
        public void ClearAllNotes()
        {
            foreach (var note in activeNotes)
            {
                if (note != null)
                    Destroy(note.gameObject);
            }
            
            activeNotes.Clear();
            foreach (var kvp in laneNotes)
            {
                kvp.Value.Clear();
            }
            
            Debug.Log("[NoteSpawner3D] Cleared all notes");
        }
        
        void OnDestroy()
        {
            ClearAllNotes();
        }
        
        // Visualization in editor
        void OnDrawGizmos()
        {
            // Draw spawn line
            Gizmos.color = Color.green;
            float width = laneCount * laneWidth;
            Gizmos.DrawLine(new Vector3(-width/2, 0, spawnDistance), 
                           new Vector3(width/2, 0, spawnDistance));
            
            // Draw hit line
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(-width/2, 0, hitPosition), 
                           new Vector3(width/2, 0, hitPosition));
        }
    }
}
