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
    public HandManager playerHandManager;
    public HandManager enemyHandManager;

    public int playerHealth;
    public int playerMana;
    public int enemyHealth;
    public int enemyMana;
    private DeckInstance playerDeckInstance;
    private DeckInstance enemyDeckInstance;

    // per-turn max mana
    public int playerMaxMana = 3;
    public int enemyMaxMana = 3;

    public int playerMaxHealth = 10;
    public int enemyMaxHealth = 20;

    private bool endSequenceStarted = false;
    public bool IsBattleEnded => endSequenceStarted;


    private void Awake()
    {
        RowEffectSystem.ClearAll();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GameSession session = SessionGrabber.getGameSession();
        if (session != null)
        {
            // Use session methods to get player data
            playerHealth = session.GetPlayerHealth();
            playerMana = session.GetPlayerMana();
            playerDeckInstance = session.GetPlayerDeck();
            playerMaxMana = session.GetPlayerMana(); // Use current mana as max for now
            playerMaxHealth = session.GetPlayerMaxHealth();

            // Load enemy data from current combat node (if available)
            LoadEnemyData(session);
        }

        // Initialize player health and mana
        playerMana = playerMaxMana;
        
        // Enemy health and mana will be set by LoadEnemyData()
        // Don't set defaults here as they will be overridden
    }

    private void LoadEnemyData(GameSession session)
    {
        if (session?.gameSessionData?.sessionNodeMapData?.currentNodeData == null)
        {
            Debug.LogWarning("[BattleManager] No node data available. Using default enemy stats.");
            enemyHealth = 10;
            enemyMana = 1;
            enemyDeckInstance = new DeckInstance();
            return;
        }

        var nodeData = session.gameSessionData.sessionNodeMapData.currentNodeData;
        
        if (nodeData is CombatNodeData combatData)
        {
            EnemyDefinition enemyDef = null;

            // Try to load enemy definition from Resources
            if (!string.IsNullOrWhiteSpace(combatData.enemyDefinitionName))
            {
                if (combatData.enemyDefinitionName.Contains("/"))
                {
                    enemyDef = Resources.Load<EnemyDefinition>(combatData.enemyDefinitionName);
                }
                else
                {
                    enemyDef = Resources.Load<EnemyDefinition>($"Enemies/{combatData.enemyDefinitionName}");
                }

                if (enemyDef == null)
                {
                    Debug.LogWarning($"[BattleManager] Could not load EnemyDefinition '{combatData.enemyDefinitionName}' from Resources/Enemies/. Using combat node data directly.");
                }
            }

            // Initialize enemy entity (support both single enemyEntity and enemies list)
            if (enemyEntity != null)
            {
                if (enemyDef != null)
                {
                    enemyEntity.Initialize(enemyDef);
                }
                else
                {
                    // Fallback: use data from CombatNodeData directly
                    enemyEntity.entityName = "Enemy";
                    enemyEntity.maxHealth = combatData.enemyHealth;
                    enemyEntity.currentHealth = combatData.enemyHealth;
                    enemyEntity.mana = combatData.enemyMana;
                    enemyEntity.maxMana = combatData.enemyMana;
                    Debug.Log($"[BattleManager] Initialized enemy from CombatNodeData: {combatData.enemyHealth} HP, {combatData.enemyMana} mana.");
                }

                // Update BattleManager's enemy stats from the entity
                enemyHealth = enemyEntity.currentHealth;
                enemyMaxHealth = enemyEntity.maxHealth;
                enemyMana = enemyEntity.mana;
                enemyMaxMana = enemyEntity.maxMana;
                enemyDeckInstance = enemyEntity.GetDeck();
                
                Debug.Log($"[BattleManager] Enemy stats set: Health {enemyHealth}/{enemyMaxHealth}, Mana {enemyMana}/{enemyMaxMana}");

                // If CombatNodeData has a deck, use it (for save/load persistence)
                if (combatData.enemyDeck != null && combatData.enemyDeck.Cards.Count > 0)
                {
                    enemyDeckInstance = combatData.enemyDeck;
                }

                // Set enemy portrait if available
                if (enemyDef != null && enemyDef.Portrait != null && uiManager != null && uiManager.enemyAvatar != null)
                {
                    uiManager.enemyAvatar.SetPortrait(enemyDef.Portrait);
                }
            }
        }
        else
        {
            Debug.LogWarning("[BattleManager] Current node is not a CombatNode. Using default enemy stats.");
            enemyHealth = 10;
            enemyMaxHealth = 10;
            enemyMana = 1;
            enemyMaxMana = 1;
            enemyDeckInstance = new DeckInstance();
        }
    }

    private void Start()
    {
        if (boardManager == null)
            boardManager = FindFirstObjectByType<BoardManager>();

        if (player == null)
            player = FindFirstObjectByType<PlayerEntity>();

        // auto-gather all enemies under parent or scene (support multiple enemies)
        if (enemies.Count == 0)
        {
            if (enemyParent)
            {
                enemies.AddRange(enemyParent.GetComponentsInChildren<EnemyEntity>());
            }
            else
            {
                enemies.AddRange(FindObjectsByType<EnemyEntity>(FindObjectsSortMode.None));
            }
        }

        // If we have a single enemyEntity but no enemies list, add it
        if (enemyEntity != null && !enemies.Contains(enemyEntity))
        {
            enemies.Add(enemyEntity);
        }

        if (playerHealth <= 0) playerHealth = 10; // default player health if not set by session

        if (player != null)
        {
            player.currentHealth = playerHealth;
        }

        // Initialize enemies health
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
        
        StartBattle();
    }

    void StartBattle()
    {
        if (uiManager != null)
        {
            uiManager.InitializeUI(playerHealth, playerMana, enemyHealth, enemyMana);
        }
        else
        {
            Debug.LogWarning("[BattleManager] UIManager is not assigned.");
        }

        BoardManager.Instance.InitializeBoard();
        
        if (playerHandManager != null)
        {
            playerHandManager.PrepareForBattle(playerDeckInstance);
        }
        else
        {
            Debug.LogWarning("[BattleManager] Player hand manager reference not assigned.");
        }

        if (enemyHandManager != null && enemyDeckInstance != null)
        {
            enemyHandManager.PrepareForBattle(enemyDeckInstance);
        }
        else if (enemyHandManager != null)
        {
            Debug.LogWarning("[BattleManager] Enemy hand manager assigned but no enemy deck available.");
        }
    }


    public void CheckBattleEnd()
    {
        if (endSequenceStarted) return;
        
        if (playerHealth <= 0)
        {
            EndBattle(false);
        }
        else if (enemyHealth <= 0)
        {
            EndBattle(true);
        }
    }

    public void EndBattle(bool wonBattle)
    {
        if (endSequenceStarted) return;
        endSequenceStarted = true;
        string bannerMessage = "Enemy Defeated";
        if(!wonBattle)
        {
            bannerMessage = "You were defeated";
        } else
        {
            SessionGrabber.getGameSession().SetPlayerGold(
                SessionGrabber.getGameSession().GetPlayerGold() + 10); // reward 10 gold for winning
        }
        StartCoroutine(ShowEndBannerAndReturnToMap(bannerMessage, wonBattle));
    }

    private IEnumerator ShowEndBannerAndReturnToMap(string message, bool wonBattle)
    {
        var tb = Object.FindFirstObjectByType<TurnBanner>();
        if (tb != null)
        {
            tb.ShowPersistentEndBanner(message);
        }

        // Wait a moment to let UI update; then load map
        yield return new WaitForSecondsRealtime(1f);

        if (tb != null)
        {
            tb.HideBanner();
        }
        // Go to Map route
        var msm = FindFirstObjectByType<MapStateManager>();
        if (msm != null)
        { 
            if(wonBattle)
            {
                msm.MarkCompleted(msm.GetCurrentNode());
                msm.ReturnToMapScene();
            }
            else
            {
                GameSession session = SessionGrabber.getGameSession();
                if (session != null)
                {
                    session.EndRun();
                    session.ResetGameSessionData();
                    SessionSaveManager.SaveGameSession(session.gameSessionData);
                }

                var sceneManagerObj = GameObject.Find("SceneManager");
                if (sceneManagerObj != null)
                {
                    var sceneSwitch = sceneManagerObj.GetComponent<SceneSwitch>();
                    if (sceneSwitch != null)
                    {
                        sceneSwitch.SceneChanger("MainMenu");
                    }
                    else
                    {
                        Debug.LogError("[BattleManager] SceneSwitch missing on SceneManager.");
                    }
                }
                else
                {
                    Debug.LogError("[BattleManager] SceneManager object not found.");
                }
            }

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

    /// <summary>
    /// Counts the number of "Rune" minions on the board owned by the player.
    /// </summary>
    public int CountPlayerRunes()
    {
        int runeCount = 0;
        var slots = FindObjectsByType<BoardSlot>(FindObjectsSortMode.None);
        if (slots == null || slots.Length == 0) return 0;

        foreach (var slot in slots)
        {
            if (slot.currentCard == null || !slot.currentCard.IsMinion) continue;
            if (!(slot.currentCard.Owner is PlayerEntity)) continue;
            
            // Check if it's a Rune card (by name or ID)
            if (slot.currentCard.Data != null && 
                (slot.currentCard.Data.cardName.Equals("Rune", System.StringComparison.OrdinalIgnoreCase) ||
                 slot.currentCard.Data.id.ToString().Equals("rune", System.StringComparison.OrdinalIgnoreCase)))
            {
                runeCount++;
            }
        }

        return runeCount;
    }

    public void ResolveMinionDamage(bool fromPlayer)
    {
        Debug.Log($"[BattleManager] Resolving minion damage. FromPlayer={fromPlayer}");
        var slots = FindObjectsByType<BoardSlot>(FindObjectsSortMode.None);
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
        var slots = FindObjectsByType<BoardSlot>(FindObjectsSortMode.None);
        if (slots == null || slots.Length == 0) return;

        var playerMelee = new List<BoardSlot>();
        var playerRanged = new List<BoardSlot>();
        var enemyMelee = new List<BoardSlot>();
        var enemyRanged = new List<BoardSlot>();

        foreach (var s in slots)
        {
            bool isEnemySlot = false;
            
            // Check slot name (e.g., "enemyMeleeA", "enemyRangedB")
            if (s.gameObject.name.StartsWith("enemy", System.StringComparison.OrdinalIgnoreCase))
            {
                isEnemySlot = true;
            }
            // Check parent name (e.g., "Enemy Melee Slots", "Enemy Ranged Slots")
            else if (s.transform.parent != null && s.transform.parent.name.Contains("Enemy", System.StringComparison.OrdinalIgnoreCase))
            {
                isEnemySlot = true;
            }

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
