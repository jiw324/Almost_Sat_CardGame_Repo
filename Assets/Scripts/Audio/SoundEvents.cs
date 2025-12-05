using System;

public static class SoundEvents
{
    public static event Action<string> OnSoundRequested;

    public static void Play(string soundId)
    {
        OnSoundRequested?.Invoke(soundId);
    }

    public static event Action<string> OnMusicRequested;

    public static void PlayMusic(string musicId)
    {
        OnMusicRequested?.Invoke(musicId);
    }
}
