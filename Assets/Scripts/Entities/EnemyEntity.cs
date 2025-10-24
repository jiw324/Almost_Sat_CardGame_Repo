using UnityEngine;

public class EnemyEntity : EntityBase
{
    private void Start()
    {
        entityName = "Test Enemy";
        maxHealth = 20;
        currentHealth = maxHealth;
    }

    public override void Die()
    {
        base.Die();
        if (BattleManager.Instance != null)
            BattleManager.Instance.RemoveDeadEnemy(this);
    }
}
