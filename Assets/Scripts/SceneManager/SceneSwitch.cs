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
            case "CardBoard":
                gameRoute = GameRoute.Combat;
                break;
            default:
                gameRoute = GameRoute.MainMenu;
                break;
        }

        SceneLoader.Instance.Go(gameRoute);
    }
}
