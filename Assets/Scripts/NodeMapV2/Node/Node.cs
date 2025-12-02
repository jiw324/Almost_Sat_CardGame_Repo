using System.Collections.Generic;
using UnityEngine;

public class Node : INode
{
    public string Id { get; }
    public NodeDefinition Definition { get; private set; }
    public Vector2Int GridPos { get; }
    public List<Node> ParentNodes { get; } = new List<Node>();
    public List<Node> NextNodes { get; } = new List<Node>();
    public bool IsVisited { get; private set; }
    public bool IsCompleted { get; private set; }
    
    // Per-node enemy assignment (for combat nodes)
    public string AssignedEnemyName { get; set; }

    public Node(NodeDefinition def, Vector2Int gridPos)
    {
        Definition = def;
        GridPos = gridPos;
        Id = $"{gridPos.x}_{gridPos.y}";
    }
    public void ConnectTo(Node child)
    {
        if (child == null) return;

        if (!NextNodes.Contains(child))
            NextNodes.Add(child);

        if (!child.ParentNodes.Contains(this))
            child.ParentNodes.Add(this);
    }
    public void DisconnectFrom(Node child)
    {
        if (child == null) return;

        NextNodes.Remove(child);
        child.ParentNodes.Remove(this);
    }
    public bool HasDefinition() => Definition != null;
    public bool IsConnectedTo(Node other) => NextNodes.Contains(other);
    public void MarkVisited() => IsVisited = true;
    public void MarkCompleted() => IsCompleted = true;
    public void Reassign(NodeDefinition newDef) => Definition = newDef;
}
