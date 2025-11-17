using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class NodeMap : MonoBehaviour
{
    private NodeGrid _nodeGrid;
    public Dictionary<int, INode[]> nodes { get; private set; }
    private HashSet<int> _startNodeXVals;
    private NodeFactory _nodeFactory;
    private NodeMapValidator _nodeMapValidator;

    private const float _maxXOffset = 0.3f;
    private const float _maxYOffset = 0.3f;
    private const int _maxNodeGenAttempts = 10;

    private int _maxStartNodes = 0;
    private const float _startNodesMult = 0.6f;
    private int _maxNumPaths = 0;
    private const float _numPathsMult = 3.5f;

    public int mapWidth { get; private set; } = 5;
    public int mapHeight { get; private set; } = 15;

    private bool _isGenerated = false;

    public void Awake()
    {
        _nodeGrid = new NodeGrid(mapWidth, mapHeight);

        nodes = new Dictionary<int, INode[]>();
        for (int y = 0; y < mapHeight; y++)
            nodes[y] = new INode[mapWidth];

        _nodeFactory = new NodeFactory();
        _nodeMapValidator = new NodeMapValidator(this, _nodeFactory);
        _maxStartNodes = Mathf.RoundToInt(mapWidth * _startNodesMult);
        _maxNumPaths = Mathf.RoundToInt(mapWidth * _numPathsMult);
        _startNodeXVals = new HashSet<int>();
    }

    public void Generate()
    {
        if (_isGenerated)
        {
            Debug.LogWarning("NodeMap already generated!");
            return;
        }

        GenerateNodeData();

        var report = _nodeMapValidator.ValidateMap(autoFix: true);
        if (!report.Success)
            Debug.LogError("Unable to validate map");
        else
            Debug.Log(report.ToString());

        SpawnNodes();
        GeneratePaths();

        _isGenerated = true;
    }

    private void GenerateNodeData()
    {
        Debug.Log("Generating NodeMap data");

        ChooseStartNodes();

        GameObject nodeAnchorParentObj = new GameObject("NodeAnchors");
        nodeAnchorParentObj.transform.SetParent(transform, false);

        for (int pathIndex = 0; pathIndex < _maxNumPaths; pathIndex++)
        {
            int startX = GetRandomStartX();
            int x = startX;

            if (nodes[0][x] == null)
                nodes[0][x] = _nodeFactory.CreateNode(
                    _nodeMapValidator.GetNodeType(1),
                    GetNodeAnchor(x, 0, nodeAnchorParentObj));

            int minDeltaX = x > 0 ? -1 : 0;
            int maxDeltaX = x < mapWidth - 1 ? 1 : 0;

            for (int y = 0; y < mapHeight - 1; y++)
            {
                int randomDeltaX;
                int attemptCount = 0;

                do
                {
                    randomDeltaX = Random.Range(minDeltaX, maxDeltaX + 1);
                    attemptCount++;
                    if (attemptCount > _maxNodeGenAttempts)
                    {
                        randomDeltaX = 0;
                        break;
                    }
                } while (CheckForCrossPath(x, x + randomDeltaX, y));

                INode nextNode = GenerateNextNode(x + randomDeltaX, y + 1, nodeAnchorParentObj);
                nodes[y][x].AddNextNode(nextNode, randomDeltaX);

                x += randomDeltaX;
            }
        }
    }

    private int GetRandomStartX()
    {
        int[] startXs = new int[_startNodeXVals.Count];
        _startNodeXVals.CopyTo(startXs);
        return startXs[Random.Range(0, startXs.Length)];
    }

    private void ChooseStartNodes()
    {
        _startNodeXVals.Clear();
        while (_startNodeXVals.Count < Mathf.Min(_maxStartNodes, mapWidth))
            _startNodeXVals.Add(Random.Range(0, mapWidth));
    }

    private INode GenerateNextNode(int nextX, int nextY, GameObject parentObj)
    {
        if (nodes[nextY][nextX] != null)
            return nodes[nextY][nextX];

        NodeAnchor nextAnchor = GetNodeAnchor(nextX, nextY, parentObj);
        INode nextNode = _nodeFactory.CreateNode(_nodeMapValidator.GetNodeType(nextY + 1), nextAnchor);
        nodes[nextY][nextX] = nextNode;
        return nextNode;
    }

    private bool CheckForCrossPath(int startX, int endX, int y)
    {
        if (endX < 0 || endX >= mapWidth) return true;
        if (startX == endX) return false;

        for (int otherX = 0; otherX < mapWidth; otherX++)
        {
            INode node = nodes[y][otherX];
            if (node == null) continue;

            foreach (INode nextNode in node.nextNodes)
            {
                if (nextNode == null) continue;

                int nextX = GetNodeXPosition(nextNode, y + 1);
                if (nextX == -1) continue;

                bool crosses = (otherX < startX && nextX > endX)
                             || (otherX > startX && nextX < endX);
                if (crosses) return true;
            }
        }
        return false;
    }

    private int GetNodeXPosition(INode node, int floorY)
    {
        var row = nodes[floorY];
        for (int x = 0; x < row.Length; x++)
            if (row[x] == node)
                return x;
        return -1;
    }

    private NodeAnchor GetNodeAnchor(int gridX, int gridY, GameObject parentObj)
    {
        float xOffset = Random.Range(-_maxXOffset, _maxXOffset);
        float yOffset = Random.Range(-_maxYOffset, _maxYOffset);

        var (anchorX, anchorY) = _nodeGrid.GetCoords(gridX, gridY);

        Vector3 localPos = new Vector3(anchorX + xOffset, 0f, anchorY + yOffset);

        var prefab = Resources.Load<GameObject>("Prefabs/NodeMap/NodeAnchor");
        GameObject obj = Instantiate(prefab, parentObj.transform);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.identity;

        obj.name = $"NodeAnchor_{gridX}_{gridY}";
        return obj.AddComponent<NodeAnchor>();
    }

    private void SpawnNodes()
    {
        GameObject nodesParentObj = new GameObject("Nodes");
        nodesParentObj.transform.SetParent(transform, false);

        // Load all available enemies once
        EnemyDefinition[] allEnemies = Resources.LoadAll<EnemyDefinition>("Enemies");
        bool hasEnemies = allEnemies != null && allEnemies.Length > 0;

        foreach (var floorPair in nodes)
        {
            int floorIndex = floorPair.Key;
            var floorRow = floorPair.Value;

            GameObject floorParentObj = new GameObject($"Floor_{floorIndex + 1}");
            floorParentObj.transform.SetParent(nodesParentObj.transform, false);

            for (int i = 0; i < floorRow.Length; i++)
            {
                INode node = floorRow[i];
                if (node == null) continue;

                var def = node.nodeDefinition;
                if (def == null || def.prefab == null)
                    continue;

                GameObject nodeObj = Instantiate(def.prefab, floorParentObj.transform);
                nodeObj.transform.position = node.nodeAnchor.transform.position;
                nodeObj.transform.localRotation = Quaternion.identity;

                nodeObj.name = $"Node_{floorIndex + 1}_{i}";

                // Assign random enemy to combat nodes at runtime
                if (node.Type == NodeType.Combat && hasEnemies)
                {
                    NodeBehaviour nodeBehaviour = nodeObj.GetComponent<NodeBehaviour>();
                    if (nodeBehaviour != null)
                    {
                        string randomEnemyName = allEnemies[Random.Range(0, allEnemies.Length)].name;
                        nodeBehaviour.SetEnemy(randomEnemyName);
                    }
                    else
                    {
                        Debug.LogWarning($"[NodeMap] Combat node {nodeObj.name} has no NodeBehaviour component.");
                    }
                }
            }
        }
    }

    private void GeneratePaths()
    {
        GameObject pathsParentObj = new GameObject("Paths");
        pathsParentObj.transform.SetParent(transform, false);

        GameObject pathPrefab = Resources.Load<GameObject>("Prefabs/NodeMap/NodePath");
        if (pathPrefab == null)
            return;

        foreach (var floorPair in nodes)
        {
            var row = floorPair.Value;
            for (int i = 0; i < row.Length; i++)
            {
                INode node = row[i];
                if (node == null) continue;

                foreach (INode nextNode in node.nextNodes)
                {
                    if (nextNode == null) continue;
                    SpawnPath(node, nextNode, pathPrefab, pathsParentObj);
                }
            }
        }
    }

    private GameObject SpawnPath(INode startNode, INode endNode, GameObject pathPrefab, GameObject parentObj)
    {
        float yLevel = startNode.nodeAnchor.transform.position.y + 0.02f;
        Vector3 midpoint = new Vector3(
            (startNode.nodeAnchor.transform.position.x + endNode.nodeAnchor.transform.position.x) / 2,
            yLevel,
            (startNode.nodeAnchor.transform.position.z + endNode.nodeAnchor.transform.position.z) / 2
        );

        Quaternion rotation = Quaternion.LookRotation(
            (endNode.nodeAnchor.transform.position - startNode.nodeAnchor.transform.position).normalized,
            Vector3.up
        );

        GameObject pathObj = Instantiate(pathPrefab, midpoint, rotation, parentObj.transform);
        pathObj.AddComponent<NodeMapPath>();

        float distance = Vector3.Distance(startNode.nodeAnchor.transform.position,
                                          endNode.nodeAnchor.transform.position);
        float scaleZ = (distance - NodeMapPath.PathNodeGap - NodeMapPath.NodeRadius)
                     / NodeMapPath.PathLength;

        Vector3 newScale = pathObj.transform.localScale;
        newScale.z *= scaleZ;
        pathObj.transform.localScale = newScale;
        pathObj.name = $"Path_{startNode.nodeAnchor.name}_to_{endNode.nodeAnchor.name}";
        return pathObj;
    }

    public float GetGridXSpacing()
    {
        return 1.5f;
    }
}
