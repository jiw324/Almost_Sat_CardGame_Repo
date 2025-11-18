using UnityEngine;
using UnityEngine.UI;

public class DeckButton : MonoBehaviour
{
    [SerializeField] private DeckDisplayMenu deckMenu;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (deckMenu.gameObject.activeSelf)
            deckMenu.Hide();
        else
            deckMenu.Show();
    }
}
