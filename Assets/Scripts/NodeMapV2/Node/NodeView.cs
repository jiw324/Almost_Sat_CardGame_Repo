using UnityEngine;

public class NodeView : MonoBehaviour
{
    public Node NodeData { get; private set; }
    public NodeDefinition Definition => NodeData?.Definition;

    public void Initialize(Node node)
    {
        NodeData = node;
        gameObject.name = $"NodeView_{node.Id}_{node.Definition.nodeType}";
    }
}