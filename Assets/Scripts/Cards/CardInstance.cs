using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardInstance
{
    public CardData Data { get; private set; }
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

    public void PlayCard(BoardSlot targetSlot, EntityBase spellTarget = null)
    {
        if (HasBeenPlayed)
        {
            Debug.LogWarning($"[CardInstance] {Data.cardName} has already been played.");
            return;
        }

        IsInHand = false;
        IsOnBoard = true;
        HasBeenPlayed = true;
        
        if (!IsMinion)
        {
            EntityBase target = spellTarget ?? targetSlot?.currentCard?.Owner;
            ResolveSpellEffects(Owner, target);
        }

        OnCardPlayed?.Invoke();
    }


    // SPELLS ONLY
    public void ResolveSpellEffects(EntityBase caster, EntityBase target)
    {
        Execute(Data?.effects, caster, target);

        var minions = UnityEngine.Object.FindObjectsOfType<MinionEntity>();
        foreach (var m in minions)
        {
            if (m == null) continue;
            var mb = m.GetComponent<MinionBehaviour>();
            if (mb == null || mb.instance == null) continue;
            if (mb.instance.IsDead())
            {
                mb.Die();
            }
        }
    }

    // MINIONS ONLY
    public void ResolveMinionSummonEffects(EntityBase caster, EntityBase target)
        => Execute(Data?.onSummonBindings, caster, target);

    public void ResolveMinionDeathEffects()
        => Execute(Data?.onDeathBindings, Owner, null);

    private void Execute(List<CardData.EffectBinding> list, EntityBase caster, EntityBase target)
    {
        if (list == null || list.Count == 0) return;
        foreach (var b in list)
        {
            if (b?.effect == null) continue;
            var effectType = b.effect.GetType().Name;
            string casterName = caster != null ? caster.entityName ?? caster.name : "(none)";
            string targetName = target != null ? target.entityName ?? target.name : "(none)";
            Debug.Log($"[CardInstance] Executing effect {effectType} from card {Data?.cardName} by {casterName} targeting {targetName} value={b.value}");
            b.effect.Execute(caster, target, b.value);
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
