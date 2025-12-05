using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles enemy AI decision-making and card playing logic.
/// </summary>
public static class EnemyAI
{
    private const int MaxAttempts = 8;
    private const float ActionDelay = 0.5f;

    private enum EnemyPlaystyle { Offensive, Defensive }

    /// <summary>
    /// Executes the enemy's turn, playing cards until out of mana or max attempts reached.
    /// </summary>
    /// <param name="coroutineRunner">MonoBehaviour to run coroutines on (typically TurnManager)</param>
    /// <returns>IEnumerator for coroutine execution</returns>
    public static IEnumerator ExecuteTurn(MonoBehaviour coroutineRunner)
    {
        var bm = BattleManager.Instance;
        if (bm == null)
        {
            Debug.LogWarning("[EnemyAI] No BattleManager present - ending enemy turn.");
            yield break;
        }

        EntityBase enemyOwner = GetEnemyOwner(bm);
        if (enemyOwner == null)
        {
            Debug.LogWarning("[EnemyAI] No enemy Entity found to own cards.");
            yield break;
        }

        // Determine playstyle based on enemy name (simple mapping)
        EnemyPlaystyle playstyle = DeterminePlaystyleForEnemy(enemyOwner.entityName);
        Debug.Log($"[EnemyAI] Enemy '{enemyOwner.entityName}' selected playstyle: {playstyle}");

        int attempts = 0;

        while (bm.enemyMana > 0 && attempts < MaxAttempts)
        {
            attempts++;

            var json = CardDatabase.Instance.GetRandomCard();
            if (json == null)
            {
                Debug.LogWarning("[EnemyAI] No card available from CardDatabase.");
                break;
            }

            Debug.Log($"[EnemyAI] Drew card id: {json.id}, name: {json.cardName}, cost: {json.cost}");

            // Convert string ID to CardId enum
            if (!CardIdExtensions.TryParse(json.id, out CardId cardId))
            {
                Debug.LogWarning($"[EnemyAI] Could not parse card ID '{json.id}' to CardId enum.");
                break;
            }

            var inst = CardFactory.CreateCard(cardId, enemyOwner);
            if (inst == null || inst.Data == null)
            {
                Debug.LogWarning("[EnemyAI] Failed to create card instance.");
                break;
            }

            if (inst.Data.cost > bm.enemyMana)
            {
                Debug.Log($"[EnemyAI] Drew {inst.Data.cardName} (cost {inst.Data.cost}) but only has {bm.enemyMana} mana. Ending play.");
                break;
            }

            // Defensive playstyle: prefer heals and minions; avoid direct damage spells
            if (playstyle == EnemyPlaystyle.Defensive)
            {
                if (inst.IsMinion)
                {
                    if (!TryPlayMinion(inst, enemyOwner, bm))
                        break;
                }
                else
                {
                    // prefer healing spells
                    string idStr = inst.Data.id.ToString().ToLowerInvariant();
                    string nameStr = (inst.Data.cardName ?? string.Empty).ToLowerInvariant();

                    bool isHealing = idStr.Contains("healing") || nameStr.Contains("heal") || nameStr.Contains("shield");

                    if (isHealing)
                    {
                        // Play heal/self-buff targeting enemy (self)
                        TryPlaySpell(inst, enemyOwner, bm);
                    }
                    else
                    {
                        // Skip aggressive/damage spells while defensive
                        Debug.Log($"[EnemyAI] Defensive playstyle: skipping aggressive spell {inst.Data.cardName}.");
                        // allow next attempt to draw a different card
                        continue;
                    }
                }
            }
            else // Offensive (existing behavior)
            {
                if (inst.IsMinion)
                {
                    if (!TryPlayMinion(inst, enemyOwner, bm))
                        break;
                }
                else
                {
                    TryPlaySpell(inst, enemyOwner, bm);
                }
            }

            yield return new WaitForSecondsRealtime(ActionDelay);

            if (bm.playerHealth <= 0 || bm.enemyHealth <= 0) break;
        }

        // Reset enemy mana at end of turn
        if (bm != null)
        {
            bm.enemyMana = 0;
            if (bm.uiManager != null) bm.uiManager.UpdateEnemyMana(bm.enemyMana);
        }
    }

    private static EnemyPlaystyle DeterminePlaystyleForEnemy(string enemyName)
    {
        if (string.IsNullOrWhiteSpace(enemyName)) return EnemyPlaystyle.Offensive;
        string lower = enemyName.ToLowerInvariant();
        // Daniel Demon and Wendy Wraith are offensive
        if (lower.Contains("daniel") || lower.Contains("wendy") || lower.Contains("demon") || lower.Contains("wraith"))
            return EnemyPlaystyle.Offensive;
        // Evan Elf and Gary Goblin are defensive
        if (lower.Contains("evan") || lower.Contains("elf") || lower.Contains("gary") || lower.Contains("goblin"))
            return EnemyPlaystyle.Defensive;
        // default
        return EnemyPlaystyle.Offensive;
    }

    /// <summary>
    /// Gets the enemy entity that will own cards played this turn.
    /// </summary>
    private static EntityBase GetEnemyOwner(BattleManager bm)
    {
        if (bm.enemies != null && bm.enemies.Count > 0)
            return bm.enemies[0];
        else if (bm.enemyEntity != null)
            return bm.enemyEntity;
        return null;
    }

    /// <summary>
    /// Attempts to play a minion card on an enemy board slot.
    /// </summary>
    private static bool TryPlayMinion(CardInstance inst, EntityBase enemyOwner, BattleManager bm)
    {
        BoardSlot chosen = FindEnemyBoardSlot(inst.Data.isRanged);
        if (chosen == null)
        {
            Debug.Log("[EnemyAI] No valid enemy-side board slot found for minion - ending play.");
            return false;
        }

        Debug.Log($"[EnemyAI] Playing minion {inst.Data.cardName} into slot {chosen.name}");
        inst.PlayCard(chosen);
        bool placed = chosen.PlaceCard(inst);
        
        if (placed)
        {
            inst.ResolveMinionSummonEffects(enemyOwner, bm.player);

            bm.enemyMana -= inst.Data.cost;
            if (bm.uiManager != null) bm.uiManager.UpdateEnemyMana(bm.enemyMana);

            Debug.Log($"[EnemyAI] Played minion {inst.Data.cardName} for cost {inst.Data.cost}. Remaining mana: {bm.enemyMana}");
            return true;
        }
        else
        {
            Debug.LogWarning("[EnemyAI] Failed to place minion on chosen slot.");
            return false;
        }
    }

    /// <summary>
    /// Attempts to play a spell card, targeting player minions if appropriate.
    /// </summary>
    private static void TryPlaySpell(CardInstance inst, EntityBase enemyOwner, BattleManager bm)
    {
        bool requiresPlayerMinionTarget = RequiresMinionTarget(inst);
        MinionEntity chosenTarget = null;

        if (requiresPlayerMinionTarget)
        {
            chosenTarget = FindPlayerMinionTarget();
            if (chosenTarget == null)
            {
                Debug.Log($"[EnemyAI] Skipping {inst.Data.cardName}: no valid player minion targets.");
                return;
            }
        }

        EntityBase target = chosenTarget ?? (EntityBase)bm.player;
        Debug.Log($"[EnemyAI] Playing spell {inst.Data.cardName} (cost {inst.Data.cost}) targeting {(chosenTarget != null ? chosenTarget.entityName : "player side")} ");
        inst.PlayCard(null, target);

        bm.enemyMana -= inst.Data.cost;
        if (bm.uiManager != null) bm.uiManager.UpdateEnemyMana(bm.enemyMana);

        Debug.Log($"[EnemyAI] Cast spell {inst.Data.cardName} for cost {inst.Data.cost}. Remaining mana: {bm.enemyMana}");
    }

    /// <summary>
    /// Checks if a spell card requires a player minion as a target.
    /// </summary>
    private static bool RequiresMinionTarget(CardInstance inst)
    {
        if (inst.Data == null) return false;

        var id = inst.Data.id.ToPersistentId();
        var name = inst.Data.cardName ?? string.Empty;
        
        return string.Equals(id, "fireball", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(id, "slash", System.StringComparison.OrdinalIgnoreCase)
            || name.IndexOf("fireball", System.StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("slash", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// Finds the first available player-owned minion to target.
    /// </summary>
    private static MinionEntity FindPlayerMinionTarget()
    {
        var allMinions = UnityEngine.Object.FindObjectsByType<MinionEntity>(FindObjectsSortMode.None);
        foreach (var me in allMinions)
        {
            var mb = me.GetComponent<MinionBehaviour>();
            if (mb == null || mb.instance == null) continue;
            if (mb.instance.Owner is PlayerEntity)
            {
                return me; // pick first
            }
        }
        return null;
    }

    /// <summary>
    /// Finds an available enemy board slot for the specified ranged/melee type.
    /// </summary>
    private static BoardSlot FindEnemyBoardSlot(bool isRanged)
    {
        var slots = UnityEngine.Object.FindObjectsByType<BoardSlot>(FindObjectsSortMode.None);
        foreach (var s in slots)
        {
            if (s.isOccupied) continue;
            if (s.isRanged != isRanged) continue;
            if (IsEnemySlot(s)) return s;
        }
        return null;
    }

    /// <summary>
    /// Checks if a board slot belongs to the enemy side.
    /// </summary>
    private static bool IsEnemySlot(BoardSlot slot)
    {
        if (slot == null) return false;
        Transform t = slot.transform;
        while (t != null)
        {
            if (!string.IsNullOrEmpty(t.name))
            {
                var nm = t.name;
                if (nm == "MeleeB" || nm == "RangedB") return true;
                if ((nm.StartsWith("Melee") || nm.StartsWith("Ranged")) && nm.EndsWith("B")) return true;
            }
            t = t.parent;
        }
        return false;
    }
}

