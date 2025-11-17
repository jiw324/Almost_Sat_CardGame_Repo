using UnityEngine;

public class MapSceneInitializer : MonoBehaviour
{
    private void Awake()
    {
        if (MapGenerationManager.Instance.ActiveMap == null)
        {
            MapGenerationManager.Instance.InitializeMapFromSession();
        }
    }
}
