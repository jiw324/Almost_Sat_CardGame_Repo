using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBehaviour : MonoBehaviour
{
    public BoardSlot slot { get; private set; }
    public CardInstance instance { get; private set; }

    // --- New activation state ---
    // True the turn the minion is summoned; cleared at the start of its owner's next turn.
    public bool hasSummoningSickness = true;
    // True once the minion has performed an attack this turn.
    public bool hasActedThisTurn = false;

    // --- Animation state ---
    [Header("Animation Settings")]
    [SerializeField] private float selectionScaleMultiplier = 1.2f;
    [SerializeField] private float selectionAnimationSpeed = 5f;
    [SerializeField] private float attackAnimationDuration = 0.5f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeIntensity = 0.1f;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool isSelected = false;
    private Coroutine currentAnimation;

    /// <summary>
    /// Can this minion currently perform an attack?
    /// </summary>
    public bool CanAct
    {
        get
        {
            if (instance == null) return false;
            if (hasSummoningSickness) return false;
            if (hasActedThisTurn) return false;

            var tm = TurnManager.Instance;
            if (tm == null) return true; // fail-safe: if no turn manager, don't block

            bool ownerIsPlayer = instance.Owner is PlayerEntity;
            bool isPlayerTurn = tm.IsPlayerTurn;

            // Minion may only act during its controller's turn
            return (ownerIsPlayer && isPlayerTurn) || (!ownerIsPlayer && !isPlayerTurn);
        }
    }

    public void Initialize(BoardSlot s, CardInstance i)
    {
        slot = s;
        instance = i;

        // if you have a world-space UI on the prefab, refresh it here
        var c3d = GetComponent<Card3DController>();
        if (c3d) c3d.Initialize(i);

        // Check if this minion ignores summoning sickness
        hasSummoningSickness = !(i.Data?.ignoreSummoningSickness ?? false);
        hasActedThisTurn = false;

        // Store original scale - use target scale from Card3DController if available
        // This ensures we get the correct final scale even if the animation hasn't finished
        var cardController = GetComponent<Card3DController>();
        if (cardController != null && cardController.TargetScale.magnitude > 0.01f)
        {
            originalScale = cardController.TargetScale;
        }
        else
        {
            originalScale = transform.localScale;
        }
        
        originalPosition = transform.localPosition;
    }

    private Vector3 GetOriginalWorldPosition()
    {
        return transform.position;
    }

    private void OnEnable()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStarted += HandlePlayerTurnStarted;
            TurnManager.Instance.OnEnemyTurnStarted += HandleEnemyTurnStarted;
        }
    }

    private void OnDisable()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStarted -= HandlePlayerTurnStarted;
            TurnManager.Instance.OnEnemyTurnStarted -= HandleEnemyTurnStarted;
        }
    }

    private void HandlePlayerTurnStarted()
    {
        if (instance == null) return;

        // Only care about player-owned minions on player turn
        if (instance.Owner is PlayerEntity)
        {
            // First own turn after being summoned: remove summoning sickness
            if (hasSummoningSickness)
                hasSummoningSickness = false;

            // Each new turn: reset action state
            hasActedThisTurn = false;
        }
    }

    private void HandleEnemyTurnStarted()
    {
        if (instance == null) return;

        // Only care about enemy-owned minions on enemy turn
        if (instance.Owner is EnemyEntity)
        {
            if (hasSummoningSickness)
                hasSummoningSickness = false;

            hasActedThisTurn = false;
        }
    }

    /// <summary>
    /// Old helper: auto-attack the first valid enemy hero.
    /// Now respects CanAct and delegates to AttackTarget.
    /// </summary>
    public void AttackEnemy()
    {
        var bm = BattleManager.Instance;
        if (bm == null || instance == null) return;

        if (!CanAct)
        {
            Debug.Log("[MinionBehaviour] Tried to auto-attack enemy but this minion cannot act yet (summoning sickness or already attacked).");
            return;
        }

        EnemyEntity target = null;

        // Prefer multi-enemy list if present
        if (bm.enemies != null && bm.enemies.Count > 0)
        {
            foreach (var e in bm.enemies)
            {
                if (e != null && e.currentHealth > 0)
                {
                    target = e;
                    break;
                }
            }
        }
        else if (bm.enemyEntity != null)
        {
            target = bm.enemyEntity;
        }

        if (target == null) return;

        AttackTarget(target);
    }

    /// <summary>
    /// New: explicit targeted attack (used by BoardManager click system).
    /// </summary>
    public void AttackTarget(EntityBase target)
    {
        if (instance == null || target == null) return;

        if (!CanAct)
        {
            Debug.Log("[MinionBehaviour] Tried to attack but this minion cannot act yet.");
            return;
        }

        int atkBase = instance.Attack;
        if (atkBase <= 0)
        {
            Debug.Log($"[MinionBehaviour] {instance.Data.cardName} has 0 attack and cannot deal damage.");
            hasActedThisTurn = true;
            return;
        }

        float statusMult = 1f;
        float rowMult = 1f;

        var minionEntity = GetComponent<MinionEntity>();
        if (minionEntity != null)
        {
            // existing Strength/Weakness on this minion
            statusMult = minionEntity.GetOutgoingDamageMultiplier();

            // NEW: row aura Strength/Weakness
            bool isPlayerRow = minionEntity.IsOwnedByPlayer;
            bool isRangedRow = slot != null && slot.isRanged;

            rowMult = RowEffectSystem.GetRowDamageMultiplier(isPlayerRow, isRangedRow);
        }

        int finalDamage = Mathf.RoundToInt(atkBase * statusMult * rowMult);
        if (finalDamage < 0) finalDamage = 0;

        // start your animation coroutine, but use finalDamage:
        StartCoroutine(AttackAnimationCoroutine(target, finalDamage));

        hasActedThisTurn = true;
    }


    private IEnumerator AttackAnimationCoroutine(EntityBase target, int damage)
    {
        // Get target position
        Vector3 targetPosition = GetEntityPosition(target);
        Vector3 startPosition = GetOriginalWorldPosition();
        Vector3 returnPosition = startPosition;

        // Move to target
        float elapsed = 0f;
        float halfDuration = attackAnimationDuration * 0.5f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            // Use world position for movement
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        // Apply damage at target
        target.TakeDamage(damage);
        string targetName = !string.IsNullOrEmpty(target.entityName) ? target.entityName : target.name;
        Debug.Log($"[MinionBehaviour] {instance.Data.cardName} attacked {targetName} for {damage}");

        // Return to original position
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.position = Vector3.Lerp(targetPosition, returnPosition, t);
            yield return null;
        }

        // Ensure we're back at original position
        transform.position = returnPosition;
    }

    private Vector3 GetEntityPosition(EntityBase entity)
    {
        if (entity == null) return transform.position;

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

        // For EnemyEntity/PlayerEntity: use the inspector-set Transform target
        if (entity.attackTargetTransform != null)
        {
            return entity.attackTargetTransform.position;
        }

        // Last resort: use a position offset from current position
        Vector3 offsetPos = transform.position;
        if (entity is EnemyEntity)
        {
            offsetPos += Vector3.forward * 3.0f;
        }
        else if (entity is PlayerEntity)
        {
            offsetPos += Vector3.back * 3.0f;
        }
        offsetPos.y += 1.0f;
        return offsetPos;
    }


    public void ReceiveDamage(int amount)
    {
        if (instance == null) return;

        instance.TakeDamage(amount);

        // Update health/damage display
        var card3D = GetComponent<Card3DController>();
        if (card3D != null)
        {
            card3D.UpdateStats();
        }

        // Play shake animation
        StartCoroutine(ShakeAnimationCoroutine());
        
        if (instance.IsDead())
            Die();
    }

    private IEnumerator ShakeAnimationCoroutine()
    {
        Vector3 startPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetZ = Random.Range(-shakeIntensity, shakeIntensity);
            
            transform.localPosition = startPosition + new Vector3(offsetX, offsetY, offsetZ);
            yield return null;
        }

        transform.localPosition = startPosition;
    }

    public void Die()
    {
        if (instance != null)
        {
            instance.ResolveMinionDeathEffects();
            Debug.Log($"{instance.Data.cardName} died.");
        }

        // Start death animation coroutine
        StartCoroutine(DeathAnimationCoroutine());
    }

    private IEnumerator DeathAnimationCoroutine()
    {
        // Scale down animation
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Ease-in curve for smooth animation
            t = t * t;
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Ensure scale is zero
        transform.localScale = targetScale;

        // Now destroy the minion
        if (slot != null)
            slot.ClearSlotAndDestroy();
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Called when minion is selected - scales it up
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (isSelected == selected) return;
        isSelected = selected;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = StartCoroutine(SelectionAnimationCoroutine(selected));
    }

    private IEnumerator SelectionAnimationCoroutine(bool selected)
    {
        Vector3 targetScale = selected ? originalScale * selectionScaleMultiplier : originalScale;
        Vector3 currentScale = transform.localScale;

        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * selectionAnimationSpeed);
            yield return null;
        }

        transform.localScale = targetScale;
        currentAnimation = null;
    }
}
