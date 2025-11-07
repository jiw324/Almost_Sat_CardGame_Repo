using UnityEngine;

public class PlayerEntity : EntityBase
{
    public int mana = 3;

    private void Start()
    {
        entityName = "Test Player";
        maxHealth = 30;
        currentHealth = maxHealth;
    }
}
