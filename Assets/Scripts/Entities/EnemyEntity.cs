using UnityEngine;

public class EnemyEntity : EntityBase
{
    private void Awake()
    {
        entityName = "Enemy";
    }

    public override void Die()
    {
        base.Die();
        if (BattleManager.Instance != null)
            BattleManager.Instance.RemoveDeadEnemy(this);
    }
}
