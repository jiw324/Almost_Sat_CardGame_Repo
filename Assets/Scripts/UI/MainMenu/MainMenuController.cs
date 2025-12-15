using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button startNewRunButton;
    [SerializeField] private Button continueRunButton;

    [Header("Audio Sliders")]
    [SerializeField] private Slider uiVolumeSlider;
    [SerializeField] private Slider mapVolumeSlider;
    [SerializeField] private Slider combatVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    [Header("Scene Management")]
    [SerializeField] private SceneSwitch sceneManager;

    private void Start()
    {
        UpdateButtonStates();

        if (GameSession.Instance != null)
            GameSession.Instance.ApplyAudioSettings();

        SyncAudioSliders();
    }

    private void UpdateButtonStates()
    {
        GameSession session = SessionGrabber.getGameSession();

        if (session == null ||
            session.gameSessionData == null ||
            session.gameSessionData.sessionPlayerData == null)
        {
            if (continueRunButton != null)
                continueRunButton.gameObject.SetActive(false);
            return;
        }

        if (continueRunButton != null)
            continueRunButton.gameObject.SetActive(session.IsInActiveRun());
    }

    /// <summary>
    /// Start a new run.
    /// Player is always sent to the deck builder first.
    /// </summary>
    public void OnStartNewRun()
    {
        GameSession session = SessionGrabber.getGameSession();

        if (session == null)
        {
            Debug.LogError("GameSession not found!");
            return;
        }

        Debug.Log("Starting new run - going to deck builder");
        GameSession.Instance.IsTutorialMode = false;
        sceneManager.SceneChanger("DeckBuilder");
    }

    /// <summary>
    /// Continue an existing run.
    /// Skips deck builder and resumes on the map.
    /// </summary>
    public void OnContinueRun()
    {
        GameSession session = SessionGrabber.getGameSession();

        if (session == null || !session.IsInActiveRun())
        {
            Debug.LogWarning("No active run to continue");
            return;
        }

        Debug.Log("Continuing existing run");
        session.LoadGameSession("Save");
        sceneManager.SceneChanger("Map");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }

    public void OnSaveGamePressed()
    {
        if (GameSession.Instance == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.SetUIVolume(uiVolumeSlider.value);
        AudioManager.Instance.SetMapVolume(mapVolumeSlider.value);
        AudioManager.Instance.SetCombatVolume(combatVolumeSlider.value);
        AudioManager.Instance.SetMusicVolume(musicVolumeSlider.value);

        GameSession.Instance.gameSessionData.audioSettings =
            AudioManager.Instance.CaptureSettings();

        SessionSaveManager.SaveGameSession(GameSession.Instance.gameSessionData);
        Debug.Log("Game session saved successfully.");
    }



    public void OnUIVolumeChanged(float value)
    {
        AudioManager.Instance?.SetUIVolume(value);
    }

    public void OnMapVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMapVolume(value);
    }

    public void OnCombatVolumeChanged(float value)
    {
        AudioManager.Instance?.SetCombatVolume(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }

    private void SyncAudioSliders()
    {
        if (AudioManager.Instance == null)
            return;

        if (uiVolumeSlider != null)
            uiVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.uiVolume);

        if (mapVolumeSlider != null)
            mapVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.mapVolume);

        if (combatVolumeSlider != null)
            combatVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.combatVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.musicVolume);
    }
}
