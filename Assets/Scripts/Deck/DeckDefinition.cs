using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Deck Definition", fileName = "NewDeckDefinition")]
public class DeckDefinition : ScriptableObject
{
    [SerializeField] private List<CardId> cardIds = new();

    public IReadOnlyList<CardId> CardIds => cardIds;

}

