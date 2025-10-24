//using UnityEngine;
//using UnityEngine.EventSystems;

//public class DrawCard : MonoBehaviour, IPointerClickHandler
//{
//    [SerializeField] private HandManager handManager;
//    public void OnPointerClick(PointerEventData eventData)
//    {
//        if (eventData.button != PointerEventData.InputButton.Left) return;
//        handManager.DrawRandomCard();
//    }
//}

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

        var go = Object.Instantiate(cardUIPrefab, handArea);
        go.name = data.cardName;

        // Create a runtime instance; owner is the player by default
        var owner = Object.FindObjectOfType<PlayerEntity>();
        var instance = new CardInstance(data, owner);

        // Initialize UI
        var ui = go.GetComponent<CardUIController>();
        if (ui != null) ui.Initialize(instance);

        // Optional sibling order
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

        // Simple random draw from the pool
        int idx = Random.Range(0, drawPool.Count);
        SpawnCard(drawPool[idx]);
    }
}
