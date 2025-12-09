using UnityEngine;

public class RelicSceneController : MonoBehaviour
{
    [Header("References")]
    public GameObject speechBubbleObject;
    private ModifiableTypewriterEffect typewriter;

    private void Start()
    {
        typewriter = speechBubbleObject.GetComponent<ModifiableTypewriterEffect>();

        string msg = BuildMessage();

        // Send to typewriter
        typewriter.SetMessage(msg);

        // Start text animation
        speechBubbleObject.SetActive(true);
        SoundEvents.Play("LootFind");
    }

    // Called by Continue button
    public void AcceptReward()
    {
        var msm = FindFirstObjectByType<MapStateManager>();
        if (msm != null)
        {
            msm.MarkCompleted(msm.GetCurrentNode());
            msm.ReturnToMapScene();
        }
    }

    private string BuildMessage()
    {
        GameSession session = SessionGrabber.getGameSession();
        if (session == null || session.gameSessionData == null)
            return "You got a relic!";

        var mapData = session.gameSessionData.sessionNodeMapData;
        if (mapData == null || mapData.currentNodeData == null)
            return "You got a relic!";

        if (mapData.currentNodeData is RelicNodeData lootData && lootData.fromEvent)
        {
            return "You gain an relic";
        }

        return "You got a relic";
    }
}
