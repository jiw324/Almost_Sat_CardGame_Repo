using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopCardUI : MonoBehaviour
{
    [Header("References")]
    public TMP_Text priceText;
    public Button clickArea;
    public CardUIController visual;

    [Header("Visual States")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.green;

    private TransactionManager transactionManager;
    private CardId cardId;
    private int cost;
    private bool isSelected = false;
    private Image borderImage;

    public void Initialize(CardId id, int price, TransactionManager mgr)
    {
        cardId = id;
        cost = price;
        transactionManager = mgr;

        priceText.text = price.ToString();

        // Create a temp CardInstance for display only
        CardInstance tempCard = CardFactory.CreateCard(id, null);
        visual.Initialize(tempCard);

        // Get the border image from the button's target graphic
        borderImage = clickArea.targetGraphic as Image;

        clickArea.onClick.AddListener(OnClick);

        UpdateVisualState();
    }

    private void OnClick()
    {
        Debug.Log($"Card clicked: {cardId}, Current state: {isSelected}");
        bool success = transactionManager.TogglePurchase(cardId, cost);

        if (success)
        {
            isSelected = !isSelected;
            Debug.Log($"Toggle successful. New state: {isSelected}");
            UpdateVisualState();
        }
        else
        {
            Debug.Log($"Cannot toggle card {cardId} - insufficient gold");
        }
    }

    private void UpdateVisualState()
    {
        if (borderImage != null)
        {
            borderImage.color = isSelected ? selectedColor : normalColor;
        }
    }
}