using System;
using System.Collections.Generic;

[Serializable]
public class DeckInstance
{
    public List<CardId> cardIds = new();

    public IReadOnlyList<CardId> Cards => cardIds;

    public DeckInstance()
    {
    }

    public DeckInstance(IEnumerable<CardId> sourceIds)
    {
        if (sourceIds == null) return;
        cardIds = new List<CardId>(sourceIds);
    }

    public void AddCard(CardId cardId)
    {
        cardIds.Add(cardId);
    }

    public bool RemoveCard(CardId cardId)
    {
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

