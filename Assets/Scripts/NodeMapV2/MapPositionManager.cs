using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapPositionManager : MonoBehaviour
{
    private NodeMap _map;
    private Dictionary<string, Node> _lookup;
    private string _currentNodeId;

    private void Awake()
    {
        _map = MapGenerationManager.Instance.ActiveMap;

        if (_map == null)
        {
            Debug.LogError("MapPositionManager: No active map");
            return;
        }

        _lookup = _map.AllNodes.ToDictionary(n => n.Id);
        _currentNodeId = MapGenerationManager.Instance.CurrentNodeId;
    }

    private void SaveSession()
    {
        SessionSaveManager.SaveGameSession(GameSession.Instance.gameSessionData);
    }

    public Node GetCurrentNode()
    {
        if (string.IsNullOrEmpty(_currentNodeId))
            return null;

        return _lookup.TryGetValue(_currentNodeId, out var n) ? n : null;
    }

    public bool IsNodeInteractable(INode target)
    {
        if (_map == null || target == null)
            return false;

        Node targetNode = target as Node;
        if (targetNode == null)
            return false;

        Node current = GetCurrentNode();
        var mgr = MapGenerationManager.Instance;

        if (current == null)
            return targetNode.GridPos.y == 0;

        int currentFloor = current.GridPos.y;
        int targetFloor = targetNode.GridPos.y;

        bool currentCompleted = mgr.IsNodeCompleted(current);

        if (!currentCompleted)
        {
            return targetFloor == currentFloor && targetNode.Id == current.Id;
        }

        if (targetFloor != currentFloor + 1)
            return false;

        return current.NextNodes.Contains(targetNode);
    }

    public bool TrySelectOrMoveToNode(INode target)
    {
        Node t = target as Node;
        if (t == null) return false;

        Node curr = GetCurrentNode();
        var mgr = MapGenerationManager.Instance;

        // CASE 1: No current node yet (start of run)
        if (curr == null)
        {
            // Re-use the same rules as IsNodeInteractable for safety
            if (!IsNodeInteractable(t))
                return false;

            _currentNodeId = t.Id;
            mgr.SetCurrentNode(t);
            mgr.MarkVisited(t);

            SaveSession();
            return true;
        }

        // CASE 2: We already have a current node
        // Only allow moves that IsNodeInteractable() approves
        if (!IsNodeInteractable(t))
            return false;

        // Clicking the current node again is allowed but does nothing
        if (t.Id == curr.Id)
            return true;

        // At this point IsNodeInteractable() has already ensured:
        // - Correct floor
        // - Parent completed if needed
        // - Node is a valid child
        _currentNodeId = t.Id;
        mgr.SetCurrentNode(t);
        mgr.MarkVisited(t);

        SaveSession();
        return true;
    }


    public IEnumerable<Node> GetAvailableNodes()
    {
        Node current = GetCurrentNode();
        var mgr = MapGenerationManager.Instance;

        if (current == null)
            return _map.Floors[0];

        bool currentCompleted = mgr.IsNodeCompleted(current);

        if (!currentCompleted)
            return new List<Node> { current };

        return current.NextNodes;
    }

}
