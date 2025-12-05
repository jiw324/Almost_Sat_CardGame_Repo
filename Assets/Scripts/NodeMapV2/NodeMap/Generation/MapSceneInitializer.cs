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

    private void Start()
    {
        if (MapStateManager.Instance != null)
            MapStateManager.Instance.RefreshAllNodeGlows();

        var fog = FindFirstObjectByType<FogController>();
        if (fog != null)
            fog.UpdateFogExternally();
    }
}
