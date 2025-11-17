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

    public NodeMap ActiveMap => _activeMap;
    public int CurrentSeed => _currentSeed;

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
    // SESSION LOADING / RUN START
    // ============================================================

    public void InitializeMapFromSession()
    {
        var sm = SessionMap;

        if (sm == null || sm.mapSeed == 0)
        {
            StartNewRun();
            return;
        }

        GenerateFromSeed(sm.mapSeed);
    }

    public void StartNewRun()
    {
        int seed = useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : debugSeed;

        GenerateFromSeed(seed);

        SessionMap.mapSeed = seed;
        SessionMap.currentNodeId = null;
        SessionMap.visitedNodeIds.Clear();
        SessionMap.completedNodeIds.Clear();

        SessionSaveManager.SaveGameSession(GameSession.Instance.gameSessionData);
    }

    // ============================================================
    // GENERATION
    // ============================================================

    public void GenerateFromSeed(int seed)
    {
        _currentSeed = seed;
        Random.InitState(seed);

        // Clear previous map
        if (_activeMap != null)
            Destroy(_activeMap.gameObject);

        // Instantiate map into the proper scene
        NodeMap map = Instantiate(nodeMapPrefab);

        Scene mapScene = SceneManager.GetSceneByName("Map");
        if (mapScene.IsValid())
            SceneManager.MoveGameObjectToScene(map.gameObject, mapScene);

        _activeMap = map;

        // Build the node graph
        map.Generate();

        // Assign node types
        var assigner = new NodeTypeAssigner(map.Factory);
        assigner.Assign(map);

        // Validate structure
        var validator = new NodeMapValidator();
        validator.Validate(map);

        // IMPORTANT:
        // Initialize Map State BEFORE spawning NodeViews
        MapStateManager.Instance.Initialize(map);

        // Spawn NodeViews AFTER state exists
        var spawner = new NodeMapSpawner();
        _visualContext = spawner.Spawn(map, map.transform);

        // Configure camera
        var cam = FindFirstObjectByType<MapCameraController>();
        if (cam != null)
            cam.SetBoundsUsingWorldBounds(_visualContext.MapBounds);
    }
}
