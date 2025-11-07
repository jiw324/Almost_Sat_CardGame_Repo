using System.Collections;
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

    // per-turn max mana
    public int playerMaxMana = 3;
    public int enemyMaxMana = 3;

    private bool endSequenceStarted = false;

    void Awake() => Instance = this;

    void Start()
    {
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

        StartBattle();
    }

    void StartBattle()
    {
        uiManager.InitializeUI(playerHealth, playerMana, enemyHealth, enemyMana);
        BoardManager.Instance.InitializeBoard();
    }

    // Resolve minion damage originating from one side and apply to the opposing side
    public void ResolveMinionDamage(bool fromPlayer)
    {
        int totalDamage = 0;
        var slots = Object.FindObjectsOfType<BoardSlot>();
        foreach (var s in slots)
        {
            if (s.currentCard == null || s.currentCard.Data == null) continue;
            var card = s.currentCard;

            bool ownedByPlayer = card.Owner is PlayerEntity;
            if (fromPlayer != ownedByPlayer) continue;

            int dmg = 0;
            if (!string.IsNullOrEmpty(card.Data.id))
            {
                string id = card.Data.id.ToLowerInvariant();
                if (id == "warrior") dmg = 2;
                else if (id == "archer") dmg = 5;
                else
                {
                    if (card.Data.effect != null)
                    {
                        var field = card.Data.effect.GetType().GetField("amount");
                        if (field != null && field.FieldType == typeof(int))
                            dmg = (int)field.GetValue(card.Data.effect);
                    }
                }
            }

            totalDamage += dmg;
        }

        if (totalDamage <= 0) return;

        if (fromPlayer)
        {
            enemyHealth -= totalDamage;
            // clamp to zero for UI
            int displayHealth = Mathf.Max(0, enemyHealth);
            uiManager.UpdateEnemyHealth(displayHealth);
            Debug.Log($"[BattleManager] Player minions dealt {totalDamage} to enemy. Enemy health now {enemyHealth}");
        }
        else
        {
            playerHealth -= totalDamage;
            int displayHealth = Mathf.Max(0, playerHealth);
            uiManager.UpdatePlayerHealth(displayHealth);
            Debug.Log($"[BattleManager] Enemy minions dealt {totalDamage} to player. Player health now {playerHealth}");
        }

        // Check defeat
        if (playerHealth <= 0)
        {
            Debug.Log("[BattleManager] Player defeated.");
            EndBattle("You were defeated");
        }
        if (enemyHealth <= 0)
        {
            Debug.Log("[BattleManager] Enemy defeated.");
            EndBattle("Enemy defeated");
        }
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
}
