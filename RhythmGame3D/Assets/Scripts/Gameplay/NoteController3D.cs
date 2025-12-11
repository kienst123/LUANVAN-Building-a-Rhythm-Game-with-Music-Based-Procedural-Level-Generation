using UnityEngine;
using RhythmGame3D.Beatmap;

namespace RhythmGame3D.Gameplay
{
    /// <summary>
    /// Controls individual 3D note behavior
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class NoteController3D : MonoBehaviour
    {
        [Header("Visual")]
        public MeshRenderer meshRenderer;
        public TrailRenderer trailRenderer;
        public ParticleSystem hitParticles;
        
        [Header("Colors")]
        public Color noteColor = new Color(0.4f, 0.7f, 1f, 1f);  // Cyan
        public Color hitColor = Color.white;
        
        // Data
        public HitObject hitObject { get; private set; }
        public bool isHit { get; private set; }
        
        // Callbacks
        public System.Action<NoteController3D> onMissed;  // Called when note is missed
        
        // Movement
        private float speed;
        private float hitPositionZ;
        private bool isMoving = true;
        
        // Long note specific
        private bool isLongNote;
        private GameObject tailObject;
        private float longNoteLength;
        
        void Awake()
        {
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
        }
        
        /// <summary>
        /// Initialize note with data
        /// </summary>
        public void Initialize(HitObject hitObj, float moveSpeed, float hitZ)
        {
            hitObject = hitObj;
            speed = moveSpeed;
            hitPositionZ = hitZ;
            isLongNote = hitObj.isLongNote;
            isHit = false;
            
            // Set color will be called separately by spawner with random color
            // So we don't set color here anymore
            
            // Create long note tail if needed
            if (isLongNote)
            {
                CreateLongNoteTail();
            }
        }
        
        /// <summary>
        /// Set note color (called from spawner for random colors)
        /// </summary>
        public void SetColor(Color color)
        {
            noteColor = color;
            
            // Update main note color - use material instance instead of PropertyBlock
            if (meshRenderer != null)
            {
                // Create a new material instance to avoid sharing
                Material mat = new Material(meshRenderer.sharedMaterial);
                mat.color = noteColor;
                
                // Enable emission for brighter colors
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", noteColor * 0.5f);
                    mat.EnableKeyword("_EMISSION");
                }
                
                meshRenderer.material = mat;
            }
            
            // Update trail color
            if (trailRenderer != null)
            {
                trailRenderer.startColor = noteColor;
                trailRenderer.endColor = new Color(noteColor.r, noteColor.g, noteColor.b, 0f);
            }
            
            // Update long note tail color if exists
            if (tailObject != null)
            {
                MeshRenderer tailRenderer = tailObject.GetComponent<MeshRenderer>();
                if (tailRenderer != null)
                {
                    Material tailMat = new Material(tailRenderer.sharedMaterial);
                    Color tailColor = noteColor;
                    tailColor.a = 0.5f;
                    tailMat.color = tailColor;
                    
                    if (tailMat.HasProperty("_EmissionColor"))
                    {
                        tailMat.SetColor("_EmissionColor", noteColor * 0.3f);
                        tailMat.EnableKeyword("_EMISSION");
                    }
                    
                    tailRenderer.material = tailMat;
                }
            }
        }
        
        void Update()
        {
            if (!isMoving) return;
            
            // Move towards camera
            transform.position += Vector3.back * speed * Time.deltaTime;
            
            // Check if passed hit position (missed)
            if (transform.position.z < hitPositionZ - 2f && !isHit)
            {
                OnMiss();
            }
        }
        
        /// <summary>
        /// Create visual tail for long notes
        /// </summary>
        void CreateLongNoteTail()
        {
            if (hitObject.endTime <= hitObject.time) return;
            
            // Calculate length based on duration
            float duration = (hitObject.endTime - hitObject.time) / 1000f;  // seconds
            longNoteLength = duration * speed;
            
            // Create tail cube
            tailObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tailObject.name = "LongNoteTail";
            tailObject.transform.parent = transform;
            
            // Position behind the head
            tailObject.transform.localPosition = new Vector3(0, 0, longNoteLength / 2f);
            tailObject.transform.localScale = new Vector3(1f, 1f, longNoteLength);
            
            // Color will be set by SetColor() method later
            // Just get the renderer ready
            MeshRenderer tailRenderer = tailObject.GetComponent<MeshRenderer>();
            
            // Remove collider
            Destroy(tailObject.GetComponent<Collider>());
            
            Debug.Log($"[NoteController3D] Created long note tail, length: {longNoteLength}");
        }
        
        /// <summary>
        /// Called when note is hit successfully
        /// </summary>
        public void OnHit(string judgment)
        {
            if (isHit) return;
            
            isHit = true;
            isMoving = false;
            
            // Flash hit color
            if (meshRenderer != null)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                propBlock.SetColor("_Color", hitColor);
                propBlock.SetColor("_EmissionColor", hitColor * 2f);
                meshRenderer.SetPropertyBlock(propBlock);
            }
            
            // Play hit particles
            if (hitParticles != null)
            {
                hitParticles.Play();
            }
            
            // Hide after short delay
            Invoke(nameof(HideNote), 0.1f);
        }
        
        /// <summary>
        /// Called when note is missed
        /// </summary>
        void OnMiss()
        {
            isHit = true;
            isMoving = false;
            
            Debug.Log($"[NoteController3D] Missed note at lane {hitObject.lane}");
            
            // Notify spawner to handle miss judgment
            onMissed?.Invoke(this);
            
            Destroy(gameObject, 0.5f);
        }
        
        /// <summary>
        /// Hide note visual
        /// </summary>
        void HideNote()
        {
            if (meshRenderer != null)
                meshRenderer.enabled = false;
            
            if (tailObject != null)
                tailObject.SetActive(false);
            
            if (trailRenderer != null)
                trailRenderer.enabled = false;
            
            // Destroy after particles finish
            Destroy(gameObject, 1f);
        }
        
        /// <summary>
        /// Get timing difference in seconds
        /// </summary>
        public float GetTimingDifference(float currentTime)
        {
            float hitTime = hitObject.time / 1000f;
            return currentTime - hitTime;
        }
        
        /// <summary>
        /// Check if note is within hit window
        /// </summary>
        public bool IsWithinHitWindow(float currentTime, float maxWindow)
        {
            float diff = Mathf.Abs(GetTimingDifference(currentTime));
            return diff <= maxWindow;
        }
        
        void OnDestroy()
        {
            // Cleanup
            if (tailObject != null && tailObject.transform.parent == null)
            {
                Destroy(tailObject);
            }
        }
    }
}
