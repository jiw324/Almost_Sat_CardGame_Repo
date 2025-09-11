using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CardMenu : MonoBehaviour
{
    private Animator animator;
    private Button button;
    private bool isOpen = false;

    private Vector3 originalPosition;

    private Vector3 focusPosition = new Vector3(240f, 0f, 0f);

    [SerializeField] private GameObject subMenuPanel; // reference to submenu

    private TMP_Text buttonText;

    [SerializeField] private static CardMenu currentlyOpenCard;

    void Start()
    {
        animator = GetComponent<Animator>();
        button = GetComponent<Button>();
        originalPosition = transform.localPosition;

        button.onClick.AddListener(OnCardClicked);

        // get the Text component from child
        buttonText = GetComponentInChildren<TMP_Text>();

        // make sure submenu is hidden initially
        if (subMenuPanel != null)
            subMenuPanel.SetActive(false);
    }

    void OnCardClicked()
    {
        if (currentlyOpenCard != null && currentlyOpenCard != this)
        {
            currentlyOpenCard.CloseCard();
        }

        if (isOpen)
        {
            CloseCard();
        }
        else
        {
            OpenCard();
        }
    }

    void OpenCard()
    {
        animator.SetBool("isOpen", true);
        if (buttonText != null)
            StartCoroutine(TextAfterDelay(0.1f, false));  // hide text after delay

        if (subMenuPanel != null)
            StartCoroutine(SubmenuAfterDelay(0.2f, true));            // show submenu

        StopCoroutine("MoveTo");
        StartCoroutine(MoveTo(focusPosition, 0.3f));

        isOpen = true;
        currentlyOpenCard = this;
    }

    public void CloseCard()
    {
        animator.SetBool("isOpen", false);
        if (buttonText != null)
            StartCoroutine(TextAfterDelay(0.2f, true));  // show text after delay

        if (subMenuPanel != null)
            StartCoroutine(SubmenuAfterDelay(0.1f, false));           // hide submenu

        StopCoroutine("MoveTo");
        StartCoroutine(MoveTo(originalPosition, 0.3f));

        isOpen = false;
        if (currentlyOpenCard == this)
            currentlyOpenCard = null;
    }

    IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localPosition = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = target;
    }

    IEnumerator TextAfterDelay(float delay, bool showText)
    {
        yield return new WaitForSeconds(delay);
        buttonText.gameObject.SetActive(showText);
    }
    IEnumerator SubmenuAfterDelay(float delay, bool showSubmenu)
    {
        yield return new WaitForSeconds(delay);
        subMenuPanel.gameObject.SetActive(showSubmenu);
    }
}
