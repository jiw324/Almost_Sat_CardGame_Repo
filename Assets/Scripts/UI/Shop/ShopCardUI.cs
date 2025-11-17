using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopCardUI : MonoBehaviour
{
    [Header("References")]
    public TMP_Text priceText;
    public Toggle toggle;
    public CardUIController visual;

    private TransactionManager transactionManager;
    private CardId cardId;
    private int cost;

    public void Initialize(CardId id, int price, TransactionManager mgr)
    {
        cardId = id;
        cost = price;
        transactionManager = mgr;

        priceText.text = price.ToString();

        // Create a temp CardInstance for display only
        CardInstance tempCard = CardFactory.CreateCard(id, null);
        visual.Initialize(tempCard);

        toggle.onValueChanged.AddListener(OnToggle);
    }

    private void OnToggle(bool isOn)
    {
        transactionManager.TogglePurchase(cardId, cost);
    }
}
