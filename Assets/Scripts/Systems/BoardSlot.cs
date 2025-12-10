using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    [SerializeField] public bool isRanged;
    [SerializeField] private Transform pedestal;
    public bool isOccupied;
    public CardInstance currentCard;
    private GameObject spawnedObject;
    public bool PlaceCard(CardInstance card)
    {
        if (card == null)
        {
            Debug.LogError("[BoardSlot] Tried to place a null CardInstance!");
            return false;
        }

        // if (card.Data.type == "spell")
        // {
        //     Debug.LogWarning($"[BoardSlot] You can't place spell cards on the board!");
        //     return false;
        // }

        // Check if this is an enemy slot
        bool isEnemySlot = false;
        
        // Check slot name (e.g., "enemyMeleeA", "enemyRangedB")
        if (name.StartsWith("enemy", System.StringComparison.OrdinalIgnoreCase))
        {
            isEnemySlot = true;
        }
        // Check parent name (e.g., "Enemy Melee Slots", "Enemy Ranged Slots")
        else if (transform.parent != null)
        {
            string parentName = transform.parent.name;
            if (parentName.Contains("Enemy", System.StringComparison.OrdinalIgnoreCase))
            {
                isEnemySlot = true;
            }
        }
        
        // Prevent enemy-owned cards from being placed on player-side slots
        if (card.Owner is EnemyEntity)
        {
            if (!isEnemySlot)
            {
                Debug.LogWarning($"[BoardSlot] Enemy tried to place a card on a non-enemy slot (slot='{name}', parent='{transform.parent?.name ?? "null"}'). Placement denied.");
                return false;
            }
        }
        
        // Prevent player-owned cards from being placed on enemy-side slots
        if (card.Owner is PlayerEntity)
        {
            if (isEnemySlot)
            {
                Debug.LogWarning($"[BoardSlot] Player tried to place a card on an enemy slot (slot='{name}', parent='{transform.parent?.name ?? "null"}'). Placement denied.");
                return false;
            }
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

        spawnedObject = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        spawnedObject.name = card.Data.name;
        Debug.Log($"**** Assigned name: {card.Data.name}");


        var controller = spawnedObject.GetComponent<Card3DController>();
        if (controller != null) controller.Initialize(card);
        else Debug.LogWarning("[BoardSlot] 3D card prefab missing Card3DController component!");

        if (card.IsMinion)
        {
            var mb = spawnedObject.GetComponent<MinionBehaviour>() ?? spawnedObject.AddComponent<MinionBehaviour>();
            mb.Initialize(this, card);

            // NEW: add MinionEntity so ScriptableObject effects can target minions
            var me = spawnedObject.GetComponent<MinionEntity>() ?? spawnedObject.AddComponent<MinionEntity>();
            me.Initialize(mb);

            Debug.Log($"[BoardSlot] Summoned minion {card.Data.cardName} (ATK {card.Attack}/{card.CurrentHP} HP).");
            return true;
        }

        Debug.Log($"[BoardSlot] Placed {card.Data.cardName} on {(isRanged ? "ranged" : "melee")} row.");
        return true;
    }

    public void ClearSlotAndDestroy()
    {
        if (spawnedObject) Destroy(spawnedObject);
        spawnedObject = null;
        currentCard = null;
        isOccupied = false;
    }

}
