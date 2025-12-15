using System.Linq;
using UnityEngine;

public class NodeView : MonoBehaviour
{
    public Node NodeData { get; private set; }
    public NodeDefinition Definition => NodeData?.Definition;

    private NodeGlowView glow;

    [SerializeField] private GameObject revealedVisualRoot;
    private GameObject hiddenVisualRoot;

    private bool isHidden;

    public void Initialize(Node node)
    {
        NodeData = node;
        gameObject.name = $"NodeView_{node.Id}_{node.Definition.nodeType}";

        glow = GetComponentInChildren<NodeGlowView>(true);

        if (revealedVisualRoot == null)
            revealedVisualRoot = gameObject;

        SetHidden(false);
        UpdateGlow();
    }

    public void SetHiddenVisualRoot(GameObject obj)
    {
        hiddenVisualRoot = obj;
        if (hiddenVisualRoot != null)
            hiddenVisualRoot.SetActive(false);
    }

    public void SetHidden(bool hidden)
    {
        isHidden = hidden;

        if (revealedVisualRoot != null)
            revealedVisualRoot.SetActive(!hidden);

        if (hiddenVisualRoot != null)
            hiddenVisualRoot.SetActive(hidden);

        if (!hidden)
        {
            UpdateGlow();

            var state = MapStateManager.Instance;
            if (state != null &&
                state.GetAvailableNodes().Any(n => n.Id == NodeData.Id))
            {
                ShowAvailable();
            }
        }
    }


    public void UpdateGlow()
    {
        if (glow == null || NodeData == null)
            return;

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

    public void ShowAvailable(bool hover = false)
    {
        if (glow == null) return;

        if (hover)
            glow.SetHover();
        else
            glow.SetAvailable();
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
