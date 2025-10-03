public class NodeFactory
{
    public INode CreateNode(NodeDefinition nodeDefinition, int depthIndex, int pathDepth)
    {
        switch (nodeDefinition.nodeType)
        {
            case NodeType.Loot:
                return new LootNode(nodeDefinition, depthIndex, pathDepth);
            case NodeType.Rest:
                return new RestNode(nodeDefinition, depthIndex, pathDepth);
            case NodeType.Shop:
                return new ShopNode(nodeDefinition, depthIndex, pathDepth);
            case NodeType.Event:
                return new EventNode(nodeDefinition, depthIndex, pathDepth);
            case NodeType.Combat:
                return new CombatNode(nodeDefinition, depthIndex, pathDepth);
            default:
                throw new System.ArgumentException("Invalid Node Type", nameof(nodeDefinition.nodeType));
        }
    }   
}
