using Godot;

namespace StarGen.App.Audio;

/// <summary>
/// Central audio library resource for app-wide music and UI cues.
/// </summary>
[GlobalClass]
public partial class AppAudioLibrary : Resource
{
    /// <summary>
    /// Intro music played during the startup splash.
    /// </summary>
    [Export]
    public AudioStream? IntroMusicStream { get; set; }

    /// <summary>
    /// Playback volume for the startup intro music.
    /// </summary>
    [Export]
    public float IntroMusicVolumeDb { get; set; } = -6.0f;

    /// <summary>
    /// Optional main menu background music.
    /// </summary>
    [Export]
    public AudioStream? MainMenuMusicStream { get; set; }

    /// <summary>
    /// Playback volume for the main menu music.
    /// </summary>
    [Export]
    public float MainMenuMusicVolumeDb { get; set; } = -12.0f;

    /// <summary>
    /// Optional positive UI confirmation cue.
    /// </summary>
    [Export]
    public AudioStream? UiConfirmStream { get; set; }

    /// <summary>
    /// Playback volume for the confirmation cue.
    /// </summary>
    [Export]
    public float UiConfirmVolumeDb { get; set; } = -10.0f;

    /// <summary>
    /// Optional UI cancellation cue.
    /// </summary>
    [Export]
    public AudioStream? UiCancelStream { get; set; }

    /// <summary>
    /// Playback volume for the cancellation cue.
    /// </summary>
    [Export]
    public float UiCancelVolumeDb { get; set; } = -10.0f;

    /// <summary>
    /// Returns the configured stream for the supplied cue.
    /// </summary>
    public AudioStream? GetStream(AppAudioCueId cueId)
    {
        if (cueId == AppAudioCueId.IntroMusic)
        {
            return IntroMusicStream;
        }

        if (cueId == AppAudioCueId.MainMenuMusic)
        {
            return MainMenuMusicStream;
        }

        if (cueId == AppAudioCueId.UiConfirm)
        {
            return UiConfirmStream;
        }

        if (cueId == AppAudioCueId.UiCancel)
        {
            return UiCancelStream;
        }

        return null;
    }

    /// <summary>
    /// Returns the configured volume for the supplied cue.
    /// </summary>
    public float GetVolumeDb(AppAudioCueId cueId)
    {
        if (cueId == AppAudioCueId.IntroMusic)
        {
            return IntroMusicVolumeDb;
        }

        if (cueId == AppAudioCueId.MainMenuMusic)
        {
            return MainMenuMusicVolumeDb;
        }

        if (cueId == AppAudioCueId.UiConfirm)
        {
            return UiConfirmVolumeDb;
        }

        if (cueId == AppAudioCueId.UiCancel)
        {
            return UiCancelVolumeDb;
        }

        return 0.0f;
    }
}
