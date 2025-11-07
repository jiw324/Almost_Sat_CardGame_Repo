using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance { get; private set; }

    private Dictionary<string, CardJSON> cardLookup = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        LoadCards();
    }

    private void LoadCards()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "cards.json");
        string json = File.ReadAllText(path);
        CardJSONWrapper wrapper = JsonUtility.FromJson<CardJSONWrapper>(json);

        foreach (var card in wrapper.cards)
        {
            if (!cardLookup.ContainsKey(card.id))
                cardLookup.Add(card.id, card);
            else
                Debug.LogWarning($"Duplicate card ID found: {card.id}");
        }

        Debug.Log($"[CardDatabase] Loaded {cardLookup.Count} cards");
    }

    public CardJSON GetCardById(string id)
    {
        return cardLookup.TryGetValue(id, out var card) ? card : null;
    }

    public CardJSON GetRandomCard()
    {
        if (cardLookup.Count == 0) return null;
        int index = Random.Range(0, cardLookup.Count);
        foreach (var kvp in cardLookup)
            if (index-- == 0) return kvp.Value;
        return null;
    }
}
