using System;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class ReactionMinigame : MonoBehaviour, IMinigame, IPointerClickHandler
{
    [SerializeField] private Transform gameArea;
    [SerializeField] private TextMeshProUGUI gameText;

    [Header("Difficulty Settings")]
    [SerializeField] private Color[] otherColors;
    [SerializeField] private float bestReactionTime = 0.3f;
    [SerializeField] private float worstReactionTime = 1.0f;
    [SerializeField] private float timeBeforeMaxChance = 3.0f;

	private TaskCompletionSource<float> completionSource;

    private float reactionTime;
    private float normalizedResult;
    private bool difficult = false;
    private int clicks = 0;

    private float elapsed = 0f;
    private float timeAtChange;
    private bool colorChanged = false;

    public async Task<float> PlayAsync()
    {
        completionSource = new TaskCompletionSource<float>();

        if (otherColors.Length > 0)
            difficult = true;

        gameText.text = "Wait for green!";
        elapsed = 0f;
        clicks = 0;
        colorChanged = false;

        return await completionSource.Task;
    }

    void Update()
	{
        elapsed += Time.deltaTime;
        if (clicks == 1)
        {
            if (elapsed >= 2f)
            {
                FinishGame();
                clicks = 2;
            }
            return;
        }

        if (clicks >= 2) return;

        if (!colorChanged)
        {
            // chance increases with time
            float realChance = Mathf.InverseLerp(0, timeBeforeMaxChance, elapsed);
            if (UnityEngine.Random.value < realChance * Time.deltaTime)
            {
                gameArea.GetComponent<Image>().color = Color.green;
                colorChanged = true;
                timeAtChange = Time.time;
            }
            if (difficult)
            {
                float decoyChance = Mathf.InverseLerp(0, timeBeforeMaxChance / 2f, elapsed);
                if (UnityEngine.Random.value < decoyChance * Time.deltaTime)
                {
                    // Get random color from otherColors
                    gameArea.GetComponent<Image>().color = otherColors[UnityEngine.Random.Range(0, otherColors.Length)];
                }
            }
        }
        else
        {
            gameText.text = $"{Time.time - timeAtChange:F2}s";
        }

        if (colorChanged && Time.time > timeAtChange + worstReactionTime)
        {
            clicks = 1;
            elapsed = 0f;
            HandleReaction();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clicks >= 2) return; // already finished

        if (clicks == 0)
        {
            // first click
            HandleReaction();
        }
        else if (clicks == 1)
        {
            // second click
            FinishGame();
        }

        clicks++;
        elapsed = 0f; // reset idle timer after click
    }


    private void HandleReaction()
    {
        if (!colorChanged)
        {
            // clicked before green
            normalizedResult = 0f;
            gameText.text = "Too fast!";
            Debug.Log("[Reaction] Clicked too early!");
            return;
        }

        reactionTime = Time.time - timeAtChange;
        // Map to 0-2 range: 0 = worst, 1.0 = default/normal, 2.0 = perfect
        float normalized01 = Mathf.Clamp01(
            Mathf.InverseLerp(worstReactionTime, bestReactionTime, reactionTime));
        normalizedResult = normalized01 * 2.0f; // Scale from 0-1 to 0-2

        string result =
            reactionTime < bestReactionTime ? "Nice!"
            : reactionTime < worstReactionTime ? "Good!"
             : "Too slow!";

        gameText.text = $"{reactionTime:F2}\n{result}";
        Debug.Log($"[Reaction] Reaction time: {reactionTime:F2}s");
    }

    private void FinishGame()
    {
        completionSource?.TrySetResult(normalizedResult);
        gameObject.SetActive(false);
    }

}

