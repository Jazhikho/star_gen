using Godot;

namespace StarGen.App;

/// <summary>
/// Lightweight startup splash that holds for a short duration and can be skipped by input.
/// </summary>
public partial class SplashScreen : Control
{
    [Signal]
    public delegate void splash_finishedEventHandler();

    private Timer? _revealTimer;
    private Label? _versionLabel;
    private Label? _statusLabel;
    private bool _finished;
    private double _elapsed;

    /// <summary>
    /// Initializes the splash sequence.
    /// </summary>
    public override void _Ready()
    {
        _revealTimer = GetNodeOrNull<Timer>("RevealTimer");
        _versionLabel = GetNodeOrNull<Label>("MarginContainer/Layout/WordmarkBlock/VersionLabel");
        _statusLabel = GetNodeOrNull<Label>("MarginContainer/Layout/WordmarkBlock/StatusLabel");

        string version = ProjectSettings.GetSetting("application/config/version", "0.3.0").AsString();
        if (_versionLabel != null)
        {
            _versionLabel.Text = $"Release {version}";
        }

        if (_statusLabel != null)
        {
            _statusLabel.Text = "Initializing generator modules...";
        }

        if (_revealTimer != null)
        {
            _revealTimer.Timeout += Finish;
            _revealTimer.Start();
        }
    }

    /// <summary>
    /// Provides a subtle pulse to keep the splash from feeling static.
    /// </summary>
    public override void _Process(double delta)
    {
        _elapsed += delta;
        Label? brandGlyph = GetNodeOrNull<Label>("MarginContainer/Layout/WordmarkBlock/GlyphLabel");
        if (brandGlyph != null)
        {
            float alpha = 0.75f + (0.25f * Mathf.Sin((float)_elapsed * 2.2f));
            brandGlyph.Modulate = new Color(0.74f, 0.88f, 1.0f, alpha);
        }
    }

    /// <summary>
    /// Allows skipping the splash with any key or click.
    /// </summary>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (_finished)
        {
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            Finish();
            GetViewport()?.SetInputAsHandled();
            return;
        }

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            Finish();
            GetViewport()?.SetInputAsHandled();
        }
    }

    private void Finish()
    {
        if (_finished)
        {
            return;
        }

        _finished = true;
        EmitSignal(SignalName.splash_finished);
    }
}
