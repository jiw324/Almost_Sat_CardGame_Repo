using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class CardDragHandler : MonoBehaviour,
	IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;

    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        originalParent = rectTransform.parent;
        originalPosition = rectTransform.anchoredPosition;

        // Move out of layout group for dragging, last sibling makes it drawn on top
        rectTransform.SetParent(canvas.transform, true);
        rectTransform.SetAsLastSibling();

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    //public void OnDrag(PointerEventData eventData)
    //{
    //    if (canvas == null) return;
    //    rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    //}

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        Vector2 localPoint;
        // Convert the screen mouse position to local position inside the canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        rectTransform.localPosition = localPoint;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        // Snap card back to hand if not placed on board
        if (rectTransform.parent == canvas.transform)
        {
            // card is from HandArea
            if (originalParent.GetComponent<HorizontalLayoutGroup>())
            {
                rectTransform.SetParent(originalParent, false);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)originalParent);
            }
            // card is from a board slot, return to slot or move to new one
            else
            {
                rectTransform.SetParent(originalParent, false);
                rectTransform.anchoredPosition = originalPosition;
            }
        }
    }
}

