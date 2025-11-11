using System.Collections.Generic;
using UnityEngine;

public class NodeMapVisualContext
{
    public Bounds MapBounds { get; }
    public List<NodeView> SpawnedNodes { get; }
    public List<GameObject> SpawnedPaths { get; }

    public NodeMapVisualContext(Bounds bounds, List<NodeView> nodes, List<GameObject> paths)
    {
        MapBounds = bounds;
        SpawnedNodes = nodes;
        SpawnedPaths = paths;
    }
}
