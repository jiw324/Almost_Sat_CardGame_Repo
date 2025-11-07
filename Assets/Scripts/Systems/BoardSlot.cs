using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    [SerializeField] public bool isRanged;
    [SerializeField] private Transform pedestal;
    public bool isOccupied;
    public CardInstance currentCard;

    public bool PlaceCard(CardInstance card)
    {
        if (card == null)
        {
            Debug.LogError("[BoardSlot] Tried to place a null CardInstance!");
            return false;
        }
        if (card.Data.type == "spell")
        {
            Debug.LogWarning($"[BoardSlot] You can't place spell cards on the board!");
            return false;
        }
        if (card.Data.isRanged != isRanged)
        {
            Debug.LogWarning($"[BoardSlot] Tried to place a " +
                $"{(card.Data.isRanged ? "ranged" : "melee")} card in a " +
                $"{(isRanged ? "ranged" : "melee")} slot!");
            return false;
        }

        currentCard = card;
        isOccupied = true;
        Vector3 spawnPos = transform.position;

        if (pedestal != null)
        {
            // Get the top of the pedestal using its collider bounds
            if (pedestal.TryGetComponent(out Collider col))
            {
                float pillarTopY = col.bounds.max.y;
                spawnPos = new Vector3(pedestal.position.x, pillarTopY + 0.05f, pedestal.position.z);
            }
            else
            {
                // fallback: just place slightly above pedestal transform
                spawnPos = pedestal.position + pedestal.up * 0.5f;
            }
        }

        // Instantiate the 3D card prefab from BoardManager
        GameObject prefab = BoardManager.Instance.cardPrefab3D;
        if (prefab == null)
        {
            Debug.LogError("[BoardSlot] No 3D card prefab assigned in BoardManager!");
            return false;
        }

        GameObject cardObject = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        cardObject.name = card.Data.name;
        Debug.Log($"**** Assigned name: {card.Data.name}");

        // Initialize its visual info
        var controller = cardObject.GetComponent<Card3DController>();
        if (controller != null)
            controller.Initialize(card);
        else
            Debug.LogError("[BoardSlot] 3D card prefab missing Card3DController component!");

        Debug.Log($"[BoardSlot] Placed {card.Data.cardName} on {(isRanged ? "ranged" : "melee")} row.");
        return true;
    }
}
