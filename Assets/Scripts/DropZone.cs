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

        if (transform.childCount > 0) return;

        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.SetParent(transform, false);
        cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;

        var playable = card.GetComponent<PlayableCard>();
        playable?.Play();
    }
}
