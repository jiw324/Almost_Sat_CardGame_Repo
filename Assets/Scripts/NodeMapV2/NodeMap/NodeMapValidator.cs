using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeMapValidator
{
    private const int MAX_FLOORS_BETWEEN_RESTS = 3;
    private const int MAX_FLOORS_BETWEEN_SHOPS = 6;
    private const float MAX_EVENT_PERCENT = 0.4f;
    private const int MAX_PASSES = 5;

    private readonly Dictionary<string, int> _ruleHits = new();
    public IReadOnlyDictionary<string, int> RuleHits => _ruleHits;
    public int PassesUsed { get; private set; } = 1;

    private void Count(string ruleName)
    {
        if (!_ruleHits.ContainsKey(ruleName))
            _ruleHits[ruleName] = 0;
        _ruleHits[ruleName]++;
    }

    public class ValidationResult
    {
        public bool IsValid = true;
        public List<string> Issues = new();
        public List<string> Warnings = new();
    }

    public ValidationResult Validate(NodeMap map)
    {
        var result = new ValidationResult();

        int top = map.MapHeight - 1;
        int mid = Mathf.RoundToInt((map.MapHeight - 1) / 2f);

        for (int pass = 0; pass < MAX_PASSES; pass++)
        {
            int issuesBefore = result.Issues.Count;

            EnforceTypeBalance(map, result);
            FixStructural(map, result);
            EnforceTypePlacement(map, result);
            EnforceRestFrequency(map, result);
            EnforceFlowRules(map, result, top, mid);
            EnforceShopFrequency(map, result);

            // Always reassert anchor floors last
            EnsureAnchorFloorsFinal(map, result);

            WarnPatternVariety(map, result);

            int issuesAfter = result.Issues.Count;
            PassesUsed = pass + 1;
            Count($"ValidationPass{pass + 1}");

            if (issuesAfter == issuesBefore)
                break;

            if (pass == MAX_PASSES - 1 && issuesAfter != issuesBefore)
            {
                Debug.LogWarning($"[Validator] Map failed to stabilize after {MAX_PASSES} passes.");
            }
        }

        return result;
    }

    // ---------- STRUCTURAL ----------
    private void FixStructural(NodeMap map, ValidationResult r)
    {
        for (int y = 0; y < map.MapHeight; y++)
        {
            if (!map.Floors.ContainsKey(y) || map.Floors[y].Count == 0)
            {
                Count("EmptyFloorFix");
                r.Issues.Add($"Added fallback Combat node for empty floor {y}.");
                var node = new Node(map.Factory.GetDefinition(NodeType.Combat), new Vector2Int(0, y));
                map.Floors[y] = new List<Node> { node };
                r.IsValid = false;
            }

            foreach (var node in map.Floors[y])
            {
                bool hasIncoming = y == 0 || map.Floors[y - 1].Any(p => p.NextNodes.Contains(node));
                bool hasOutgoing = y == map.MapHeight - 1 || node.NextNodes.Count > 0;

                if (!hasIncoming && y > 0)
                {
                    Count("MissingIncomingFix");
                    var parent = map.Floors[y - 1].OrderBy(p => Mathf.Abs(p.GridPos.x - node.GridPos.x)).First();
                    parent.ConnectTo(node);
                    r.Issues.Add($"Fixed missing incoming for {node.GridPos}");
                }

                if (!hasOutgoing && y < map.MapHeight - 1)
                {
                    Count("MissingOutgoingFix");
                    var child = map.Floors[y + 1].OrderBy(n => Mathf.Abs(n.GridPos.x - node.GridPos.x)).First();
                    node.ConnectTo(child);
                    r.Issues.Add($"Fixed missing outgoing for {node.GridPos}");
                }
            }
        }
    }

    // ---------- TYPE PLACEMENT ----------
    private void EnforceTypePlacement(NodeMap map, ValidationResult r)
    {
        int top = map.MapHeight - 1;
        int mid = Mathf.RoundToInt((map.MapHeight - 1) / 2f);

        // Skip anchors — those are handled separately
        for (int y = 0; y < map.MapHeight; y++)
        {
            if (y == 0 || y == mid || y == top) continue;
            foreach (var node in map.Floors[y])
            {
                if (node.Definition == null)
                {
                    node.Reassign(map.Factory.GetDefinition(NodeType.Combat));
                    r.Issues.Add($"Filled missing definition on floor {y}.");
                }
            }
        }
    }

    // ---------- FLOW RULES ----------
    private void EnforceFlowRules(NodeMap map, ValidationResult r, int top, int mid)
    {
        for (int y = 1; y < map.MapHeight; y++)
        {
            var prevTypes = map.Floors[y - 1].Select(n => n.Definition.nodeType).ToList();
            var curr = map.Floors[y];

            bool isAnchor = (y == 0 || y == mid || y == top);
            if (isAnchor) continue;

            // Rest -> Rest
            if (prevTypes.Contains(NodeType.Rest) && y < top)
            {
                foreach (var n in curr.Where(n => n.Definition.nodeType == NodeType.Rest))
                {
                    Count("NoRestChainFix");
                    NodeType newType = Random.value < 0.65f ? NodeType.Event : NodeType.Combat;
                    n.Reassign(map.Factory.GetDefinition(newType));
                    r.Issues.Add($"Changed Rest at floor {y} to {newType} (avoided Rest chain).");
                }
            }

            // Shop -> Shop
            if (prevTypes.Contains(NodeType.Shop) && y < top)
            {
                foreach (var n in curr.Where(n => n.Definition.nodeType == NodeType.Shop))
                {
                    Count("NoShopChainFix");
                    NodeType newType = Random.value < 0.65f ? NodeType.Event : NodeType.Combat;
                    n.Reassign(map.Factory.GetDefinition(newType));
                    r.Issues.Add($"Changed Shop at floor {y} to {newType} (avoided Shop chain).");
                }
            }

            // Loot -> Loot
            if (prevTypes.Contains(NodeType.Loot) && y < top)
            {
                foreach (var n in curr.Where(n => n.Definition.nodeType == NodeType.Loot))
                {
                    Count("NoLootChainFix");
                    NodeType newType = Random.value < 0.65f ? NodeType.Event : NodeType.Combat;
                    n.Reassign(map.Factory.GetDefinition(newType));
                    r.Issues.Add($"Changed Loot at floor {y} to {newType} (avoided Loot chain).");
                }
            }
        }
    }

    // ---------- REST FREQUENCY ----------
    private void EnforceRestFrequency(NodeMap map, ValidationResult r)
    {
        int lastRest = -MAX_FLOORS_BETWEEN_RESTS;
        for (int y = 0; y < map.MapHeight; y++)
        {
            var types = map.Floors[y].Select(n => n.Definition.nodeType);
            if (types.Contains(NodeType.Rest)) lastRest = y;

            if (y < map.MapHeight - 1 && y - lastRest > MAX_FLOORS_BETWEEN_RESTS)
            {
                Count("RestSpacingFix");
                var target = map.Floors[y][Random.Range(0, map.Floors[y].Count)];
                target.Reassign(map.Factory.GetDefinition(NodeType.Rest));
                r.Issues.Add($"Added Rest at floor {y} to maintain spacing.");
                lastRest = y;
            }
        }
    }

    // ---------- SHOP FREQUENCY ----------
    private void EnforceShopFrequency(NodeMap map, ValidationResult r)
    {
        int lastShop = -MAX_FLOORS_BETWEEN_SHOPS;
        for (int y = 0; y < map.MapHeight; y++)
        {
            var types = map.Floors[y].Select(n => n.Definition.nodeType);
            if (types.Contains(NodeType.Shop)) lastShop = y;

            bool prevHadShop = y > 0 && map.Floors[y - 1].Any(n => n.Definition.nodeType == NodeType.Shop);
            if (!prevHadShop && y - lastShop > MAX_FLOORS_BETWEEN_SHOPS)
            {
                Count("ShopSpacingFix");
                var target = map.Floors[y][Random.Range(0, map.Floors[y].Count)];
                target.Reassign(map.Factory.GetDefinition(NodeType.Shop));
                r.Issues.Add($"Added Shop at floor {y} to maintain spacing.");
                lastShop = y;
            }
        }
    }

    // ---------- TYPE BALANCE ----------
    private void EnforceTypeBalance(NodeMap map, ValidationResult r)
    {
        var all = map.AllNodes.ToList();
        int total = all.Count;
        if (total == 0) return;

        int events = all.Count(n => n.Definition.nodeType == NodeType.Event);
        if ((float)events / total > MAX_EVENT_PERCENT)
        {
            int toConvert = events - Mathf.RoundToInt(total * MAX_EVENT_PERCENT);
            var eventNodes = all.Where(n =>
                n.Definition.nodeType == NodeType.Event &&
                n.GridPos.y != 0 &&
                n.GridPos.y != Mathf.RoundToInt((map.MapHeight - 1) / 2f) &&
                n.GridPos.y != map.MapHeight - 1)
                .OrderBy(_ => Random.value)
                .Take(toConvert);

            foreach (var n in eventNodes)
            {
                Count("EventCapFix");
                NodeType newType = Random.value < 0.7f ? NodeType.Combat : NodeType.Rest;
                n.Reassign(map.Factory.GetDefinition(newType));
            }

            r.Issues.Add($"Reduced Events by {toConvert} to maintain <= {MAX_EVENT_PERCENT * 100f}% total.");
        }
    }

    // ---------- FINAL ANCHOR ENFORCEMENT ----------
    private void EnsureAnchorFloorsFinal(NodeMap map, ValidationResult r)
    {
        int top = map.MapHeight - 1;
        int mid = Mathf.RoundToInt((map.MapHeight - 1) / 2f);

        // First floor: all Combat
        foreach (var n in map.Floors[0])
        {
            if (n.Definition.nodeType != NodeType.Combat)
            {
                n.Reassign(map.Factory.GetDefinition(NodeType.Combat));
                r.Issues.Add("[Final] Reassigned first-floor node to Combat.");
            }
        }

        // Middle floor: all Loot
        foreach (var n in map.Floors[mid])
        {
            if (n.Definition.nodeType != NodeType.Loot)
            {
                n.Reassign(map.Factory.GetDefinition(NodeType.Loot));
                r.Issues.Add("[Final] Reassigned middle-floor node to Loot.");
            }
        }

        // Top floor: all Rest
        foreach (var n in map.Floors[top])
        {
            if (n.Definition.nodeType != NodeType.Rest)
            {
                n.Reassign(map.Factory.GetDefinition(NodeType.Rest));
                r.Issues.Add("[Final] Reassigned top-floor node to Rest.");
            }
        }
    }

    // ---------- WARNINGS ----------
    private void WarnPatternVariety(NodeMap map, ValidationResult r)
    {
        for (int y = 2; y < map.MapHeight; y++)
        {
            var t1 = map.Floors[y - 2].First().Definition.nodeType;
            var t2 = map.Floors[y - 1].First().Definition.nodeType;
            var t3 = map.Floors[y].First().Definition.nodeType;

            if (t1 == t2 && t2 == t3)
                r.Warnings.Add($"Same node type ({t1}) appears 3 floors in a row starting at floor {y - 2}.");
        }
    }
}
