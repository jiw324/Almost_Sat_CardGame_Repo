using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    [SerializeField] public bool isRanged;
    public bool isOccupied;
    public CardInstance currentCard;

    public void PlaceCard(CardInstance card)
    {
        if (card == null)
        {
            Debug.LogError("[BoardSlot] Tried to place a null CardInstance!");
            return;
        }

        currentCard = card;
        isOccupied = true;

        Transform pillar = transform.Find("Pillar");
        Vector3 spawnPos = transform.position;

        if (pillar != null)
        {
            // Get the top of the pillar using its collider bounds
            if (pillar.TryGetComponent(out Collider col))
            {
                float pillarTopY = col.bounds.max.y;
                spawnPos = new Vector3(pillar.position.x, pillarTopY + 0.05f, pillar.position.z);
            }
            else
            {
                // fallback: just place slightly above pillar transform
                spawnPos = pillar.position + pillar.up * 0.5f;
            }
        }

        // Instantiate the 3D card prefab from BoardManager
        GameObject prefab = BoardManager.Instance.cardPrefab3D;
        if (prefab == null)
        {
            Debug.LogError("[BoardSlot] No 3D card prefab assigned in BoardManager!");
            return;
        }

        GameObject cardObject = Instantiate(prefab, spawnPos, Quaternion.identity, transform);

        // face toward camera (so the player can see the card front)
        cardObject.transform.LookAt(Camera.main.transform);
        cardObject.transform.rotation = Quaternion.Euler(0, cardObject.transform.eulerAngles.y, 0);

        // Initialize its visual info
        var controller = cardObject.GetComponent<Card3DController>();
        if (controller != null)
            controller.Initialize(card);
        else
            Debug.LogWarning("[BoardSlot] 3D card prefab missing Card3DController component!");

        Debug.Log($"[BoardSlot] Placed {card.Data.cardName} on {(isRanged ? "ranged" : "melee")} row.");
    }
}
