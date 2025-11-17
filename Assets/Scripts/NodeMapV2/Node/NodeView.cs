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

        if (state.IsNodeCompleted(NodeData))
        {
            glow.SetCompleted();
            return;
        }

        if (state.IsNodeVisited(NodeData))
        {
            glow.SetVisited();
            return;
        }

        glow.ClearGlow();
    }

    public void ShowAvailable()
    {
        if (glow == null) return;
        glow.SetAvailable();
    }

    public void ShowHover()
    {
        if (glow == null) return;
        glow.SetHover();
    }

    public void ClearHoverGlow()
    {
        if (glow == null) return;

        var state = MapStateManager.Instance;

        bool isAvailable =
            state.GetAvailableNodes().Any(n => n.Id == NodeData.Id);

        if (isAvailable)
        {
            glow.SetAvailable();
            return;
        }

        UpdateGlow();
    }
}
