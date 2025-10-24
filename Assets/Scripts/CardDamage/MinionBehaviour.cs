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
        if (bm == null || bm.enemies.Count == 0) return;

        // very simple targeting: first living enemy
        EnemyEntity target = null;
        foreach (var e in bm.enemies)
        {
            if (e != null && e.currentHealth > 0) { target = e; break; }
        }
        if (target == null) return;

        target.TakeDamage(instance.Attack);
        Debug.Log($"{instance.Data.cardName} attacked {target.name} for {instance.Attack}");
    }

    public void ReceiveDamage(int amount)
    {
        instance.TakeDamage(amount);
        if (instance.IsDead())
            Die();
    }

    public void Die()
    {
        Debug.Log($"{instance.Data.cardName} died.");
        slot.ClearSlotAndDestroy();
    }
}
