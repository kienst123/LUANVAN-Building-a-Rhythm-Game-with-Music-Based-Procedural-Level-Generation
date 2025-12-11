using UnityEngine;

namespace RhythmGame3D.UI.Menu3D
{
    /// <summary>
    /// Camera controller for 3D menu with smooth movement and mouse parallax
    /// Creates dynamic, immersive menu experience
    /// </summary>
    public class MenuCamera3D : MonoBehaviour
    {
        [Header("Base Position")]
        [SerializeField] private Vector3 basePosition = new Vector3(0f, 0f, -10f); // Closer!
        [SerializeField] private Vector3 baseRotation = new Vector3(0f, 0f, 0f);
        
        [Header("Mouse Parallax")]
        [SerializeField] private bool enableParallax = true;
        [SerializeField] private float parallaxAmount = 2f;
        [SerializeField] private float parallaxSmoothness = 3f;
        
        [Header("Idle Animation")]
        [SerializeField] private bool enableIdleAnimation = true;
        [SerializeField] private float idleDollyAmount = 0.5f;
        [SerializeField] private float idleDollySpeed = 0.5f;
        [SerializeField] private float idleRotationAmount = 1f;
        [SerializeField] private float idleRotationSpeed = 0.3f;
        
        [Header("Transitions")]
        [SerializeField] private float transitionSpeed = 2f;
        
        private Vector3 targetPosition;
        private Vector3 targetRotation;
        private Vector3 parallaxOffset;
        private float idleTime;
        
        void Start()
        {
            targetPosition = basePosition;
            targetRotation = baseRotation;
            
            // Set initial position
            transform.position = basePosition;
            transform.rotation = Quaternion.Euler(baseRotation);
        }
        
        void Update()
        {
            UpdateParallax();
            UpdateIdleAnimation();
            ApplyMovement();
        }
        
        /// <summary>
        /// Update mouse parallax offset
        /// </summary>
        void UpdateParallax()
        {
            if (!enableParallax) return;
            
            // Get mouse position normalized (-1 to 1)
            Vector2 mousePos = Input.mousePosition;
            float normalizedX = (mousePos.x / Screen.width - 0.5f) * 2f;
            float normalizedY = (mousePos.y / Screen.height - 0.5f) * 2f;
            
            // Calculate parallax offset
            Vector3 targetParallax = new Vector3(
                normalizedX * parallaxAmount,
                normalizedY * parallaxAmount,
                0f
            );
            
            // Smooth interpolation
            parallaxOffset = Vector3.Lerp(parallaxOffset, targetParallax, Time.deltaTime * parallaxSmoothness);
        }
        
        /// <summary>
        /// Update idle camera animation
        /// </summary>
        void UpdateIdleAnimation()
        {
            if (!enableIdleAnimation) return;
            
            idleTime += Time.deltaTime;
            
            // Dolly in/out animation
            float dollyZ = Mathf.Sin(idleTime * idleDollySpeed) * idleDollyAmount;
            
            // Gentle rotation animation
            float rotationY = Mathf.Sin(idleTime * idleRotationSpeed) * idleRotationAmount;
            
            targetPosition = basePosition + new Vector3(0f, 0f, dollyZ) + parallaxOffset;
            targetRotation = baseRotation + new Vector3(0f, rotationY, 0f);
        }
        
        /// <summary>
        /// Apply smooth camera movement
        /// </summary>
        void ApplyMovement()
        {
            // Smooth position transition
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * transitionSpeed
            );
            
            // Smooth rotation transition
            Quaternion targetQuat = Quaternion.Euler(targetRotation);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetQuat,
                Time.deltaTime * transitionSpeed
            );
        }
        
        /// <summary>
        /// Move camera to specific position (for menu transitions)
        /// </summary>
        public void MoveTo(Vector3 position, Vector3 rotation)
        {
            basePosition = position;
            baseRotation = rotation;
        }
        
        /// <summary>
        /// Reset camera to default position
        /// </summary>
        public void ResetToDefault()
        {
            basePosition = new Vector3(0f, 0f, -10f); // Closer!
            baseRotation = Vector3.zero;
        }
        
        /// <summary>
        /// Focus on specific target object
        /// </summary>
        public void FocusOn(Transform target, float distance = 10f)
        {
            Vector3 direction = (transform.position - target.position).normalized;
            basePosition = target.position + direction * distance;
            baseRotation = Quaternion.LookRotation(target.position - basePosition).eulerAngles;
        }
    }
}
