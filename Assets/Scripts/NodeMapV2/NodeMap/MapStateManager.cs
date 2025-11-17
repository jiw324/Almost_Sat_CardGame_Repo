using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapStateManager : MonoBehaviour
{
    public static MapStateManager Instance { get; private set; }

    private NodeMap _map;
    private Dictionary<string, Node> _lookup;

    private readonly HashSet<string> _visited = new();
    private readonly HashSet<string> _completed = new();
    private string _currentNodeId;

    private SessionNodeMapData SessionMap =>
        GameSession.Instance?.gameSessionData?.sessionNodeMapData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ============================================================
    // INITIALIZATION
    // ============================================================

    public void Initialize(NodeMap map)
    {
        _map = map;
        _lookup = _map.AllNodes.ToDictionary(n => n.Id);

        RestoreFromSession();
    }

    private void RestoreFromSession()
    {
        if (SessionMap == null)
            return;

        _visited.Clear();
        _completed.Clear();
        _currentNodeId = null;

        foreach (var id in SessionMap.visitedNodeIds)
            if (_lookup.ContainsKey(id))
                _visited.Add(id);

        foreach (var id in SessionMap.completedNodeIds)
            if (_lookup.ContainsKey(id))
                _completed.Add(id);

        if (!string.IsNullOrEmpty(SessionMap.currentNodeId) &&
            _lookup.ContainsKey(SessionMap.currentNodeId))
        {
            _currentNodeId = SessionMap.currentNodeId;
        }
    }

    private void SaveSession()
    {
        SessionSaveManager.SaveGameSession(GameSession.Instance.gameSessionData);
    }

    // ============================================================
    // STATE GETTERS
    // ============================================================

    public Node GetCurrentNode()
    {
        if (string.IsNullOrEmpty(_currentNodeId)) return null;
        return _lookup.TryGetValue(_currentNodeId, out var n) ? n : null;
    }

    public bool IsNodeVisited(INode node)
    {
        if (node == null) return false;
        return _visited.Contains(node.Id);
    }

    public bool IsNodeCompleted(INode node)
    {
        if (node == null) return false;
        return _completed.Contains(node.Id);
    }

    // ============================================================
    // STATE SETTERS
    // ============================================================

    public void SetCurrentNode(Node node)
    {
        _currentNodeId = node?.Id;
        SessionMap.currentNodeId = _currentNodeId;
        SaveSession();
    }

    public void MarkVisited(Node node)
    {
        if (node == null) return;

        if (_visited.Add(node.Id))
            SessionMap.visitedNodeIds.Add(node.Id);

        SaveSession();
    }

    public void MarkCompleted(Node node)
    {
        if (node == null) return;

        if (_completed.Add(node.Id))
            SessionMap.completedNodeIds.Add(node.Id);

        SaveSession();
    }

    // ============================================================
    // GAMEPLAY RULES
    // ============================================================

    public bool IsNodeInteractable(INode target)
    {
        if (_map == null || target == null)
            return false;

        Node targetNode = target as Node;
        if (targetNode == null) return false;

        Node current = GetCurrentNode();

        // 0 — No selection yet: floor 0 only
        if (current == null)
            return targetNode.GridPos.y == 0;

        int currentFloor = current.GridPos.y;
        int targetFloor = targetNode.GridPos.y;

        bool currentCompleted = IsNodeCompleted(current);

        // 1 — Current not completed ? only current node clickable
        if (!currentCompleted)
            return targetFloor == currentFloor && targetNode.Id == current.Id;

        // 2 — Current completed ? only next-floor children
        if (targetFloor != currentFloor + 1)
            return false;

        return current.NextNodes.Contains(targetNode);
    }

    public IEnumerable<Node> GetAvailableNodes()
    {
        Node current = GetCurrentNode();

        if (current == null)
            return _map.Floors[0];

        bool currentCompleted = IsNodeCompleted(current);

        if (!currentCompleted)
            return new List<Node> { current };

        return current.NextNodes;
    }

    public bool TrySelectOrMoveToNode(INode target)
    {
        Node t = target as Node;
        if (t == null) return false;

        // Must respect interactability
        if (!IsNodeInteractable(t))
            return false;

        Node current = GetCurrentNode();

        // Clicking the current node again is allowed but does nothing
        if (current != null && t.Id == current.Id)
            return true;

        // Finish previous node
        if (current != null && !IsNodeCompleted(current))
            MarkCompleted(current);

        // New node becomes visited
        SetCurrentNode(t);
        MarkVisited(t);

        SaveSession();
        return true;
    }

    // ============================================================
    // VISUAL SYNC
    // ============================================================

    public void RefreshAllNodeGlows()
    {
        var allViews = Object.FindObjectsByType<NodeView>(FindObjectsSortMode.None);
        foreach (var view in allViews)
            view.UpdateGlow();

        var availableIds = GetAvailableNodes()
            .Select(n => n.Id)
            .ToHashSet();

        foreach (var view in allViews)
        {
            if (availableIds.Contains(view.NodeData.Id))
                view.ShowAvailable(false);
        }
    }
}
