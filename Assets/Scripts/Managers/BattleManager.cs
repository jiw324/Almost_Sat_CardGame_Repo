
using System.Collections.Generic;

using System.Collections;

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

    public UIManager uiManager;
    public PlayerEntity playerEntity;
    public EnemyEntity enemyEntity;

    public int playerHealth;
    public int playerMana;
    public int enemyHealth;
    public int enemyMana;

    // per-turn max mana
    public int playerMaxMana = 3;
    public int enemyMaxMana = 3;

    private bool endSequenceStarted = false;


    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GameSession session = SessionGrabber.getGameSession();
        if (session != null)
        {
            playerHealth = session.gameSessionData.sessionPlayerData.health;
            playerMaxMana = session.gameSessionData.sessionPlayerData.mana;
        }

        // Initialize health and mana
        playerMana = playerMaxMana;
        enemyHealth = 10;       // Temporary before enemy loading code is written
        enemyMana = enemyMaxMana;

        //StartBattle();
    }

    private void Start()
    {
        if (boardManager == null)
            boardManager = FindObjectOfType<BoardManager>();

        if (player == null)
            player = FindObjectOfType<PlayerEntity>();

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

        uiManager.InitializeUI(playerHealth, playerMana, enemyHealth, enemyMana);
    }


    public void EndBattle(string message)
    {
        if (endSequenceStarted) return;
        endSequenceStarted = true;
        StartCoroutine(ShowEndBannerAndReturnToMap(message));
    }

    private IEnumerator ShowEndBannerAndReturnToMap(string message)
    {
        var tb = Object.FindObjectOfType<TurnBanner>();
        if (tb != null)
        {
            tb.ShowPersistentEndBanner(message);
        }

        // Wait a moment to let UI update; then load map
        yield return new WaitForSecondsRealtime(1f);

        // Go to Map route
        if (SceneLoader.Instance != null)
        {
            if (tb != null)
                tb.HideBanner();

            SceneLoader.Instance.Go(GameRoute.Map);
        }
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
