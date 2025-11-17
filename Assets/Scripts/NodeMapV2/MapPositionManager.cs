using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapPositionManager : MonoBehaviour
{
    private NodeMap _map;
    private string _currentNodeId;

    // Cache lookup for performance
    private Dictionary<string, Node> _nodeLookup;

    private void Awake()
    {
        // Get active map from generation manager
        _map = MapGenerationManager.Instance.ActiveMap;

        if (_map == null)
        {
            Debug.LogError("MapPositionManager: No active map found. Did you call StartNewRun()?");
            return;
        }

        _nodeLookup = _map.AllNodes.ToDictionary(n => n.Id, n => n);

        // Restore state if loading a save
        _currentNodeId = MapGenerationManager.Instance.CurrentNodeId;

        if (string.IsNullOrEmpty(_currentNodeId))
            _currentNodeId = AutoFindStartNode();
    }

    private string AutoFindStartNode()
    {
        if (!_map.Floors.ContainsKey(0) || _map.Floors[0].Count == 0)
            return null;

        return _map.Floors[0][0].Id;
    }

    public Node GetCurrentNode()
    {
        if (_nodeLookup.TryGetValue(_currentNodeId, out var node))
            return node;

        return null;
    }

    public bool IsNodeInteractable(INode target)
    {
        if (target == null)
            return false;

        Node current = GetCurrentNode();
        if (current == null)
            return false;

        // Same node (always interactable)
        if (target.Id == current.Id)
            return true;

        // Can only move to nodes that are "next nodes" from current
        return current.NextNodes.Contains(target as Node);
    }

    /// <summary>
    /// Player clicked a node. Validate movement rules and update position.
    /// </summary>
    public bool TrySelectOrMoveToNode(INode target)
    {
        if (target == null)
            return false;

        Node current = GetCurrentNode();
        Node targetNode = target as Node;

        // Clicking same node — allowed, but no movement
        if (targetNode.Id == current.Id)
            return true;

        // Must be directly reachable (current ? target)
        if (!current.NextNodes.Contains(targetNode))
            return false;

        // Movement is allowed!
        MoveToNode(targetNode);
        return true;
    }

    private void MoveToNode(Node node)
    {
        _currentNodeId = node.Id;

        // Update visited status in both the node and the manager system
        MapGenerationManager.Instance.MarkVisited(node);

        // Eventual spot for animating a "map token" moving between nodes
        Debug.Log($"Player moved to node {node.Id} ({node.Definition.nodeType})");
    }

    /// <summary>
    /// Helper: check if target is the next immediate floor
    /// </summary>
    private bool IsNextFloor(Node current, Node target)
    {
        return target.GridPos.y == current.GridPos.y + 1;
    }
}
