using System.Collections.Generic;
using UnityEngine;
using static TurnManager;  // for Side enum

/// <summary>
/// Row-level status auras (Strength / Weakness) that affect minions on a whole row
/// for a number of that side's turns.
/// </summary>
public static class RowEffectSystem
{
    public enum RowStatusType
    {
        Strength,
        Weakness
    }

    private class RowEffect
    {
        public bool isPlayerRow;      // true = player's side, false = enemy's side
        public bool isRangedRow;      // true = ranged row, false = melee row
        public RowStatusType status;
        public int turnsRemaining;    // how many of that side's OWN turns remain
    }

    private static readonly List<RowEffect> activeEffects = new();

    /// <summary>
    /// Add a Strength or Weakness aura to a row for some number of that side's turns.
    /// </summary>
    public static void AddRowStatusEffect(bool isPlayerRow, bool isRangedRow, RowStatusType status, int turns)
    {
        if (turns <= 0) return;

        activeEffects.Add(new RowEffect
        {
            isPlayerRow = isPlayerRow,
            isRangedRow = isRangedRow,
            status = status,
            turnsRemaining = turns
        });

        Debug.Log($"[RowEffectSystem] Added {status} to " +
                  $"{(isPlayerRow ? "PLAYER" : "ENEMY")} " +
                  $"{(isRangedRow ? "RANGED" : "MELEE")} row for {turns} turns.");
    }

    /// <summary>
    /// Compute the row-level multiplier (from Strength/Weakness auras) for a minion.
    /// This does NOT include the minion's own Strength/Weakness ¨C that's already in EntityBase.
    /// </summary>
    public static float GetRowDamageMultiplier(bool isPlayerRow, bool isRangedRow)
    {
        bool hasStrength = false;
        bool hasWeakness = false;

        foreach (var e in activeEffects)
        {
            if (e.turnsRemaining <= 0) continue;
            if (e.isPlayerRow != isPlayerRow) continue;
            if (e.isRangedRow != isRangedRow) continue;

            if (e.status == RowStatusType.Strength)
                hasStrength = true;
            else if (e.status == RowStatusType.Weakness)
                hasWeakness = true;
        }

        // Use the same semantics as EntityBase.GetOutgoingDamageMultiplier:
        float mult = 1f;

        if (hasStrength)
            mult *= 1.25f;

        if (hasWeakness)
            mult *= 0.75f;

        if (mult < 0f) mult = 0f;
        return mult;
    }

    /// <summary>
    /// Called at the start of a side's turn. Decrements that side's row aura durations.
    /// </summary>
    public static void OnTurnStart(Side side)
    {
        bool isPlayerRow = side == Side.Player;

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var e = activeEffects[i];
            if (e.isPlayerRow != isPlayerRow) continue;

            e.turnsRemaining--;
            if (e.turnsRemaining <= 0)
            {
                Debug.Log($"[RowEffectSystem] Expired {e.status} on " +
                          $"{(e.isPlayerRow ? "PLAYER" : "ENEMY")} " +
                          $"{(e.isRangedRow ? "RANGED" : "MELEE")} row.");
                activeEffects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Clear all row auras (e.g. when starting a new battle).
    /// </summary>
    public static void ClearAll()
    {
        activeEffects.Clear();
    }
}
