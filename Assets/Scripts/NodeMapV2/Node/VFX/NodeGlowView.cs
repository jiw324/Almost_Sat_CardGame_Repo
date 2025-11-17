using UnityEngine;

public class NodeGlowView : MonoBehaviour
{
    private MeshRenderer ringRenderer;

    [Header("Materials")]
    public Material defaultMat;
    public Material visitedMat;
    public Material completedMat;
    public Material availableMat;
    public Material hoverMat;

    private void Awake()
    {
        ringRenderer = GetComponent<MeshRenderer>();
    }

    public void ClearGlow()
    {
        ringRenderer.material = defaultMat;
    }

    public void SetVisited()
    {
        ringRenderer.material = visitedMat;
    }

    public void SetCompleted()
    {
        ringRenderer.material = completedMat;
    }

    public void SetAvailable(bool hover)
    {
        ringRenderer.material = hover ? hoverMat : availableMat;
    }
}
