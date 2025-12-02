using System.Collections.Generic;
using UnityEngine;

public class NodeMapSpawner
{
    private readonly GameObject _pathPrefab;
    private readonly float _pathThickness;

    public NodeMapSpawner(float pathThickness = 0.05f)
    {
        _pathPrefab = Resources.Load<GameObject>("Prefabs/NodeMap/NodePath");
        _pathThickness = pathThickness;
    }

    public NodeMapVisualContext Spawn(NodeMap map, Transform parent)
    {
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

            if (node.Definition?.revealedPrefab != null)
            {
                GameObject nodeObj = Object.Instantiate(node.Definition.revealedPrefab, worldPos, Quaternion.identity, nodeParent);
                NodeView view = nodeObj.GetComponent<NodeView>() ?? nodeObj.AddComponent<NodeView>();
                view.Initialize(node);
                spawnedNodes.Add(view);
            }

            foreach (Node next in node.NextNodes)
            {
                Vector3 nextPos = map.Grid.GridToWorld(next.GridPos.x, next.GridPos.y);
                UpdateBounds(nextPos, ref min, ref max);

                GameObject path = CreatePath(worldPos, nextPos, pathParent);
                if (path != null)
                    spawnedPaths.Add(path);
            }
        }

        if (map.BossNode != null)
        {
            Vector3 bossPos = map.Grid.GridToWorld(map.BossNode.GridPos.x, map.BossNode.GridPos.y);
            UpdateBounds(bossPos, ref min, ref max);

            var combatDef = map.Factory.GetDefinition(NodeType.Combat);
            GameObject prefab = combatDef.revealedPrefab;

            GameObject bossObj = Object.Instantiate(prefab, bossPos, Quaternion.identity, nodeParent);
            bossObj.transform.localScale *= 2.5f;

            NodeView bossView = bossObj.GetComponent<NodeView>() ?? bossObj.AddComponent<NodeView>();
            bossView.Initialize(map.BossNode);
            spawnedNodes.Add(bossView);
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
            return null;

        Vector3 direction = end - start;
        float distance = direction.magnitude;

        GameObject pathObj = Object.Instantiate(_pathPrefab, parent);
        pathObj.transform.position = start + direction * 0.5f;
        pathObj.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        Vector3 scale = pathObj.transform.localScale;
        scale.z = distance;
        pathObj.transform.localScale = scale;

        return pathObj;
    }
}
