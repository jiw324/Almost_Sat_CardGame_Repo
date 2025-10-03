using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTurnState : GameStateBase
{
    public PlayerTurnState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("Enter Player Turn");

        // Subscribe to EndTurn input event (for testing)
        gameManager.InputActions.Player.Attack.performed += OnEndTurn;
    }

    public override void Update()
    {
        // Game logic
    }

    public override void Exit()
    {
        Debug.Log("Exit Player Turn");

        gameManager.InputActions.Player.Attack.performed -= OnEndTurn;
    }

    private void OnEndTurn(InputAction.CallbackContext ctx)
    {
        Debug.Log("Player ended turn");
        gameManager.ChangeState(new EnemyTurnState(gameManager));
    }
}


public class EnemyTurnState : GameStateBase
{
    public EnemyTurnState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("Enemy turn started...");
        // Call to enemy action
        gameManager.StartCoroutine(EnemyActionRoutine());
    }

    private IEnumerator EnemyActionRoutine()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Enemy made a decision");
        gameManager.ChangeState(new EndTurnState(gameManager));
    }
}


public class EndTurnState : GameStateBase
{
    public EndTurnState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("Ending turn & Resolving card effects");
        // Resolve game state
        gameManager.StartCoroutine(ResolveRoutine());
    }

    private IEnumerator ResolveRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        gameManager.ChangeState(new PlayerTurnState(gameManager));
    }
}