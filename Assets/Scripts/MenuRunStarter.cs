using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuRunStarter : MonoBehaviour
{
    public void StartRun()
    {
        SceneLoader.Instance.Go(GameRoute.Map);
    }
}
