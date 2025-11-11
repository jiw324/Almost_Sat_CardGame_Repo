#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class MapGenerationTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private NodeMap nodeMapPrefab;
    [SerializeField, Min(1)] private int testIterations = 100;
    [SerializeField] private int randomSeed = 0; // 0 = random
    [SerializeField] private bool autoRunOnPlay = false;

    [Header("Visualization Settings")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color nodeColor = Color.cyan;
    [SerializeField] private Color lineColor = new Color(0.8f, 0.8f, 1f, 0.6f);
    [SerializeField] private float nodeRadius = 0.12f;

    private NodeMap currentMap;
    private List<NodeMap> testMaps = new();

    private void Start()
    {
        if (autoRunOnPlay)
            RunSingleTest();
    }

    [ContextMenu("Generate Single Map")]
    public void RunSingleTest()
    {
        ClearMaps();
        GenerateNewMap();
        ValidateAndReport(currentMap);
    }

    [ContextMenu("Run Volume Test")]
    public void RunVolumeTest()
    {
        ClearMaps();
        RunMultipleGenerations(testIterations);
    }

    private void ClearMaps()
    {
        foreach (var m in testMaps)
            if (m != null) DestroyImmediate(m.gameObject);

        testMaps.Clear();
        currentMap = null;
    }

    private void GenerateNewMap()
    {
        if (randomSeed != 0)
            Random.InitState(randomSeed);
        else
            Random.InitState(System.Environment.TickCount);

        GameObject obj = new GameObject("NodeMap_Debug");
        obj.transform.SetParent(transform);
        currentMap = obj.AddComponent<NodeMap>();

        if (nodeMapPrefab != null)
        {
            currentMap.GetType().GetFields().ToList().ForEach(field =>
            {
                if (field.IsPublic && field.FieldType == typeof(int))
                    field.SetValue(currentMap, field.GetValue(nodeMapPrefab));
            });
        }

        currentMap.Generate();
        testMaps.Add(currentMap);
    }

    private void RunMultipleGenerations(int count)
    {
        Debug.Log($"Running {count} map generation tests...");

        int validCount = 0;
        var allNodeCounts = new List<int>();
        var allConnCounts = new List<int>();
        var allOutDegrees = new List<float>();
        var allMergeRatios = new List<float>();
        var allIsolatedNodes = new List<int>();
        var allEmptyFloors = new List<int>();
        var allDurations = new List<float>();

        for (int i = 0; i < count; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            Random.InitState(System.DateTime.Now.Millisecond + i * 997);
            GameObject obj = new GameObject($"NodeMap_Test_{i}");
            obj.transform.SetParent(transform);
            NodeMap map = obj.AddComponent<NodeMap>();
            map.Generate();
            sw.Stop();

            testMaps.Add(map);

            bool valid = ValidateMapStructure(map, out int nodes, out int connections);
            if (valid) validCount++;

            allNodeCounts.Add(nodes);
            allConnCounts.Add(connections);
            allOutDegrees.Add(nodes > 0 ? (float)connections / nodes : 0);
            allDurations.Add(sw.ElapsedMilliseconds);

            // Merge ratio: nodes with multiple parents / total nodes
            int multiParentNodes = map.AllNodes.Count(n =>
            {
                int incoming = map.Floors.Values
                    .SelectMany(f => f)
                    .Count(p => p.NextNodes.Contains(n));
                return incoming >= 2;
            });
            allMergeRatios.Add(map.AllNodes.Count() > 0
                ? (float)multiParentNodes / map.AllNodes.Count()
                : 0);

            // Isolated nodes (should be 0)
            int isolated = map.AllNodes.Count(n =>
            {
                bool incoming = map.Floors.Values.Any(f => f.Any(p => p.NextNodes.Contains(n)));
                bool outgoing = n.NextNodes.Count > 0;
                return !incoming && !outgoing;
            });
            allIsolatedNodes.Add(isolated);

            // Empty floors (should be 0)
            int empties = map.Floors.Values.Count(f => f.Count == 0);
            allEmptyFloors.Add(empties);
        }

        // Summary stats
        string report =
            "========== MAP GENERATION REPORT ==========\n" +
            $"Total Runs: {count}\n" +
            $"Valid Maps: {validCount}/{count}  ({(float)validCount / count * 100f:F1}%)\n" +
            "------------------------------------------\n" +
            $"Avg Nodes:        {allNodeCounts.Average():F1}\n" +
            $"Avg Connections:  {allConnCounts.Average():F1}\n" +
            $"Avg Out-Degree:   {allOutDegrees.Average():F2}\n" +
            $"Avg Merge Ratio:  {allMergeRatios.Average() * 100f:F1}%\n" +
            $"Avg Isolated:     {allIsolatedNodes.Average():F2}\n" +
            $"Avg Empty Floors: {allEmptyFloors.Average():F2}\n" +
            $"Avg Time:         {allDurations.Average():F1} ms\n" +
            "------------------------------------------\n" +
            $"Min Nodes: {allNodeCounts.Min()}   Max Nodes: {allNodeCounts.Max()}\n" +
            $"Min Conns: {allConnCounts.Min()}   Max Conns: {allConnCounts.Max()}\n" +
            "==========================================";

        Debug.Log(report);
    }

    private void ValidateAndReport(NodeMap map)
    {
        bool valid = ValidateMapStructure(map, out int nodes, out int connections);

        Debug.Log(
            "Single Map Report:\n" +
            $"- Nodes: {nodes}\n" +
            $"- Connections: {connections}\n" +
            $"- Structure Valid: {valid}"
        );
    }

    private bool ValidateMapStructure(NodeMap map, out int nodeCount, out int connectionCount)
    {
        nodeCount = map.AllNodes.Count();
        connectionCount = map.AllNodes.Sum(n => n.NextNodes.Count);
        bool success = true;

        for (int y = 0; y < map.Floors.Count; y++)
        {
            var floor = map.Floors[y];
            if (floor.Count == 0)
            {
                Debug.LogError($"Floor {y} is empty.");
                success = false;
            }

            foreach (var node in floor)
            {
                bool hasIncoming = y == 0 || map.Floors[y - 1].Any(p => p.NextNodes.Contains(node));
                bool hasOutgoing = y == map.MapHeight - 1 || node.NextNodes.Count > 0;

                if (!hasIncoming)
                {
                    Debug.LogError($"Invalid: Node {node.GridPos} has no incoming connection.");
                    success = false;
                }
                if (!hasOutgoing)
                {
                    Debug.LogError($"Invalid: Node {node.GridPos} has no outgoing connection.");
                    success = false;
                }
            }
        }

        return success;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || currentMap == null || currentMap.Grid == null)
            return;

        Gizmos.color = nodeColor;

        foreach (var node in currentMap.AllNodes)
        {
            Vector3 pos = currentMap.Grid.GridToWorld(node.GridPos.x, node.GridPos.y);
            Gizmos.DrawSphere(pos, nodeRadius);

            Gizmos.color = lineColor;
            foreach (var next in node.NextNodes)
            {
                Vector3 nextPos = currentMap.Grid.GridToWorld(next.GridPos.x, next.GridPos.y);
                Gizmos.DrawLine(pos, nextPos);
            }

            Gizmos.color = nodeColor;
        }
    }
}
#endif
