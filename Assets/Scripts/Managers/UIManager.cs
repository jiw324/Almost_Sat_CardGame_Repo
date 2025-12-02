using UnityEngine;

public class UIManager : MonoBehaviour
{
    public AvatarUI playerUI;
    public AvatarUI enemyUI;

    public void InitializeUI(int playerHealth, int playerMana, int enemyHealth, int enemyMana)
    {
        if (playerUI != null)
        {
            playerUI.SetHealth(playerHealth);
            playerUI.SetMana(playerMana);
        }
        else
        {
            Debug.LogWarning("[UIManager] PlayerUI is not assigned.");
        }

        if (enemyUI != null)
        {
            enemyUI.SetHealth(enemyHealth);
            enemyUI.SetMana(enemyMana);
        }
        else
        {
            Debug.LogWarning("[UIManager] EnemyUI is not assigned.");
        }
    }

    public void UpdatePlayerHealth(int newHealth)
    {
        if (playerUI != null)
            playerUI.SetHealth(newHealth);
    }

    public void UpdatePlayerMana(int newMana)
    {
        if (playerUI != null)
            playerUI.SetMana(newMana);
    }

    public void UpdateEnemyHealth(int newHealth)
    {
        if (enemyUI != null)
            enemyUI.SetHealth(newHealth);
    }

    public void UpdateEnemyMana(int newMana)
    {
        if (enemyUI != null)
            enemyUI.SetMana(newMana);
    }
    
}
