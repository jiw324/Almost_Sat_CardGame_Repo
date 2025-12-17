using UnityEngine;

public class CardPlayHistoryDisplay : MonoBehaviour
{
    [SerializeField] private CardDisplayBar displayBar;
    [SerializeField] private GameObject cardDisplayPrefab;
    
    private int currentTurnNumber = 0;
    
    private void Start()
    {
        SubscribeToTurnEvents();
    }
    
    private void SubscribeToTurnEvents()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStarted += () => currentTurnNumber++;
        }
    }
    
    public void OnPlayerPlayedCard(CardData cardData)
    {
        if (displayBar != null && cardData != null)
        {
            displayBar.AddCard(cardData);
            Debug.Log($"[PlayHistory] Player played: {cardData.cardName} on turn {currentTurnNumber}");
        }
    }
    
    public void OnEnemyPlayedCard(CardData cardData)
    {
        if (displayBar != null && cardData != null)
        {
            displayBar.AddCard(cardData);
            Debug.Log($"[PlayHistory] Enemy played: {cardData.cardName} on turn {currentTurnNumber}");
        }
    }
    
    private int GetCurrentTurn()
    {
        return currentTurnNumber;
    }
}

