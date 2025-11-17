[System.Serializable]
public class CardJSON
{
    public string id;
    public string cardName;
    public string description;
    public int cost;
    public string type;
    public bool isRanged;
    public string effectId;
    public int damage;
    public string spriteName; // Optional: if not set, will use CardId name
}