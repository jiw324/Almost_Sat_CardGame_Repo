using UnityEngine;

public class UIManager : MonoBehaviour
{
    public AvatarUI playerUI;
    public AvatarUI enemyUI;
    public BoardManager boardManager;

    public void InitializeUI(int playerHealth, int playerMana, int enemyHealth, int enemyMana)
    {
        playerUI.SetHealth(playerHealth);
        playerUI.SetMana(playerMana);
        enemyUI.SetHealth(enemyHealth);
        enemyUI.SetMana(enemyMana);
    }

    public void UpdatePlayerHealth(int newHealth)
    {
        playerUI.SetHealth(newHealth);
    }

    public void UpdatePlayerMana(int newMana)
    {
        playerUI.SetMana(newMana);
    }

    public void UpdateEnemyHealth(int newHealth)
    {
        enemyUI.SetHealth(newHealth);
    }

    public void UpdateEnemyMana(int newMana)
    {
        enemyUI.SetMana(newMana);
    }
}
