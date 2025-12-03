using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/AOE Damage")]
public class AOEDamageEffect : CardEffect
{
    [SerializeField] private int damageAmount = 5;

    public override void Execute(EntityBase caster, EntityBase target, int value)
    {
        var bm = BattleManager.Instance;
        if (bm == null)
        {
            Debug.LogWarning("[AOEDamageEffect] No BattleManager found.");
            return;
        }

        // Use the magnitude of the incoming value.
        // 0 is valid (no damage if minigame failed).
        int dmg = Mathf.Abs(value);
        if (dmg <= 0)
        {
            Debug.Log("[AOEDamageEffect] Damage = 0, nothing to apply.");
            return;
        }

        //Negative value means "hit everything on the board"
        bool hitAllSides = value < 0;

        if (hitAllSides)
        {
            // Global AOE: hit player, all enemies, and all minions.
            if (bm.player != null)
                bm.player.TakeDamage(dmg);

            if (bm.enemies != null && bm.enemies.Count > 0)
            {
                foreach (var enemy in bm.enemies)
                {
                    if (enemy != null)
                        enemy.TakeDamage(dmg);
                }
            }
            else if (bm.enemyEntity != null)
            {
                bm.enemyEntity.TakeDamage(dmg);
            }

            var allMinions = Object.FindObjectsOfType<MinionEntity>();
            foreach (var me in allMinions)
            {
                if (me != null)
                    me.TakeDamage(dmg);
            }

            Debug.Log($"[AOEDamageEffect] Global AOE dealt {dmg} damage to ALL entities (heroes + minions).");
            return;
        }

        //Normal AOE behaviour: only hit the opposite side (hero + minions)
        bool casterIsPlayer = caster is PlayerEntity;

        if (casterIsPlayer)
        {
            // Hit enemy heroes
            if (bm.enemies != null && bm.enemies.Count > 0)
            {
                foreach (var enemy in bm.enemies)
                {
                    if (enemy != null)
                        enemy.TakeDamage(dmg);
                }
            }
            else if (bm.enemyEntity != null)
            {
                bm.enemyEntity.TakeDamage(dmg);
            }

            // Hit enemy minions
            var allMinions = Object.FindObjectsOfType<MinionEntity>();
            foreach (var me in allMinions)
            {
                if (me == null) continue;

                var mb = me.GetComponent<MinionBehaviour>();
                if (mb == null || mb.instance == null) continue;

                // Only damage minions not owned by the player
                if (mb.instance.Owner is PlayerEntity) continue;

                me.TakeDamage(dmg);
            }

            Debug.Log($"[AOEDamageEffect] AOE dealt {dmg} damage to ENEMY side (hero + minions).");
        }
        else
        {
            // Enemy caster: hit player hero
            if (bm.player != null)
                bm.player.TakeDamage(dmg);

            // Hit player minions
            var allMinions = Object.FindObjectsOfType<MinionEntity>();
            foreach (var me in allMinions)
            {
                if (me == null) continue;

                var mb = me.GetComponent<MinionBehaviour>();
                if (mb == null || mb.instance == null) continue;

                // Only damage minions owned by the player
                if (!(mb.instance.Owner is PlayerEntity)) continue;

                me.TakeDamage(dmg);
            }

            Debug.Log($"[AOEDamageEffect] AOE dealt {dmg} damage to PLAYER side (hero + minions).");
        }
    }
}
