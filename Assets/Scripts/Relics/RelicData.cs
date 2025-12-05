using UnityEngine;

/// <summary>
/// Static data definition for a relic.
/// This is the authoring-time object you create as an asset in the editor.
/// Runtime usage will typically wrap this in a RelicInstance.
/// </summary>
[CreateAssetMenu(fileName = "NewRelic", menuName = "Relics/Relic")]
public class RelicData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable ID for this relic (should match DB/JSON id if used).")]
    public string relicId;

    [Tooltip("Display name for the relic.")]
    public string relicName;

    [TextArea]
    [Tooltip("Description shown in UI.")]
    public string description;

    [Header("Gameplay")]
    [Tooltip("Rarity label, e.g. Common, Rare, Epic, Legendary.")]
    public string rarity;

    [Tooltip("Logical effect type tag, e.g. MaxHealthBuff, ExtraCardDraw.")]
    public string effectType;

    [Tooltip("Optional numeric effect value (e.g. +5 Max HP) used for UI display.")]
    public int effectValue;

    [Tooltip("Optional faction identifier this relic belongs to.")]
    public string factionId;

    [Header("Meta")]
    [Tooltip("Condition text or key describing how this relic is unlocked.")]
    public string unlockCondition;

    [Header("Presentation")]
    [Tooltip("Icon sprite used when displaying this relic in UI.")]
    public Sprite icon;

    /// <summary>
    /// Helper for debugging/logging.
    /// </summary>
    public override string ToString()
    {
        return $"Relic[id={relicId}, name={relicName}, rarity={rarity}, effectType={effectType}]";
    }
}


