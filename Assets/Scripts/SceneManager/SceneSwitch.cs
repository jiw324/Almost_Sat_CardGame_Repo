using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    public void SceneChanger(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
