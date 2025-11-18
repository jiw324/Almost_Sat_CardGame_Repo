using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapPositionManager : MonoBehaviour
{
    private NodeMap _nodeMap;
    private INode _currentNode;
    private List<INode> _reachableNodes = new List<INode>();

    public INode CurrentNode => _currentNode;
    public IReadOnlyList<INode> ReachableNodes => _reachableNodes;

    public void Initialize(NodeMap nodeMap)
    {
        _nodeMap = nodeMap;
        _currentNode = null;

        LogStarterNodes();
    }

    private void LogStarterNodes()
    {
        if (!_nodeMap.Floors.ContainsKey(0))
        {
            Debug.LogError("NodeMap does not contain a first row (floor 0).");
            return;
        }

        Debug.Log("Starter nodes (first row):");
        foreach (var node in _nodeMap.Floors[0])
        {
            if (node != null)
                Debug.Log($" - Node at (x={node.GridPos.x}, y=0)");
        }
    }

    public void SelectStarterNode(INode starterNode)
    {
        if (!_nodeMap.Floors.ContainsKey(0) || !_nodeMap.Floors[0].Any(n => n == starterNode))
        {
            Debug.LogWarning("Attempted to select a starter node not in first row.");
            return;
        }

        _currentNode = starterNode;
        var (x, y) = GetNodeCoords(_currentNode);
        Debug.Log($"Selected starting node = Node ({x}, {y})");

        UpdateReachableNodes();
    }

    private void UpdateReachableNodes()
    {
        _reachableNodes.Clear();

        if (_currentNode == null)
            return;

        foreach (var next in _currentNode.NextNodes)
        {
            if (next != null)
                _reachableNodes.Add(next);
        }

        DebugReachableNodes();
    }

    private void DebugReachableNodes()
    {
        if (_reachableNodes.Count == 0)
        {
            Debug.Log("No reachable nodes from current node.");
            return;
        }

        Debug.Log("Reachable nodes:");
        foreach (var node in _reachableNodes)
        {
            var (x, y) = GetNodeCoords(node);
            Debug.Log($" - Node ({x}, {y})");
        }
    }

    public void MoveToNode(INode targetNode)
    {
        if (!_reachableNodes.Contains(targetNode))
        {
            Debug.LogWarning($"Cannot move to node {targetNode}, not in reachable list.");
            return;
        }

        _currentNode = targetNode;
        var (x, y) = GetNodeCoords(_currentNode);
        Debug.Log($"Moved to node = Node ({x}, {y})");

        UpdateReachableNodes();
    }

    private (int x, int y) GetNodeCoords(INode target)
    {
        if (target == null)
            return (-1, -1);

        // Use GridPos directly from the Node
        return (target.GridPos.x, target.GridPos.y);
    }
}
