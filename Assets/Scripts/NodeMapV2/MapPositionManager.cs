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
        if (!_nodeMap.nodes.ContainsKey(0))
        {
            Debug.LogError("NodeMap does not contain a first row (floor 0).");
            return;
        }

        Debug.Log("Starter nodes (first row):");
        for (int x = 0; x < _nodeMap.nodes[0].Length; x++)
        {
            if (_nodeMap.nodes[0][x] != null)
                Debug.Log($" - Node at (x={x}, y=0)");
        }
    }

    public void SelectStarterNode(INode starterNode)
    {
        if (!_nodeMap.nodes[0].Contains(starterNode))
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

        foreach (var next in _currentNode.nextNodes)
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
        foreach (var kvp in _nodeMap.nodes)
        {
            int y = kvp.Key;
            INode[] row = kvp.Value;

            for (int x = 0; x < row.Length; x++)
            {
                if (row[x] == target)
                    return (x, y);
            }
        }
        return (-1, -1); // Not found (shouldn't happen)
    }
}
