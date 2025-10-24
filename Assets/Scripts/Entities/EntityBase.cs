using UnityEngine;

public abstract class EntityBase : MonoBehaviour
{
    public string entityName;
    public int maxHealth;
    public int currentHealth;

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= Mathf.Max(0, amount);
        Debug.Log($"{name} took {amount} damage. HP = {currentHealth}");
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        Debug.Log($"[EntityBase] {entityName} has been destroyed.");
    }
}
