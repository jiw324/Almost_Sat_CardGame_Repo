using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Shield")]
public class ShieldEffectSO : CardEffectSO
{
    public int amount = 5;

    public override void Execute(Actor target)
    {
        target.GainShield(amount);
    }
}
