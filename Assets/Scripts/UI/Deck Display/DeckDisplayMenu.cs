using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckDisplayMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform contentArea;   // parent container for cards (like handArea)
    [SerializeField] private GameObject cardUIPrefab; // same prefab used in HandManager
    [SerializeField] private PlayerEntity owner;   
    [SerializeField] private Button closeButton; 
    [SerializeField] private DeckDefinition deckDefinition;

    private DeckInstance deck;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        deck = SessionGrabber.getGameSession().GetPlayerDeck();
        //deck = new DeckInstance(deckDefinition.CardIds);
        Debug.Log(deck.Cards.Count + " cards loaded into DeckDisplayMenu.");
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Opens the deck menu and displays all cards from the given deck instance.
    /// </summary>
    public void Show()
    {
        Debug.Log(deck.Cards.Count + " cards loaded into DeckDisplayMenu.");
        if (deck == null || deck.Cards == null || deck.Cards.Count == 0)
        {
            Debug.LogWarning("[DeckDisplayMenu] No deck or cards to display.");
            return;
        }

        gameObject.SetActive(true);
        PopulateCards(deck.Cards);
    }

    public void Hide()
    {
        ClearCards();
        gameObject.SetActive(false);
    }

    private void PopulateCards(IReadOnlyList<CardId> cardIds)
    {
        ClearCards();

        foreach (CardId id in cardIds)
        {
            // create a CardInstance like HandManager does
            CardInstance card = CardFactory.CreateCard(id, owner);
            if (card == null)
                continue;

            GameObject cardObj = Instantiate(cardUIPrefab, contentArea);
            cardObj.name = card.Data.id.ToString();
            Debug.Log(cardObj.name + " instantiated in DeckDisplayMenu.");
            var controller = cardObj.GetComponent<CardUIController>();
            controller.Initialize(card);
            var visualRoot = cardObj.transform.Find("VisualRoot");
            if (visualRoot != null)
            {
                visualRoot.localScale = Vector3.one * 0.5f; // Half-size visually
            }
        }

        // Force Unity to refresh layout if using GridLayoutGroup or VerticalLayoutGroup
        if (contentArea is RectTransform rect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    private void ClearCards()
    {
        foreach (Transform child in contentArea)
            Destroy(child.gameObject);
    }
}
