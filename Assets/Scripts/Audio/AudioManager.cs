using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Library")]
    public SoundLibrary library;

    [Header("Audio Channels")]
    public AudioChannels channels = new AudioChannels();

    [Header("Channel Volumes")]
    [Range(0f, 1f)] public float uiVolume = 1f;
    [Range(0f, 1f)] public float mapVolume = 1f;
    [Range(0f, 1f)] public float combatVolume = 1f;


    private Dictionary<string, SoundLibrary.SoundEntry> sfxDict;
    private Dictionary<string, AudioClip> musicDict;

    private Coroutine musicFadeRoutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        channels.Initialize(this.gameObject);
        BuildDictionaries();
    }

    private void OnEnable()
    {
        SoundEvents.OnSoundRequested += PlaySoundById;
        SoundEvents.OnMusicRequested += PlayMusicById;
    }

    private void OnDisable()
    {
        SoundEvents.OnSoundRequested -= PlaySoundById;
        SoundEvents.OnMusicRequested -= PlayMusicById;
    }

    private void BuildDictionaries()
    {
        sfxDict = new Dictionary<string, SoundLibrary.SoundEntry>();
        musicDict = new Dictionary<string, AudioClip>();

        foreach (var entry in library.sounds)
        {
            if (!sfxDict.ContainsKey(entry.id))
                sfxDict.Add(entry.id, entry);
        }

        foreach (var entry in library.music)
        {
            if (!musicDict.ContainsKey(entry.id))
                musicDict.Add(entry.id, entry.clip);
        }
    }

    public void PlaySoundById(string id)
    {
        if (!sfxDict.TryGetValue(id, out var entry))
        {
            Debug.LogWarning("AudioManager: Sound '" + id + "' not found.");
            return;
        }

        AudioSource src = GetChannelSource(entry.channel);

        if (src != null)
            src.PlayOneShot(entry.clip);
    }


    private AudioSource GetChannelSource(SoundChannel ch)
    {
        switch (ch)
        {
            case SoundChannel.UI:
                channels.ui.volume = uiVolume;
                return channels.ui;

            case SoundChannel.Map:
                channels.map.volume = mapVolume;
                return channels.map;

            case SoundChannel.Combat:
                channels.combat.volume = combatVolume;
                return channels.combat;

            default:
                return channels.ui;
        }
    }

    public void PlayMusicById(string id)
    {
        if (!musicDict.TryGetValue(id, out var clip))
        {
            Debug.LogWarning($"AudioManager: Music '{id}' not found.");
            return;
        }

        PlayMusic(clip);
    }

    public void PlayMusic(AudioClip clip, float fade = 1f)
    {
        if (clip == null) return;

        var music = channels.music;

        if (music.clip == clip && music.isPlaying)
            return;

        if (musicFadeRoutine != null)
            StopCoroutine(musicFadeRoutine);

        musicFadeRoutine = StartCoroutine(FadeToNewMusic(clip, fade));
    }

    private IEnumerator FadeToNewMusic(AudioClip newClip, float fade)
    {
        AudioSource music = channels.music;
        float startVol = music.volume;

        for (float t = 0; t < fade; t += Time.deltaTime)
        {
            music.volume = Mathf.Lerp(startVol, 0, t / fade);
            yield return null;
        }

        music.clip = newClip;
        music.Play();

        for (float t = 0; t < fade; t += Time.deltaTime)
        {
            music.volume = Mathf.Lerp(0, startVol, t / fade);
            yield return null;
        }

        music.volume = startVol;
    }
}
