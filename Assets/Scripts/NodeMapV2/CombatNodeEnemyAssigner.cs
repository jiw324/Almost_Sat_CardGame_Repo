using UnityEngine;

/// <summary>
/// Utility class for assigning enemies to combat nodes
/// Used to set up enemies when nodes are generated
/// </summary>
public static class CombatNodeEnemyAssigner
{
    /// <summary>
    /// Assigns an enemy to a combat node by storing it in the node's per-node assignment.
    /// </summary>
    /// <param name="node">The Node instance (data model)</param>
    /// <param name="enemyDefinitionName">Name of EnemyDefinition in Resources/Enemies/</param>
    public static void AssignEnemyToNode(Node node, string enemyDefinitionName)
    {
        if (node == null)
        {
            Debug.LogError("[CombatNodeEnemyAssigner] Node is null.");
            return;
        }

        if (node.Definition == null || node.Definition.nodeType != NodeType.Combat)
        {
            Debug.LogWarning($"[CombatNodeEnemyAssigner] Node {node.Id} is not a combat node.");
            return;
        }

        CombatNodeDefinition combatDef = node.Definition as CombatNodeDefinition;
        if (combatDef == null)
        {
            Debug.LogError($"[CombatNodeEnemyAssigner] Node {node.Id} definition is not a CombatNodeDefinition.");
            return;
        }

        node.AssignedEnemyName = enemyDefinitionName;
        Debug.Log($"[CombatNodeEnemyAssigner] Assigned enemy '{enemyDefinitionName}' to node {node.Id}.");
    }

    /// <summary>
    /// Assigns a random enemy from a pool to a combat node.
    /// </summary>
    /// <param name="node">The Node instance (data model)</param>
    /// <param name="enemyPool">Array of enemy definition names to choose from</param>
    public static void AssignRandomEnemyToNode(Node node, string[] enemyPool)
    {
        if (enemyPool == null || enemyPool.Length == 0)
        {
            Debug.LogError("[CombatNodeEnemyAssigner] Enemy pool is empty.");
            return;
        }

        string randomEnemy = enemyPool[Random.Range(0, enemyPool.Length)];
        AssignEnemyToNode(node, randomEnemy);
    }

    /// <summary>
    /// Sets up a node to randomly select from a pool when entered.
    /// </summary>
    /// <param name="node">The Node instance (data model)</param>
    /// <param name="enemyPool">Array of enemy definition names for the pool</param>
    public static void SetRandomEnemyPool(Node node, string[] enemyPool)
    {
        if (node == null)
        {
            Debug.LogError("[CombatNodeEnemyAssigner] Node is null.");
            return;
        }

        if (node.Definition == null || node.Definition.nodeType != NodeType.Combat)
        {
            Debug.LogWarning($"[CombatNodeEnemyAssigner] Node {node.Id} is not a combat node.");
            return;
        }

        CombatNodeDefinition combatDef = node.Definition as CombatNodeDefinition;
        if (combatDef == null)
        {
            Debug.LogError($"[CombatNodeEnemyAssigner] Node {node.Id} definition is not a CombatNodeDefinition.");
            return;
        }

        combatDef.enemyPool = enemyPool;
        combatDef.useRandomPool = true;
        Debug.Log($"[CombatNodeEnemyAssigner] Set random enemy pool on node {node.Id} with {enemyPool.Length} enemies.");
    }

    /// <summary>
    /// Gets the enemy definition name for a combat node, handling random pools if configured.
    /// Priority: 1) Per-node assignment, 2) Random pool, 3) Definition's enemyDefinitionName
    /// </summary>
    /// <param name="node">The Node instance</param>
    /// <returns>Enemy definition name, or null if not configured</returns>
    public static string GetEnemyDefinitionName(Node node)
    {
        if (node == null || node.Definition == null || node.Definition.nodeType != NodeType.Combat)
            return null;

        // Check per-node assignment first (highest priority)
        if (!string.IsNullOrWhiteSpace(node.AssignedEnemyName))
        {
            return node.AssignedEnemyName;
        }

        CombatNodeDefinition combatDef = node.Definition as CombatNodeDefinition;
        if (combatDef == null)
            return null;

        // Check random pool
        if (combatDef.useRandomPool && combatDef.enemyPool != null && combatDef.enemyPool.Length > 0)
        {
            return combatDef.enemyPool[Random.Range(0, combatDef.enemyPool.Length)];
        }

        // Fall back to definition's enemyDefinitionName
        return combatDef.enemyDefinitionName;
    }
}

