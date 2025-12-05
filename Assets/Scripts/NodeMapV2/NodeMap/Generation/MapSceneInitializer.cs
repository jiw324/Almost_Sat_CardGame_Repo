using UnityEngine;

public class MapSceneInitializer : MonoBehaviour
{
    private void Awake()
    {
        var session = GameSession.Instance;

        if (MapGenerationManager.Instance.ActiveMap == null)
        {
            if (session != null && session.IsTutorialMode)
            {
                MapGenerationManager.Instance.InitializeTutorialFromSession();
            }
            else
            {
                MapGenerationManager.Instance.InitializeMapFromSession();
            }
        }
    }

    private void Start()
    {
        if (MapStateManager.Instance != null)
            MapStateManager.Instance.RefreshAllNodeGlows();

        var fog = FindFirstObjectByType<FogController>();
        if (fog != null)
            fog.UpdateFogExternally();

        var state = MapStateManager.Instance;
        var map = MapGenerationManager.Instance.ActiveMap;

        if (state != null && map != null)
        {
            Node current = state.GetCurrentNode();
            if (current != null && current == map.BossNode && state.IsNodeCompleted(current))
            {
                Debug.Log("Boss completed & map initialized — showing victory screen...");
                VictoryScreenManager.ShowVictory();
            }
        }
    }
}
