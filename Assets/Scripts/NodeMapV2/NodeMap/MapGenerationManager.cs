using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapGenerationManager : MonoBehaviour
{
    public static MapGenerationManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private NodeMap nodeMapPrefab;

    [Header("Seed Settings")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int debugSeed = 12345;

    private NodeMap _activeMap;
    private NodeMapVisualContext _visualContext;
    private int _currentSeed;

    private readonly HashSet<string> _visited = new();
    private readonly HashSet<string> _completed = new();
    private string _currentNodeId;

    public NodeMap ActiveMap => _activeMap;
    public int CurrentSeed => _currentSeed;
    public string CurrentNodeId => _currentNodeId;

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

    private SessionNodeMapData SessionMap =>
        GameSession.Instance?.gameSessionData?.sessionNodeMapData;

    public void InitializeMapFromSession()
    {
        var sm = SessionMap;

        if (sm == null || sm.mapSeed == 0)
        {
            StartNewRun();
            return;
        }

        GenerateFromSeed(sm.mapSeed);
        ApplySessionState(sm);
    }

    public void StartNewRun()
    {
        int seed = useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : debugSeed;

        GenerateFromSeed(seed);

        SessionMap.mapSeed = seed;
        SessionMap.currentNodeId = null;
        SessionMap.visitedNodeIds.Clear();
        SessionMap.completedNodeIds.Clear();

        SaveSession();
    }

    public void GenerateFromSeed(int seed)
    {
        _currentSeed = seed;
        Random.InitState(seed);

        ClearActiveMap();

        NodeMap map = Instantiate(nodeMapPrefab);

        Scene mapScene = SceneManager.GetSceneByName("Map");
        if (mapScene.IsValid())
        {
            SceneManager.MoveGameObjectToScene(map.gameObject, mapScene);
        }

        _activeMap = map;
        map.Generate();

        var assigner = new NodeTypeAssigner(map.Factory);
        assigner.Assign(map);

        var validator = new NodeMapValidator();
        validator.Validate(map);

        var spawner = new NodeMapSpawner();
        _visualContext = spawner.Spawn(map, map.transform);

        var cam = FindFirstObjectByType<MapCameraController>();
        if (cam != null)
            cam.SetBoundsUsingWorldBounds(_visualContext.MapBounds);

        _visited.Clear();
        _completed.Clear();
        _currentNodeId = null;
    }

    private void ClearActiveMap()
    {
        if (_activeMap != null)
            Destroy(_activeMap.gameObject);

        _activeMap = null;
        _visualContext = null;

        _visited.Clear();
        _completed.Clear();
        _currentNodeId = null;
    }

    private void ApplySessionState(SessionNodeMapData sm)
    {
        if (_activeMap == null)
            return;

        Dictionary<string, Node> lookup = _activeMap.AllNodes.ToDictionary(n => n.Id);

        _visited.Clear();
        foreach (string id in sm.visitedNodeIds)
        {
            if (lookup.TryGetValue(id, out var n))
            {
                n.MarkVisited();
                _visited.Add(id);
            }
        }

        _completed.Clear();
        foreach (string id in sm.completedNodeIds)
        {
            if (lookup.TryGetValue(id, out var n))
            {
                n.MarkCompleted();
                _completed.Add(id);
            }
        }

        if (!string.IsNullOrEmpty(sm.currentNodeId) && lookup.TryGetValue(sm.currentNodeId, out var curr))
        {
            _currentNodeId = curr.Id;
        }
    }

    public void MarkVisited(Node node)
    {
        if (node == null) return;

        node.MarkVisited();
        if (_visited.Add(node.Id))
            SessionMap.visitedNodeIds.Add(node.Id);

        SaveSession();
    }

    public void MarkCompleted(Node node)
    {
        if (node == null) return;

        node.MarkCompleted();
        if (_completed.Add(node.Id))
            SessionMap.completedNodeIds.Add(node.Id);

        SaveSession();
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

        SaveSession();
    }

    private void SaveSession()
    {
        SessionSaveManager.SaveGameSession(GameSession.Instance.gameSessionData);
    }

    public void UpdateAllNodeGlows()
    {
        if (_visualContext == null || _visualContext.SpawnedNodes == null)
            return;

        foreach (var view in _visualContext.SpawnedNodes)
            view.UpdateGlow();

        var posMgr = FindFirstObjectByType<MapPositionManager>();
        if (posMgr == null) return;

        var available = posMgr.GetAvailableNodes()
                              .Select(n => n.Id)
                              .ToHashSet();

        foreach (var view in _visualContext.SpawnedNodes)
        {
            if (available.Contains(view.NodeData.Id))
                view.ShowAvailable(false);
        }
    }

}
