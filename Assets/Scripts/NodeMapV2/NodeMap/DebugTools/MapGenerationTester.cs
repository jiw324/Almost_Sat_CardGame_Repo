#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class MapGenerationTester : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private NodeMap nodeMapPrefab;
    [SerializeField] private bool autoRunOnPlay = false;
    [SerializeField, Min(1)] private int testIterations = 50;

    [Header("Phase Toggles")]
    [SerializeField] private bool runAssigner = true;
    [SerializeField] private bool runValidator = true;
    [SerializeField] private bool runSpawner = true;

    private NodeMap _currentMap;
    private List<NodeMap> _testMaps = new();
    private List<IMapTestModule> _modules = new();

    private void Awake()
    {
        _modules = new List<IMapTestModule>
        {
            new StructureValidationModule(),
            new NodeTypeDistributionModule(),
            new ValidationModule(),
            // new RenderingValidationModule()
        };
    }

    private void Start()
    {
        if (autoRunOnPlay)
            RunSingleTest();
    }

    [ContextMenu("Generate Single Map")]
    public void RunSingleTest()
    {
        if (_modules == null || _modules.Count == 0)
            Awake();

        ClearMaps();

        foreach (var module in _modules)
            module.Reset();

        _currentMap = GenerateMapInstance();
        RunAllModules(_currentMap);
        _testMaps.Add(_currentMap);

        PrintModuleReports();
    }

    [ContextMenu("Run Volume Test")]
    public void RunVolumeTest()
    {
        if (_modules == null || _modules.Count == 0)
            Awake();

        ClearMaps();

        foreach (var module in _modules)
            module.Reset();

        Debug.Log($"Running {testIterations} test iterations...");

        for (int i = 0; i < testIterations; i++)
        {
            NodeMap map = GenerateMapInstance();
            RunAllModules(map);
            _testMaps.Add(map);
        }

        PrintModuleReports();
    }

    private NodeMap GenerateMapInstance()
    {
        GameObject obj = new GameObject("NodeMap_Debug");
        obj.transform.SetParent(transform);

        NodeMap map = obj.AddComponent<NodeMap>();
        map.Generate();  // Phase 1 – structure generation

        // -------------- Phase 2 – Node type assignment --------------
        if (runAssigner)
        {
            if (map.Factory == null)
                Debug.LogError("NodeMap.Factory is null. NodeTypeAssigner cannot run.");
            else
            {
                var assigner = new NodeTypeAssigner(map.Factory);
                assigner.Assign(map);
            }
        }

        if (runValidator)
        {
            var validator = new NodeMapValidator();
            validator.Validate(map);
        }

        if (runSpawner)
        {
            var spawner = new NodeMapSpawner();
            NodeMapVisualContext context = spawner.Spawn(map, map.transform);

            var camera = FindFirstObjectByType<MapCameraController>();
            if (camera != null)
            {
                camera.SetBounds(context.MapBounds.center, map.MapWidth, map.MapHeight,
                                 map.Grid.XSpacing, map.Grid.YSpacing);
            }
        }

        return map;
    }



    private void ClearMaps()
    {
        foreach (var m in _testMaps)
            if (m != null)
                SafeDestroy(m.gameObject);

        _testMaps.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
            SafeDestroy(transform.GetChild(i).gameObject);

        _currentMap = null;
    }

    private void SafeDestroy(GameObject obj)
    {
        if (obj == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(obj);
        else
#endif
            Destroy(obj);
    }

    private void RunAllModules(NodeMap map)
    {
        foreach (var module in _modules)
            module.Run(map);
    }

    private void PrintModuleReports()
    {
        string header = "========== MAP TEST SUMMARY ==========";
        string footer = "======================================";
        string allReports = "";

        foreach (var module in _modules)
            allReports += module.GetReport() + "\n\n";

        string full = $"{header}\n\n{allReports}{footer}";

#if UNITY_EDITOR
        EditorDeferredLog(full);
#else
        Debug.Log(full);
#endif
    }

#if UNITY_EDITOR
    private void EditorDeferredLog(string message)
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying)
                Debug.Log(message);
            else
                UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", message);

            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        };
    }
#endif

    private void OnDrawGizmos()
    {
        if (_currentMap == null || _currentMap.Grid == null)
            return;

        Gizmos.color = Color.cyan;

        foreach (var node in _currentMap.AllNodes)
        {
            Vector3 pos = _currentMap.Grid.GridToWorld(node.GridPos.x, node.GridPos.y);
            Gizmos.DrawSphere(pos, 0.12f);

            Gizmos.color = new Color(0.8f, 0.8f, 1f, 0.6f);
            foreach (var next in node.NextNodes)
            {
                Vector3 nextPos = _currentMap.Grid.GridToWorld(next.GridPos.x, next.GridPos.y);
                Gizmos.DrawLine(pos, nextPos);
            }

            Gizmos.color = Color.cyan;
        }
    }
}
#endif
