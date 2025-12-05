using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    private void Start()
    {
        if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            SoundEvents.PlayMusic("Menu");
        }
    }
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
            case "Loot":
                gameRoute = GameRoute.Loot;
                break;
            case "DeckBuilder":
                gameRoute = GameRoute.DeckBuilder;
                break;
            default:
                gameRoute = GameRoute.MainMenu;
                break;
        }

        if (SceneLoader.Instance == null)
        {
            Debug.LogError("SceneLoader.Instance is null");
            return;
        }

        SceneLoader.Instance.Go(gameRoute);
        PlayMusicForScene(gameRoute);
    }

    private void PlayMusicForScene(GameRoute gameRoute)
    {
        switch (gameRoute)
        {
            case GameRoute.Map:
                SoundEvents.PlayMusic("Map");
                break;

            case GameRoute.Combat:
                SoundEvents.PlayMusic("Combat");
                break;

            case GameRoute.Shop:
                SoundEvents.PlayMusic("Shop");
                break;

            case GameRoute.Loot:
                SoundEvents.PlayMusic("Map");
                break;

            case GameRoute.Rest:
                SoundEvents.PlayMusic("Rest");
                break;

            case GameRoute.Event:
                SoundEvents.PlayMusic("Menu");
                break;

            case GameRoute.MainMenu:
                SoundEvents.PlayMusic("Menu");
                break;

            default:
                break;
        }
    }
}
