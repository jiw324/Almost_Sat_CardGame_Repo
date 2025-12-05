using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapEnvironmentSpawner
{
    private readonly GameObject _tree1Prefab, _tree2Prefab;
    private readonly Material _groundMaterial;

    private readonly float _groundPadding;
    private readonly float _treePadding;
    private readonly float _treeSpacingMin;
    private readonly float _treeSpacingMax;

    private readonly int _ringRows = 3;
    private readonly float _rowSpacing = 1.25f;
    private readonly float _groundMargin = 3f;

    public MapEnvironmentSpawner(
        float groundPadding = 2.5f,
        float treePadding = 3f,
        float treeSpacingMin = 0.75f,
        float treeSpacingMax = 1.4f)
    {
        _tree1Prefab = Resources.Load<GameObject>("Prefabs/NodeMap/Environment/Tree1");
        if (_tree1Prefab == null)
            Debug.LogError("MapEnvironmentSpawner: Could not find Tree prefab in Resources/Prefabs/NodeMap/Environment/Tree1");

        _tree2Prefab = Resources.Load<GameObject>("Prefabs/NodeMap/Environment/Tree2");
        if (_tree2Prefab == null)
            Debug.LogError("MapEnvironmentSpawner: Could not find Tree prefab in Resources/Prefabs/NodeMap/Environment/Tree2");

        _groundMaterial = Resources.Load<Material>("Materials/NodeMap/Environment/Ground_Grass");
        if (_groundMaterial == null)
            Debug.LogError("MapEnvironmentSpawner: Could not find Ground material in Resources/Materials/NodeMap/Environment/Ground_Grass");

        _groundPadding = groundPadding;
        _treePadding = treePadding;
        _treeSpacingMin = treeSpacingMin;
        _treeSpacingMax = treeSpacingMax;
    }

    // Existing synchronous path (kept for compatibility / tools)
    public void SpawnEnvironment(NodeMapVisualContext ctx, Transform parent)
    {
        Bounds b = ctx.MapBounds;

        Transform envRoot = new GameObject("Environment").transform;
        envRoot.SetParent(parent);

        float forestOuterRadius = _treePadding + (_ringRows - 1) * _rowSpacing;
        float groundExtent = Mathf.Max(_groundPadding, forestOuterRadius + _groundMargin);

        SpawnGroundPlane(b, envRoot, groundExtent);
        SpawnForestRing(b, envRoot);
    }

    // New async / chunked environment spawn
    public IEnumerator SpawnEnvironmentAsync(NodeMapVisualContext ctx, Transform parent, int batchSize = 64)
    {
        if (ctx == null)
            yield break;

        Bounds b = ctx.MapBounds;

        Transform envRoot = new GameObject("Environment").transform;
        envRoot.SetParent(parent);

        float forestOuterRadius = _treePadding + (_ringRows - 1) * _rowSpacing;
        float groundExtent = Mathf.Max(_groundPadding, forestOuterRadius + _groundMargin);

        SpawnGroundPlane(b, envRoot, groundExtent);

        if (_tree1Prefab == null || _tree2Prefab == null)
            yield break;

        for (int row = 0; row < _ringRows; row++)
        {
            float offset = _treePadding + row * _rowSpacing;

            float xMin = b.min.x - offset;
            float xMax = b.max.x + offset;
            float zMin = b.min.z - offset;
            float zMax = b.max.z + offset;

            // Bottom edge
            yield return SpawnTreeLineAsync(new Vector3(xMin, 0, zMin), new Vector3(xMax, 0, zMin), envRoot, batchSize);
            // Top edge
            yield return SpawnTreeLineAsync(new Vector3(xMin, 0, zMax), new Vector3(xMax, 0, zMax), envRoot, batchSize);
            // Left edge
            yield return SpawnTreeLineAsync(new Vector3(xMin, 0, zMin), new Vector3(xMin, 0, zMax), envRoot, batchSize);
            // Right edge
            yield return SpawnTreeLineAsync(new Vector3(xMax, 0, zMin), new Vector3(xMax, 0, zMax), envRoot, batchSize);
        }
    }

    private void SpawnGroundPlane(Bounds b, Transform parent, float padding)
    {
        float xMin = b.min.x - padding;
        float xMax = b.max.x + padding;
        float zMin = b.min.z - padding;
        float zMax = b.max.z + padding;

        float width = xMax - xMin;
        float depth = zMax - zMin;

        Vector3 center = new Vector3(
            (xMin + xMax) * 0.5f,
            parent.position.y - 0.01f,
            (zMin + zMax) * 0.5f
        );

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "GroundPlane";
        ground.transform.SetParent(parent);
        ground.transform.position = center;

        ground.transform.localScale = new Vector3(width / 10f, 1f, depth / 10f);

        if (_groundMaterial != null)
        {
            var renderer = ground.GetComponent<MeshRenderer>();
            renderer.material = _groundMaterial;
        }
    }

    private void SpawnForestRing(Bounds b, Transform parent)
    {
        if (_tree1Prefab == null || _tree2Prefab == null) return;

        for (int row = 0; row < _ringRows; row++)
        {
            float offset = _treePadding + row * _rowSpacing;

            float xMin = b.min.x - offset;
            float xMax = b.max.x + offset;
            float zMin = b.min.z - offset;
            float zMax = b.max.z + offset;

            SpawnTreeLine(new Vector3(xMin, 0, zMin), new Vector3(xMax, 0, zMin), parent);
            SpawnTreeLine(new Vector3(xMin, 0, zMax), new Vector3(xMax, 0, zMax), parent);
            SpawnTreeLine(new Vector3(xMin, 0, zMin), new Vector3(xMin, 0, zMax), parent);
            SpawnTreeLine(new Vector3(xMax, 0, zMin), new Vector3(xMax, 0, zMax), parent);
        }
    }

    private void SpawnTreeLine(Vector3 start, Vector3 end, Transform parent)
    {
        Vector3 direction = (end - start).normalized;
        float length = Vector3.Distance(start, end);

        float dist = 0f;
        while (dist < length)
        {
            float step = Random.Range(_treeSpacingMin, _treeSpacingMax);
            dist += step;

            if (dist > length) break;

            Vector3 pos = start + direction * dist;

            pos.x += Random.Range(-0.3f, 0.3f);
            pos.z += Random.Range(-0.3f, 0.3f);

            GameObject randomTree = Random.value < 0.5f ? _tree1Prefab : _tree2Prefab;
            Object.Instantiate(randomTree, pos, Quaternion.identity, parent);
        }
    }

    private IEnumerator SpawnTreeLineAsync( Vector3 start, Vector3 end, Transform parent, int batchSize)
    {
        Vector3 direction = (end - start).normalized;
        float length = Vector3.Distance(start, end);

        float dist = 0f;
        int counter = 0;

        while (dist < length)
        {
            float step = Random.Range(_treeSpacingMin, _treeSpacingMax);
            dist += step;
            if (dist > length)
                break;

            Vector3 pos = start + direction * dist;
            pos.x += Random.Range(-0.3f, 0.3f);
            pos.z += Random.Range(-0.3f, 0.3f);

            GameObject randomTree = Random.value < 0.5f ? _tree1Prefab : _tree2Prefab;
            Object.Instantiate(randomTree, pos, Quaternion.identity, parent);

            // batching
            counter++;
            if (counter >= batchSize)
            {
                counter = 0;
                yield return null;
            }
        }
    }

}
