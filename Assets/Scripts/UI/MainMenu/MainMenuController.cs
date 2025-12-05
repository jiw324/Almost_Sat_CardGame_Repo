using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button startNewRunButton;
    [SerializeField] private Button continueRunButton;
    [SerializeField] private SceneSwitch sceneManager;

    private void Start()
    {
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        GameSession session = SessionGrabber.getGameSession();

        if (session == null || session.gameSessionData == null || session.gameSessionData.sessionPlayerData == null)
        {
            // No session yet, hide continue button
            if (continueRunButton != null)
                continueRunButton.gameObject.SetActive(false);
            return;
        }

        // Show/hide continue button based on active run
        if (continueRunButton != null)
        {
            continueRunButton.gameObject.SetActive(session.IsInActiveRun());
        }
    }

    /// <summary>
    /// Call this from "Start New Run" button
    /// Player ALWAYS goes to deck builder to build/modify their deck
    /// </summary>
    public void OnStartNewRun()
    {
        GameSession session = SessionGrabber.getGameSession();

        if (session == null)
        {
            Debug.LogError("GameSession not found!");
            return;
        }

        // Always go to deck builder for new runs
        // If they had a previous deck, it will be loaded so they can modify it
        // If first time, deck will be empty and pool will have all cards
        Debug.Log("Starting new run - going to deck builder");
        sceneManager.SceneChanger("DeckBuilder");
    }

    /// <summary>
    /// Call this from "Continue Run" button
    /// Skip deck builder - player is resuming mid-run
    /// </summary>
    public void OnContinueRun()
    {
        GameSession session = SessionGrabber.getGameSession();

        if (session == null || !session.IsInActiveRun())
        {
            Debug.LogWarning("No active run to continue");
            return;
        }

        // Resume the run - skip deck builder entirely
        Debug.Log("Continuing existing run - skipping deck builder");
        session.LoadGameSession("Save");
        sceneManager.SceneChanger("Map");
    }
}