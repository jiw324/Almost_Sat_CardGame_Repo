using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public UIManager uiManager;
    public PlayerEntity playerEntity;
    public EnemyEntity enemyEntity;
    [SerializeField] private HandManager playerHandManager;
    [SerializeField] private HandManager enemyHandManager;

    public int playerHealth;
    public int playerMana;
    public int enemyHealth;
    public int enemyMana;
    private DeckInstance playerDeckInstance;
    private DeckInstance enemyDeckInstance;

    void Awake() => Instance = this;

    void Start()
    {
        GameSession session = SessionGrabber.getGameSession();
        if (session != null)
        {
            playerHealth = session.GetPlayerHealth();
            playerMana = session.GetPlayerMana();
            playerDeckInstance = session.GetPlayerDeck();

            // Load enemy data from current combat node
            LoadEnemyData(session);
        }

        StartBattle();
    }

    private void LoadEnemyData(GameSession session)
    {
        var nodeData = session.gameSessionData.sessionNodeMapData.currentNodeData;
        
        if (nodeData is CombatNodeData combatData)
        {
            EnemyDefinition enemyDef = null;

            // Try to load enemy definition from Resources
            if (!string.IsNullOrWhiteSpace(combatData.enemyDefinitionName))
            {
                enemyDef = Resources.Load<EnemyDefinition>($"Enemies/{combatData.enemyDefinitionName}");
                if (enemyDef == null)
                {
                    Debug.LogWarning($"[BattleManager] Could not load EnemyDefinition '{combatData.enemyDefinitionName}' from Resources/Enemies/. Using combat node data directly.");
                }
            }

            // Initialize enemy entity
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

                enemyHealth = enemyEntity.currentHealth;
                enemyMana = enemyEntity.mana;
                enemyDeckInstance = enemyEntity.GetDeck();

                // If CombatNodeData has a deck, use it (for save/load persistence)
                if (combatData.enemyDeck != null && combatData.enemyDeck.Cards.Count > 0)
                {
                    enemyDeckInstance = combatData.enemyDeck;
                }

                // Set enemy portrait if available
                if (enemyDef != null && enemyDef.Portrait != null && uiManager != null && uiManager.enemyUI != null)
                {
                    uiManager.enemyUI.SetPortrait(enemyDef.Portrait);
                }
            }
        }
        else
        {
            Debug.LogWarning("[BattleManager] Current node is not a CombatNode. Using default enemy stats.");
            enemyHealth = 10;
            enemyMana = 1;
            enemyDeckInstance = new DeckInstance();
        }
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
}
