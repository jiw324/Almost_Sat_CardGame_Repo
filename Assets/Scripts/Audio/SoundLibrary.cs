using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [Serializable]
    public class SoundEntry
    {
        public string id;
        public AudioClip clip;
        public SoundChannel channel;
    }

    public List<SoundEntry> sounds = new();
    public List<MusicEntry> music = new();

    [Serializable]
    public class MusicEntry
    {
        public string id;
        public AudioClip clip;
    }
}

public enum SoundChannel
{
    UI,
    Map,
    Combat,
    Ambient
}
