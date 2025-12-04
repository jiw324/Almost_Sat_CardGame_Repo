using System;

public static class SoundEvents
{
    // Generic sound request
    public static event Action<string> OnSoundRequested;

    /// <summary>
    /// Request a sound to play by ID. 
    /// Example: SoundEvents.Play("NodeClick");
    /// </summary>
    public static void Play(string soundId)
    {
        OnSoundRequested?.Invoke(soundId);
    }

    // Generic music request
    public static event Action<string> OnMusicRequested;

    public static void PlayMusic(string musicId)
    {
        OnMusicRequested?.Invoke(musicId);
    }
}
