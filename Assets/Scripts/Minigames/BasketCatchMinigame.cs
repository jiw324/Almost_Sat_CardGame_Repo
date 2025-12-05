using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Basket catch minigame - catch correct ingredients, avoid wrong ones.
/// Ingredients fall from the top, player moves basket left/right to catch them.
/// Score is based on correct catches and avoiding wrong ingredients.
/// </summary>
public class BasketCatchMinigame : MonoBehaviour, IMinigame
{
    [Header("UI References")]
    [SerializeField] private RectTransform gameArea;
    [SerializeField] private RectTransform basketTransform;
    [SerializeField] private Sprite basketSprite;
    [SerializeField] private Sprite[] correctIngredientSprites; // Sprites for correct ingredients
    [SerializeField] private Sprite[] wrongIngredientSprites;   // Sprites for wrong ingredients
    [SerializeField] private Transform ingredientContainer;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 10.0f;
    [SerializeField] private float ingredientSpawnRate = 0.8f;
    [SerializeField] private float fallSpeed = 200f;            // Pixels per second
    [SerializeField] private float basketSpeed = 400f;          // Pixels per second movement
    [SerializeField] private float basketY = -180f;            // Y position of basket (fixed)
    [SerializeField] private float spawnY = 200f;               // Y position to spawn ingredients
    [SerializeField] private Vector2 ingredientSize = new Vector2(80f, 80f); // Size of ingredient sprites
    [SerializeField] private float correctCatchPoints = 1f;
    [SerializeField] private float wrongCatchPenalty = -2f;
    [SerializeField] private float perfectScoreThreshold = 12f;
    [SerializeField] private float goodScoreThreshold = 6f;

    private TaskCompletionSource<float> completionSource;
    private float timeRemaining = 0f;
    private float nextSpawnTime = 0f;
    private bool isActive = false;
    private float basketX = 0f;
    private float currentScore = 0f;
    private System.Collections.Generic.List<GameObject> activeIngredients = new System.Collections.Generic.List<GameObject>();
    private System.Collections.Generic.List<bool> ingredientIsCorrect = new System.Collections.Generic.List<bool>();

    public async Task<float> PlayAsync()
    {
        completionSource = new TaskCompletionSource<float>();
        ResetGame();

        isActive = true;
        timeRemaining = gameDuration;
        nextSpawnTime = ingredientSpawnRate;
        basketX = 0f;

        // Setup basket sprite and position
        if (basketTransform != null)
        {
            Image basketImage = basketTransform.GetComponent<Image>();
            if (basketImage == null)
            {
                basketImage = basketTransform.gameObject.AddComponent<Image>();
            }
            
            if (basketSprite != null)
            {
                basketImage.sprite = basketSprite;
                basketImage.preserveAspect = true;
            }
            
            // Position basket
            basketTransform.anchoredPosition = new Vector2(0f, basketY);
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

            HandleBasketMovement();
            UpdateIngredientSpawning();
            UpdateIngredients();
            CheckCatches();
        }

        isActive = false;
        float score = CalculateScore();
        CleanupIngredients();
        completionSource.TrySetResult(score);

        return await completionSource.Task;
    }

    private void HandleBasketMovement()
    {
        if (basketTransform == null || gameArea == null) return;

        Keyboard kb = Keyboard.current;
        float moveDirection = 0f;

        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
                moveDirection = -1f;
            else if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
                moveDirection = 1f;
        }

        // Update basket position
        float moveAmount = moveDirection * basketSpeed * Time.deltaTime;
        float halfWidth = gameArea.rect.width / 2f;
        basketX = Mathf.Clamp(basketX + moveAmount, -halfWidth + 50f, halfWidth - 50f);

        basketTransform.anchoredPosition = new Vector2(basketX, basketY);
    }

    private void UpdateIngredientSpawning()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnIngredient();
            nextSpawnTime = Time.time + ingredientSpawnRate;
        }
    }

    private void SpawnIngredient()
    {
        if (gameArea == null || ingredientContainer == null) return;

        // Randomly decide if correct or wrong (70% correct, 30% wrong)
        bool isCorrect = Random.value > 0.3f;
        
        // Get the appropriate sprite list
        Sprite[] spriteList = isCorrect ? correctIngredientSprites : wrongIngredientSprites;
        
        // Check if we have sprites available
        if (spriteList == null || spriteList.Length == 0)
        {
            Debug.LogWarning($"[BasketCatchMinigame] No sprites available for {(isCorrect ? "correct" : "wrong")} ingredients!");
            return;
        }
        
        // Pick a random sprite from the list
        Sprite spriteToUse = spriteList[Random.Range(0, spriteList.Length)];
        
        // Create a new GameObject with Image component
        GameObject ingredient = new GameObject($"Ingredient_{(isCorrect ? "Correct" : "Wrong")}");
        ingredient.transform.SetParent(ingredientContainer, false);
        
        // Add RectTransform
        RectTransform ingredientRect = ingredient.AddComponent<RectTransform>();
        
        // Add Image component and set the sprite
        Image ingredientImage = ingredient.AddComponent<Image>();
        ingredientImage.sprite = spriteToUse;
        ingredientImage.preserveAspect = true; // Maintain sprite aspect ratio
        
        // Set size
        ingredientRect.sizeDelta = ingredientSize;
        
        // Spawn at random X position within game area
        float halfWidth = gameArea.rect.width / 2f;
        float spawnX = Random.Range(-halfWidth + 30f, halfWidth - 30f);
        ingredientRect.anchoredPosition = new Vector2(spawnX, spawnY);

        activeIngredients.Add(ingredient);
        ingredientIsCorrect.Add(isCorrect);
    }

    private void UpdateIngredients()
    {
        if (gameArea == null) return;

        float bottomBound = -gameArea.rect.height / 2f;

        for (int i = activeIngredients.Count - 1; i >= 0; i--)
        {
            GameObject ingredient = activeIngredients[i];
            if (ingredient == null)
            {
                activeIngredients.RemoveAt(i);
                ingredientIsCorrect.RemoveAt(i);
                continue;
            }

            RectTransform rect = ingredient.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Move ingredient down
                Vector2 pos = rect.anchoredPosition;
                pos.y -= fallSpeed * Time.deltaTime;
                rect.anchoredPosition = pos;

                // Remove if off screen (missed)
                if (pos.y < bottomBound - 50f)
                {
                    Destroy(ingredient);
                    activeIngredients.RemoveAt(i);
                    ingredientIsCorrect.RemoveAt(i);
                }
            }
        }
    }

    private void CheckCatches()
    {
        if (basketTransform == null) return;

        Vector2 basketPos = basketTransform.anchoredPosition;
        Rect basketBounds = new Rect(basketPos.x - 40f, basketPos.y - 20f, 80f, 40f);

        for (int i = activeIngredients.Count - 1; i >= 0; i--)
        {
            GameObject ingredient = activeIngredients[i];
            if (ingredient == null) continue;

            RectTransform ingredientRect = ingredient.GetComponent<RectTransform>();
            if (ingredientRect == null) continue;

            Vector2 ingredientPos = ingredientRect.anchoredPosition;
            Vector2 ingredientSize = ingredientRect.sizeDelta;
            Rect ingredientBounds = new Rect(
                ingredientPos.x - ingredientSize.x / 2f, 
                ingredientPos.y - ingredientSize.y / 2f, 
                ingredientSize.x, 
                ingredientSize.y
            );

            if (basketBounds.Overlaps(ingredientBounds))
            {
                // Caught!
                bool isCorrect = ingredientIsCorrect[i];
                if (isCorrect)
                {
                    currentScore += correctCatchPoints;
                }
                else
                {
                    currentScore += wrongCatchPenalty;
                }

                UpdateScore();
                Destroy(ingredient);
                activeIngredients.RemoveAt(i);
                ingredientIsCorrect.RemoveAt(i);
            }
        }
    }

    private void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore:F0}";
        }
    }

    private float CalculateScore()
    {
        // Map to 0-2 range: 0 = failure, 1.0 = default/normal, 2.0 = perfect
        if (currentScore >= perfectScoreThreshold)
        {
            // Perfect score - 2.0
            return 2.0f;
        }
        else if (currentScore >= goodScoreThreshold)
        {
            // Good score - scale from 1.0 to 1.9
            float normalized = Mathf.InverseLerp(goodScoreThreshold, perfectScoreThreshold, currentScore);
            return 1.0f + (normalized * 0.9f);
        }
        else if (currentScore > 0f)
        {
            // Poor score - scale from 0.1 to 1.0
            float normalized = Mathf.InverseLerp(0f, goodScoreThreshold, currentScore);
            return 0.1f + (normalized * 0.9f);
        }
        else
        {
            // Negative or zero score = failure
            return 0f;
        }
    }

    private void CleanupIngredients()
    {
        foreach (GameObject ingredient in activeIngredients)
        {
            if (ingredient != null)
                Destroy(ingredient);
        }
        activeIngredients.Clear();
        ingredientIsCorrect.Clear();
    }

    private void ResetGame()
    {
        currentScore = 0f;
        timeRemaining = 0f;
        basketX = 0f;

        if (scoreText != null)
            scoreText.text = "Score: 0";
        if (timerText != null)
            timerText.text = $"Time: {gameDuration:F1}s";

        CleanupIngredients();
    }
}

