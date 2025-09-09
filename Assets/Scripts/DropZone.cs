using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        CardDragHandler card = eventData.pointerDrag?.GetComponent<CardDragHandler>();
        if (card == null) return;

        // Don't allow mulitple cards in one drop zone
        if (transform.childCount > 0)
        {
            Debug.Log($"Slot {name} is already occupied");
            return;
        }

        RectTransform cardRect = card.GetComponent<RectTransform>();

        // Set the card as a child of the drop zone
        cardRect.SetParent(transform, false);

        // Snap the card to the drop zone's center
        cardRect.SetParent(transform, false);
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.SetAsLastSibling();

        Debug.Log($"Card {card.name} dropped on {name}!");
    }
}
