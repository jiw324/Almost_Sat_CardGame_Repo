using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTurnState : TurnStateBase
{
    public PlayerTurnState(TurnManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("[TurnStates] 1. Enter Player Turn");

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
        turnManager.ChangeState(turnManager.EnemyTurnState);
    }
}


public class EnemyTurnState : TurnStateBase
{
    public EnemyTurnState(TurnManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("[TurnStates] 4. Enemy turn started...");
        // Call to enemy action
        turnManager.StartCoroutine(EnemyActionRoutine());
    }

    private IEnumerator EnemyActionRoutine()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("[TurnStates] 5. Enemy made a decision");
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
        yield return new WaitForSeconds(0.5f);
        turnManager.ChangeState(turnManager.PlayerTurnState);
    }
}