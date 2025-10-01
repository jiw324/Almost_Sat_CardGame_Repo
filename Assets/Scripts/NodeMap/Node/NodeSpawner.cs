using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class NodeSpawner : MonoBehaviour
{
    private List<NodeAnchor> nodeAnchors;
    [SerializeField] private List<NodeDefinition> predefinedNodes;
    [SerializeField] private GameObject pathPrefab;
    private GameObject pathParentObj;
    private NodeFactory nodeFactory;

    public void Awake()
    {
        nodeFactory = new NodeFactory();
        nodeAnchors = new List<NodeAnchor>();
    }

    public void Start()
    {
        nodeAnchors = FindObjectsByType<NodeAnchor>(FindObjectsSortMode.None)
            .OrderBy(anchor => anchor.name).ToList();
        pathParentObj = FindObjectsByType<PathList>(FindObjectsSortMode.None).ToArray()[0].gameObject;
        if (nodeAnchors.Count == predefinedNodes.Count)
        {
            SpawnNodes();
            SpawnPaths();
        }
    }

    private void SpawnNodes()
    {
        for (int i = 0; i < predefinedNodes.Count; i++)
        {
            INode node = nodeFactory.CreateNode(predefinedNodes[i], 0, 0);
            GameObject nodeObj = Instantiate(node.nodeDefinition.prefab, nodeAnchors[i].transform.position,
                Quaternion.identity);
            nodeObj.transform.parent = nodeAnchors[i].transform;
        }
    }

    private void SpawnPaths()
    {
        for (int i = 0; i < nodeAnchors.Count - 1; i++)
        {
            Vector3 nodeOnePos = nodeAnchors[i].transform.position;
            Vector3 nodeTwoPos = nodeAnchors[i + 1].transform.position;
            Vector3 midpoint = new Vector3(nodeOnePos.x + ((nodeTwoPos.x - nodeOnePos.x) / 2f), 0.02f,
                                           nodeOnePos.z + ((nodeTwoPos.z - nodeOnePos.z) / 2f));

            Vector3 dir = nodeTwoPos - nodeOnePos;
            float angleY = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

            GameObject pathObj = Instantiate(pathPrefab, midpoint, Quaternion.identity);
            pathObj.transform.Rotate(0, angleY, 0);
            pathObj.transform.parent = pathParentObj.transform;
            pathObj.name = "Path_" + (i + 1);
        }
    }
}
