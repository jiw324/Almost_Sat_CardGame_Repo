using UnityEngine;

public class MinionEntity : EntityBase
{
    [SerializeField] private MinionBehaviour minion;

    public void Initialize(MinionBehaviour mb)
    {
        minion = mb;
        if (minion == null || minion.instance == null || minion.instance.Data == null)
            return;

        entityName = minion.instance.Data.cardName;
        maxHealth = minion.instance.Data.minionHealth;
        currentHealth = minion.instance.CurrentHP;
    }

    /// <summary>
    /// True if this minion is controlled by the player.
    /// </summary>
    public bool IsOwnedByPlayer
    {
        get
        {
            if (minion == null || minion.instance == null) return false;
            return minion.instance.Owner is PlayerEntity;
        }
    }


    public override void TakeDamage(int amount)
    {
        if (minion == null || minion.instance == null) return;
        amount = Mathf.Max(0, amount);
        minion.ReceiveDamage(amount);
        currentHealth = minion.instance.CurrentHP; // mirror runtime HP
        if (currentHealth <= 0) Die();
    }

    public override void Die()
    {
        // MinionBehaviour handles slot cleanup + death effects
        if (minion != null)
        {
            minion.Die();
        }
        else
        {
            base.Die();
        }
    }
}
