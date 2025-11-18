using System.Collections.Generic;
using UnityEngine;

public class NodeFactory
{
    private readonly Dictionary<NodeType, NodeDefinition> _definitions = new();

    public NodeFactory()
    {
        LoadDefinitions();
    }

    private void LoadDefinitions()
    {
        NodeDefinition[] loadedDefs = Resources.LoadAll<NodeDefinition>("NodeDefinitions");

        foreach (var def in loadedDefs)
        {
            if (_definitions.ContainsKey(def.nodeType))
                Debug.LogWarning($"Duplicate NodeDefinition found for type {def.nodeType}, overwriting...");

            _definitions[def.nodeType] = def;
        }
    }

    public Node CreateNode(NodeType type, Vector2Int gridPos)
    {
        if (!_definitions.TryGetValue(type, out var def))
            throw new System.Exception($"No NodeDefinition found for type {type}");

        return new Node(def, gridPos);
    }

    public NodeDefinition GetDefinition(NodeType type)
    {
        if (!_definitions.TryGetValue(type, out var def))
            throw new KeyNotFoundException($"No NodeDefinition registered for type {type}");
        return def;
    }
}
