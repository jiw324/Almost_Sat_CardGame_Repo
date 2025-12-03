using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TypingMinigame : MonoBehaviour, IMinigame
{
    [Header("UI References")]
    [SerializeField] private RectTransform wordsContainer;        // Parent container for word panels
    [SerializeField] private GameObject wordPanelTemplate;  // Disabled template panel
    [SerializeField] private TMP_Text timerText;            // Displays time remaining
    [SerializeField] private TMP_Text progressText;         // Shows progress (e.g., "3/9 words")

    [Header("Game Settings")]
    [SerializeField] private int totalWords = 3;              // Number of words to type (scalable)
    private const float secondsPerWord = 1.0f;               // Time per word (total time = totalWords * secondsPerWord)

    private List<string> words;
    private List<WordPanel> wordPanels = new();
    private TaskCompletionSource<float> completionSource;
    
    private int currentWordIndex = 0;
    private string currentInput = "";
    private float timeRemaining = 0f;
    private int wordsCompleted = 0;
    private bool isActive = false;

    private class WordPanel
    {
        public GameObject panel;
        public TMP_Text wordText;
        public Image background;
        public bool isCompleted = false;
        public string originalWord;  // Store the original word for reference
    }

    public async Task<float> PlayAsync()
    {
        completionSource = new TaskCompletionSource<float>();
        ResetUI();
        
        // Generate words and panels
        await GenerateWordPanels();
        
        // Initialize game state
        currentWordIndex = 0;
        currentInput = "";
        wordsCompleted = 0;
        timeRemaining = totalWords * secondsPerWord;
        isActive = true;
        
        // Ensure Input System is enabled
        if (Keyboard.current == null)
        {
            Debug.LogError("[TypingMinigame] Keyboard.current is null! Input System may not be initialized.");
        }
        
        // Update UI
        UpdateWordPanelDisplay();
        UpdateProgressDisplay();
        HighlightCurrentWord();
        
        // Main game loop
        while (isActive && currentWordIndex < words.Count)
        {
            await Task.Yield();
            
            // Handle input
            HandleInput();
            
            // Update timer
            timeRemaining -= Time.deltaTime;
            if (timerText != null)
            {
                timerText.text = $"Time: {timeRemaining:F1}s";
            }
            
            // Check for timeout
            if (timeRemaining <= 0f)
            {
                // Time's up - end game
                break;
            }
        }
        
        isActive = false;
        
        // Calculate final score
        float score = CalculateScore();
        completionSource.TrySetResult(score);
        
        return await completionSource.Task;
    }

    private void HandleInput()
    {
        if (!isActive) return;
        
        // Get keyboard input
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            Debug.LogWarning("[TypingMinigame] Keyboard.current is null!");
            return;
        }
        
        // Handle backspace
        if (keyboard.backspaceKey.wasPressedThisFrame)
        {
            if (currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                UpdateWordPanelDisplay();
            }
            return;
        }
        
        // Handle space (submit word)
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            CheckWordCompletion();
            return;
        }
        
        // Handle regular character input using Input System
        // Check all letter keys (A-Z)
        for (Key key = Key.A; key <= Key.Z; key++)
        {
            if (keyboard[key].wasPressedThisFrame)
            {
                char character = (char)('a' + (key - Key.A));
                // Check if shift is held for uppercase
                if (keyboard.shiftKey.isPressed || keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)
                {
                    character = char.ToUpper(character);
                }
                currentInput += character;
                UpdateWordPanelDisplay();
                
                // Auto-submit if word matches exactly
                if (currentWordIndex < words.Count && 
                    currentInput.Equals(words[currentWordIndex], System.StringComparison.OrdinalIgnoreCase))
                {
                    CheckWordCompletion();
                }
                return; // Only process one key per frame
            }
        }
        
        // Also check for number keys (0-9)
        for (Key key = Key.Digit0; key <= Key.Digit9; key++)
        {
            if (keyboard[key].wasPressedThisFrame)
            {
                char character = (char)('0' + (key - Key.Digit0));
                currentInput += character;
                UpdateWordPanelDisplay();
                
                // Auto-submit if word matches exactly
                if (currentWordIndex < words.Count && 
                    currentInput.Equals(words[currentWordIndex], System.StringComparison.OrdinalIgnoreCase))
                {
                    CheckWordCompletion();
                }
                return;
            }
        }
    }
    
    private void CheckWordCompletion()
    {
        if (currentWordIndex >= words.Count) return;
        
        string targetWord = words[currentWordIndex];
        bool isCorrect = currentInput.Equals(targetWord, System.StringComparison.OrdinalIgnoreCase);
        
        if (isCorrect)
        {
            // Word completed!
            wordsCompleted++;
            wordPanels[currentWordIndex].isCompleted = true;
            MarkWordCompleted(currentWordIndex);
            
            // Move to next word
            currentWordIndex++;
            currentInput = "";
            
            if (currentWordIndex < words.Count)
            {
                // Move to next word (timer continues, doesn't reset)
                HighlightCurrentWord();
            }
            else
            {
                // All words completed!
                isActive = false;
            }
            
            UpdateProgressDisplay();
        }
        else
        {
            // Wrong word - clear input and show feedback
            currentInput = "";
            UpdateWordPanelDisplay();
            // Could add visual feedback here (shake, red flash, etc.)
        }
    }
    
    private void UpdateWordPanelDisplay()
    {
        if (currentWordIndex >= wordPanels.Count || currentWordIndex < 0) return;
        
        WordPanel currentPanel = wordPanels[currentWordIndex];
        if (currentPanel.wordText == null) return;
        
        string targetWord = words[currentWordIndex];
        string display = "";
        
        // Build the colored text based on what's been typed
        for (int i = 0; i < targetWord.Length; i++)
        {
            if (i < currentInput.Length)
            {
                // Character has been typed - check if it's correct
                char typedChar = currentInput[i];
                char targetChar = targetWord[i];
                
                if (char.ToLowerInvariant(typedChar) == char.ToLowerInvariant(targetChar))
                {
                    // Correct character - green
                    display += $"<color=#00FF00>{targetChar}</color>";
                }
                else
                {
                    // Incorrect character - red
                    display += $"<color=#FF0000>{typedChar}</color>";
                }
            }
            else
            {
                // Not typed yet - white
                display += $"<color=#FFFFFF>{targetWord[i]}</color>";
            }
        }
        
        currentPanel.wordText.text = display;
    }
    
    private void UpdateProgressDisplay()
    {
        if (progressText != null)
        {
            progressText.text = $"Words: {wordsCompleted}/{words.Count}";
        }
    }
    
    private void HighlightCurrentWord()
    {
        // Reset all word panels
        for (int i = 0; i < wordPanels.Count; i++)
        {
            if (wordPanels[i].background != null)
            {
                if (wordPanels[i].isCompleted)
                {
                    wordPanels[i].background.color = new Color(0f, 1f, 0f, 0.3f); // Green for completed
                }
                else if (i == currentWordIndex)
                {
                    wordPanels[i].background.color = new Color(1f, 1f, 0f, 0.5f); // Yellow for current
                }
                else
                {
                    wordPanels[i].background.color = new Color(1f, 1f, 1f, 0.1f); // White for pending
                }
            }
        }
    }
    
    private void MarkWordCompleted(int index)
    {
        if (index >= 0 && index < wordPanels.Count)
        {
            WordPanel panel = wordPanels[index];
            if (panel.wordText != null)
            {
                // Show completed word in all green
                string completedWord = panel.originalWord;
                string display = "";
                foreach (char c in completedWord)
                {
                    display += $"<color=#00FF00>{c}</color>";
                }
                panel.wordText.text = display;
            }
            if (panel.background != null)
            {
                panel.background.color = new Color(0f, 1f, 0f, 0.3f);
            }
        }
    }
    
    private float CalculateScore()
    {
        if (words.Count == 0) return 0f;
        
        // Base score: percentage of words completed
        float completionRatio = (float)wordsCompleted / words.Count;
        
        // Bonus for completing all words
        if (wordsCompleted == words.Count)
        {
            // Perfect score
            return 1.0f;
        }
        else if (wordsCompleted == 0)
        {
            // Failed
            return 0.0f;
        }
        else
        {
            // Partial completion: scale from 0.3 to 0.9 based on completion
            // This gives some credit for partial completion but rewards full completion
            return 0.3f + (completionRatio * 0.6f);
        }
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
        wordPanels.Clear();

        if (timerText != null)
            timerText.text = $"{(totalWords * secondsPerWord):F1}s";
        if (progressText != null)
            progressText.text = "Words: 0/0";
    }

    // Task to generate word panels with slight delay for effect
    private async Task GenerateWordPanels()
    {
        await Task.Delay(300);
        words = WordDatabase.GetRandomWords(totalWords);

        foreach (string word in words)
        {
            CreateWordPanel(word);
            await Task.Delay(50);
        }
    }

    private void CreateWordPanel(string word)
    {
        GameObject panel = Instantiate(wordPanelTemplate, wordsContainer);
        panel.SetActive(true);

        TMP_Text wordText = panel.GetComponentInChildren<TMP_Text>();

        // Get background image for highlighting
        Image background = panel.GetComponent<Image>();
        if (background == null)
        {
            background = panel.GetComponentInChildren<Image>();
        }

        // Force TextMeshPro to update and measure
        if (wordText != null)
        {
            wordText.ForceMeshUpdate();
            float width = wordText.preferredWidth + 34f; // padding

            // Apply width to LayoutElement or RectTransform
            LayoutElement layout = panel.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.preferredWidth = width;
            }
            else
            {
                panel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
        }

        // Store panel reference
        WordPanel wordPanel = new WordPanel
        {
            panel = panel,
            wordText = wordText,
            background = background,
            originalWord = word  // Store original word for reference
        };
        wordPanels.Add(wordPanel);
        
        // Initialize display with white text
        if (wordText != null)
        {
            string display = "";
            foreach (char c in word)
            {
                display += $"<color=#FFFFFF>{c}</color>";
            }
            wordText.text = display;
        }
        
        MatchParentWidthToContainer();
    }
}
