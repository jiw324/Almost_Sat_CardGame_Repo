using UnityEngine;

public class UIManager : MonoBehaviour
{
    public AvatarUI playerAvatar;
    public AvatarUI enemyAvatar;

    public AvatarUI GetPlayerAvatar() => playerAvatar;
    public AvatarUI GetEnemyAvatar() => enemyAvatar;

    public void InitializeUI(int playerHealth, int playerMana, int enemyHealth, int enemyMana)
    {
        if (playerAvatar != null)
        {
            playerAvatar.SetHealth(playerHealth);
            playerAvatar.SetMana(playerMana);
        }
        else
        {
            Debug.LogWarning("[UIManager] PlayerUI is not assigned.");
        }

        if (enemyAvatar != null)
        {
            enemyAvatar.SetHealth(enemyHealth);
            enemyAvatar.SetMana(enemyMana);
        }
        else
        {
            Debug.LogWarning("[UIManager] EnemyUI is not assigned.");
        }
    }

    public void UpdatePlayerHealth(int newHealth)
    {
        if (playerAvatar != null)
            playerAvatar.SetHealth(newHealth);
    }

    public void UpdatePlayerMana(int newMana)
    {
        if (playerAvatar != null)
            playerAvatar.SetMana(newMana);
    }

    public void UpdateEnemyHealth(int newHealth)
    {
        if (enemyAvatar != null)
            enemyAvatar.SetHealth(newHealth);
    }

    public void UpdateEnemyMana(int newMana)
    {
        if (enemyAvatar != null)
            enemyAvatar.SetMana(newMana);
    }
    
}
