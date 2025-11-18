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

    public int playerMaxHealth = 10;
    public int enemyMaxHealth = 20;

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
            playerMaxHealth = session.gameSessionData.sessionPlayerData.health;
        }

        // Initialize health and mana
        playerMana = playerMaxMana;
        enemyHealth = 20; // set default enemy health to 20
        enemyMana = enemyMaxMana;

        enemyMaxHealth = enemyHealth;

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

        if (playerHealth <= 0) playerHealth = 10; // default player health if not set by session

        if (player != null)
        {
            player.currentHealth = playerHealth;
        }

        if (enemies != null && enemies.Count > 0)
        {
            foreach (var e in enemies)
            {
                if (e != null)
                    e.currentHealth = enemyHealth;
            }
        }
        else if (enemyEntity != null)
        {
            enemyEntity.currentHealth = enemyHealth;
        }

        Debug.Log($"BattleManager initialized with {enemies.Count} enemies.");
        boardManager.InitializeBoard();

        if (uiManager != null)
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

    public void ResolveMinionDamage(bool fromPlayer)
    {
        Debug.Log($"[BattleManager] Resolving minion damage. FromPlayer={fromPlayer}");
        var slots = FindObjectsOfType<BoardSlot>();
        if (slots == null || slots.Length == 0) return;

        foreach (var s in slots)
        {
            if (s.currentCard == null || !s.currentCard.IsMinion) continue;

            var owner = s.currentCard.Owner;
            if (owner == null) continue;

            if (fromPlayer && !(owner is PlayerEntity)) continue;
            if (!fromPlayer && !(owner is EnemyEntity)) continue;

            int atk = s.currentCard.Attack;
            if (atk <= 0) continue;

            if (fromPlayer)
            {
                EnemyEntity target = null;
                var bm = Instance;
                if (bm != null)
                {
                    if (bm.enemies != null && bm.enemies.Count > 0)
                    {
                        foreach (var e in bm.enemies)
                        {
                            if (e != null && e.currentHealth > 0) { target = e; break; }
                        }
                    }
                    if (target == null && bm.enemyEntity != null) target = bm.enemyEntity;
                }

                if (target != null)
                {
                    target.TakeDamage(atk);
                    Debug.Log($"[BattleManager] Player minion {s.currentCard.Data.cardName} dealt {atk} to {target.name}");
                }
            }
            else
            {
                if (Instance != null && Instance.player != null)
                {
                    Instance.player.TakeDamage(atk);
                    Debug.Log($"[BattleManager] Enemy minion {s.currentCard.Data.cardName} dealt {atk} to Player");
                }
            }
        }
    }

    public void ResolveAllMinionDamage()
    {
        Debug.Log("[BattleManager] Resolving all minion damage (both sides)");
        var slots = FindObjectsOfType<BoardSlot>();
        if (slots == null || slots.Length == 0) return;

        var playerMelee = new List<BoardSlot>();
        var playerRanged = new List<BoardSlot>();
        var enemyMelee = new List<BoardSlot>();
        var enemyRanged = new List<BoardSlot>();

        foreach (var s in slots)
        {
            bool isEnemySlot = false;
            if (s.gameObject.name.Contains("B") || (s.transform.parent != null && s.transform.parent.name.Contains("B")))
                isEnemySlot = true;

            if (s.isRanged)
            {
                if (isEnemySlot) enemyRanged.Add(s); else playerRanged.Add(s);
            }
            else
            {
                if (isEnemySlot) enemyMelee.Add(s); else playerMelee.Add(s);
            }
        }

        System.Comparison<BoardSlot> cmp = (a, b) => a.transform.position.x.CompareTo(b.transform.position.x);
        playerMelee.Sort(cmp); playerRanged.Sort(cmp); enemyMelee.Sort(cmp); enemyRanged.Sort(cmp);

        int IndexOfSlot(BoardSlot slot, List<BoardSlot> list)
        {
            for (int i = 0; i < list.Count; i++) if (list[i] == slot) return i;
            return -1;
        }

        var minionDamageMap = new Dictionary<MinionEntity, int>();
        int damageToPlayer = 0;
        int damageToEnemyHero = 0;

        foreach (var s in slots)
        {
            if (s.currentCard == null || !s.currentCard.IsMinion) continue;
            if (!(s.currentCard.Owner is PlayerEntity)) continue;

            int atk = s.currentCard.Attack;
            if (atk <= 0) continue;

            List<BoardSlot> ownList = s.isRanged ? playerRanged : playerMelee;
            List<BoardSlot> oppList = s.isRanged ? enemyRanged : enemyMelee;

            int idx = IndexOfSlot(s, ownList);
            BoardSlot targetSlot = null;
            if (idx >= 0 && idx < oppList.Count)
            {
                targetSlot = oppList[idx];
            }

            if (targetSlot != null && targetSlot.currentCard != null && targetSlot.currentCard.IsMinion)
            {
                var me = targetSlot.GetComponentInChildren<MinionEntity>();
                if (me != null)
                {
                    if (!minionDamageMap.ContainsKey(me)) minionDamageMap[me] = 0;
                    minionDamageMap[me] += atk;
                    Debug.Log($"[BattleManager] Player minion {s.currentCard.Data.cardName} will deal {atk} to {me.entityName}");
                }
            }
            else
            {
                damageToEnemyHero += atk;
                Debug.Log($"[BattleManager] Player minion {s.currentCard.Data.cardName} will deal {atk} to enemy hero");
            }
        }

        foreach (var s in slots)
        {
            if (s.currentCard == null || !s.currentCard.IsMinion) continue;
            if (!(s.currentCard.Owner is EnemyEntity)) continue;

            int atk = s.currentCard.Attack;
            if (atk <= 0) continue;

            List<BoardSlot> ownList = s.isRanged ? enemyRanged : enemyMelee;
            List<BoardSlot> oppList = s.isRanged ? playerRanged : playerMelee;

            int idx = IndexOfSlot(s, ownList);
            BoardSlot targetSlot = null;
            if (idx >= 0 && idx < oppList.Count)
            {
                targetSlot = oppList[idx];
            }

            if (targetSlot != null && targetSlot.currentCard != null && targetSlot.currentCard.IsMinion)
            {
                var me = targetSlot.GetComponentInChildren<MinionEntity>();
                if (me != null)
                {
                    if (!minionDamageMap.ContainsKey(me)) minionDamageMap[me] = 0;
                    minionDamageMap[me] += atk;
                    Debug.Log($"[BattleManager] Enemy minion {s.currentCard.Data.cardName} will deal {atk} to {me.entityName}");
                }
            }
            else
            {
                damageToPlayer += atk;
                Debug.Log($"[BattleManager] Enemy minion {s.currentCard.Data.cardName} will deal {atk} to player");
            }
        }

        foreach (var kvp in minionDamageMap)
        {
            var minionEntity = kvp.Key;
            int dmg = kvp.Value;
            if (minionEntity == null) continue;
            var mb = minionEntity.GetComponent<MinionBehaviour>();
            if (mb != null)
            {
                Debug.Log($"[BattleManager] Applying {dmg} damage to minion {minionEntity.entityName}");
                mb.ReceiveDamage(dmg);
            }
            else
            {
                var ci = minionEntity.GetComponent<CardInstance>();
                if (ci != null) ci.TakeDamage(dmg);
            }
        }

        if (damageToPlayer > 0 && player != null)
        {
            Debug.Log($"[BattleManager] Applying {damageToPlayer} damage to player hero");
            player.TakeDamage(damageToPlayer);
        }
        if (damageToEnemyHero > 0)
        {
            EnemyEntity heroTarget = null;
            if (enemies != null && enemies.Count > 0)
            {
                foreach (var e in enemies) if (e != null && e.currentHealth > 0) { heroTarget = e; break; }
            }
            if (heroTarget == null && enemyEntity != null) heroTarget = enemyEntity;
            if (heroTarget != null)
            {
                Debug.Log($"[BattleManager] Applying {damageToEnemyHero} damage to enemy hero {heroTarget.name}");
                heroTarget.TakeDamage(damageToEnemyHero);
            }
        }
    }
}
