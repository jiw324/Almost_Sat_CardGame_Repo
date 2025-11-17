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
        gameObject.name = $"NodeView_{node.Id}_{node.Definition.nodeType}";

        glow = GetComponentInChildren<NodeGlowView>(true);

        UpdateGlow();
    }

    public void UpdateGlow()
    {
        if (glow == null) return;

        var state = MapStateManager.Instance;

        // Priority 1 — Completed
        if (state.IsNodeCompleted(NodeData))
        {
            glow.SetCompleted();
            return;
        }

        // Priority 2 — Visited (current node)
        if (state.IsNodeVisited(NodeData))
        {
            glow.SetVisited();
            return;
        }

        // Default
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

        var state = MapStateManager.Instance;

        // Determine if this node is “available” after hover ends
        bool isAvailable =
            state.GetAvailableNodes().Any(n => n.Id == NodeData.Id);

        if (isAvailable)
        {
            glow.SetAvailable(false); // return to dim white available glow
            return;
        }

        // Otherwise revert to state-dependent glow
        UpdateGlow();
    }
}
