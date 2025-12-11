using UnityEngine;
using static TurnManager;

public static class StatusSystem
{
    /// <summary>
    /// Called at the *start* of a side's turn (player or enemy).
    /// Decrements Strength/Weakness durations for that side's hero + minions.
    /// </summary>
    public static void OnTurnStart(Side side)
    {
        var bm = BattleManager.Instance;
        if (bm == null) return;

        // Hero for this side
        if (side == Side.Player)
        {
            if (bm.player != null)
                bm.player.OnStatusTurnStart();
        }
        else // Enemy side
        {
            if (bm.enemies != null && bm.enemies.Count > 0)
            {
                foreach (var e in bm.enemies)
                {
                    if (e != null)
                        e.OnStatusTurnStart();
                }
            }
            else if (bm.enemyEntity != null)
            {
                bm.enemyEntity.OnStatusTurnStart();
            }
        }

        // Minions for this side
        var allMinions = Object.FindObjectsOfType<MinionEntity>();
        foreach (var me in allMinions)
        {
            if (me == null) continue;

            bool ownedByPlayer = me.IsOwnedByPlayer;
            if (side == Side.Player && ownedByPlayer)
            {
                me.OnStatusTurnStart();
            }
            else if (side == Side.Enemy && !ownedByPlayer)
            {
                me.OnStatusTurnStart();
            }
        }

        RowEffectSystem.OnTurnStart(side);
    }

    /// <summary>
    /// Called at the *end* of a side's turn.
    /// Applies Poison ticks and decay for that side's hero + minions.
    /// </summary>
    public static void OnTurnEnd(Side side)
    {
        var bm = BattleManager.Instance;
        if (bm == null) return;

        // Hero for this side
        if (side == Side.Player)
        {
            if (bm.player != null)
                bm.player.OnStatusTurnEnd();
        }
        else // Enemy side
        {
            if (bm.enemies != null && bm.enemies.Count > 0)
            {
                foreach (var e in bm.enemies)
                {
                    if (e != null)
                        e.OnStatusTurnEnd();
                }
            }
            else if (bm.enemyEntity != null)
            {
                bm.enemyEntity.OnStatusTurnEnd();
            }
        }

        // Minions for this side
        var allMinions = Object.FindObjectsOfType<MinionEntity>();
        foreach (var me in allMinions)
        {
            if (me == null) continue;

            bool ownedByPlayer = me.IsOwnedByPlayer;
            if (side == Side.Player && ownedByPlayer)
            {
                me.OnStatusTurnEnd();
            }
            else if (side == Side.Enemy && !ownedByPlayer)
            {
                me.OnStatusTurnEnd();
            }
        }
    }
}
