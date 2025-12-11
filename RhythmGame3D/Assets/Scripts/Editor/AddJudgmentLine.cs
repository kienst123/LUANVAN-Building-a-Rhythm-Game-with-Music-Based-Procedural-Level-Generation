using UnityEngine;
using UnityEditor;

namespace RhythmGame3D.Editor
{
    /// <summary>
    /// Quick tool to add Judgment Line to GameScene
    /// The line shows where players should hit notes
    /// </summary>
    public class AddJudgmentLine : EditorWindow
    {
        [MenuItem("RhythmGame/Add Judgment Line to Scene")]
        public static void AddJudgmentLineToScene()
        {
            // Check if Judgment Line already exists
            GameObject existingLine = GameObject.Find("JudgmentLine");
            if (existingLine != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Judgment Line Exists",
                    "Judgment Line already exists. Overwrite?",
                    "Yes", "No"
                );

                if (!overwrite)
                    return;

                DestroyImmediate(existingLine);
            }

            // Create Judgment Line
            CreateJudgmentLine();

            EditorUtility.DisplayDialog("Success", 
                "Judgment Line added!\n\n" +
                "A bright horizontal line now shows where to hit notes.\n" +
                "Position: Z = 0 (hit position)", 
                "OK");
            
            Debug.Log("[AddJudgmentLine] Judgment Line added successfully!");
        }

        static void CreateJudgmentLine()
        {
            // Main container
            GameObject lineContainer = new GameObject("JudgmentLine");
            lineContainer.transform.position = new Vector3(0, 0, 0);

            // Create main line using Quad
            GameObject lineObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            lineObj.name = "MainLine";
            lineObj.transform.SetParent(lineContainer.transform, false);
            
            // Position at hit position (Z = 0, Y = 0.02 to be above lanes)
            lineObj.transform.localPosition = new Vector3(0, 0.02f, 0);
            
            // Rotate to face camera (rotate 90 degrees around X)
            lineObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            // Scale: width = 7 units (covers all 4 lanes + margin), height = 0.15 (visible line)
            lineObj.transform.localScale = new Vector3(7f, 0.15f, 1f);

            // Create bright glowing material
            Material lineMaterial = new Material(Shader.Find("Standard"));
            lineMaterial.SetFloat("_Mode", 2); // Fade mode
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.EnableKeyword("_ALPHABLEND_ON");
            lineMaterial.renderQueue = 3000;
            
            // Bright white with high emission
            Color brightWhite = new Color(1f, 1f, 1f, 0.9f);
            lineMaterial.SetColor("_Color", brightWhite);
            lineMaterial.EnableKeyword("_EMISSION");
            lineMaterial.SetColor("_EmissionColor", new Color(1.5f, 1.5f, 1.5f)); // Bright glow
            lineMaterial.SetFloat("_Metallic", 0f);
            lineMaterial.SetFloat("_Glossiness", 0.8f);
            
            // Apply material
            MeshRenderer renderer = lineObj.GetComponent<MeshRenderer>();
            renderer.material = lineMaterial;

            // Remove collider (we don't need physics)
            Collider collider = lineObj.GetComponent<Collider>();
            if (collider != null)
                DestroyImmediate(collider);

            // Create glow effect (duplicate line with softer color)
            GameObject glowObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glowObj.name = "GlowEffect";
            glowObj.transform.SetParent(lineContainer.transform, false);
            glowObj.transform.localPosition = new Vector3(0, 0.01f, 0); // Slightly below main line
            glowObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            glowObj.transform.localScale = new Vector3(7.5f, 0.4f, 1f); // Bigger and thicker

            // Glowing cyan material
            Material glowMaterial = new Material(Shader.Find("Standard"));
            glowMaterial.SetFloat("_Mode", 2);
            glowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            glowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive blend for glow
            glowMaterial.SetInt("_ZWrite", 0);
            glowMaterial.EnableKeyword("_ALPHABLEND_ON");
            glowMaterial.renderQueue = 2999; // Below main line
            
            Color cyanGlow = new Color(0.3f, 0.8f, 1f, 0.4f);
            glowMaterial.SetColor("_Color", cyanGlow);
            glowMaterial.EnableKeyword("_EMISSION");
            glowMaterial.SetColor("_EmissionColor", new Color(0.5f, 1.2f, 1.5f));
            
            MeshRenderer glowRenderer = glowObj.GetComponent<MeshRenderer>();
            glowRenderer.material = glowMaterial;

            // Remove collider
            Collider glowCollider = glowObj.GetComponent<Collider>();
            if (glowCollider != null)
                DestroyImmediate(glowCollider);

            // Create lane dividers (4 small vertical markers at lane boundaries)
            for (int i = 0; i <= 4; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = $"LaneMarker_{i}";
                marker.transform.SetParent(lineContainer.transform, false);
                
                // Position at lane boundaries
                float xPos = -3f + (i * 1.5f);
                marker.transform.localPosition = new Vector3(xPos, 0.03f, 0);
                marker.transform.localScale = new Vector3(0.05f, 0.3f, 0.05f);
                
                MeshRenderer markerRenderer = marker.GetComponent<MeshRenderer>();
                markerRenderer.material = lineMaterial; // Same bright material
                
                Collider markerCollider = marker.GetComponent<Collider>();
                if (markerCollider != null)
                    DestroyImmediate(markerCollider);
            }

            Debug.Log("[AddJudgmentLine] Created judgment line at Z=0 with glow effect and lane markers");
        }
    }
}
