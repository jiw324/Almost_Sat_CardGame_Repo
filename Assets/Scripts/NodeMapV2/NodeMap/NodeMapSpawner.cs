using UnityEngine;
using System.Collections.Generic;

public class NodeMapSpawner : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private GameObject pathPrefab;
    [SerializeField] private Transform mapParent;

    // A dictionary to link logical nodes to their spawned GameObjects
    private readonly Dictionary<Node, NodeView> _nodeViews = new();

    public void Spawn(NodeMap nodeMap, NodeGrid grid)
    {
        if (nodeMap == null)
        {
            Debug.LogError("NodeMapSpawner: NodeMap is null, cannot spawn visuals.");
            return;
        }

        if (grid == null)
        {
            Debug.LogError("NodeMapSpawner: NodeGrid is null, cannot convert coordinates.");
            return;
        }

        // Create a clean root container for visuals
        Transform root = mapParent != null ? mapParent : new GameObject("NodeMap_Visual").transform;

        // --- Step 1: Spawn nodes ---
        foreach (var rowPair in nodeMap.Rows)
        {
            foreach (Node node in rowPair.Value)
            {
                NodeDefinition def = node.Definition;
                if (def == null || def.prefab == null)
                {
                    Debug.LogWarning($"Node {node.Id} has no prefab assigned.");
                    continue;
                }

                // Convert grid coordinates to world-space
                Vector3 worldPos = grid.GridToWorld(node.GridPos.x, node.GridPos.y);

                // Instantiate prefab
                GameObject nodeObj = Instantiate(def.prefab, worldPos, Quaternion.identity, root);
                nodeObj.name = $"Node_{node.Id}_{def.nodeType}";

                // Ensure NodeView component
                NodeView view = nodeObj.GetComponent<NodeView>();
                if (view == null)
                    view = nodeObj.AddComponent<NodeView>();

                view.Initialize(node);
                _nodeViews[node] = view;
            }
        }

        // --- Step 2: Spawn connection paths ---
        if (pathPrefab != null)
        {
            foreach (var node in nodeMap.AllNodes())
            {
                if (!_nodeViews.ContainsKey(node))
                    continue;

                NodeView startView = _nodeViews[node];
                foreach (Node next in node.NextNodes)
                {
                    if (!_nodeViews.ContainsKey(next))
                        continue;

                    NodeView endView = _nodeViews[next];
                    SpawnPath(startView.transform.position, endView.transform.position, root);
                }
            }
        }

        Debug.Log("NodeMapSpawner: Map successfully spawned.");
    }

    private void SpawnPath(Vector3 startPos, Vector3 endPos, Transform parent)
    {
        Vector3 midpoint = (startPos + endPos) / 2f;
        Quaternion rotation = Quaternion.LookRotation(endPos - startPos, Vector3.up);
        GameObject pathObj = Instantiate(pathPrefab, midpoint, rotation, parent);

        // Adjust scale to match distance
        float distance = Vector3.Distance(startPos, endPos);
        Vector3 scale = pathObj.transform.localScale;
        scale.z = distance;
        pathObj.transform.localScale = scale;
    }
}
