using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplayBar : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private RectTransform cardContainer;
    [SerializeField] private GameObject cardDisplayPrefab;
    
    [Header("Visuals")]
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    [SerializeField] private Image backgroundImage;
    
    private List<CardDisplayItem> displayedCards = new List<CardDisplayItem>();
    
    private void Start()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
    }
    
    public void AddCard(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[CardDisplayBar] Cannot add null CardData");
            return;
        }
        
        AddCardData(cardData);
    }
    
    private void AddCardData(CardData cardData)
    {
        if (cardData == null || cardDisplayPrefab == null || cardContainer == null)
        {
            Debug.LogWarning("[CardDisplayBar] Cannot add card - missing references");
            return;
        }
        
        GameObject cardObj = Instantiate(cardDisplayPrefab, cardContainer);
        CardDisplayItem displayItem = cardObj.GetComponent<CardDisplayItem>();
        
        if (displayItem != null)
        {
            displayItem.InitializeWithCardData(cardData);
            displayedCards.Add(displayItem);
            UpdateLayout();
        }
        else
        {
            Debug.LogError("[CardDisplayBar] CardDisplayItem component not found on prefab");
            Destroy(cardObj);
        }
    }
    
    public void RemoveCard(CardDisplayItem item)
    {
        if (item != null && displayedCards.Contains(item))
        {
            displayedCards.Remove(item);
            Destroy(item.gameObject);
            UpdateLayout();
        }
    }
    
    public void ClearAllCards()
    {
        foreach (var item in displayedCards)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        displayedCards.Clear();
        UpdateLayout();
    }
    
    private void UpdateLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer);
    }
    
    public int GetCardCount()
    {
        return displayedCards.Count;
    }
    
    public List<CardDisplayItem> GetDisplayedCards()
    {
        return new List<CardDisplayItem>(displayedCards);
    }
}

