using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Gain Shield")]
public class GainShieldEffect : CardEffect
{
    [SerializeField] private int amount = 5;

    public override void Execute(EntityBase caster, EntityBase target, int value)
    {
        int shield = value > 0 ? value : amount;
        if (caster is PlayerEntity p) p.AddShield(shield);
    }
}
