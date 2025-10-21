using UnityEngine;

[RequireComponent(typeof(CardUI))]
public class PlayableCard : MonoBehaviour
{
    private CardUI ui;
    private CardData data;

    public EntityBase demoTarget; // Assign in inspector (enemy placeholder)

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
        if (data == null || data.effect == null) return;

        data.effect.Execute(null, demoTarget); // null as owner for demo

        // After playing, remove the card from hand
        Destroy(gameObject);
    }
}
