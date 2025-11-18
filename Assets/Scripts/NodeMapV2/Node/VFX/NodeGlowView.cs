using UnityEngine;

public class NodeGlowView : MonoBehaviour
{
    private MeshRenderer ringRenderer;

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
        ringRenderer.sharedMaterial = defaultMat;
    }

    public void SetVisited()
    {
        ringRenderer.sharedMaterial = visitedMat;
    }

    public void SetCompleted()
    {
        ringRenderer.sharedMaterial = completedMat;
    }

    public void SetAvailable()
    {
        ringRenderer.sharedMaterial = availableMat;
    }

    public void SetHover()
    {
        ringRenderer.sharedMaterial = hoverMat;
    }
}
