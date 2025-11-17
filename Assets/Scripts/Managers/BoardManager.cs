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
    private System.Collections.Generic.List<CardData.EffectBinding> pendingSummonTargetedBindings;

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
            // For on-summon "attack" bindings: empty click should target EnemyEntity
            var target = ResolveClickToEntityOrEnemyIfEmpty(hit);

            var bm = BattleManager.Instance;
            var caster = bm ? bm.player : null;

            if (target != null && pendingSummonTargetedBindings != null)
            {
                foreach (var b in pendingSummonTargetedBindings)
                {
                    if (b?.effect == null) continue;
                    b.effect.Execute(caster, target, b.value);
                }
            }

            pendingSummonCard = null;
            pendingSummonSlot = null;
            pendingSummonTargetedBindings = null;
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
                        // Partition onSummon: auto-self (GainShield) vs. targeted (Damage, etc.)
                        var bm = BattleManager.Instance;
                        var caster = bm ? bm.player : null;

                        var all = slot.currentCard?.Data?.onSummonBindings;
                        if (all != null && all.Count > 0)
                        {
                            // Auto apply any GainShield effects to player immediately
                            foreach (var b in all)
                            {
                                if (b?.effect is GainShieldEffect)
                                {
                                    b.effect.Execute(caster, null, b.value); // no second click
                                }
                            }

                            // Keep only targeted/attack-like ones for next click
                            pendingSummonTargetedBindings = new System.Collections.Generic.List<CardData.EffectBinding>();
                            foreach (var b in all)
                            {
                                if (b?.effect is GainShieldEffect) continue; // already handled
                                pendingSummonTargetedBindings.Add(b);
                            }

                            // If there are targeted bindings left, arm the pending state
                            if (pendingSummonTargetedBindings.Count > 0)
                            {
                                pendingSummonCard = slot.currentCard;
                                pendingSummonSlot = slot;
                            }
                        }
                    }

                }
                // clicks elsewhere while minion card is selected: ignore
                return;
            }

            // 2b) SPELL CARD: second click chooses the target
            var targetEntity = ResolveClickToSpellTarget(hit);
            if (targetEntity != null)
            {
                var bm = BattleManager.Instance;
                var caster = bm ? bm.player : null;
                inst.ResolveSpellEffects(caster, targetEntity);

                var hm = FindFirstObjectByType<HandManager>();
                if (hm != null) hm.RemoveByInstance(inst);

                Destroy(selectedCard.gameObject);
                DeselectCard();
            }
            return;
        }

        // 3) If an on-board minion is selected, second click chooses the target to attack/effect
        if (selectedMinion != null)
        {
            var target = ResolveClickToEntityStrict(hit);
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
            return false;
        }

        if (inst.Owner is PlayerEntity)
        {
            if (BattleManager.Instance == null)
            {
                Debug.LogWarning("[BoardManager] No BattleManager found to check mana.");
                return false;
            }

            if (BattleManager.Instance.playerMana < inst.Data.cost)
            {
                Debug.Log("[BoardManager] Not enough player mana to play that card.");
                return false;
            }

            BattleManager.Instance.playerMana -= inst.Data.cost;
            BattleManager.Instance.uiManager.UpdatePlayerMana(BattleManager.Instance.playerMana);
        }

        return false;
    }

    // Strict resolver: only returns a target if you click directly on it
    private EntityBase ResolveClickToEntityStrict(RaycastHit hit)
    {
        // Enemy hero
        var ee = hit.collider.GetComponentInParent<EnemyEntity>();
        if (ee != null) return ee;

        // Any minion on the board
        var me = hit.collider.GetComponentInParent<MinionEntity>();
        if (me != null) return me;

        // Otherwise: no valid target
        return null;
    }

    // SPELLS: enemy/minion if clicked; if empty slot is clicked, treat as EnemyEntity
    private EntityBase ResolveClickToSpellTarget(RaycastHit hit)
    {
        // Direct hit: enemy or minion
        var ee = hit.collider.GetComponentInParent<EnemyEntity>();
        if (ee != null) return ee;

        var me = hit.collider.GetComponentInParent<MinionEntity>();
        if (me != null) return me;

        // Empty slot → enemy
        var slot = hit.collider.GetComponentInParent<BoardSlot>();
        if (slot != null && !slot.isOccupied)
        {
            var enemies = FindObjectsOfType<EnemyEntity>();
            if (enemies != null && enemies.Length > 0)
                return enemies[0];
            return null;
        }

        // Anything else: no target
        return null;
    }

    // For on-summon: enemy/minion if clicked, or enemy if empty slot is clicked
    private EntityBase ResolveClickToEntityOrEnemyIfEmpty(RaycastHit hit)
    {
        // Direct hit: enemy or minion
        var ee = hit.collider.GetComponentInParent<EnemyEntity>();
        if (ee != null) return ee;

        var me = hit.collider.GetComponentInParent<MinionEntity>();
        if (me != null) return me;

        // Empty slot → enemy
        var slot = hit.collider.GetComponentInParent<BoardSlot>();
        if (slot != null && !slot.isOccupied)
        {
            var enemies = FindObjectsOfType<EnemyEntity>();
            if (enemies != null && enemies.Length > 0)
                return enemies[0];   // or choose by index if you ever have >1
            return null;
        }

        // Anything else: no target
        return null;
    }
}
