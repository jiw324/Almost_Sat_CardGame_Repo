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

        bool casterIsPlayer = caster is PlayerEntity;

        if (casterIsPlayer)
        {
            var enemiesCopy = bm.enemies?.ToArray();
            if (enemiesCopy != null)
            {
                foreach (var enemy in enemiesCopy)
                {
                    if (enemy != null) enemy.TakeDamage(dmg);
                }
            }

            var allMinions = Object.FindObjectsOfType<MinionEntity>();
            foreach (var me in allMinions)
            {
                var mb = me.GetComponent<MinionBehaviour>();
                if (mb == null || mb.instance == null) continue;

                if (!(mb.instance.Owner is PlayerEntity))
                {
                    if (target != null && target is MinionEntity && target == me)
                        continue;

                    me.TakeDamage(dmg);
                }
            }
        }
        else
        {
            if (bm.player != null) bm.player.TakeDamage(dmg);

            var allMinions = Object.FindObjectsOfType<MinionEntity>();
            foreach (var me in allMinions)
            {
                var mb = me.GetComponent<MinionBehaviour>();
                if (mb == null || mb.instance == null) continue;

                if (mb.instance.Owner is PlayerEntity)
                {
                    if (target != null && target is MinionEntity && target == me)
                        continue;

                    me.TakeDamage(dmg);
                }
            }
        }

        Debug.Log($"AOE dealt {dmg} damage to {(casterIsPlayer ? "enemy" : "player")} side (hero + minions), skipping explicit target minion if present.");
    }
}
