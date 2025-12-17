using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplayItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardCostText;
    [SerializeField] private TextMeshProUGUI cardTypeText;
    [SerializeField] private Image backgroundImage;
    
    public CardInstance CardInstance { get; private set; }
    
    public void Initialize(CardInstance instance)
    {
        if (instance == null)
        {
            Debug.LogWarning("[CardDisplayItem] Cannot initialize with null card instance");
            return;
        }
        
        CardInstance = instance;
        UpdateVisuals(instance.Data);
    }
    
    public void InitializeWithCardData(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning("[CardDisplayItem] Cannot initialize with null card data");
            return;
        }
        
        CardInstance = null;
        UpdateVisualsFromCardData(cardData);
    }
    
    private void UpdateVisuals(CardData data)
    {
        if (data == null) return;
        
        if (cardNameText != null)
            cardNameText.text = data.cardName;
        
        if (cardCostText != null)
            cardCostText.text = $"Cost: {data.cost}";
        
        if (cardTypeText != null)
            cardTypeText.text = data.type;
    }
    
    private void UpdateVisualsFromCardData(CardData data)
    {
        if (data == null) return;
        
        if (cardNameText != null)
            cardNameText.text = data.cardName;
        
        if (cardCostText != null)
            cardCostText.text = $"Cost: {data.cost}";
        
        if (cardTypeText != null)
            cardTypeText.text = data.type;
    }
    
    public void SetHighlight(bool highlighted)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = highlighted ? 
                new Color(0.3f, 0.3f, 0.5f, 0.9f) : 
                new Color(0.2f, 0.2f, 0.2f, 0.9f);
        }
    }
}

