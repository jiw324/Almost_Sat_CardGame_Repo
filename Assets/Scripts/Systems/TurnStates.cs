using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTurnState : TurnStateBase
{
    public PlayerTurnState(TurnManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("[TurnStates] 1. Enter Player Turn");

        // Reset player mana at start of their turn
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.playerMana = BattleManager.Instance.playerMaxMana;
            if (BattleManager.Instance.uiManager != null) BattleManager.Instance.uiManager.UpdatePlayerMana(BattleManager.Instance.playerMana);
        }

        turnManager.turnBanner.ShowPlayerTurnBanner();

        // Subscribe to EndTurn input event (for testing)
        turnManager.InputActions.Player.NextTurn.performed += OnEndTurn;
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("[TurnStates] 3. Exit Player Turn");
        turnManager.InputActions.Player.NextTurn.performed -= OnEndTurn;
    }

    private void OnEndTurn(InputAction.CallbackContext ctx)
    {
        Debug.Log("[TurnStates] 2. Player ended turn");

        var bm = BattleManager.Instance;
        if (bm != null)
        {
            bm.playerMana = 0;
            if (bm.uiManager != null) bm.uiManager.UpdatePlayerMana(bm.playerMana);
        }

        turnManager.EndCurrentTurn();
    }
}


public class EnemyTurnState : TurnStateBase
{
    public EnemyTurnState(TurnManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("[TurnStates] 4. Enemy turn started...");

        // Reset enemy mana at start of their turn
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.enemyMana = BattleManager.Instance.enemyMaxMana;
            if (BattleManager.Instance.uiManager != null) BattleManager.Instance.uiManager.UpdateEnemyMana(BattleManager.Instance.enemyMana);
        }

        turnManager.StartCoroutine(EnemyActionRoutine());
    }

    private bool SlotIsEnemy(BoardSlot s)
    {
        if (s == null) return false;
        Transform t = s.transform;
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

    private IEnumerator EnemyActionRoutine()
    {
        yield return turnManager.turnBanner.ShowEnemyTurnBannerEnumerator();

        var bm = BattleManager.Instance;
        if (bm == null)
        {
            Debug.LogWarning("[EnemyAI] No BattleManager present — ending enemy turn.");
            turnManager.EndCurrentTurn();
            yield break;
        }

        EntityBase enemyOwner = null;
        if (bm.enemies != null && bm.enemies.Count >0)
            enemyOwner = bm.enemies[0];
        else if (bm.enemyEntity != null)
            enemyOwner = bm.enemyEntity;

        if (enemyOwner == null)
        {
            Debug.LogWarning("[EnemyAI] No enemy Entity found to own cards.");
        }

        int attempts =0;
        const int maxAttempts =8;

        while (bm.enemyMana >0 && attempts < maxAttempts)
        {
            attempts++;

            var json = CardDatabase.Instance.GetRandomCard();
            if (json == null)
            {
                Debug.LogWarning("[EnemyAI] No card available from CardDatabase.");
                break;
            }

            Debug.Log($"[EnemyAI] Drew card id: {json.id}, name: {json.cardName}, cost: {json.cost}");

            var inst = CardFactory.CreateCard(json.id, enemyOwner);
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

            if (inst.IsMinion)
            {
                BoardSlot chosen = null;
                var slots = Object.FindObjectsOfType<BoardSlot>();
                foreach (var s in slots)
                {
                    if (s.isOccupied) continue;
                    if (s.isRanged != inst.Data.isRanged) continue;
                    if (SlotIsEnemy(s)) { chosen = s; break; }
                }

                if (chosen == null)
                {
                    Debug.Log("[EnemyAI] No valid enemy-side board slot found for minion — ending play.");
                    break;
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
                }
                else
                {
                    Debug.LogWarning("[EnemyAI] Failed to place minion on chosen slot.");
                    break;
                }
            }
            else
            {
                bool requiresPlayerMinionTarget = false;
                if (inst.Data != null)
                {
                    var id = inst.Data.id ?? string.Empty;
                    var name = inst.Data.cardName ?? string.Empty;
                    requiresPlayerMinionTarget = string.Equals(id, "fireball", System.StringComparison.OrdinalIgnoreCase)
                        || string.Equals(id, "slash", System.StringComparison.OrdinalIgnoreCase)
                        || name.IndexOf("fireball", System.StringComparison.OrdinalIgnoreCase) >=0
                        || name.IndexOf("slash", System.StringComparison.OrdinalIgnoreCase) >=0;
                }

                MinionEntity chosenTarget = null;
                if (requiresPlayerMinionTarget)
                {
                    var allMinions = Object.FindObjectsOfType<MinionEntity>();
                    foreach (var me in allMinions)
                    {
                        var mb = me.GetComponent<MinionBehaviour>();
                        if (mb == null || mb.instance == null) continue;
                        if (mb.instance.Owner is PlayerEntity)
                        {
                            chosenTarget = me; // pick first
                            break;
                        }
                    }

                    if (chosenTarget == null)
                    {
                        Debug.Log($"[EnemyAI] Skipping {inst.Data.cardName}: no valid player minion targets.");
                        continue;
                    }
                }

                Debug.Log($"[EnemyAI] Playing spell {inst.Data.cardName} (cost {inst.Data.cost}) targeting {(chosenTarget != null ? chosenTarget.entityName : "player side")}");
                inst.PlayCard(null, (EntityBase)(chosenTarget ?? (EntityBase)bm.player));

                bm.enemyMana -= inst.Data.cost;
                if (bm.uiManager != null) bm.uiManager.UpdateEnemyMana(bm.enemyMana);

                Debug.Log($"[EnemyAI] Cast spell {inst.Data.cardName} for cost {inst.Data.cost}. Remaining mana: {bm.enemyMana}");
            }

            yield return new WaitForSecondsRealtime(0.5f);

            if (bm.playerHealth <=0 || bm.enemyHealth <=0) break;
        }

        if (bm != null)
        {
            bm.enemyMana = 0;
            if (bm.uiManager != null) bm.uiManager.UpdateEnemyMana(bm.enemyMana);
        }

        turnManager.EndCurrentTurn();
    }

}


public class EndTurnState : TurnStateBase
{
    public EndTurnState(TurnManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("[TurnStates] 6. Ending turn & Resolving card effects");
        // Resolve game state
        turnManager.StartCoroutine(ResolveRoutine());
    }

    private IEnumerator ResolveRoutine()
    {
        // Resolve damage for the side that ended their turn
        bool fromPlayer = TurnManager.Instance.SideEndingTurn == TurnManager.Side.Player;

        // First, resolve any spells placed on board for this side
        //BoardManager.Instance.ResolveAndClearSpellsForSide(fromPlayer);

        // Then, resolve minion damage
        if (BattleManager.Instance != null)
        {
            // resolve all minion attacks simultaneously (both sides)
            BattleManager.Instance.ResolveAllMinionDamage();
        }

        yield return new WaitForSeconds(0.5f);

        // Check for victory/defeat
        if (BattleManager.Instance != null)
        {
            if (BattleManager.Instance.playerHealth <= 0)
            {
                Debug.Log("[EndTurn] Player has been defeated.");
                turnManager.turnBanner.ShowPersistentEndBanner("You were defeated");
                yield break;
            }
            if (BattleManager.Instance.enemyHealth <= 0)
            {
                Debug.Log("[EndTurn] Enemy has been defeated.");
                turnManager.turnBanner.ShowPersistentEndBanner("Enemy defeated");
                yield break;
            }
        }

        // Switch to the other player's turn
        if (TurnManager.Instance.SideEndingTurn == TurnManager.Side.Player)
            turnManager.ChangeState(turnManager.EnemyTurnState);
        else
            turnManager.ChangeState(turnManager.PlayerTurnState);
    }
}