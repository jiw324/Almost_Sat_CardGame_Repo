using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CardPlayHistory : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float barWidth = 400f;  // Width of the history bar
    [SerializeField] private float barOffsetX = 100f;  // X position from left
    [SerializeField] private float barOffsetY = 500f;  // Y position from bottom
    [SerializeField] private float barHeight = 700f;  // Height of the history bar
    
    [Header("Visual Settings")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.7f);
    [SerializeField] private Color playerCardColor = new Color(0.3f, 0.6f, 1f, 1f);      // Bright blue border
    [SerializeField] private Color enemyCardColor = new Color(1f, 0.2f, 0.2f, 1f);        // Bright red border
    [SerializeField] private float cardHeight = 80f;    // Taller to show image better
    [SerializeField] private float cardSpacing = 5f;
    [SerializeField] private int maxCardsShown = 20;
    
    [Header("Hover Preview")]
    [SerializeField] private GameObject cardUIPrefab;  // Same prefab used in HandManager
    [SerializeField] private Vector2 previewPosition = new Vector2(250f, 300f);  // Fixed screen position (left and lower)
    
    [Header("Test Mode")]
    [SerializeField] private bool showTestCards = false;
    [SerializeField] private int numberOfTestCards = 3;
    
    private Canvas canvas;
    private GameObject displayBarObject;
    private RectTransform contentRect;
    private List<GameObject> cardHistory = new List<GameObject>();
    private bool battleStarted = false;
    
    // Hover preview
    private GameObject hoverPreviewObject;
    private bool isPreviewShowing = false;
    
    private void Awake()
    {
        // Check if another instance already exists
        CardPlayHistory[] existing = FindObjectsByType<CardPlayHistory>(FindObjectsSortMode.None);
        if (existing.Length > 1)
        {
            Debug.Log("[PlayHistory] Another instance already exists, destroying this one.");
            Destroy(gameObject);
            return;
        }
        
        // Make this persist across scene loads
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        StartCoroutine(SetupHistoryBar());
    }
    
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Check if we're in a battle scene
        if (scene.name.Contains("Battle") || scene.name.Contains("Forest"))
        {
            Debug.Log($"[PlayHistory] Battle scene detected: {scene.name}");
            
            // Reset battle state for new battle
            battleStarted = false;
            ClearHistory();
            
            // If bar hasn't been created yet, create it
            if (displayBarObject == null)
            {
                StartCoroutine(SetupHistoryBar());
            }
            else
            {
                // Hide the bar until mulligan is confirmed
                displayBarObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log($"[PlayHistory] Non-battle scene: {scene.name}, hiding bar");
            
            // Hide the bar in non-battle scenes
            if (displayBarObject != null)
            {
                displayBarObject.SetActive(false);
            }
        }
    }
    
    private IEnumerator SetupHistoryBar()
    {
        Debug.Log("[PlayHistory] Setting up play history bar...");
        
        canvas = FindOrCreateCanvas();
        yield return null;
        
        CreateHistoryBar();
        
        // Hide the bar initially during card selection phase
        if (displayBarObject != null)
        {
            displayBarObject.SetActive(false);
        }
        
        Debug.Log("[PlayHistory] Play history bar created (hidden until battle starts)");
        
        // Start monitoring for battle start
        StartCoroutine(WaitForBattleStart());
    }
    
    private IEnumerator WaitForBattleStart()
    {
        // Don't auto-show the bar anymore - wait for manual trigger after mulligan
        Debug.Log("[PlayHistory] Waiting for mulligan confirm before showing bar");
        yield break;
    }
    
    /// <summary>
    /// Call this method when the mulligan/card selection phase ends to show the bar
    /// </summary>
    public void ShowBarAfterMulligan()
    {
        Debug.Log("[PlayHistory] Mulligan finished - showing bar now!");
        OnBattleStarted();
    }
    
    private void OnBattleStarted()
    {
        if (battleStarted) return;
        
        battleStarted = true;
        
        Debug.Log("[PlayHistory] Battle started! Showing history bar.");
        
        // Show the bar now that battle has started
        if (displayBarObject != null)
        {
            displayBarObject.SetActive(true);
        }
        
        // Show test cards if enabled (for testing)
        if (showTestCards)
        {
            ShowTestCards();
        }
    }
    
    private void ShowTestCards()
    {
        Debug.Log($"[PlayHistory] Adding {numberOfTestCards} test cards...");
        
        for (int i = 0; i < numberOfTestCards; i++)
        {
            CardData testCard = ScriptableObject.CreateInstance<CardData>();
            testCard.cardName = $"Test Card {i + 1}";
            testCard.cost = i + 1;
            testCard.type = i % 2 == 0 ? "Attack" : "Spell";
            
            // Alternate between player and enemy cards
            if (i % 2 == 0)
            {
                OnPlayerPlayedCard(testCard);
            }
            else
            {
                OnEnemyPlayedCard(testCard);
            }
        }
        
        Debug.Log("[PlayHistory] Test cards added!");
    }
    
    private Canvas FindOrCreateCanvas()
    {
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        
        if (existingCanvas != null)
        {
            Debug.Log("[PlayHistory] Found existing Canvas");
            
            // Ensure it has a GraphicRaycaster
            if (existingCanvas.GetComponent<GraphicRaycaster>() == null)
            {
                existingCanvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log("[PlayHistory] Added GraphicRaycaster to existing Canvas");
            }
            
            // Ensure it has proper CanvasScaler settings
            CanvasScaler scaler = existingCanvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = existingCanvas.gameObject.AddComponent<CanvasScaler>();
                Debug.Log("[PlayHistory] Added CanvasScaler to existing Canvas");
            }
            
            // Set to scale with screen size
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;  // Balance between width and height
            Debug.Log($"[PlayHistory] Set CanvasScaler to ScaleWithScreenSize");
            
            return existingCanvas;
        }
        
        Debug.Log("[PlayHistory] Creating Canvas...");
        GameObject canvasObj = new GameObject("Canvas");
        Canvas newCanvas = canvasObj.AddComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler newScaler = canvasObj.AddComponent<CanvasScaler>();
        newScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        newScaler.referenceResolution = new Vector2(1920, 1080);
        newScaler.matchWidthOrHeight = 0.5f;  // Balance between width and height
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        return newCanvas;
    }
    
    private void CreateHistoryBar()
    {
        Debug.Log("[PlayHistory] Creating history bar...");
        
        displayBarObject = new GameObject("PlayHistoryBar");
        displayBarObject.transform.SetParent(canvas.transform, false);
        
        RectTransform barRect = displayBarObject.AddComponent<RectTransform>();
        // Position it in the lower-left corner only (the red box area)
        barRect.anchorMin = new Vector2(0, 0);      // Bottom left corner
        barRect.anchorMax = new Vector2(0, 0);      // Bottom left corner
        barRect.pivot = new Vector2(0, 0);          // Anchor from bottom left
        barRect.anchoredPosition = new Vector2(barOffsetX, barOffsetY);  // Position from bottom
        barRect.sizeDelta = new Vector2(barWidth, barHeight);  // Size of the history bar
        
        Debug.Log($"[PlayHistory] Bar created at position ({barOffsetX}, {barOffsetY}) with size ({barWidth}, {barHeight})");
        
        Image bgImage = displayBarObject.AddComponent<Image>();
        bgImage.color = backgroundColor;
        bgImage.raycastTarget = false;  // Don't block raycasts on background
        
        GameObject scrollViewObj = new GameObject("ScrollView");
        scrollViewObj.transform.SetParent(displayBarObject.transform, false);
        
        RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.sizeDelta = Vector2.zero;
        
        ScrollRect scrollComponent = scrollViewObj.AddComponent<ScrollRect>();
        scrollComponent.horizontal = false;
        scrollComponent.vertical = true;
        scrollComponent.scrollSensitivity = 20;
        scrollComponent.movementType = ScrollRect.MovementType.Clamped;
        scrollComponent.inertia = true;
        scrollComponent.decelerationRate = 0.135f;
        
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        
        Mask maskComponent = viewportObj.AddComponent<Mask>();
        maskComponent.showMaskGraphic = false;
        
        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = new Color(1, 1, 1, 0.01f);
        viewportImage.raycastTarget = true;  // Allow scroll events
        
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        
        contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        
        VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = cardSpacing;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        
        ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scrollComponent.viewport = viewportRect;
        scrollComponent.content = contentRect;
        
        // Create hover preview panel
        CreateHoverPreview();
        
        Debug.Log("[PlayHistory] History bar created successfully");
    }
    
    private void CreateHoverPreview()
    {
        // Create a container for the card preview
        hoverPreviewObject = new GameObject("HoverPreviewContainer");
        hoverPreviewObject.transform.SetParent(canvas.transform, false);
        
        RectTransform previewRect = hoverPreviewObject.AddComponent<RectTransform>();
        // Fixed position on screen
        previewRect.anchorMin = new Vector2(0, 0);
        previewRect.anchorMax = new Vector2(0, 0);
        previewRect.pivot = new Vector2(0.5f, 0.5f);
        previewRect.anchoredPosition = previewPosition;
        previewRect.sizeDelta = new Vector2(300, 400);  // Size for card
        
        // Make it render on top
        Canvas previewCanvas = hoverPreviewObject.AddComponent<Canvas>();
        previewCanvas.overrideSorting = true;
        previewCanvas.sortingOrder = 1000;
        
        hoverPreviewObject.AddComponent<GraphicRaycaster>();
        
        // Hide initially
        hoverPreviewObject.SetActive(false);
        
        Debug.Log("[PlayHistory] Hover preview container created at fixed position");
    }
    
    public void OnPlayerPlayedCard(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[PlayHistory] OnPlayerPlayedCard called with NULL cardData!");
            return;
        }
        
        Debug.Log($"[PlayHistory] Player played: {cardData.cardName} (Type: {cardData.type})");
        AddCardToHistory(cardData, true);
    }
    
    public void OnEnemyPlayedCard(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[PlayHistory] OnEnemyPlayedCard called with NULL cardData!");
            return;
        }
        
        Debug.Log($"[PlayHistory] Enemy played: {cardData.cardName} (Type: {cardData.type})");
        AddCardToHistory(cardData, false);
    }
    
    private void AddCardToHistory(CardData cardData, bool isPlayer)
    {
        if (contentRect == null)
        {
            Debug.LogWarning("[PlayHistory] Content rect not initialized!");
            return;
        }
        
        Debug.Log($"[PlayHistory] Adding to history: {cardData.cardName} (Type: {cardData.type}, IsPlayer: {isPlayer})");
        
        GameObject cardObj = new GameObject($"{(isPlayer ? "Player" : "Enemy")}_{cardData.cardName}");
        cardObj.transform.SetParent(contentRect, false);
        
        RectTransform rect = cardObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(barWidth - 20, cardHeight);
        
        // Border with team color (must be raycastTarget for hover to work)
        Image borderImage = cardObj.AddComponent<Image>();
        borderImage.color = isPlayer ? playerCardColor : enemyCardColor;
        borderImage.raycastTarget = true;  // Enable raycasts
        
        CreateCardVisual(cardObj, cardData);
        
        // Add hover functionality
        AddHoverEvents(cardObj, cardData);
        
        cardHistory.Add(cardObj);
        
        if (cardHistory.Count > maxCardsShown)
        {
            GameObject oldestCard = cardHistory[0];
            cardHistory.RemoveAt(0);
            Destroy(oldestCard);
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        
        StartCoroutine(ScrollToBottom());
    }
    
    private void AddHoverEvents(GameObject cardObj, CardData cardData)
    {
        // Add EventTrigger component
        EventTrigger trigger = cardObj.AddComponent<EventTrigger>();
        
        // Pointer Enter (hover start)
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnCardHoverEnter(cardObj, cardData); });
        trigger.triggers.Add(entryEnter);
        
        // Pointer Exit (hover end)
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnCardHoverExit(); });
        trigger.triggers.Add(entryExit);
        
        // Add Scroll event that passes through to ScrollRect
        EventTrigger.Entry entryScroll = new EventTrigger.Entry();
        entryScroll.eventID = EventTriggerType.Scroll;
        entryScroll.callback.AddListener((data) => 
        { 
            // Pass scroll event to the ScrollRect
            var scrollRect = displayBarObject?.GetComponentInChildren<ScrollRect>();
            if (scrollRect != null)
            {
                PointerEventData pointerData = data as PointerEventData;
                if (pointerData != null)
                {
                    scrollRect.OnScroll(pointerData);
                }
            }
        });
        trigger.triggers.Add(entryScroll);
    }
    
    private void OnCardHoverEnter(GameObject cardObj, CardData cardData)
    {
        Debug.Log($"[PlayHistory] Hover ENTER on card: {cardData?.cardName}");
        
        if (hoverPreviewObject == null || cardData == null) return;
        
        if (cardUIPrefab == null)
        {
            Debug.LogWarning("[PlayHistory] Card UI Prefab not assigned! Please assign it in the Inspector.");
            return;
        }
        
        // Clear previous card
        foreach (Transform child in hoverPreviewObject.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Create a temporary CardInstance for display
        CardInstance tempCard = CardFactory.CreateCard(cardData.id, null);
        if (tempCard == null)
        {
            Debug.LogWarning($"[PlayHistory] Failed to create card instance for {cardData.cardName}");
            return;
        }
        
        // Instantiate the actual card prefab
        GameObject cardInstance = Instantiate(cardUIPrefab, hoverPreviewObject.transform);
        CardUIController controller = cardInstance.GetComponent<CardUIController>();
        
        if (controller != null)
        {
            controller.Initialize(tempCard, false);  // false = disable hover layering
            
            // Set scale and position
            cardInstance.transform.localScale = Vector3.one;
            cardInstance.transform.localPosition = Vector3.zero;
            cardInstance.transform.localRotation = Quaternion.identity;
        }
        
        // Show preview at fixed position
        hoverPreviewObject.SetActive(true);
        isPreviewShowing = true;
        
        Debug.Log("[PlayHistory] Hover preview shown with actual card prefab!");
    }
    
    private void OnCardHoverExit()
    {
        Debug.Log("[PlayHistory] Hover EXIT");
        
        if (hoverPreviewObject != null)
        {
            // Clear the card instance
            foreach (Transform child in hoverPreviewObject.transform)
            {
                Destroy(child.gameObject);
            }
            
            hoverPreviewObject.SetActive(false);
            isPreviewShowing = false;
        }
    }
    
    private void CreateCardVisual(GameObject parent, CardData cardData)
    {
        // Card artwork background (dark background for the image)
        GameObject artworkObj = new GameObject("CardArtwork");
        artworkObj.transform.SetParent(parent.transform, false);
        
        RectTransform artworkRect = artworkObj.AddComponent<RectTransform>();
        artworkRect.anchorMin = Vector2.zero;
        artworkRect.anchorMax = Vector2.one;
        artworkRect.offsetMin = new Vector2(4, 4);  // 4px padding for thicker border
        artworkRect.offsetMax = new Vector2(-4, -4);
        
        Image artworkBg = artworkObj.AddComponent<Image>();
        artworkBg.color = new Color(0.1f, 0.1f, 0.1f); // Very dark background
        
        // Card image
        if (cardData.artwork != null)
        {
            GameObject imageObj = new GameObject("CardImage");
            imageObj.transform.SetParent(artworkObj.transform, false);
            
            RectTransform imageRect = imageObj.AddComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.sizeDelta = Vector2.zero;
            
            Image cardImage = imageObj.AddComponent<Image>();
            cardImage.sprite = cardData.artwork;
            cardImage.preserveAspect = true;
            
            // Add AspectRatioFitter to maintain card proportions
            AspectRatioFitter fitter = imageObj.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        }
        else
        {
            // Fallback: Show card name if no artwork
            GameObject nameObj = new GameObject("CardName");
            nameObj.transform.SetParent(artworkObj.transform, false);
            
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = Vector2.one;
            nameRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = cardData.cardName;
            nameText.fontSize = 12;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
        }
    }
    
    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        
        ScrollRect scrollRect = displayBarObject?.GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
    
    public void ClearHistory()
    {
        foreach (var card in cardHistory)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }
        cardHistory.Clear();
        
        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }
    }
    
    public int GetHistoryCount()
    {
        return cardHistory.Count;
    }
}

