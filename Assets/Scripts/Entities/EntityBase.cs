using UnityEngine;

public abstract class EntityBase : MonoBehaviour
{
    public string entityName;
    public int maxHealth;
    public int currentHealth;

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"[{entityName}] HP: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        Debug.Log($"[EntityBase] {entityName} has been destroyed.");
    }
}
