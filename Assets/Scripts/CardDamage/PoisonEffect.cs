using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Poison")]
public class PoisonEffect : CardEffect
{
    public override void Execute(EntityBase caster, EntityBase target, int value)
    {
        int stacks = Mathf.Max(1, value);

        if (target == null)
        {
            Debug.LogWarning("[PoisonEffect] No target to poison.");
            return;
        }

        target.ApplyPoisonStacks(stacks);
        Debug.Log($"[PoisonEffect] Applied {stacks} poison stacks to {target.entityName}.");
    }
}
