using UnityEngine;

public class NodeMapSpawner : MonoBehaviour
{
    [SerializeField] private Vector3 mapOffsetFromCamera = new Vector3(0, -2f, 5f);
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
    }
}
