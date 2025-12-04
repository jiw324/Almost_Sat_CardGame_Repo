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

    private SessionNodeMapData TutorialSessionMap =>
        GameSession.Instance?.gameSessionData?.tutorialNodeMapData;

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
        if (debugSeed == -999)
        {
            InitializeTutorialFromSession();
            return;
        }

        GameSession.Instance.IsTutorialMode = false;

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
        GameSession.Instance.IsTutorialMode = false;

        int seed = useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : debugSeed;

        GenerateFromSeed(seed);

        if (SessionMap != null)
        {
            SessionMap.mapSeed = seed;
            SessionMap.currentNodeId = null;
            SessionMap.visitedNodeIds.Clear();
            SessionMap.completedNodeIds.Clear();
        }

        SessionSaveManager.SaveGameSession(GameSession.Instance.gameSessionData);
    }

    public void InitializeTutorialFromSession()
    {
        GameSession.Instance.IsTutorialMode = true;

        var tMap = TutorialSessionMap;
        if (tMap == null)
        {
            GameSession.Instance.gameSessionData.tutorialNodeMapData = new SessionNodeMapData();
        }

        tMap = TutorialSessionMap;

        if (tMap.mapSeed == 0)
        {
            int seed = 1;
            GenerateTutorialFromSeed(seed);

            tMap.mapSeed = seed;
            tMap.currentNodeId = null;
            tMap.visitedNodeIds.Clear();
            tMap.completedNodeIds.Clear();

            SessionSaveManager.SaveGameSession(GameSession.Instance.gameSessionData);
        }
        else
        {
            GenerateTutorialFromSeed(tMap.mapSeed);
        }
    }

    public void GenerateFromSeed(int seed)
    {
        GameSession.Instance.IsTutorialMode = false;

        // Grab seed and initialize Random class with seed
        _currentSeed = seed;
        Random.InitState(seed);

        if (_activeMap != null)
            Destroy(_activeMap.gameObject);

        // Instantiate map object and move to map scene
        NodeMap map = Instantiate(nodeMapPrefab);
        Scene mapScene = SceneManager.GetSceneByName("Map");
        if (mapScene.IsValid())
            SceneManager.MoveGameObjectToScene(map.gameObject, mapScene);

        _activeMap = map;

        // Generate structure of map
        map.Generate();

        // Assign node types on map
        var assigner = new NodeTypeAssigner(map.Factory);
        assigner.Assign(map);

        // Assign random enemies to combat nodes
        AssignEnemiesToCombatNodes(map);

        // Validate node map against rules
        var validator = new NodeMapValidator();
        validator.Validate(map);

        MapStateManager.Instance.Initialize(map);

        // Spawn prefabs for node map
        var spawner = new NodeMapSpawner();
        _visualContext = spawner.Spawn(map, map.transform);

        // Spawn environment around node map
        var env = new MapEnvironmentSpawner();
        env.SpawnEnvironment(_visualContext, map.transform);

        Bounds finalBounds = _visualContext.MapBounds;

        // Position map camera and set movement bounds
        var cam = FindFirstObjectByType<MapCameraController>();
        if (cam != null)
            cam.SetBoundsUsingWorldBounds(finalBounds);

        // Spawn fog over map
        if (!GameSession.Instance.IsTutorialMode && fogPrefab != null)
        {
            GameObject fogObj = Instantiate(fogPrefab, _activeMap.transform);
            fogObj.name = "Fog";

            var fog = fogObj.GetComponent<FogController>();
            if (fog != null)
                fog.Initialize(_activeMap, finalBounds);
        }
    }

    private void GenerateTutorialFromSeed(int seed)
    {
        GameSession.Instance.IsTutorialMode = true;

        _currentSeed = seed;
        Random.InitState(seed);

        if (_activeMap != null)
            Destroy(_activeMap.gameObject);

        NodeMap map = Instantiate(nodeMapPrefab);
        Scene mapScene = SceneManager.GetSceneByName("Map");
        if (mapScene.IsValid())
            SceneManager.MoveGameObjectToScene(map.gameObject, mapScene);

        _activeMap = map;

        map.GenerateTutorial();

        AssignEnemiesToCombatNodes(map);

        MapStateManager.Instance.Initialize(map);

        var spawner = new NodeMapSpawner();
        _visualContext = spawner.Spawn(map, map.transform);

        var env = new MapEnvironmentSpawner();
        env.SpawnEnvironment(_visualContext, map.transform);

        Bounds finalBounds = _visualContext.MapBounds;

        var cam = FindFirstObjectByType<MapCameraController>();
        if (cam != null)
            cam.SetBoundsUsingWorldBounds(finalBounds);
    }

    /// <summary>
    /// Assigns random enemies to all combat nodes in the map.
    /// </summary>
    private void AssignEnemiesToCombatNodes(NodeMap map)
    {
        // Dynamically load all enemies from Resources/Enemies/
        EnemyDefinition[] enemyDefinitions = Resources.LoadAll<EnemyDefinition>("Enemies");

        if (enemyDefinitions == null || enemyDefinitions.Length == 0)
        {
            Debug.LogWarning("[MapGenerationManager] No enemies found in Resources/Enemies/. Using fallback.");
            return;
        }

        // Extract enemy names from the loaded definitions
        string[] availableEnemies = new string[enemyDefinitions.Length];
        for (int i = 0; i < enemyDefinitions.Length; i++)
        {
            availableEnemies[i] = enemyDefinitions[i].name; // Use the asset name (without .asset extension)
        }

        Debug.Log($"[MapGenerationManager] Loaded {availableEnemies.Length} enemies: {string.Join(", ", availableEnemies)}");

        int assignedCount = 0;
        foreach (var floorPair in map.Floors)
        {
            foreach (var node in floorPair.Value)
            {
                if (node.Definition != null && node.Definition.nodeType == NodeType.Combat)
                {
                    // Randomly assign an enemy to this combat node
                    string randomEnemy = availableEnemies[Random.Range(0, availableEnemies.Length)];
                    CombatNodeEnemyAssigner.AssignEnemyToNode(node, randomEnemy);
                    assignedCount++;
                }
            }
        }

        // Also check boss node
        if (map.BossNode != null && map.BossNode.Definition != null && map.BossNode.Definition.nodeType == NodeType.Combat)
        {
            // Boss could use a different enemy or same pool - using same pool for now
            string randomEnemy = availableEnemies[Random.Range(0, availableEnemies.Length)];
            CombatNodeEnemyAssigner.AssignEnemyToNode(map.BossNode, randomEnemy);
            assignedCount++;
        }

        Debug.Log($"[MapGenerationManager] Assigned enemies to {assignedCount} combat nodes.");
    }
}
