using System;
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
            Debug.LogWarning($"[BoardManager] Duplicate BoardManager found on {gameObject.name} in scene {gameObject.scene.name}. Destroying.");
            DestroyImmediate(gameObject);
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
        Debug.Log("[BoardManager] OnDisable called.");
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

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var slot = hit.collider.GetComponentInParent<BoardSlot>();
            if (slot != null)
            {
                Debug.Log($"[BoardManager] Clicked pillar: {slot.name} of {slot.transform.parent.name}");
                TryPlaceSelectedCard(slot);
                return;
            }
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

        var cardInstance = selectedCard.Instance;
        if (cardInstance == null)
        {
            Debug.LogWarning("[BoardManager] Selected card instance is null.");
            return;
        }

        if (cardInstance.Owner is PlayerEntity)
        {
            if (BattleManager.Instance == null)
            {
                Debug.LogWarning("[BoardManager] No BattleManager found to check mana.");
                return;
            }

            if (BattleManager.Instance.playerMana < cardInstance.Data.cost)
            {
                Debug.Log("[BoardManager] Not enough player mana to play that card.");
                return;
            }

            BattleManager.Instance.playerMana -= cardInstance.Data.cost;
            BattleManager.Instance.uiManager.UpdatePlayerMana(BattleManager.Instance.playerMana);
        }

        // If it's a spell, place temporarily and leave it to resolution; do not call PlaceCard
        if (cardInstance.Data.type == "spell")
        {
            bool placed = slot.PlaceSpell(cardInstance);
            if (placed)
            {
                cardInstance.PlayCard(slot);
                Destroy(selectedCard.gameObject);
                selectedCard = null;
            }
            return;
        }

        if (slot.PlaceCard(cardInstance))
        {
            // mark card as played
            cardInstance.PlayCard(slot);

            Destroy(selectedCard.gameObject);
            selectedCard = null;
        }
    }

    // Resolve spell cards on board (called from EndTurnResolve)
    public void ResolveAndClearSpellsForSide(bool fromPlayer)
    {
        if (BattleManager.Instance == null)
        {
            Debug.LogWarning("[BoardManager] No BattleManager instance while resolving spells.");
            return;
        }

        var slots = UnityEngine.Object.FindObjectsOfType<BoardSlot>();
        if (slots == null) return;

        foreach (var s in slots)
        {
            if (s == null) continue;
            if (s.currentCard == null) continue;
            var card = s.currentCard;
            if (card.Data == null) continue;

            bool ownedByPlayer = card.Owner is PlayerEntity;
            if (fromPlayer != ownedByPlayer)
                continue;

            if (card.Data.type == "spell")
            {
                // Ensure owner exists; fallback to battle manager entity if missing
                EntityBase caster = card.Owner ?? (ownedByPlayer ? (EntityBase)BattleManager.Instance.playerEntity : (EntityBase)BattleManager.Instance.enemyEntity);

                // Determine target: heals target the owner, damage targets the opponent
                CardEffect effect = card.Data.effect;
                EntityBase target = null;
                bool isHeal = effect is HealEffect;

                if (isHeal)
                {
                    target = ownedByPlayer ? (EntityBase)BattleManager.Instance.playerEntity : (EntityBase)BattleManager.Instance.enemyEntity;
                }
                else
                {
                    target = ownedByPlayer ? (EntityBase)BattleManager.Instance.enemyEntity : (EntityBase)BattleManager.Instance.playerEntity;
                }

                if (effect != null)
                {
                    try
                    {
                        if (caster != null && target != null)
                            effect.Execute(caster, target);
                        else
                            Debug.LogWarning("[BoardManager] Missing caster or target for spell execution.");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[BoardManager] Exception while executing spell effect: {ex}");
                    }
                }

                // remove spell visual from board
                s.ClearCurrentCard();
            }
        }
    }
}
