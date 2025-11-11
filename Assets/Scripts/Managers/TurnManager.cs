using System;
using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{

    public static TurnManager Instance { get; private set; }

    [SerializeField] public TurnBanner turnBanner;

    private TurnStateBase currentState;
    private TurnStateBase playerTurnState;
    private TurnStateBase enemyTurnState;
    private TurnStateBase endTurnState;

    public TurnStateBase PlayerTurnState => playerTurnState;
    public TurnStateBase EnemyTurnState => enemyTurnState;
    public TurnStateBase EndTurnState => endTurnState;

    public event Action<TurnStateBase> OnStateChanged;

    public InputSystem_Actions InputActions { get; private set; }

    public enum Side { Player, Enemy }

    public Side SideEndingTurn { get; set; } = Side.Player;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        InputActions.Enable();
    }
    private void OnDisable()
    {
        InputActions.Disable();
    }

    private void Start()
    {
        playerTurnState = new PlayerTurnState(this);
        enemyTurnState = new EnemyTurnState(this);
        endTurnState = new EndTurnState(this);

        StartCoroutine(BeginBattleRoutine());
    }

    private IEnumerator BeginBattleRoutine()
    {
        yield return turnBanner.ShowIntroBannerEnumerator();
        ChangeState(playerTurnState);
    }

    public void ChangeState(TurnStateBase newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
        OnStateChanged?.Invoke(currentState);
    }

    public void EndCurrentTurn()
    {
        if (currentState == playerTurnState)
        {
            SideEndingTurn = Side.Player;
        }
        else if (currentState == enemyTurnState)
        {
            SideEndingTurn = Side.Enemy;
        }
        else
        {
            SideEndingTurn = Side.Player;
        }

        ChangeState(endTurnState);
    }

    // Update is called once per frame
    private void Update()
    {
        currentState?.Update();
    }
}
