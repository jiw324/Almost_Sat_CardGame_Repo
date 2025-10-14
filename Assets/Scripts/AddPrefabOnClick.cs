using UnityEngine;
using UnityEngine.EventSystems;

public class AddPrefabOnClick : MonoBehaviour, IPointerClickHandler
{
    [Header("Prefab & Parent")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private RectTransform parent;

    [Header("Placement")]
    [SerializeField] private bool asFirstSibling = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Draw Card clicked");
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!prefab || !parent) return;

        Debug.Log("Instantiating Card");

        var go = Instantiate(prefab, parent);
        var rt = (RectTransform)go.transform;

        rt.anchoredPosition3D = Vector3.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;

        if (asFirstSibling) rt.SetAsFirstSibling(); else rt.SetAsLastSibling();

        go.name = $"{prefab.name} {parent.childCount}";
    }
}
