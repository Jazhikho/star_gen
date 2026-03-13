using System;
using System.Collections.Generic;
using Godot;

namespace StarGen.Services.Persistence;

/// <summary>
/// Stores and applies user-facing window mode and resolution settings.
/// </summary>
public static class WindowSettingsService
{
	private const string ConfigPath = "user://window_settings.cfg";
	private const string SectionName = "window";
	private const string FullscreenKey = "fullscreen";
	private const string WidthKey = "width";
	private const string HeightKey = "height";
	public static readonly Vector2I MinimumSupportedWindowSize = new Vector2I(640, 480);

	private static readonly IReadOnlyList<Vector2I> _commonResolutions =
	[
		new Vector2I(1280, 720),
		new Vector2I(1600, 900),
		new Vector2I(1920, 1080),
		new Vector2I(2560, 1440),
	];

	/// <summary>
	/// Immutable snapshot of the current user-selected window state.
	/// </summary>
	public readonly struct WindowSettingsState
	{
		/// <summary>
		/// Creates a new window-settings snapshot.
		/// </summary>
		public WindowSettingsState(bool fullscreen, Vector2I resolution)
		{
			Fullscreen = fullscreen;
			Resolution = NormalizeResolution(resolution);
		}

		/// <summary>
		/// Whether the window should run in fullscreen mode.
		/// </summary>
		public bool Fullscreen { get; }

		/// <summary>
		/// The requested windowed resolution.
		/// </summary>
		public Vector2I Resolution { get; }
	}

	/// <summary>
	/// Returns the common windowed resolutions exposed by the UI.
	/// </summary>
	public static IReadOnlyList<Vector2I> GetCommonResolutions()
	{
		return _commonResolutions;
	}

	/// <summary>
	/// Applies the saved settings if they exist; otherwise persists the current window state.
	/// </summary>
	public static void ApplySavedOrCurrent()
	{
		Window? window = GetRootWindow();
		if (window != null)
		{
			ApplyMinimumSize(window);
		}

		WindowSettingsState settings = LoadOrCaptureCurrent();
		Apply(settings, saveAfterApply: false);
		if (!HasSavedSettings())
		{
			Save(settings);
		}
	}

	/// <summary>
	/// Returns the persisted settings if present; otherwise captures the current window state.
	/// </summary>
	public static WindowSettingsState LoadOrCaptureCurrent()
	{
		ConfigFile configFile = new();
		Error error = configFile.Load(ConfigPath);
		if (error != Error.Ok)
		{
			return CaptureCurrent();
		}

		bool fullscreen = configFile.GetValue(SectionName, FullscreenKey, false).AsBool();
		int width = (int)configFile.GetValue(SectionName, WidthKey, 1600).AsInt32();
		int height = (int)configFile.GetValue(SectionName, HeightKey, 900).AsInt32();
		return new WindowSettingsState(fullscreen, new Vector2I(width, height));
	}

	/// <summary>
	/// Captures the current window mode and size from the active display server.
	/// </summary>
	public static WindowSettingsState CaptureCurrent()
	{
		Window? window = GetRootWindow();
		DisplayServer.WindowMode mode = window != null
			? ToDisplayServerMode(window.Mode)
			: DisplayServer.WindowGetMode();
		bool fullscreen = mode == DisplayServer.WindowMode.Fullscreen
			|| mode == DisplayServer.WindowMode.ExclusiveFullscreen;
		Vector2I size = window != null
			? NormalizeResolution(window.Size)
			: NormalizeResolution(DisplayServer.WindowGetSize());
		return new WindowSettingsState(fullscreen, size);
	}

	/// <summary>
	/// Saves the provided settings to the user profile.
	/// </summary>
	public static void Save(WindowSettingsState settings)
	{
		ConfigFile configFile = new();
		configFile.SetValue(SectionName, FullscreenKey, settings.Fullscreen);
		configFile.SetValue(SectionName, WidthKey, settings.Resolution.X);
		configFile.SetValue(SectionName, HeightKey, settings.Resolution.Y);
		Error error = configFile.Save(ConfigPath);
		if (error != Error.Ok)
		{
			GD.PushError($"WindowSettingsService: failed to save settings ({error})");
		}
	}

	/// <summary>
	/// Applies the provided settings to the active window and optionally persists them.
	/// </summary>
	public static void Apply(WindowSettingsState settings, bool saveAfterApply = true)
	{
		if (IsHeadless())
		{
			if (saveAfterApply)
			{
				Save(settings);
			}

			return;
		}

		Window? window = GetRootWindow();
		if (window != null)
		{
			ApplyMinimumSize(window);
			ApplyToWindow(window, settings);
		}

		if (settings.Fullscreen)
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			DisplayServer.WindowSetSize(settings.Resolution);
			CenterWindow(settings.Resolution);
		}

		if (saveAfterApply)
		{
			Save(settings);
		}
	}

	/// <summary>
	/// Applies and persists the provided settings.
	/// </summary>
	public static void ApplyAndSave(WindowSettingsState settings)
	{
		Apply(settings, saveAfterApply: true);
	}

	/// <summary>
	/// Applies the provided settings directly to a specific Godot window.
	/// </summary>
	public static void ApplyToWindow(Window window, WindowSettingsState settings)
	{
		if (window == null)
		{
			throw new ArgumentNullException(nameof(window));
		}

		ApplyMinimumSize(window);

		if (settings.Fullscreen)
		{
			window.Mode = Window.ModeEnum.Fullscreen;
			return;
		}

		window.Mode = Window.ModeEnum.Windowed;
		window.Size = settings.Resolution;
	}

	/// <summary>
	/// Returns a resolution index for the provided size within the common resolution list.
	/// </summary>
	public static int FindResolutionIndex(Vector2I resolution)
	{
		for (int index = 0; index < _commonResolutions.Count; index += 1)
		{
			if (_commonResolutions[index] == resolution)
			{
				return index;
			}
		}

		return -1;
	}

	/// <summary>
	/// Formats a resolution for UI labels.
	/// </summary>
	public static string FormatResolutionLabel(Vector2I resolution)
	{
		return $"{resolution.X} x {resolution.Y}";
	}

	/// <summary>
	/// Formats a resolution for compact metadata values.
	/// </summary>
	public static string FormatResolutionKey(Vector2I resolution)
	{
		return $"{resolution.X}x{resolution.Y}";
	}

	/// <summary>
	/// Parses a compact resolution key.
	/// </summary>
	public static bool TryParseResolutionKey(string key, out Vector2I resolution)
	{
		resolution = new Vector2I(1600, 900);
		if (string.IsNullOrWhiteSpace(key))
		{
			return false;
		}

		string[] parts = key.Split('x', StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length != 2)
		{
			return false;
		}

		bool parsedWidth = int.TryParse(parts[0], out int width);
		bool parsedHeight = int.TryParse(parts[1], out int height);
		if (!parsedWidth || !parsedHeight)
		{
			return false;
		}

		resolution = NormalizeResolution(new Vector2I(width, height));
		return true;
	}

	/// <summary>
	/// Returns whether a persisted settings file is present.
	/// </summary>
	public static bool HasSavedSettings()
	{
		string absolutePath = ProjectSettings.GlobalizePath(ConfigPath);
		return FileAccess.FileExists(absolutePath);
	}

	private static Vector2I NormalizeResolution(Vector2I resolution)
	{
		if (resolution.X <= 0 || resolution.Y <= 0)
		{
			return new Vector2I(1600, 900);
		}

		int clampedWidth = Math.Max(resolution.X, MinimumSupportedWindowSize.X);
		int clampedHeight = Math.Max(resolution.Y, MinimumSupportedWindowSize.Y);
		return new Vector2I(clampedWidth, clampedHeight);
	}

	private static bool IsHeadless()
	{
		string displayName = DisplayServer.GetName();
		return string.Equals(displayName, "headless", StringComparison.OrdinalIgnoreCase);
	}

	private static Window? GetRootWindow()
	{
		SceneTree? tree = Engine.GetMainLoop() as SceneTree;
		return tree?.Root;
	}

	private static void ApplyMinimumSize(Window window)
	{
		window.MinSize = MinimumSupportedWindowSize;
	}

	private static DisplayServer.WindowMode ToDisplayServerMode(Window.ModeEnum mode)
	{
		if (mode == Window.ModeEnum.Fullscreen)
		{
			return DisplayServer.WindowMode.Fullscreen;
		}

		if (mode == Window.ModeEnum.ExclusiveFullscreen)
		{
			return DisplayServer.WindowMode.ExclusiveFullscreen;
		}

		return DisplayServer.WindowMode.Windowed;
	}

	private static void CenterWindow(Vector2I resolution)
	{
		int screen = DisplayServer.WindowGetCurrentScreen();
		Rect2I usableRect = DisplayServer.ScreenGetUsableRect(screen);
		if (usableRect.Size.X <= 0 || usableRect.Size.Y <= 0)
		{
			return;
		}

		int centeredX = usableRect.Position.X + (usableRect.Size.X - resolution.X) / 2;
		int centeredY = usableRect.Position.Y + (usableRect.Size.Y - resolution.Y) / 2;
		DisplayServer.WindowSetPosition(new Vector2I(centeredX, centeredY));
	}
}
