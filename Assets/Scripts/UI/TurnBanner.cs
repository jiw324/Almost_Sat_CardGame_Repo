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

    private IEnumerator ShowBannerRoutine(string type)
    {
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
                    battleStartText.SetActive(true);
                    turnText.gameObject.SetActive(false);
                }
                break;

            case "player":
                if (turnText)
                {
                    turnText.gameObject.SetActive(true);
                    turnText.text = "Player Turn";
                    battleStartText.SetActive(false);
                }
                break;

            case "enemy":
                if (turnText)
                {
                    turnText.gameObject.SetActive(true);
                    turnText.text = "Enemy Turn";
                    battleStartText.SetActive(false);
                }
                break;
        }

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f &&
            !animator.IsInTransition(0)
        );

        // Hide everything
        if (background) background.SetActive(false);
        if (turnText) turnText.gameObject.SetActive(false);
        if (battleStartText) battleStartText.SetActive(false);

        Time.timeScale = 1f; // resume gameplay
        isShowing = false;
    }
}
