using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeTypeDistributionModule : IMapTestModule
{
    public string ModuleName => "Node Type Distribution";

    private int _totalRuns;
    private int _unassignedNodes;
    private Dictionary<NodeType, int> _typeCounts = new();
    private int _totalAssignedNodes;

    public void Reset()
    {
        _totalRuns = 0;
        _unassignedNodes = 0;
        _typeCounts.Clear();
        _totalAssignedNodes = 0;
    }

    public void Run(NodeMap map)
    {
        _totalRuns++;

        foreach (var node in map.AllNodes)
        {
            if (!node.HasDefinition())
            {
                _unassignedNodes++;
                continue;
            }

            NodeType type = node.Definition.nodeType;
            if (!_typeCounts.ContainsKey(type))
                _typeCounts[type] = 0;

            _typeCounts[type]++;
            _totalAssignedNodes++;
        }
    }

    public string GetReport()
    {
        if (_totalRuns == 0)
            return $"[{ModuleName}] No runs performed.";

        if (_totalAssignedNodes == 0)
            return $"[{ModuleName}] No nodes were assigned definitions.";

        float totalNodes = _totalAssignedNodes + _unassignedNodes;
        string report =
            $"========== {ModuleName.ToUpper()} ==========\n" +
            $"Total Runs: {_totalRuns}\n" +
            $"Total Assigned Nodes: {_totalAssignedNodes}\n" +
            $"Unassigned Nodes: {_unassignedNodes} ({_unassignedNodes / totalNodes * 100f:F1}%)\n" +
            $"------------------------------------------\n";

        // Compute distribution
        foreach (var kvp in _typeCounts.OrderBy(k => k.Key.ToString()))
        {
            float percentage = (float)kvp.Value / _totalAssignedNodes * 100f;
            report += $"{kvp.Key}: {kvp.Value} ({percentage:F1}%)\n";
        }

        report += "==========================================";

        return report;
    }
}
