using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckBuilderMenu : MonoBehaviour
{
    [Header("Card Pool References")]
    [SerializeField] private Transform cardPoolArea;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private DeckDefinition availableCardsPool; // All cards player can choose from
    [SerializeField] private PlayerEntity owner;

    [Header("Deck List References")]
    [SerializeField] private Transform deckListArea;
    [SerializeField] private GameObject deckEntryPrefab; // Simple text entry prefab
    [SerializeField] private TMP_Text deckCountText; // Shows "Cards: 30/30"

    [Header("UI Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button clearDeckButton;

    [Header("Deck Rules")]
    [SerializeField] private int maxDeckSize = 30;
    [SerializeField] private int minDeckSize = 20;
    //[SerializeField] private int maxCopiesPerCard = 2;

    // Tracking
    private List<CardId> availableCards = new List<CardId>();
    private List<CardId> currentDeck = new List<CardId>();
    private List<GameObject> deckEntryObjects = new List<GameObject>();

    private void Awake()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveDeck);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(Cancel);
        if (clearDeckButton != null)
            clearDeckButton.onClick.AddListener(ClearDeck);
    }

    private void Start()
    {
        InitializeDeckBuilder();
    }

    private void InitializeDeckBuilder()
    {
        // Load the player's current deck
        DeckInstance playerDeck = SessionGrabber.getGameSession().GetPlayerDeck();
        if (playerDeck != null && playerDeck.Cards != null)
        {
            currentDeck = new List<CardId>(playerDeck.Cards);
        }

        // Load all available cards from the pool
        if (availableCardsPool != null && availableCardsPool.CardIds != null)
        {
            availableCards = new List<CardId>(availableCardsPool.CardIds);

            // Remove cards that are already in the deck from the available pool
            foreach (CardId cardInDeck in currentDeck)
            {
                availableCards.Remove(cardInDeck);
            }
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        PopulateCardPool();
        PopulateDeckList();
        UpdateDeckCount();
        UpdateSaveButtonState();
    }

    private void SortAvailableCards()
    {
        if (availableCardsPool == null || availableCardsPool.CardIds == null)
            return;

        // Create a dictionary for O(1) lookup of card indices
        var indexMap = new Dictionary<CardId, int>();
        for (int i = 0; i < availableCardsPool.CardIds.Count; i++)
        {
            indexMap[availableCardsPool.CardIds[i]] = i;
        }

        // Sort availableCards based on their order in the DeckDefinition
        availableCards = availableCards
            .OrderBy(cardId => indexMap.ContainsKey(cardId) ? indexMap[cardId] : int.MaxValue)
            .ToList();
    }

    private void PopulateCardPool()
    {
        // Sort cards before displaying
        SortAvailableCards();

        // Clear existing cards
        foreach (Transform child in cardPoolArea)
        {
            Destroy(child.gameObject);
        }

        // Create card UIs for available cards
        foreach (CardId id in availableCards)
        {
            CardInstance card = CardFactory.CreateCard(id, owner);
            if (card == null)
                continue;

            GameObject cardObj = Instantiate(cardUIPrefab, cardPoolArea);
            cardObj.name = card.Data.id.ToString();

            var controller = cardObj.GetComponent<CardUIController>();
            controller.Initialize(card);

            // Scale down the card visually
            var visualRoot = cardObj.transform.Find("VisualRoot");
            if (visualRoot != null)
            {
                visualRoot.localScale = Vector3.one * 0.5f;
            }

            // Add click handler to add card to deck
            var button = cardObj.GetComponent<Button>();
            if (button == null)
                button = cardObj.AddComponent<Button>();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => AddCardToDeck(id, cardObj));
        }

        // Force layout rebuild
        if (cardPoolArea is RectTransform rect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    private void PopulateDeckList()
    {
        // Clear existing entries
        foreach (GameObject entry in deckEntryObjects)
        {
            if (entry != null)
                Destroy(entry);
        }
        deckEntryObjects.Clear();

        // Group cards by ID and count them
        var cardCounts = new Dictionary<CardId, int>();
        foreach (CardId id in currentDeck)
        {
            if (!cardCounts.ContainsKey(id))
                cardCounts[id] = 0;
            cardCounts[id]++;
        }

        // Create text entries for each unique card
        foreach (var kvp in cardCounts)
        {
            CardId id = kvp.Key;
            int count = kvp.Value;

            CardInstance card = CardFactory.CreateCard(id, owner);
            if (card == null)
                continue;

            GameObject entryObj = Instantiate(deckEntryPrefab, deckListArea);

            var textComponent = entryObj.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                string displayText = count > 1
                    ? $"{card.Data.cardName} x{count}"
                    : card.Data.cardName;
                textComponent.text = displayText;
            }

            var iconComponent = entryObj.transform.Find("Icon");
            if (iconComponent != null)
            {
                if (card.Data.type == "spell")
                {
                    iconComponent.Find("Spell").gameObject.SetActive(true);
                }
                else if (card.Data.isRanged)
                {
                    iconComponent.Find("Ranged").gameObject.SetActive(true);
                }
                else
                {
                    iconComponent.Find("Melee").gameObject.SetActive(true);
                }
            }

            // Add click handler to remove card from deck
            var button = entryObj.GetComponent<Button>();
            if (button == null)
                button = entryObj.AddComponent<Button>();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => RemoveCardFromDeck(id));

            deckEntryObjects.Add(entryObj);
        }

        // Force layout rebuild
        if (deckListArea is RectTransform rect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    private void AddCardToDeck(CardId cardId, GameObject cardObj)
    {
        // Check deck size limit
        if (currentDeck.Count >= maxDeckSize)
        {
            Debug.LogWarning($"Deck is full! Maximum {maxDeckSize} cards.");
            AudioManager.Instance.PlaySoundById("UIButtonError");
            return;
        }

        // Add to deck
        currentDeck.Add(cardId);

        // Trigger Audio
        AudioManager.Instance.PlaySoundById("Cards");

        // Remove this specific card from the available pool
        availableCards.Remove(cardId);

        // Destroy the clicked card object immediately
        Destroy(cardObj);

        // Refresh only the deck list and counters
        PopulateDeckList();
        UpdateDeckCount();
        UpdateSaveButtonState();
    }

    private void RemoveCardFromDeck(CardId cardId)
    {
        if (!currentDeck.Contains(cardId))
            return;

        // Remove one copy from deck
        currentDeck.Remove(cardId);

        // Add back to available pool
        availableCards.Add(cardId);

        // Trigger Audio
        AudioManager.Instance.PlaySoundById("Cards");

        // Refresh both pool and deck list
        RefreshUI();
    }

    private void UpdateDeckCount()
    {
        if (deckCountText != null)
        {
            deckCountText.text = $"Cards: {currentDeck.Count}/{maxDeckSize}";

            // Color code based on validity
            if (currentDeck.Count < minDeckSize)
                deckCountText.color = Color.red;
            else if (currentDeck.Count > maxDeckSize)
                deckCountText.color = Color.red;
            else
                deckCountText.color = Color.white;
        }
    }

    private void UpdateSaveButtonState()
    {
        if (saveButton != null)
        {
            bool isValid = currentDeck.Count >= minDeckSize && currentDeck.Count <= maxDeckSize;
            saveButton.interactable = isValid;
        }
    }

    private void SaveDeck()
    {
        if (currentDeck.Count < minDeckSize || currentDeck.Count > maxDeckSize)
        {
            Debug.LogWarning($"Deck must have between {minDeckSize} and {maxDeckSize} cards.");
            AudioManager.Instance.PlaySoundById("UIButtonError");
            return;
        }

        // Create new deck instance and save to session
        DeckInstance newDeck = new DeckInstance(currentDeck);
        SessionGrabber.getGameSession().SetPlayerDeck(newDeck);

        AudioManager.Instance.PlaySoundById("UIButton");

        Debug.Log($"Deck saved with {currentDeck.Count} cards!");

        SessionGrabber.getGameSession().StartNewRun();
    }

    private void Cancel()
    {
        // Discard changes and close
        gameObject.SetActive(false);
    }

    private void ClearDeck()
    {
        // Move all cards back to available pool
        foreach (CardId cardId in currentDeck)
        {
            if (!availableCards.Contains(cardId))
                availableCards.Add(cardId);
        }

        currentDeck.Clear();
        RefreshUI();
    }
}