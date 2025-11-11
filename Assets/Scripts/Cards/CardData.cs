using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card")]
public class CardData : ScriptableObject
{
    public CardId id;
    public string cardName;
    public int cost;
    [TextArea] public string description;
    public string type;
    public Sprite artwork;
    public bool isRanged; 
    public CardEffect effect;
    public int damage;

    public string PrintCard()
    {
        return $"id: [{id}], name: [{cardName}], cost: [{cost}], type: [{type}], damage: [{damage}], ranged: [{isRanged}], effect: [{effect}]\ndescritpion: [{description}]";
    }
}
