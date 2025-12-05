using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeMapSpawner
{
    private readonly GameObject _pathPrefab;
    private readonly float _pathThickness;

    private NodeMapVisualContext _context;
    public NodeMapVisualContext Context => _context;

    public NodeMapSpawner(float pathThickness = 0.05f)
    {
        _pathPrefab = Resources.Load<GameObject>("Prefabs/NodeMap/NodePath");
        _pathThickness = pathThickness;
    }

    // Existing synchronous path (kept for compatibility / tools)
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
                GameObject revealed = Object.Instantiate(node.Definition.revealedPrefab, worldPos, Quaternion.identity, nodeParent);

                NodeView view = revealed.GetComponent<NodeView>() ?? revealed.AddComponent<NodeView>();
                view.Initialize(node);

                spawnedNodes.Add(view);

                if (node.Definition.hiddenPrefab != null)
                {
                    GameObject hidden = Object.Instantiate(node.Definition.hiddenPrefab, revealed.transform);
                    hidden.transform.localPosition = Vector3.zero;
                    hidden.transform.localRotation = Quaternion.identity;
                    view.SetHiddenVisualRoot(hidden);
                }
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

        if (map.BossNode != null && map.BossNode.Definition != null)
        {
            Vector3 bossPos = map.Grid.GridToWorld(map.BossNode.GridPos.x, map.BossNode.GridPos.y);
            UpdateBounds(bossPos, ref min, ref max);

            GameObject revealed = Object.Instantiate(map.BossNode.Definition.revealedPrefab, bossPos, Quaternion.identity, nodeParent);
            revealed.transform.localScale *= 1.75f;

            NodeView view = revealed.GetComponent<NodeView>() ?? revealed.AddComponent<NodeView>();
            view.Initialize(map.BossNode);
            spawnedNodes.Add(view);

            if (map.BossNode.Definition.hiddenPrefab != null)
            {
                GameObject hidden = Object.Instantiate(map.BossNode.Definition.hiddenPrefab, revealed.transform);
                hidden.transform.localPosition = Vector3.zero;
                hidden.transform.localRotation = Quaternion.identity;
                view.SetHiddenVisualRoot(hidden);
            }
        }

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);

        _context = new NodeMapVisualContext(bounds, spawnedNodes, spawnedPaths);
        return _context;
    }

    // New: async / chunked spawn to avoid frame hitch
    public IEnumerator SpawnAsync(NodeMap map, Transform parent, int batchSize = 32)
    {
        var nodeParent = new GameObject("Nodes").transform;
        var pathParent = new GameObject("Paths").transform;
        nodeParent.SetParent(parent);
        pathParent.SetParent(parent);

        List<NodeView> spawnedNodes = new();
        List<GameObject> spawnedPaths = new();

        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;

        int counter = 0;

        foreach (Node node in map.AllNodes)
        {
            Vector3 worldPos = map.Grid.GridToWorld(node.GridPos.x, node.GridPos.y);
            UpdateBounds(worldPos, ref min, ref max);

            if (node.Definition?.revealedPrefab != null)
            {
                GameObject revealed = Object.Instantiate(node.Definition.revealedPrefab, worldPos, Quaternion.identity, nodeParent);

                NodeView view = revealed.GetComponent<NodeView>() ?? revealed.AddComponent<NodeView>();
                view.Initialize(node);

                spawnedNodes.Add(view);

                if (node.Definition.hiddenPrefab != null)
                {
                    GameObject hidden = Object.Instantiate(node.Definition.hiddenPrefab, revealed.transform);
                    hidden.transform.localPosition = Vector3.zero;
                    hidden.transform.localRotation = Quaternion.identity;
                    view.SetHiddenVisualRoot(hidden);
                }

                counter++;
                if (counter >= batchSize)
                {
                    counter = 0;
                    yield return null;
                }
            }

            foreach (Node next in node.NextNodes)
            {
                Vector3 nextPos = map.Grid.GridToWorld(next.GridPos.x, next.GridPos.y);
                UpdateBounds(nextPos, ref min, ref max);

                GameObject path = CreatePath(worldPos, nextPos, pathParent);
                if (path != null)
                    spawnedPaths.Add(path);

                counter++;
                if (counter >= batchSize)
                {
                    counter = 0;
                    yield return null;
                }
            }
        }

        if (map.BossNode != null && map.BossNode.Definition != null)
        {
            Vector3 bossPos = map.Grid.GridToWorld(map.BossNode.GridPos.x, map.BossNode.GridPos.y);
            UpdateBounds(bossPos, ref min, ref max);

            GameObject revealed = Object.Instantiate(map.BossNode.Definition.revealedPrefab, bossPos, Quaternion.identity, nodeParent);
            revealed.transform.localScale *= 1.75f;

            NodeView view = revealed.GetComponent<NodeView>() ?? revealed.AddComponent<NodeView>();
            view.Initialize(map.BossNode);
            spawnedNodes.Add(view);

            if (map.BossNode.Definition.hiddenPrefab != null)
            {
                GameObject hidden = Object.Instantiate(map.BossNode.Definition.hiddenPrefab, revealed.transform);
                hidden.transform.localPosition = Vector3.zero;
                hidden.transform.localRotation = Quaternion.identity;
                view.SetHiddenVisualRoot(hidden);
            }

            counter++;
            if (counter >= batchSize)
            {
                counter = 0;
                yield return null;
            }
        }

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);

        _context = new NodeMapVisualContext(bounds, spawnedNodes, spawnedPaths);
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
        scale.x = _pathThickness;
        pathObj.transform.localScale = scale;

        return pathObj;
    }
}
