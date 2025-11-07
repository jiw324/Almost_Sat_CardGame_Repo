using System;
using System.Collections.Generic;

[Serializable]
public class DeckInstance
{
    public List<string> cardIds = new();

    public IReadOnlyList<string> Cards => cardIds;

    public DeckInstance()
    {
    }

    public DeckInstance(IEnumerable<string> sourceIds)
    {
        if (sourceIds == null) return;
        cardIds = new List<string>(sourceIds);
    }

    public void AddCard(string cardId)
    {
        if (string.IsNullOrWhiteSpace(cardId))
            return;
        cardIds.Add(cardId);
    }

    public bool RemoveCard(string cardId)
    {
        if (string.IsNullOrWhiteSpace(cardId))
            return false;
        return cardIds.Remove(cardId);
    }

    public void Clear()
    {
        cardIds.Clear();
    }

    public DeckInstance Clone()
    {
        return new DeckInstance(cardIds);
    }
}

