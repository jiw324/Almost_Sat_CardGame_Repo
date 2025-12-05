using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simplified Relic Bag UI - Automatically creates and manages a popup
/// Just assign the button and it works!
/// </summary>
public class SimpleRelicBagUI : MonoBehaviour
{
    [Header("Required: Assign Your Button")]
    [SerializeField] private Button bagIconButton;
    
    [Header("Optional: Customize Popup")]
    [SerializeField] private Vector2 popupSize = new Vector2(500, 600);
    [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.7f);
    
    // Auto-created UI elements
    private GameObject popupPanel;
    private GameObject contentArea;
    private TextMeshProUGUI emptyText;
    private bool isOpen = false;
    private RelicBag playerRelicBag;

    private void Start()
    {
        if (bagIconButton == null)
        {
            Debug.LogError("[SimpleRelicBagUI] Bag Icon Button not assigned!");
            return;
        }

        // Automatically add icon to button if it doesn't have one
        SetupButtonIcon();

        // Set up button click
        bagIconButton.onClick.AddListener(TogglePopup);
        
        // Find or create player's relic bag
        playerRelicBag = FindOrCreateRelicBag();
        
        // Create the popup UI (hidden initially)
        CreatePopupUI();
    }

    private void SetupButtonIcon()
    {
        // Check if button already has an Icon child
        Transform existingIcon = bagIconButton.transform.Find("Icon");
        if (existingIcon != null)
        {
            Debug.Log("[SimpleRelicBagUI] Icon already exists on button");
            return;
        }

        // Create the Icon child
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(bagIconButton.transform, false);
        
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-10, -10); // Slight padding
        iconRect.anchoredPosition = Vector2.zero;
        
        Image iconImage = iconObj.AddComponent<Image>();
        
        // Try to load the RelicNode sprite
        Sprite relicSprite = Resources.Load<Sprite>("Art/NodeIcons/RelicNode");
        if (relicSprite == null)
        {
            // Try alternate path
            relicSprite = LoadRelicNodeSprite();
        }
        
        if (relicSprite != null)
        {
            iconImage.sprite = relicSprite;
            iconImage.preserveAspect = true;
            Debug.Log("[SimpleRelicBagUI] Relic icon added to button!");
        }
        else
        {
            iconImage.color = Color.white;
            Debug.LogWarning("[SimpleRelicBagUI] Could not find RelicNode sprite. Using white placeholder.");
        }
    }

    private Sprite LoadRelicNodeSprite()
    {
        // Try to find the sprite in different possible locations
        string[] possiblePaths = {
            "Art/NodeIcons/RelicNode",
            "NodeIcons/RelicNode",
            "RelicNode"
        };
        
        foreach (string path in possiblePaths)
        {
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                return sprite;
            }
        }
        
        // If not in Resources, try to find it in the scene/assets
        UnityEngine.Object[] allSprites = Resources.FindObjectsOfTypeAll(typeof(Sprite));
        foreach (UnityEngine.Object obj in allSprites)
        {
            if (obj.name == "RelicNode")
            {
                return obj as Sprite;
            }
        }
        
        return null;
    }

    private RelicBag FindOrCreateRelicBag()
    {
        RelicBag bag = FindFirstObjectByType<RelicBag>();
        
        if (bag == null)
        {
            var session = GameSession.Instance;
            if (session != null)
            {
                bag = session.GetComponent<RelicBag>();
                if (bag == null)
                {
                    bag = session.gameObject.AddComponent<RelicBag>();
                }
            }
        }
        
        return bag;
    }

    private void CreatePopupUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SimpleRelicBagUI] No Canvas found!");
            return;
        }

        // Create main popup panel (full screen overlay)
        popupPanel = new GameObject("RelicBagPopup");
        popupPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform popupRect = popupPanel.AddComponent<RectTransform>();
        popupRect.anchorMin = Vector2.zero;
        popupRect.anchorMax = Vector2.one;
        popupRect.sizeDelta = Vector2.zero;
        
        // Add background overlay
        Image overlay = popupPanel.AddComponent<Image>();
        overlay.color = overlayColor;
        
        // Make overlay clickable to close
        Button overlayButton = popupPanel.AddComponent<Button>();
        overlayButton.onClick.AddListener(ClosePopup);
        overlayButton.transition = Selectable.Transition.None;
        
        // Create the actual panel window
        GameObject panelWindow = new GameObject("PanelWindow");
        panelWindow.transform.SetParent(popupPanel.transform, false);
        
        RectTransform panelRect = panelWindow.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = popupSize;
        panelRect.anchoredPosition = Vector2.zero;
        
        Image panelImage = panelWindow.AddComponent<Image>();
        panelImage.color = panelColor;
        
        // Create title
        GameObject title = new GameObject("Title");
        title.transform.SetParent(panelWindow.transform, false);
        
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(-20, 50);
        titleRect.anchoredPosition = new Vector2(0, -10);
        
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "Relic Bag";
        titleText.fontSize = 32;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        
        // Create close button
        GameObject closeButton = new GameObject("CloseButton");
        closeButton.transform.SetParent(panelWindow.transform, false);
        
        RectTransform closeRect = closeButton.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.sizeDelta = new Vector2(40, 40);
        closeRect.anchoredPosition = new Vector2(-10, -10);
        
        Image closeImage = closeButton.AddComponent<Image>();
        closeImage.color = Color.red;
        
        Button closeBtn = closeButton.AddComponent<Button>();
        closeBtn.onClick.AddListener(ClosePopup);
        
        GameObject closeText = new GameObject("X");
        closeText.transform.SetParent(closeButton.transform, false);
        RectTransform closeTextRect = closeText.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI closeTextTMP = closeText.AddComponent<TextMeshProUGUI>();
        closeTextTMP.text = "X";
        closeTextTMP.fontSize = 24;
        closeTextTMP.alignment = TextAlignmentOptions.Center;
        closeTextTMP.color = Color.white;
        
        // Create content area (scrollable)
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(panelWindow.transform, false);
        
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.sizeDelta = new Vector2(-20, -80);
        scrollRect.anchoredPosition = new Vector2(0, -40);
        
        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        
        // Create viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = true;
        
        // Create content
        contentArea = new GameObject("Content");
        contentArea.transform.SetParent(viewport.transform, false);
        
        RectTransform contentRect = contentArea.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 300);
        
        VerticalLayoutGroup layout = contentArea.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = false;
        
        ContentSizeFitter fitter = contentArea.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scroll.viewport = viewportRect;
        scroll.content = contentRect;
        
        // Create empty state message
        GameObject emptyMessage = new GameObject("EmptyMessage");
        emptyMessage.transform.SetParent(panelWindow.transform, false);
        
        RectTransform emptyRect = emptyMessage.AddComponent<RectTransform>();
        emptyRect.anchorMin = new Vector2(0, 0);
        emptyRect.anchorMax = new Vector2(1, 1);
        emptyRect.sizeDelta = new Vector2(-40, -100);
        emptyRect.anchoredPosition = new Vector2(0, -25);
        
        emptyText = emptyMessage.AddComponent<TextMeshProUGUI>();
        emptyText.text = "Your bag is empty!\n\nCollect relics from Relic nodes on the map.";
        emptyText.fontSize = 20;
        emptyText.alignment = TextAlignmentOptions.Center;
        emptyText.color = Color.gray;
        
        // Initially hide the popup
        popupPanel.SetActive(false);
    }

    private void TogglePopup()
    {
        if (isOpen)
        {
            ClosePopup();
        }
        else
        {
            OpenPopup();
        }
    }

    private void OpenPopup()
    {
        if (popupPanel == null) return;
        
        popupPanel.SetActive(true);
        isOpen = true;
        
        RefreshRelicDisplay();
    }

    private void ClosePopup()
    {
        if (popupPanel == null) return;
        
        popupPanel.SetActive(false);
        isOpen = false;
    }

    private void RefreshRelicDisplay()
    {
        // Clear existing items
        foreach (Transform child in contentArea.transform)
        {
            Destroy(child.gameObject);
        }

        if (playerRelicBag == null)
        {
            ShowEmptyMessage(true);
            return;
        }

        List<RelicData> relics = playerRelicBag.GetAllRelics();

        if (relics == null || relics.Count == 0)
        {
            ShowEmptyMessage(true);
            return;
        }

        ShowEmptyMessage(false);

        // Display each relic
        foreach (RelicData relic in relics)
        {
            if (relic == null) continue;
            CreateRelicItem(relic);
        }

        Debug.Log($"[SimpleRelicBagUI] Displayed {relics.Count} relics");
    }

    private void CreateRelicItem(RelicData relic)
    {
        // Create relic display item
        GameObject item = new GameObject($"Relic_{relic.relicName}");
        item.transform.SetParent(contentArea.transform, false);
        
        RectTransform itemRect = item.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(0, 100);
        
        Image itemBg = item.AddComponent<Image>();
        itemBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        // Create icon
        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(item.transform, false);
        
        RectTransform iconRect = icon.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.sizeDelta = new Vector2(80, 80);
        iconRect.anchoredPosition = new Vector2(10, 0);
        
        Image iconImage = icon.AddComponent<Image>();
        if (relic.icon != null)
        {
            iconImage.sprite = relic.icon;
        }
        iconImage.color = Color.white;
        
        // Create name
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(item.transform, false);
        
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.sizeDelta = new Vector2(-110, 30);
        nameRect.anchoredPosition = new Vector2(100, -10);
        
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = relic.relicName;
        nameText.fontSize = 20;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = Color.white;
        
        // Create description
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(item.transform, false);
        
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0);
        descRect.anchorMax = new Vector2(1, 0);
        descRect.pivot = new Vector2(0, 0);
        descRect.sizeDelta = new Vector2(-110, 50);
        descRect.anchoredPosition = new Vector2(100, 10);
        
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = relic.description;
        descText.fontSize = 14;
        descText.color = Color.gray;
        descText.enableWordWrapping = true;
        
        // Create effect value
        if (relic.effectValue > 0)
        {
            GameObject effectObj = new GameObject("EffectValue");
            effectObj.transform.SetParent(item.transform, false);
            
            RectTransform effectRect = effectObj.AddComponent<RectTransform>();
            effectRect.anchorMin = new Vector2(1, 0.5f);
            effectRect.anchorMax = new Vector2(1, 0.5f);
            effectRect.pivot = new Vector2(1, 0.5f);
            effectRect.sizeDelta = new Vector2(60, 30);
            effectRect.anchoredPosition = new Vector2(-10, 0);
            
            TextMeshProUGUI effectText = effectObj.AddComponent<TextMeshProUGUI>();
            effectText.text = $"+{relic.effectValue}";
            effectText.fontSize = 24;
            effectText.fontStyle = FontStyles.Bold;
            effectText.alignment = TextAlignmentOptions.Center;
            effectText.color = Color.green;
        }
    }

    private void ShowEmptyMessage(bool show)
    {
        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(show);
        }
    }

    private void OnDestroy()
    {
        if (bagIconButton != null)
        {
            bagIconButton.onClick.RemoveListener(TogglePopup);
        }
    }
}

