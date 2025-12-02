using UnityEngine;

[CreateAssetMenu(fileName = "CombatNodeDefinition", menuName = "Nodes/Definitions/Combat")]
public class CombatNodeDefinition : NodeDefinition
{
    [Header("Enemy Assignment")]
    [Tooltip("Name of EnemyDefinition asset in Resources/Enemies/ (leave empty to use random pool)")]
    public string enemyDefinitionName;
    
    [Tooltip("Pool of enemy names to randomly select from (used if enemyDefinitionName is empty)")]
    public string[] enemyPool;
    
    [Tooltip("If true, randomly selects from pool each time node is entered")]
    public bool useRandomPool = false;
}
