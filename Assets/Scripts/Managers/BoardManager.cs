using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    public List<BoardSlot> playerMeleeRow = new();
    public List<BoardSlot> playerRangedRow = new();
    public List<BoardSlot> enemyMeleeRow = new();
    public List<BoardSlot> enemyRangedRow = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InitializeBoard()
    {
        Debug.Log("[BoardManager] Board initialized.");
        // Eventually populate rows dynamically or via inspector
    }

    public bool TryPlaceCard(CardInstance card, BoardSlot slot)
    {
        Debug.Log($"[BoardManager] Attempting to place {card.Data.cardName} on {(slot.isRanged ? "ranged" : "melee")} row.");
        if (slot.isOccupied || card.Data.isRanged != slot.isRanged)
        {
            Debug.LogWarning("[BoardManager] Invalid placement: Slot occupied or type mismatch.");
            Debug.Log($"[BoardManager] Slot occupied: {slot.isOccupied}, Card isRanged: {card.Data.isRanged}, Slot isRanged: {slot.isRanged}");
            return false;
        }
            

        slot.PlaceCard(card);
        return true;
    }
}
