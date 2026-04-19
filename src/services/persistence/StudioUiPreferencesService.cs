using Godot;

namespace StarGen.Services.Persistence;

/// <summary>
/// Stores lightweight UI preferences shared by the generation studios.
/// </summary>
public static class StudioUiPreferencesService
{
	private const string ConfigPath = "user://studio_ui_preferences.cfg";
	private const string SectionName = "studio_ui";
	private const string ShowSeedsKey = "show_seed_controls";
	private const string SkipIntroKey = "skip_intro";

	/// <summary>
	/// Immutable snapshot of the current studio UI preferences.
	/// </summary>
	public readonly struct StudioUiPreferences
	{
		/// <summary>
		/// Creates a new studio preference snapshot.
		/// </summary>
		public StudioUiPreferences(bool showSeedControls, bool skipIntro)
		{
			ShowSeedControls = showSeedControls;
			SkipIntro = skipIntro;
		}

		/// <summary>
		/// Whether deterministic seed fields should be shown in the studios.
		/// </summary>
		public bool ShowSeedControls { get; }

		/// <summary>
		/// Whether the startup intro should be skipped and the app should open directly to the main menu.
		/// </summary>
		public bool SkipIntro { get; }
	}

	/// <summary>
	/// Returns the saved studio preferences or defaults if none have been saved yet.
	/// </summary>
	public static StudioUiPreferences LoadOrDefault()
	{
		ConfigFile configFile = new();
		Error error = configFile.Load(ConfigPath);
		if (error != Error.Ok)
		{
			return CreateDefault();
		}

		bool showSeedControls = configFile.GetValue(SectionName, ShowSeedsKey, false).AsBool();
		bool skipIntro = configFile.GetValue(SectionName, SkipIntroKey, false).AsBool();
		return new StudioUiPreferences(showSeedControls, skipIntro);
	}

	/// <summary>
	/// Persists the provided studio preferences.
	/// </summary>
	public static void Save(StudioUiPreferences preferences)
	{
		ConfigFile configFile = new();
		configFile.SetValue(SectionName, ShowSeedsKey, preferences.ShowSeedControls);
		configFile.SetValue(SectionName, SkipIntroKey, preferences.SkipIntro);
		Error error = configFile.Save(ConfigPath);
		if (error != Error.Ok)
		{
			GD.PushError($"StudioUiPreferencesService: failed to save preferences ({error})");
		}
	}

	/// <summary>
	/// Returns the default studio preference set.
	/// </summary>
	public static StudioUiPreferences CreateDefault()
	{
		return new StudioUiPreferences(false, false);
	}
}
