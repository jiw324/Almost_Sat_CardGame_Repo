using System.Collections.Generic;

[System.Serializable]
public class GameSessionData
{
    public SessionPlayerData sessionPlayerData;
    public SessionNodeMapData sessionNodeMapData;

    public void ResetSessionData()
    {
        sessionPlayerData.ResetSessionData();
        sessionNodeMapData.ResetSessionData();
    }
}