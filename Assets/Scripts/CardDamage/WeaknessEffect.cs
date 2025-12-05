using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Weakness")]
public class WeaknessEffect : CardEffect
{
    public override void Execute(EntityBase caster, EntityBase target, int value)
    {
        int turns = Mathf.Max(1, value);

        EntityBase effectiveTarget = target;
        if (effectiveTarget == null)
        {
            effectiveTarget = caster;
        }

        if (effectiveTarget == null)
        {
            Debug.LogWarning("[WeaknessEffect] No valid target or caster.");
            return;
        }

        effectiveTarget.ApplyWeakness(turns);
        Debug.Log($"[WeaknessEffect] Applied Weakness to {effectiveTarget.entityName} for {turns} turns.");
    }
}
