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
            
            // Draw cards at start of turn
            if (BattleManager.Instance.playerHandManager != null)
            {
                int cardsToDraw = BattleManager.Instance.playerHandManager.GetCardsPerTurn();
                BattleManager.Instance.playerHandManager.DrawCards(cardsToDraw);
            }
        }

        turnManager.turnBanner.ShowPlayerTurnBanner();
    }

    public override void Update() { }

    public override void Exit()
    {
        Debug.Log("[TurnStates] 3. Exit Player Turn");
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
            
            // Draw cards at start of turn
            if (BattleManager.Instance.enemyHandManager != null)
            {
                int cardsToDraw = BattleManager.Instance.enemyHandManager.GetCardsPerTurn();
                BattleManager.Instance.enemyHandManager.DrawCards(cardsToDraw);
            }
        }

        turnManager.StartCoroutine(EnemyActionRoutine());
    }

    private IEnumerator EnemyActionRoutine()
    {
        yield return turnManager.turnBanner.ShowEnemyTurnBannerEnumerator();

        yield return EnemyAI.ExecuteTurn(turnManager);

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

        // Discard hand of the player who just ended their turn
        if (BattleManager.Instance != null)
        {
            if (fromPlayer && BattleManager.Instance.playerHandManager != null)
            {
                BattleManager.Instance.playerHandManager.DiscardAllHand();
            }
            else if (!fromPlayer && BattleManager.Instance.enemyHandManager != null)
            {
                BattleManager.Instance.enemyHandManager.DiscardAllHand();
            }
        }

        StatusSystem.OnTurnEnd(turnManager.SideEndingTurn);
        // First, resolve any spells placed on board for this side
        //BoardManager.Instance.ResolveAndClearSpellsForSide(fromPlayer);

        yield return new WaitForSeconds(0.5f);

        // Check for victory/defeat (backup check in case health changed during turn resolution)
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.CheckBattleEnd();
            if (BattleManager.Instance.IsBattleEnded)
            {
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
