using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform handArea;     // parent for card UI prefabs
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private PlayerEntity owner;     // who this hand belongs to
    [SerializeField] private MulliganOverlay mulliganOverlay;

    [Header("Settings")]
    [SerializeField] private int maxHandSize = 10;
    [SerializeField] private int openingHandSize = 4;
    [SerializeField] private int mulliganSampleSize = 8;

    private readonly List<CardInstance> cardsInHand = new();
    private readonly Queue<CardId> drawPile = new();
    private readonly List<CardId> discardPile = new();
    private DeckInstance sourceDeck;

    private void Start()
    {
        if (handArea == null)
            Debug.LogError("[HandManager] Missing handArea reference!");
        if (cardUIPrefab == null)
            Debug.LogError("[HandManager] Missing cardUIPrefab reference!");
        if (owner == null)
            Debug.LogWarning("[HandManager] Owner not set — using PlayerEntity in scene?");
        if (mulliganOverlay == null)
            Debug.Log("[HandManager] No mulligan overlay assigned. Opening hand will draw automatically.");
    }

    // ---- Public methods ----

    // TODO: Remove once all callers migrate to DrawCardFromDeck
    public void DrawRandomCard()
    {
        Debug.LogWarning("[HandManager] DrawRandomCard is deprecated. Use DrawCardFromDeck instead.");
        DrawCardFromDeck();
    }

    public bool DrawCardFromDeck()
    {
        if (cardsInHand.Count >= maxHandSize)
        {
            Debug.Log("[HandManager] Hand full!");
            return false;
        }

        CardInstance newCard = TakeCardFromDeck();
        if (newCard == null)
            return false;

        AddCardToHand(newCard);
        return true;
    }

    public void PrepareForBattle(DeckInstance deckInstance)
    {
        sourceDeck = deckInstance;
        if (sourceDeck == null || sourceDeck.Cards == null || sourceDeck.Cards.Count == 0)
        {
            Debug.LogWarning("[HandManager] No deck instance provided. Drawing will fall back to database random draws.");
        }

        InitializeDeck();
        if (mulliganOverlay != null)
        {
            if (!mulliganOverlay.gameObject.activeSelf)
                mulliganOverlay.gameObject.SetActive(true);
            BeginMulligan();
        }
        else
            DrawOpeningHand();
    }

    public void PrepareForBattle()
    {
        PrepareForBattle(null);
    }

    public void InitializeDeck()
    {
        ClearHand();
        drawPile.Clear();
        discardPile.Clear();

        var sourceCards = sourceDeck?.Cards;
        if (sourceCards == null || sourceCards.Count == 0)
        {
            Debug.LogWarning("[HandManager] Deck is empty. No cards enqueued.");
            return;
        }

        List<CardId> shuffled = new(sourceCards);
        Shuffle(shuffled);

        foreach (var cardId in shuffled)
        {
            drawPile.Enqueue(cardId);
        }
    }

    public void DrawOpeningHand()
    {
        if (openingHandSize <= 0)
            return;

        for (int i = 0; i < openingHandSize; i++)
        {
            if (!DrawCardFromDeck())
                break;
        }
    }

    private void BeginMulligan()
    {
        int sampleCount = mulliganSampleSize > 0 ? mulliganSampleSize : openingHandSize;
        if (sampleCount <= 0)
        {
            DrawOpeningHand();
            return;
        }

        List<CardInstance> sample = DrawCardsForMulligan(sampleCount);
        if (sample.Count == 0)
        {
            Debug.LogWarning("[HandManager] Unable to draw cards for mulligan. Falling back to automatic draw.");
            DrawOpeningHand();
            return;
        }

        int requiredSelection = Mathf.Clamp(openingHandSize, 0, sample.Count);
        mulliganOverlay.Show(sample, requiredSelection, OnMulliganComplete);
    }

    private List<CardInstance> DrawCardsForMulligan(int count)
    {
        List<CardInstance> cards = new();
        for (int i = 0; i < count; i++)
        {
            CardInstance card = TakeCardFromDeck();
            if (card == null)
                break;
            cards.Add(card);
        }

        return cards;
    }

    private void OnMulliganComplete(List<CardInstance> selected, List<CardInstance> unselected)
    {
        int added = 0;

        if (selected != null)
        {
            foreach (var card in selected)
            {
                AddCardToHand(card);
                added++;
            }
        }

        if (added < openingHandSize)
        {
            Debug.LogWarning($"[HandManager] Mulligan selection returned {added} cards, expected {openingHandSize}. Drawing additional cards to compensate.");
        }

        if (unselected != null)
        {
            ReturnCardsToDeck(unselected);
        }

        for (int i = added; i < openingHandSize; i++)
        {
            if (!DrawCardFromDeck())
                break;
        }

        if (handArea is RectTransform handRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(handRect);
        }
        }

    public void AddCardToHand(CardInstance card)
    {
        if (card == null) return;
        cardsInHand.Add(card);
        Debug.Log($"[HandManager] Added {card.Data.name} to hand.\n{card.Data.PrintCard()}");

        GameObject go = Instantiate(cardUIPrefab, handArea);
        go.name = card.Data.id.ToString();
        var controller = go.GetComponent<CardUIController>();
        controller.Initialize(card);

        // layout groups handle positioning
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
    }

    public void RemoveCardFromHand(CardInstance card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
        }
    }

    //TODO: temp for testing
    public void RemoveTopCard()
    {
        if (cardsInHand.Count == 0) return;
        CardInstance card = cardsInHand[cardsInHand.Count - 1];
        cardsInHand.RemoveAt(cardsInHand.Count - 1);
        AddToDiscard(card.Data.id);
        Destroy(handArea.GetChild(0).gameObject);
    }

    public void ClearHand()
    {
        cardsInHand.Clear();
        foreach (Transform child in handArea)
            Destroy(child.gameObject);
    }

    private CardInstance TakeCardFromDeck()
    {
        CardId? cardId = GetNextCardIdFromDeck();
        if (!cardId.HasValue)
        {
            Debug.LogWarning("[HandManager] Deck is empty. No card drawn.");
            return null;
        }

        CardInstance newCard = CardFactory.CreateCard(cardId.Value, owner);
        if (newCard == null)
            return null;

        return newCard;
    }

    private void ReturnCardsToDeck(IEnumerable<CardInstance> cards)
    {
        if (cards == null)
            return;

        List<CardId> combined = new(drawPile);

        foreach (var card in cards)
        {
            if (card?.Data == null)
                continue;
            combined.Add(card.Data.id);
        }

        Shuffle(combined);
        drawPile.Clear();
        foreach (var id in combined)
        {
            drawPile.Enqueue(id);
        }
    }

    private CardId? GetNextCardIdFromDeck()
    {
        if (drawPile.Count == 0)
        {
            if (ReloadDeckFromDiscard())
                return drawPile.Dequeue();

            if (CardDatabase.Instance.TryGetRandomCardId(out var randomId))
                return randomId;

            return null;
        }

        return drawPile.Dequeue();
    }

    public void DiscardCard(CardInstance card)
    {
        if (card == null || card.Data == null)
            return;

        RemoveCardFromHand(card);
        AddToDiscard(card.Data.id);
    }

    public void ShuffleDiscardIntoDeck()
    {
        ReloadDeckFromDiscard();
    }

    private bool ReloadDeckFromDiscard()
    {
        if (discardPile.Count == 0)
            return false;

        List<CardId> shuffled = new(discardPile);
        Shuffle(shuffled);
        Debug.Log($"[HandManager] Shuffling discard pile into draw pile ({discardPile.Count} cards).");
        foreach (var id in shuffled)
        {
            drawPile.Enqueue(id);
        }

        discardPile.Clear();
        Debug.Log($"[HandManager] Draw pile now contains {drawPile.Count} cards after reshuffle.");
        return drawPile.Count > 0;
    }

    private void AddToDiscard(CardId cardId)
    {
        discardPile.Add(cardId);
        Debug.Log($"[HandManager] Added card '{cardId}' to discard pile (size now {discardPile.Count}).");
    }

    private static void Shuffle(List<CardId> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }
}
