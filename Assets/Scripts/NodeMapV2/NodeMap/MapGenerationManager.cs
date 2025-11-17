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

    private readonly HashSet<string> _visitedNodeIds = new HashSet<string>();
    private readonly HashSet<string> _completedNodeIds = new HashSet<string>();
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

    public void StartNewRun()
    {
        int seed = useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : debugSeed;
        GenerateFromSeed(seed);
    }

    public void GenerateFromSeed(int seed)
    {
        _currentSeed = seed;

        Random.InitState(seed);

        ClearActiveMap();

        if (nodeMapPrefab == null)
        {
            Debug.LogError("MapGenerationManager: NodeMap prefab not assigned.");
            return;
        }

        NodeMap mapInstance = Instantiate(nodeMapPrefab);

        // Explicitly move into the MapScene
        Scene mapScene = SceneManager.GetSceneByName("Map");
        if (mapScene.IsValid())
        {
            SceneManager.MoveGameObjectToScene(mapInstance.gameObject, mapScene);
        }

        _activeMap = mapInstance;


        mapInstance.Generate();

        if (mapInstance.Factory == null)
        {
            Debug.LogError("MapGenerationManager: NodeMap.Factory is null. NodeTypeAssigner cannot run.");
        }
        else
        {
            var assigner = new NodeTypeAssigner(mapInstance.Factory);
            assigner.Assign(mapInstance);
        }

        var validator = new NodeMapValidator();
        var validationResult = validator.Validate(mapInstance);
        if (validationResult.ruleViolated)
        {
            Debug.LogWarning($"MapGenerationManager: Map from seed {seed} had validation fixes:\n{validationResult.Message}");
        }

        var spawner = new NodeMapSpawner();
        _visualContext = spawner.Spawn(mapInstance, mapInstance.transform);

        var camera = FindFirstObjectByType<MapCameraController>();
        if (camera != null)
        {
            camera.SetBoundsUsingWorldBounds(_visualContext.MapBounds);
        }

        _visitedNodeIds.Clear();
        _completedNodeIds.Clear();

        // IMPORTANT: do NOT auto-select a start node.
        // The first click on a floor-0 node will choose the start.
        _currentNodeId = null;
    }

    private void ClearActiveMap()
    {
        if (_activeMap == null)
            return;

        Destroy(_activeMap.gameObject);
        _activeMap = null;
        _visualContext = null;
        _currentNodeId = null;
        _visitedNodeIds.Clear();
        _completedNodeIds.Clear();
    }

    public Node GetNodeById(string id)
    {
        if (_activeMap == null || string.IsNullOrEmpty(id))
            return null;

        return _activeMap.AllNodes.FirstOrDefault(n => n.Id == id);
    }

    public bool IsNodeVisited(INode node)
    {
        if (node == null) return false;
        return _visitedNodeIds.Contains(node.Id);
    }

    public bool IsNodeCompleted(INode node)
    {
        if (node == null) return false;
        return _completedNodeIds.Contains(node.Id);
    }

    public void MarkVisited(Node node)
    {
        if (node == null) return;
        node.MarkVisited();
        _visitedNodeIds.Add(node.Id);
    }

    public void MarkCompleted(Node node)
    {
        if (node == null) return;
        node.MarkCompleted();
        _completedNodeIds.Add(node.Id);
    }

    // Called by MapPositionManager whenever the current node changes
    public void SetCurrentNode(Node node)
    {
        _currentNodeId = node != null ? node.Id : null;
    }

    [System.Serializable]
    public class MapSaveData
    {
        public int seed;
        public string currentNodeId;
        public List<string> visitedNodeIds = new List<string>();
        public List<string> completedNodeIds = new List<string>();
    }

    public MapSaveData CreateSaveData()
    {
        var data = new MapSaveData
        {
            seed = _currentSeed,
            currentNodeId = _currentNodeId
        };

        if (_activeMap != null)
        {
            foreach (Node node in _activeMap.AllNodes)
            {
                if (node.IsVisited)
                    data.visitedNodeIds.Add(node.Id);
                if (node.IsCompleted)
                    data.completedNodeIds.Add(node.Id);
            }
        }

        return data;
    }

    public void LoadFromSaveData(MapSaveData data)
    {
        if (data == null)
        {
            Debug.LogError("MapGenerationManager: LoadFromSaveData called with null data.");
            return;
        }

        GenerateFromSeed(data.seed);

        _currentNodeId = data.currentNodeId;
        _visitedNodeIds.Clear();
        _completedNodeIds.Clear();

        if (_activeMap == null)
            return;

        Dictionary<string, Node> idLookup = _activeMap.AllNodes.ToDictionary(n => n.Id, n => n);

        foreach (string id in data.visitedNodeIds)
        {
            if (idLookup.TryGetValue(id, out var node))
                MarkVisited(node);
        }

        foreach (string id in data.completedNodeIds)
        {
            if (idLookup.TryGetValue(id, out var node))
                MarkCompleted(node);
        }
    }
}
