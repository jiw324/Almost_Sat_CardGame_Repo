using System.Collections.Generic;
using UnityEngine;

public interface INode
{
    string Id { get; } // Currently "gridX_gridY", upgrade to GUID later
    NodeDefinition Definition { get; }
    Vector2Int GridPos { get; }
    List<Node> NextNodes { get; }
    List<Node> ParentNodes { get; }
    bool IsVisited { get; }
    bool IsCompleted { get; }
    void ConnectTo(Node other);
    void DisconnectFrom(Node other);
    bool HasDefinition();
    void MarkVisited();
    void MarkCompleted();
    bool IsConnectedTo(Node other);
    void Reassign(NodeDefinition newDef);
}
