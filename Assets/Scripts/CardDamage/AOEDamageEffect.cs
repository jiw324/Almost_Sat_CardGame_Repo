using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/AOE Damage")]
public class AOEDamageEffect : CardEffect
{
    [SerializeField] private int damageAmount = 5;

    public override void Execute(EntityBase caster, EntityBase target)
    {
        var bm = BattleManager.Instance;
        if (bm == null)
        {
            Debug.LogWarning("AOEDamageEffect: no BattleManager found.");
            return;
        }

        foreach (var enemy in bm.enemies)
        {
            if (enemy == null) continue;
            enemy.TakeDamage(damageAmount);
        }

        Debug.Log($"AOE dealt {damageAmount} damage to all enemies.");
    }
}
