using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class NodeMap
{
    private List<INode> nodes { get; }

    public Stack<INode> nodePath { get; private set; }

    public NodeMap(List<INode> nodes)
    {
        this.nodes = nodes;
        nodePath = createNodePath();
    }

    private Stack<INode> createNodePath()
    {
        nodePath = new Stack<INode>();
        foreach(INode node in nodes)
        {
            nodePath.Push(node);
        }

        return nodePath;
    }
}
