using System.Collections.Generic;
using System.Linq;
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

    [Header("Shop Data")]
    public List<CardId> cardsForSale;
    public List<int> cardPrices;

    [Header("Shop Settings")]
    [Tooltip("Number of items to display. Leave at 0 to show all items.")]
    public int itemsToDisplay = 3;
    [Tooltip("Randomize which items appear in the shop")]
    public bool randomizeItems = true;

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
        // Create list of indices
        List<int> indices = new List<int>();
        for (int i = 0; i < cardsForSale.Count; i++)
        {
            indices.Add(i);
        }

        // Randomize if enabled
        if (randomizeItems)
        {
            indices = indices.OrderBy(x => Random.value).ToList();
        }

        // Determine how many items to show
        int count = itemsToDisplay > 0 ? Mathf.Min(itemsToDisplay, cardsForSale.Count) : cardsForSale.Count;

        // Display the selected items
        for (int i = 0; i < count; i++)
        {
            int index = indices[i];
            CardId id = cardsForSale[index];
            int cost = cardPrices[index];

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
        var msm = FindFirstObjectByType<MapStateManager>();
        if (msm != null)
        {
            msm.MarkCompleted(msm.GetCurrentNode());
            msm.ReturnToMapScene();
        }
    }
}