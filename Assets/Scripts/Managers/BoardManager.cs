using UnityEngine;
using UnityEngine.InputSystem;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }
    [SerializeField] public GameObject cardPrefab3D;
    [SerializeField] private Camera mainCamera;

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
    }

    private void OnDisable()
    {
        inputActions.Player.Click.performed -= OnClickPerformed;
        inputActions.Disable();
    }

    public void InitializeBoard()
    {
        Debug.Log("[BoardManager] Board initialized.");
    }

    private void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var slot = hit.collider.GetComponentInParent<BoardSlot>();
            if (slot != null)
            {
                Debug.Log($"[BoardManager] Clicked pillar: {slot.name} of {slot.transform.parent.name}");
                TryPlaceSelectedCard(slot);
                return;
            }

            // log other clicks
            Debug.Log($"[BoardManager] Clicked something else: {hit.collider.name}");
        }
    }

    public void SelectCard(CardUIController card)
    {
        if (selectedCard != null)
            selectedCard.SetSelectedVisual(false);

        selectedCard = card;
        selectedCard.SetSelectedVisual(true);
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
