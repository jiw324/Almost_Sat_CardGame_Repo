using UnityEngine;

public class EnemyEntity : EntityBase
{
    public int mana = 1;
    public int maxMana = 1;
    private DeckInstance deckInstance;

    public void Initialize(EnemyDefinition enemyDef)
    {
        if (enemyDef == null)
        {
            Debug.LogWarning("[EnemyEntity] No enemy definition provided. Using defaults.");
            entityName = "Test Enemy";
            maxHealth = 20;
            currentHealth = maxHealth;
            mana = 1;
            maxMana = 1;
            deckInstance = new DeckInstance();
            return;
        }

        entityName = enemyDef.EnemyName;
        maxHealth = enemyDef.MaxHealth;
        currentHealth = maxHealth;
        mana = enemyDef.StartingMana;
        maxMana = enemyDef.StartingMana;
        deckInstance = enemyDef.CreateDeckInstance();

        Debug.Log($"[EnemyEntity] Initialized {entityName} with {maxHealth} HP, {mana} mana, and {deckInstance.Cards.Count} cards in deck.");
    }

    public DeckInstance GetDeck()
    {
        return deckInstance;
    }

    public void SetMana(int newMana)
    {
        mana = Mathf.Clamp(newMana, 0, maxMana);
    }

    public void AddMana(int amount)
    {
        SetMana(mana + amount);
    }
}
