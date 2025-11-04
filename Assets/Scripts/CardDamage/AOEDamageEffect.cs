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

        foreach (var enemy in bm.enemies)
        {
            if (enemy == null) continue;
            enemy.TakeDamage(dmg);
        }

        Debug.Log($"AOE dealt {dmg} damage to all enemies.");
    }
}
