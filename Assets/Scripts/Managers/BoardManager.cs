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
    private MinionBehaviour selectedMinion;
    private InputSystem_Actions inputActions;

    private BoardSlot pendingSummonSlot;
    private CardInstance pendingSummonCard;

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
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        // 1) If we are waiting for a summon target right after placing a minion
        if (pendingSummonCard != null && pendingSummonSlot != null)
        {
            var target = ResolveClickToEntityOrNearest(hit);
            var bm = BattleManager.Instance;
            var caster = bm ? bm.player : null;
            pendingSummonCard.ResolveMinionSummonEffects(caster, target);
            pendingSummonCard = null;
            pendingSummonSlot = null;
            return;
        }

        // 2) If a hand card is selected
        if (selectedCard != null)
        {
            var inst = selectedCard.Instance;

            // 2a) MINION CARD: placing on a slot
            var slot = hit.collider.GetComponentInParent<BoardSlot>();
            if (inst.IsMinion)
            {
                if (slot != null)
                {
                    // Try place; if success, optionally prompt for summon target
                    if (TryPlaceSelectedCard(slot))
                    {
                        // If this minion has on-summon effects, defer and wait for target selection
                        if (inst.Data.onSummonBindings != null && inst.Data.onSummonBindings.Count > 0)
                        {
                            pendingSummonCard = slot.currentCard;  // just placed card
                            pendingSummonSlot = slot;
                        }
                    }
                }
                // clicks elsewhere while minion card is selected: ignore
                return;
            }

            // 2b) SPELL CARD: second click chooses the target (entity or empty slot �� nearest)
            var targetEntity = ResolveClickToEntityOrNearest(hit);
            if (targetEntity != null)
            {
                var bm = BattleManager.Instance;
                var caster = bm ? bm.player : null;
                inst.ResolveSpellEffects(caster, targetEntity);
                var hm = FindFirstObjectByType<HandManager>();
                if (hm != null) hm.RemoveByInstance(inst);
                // destroy hand UI and clear selection
                Destroy(selectedCard.gameObject);
                DeselectCard();
            }
            return;
        }

        // 3) If an on-board minion is selected, second click chooses the target to attack/effect
        if (selectedMinion != null)
        {
            var target = ResolveClickToEntityOrNearest(hit);
            if (target != null)
            {
                selectedMinion.AttackTarget(target);
                selectedMinion = null; // done
            }
            return;
        }

        // 4) Nothing selected yet: first click on a minion selects it (for directed attack)
        {
            var clickedMinion = hit.collider.GetComponentInParent<MinionBehaviour>();
            if (clickedMinion != null)
            {
                selectedMinion = clickedMinion; // first click selects; no immediate attack
                return;
            }
        }

        // Otherwise: click on empty / unrelated �� no action
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

        selectedMinion = null;
    }

    public void DeselectCard()
    {
        if (selectedCard != null)
        {
            selectedCard.SetSelectedVisual(false);
            selectedCard = null;
        }
    }

    private bool TryPlaceSelectedCard(BoardSlot slot)
    {
        if (selectedCard == null) 
        {
            Debug.Log("[BoardManager] No card selected."); 
            return false;
        }
        if (slot.isOccupied) 
        { 
            Debug.Log("[BoardManager] Slot already occupied."); 
            return false; 
        }

        var inst = selectedCard.Instance;
        if (slot.PlaceCard(inst))
        {
            var hm = FindFirstObjectByType<HandManager>();
            if (hm != null) hm.RemoveByInstance(selectedCard.Instance);
            Destroy(selectedCard.gameObject); // remove from hand UI
            selectedCard = null;
            return true;
        }

        if (inst == null)
        {
            Debug.LogWarning("[BoardManager] Selected card instance is null.");
            return;
        }

        if (inst.Owner is PlayerEntity)
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

        return false;
    }

    // Convert a click to an EntityBase target. If the click is an empty slot, choose nearest unit (minion or enemy).
    private EntityBase ResolveClickToEntityOrNearest(RaycastHit hit)
    {
        // Direct hits: enemy or minion-entity
        var ee = hit.collider.GetComponentInParent<EnemyEntity>();
        if (ee != null) return ee;

        var me = hit.collider.GetComponentInParent<MinionEntity>();
        if (me != null) return me;

        // Empty slot? pick nearest unit to that slot
        var slot = hit.collider.GetComponentInParent<BoardSlot>();
        if (slot != null && !slot.isOccupied)
        {
            return FindNearestUnit(slot.transform.position);
        }

        // If none matched, try nearest to hit point (safety)
        return FindNearestUnit(hit.point);
    }

    private EntityBase FindNearestUnit(Vector3 from)
    {
        EntityBase best = null;
        float bestSqr = float.PositiveInfinity;

        // All minion entities
        var minions = FindObjectsOfType<MinionEntity>();
        foreach (var m in minions)
        {
            float d = (m.transform.position - from).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = m; }
        }

        // Enemy entity as a target (off-board but has a transform)
        var enemy = BattleManager.Instance ? BattleManager.Instance.player?.GetComponentInParent<EnemyEntity>() : null;
        // Above line won��t find enemy; instead scan scene:
        if (best == null)
        {
            var enemies = FindObjectsOfType<EnemyEntity>();
            foreach (var e in enemies)
            {
                float d = (e.transform.position - from).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = e; }
            }
        }
        else
        {
            var enemies = FindObjectsOfType<EnemyEntity>();
            foreach (var e in enemies)
            {
                float d = (e.transform.position - from).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = e; }
            }
        }

        return best;
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
