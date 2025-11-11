using UnityEngine;
using UnityEngine.InputSystem;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public GameSessionData gameSessionData;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        gameSessionData.ResetSessionData();
        Debug.Log("Game Session Reset");
    }

    public void LoadGameSession(string filePath)
    {
        GameSessionData loadedData = SessionSaveManager.LoadGameSession(filePath);

        if(loadedData != null)
        {
            gameSessionData = loadedData;
        } else
        {
            Debug.Log("Failed to Load Save Data: Null Session Data");
        }
    }
}
