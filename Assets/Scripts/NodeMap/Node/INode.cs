using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEditor.Experimental.GraphView;

public interface INode
{
    List<INode> parents { get; }
    List<INode> children { get; }
    NodeType nodeType { get; }
    int depthIndex { get; }
    int pathDepth { get; }
    float nodeWeight { get; }

    List<INode> GetParents();
    List<INode> GetChildren();
    NodeType GetNodeType();
    int GetDepthIndex();
    int GetPathDepth();
    float GetNodeWeight();
}
