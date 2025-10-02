using System.Collections.Generic;

public interface INode
{
    NodeDefinition nodeDefinition { get; }
    List<INode> parents { get; }
    List<INode> children { get; }
    NodeType nodeType { get; }
    int depthIndex { get; }
    int pathDepth { get; }
    float nodeWeight { get; }
}
