using UnityEngine;

public class SceneSwitch : MonoBehaviour
{
    public void SceneChanger(string route)
    {
        GameRoute gameRoute;
        switch(route)
        {
            case "MainMenu":
                gameRoute = GameRoute.MainMenu;
                break;
            case "CardBoard":
                gameRoute = GameRoute.Combat;
                break;
            case "Map":
                gameRoute = GameRoute.Map;
                break;
            default:
                gameRoute = GameRoute.MainMenu;
                break;
        }

        SceneLoader.Instance.Go(gameRoute);
    }
}
