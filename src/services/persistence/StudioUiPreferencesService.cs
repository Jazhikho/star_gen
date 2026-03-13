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

	/// <summary>
	/// Immutable snapshot of the current studio UI preferences.
	/// </summary>
	public readonly struct StudioUiPreferences
	{
		/// <summary>
		/// Creates a new studio preference snapshot.
		/// </summary>
		public StudioUiPreferences(bool showSeedControls)
		{
			ShowSeedControls = showSeedControls;
		}

		/// <summary>
		/// Whether deterministic seed fields should be shown in the studios.
		/// </summary>
		public bool ShowSeedControls { get; }
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
		return new StudioUiPreferences(showSeedControls);
	}

	/// <summary>
	/// Persists the provided studio preferences.
	/// </summary>
	public static void Save(StudioUiPreferences preferences)
	{
		ConfigFile configFile = new();
		configFile.SetValue(SectionName, ShowSeedsKey, preferences.ShowSeedControls);
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
		return new StudioUiPreferences(false);
	}
}
