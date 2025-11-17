using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/AOE Damage")]
public class AOEDamageEffect : CardEffect
{
    [SerializeField] private int damageAmount = 5;

    public override void Execute(EntityBase caster, EntityBase target, int value)
    {
        int dmg = value > 0 ? value : damageAmount;

        var bm = BattleManager.Instance;
        if (bm == null)
        {
            Debug.LogWarning("AOEDamageEffect: no BattleManager found.");
            return;
        }

        // 1) Enemy hero/entities
        foreach (var enemy in bm.enemies)
        {
            if (enemy != null) enemy.TakeDamage(dmg);
        }

        // 2) Enemy minions only (don¡¯t hurt player minions)
        var allMinions = Object.FindObjectsOfType<MinionEntity>();
        foreach (var me in allMinions)
        {
            var mb = me.GetComponent<MinionBehaviour>();
            if (mb == null || mb.instance == null) continue;

            // treat anything not owned by bm.player as "enemy minion"
            if (mb.instance.Owner != bm.player)
                me.TakeDamage(dmg);
        }

        Debug.Log($"AOE dealt {dmg} damage to enemy side (hero + minions).");
    }
}
