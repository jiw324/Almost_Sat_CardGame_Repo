using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Row Status Aura")]
public class RowStatusAuraEffect : CardEffect
{
    public enum AuraTargetRow { Melee, Ranged }
    public enum AuraTargetSide { CasterSide, OpponentSide }
    public enum AuraStatusKind { Strength, Weakness }

    [SerializeField] private AuraTargetSide targetSide = AuraTargetSide.CasterSide;
    [SerializeField] private AuraTargetRow targetRow = AuraTargetRow.Melee;
    [SerializeField] private AuraStatusKind statusKind = AuraStatusKind.Strength;

    public override void Execute(EntityBase caster, EntityBase target, int value)
    {
        if (caster == null)
        {
            Debug.LogWarning("[RowStatusAuraEffect] Caster is null.");
            return;
        }

        int turns = Mathf.Max(1, value);
        bool casterIsPlayer = caster is PlayerEntity;

        // Decide side
        bool affectPlayerRow =
            targetSide == AuraTargetSide.CasterSide
            ? casterIsPlayer
            : !casterIsPlayer;

        // Decide row
        bool isRangedRow = targetRow == AuraTargetRow.Ranged;

        // Decide Strength or Weakness
        RowEffectSystem.RowStatusType rowStatus =
            (statusKind == AuraStatusKind.Strength)
            ? RowEffectSystem.RowStatusType.Strength
            : RowEffectSystem.RowStatusType.Weakness;

        RowEffectSystem.AddRowStatusEffect(
            isPlayerRow: affectPlayerRow,
            isRangedRow: isRangedRow,
            status: rowStatus,
            turns: turns
        );

        Debug.Log($"[RowStatusAuraEffect] Applied {statusKind} to " +
                  $"{(affectPlayerRow ? "PLAYER" : "ENEMY")} " +
                  $"{(isRangedRow ? "RANGED" : "MELEE")} row " +
                  $"for {turns} turns.");
    }
}
