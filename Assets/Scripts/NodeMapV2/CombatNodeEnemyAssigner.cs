using UnityEngine;

/// <summary>
/// Utility class for assigning enemies to combat nodes.
/// Can be used to set up enemies when nodes are generated or manually assign them.
/// </summary>
public static class CombatNodeEnemyAssigner
{
    /// <summary>
    /// Assigns an enemy to a combat node by finding the NodeBehaviour component.
    /// </summary>
    /// <param name="nodeGameObject">The GameObject with NodeBehaviour component</param>
    /// <param name="enemyDefinitionName">Name of EnemyDefinition in Resources/Enemies/</param>
    public static void AssignEnemyToNode(GameObject nodeGameObject, string enemyDefinitionName)
    {
        if (nodeGameObject == null)
        {
            Debug.LogError("[CombatNodeEnemyAssigner] Node GameObject is null.");
            return;
        }

        NodeBehaviour nodeBehaviour = nodeGameObject.GetComponent<NodeBehaviour>();
        if (nodeBehaviour == null)
        {
            Debug.LogError($"[CombatNodeEnemyAssigner] No NodeBehaviour found on {nodeGameObject.name}.");
            return;
        }

        if (nodeBehaviour.definition.nodeType != NodeType.Combat)
        {
            Debug.LogWarning($"[CombatNodeEnemyAssigner] Node {nodeGameObject.name} is not a combat node.");
            return;
        }

        nodeBehaviour.SetEnemy(enemyDefinitionName);
        Debug.Log($"[CombatNodeEnemyAssigner] Assigned enemy '{enemyDefinitionName}' to node {nodeGameObject.name}.");
    }

    /// <summary>
    /// Assigns a random enemy from a pool to a combat node.
    /// </summary>
    /// <param name="nodeGameObject">The GameObject with NodeBehaviour component</param>
    /// <param name="enemyPool">Array of enemy definition names to choose from</param>
    public static void AssignRandomEnemyToNode(GameObject nodeGameObject, string[] enemyPool)
    {
        if (enemyPool == null || enemyPool.Length == 0)
        {
            Debug.LogError("[CombatNodeEnemyAssigner] Enemy pool is empty.");
            return;
        }

        string randomEnemy = enemyPool[Random.Range(0, enemyPool.Length)];
        AssignEnemyToNode(nodeGameObject, randomEnemy);
    }

    /// <summary>
    /// Sets up a node to randomly select from a pool when entered.
    /// </summary>
    /// <param name="nodeGameObject">The GameObject with NodeBehaviour component</param>
    /// <param name="enemyPool">Array of enemy definition names for the pool</param>
    public static void SetRandomEnemyPool(GameObject nodeGameObject, string[] enemyPool)
    {
        if (nodeGameObject == null)
        {
            Debug.LogError("[CombatNodeEnemyAssigner] Node GameObject is null.");
            return;
        }

        NodeBehaviour nodeBehaviour = nodeGameObject.GetComponent<NodeBehaviour>();
        if (nodeBehaviour == null)
        {
            Debug.LogError($"[CombatNodeEnemyAssigner] No NodeBehaviour found on {nodeGameObject.name}.");
            return;
        }

        nodeBehaviour.SetRandomEnemyPool(enemyPool);
        Debug.Log($"[CombatNodeEnemyAssigner] Set random enemy pool on {nodeGameObject.name} with {enemyPool.Length} enemies.");
    }
}

