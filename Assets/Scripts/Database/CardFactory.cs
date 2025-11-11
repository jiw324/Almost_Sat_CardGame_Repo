using UnityEngine;

public static class CardFactory
{
    public static CardInstance CreateCard(CardId id, EntityBase owner)
    {
        CardJSON data = CardDatabase.Instance.GetCardById(id);
        if (data == null)
        {
            Debug.LogError($"[CardFactory] No card found with ID '{id}'");
            return null;
        }

        // Create a runtime CardData ScriptableObject
        CardData cardData = ScriptableObject.CreateInstance<CardData>();
        cardData.id = id;
        cardData.cardName = data.cardName;
        cardData.description = data.description;
        cardData.cost = data.cost;
        cardData.type = data.type;
        cardData.isRanged = data.isRanged;
        cardData.effect = CardEffectLibrary.GetEffectById(data.effectId);
        cardData.damage = data.damage;

        // Wrap it in a CardInstance
        return new CardInstance(cardData, owner);
    }
}
