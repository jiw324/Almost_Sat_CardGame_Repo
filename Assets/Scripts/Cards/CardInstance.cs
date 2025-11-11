using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardInstance
{
    public CardData Data { get; private set; }

    // Runtime-specific values
    public bool IsInHand { get; private set; }
    public bool IsOnBoard { get; private set; }
    public EntityBase Owner { get; private set; }

    public int CurrentCost { get; private set; }
    public bool HasBeenPlayed { get; private set; }

    public event Action OnCardPlayed;
    public event Action OnCardDestroyed;

    public bool IsMinion => Data != null && Data.isMinion;
    public int CurrentHP { get; private set; }
    public int Attack => Data != null ? Data.minionAttack : 0;

    public CardInstance(CardData data, EntityBase owner)
    {
        Data = data;
        Owner = owner;
        CurrentCost = data.cost;
        IsInHand = true;
        IsOnBoard = false;

        if (IsMinion)
            CurrentHP = Data.minionHealth;
    }

    public void PlayCard(BoardSlot targetSlot)
    {
        if (HasBeenPlayed)
        {
            Debug.LogWarning($"[CardInstance] {Data.cardName} has already been played.");
            return;
        }

        IsInHand = false;
        IsOnBoard = true;
        HasBeenPlayed = true;
        
        // Maybe minigame play goes here

        if (!IsMinion) // SPELLS: execute spell effects only
            ResolveSpellEffects(Owner, targetSlot?.currentCard?.Owner);

        OnCardPlayed?.Invoke();
    }


    // SPELLS ONLY
    public void ResolveSpellEffects(EntityBase caster, EntityBase target)
        => Execute(Data?.effects, caster, target);

    // MINIONS ONLY
    public void ResolveMinionSummonEffects(EntityBase caster, EntityBase target)
        => Execute(Data?.onSummonBindings, caster, target);

    public void ResolveMinionDeathEffects()
        => Execute(Data?.onDeathBindings, Owner, null);

    private void Execute(List<CardData.EffectBinding> list, EntityBase caster, EntityBase target)
    {
        if (list == null || list.Count == 0) return;
        foreach (var b in list) if (b?.effect != null) b.effect.Execute(caster, target, b.value);
    }
    //public void ResolveEffect(EntityBase caster, EntityBase target)
    //{
    //    if (Data == null || Data.effects == null || Data.effects.Count == 0)
    //    {
    //        Debug.LogWarning($"[CardInstance] {Data?.cardName} has no effects assigned.");
    //        return;
    //    }

    //    foreach (var binding in Data.effects)
    //    {
    //        if (binding?.effect == null) continue;
    //        binding.effect.Execute(caster, target, binding.value);
    //    }
    //}
    //public void ResolveEffect(BoardSlot targetSlot)
    //{
    //    if (Data.effect != null)
    //    {
    //        EntityBase target = targetSlot?.currentCard?.Owner; // example target logic
    //        ResolveEffect(Owner, target);
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"[CardInstance] {Data.cardName} has no effect assigned.");
    //    }
    //}

    public void TakeDamage(int amount)
    {
        if (!IsMinion) return;
        amount = Mathf.Max(0, amount);
        CurrentHP -= amount;
        Debug.Log($"{Data.cardName} took {amount} dmg. HP={CurrentHP}");
    }

    public bool IsDead() => IsMinion && CurrentHP <= 0;

    public void DestroyCard()
    {
        IsOnBoard = false;
        OnCardDestroyed?.Invoke();
    }
}
