using System.Collections.Generic;

[System.Serializable]
public class GameSessionData
{
    public SessionPlayerData sessionPlayerData;
    public SessionNodeMapData sessionNodeMapData;
    public SessionNodeMapData tutorialNodeMapData;
    public SessionAudioData audioSettings;

    public void ResetSessionData(DeckDefinition defaultDeck = null)
    {
        if (sessionPlayerData == null)
            sessionPlayerData = new SessionPlayerData();
        sessionPlayerData.ResetSessionData(defaultDeck);

        if (sessionNodeMapData == null)
            sessionNodeMapData = new SessionNodeMapData();
        sessionNodeMapData.ResetSessionData();

        if (tutorialNodeMapData == null)
            tutorialNodeMapData = new SessionNodeMapData();
        tutorialNodeMapData.ResetSessionData();

        if (audioSettings == null)
            audioSettings = new SessionAudioData();
    }
}
