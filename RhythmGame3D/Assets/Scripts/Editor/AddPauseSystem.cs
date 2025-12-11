using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using RhythmGame3D.UI;

namespace RhythmGame3D.Editor
{
    /// <summary>
    /// Quick tool to add Pause System to existing GameScene
    /// </summary>
    public class AddPauseSystem : EditorWindow
    {
        [MenuItem("RhythmGame/Add Pause System to Current Scene")]
        public static void AddPauseSystemToScene()
        {
            // Find Canvas in scene
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No Canvas found in scene!", "OK");
                return;
            }

            // Check if Pause Panel already exists
            Transform existingPause = canvas.transform.Find("PausePanel");
            if (existingPause != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Pause Panel Exists",
                    "Pause Panel already exists. Overwrite?",
                    "Yes", "No"
                );

                if (!overwrite)
                    return;

                DestroyImmediate(existingPause.gameObject);
            }

            // Create Pause Panel
            GameObject pausePanel = CreatePausePanel(canvas.gameObject);
            
            // Setup PauseManager
            SetupPauseManager(canvas.gameObject, pausePanel);

            EditorUtility.DisplayDialog("Success", "Pause System added!\n\nPress ESC or P to pause during gameplay.", "OK");
            
            Debug.Log("[AddPauseSystem] Pause System added successfully!");
        }

        static GameObject CreatePausePanel(GameObject canvas)
        {
            // Main Panel
            GameObject panel = new GameObject("PausePanel");
            panel.transform.SetParent(canvas.transform, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Semi-transparent black background
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.85f);

            // Content Container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(panel.transform, false);

            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.pivot = new Vector2(0.5f, 0.5f);
            contentRT.sizeDelta = new Vector2(400, 400);

            Image contentBg = content.AddComponent<Image>();
            contentBg.color = new Color(0.15f, 0.15f, 0.2f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(content.transform, false);

            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1);
            titleRT.anchorMax = new Vector2(0.5f, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.anchoredPosition = new Vector2(0, -30);
            titleRT.sizeDelta = new Vector2(350, 60);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "PAUSED";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            // Hint Text
            GameObject hintObj = new GameObject("HintText");
            hintObj.transform.SetParent(content.transform, false);

            RectTransform hintRT = hintObj.AddComponent<RectTransform>();
            hintRT.anchorMin = new Vector2(0.5f, 0.5f);
            hintRT.anchorMax = new Vector2(0.5f, 0.5f);
            hintRT.pivot = new Vector2(0.5f, 0.5f);
            hintRT.anchoredPosition = new Vector2(0, 30);
            hintRT.sizeDelta = new Vector2(350, 40);

            TextMeshProUGUI hintText = hintObj.AddComponent<TextMeshProUGUI>();
            hintText.text = "Press ESC or P to resume";
            hintText.fontSize = 18;
            hintText.alignment = TextAlignmentOptions.Center;
            hintText.color = new Color(0.7f, 0.7f, 0.7f);

            // Resume Button
            CreateButton(content, "ResumeButton", "RESUME", new Vector2(0, -30), new Color(0.2f, 0.8f, 0.2f));

            // Return to Menu Button
            CreateButton(content, "ReturnToMenuButton", "RETURN TO MENU", new Vector2(0, -110), new Color(0.8f, 0.3f, 0.3f));

            panel.SetActive(false);
            return panel;
        }

        static GameObject CreateButton(GameObject parent, string name, string text, Vector2 position, Color color)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(300, 60);

            Image img = buttonObj.AddComponent<Image>();
            img.color = color;

            Button btn = buttonObj.AddComponent<Button>();
            btn.targetGraphic = img;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return buttonObj;
        }

        static void SetupPauseManager(GameObject canvas, GameObject pausePanel)
        {
            // Check if PauseManager already exists
            PauseManager existingManager = canvas.GetComponent<PauseManager>();
            if (existingManager != null)
            {
                DestroyImmediate(existingManager);
            }

            // Add PauseManager component
            PauseManager pauseManager = canvas.AddComponent<PauseManager>();

            // Assign references using SerializedObject
            SerializedObject so = new SerializedObject(pauseManager);
            
            so.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            so.FindProperty("resumeButton").objectReferenceValue = pausePanel.transform.Find("Content/ResumeButton").GetComponent<Button>();
            so.FindProperty("returnToMenuButton").objectReferenceValue = pausePanel.transform.Find("Content/ReturnToMenuButton").GetComponent<Button>();
            so.FindProperty("mainMenuSceneName").stringValue = "MainMenu";
            so.FindProperty("pauseKey").intValue = (int)KeyCode.Escape;
            so.FindProperty("alternatePauseKey").intValue = (int)KeyCode.P;

            so.ApplyModifiedProperties();

            Debug.Log("[AddPauseSystem] PauseManager component added and configured");
        }
    }
}
