using UnityEngine;

public abstract class EntityBase : MonoBehaviour
{
    [Header("Entity Stats")]
    public string entityName;
    public int maxHealth = 30;
    public int currentHealth = 30;

    [Header("Attack Animation")]
    [Tooltip("GameObject/Transform where minions should fly to when attacking this entity. Leave empty to use fallback calculation.")]
    public Transform attackTargetTransform;


    // Strength / Weakness: durations in *owner turns*.
    // Magnitude is fixed: Strength = +25%, Weakness = -25%.
    [Header("Status Effects")]
    [SerializeField] private int strengthTurnsRemaining = 0;
    [SerializeField] private int weaknessTurnsRemaining = 0;

    // Poison: stack-based, exponential decay via halving each tick.
    // Damage is applied at the *end* of the owner's turn, equal to poisonStacks.
    [SerializeField] private int poisonStacks = 0;

    // --- Public read-only accessors if needed for UI later ---
    public bool HasStrength => strengthTurnsRemaining > 0;
    public bool HasWeakness => weaknessTurnsRemaining > 0;
    public int PoisonStacks => poisonStacks;

    public virtual void TakeDamage(int amount)
    {
        amount = Mathf.Max(0, amount);
        currentHealth -= amount;
        Debug.Log($"{entityName} took {amount} damage. Current HP: {currentHealth}");

        var bm = BattleManager.Instance;
        if (bm != null)
        {
            if (this is PlayerEntity)
            {
                bm.playerHealth = currentHealth;
                if (bm.uiManager != null)
                    bm.uiManager.UpdatePlayerHealth(currentHealth);
            }
            else if (this is EnemyEntity)
            {
                bm.enemyHealth = currentHealth;
                if (bm.uiManager != null)
                    bm.uiManager.UpdateEnemyHealth(currentHealth);
            }

            bm.CheckBattleEnd();
        }

        if (currentHealth <= 0)
            Die();
    }

    public virtual void Die()
    {
        Debug.Log($"{entityName} has died.");
    }


    /// <summary>
    /// Apply Strength for the given number of owner turns.
    /// Stacks add more turns, not more %.
    /// </summary>
    public void ApplyStrength(int turns)
    {
        if (turns <= 0) return;
        strengthTurnsRemaining += turns;
        Debug.Log($"{entityName} gained Strength for {turns} turns (total {strengthTurnsRemaining}).");
        RefreshOwnedCardsStatusIcons();
    }

    /// <summary>
    /// Apply Weakness for the given number of owner turns.
    /// Stacks add more turns, not more %.
    /// </summary>
    public void ApplyWeakness(int turns)
    {
        if (turns <= 0) return;
        weaknessTurnsRemaining += turns;
        Debug.Log($"{entityName} gained Weakness for {turns} turns (total {weaknessTurnsRemaining}).");
        RefreshOwnedCardsStatusIcons();
    }

    /// <summary>
    /// Add poison stacks. Damage = stacks at end of each owner turn, then stacks /= 2.
    /// </summary>
    public void ApplyPoisonStacks(int stacks)
    {
        if (stacks <= 0) return;
        poisonStacks += stacks;
        Debug.Log($"{entityName} gained {stacks} Poison stacks (total {poisonStacks}).");
        RefreshOwnedCardsStatusIcons();
    }

    /// <summary>
    /// Multiplier to outgoing damage from this entity (minion attacks or spell damage).
    /// Strength = +25%, Weakness = -25%. They affect duration only, not magnitude.
    /// </summary>
    public float GetOutgoingDamageMultiplier()
    {
        float mult = 1f;

        if (HasStrength)
            mult *= 1.25f;

        if (HasWeakness)
            mult *= 0.75f;

        // Clamp to non-negative in case of odd combinations.
        if (mult < 0f)
            mult = 0f;

        return mult;
    }

    /// <summary>
    /// Called at the *start* of this entity owner's turn.
    /// Handles duration countdown for Strength / Weakness.
    /// </summary>
    public void OnStatusTurnStart()
    {
        if (strengthTurnsRemaining > 0)
        {
            strengthTurnsRemaining--;
            if (strengthTurnsRemaining <= 0)
                Debug.Log($"{entityName}'s Strength has expired.");
        }

        if (weaknessTurnsRemaining > 0)
        {
            weaknessTurnsRemaining--;
            if (weaknessTurnsRemaining <= 0)
                Debug.Log($"{entityName}'s Weakness has expired.");
        }

        RefreshOwnedCardsStatusIcons();
    }

    /// <summary>
    /// Called at the *end* of this entity owner's turn.
    /// Handles Poison tick & decay.
    /// </summary>
    public void OnStatusTurnEnd()
    {
        if (poisonStacks > 0)
        {
            int dmg = poisonStacks;
            Debug.Log($"{entityName} takes {dmg} poison damage (stacks {poisonStacks}).");
            TakeDamage(dmg);

            poisonStacks /= 2;
            if (poisonStacks <= 0)
            {
                poisonStacks = 0;
                Debug.Log($"{entityName}'s Poison has fully worn off.");
            }
            else
            {
                Debug.Log($"{entityName}'s Poison stacks decay to {poisonStacks}.");
            }

            RefreshOwnedCardsStatusIcons();
        }
    }

    private void RefreshOwnedCardsStatusIcons()
    {
        var card3D = GetComponent<Card3DController>();
        if (card3D != null)
        {
            card3D.UpdateStatusIcons();
        }
    }
}
