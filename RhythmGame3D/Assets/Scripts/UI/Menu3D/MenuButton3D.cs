using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace RhythmGame3D.UI.Menu3D
{
    /// <summary>
    /// 3D button for main menu with hover effects, glow, and animations
    /// Replaces traditional 2D Canvas UI buttons
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class MenuButton3D : MonoBehaviour
    {
        [Header("Visual")]
        public MeshRenderer buttonRenderer;
        public TextMeshPro buttonText;
        public Light glowLight;
        
        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.1f, 0.1f, 0.3f, 0.8f);
        [SerializeField] private Color hoverColor = new Color(0f, 0.94f, 1f, 1f); // Cyan
        [SerializeField] private Color pressColor = new Color(1f, 0f, 0.9f, 1f); // Magenta
        [SerializeField] private Color glowColor = new Color(0f, 0.94f, 1f, 1f);
        
        [Header("Animation")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float animationSpeed = 5f;
        [SerializeField] private float bobSpeed = 1f;
        [SerializeField] private float bobAmount = 0.1f;
        
        [Header("Event")]
        public UnityEvent onButtonClick = new UnityEvent();
        
        private Vector3 originalScale;
        private Vector3 targetScale;
        private Color currentColor;
        private Color targetColor;
        private bool isHovered = false;
        private float bobOffset;
        private Vector3 originalPosition;
        private Material buttonMaterial;
        
        void Start()
        {
            // Validate references
            if (buttonRenderer == null)
            {
                Debug.LogError($"[MenuButton3D] {gameObject.name} missing buttonRenderer!");
                enabled = false;
                return;
            }
            
            if (buttonText == null)
            {
                Debug.LogWarning($"[MenuButton3D] {gameObject.name} missing buttonText!");
            }
            
            originalScale = transform.localScale;
            targetScale = originalScale;
            currentColor = normalColor;
            targetColor = normalColor;
            originalPosition = transform.localPosition;
            
            // Random bob offset for variety
            bobOffset = Random.Range(0f, Mathf.PI * 2f);
            
            // Setup material
            if (buttonRenderer != null)
            {
                buttonMaterial = new Material(buttonRenderer.sharedMaterial);
                buttonRenderer.material = buttonMaterial;
                
                // Enable emission
                if (buttonMaterial.HasProperty("_EmissionColor"))
                {
                    buttonMaterial.EnableKeyword("_EMISSION");
                }
            }
            
            // Setup glow light
            if (glowLight != null)
            {
                glowLight.color = glowColor;
                glowLight.intensity = 0f;
            }
            
            UpdateVisuals();
        }
        
        void Update()
        {
            // Smooth scale animation
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
            
            // Smooth color transition
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * animationSpeed);
            
            // Bob animation
            float bobY = Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobAmount;
            transform.localPosition = originalPosition + Vector3.up * bobY;
            
            UpdateVisuals();
            
            // Check for mouse hover (raycast from camera)
            CheckHover();
        }
        
        /// <summary>
        /// Check if mouse is hovering over button
        /// </summary>
        void CheckHover()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;
            
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            bool wasHovered = isHovered;
            
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    isHovered = true;
                    
                    if (!wasHovered)
                    {
                        OnHoverEnter();
                    }
                    
                    // Check for click
                    if (Input.GetMouseButtonDown(0))
                    {
                        OnButtonPressed();
                    }
                }
                else
                {
                    if (wasHovered)
                    {
                        OnHoverExit();
                    }
                    isHovered = false;
                }
            }
            else
            {
                if (wasHovered)
                {
                    OnHoverExit();
                }
                isHovered = false;
            }
        }
        
        /// <summary>
        /// Called when mouse enters button
        /// </summary>
        void OnHoverEnter()
        {
            targetScale = originalScale * hoverScale;
            targetColor = hoverColor;
            
            // Increase glow
            if (glowLight != null)
            {
                glowLight.intensity = 2f;
            }
            
            Debug.Log($"[MenuButton3D] Hover: {gameObject.name}");
        }
        
        /// <summary>
        /// Called when mouse exits button
        /// </summary>
        void OnHoverExit()
        {
            targetScale = originalScale;
            targetColor = normalColor;
            
            // Decrease glow
            if (glowLight != null)
            {
                glowLight.intensity = 0.5f;
            }
        }
        
        /// <summary>
        /// Called when button is clicked
        /// </summary>
        void OnButtonPressed()
        {
            // Flash effect
            StartCoroutine(PressAnimation());
            
            // Invoke event
            onButtonClick?.Invoke();
            
            Debug.Log($"[MenuButton3D] Clicked: {gameObject.name}");
        }
        
        /// <summary>
        /// Press animation coroutine
        /// </summary>
        System.Collections.IEnumerator PressAnimation()
        {
            // Press down
            targetScale = originalScale * 0.9f;
            targetColor = pressColor;
            
            if (glowLight != null)
            {
                glowLight.intensity = 5f;
            }
            
            yield return new WaitForSeconds(0.1f);
            
            // Release
            targetScale = originalScale * hoverScale;
            targetColor = hoverColor;
            
            if (glowLight != null)
            {
                glowLight.intensity = 2f;
            }
        }
        
        /// <summary>
        /// Update visual appearance
        /// </summary>
        void UpdateVisuals()
        {
            if (buttonMaterial != null)
            {
                buttonMaterial.color = currentColor;
                
                // Set emission for glow effect
                if (buttonMaterial.HasProperty("_EmissionColor"))
                {
                    Color emissionColor = currentColor * (isHovered ? 2f : 0.5f);
                    buttonMaterial.SetColor("_EmissionColor", emissionColor);
                }
            }
            
            // Update text color
            if (buttonText != null)
            {
                buttonText.color = Color.white;
                
                // Add outline glow effect
                if (buttonText.fontMaterial != null)
                {
                    buttonText.fontMaterial.SetColor("_GlowColor", currentColor);
                }
            }
        }
        
        /// <summary>
        /// Set button text
        /// </summary>
        public void SetText(string text)
        {
            if (buttonText != null)
            {
                buttonText.text = text;
            }
        }
        
        /// <summary>
        /// Add click listener
        /// </summary>
        public void AddClickListener(UnityAction action)
        {
            onButtonClick.AddListener(action);
        }
    }
}
