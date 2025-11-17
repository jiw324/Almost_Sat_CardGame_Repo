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

    private List<NodeMap> _testMaps = new();
    private List<IMapTestModule> _modules = new();

    private const int MAX_GENERATION_ATTEMPTS = 10;

    private bool _lastMapValid;
    private bool _lastMapHadFixes;
    private int _lastPassesUsed;
    private int _lastRegenAttempts;
    private Dictionary<string, int> _lastRuleHits = new();
    private string _lastValidationMessage = "";

    private void Awake()
    {
        _modules = new List<IMapTestModule>
        {
            new StructureValidationModule(),
            new NodeTypeDistributionModule(),
            new ValidationModule(),
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
        InitializeModules();
        ClearMaps();

        NodeMap map = GenerateMapInstance();
        RunAllModules(map);
        _testMaps.Add(map);

        PrintModuleReports();
    }

    [ContextMenu("Run Volume Test")]
    public void RunVolumeTest()
    {
        InitializeModules();
        ClearMaps();

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
        int attempts = 0;
        ResetLastRunStats();

        while (attempts < MAX_GENERATION_ATTEMPTS)
        {
            // Create an empty object containing NodeMap
            GameObject obj = new GameObject("NodeMap_Debug");
            obj.transform.SetParent(transform);

            NodeMap map = obj.AddComponent<NodeMap>();
            map.Generate();

            // Assign node types
            if (runAssigner)
            {
                var assigner = new NodeTypeAssigner(map.Factory);
                assigner.Assign(map);
            }

            // Validate
            if (runValidator)
            {
                var validator = new NodeMapValidator();
                var res = validator.Validate(map);

                _lastPassesUsed = validator.PassesUsed;
                _lastValidationMessage = res.Message;
                _lastRuleHits = new Dictionary<string, int>(validator.RuleHits);

                int totalFixHits = 0;
                foreach (var kvp in validator.RuleHits)
                    totalFixHits += kvp.Value;

                _lastMapHadFixes = totalFixHits > 0;
                _lastMapValid = !res.ruleViolated;

                if (_lastMapValid)
                {
                    _lastRegenAttempts = attempts;
                    return map;
                }
            }

            // invalid map -> destroy and retry
            SafeDestroy(obj);
            attempts++;
        }

        // fallback
        GameObject fallback = new GameObject("NodeMap_Debug_Fallback");
        fallback.transform.SetParent(transform);

        NodeMap fallbackMap = fallback.AddComponent<NodeMap>();
        fallbackMap.Generate();

        if (runAssigner)
        {
            var assigner = new NodeTypeAssigner(fallbackMap.Factory);
            assigner.Assign(fallbackMap);
        }

        _lastRegenAttempts = MAX_GENERATION_ATTEMPTS;
        return fallbackMap;
    }

    private void ResetLastRunStats()
    {
        _lastMapValid = false;
        _lastMapHadFixes = false;
        _lastPassesUsed = 0;
        _lastRegenAttempts = 0;
        _lastRuleHits.Clear();
        _lastValidationMessage = "";
    }

    private void InitializeModules()
    {
        foreach (var module in _modules)
            module.Reset();
    }

    private void RunAllModules(NodeMap map)
    {
        foreach (var module in _modules)
        {
            module.Run(map);

            if (module is ValidationModule vm && runValidator)
            {
                vm.RecordValidationStats(
                    _lastMapValid,
                    _lastMapHadFixes,
                    _lastPassesUsed,
                    _lastRuleHits,
                    _lastValidationMessage);

                vm.RecordRegenerationAttempts(_lastRegenAttempts);
            }
        }
    }

    private void ClearMaps()
    {
        foreach (var m in _testMaps)
            if (m != null)
                SafeDestroy(m.gameObject);

        _testMaps.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
            SafeDestroy(transform.GetChild(i).gameObject);
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
        EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying)
                Debug.Log(message);
            else
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", message);

            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        };
    }
#endif
}
#endif
