public interface INode
{
    NodeDefinition nodeDefinition { get; }
    NodeAnchor nodeAnchor { get; }
    INode[] nextNodes { get; }

    void AddNextNode(INode node, int deltaX);
    void ReassignDefinition(NodeDefinition newDefinition);
    NodeType Type { get; }
}
