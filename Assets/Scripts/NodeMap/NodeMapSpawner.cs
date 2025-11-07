using UnityEngine;

public class NodeMapSpawner : MonoBehaviour
{
    [SerializeField] private Vector3 mapOffsetFromCamera;
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

        float gridXSpacing = _nodeMap.GetGridXSpacing();
        float halfMapWidthWorld = (_nodeMap.mapWidth - 1) * 0.5f * gridXSpacing;
        _nodeMap.transform.position -= mainCamera.transform.right * halfMapWidthWorld;

        MapCameraController camController = mainCamera.GetComponent<MapCameraController>();
        if (camController != null)
        {
            float gridX = _nodeMap.GetGridXSpacing();
            float gridY = 1.5f;

            camController.SetBounds(
                _nodeMap.transform.position,
                _nodeMap.mapWidth,
                _nodeMap.mapHeight,
                gridX,
                gridY
            );
        }

    }
}
