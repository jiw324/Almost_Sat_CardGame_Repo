using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Gain Shield")]
public class GainShieldEffect : CardEffect
{
    [SerializeField] private int amount = 5;

    public override void Execute(EntityBase caster, EntityBase target)
    {
        // caster is the player in our flow
        if (caster is PlayerEntity p) p.AddShield(amount);
    }
}
