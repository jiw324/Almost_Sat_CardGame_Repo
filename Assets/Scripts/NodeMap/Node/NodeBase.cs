public abstract class NodeBase : INode
{
    public NodeDefinition nodeDefinition { get; private set; }
    public NodeAnchor nodeAnchor { get; }
    public INode[] nextNodes { get; } = new INode[3];

    public NodeType Type => nodeDefinition.nodeType;

    protected NodeBase(NodeDefinition def, NodeAnchor anchor)
    {
        nodeDefinition = def;
        nodeAnchor = anchor;
    }

    public void ReassignDefinition(NodeDefinition newDefinition)
    {
        nodeDefinition = newDefinition;
    }

    public virtual void AddNextNode(INode node, int xDelta)
    {
        if (xDelta >= -1 && xDelta <= 1 && nextNodes[xDelta + 1] == null)
        {
            nextNodes[xDelta + 1] = node;
        }
    }
}
