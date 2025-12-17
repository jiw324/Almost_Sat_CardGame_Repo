using UnityEngine;

/// <summary>
/// Helper script to initialize CombatNodeData when entering a combat node.
/// Attach this to a GameObject in the combat scene or call it from scene transition code.
/// </summary>
public static class CombatNodeDataInitializer
{
    /// <summary>
    /// Creates and assigns CombatNodeData to the current session.
    /// Call this when entering a combat node.
    /// </summary>
    /// <param name="enemyDefinitionName">Name of EnemyDefinition asset in Resources/Enemies/ (without extension)</param>
    public static void InitializeCombatNode(string enemyDefinitionName)
    {
        GameSession session = SessionGrabber.getGameSession();
        if (session == null)
        {
            Debug.LogError("[CombatNodeDataInitializer] No GameSession found!");
            return;
        }

        CombatNodeData combatData = new CombatNodeData
        {
            enemyDefinitionName = enemyDefinitionName
        };

        // Try to load enemy definition and populate data
        if (!string.IsNullOrWhiteSpace(enemyDefinitionName))
        {
            EnemyDefinition enemyDef = null;

            if (enemyDefinitionName.Contains("/"))
            {
                enemyDef = Resources.Load<EnemyDefinition>(enemyDefinitionName);
            }
            else
            {
                enemyDef = Resources.Load<EnemyDefinition>($"Enemies/{enemyDefinitionName}");
            }

            if (enemyDef != null)
            {
                combatData.enemyHealth = enemyDef.MaxHealth;
                combatData.enemyMana = enemyDef.StartingMana;
                combatData.enemyDeck = enemyDef.CreateDeckInstance();
                Debug.Log($"[CombatNodeDataInitializer] Loaded enemy '{enemyDefinitionName}' with {combatData.enemyHealth} HP, {combatData.enemyMana} mana, {combatData.enemyDeck.Cards.Count} cards.");
            }
            else
            {
                Debug.LogWarning($"[CombatNodeDataInitializer] Could not load EnemyDefinition '{enemyDefinitionName}'. Using default values.");
            }
        }

        // Assign to session
        session.gameSessionData.sessionNodeMapData.currentNodeType = NodeType.Combat;
        session.gameSessionData.sessionNodeMapData.currentNodeData = combatData;
    }

    /// <summary>
    /// Creates CombatNodeData with direct values (no EnemyDefinition).
    /// Use this if you want to set enemy stats manually.
    /// </summary>
    public static void InitializeCombatNodeDirect(int enemyHealth, int enemyMana, DeckInstance enemyDeck)
    {
        GameSession session = SessionGrabber.getGameSession();
        if (session == null)
        {
            Debug.LogError("[CombatNodeDataInitializer] No GameSession found!");
            return;
        }

        CombatNodeData combatData = new CombatNodeData
        {
            enemyHealth = enemyHealth,
            enemyMana = enemyMana,
            enemyDeck = enemyDeck ?? new DeckInstance()
        };

        session.gameSessionData.sessionNodeMapData.currentNodeType = NodeType.Combat;
        session.gameSessionData.sessionNodeMapData.currentNodeData = combatData;
        
        Debug.Log($"[CombatNodeDataInitializer] Created combat node with {enemyHealth} HP, {enemyMana} mana, {enemyDeck?.Cards.Count ?? 0} cards.");
    }
}

