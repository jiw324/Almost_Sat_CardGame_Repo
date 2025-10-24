using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Damage Enemy")]
public class DamageEnemyEffect : CardEffect
{
    [SerializeField] private int amount = 5;

    public override void Execute(EntityBase caster, EntityBase target)
    {
        if (target != null) target.TakeDamage(amount);
    }
}
