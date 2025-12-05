using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// Editor tool to automatically generate the complete Relic Bag UI system
/// Usage: In Unity menu bar, click Tools → Generate Relic Bag UI
/// </summary>
public class RelicBagUIGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Relic Bag UI")]
    public static void GenerateRelicBagUI()
    {
        // Find Canvas in current scene
        Canvas canvas = FindObjectOfType<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene! Please add a Canvas first.");
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene!\nPlease add a Canvas first.", "OK");
            return;
        }

        // Check if bag icon already exists
        Transform existingButton = canvas.transform.Find("BagIconButton");
        if (existingButton != null)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Bag Icon Already Exists",
                "A BagIconButton already exists. Do you want to replace it?",
                "Yes, Replace",
                "No, Cancel"
            );
            
            if (overwrite)
            {
                DestroyImmediate(existingButton.gameObject);
            }
            else
            {
                return;
            }
        }

        // Create everything
        GameObject bagButton = CreateBagIconButton(canvas);
        
        // Add the SimpleRelicBagUI component if not already present
        SimpleRelicBagUI bagUI = canvas.GetComponent<SimpleRelicBagUI>();
        if (bagUI == null)
        {
            bagUI = canvas.gameObject.AddComponent<SimpleRelicBagUI>();
        }
        
        // Assign the button
        SerializedObject so = new SerializedObject(bagUI);
        SerializedProperty buttonProp = so.FindProperty("bagIconButton");
        buttonProp.objectReferenceValue = bagButton.GetComponent<Button>();
        so.ApplyModifiedProperties();

        // Mark scene as dirty so it can be saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log("✅ Relic Bag UI generated successfully!");
        EditorUtility.DisplayDialog(
            "Success!",
            "Relic Bag UI has been created!\n\n" +
            "• Bag icon button is in the top-right corner\n" +
            "• SimpleRelicBagUI component added to Canvas\n" +
            "• Everything is connected and ready!\n\n" +
            "Press Play to test it!",
            "OK"
        );
        
        // Select the canvas so user can see the component
        Selection.activeGameObject = canvas.gameObject;
    }

    private static GameObject CreateBagIconButton(Canvas canvas)
    {
        // Create button
        GameObject button = new GameObject("BagIconButton");
        button.transform.SetParent(canvas.transform, false);
        
        RectTransform buttonRect = button.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 1);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.pivot = new Vector2(1, 1);
        buttonRect.anchoredPosition = new Vector2(-20, -20);
        buttonRect.sizeDelta = new Vector2(60, 60);
        
        // Add button component
        Button buttonComp = button.AddComponent<Button>();
        
        // Add button background image
        Image buttonImage = button.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Create Icon child
        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(button.transform, false);
        
        RectTransform iconRect = icon.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-10, -10);
        iconRect.anchoredPosition = Vector2.zero;
        
        Image iconImage = icon.AddComponent<Image>();
        
        // Try to load RelicNode sprite
        Sprite relicSprite = LoadRelicNodeSprite();
        if (relicSprite != null)
        {
            iconImage.sprite = relicSprite;
            iconImage.preserveAspect = true;
            Debug.Log("✅ RelicNode sprite loaded successfully!");
        }
        else
        {
            iconImage.color = Color.white;
            Debug.LogWarning("⚠️ Could not find RelicNode sprite. Using white placeholder.");
        }

        return button;
    }

    private static Sprite LoadRelicNodeSprite()
    {
        // Try to find RelicNode sprite in Assets
        string[] guids = AssetDatabase.FindAssets("RelicNode t:Sprite");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null && sprite.name == "RelicNode")
            {
                Debug.Log($"Found RelicNode sprite at: {path}");
                return sprite;
            }
        }
        
        // Fallback: try direct path
        Sprite directSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/NodeIcons/RelicNode.png");
        if (directSprite != null)
        {
            return directSprite;
        }
        
        return null;
    }

    [MenuItem("Tools/Remove Relic Bag UI")]
    public static void RemoveRelicBagUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene!");
            return;
        }

        bool removed = false;

        // Remove button
        Transform button = canvas.transform.Find("BagIconButton");
        if (button != null)
        {
            DestroyImmediate(button.gameObject);
            removed = true;
        }

        // Remove component
        SimpleRelicBagUI bagUI = canvas.GetComponent<SimpleRelicBagUI>();
        if (bagUI != null)
        {
            DestroyImmediate(bagUI);
            removed = true;
        }

        if (removed)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
            Debug.Log("✅ Relic Bag UI removed successfully!");
        }
        else
        {
            Debug.Log("No Relic Bag UI found to remove.");
        }
    }
}

