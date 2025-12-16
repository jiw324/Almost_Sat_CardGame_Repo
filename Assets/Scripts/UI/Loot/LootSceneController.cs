using UnityEngine;

public class LootSceneController : MonoBehaviour
{
    [Header("Gold Range")]
    public int minGold = 5;
    public int maxGold = 15;

    [Header("Mana Reward")]
    [Range(0f, 1f)]
    public float manaChance = 1f;

    [Header("References")]
    public GameObject speechBubbleObject;
    private ModifiableTypewriterEffect typewriter;

    private int rolledGold;
    private bool grantsMana;

    private void Start()
    {
        typewriter = speechBubbleObject.GetComponent<ModifiableTypewriterEffect>();

        // Roll gold at scene start
        rolledGold = Random.Range(minGold, maxGold + 1);

        // Roll for mana reward
        grantsMana = Random.value < manaChance;

        // Build message
        string msg = $"Chest Found!\n\n\n\n\n\n+{rolledGold} Gold";
        if (grantsMana)
        {
            msg += "\n+1 Max Mana";
        }

        // Send to typewriter
        typewriter.SetMessage(msg);

        // Start text animation
        speechBubbleObject.SetActive(true);
        SoundEvents.Play("LootFind");
    }

    // Called by Continue button
    public void AcceptReward()
    {
        GameSession session = SessionGrabber.getGameSession();
        if (session != null)
        {
            session.SetPlayerGold(session.GetPlayerGold() + rolledGold);

            if (grantsMana)
            {
                session.SetPlayerMana(session.GetPlayerMana() + 1);
            }
        }

        var msm = FindFirstObjectByType<MapStateManager>();
        if (msm != null)
        {
            msm.MarkCompleted(msm.GetCurrentNode());
            msm.ReturnToMapScene();
        }
    }
}