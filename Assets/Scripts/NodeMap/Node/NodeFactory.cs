
public class NodeFactory
{
    public INode CreateNode(NodeType nodeType, int depthIndex, int pathDepth)
    {
        switch (nodeType)
        {
            case NodeType.Loot:
                return new LootNode(depthIndex, pathDepth);
            case NodeType.Rest:
                return new RestNode(depthIndex, pathDepth);
            case NodeType.Shop:
                return new ShopNode(depthIndex, pathDepth);
            case NodeType.Event:
                return new EventNode(depthIndex, pathDepth);
            case NodeType.Combat:
                return new CombatNode(depthIndex, pathDepth);
            default:
                throw new System.ArgumentException("Invalid Node Type", nameof(nodeType));
        }
    }   
}
