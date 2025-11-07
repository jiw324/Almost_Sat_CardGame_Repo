using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Deck Definition", fileName = "NewDeckDefinition")]
public class DeckDefinition : ScriptableObject
{
    [SerializeField] private List<string> cardIds = new();

    public IReadOnlyList<string> CardIds => cardIds;

}

