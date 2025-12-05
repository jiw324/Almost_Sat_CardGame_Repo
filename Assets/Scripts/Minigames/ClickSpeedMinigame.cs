using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Clicks per second test minigame
/// Player clicks as fast as possible for a set duration.
/// Score is based on clicks per second achieved.
/// </summary>
public class ClickSpeedMinigame : MonoBehaviour, IMinigame
{
    [Header("UI References")]
    [SerializeField] private RectTransform shurikenTransform;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text clicksText;
    [SerializeField] private TMP_Text cpsText;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 5.0f;    // How long the game lasts
    [SerializeField] private float baseSpinSpeed = 180f;    // Base rotation speed (degrees per second)
    [SerializeField] private float maxSpinSpeed = 1080f;     // Maximum rotation speed at high CPS
    [SerializeField] private float minCPSForPerfect = 8.0f; // CPS needed for perfect score (2.0)
    [SerializeField] private float minCPSForGood = 4.0f;   // CPS needed for good score (1.0)
    [SerializeField] private float cpsWindow = 0.2f;       // Time window for calculating CPS (seconds)

    private TaskCompletionSource<float> completionSource;
    private int clickCount = 0;
    private float timeRemaining = 0f;
    private bool isActive = false;
    private float currentRotation = 0f;
    private float currentCPS = 0f;
    private System.Collections.Generic.List<float> clickTimestamps = new System.Collections.Generic.List<float>();
    private int lastClickFrame = -1; // Prevent double clicks in the same frame

    public async Task<float> PlayAsync()
    {
        completionSource = new TaskCompletionSource<float>();
        ResetGame();

        if (shurikenTransform != null)
        {
            var button = shurikenTransform.GetComponent<Button>();
            if (button == null)
            {
                button = shurikenTransform.gameObject.AddComponent<Button>();
            }
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        isActive = true;
        timeRemaining = gameDuration;
        clickTimestamps.Clear();

        // Main game loop
        while (isActive && timeRemaining > 0f)
        {
            await Task.Yield();

            // Update timer
            timeRemaining -= Time.deltaTime;
            if (timerText != null)
            {
                timerText.text = $"Time: {timeRemaining:F1}s";
            }

            // Calculate current CPS from recent clicks
            CalculateCurrentCPS();

            // Update rotation speed based on CPS
            float currentSpinSpeed = CalculateSpinSpeed();

            // Rotate shuriken
            if (shurikenTransform != null)
            {
                currentRotation += currentSpinSpeed * Time.deltaTime;
                shurikenTransform.rotation = Quaternion.Euler(0, 0, currentRotation);
            }

            // Update UI continuously
            UpdateUI();
        }

        isActive = false;
        float score = CalculateScore();
        
        // Calculate and log final CPS
        float elapsed = gameDuration - timeRemaining;
        float finalCPS = elapsed > 0.1f ? clickCount / elapsed : 0f;
        Debug.Log($"[ClickSpeedMinigame] Game finished! Clicks: {clickCount}, Time: {elapsed:F2}s, Final CPS: {finalCPS:F2}, Score: {score:F2}");
        
        // Show final CPS result
        if (cpsText != null)
        {
            cpsText.text = $"CPS: {finalCPS:F2}";
        }
        
        // Wait 1 second before closing (showing the final CPS during this time)
        await Task.Delay(1000);
        
        completionSource.TrySetResult(score);

        return await completionSource.Task;
    }

    private void OnClick()
    {
        if (!isActive || timeRemaining <= 0f) return;

        // Prevent double registration in the same frame
        int currentFrame = Time.frameCount;
        if (currentFrame == lastClickFrame)
        {
            return; // Already processed a click this frame
        }
        lastClickFrame = currentFrame;

        clickCount++;
        float currentTime = gameDuration - timeRemaining;
        clickTimestamps.Add(currentTime);

        // Optional: Add visual feedback (scale pulse, color flash)
        if (shurikenTransform != null)
        {
            // Quick scale pulse using coroutine
            StartCoroutine(PulseShuriken());
        }
    }

    private void CalculateCurrentCPS()
    {
        if (clickTimestamps.Count == 0)
        {
            currentCPS = 0f;
            return;
        }

        float currentTime = gameDuration - timeRemaining;
        float windowStart = currentTime - cpsWindow;

        // Remove clicks outside the time window
        clickTimestamps.RemoveAll(timestamp => timestamp < windowStart);

        if (clickTimestamps.Count == 0)
        {
            currentCPS = 0f;
            return;
        }

        // Calculate CPS based on clicks in the window
        if (currentTime > 0.01f && clickTimestamps.Count > 0)
        {
            currentCPS = clickTimestamps.Count / cpsWindow;
        }
        else
        {
            currentCPS = 0f;
        }
    }

    private float CalculateSpinSpeed()
    {
        // Map CPS to rotation speed
        // Base speed at 0 CPS, max speed at high CPS (minCPSForPerfect or higher)
        if (currentCPS <= 0f)
        {
            return baseSpinSpeed;
        }
        else if (currentCPS >= minCPSForPerfect)
        {
            return maxSpinSpeed;
        }
        else
        {
            // Interpolate between base and max speed
            float normalized = Mathf.InverseLerp(0f, minCPSForPerfect, currentCPS);
            return Mathf.Lerp(baseSpinSpeed, maxSpinSpeed, normalized);
        }
    }

    private void UpdateUI()
    {
        if (clicksText != null)
        {
            clicksText.text = $"Clicks: {clickCount}";
        }
    }

    private float CalculateScore()
    {
        if (gameDuration <= 0f) return 0f;

        float elapsed = gameDuration - timeRemaining;
        if (elapsed <= 0.1f) return 0f; // No time elapsed = failure

        float cps = clickCount / elapsed;

        // Map to 0-2 range: 0 = failure, 1.0 = default/normal, 2.0 = perfect
        if (cps >= minCPSForPerfect)
        {
            // Perfect score - 2.0
            return 2.0f;
        }
        else if (cps >= minCPSForGood)
        {
            // Good score - scale from 1.0 to 1.9
            float normalized = Mathf.InverseLerp(minCPSForGood, minCPSForPerfect, cps);
            return 1.0f + (normalized * 0.9f);
        }
        else if (cps > 0f)
        {
            // Poor score - scale from 0.1 to 1.0
            float normalized = Mathf.InverseLerp(0f, minCPSForGood, cps);
            return 0.1f + (normalized * 0.9f);
        }
        else
        {
            // No clicks = failure
            return 0f;
        }
    }

    private IEnumerator PulseShuriken()
    {
        if (shurikenTransform == null) yield break;

        Vector3 originalScale = Vector3.one;
        Vector3 pulseScale = Vector3.one * 1.2f;
        float duration = 0.1f;
        float elapsed = 0f;

        // Scale up
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            shurikenTransform.localScale = Vector3.Lerp(originalScale, pulseScale, t);
            yield return null;
        }

        // Scale down
        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            shurikenTransform.localScale = Vector3.Lerp(pulseScale, originalScale, t);
            yield return null;
        }

        shurikenTransform.localScale = originalScale;
    }

    private void ResetGame()
    {
        clickCount = 0;
        timeRemaining = 0f;
        currentRotation = 0f;
        currentCPS = 0f;
        clickTimestamps.Clear();
        lastClickFrame = -1;

        if (timerText != null)
            timerText.text = $"Time: {gameDuration:F1}s";
        if (clicksText != null)
            clicksText.text = "Clicks: 0";
        if (cpsText != null)
            cpsText.text = ""; // Clear CPS text at start

        if (shurikenTransform != null)
        {
            shurikenTransform.rotation = Quaternion.identity;
            shurikenTransform.localScale = Vector3.one;
        }
    }
}

