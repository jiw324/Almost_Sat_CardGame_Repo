using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndTurnButton : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    private Button button;
    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnPressed);

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        turnManager.OnStateChanged += HandleStateChange;
    }

    void OnDisable()
    {
        turnManager.OnStateChanged -= HandleStateChange;
    }

    private void OnPressed()
    {

        turnManager.EndCurrentTurn();
    }

    private void HandleStateChange(TurnStateBase newState)
    {
        if (newState == turnManager.PlayerTurnState)
            FadeIn();
        else
            FadeOut();
    }

    private void FadeIn()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(1f, true));
    }

    private void FadeOut()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(0f, false));
    }

    private IEnumerator FadeRoutine(float targetAlpha, bool interactable)
    {
        float duration = 0.5f;
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        if (!interactable)
            button.interactable = false;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // Enable interaction only when fully visible
        button.interactable = interactable;

        fadeRoutine = null;
    }
}
