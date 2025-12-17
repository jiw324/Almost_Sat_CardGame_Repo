using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

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
    private Coroutine _spawnRoutine;

    public List<NodeView> SpawnedNodeViews { get; private set; } = new();

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

        GameSession.Instance.CaptureAudioSettings();
        SessionSaveManager.SaveGameSession(GameSession.Instance.gameSessionData);
    }

    public void InitializeTutorialFromSession()
    {
        GameSession.Instance.IsTutorialMode = true;

        var tMap = TutorialSessionMap;
        if (tMap == null)
            GameSession.Instance.gameSessionData.tutorialNodeMapData = new SessionNodeMapData();

        tMap = TutorialSessionMap;

        if (tMap.mapSeed == 0)
        {
            int seed = 1;
            GenerateTutorialFromSeed(seed);

            tMap.mapSeed = seed;
            tMap.currentNodeId = null;
            tMap.visitedNodeIds.Clear();
            tMap.completedNodeIds.Clear();

            GameSession.Instance.CaptureAudioSettings();
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

        _currentSeed = seed;
        Random.InitState(seed);

        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

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

        AssignEnemiesToCombatNodes(map);

        var validator = new NodeMapValidator();
        validator.Validate(map);

        MapStateManager.Instance.Initialize(map);

        SpawnedNodeViews.Clear();
        _visualContext = null;

        _spawnRoutine = StartCoroutine(SpawnVisualsCoroutine(map, isTutorial: false));
    }

    private void GenerateTutorialFromSeed(int seed)
    {
        GameSession.Instance.IsTutorialMode = true;

        _currentSeed = seed;
        Random.InitState(seed);

        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

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

        SpawnedNodeViews.Clear();
        _visualContext = null;

        _spawnRoutine = StartCoroutine(SpawnVisualsCoroutine(map, isTutorial: true));
    }

    private IEnumerator SpawnVisualsCoroutine(NodeMap map, bool isTutorial)
    {
        if (map == null)
            yield break;

        var spawner = new NodeMapSpawner();
        yield return spawner.SpawnAsync(map, map.transform, batchSize: 32);

        _visualContext = spawner.Context;
        if (_visualContext == null)
            yield break;

        SpawnedNodeViews = _visualContext.SpawnedNodes ?? new List<NodeView>();

        var env = new MapEnvironmentSpawner();
        yield return env.SpawnEnvironmentAsync(_visualContext, map.transform, batchSize: 64);

        Bounds finalBounds = _visualContext.MapBounds;

        var cam = FindFirstObjectByType<MapCameraController>();
        if (cam != null)
            cam.SetBoundsUsingWorldBounds(finalBounds);

        if (!isTutorial && fogPrefab != null)
        {
            GameObject fogObj = Instantiate(fogPrefab, _activeMap.transform);
            fogObj.name = "Fog";

            var fog = fogObj.GetComponent<FogController>();
            if (fog != null)
                fog.Initialize(_activeMap, finalBounds);
        }

        _spawnRoutine = null;
    }

    private void AssignEnemiesToCombatNodes(NodeMap map)
    {
        EnemyDefinition[] enemyDefinitions = Resources.LoadAll<EnemyDefinition>("Enemies");

        if (enemyDefinitions == null || enemyDefinitions.Length == 0)
        {
            Debug.LogWarning("[MapGenerationManager] No enemies found in Resources/Enemies/. Using fallback.");
            return;
        }

        string[] availableEnemies = new string[enemyDefinitions.Length];
        for (int i = 0; i < enemyDefinitions.Length; i++)
            availableEnemies[i] = enemyDefinitions[i].name;

        int assignedCount = 0;

        foreach (var floorPair in map.Floors)
        {
            foreach (var node in floorPair.Value)
            {
                if (node.Definition != null && node.Definition.nodeType == NodeType.Combat)
                {
                    string randomEnemy = availableEnemies[Random.Range(0, availableEnemies.Length)];
                    CombatNodeEnemyAssigner.AssignEnemyToNode(node, randomEnemy);
                    assignedCount++;
                }
            }
        }

        // Assign a boss from Resources/Bosses (supports multiple boss assets in that folder)
        if (map.BossNode != null &&
            map.BossNode.Definition != null &&
            map.BossNode.Definition.nodeType == NodeType.Combat)
        {
            EnemyDefinition[] bossDefinitions = Resources.LoadAll<EnemyDefinition>("Bosses");
            if (bossDefinitions != null && bossDefinitions.Length > 0)
            {
                // Pick the first boss found.
                EnemyDefinition bossEnemy = bossDefinitions[0];
                string bossPath = $"Bosses/{bossEnemy.name}";
                CombatNodeEnemyAssigner.AssignEnemyToNode(map.BossNode, bossPath);             
                Debug.Log($"[MapGenerationManager] Assigned boss enemy '{bossEnemy.name}' to boss node.");
                return;
            }
            else
            {
                // Fallback to one of the regular enemies
                string randomEnemy = availableEnemies[Random.Range(0, availableEnemies.Length)];
                CombatNodeEnemyAssigner.AssignEnemyToNode(map.BossNode, randomEnemy);
            }
            assignedCount++;
        }

        Debug.Log($"[MapGenerationManager] Assigned enemies to {assignedCount} combat nodes.");
    }
}
