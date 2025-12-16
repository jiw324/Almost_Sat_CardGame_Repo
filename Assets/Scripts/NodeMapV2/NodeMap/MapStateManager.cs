using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapStateManager : MonoBehaviour
{
    public static MapStateManager Instance { get; private set; }

    public event System.Action OnNodeChanged;

    private NodeMap _map;
    private Dictionary<string, Node> _lookup;

    private readonly HashSet<string> _visited = new();
    private readonly HashSet<string> _completed = new();
    private string _currentNodeId;

    private SessionNodeMapData SessionMap =>
        GameSession.Instance?.GetActiveNodeMapData();

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

    private void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.ctrlKey.isPressed && kb.cKey.wasPressedThisFrame)
        {
            Node current = GetCurrentNode();
            if (current != null)
            {
                MarkCompleted(current);
                ReturnToMapScene();
            }
        }
    }

    public void ReturnToMapScene()
    {
        var sceneManagerObj = GameObject.Find("SceneManager");
        if (sceneManagerObj == null)
        {
            Debug.LogError("MapStateManager: Could not find SceneManager object.");
            return;
        }

        var sceneSwitch = sceneManagerObj.GetComponent<SceneSwitch>();
        if (sceneSwitch == null)
        {
            Debug.LogError("MapStateManager: SceneManager missing SceneSwitch component.");
            return;
        }

        sceneSwitch.SceneChanger("Map");
    }

    public void Initialize(NodeMap map)
    {
        _map = map;
        _lookup = _map.AllNodes.ToDictionary(n => n.Id);

        if (_map.BossNode != null && !_lookup.ContainsKey(_map.BossNode.Id))
            _lookup.Add(_map.BossNode.Id, _map.BossNode);

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
        GameSession.Instance.CaptureAudioSettings();
        SessionSaveManager.SaveGameSession(GameSession.Instance.gameSessionData);
    }

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

    public void SetCurrentNode(Node node)
    {
        _currentNodeId = node?.Id;
        SessionMap.currentNodeId = _currentNodeId;

        OnNodeChanged?.Invoke();

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

        bool newlyCompleted = _completed.Add(node.Id);
        if (newlyCompleted)
            SessionMap.completedNodeIds.Add(node.Id);

        SaveSession();

        if (_map != null && node == _map.BossNode)
        {
            Debug.Log("Boss defeated! Showing victory screen...");
            VictoryScreenManager.ShowVictory();
        }

        Debug.Log("MarkCompleted called for " + node.Id
          + " | IsBoss = " + (node == _map.BossNode));

    }


    public bool IsNodeInteractable(INode target)
    {
        if (_map == null || target == null)
            return false;

        Node targetNode = target as Node;
        if (targetNode == null) return false;

        Node current = GetCurrentNode();

        if (current == null)
            return targetNode.GridPos.y == 0;

        int currentFloor = current.GridPos.y;
        int targetFloor = targetNode.GridPos.y;

        bool currentCompleted = IsNodeCompleted(current);

        if (!currentCompleted)
            return targetFloor == currentFloor && targetNode.Id == current.Id;

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

        if (!IsNodeInteractable(t))
            return false;

        Node current = GetCurrentNode();

        if (current != null && t.Id == current.Id)
            return true;

        if (current != null && !IsNodeCompleted(current))
            MarkCompleted(current);

        SetCurrentNode(t);
        MarkVisited(t);

        SaveSession();
        return true;
    }

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
                view.ShowAvailable();
        }
    }
}
