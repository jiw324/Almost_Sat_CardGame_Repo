using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapPositionManager : MonoBehaviour
{
    private NodeMap _map;
    private Dictionary<string, Node> _nodeLookup;
    private string _currentNodeId;

    private void Awake()
    {
        _map = MapGenerationManager.Instance.ActiveMap;

        if (_map == null)
        {
            Debug.LogError("MapPositionManager: No active map found. Did you call StartNewRun()?");
            return;
        }

        _nodeLookup = _map.AllNodes.ToDictionary(n => n.Id, n => n);

        // May be null on a brand new run — that's what we want
        _currentNodeId = MapGenerationManager.Instance.CurrentNodeId;
    }

    public Node GetCurrentNode()
    {
        if (string.IsNullOrEmpty(_currentNodeId))
            return null;

        if (_nodeLookup.TryGetValue(_currentNodeId, out var node))
            return node;

        return null;
    }

    public bool IsNodeInteractable(INode target)
    {
        if (_map == null || target == null)
            return false;

        Node targetNode = target as Node;
        if (targetNode == null)
            return false;

        Node current = GetCurrentNode();

        // No current node yet: first visit to map.
        // Only allow clicking nodes on the first floor (y == 0).
        if (current == null)
        {
            return targetNode.GridPos.y == 0;
        }

        // After a start node is chosen:
        // - The current node is always interactable
        if (targetNode.Id == current.Id)
            return true;

        // - Any direct child (NextNode) is interactable
        return current.NextNodes.Contains(targetNode);
    }

    /// <summary>
    /// Player clicked a node. Validate movement rules and update position.
    /// </summary>
    public bool TrySelectOrMoveToNode(INode target)
    {
        if (_map == null || target == null)
            return false;

        Node targetNode = target as Node;
        if (targetNode == null)
            return false;

        Node current = GetCurrentNode();
        var manager = MapGenerationManager.Instance;

        // CASE 1: First ever selection (no current node yet)
        if (current == null)
        {
            // Must pick a node on the first floor
            if (targetNode.GridPos.y != 0)
                return false;

            _currentNodeId = targetNode.Id;
            manager.SetCurrentNode(targetNode);
            manager.MarkVisited(targetNode);

            Debug.Log($"[MapPositionManager] Start node chosen: {targetNode.Id}");
            return true;
        }

        // CASE 2: Re-clicking the current node (allowed, but no movement)
        if (targetNode.Id == current.Id)
            return true;

        // CASE 3: Moving to a child node
        if (!current.NextNodes.Contains(targetNode))
            return false;

        _currentNodeId = targetNode.Id;
        manager.SetCurrentNode(targetNode);
        manager.MarkVisited(targetNode);

        Debug.Log($"[MapPositionManager] Moved from {current.Id} to {targetNode.Id}");
        return true;
    }
}
