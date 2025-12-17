using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }
    [SerializeField] public GameObject cardPrefab3D;
    [SerializeField] private Camera mainCamera;

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
        // Lock input during banner animations
        if (TurnManager.Instance != null && TurnManager.Instance.turnBanner != null)
        {
            if (TurnManager.Instance.turnBanner.IsShowing)
            {
                Debug.Log("[BoardManager] Input locked - banner is showing.");
                return;
            }
        }
        
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit)) {
            Debug.Log("[BoardManager] Raycast hit nothing.");
            return;
        }
        
        Debug.Log($"[BoardManager] Raycast hit: {hit.collider.name} (GameObject: {hit.collider.gameObject.name}, Tag: {hit.collider.tag})");
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
                        AudioManager.Instance.PlaySoundById(inst.Data.playCardSoundID);
                    }
                }
                // clicks elsewhere while minion card is selected: ignore
                return;
            }

            // 2b) SPELL CARD: second click chooses the target
            // Check both physics raycast and UI raycast for targeting
            var targetEntity = ResolveClickToSpellTarget(hit, inst);
            
            // If no target from physics raycast, try UI raycast (for portraits)
            if (targetEntity == null)
            {
                targetEntity = ResolveClickToEntityFromUI();
            }
            
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

                AudioManager.Instance.PlaySoundById(inst.Data.playCardSoundID);

                // Show spell flash animation
                StartCoroutine(SpellFlashAnimation(inst, targetEntity));

                _ = inst.PlayCardAsync(null, targetEntity);
                
                // Track player spell in history
                var playHistory = FindFirstObjectByType<CardPlayHistory>();
                if (playHistory != null && inst.Data != null)
                {
                    playHistory.OnPlayerPlayedCard(inst.Data);
                }

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
            // Check if we clicked on the same minion that's selected - if so, deselect it
            var clickedMinion = hit.collider.GetComponentInParent<MinionBehaviour>();
            if (clickedMinion != null && clickedMinion == selectedMinion)
            {
                Debug.Log("[BoardManager] Clicked on the same minion that's already selected. Deselecting it.");
                selectedMinion.SetSelected(false);
                selectedMinion = null;
                return;
            }

            // First try to get a direct entity from the click
            var target = ResolveClickToEntityStrict(hit);

            // Detect self (minion should not attack itself)
            var selfEntity = selectedMinion.GetComponent<MinionEntity>();
            if (target != null && selfEntity != null && ReferenceEquals(target, selfEntity))
            {
                // If we clicked on ourselves, treat as "no valid target"
                target = null;
            }

            // If no target from physics raycast, try UI raycast (for portraits)
            if (target == null)
            {
                target = ResolveClickToEntityFromUI();
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
                AudioManager.Instance.PlaySoundById(selectedMinion.instance.Data.cardAttackSoundID);
                selectedMinion.SetSelected(false);
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
                    // If clicking the same minion that's already selected, deselect it
                    if (selectedMinion == clickedMinion)
                    {
                        Debug.Log("[BoardManager] Clicked on the same minion that's already selected. Deselecting it.");
                        selectedMinion.SetSelected(false);
                        selectedMinion = null;
                        return;
                    }
                    
                    // Deselect previous minion
                    if (selectedMinion != null)
                    {
                        selectedMinion.SetSelected(false);
                    }
                    
                    selectedMinion = clickedMinion; // first click selects; no immediate attack
                    selectedMinion.SetSelected(true);
                }
                return;
            }
        }

    }

    public bool IsSelectedCard(CardUIController card)
    {
        return selectedCard == card;
    }

    public void SelectCard(CardUIController card)
    {
        // If clicking the same card that's already selected, deselect it
        if (selectedCard == card)
        {
            DeselectCard();
            return;
        }

        if (selectedCard != null)
            selectedCard.SetSelectedVisual(false);

        selectedCard = card;

        if (selectedCard != null)
            selectedCard.SetSelectedVisual(true);

        // Deselect minion when selecting a card
        if (selectedMinion != null)
        {
            selectedMinion.SetSelected(false);
            selectedMinion = null;
        }
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

        // Prevent player cards from being placed on enemy slots
        if (inst.Owner is PlayerEntity)
        {
            if (IsEnemySlot(slot))
            {
                Debug.Log("[BoardManager] Cannot place player card on enemy slot.");
                return false;
            }
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
            // Track player card in history
            var playHistory = FindFirstObjectByType<CardPlayHistory>();
            if (playHistory != null && inst.Data != null)
            {
                playHistory.OnPlayerPlayedCard(inst.Data);
            }
            
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

    // SPELLS: enemy/minion if clicked; if empty slot is clicked, apply buff/debuff to heroes
    private EntityBase ResolveClickToSpellTarget(RaycastHit hit, CardInstance spell)
    {
        // 1) Direct hit: enemy hero
        var ee = hit.collider.GetComponentInParent<EnemyEntity>();
        if (ee != null) return ee;

        // 2) Direct hit: any minion
        var me = hit.collider.GetComponentInParent<MinionEntity>();
        if (me != null) return me;

        // 3) Empty board slot → special handling for buff/debuff spells
        var slot = hit.collider.GetComponentInParent<BoardSlot>();
        if (slot != null && !slot.isOccupied)
        {
            return ResolveEmptySlotSpellTarget(spell);
        }

        // 4) Anything else: no valid target
        return null;
    }

    /// <summary>
    /// When a spell is cast on an empty slot, decide who should receive it:
    /// - Pure buff card (Strength only): owner hero
    /// - Pure debuff card (Weakness / Poison only): opponent hero
    /// - Mixed/other effects: fall back to default enemy hero (old behavior).
    /// </summary>
    private EntityBase ResolveEmptySlotSpellTarget(CardInstance spell)
    {
        var bm = BattleManager.Instance;
        if (bm == null) return null;
        if (spell == null || spell.Data == null || spell.Data.effects == null || spell.Data.effects.Count == 0)
        {
            // No info → old behavior: treat as attack to enemy hero
            return GetDefaultEnemyHero();
        }

        bool hasStrength = false;
        bool hasWeakness = false;
        bool hasPoison = false;
        bool hasOther = false;

        foreach (var binding in spell.Data.effects)
        {
            if (binding?.effect == null) continue;

            if (binding.effect is StrengthEffect)
                hasStrength = true;
            else if (binding.effect is WeaknessEffect)
                hasWeakness = true;
            else if (binding.effect is PoisonEffect)
                hasPoison = true;
            else
                hasOther = true;
        }

        bool anyBuffLike = hasStrength;
        bool anyDebuffLike = hasWeakness || hasPoison;

        // If the card mixes buffs + debuffs or has other effects, keep old behavior
        if (hasOther || (anyBuffLike && anyDebuffLike))
        {
            return GetDefaultEnemyHero();
        }

        var owner = spell.Owner;

        // Player casting
        if (owner is PlayerEntity)
        {
            // Buff-only → self hero
            if (anyBuffLike && !anyDebuffLike)
            {
                return bm.player;
            }

            // Debuff-only → enemy hero
            if (anyDebuffLike && !anyBuffLike)
            {
                return GetDefaultEnemyHero();
            }
        }
        // Enemy casting
        else if (owner is EnemyEntity enemyOwner)
        {
            // Buff-only → that specific enemy hero
            if (anyBuffLike && !anyDebuffLike)
            {
                return enemyOwner;
            }

            // Debuff-only → player hero
            if (anyDebuffLike && !anyBuffLike)
            {
                return bm.player;
            }
        }

        // Fallback: behave like a normal attack spell to enemy hero
        return GetDefaultEnemyHero();
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
            var enemies = FindObjectsByType<EnemyEntity>(FindObjectsSortMode.None);
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

    /// <summary>
    /// Shows a spell card briefly on the board when cast.
    /// Can be called from EnemyAI or player actions.
    /// </summary>
    public void ShowSpellFlashAnimation(CardInstance spell, EntityBase target)
    {
        if (spell != null && target != null)
        {
            StartCoroutine(SpellFlashAnimation(spell, target));
        }
    }

    /// <summary>
    /// Shows a spell card briefly on the board when cast
    /// </summary>
    private System.Collections.IEnumerator SpellFlashAnimation(CardInstance spell, EntityBase target)
    {
        if (cardPrefab3D == null || spell == null) yield break;

        // Calculate position - use attackTargetTransform for entities, or target position for minions
        Vector3 flashPosition;
        if (target != null)
        {
            flashPosition = GetEntityPositionForSpell(target);
            
            // Offset upward for better visibility (smaller offset to keep it in camera view)
            // For minions, use a smaller offset since they're already on the board
            if (target is MinionEntity)
            {
                flashPosition += Vector3.up * 1.5f;
            }
            else
            {
                // For enemy/player entities, use a smaller offset to keep it visible
                flashPosition += Vector3.up * 0.5f;
                
                // Shift enemy target left (toward center of screen) for better visibility
                if (target is EnemyEntity)
                {
                    flashPosition += Vector3.left; // Adjust this value to move more/less left
                }
                else if (target is PlayerEntity)
                {
                    flashPosition += Vector3.right; // Adjust this value to move more/less right
                }
            }
        }
        else
        {
            // Center of board (you may need to adjust this based on your scene)
            flashPosition = new Vector3(0, 0, 4);
            if (mainCamera != null)
            {
                flashPosition = mainCamera.transform.position + mainCamera.transform.forward * 5f;
            }
            flashPosition += Vector3.up * 1.5f;
        }

        // Instantiate the card
        GameObject flashCard = Instantiate(cardPrefab3D, flashPosition, Quaternion.identity);
        flashCard.name = $"SpellFlash_{spell.Data.cardName}";

        // Initialize the card controller
        var controller = flashCard.GetComponent<Card3DController>();
        if (controller != null)
        {
            controller.Initialize(spell);
        }

        // Scale up quickly
        float duration = 0.8f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = flashCard.transform.localScale;

        while (elapsed < duration * 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.3f);
            flashCard.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Hold for a moment
        yield return new WaitForSeconds(duration * 0.4f);

        // Fade out and scale down
        elapsed = 0f;
        Vector3 finalScale = targetScale;
        CanvasGroup canvasGroup = flashCard.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = flashCard.AddComponent<CanvasGroup>();
        }

        while (elapsed < duration * 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.3f);
            flashCard.transform.localScale = Vector3.Lerp(finalScale, Vector3.zero, t);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - t;
            }
            yield return null;
        }

        // Clean up
        Destroy(flashCard);
    }

    /// <summary>
    /// Resolves entity target from UI raycast (for clicking on portraits).
    /// Used for both spell targeting and minion attack targeting.
    /// </summary>
    private EntityBase ResolveClickToEntityFromUI()
    {
        // Check if we're clicking on UI
        if (EventSystem.current == null) return null;
        
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Mouse.current.position.ReadValue();
        
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        
        if (results.Count == 0) return null;
        
        // Check each UI element that was hit
        foreach (var result in results)
        {
            GameObject hitObject = result.gameObject;
            
            // Check if this UI element is part of the enemy or player portrait
            // Look for AvatarUI component in parent hierarchy
            AvatarUI avatarUI = hitObject.GetComponentInParent<AvatarUI>();
            if (avatarUI == null) continue;
            
            // Find which entity this portrait belongs to
            var bm = BattleManager.Instance;
            if (bm == null) continue;
            
            // Check if it's the enemy UI
            if (bm.uiManager != null && bm.uiManager.enemyAvatar == avatarUI)
            {
                // Return enemy entity
                return GetDefaultEnemyHero();
            }
            
            // Check if it's the player UI
            if (bm.uiManager != null && bm.uiManager.playerAvatar == avatarUI)
            {
                // Return player entity
                return bm.player;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Checks if a board slot belongs to the enemy side.
    /// </summary>
    private bool IsEnemySlot(BoardSlot slot)
    {
        if (slot == null) return false;
        
        // Check slot name (e.g., "enemyMeleeA", "enemyRangedB")
        if (slot.name.StartsWith("enemy", System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // Check parent name (e.g., "Enemy Melee Slots", "Enemy Ranged Slots")
        if (slot.transform.parent != null)
        {
            string parentName = slot.transform.parent.name;
            if (parentName.Contains("Enemy", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Gets the world position for a spell animation target.
    /// Uses attackTargetTransform for PlayerEntity/EnemyEntity, or transform.position for MinionEntity.
    /// </summary>
    private Vector3 GetEntityPositionForSpell(EntityBase entity)
    {
        if (entity == null) return Vector3.zero;

        // For MinionEntity: use the minion's actual position
        var minionEntity = entity as MinionEntity;
        if (minionEntity != null)
        {
            var mb = minionEntity.GetComponent<MinionBehaviour>();
            if (mb != null && mb.slot != null)
            {
                return mb.slot.transform.position;
            }
            // Fallback: use minion's transform if slot not available
            if (minionEntity.transform != null)
            {
                return minionEntity.transform.position;
            }
        }

        // For EnemyEntity/PlayerEntity: use the inspector-set attackTargetTransform if available
        if (entity.attackTargetTransform != null)
        {
            return entity.attackTargetTransform.position;
        }

        // Fallback: use a position offset from current position
        Vector3 offsetPos = Vector3.zero;
        if (entity.transform != null)
        {
            offsetPos = entity.transform.position;
        }
        
        if (entity is EnemyEntity)
        {
            offsetPos += Vector3.forward * 3.0f; // Move forward
        }
        else if (entity is PlayerEntity)
        {
            offsetPos += Vector3.back * 3.0f; // Move backward
        }
        offsetPos.y += 0.0f;
        return offsetPos;
    }

}
