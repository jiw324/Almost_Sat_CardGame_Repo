using UnityEngine;

public class Navigator : MonoBehaviour
{
    public void Navigate(GameRoute gameRoute)
    {
        SceneLoader.Instance.Go(gameRoute);
    }
}
