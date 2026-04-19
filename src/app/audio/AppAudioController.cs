using Godot;

namespace StarGen.App.Audio;

/// <summary>
/// Shared app-level audio controller that owns reusable music and UI players.
/// </summary>
public partial class AppAudioController : Node
{
    /// <summary>
    /// Shared audio library resource that declares all app-owned cues.
    /// </summary>
    [Export]
    public AppAudioLibrary? Library { get; set; }

    private AudioStreamPlayer? _musicPlayer;
    private AudioStreamPlayer? _uiPlayer;
    private float _activeMusicVolumeDb;

    /// <summary>
    /// Caches child player nodes.
    /// </summary>
    public override void _Ready()
    {
        _musicPlayer = GetNodeOrNull<AudioStreamPlayer>("MusicPlayer");
        _uiPlayer = GetNodeOrNull<AudioStreamPlayer>("UiPlayer");
    }

    /// <summary>
    /// Returns whether the requested cue is configured in the shared library.
    /// </summary>
    public bool HasCue(AppAudioCueId cueId)
    {
        if (Library == null)
        {
            return false;
        }

        return Library.GetStream(cueId) != null;
    }

    /// <summary>
    /// Starts music playback for the supplied cue when configured.
    /// </summary>
    public bool PlayMusic(AppAudioCueId cueId)
    {
        if (_musicPlayer == null)
        {
            GD.PushError("AppAudioController: MusicPlayer node is missing.");
            return false;
        }

        if (Library == null)
        {
            GD.PushError("AppAudioController: Library resource is missing.");
            return false;
        }

        AudioStream? stream = Library.GetStream(cueId);
        if (stream == null)
        {
            return false;
        }

        _activeMusicVolumeDb = Library.GetVolumeDb(cueId);
        _musicPlayer.Stop();
        _musicPlayer.Stream = stream;
        _musicPlayer.VolumeDb = _activeMusicVolumeDb;
        _musicPlayer.Play();
        return true;
    }

    /// <summary>
    /// Plays a one-shot UI cue when configured.
    /// </summary>
    public bool PlayUiCue(AppAudioCueId cueId)
    {
        if (_uiPlayer == null)
        {
            GD.PushError("AppAudioController: UiPlayer node is missing.");
            return false;
        }

        if (Library == null)
        {
            GD.PushError("AppAudioController: Library resource is missing.");
            return false;
        }

        AudioStream? stream = Library.GetStream(cueId);
        if (stream == null)
        {
            return false;
        }

        _uiPlayer.Stop();
        _uiPlayer.Stream = stream;
        _uiPlayer.VolumeDb = Library.GetVolumeDb(cueId);
        _uiPlayer.Play();
        return true;
    }

    /// <summary>
    /// Fades the active music volume toward the requested level.
    /// </summary>
    public void FadeMusicTo(Node tweenOwner, float targetVolumeDb, double durationSeconds)
    {
        if (_musicPlayer == null)
        {
            return;
        }

        if (!_musicPlayer.Playing)
        {
            return;
        }

        Tween tween = tweenOwner.CreateTween();
        tween.TweenProperty(_musicPlayer, "volume_db", targetVolumeDb, durationSeconds);
    }

    /// <summary>
    /// Stops the active music cue and restores its configured playback volume.
    /// </summary>
    public void StopMusic()
    {
        if (_musicPlayer == null)
        {
            return;
        }

        if (_musicPlayer.Playing)
        {
            _musicPlayer.Stop();
        }

        _musicPlayer.VolumeDb = _activeMusicVolumeDb;
    }
}
