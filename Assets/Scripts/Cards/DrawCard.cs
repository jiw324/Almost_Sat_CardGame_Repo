// DrawCard.cs ¡ª owner from BattleManager, no Object.FindObjectOfType
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DrawCard : MonoBehaviour, IPointerClickHandler
{
    [Header("Prefab & Parent")]
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private Transform handArea;

    [Header("Deck/Pool")]
    [SerializeField] private List<CardData> drawPool = new List<CardData>();

    [Header("Placement")]
    [SerializeField] private bool asFirstSibling = false;

    private void SpawnCard(CardData data)
    {
        if (!data || !cardUIPrefab || !handArea) return;

        var go = Instantiate(cardUIPrefab, handArea);
        go.name = data.cardName;

        var bm = BattleManager.Instance;
        var owner = bm ? bm.player : null; // <- no deprecated API

        var instance = new CardInstance(data, owner);

        var ui = go.GetComponent<CardUIController>();
        if (ui != null) ui.Initialize(instance);

        var rt = go.transform as RectTransform;
        if (rt != null)
        {
            if (asFirstSibling) rt.SetAsFirstSibling();
            else rt.SetAsLastSibling();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (drawPool == null || drawPool.Count == 0) return;

        int idx = Random.Range(0, drawPool.Count);
        SpawnCard(drawPool[idx]);
    }
}
