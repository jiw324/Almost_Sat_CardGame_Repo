using UnityEngine;

public class NodeBehaviour : MonoBehaviour
{
    [SerializeField] private NodeDefinition nodeDefinition;

    public NodeDefinition definition => nodeDefinition;
}
