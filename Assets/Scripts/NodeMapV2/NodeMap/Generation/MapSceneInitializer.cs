using UnityEngine;

public class MapSceneInitializer : MonoBehaviour
{
    private void Awake()
    {
        var mgm = MapGenerationManager.Instance;

        if (mgm.ActiveMap == null)
        {
            if (GameSession.Instance.IsTutorialMode)
                mgm.InitializeTutorialFromSession();
            else
                mgm.InitializeMapFromSession();
        }
    }

    private void Start()
    {
        if (MapStateManager.Instance != null)
            MapStateManager.Instance.RefreshAllNodeGlows();
    }
}
