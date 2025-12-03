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
    public int effectValue;      

    public CardJSONEffectEntry[] effects;

    public bool isMinion;       
    public int minionAttack;    
    public int minionHealth;    
    public CardJSONEffectEntry[] onSummon;  
    public CardJSONEffectEntry[] onDeath;   
    
    // Minigame support
    public string minigameId;   // Optional: ID of the minigame to play when this card is played
}

[System.Serializable]
public class CardJSONEffectEntry
{
    public string effectId;
    public int effectValue;
}
