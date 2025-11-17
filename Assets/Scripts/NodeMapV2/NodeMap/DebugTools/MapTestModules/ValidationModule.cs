using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ValidationModule : IMapTestModule
{
    public string ModuleName => "Node Map Validation";

    private int _totalRuns;
    private int _postValid;
    private int _mapsWithFixes;

    private readonly Dictionary<string, int> _ruleHitTotals = new();
    private readonly Dictionary<int, int> _passHistogram = new();
    private readonly Dictionary<int, int> _regenAttemptsHistogram = new();

    private readonly List<string> _lastIssues = new();

    public void Reset()
    {
        _totalRuns = 0;
        _postValid = 0;
        _mapsWithFixes = 0;

        _ruleHitTotals.Clear();
        _passHistogram.Clear();
        _regenAttemptsHistogram.Clear();
        _lastIssues.Clear();
    }

    public void Run(NodeMap map)
    {
        _totalRuns++;
    }

    public void RecordValidationStats(
        bool isValid,
        bool hadFixes,
        int passes,
        Dictionary<string, int> ruleHits,
        string message)
    {
        if (isValid)
            _postValid++;

        if (hadFixes)
            _mapsWithFixes++;

        if (!_passHistogram.ContainsKey(passes))
            _passHistogram[passes] = 0;
        _passHistogram[passes]++;

        foreach (var kvp in ruleHits)
            _ruleHitTotals[kvp.Key] = _ruleHitTotals.TryGetValue(kvp.Key, out int v) ? v + kvp.Value : kvp.Value;

        _lastIssues.Clear();
        if (!string.IsNullOrEmpty(message))
            _lastIssues.Add(message);
    }

    public void RecordRegenerationAttempts(int attempts)
    {
        if (!_regenAttemptsHistogram.ContainsKey(attempts))
            _regenAttemptsHistogram[attempts] = 0;
        _regenAttemptsHistogram[attempts]++;
    }

    public string GetReport()
    {
        if (_totalRuns == 0)
            return $"[{ModuleName}] No runs performed.";

        float postPercent = (float)_postValid / _totalRuns * 100f;

        string report =
            $"========== {ModuleName.ToUpper()} ==========\n" +
            $"Total Runs: {_totalRuns}\n" +
            $"------------------------------------------\n" +
            $"Post-Validation Valid:  {_postValid}/{_totalRuns} ({postPercent:F1}%)\n" +
            $"------------------------------------------\n" +
            $"Maps with Fixes:    {_mapsWithFixes}\n";

        if (_passHistogram.Count > 0)
        {
            report += "\nPass Distribution:\n";
            foreach (var kvp in _passHistogram.OrderBy(k => k.Key))
            {
                float pct = (float)kvp.Value / _totalRuns * 100f;
                report += $" • {kvp.Key} pass(es): {kvp.Value} maps ({pct:F1}%)\n";
            }
        }

        if (_regenAttemptsHistogram.Count > 0)
        {
            report += "\nRegeneration Attempts:\n";
            foreach (var kvp in _regenAttemptsHistogram.OrderBy(k => k.Key))
            {
                float pct = (float)kvp.Value / _totalRuns * 100f;
                report += $" • {kvp.Key} attempt(s): {kvp.Value} maps ({pct:F1}%)\n";
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

        report += "==========================================";
        return report;
    }
}
