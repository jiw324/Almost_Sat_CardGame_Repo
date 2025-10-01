using System.Collections.Generic;

public class ShopNode : INode
{
    public NodeDefinition nodeDefinition { get; }
    public List<INode> parents { get; }
    public List<INode> children { get; }
    public NodeType nodeType { get; }
    public int depthIndex { get; }
    public int pathDepth { get; }
    public float nodeWeight { get; }

    public ShopNode(NodeDefinition nodeDefinition, int depthIndex, int pathDepth)
    {
        this.nodeDefinition = nodeDefinition;
        this.parents = new List<INode>();
        this.children = new List<INode>();
        this.nodeType = NodeType.Shop;
        this.nodeWeight = 0;
        this.depthIndex = depthIndex;
        this.pathDepth = pathDepth;
    }
}