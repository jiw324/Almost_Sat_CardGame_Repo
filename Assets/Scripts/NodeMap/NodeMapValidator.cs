using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeMapValidator
{
    private readonly NodeMap _nodeMap;
    private readonly NodeFactory _nodeFactory;

    private Dictionary<NodeType, float> _lateFloorWeights;
    private Dictionary<NodeType, float> _earlyFloorWeights;
    private Dictionary<int, NodeType> _specialFloorAssignments;

    public NodeMapValidator(NodeMap nodeMap, NodeFactory nodeFactory)
    {
        _nodeMap = nodeMap;
        _nodeFactory = nodeFactory;
        InitializeNodeWeights();
    }

    private void InitializeNodeWeights()
    {
        _specialFloorAssignments = new Dictionary<int, NodeType>
        {
            { 1, NodeType.Combat },
            { Mathf.RoundToInt(_nodeMap.mapHeight / 2), NodeType.Loot },
            { _nodeMap.mapHeight, NodeType.Rest }
        };

        float combatWeight = 0.50f;
        float eventWeight  = 0.25f;
        float restWeight   = 0.15f;
        float shopWeight   = 0.09f;
        float lootWeight   = 0.01f;

        _lateFloorWeights = new Dictionary<NodeType, float>
        {
            { NodeType.Combat, combatWeight },
            { NodeType.Event,  eventWeight  },
            { NodeType.Rest,   restWeight   },
            { NodeType.Shop,   shopWeight   },
            { NodeType.Loot,   lootWeight   }
        };

        combatWeight = 0.60f;
        eventWeight  = 0.30f;
        shopWeight   = 0.10f;

        _earlyFloorWeights = new Dictionary<NodeType, float>
        {
            { NodeType.Combat, combatWeight },
            { NodeType.Event,  eventWeight  },
            { NodeType.Shop,   shopWeight   }
        };
    }

    public NodeType GetNodeType(int mapFloor)
    {
        Dictionary<NodeType, float> floorWeights =
            mapFloor < Mathf.RoundToInt(_nodeMap.mapHeight / 2)
                ? _earlyFloorWeights
                : _lateFloorWeights;

        if (_specialFloorAssignments.ContainsKey(mapFloor))
            return _specialFloorAssignments[mapFloor];

        float totalWeight = floorWeights.Values.Sum();
        float roll = Random.value * totalWeight;

        float cumulative = 0f;
        foreach (var weightPair in floorWeights)
        {
            cumulative += weightPair.Value;
            if (roll <= cumulative)
                return weightPair.Key;
        }

        return floorWeights.Keys.FirstOrDefault();
    }

    private NodeType GetNodeTypeExcluding(int mapFloor, params NodeType[] excluded)
    {
        NodeType newType;
        int attempts = 0;
        do
        {
            newType = GetNodeType(mapFloor);
            attempts++;
        } while (excluded.Contains(newType) && attempts < 20);
        return newType;
    }

    // ---------------- Report structures ----------------
    public class NodeValidationChange
    {
        public int Floor { get; }
        public int Column { get; }
        public NodeType OldType { get; }
        public NodeType NewType { get; }
        public string Reason { get; }

        public NodeValidationChange(int floor, int column, NodeType oldType, NodeType newType, string reason)
        {
            Floor  = floor;
            Column = column;
            OldType = oldType;
            NewType = newType;
            Reason  = reason;
        }

        public override string ToString()
        {
            return $"Floor {Floor + 1}, X:{Column} | {OldType} → {NewType} ({Reason})";
        }
    }

    public class NodeMapValidationReport
    {
        public bool Success { get; set; }
        public int PassCount { get; set; }
        public List<NodeValidationChange> Changes { get; set; } = new();

        public override string ToString()
        {
            if (Changes.Count == 0)
                return $"Validation completed successfully in {PassCount} pass(es).";
            string header = $"Validation completed in {PassCount} pass(es). {Changes.Count} changes:\n";
            return header + string.Join("\n", Changes.Select(c => " - " + c.ToString()));
        }
    }

    public NodeMapValidationReport ValidateMap(bool autoFix = true, int maxPasses = 5)
    {
        Debug.Log("Validating generated map");
        var report = new NodeMapValidationReport();

        for (int pass = 0; pass < maxPasses; pass++)
        {
            int changesMadeThisPass = 0;
            HashSet<INode> checkedNodesThisPass = new();

            // ---------- Phase 1: Placement rules ----------
            for (int floorIndex = 0; floorIndex < _nodeMap.mapHeight; floorIndex++)
            {
                var floorNodes = _nodeMap.nodes[floorIndex];
                if (floorNodes == null) continue;

                for (int columnIndex = 0; columnIndex < floorNodes.Length; columnIndex++)
                {
                    var currentNode = floorNodes[columnIndex];
                    if (currentNode == null) continue;

                    var nodeType = currentNode.nodeDefinition.nodeType;

                    // Rule 1: No Rest nodes before halfway
                    if (nodeType == NodeType.Rest && floorIndex < _nodeMap.mapHeight / 2)
                    {
                        if (autoFix)
                        {
                            var replacementType = NodeType.Combat;
                            currentNode.ReassignDefinition(_nodeFactory.GetDefinition(replacementType));
                            report.Changes.Add(new NodeValidationChange(floorIndex, columnIndex, nodeType, replacementType, "Rest below halfway"));
                            changesMadeThisPass++;
                        }
                        else { report.Success = false; }
                    }

                    // Rule 2: No Rest nodes on second-to-last floor
                    if (floorIndex == _nodeMap.mapHeight - 2 && currentNode.nodeDefinition.nodeType == NodeType.Rest)
                    {
                        if (autoFix)
                        {
                            var replacementType = NodeType.Combat;
                            currentNode.ReassignDefinition(_nodeFactory.GetDefinition(replacementType));
                            report.Changes.Add(new NodeValidationChange(floorIndex, columnIndex, nodeType, replacementType, "Rest on 2nd-to-last floor"));
                            changesMadeThisPass++;
                        }
                        else { report.Success = false; }
                    }
                }
            }

            // ---------- Phase 2: Consecutive identical Shop/Rest nodes ----------
            for (int floorIndex = 0; floorIndex < _nodeMap.mapHeight - 1; floorIndex++)
            {
                var floorNodes = _nodeMap.nodes[floorIndex];
                if (floorNodes == null) continue;

                for (int columnIndex = 0; columnIndex < floorNodes.Length; columnIndex++)
                {
                    var parentNode = floorNodes[columnIndex];
                    if (parentNode == null) continue;

                    var parentType = parentNode.nodeDefinition.nodeType;
                    var nextNodes = parentNode.nextNodes;

                    for (int i = 0; i < nextNodes.Length; i++)
                    {
                        var childNode = nextNodes[i];
                        if (childNode == null) continue;

                        var childType = childNode.nodeDefinition.nodeType;

                        bool identicalConsecutive =
                            (parentType == NodeType.Rest && childType == NodeType.Rest) ||
                            (parentType == NodeType.Shop && childType == NodeType.Shop);

                        if (identicalConsecutive)
                        {
                            if (autoFix)
                            {
                                var replacementType = NodeType.Combat;
                                if (childType != replacementType)
                                {
                                    childNode.ReassignDefinition(_nodeFactory.GetDefinition(replacementType));
                                    report.Changes.Add(
                                        new NodeValidationChange(
                                            floorIndex + 1, columnIndex, childType, replacementType,
                                            "Consecutive identical Shop/Rest pair"));
                                    changesMadeThisPass++;
                                }
                            }
                            else { report.Success = false; }
                        }
                    }
                }
            }

            // ---------- Phase 3: Prevent duplicate neighbor types per parent ----------
            for (int floorIndex = 0; floorIndex < _nodeMap.mapHeight - 1; floorIndex++)
            {
                var floorNodes = _nodeMap.nodes[floorIndex];
                if (floorNodes == null) continue;

                for (int columnIndex = 0; columnIndex < floorNodes.Length; columnIndex++)
                {
                    var parentNode = floorNodes[columnIndex];
                    if (parentNode == null) continue;

                    var childNodes = parentNode.nextNodes.Where(n => n != null).ToList();
                    if (childNodes.Count < 2) continue;

                    var groupedChildren = childNodes
                        .GroupBy(n => n.nodeDefinition.nodeType)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var group in groupedChildren)
                    {
                        var nodeType = group.Key;
                        var duplicates = group.Value;

                        // Allow duplicates for Combat, Event, or special-floor nodes
                        bool isSpecialFloorType = duplicates.Any(child =>
                        {
                            int floor = GetNodeFloor(child);
                            return _specialFloorAssignments.TryGetValue(floor + 1, out var specialType) && specialType == nodeType;
                        });

                        if (nodeType == NodeType.Combat || nodeType == NodeType.Event || isSpecialFloorType)
                            continue;

                        if (duplicates.Count <= 1)
                            continue;

                        for (int dupIndex = 1; dupIndex < duplicates.Count; dupIndex++)
                        {
                            var duplicateNode = duplicates[dupIndex];
                            var oldType = duplicateNode.nodeDefinition.nodeType;

                            if (autoFix)
                            {
                                if (oldType != NodeType.Combat)
                                {
                                    duplicateNode.ReassignDefinition(_nodeFactory.GetDefinition(NodeType.Combat));
                                    report.Changes.Add(
                                        new NodeValidationChange(
                                            floorIndex + 1, columnIndex, oldType, NodeType.Combat,
                                            "Duplicate neighbor type (non-Combat/Event/SpecialFloor)"));
                                    changesMadeThisPass++;
                                }
                            }
                            else { report.Success = false; }
                        }
                    }
                }
            }

            // --- Final cleanup: enforce Rest restriction on 2nd-to-last floor ---
            int secondToLastFloor = _nodeMap.mapHeight - 2;
            var secondToLastFloorNodes = _nodeMap.nodes[secondToLastFloor];
            if (secondToLastFloorNodes != null)
            {
                for (int columnIndex = 0; columnIndex < secondToLastFloorNodes.Length; columnIndex++)
                {
                    var node = secondToLastFloorNodes[columnIndex];
                    if (node != null && node.nodeDefinition.nodeType == NodeType.Rest)
                    {
                        var replacementType = NodeType.Combat;
                        node.ReassignDefinition(_nodeFactory.GetDefinition(replacementType));
                        report.Changes.Add(new NodeValidationChange(secondToLastFloor, columnIndex, NodeType.Rest, replacementType, "Final cleanup: Rest on 2nd-to-last floor"));
                        changesMadeThisPass++;
                    }
                }
            }

            report.PassCount = pass + 1;

            if (changesMadeThisPass == 0)
            {
                report.Success = true;
                return report;
            }
        }

        report.Success = false;
        Debug.LogError("NodeMap could not stabilize after maximum passes.");
        return report;
    }

    private int GetNodeFloor(INode node)
    {
        for (int floorIndex = 0; floorIndex < _nodeMap.mapHeight; floorIndex++)
        {
            if (_nodeMap.nodes[floorIndex].Contains(node))
                return floorIndex;
        }
        return -1;
    }
}
