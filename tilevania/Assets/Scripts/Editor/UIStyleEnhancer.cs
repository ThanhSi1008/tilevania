using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor script to enhance UI styling in AuthScene.
/// Improves colors, spacing, shadows, and overall visual appeal.
/// </summary>
public class UIStyleEnhancer : EditorWindow
{
    [MenuItem("Tools/UI Style Enhancer")]
    public static void ShowWindow()
    {
        GetWindow<UIStyleEnhancer>("UI Style Enhancer");
    }

    private void OnGUI()
    {
        GUILayout.Label("UI Style Enhancer", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This tool will enhance the UI styling in AuthScene:\n" +
            "• Modern color scheme (gradients, shadows)\n" +
            "• Better button styling\n" +
            "• Improved panel backgrounds\n" +
            "• Better text styling\n" +
            "• Enhanced spacing and layout",
            MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("Enhance AuthScene UI", GUILayout.Height(40)))
        {
            EnhanceUIStyle();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Reset to Default Colors", GUILayout.Height(30)))
        {
            ResetToDefault();
        }
    }

    private void EnhanceUIStyle()
    {
        // Load AuthScene
        string scenePath = "Assets/Scenes/AuthScene.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath);

        if (!scene.IsValid())
        {
            EditorUtility.DisplayDialog("Error", "Could not load AuthScene!", "OK");
            return;
        }

        int changes = 0;

        // Find all UI elements
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene!", "OK");
            return;
        }

        // Enhance all buttons
        Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            EnhanceButton(button);
            changes++;
        }

        // Enhance all panels (Images used as backgrounds)
        Image[] images = canvas.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            // Check if it's a panel background (has alpha < 1 or is a panel)
            if (image.gameObject.name.Contains("Panel") || image.color.a < 1f)
            {
                EnhancePanelBackground(image);
                changes++;
            }
        }

        // Enhance all text elements
        TextMeshProUGUI[] texts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            EnhanceText(text);
            changes++;
        }

        // Add shadows to buttons
        foreach (Button button in buttons)
        {
            AddShadowToButton(button);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.DisplayDialog("Success", 
            $"Enhanced {changes} UI elements!\n\n" +
            "Changes include:\n" +
            "• Modern color scheme\n" +
            "• Better button styling\n" +
            "• Improved panel backgrounds\n" +
            "• Enhanced text styling", 
            "OK");
    }

    private void EnhanceButton(Button button)
    {
        // Get or add Image component
        Image image = button.GetComponent<Image>();
        if (image == null)
        {
            image = button.gameObject.AddComponent<Image>();
        }

        // Set modern button colors
        ColorBlock colors = button.colors;
        
        // Primary button (Login, Register, Play) - Blue gradient
        if (button.gameObject.name.Contains("Login") || 
            button.gameObject.name.Contains("Register") || 
            button.gameObject.name.Contains("Play"))
        {
            colors.normalColor = new Color(0.2f, 0.4f, 0.8f, 1f); // Nice blue
            colors.highlightedColor = new Color(0.3f, 0.5f, 0.9f, 1f);
            colors.pressedColor = new Color(0.15f, 0.35f, 0.7f, 1f);
            colors.selectedColor = colors.highlightedColor;
            
            // Set button background color
            image.color = colors.normalColor;
        }
        // Secondary buttons (GoRegister, GoLogin, Logout) - Gray/White
        else
        {
            colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f); // Light gray
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f); // White
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.selectedColor = colors.highlightedColor;
            
            image.color = colors.normalColor;
        }

        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.15f;
        button.colors = colors;

        // Add rounded corners effect (using sprite if available)
        if (image.sprite == null)
        {
            // Try to get default UI sprite
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.asset");
            if (image.sprite != null)
            {
                image.type = Image.Type.Sliced;
            }
        }
    }

    private void EnhancePanelBackground(Image panel)
    {
        // Modern semi-transparent dark background
        if (panel.gameObject.name.Contains("LoginPanel") || 
            panel.gameObject.name.Contains("RegisterPanel") || 
            panel.gameObject.name.Contains("MainMenuPanel"))
        {
            // Dark blue-gray with transparency
            panel.color = new Color(0.15f, 0.2f, 0.25f, 0.95f);
        }
        else if (panel.color.a < 1f)
        {
            // Slightly lighter for other panels
            panel.color = new Color(0.2f, 0.25f, 0.3f, 0.9f);
        }
    }

    private void EnhanceText(TextMeshProUGUI text)
    {
        // Skip if it's input field placeholder or empty
        if (text.text == "" || text.text == "\u200B") return;

        // Title text - larger, bolder
        if (text.gameObject.name.Contains("Title") || 
            text.gameObject.name.Contains("Welcome"))
        {
            text.fontSize = 36;
            text.fontStyle = FontStyles.Bold;
            text.color = new Color(1f, 1f, 1f, 1f); // White
        }
        // Button text - medium, bold
        else if (text.transform.parent != null && 
                 text.transform.parent.GetComponent<Button>() != null)
        {
            text.fontSize = 20;
            text.fontStyle = FontStyles.Bold;
            text.color = new Color(1f, 1f, 1f, 1f); // White for buttons
        }
        // Regular text - default size
        else
        {
            if (text.fontSize < 16) text.fontSize = 16;
            text.color = new Color(0.9f, 0.9f, 0.9f, 1f); // Light gray
        }
    }

    private void AddShadowToButton(Button button)
    {
        // Check if shadow already exists
        Shadow shadow = button.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = button.gameObject.AddComponent<Shadow>();
        }

        // Add Outline for better visibility
        Outline outline = button.GetComponent<Outline>();
        if (outline == null)
        {
            outline = button.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.3f); // Subtle black outline
            outline.effectDistance = new Vector2(2f, -2f);
        }
    }

    private void ResetToDefault()
    {
        string scenePath = "Assets/Scenes/AuthScene.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath);

        if (!scene.IsValid())
        {
            EditorUtility.DisplayDialog("Error", "Could not load AuthScene!", "OK");
            return;
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Reset buttons
        Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            button.colors = colors;

            Image img = button.GetComponent<Image>();
            if (img != null) img.color = Color.white;
        }

        // Reset panels
        Image[] images = canvas.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            if (img.gameObject.name.Contains("Panel"))
            {
                img.color = new Color(1f, 1f, 1f, 0.392f);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.DisplayDialog("Success", "Reset UI to default colors!", "OK");
    }
}

