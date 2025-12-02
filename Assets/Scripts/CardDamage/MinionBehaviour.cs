using UnityEngine;

public class MinionBehaviour : MonoBehaviour
{
    public BoardSlot slot { get; private set; }
    public CardInstance instance { get; private set; }

    public void Initialize(BoardSlot s, CardInstance i)
    {
        slot = s;
        instance = i;

        // if you have a world-space UI on the prefab, refresh it here
        var c3d = GetComponent<Card3DController>();
        if (c3d) c3d.Initialize(i);
    }

    public void AttackEnemy()
    {
        var bm = BattleManager.Instance;
        if (bm == null || bm.enemies.Count == 0 || instance == null) return;

        // Only allow player-owned minions to attack on click
        if (instance.Owner != bm.player) return;

        // very simple targeting: first living enemy
        EnemyEntity target = null;
        foreach (var e in bm.enemies)
        {
            if (e != null && e.currentHealth > 0) { target = e; break; }
        }
        if (target == null) return;

        int atk = instance.Attack;
        target.TakeDamage(atk);
        Debug.Log($"{instance.Data.cardName} attacked {target.name} for {atk}");
    }

    // Called by BoardManager when player has selected this minion and chosen a target EntityBase
    public void AttackTarget(EntityBase target)
    {
        if (instance == null || target == null) return;
        int atk = instance.Attack;
        target.TakeDamage(atk);
        //instance.ResolveAttackEffects(target); // if you later add attack triggers
        Debug.Log($"{instance.Data.cardName} attacked {target.name} for {atk}");
    }

    public void ReceiveDamage(int amount)
    {
        instance.TakeDamage(amount);
        if (instance.IsDead())
            Die();
    }

    public void Die()
    {
        instance.ResolveMinionDeathEffects();
        Debug.Log($"{instance.Data.cardName} died.");
        slot.ClearSlotAndDestroy();
    }


}
