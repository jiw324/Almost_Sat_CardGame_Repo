using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// 2D platformer minigame similar to dino dash.
/// Player jumps over gaps in the floor by pressing space/jump key.
/// Score is based on how many gaps successfully cleared.
/// </summary>
public class PlatformerMinigame : MonoBehaviour, IMinigame
{
    [Header("UI References")]
    [SerializeField] private RectTransform gameArea;
    [SerializeField] private RectTransform playerTransform;
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private GameObject gapPrefab;
    [SerializeField] private Transform gapContainer;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 10.0f;
    [SerializeField] private float gapSpawnRate = 1.5f;
    [SerializeField] private float gapSpeed = 300f;         // Pixels per second (how fast gaps move left)
    [SerializeField] private float jumpHeight = 100f;      // Jump height in pixels
    [SerializeField] private float jumpDuration = 0.5f;     // Time to complete jump
    [SerializeField] private float groundY = -150f;
    [SerializeField] private float playerX = -200f;
    [SerializeField] private Vector2 playerSize = new Vector2(50f, 50f);
    [SerializeField] private int perfectScoreThreshold = 8;
    [SerializeField] private int goodScoreThreshold = 4;

    private TaskCompletionSource<float> completionSource;
    private float timeRemaining = 0f;
    private float gameStartTime = 0f;
    private float nextSpawnTime = 0f;
    private bool isActive = false;
    private bool isJumping = false;
    private float jumpStartTime = 0f;
    private float playerY = 0f;
    private int gapsCleared = 0;
    private System.Collections.Generic.List<GameObject> activeGaps = new System.Collections.Generic.List<GameObject>();

    public async Task<float> PlayAsync()
    {
        completionSource = new TaskCompletionSource<float>();
        ResetGame();

        isActive = true;
        timeRemaining = gameDuration;
        gameStartTime = Time.time;
        nextSpawnTime = gapSpawnRate;
        playerY = groundY;

        // Setup player sprite and position
        if (playerTransform != null)
        {
            // Get or add Image component and set the sprite
            Image playerImage = playerTransform.GetComponent<Image>();
            if (playerImage == null)
            {
                playerImage = playerTransform.gameObject.AddComponent<Image>();
            }
            
            if (playerSprite != null)
            {
                playerImage.sprite = playerSprite;
                playerImage.preserveAspect = true;
            }
            
            // Set player size
            playerTransform.sizeDelta = playerSize;
            
            // Position player
            playerTransform.anchoredPosition = new Vector2(playerX, groundY);
        }

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

            HandleJumpInput();
            UpdateJump();
            UpdateGapSpawning();
            UpdateGaps();
            CheckGapCollisions();
        }

        isActive = false;
        float score = CalculateScore();
        
        await Task.Delay(1000);
        
        CleanupGaps();

        completionSource.TrySetResult(score);

        return await completionSource.Task;
    }

    private void HandleJumpInput()
    {
        if (isJumping) return; // Can't jump while already jumping

        Keyboard kb = Keyboard.current;
        if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame))
        {
            StartJump();
        }
    }

    private void StartJump()
    {
        isJumping = true;
        jumpStartTime = Time.time - gameStartTime; // Use relative time
    }

    private void UpdateJump()
    {
        if (!isJumping || playerTransform == null) return;

        float currentTime = Time.time - gameStartTime;
        float elapsed = currentTime - jumpStartTime;
        RectTransform playerRect = playerTransform;

        if (elapsed < jumpDuration)
        {
            // Going up
            float progress = elapsed / jumpDuration;
            float jumpProgress = Mathf.Sin(progress * Mathf.PI); // Smooth arc
            playerY = groundY + (jumpHeight * jumpProgress);
            playerRect.anchoredPosition = new Vector2(playerX, playerY);
        }
        else
        {
            // Landing
            isJumping = false;
            playerY = groundY;
            playerRect.anchoredPosition = new Vector2(playerX, groundY);
        }
    }

    private void UpdateGapSpawning()
    {
        float currentTime = Time.time - gameStartTime;
        if (currentTime >= nextSpawnTime)
        {
            SpawnGap();
            nextSpawnTime = currentTime + gapSpawnRate;
        }
    }

    private void SpawnGap()
    {
        if (gapPrefab == null || gapContainer == null || gameArea == null) return;

        // Instantiate the gap prefab
        GameObject gap = Instantiate(gapPrefab, gapContainer);
        RectTransform gapRect = gap.GetComponent<RectTransform>();
        
        if (gapRect == null)
        {
            Debug.LogWarning("[PlatformerMinigame] Gap prefab missing RectTransform component!");
            Destroy(gap);
            return;
        }
        
        // Spawn on the right side of the game area, positioned at ground level
        float spawnX = gameArea.rect.width / 2f;
        // Position at ground level (assuming gap's pivot is at center)
        gapRect.anchoredPosition = new Vector2(spawnX, groundY);

        activeGaps.Add(gap);
    }

    private void UpdateGaps()
    {
        if (gameArea == null) return;

        float leftBound = -gameArea.rect.width / 2f;

        for (int i = activeGaps.Count - 1; i >= 0; i--)
        {
            GameObject gap = activeGaps[i];
            if (gap == null)
            {
                activeGaps.RemoveAt(i);
                continue;
            }

            RectTransform rect = gap.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 gapSizeActual = rect.sizeDelta;
                
                // Move gap left
                Vector2 pos = rect.anchoredPosition;
                pos.x -= gapSpeed * Time.deltaTime;
                rect.anchoredPosition = pos;

                if (pos.x + gapSizeActual.x / 2f < playerX - playerSize.x / 2f)
                {
                    // Player successfully cleared the gap
                    gapsCleared++;
                    UpdateScore();
                    Destroy(gap);
                    activeGaps.RemoveAt(i);
                    continue;
                }

                // Remove if off screen
                if (pos.x + gapSizeActual.x / 2f < leftBound - 100f)
                {
                    Destroy(gap);
                    activeGaps.RemoveAt(i);
                }
            }
        }
    }

    private void CheckGapCollisions()
    {
        // Player only hits a gap if they're on the ground
        if (playerTransform == null || isJumping) return;

        Vector2 playerPos = playerTransform.anchoredPosition;
        Vector2 playerSizeActual = playerTransform.sizeDelta;
        float playerLeft = playerPos.x - playerSizeActual.x / 2f;
        float playerRight = playerPos.x + playerSizeActual.x / 2f;
        float playerCenterY = playerPos.y;
        float playerBottom = playerPos.y - playerSizeActual.y / 2f;

        // Check if player is on the ground
        if (Mathf.Abs(playerCenterY - groundY) > 5f) return; // Not on ground

        for (int i = activeGaps.Count - 1; i >= 0; i--)
        {
            GameObject gap = activeGaps[i];
            if (gap == null) continue;

            RectTransform gapRect = gap.GetComponent<RectTransform>();
            if (gapRect == null) continue;

            Vector2 gapPos = gapRect.anchoredPosition;
            Vector2 gapSizeActual = gapRect.sizeDelta;
            float gapLeft = gapPos.x - gapSizeActual.x / 2f;
            float gapRight = gapPos.x + gapSizeActual.x / 2f;

            // Check if player's X range overlaps with gap's X range
            // Since player is on ground and gap is at ground level, X overlap means player fell in
            if (playerRight > gapLeft && playerLeft < gapRight)
            {
                Destroy(gap);
                activeGaps.RemoveAt(i);
                
                // End the game immediately
                isActive = false;

                break; // Exit loop since game is ending
            }
        }
    }

    private void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Cleared: {gapsCleared}";
        }
    }

    private float CalculateScore()
    {
        if (!isActive && timeRemaining > 0f)
        {
            return 0f; // Fell into a gap
        }

        // Map to 0-2 range: 0 = failure, 1.0 = default/normal, 2.0 = perfect
        if (gapsCleared >= perfectScoreThreshold)
        {
            // Perfect score - cleared many gaps
            return 2.0f;
        }
        else if (gapsCleared >= goodScoreThreshold)
        {
            // Good score - scale from 1.0 to 1.9
            float normalized = Mathf.InverseLerp(goodScoreThreshold, perfectScoreThreshold, gapsCleared);
            return 1.0f + (normalized * 0.9f);
        }
        else if (gapsCleared > 0)
        {
            // Poor score - scale from 0.1 to 1.0
            float normalized = Mathf.InverseLerp(0f, goodScoreThreshold, gapsCleared);
            return 0.1f + (normalized * 0.9f);
        }
        else
        {
            // No gaps cleared = failure
            return 0f;
        }
    }

    private void CleanupGaps()
    {
        foreach (GameObject gap in activeGaps)
        {
            if (gap != null)
                Destroy(gap);
        }
        activeGaps.Clear();
    }

    private void ResetGame()
    {
        gapsCleared = 0;
        timeRemaining = 0f;
        isJumping = false;
        playerY = groundY;

        if (scoreText != null)
            scoreText.text = "Cleared: 0 | Fell: 0";
        if (timerText != null)
            timerText.text = $"Time: {gameDuration:F1}s";

        CleanupGaps();
    }
}

