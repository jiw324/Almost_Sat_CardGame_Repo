
using UnityEngine;

[System.Serializable]
public class BoardSlot
{
    public bool isRanged;
    public bool isOccupied;
    public CardInstance currentCard;

    public void PlaceCard(CardInstance card)
    {
        currentCard = card;
        isOccupied = true;
        Debug.Log($"[BoardSlot] Placed {card.Data.cardName} on {(isRanged ? "ranged" : "melee")} row.");
    }
}
