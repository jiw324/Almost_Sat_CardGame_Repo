using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform handArea;     // parent for card UI prefabs
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private PlayerEntity owner;     // who this hand belongs to
    [SerializeField] private DeckDefinition startingDeck;

    [Header("Settings")]
    [SerializeField] private int maxHandSize = 10;
    [SerializeField] private int openingHandSize = 5;

    private readonly List<CardInstance> cardsInHand = new();
    private readonly Queue<string> drawPile = new();
    private readonly List<string> discardPile = new();

    private void Start()
    {
        if (handArea == null)
            Debug.LogError("[HandManager] Missing handArea reference!");
        if (cardUIPrefab == null)
            Debug.LogError("[HandManager] Missing cardUIPrefab reference!");
        if (owner == null)
            Debug.LogWarning("[HandManager] Owner not set — using PlayerEntity in scene?");
        if (startingDeck == null)
            Debug.LogWarning("[HandManager] No starting deck assigned. Drawing will fall back to database random draws.");
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

        string cardId = GetNextCardIdFromDeck();
        if (string.IsNullOrEmpty(cardId))
        {
            Debug.LogWarning("[HandManager] Deck is empty. No card drawn.");
            return false;
        }

        CardInstance newCard = CardFactory.CreateCard(cardId, owner);
        if (newCard == null)
            return false;

        AddCardToHand(newCard);
        return true;
    }

    public void PrepareForBattle()
    {
        InitializeDeck();
        DrawOpeningHand();
    }

    public void InitializeDeck()
    {
        ClearHand();
        drawPile.Clear();
        discardPile.Clear();

        if (startingDeck == null)
            return;

        var sourceCards = startingDeck.CardIds;
        if (sourceCards == null || sourceCards.Count == 0)
        {
            Debug.LogWarning("[HandManager] Starting deck is empty. No cards enqueued.");
            return;
        }

        List<string> shuffled = new(sourceCards);
        Shuffle(shuffled);

        foreach (var cardId in shuffled)
        {
            if (string.IsNullOrWhiteSpace(cardId))
                continue;

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

    public void AddCardToHand(CardInstance card)
    {
        if (card == null) return;
        cardsInHand.Add(card);
        Debug.Log($"[HandManager] Added {card.Data.name} to hand.\n{card.Data.PrintCard()}");

        GameObject go = Instantiate(cardUIPrefab, handArea);
        go.name = card.Data.id;
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

    private string GetNextCardIdFromDeck()
    {
        if (drawPile.Count == 0)
        {
            if (ReloadDeckFromDiscard())
                return drawPile.Dequeue();

            if (startingDeck == null)
            {
                CardJSON fallback = CardDatabase.Instance.GetRandomCard();
                return fallback?.id;
            }

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

        List<string> shuffled = new(discardPile);
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

    private void AddToDiscard(string cardId)
    {
        if (string.IsNullOrWhiteSpace(cardId))
            return;

        discardPile.Add(cardId);
        Debug.Log($"[HandManager] Added card '{cardId}' to discard pile (size now {discardPile.Count}).");
    }

    private static void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }
}
