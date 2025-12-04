using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

[CreateAssetMenu(menuName = "Cards/Effects/Runic Blast")]
public class RunicBlastEffect : CardEffect
{
    [SerializeField] private int wordsPerRune = 3;
    public override void Execute(EntityBase caster, EntityBase target, int value)
    {
        // This effect needs to be async, so we'll start a coroutine
        // We need a MonoBehaviour to run the coroutine - use BattleManager or create a helper
        var bm = BattleManager.Instance;
        if (bm == null)
        {
            Debug.LogWarning("[RunicBlastEffect] No BattleManager found.");
            return;
        }

        // Only players can use runic blast
        if (!(caster is PlayerEntity))
        {
            Debug.LogWarning("[RunicBlastEffect] Only players can use Runic Blast.");
            return;
        }

        // Count runes on the board
        int runeCount = bm.CountPlayerRunes();
        
        if (runeCount == 0)
        {
            Debug.Log("[RunicBlastEffect] No runes on board. Runic Blast has no effect.");
            return;
        }

        Debug.Log($"[RunicBlastEffect] Found {runeCount} runes on board. Starting typing minigame...");

        // Start the coroutine to handle async minigame
        bm.StartCoroutine(PlayRunicBlastCoroutine(bm, runeCount, target, value));
    }

    private IEnumerator PlayRunicBlastCoroutine(BattleManager bm, int runeCount, EntityBase target, int baseValue)
    {
        // Get typing minigame prefab from registry
        if (MinigameRegistry.Instance == null || MinigameManager.Instance == null)
        {
            Debug.LogWarning("[RunicBlastEffect] MinigameRegistry or MinigameManager not found.");
            yield break;
        }

        GameObject typingPrefab = MinigameRegistry.Instance.GetMinigamePrefab("typing");
        if (typingPrefab == null)
        {
            Debug.LogWarning("[RunicBlastEffect] Typing minigame prefab not found in registry.");
            yield break;
        }

        // Play minigame with word count based on runes
        float minigameResult = 0f;
        bool taskCompleted = false;

        var task = MinigameManager.Instance.StartMinigameAsync(typingPrefab, (instance) => {
            // Configure the typing minigame with word count based on runes
            var typingMinigame = instance.GetComponent<TypingMinigame>();
            if (typingMinigame != null)
            {
                typingMinigame.SetWordCount(runeCount * wordsPerRune);
                Debug.Log($"[RunicBlastEffect] Configured typing minigame with {runeCount * wordsPerRune} words (based on {runeCount} runes).");
            }
        });
        
        // Wait for the async task to complete
        task.ContinueWith(t => {
            if (t.IsCompletedSuccessfully)
            {
                minigameResult = t.Result;
            }
            taskCompleted = true;
        });

        while (!taskCompleted)
        {
            yield return null;
        }

        // Calculate damage: base damage per rune (from baseValue) × number of runes × minigame result (0-2 range)
        // baseValue is the effectValue from the card JSON (base damage per rune)
        int baseDamagePerRune = Mathf.Max(1, baseValue); // Ensure at least 1 damage per rune
        int baseDamage = baseDamagePerRune * runeCount;
        int finalDamage = Mathf.RoundToInt(baseDamage * minigameResult);

        Debug.Log($"[RunicBlastEffect] Minigame result: {minigameResult:F2}, Base damage: {baseDamage}, Final damage: {finalDamage}");

        // Apply damage to target
        if (finalDamage > 0 && target != null)
        {
            target.TakeDamage(finalDamage);
            if (bm.uiManager != null && target is EnemyEntity)
            {
                bm.uiManager.UpdateEnemyHealth(bm.enemyHealth);
            }
        }
        else if (finalDamage > 0)
        {
            // If no target specified, target enemy hero
            EnemyEntity enemyTarget = null;
            if (bm.enemies != null && bm.enemies.Count > 0)
            {
                foreach (var e in bm.enemies)
                {
                    if (e != null && e.currentHealth > 0) { enemyTarget = e; break; }
                }
            }
            if (enemyTarget == null && bm.enemyEntity != null) enemyTarget = bm.enemyEntity;
            
            if (enemyTarget != null)
            {
                enemyTarget.TakeDamage(finalDamage);
                if (bm.uiManager != null) bm.uiManager.UpdateEnemyHealth(bm.enemyHealth);
            }
        }
    }
}

