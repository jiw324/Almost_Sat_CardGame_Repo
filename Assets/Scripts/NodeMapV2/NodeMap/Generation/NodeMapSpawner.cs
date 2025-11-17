using System.Collections.Generic;
using UnityEngine;

public class NodeMapSpawner
{
    private readonly GameObject _pathPrefab;
    private readonly float _pathThickness;

    public NodeMapSpawner(float pathThickness = 0.05f)
    {
        // Load the existing path prefab once from Resources
        _pathPrefab = Resources.Load<GameObject>("Prefabs/NodeMap/NodePath");

        if (_pathPrefab == null)
            Debug.LogError("NodeMapSpawner: Could not find NodePath prefab in Resources/Prefabs/NodeMap/.");

        _pathThickness = pathThickness;
    }

    public NodeMapVisualContext Spawn(NodeMap map, Transform parent)
    {
        if (map == null || map.Grid == null)
            throw new System.Exception("NodeMapSpawner: map or grid not initialized.");

        // Create containers
        var nodeParent = new GameObject("Nodes").transform;
        var pathParent = new GameObject("Paths").transform;
        nodeParent.SetParent(parent);
        pathParent.SetParent(parent);

        List<NodeView> spawnedNodes = new();
        List<GameObject> spawnedPaths = new();

        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;

        foreach (Node node in map.AllNodes)
        {
            Vector3 worldPos = map.Grid.GridToWorld(node.GridPos.x, node.GridPos.y);
            UpdateBounds(worldPos, ref min, ref max);

            if (node.Definition?.prefab == null)
                continue;

            // Instantiate node prefab
            GameObject nodeObj = Object.Instantiate(node.Definition.prefab, worldPos, Quaternion.identity, nodeParent);
            NodeView view = nodeObj.GetComponent<NodeView>() ?? nodeObj.AddComponent<NodeView>();
            view.Initialize(node);
            spawnedNodes.Add(view);

            // Create paths for all outgoing connections
            foreach (Node next in node.NextNodes)
            {
                Vector3 nextPos = map.Grid.GridToWorld(next.GridPos.x, next.GridPos.y);
                UpdateBounds(nextPos, ref min, ref max);

                GameObject path = CreatePath(worldPos, nextPos, pathParent);
                if (path != null)
                    spawnedPaths.Add(path);
            }
        }

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);

        return new NodeMapVisualContext(bounds, spawnedNodes, spawnedPaths);
    }

    private void UpdateBounds(Vector3 pos, ref Vector3 min, ref Vector3 max)
    {
        min = Vector3.Min(min, pos);
        max = Vector3.Max(max, pos);
    }

    private GameObject CreatePath(Vector3 start, Vector3 end, Transform parent)
    {
        if (_pathPrefab == null)
        {
            Debug.LogWarning("NodeMapSpawner: No path prefab assigned or found in Resources.");
            return null;
        }

        Vector3 direction = end - start;
        float distance = direction.magnitude;

        GameObject pathObj = Object.Instantiate(_pathPrefab, parent);
        pathObj.name = $"Path_{start}_{end}";

        // Position at midpoint
        pathObj.transform.position = start + direction * 0.5f;

        // Rotate to face target
        pathObj.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        // Stretch to match distance between nodes
        Vector3 localScale = pathObj.transform.localScale;
        localScale.z = distance;
        pathObj.transform.localScale = localScale;

        return pathObj;
    }
}
