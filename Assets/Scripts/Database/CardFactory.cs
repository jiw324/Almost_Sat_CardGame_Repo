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
 
        var bindings = new List<CardData.EffectBinding>();

        if (data.effects != null && data.effects.Length > 0)
        {
            foreach (var e in data.effects)
            {
                if (string.IsNullOrEmpty(e.effectId)) continue;
                var eff = CardEffectLibrary.GetEffectById(e.effectId);
                if (eff == null)
                {
                    Debug.LogWarning($"[CardFactory] Unknown effectId '{e.effectId}' in card '{data.id}'");
                    continue;
                }

                bindings.Add(new CardData.EffectBinding
                {
                    effect = eff,
                    value = e.effectValue
                });
            }
        }
        else if (!string.IsNullOrEmpty(data.effectId))
        {
            var eff = CardEffectLibrary.GetEffectById(data.effectId);
            if (eff == null)
            {
                Debug.LogWarning($"[CardFactory] Unknown effectId '{data.effectId}' in card '{data.id}'");
            }
            else
            {
                bindings.Add(new CardData.EffectBinding
                {
                    effect = eff,
                    value = data.effectValue
                });
            }
        }

        cardData.effects = bindings;

        // Wrap it in a CardInstance
        return new CardInstance(cardData, owner);
    }
}
