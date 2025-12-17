using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AutoCardDisplaySetup : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float barWidth = 250f;
    [SerializeField] private float barOffsetX = 10f;
    [SerializeField] private bool showPlayHistory = false;
    [SerializeField] private bool showPlayerDeck = false;
    
    [Header("Visual Settings")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.7f);
    [SerializeField] private float cardHeight = 80f;
    [SerializeField] private float cardSpacing = 5f;
    
    [Header("Test Mode")]
    [SerializeField] private bool useTestMode = true;
    [SerializeField] private int numberOfTestCards = 5;
    
    private Canvas canvas;
    private GameObject displayBarObject;
    private CardDisplayBar displayBar;
    private GameObject cardItemPrefab;
    
    private void Start()
    {
        StartCoroutine(SetupEverything());
    }
    
    private IEnumerator SetupEverything()
    {
        Debug.Log("[AutoSetup] Starting automatic setup...");
        
        canvas = FindOrCreateCanvas();
        yield return null;
        
        CreateCardDisplayItemPrefab();
        yield return null;
        
        CreateDisplayBar();
        yield return new WaitForSeconds(0.5f);
        
        PopulateBar();
        
        Debug.Log("[AutoSetup] Setup complete! Bar should be visible on the left side.");
    }
    
    private Canvas FindOrCreateCanvas()
    {
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        
        if (existingCanvas != null)
        {
            Debug.Log("[AutoSetup] Found existing Canvas");
            return existingCanvas;
        }
        
        Debug.Log("[AutoSetup] Creating Canvas...");
        GameObject canvasObj = new GameObject("Canvas");
        Canvas newCanvas = canvasObj.AddComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("[AutoSetup] Canvas created successfully");
        return newCanvas;
    }
    
    private void CreateCardDisplayItemPrefab()
    {
        Debug.Log("[AutoSetup] Creating card display item prefab...");
        
        cardItemPrefab = new GameObject("CardDisplayItem");
        
        RectTransform rect = cardItemPrefab.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(barWidth - 20, cardHeight);
        
        Image bgImage = cardItemPrefab.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        CardDisplayItem displayItem = cardItemPrefab.AddComponent<CardDisplayItem>();
        
        CreateCardNameText(cardItemPrefab);
        CreateCardCostText(cardItemPrefab);
        CreateCardTypeText(cardItemPrefab);
        
        var bgField = typeof(CardDisplayItem).GetField("backgroundImage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (bgField != null) bgField.SetValue(displayItem, bgImage);
        
        Debug.Log("[AutoSetup] Card display item prefab created");
    }
    
    private void CreateCardNameText(GameObject parent)
    {
        GameObject textObj = new GameObject("CardName");
        textObj.transform.SetParent(parent.transform);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(10, 0);
        rect.offsetMax = new Vector2(-10, -5);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        text.fontStyle = FontStyles.Bold;
        
        var nameField = typeof(CardDisplayItem).GetField("cardNameText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (nameField != null)
        {
            CardDisplayItem displayItem = parent.GetComponent<CardDisplayItem>();
            if (displayItem != null) nameField.SetValue(displayItem, text);
        }
    }
    
    private void CreateCardCostText(GameObject parent)
    {
        GameObject textObj = new GameObject("CardCost");
        textObj.transform.SetParent(parent.transform);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0.3f, 0.5f);
        rect.pivot = new Vector2(0, 0);
        rect.offsetMin = new Vector2(10, 5);
        rect.offsetMax = new Vector2(0, -5);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 12;
        text.color = Color.cyan;
        text.alignment = TextAlignmentOptions.Left;
        
        var costField = typeof(CardDisplayItem).GetField("cardCostText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (costField != null)
        {
            CardDisplayItem displayItem = parent.GetComponent<CardDisplayItem>();
            if (displayItem != null) costField.SetValue(displayItem, text);
        }
    }
    
    private void CreateCardTypeText(GameObject parent)
    {
        GameObject textObj = new GameObject("CardType");
        textObj.transform.SetParent(parent.transform);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.3f, 0);
        rect.anchorMax = new Vector2(1, 0.5f);
        rect.pivot = new Vector2(0, 0);
        rect.offsetMin = new Vector2(5, 5);
        rect.offsetMax = new Vector2(-10, -5);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 11;
        text.color = new Color(0.8f, 0.8f, 0.8f);
        text.alignment = TextAlignmentOptions.Left;
        
        var typeField = typeof(CardDisplayItem).GetField("cardTypeText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (typeField != null)
        {
            CardDisplayItem displayItem = parent.GetComponent<CardDisplayItem>();
            if (displayItem != null) typeField.SetValue(displayItem, text);
        }
    }
    
    private void CreateDisplayBar()
    {
        Debug.Log("[AutoSetup] Creating display bar...");
        
        displayBarObject = new GameObject("CardDisplayBar_Left");
        displayBarObject.transform.SetParent(canvas.transform, false);
        
        RectTransform barRect = displayBarObject.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0, 0);
        barRect.anchorMax = new Vector2(0, 1);
        barRect.pivot = new Vector2(0, 0.5f);
        barRect.anchoredPosition = new Vector2(barOffsetX, 0);
        barRect.sizeDelta = new Vector2(barWidth, 0);
        
        Image bgImage = displayBarObject.AddComponent<Image>();
        bgImage.color = backgroundColor;
        
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
        
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        
        viewportObj.AddComponent<Mask>().showMaskGraphic = false;
        viewportObj.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
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
        
        displayBar = displayBarObject.AddComponent<CardDisplayBar>();
        
        var prefabField = typeof(CardDisplayBar).GetField("cardDisplayPrefab",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var containerField = typeof(CardDisplayBar).GetField("cardContainer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (prefabField != null) prefabField.SetValue(displayBar, cardItemPrefab);
        if (containerField != null) containerField.SetValue(displayBar, contentRect);
        
        Debug.Log("[AutoSetup] Display bar created successfully");
    }
    
    private void PopulateBar()
    {
        if (displayBar == null)
        {
            Debug.LogWarning("[AutoSetup] Display bar not initialized!");
            return;
        }
        
        if (useTestMode)
        {
            ShowTestCards();
        }
        else if (showPlayHistory)
        {
            SetupPlayHistory();
        }
        else if (showPlayerDeck)
        {
            ShowPlayerDeck();
        }
        
        Debug.Log("[AutoSetup] Bar populated with cards");
    }
    
    private void ShowTestCards()
    {
        Debug.Log($"[AutoSetup] Adding {numberOfTestCards} test cards...");
        
        for (int i = 0; i < numberOfTestCards; i++)
        {
            CardData testCard = ScriptableObject.CreateInstance<CardData>();
            testCard.cardName = $"Test Card {i + 1}";
            testCard.cost = (i % 10);
            testCard.type = i % 2 == 0 ? "Attack" : "Skill";
            testCard.description = "Test card for display";
            
            displayBar.AddCard(testCard);
        }
    }
    
    private void SetupPlayHistory()
    {
        Debug.Log("[AutoSetup] Setting up play history tracking...");
        
        GameObject historyObj = new GameObject("PlayHistoryTracker");
        historyObj.transform.SetParent(transform);
        
        CardPlayHistoryDisplay historyDisplay = historyObj.AddComponent<CardPlayHistoryDisplay>();
        
        var displayBarField = typeof(CardPlayHistoryDisplay).GetField("displayBar",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cardPrefabField = typeof(CardPlayHistoryDisplay).GetField("cardDisplayPrefab",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (displayBarField != null) displayBarField.SetValue(historyDisplay, displayBar);
        if (cardPrefabField != null) cardPrefabField.SetValue(historyDisplay, cardItemPrefab);
        
        Debug.Log("[AutoSetup] Play history tracking enabled");
    }
    
    private void ShowPlayerDeck()
    {
        Debug.Log("[AutoSetup] Loading player deck...");
        
        GameSession session = SessionGrabber.getGameSession();
        if (session != null)
        {
            DeckInstance playerDeck = session.GetPlayerDeck();
            if (playerDeck != null && playerDeck.Cards != null)
            {
                int cardsAdded = 0;
                foreach (var cardId in playerDeck.Cards)
                {
                    if (CardDatabase.Instance != null)
                    {
                        CardJSON cardJson = CardDatabase.Instance.GetCardById(cardId);
                        if (cardJson != null)
                        {
                            CardInstance tempInstance = CardFactory.CreateCard(cardId, null);
                            if (tempInstance != null && tempInstance.Data != null)
                            {
                                displayBar.AddCard(tempInstance.Data);
                                cardsAdded++;
                            }
                        }
                    }
                }
                Debug.Log($"[AutoSetup] Added {cardsAdded} cards from player deck");
            }
        }
        else
        {
            Debug.LogWarning("[AutoSetup] GameSession not found - showing test cards instead");
            ShowTestCards();
        }
    }
    
    public void AddCard(CardData card)
    {
        if (displayBar != null && card != null)
        {
            displayBar.AddCard(card);
        }
    }
    
    public CardDisplayBar GetDisplayBar()
    {
        return displayBar;
    }
}

