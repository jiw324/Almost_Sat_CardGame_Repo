using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoldDisplay : MonoBehaviour
{
    public TMP_Text goldText;

    public void UpdateGold(int newGold)
    {
        goldText.text = newGold.ToString();
    }
}
