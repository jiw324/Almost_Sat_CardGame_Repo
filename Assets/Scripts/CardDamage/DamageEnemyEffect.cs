using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Damage Enemy")]
public class DamageEnemyEffect : CardEffect
{
    [SerializeField] private int amount = 5;

    public override void Execute(EntityBase caster, EntityBase target, int value)
    {
        int dmg = value > 0 ? value : amount;
        if (target != null) target.TakeDamage(dmg);
    }
}