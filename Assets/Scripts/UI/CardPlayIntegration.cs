using UnityEngine;
using System.Collections;

public class CardPlayIntegration : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float checkInterval = 0.2f;
    [SerializeField] private bool trackPlayerCards = true;
    [SerializeField] private bool trackEnemyCards = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private BattleManager battleManager;
    private AutoCardDisplaySetup displaySetup;
    private CardDisplayBar displayBar;
    
    private int lastPlayerHandCount = 0;
    
    private bool isInitialized = false;
    
    private void Start()
    {
        StartCoroutine(InitializeWithDelay());
    }
    
    private IEnumerator InitializeWithDelay()
    {
        yield return new WaitForSeconds(1f);
        
        battleManager = BattleManager.Instance;
        displaySetup = GetComponent<AutoCardDisplaySetup>();
        
        if (displaySetup != null)
        {
            displayBar = displaySetup.GetDisplayBar();
        }
        
        if (battleManager == null)
        {
            LogWarning("BattleManager not found! Card play tracking disabled.");
            yield break;
        }
        
        if (displayBar == null)
        {
            LogWarning("Display bar not found! Waiting for setup...");
            
            yield return new WaitForSeconds(2f);
            if (displaySetup != null)
            {
                displayBar = displaySetup.GetDisplayBar();
            }
            
            if (displayBar == null)
            {
                LogWarning("Display bar still not found! Card play tracking disabled.");
                yield break;
            }
        }
        
        isInitialized = true;
        Log("Card play integration initialized successfully!");
        
        StartCoroutine(MonitorCardPlays());
    }
    
    private IEnumerator MonitorCardPlays()
    {
        while (isInitialized)
        {
            yield return new WaitForSeconds(checkInterval);
            
            if (battleManager == null || displayBar == null)
                continue;
            
            if (trackPlayerCards)
            {
                CheckPlayerCardPlays();
            }
            
            if (trackEnemyCards)
            {
                CheckEnemyCardPlays();
            }
        }
    }
    
    private void CheckPlayerCardPlays()
    {
        if (battleManager.playerHandManager == null)
            return;
        
        var handManager = battleManager.playerHandManager;
        
        var cardsInHandField = typeof(HandManager).GetField("cardsInHand",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (cardsInHandField != null)
        {
            var cardsInHand = cardsInHandField.GetValue(handManager) as System.Collections.Generic.List<CardInstance>;
            
            if (cardsInHand != null)
            {
                int currentHandCount = cardsInHand.Count;
                
                if (currentHandCount < lastPlayerHandCount)
                {
                    Log($"Player hand decreased from {lastPlayerHandCount} to {currentHandCount}");
                }
                
                lastPlayerHandCount = currentHandCount;
            }
        }
    }
    
    private void CheckEnemyCardPlays()
    {
        if (battleManager.enemyEntity != null)
        {
            CheckEnemyBoard(battleManager.enemyEntity);
        }
        
        if (battleManager.enemies != null && battleManager.enemies.Count > 0)
        {
            foreach (var enemy in battleManager.enemies)
            {
                if (enemy != null)
                {
                    CheckEnemyBoard(enemy);
                }
            }
        }
    }
    
    private void CheckEnemyBoard(EnemyEntity enemy)
    {
        // Placeholder - call OnCardPlayed() directly from EnemyAI
    }
    
    public void OnPlayerCardPlayed(CardData cardData)
    {
        if (displayBar == null || cardData == null)
            return;
        
        Log($"Player played: {cardData.cardName}");
        displayBar.AddCard(cardData);
    }
    
    public void OnEnemyCardPlayed(CardData cardData)
    {
        if (displayBar == null || cardData == null)
            return;
        
        Log($"Enemy played: {cardData.cardName}");
        displayBar.AddCard(cardData);
    }
    
    public void OnCardPlayed(CardData cardData, bool isPlayer)
    {
        if (isPlayer)
        {
            OnPlayerCardPlayed(cardData);
        }
        else
        {
            OnEnemyCardPlayed(cardData);
        }
    }
    
    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[CardPlayIntegration] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        if (showDebugLogs)
        {
            Debug.LogWarning($"[CardPlayIntegration] {message}");
        }
    }
}

