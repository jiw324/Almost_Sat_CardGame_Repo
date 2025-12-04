using UnityEngine;
using UnityEditor;

public class MenuNavigationManager : MonoBehaviour
{
    [SerializeField] public SceneSwitch sceneSwitch;

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }

    public void PlayGame()
    {
        GameSession.Instance.IsTutorialMode = false;
        sceneSwitch.SceneChanger("Map");
    }

    public void PlayTutorial()
    {
        GameSession.Instance.IsTutorialMode = true;
        sceneSwitch.SceneChanger("Map");
    }
}
