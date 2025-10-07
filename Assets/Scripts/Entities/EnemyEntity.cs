using UnityEngine;

public class EnemyEntity : EntityBase
{
    private void Start()
    {
        entityName = "Test Enemy";
        maxHealth = 20;
        currentHealth = maxHealth;
    }
}
