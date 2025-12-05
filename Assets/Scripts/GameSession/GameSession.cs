using UnityEngine;
using UnityEngine.InputSystem;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public GameSessionData gameSessionData;
    [SerializeField] private DeckDefinition startingPlayerDeck;

    /// <summary>
    /// Indicates whether the current run is in tutorial mode.
    /// Used by map generation and UI to switch behavior.
    /// </summary>
    public bool IsTutorialMode { get; set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsurePlayerDeckInitialized();
    }

    //-------Temporary for testing Reset/Save/Load - Need to hook up to pause menu--------------------
    private InputAction saveAction;
    private InputAction loadAction;
    private InputAction resetAction;

    private void OnEnable()
    {
        saveAction = new InputAction(binding: "<Keyboard>/s");
        saveAction.performed += _ => SessionSaveManager.SaveGameSession(gameSessionData);
        saveAction.Enable();

        loadAction = new InputAction(binding: "<Keyboard>/l");
        loadAction.performed += _ => LoadGameSession("Save");
        loadAction.Enable();

        resetAction = new InputAction(binding: "<Keyboard>/r");
        resetAction.performed += _ => ResetGameSessionData();
        resetAction.Enable();
    }

    private void OnDisable()
    {
        saveAction.Disable();
        loadAction.Disable();
        resetAction.Disable();
    }
    //----------------------------------------------------------------------------------------

    public void ResetGameSessionData()
    {
        gameSessionData.ResetSessionData(startingPlayerDeck);
        EnsurePlayerDeckInitialized();
        Debug.Log("Game Session Reset");
    }

    public void LoadGameSession(string filePath)
    {
        GameSessionData loadedData = SessionSaveManager.LoadGameSession(filePath);

        if(loadedData != null)
        {
            gameSessionData = loadedData;
            EnsurePlayerDeckInitialized();
        } else
        {
            Debug.Log("Failed to Load Save Data: Null Session Data");
        }
    }

    private void EnsurePlayerDeckInitialized()
    {
        if (gameSessionData == null || gameSessionData.sessionPlayerData == null)
            return;

        var playerData = gameSessionData.sessionPlayerData;
        if (playerData.deck == null || playerData.deck.Cards == null)
        {
            playerData.deck = startingPlayerDeck != null
                ? new DeckInstance(startingPlayerDeck.CardIds)
                : new DeckInstance();
        }
        else if (playerData.deck.Cards.Count == 0 && startingPlayerDeck != null)
        {
            playerData.deck = new DeckInstance(startingPlayerDeck.CardIds);
        }
    }

    public int GetPlayerHealth()
    {
        return gameSessionData.sessionPlayerData.health;
    }

    public int GetPlayerMaxHealth()
    {
        return gameSessionData.sessionPlayerData.maxHealth;
    }

    public int GetPlayerMana()
    {
        return gameSessionData.sessionPlayerData.mana;
    }

    public int GetPlayerGold()
    {
        return gameSessionData.sessionPlayerData.gold;
    }

    public DeckInstance GetPlayerDeck()
    {
        return gameSessionData.sessionPlayerData.deck;
    }

    /// <summary>
    /// Returns the active node map data for the current mode (main run vs tutorial).
    /// Used by map systems to read/write node progression.
    /// </summary>
    public SessionNodeMapData GetActiveNodeMapData()
    {
        if (gameSessionData == null)
            return null;

        return IsTutorialMode
            ? gameSessionData.tutorialNodeMapData
            : gameSessionData.sessionNodeMapData;
    }

    public System.Collections.Generic.IReadOnlyList<RelicData> GetPlayerRelics()
    {
        if (gameSessionData == null || gameSessionData.sessionPlayerData == null)
            return System.Array.Empty<RelicData>();

        // Expose as IReadOnlyList so callers can't modify the list directly.
        System.Collections.Generic.IReadOnlyList<RelicData> relicList =
            gameSessionData.sessionPlayerData.relics;

        return relicList ?? System.Array.Empty<RelicData>();
    }

    public void SetPlayerHealth(int health)
    {
        gameSessionData.sessionPlayerData.health = health;
    }

    public void SetPlayerGold(int gold)
    {
        gameSessionData.sessionPlayerData.gold = gold;
    }
}
