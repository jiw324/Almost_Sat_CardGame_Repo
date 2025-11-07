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

        // Try to get effect from library first
        var effect = CardEffectLibrary.GetEffectById(data.effectId);

        if (effect == null && !string.IsNullOrEmpty(data.effectId))
        {
            if (data.effectId == "damage")
            {
                var dmg = ScriptableObject.CreateInstance<DamageEffect>();
                if (data.id == "fireball") dmg.amount = 8;
                else if (data.id == "slash") dmg.amount = 4;
                else if (data.id == "warrior") dmg.amount = 2;
                else if (data.id == "archer") dmg.amount = 5;
                else dmg.amount = 0;
                effect = dmg;
            }
            else if (data.effectId == "heal")
            {
                var heal = ScriptableObject.CreateInstance<HealEffect>();
                if (data.id == "healinghands") heal.amount = 5;
                else heal.amount = 0;
                effect = heal;
            }
        }

        cardData.effect = effect;

        // Wrap it in a CardInstance
        return new CardInstance(cardData, owner);
    }
}
