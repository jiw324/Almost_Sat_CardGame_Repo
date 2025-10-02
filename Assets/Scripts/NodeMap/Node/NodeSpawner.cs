using System;
using System.Collections.Generic;
using System.IO;
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
        const float NODE_RADIUS = 0.5f;
        const float PATH_LENGTH = 0.5f;
        const float PATH_NODE_GAP = 0.1f;
        for (int i = 0; i < nodeAnchors.Count - 1; i++)
        {
            Vector3 nodeOnePos = nodeAnchors[i].transform.position;
            Vector3 nodeTwoPos = nodeAnchors[i + 1].transform.position;
            float nodeDist = Vector3.Distance(nodeOnePos, nodeTwoPos);
            float pathScale = (nodeDist - PATH_NODE_GAP - NODE_RADIUS) / PATH_LENGTH;
            Vector3 midpoint = new Vector3(nodeOnePos.x + ((nodeTwoPos.x - nodeOnePos.x) / 2f), 0.02f,
                                           nodeOnePos.z + ((nodeTwoPos.z - nodeOnePos.z) / 2f));

            Vector3 dir = (nodeTwoPos - nodeOnePos).normalized;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            GameObject pathObj = Instantiate(pathPrefab, midpoint, rot, pathParentObj.transform);
            Vector3 currentScale = pathObj.transform.localScale;
            currentScale.z *= pathScale;
            pathObj.transform.localScale = currentScale;
            pathObj.name = "Path_" + (i + 1);
        }
    }
}
