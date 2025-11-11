using System.Collections.Generic;
using UnityEngine;

public class Node : INode
{
    public string Id { get; }
    public NodeDefinition Definition { get; private set; }
    public Vector2Int GridPos { get; }
    public List<INode> NextNodes { get; } = new();
    public bool IsVisited { get; private set; }
    public bool IsCompleted { get; private set; }

    public Node(NodeDefinition def, Vector2Int gridPos)
    {
        Definition = def;
        GridPos = gridPos;
        Id = $"{gridPos.x}_{gridPos.y}";
    }

    public void ConnectTo(INode other)
    {
        if (other != null && !NextNodes.Contains(other))
            NextNodes.Add(other);
    }
    public bool HasDefinition() => Definition != null;
    public bool IsConnectedTo(INode other) => NextNodes.Contains(other);
    public void MarkVisited() => IsVisited = true;
    public void MarkCompleted() => IsCompleted = true;
    public void Reassign(NodeDefinition newDef) => Definition = newDef;
}
