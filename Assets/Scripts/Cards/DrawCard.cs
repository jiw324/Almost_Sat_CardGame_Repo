using UnityEngine;
using UnityEngine.EventSystems;

public class DrawCard : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private HandManager handManager;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        handManager.DrawCardFromDeck();
    }
}
