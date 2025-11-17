using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Definition", fileName = "NewEnemyDefinition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Enemy Stats")]
    [SerializeField] private string enemyName = "Enemy";
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int startingMana = 1;

    [Header("Deck")]
    [SerializeField] private DeckDefinition deckDefinition;

    public string EnemyName => enemyName;
    public int MaxHealth => maxHealth;
    public int StartingMana => startingMana;
    public DeckDefinition DeckDefinition => deckDefinition;

    public DeckInstance CreateDeckInstance()
    {
        if (deckDefinition == null)
            return new DeckInstance();

        return new DeckInstance(deckDefinition.CardIds);
    }
}

