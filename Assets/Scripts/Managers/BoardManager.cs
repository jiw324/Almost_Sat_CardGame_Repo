using System;
using System.Threading.Tasks;
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
    }

    private void OnDisable()
    {
        Debug.Log("[BoardManager] OnDisable called.");
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

                if (inst.Owner is PlayerEntity)
                {
                    if (bm == null)
                    {
                        Debug.LogWarning("[BoardManager] No BattleManager found to check mana.");
                        return;
                    }

                    if (bm.playerMana < inst.Data.cost)
                    {
                        Debug.Log("[BoardManager] Not enough player mana to cast that spell.");
                        return;
                    }

                    bm.playerMana -= inst.Data.cost;
                    if (bm.uiManager != null) bm.uiManager.UpdatePlayerMana(bm.playerMana);
                }

                Debug.Log($"[BoardManager] Casting spell {inst.Data.cardName} by Player targeting {GetTargetDescription(targetEntity)}");
                _ = inst.PlayCardAsync(null, targetEntity);

                if (bm != null && bm.uiManager != null)
                {
                    bm.uiManager.UpdatePlayerHealth(bm.playerHealth);
                    bm.uiManager.UpdateEnemyHealth(bm.enemyHealth);
                }

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
            // First try to get a direct entity from the click
            var target = ResolveClickToEntityStrict(hit);

            // Detect self (minion should not attack itself)
            var selfEntity = selectedMinion.GetComponent<MinionEntity>();
            if (target != null && selfEntity != null && ReferenceEquals(target, selfEntity))
            {
                // If we clicked on ourselves, treat as "no valid target"
                target = null;
            }

            // If no direct entity, but we clicked something like an empty slot,
            // make the minion behave like a spell: attack the enemy hero.
            if (target == null)
            {
                var slot = hit.collider.GetComponentInParent<BoardSlot>();
                if (slot != null && !slot.isOccupied)
                {
                    target = GetDefaultEnemyHero();
                }
            }

            // Only attack if we ended up with a valid target
            if (target != null)
            {
                selectedMinion.AttackTarget(target);
                selectedMinion = null; // done
            }

            return;
        }


        // 4) Nothing selected yet: first click on a *friendly* minion selects it (for directed attack)
        {
            var clickedMinion = hit.collider.GetComponentInParent<MinionBehaviour>();
            if (clickedMinion != null && clickedMinion.instance != null)
            {
                // Only allow selecting player-owned minions
                if (clickedMinion.instance.Owner is PlayerEntity)
                {
                    selectedMinion = clickedMinion; // first click selects; no immediate attack
                }
                return;
            }
        }

    }

    private async void PlayReactionCard()
    {
        if (MinigameManager.Instance == null)
        {
            Debug.LogWarning("[BoardManager] No MinigameManager instance found!");
            return;
        }
        Debug.Log("[BoardManager] Launching Reaction Minigame!");
        {
            float result = await MinigameManager.Instance.StartMinigameAsync(reactionMinigamePrefab);
            Debug.Log($"[BoardManager] Minigame complete. Score: {result:F2}");
        }
    }

    private void OnTestMinigame(InputAction.CallbackContext ctx)
    {
        PlayReactionCard();
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
        if (inst == null)
        {
            Debug.LogWarning("[BoardManager] Selected card instance is null.");
            return false;
        }

        var bm = BattleManager.Instance;

        if (inst.Owner is PlayerEntity)
        {
            if (bm == null)
            {
                Debug.LogWarning("[BoardManager] No BattleManager found to check mana.");
                return false;
            }

            if (bm.playerMana < inst.Data.cost)
            {
                Debug.Log("[BoardManager] Not enough player mana to play that card.");
                return false;
            }

            bm.playerMana -= inst.Data.cost;
            if (bm.uiManager != null) bm.uiManager.UpdatePlayerMana(bm.playerMana);
        }

        _ = inst.PlayCardAsync(slot);
        if (slot.PlaceCard(inst))
        {
            var hm = FindFirstObjectByType<HandManager>();
            if (hm != null) hm.RemoveByInstance(inst);
            Destroy(selectedCard.gameObject);
            selectedCard = null;
            return true;
        }
        else
        {
            if (inst.Owner is PlayerEntity && bm != null)
            {
                bm.playerMana += inst.Data.cost;
                if (bm.uiManager != null) bm.uiManager.UpdatePlayerMana(bm.playerMana);
            }
            Debug.LogWarning("[BoardManager] Failed to place selected card on slot.");
            return false;
        }
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
                return enemies[0];
            return null;
        }

        // Anything else: no target
        return null;
    }

    private string GetTargetDescription(EntityBase target)
    {
        if (target == null) return "(none)";
        if (target is PlayerEntity) return "Player";
        if (target is EnemyEntity) return "Enemy Hero";
        var m = target as MinionEntity;
        if (m != null) return $"Minion:{m.entityName}";
        return target.entityName ?? target.name ?? target.GetType().Name;
    }

    private EnemyEntity GetDefaultEnemyHero()
    {
        var bm = BattleManager.Instance;
        if (bm == null) return null;

        // Prefer enemies list if present
        if (bm.enemies != null && bm.enemies.Count > 0)
        {
            foreach (var e in bm.enemies)
            {
                if (e != null && e.currentHealth > 0)
                    return e;
            }
        }

        // Fallback to single enemyEntity
        if (bm.enemyEntity != null && bm.enemyEntity.currentHealth > 0)
            return bm.enemyEntity;

        return null;
    }

}
