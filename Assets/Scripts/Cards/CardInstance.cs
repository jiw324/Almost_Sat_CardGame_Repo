using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public async Task PlayCardAsync(BoardSlot targetSlot, EntityBase spellTarget = null)
    {
        if (HasBeenPlayed)
        {
            Debug.LogWarning($"[CardInstance] {Data.cardName} has already been played.");
            return;
        }

        IsInHand = false;
        IsOnBoard = true;
        HasBeenPlayed = true;

        // Check if this card has a minigame
        float minigameMultiplier = 1.0f;
        if (Data.minigamePrefab != null && MinigameManager.Instance != null)
        {
            // Play minigame first, then resolve effects with the result
            minigameMultiplier = await MinigameManager.Instance.StartMinigameAsync(Data.minigamePrefab);
            Debug.Log($"[CardInstance] Minigame completed with multiplier: {minigameMultiplier:F2}");
        }

        // Resolve card effects with minigame multiplier
        ResolveCardWithMinigameResult(targetSlot, spellTarget, minigameMultiplier);
    }

    // Synchronous wrapper for backward compatibility (calls async version without awaiting)
    public void PlayCard(BoardSlot targetSlot, EntityBase spellTarget = null)
    {
        // Fire and forget - for places that can't await
        _ = PlayCardAsync(targetSlot, spellTarget);
    }

    private void ResolveCardWithMinigameResult(BoardSlot targetSlot, EntityBase spellTarget, float minigameMultiplier)
    {
        if (!IsMinion)
        {
            // SPELLS: Resolve effects with minigame multiplier
            EntityBase target = spellTarget ?? targetSlot?.currentCard?.Owner;
            ResolveSpellEffects(Owner, target, minigameMultiplier);
        }
        // MINIONS: Summon effects are handled separately in BoardManager after placement
        // Minigame support for minions can be added later if needed

        OnCardPlayed?.Invoke();
    }


    // SPELLS ONLY
    public void ResolveSpellEffects(EntityBase caster, EntityBase target, float minigameMultiplier = 1.0f)
    {
        Execute(Data?.effects, caster, target, minigameMultiplier);

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
    public void ResolveMinionSummonEffects(EntityBase caster, EntityBase target, float minigameMultiplier = 1.0f)
        => Execute(Data?.onSummonBindings, caster, target, minigameMultiplier);

    public void ResolveMinionDeathEffects()
        => Execute(Data?.onDeathBindings, Owner, null, 1.0f);

    private void Execute(List<CardData.EffectBinding> list, EntityBase caster, EntityBase target, float minigameMultiplier = 1.0f)
    {
        if (list == null || list.Count == 0) return;

        foreach (var b in list)
        {
            if (b?.effect == null) continue;

            var effectType = b.effect.GetType().Name;
            string casterName = caster != null ? caster.entityName ?? caster.name : "(none)";
            string targetName = target != null ? target.entityName ?? target.name : "(none)";

            // use sign of base value as a flag (e.g., negative => "hit everything"),
            // while applying the minigame multiplier to the magnitude.
            int baseValue = b.value;
            bool flagIsNegative = baseValue < 0;                 // e.g. negative => global AOE
            int magnitude = Mathf.Abs(baseValue);

            int adjustedMagnitude = Mathf.RoundToInt(magnitude * minigameMultiplier);
            int finalValue = flagIsNegative ? -adjustedMagnitude : adjustedMagnitude;

            Debug.Log($"[CardInstance] Executing effect {effectType} from card {Data?.cardName} by {casterName} targeting {targetName} value={finalValue} (base={baseValue}, multiplier={minigameMultiplier:F2})");
            b.effect.Execute(caster, target, finalValue);
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
