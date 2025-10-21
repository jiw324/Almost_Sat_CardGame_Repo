using System.Collections.Generic;
using UnityEngine;

public class NodeMap : MonoBehaviour
{
    private NodeGrid _nodeGrid;
    private Dictionary<int, INode[]> _nodes;
    private HashSet<int> _startNodeXVals;
    private NodeFactory _nodeFactory;

    private const float _maxXOffset = 0.25f;
    private const float _maxYOffset = 0.25f;
    private int _maxNodesPerFloor = 0;

    public void Awake()
    {
        _nodeGrid = new NodeGrid(7, 15);
        _nodes = new Dictionary<int, INode[]>();
        for(int y = 0; y < _nodeGrid.height; y++)
        {
            _nodes[y] = new INode[_nodeGrid.width];
        }
        _startNodeXVals = new HashSet<int>();
        _nodeFactory = new NodeFactory();
        _maxNodesPerFloor = Mathf.RoundToInt(_nodeGrid.width * 0.75f);
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
        int firstX = 0;
        for (int i = 0; i < _maxNodesPerFloor; i++)
        {
            int randomX = Random.Range(0, _nodeGrid.width);
            if (i == 0)
            {
                firstX = randomX;
            }
            else if (i == 1)
            {
                while (randomX == firstX)
                {
                    randomX = Random.Range(0, _nodeGrid.width);
                }
            }
            _startNodeXVals.Add(randomX);
        }
    }

    private void CreateRoutes()
    {
        GameObject nodeAnchorParentObj = new GameObject("NodeAnchors");
        nodeAnchorParentObj.transform.position = Vector3.zero;
        nodeAnchorParentObj.transform.rotation = Quaternion.identity;
        nodeAnchorParentObj.transform.parent = transform;

        foreach(int gridX in _startNodeXVals)
        {
            int x = gridX;
            _nodes[0][x] = _nodeFactory.CreateNode(NodeType.Combat, GetNodeAnchor(x, 0, nodeAnchorParentObj));
            int minDeltaX = gridX > 0 ? -1 : 0;
            int maxDeltaX = gridX < _nodeGrid.width - 1 ? 1 : 0;

            for(int i = 0; i < _nodeGrid.height - 1; i++)
            {
                int randomDeltaX;
                do
                {
                    randomDeltaX = Random.Range(minDeltaX, maxDeltaX + 1);
                } while (CheckForCrossPath(x, x + randomDeltaX, i));

                INode nextNode = GenerateNextNode(x + randomDeltaX, i + 1, nodeAnchorParentObj);
                _nodes[i][x].AddNextNode(nextNode, randomDeltaX);

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
        if(endX < 0 || endX >= _nodeGrid.width)
        {
            return true;
        }
        if(startX == endX)
        {
            return false;
        } else
        {
            INode checkNode = _nodes[y][endX];
            if (checkNode != null)
            {
                if (checkNode.nextNodes[startX - endX + 1] == null)
                {
                    return false;
                }
                return true;
            }
            return false;
        }
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
                    GameObject nodeObj = Instantiate(node.nodeDefinition.prefab, node.nodeAnchor.transform.position,
                        Quaternion.identity);
                    nodeObj.transform.SetParent(floorParentObj.transform);
                    nodeObj.name = $"Node_{nodeData.Key + 1}_{i}";
                }
            }
        }
    }

    private void GeneratePaths()
    {
        GameObject pathPrefab = Resources.Load<GameObject>("Prefabs/NodeMap/NodePath");

        if (pathPrefab != null)
        {
            foreach (var nodeData in _nodes)
            {
                for (int i = 0; i < nodeData.Value.Length; i++)
                {
                    if (nodeData.Value[i] == null)
                    {
                        continue;
                    }

                    INode node = nodeData.Value[i];

                    for (int n = 0; n < node.nextNodes.Length; n++)
                    {
                        INode nextNode = node.nextNodes[n];
                        if (nextNode == null)
                        {
                            continue;
                        }

                        SpawnPath(node, nextNode, pathPrefab);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Path prefab not found");
        }
    }
    
    private GameObject SpawnPath(INode startNode, INode endNode, GameObject pathPrefab)
    {    
        GameObject pathsParentObj = new GameObject("Paths");
        pathsParentObj.transform.position = Vector3.zero;
        pathsParentObj.transform.rotation = Quaternion.identity;
        pathsParentObj.transform.parent = transform;

        Vector3 midpoint = new Vector3(
            (startNode.nodeAnchor.transform.position.x + endNode.nodeAnchor.transform.position.x) / 2,
            0.02f,
            (startNode.nodeAnchor.transform.position.z + endNode.nodeAnchor.transform.position.z) / 2
        );

        Quaternion rotation = Quaternion.LookRotation((endNode.nodeAnchor.transform.position -
            startNode.nodeAnchor.transform.position).normalized, Vector3.up);

        GameObject pathObj = Instantiate(pathPrefab, midpoint, rotation, pathsParentObj.transform);
        pathObj.AddComponent<NodeMapPath>();

        float distance = Vector3.Distance(startNode.nodeAnchor.transform.position,
            endNode.nodeAnchor.transform.position);
        float scaleZ = (distance - NodeMapPath.PathNodeGap - NodeMapPath.NodeRadius) / NodeMapPath.PathLength;

        Vector3 newScale = pathObj.transform.localScale;
        newScale.z *= scaleZ;
        pathObj.transform.localScale = newScale;

        pathObj.name = "Path"; 

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
