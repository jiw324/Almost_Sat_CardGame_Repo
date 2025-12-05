using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One-click editor utility to permanently add the Relic Bag UI into the Map3D scene.
/// After running the menu item once, the bag button & panel will be saved into Map3D.unity
/// and persist in the hierarchy.
/// </summary>
public static class RelicBagEditorUtility
{
    private const string TargetScenePath = "Assets/Scenes/Map3D.unity";
    private const string ButtonObjectName = "RelicBagButton"; // legacy UI name
    private const string PanelObjectName = "RelicBagPanel";
    private const string SpriteObjectName = "RelicBagSprite";

    [MenuItem("Tools/Relics/Setup Relic Bag In Map3D Scene")]
    public static void SetupRelicBagInMap3D()
    {
        // Open the Map3D scene
        Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);

        // Clean up any previous relic bag UI/sprite if it exists (so rerunning is safe)
        RemoveExistingRelicBag(scene);

        EnsureEventSystemExists();
        Canvas canvas = FindOrCreateCanvas();
        CreateRelicBagUI(canvas);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Relic Bag", "Relic Bag UI has been added to Map3D and saved.", "OK");
    }

    private static void EnsureEventSystemExists()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
            return;

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    private static Canvas FindOrCreateCanvas()
    {
        Canvas existing = Object.FindObjectOfType<Canvas>();
        if (existing != null)
            return existing;

        GameObject canvasGO = new GameObject("RelicUI_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateRelicBagUI(Canvas canvas)
    {
        GameObject root = new GameObject("RelicBagUIRoot", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        GameObject panel = new GameObject(PanelObjectName, typeof(RectTransform));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(600f, 400f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.localPosition = Vector3.zero;
        panelRect.offsetMin = new Vector2(-300f, -200f);
        panelRect.offsetMax = new Vector2(300f, 200f);

        Image panelImage = panel.AddComponent<Image>();
        // Match runtime popup opacity (less transparent)
        panelImage.color = new Color(0f, 0f, 0f, 0.95f);
        panelImage.raycastTarget = true;

        // Attach menu controller so baked-in panel also shows relics
        panel.AddComponent<RelicBagMenu>();

        panel.SetActive(false);

        // Create a world-space sprite that acts as the button
        GameObject spriteGO = new GameObject(SpriteObjectName);
        var spriteRenderer = spriteGO.AddComponent<SpriteRenderer>();
        Sprite iconSprite = LoadRelicBagIcon();
        if (iconSprite != null)
        {
            spriteRenderer.sprite = iconSprite;
        }
        // Match runtime size boost
        spriteGO.transform.localScale = Vector3.one * 1.5f;

        var spriteButton = spriteGO.AddComponent<RelicBagSpriteButton>();
        spriteButton.panelToToggle = panel;
        spriteButton.viewportX = 0.93f;
        spriteButton.viewportY = 0.12f;
        spriteButton.distanceFromCamera = 5f;
    }

    private static Sprite LoadRelicBagIcon()
    {
        // Direct editor load from your art folder
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/NodeIcons/LootNode.png");
        if (sprite != null)
            return sprite;

        Debug.LogWarning("[RelicBagEditorUtility] Could not find icon at Assets/Art/NodeIcons/LootNode.png");
        return null;
    }

    private static void RemoveExistingRelicBag(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                if (t.name == ButtonObjectName || t.name == SpriteObjectName || t.name == PanelObjectName || t.name == "RelicBagUIRoot")
                {
                    Object.DestroyImmediate(t.gameObject);
                }
            }
        }
    }
}


