using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeMapValidator
{
    private const int REST_FREQUENCY = 3;
    private const int SHOP_FREQUENCY = 3;
    private const int MAX_PASSES = 6;

    public int PassesUsed { get; private set; }
    public Dictionary<string, int> RuleHits { get; } = new Dictionary<string, int>();

    public struct RuleValidationResult
    {
        public bool ruleViolated;
        public string Message;
    }

    public RuleValidationResult Validate(NodeMap map)
    {
        RuleValidationResult finalResult = CreateRuleValidationResult();
        List<string> messages = new List<string>();
        PassesUsed = 0;

        for (int pass = 0; pass < MAX_PASSES; pass++)
        {
            PassesUsed++;
            bool anyChanges = false;

            var emptyFloors = Rule_NoEmptyFloors(map);
            if (emptyFloors.ruleViolated)
            {
                anyChanges = true;
                messages.Add(emptyFloors.Message);
                AddRuleHit("NoEmptyFloors");
            }

            var freqRest = Rule_EnsureTypeEveryXFloors(map, NodeType.Rest, REST_FREQUENCY);
            if (freqRest.ruleViolated)
            {
                anyChanges = true;
                messages.Add(freqRest.Message);
                AddRuleHit("RestFrequency");
            }

            var freqShop = Rule_EnsureTypeEveryXFloors(map, NodeType.Shop, SHOP_FREQUENCY);
            if (freqShop.ruleViolated)
            {
                anyChanges = true;
                messages.Add(freqShop.Message);
                AddRuleHit("ShopFrequency");
            }

            for (int y = 0; y < map.MapHeight; y++)
            {
                var floor = map.Floors[y];
                for (int x = 0; x < floor.Count; x++)
                {
                    var topoIn = Rule_MissingIncoming(floor[x], map);
                    if (topoIn.ruleViolated)
                    {
                        anyChanges = true;
                        messages.Add(topoIn.Message);
                        AddRuleHit("MissingIncoming");
                    }

                    var topoOut = Rule_MissingOutgoing(floor[x], map);
                    if (topoOut.ruleViolated)
                    {
                        anyChanges = true;
                        messages.Add(topoOut.Message);
                        AddRuleHit("MissingOutgoing");
                    }
                }
            }

            for (int y = 0; y < map.MapHeight; y++)
            {
                var floor = map.Floors[y];
                for (int x = 0; x < floor.Count; x++)
                {
                    var a1 = Rule_AnchorFloors(floor[x], map, 0, NodeType.Combat);
                    if (a1.ruleViolated)
                    {
                        anyChanges = true;
                        messages.Add(a1.Message);
                        AddRuleHit("AnchorFirstFloor");
                    }

                    var a2 = Rule_AnchorFloors(floor[x], map, Mathf.RoundToInt((map.MapHeight - 1) / 2), NodeType.Relic);
                    if (a2.ruleViolated)
                    {
                        anyChanges = true;
                        messages.Add(a2.Message);
                        AddRuleHit("AnchorMiddleFloor");
                    }

                    var a3 = Rule_AnchorFloors(floor[x], map, map.MapHeight - 1, NodeType.Rest);
                    if (a3.ruleViolated)
                    {
                        anyChanges = true;
                        messages.Add(a3.Message);
                        AddRuleHit("AnchorTopFloor");
                    }
                }
            }

            for (int y = 0; y < map.MapHeight; y++)
            {
                var floor = map.Floors[y];
                for (int x = 0; x < floor.Count; x++)
                {
                    var chain = Rule_NoRestShopChains(floor[x], map);
                    if (chain.ruleViolated)
                    {
                        anyChanges = true;
                        messages.Add(chain.Message);
                        AddRuleHit("RestShopChain");
                    }

                    var triple = Rule_NoTripleChains(floor[x], map);
                    if (triple.ruleViolated)
                    {
                        anyChanges = true;
                        messages.Add(triple.Message);
                        AddRuleHit("TripleChain");
                    }
                }
            }

            if (!anyChanges)
            {
                finalResult.ruleViolated = false;
                finalResult.Message = string.Join("", messages);
                return finalResult;
            }

            finalResult.ruleViolated = true;
        }

        finalResult.Message = string.Join("", messages);
        return finalResult;
    }

    private void AddRuleHit(string name)
    {
        if (!RuleHits.ContainsKey(name))
            RuleHits[name] = 0;
        RuleHits[name]++;
    }

    private RuleValidationResult Rule_NoEmptyFloors(NodeMap map)
    {
        RuleValidationResult result = CreateRuleValidationResult();

        for (int y = 0; y < map.MapHeight; y++)
        {
            var floor = map.Floors[y];

            if (floor.Count > 0) continue;

            result.ruleViolated = true;
            int floorMidpoint = Mathf.RoundToInt((map.MapWidth - 1) / 2);
            Vector2Int pos = new Vector2Int(floorMidpoint, y);

            Node newNode = map.Factory.CreateNode(NodeType.Combat, pos);
            floor.Add(newNode);
            result.Message += $"[Topology] Floor {y} was empty. Inserted fallback Combat node at ({floorMidpoint},{y}).\n";
        }

        return result;
    }

    private RuleValidationResult Rule_MissingIncoming(Node node, NodeMap map)
    {
        RuleValidationResult result = CreateRuleValidationResult();

        if (node.ParentNodes.Count > 0 || node.GridPos.y == 0) return result;

        var validNeighbors = GetValidNeighbors(map, node.GridPos.y - 1, node.GridPos.x);

        if (validNeighbors.Count > 0)
        {
            Node parent = validNeighbors
                .OrderBy(p => Mathf.Abs(p.GridPos.x - node.GridPos.x))
                .First();
            parent.ConnectTo(node);

            result.ruleViolated = true;
            result.Message = $"[Topology] Added missing incoming: {parent.GridPos} -> {node.GridPos}\n";
            return result;
        }

        int px = Mathf.Clamp(node.GridPos.x, 0, map.MapWidth - 1);
        Vector2Int newPos = new Vector2Int(px, node.GridPos.y - 1);
        Node fallback = map.Factory.CreateNode(NodeType.Combat, newPos);
        map.Floors[node.GridPos.y - 1].Add(fallback);
        fallback.ConnectTo(node);

        result.ruleViolated = true;
        result.Message = $"[Topology] Created fallback parent {fallback.GridPos} -> {node.GridPos}.\n";
        return result;
    }

    private RuleValidationResult Rule_MissingOutgoing(Node node, NodeMap map)
    {
        RuleValidationResult result = CreateRuleValidationResult();

        if (node.NextNodes.Count > 0 || node.GridPos.y == map.MapHeight - 1) return result;

        var validNeighbors = GetValidNeighbors(map, node.GridPos.y + 1, node.GridPos.x);

        if (validNeighbors.Count > 0)
        {
            Node child = validNeighbors
                .OrderBy(n => Mathf.Abs(n.GridPos.x - node.GridPos.x))
                .First();
            node.ConnectTo(child);

            result.ruleViolated = true;
            result.Message = $"[Topology] Added missing outgoing: {node.GridPos} -> {child.GridPos}\n";
            return result;
        }

        int cx = Mathf.Clamp(node.GridPos.x, 0, map.MapWidth - 1);
        Vector2Int newPos = new Vector2Int(cx, node.GridPos.y + 1);
        Node fallback = map.Factory.CreateNode(NodeType.Combat, newPos);
        map.Floors[node.GridPos.y + 1].Add(fallback);
        node.ConnectTo(fallback);

        result.ruleViolated = true;
        result.Message = $"[Topology] Created fallback child {node.GridPos} -> {fallback.GridPos}\n";
        return result;
    }

    private RuleValidationResult Rule_AnchorFloors(Node node, NodeMap map, int floor, NodeType nodeType)
    {
        RuleValidationResult result = CreateRuleValidationResult();

        if (node.GridPos.y != floor || node.Definition.nodeType == nodeType)
            return result;

        node.Reassign(map.Factory.GetDefinition(nodeType));

        result.ruleViolated = true;
        result.Message = $"[Anchor] Set floor {floor} node at {node.GridPos} -> {nodeType}\n";
        return result;
    }

    private RuleValidationResult Rule_NoRestShopChains(Node node, NodeMap map)
    {
        RuleValidationResult result = CreateRuleValidationResult();

        if ((node.Definition.nodeType != NodeType.Rest && node.Definition.nodeType != NodeType.Shop) ||
            (node.GridPos.y == 0 || node.GridPos.y == ((map.MapHeight - 1) / 2) || node.GridPos.y == map.MapHeight - 1))
        {
            return result;
        }

        if (node.Definition.nodeType == NodeType.Rest && node.GridPos.y == map.MapHeight - 2)
        {
            NodeType newType = GetRandomReplacement(node.Definition.nodeType);
            node.Reassign(map.Factory.GetDefinition(newType));

            result.ruleViolated = true;
            result.Message = $"[Flow] Rest at floor {node.GridPos} creates Rest -> Rest chain. Reassigned to {newType}\n";
            return result;
        }

        bool parentSame = node.ParentNodes.Any(p => p.Definition.nodeType == node.Definition.nodeType);

        if (!parentSame) return result;

        NodeType replacement = GetRandomReplacement(node.Definition.nodeType);

        node.Reassign(map.Factory.GetDefinition(replacement));
        result.ruleViolated = true;
        result.Message = $"[Flow] {node.Definition.nodeType} -> {node.Definition.nodeType} chain broken at {node.GridPos}. Reassigned to {replacement}\n";
        return result;
    }

    private RuleValidationResult Rule_NoTripleChains(Node node, NodeMap map)
    {
        RuleValidationResult result = CreateRuleValidationResult();

        if (node.GridPos.y == 0 || node.GridPos.y == ((map.MapHeight - 1) / 2) || node.GridPos.y == map.MapHeight - 1)
        {
            return result;
        }

        foreach (Node parent in node.ParentNodes)
        {
            NodeType parentType = parent.Definition.nodeType;
            if (parentType != node.Definition.nodeType)
                continue;

            foreach (Node grandparent in parent.ParentNodes)
            {
                if (grandparent.Definition.nodeType == node.Definition.nodeType)
                {
                    NodeType newType = GetRandomReplacement(node.Definition.nodeType);

                    node.Reassign(map.Factory.GetDefinition(newType));

                    result.ruleViolated = true;
                    result.Message = $"[Flow] Triple {node.Definition.nodeType} repetition broken at {node.GridPos}. Reassigned to {newType}.\n";
                    return result;
                }
            }
        }

        return result;
    }

    private RuleValidationResult Rule_EnsureTypeEveryXFloors(NodeMap map, NodeType requiredType, int frequency = 3)
    {
        RuleValidationResult result = CreateRuleValidationResult();

        for (int y = 0; y <= map.MapHeight - frequency; y++)
        {
            int[] checkFloors = Enumerable.Range(y, frequency).ToArray();

            bool hasRequired = checkFloors.Any(f =>
                map.Floors[f].Any(n => n.Definition.nodeType == requiredType));
            if (hasRequired) continue;

            List<int> nonAnchorFloors = checkFloors
                .Where(f => f != 0 && f != (map.MapHeight - 1) / 2 && f != map.MapHeight - 1)
                .OrderBy(f => f)
                .ToList();
            if (nonAnchorFloors.Count == 0) continue;

            int targetFloor = nonAnchorFloors[nonAnchorFloors.Count / 2];
            var floorNodes = map.Floors[targetFloor];
            Node targetNode = floorNodes
                .OrderBy(n => n.GridPos.x)
                .ElementAt(floorNodes.Count / 2);

            targetNode.Reassign(map.Factory.GetDefinition(requiredType));

            result.ruleViolated = true;
            result.Message += $"[Frequency] Inserted {requiredType} on floor {targetFloor} to satisfy {requiredType} every {frequency} floors\n";
        }

        return result;
    }

    private NodeType GetRandomReplacement(NodeType repeatedType)
    {
        float r = Random.value;

        if (repeatedType == NodeType.Combat)
            return (r < 0.5f) ? NodeType.Event : NodeType.Shop;

        if (repeatedType == NodeType.Event)
            return (r < 0.5f) ? NodeType.Combat : NodeType.Shop;

        if (repeatedType == NodeType.Shop)
            return (r < 0.6f) ? NodeType.Combat : NodeType.Event;

        if (repeatedType == NodeType.Rest)
            return (r < 0.7f) ? NodeType.Combat : NodeType.Event;

        if (repeatedType == NodeType.Relic)
            return (r < 0.7f) ? NodeType.Combat : NodeType.Event;

        return NodeType.Combat;
    }

    private RuleValidationResult CreateRuleValidationResult()
    {
        return new RuleValidationResult()
        {
            ruleViolated = false,
            Message = ""
        };
    }

    private List<Node> GetValidNeighbors(NodeMap map, int y, int targetX)
    {
        if (y < 0 || y >= map.MapHeight)
            return new List<Node>();

        return map.Floors[y].Where(n =>
            Mathf.Abs(n.GridPos.x - targetX) <= 1
        ).ToList();
    }
}
