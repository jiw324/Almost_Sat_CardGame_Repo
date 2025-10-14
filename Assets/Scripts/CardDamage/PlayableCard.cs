
using UnityEngine;

[RequireComponent(typeof(CardUI))]
public class PlayableCard : MonoBehaviour
{
    private CardUI ui;
    private CardData data;

    public Actor demoTarget;          
    public Team ownerTeam = Team.Player;

    private void Awake() { ui = GetComponent<CardUI>(); }
    private void Start() { data = ui.GetCardData(); }

    public void Play()
    {
        if (data == null || data.effects == null) return;

        foreach (var effect in data.effects)
        {
            if (effect == null) continue;

            // determine candidate targets from effect¡¯s targeting config
            Actor[] candidates = null;
            switch (effect.targetGroup)
            {
                case TargetGroup.Allies:
                    candidates = TargetingSystem.GetAlliesOf(ownerTeam);
                    break;
                case TargetGroup.Enemies:
                    candidates = TargetingSystem.GetEnemiesOf(ownerTeam);
                    break;
            }

            if (effect.isAOE)
            {
                // AOE: apply to all candidates (e.g., all enemies for AOE attack,
                // or all allies for AOE shield)
                effect.ExecuteMany(candidates);
            }
            else
            {
                // Single target: use explicit override if provided; else pick first from list
                Actor single =
                    demoTarget ??
                    (effect.targetGroup == TargetGroup.Allies
                        ? TargetingSystem.GetFirstAlly(ownerTeam)
                        : TargetingSystem.GetFirstEnemy(ownerTeam));

                if (single != null) effect.Execute(single);
            }
        }

        // After playing, remove the card from hand
        Destroy(gameObject);
    }
}
