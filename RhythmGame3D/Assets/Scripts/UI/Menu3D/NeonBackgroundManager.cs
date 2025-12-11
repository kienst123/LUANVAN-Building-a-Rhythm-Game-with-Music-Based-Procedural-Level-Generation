using UnityEngine;

namespace RhythmGame3D.UI.Menu3D
{
    /// <summary>
    /// Manages the 3D neon tunnel background for main menu
    /// Creates animated grid lines, rotating patterns, and particles
    /// </summary>
    public class NeonBackgroundManager : MonoBehaviour
    {
        [Header("Tunnel Settings")]
        [SerializeField] private Material gridMaterial;
        [SerializeField] private Color primaryColor = new Color(0f, 0.94f, 1f, 1f); // Cyan
        [SerializeField] private Color secondaryColor = new Color(1f, 0f, 0.9f, 1f); // Magenta
        
        [Header("Animation")]
        [SerializeField] private float gridScrollSpeed = 2f;
        [SerializeField] private float rotationSpeed = 5f;
        
        [Header("Grid Lines")]
        [SerializeField] private int gridLineCount = 20;
        [SerializeField] private float gridSpacing = 2f;
        [SerializeField] private float tunnelLength = 100f;
        
        [Header("Particles")]
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private int particleCount = 50;
        [SerializeField] private float particleSpeed = 3f;
        
        private GameObject gridContainer;
        private GameObject rotatingRings;
        private ParticleSystem menuParticleSystem;
        private float scrollOffset = 0f;
        
        void Start()
        {
            CreateTunnelGrid();
            CreateRotatingRings();
            CreateParticleSystem();
        }
        
        void Update()
        {
            AnimateGrid();
            AnimateRings();
        }
        
        /// <summary>
        /// Create the main grid tunnel
        /// </summary>
        void CreateTunnelGrid()
        {
            gridContainer = new GameObject("GridTunnel");
            gridContainer.transform.parent = transform;
            
            // Create vertical lines
            for (int i = 0; i < gridLineCount; i++)
            {
                float angle = (360f / gridLineCount) * i;
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * 10f;
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * 10f;
                
                CreateGridLine(new Vector3(x, y, 0f), new Vector3(x, y, tunnelLength));
            }
            
            // Create horizontal ring lines
            for (int i = 0; i < tunnelLength / gridSpacing; i++)
            {
                float z = i * gridSpacing;
                CreateCircleRing(z, 10f);
            }
        }
        
        /// <summary>
        /// Create a single grid line
        /// </summary>
        void CreateGridLine(Vector3 start, Vector3 end)
        {
            GameObject lineObj = new GameObject("GridLine");
            lineObj.transform.parent = gridContainer.transform;
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            
            line.startWidth = 0.05f;
            line.endWidth = 0.05f;
            
            if (gridMaterial != null)
            {
                line.material = gridMaterial;
            }
            else
            {
                line.material = new Material(Shader.Find("Sprites/Default"));
            }
            
            line.startColor = primaryColor;
            line.endColor = new Color(primaryColor.r, primaryColor.g, primaryColor.b, 0.1f);
        }
        
        /// <summary>
        /// Create a circular ring at specific Z position
        /// </summary>
        void CreateCircleRing(float z, float radius)
        {
            GameObject ringObj = new GameObject($"Ring_{z}");
            ringObj.transform.parent = gridContainer.transform;
            ringObj.transform.position = new Vector3(0, 0, z);
            
            LineRenderer line = ringObj.AddComponent<LineRenderer>();
            int segments = 40;
            line.positionCount = segments + 1;
            line.loop = true;
            
            for (int i = 0; i <= segments; i++)
            {
                float angle = (360f / segments) * i;
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                line.SetPosition(i, new Vector3(x, y, 0));
            }
            
            line.startWidth = 0.05f;
            line.endWidth = 0.05f;
            
            if (gridMaterial != null)
            {
                line.material = gridMaterial;
            }
            else
            {
                line.material = new Material(Shader.Find("Sprites/Default"));
            }
            
            line.startColor = secondaryColor;
            line.endColor = secondaryColor;
        }
        
        /// <summary>
        /// Create rotating hexagon/circle patterns
        /// </summary>
        void CreateRotatingRings()
        {
            rotatingRings = new GameObject("RotatingRings");
            rotatingRings.transform.parent = transform;
            rotatingRings.transform.position = new Vector3(0, 0, 30f);
            
            // Create multiple hexagon layers
            for (int layer = 0; layer < 3; layer++)
            {
                GameObject hexagon = CreateHexagon(5f + layer * 2f);
                hexagon.transform.parent = rotatingRings.transform;
            }
        }
        
        /// <summary>
        /// Create a hexagon shape
        /// </summary>
        GameObject CreateHexagon(float radius)
        {
            GameObject hexObj = new GameObject($"Hexagon_{radius}");
            
            LineRenderer line = hexObj.AddComponent<LineRenderer>();
            int sides = 6;
            line.positionCount = sides + 1;
            line.loop = true;
            
            for (int i = 0; i <= sides; i++)
            {
                float angle = (360f / sides) * i;
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                line.SetPosition(i, new Vector3(x, y, 0));
            }
            
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            
            if (gridMaterial != null)
            {
                line.material = gridMaterial;
            }
            else
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.SetColor("_Color", primaryColor);
                line.material = mat;
            }
            
            line.startColor = primaryColor;
            line.endColor = primaryColor;
            
            return hexObj;
        }
        
        /// <summary>
        /// Create particle system for floating elements
        /// </summary>
        void CreateParticleSystem()
        {
            GameObject particleObj = new GameObject("FloatingParticles");
            particleObj.transform.parent = transform;
            
            menuParticleSystem = particleObj.AddComponent<ParticleSystem>();
            
            var main = menuParticleSystem.main;
            main.maxParticles = particleCount;
            main.startLifetime = 5f;
            main.startSpeed = particleSpeed;
            main.startSize = 0.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            var emission = menuParticleSystem.emission;
            emission.rateOverTime = particleCount / 5f;
            
            var shape = menuParticleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 15f;
            
            var colorOverLifetime = menuParticleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(primaryColor, 0f),
                    new GradientColorKey(secondaryColor, 0.5f),
                    new GradientColorKey(primaryColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            
            colorOverLifetime.color = gradient;
        }
        
        /// <summary>
        /// Animate grid scrolling
        /// </summary>
        void AnimateGrid()
        {
            if (gridContainer == null) return;
            
            scrollOffset += gridScrollSpeed * Time.deltaTime;
            
            // Move grid forward to create infinite tunnel effect
            gridContainer.transform.position = new Vector3(0, 0, -scrollOffset % gridSpacing);
        }
        
        /// <summary>
        /// Animate rotating rings
        /// </summary>
        void AnimateRings()
        {
            if (rotatingRings == null) return;
            
            // Rotate each hexagon at different speeds
            int childIndex = 0;
            foreach (Transform child in rotatingRings.transform)
            {
                float speed = rotationSpeed * (childIndex % 2 == 0 ? 1f : -1f);
                child.Rotate(Vector3.forward, speed * Time.deltaTime);
                childIndex++;
            }
        }
    }
}
