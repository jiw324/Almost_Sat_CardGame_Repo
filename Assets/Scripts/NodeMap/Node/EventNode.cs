using System.Collections.Generic;

public class EventNode : INode
{
    public List<INode> parents { get; }
    public List<INode> children { get; }
    public NodeType nodeType { get; }
    public int depthIndex { get; }
    public int pathDepth { get; }
    public float nodeWeight { get; }

    public EventNode(int depthIndex, int pathDepth)
    {
        this.parents = new List<INode>();
        this.children = new List<INode>();
        this.nodeType = NodeType.Event;
        this.nodeWeight = 0;
        this.depthIndex = depthIndex;
        this.pathDepth = pathDepth;
    }

    public List<INode> GetParents() { return parents; }
    public List<INode> GetChildren() { return children; }
    public NodeType GetNodeType() { return nodeType; }
    public float GetNodeWeight() { return nodeWeight; }
    public int GetDepthIndex() { return depthIndex; }
    public int GetPathDepth() { return pathDepth; }
}