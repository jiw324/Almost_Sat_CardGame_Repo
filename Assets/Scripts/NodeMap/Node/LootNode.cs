public class LootNode : NodeBase
{
    public LootNode(NodeDefinition nodeDefinition, NodeAnchor nodeAnchor)
        : base(nodeDefinition, nodeAnchor)
    {
    }

    public override void AddNextNode(INode node, int xDelta)
    {
        base.AddNextNode(node, xDelta);
    }
}