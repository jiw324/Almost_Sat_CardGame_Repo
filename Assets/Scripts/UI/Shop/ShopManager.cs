using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("References")]
    public TransactionManager transactionManager;
    public Transform shopContentArea;
    public GameObject shopCardPrefab;
    public Button continueButton;
    public GoldDisplay goldDisplay;
    public SceneSwitch sceneManager;

    [Header("Shop Data")]
    public List<CardId> cardsForSale;
    public List<int> cardPrices;

    private DeckInstance playerDeck;
    private int playerGold;

    private void Start()
    {
        var session = SessionGrabber.getGameSession();
        playerDeck = session.GetPlayerDeck();
        playerGold = session.GetPlayerGold();

        transactionManager.Initialize(playerDeck, playerGold);
        transactionManager.OnGoldChanged += OnGoldChanged;

        goldDisplay.UpdateGold(playerGold);

        BuildShop();

        continueButton.onClick.AddListener(TryFinishNode);
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        if (transactionManager != null)
        {
            transactionManager.OnGoldChanged -= OnGoldChanged;
        }
    }

    private void OnGoldChanged(int newGold)
    {
        Debug.Log($"OnGoldChanged called with {newGold}");
        goldDisplay.UpdateGold(newGold);
    }

    private void BuildShop()
    {
        for (int i = 0; i < cardsForSale.Count; i++)
        {
            CardId id = cardsForSale[i];
            int cost = cardPrices[i];

            GameObject cardObj = Instantiate(shopCardPrefab, shopContentArea);
            cardObj.transform.localScale = Vector3.one * 1.5f;
            ShopCardUI ui = cardObj.GetComponent<ShopCardUI>();
            ui.Initialize(id, cost, transactionManager);
        }
    }

    private void TryFinishNode()
    {
        if (!transactionManager.TryCommitChanges())
        {
            Debug.Log("Cannot continue: insufficient gold!");
            // Optional: Show error popup to player
            return;
        }

        Debug.Log("Purchases committed.");
        ExitNode();
    }

    private void ExitNode()
    {
        sceneManager.SceneChanger("Map");
    }
}