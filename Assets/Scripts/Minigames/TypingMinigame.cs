using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TypingMinigame : MonoBehaviour, IMinigame
{
    [Header("UI References")]
    [SerializeField] private RectTransform wordsContainer;        // Parent container for word panels
    [SerializeField] private GameObject wordPanelTemplate;  // Disabled template panel
    [SerializeField] private TMP_Text inputText;            // Displays current input (placeholder)
    [SerializeField] private TMP_Text timerText;            // Displays time remaining or title

    [Header("Demo Settings")]
    [SerializeField] private int totalWords = 9;
    [SerializeField] private float totalTime = 5f;
    [SerializeField] private bool autoFillOnStart = true;

    private List<string> words;
    private List<RectTransform> spawnedPanels = new();
    private TaskCompletionSource<float> completionSource;

    public async Task<float> PlayAsync()
    {
        completionSource = new TaskCompletionSource<float>();
        ResetUI();

        if (autoFillOnStart)
            await GenerateWordPanels();

        // Optional fake timer animation (for demo)
        float timeRemaining = totalTime;
        while (timeRemaining > 0)
        {
            await Task.Yield();
            timeRemaining -= Time.deltaTime;
            timerText.text = $"{timeRemaining:F1}s";
        }

        // Demo only: instantly return 1 (success)
        completionSource.TrySetResult(1f);
        return await completionSource.Task;
    }

    private void MatchParentWidthToContainer()
    {
        // assuming these are set in the inspector
        RectTransform typingRect = GetComponent<RectTransform>();
        RectTransform containerRect = wordsContainer;

        // Force layout update first
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

        // Now copy the width
        float newWidth = containerRect.rect.width + 60f; // optional padding
        typingRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }

    private void ResetUI()
    {
        // Clear existing panels
        foreach (Transform child in wordsContainer)
            if (child.gameObject != wordPanelTemplate)
                Destroy(child.gameObject);
        spawnedPanels.Clear();

        inputText.text = "";
        timerText.text = $"{totalTime:F1}s";
    }

    // Task to generate word panels with slight delay for effect
    private async Task GenerateWordPanels()
    {
        await Task.Delay(500);
        words = WordDatabase.GetRandomWords(totalWords);

        foreach (string word in words)
        {
            CreateWordPanel(word);
            await Task.Delay(75);
        }
    }

    private void CreateWordPanel(string word)
    {
        GameObject panel = Instantiate(wordPanelTemplate, wordsContainer);
        panel.SetActive(true);

        TMP_Text wordText = panel.GetComponentInChildren<TMP_Text>();
        wordText.text = word;

        // Force TextMeshPro to update and measure
        wordText.ForceMeshUpdate();
        float width = wordText.preferredWidth + 34f; // padding

        // Apply width to LayoutElement or RectTransform
        LayoutElement layout = panel.GetComponent<LayoutElement>();
        if (layout != null)
        {
            layout.preferredWidth = width;
            Debug.Log($"Set LayoutElement preferredWidth to {width} for word '{word}'");
        }

        else
            panel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        spawnedPanels.Add(panel.GetComponent<RectTransform>());
        MatchParentWidthToContainer();
    }
}
