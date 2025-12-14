using System.Collections.Generic;

[System.Serializable]
public class SessionNodeMapData
{
    // Persistent map structure state
    public int mapSeed;
    public string currentNodeId;
    public List<string> visitedNodeIds = new List<string>();
    public List<string> completedNodeIds = new List<string>();

    // Per-node gameplay state (scene-specific)
    public NodeType currentNodeType;
    public string currentNodeJson;

    [System.NonSerialized]
    public SessionNodeData currentNodeData;

    public void ResetSessionData()
    {
        mapSeed = 0;
        currentNodeId = null;
        visitedNodeIds.Clear();
        completedNodeIds.Clear();

        currentNodeType = NodeType.Combat;
        currentNodeJson = "";
        currentNodeData = null;
    }
}
