using System.Collections;
using UnityEngine;
using TMPro;

public class TurnBanner : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject background;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private GameObject battleStartText;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private static readonly int ShowBanner = Animator.StringToHash("ShowBanner");

    private bool isShowing = false;

    private void Awake()
    {
        // Ensure everything starts hidden
        if (background) background.SetActive(false);
        if (turnText) turnText.gameObject.SetActive(false);
        if (battleStartText) battleStartText.SetActive(false);
    }

    public void ShowIntroBanner()
    {
        StartCoroutine(ShowBannerRoutine("intro"));
    }

    public void ShowPlayerTurnBanner()
    {
        StartCoroutine(ShowBannerRoutine("player"));
    }

    public void ShowEnemyTurnBanner()
    {
        StartCoroutine(ShowBannerRoutine("enemy"));
    }

    public IEnumerator ShowIntroBannerEnumerator()
    {
        yield return ShowBannerRoutine("intro");
    }

    public IEnumerator ShowPlayerTurnBannerEnumerator()
    {
        yield return ShowBannerRoutine("player");
    }

    public IEnumerator ShowEnemyTurnBannerEnumerator()
    {
        yield return ShowBannerRoutine("enemy");
    }

    public void ShowPersistentEndBanner(string message)
    {
        if (isShowing) return;
        isShowing = true;
        Time.timeScale = 0f; // pause gameplay while showing banner

        if (background) background.SetActive(true);
        animator.ResetTrigger("Show");
        animator.SetTrigger("Show");

        if (turnText)
        {
            turnText.gameObject.SetActive(true);
            turnText.text = message;
        }
        if (battleStartText) battleStartText.SetActive(false);
    }

    public void HideBanner()
    {
        if (!isShowing) return;
        if (background) background.SetActive(false);
        if (turnText) turnText.gameObject.SetActive(false);
        if (battleStartText) battleStartText.SetActive(false);
        Time.timeScale = 1f; // resume gameplay
        isShowing = false;
    }

    public IEnumerator ShowEndBannerEnumerator(string message, float duration = 1.6f)
    {
        Debug.Log($"[TurnBanner] Showing end banner: {message}");
        ShowPersistentEndBanner(message);
        yield return new WaitForSecondsRealtime(duration);
        HideBanner();
    }


    private IEnumerator ShowBannerRoutine(string type)
    {
        Debug.Log($"[TurnBanner] Showing banner type: {type}");
        if (isShowing)
            yield break;

        isShowing = true;
        Time.timeScale = 0f; // pause gameplay while showing banner

        // Show background and play animation
        if (background) background.SetActive(true);
        animator.ResetTrigger("Show");
        animator.SetTrigger("Show");

        switch (type)
        {
            case "intro":
                if (battleStartText)
                {
                    Debug.Log("[TurnBanner] Battle start active");
                    battleStartText.SetActive(true);
                    turnText.gameObject.SetActive(false);
                }
                break;

            case "player":
                if (turnText)
                {
                    Debug.Log("[TurnBanner] Player turn active");
                    turnText.gameObject.SetActive(true);
                    turnText.text = "Player Turn";
                    battleStartText.SetActive(false);
                }
                break;

            case "enemy":
                if (turnText)
                {
                    Debug.Log("[TurnBanner] Enemy turn active");
                    turnText.gameObject.SetActive(true);
                    turnText.text = "Enemy Turn";
                    battleStartText.SetActive(false);
                }
                break;
        }

        yield return new WaitForSecondsRealtime(1.6f);

        // Hide everything
        if (background) background.SetActive(false);
        if (turnText) turnText.gameObject.SetActive(false);
        if (battleStartText) battleStartText.SetActive(false);

        Time.timeScale = 1f; // resume gameplay
        isShowing = false;
    }
}
