using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public BoardManager boardManager;
    public PlayerEntity playerEntity;
    public EnemyEntity enemyEntity;


    void Awake() => Instance = this;

    void Start()
    {
        StartBattle();
    }

    void StartBattle()
    {
        boardManager.InitializeBoard();
    }
}
