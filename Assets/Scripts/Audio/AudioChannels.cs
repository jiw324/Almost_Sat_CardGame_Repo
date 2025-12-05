using UnityEngine;

[System.Serializable]
public class AudioChannels
{
    [Header("Primary Channels")]
    public AudioSource ui;        // UI sounds
    public AudioSource map;       // Map node sounds
    public AudioSource combat;    // Combat SFX
    public AudioSource music;     // Background music

    public void Initialize(GameObject parent)
    {
        ui = CreateSource(parent, "UI_Channel");
        map = CreateSource(parent, "Map_Channel");
        combat = CreateSource(parent, "Combat_Channel");
        music = CreateSource(parent, "Music_Channel", loop: true);
    }

    private AudioSource CreateSource(GameObject parent, string name, bool loop = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);

        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = loop;
        src.spatialBlend = 0f;

        return src;
    }
}
