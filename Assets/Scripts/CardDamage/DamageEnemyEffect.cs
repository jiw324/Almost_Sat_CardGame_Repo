using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Damage Enemy")]
public class DamageEnemyEffect : CardEffect
{
    [SerializeField] private int amount = 5;

    public override void Execute(EntityBase caster, EntityBase target, int value)
    {
        // Use the provided value directly (0 is valid - means no damage from minigame failure)
        // Only fall back to default amount if value is negative (shouldn't happen in normal flow)
        int dmg = value >= 0 ? value : amount;
        if (target != null && dmg > 0) target.TakeDamage(dmg);
    }
}