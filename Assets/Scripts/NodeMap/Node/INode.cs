public interface INode
{
    NodeDefinition nodeDefinition { get; }
    NodeAnchor nodeAnchor { get; }
    INode[] nextNodes { get; }
    void AddNextNode(INode node, int xDelta);
}
