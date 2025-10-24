using System;
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

        ResolveEffect(targetSlot);
        OnCardPlayed?.Invoke();
    }

    public void ResolveEffect(EntityBase caster, EntityBase target)
    {
        if (Data != null && Data.effect != null)
            Data.effect.Execute(caster, target);
    }
    public void ResolveEffect(BoardSlot targetSlot)
    {
        if (Data.effect != null)
        {
            EntityBase target = targetSlot?.currentCard?.Owner; // example target logic
            Data.effect.Execute(Owner, target);
        }
        else
        {
            Debug.LogWarning($"[CardInstance] {Data.cardName} has no effect assigned.");
        }
    }

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
