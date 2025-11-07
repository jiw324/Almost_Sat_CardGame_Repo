using System.Collections.Generic;

[System.Serializable]
public class SessionPlayerData
{
    public int health = 10;
    public int mana = 1;
    public int gold = 5;
    public List<CardInstance> cards = new List<CardInstance>();

    public void ResetSessionData()
    {
        health = 10;
        mana = 1;
        gold = 5;
        cards = new List<CardInstance>();
    }
}
