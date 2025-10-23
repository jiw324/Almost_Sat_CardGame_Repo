using System.Collections.Generic;

public static class CardEffectLibrary
{
    private static Dictionary<string, CardEffect> effects = new();

    public static void RegisterEffect(string id, CardEffect effect)
    {
        effects[id] = effect;
    }

    public static CardEffect GetEffectById(string id)
    {
        return effects.TryGetValue(id, out var effect) ? effect : null;
    }
}
