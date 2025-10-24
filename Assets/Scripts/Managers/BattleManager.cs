using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("References")]
    public BoardManager boardManager;
    public PlayerEntity player;
    public List<EnemyEntity> enemies = new List<EnemyEntity>();

    [Header("Scene Setup")]
    [SerializeField] private Transform enemyParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (boardManager == null)
            boardManager = GetComponent<BoardManager>();

        if (player == null)
            player = GetComponent<PlayerEntity>();

        // auto-gather all enemies under parent or scene
        if (enemies.Count == 0)
        {
            if (enemyParent)
            {
                enemies.AddRange(enemyParent.GetComponentsInChildren<EnemyEntity>());
            }
            else
            {
                enemies.AddRange(FindObjectsOfType<EnemyEntity>());
            }
        }

        Debug.Log($"BattleManager initialized with {enemies.Count} enemies.");
        boardManager.InitializeBoard();
    }

    public EnemyEntity GetRandomEnemy()
    {
        if (enemies == null || enemies.Count == 0) return null;
        int index = Random.Range(0, enemies.Count);
        return enemies[index];
    }

    public void RemoveDeadEnemy(EnemyEntity e)
    {
        if (enemies.Contains(e))
        {
            enemies.Remove(e);
            Debug.Log($"{e.name} removed from enemy list.");
        }
    }
}
