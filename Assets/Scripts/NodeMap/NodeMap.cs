using System.Collections.Generic;
using UnityEngine;

public class NodeMap : MonoBehaviour
{
    private NodeGrid _nodeGrid;
    private Dictionary<int, INode[]> _nodes;
    private HashSet<int> _startNodeXVals;
    private NodeFactory _nodeFactory;

    private const float _maxXOffset = 0.3f;
    private const float _maxYOffset = 0.3f;
    private int _maxNodesPerFloor = 0;

    public void Awake()
    {
        _nodeGrid = new NodeGrid(8, 15);
        _nodes = new Dictionary<int, INode[]>();
        for (int y = 0; y < _nodeGrid.height; y++)
        {
            _nodes[y] = new INode[_nodeGrid.width];
        }

        _nodeFactory = new NodeFactory();
        _maxNodesPerFloor = Mathf.RoundToInt(_nodeGrid.width * 0.75f);
        _startNodeXVals = new HashSet<int>();
    }

    public void Start()
    {
        GenerateNodes();
        GeneratePaths();
    }

    private void GenerateNodes()
    {
        Debug.Log("Generating Nodes");

        ChooseStartNodes();
        CreateRoutes();
        SpawnNodes();
    }

    private void ChooseStartNodes()
    {
        _startNodeXVals.Clear();

        while (_startNodeXVals.Count < _maxNodesPerFloor)
        {
            _startNodeXVals.Add(Random.Range(0, _nodeGrid.width));
        }
    }

    private void CreateRoutes()
    {
        GameObject nodeAnchorParentObj = new GameObject("NodeAnchors");
        nodeAnchorParentObj.transform.position = Vector3.zero;
        nodeAnchorParentObj.transform.rotation = Quaternion.identity;
        nodeAnchorParentObj.transform.parent = transform;

        foreach (int gridX in _startNodeXVals)
        {
            int x = gridX;
            if (_nodes[0][x] == null)
            {
                _nodes[0][x] = _nodeFactory.CreateNode(NodeType.Combat, GetNodeAnchor(x, 0, nodeAnchorParentObj));
            }

            int minDeltaX = x > 0 ? -1 : 0;
            int maxDeltaX = x < _nodeGrid.width - 1 ? 1 : 0;

            for (int y = 0; y < _nodeGrid.height - 1; y++)
            {
                int randomDeltaX;
                int attemptCount = 0;

                do
                {
                    randomDeltaX = Random.Range(minDeltaX, maxDeltaX + 1);
                    attemptCount++;
                    if (attemptCount > 10)
                    {
                        randomDeltaX = 0;
                        break;
                    }
                } while (CheckForCrossPath(x, x + randomDeltaX, y));

                INode nextNode = GenerateNextNode(x + randomDeltaX, y + 1, nodeAnchorParentObj);
                _nodes[y][x].AddNextNode(nextNode, randomDeltaX);

                x += randomDeltaX;
            }
        }
    }

    private INode GenerateNextNode(int nextX, int nextY, GameObject parentObj)
    {
        if (_nodes[nextY][nextX] != null)
        {
            return _nodes[nextY][nextX];
        }

        NodeAnchor nextAnchor = GetNodeAnchor(nextX, nextY, parentObj);
        INode nextNode = _nodeFactory.CreateNode(NodeType.Combat, nextAnchor);
        _nodes[nextY][nextX] = nextNode;
        return nextNode;
    }

    private bool CheckForCrossPath(int startX, int endX, int y)
    {
        if (endX < 0 || endX >= _nodeGrid.width)
            return true;

        if (startX == endX)
            return false;

        for (int otherX = 0; otherX < _nodeGrid.width; otherX++)
        {
            INode node = _nodes[y][otherX];
            if (node == null) continue;

            foreach (INode nextNode in node.nextNodes)
            {
                if (nextNode == null) continue;

                int nextX = GetNodeXPosition(nextNode, y + 1);
                if (nextX == -1) continue;

                bool crosses = (otherX < startX && nextX > endX) || (otherX > startX && nextX < endX);
                if (crosses) return true;
            }
        }

        return false;
    }

    private int GetNodeXPosition(INode node, int floorY)
    {
        INode[] row = _nodes[floorY];
        for (int x = 0; x < row.Length; x++)
        {
            if (row[x] == node)
                return x;
        }
        return -1;
    }

    private void SpawnNodes()
    {
        GameObject nodesParentObj = new GameObject("Nodes");
        nodesParentObj.transform.position = Vector3.zero;
        nodesParentObj.transform.rotation = Quaternion.identity;
        nodesParentObj.transform.parent = transform;

        foreach (var nodeData in _nodes)
        {
            GameObject floorParentObj = new GameObject($"Floor_{nodeData.Key + 1}");
            floorParentObj.transform.position = Vector3.zero;
            floorParentObj.transform.rotation = Quaternion.identity;
            floorParentObj.transform.parent = nodesParentObj.transform;

            for (int i = 0; i < nodeData.Value.Length; i++)
            {
                INode node = nodeData.Value[i];
                if (node != null)
                {
                    GameObject nodeObj = Instantiate(node.nodeDefinition.prefab, node.nodeAnchor.transform.position, Quaternion.identity);
                    nodeObj.transform.SetParent(floorParentObj.transform);
                    nodeObj.name = $"Node_{nodeData.Key + 1}_{i}";
                }
            }
        }
    }

    private void GeneratePaths()
    {
        GameObject pathsParentObj = new GameObject("Paths");
        pathsParentObj.transform.position = Vector3.zero;
        pathsParentObj.transform.rotation = Quaternion.identity;
        pathsParentObj.transform.parent = transform;

        GameObject pathPrefab = Resources.Load<GameObject>("Prefabs/NodeMap/NodePath");

        if (pathPrefab != null)
        {
            foreach (var nodeData in _nodes)
            {
                for (int i = 0; i < nodeData.Value.Length; i++)
                {
                    if (nodeData.Value[i] == null)
                        continue;

                    INode node = nodeData.Value[i];

                    foreach (INode nextNode in node.nextNodes)
                    {
                        if (nextNode == null) continue;
                        SpawnPath(node, nextNode, pathPrefab, pathsParentObj);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Path prefab not found");
        }
    }

    private GameObject SpawnPath(INode startNode, INode endNode, GameObject pathPrefab, GameObject parentObj)
    {
        Vector3 midpoint = new Vector3(
            (startNode.nodeAnchor.transform.position.x + endNode.nodeAnchor.transform.position.x) / 2,
            0.02f,
            (startNode.nodeAnchor.transform.position.z + endNode.nodeAnchor.transform.position.z) / 2
        );

        Quaternion rotation = Quaternion.LookRotation(
            (endNode.nodeAnchor.transform.position - startNode.nodeAnchor.transform.position).normalized,
            Vector3.up
        );

        GameObject pathObj = Instantiate(pathPrefab, midpoint, rotation, parentObj.transform);
        pathObj.AddComponent<NodeMapPath>();

        float distance = Vector3.Distance(
            startNode.nodeAnchor.transform.position,
            endNode.nodeAnchor.transform.position
        );

        float scaleZ = (distance - NodeMapPath.PathNodeGap - NodeMapPath.NodeRadius) / NodeMapPath.PathLength;

        Vector3 newScale = pathObj.transform.localScale;
        newScale.z *= scaleZ;
        pathObj.transform.localScale = newScale;

        pathObj.name = $"Path_{startNode.nodeAnchor.name}_to_{endNode.nodeAnchor.name}";
        return pathObj;
    }

    private NodeAnchor GetNodeAnchor(int gridX, int gridY, GameObject parentObj)
    {
        float xOffset = Random.Range(-_maxXOffset, _maxXOffset);
        float yOffset = Random.Range(-_maxYOffset, _maxYOffset);

        var (anchorX, anchorY) = _nodeGrid.GetCoords(gridX, gridY);
        Vector3 nodeAnchorPos = new Vector3(anchorX + xOffset, 0.02f, anchorY + yOffset);

        GameObject nodeAnchorPrefab = Resources.Load<GameObject>("Prefabs/NodeMap/NodeAnchor");

        GameObject nodeAnchorObj = Instantiate(nodeAnchorPrefab, nodeAnchorPos, Quaternion.identity, parentObj.transform);
        nodeAnchorObj.name = $"NodeAnchor_{gridX}_{gridY}";
        return nodeAnchorObj.AddComponent<NodeAnchor>();
    }
}
