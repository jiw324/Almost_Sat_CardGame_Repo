using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card")]
public class CardData : ScriptableObject
{
    public string id;
    public string cardName;
    public int cost;
    [TextArea] public string description;
    public string type;
    public Sprite artwork;
    public bool isRanged; 
    public CardEffect effect;

    [Serializable]
    public class EffectBinding
    {
        public CardEffect effect;
        public int value; // effectValue from JSON
    }

    public List<EffectBinding> effects = new List<EffectBinding>();

    [Header("Minion (leave off for spells)")]
    public bool isMinion = false;
    public int minionAttack = 2;
    public int minionHealth = 5;
    public string PrintCard()
    {
        return $"id: [{id}], name: [{cardName}], cost: [{cost}], type: [{type}], ranged: [{isRanged}], effects: [{effects?.Count ?? 0}]\ndescritpion: [{description}]";
    }
}
