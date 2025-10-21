using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Haptics;

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
        _nodeGrid = new NodeGrid(5, 5);
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
        SpawnNodes(_nodeFactory);
        //SpawnPaths();
    }

    private void GenerateNodes()
    {
        Debug.Log("Generating Nodes");

        ChooseStartNodes();
        GeneratePaths();
    }

    private void ChooseStartNodes()
    {
        int firstX = 0;
        for (int i = 0; i < _maxNodesPerFloor; i++)
        {
            int randomX = UnityEngine.Random.Range(0, _nodeGrid.width);
            if (i == 0)
            {
                firstX = randomX;
            }
            else if (i == 1)
            {
                while (randomX == firstX)
                {
                    randomX = UnityEngine.Random.Range(0, _nodeGrid.width);
                }
            }
            _startNodeXVals.Add(randomX);
        }
    }

    private void GeneratePaths()
    {
        GameObject nodeAnchorParentObj = new GameObject("NodeAnchors");
        nodeAnchorParentObj.transform.position = Vector3.zero;
        nodeAnchorParentObj.transform.rotation = Quaternion.identity;
        nodeAnchorParentObj.transform.parent = transform;        

        foreach(int gridX in _startNodeXVals)
        {
            int x = gridX;
            int minDeltaX = gridX > 0 ? -1 : 0;
            int maxDeltaX = gridX < _nodeGrid.width - 1 ? 1 : 0;

            INode startNode = _nodeFactory.CreateNode(NodeType.Combat, GetNodeAnchor(x, 0, nodeAnchorParentObj));

            if(_nodes[0][gridX] == null)
            {
                _nodes[0][gridX] = startNode;
            }

            for(int i = 0; i < _nodeGrid.height - 1; i++)
            {
                if (i > 0)
                {
                    startNode = _nodeFactory.CreateNode(NodeType.Combat, GetNodeAnchor(x, i, nodeAnchorParentObj));
                }

                int randomDeltaX;
                do
                {
                    randomDeltaX = UnityEngine.Random.Range(minDeltaX, maxDeltaX + 1);
                } while (CheckForCrossPath(x, x + randomDeltaX, i));

                GenerateNextNodeAndPath(startNode, x, x += randomDeltaX, i + 1, nodeAnchorParentObj);
            }
        }
    }

    private void GenerateNextNodeAndPath(INode startNode, int startX, int nextX, int nextY, GameObject parentObj)
    {
        INode nextNode;
        if (_nodes[nextY][nextX] == null)
        {
            nextNode = _nodeFactory.CreateNode(NodeType.Combat, GetNodeAnchor(nextX, nextY, parentObj));
            _nodes[nextY][nextX] = nextNode;
        }
        else
        {
            nextNode = _nodes[nextY][nextX];
        }

        startNode.AddNextNode(nextNode, nextX - startX);
    }
    
    private bool CheckForCrossPath(int startX, int endX, int y)
    {
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

    private void SpawnNodes(NodeFactory nodeFactory)
    {
        GameObject nodesParentObj = new GameObject("Nodes");
        nodesParentObj.transform.position = Vector3.zero;
        nodesParentObj.transform.rotation = Quaternion.identity;
        nodesParentObj.transform.parent = transform;

        foreach (var nodeData in _nodes)
        {
            GameObject floorParentObj = new GameObject($"Floor_{nodeData.Key + 1}");
            nodesParentObj.transform.position = Vector3.zero;
            nodesParentObj.transform.rotation = Quaternion.identity;
            nodesParentObj.transform.parent = nodesParentObj.transform;

            for(int i = 0; i < nodeData.Value.Length; i++)
            {
                INode node = nodeData.Value[i];
                if(node != null)
                {
                    GameObject nodeObj = Instantiate(node.nodeDefinition.prefab, node.nodeAnchor.transform.position,
                    Quaternion.identity);
                    nodeObj.transform.SetParent(floorParentObj.transform);
                    nodeObj.name = $"Node_{nodeData.Key + 1}_{i}";
                }
            }
        }
    }

    // private void SpawnPaths()
    // {
    //     GameObject pathPrefab = Resources.Load<GameObject>("Prefabs/NodeMap/NodePath");

    //     if (pathPrefab != null)
    //     {
    //         GameObject pathsParentObj = new GameObject("Paths");
    //         pathsParentObj.transform.position = Vector3.zero;
    //         pathsParentObj.transform.rotation = Quaternion.identity;
    //         pathsParentObj.transform.parent = transform;

    //         int index = 0;
    //         foreach (var path in _nodeMapPaths.Values)
    //         {
    //             //path.Spawn(pathsParentObj, pathPrefab, index);
    //             index++;
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("Path prefab not found");
    //     }
    // }

    private NodeAnchor GetNodeAnchor(int gridX, int gridY, GameObject parentObj)
    {
        float xOffset = UnityEngine.Random.Range(-_maxXOffset, _maxXOffset);
        float yOffset = UnityEngine.Random.Range(-_maxYOffset, _maxYOffset);

        var (anchorX, anchorY) = _nodeGrid.GetCoords(gridX, gridY);
        Vector3 nodeAnchorPos = new Vector3(anchorX + xOffset, 0.02f, anchorY + yOffset);

        GameObject nodeAnchorPrefab = Resources.Load<GameObject>("Prefabs/NodeMap/NodeAnchor");

        GameObject nodeAnchorObj = Instantiate(nodeAnchorPrefab, nodeAnchorPos, Quaternion.identity, parentObj.transform);
        nodeAnchorObj.name = $"NodeAnchor_{gridX}_{gridY}";
        return nodeAnchorObj.AddComponent<NodeAnchor>();
    }
}
