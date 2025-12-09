using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Executes the enemy's turn in phases:
    /// Phase 1: Play cards (minions and spells) until out of mana or no playable cards
    /// Phase 2: Attack with all enemy minions that can act
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

        // Get enemy hand manager to use cards from deck
        HandManager enemyHand = bm.enemyHandManager;
        if (enemyHand == null)
        {
            Debug.LogWarning("[EnemyAI] No enemy hand manager found. Cannot play cards.");
            yield break;
        }

        // ===== PHASE 1: PLAY CARDS =====
        Debug.Log("[EnemyAI] Phase 1: Playing cards...");
        int attempts = 0;

        while (bm.enemyMana > 0 && attempts < MaxAttempts)
        {
            attempts++;

            // Get cards from enemy's hand
            var cardsInHand = enemyHand.GetCardsInHand();
            if (cardsInHand == null || cardsInHand.Count == 0)
            {
                Debug.Log("[EnemyAI] No cards in hand. Moving to attack phase.");
                break;
            }

            // Find a playable card (cost <= available mana), preferring playstyle-appropriate cards
            CardInstance inst = SelectCardToPlay(cardsInHand, bm.enemyMana, playstyle);
            if (inst == null)
            {
                Debug.Log($"[EnemyAI] No playable cards in hand (need {bm.enemyMana} mana or less). Moving to attack phase.");
                break;
            }

            Debug.Log($"[EnemyAI] Selected card from hand: {inst.Data.cardName} (cost {inst.Data.cost})");

            // Play the selected card based on type
            bool cardPlayed = false;
            if (inst.IsMinion)
            {
                cardPlayed = TryPlayMinion(inst, enemyOwner, bm);
            }
            else
            {
                // For defensive playstyle, check if spell is appropriate
                if (playstyle == EnemyPlaystyle.Defensive)
                {
                    // Check if it's a self-buff (healing/shield/strength)
                    bool isSelfBuff = IsSelfBuffSpell(inst);
                    if (!isSelfBuff)
                    {
                        // Defensive playstyle: skip damage spells, look for alternative
                        Debug.Log($"[EnemyAI] Defensive playstyle: skipping damage spell {inst.Data.cardName}. Looking for alternative.");
                        
                        // Try to find a defensive alternative
                        CardInstance alternativeCard = FindDefensiveAlternative(cardsInHand, inst, bm.enemyMana);
                        if (alternativeCard != null)
                        {
                            inst = alternativeCard;
                            Debug.Log($"[EnemyAI] Found alternative defensive card: {inst.Data.cardName}");
                        }
                        else
                        {
                            Debug.Log("[EnemyAI] No suitable defensive cards found. Moving to attack phase.");
                            break;
                        }
                    }
                }
                
                cardPlayed = TryPlaySpell(inst, enemyOwner, bm);
            }

            // If card wasn't played (e.g., placement failed), break to avoid infinite loop
            if (!cardPlayed)
            {
                Debug.LogWarning("[EnemyAI] Card selection failed to play. Moving to attack phase.");
                break;
            }

            yield return new WaitForSecondsRealtime(ActionDelay);

            if (bm.playerHealth <= 0 || bm.enemyHealth <= 0) break;
        }

        // ===== PHASE 2: ATTACK WITH MINIONS =====
        Debug.Log("[EnemyAI] Phase 2: Attacking with minions...");
        yield return coroutineRunner.StartCoroutine(AttackWithAllMinions(bm, enemyOwner));

        // Reset enemy mana at end of turn
        if (bm != null)
        {
            bm.enemyMana = 0;
            if (bm.uiManager != null) bm.uiManager.UpdateEnemyMana(bm.enemyMana);
        }
    }

    /// <summary>
    /// Selects a card to play from hand based on playstyle and available mana.
    /// </summary>
    private static CardInstance SelectCardToPlay(List<CardInstance> cardsInHand, int availableMana, EnemyPlaystyle playstyle)
    {
        CardInstance preferredCard = null;
        CardInstance fallbackCard = null;
        
        foreach (var card in cardsInHand)
        {
            if (card == null || card.Data == null || card.Data.cost > availableMana)
                continue;
            
            // For defensive playstyle, prefer minions and healing spells
            if (playstyle == EnemyPlaystyle.Defensive)
            {
                if (card.IsMinion)
                {
                    preferredCard = card;
                    break; // Minions are highest priority for defensive
                }
                else
                {
                    if (IsSelfBuffSpell(card))
                    {
                        if (preferredCard == null)
                            preferredCard = card; // Healing spells are second priority
                    }
                }
            }
            
            // For any playstyle, keep first affordable card as fallback
            if (fallbackCard == null)
                fallbackCard = card;
        }
        
        return preferredCard ?? fallbackCard;
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
            // Handle minion summon effects similar to BoardManager
            // Auto-apply self-buffs (GainShield) immediately, targeted effects need a target
            if (inst.Data?.onSummonBindings != null && inst.Data.onSummonBindings.Count > 0)
            {
                // Auto-apply GainShield effects to enemy owner immediately
                foreach (var binding in inst.Data.onSummonBindings)
                {
                    if (binding?.effect is GainShieldEffect)
                    {
                        binding.effect.Execute(enemyOwner, null, binding.value);
                    }
                }

                // For targeted effects (damage, etc.), default to player hero
                // (In the future, we could add smarter targeting logic here)
                foreach (var binding in inst.Data.onSummonBindings)
                {
                    if (binding?.effect is GainShieldEffect) continue; // Already handled
                    if (binding?.effect != null)
                    {
                        // Default target for damage effects is player hero
                        binding.effect.Execute(enemyOwner, bm.player, binding.value);
                    }
                }
            }

            bm.enemyMana -= inst.Data.cost;
            if (bm.uiManager != null) bm.uiManager.UpdateEnemyMana(bm.enemyMana);

            // Remove card from hand and add to discard
            if (bm.enemyHandManager != null)
            {
                bm.enemyHandManager.RemoveByInstance(inst);
                bm.enemyHandManager.DiscardCard(inst);
            }

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
    /// Returns true if the spell was successfully played, false otherwise.
    /// </summary>
    private static bool TryPlaySpell(CardInstance inst, EntityBase enemyOwner, BattleManager bm)
    {
        if (inst == null || inst.Data == null)
        {
            Debug.LogWarning("[EnemyAI] Invalid card instance for spell.");
            return false;
        }

        // Check if this is a self-buff spell (healing/shield/strength)
        bool isSelfBuff = IsSelfBuffSpell(inst);
        
        bool requiresPlayerMinionTarget = RequiresMinionTarget(inst);
        MinionEntity chosenTarget = null;

        if (requiresPlayerMinionTarget)
        {
            // For spells, we don't care about row type, just find any player minion
            chosenTarget = FindPlayerMinionTargetForSpell();
            if (chosenTarget == null)
            {
                Debug.Log($"[EnemyAI] Skipping {inst.Data.cardName}: no valid player minion targets.");
                return false;
            }
        }

        // Determine target: self-buffs target enemy owner, damage spells target player/minions
        EntityBase target;
        if (isSelfBuff)
        {
            target = enemyOwner; // Self-buff: target the enemy (self)
        }
        else if (chosenTarget != null)
        {
            target = chosenTarget; // Specific minion target
        }
        else
        {
            target = bm.player; // Default: target player hero
        }

        Debug.Log($"[EnemyAI] Playing spell {inst.Data.cardName} (cost {inst.Data.cost}) targeting {target?.entityName ?? "unknown"}");
        inst.PlayCard(null, target);

        bm.enemyMana -= inst.Data.cost;
        if (bm.uiManager != null) bm.uiManager.UpdateEnemyMana(bm.enemyMana);

        // Remove card from hand and add to discard
        if (bm.enemyHandManager != null)
        {
            bm.enemyHandManager.RemoveByInstance(inst);
            bm.enemyHandManager.DiscardCard(inst);
        }

        Debug.Log($"[EnemyAI] Cast spell {inst.Data.cardName} for cost {inst.Data.cost}. Remaining mana: {bm.enemyMana}");
        return true;
    }

    /// <summary>
    /// Checks if a spell is a self-buff (healing, shield, strength).
    /// </summary>
    private static bool IsSelfBuffSpell(CardInstance inst)
    {
        if (inst?.Data?.effects == null) return false;

        foreach (var binding in inst.Data.effects)
        {
            if (binding?.effect == null) continue;
            
            // Check for self-buff effect types
            if (binding.effect is GainShieldEffect || binding.effect is StrengthEffect)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Finds a defensive alternative card (minion or self-buff spell) from hand.
    /// </summary>
    private static CardInstance FindDefensiveAlternative(List<CardInstance> cardsInHand, CardInstance currentCard, int availableMana)
    {
        foreach (var card in cardsInHand)
        {
            if (card == currentCard) continue; // Skip the one we're rejecting
            if (card == null || card.Data == null || card.Data.cost > availableMana) continue;
            
            // Prefer minions
            if (card.IsMinion)
            {
                return card;
            }
            
            // Or self-buff spells
            if (IsSelfBuffSpell(card))
            {
                return card;
            }
        }
        return null;
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
    /// Finds a player minion to target for spells (any player minion, row type doesn't matter).
    /// </summary>
    private static MinionEntity FindPlayerMinionTargetForSpell()
    {
        var allMinions = UnityEngine.Object.FindObjectsByType<MinionEntity>(FindObjectsSortMode.None);
        List<MinionEntity> playerMinions = new List<MinionEntity>();
        
        foreach (var me in allMinions)
        {
            var mb = me.GetComponent<MinionBehaviour>();
            if (mb == null || mb.instance == null) continue;
            if (mb.instance.Owner is PlayerEntity)
            {
                playerMinions.Add(me);
            }
        }
        
        if (playerMinions.Count > 0)
        {
            return playerMinions[UnityEngine.Random.Range(0, playerMinions.Count)];
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
    /// New naming: slots are named "enemyMeleeA", "enemyRangedB", etc., or parent is "Enemy Melee Slots" / "Enemy Ranged Slots"
    /// </summary>
    private static bool IsEnemySlot(BoardSlot slot)
    {
        if (slot == null) return false;
        
        // Check slot name itself (e.g., "enemyMeleeA", "enemyRangedB")
        string slotName = slot.name;
        if (!string.IsNullOrEmpty(slotName) && slotName.StartsWith("enemy", System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // Check parent hierarchy for "Enemy Melee Slots" or "Enemy Ranged Slots"
        Transform t = slot.transform.parent;
        while (t != null)
        {
            if (!string.IsNullOrEmpty(t.name))
            {
                var nm = t.name;
                if (nm.Contains("Enemy", System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            t = t.parent;
        }
        
        return false;
    }

    /// <summary>
    /// Phase 2: Attack with all enemy minions that can act.
    /// </summary>
    private static IEnumerator AttackWithAllMinions(BattleManager bm, EntityBase enemyOwner)
    {
        // Find all enemy minions on the board that can act
        List<MinionBehaviour> attackableMinions = FindEnemyMinionsThatCanAct();
        
        if (attackableMinions.Count == 0)
        {
            Debug.Log("[EnemyAI] No enemy minions available to attack.");
            yield break;
        }

        Debug.Log($"[EnemyAI] Found {attackableMinions.Count} enemy minion(s) that can attack.");

        // Attack with each minion
        foreach (var minion in attackableMinions)
        {
            if (minion == null || minion.instance == null) continue;
            
            // Check if battle ended
            if (bm.playerHealth <= 0 || bm.enemyHealth <= 0) break;
            
            // Find a target for this minion
            EntityBase target = FindAttackTarget(minion, bm);
            if (target == null)
            {
                Debug.Log($"[EnemyAI] No valid target found for {minion.instance.Data.cardName}. Skipping.");
                continue;
            }

            Debug.Log($"[EnemyAI] {minion.instance.Data.cardName} attacking {target.entityName}");
            minion.AttackTarget(target);
            
            // Wait for attack animation to complete
            yield return new WaitForSecondsRealtime(ActionDelay * 2f); // Longer delay for attack animations
        }

        Debug.Log("[EnemyAI] Finished attacking with all minions.");
    }

    /// <summary>
    /// Finds all enemy minions on the board that can act (no summoning sickness, haven't acted this turn).
    /// </summary>
    private static List<MinionBehaviour> FindEnemyMinionsThatCanAct()
    {
        List<MinionBehaviour> result = new List<MinionBehaviour>();
        
        var allMinions = UnityEngine.Object.FindObjectsByType<MinionBehaviour>(FindObjectsSortMode.None);
        foreach (var mb in allMinions)
        {
            if (mb == null || mb.instance == null) continue;
            
            // Only enemy-owned minions
            if (!(mb.instance.Owner is EnemyEntity)) continue;
            
            // Must be able to act (no summoning sickness, hasn't acted this turn)
            if (!mb.CanAct) continue;
            
            // Must have attack > 0
            if (mb.instance.Attack <= 0) continue;
            
            result.Add(mb);
        }
        
        return result;
    }

    /// <summary>
    /// Finds the best target for an enemy minion to attack.
    /// Priority: Player minions > Player hero
    /// </summary>
    private static EntityBase FindAttackTarget(MinionBehaviour attacker, BattleManager bm)
    {
        if (attacker == null || attacker.instance == null || bm == null)
            return null;

        // Priority 1: Find a player minion to attack
        // Prefer attacking minions in the same row (melee vs melee, ranged vs ranged)
        MinionEntity targetMinion = FindPlayerMinionTarget(attacker.instance.Data.isRanged);
        if (targetMinion != null)
        {
            return targetMinion;
        }

        // Priority 2: Attack player hero if no minions available
        if (bm.player != null && bm.playerHealth > 0)
        {
            return bm.player;
        }

        return null;
    }

    /// <summary>
    /// Finds a player minion to target, optionally preferring same row type (ranged vs ranged, melee vs melee).
    /// </summary>
    private static MinionEntity FindPlayerMinionTarget(bool attackerIsRanged)
    {
        var allMinions = UnityEngine.Object.FindObjectsByType<MinionEntity>(FindObjectsSortMode.None);
        List<MinionEntity> sameRowMinions = new List<MinionEntity>();
        List<MinionEntity> otherMinions = new List<MinionEntity>();
        
        foreach (var me in allMinions)
        {
            var mb = me.GetComponent<MinionBehaviour>();
            if (mb == null || mb.instance == null) continue;
            if (!(mb.instance.Owner is PlayerEntity)) continue;
            
            // Check if minion is in same row type (ranged vs ranged, melee vs melee)
            if (mb.instance.Data.isRanged == attackerIsRanged)
            {
                sameRowMinions.Add(me);
            }
            else
            {
                otherMinions.Add(me);
            }
        }
        
        // Prefer same row type if available
        if (sameRowMinions.Count > 0)
        {
            return sameRowMinions[UnityEngine.Random.Range(0, sameRowMinions.Count)];
        }
        
        // Otherwise return any player minion
        if (otherMinions.Count > 0)
        {
            return otherMinions[UnityEngine.Random.Range(0, otherMinions.Count)];
        }
        
        return null;
    }
}


