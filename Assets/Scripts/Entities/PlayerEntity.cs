using UnityEngine;

public class PlayerEntity : EntityBase
{
    public int mana = 3;

    public int Shield { get; private set; }

    public void AddShield(int value)
    {
        Shield += Mathf.Max(0, value);
        Debug.Log($"Player gained {value} shield. Total Shield = {Shield}");
    }

    public override void TakeDamage(int value)
    {
        var dmg = Mathf.Max(0, value);
        if (Shield > 0)
        {
            int use = Mathf.Min(Shield, dmg);
            Shield -= use;
            dmg -= use;
        }
        base.TakeDamage(dmg);
    }
    private void Start()
    {
        entityName = "Test Player";
        maxHealth = 30;
        currentHealth = maxHealth;
    }
}
