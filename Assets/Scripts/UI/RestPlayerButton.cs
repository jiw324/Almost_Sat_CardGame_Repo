using UnityEngine;

public class RestPlayerButton : MonoBehaviour
{
    public void OnRestButtonClicked()
    {
        GameSession session = SessionGrabber.getGameSession();
        if (session != null)
        {
            Debug.Log("Player current health: " + session.GetPlayerHealth());
            session.SetPlayerHealth(session.GetPlayerMaxHealth());
            Debug.Log("Player healed to full health: " + session.GetPlayerHealth());
        }
        else
        {
            Debug.LogWarning("No active GameSession found.");
        }
    }
}
