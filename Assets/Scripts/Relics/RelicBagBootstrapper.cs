using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public static class RelicBagBootstrapper
{
    private const string TargetSceneName = "Map3D";
    private const string ButtonObjectName = "RelicBagButton"; // legacy UI name
    private const string PanelObjectName = "RelicBagPanel";
    private const string SpriteObjectName = "RelicBagSprite";

    /// <summary>
    /// Register for scene load so we can inject UI when Map3D is actually loaded,
    /// even if another scene (like Bootstrap) was loaded first.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != TargetSceneName)
            return;

        EnsureEventSystemExists();

        // If the scene already has a relic bag (e.g. baked in via editor tool),
        // just make sure the trigger is wired to toggle the panel.
        if (SceneAlreadyHasRelicBag(scene))
        {
            WireExistingRelicBag(scene);
            Debug.Log("[RelicBagBootstrapper] Found existing Relic Bag in scene, ensured wiring.");
            return;
        }

        // Otherwise, create a fresh UI for this scene.
        EnsureEventSystemExists();

        Canvas canvas = FindOrCreateCanvas();
        CreateRelicBagUI(canvas);
    }

    /// <summary>
    /// Checks the loaded scene (including inactive objects) to see
    /// if a Relic Bag trigger already exists (sprite or legacy button).
    /// </summary>
    private static bool SceneAlreadyHasRelicBag(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            // includeInactive: true so we find buttons even if a parent is disabled
            var transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                if (t.name == SpriteObjectName || t.name == ButtonObjectName)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// For an existing baked-in relic bag, ensure the trigger toggles the panel.
    /// </summary>
    private static void WireExistingRelicBag(Scene scene)
    {
        Transform foundSprite = null;
        Transform foundButton = null;
        Transform foundPanel = null;

        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                if (t.name == SpriteObjectName)
                    foundSprite = t;
                else if (t.name == ButtonObjectName)
                    foundButton = t;
                else if (t.name == PanelObjectName)
                    foundPanel = t;
            }
        }

        if (foundPanel == null)
        {
            Debug.LogWarning("[RelicBagBootstrapper] Could not find RelicBagPanel to wire.");
            return;
        }

        // Ensure the menu controller is present (but do NOT change the panel's layout/position)
        var menu = foundPanel.GetComponent<RelicBagMenu>();
        if (menu == null)
        {
            menu = foundPanel.gameObject.AddComponent<RelicBagMenu>();
        }

        // Prefer sprite-based trigger if present
        if (foundSprite != null)
        {
            var spriteButton = foundSprite.GetComponent<RelicBagSpriteButton>();
            if (spriteButton == null)
                spriteButton = foundSprite.gameObject.AddComponent<RelicBagSpriteButton>();

            spriteButton.panelToToggle = foundPanel.gameObject;
            return;
        }

        // Legacy UI button fallback
        if (foundButton != null)
        {
            var button = foundButton.GetComponent<Button>();
            if (button == null)
            {
                button = foundButton.gameObject.AddComponent<Button>();
            }

            var panelGO = foundPanel.gameObject;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                bool isActive = panelGO.activeSelf;
                panelGO.SetActive(!isActive);
            });
        }
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

        // Add common UI components
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        return canvas;
    }

    private static void CreateRelicBagUI(Canvas canvas)
    {
        // Root container under the canvas (helps keep things grouped)
        GameObject root = new GameObject("RelicBagUIRoot", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // Create the bag panel (centered, hidden by default) as UI
        GameObject panel = new GameObject(PanelObjectName, typeof(RectTransform));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(600f, 400f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        // Slightly less transparent popup background
        panelImage.color = new Color(0f, 0f, 0f, 0.95f);
        panelImage.raycastTarget = true;

        // Attach menu controller to handle grid + relic display
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
        // Make the sprite reasonably large
        spriteGO.transform.localScale = Vector3.one * 1.5f;

        // Attach behavior to position it near bottom-right of the camera and toggle the panel
        var spriteButton = spriteGO.AddComponent<RelicBagSpriteButton>();
        spriteButton.panelToToggle = panel;
        spriteButton.viewportX = 0.93f;
        spriteButton.viewportY = 0.12f;
        spriteButton.distanceFromCamera = 5f;
    }

    /// <summary>
    /// Load the LootNode icon from the project to use as the relic bag icon.
    /// Tries Resources first (for builds), then falls back to AssetDatabase in the editor.
    /// </summary>
    private static Sprite LoadRelicBagIcon()
    {
        // Preferred: via Resources folder (for builds).
        Sprite sprite = Resources.Load<Sprite>("NodeIcons/LootNode");
        if (sprite != null)
            return sprite;

#if UNITY_EDITOR
        // Editor-only fallback: load directly via AssetDatabase from Assets/Art/NodeIcons/LootNode.png
        sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/NodeIcons/LootNode.png");
        if (sprite != null)
            return sprite;
#endif

        Debug.LogWarning("[RelicBagBootstrapper] Could not find icon 'LootNode'. Button will use text only.");
        return null;
    }
}


