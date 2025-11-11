[System.Serializable]
public class SessionNodeMapData
{
    public NodeType currentNodeType;
    public string currentNodeJson;
    
    [System.NonSerialized]
    public ISessionNodeData currentNodeData;

    public void ResetSessionData()
    {
        currentNodeType = NodeType.Combat;
        currentNodeJson = "";
        currentNodeData = null;
    }
}