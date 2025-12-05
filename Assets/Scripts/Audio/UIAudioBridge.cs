using UnityEngine;

public class UIAudioBridge : MonoBehaviour
{
    private string clickSoundId = "UIButton";
    private string clickErrorSoundId = "UIButtonError";
    private string cardsSoundId = "Cards";

    /// <summary>
    /// Plays the default click sound.
    /// Use this for simple UI buttons.
    /// </summary>
    public void PlayClick()
    {
        if (!string.IsNullOrEmpty(clickSoundId))
            SoundEvents.Play(clickSoundId);
    }

    public void PlayClickError()
    {
        if (!string.IsNullOrEmpty(clickErrorSoundId))
            SoundEvents.Play(clickErrorSoundId);
    }

    public void PlayCards()
    {
        if (!string.IsNullOrEmpty(cardsSoundId))
            SoundEvents.Play(cardsSoundId);
    }

    /// <summary>
    /// Plays any UI sound by ID.
    /// Assign this in buttons where you want custom sounds.
    /// </summary>
    public void PlaySoundById(string soundId)
    {
        if (!string.IsNullOrEmpty(soundId))
            SoundEvents.Play(soundId);
    }
}
