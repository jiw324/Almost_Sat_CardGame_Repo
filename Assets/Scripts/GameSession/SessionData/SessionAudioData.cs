using UnityEngine;

[System.Serializable]
public class SessionAudioData
{
    [Range(0f, 1f)] public float uiVolume = 1f;
    [Range(0f, 1f)] public float mapVolume = 1f;
    [Range(0f, 1f)] public float combatVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
}
