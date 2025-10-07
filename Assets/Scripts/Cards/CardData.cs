using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int cost;
    [TextArea] public string description;
    public Sprite artwork;
    public bool isRanged; 
    public CardEffect effect; 
}
