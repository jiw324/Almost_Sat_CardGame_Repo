using System.Collections.Generic;
using System.Diagnostics;

public abstract class NodeBase : INode
{
    public NodeDefinition nodeDefinition { get; }
    public NodeAnchor nodeAnchor { get; }
    public INode[] nextNodes { get; }

    private const int maxNextNodes = 3;

    protected NodeBase(NodeDefinition nodeDefinition, NodeAnchor nodeAnchor)
    {
        this.nodeDefinition = nodeDefinition;
        this.nodeAnchor = nodeAnchor;
        nextNodes = new INode[maxNextNodes];
    }

    public virtual void AddNextNode(INode node, int xDelta)
    {
        if (xDelta >= -1 && xDelta <= 1 && nextNodes[xDelta + 1] == null)
        {
            nextNodes[xDelta + 1] = node;
        }
    }
}
