using UnityEngine;
using UnityEngine.EventSystems;

public class DrawCard : MonoBehaviour, IPointerClickHandler
{
    [Header("Prefab & Parent")]
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private Transform handArea;

    [Header("Placement")]
    [SerializeField] private bool asFirstSibling = false;

    private void SpawnTestCard(string name)
    {
        var go = Instantiate(cardUIPrefab, handArea);
        go.name = name;
        var controller = go.GetComponent<CardUIController>();
        var rt = (RectTransform)go.transform;

        var data = ScriptableObject.CreateInstance<CardData>();
        data.cardName = name;
        data.cost = Random.Range(1, 5);
        data.isRanged = Random.value > 0.5f;
        data.description = $"This is a test card. Type: {(data.isRanged ? "Ranged" : "Melee")}";

        var instance = new CardInstance(data, null);
        controller.Initialize(instance);

        rt.anchoredPosition3D = Vector3.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;


        if (asFirstSibling) rt.SetAsFirstSibling(); else rt.SetAsLastSibling();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[DrawCard] Pointer click");
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (!cardUIPrefab || !handArea) return;

        string name = $"{cardUIPrefab.name} {handArea.childCount + 1}";
        SpawnTestCard(name);

    }
}
