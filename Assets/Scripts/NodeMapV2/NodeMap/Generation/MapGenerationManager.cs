using UnityEngine;
using UnityEngine.SceneManagement;

public class MapGenerationManager : MonoBehaviour
{
    public static MapGenerationManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private NodeMap nodeMapPrefab;
    [SerializeField] private GameObject fogPrefab;

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

    public void GenerateFromSeed(int seed)
    {
        _currentSeed = seed;
        Random.InitState(seed);

        if (_activeMap != null)
            Destroy(_activeMap.gameObject);

        NodeMap map = Instantiate(nodeMapPrefab);

        Scene mapScene = SceneManager.GetSceneByName("Map");
        if (mapScene.IsValid())
            SceneManager.MoveGameObjectToScene(map.gameObject, mapScene);

        _activeMap = map;

        map.Generate();

        var assigner = new NodeTypeAssigner(map.Factory);
        assigner.Assign(map);

        var validator = new NodeMapValidator();
        validator.Validate(map);

        MapStateManager.Instance.Initialize(map);

        var spawner = new NodeMapSpawner();
        _visualContext = spawner.Spawn(map, map.transform);

        var env = new MapEnvironmentSpawner();
        env.SpawnEnvironment(_visualContext, map.transform);

        // IMPORTANT: Recalculate bounds in case environment expanded them
        Bounds finalBounds = _visualContext.MapBounds;

        var cam = FindFirstObjectByType<MapCameraController>();
        if (cam != null)
            cam.SetBoundsUsingWorldBounds(finalBounds);

        if (fogPrefab != null)
        {
            GameObject fogObj = Instantiate(fogPrefab, _activeMap.transform);
            fogObj.name = "Fog";

            var fog = fogObj.GetComponent<FogController>();
            if (fog != null)
                fog.Initialize(_activeMap, finalBounds);
        }
    }
}

