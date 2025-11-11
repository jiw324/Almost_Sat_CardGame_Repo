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
        Debug.Log($"Assigning types to {map.AllNodes.Count()} nodes...");
        int height = map.MapHeight;

        foreach (var floorPair in map.Floors)
        {
            int y = floorPair.Key;
            List<Node> floorNodes = floorPair.Value;

            foreach (Node node in floorNodes)
            {
                if (node.HasDefinition()) continue;

                NodeType type = ChooseNodeType(y, height);
                NodeDefinition def = _factory.GetDefinition(type);
                node.Reassign(def);
                // Debug.Log($"Assigned {type} to node {node.GridPos}");
            }
        }

        // Force top floor to be final rest floor
        foreach (var node in map.Floors[height - 1])
        {
            node.Reassign(_factory.GetDefinition(NodeType.Rest));
        }

        // Force middle floor to be a loot floor
        foreach (var node in map.Floors[Mathf.RoundToInt(height / 2)])
        {
            node.Reassign(_factory.GetDefinition(NodeType.Loot));
        }

        // Force first floor to be a combat floor
        foreach (var node in map.Floors[0])
        {
            node.Reassign(_factory.GetDefinition(NodeType.Combat));
        }
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
        // Early floors
        if (progress < 0.33f)
        {
            return new Dictionary<NodeType, float>
            {
                { NodeType.Combat, 0.5f },
                { NodeType.Shop, 0.15f },
                { NodeType.Event, 0.25f },
                { NodeType.Rest, 0.05f },
                { NodeType.Loot, 0.05f }
            };
        }
        // Mid floors
        else if (progress < 0.75f)
        {
            return new Dictionary<NodeType, float>
            {
                { NodeType.Combat, 0.4f },
                { NodeType.Shop, 0.2f },
                { NodeType.Event, 0.25f },
                { NodeType.Rest, 0.1f },
                { NodeType.Loot, 0.05f }
            };
        }
        // Late floors
        else
        {
            return new Dictionary<NodeType, float>
            {
                { NodeType.Combat, 0.35f },
                { NodeType.Shop, 0.25f },
                { NodeType.Event, 0.15f },
                { NodeType.Rest, 0.15f },
                { NodeType.Loot, 0.1f }
            };
        }
    }
}
