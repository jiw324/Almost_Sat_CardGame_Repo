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
    [Range(0f, 1f)] public float musicVolume = 1f;

    private Dictionary<string, SoundLibrary.SoundEntry> sfxDict;
    private Dictionary<string, SoundLibrary.MusicEntry> musicDict;
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

        channels.Initialize(gameObject);
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
        musicDict = new Dictionary<string, SoundLibrary.MusicEntry>();

        foreach (var s in library.sounds)
            if (!sfxDict.ContainsKey(s.id))
                sfxDict.Add(s.id, s);

        foreach (var m in library.music)
            if (!musicDict.ContainsKey(m.id))
                musicDict.Add(m.id, m);
    }

    public void SetUIVolume(float v) { uiVolume = v; ApplyVolumes(); }
    public void SetMapVolume(float v) { mapVolume = v; ApplyVolumes(); }
    public void SetCombatVolume(float v) { combatVolume = v; ApplyVolumes(); }
    public void SetMusicVolume(float v) { musicVolume = v; ApplyVolumes(); }

    public void ApplySettings(SessionAudioData data)
    {
        if (data == null) return;

        uiVolume = data.uiVolume;
        mapVolume = data.mapVolume;
        combatVolume = data.combatVolume;
        musicVolume = data.musicVolume;

        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        channels.ui.volume = uiVolume;
        channels.map.volume = mapVolume;
        channels.combat.volume = combatVolume;
        channels.music.volume = musicVolume;
    }

    public SessionAudioData CaptureSettings()
    {
        return new SessionAudioData
        {
            uiVolume = uiVolume,
            mapVolume = mapVolume,
            combatVolume = combatVolume,
            musicVolume = musicVolume
        };
    }

    public void PlaySoundById(string id)
    {
        if (!sfxDict.TryGetValue(id, out var entry)) return;
        GetChannelSource(entry.channel)?.PlayOneShot(entry.clip);
    }

    private AudioSource GetChannelSource(SoundChannel ch)
    {
        return ch switch
        {
            SoundChannel.UI => channels.ui,
            SoundChannel.Map => channels.map,
            SoundChannel.Combat => channels.combat,
            _ => channels.ui
        };
    }

    public void PlayMusicById(string id)
    {
        if (!musicDict.TryGetValue(id, out var entry)) return;
        PlayMusic(entry.clip);
    }

    public void PlayMusic(AudioClip clip, float fade = 1f)
    {
        if (clip == null) return;

        if (musicFadeRoutine != null)
            StopCoroutine(musicFadeRoutine);

        musicFadeRoutine = StartCoroutine(FadeToNewMusic(clip, fade));
    }

    private IEnumerator FadeToNewMusic(AudioClip newClip, float fade)
    {
        var music = channels.music;
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
            music.volume = Mathf.Lerp(0, musicVolume, t / fade);
            yield return null;
        }

        music.volume = musicVolume;
    }
}
