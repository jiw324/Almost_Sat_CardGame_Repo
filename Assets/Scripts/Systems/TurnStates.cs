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
            BattleManager.Instance.uiManager.UpdatePlayerMana(BattleManager.Instance.playerMana);
        }

        turnManager.turnBanner.ShowPlayerTurnBanner();

        // Subscribe to EndTurn input event (for testing)
        turnManager.InputActions.Player.NextTurn.performed += OnEndTurn;
    }

    public override void Update()
    {
        // Game logic
    }

    public override void Exit()
    {
        Debug.Log("[TurnStates] 3. Exit Player Turn");

        turnManager.InputActions.Player.NextTurn.performed -= OnEndTurn;
    }

    private void OnEndTurn(InputAction.CallbackContext ctx)
    {
        Debug.Log("[TurnStates] 2. Player ended turn");

        TurnManager.Instance.SideEndingTurn = TurnManager.Side.Player;
        turnManager.ChangeState(turnManager.EndTurnState);
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
            BattleManager.Instance.uiManager.UpdateEnemyMana(BattleManager.Instance.enemyMana);
        }

        turnManager.turnBanner.ShowEnemyTurnBanner();

        // Call to enemy action
        turnManager.StartCoroutine(EnemyActionRoutine());
    }

    private IEnumerator EnemyActionRoutine()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("[TurnStates] 5. Enemy made a decision");

        int maxDraws = 30; // safeguard to avoid infinite loops
        int draws = 0;

        // Continue drawing and attempting to play minions until enemy mana is depleted or we reach draw limit
        while (BattleManager.Instance != null && BattleManager.Instance.enemyMana > 0 && draws < maxDraws)
        {
            draws++;
            yield return new WaitForSeconds(0.1f);

            CardJSON randomData = CardDatabase.Instance.GetRandomCard();
            if (randomData == null)
            {
                Debug.LogWarning("[EnemyTurn] No cards available to draw.");
                break;
            }

            var enemy = BattleManager.Instance.enemyEntity;
            CardInstance card = CardFactory.CreateCard(randomData.id, enemy);
            if (card == null)
                continue;

            Debug.Log($"[EnemyTurn] Drew card: {card.Data.cardName}");

            if (card.Data.type != "minion")
            {
                // attempt to place spells on enemy side visually if mana allows
                if (BattleManager.Instance.enemyMana >= card.Data.cost)
                {
                    // find an enemy-side slot matching ranged/melee
                    BoardSlot[] slots = Object.FindObjectsOfType<BoardSlot>();
                    BoardSlot chosen = null;
                    foreach (var s in slots)
                    {
                        if (s.transform.parent == null) continue;
                        string parentName = s.transform.parent.name;
                        if (parentName != "MeleeB" && parentName != "RangedB") continue;

                        if (s.isOccupied) continue;
                        if (s.isRanged == card.Data.isRanged)
                        {
                            chosen = s;
                            break;
                        }
                    }

                    if (chosen != null)
                    {
                        bool placed = chosen.PlaceSpell(card);
                        if (placed)
                        {
                            // pay mana and update UI
                            BattleManager.Instance.enemyMana -= card.Data.cost;
                            BattleManager.Instance.uiManager.UpdateEnemyMana(BattleManager.Instance.enemyMana);

                            // mark as played so it won't be played again
                            card.PlayCard(chosen);
                            Debug.Log($"[EnemyTurn] Placed spell {card.Data.cardName} on {chosen.name} for visual. Mana left: {BattleManager.Instance.enemyMana}");

                            yield return new WaitForSeconds(0.6f);
                            continue;
                        }
                    }

                    // fallback: if no slot found, cast immediately
                    if (card.Data.effect != null)
                    {
                        BattleManager.Instance.enemyMana -= card.Data.cost;
                        BattleManager.Instance.uiManager.UpdateEnemyMana(BattleManager.Instance.enemyMana);
                        card.Data.effect.Execute(card.Owner, BattleManager.Instance.playerEntity);
                        Debug.Log($"[EnemyTurn] Enemy cast spell {card.Data.cardName} immediately (no slot). Mana left: {BattleManager.Instance.enemyMana}");
                        yield return new WaitForSeconds(0.25f);
                        continue;
                    }
                }

                continue;
            }

            // if not enough mana for this minion, skip
            if (BattleManager.Instance.enemyMana < card.Data.cost)
            {
                Debug.Log($"[EnemyTurn] Not enough mana to play {card.Data.cardName} (cost {card.Data.cost}). Current mana: {BattleManager.Instance.enemyMana}");
                // try drawing again to find cheaper cards
                continue;
            }

            // find available enemy-side slot matching ranged/melee
            BoardSlot[] mslots = Object.FindObjectsOfType<BoardSlot>();
            BoardSlot mchosen = null;
            foreach (var s in mslots)
            {
                if (s.transform.parent == null) continue;
                string parentName = s.transform.parent.name;
                if (parentName != "MeleeB" && parentName != "RangedB") continue;

                if (s.isOccupied) continue;
                if (s.isRanged == card.Data.isRanged)
                {
                    mchosen = s;
                    break;
                }
            }

            if (mchosen == null)
            {
                Debug.Log($"[EnemyTurn] No available slot for {card.Data.cardName} (ranged={card.Data.isRanged}).");
                // cannot place this minion; try drawing again
                continue;
            }

            bool mplaced = mchosen.PlaceCard(card);
            if (mplaced)
            {
                // subtract mana and update UI
                BattleManager.Instance.enemyMana -= card.Data.cost;
                BattleManager.Instance.uiManager.UpdateEnemyMana(BattleManager.Instance.enemyMana);

                card.PlayCard(mchosen);
                Debug.Log($"[EnemyTurn] Played {card.Data.cardName} on {mchosen.name}. Mana left: {BattleManager.Instance.enemyMana}");

                yield return new WaitForSeconds(0.25f);
            }
            else
            {
                Debug.LogWarning($"[EnemyTurn] Failed to place {card.Data.cardName} on {mchosen.name}");
            }

            // If no slots remain for either type, stop early
            bool anyMeleeFree = false, anyRangedFree = false;
            foreach (var s in Object.FindObjectsOfType<BoardSlot>())
            {
                if (s.transform.parent == null) continue;
                string parentName = s.transform.parent.name;
                if (parentName != "MeleeB" && parentName != "RangedB") continue;
                if (!s.isOccupied)
                {
                    if (s.isRanged) anyRangedFree = true; else anyMeleeFree = true;
                }
            }
            if (!anyMeleeFree && !anyRangedFree)
            {
                Debug.Log("[EnemyTurn] No free enemy slots remain.");
                break;
            }
        }

        yield return new WaitForSeconds(0.5f);
        Debug.Log("[TurnStates] 5. Enemy finished actions");
        TurnManager.Instance.SideEndingTurn = TurnManager.Side.Enemy;
        turnManager.ChangeState(turnManager.EndTurnState);
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
        BoardManager.Instance.ResolveAndClearSpellsForSide(fromPlayer);

        // Then, resolve minion damage
        BattleManager.Instance.ResolveMinionDamage(fromPlayer);

        yield return new WaitForSeconds(0.5f);

        // Check for victory/defeat
        if (BattleManager.Instance != null)
        {
            if (BattleManager.Instance.playerHealth <= 0)
            {
                Debug.Log("[EndTurn] Player has been defeated.");
                yield break;
            }
            if (BattleManager.Instance.enemyHealth <= 0)
            {
                Debug.Log("[EndTurn] Enemy has been defeated.");
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