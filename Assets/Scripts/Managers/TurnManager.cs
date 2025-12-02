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
    }

    private IEnumerator BeginBattleRoutine()
    {
        yield return turnBanner.ShowIntroBannerEnumerator();
        ChangeState(playerTurnState);
    }

    public void OnMulliganFinished()
    {
        StartCoroutine(BeginBattleRoutine());
    }

    public void ChangeState(TurnStateBase newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
        OnStateChanged?.Invoke(currentState);

        var bm = BattleManager.Instance;
        if (bm != null)
        {
            if (newState == enemyTurnState)
            {
                bm.enemyMana = bm.enemyMaxMana;
                if (bm.uiManager != null) bm.uiManager.UpdateEnemyMana(bm.enemyMana);

                bm.playerMana = 0;
                if (bm.uiManager != null) bm.uiManager.UpdatePlayerMana(bm.playerMana);
            }
            else if (newState == playerTurnState)
            {
                bm.playerMana = bm.playerMaxMana;
                if (bm.uiManager != null) bm.uiManager.UpdatePlayerMana(bm.playerMana);

                bm.enemyMana = 0;
                if (bm.uiManager != null) bm.uiManager.UpdateEnemyMana(bm.enemyMana);
            }
        }
    }

    public void EndCurrentTurn()
    {
        if (currentState == playerTurnState)
        {
            SideEndingTurn = Side.Player;
            ChangeState(enemyTurnState);
            return;
        }

        if (currentState == enemyTurnState)
        {
            SideEndingTurn = Side.Enemy;
            ChangeState(endTurnState);
            return;
        }

        SideEndingTurn = Side.Player;
        ChangeState(endTurnState);
    }

    private void Update()
    {
        currentState?.Update();
    }
}
