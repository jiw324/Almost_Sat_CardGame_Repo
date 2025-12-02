using UnityEngine;
using UnityEditor;

public class QuitGameScript : MonoBehaviour
{
    public void QuitGame()
    {
        // This will only work in a built application
        Application.Quit();

        // This will exit Play Mode in the Unity Editor
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }
}