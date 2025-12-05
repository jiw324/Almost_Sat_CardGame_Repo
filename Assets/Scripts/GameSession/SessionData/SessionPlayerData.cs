using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SessionPlayerData
{
    public int maxHealth = 30;
    public int health = 10;
    public int mana = 3;
    public int gold = 10;
    public DeckInstance deck = new DeckInstance();
    public RelicInventory relicInventory = new RelicInventory();
    public List<RelicData> relics = new List<RelicData>();
    public bool isInActiveRun = false;

    public void ResetSessionData(DeckDefinition defaultDeck = null)
    {
        maxHealth = 30;
        health = 30;
        mana = 3;
        gold = 10;
        isInActiveRun = false;
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
