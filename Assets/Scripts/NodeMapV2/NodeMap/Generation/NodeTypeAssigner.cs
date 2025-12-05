using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeTypeAssigner
{
    private NodeFactory _factory;

    public NodeTypeAssigner(NodeFactory factory)
    {
        _factory = factory;
    }

    public void Assign(NodeMap map)
    {
        int height = map.MapHeight;
        int top = height - 1;
        int mid = Mathf.RoundToInt((height - 1) / 2f);

        if (map.Floors.ContainsKey(top))
        {
            foreach (var node in map.Floors[top])
                node.Reassign(_factory.GetDefinition(NodeType.Rest));
        }

        if (map.Floors.ContainsKey(mid))
        {
            foreach (var node in map.Floors[mid])
                node.Reassign(_factory.GetDefinition(NodeType.Relic));
        }

        if (map.Floors.ContainsKey(0))
        {
            foreach (var node in map.Floors[0])
                node.Reassign(_factory.GetDefinition(NodeType.Combat));
        }

        foreach (var floorPair in map.Floors)
        {
            int y = floorPair.Key;
            if (y == 0 || y == mid || y == top) continue;

            List<Node> floorNodes = floorPair.Value;
            foreach (Node node in floorNodes)
            {
                if (node.HasDefinition()) continue;

                NodeType type = ChooseNodeType(y, height);
                NodeDefinition def = _factory.GetDefinition(type);
                node.Reassign(def);
            }
        }

        // boss node always combat
        if (map.BossNode != null)
            map.BossNode.Reassign(_factory.GetDefinition(NodeType.Combat));
    }

    private NodeType ChooseNodeType(int y, int height)
    {
        float progress = (float)y / (height - 1);
        Dictionary<NodeType, float> weights = GetWeightsForProgress(progress);

        float total = weights.Values.Sum();
        float roll = Random.value * total;
        float cumulative = 0f;

        foreach (var kvp in weights)
        {
            cumulative += kvp.Value;
            if (roll <= cumulative)
                return kvp.Key;
        }

        return NodeType.Combat;
    }

    private Dictionary<NodeType, float> GetWeightsForProgress(float progress)
    {
        if (progress < 0.33f)
        {
            return new Dictionary<NodeType, float>
            {
                { NodeType.Combat, 0.35f },
                { NodeType.Shop,   0.15f },
                { NodeType.Event,  0.2f },
                { NodeType.Rest,   0.25f },
                { NodeType.Relic,   0.05f }
            };
        }
        else if (progress < 0.75f)
        {
            return new Dictionary<NodeType, float>
            {
                { NodeType.Combat, 0.35f },
                { NodeType.Shop,   0.2f },
                { NodeType.Event,  0.2f },
                { NodeType.Rest,   0.2f },
                { NodeType.Relic,   0.05f }
            };
        }
        else
        {
            return new Dictionary<NodeType, float>
            {
                { NodeType.Combat, 0.35f },
                { NodeType.Shop,   0.25f },
                { NodeType.Event,  0.15f },
                { NodeType.Rest,   0.15f },
                { NodeType.Relic,   0.1f }
            };
        }
    }
}
