using UnityEngine;

public class MinionBehaviour : MonoBehaviour
{
    public BoardSlot slot { get; private set; }
    public CardInstance instance { get; private set; }

    // --- New activation state ---
    // True the turn the minion is summoned; cleared at the start of its owner's next turn.
    private bool hasSummoningSickness = true;
    // True once the minion has performed an attack this turn.
    private bool hasActedThisTurn = false;

    /// <summary>
    /// Can this minion currently perform an attack?
    /// </summary>
    public bool CanAct
    {
        get
        {
            if (instance == null) return false;
            if (hasSummoningSickness) return false;
            if (hasActedThisTurn) return false;

            var tm = TurnManager.Instance;
            if (tm == null) return true; // fail-safe: if no turn manager, don't block

            bool ownerIsPlayer = instance.Owner is PlayerEntity;
            bool isPlayerTurn = tm.IsPlayerTurn;

            // Minion may only act during its controller's turn
            return (ownerIsPlayer && isPlayerTurn) || (!ownerIsPlayer && !isPlayerTurn);
        }
    }

    public void Initialize(BoardSlot s, CardInstance i)
    {
        slot = s;
        instance = i;

        // if you have a world-space UI on the prefab, refresh it here
        var c3d = GetComponent<Card3DController>();
        if (c3d) c3d.Initialize(i);

        hasSummoningSickness = true;
        hasActedThisTurn = false;
    }

    private void OnEnable()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStarted += HandlePlayerTurnStarted;
            TurnManager.Instance.OnEnemyTurnStarted += HandleEnemyTurnStarted;
        }
    }

    private void OnDisable()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStarted -= HandlePlayerTurnStarted;
            TurnManager.Instance.OnEnemyTurnStarted -= HandleEnemyTurnStarted;
        }
    }

    private void HandlePlayerTurnStarted()
    {
        if (instance == null) return;

        // Only care about player-owned minions on player turn
        if (instance.Owner is PlayerEntity)
        {
            // First own turn after being summoned: remove summoning sickness
            if (hasSummoningSickness)
                hasSummoningSickness = false;

            // Each new turn: reset action state
            hasActedThisTurn = false;
        }
    }

    private void HandleEnemyTurnStarted()
    {
        if (instance == null) return;

        // Only care about enemy-owned minions on enemy turn
        if (instance.Owner is EnemyEntity)
        {
            if (hasSummoningSickness)
                hasSummoningSickness = false;

            hasActedThisTurn = false;
        }
    }

    /// <summary>
    /// Old helper: auto-attack the first valid enemy hero.
    /// Now respects CanAct and delegates to AttackTarget.
    /// </summary>
    public void AttackEnemy()
    {
        var bm = BattleManager.Instance;
        if (bm == null || instance == null) return;

        if (!CanAct)
        {
            Debug.Log("[MinionBehaviour] Tried to auto-attack enemy but this minion cannot act yet (summoning sickness or already attacked).");
            return;
        }

        EnemyEntity target = null;

        // Prefer multi-enemy list if present
        if (bm.enemies != null && bm.enemies.Count > 0)
        {
            foreach (var e in bm.enemies)
            {
                if (e != null && e.currentHealth > 0)
                {
                    target = e;
                    break;
                }
            }
        }
        else if (bm.enemyEntity != null)
        {
            target = bm.enemyEntity;
        }

        if (target == null) return;

        AttackTarget(target);
    }

    /// <summary>
    /// New: explicit targeted attack (used by BoardManager click system).
    /// </summary>
    public void AttackTarget(EntityBase target)
    {
        if (instance == null || target == null) return;

        if (!CanAct)
        {
            Debug.Log("[MinionBehaviour] Tried to attack but this minion cannot act yet (summoning sickness or already attacked).");
            return;
        }

        int atk = instance.Attack;
        if (atk <= 0)
        {
            Debug.Log($"[MinionBehaviour] {instance.Data.cardName} has 0 attack and cannot deal damage.");
            hasActedThisTurn = true; // still consumes its action
            return;
        }

        // --- NEW: apply status multiplier from MinionEntity, if present ---
        int finalDamage = atk;
        var minionEntity = GetComponent<MinionEntity>();
        if (minionEntity != null)
        {
            float mult = minionEntity.GetOutgoingDamageMultiplier();
            finalDamage = Mathf.RoundToInt(atk * mult);
        }

        if (finalDamage <= 0)
        {
            Debug.Log($"[MinionBehaviour] {instance.Data.cardName}'s modified attack is <= 0, no damage dealt.");
        }
        else
        {
            target.TakeDamage(finalDamage);
        }

        hasActedThisTurn = true;

        string targetName = !string.IsNullOrEmpty(target.entityName) ? target.entityName : target.name;
        Debug.Log($"[MinionBehaviour] {instance.Data.cardName} attacked {targetName} for {finalDamage}");
    }


    public void ReceiveDamage(int amount)
    {
        if (instance == null) return;

        instance.TakeDamage(amount);
        if (instance.IsDead())
            Die();
    }

    public void Die()
    {
        if (instance != null)
        {
            instance.ResolveMinionDeathEffects();
            Debug.Log($"{instance.Data.cardName} died.");
        }

        if (slot != null)
            slot.ClearSlotAndDestroy();
        else
            Destroy(gameObject);
    }
}
