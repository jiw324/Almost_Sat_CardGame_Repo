using UnityEngine;

/// <summary>
/// Helper to handle loot node behavior: pick a random relic from JSON,
/// grant it to the player, and record it in the session's LootNodeData.
/// Call this when entering a loot node.
/// </summary>
public static class LootNodeDataInitializer
{
    public static void InitializeLootNode()
    {
        GameSession session = SessionGrabber.getGameSession();
        if (session == null)
        {
            Debug.LogError("[LootNodeDataInitializer] No GameSession found!");
            return;
        }

        if (RelicDatabase.Instance == null)
        {
            Debug.LogError("[LootNodeDataInitializer] No RelicDatabase instance in scene!");
            return;
        }

        if (!RelicDatabase.Instance.TryGetRandomRelic(out RelicJSON relicJson))
        {
            Debug.LogWarning("[LootNodeDataInitializer] No relics available in RelicDatabase.");
            return;
        }

        // Create runtime RelicData and add to player's relic list
        RelicData relicData = RelicDatabase.Instance.CreateRuntimeRelicData(relicJson);
        var playerRelics = session.gameSessionData.sessionPlayerData.relics;
        if (playerRelics == null)
        {
            session.gameSessionData.sessionPlayerData.relics = new System.Collections.Generic.List<RelicData>();
        }
        session.gameSessionData.sessionPlayerData.relics.Add(relicData);

        // Store simple node data for saving/loading
        LootNodeData lootData = new LootNodeData
        {
            grantedRelicId = relicJson.id
        };

        session.gameSessionData.sessionNodeMapData.currentNodeType = NodeType.Loot;
        session.gameSessionData.sessionNodeMapData.currentNodeData = lootData;

        Debug.Log($"[LootNodeDataInitializer] Granted relic '{relicJson.id}' to player.");
    }
}


