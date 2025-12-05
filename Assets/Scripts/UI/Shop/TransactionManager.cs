using System;
using System.Collections.Generic;
using UnityEngine;

public class TransactionManager : MonoBehaviour
{
    private DeckInstance originalDeck;
    private int originalGold;

    public DeckInstance TempDeck { get; private set; }
    public int TempGold { get; private set; }

    private List<PurchaseEntry> pendingPurchases = new();

    public event Action<int> OnGoldChanged;

    public void Initialize(DeckInstance deck, int gold)
    {
        originalDeck = deck;
        originalGold = gold;

        TempDeck = deck.Clone();
        TempGold = gold;
    }

    public bool TogglePurchase(CardId cardId, int cost)
    {
        var entry = pendingPurchases.Find(x => x.cardId == cardId);

        if (entry == null)
        {
            // Check if player can afford it
            if (TempGold < cost)
            {
                Debug.Log($"Cannot purchase {cardId}: insufficient gold ({TempGold}/{cost})");
                return false;
            }

            // Add as purchased
            SoundEvents.Play("Coins");
            pendingPurchases.Add(new PurchaseEntry(cardId, cost));
            TempGold -= cost;
            Debug.Log($"Purchased {cardId} for {cost}. Gold: {originalGold} -> {TempGold}");
        }
        else
        {
            // Remove purchase → refund gold
            SoundEvents.Play("Coins");
            pendingPurchases.Remove(entry);
            TempGold += cost;
            Debug.Log($"Refunded {cardId} for {cost}. Gold: {TempGold}");
        }

        Debug.Log($"Invoking OnGoldChanged with {TempGold}. Subscribers: {OnGoldChanged?.GetInvocationList().Length}");
        OnGoldChanged?.Invoke(TempGold);
        return true;
    }

    public bool IsPurchased(CardId cardId)
    {
        return pendingPurchases.Exists(x => x.cardId == cardId);
    }

    public bool TryCommitChanges()
    {
        if (TempGold < 0)
            return false;

        // Apply purchases to permanent deck & gold
        foreach (var entry in pendingPurchases)
            originalDeck.AddCard(entry.cardId);

        SessionGrabber.getGameSession().SetPlayerGold(TempGold);

        return true;
    }

    private class PurchaseEntry
    {
        public CardId cardId;
        public int cost;

        public PurchaseEntry(CardId cardId, int cost)
        {
            this.cardId = cardId;
            this.cost = cost;
        }
    }
}