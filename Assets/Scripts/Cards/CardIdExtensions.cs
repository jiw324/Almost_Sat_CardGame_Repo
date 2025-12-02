using System;

public static class CardIdExtensions
{
    public static bool TryParse(string rawId, out CardId cardId)
    {
        if (string.IsNullOrWhiteSpace(rawId))
        {
            cardId = default;
            return false;
        }

        return Enum.TryParse(rawId, true, out cardId);
    }

    public static string ToPersistentId(this CardId cardId)
    {
        return cardId.ToString().ToLowerInvariant();
    }
}

