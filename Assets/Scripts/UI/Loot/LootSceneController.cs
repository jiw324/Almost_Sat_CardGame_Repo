using UnityEngine;

public class LootSceneController : MonoBehaviour
{
    [Header("Gold Range")]
    public int minGold = 5;
    public int maxGold = 15;

    [Header("References")]
    public GameObject speechBubbleObject;
    private ModifiableTypewriterEffect typewriter;

    private int rolledGold; // <- stored gold for later

    private void Start()
    {
        typewriter = speechBubbleObject.GetComponent<ModifiableTypewriterEffect>();

        // Roll gold at scene start
        rolledGold = Random.Range(minGold, maxGold + 1);

        // Build message
        string msg = $"Chest Found!\n\n\n\n\n\n+{rolledGold} Gold";

        // Send to typewriter
        typewriter.SetMessage(msg);

        // Start text animation
        speechBubbleObject.SetActive(true);
    }

    // Called by Continue button
    public void AcceptReward()
    {
        GameSession session = SessionGrabber.getGameSession();
        if (session != null)
        {
            session.SetPlayerGold(session.GetPlayerGold() + rolledGold);
        }
    }
}
