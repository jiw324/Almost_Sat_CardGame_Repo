using UnityEngine;

public class MinionEntity : EntityBase
{
    [SerializeField] private MinionBehaviour minion;

    public void Initialize(MinionBehaviour mb)
    {
        minion = mb;
        entityName = mb.instance.Data.cardName;
        maxHealth = mb.instance.Data.minionHealth;
        currentHealth = mb.instance.CurrentHP;
    }

    public override void TakeDamage(int amount)
    {
        if (minion == null || minion.instance == null) return;
        minion.ReceiveDamage(amount);
        currentHealth = minion.instance.CurrentHP; // mirror runtime HP
        if (currentHealth <= 0) Die();
    }

    public override void Die()
    {
        // MinionBehaviour handles slot cleanup + death effects
        if (minion != null) minion.Die();
    }
}
