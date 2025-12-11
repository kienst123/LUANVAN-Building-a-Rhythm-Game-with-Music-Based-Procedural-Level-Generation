using UnityEngine;

namespace RhythmGame3D.UI.Menu3D
{
    /// <summary>
    /// Audio spectrum visualizer for menu music
    /// Creates animated 3D bars that react to audio frequency
    /// </summary>
    public class AudioVisualizer3D : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource audioSource;
        
        [Header("Visualizer Settings")]
        [SerializeField] private int barCount = 64;
        [SerializeField] private float radius = 12f;
        [SerializeField] private float barWidth = 0.3f;
        [SerializeField] private float barMinHeight = 0.5f;
        [SerializeField] private float barMaxHeight = 8f;
        [SerializeField] private float heightMultiplier = 50f;
        
        [Header("Colors")]
        [SerializeField] private Color lowFreqColor = new Color(0f, 0.94f, 1f, 1f); // Cyan
        [SerializeField] private Color midFreqColor = new Color(1f, 0f, 0.9f, 1f); // Magenta
        [SerializeField] private Color highFreqColor = new Color(1f, 1f, 0.2f, 1f); // Yellow
        
        [Header("Animation")]
        [SerializeField] private float smoothSpeed = 10f;
        [SerializeField] private bool rotateVisualizer = true;
        [SerializeField] private float rotationSpeed = 10f;
        
        private GameObject[] bars;
        private Material[] barMaterials;
        private float[] spectrumData;
        private float[] smoothedSpectrum;
        
        void Start()
        {
            // Initialize spectrum arrays
            spectrumData = new float[barCount];
            smoothedSpectrum = new float[barCount];
            
            // Create bars
            CreateVisualizerBars();
            
            // Auto-find audio source if not assigned
            if (audioSource == null)
            {
                audioSource = FindObjectOfType<AudioSource>();
                
                if (audioSource == null)
                {
                    Debug.LogWarning("[AudioVisualizer3D] No AudioSource found! Creating one...");
                    GameObject audioObj = new GameObject("MenuMusic");
                    audioSource = audioObj.AddComponent<AudioSource>();
                    audioSource.loop = true;
                    audioSource.playOnAwake = true;
                }
            }
        }
        
        void Update()
        {
            if (audioSource == null || !audioSource.isPlaying) return;
            
            // Get spectrum data
            audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
            
            // Update bars
            UpdateBars();
            
            // Rotate visualizer
            if (rotateVisualizer)
            {
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Create visualizer bars in a circle
        /// </summary>
        void CreateVisualizerBars()
        {
            bars = new GameObject[barCount];
            barMaterials = new Material[barCount];
            
            for (int i = 0; i < barCount; i++)
            {
                // Calculate position around circle
                float angle = (360f / barCount) * i;
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                
                // Create bar object
                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = $"VisualizerBar_{i}";
                bar.transform.parent = transform;
                bar.transform.position = new Vector3(x, y, 0f);
                bar.transform.localScale = new Vector3(barWidth, barMinHeight, barWidth);
                
                // Rotate bar to point outward
                bar.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
                
                // Remove collider
                Destroy(bar.GetComponent<Collider>());
                
                // Setup material with emission
                MeshRenderer renderer = bar.GetComponent<MeshRenderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.EnableKeyword("_EMISSION");
                
                // Assign color based on frequency range
                Color barColor = GetColorForFrequency(i, barCount);
                mat.color = barColor;
                mat.SetColor("_EmissionColor", barColor * 2f);
                
                renderer.material = mat;
                
                bars[i] = bar;
                barMaterials[i] = mat;
            }
            
            Debug.Log($"[AudioVisualizer3D] Created {barCount} visualizer bars");
        }
        
        /// <summary>
        /// Update bar heights based on spectrum data
        /// </summary>
        void UpdateBars()
        {
            for (int i = 0; i < barCount; i++)
            {
                if (bars[i] == null) continue;
                
                // Smooth spectrum data
                smoothedSpectrum[i] = Mathf.Lerp(
                    smoothedSpectrum[i],
                    spectrumData[i] * heightMultiplier,
                    Time.deltaTime * smoothSpeed
                );
                
                // Calculate bar height
                float barHeight = Mathf.Clamp(
                    smoothedSpectrum[i],
                    barMinHeight,
                    barMaxHeight
                );
                
                // Update bar scale
                Vector3 scale = bars[i].transform.localScale;
                scale.y = barHeight;
                bars[i].transform.localScale = scale;
                
                // Update emission intensity based on height
                if (barMaterials[i] != null)
                {
                    Color baseColor = GetColorForFrequency(i, barCount);
                    float intensity = Mathf.Clamp01(barHeight / barMaxHeight);
                    Color emissionColor = baseColor * intensity * 3f;
                    barMaterials[i].SetColor("_EmissionColor", emissionColor);
                }
            }
        }
        
        /// <summary>
        /// Get color based on frequency range (low = cyan, mid = magenta, high = yellow)
        /// </summary>
        Color GetColorForFrequency(int index, int total)
        {
            float t = (float)index / total;
            
            if (t < 0.33f)
            {
                // Low frequencies - Cyan
                return lowFreqColor;
            }
            else if (t < 0.66f)
            {
                // Mid frequencies - Magenta
                return midFreqColor;
            }
            else
            {
                // High frequencies - Yellow
                return highFreqColor;
            }
        }
        
        /// <summary>
        /// Set audio source for visualization
        /// </summary>
        public void SetAudioSource(AudioSource source)
        {
            audioSource = source;
        }
    }
}
