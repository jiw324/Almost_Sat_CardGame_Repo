using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int cost;
    [TextArea] public string description;
    public Sprite artwork;
<<<<<<< Updated upstream:Assets/Scripts/CardData.cs

    public CardEffectSO[] effects;
=======
    public bool isRanged; 
    public CardEffect effect;

    [Header("Minion (leave off for spells)")]
    public bool isMinion = false;
    public int minionAttack = 2;
    public int minionHealth = 5;
>>>>>>> Stashed changes:Assets/Scripts/Cards/CardData.cs
}
