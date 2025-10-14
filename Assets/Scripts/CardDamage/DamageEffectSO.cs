using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Damage")]
public class DamageEffectSO : CardEffectSO
{
    public int amount = 5;

    private void OnEnable()
    {
        targetGroup = TargetGroup.Enemies;
    }

    public override void Execute(Actor target)
    {
        target.TakeDamage(amount);
    }
}
