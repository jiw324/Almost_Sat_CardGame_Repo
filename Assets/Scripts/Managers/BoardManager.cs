using UnityEngine;
using UnityEngine.InputSystem;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }
    [SerializeField] public GameObject cardPrefab3D;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject reactionMinigamePrefab;

    private CardUIController selectedCard;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        inputActions = new InputSystem_Actions();
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Click.performed += OnClickPerformed;
        inputActions.Player.Minigame.performed += OnTestMinigame;
    }

    private void OnDisable()
    {
        inputActions.Player.Click.performed -= OnClickPerformed;
        inputActions.Player.Minigame.performed -= OnTestMinigame;
        inputActions.Disable();
    }

    public void InitializeBoard()
    {
        Debug.Log("[BoardManager] Board initialized.");
    }

    private void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        // If a hand card is selected, try to place it on a slot (existing behavior)
        var slot = hit.collider.GetComponentInParent<BoardSlot>();
        if (selectedCard != null && slot != null)
        {
            TryPlaceSelectedCard(slot);
            return;
        }

        // No card selected: clicking a minion triggers its attack
        var minion = hit.collider.GetComponentInParent<MinionBehaviour>();
        if (minion != null)
        {
            minion.AttackEnemy();
            return;
        }
    }

    private void OnTestMinigame(InputAction.CallbackContext ctx)
    {
        if (MinigameManager.Instance == null)
        {
            Debug.LogWarning("[BoardManager] No MinigameManager instance found!");
            return;
        }

        Debug.Log("[BoardManager] Launching Reaction Minigame!");
        MinigameManager.Instance.StartMinigame(
            reactionMinigamePrefab,
            result =>
            {
                Debug.Log($"[BoardManager] Minigame complete. Score: {result:F2}");
            });
    }

    public bool IsSelectedCard(CardUIController card)
    {
        return selectedCard == card;
    }

    public void SelectCard(CardUIController card)
    {
        if (selectedCard != null)
            selectedCard.SetSelectedVisual(false);

        selectedCard = card;

        if (selectedCard != null)
            selectedCard.SetSelectedVisual(true);
    }

    public void DeselectCard()
    {
        if (selectedCard != null)
        {
            selectedCard.SetSelectedVisual(false);
            selectedCard = null;
        }
    }

    public void TryPlaceSelectedCard(BoardSlot slot)
    {
        if (selectedCard == null)
        {
            Debug.Log("[BoardManager] No card selected.");
            return;
        }

        if (slot.isOccupied)
        {
            Debug.Log("[BoardManager] Slot already occupied.");
            return;
        }

        if (slot.PlaceCard(selectedCard.Instance))
        {
            Destroy(selectedCard.gameObject); // remove from hand
            selectedCard = null;
        }
    }
}
