using UnityEngine;
using UnityEngine.EventSystems;

public class DiscardCard : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform parent;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Discard Card clicked");
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!parent || parent.childCount == 0) return;

        Debug.Log("Deleting Card");
        Destroy(parent.GetChild(0).gameObject);
    }
}
