public class RestNode : NodeBase
{
    public RestNode(NodeDefinition nodeDefinition, NodeAnchor nodeAnchor)
        : base(nodeDefinition, nodeAnchor)
    {
    }

    public override void AddNextNode(INode node, int xDelta)
    {
        base.AddNextNode(node, xDelta);
    }
}