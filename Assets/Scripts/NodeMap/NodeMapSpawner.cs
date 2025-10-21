using UnityEngine;

public class NodeMapSpawner : MonoBehaviour
{
    private NodeMap _nodeMap;

    private void Start()
    {
        GameObject map = GameObject.Find("NodeMap");
        _nodeMap = map.AddComponent<NodeMap>();
    }
}
