using System.Collections.Generic;
using UnityEngine;

public interface INode
{
    string Id { get; } // Currently "gridX_gridY", upgrade to GUID later
    NodeDefinition Definition { get; }
    Vector2Int GridPos { get; }
    List<INode> NextNodes { get; }
    bool IsVisited { get; }
    bool IsCompleted { get; }
    void ConnectTo(INode other);
    void MarkVisited();
    void MarkCompleted();
    bool IsConnectedTo(INode other);
    void Reassign(NodeDefinition newDef);
}
