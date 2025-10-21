using UnityEngine;

public class NodeMapPath : MonoBehaviour
{
    private const float NodeRadius = 0.5f;
    private const float PathLength = 0.5f;
    private const float PathNodeGap = 0.1f;

    // public void Spawn(GameObject pathParentObj, GameObject pathPrefab, int pathIndex)
    // {
    //     Vector3 startPos = startNode.nodeDefinition.prefab.transform.position;
    //     Vector3 endPos = endNode.nodeDefinition.prefab.transform.position;

    //     float distance = Vector3.Distance(startPos, endPos);
    //     float scaleZ = (distance - PathNodeGap - NodeRadius) / PathLength;

    //     Vector3 midpoint = new Vector3(
    //         (startPos.x + endPos.x) / 2,
    //         0.02f,
    //         (startPos.z + endPos.z) / 2
    //     );

    //     Quaternion rotation = Quaternion.LookRotation((endPos - startPos).normalized, Vector3.up);

    //     GameObject pathObj = Instantiate(pathPrefab, midpoint, rotation, pathParentObj.transform);

    //     Vector3 newScale = pathObj.transform.localScale;
    //     newScale.z *= scaleZ;
    //     pathObj.transform.localScale = newScale;

    //     pathObj.name = $"Path_{pathIndex}";
    // }
}
