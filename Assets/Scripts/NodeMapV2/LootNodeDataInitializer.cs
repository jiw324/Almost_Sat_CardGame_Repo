using UnityEngine;

/// <summary>
/// Helper to handle loot node behavior: pick a random relic from JSON,
/// grant it to the player, and record it in the session's LootNodeData.
/// Call this when entering a loot node.
/// </summary>
public static class LootNodeDataInitializer
{
    /// <summary>
    /// Initialize loot for a node. If fromEvent is true, this loot came from an Event node.
    /// </summary>
    public static void InitializeLootNode(bool fromEvent)
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

        // If this loot node was already resolved in this session, don't grant again
        var mapData = session.gameSessionData.sessionNodeMapData;
        if (mapData != null &&
            mapData.currentNodeType == NodeType.Loot &&
            mapData.currentNodeData is LootNodeData existingLoot &&
            !string.IsNullOrEmpty(existingLoot.grantedRelicId))
        {
            Debug.Log("[LootNodeDataInitializer] Loot for this node was already granted, skipping.");
            return;
        }

        // Always grant the Torch relic on visiting a loot/event node
        if (!RelicDatabase.Instance.TryGetRelicById("torch", out RelicJSON relicJson))
        {
            Debug.LogWarning("[LootNodeDataInitializer] Torch relic not found in RelicDatabase.");
            return;
        }

        // Create runtime RelicData and add to player's relic list / bag
        RelicData relicData = RelicDatabase.Instance.CreateRuntimeRelicData(relicJson);

        // Prefer RelicBag component if present (so any UI based on RelicBag stays in sync)
        var bag = Object.FindFirstObjectByType<RelicBag>();
        if (bag != null)
        {
            bag.AddRelic(relicData);
        }
        else
        {
            var playerRelics = session.gameSessionData.sessionPlayerData.relics;
            if (playerRelics == null)
            {
                session.gameSessionData.sessionPlayerData.relics =
                    new System.Collections.Generic.List<RelicData>();
            }
            session.gameSessionData.sessionPlayerData.relics.Add(relicData);
        }

        // Store simple node data for saving/loading
        LootNodeData lootData = new LootNodeData
        {
            grantedRelicId = relicJson.id,
            fromEvent = fromEvent
        };

        session.gameSessionData.sessionNodeMapData.currentNodeType = NodeType.Loot;
        session.gameSessionData.sessionNodeMapData.currentNodeData = lootData;

        Debug.Log($"[LootNodeDataInitializer] Granted relic '{relicJson.id}' to player.");
    }
}


