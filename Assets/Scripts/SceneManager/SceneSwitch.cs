using UnityEngine;

public class SceneSwitch : MonoBehaviour
{
    public void SceneChanger(string route)
    {
        GameRoute gameRoute = GameRoute.MainMenu;
        switch(route)
        {
            case "MainMenu":
                gameRoute = GameRoute.MainMenu;
                break;
            case "Combat":
                gameRoute = GameRoute.Combat;
                break;
            case "Map":
                gameRoute = GameRoute.Map;
                break;
            case "Shop":
                gameRoute = GameRoute.Shop;
                break;
            case "Rest":
                gameRoute = GameRoute.Rest;
                break;
            default:
                gameRoute = GameRoute.MainMenu;
                break;
        }

        if (SceneLoader.Instance == null)
        {
            Debug.LogError("[SceneSwitch] SceneLoader.Instance is null! Make sure SceneLoader exists in the scene and has been initialized.");
            return;
        }

        SceneLoader.Instance.Go(gameRoute);
    }
}
