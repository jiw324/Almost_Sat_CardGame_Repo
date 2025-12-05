using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Strength")]
public class StrengthEffect : CardEffect
{
    public override void Execute(EntityBase caster, EntityBase target, int value)
    {
        int turns = Mathf.Max(1, value);

        // Fallbacks:
        // - if no target but caster exists, buff caster.
        EntityBase effectiveTarget = target;

        if (effectiveTarget == null)
        {
            effectiveTarget = caster;
        }

        if (effectiveTarget == null)
        {
            Debug.LogWarning("[StrengthEffect] No valid target or caster.");
            return;
        }

        effectiveTarget.ApplyStrength(turns);
        Debug.Log($"[StrengthEffect] Applied Strength to {effectiveTarget.entityName} for {turns} turns.");
    }
}
