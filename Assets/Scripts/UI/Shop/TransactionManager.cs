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

    public event Action<int> OnGoldChanged;   // For UI updates

    public void Initialize(DeckInstance deck, int gold)
    {
        originalDeck = deck;
        originalGold = gold;

        TempDeck = deck.Clone();
        TempGold = gold;
    }

    public void TogglePurchase(CardId cardId, int cost)
    {
        // Find existing toggle
        var entry = pendingPurchases.Find(x => x.cardId == cardId);

        if (entry == null)
        {
            // Add as purchased
            pendingPurchases.Add(new PurchaseEntry(cardId, cost));
            TempGold -= cost;
        }
        else
        {
            // Remove purchase → refund gold
            pendingPurchases.Remove(entry);
            TempGold += cost;
        }

        OnGoldChanged?.Invoke(TempGold);
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