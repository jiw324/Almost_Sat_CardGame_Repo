using System.Linq;
using UnityEngine;

public class NodeView : MonoBehaviour
{
    public Node NodeData { get; private set; }
    public NodeDefinition Definition => NodeData?.Definition;

    private NodeGlowView glow;

    public void Initialize(Node node)
    {
        NodeData = node;
        glow = GetComponentInChildren<NodeGlowView>(true);

        UpdateGlow();
    }

    public void UpdateGlow()
    {
        if (glow == null) return;

        var mgr = MapGenerationManager.Instance;

        if (mgr.IsNodeCompleted(NodeData))
        {
            glow.SetCompleted();
            return;
        }

        if (mgr.IsNodeVisited(NodeData))
        {
            glow.SetVisited();
            return;
        }

        glow.ClearGlow();
    }

    public void ShowAvailable(bool hover)
    {
        if (glow == null) return;
        glow.SetAvailable(hover);
    }

    public void ClearHoverGlow()
    {
        if (glow == null) return;

        var mgr = MapGenerationManager.Instance;
        var posMgr = FindFirstObjectByType<MapPositionManager>();

        bool isAvailable = posMgr.GetAvailableNodes()
            .Any(n => n.Id == NodeData.Id);

        if (isAvailable)
        {
            glow.SetAvailable(false);
            return;
        }

        UpdateGlow();
    }

}
