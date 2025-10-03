using System.Collections.Generic;

public class LootNode : INode
{
    public NodeDefinition nodeDefinition { get; }
    public List<INode> parents { get; }
    public List<INode> children { get; }
    public NodeType nodeType { get; }
    public int depthIndex { get; }
    public int pathDepth { get; }
    public float nodeWeight { get; }

    public LootNode(NodeDefinition nodeDefinition, int depthIndex, int pathDepth)
    {
        this.nodeDefinition = nodeDefinition;
        this.parents = new List<INode>();
        this.children = new List<INode>();
        this.nodeType = NodeType.Loot;
        this.nodeWeight = 0;
        this.depthIndex = depthIndex;
        this.pathDepth = pathDepth;
    }
}