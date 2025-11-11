using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ValidationModule : IMapTestModule
{
    public string ModuleName => "Node Map Validation";

    private int _totalRuns;
    private int _preValid;
    private int _postValid;
    private int _postValidNoWarn;
    private int _totalIssues;
    private int _totalWarnings;
    private int _mapsWithFixes;

    private readonly Dictionary<string, int> _ruleHitTotals = new();
    private readonly Dictionary<int, int> _passHistogram = new(); // pass count distribution

    private readonly List<string> _lastIssues = new();
    private readonly List<string> _lastWarnings = new();

    public void Reset()
    {
        _totalRuns = 0;
        _preValid = 0;
        _postValid = 0;
        _postValidNoWarn = 0;
        _totalIssues = 0;
        _totalWarnings = 0;
        _mapsWithFixes = 0;

        _ruleHitTotals.Clear();
        _passHistogram.Clear();
        _lastIssues.Clear();
        _lastWarnings.Clear();
    }

    public void Run(NodeMap map)
    {
        _totalRuns++;

        var preValidator = new NodeMapValidator();
        var preResult = preValidator.Validate(map);
        bool preHadIssues = preResult.Issues.Count > 0 || preResult.Warnings.Count > 0;
        if (!preHadIssues)
            _preValid++;

        // Main validation
        var validator = new NodeMapValidator();
        var result = validator.Validate(map);

        // Track how many passes were used
        int passes = validator.PassesUsed;
        if (!_passHistogram.ContainsKey(passes))
            _passHistogram[passes] = 0;
        _passHistogram[passes]++;

        bool postHadIssues = result.Issues.Count > 0 || result.Warnings.Count > 0;
        bool postHadIssuesOnly = result.Issues.Count > 0;

        if (!postHadIssues)
            _postValid++;
        if (!postHadIssuesOnly)
            _postValidNoWarn++;

        _totalIssues += result.Issues.Count;
        _totalWarnings += result.Warnings.Count;
        if (result.Issues.Count > 0)
            _mapsWithFixes++;

        foreach (var kvp in validator.RuleHits)
            _ruleHitTotals[kvp.Key] = _ruleHitTotals.TryGetValue(kvp.Key, out int v) ? v + kvp.Value : kvp.Value;

        _lastIssues.Clear();
        _lastWarnings.Clear();
        _lastIssues.AddRange(result.Issues);
        _lastWarnings.AddRange(result.Warnings);
    }

    public string GetReport()
    {
        if (_totalRuns == 0)
            return $"[{ModuleName}] No runs performed.";

        float prePercent = (float)_preValid / _totalRuns * 100f;
        float postPercent = (float)_postValid / _totalRuns * 100f;
        float postNoWarnPercent = (float)_postValidNoWarn / _totalRuns * 100f;
        float avgPasses = _passHistogram.Sum(p => p.Key * p.Value) / (float)_totalRuns;

        string report =
            $"========== {ModuleName.ToUpper()} ==========\n" +
            $"Total Runs: {_totalRuns}\n" +
            $"------------------------------------------\n" +
            $"Pre-Validation Valid:   {_preValid}/{_totalRuns} ({prePercent:F1}%)\n" +
            $"Post-Validation Valid:  {_postValid}/{_totalRuns} ({postPercent:F1}%)\n" +
            $"Post-Valid (No Warns):  {_postValidNoWarn}/{_totalRuns} ({postNoWarnPercent:F1}%)\n" +
            $"------------------------------------------\n" +
            $"Total Issues Fixed: {_totalIssues}\n" +
            $"Total Warnings:     {_totalWarnings}\n" +
            $"Maps with Fixes:    {_mapsWithFixes}\n" +
            $"Average Passes:     {avgPasses:F2}\n";

        if (_passHistogram.Count > 0)
        {
            report += "\nPass Distribution:\n";
            foreach (var kvp in _passHistogram.OrderBy(k => k.Key))
            {
                float pct = (float)kvp.Value / _totalRuns * 100f;
                report += $" • {kvp.Key} pass(es): {kvp.Value} maps ({pct:F1}%)\n";
            }
        }

        if (_ruleHitTotals.Count > 0)
        {
            report += "\nRule Fix Frequency (all runs):\n";
            foreach (var kvp in _ruleHitTotals.OrderByDescending(k => k.Value))
                report += $" • {kvp.Key}: {kvp.Value}\n";
        }

        if (_lastIssues.Count > 0)
        {
            report += "\nLast Run Fixes:\n";
            foreach (var i in _lastIssues.Take(6))
                report += $" • {i}\n";
            if (_lastIssues.Count > 6)
                report += $"   (+{_lastIssues.Count - 6} more)\n";
        }

        if (_lastWarnings.Count > 0)
        {
            report += "\nLast Run Warnings:\n";
            foreach (var w in _lastWarnings.Take(4))
                report += $" • {w}\n";
            if (_lastWarnings.Count > 4)
                report += $"   (+{_lastWarnings.Count - 4} more)\n";
        }

        report += "==========================================";
        return report;
    }
}
