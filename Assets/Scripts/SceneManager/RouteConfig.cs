using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RouteEntry
{
    public GameRoute route;

    #if UNITY_EDITOR
    public UnityEditor.SceneAsset sceneAsset;
    #endif

    public string scenePath;
}


[CreateAssetMenu(menuName = "Routing/Route Config")]
public class RouteConfig : ScriptableObject
{
    public List<RouteEntry> routes;

    public bool GetScenePath(GameRoute route, out string scenePath)
    {
        foreach (var r in routes)
        {
            if (r.route == route && !string.IsNullOrEmpty(r.scenePath))
            {
                scenePath = r.scenePath;
                return true;
            }
        }
        scenePath = null;
        return false;
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (var entry in routes)
        {
            if (entry.sceneAsset != null)
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(entry.sceneAsset);

                entry.scenePath = path;
            }
        }
    }
    #endif

}
