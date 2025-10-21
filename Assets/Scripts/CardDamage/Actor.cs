using UnityEngine;

public class Actor : MonoBehaviour
{
    public string actorName = "Unit";
    public int maxHP = 50;
    public int currentHP = 50;
    public int shield = 0;

    public void GainShield(int amount)
    {
        shield += Mathf.Max(0, amount);
        Debug.Log($"{actorName} gains {amount} shield (now {shield}).");
    }

    public void TakeDamage(int amount)
    {
        int remaining = amount;

        int absorbed = Mathf.Min(shield, remaining);
        shield -= absorbed;
        remaining -= absorbed;

        if (remaining > 0)
        {
            currentHP = Mathf.Max(0, currentHP - remaining);
            Debug.Log($"{actorName} takes {remaining} damage (HP {currentHP}/{maxHP}).");
        }
        else
        {
            Debug.Log($"{actorName}'s shield absorbed all damage (shield {shield}).");
        }
    }
}
