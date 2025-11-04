using System.Collections.Generic;
using UnityEngine;

public static class CardFactory
{
    public static CardInstance CreateCard(string id, EntityBase owner)
    {
        CardJSON data = CardDatabase.Instance.GetCardById(id);
        if (data == null)
        {
            Debug.LogError($"[CardFactory] No card found with ID '{id}'");
            return null;
        }

        // Create a runtime CardData ScriptableObject
        CardData cardData = ScriptableObject.CreateInstance<CardData>();
        cardData.id = data.id;
        cardData.cardName = data.cardName;
        cardData.description = data.description;
        cardData.cost = data.cost;
        cardData.type = data.type;
        cardData.isRanged = data.isRanged;

        static void Fill(CardJSONEffectEntry[] src, List<CardData.EffectBinding> dst, string cardId)
        {
            if (src == null) return;
            foreach (var e in src)
            {
                if (e == null || string.IsNullOrEmpty(e.effectId)) continue;
                var so = CardEffectLibrary.GetEffectById(e.effectId);
                if (so == null) { Debug.LogWarning($"[CardFactory] Unknown effectId '{e.effectId}' on '{cardId}'"); continue; }
                dst.Add(new CardData.EffectBinding { effect = so, value = e.effectValue });
            }
        }

        if (!data.isMinion)
        {
            // SPELL PATH ONLY: populate 'effects'; ignore minion fields entirely
            if (data.effects != null && data.effects.Length > 0)
                Fill(data.effects, cardData.effects, data.id);
            else if (!string.IsNullOrEmpty(data.effectId))
                Fill(new[] { new CardJSONEffectEntry { effectId = data.effectId, effectValue = data.effectValue } }, cardData.effects, data.id);

            cardData.isMinion = false;
            return new CardInstance(cardData, owner);
        }

        // MINION PATH ONLY: populate stats + triggers; do NOT populate 'effects'
        cardData.isMinion = true;
        cardData.minionAttack = data.minionAttack;
        cardData.minionHealth = data.minionHealth;
        Fill(data.onSummon, cardData.onSummonBindings, data.id);
        Fill(data.onDeath, cardData.onDeathBindings, data.id);

        // Wrap it in a CardInstance
        return new CardInstance(cardData, owner);
    }
}
