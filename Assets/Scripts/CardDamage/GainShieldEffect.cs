using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Gain Shield")]
public class GainShieldEffect : CardEffect
{
    [SerializeField] private int amount = 3;

    public override void Execute(EntityBase caster, EntityBase target, int value)
    {
        // Use the provided value directly (0 is valid - means no effect from minigame failure)
        // Only fall back to default amount if value is negative (shouldn't happen in normal flow)
        int shield = value >= 0 ? value : amount;
        var bm = BattleManager.Instance;
        if (bm == null)
        {
            Debug.LogWarning("GainShieldEffect: no BattleManager found.");
            return;
        }

        if (caster is PlayerEntity)
        {
            bm.playerHealth = Mathf.Min(bm.playerMaxHealth, bm.playerHealth + shield);
            if (bm.player != null)
            {
                bm.player.currentHealth = bm.playerHealth;
            }
            if (bm.uiManager != null) bm.uiManager.UpdatePlayerHealth(bm.playerHealth);
            Debug.Log($"Player healed {shield}. New health = {bm.playerHealth}");
        }
        else if (caster is EnemyEntity)
        {
            bm.enemyHealth = Mathf.Min(bm.enemyMaxHealth, bm.enemyHealth + shield);
            if (bm.enemies != null && bm.enemies.Count > 0)
            {
                foreach (var e in bm.enemies)
                {
                    if (e != null) e.currentHealth = bm.enemyHealth;
                }
            }
            else if (bm.enemyEntity != null)
            {
                bm.enemyEntity.currentHealth = bm.enemyHealth;
            }

            if (bm.uiManager != null) bm.uiManager.UpdateEnemyHealth(bm.enemyHealth);
            Debug.Log($"Enemy healed {shield}. New health = {bm.enemyHealth}");
        }
    }
}
