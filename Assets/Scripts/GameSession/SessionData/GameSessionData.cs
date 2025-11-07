using System.Collections.Generic;

[System.Serializable]
public class GameSessionData
{
    public SessionPlayerData sessionPlayerData;
    public SessionNodeMapData sessionNodeMapData;

    public void ResetSessionData(DeckDefinition defaultDeck = null)
    {
        sessionPlayerData.ResetSessionData(defaultDeck);
        sessionNodeMapData.ResetSessionData();
    }
}