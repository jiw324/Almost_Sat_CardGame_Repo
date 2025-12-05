using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SessionPlayerData
{
    public int maxHealth = 30;
    public int health = 10;
    public int mana = 1;
    public int gold = 10;
    public DeckInstance deck = new DeckInstance();

    // Relics currently owned by the player during this run.
    // These are serialized with the rest of the session.
    public List<RelicData> relics = new List<RelicData>();

    public void ResetSessionData(DeckDefinition defaultDeck = null)
    {
        maxHealth = 30;
        health = 10;
        mana = 1;
        gold = 10;
        if (deck == null)
            deck = new DeckInstance();
        else
            deck.Clear();

        // Clear relics at the start of a new run and give a default one if available.
        if (relics == null)
            relics = new List<RelicData>();
        else
            relics.Clear();

        // Try to grant a default starter relic created via RelicAssetCreator.
        RelicData starter = Resources.Load<RelicData>("Relics/StarterRelic");
        if (starter != null)
        {
            relics.Add(starter);
        }

        if (defaultDeck != null)
            deck = new DeckInstance(defaultDeck.CardIds);
    }
}
