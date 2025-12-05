using UnityEngine;
using UnityEngine.InputSystem;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public GameSessionData gameSessionData;
    [SerializeField] private DeckDefinition startingPlayerDeck;

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

    // Debug methods for pause menu (to be expanded later)
    public void SaveGame()
    {
        if (gameSessionData != null)
        {
            SessionSaveManager.SaveGameSession(gameSessionData);
            Debug.Log("[GameSession] Game saved successfully.");
        }
        else
        {
            Debug.LogWarning("[GameSession] Cannot save: gameSessionData is null.");
        }
    }

    public void LoadGame()
    {
        LoadGameSession("Save");
        Debug.Log("[GameSession] Game loaded successfully.");
    }

    //-------Temporary for testing Reset/Save/Load - Ctrl+Key functionality--------------------
    private void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;

        // Check for Ctrl+S (Save)
        if (kb.ctrlKey.isPressed && kb.sKey.wasPressedThisFrame)
        {
            SaveGame();
        }

        // Check for Ctrl+L (Load)
        if (kb.ctrlKey.isPressed && kb.lKey.wasPressedThisFrame)
        {
            LoadGame();
        }

        // Check for Ctrl+R (Reset)
        if (kb.ctrlKey.isPressed && kb.rKey.wasPressedThisFrame)
        {
            ResetGameSessionData();
        }
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

        if (loadedData != null)
        {
            gameSessionData = loadedData;
            EnsurePlayerDeckInitialized();
        }
        else
        {
            Debug.Log("Failed to Load Save Data: Null Session Data");
        }
    }

    private void EnsurePlayerDeckInitialized()
    {
        if (gameSessionData == null)
            return;

        if (gameSessionData.sessionPlayerData == null)
            gameSessionData.sessionPlayerData = new SessionPlayerData();

        if (gameSessionData.sessionNodeMapData == null)
            gameSessionData.sessionNodeMapData = new SessionNodeMapData();

        if (gameSessionData.tutorialNodeMapData == null)
            gameSessionData.tutorialNodeMapData = new SessionNodeMapData();

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

    public SessionNodeMapData GetActiveNodeMapData()
    {
        if (gameSessionData == null)
            return null;

        if (IsTutorialMode)
        {
            if (gameSessionData.tutorialNodeMapData == null)
                gameSessionData.tutorialNodeMapData = new SessionNodeMapData();
            return gameSessionData.tutorialNodeMapData;
        }

        if (gameSessionData.sessionNodeMapData == null)
            gameSessionData.sessionNodeMapData = new SessionNodeMapData();
        return gameSessionData.sessionNodeMapData;
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

    public void SetPlayerHealth(int health)
    {
        gameSessionData.sessionPlayerData.health = health;
    }

    public void SetPlayerGold(int gold)
    {
        gameSessionData.sessionPlayerData.gold = gold;
    }
    
    public RelicInventory GetPlayerRelicInventory()
    {
        return gameSessionData.sessionPlayerData.relicInventory;
    }
}
