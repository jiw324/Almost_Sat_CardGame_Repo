using UnityEngine;

[CreateAssetMenu(fileName = "NodeDefinition", menuName = "Nodes/Node Definition")]
public class NodeDefinition : ScriptableObject
{
    public NodeType nodeType;
    public GameObject prefab;
    public string nodeSceneName;
}
