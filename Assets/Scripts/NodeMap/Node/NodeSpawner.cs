using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeSpawner : MonoBehaviour
{
    [SerializeField] private List<NodeDefinition> predefinedNodes;
    [SerializeField] private GameObject pathPrefab;
    private List<NodeAnchor> nodeAnchors;
    private GameObject pathParentObj;
    private NodeFactory nodeFactory;
    private const float NodeRadius = 0.5f;
    private const float PathLength = 0.5f;
    private const float PathNodeGap = 0.1f;

    private void Awake()
    {
        nodeFactory = new NodeFactory();
        nodeAnchors = new List<NodeAnchor>();
    }

    private void Start()
    {
        InitializeAnchorsAndPaths();

        if (nodeAnchors.Count == predefinedNodes.Count)
        {
            SpawnNodes();
            SpawnPaths();
        }
    }

    private void InitializeAnchorsAndPaths()
    {
        nodeAnchors = FindObjectsByType<NodeAnchor>(FindObjectsSortMode.None)
            .OrderBy(anchor => anchor.name).ToList();

        pathParentObj = FindObjectsByType<PathList>(FindObjectsSortMode.None)
            .FirstOrDefault()?.gameObject;
    }

    private void SpawnNodes()
    {
        for (int i = 0; i < predefinedNodes.Count; i++)
        {
            var node = nodeFactory.CreateNode(predefinedNodes[i], 0, 0);
            var anchor = nodeAnchors[i];

            GameObject nodeObj = Instantiate(node.nodeDefinition.prefab, anchor.transform.position,
                Quaternion.identity);
            nodeObj.transform.SetParent(anchor.transform);
        }
    }

    private void SpawnPaths()
    {
        for (int i = 0; i < nodeAnchors.Count - 1; i++)
        {
            CreatePathBetween(nodeAnchors[i], nodeAnchors[i + 1], i + 1);
        }
    }

    private void CreatePathBetween(NodeAnchor startAnchor, NodeAnchor endAnchor, int pathIndex)
    {
        Vector3 startPos = startAnchor.transform.position;
        Vector3 endPos = endAnchor.transform.position;

        float distance = Vector3.Distance(startPos, endPos);
        float scaleZ = (distance - PathNodeGap - NodeRadius) / PathLength;

        Vector3 midpoint = new Vector3(
            (startPos.x + endPos.x) * 0.5f,
            0.02f,
            (startPos.z + endPos.z) * 0.5f
        );

        Quaternion rotation = Quaternion.LookRotation((endPos - startPos).normalized, Vector3.up);

        GameObject pathObj = Instantiate(pathPrefab, midpoint, rotation, pathParentObj.transform);

        Vector3 newScale = pathObj.transform.localScale;
        newScale.z *= scaleZ;
        pathObj.transform.localScale = newScale;

        pathObj.name = $"Path_{pathIndex}";
    }
}
