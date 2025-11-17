using UnityEngine;

public class NodeBehaviour : MonoBehaviour
{
    [SerializeField] private NodeDefinition nodeDefinition;
    
    [Header("Combat Node Settings")]
    [SerializeField] private string enemyDefinitionName; // For combat nodes: name of EnemyDefinition in Resources/Enemies/
    [SerializeField] private bool useRandomEnemy = false; // If true, randomly selects from available enemies
    [SerializeField] private string[] randomEnemyPool; // Pool of enemy names to randomly select from

    public NodeDefinition definition => nodeDefinition;
    
    /// <summary>
    /// Gets the enemy definition name for this combat node.
    /// Returns null if not a combat node or no enemy assigned.
    /// </summary>
    public string GetEnemyDefinitionName()
    {
        if (nodeDefinition == null || nodeDefinition.nodeType != NodeType.Combat)
            return null;

        if (useRandomEnemy && randomEnemyPool != null && randomEnemyPool.Length > 0)
        {
            // Randomly select from pool
            return randomEnemyPool[Random.Range(0, randomEnemyPool.Length)];
        }

        return string.IsNullOrWhiteSpace(enemyDefinitionName) ? null : enemyDefinitionName;
    }
    
    /// <summary>
    /// Sets the enemy for this combat node.
    /// </summary>
    public void SetEnemy(string enemyDefName)
    {
        if (nodeDefinition != null && nodeDefinition.nodeType == NodeType.Combat)
        {
            enemyDefinitionName = enemyDefName;
            useRandomEnemy = false;
        }
    }
    
    /// <summary>
    /// Sets up this node to randomly select from an enemy pool when entered.
    /// </summary>
    public void SetRandomEnemyPool(string[] enemyPool)
    {
        if (nodeDefinition != null && nodeDefinition.nodeType == NodeType.Combat)
        {
            randomEnemyPool = enemyPool;
            useRandomEnemy = enemyPool != null && enemyPool.Length > 0;
        }
    }
}
