
using UnityEngine;

public class CardController : MonoBehaviour
{
    public CardInstance Instance { get; private set; }
    private BoardSlot selectedSlot;

    public void Initialize(CardInstance instance)
    {
        Instance = instance;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update text, art, cost, etc.
    }

    public void SetSelectedSlot(BoardSlot slot)
    {
        selectedSlot = slot;
    }

    public void OnClickPlay()
    {
        if (selectedSlot == null)
        {
            Debug.LogWarning("[CardController] No slot selected!");
            return;
        }

        if (BoardManager.Instance.TryPlaceCard(Instance, selectedSlot))
        {
            Debug.Log($"[CardController] Played {Instance.Data.cardName}!");
        }
        else
        {
            Debug.Log("[CardController] Invalid placement.");
        }
    }
}
