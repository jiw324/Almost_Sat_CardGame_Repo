using UnityEngine;

[CreateAssetMenu(fileName = "NewRelic", menuName = "Relics/Relic")]
public class RelicData : ScriptableObject
{
    public string relicName;
    [TextArea] public string description;
    public Sprite artwork;
    public int effectValue;
}
