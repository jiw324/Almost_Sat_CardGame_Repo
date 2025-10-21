using UnityEngine;

public static class SessionGrabber
{
    public static GameSession getGameSession()
    {
        GameObject gameSession = GameObject.FindWithTag("GameSession");
        GameSession session = gameSession.GetComponent<GameSession>();
        Debug.Log(session.gameSessionData.sessionPlayerData.health);
        return session;
    }
}
