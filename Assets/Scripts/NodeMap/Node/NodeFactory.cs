using System.Collections.Generic;
using UnityEngine;

public class NodeFactory
{
    public Dictionary<NodeType, NodeDefinition> definitions { get; private set; } = new();

    public NodeFactory()
    {
        NodeDefinition[] loadedDefs = Resources.LoadAll<NodeDefinition>("NodeDefinitions");

        foreach (var def in loadedDefs)
        {
            if (definitions.ContainsKey(def.nodeType))
                Debug.LogWarning($"Overwriting duplicate NodeDefinition for {def.nodeType}");
            
            definitions[def.nodeType] = def;
        }
    }

    public INode CreateNode(NodeType nodeType, NodeAnchor nodeAnchor)
    {
        if (!definitions.TryGetValue(nodeType, out var def))
        {
            throw new System.Exception($"No NodeDefinition found for type {nodeType}");
        }

        return nodeType switch
        {
            NodeType.Loot   => new LootNode(def, nodeAnchor),
            NodeType.Rest   => new RestNode(def, nodeAnchor),
            NodeType.Shop   => new ShopNode(def, nodeAnchor),
            NodeType.Event  => new EventNode(def, nodeAnchor),
            NodeType.Combat => new CombatNode(def, nodeAnchor),
            _               => throw new System.ArgumentException("Invalid Node Type", nameof(nodeType))
        };
    }
}
