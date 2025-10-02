using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    private GameStateBase currentState;
    public InputSystem_Actions InputActions { get; private set; }
    
    private void Awake()
    {
        // Ensure only one instance of GameManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        InputActions.Enable();
    }
    private void OnDisable()
    {
        InputActions.Disable();
    }

    private void Start()
    {
        ChangeState(new PlayerTurnState(this));
    }

    public void ChangeState(GameStateBase newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    // Update is called once per frame
    private void Update()
    {
        currentState?.Update();
    }
}
