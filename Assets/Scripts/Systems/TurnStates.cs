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
        //turnManager.StartCoroutine(EnemyActionRoutine());
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
        //BattleManager.Instance.ResolveMinionDamage(fromPlayer);

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