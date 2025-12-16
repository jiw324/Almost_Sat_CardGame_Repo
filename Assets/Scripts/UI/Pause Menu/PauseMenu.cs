using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject exitPanel;
    [SerializeField] private GameObject screenDim;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [SerializeField] private InputActionReference pauseAction;

    private static readonly int OpenPause = Animator.StringToHash("OpenPause");
    private static readonly int ClosePause = Animator.StringToHash("ClosePause");
    private static readonly int FlipAndExpand = Animator.StringToHash("FlipAndExpand");
    private static readonly int BackToIdle = Animator.StringToHash("BackToIdle");

    [Header("Audio Sliders")]
    [SerializeField] private Slider uiSlider;
    [SerializeField] private Slider mapSlider;
    [SerializeField] private Slider combatSlider;
    [SerializeField] private Slider musicSlider;

    private bool isOpen = false;

    private void Awake()
    {
        // Start hidden
        pausePanel.SetActive(false);
        buttonsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        exitPanel.SetActive(false);
        screenDim.SetActive(false);

        Debug.Log("PauseMenu Awake on " + gameObject.scene.name);
    }

    void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed += OnPausePressed;
            pauseAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPausePressed;
        }
    }

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        Debug.Log("Pause input received");
        if (!isOpen)
        {
            OpenMenu();
        }
        else
        {
            // If we're in options or exit, go back to main buttons instead of closing
            if (optionsPanel.activeSelf || exitPanel.activeSelf)
            {
                BackToMainButtons();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    public void OpenMenu()
    {
        pausePanel.SetActive(true);
        screenDim.SetActive(true);

        // Play the one-time intro animation
        animator.ResetTrigger(ClosePause);
        animator.SetTrigger(OpenPause);

        StartCoroutine(PanelAfterDelay(0.3f, buttonsPanel, true));

        Time.timeScale = 0f;
        isOpen = true;
    }

    public void ResumeGame()
    {
        animator.SetTrigger(ClosePause);
        StartCoroutine(PanelAfterDelay(0.3f, buttonsPanel, false));
        StartCoroutine(HideAfterDelay(0.5f));

        Time.timeScale = 1f;
        isOpen = false;
    }

    public void OpenOptions()
    {
        animator.SetTrigger(FlipAndExpand);
        StartCoroutine(PanelAfterDelay(0.3f, buttonsPanel, false));
        StartCoroutine(PanelAfterDelay(0.3f, optionsPanel, true));
        //optionsPanel.SetActive(true);
        SyncAudioSliders();
    }

    private void SyncAudioSliders()
    {
        if (AudioManager.Instance == null)
            return;

        uiSlider.SetValueWithoutNotify(AudioManager.Instance.uiVolume);
        mapSlider.SetValueWithoutNotify(AudioManager.Instance.mapVolume);
        combatSlider.SetValueWithoutNotify(AudioManager.Instance.combatVolume);
        musicSlider.SetValueWithoutNotify(AudioManager.Instance.musicVolume);
    }

    public void OpenExit()
    {
        animator.SetTrigger(FlipAndExpand);
        StartCoroutine(PanelAfterDelay(0.3f, buttonsPanel, false));
        StartCoroutine(PanelAfterDelay(0.3f, exitPanel, true));
    }

    public void BackToMainButtons()
    {
        animator.SetTrigger(BackToIdle);
        StartCoroutine(PanelAfterDelay(0.15f, optionsPanel, false));
        StartCoroutine(PanelAfterDelay(0.15f, exitPanel, false));
        StartCoroutine(PanelAfterDelay(0.2f, buttonsPanel, true));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        pausePanel.SetActive(false);
        buttonsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        exitPanel.SetActive(false);
        screenDim.SetActive(false);
    }

    private IEnumerator PanelAfterDelay(float delay, GameObject panel, bool show)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (panel != null)
            panel.SetActive(show);
    }

    public void OnSaveGamePressed()
    {
        if (GameSession.Instance == null) return;

        GameSession.Instance.CaptureAudioSettings();
        SessionSaveManager.SaveGameSession(GameSession.Instance.gameSessionData);
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

    public void TriggerUIButtonSound()
    {
        AudioManager.Instance.PlaySoundById("UIButton");
    }

}
