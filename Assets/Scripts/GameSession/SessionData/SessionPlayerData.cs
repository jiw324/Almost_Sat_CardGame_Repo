[System.Serializable]
public class SessionPlayerData
{
    public int health = 10;
    public int mana = 1;
    public int gold = 5;
    public DeckInstance deck = new DeckInstance();

    public void ResetSessionData(DeckDefinition defaultDeck = null)
    {
        health = 10;
        mana = 1;
        gold = 5;
        if (deck == null)
            deck = new DeckInstance();
        else
            deck.Clear();

        if (defaultDeck != null)
            deck = new DeckInstance(defaultDeck.CardIds);
    }
}
