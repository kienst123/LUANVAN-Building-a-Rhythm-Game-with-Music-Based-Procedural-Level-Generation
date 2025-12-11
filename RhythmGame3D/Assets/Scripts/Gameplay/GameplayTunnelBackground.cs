using UnityEngine;

namespace RhythmGame3D.Gameplay
{
    /// <summary>
    /// Creates an infinite scrolling tunnel background for gameplay scene
    /// Similar to menu background but optimized for gameplay
    /// </summary>
    public class GameplayTunnelBackground : MonoBehaviour
    {
        [Header("Tunnel Settings")]
        [SerializeField] private Color primaryColor = new Color(0f, 0.94f, 1f); // Cyan
        [SerializeField] private Color secondaryColor = new Color(1f, 0f, 0.9f); // Magenta
        [SerializeField] private float scrollSpeed = 2f;
        [SerializeField] private float rotationSpeed = 5f;
        
        [Header("Grid Settings")]
        [SerializeField] private int verticalLines = 20;
        [SerializeField] private int ringCount = 30;
        [SerializeField] private float ringSpacing = 5f;
        [SerializeField] private float tunnelRadius = 25f;
        
        [Header("Hexagon Layers")]
        [SerializeField] private int hexagonCount = 3;
        [SerializeField] private float hexagonSize = 15f;
        
        [Header("Particles")]
        [SerializeField] private int particleCount = 30;
        [SerializeField] private float particleSpeed = 5f;
        
        private LineRenderer[] verticalLineRenderers;
        private LineRenderer[] ringLineRenderers;
        private GameObject[] hexagons;
        private GameObject particleContainer;
        private float scrollOffset = 0f;
        
        void Start()
        {
            CreateTunnelGrid();
            CreateHexagonLayers();
            CreateParticles();
        }
        
        void Update()
        {
            AnimateTunnel();
            AnimateHexagons();
        }
        
        void CreateTunnelGrid()
        {
            // Create vertical lines
            verticalLineRenderers = new LineRenderer[verticalLines];
            
            for (int i = 0; i < verticalLines; i++)
            {
                GameObject lineObj = new GameObject($"VerticalLine_{i}");
                lineObj.transform.parent = transform;
                lineObj.transform.localPosition = Vector3.zero;
                
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.positionCount = 2;
                
                // Create material
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = primaryColor;
                lr.material = mat;
                
                float angle = (360f / verticalLines) * i;
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * tunnelRadius;
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * tunnelRadius;
                
                lr.SetPosition(0, new Vector3(x, y, -10f));
                lr.SetPosition(1, new Vector3(x, y, 200f));
                
                verticalLineRenderers[i] = lr;
            }
            
            // Create ring lines
            ringLineRenderers = new LineRenderer[ringCount];
            
            for (int i = 0; i < ringCount; i++)
            {
                GameObject ringObj = new GameObject($"Ring_{i}");
                ringObj.transform.parent = transform;
                ringObj.transform.localPosition = Vector3.zero;
                
                LineRenderer lr = ringObj.AddComponent<LineRenderer>();
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.positionCount = verticalLines + 1;
                lr.loop = true;
                
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = secondaryColor;
                lr.material = mat;
                
                float z = i * ringSpacing;
                
                for (int j = 0; j <= verticalLines; j++)
                {
                    float angle = (360f / verticalLines) * j;
                    float x = Mathf.Cos(angle * Mathf.Deg2Rad) * tunnelRadius;
                    float y = Mathf.Sin(angle * Mathf.Deg2Rad) * tunnelRadius;
                    
                    lr.SetPosition(j, new Vector3(x, y, z));
                }
                
                ringLineRenderers[i] = lr;
            }
        }
        
        void CreateHexagonLayers()
        {
            hexagons = new GameObject[hexagonCount];
            
            for (int layer = 0; layer < hexagonCount; layer++)
            {
                GameObject hexObj = new GameObject($"Hexagon_Layer_{layer}");
                hexObj.transform.parent = transform;
                hexObj.transform.localPosition = new Vector3(0f, 0f, 50f + layer * 30f);
                hexObj.transform.localScale = Vector3.one * (hexagonSize + layer * 5f);
                
                LineRenderer lr = hexObj.AddComponent<LineRenderer>();
                lr.startWidth = 0.2f;
                lr.endWidth = 0.2f;
                lr.positionCount = 7;
                lr.loop = true;
                
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = Color.Lerp(primaryColor, secondaryColor, layer / (float)hexagonCount);
                lr.material = mat;
                
                // Create hexagon shape
                for (int i = 0; i < 7; i++)
                {
                    float angle = 60f * i;
                    float x = Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = Mathf.Sin(angle * Mathf.Deg2Rad);
                    lr.SetPosition(i, new Vector3(x, y, 0f));
                }
                
                hexagons[layer] = hexObj;
            }
        }
        
        void CreateParticles()
        {
            particleContainer = new GameObject("Particles");
            particleContainer.transform.parent = transform;
            
            for (int i = 0; i < particleCount; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.name = $"Particle_{i}";
                particle.transform.parent = particleContainer.transform;
                particle.transform.localScale = Vector3.one * 0.5f;
                
                // Random position in tunnel
                float angle = Random.Range(0f, 360f);
                float radius = Random.Range(5f, tunnelRadius - 2f);
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                float z = Random.Range(0f, 150f);
                
                particle.transform.localPosition = new Vector3(x, y, z);
                
                // Setup material with emission
                MeshRenderer renderer = particle.GetComponent<MeshRenderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.EnableKeyword("_EMISSION");
                mat.color = Random.value > 0.5f ? primaryColor : secondaryColor;
                mat.SetColor("_EmissionColor", mat.color * 2f);
                renderer.material = mat;
                
                Destroy(particle.GetComponent<Collider>());
            }
        }
        
        void AnimateTunnel()
        {
            // Scroll rings toward camera
            scrollOffset += scrollSpeed * Time.deltaTime;
            
            for (int i = 0; i < ringLineRenderers.Length; i++)
            {
                if (ringLineRenderers[i] == null) continue;
                
                // Update ring positions
                float baseZ = (i * ringSpacing - scrollOffset) % (ringCount * ringSpacing);
                if (baseZ < -ringSpacing) baseZ += ringCount * ringSpacing;
                
                for (int j = 0; j < verticalLines + 1; j++)
                {
                    Vector3 pos = ringLineRenderers[i].GetPosition(j);
                    ringLineRenderers[i].SetPosition(j, new Vector3(pos.x, pos.y, baseZ));
                }
            }
            
            // Animate particles
            if (particleContainer != null)
            {
                foreach (Transform particle in particleContainer.transform)
                {
                    Vector3 pos = particle.localPosition;
                    pos.z -= particleSpeed * Time.deltaTime;
                    
                    if (pos.z < -10f)
                    {
                        pos.z = 150f;
                        
                        // Randomize position again
                        float angle = Random.Range(0f, 360f);
                        float radius = Random.Range(5f, tunnelRadius - 2f);
                        pos.x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                        pos.y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                    }
                    
                    particle.localPosition = pos;
                }
            }
        }
        
        void AnimateHexagons()
        {
            for (int i = 0; i < hexagons.Length; i++)
            {
                if (hexagons[i] == null) continue;
                
                // Rotate hexagons
                float rotDir = (i % 2 == 0) ? 1f : -1f;
                hexagons[i].transform.Rotate(0f, 0f, rotationSpeed * rotDir * Time.deltaTime);
                
                // Move toward camera
                Vector3 pos = hexagons[i].transform.localPosition;
                pos.z -= scrollSpeed * 0.5f * Time.deltaTime;
                
                // Loop back when too close
                if (pos.z < 0f)
                {
                    pos.z += hexagonCount * 30f;
                }
                
                hexagons[i].transform.localPosition = pos;
            }
        }
        
        /// <summary>
        /// Adjust tunnel intensity based on game events (combo, health, etc)
        /// </summary>
        public void SetIntensity(float intensity)
        {
            intensity = Mathf.Clamp01(intensity);
            
            Color boostedPrimary = primaryColor * (1f + intensity);
            Color boostedSecondary = secondaryColor * (1f + intensity);
            
            // Update line colors
            foreach (var lr in verticalLineRenderers)
            {
                if (lr != null && lr.material != null)
                {
                    lr.material.color = boostedPrimary;
                }
            }
            
            foreach (var lr in ringLineRenderers)
            {
                if (lr != null && lr.material != null)
                {
                    lr.material.color = boostedSecondary;
                }
            }
        }
        
        /// <summary>
        /// Pulse effect for beat sync
        /// </summary>
        public void PulseOnBeat()
        {
            foreach (var hexagon in hexagons)
            {
                if (hexagon != null)
                {
                    StartCoroutine(PulseScale(hexagon.transform));
                }
            }
        }
        
        System.Collections.IEnumerator PulseScale(Transform target)
        {
            Vector3 originalScale = target.localScale;
            Vector3 targetScale = originalScale * 1.1f;
            
            float duration = 0.1f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                target.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            elapsed = 0f;
            while (elapsed < duration)
            {
                target.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            target.localScale = originalScale;
        }
    }
}
