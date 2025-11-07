using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform handArea;     // parent for card UI prefabs
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private PlayerEntity owner;     // who this hand belongs to

    [Header("Settings")]
    [SerializeField] private int maxHandSize = 10;

    private readonly List<CardInstance> cardsInHand = new();

    private void Start()
    {
        if (handArea == null)
            Debug.LogError("[HandManager] Missing handArea reference!");
        if (cardUIPrefab == null)
            Debug.LogError("[HandManager] Missing cardUIPrefab reference!");
        if (owner == null)
            Debug.LogWarning("[HandManager] Owner not set — using PlayerEntity in scene?");
    }

    // ---- Public methods ----

    // TODO: Fix with the actual "deck" later
    public void DrawRandomCard()
    {
        if (cardsInHand.Count >= maxHandSize)
        {
            Debug.Log("[HandManager] Hand full!");
            return;
        }

        CardJSON randomData = CardDatabase.Instance.GetRandomCard();
        if (randomData == null)
        {
            Debug.LogWarning("[HandManager] No cards found in database!");
            return;
        }

        CardInstance newCard = CardFactory.CreateCard(randomData.id, owner);
        AddCardToHand(newCard);
    }

    public void AddCardToHand(CardInstance card)
    {
        if (card == null) return;
        cardsInHand.Add(card);
        Debug.Log($"[HandManager] Added {card.Data.name} to hand.\n{card.Data.PrintCard()}");

        GameObject go = Instantiate(cardUIPrefab, handArea);
        go.name = card.Data.id;
        var controller = go.GetComponent<CardUIController>();
        controller.Initialize(card);

        // layout groups handle positioning
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
    }

    public void RemoveCardFromHand(CardInstance card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
        }
    }

    //TODO: temp for testing
    public void RemoveTopCard()
    {
        if (cardsInHand.Count == 0) return;

        cardsInHand.RemoveAt(cardsInHand.Count - 1);
        Destroy(handArea.GetChild(0).gameObject);
    }

    public void ClearHand()
    {
        cardsInHand.Clear();
        foreach (Transform child in handArea)
            Destroy(child.gameObject);
    }

    // Add inside HandManager class
    public void RemoveByInstance(CardInstance instance)
    {
        if (instance == null) return;
        int idx = cardsInHand.IndexOf(instance);
        if (idx >= 0)
        {
            cardsInHand.RemoveAt(idx);
        }
    }
}
