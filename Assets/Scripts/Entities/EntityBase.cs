using UnityEngine;

public abstract class EntityBase : MonoBehaviour
{
    public string entityName;
    public int currentHealth;

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= Mathf.Max(0, amount);
        Debug.Log($"{name} took {amount} damage. HP = {currentHealth}");

        if (BattleManager.Instance != null)
        {
            var bm = BattleManager.Instance;
            if (this is PlayerEntity)
            {
                bm.playerHealth = currentHealth;
                if (bm.uiManager != null) bm.uiManager.UpdatePlayerHealth(currentHealth);
            }
            else if (this is EnemyEntity)
            {
                bm.enemyHealth = currentHealth;
                if (bm.uiManager != null) bm.uiManager.UpdateEnemyHealth(currentHealth);
            }
        }

        if (currentHealth <= 0) Die();
    }

    public virtual void Die()
    {
        Debug.Log($"[EntityBase] {entityName} has been destroyed.");
    }
}
