using Godot;
using StarGen.App.Audio;
using StarGen.App.Shared;
using System.Collections.Generic;

namespace StarGen.App;

/// <summary>
/// Startup splash that plays the intro video, then cross-fades into the StarGen logo.
/// </summary>
public partial class SplashScreen : Control
{
	[Signal]
	public delegate void splash_finishedEventHandler();

	/// <summary>The resource path for the StarGen logo shown after the intro video.</summary>
	[Export]
	private string LogoTextureResourcePath { get; set; } = "res://StarGen.png";

	/// <summary>The duration of the video-to-logo cross-fade.</summary>
	[Export]
	private float TransitionDurationSeconds { get; set; } = 0.9f;

	/// <summary>The hold time for the logo after the transition completes.</summary>
	[Export]
	private float LogoHoldSeconds { get; set; } = 0.65f;

	private VideoStreamPlayer? _videoPlayer;
	private Control? _videoLayer;
	private CenterContainer? _logoLayer;
	private TextureRect? _logoTexture;
	private Label? _versionLabel;
	private Label? _statusLabel;
	private Label? _skipLabel;
	private AppAudioController? _audioController;
	private bool _finished;
	private bool _transitionStarted;

	/// <summary>
	/// Initializes the splash sequence.
	/// </summary>
	public override void _Ready()
	{
		_videoLayer = GetNodeOrNull<Control>("CenterStage/StageVBox/MediaFrame/VideoLayer");
		_videoPlayer = GetNodeOrNull<VideoStreamPlayer>("CenterStage/StageVBox/MediaFrame/VideoLayer/VideoPlayer");
		_logoLayer = GetNodeOrNull<CenterContainer>("CenterStage/StageVBox/MediaFrame/LogoLayer");
		_logoTexture = GetNodeOrNull<TextureRect>("CenterStage/StageVBox/MediaFrame/LogoLayer/LogoTexture");
		_versionLabel = GetNodeOrNull<Label>("CenterStage/StageVBox/LogoVBox/VersionLabel");
		_statusLabel = GetNodeOrNull<Label>("CenterStage/StageVBox/LogoVBox/StatusLabel");
		_skipLabel = GetNodeOrNull<Label>("SkipLabel");
		_audioController = GetNodeOrNull<AppAudioController>("../AudioController");

		if (_videoPlayer != null)
		{
			_videoPlayer.Connect("finished", Callable.From(OnVideoFinished));
		}

		string version = UserFacingVersionHelper.GetDisplayVersion();
		if (_versionLabel != null)
		{
			_versionLabel.Text = $"Release {version}";
		}

		if (_statusLabel != null)
		{
			_statusLabel.Text = "Receiving star charts...";
		}

		if (_skipLabel != null)
		{
			_skipLabel.Text = "Press any key or click to skip";
		}

		if (_logoLayer != null)
		{
			_logoLayer.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		}

		LoadLogoTexture();
		LoadIntroVideo();

		if (IsInsideTree())
		{
			StartPlayback();
		}
	}

	/// <summary>
	/// Allows skipping the intro with any key or click.
	/// </summary>
	public override void _UnhandledInput(InputEvent @event)
	{
		if (_finished || _transitionStarted)
		{
			return;
		}

		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			BeginLogoTransition();
			GetViewport()?.SetInputAsHandled();
			return;
		}

		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			BeginLogoTransition();
			GetViewport()?.SetInputAsHandled();
		}
	}

	private void LoadIntroVideo()
	{
		if (_videoPlayer == null)
		{
			GD.PushError("SplashScreen: VideoPlayer node is missing.");
			return;
		}

		if (_videoPlayer.Stream != null)
		{
			return;
		}

		string resolvedVideoPath = ResolveIntroVideoResourcePath();
		if (string.IsNullOrWhiteSpace(resolvedVideoPath))
		{
			return;
		}

		VideoStream? introVideo = ResourceLoader.Load<VideoStream>(resolvedVideoPath);
		if (introVideo == null)
		{
			GD.PushError($"SplashScreen: failed to load intro video '{resolvedVideoPath}'.");
			return;
		}

		_videoPlayer.Stream = introVideo;
	}

	private void LoadLogoTexture()
	{
		if (_logoTexture == null)
		{
			GD.PushError("SplashScreen: LogoTexture node is missing.");
			return;
		}

		if (string.IsNullOrWhiteSpace(LogoTextureResourcePath))
		{
			GD.PushError("SplashScreen: logo texture path is empty.");
			return;
		}

		Texture2D? logoTexture = ResourceLoader.Load<Texture2D>(LogoTextureResourcePath);
		if (logoTexture == null)
		{
			GD.PushError($"SplashScreen: failed to load logo texture '{LogoTextureResourcePath}'.");
			return;
		}

		_logoTexture.Texture = logoTexture;
	}

	private string ResolveIntroVideoResourcePath()
	{
		DirAccess? rootDir = DirAccess.Open("res://");
		if (rootDir == null)
		{
			GD.PushError("SplashScreen: failed to open root directory while searching for intro video.");
			return string.Empty;
		}

		List<string> ogvFiles = new List<string>();
		rootDir.ListDirBegin();
		while (true)
		{
			string fileName = rootDir.GetNext();
			if (string.IsNullOrEmpty(fileName))
			{
				break;
			}

			if (rootDir.CurrentIsDir())
			{
				continue;
			}

			if (fileName.EndsWith(".ogv"))
			{
				ogvFiles.Add(fileName);
			}
		}
		rootDir.ListDirEnd();

		if (ogvFiles.Count == 0)
		{
			GD.PushError("SplashScreen: no root .ogv file was found for intro video.");
			return string.Empty;
		}

		if (ogvFiles.Count > 1)
		{
			GD.PushError("SplashScreen: multiple root .ogv files were found; keep only one intro video in the project root.");
			return string.Empty;
		}

		return "res://" + ogvFiles[0];
	}

	private void StartPlayback()
	{
		if (_videoPlayer == null)
		{
			BeginLogoTransition();
			return;
		}

		if (_videoPlayer.Stream == null)
		{
			BeginLogoTransition();
			return;
		}

		_videoPlayer.Play();

		_audioController?.PlayMusic(AppAudioCueId.IntroMusic);
	}

	private void OnVideoFinished()
	{
		BeginLogoTransition();
	}

	private async void BeginLogoTransition()
	{
		if (_finished || _transitionStarted)
		{
			return;
		}

		_transitionStarted = true;

		if (_statusLabel != null)
		{
			_statusLabel.Text = "Loading main menu...";
		}

		if (_skipLabel != null)
		{
			_skipLabel.Text = string.Empty;
		}

		if (!IsInsideTree())
		{
			StopPlayback();
			Finish();
			return;
		}

		Tween transitionTween = CreateTween();
		transitionTween.SetParallel(true);

		if (_videoPlayer != null)
		{
			transitionTween.TweenProperty(_videoPlayer, "modulate:a", 0.0f, TransitionDurationSeconds);
		}

		if (_videoLayer != null)
		{
			transitionTween.TweenProperty(_videoLayer, "modulate:a", 0.0f, TransitionDurationSeconds);
		}

		if (_logoLayer != null)
		{
			transitionTween.TweenProperty(_logoLayer, "modulate:a", 1.0f, TransitionDurationSeconds);
		}

		if (_skipLabel != null)
		{
			transitionTween.TweenProperty(_skipLabel, "modulate:a", 0.0f, TransitionDurationSeconds * 0.5f);
		}

		_audioController?.FadeMusicTo(this, -40.0f, TransitionDurationSeconds);

		await ToSignal(transitionTween, Tween.SignalName.Finished);
		StopPlayback();

		if (_logoLayer != null)
		{
			_logoLayer.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		}

		if (LogoHoldSeconds <= 0.0f)
		{
			Finish();
			return;
		}

		SceneTree? tree = GetTree();
		if (tree == null)
		{
			Finish();
			return;
		}

		SceneTreeTimer holdTimer = tree.CreateTimer(LogoHoldSeconds);
		await ToSignal(holdTimer, SceneTreeTimer.SignalName.Timeout);
		Finish();
	}

	private void StopPlayback()
	{
		if (_videoPlayer != null && _videoPlayer.IsPlaying())
		{
			_videoPlayer.Stop();
		}

		_audioController?.StopMusic();
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
