using UnityEngine;

public class NodeMapSpawner : MonoBehaviour
{
    [SerializeField] private Vector3 mapOffsetFromCamera = new Vector3(0, -3f, 15f);
    private NodeMap _nodeMap;

    private void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found in the scene.");
            return;
        }

        Vector3 spawnPosition =
            mainCamera.transform.position +
            mainCamera.transform.forward * mapOffsetFromCamera.z +
            mainCamera.transform.up * mapOffsetFromCamera.y +
            mainCamera.transform.right * mapOffsetFromCamera.x;

        GameObject mapObject = new GameObject("NodeMap");
        mapObject.transform.position = spawnPosition;
        mapObject.transform.rotation = Quaternion.identity;

        _nodeMap = mapObject.AddComponent<NodeMap>();
        _nodeMap.Generate();

        float halfMapWidthWorld = (_nodeMap.mapWidth - 1) * 0.5f * _nodeMap.GetGridXSpacing();
        _nodeMap.transform.position -= mainCamera.transform.right * halfMapWidthWorld;

        MapPositionManager positionManager = mapObject.AddComponent<MapPositionManager>();
        positionManager.Initialize(_nodeMap);

        INode firstStarter = GetFirstStarterNode(_nodeMap);
        if (firstStarter != null)
        {
            positionManager.SelectStarterNode(firstStarter);
        }
        else
        {
            Debug.LogWarning("No starter node found to select at initialization.");
        }

        MapCameraController camController = mainCamera.GetComponent<MapCameraController>();
        if (camController != null)
        {
            float gridX = _nodeMap.GetGridXSpacing();
            float gridY = 1.5f;
            camController.SetBounds(_nodeMap.transform.position, _nodeMap.mapWidth, _nodeMap.mapHeight, gridX, gridY);
        }
    }

    private INode GetFirstStarterNode(NodeMap nodeMap)
    {
        if (!nodeMap.nodes.ContainsKey(0))
            return null;

        foreach (var node in nodeMap.nodes[0])
        {
            if (node != null)
                return node;
        }

        return null;
    }
}
