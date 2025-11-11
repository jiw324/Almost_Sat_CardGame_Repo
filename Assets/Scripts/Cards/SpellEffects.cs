using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/DamageEffect")]
public class DamageEffect : CardEffect
{
    public int amount = 0;

    public override void Execute(EntityBase caster, EntityBase target)
    {
        if (BattleManager.Instance == null)
        {
            Debug.LogWarning("[DamageEffect] No BattleManager instance available.");
            return;
        }

        string casterName = caster?.entityName ?? "Unknown";

        // Determine which side deals damage based on caster
        if (caster is PlayerEntity)
        {
            BattleManager.Instance.enemyHealth -= amount;
            int display = Mathf.Max(0, BattleManager.Instance.enemyHealth);
            if (BattleManager.Instance.uiManager != null)
                BattleManager.Instance.uiManager.UpdateEnemyHealth(display);
            Debug.Log($"[DamageEffect] {casterName} deals {amount} damage to Enemy. Enemy health now {BattleManager.Instance.enemyHealth}");
        }
        else if (caster is EnemyEntity)
        {
            BattleManager.Instance.playerHealth -= amount;
            int display = Mathf.Max(0, BattleManager.Instance.playerHealth);
            if (BattleManager.Instance.uiManager != null)
                BattleManager.Instance.uiManager.UpdatePlayerHealth(display);
            Debug.Log($"[DamageEffect] {casterName} deals {amount} damage to Player. Player health now {BattleManager.Instance.playerHealth}");
        }
        else
        {
            Debug.LogWarning("[DamageEffect] Unknown caster type, cannot determine target.");
        }
    }
}

[CreateAssetMenu(menuName = "Cards/Effects/HealEffect")]
public class HealEffect : CardEffect
{
    public int amount = 0;

    public override void Execute(EntityBase caster, EntityBase target)
    {
        if (BattleManager.Instance == null)
        {
            Debug.LogWarning("[HealEffect] No BattleManager instance available.");
            return;
        }

        string casterName = caster?.entityName ?? "Unknown";

        if (caster is PlayerEntity)
        {
            int max = (BattleManager.Instance.playerEntity != null) ? BattleManager.Instance.playerEntity.maxHealth : int.MaxValue;
            BattleManager.Instance.playerHealth += amount;
            if (BattleManager.Instance.playerHealth > max) BattleManager.Instance.playerHealth = max;
            int display = Mathf.Max(0, BattleManager.Instance.playerHealth);
            if (BattleManager.Instance.uiManager != null)
                BattleManager.Instance.uiManager.UpdatePlayerHealth(display);
            Debug.Log($"[HealEffect] {casterName} healed Player for {amount}. Player health now {BattleManager.Instance.playerHealth}");
        }
        else if (caster is EnemyEntity)
        {
            int max = (BattleManager.Instance.enemyEntity != null) ? BattleManager.Instance.enemyEntity.maxHealth : int.MaxValue;
            BattleManager.Instance.enemyHealth += amount;
            if (BattleManager.Instance.enemyHealth > max) BattleManager.Instance.enemyHealth = max;
            int display = Mathf.Max(0, BattleManager.Instance.enemyHealth);
            if (BattleManager.Instance.uiManager != null)
                BattleManager.Instance.uiManager.UpdateEnemyHealth(display);
            Debug.Log($"[HealEffect] {casterName} healed Enemy for {amount}. Enemy health now {BattleManager.Instance.enemyHealth}");
        }
        else
        {
            Debug.LogWarning("[HealEffect] Unknown caster type, cannot determine target.");
        }
    }
}