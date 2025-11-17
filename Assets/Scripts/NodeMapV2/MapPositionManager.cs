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
        Node t = target as Node;
        if (t == null) return false;

        Node curr = GetCurrentNode();
        if (curr == null)
            return t.GridPos.y == 0;

        if (t.Id == curr.Id) return true;

        return curr.NextNodes.Contains(t);
    }

    public bool TrySelectOrMoveToNode(INode target)
    {
        Node t = target as Node;
        if (t == null) return false;

        Node curr = GetCurrentNode();
        var mgr = MapGenerationManager.Instance;

        // Starting node selection
        if (curr == null)
        {
            if (t.GridPos.y != 0)
                return false;

            _currentNodeId = t.Id;
            mgr.SetCurrentNode(t);
            mgr.MarkVisited(t);

            SaveSession();
            return true;
        }

        // Clicking current
        if (t.Id == curr.Id)
            return true;

        // Moving to child node
        if (!curr.NextNodes.Contains(t))
            return false;

        _currentNodeId = t.Id;
        mgr.SetCurrentNode(t);
        mgr.MarkVisited(t);

        SaveSession();
        return true;
    }
}
