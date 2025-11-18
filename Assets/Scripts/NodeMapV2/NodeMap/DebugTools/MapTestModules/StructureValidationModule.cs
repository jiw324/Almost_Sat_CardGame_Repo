using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StructureValidationModule : IMapTestModule
{
    public string ModuleName => "Structure Validation";

    private int _totalRuns;
    private int _validCount;
    private List<int> _nodeCounts = new();
    private List<int> _connCounts = new();
    private List<float> _outDegrees = new();
    private List<float> _mergeRatios = new();
    private List<int> _isolatedNodes = new();
    private List<int> _emptyFloors = new();
    private List<float> _durations = new();

    public void Reset()
    {
        _totalRuns = 0;
        _validCount = 0;
        _nodeCounts.Clear();
        _connCounts.Clear();
        _outDegrees.Clear();
        _mergeRatios.Clear();
        _isolatedNodes.Clear();
        _emptyFloors.Clear();
        _durations.Clear();
    }

    public void Run(NodeMap map)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        bool valid = ValidateMapStructure(map,
            out int nodes,
            out int connections,
            out int isolated,
            out int empties,
            out float mergeRatio);

        sw.Stop();

        _totalRuns++;
        if (valid) _validCount++;
        _nodeCounts.Add(nodes);
        _connCounts.Add(connections);
        _outDegrees.Add(nodes > 0 ? (float)connections / nodes : 0);
        _isolatedNodes.Add(isolated);
        _emptyFloors.Add(empties);
        _mergeRatios.Add(mergeRatio);
        _durations.Add(sw.ElapsedMilliseconds);
    }

    public string GetReport()
    {
        if (_totalRuns == 0)
            return $"[{ModuleName}] No test runs recorded.";

        string report =
            $"========== {ModuleName.ToUpper()} ==========\n" +
            $"Total Runs: {_totalRuns}\n" +
            $"Valid Maps: {_validCount}/{_totalRuns} ({(float)_validCount / _totalRuns * 100f:F1}%)\n" +
            $"------------------------------------------\n" +
            $"Avg Nodes:        {_nodeCounts.Average():F1}\n" +
            $"Avg Connections:  {_connCounts.Average():F1}\n" +
            $"Avg Out-Degree:   {_outDegrees.Average():F2}\n" +
            $"Avg Merge Ratio:  {_mergeRatios.Average() * 100f:F1}%\n" +
            $"Avg Isolated:     {_isolatedNodes.Average():F2}\n" +
            $"Avg Empty Floors: {_emptyFloors.Average():F2}\n" +
            $"Avg Time:         {_durations.Average():F1} ms\n" +
            $"------------------------------------------\n" +
            $"Min Nodes: {_nodeCounts.Min()}   Max Nodes: {_nodeCounts.Max()}\n" +
            $"Min Conns: {_connCounts.Min()}   Max Conns: {_connCounts.Max()}\n" +
            $"==========================================";

        return report;
    }

    private bool ValidateMapStructure(NodeMap map,
        out int nodeCount,
        out int connectionCount,
        out int isolatedCount,
        out int emptyFloors,
        out float mergeRatio)
    {
        nodeCount = map.AllNodes.Count();
        connectionCount = map.AllNodes.Sum(n => n.NextNodes.Count);
        isolatedCount = 0;
        emptyFloors = 0;
        bool success = true;

        for (int y = 0; y < map.Floors.Count; y++)
        {
            var floor = map.Floors[y];
            if (floor.Count == 0)
            {
                emptyFloors++;
                success = false;
            }

            foreach (var node in floor)
            {
                bool hasIncoming = y == 0 || map.Floors[y - 1].Any(p => p.NextNodes.Contains(node));
                bool hasOutgoing = y == map.MapHeight - 1 || node.NextNodes.Count > 0;

                if (!hasIncoming || !hasOutgoing)
                    success = false;

                if (!hasIncoming && !hasOutgoing)
                    isolatedCount++;
            }
        }

        // Merge ratio: nodes with 2+ incoming edges / total nodes
        int multiParentNodes = map.AllNodes.Count(n =>
        {
            int incoming = map.Floors.Values
                .SelectMany(f => f)
                .Count(p => p.NextNodes.Contains(n));
            return incoming >= 2;
        });

        mergeRatio = nodeCount > 0
            ? (float)multiParentNodes / nodeCount
            : 0f;

        return success;
    }
}
