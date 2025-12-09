using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card")]
public class CardData : ScriptableObject
{
    public CardId id;
    public string cardName;
    public int cost;
    [TextArea] public string description;
    public string type;
    public Sprite artwork;
    public bool isRanged; 
    public CardEffect effect;
    public int damage;

    [Serializable]
    public class EffectBinding
    {
        public CardEffect effect;
        public int value; 
    }

    public List<EffectBinding> effects = new List<EffectBinding>();

    [Header("Minion (leave off for spells)")]
    public bool isMinion = false;
    public int minionAttack = 2;
    public int minionHealth = 5;
    public bool ignoreSummoningSickness = false; // If true, minion can attack immediately on the turn it's summoned
    public List<EffectBinding> onSummonBindings = new List<EffectBinding>();
    public List<EffectBinding> onDeathBindings = new List<EffectBinding>();
    
    [Header("Minigame")]
    public GameObject minigamePrefab;  // Optional: Prefab for the minigame to play when this card is played
    public bool hasMinigame;

    public string PrintCard()
    {
        return $"id: [{id}], name: [{cardName}], cost: [{cost}], type: [{type}], ranged: [{isRanged}], effects: [{effects?.Count ?? 0}]\ndescritpion: [{description}]";
    }
}
