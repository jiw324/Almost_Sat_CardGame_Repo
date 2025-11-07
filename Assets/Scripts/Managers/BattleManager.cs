using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public UIManager uiManager;
    public PlayerEntity playerEntity;
    public EnemyEntity enemyEntity;

    public int playerHealth;
    public int playerMana;
    public int enemyHealth;
    public int enemyMana;

    void Awake() => Instance = this;

    void Start()
    {
        GameSession session = SessionGrabber.getGameSession();
        if (session != null)
        {
            playerHealth = session.gameSessionData.sessionPlayerData.health;
            playerMana = session.gameSessionData.sessionPlayerData.mana;
            enemyHealth = 10;       // Temporary before enemy loading code is written
            enemyMana = 1;
        }

        StartBattle();
    }

    void StartBattle()
    {
        uiManager.InitializeUI(playerHealth, playerMana, enemyHealth, enemyMana);
        BoardManager.Instance.InitializeBoard();
    }
}
