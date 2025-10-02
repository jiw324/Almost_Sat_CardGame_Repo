using UnityEngine;

[RequireComponent(typeof(CardUI))]
public class PlayableCard : MonoBehaviour
{
    private CardUI ui;
    private CardData data;

    public Actor demoTarget; // Assign in inspector (enemy placeholder)

    private void Awake()
    {
        ui = GetComponent<CardUI>();
    }

    private void Start()
    {
        data = ui.GetCardData();
    }

    public void Play()
    {
        if (data == null || data.effects == null) return;

        foreach (var effect in data.effects)
        {
            effect.Execute(demoTarget);
        }

        // After playing, remove the card from hand
        Destroy(gameObject);
    }
}
