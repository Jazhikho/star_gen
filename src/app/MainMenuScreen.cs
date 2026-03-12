using System.Collections.Generic;
using Godot;
using StarGen.Services.Persistence;

namespace StarGen.App;

/// <summary>
/// Primary menu for choosing galaxy, system, or object generation and viewing help or options.
/// </summary>
public partial class MainMenuScreen : Control
{
	[Signal]
	public delegate void galaxy_generation_requestedEventHandler();

	[Signal]
	public delegate void system_generation_requestedEventHandler();

	[Signal]
	public delegate void object_generation_requestedEventHandler();

	[Signal]
	public delegate void quit_requestedEventHandler();

	private enum ContentPanel
	{
		Overview,
		Help,
		Credits,
		ReleaseNotes,
		Options,
	}

	private Label? _versionLabel;
	private Label? _heroNoteLabel;
	private Button? _galaxyButton;
	private Button? _systemButton;
	private Button? _objectButton;
	private Button? _helpButton;
	private Button? _creditsButton;
	private Button? _releaseNotesButton;
	private Button? _optionsButton;
	private Button? _quitButton;
	private Control? _overviewPanel;
	private Control? _helpPanel;
	private Control? _creditsPanel;
	private Control? _releaseNotesPanel;
	private Control? _optionsPanel;
	private RichTextLabel? _helpText;
	private RichTextLabel? _creditsText;
	private RichTextLabel? _releaseNotesText;
	private CheckButton? _fullscreenCheck;
	private OptionButton? _resolutionOption;
	private Button? _applyOptionsButton;
	private Label? _optionsStatusLabel;

	/// <summary>
	/// Initializes menu wiring and static content.
	/// </summary>
	public override void _Ready()
	{
		CacheNodeReferences();
		ConnectSignals();
		PopulateStaticText();
		PopulateResolutionOptions();
		RefreshWindowSettings();
		ShowPanel(ContentPanel.Overview);
	}

	/// <summary>
	/// Synchronizes the options UI with the current window state.
	/// </summary>
	public void RefreshWindowSettings()
	{
		if (_fullscreenCheck == null || _resolutionOption == null)
		{
			return;
		}

		WindowSettingsService.WindowSettingsState currentSettings = WindowSettingsService.CaptureCurrent();
		_fullscreenCheck.ButtonPressed = currentSettings.Fullscreen;

		int index = FindResolutionIndex(currentSettings.Resolution);
		if (index < 0)
		{
			_resolutionOption.AddItem(WindowSettingsService.FormatResolutionLabel(currentSettings.Resolution));
			_resolutionOption.SetItemMetadata(
				_resolutionOption.ItemCount - 1,
				WindowSettingsService.FormatResolutionKey(currentSettings.Resolution));
			index = _resolutionOption.ItemCount - 1;
		}

		_resolutionOption.Select(index);
		_resolutionOption.Disabled = currentSettings.Fullscreen;

		if (_optionsStatusLabel != null)
		{
			string modeText;
			if (currentSettings.Fullscreen)
			{
				modeText = "Fullscreen active";
			}
			else
			{
				modeText = $"Windowed at {currentSettings.Resolution.X} x {currentSettings.Resolution.Y}";
			}
			_optionsStatusLabel.Text = modeText;
		}
	}

	private void CacheNodeReferences()
	{
		const string Root = "MarginContainer/ScrollContainer/Layout";
		_versionLabel = GetNodeOrNull<Label>($"{Root}/HeroPanel/MarginContainer/HeroVBox/TopRow/VersionLabel");
		_heroNoteLabel = GetNodeOrNull<Label>($"{Root}/HeroPanel/MarginContainer/HeroVBox/ReleaseNote");
		_galaxyButton = GetNodeOrNull<Button>($"{Root}/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardGalaxy/MarginContainer/VBoxContainer/GalaxyButton");
		_systemButton = GetNodeOrNull<Button>($"{Root}/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardSystem/MarginContainer/VBoxContainer/SystemButton");
		_objectButton = GetNodeOrNull<Button>($"{Root}/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardObject/MarginContainer/VBoxContainer/ObjectButton");
		_helpButton = GetNodeOrNull<Button>($"{Root}/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/HelpButton");
		_creditsButton = GetNodeOrNull<Button>($"{Root}/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/CreditsButton");
		_releaseNotesButton = GetNodeOrNull<Button>($"{Root}/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/ReleaseNotesButton");
		_optionsButton = GetNodeOrNull<Button>($"{Root}/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/OptionsButton");
		_quitButton = GetNodeOrNull<Button>($"{Root}/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/QuitButton");
		_overviewPanel = GetNodeOrNull<Control>($"{Root}/UtilityRow/ContentPanel/MarginContainer/ContentStack/OverviewPanel");
		_helpPanel = GetNodeOrNull<Control>($"{Root}/UtilityRow/ContentPanel/MarginContainer/ContentStack/HelpPanel");
		_creditsPanel = GetNodeOrNull<Control>($"{Root}/UtilityRow/ContentPanel/MarginContainer/ContentStack/CreditsPanel");
		_releaseNotesPanel = GetNodeOrNull<Control>($"{Root}/UtilityRow/ContentPanel/MarginContainer/ContentStack/ReleaseNotesPanel");
		_optionsPanel = GetNodeOrNull<Control>($"{Root}/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel");
		_helpText = GetNodeOrNull<RichTextLabel>($"{Root}/UtilityRow/ContentPanel/MarginContainer/ContentStack/HelpPanel/HelpText");
		_creditsText = GetNodeOrNull<RichTextLabel>($"{Root}/UtilityRow/ContentPanel/MarginContainer/ContentStack/CreditsPanel/CreditsText");
		_releaseNotesText = GetNodeOrNull<RichTextLabel>($"{Root}/UtilityRow/ContentPanel/MarginContainer/ContentStack/ReleaseNotesPanel/ReleaseNotesText");
		_fullscreenCheck = GetNodeOrNull<CheckButton>($"{Root}/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/FullscreenCheck");
		_resolutionOption = GetNodeOrNull<OptionButton>($"{Root}/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/ResolutionRow/ResolutionOption");
		_applyOptionsButton = GetNodeOrNull<Button>($"{Root}/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/ApplyOptionsButton");
		_optionsStatusLabel = GetNodeOrNull<Label>($"{Root}/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/OptionsStatusLabel");
	}

	private void ConnectSignals()
	{
		if (_galaxyButton != null) _galaxyButton.Pressed += () => EmitSignal(SignalName.galaxy_generation_requested);
		if (_systemButton != null) _systemButton.Pressed += () => EmitSignal(SignalName.system_generation_requested);
		if (_objectButton != null) _objectButton.Pressed += () => EmitSignal(SignalName.object_generation_requested);
		if (_helpButton != null) _helpButton.Pressed += () => ShowPanel(ContentPanel.Help);
		if (_creditsButton != null) _creditsButton.Pressed += () => ShowPanel(ContentPanel.Credits);
		if (_releaseNotesButton != null) _releaseNotesButton.Pressed += () => ShowPanel(ContentPanel.ReleaseNotes);
		if (_optionsButton != null) _optionsButton.Pressed += () => ShowPanel(ContentPanel.Options);
		if (_quitButton != null) _quitButton.Pressed += () => EmitSignal(SignalName.quit_requested);
		if (_applyOptionsButton != null) _applyOptionsButton.Pressed += ApplyWindowSettings;
		if (_fullscreenCheck != null) _fullscreenCheck.Toggled += enabled =>
		{
			if (_resolutionOption != null)
			{
				_resolutionOption.Disabled = enabled;
			}
		};
	}

	private void PopulateStaticText()
	{
		string version = ProjectSettings.GetSetting("application/config/version", "0.4.1.0").AsString();
		if (_versionLabel != null)
		{
			_versionLabel.Text = $"Version {version}";
		}

		if (_heroNoteLabel != null)
		{
			_heroNoteLabel.Text = "";
		}

		if (_helpText != null)
		{
			_helpText.Text =
				"How to use StarGen\n\n" +
				"- Galaxy Studio: Configure a galaxy profile first, then open the galaxy viewer to explore sectors and star systems.\n\n" +
				"- System Studio: Set stellar counts, seed, and Traveller assumptions before opening the system viewer.\n\n" +
				"- Object Studio: Choose a star, planet, moon, or asteroid preset before launching the object viewer.";
		}

		if (_creditsText != null)
		{
			_creditsText.Text =
				"Credits\n\n" +
				"Design and direction: Jazhikho\n\n" +
				"StarGen uses astronomy and worldbuilding references for its generation parameters. See the project's Sources folder for further reading.";
		}

		if (_releaseNotesText != null)
		{
			_releaseNotesText.Text = GetReleaseNotesContent();
		}
	}

	/// <summary>
	/// Returns the full release notes text shown in the Release Notes panel.
	/// </summary>
	private static string GetReleaseNotesContent()
	{
		return
			"Version 0.4.1.0\n\n" +
			"- Reworked the app entry flow around dedicated galaxy, system, and object generation studios.\n" +
			"- Main-menu launches now open viewers with generated content instead of empty setup states.\n\n" +
			"Version 0.4.0.1\n\n" +
			"- Fixed galaxy-sector star snapshot lifetime so returned stars remain valid under full headless test runs.\n" +
			"- Added regression coverage for detached galaxy-sector star snapshots.\n\n" +
			"Version 0.4.0\n\n" +
				"- New Main Menu and Release Notes.\n" +
				"- Save/load: save and load body files (.sgt, .sgp, .sga, .sgb) and system files (.sgs) from the object and system viewers.\n" +
				"- Gas giant variety: gas giants in the system viewer now use varied archetypes and per-planet variation.\n" +
				"- Edit and save: edit a body in the object viewer (Edit dialog) and save as file; optional Traveller UWP size code in the editor.\n\n" +
			"Version 0.2.0\n\n" +
				"- Asteroid belt generation and rendering in the system viewer.\n" +
				"- Scientific calibration: GenerationRealismProfile, benchmarks, ensemble harness, and distribution tests.\n" +
				"- Belt renderer and generator integration; OrbitSlotGenerator, OrbitalMechanics, StellarConfigGenerator, and SystemValidator updates.\n" +
				"- GalaxyInspectorPanel and test suite updates. Removed Concepts/AsteroidBelt demo scenes and Tests/RunGalaxyTests.gd.\n\n" +
			"Version 0.1.0\n\n" +
				"- First unofficial release.\n" +
				"- Object and system viewers; galaxy data model and viewer (welcome screen, GalaxyConfig, density models, save/load).\n" +
				"- Population framework, stations, and jump lanes (domain and prototype).";
	}

	private void PopulateResolutionOptions()
	{
		if (_resolutionOption == null || _resolutionOption.ItemCount > 0)
		{
			return;
		}

		foreach (Vector2I resolution in CommonResolutions)
		{
			_resolutionOption.AddItem(WindowSettingsService.FormatResolutionLabel(resolution));
			_resolutionOption.SetItemMetadata(_resolutionOption.ItemCount - 1, WindowSettingsService.FormatResolutionKey(resolution));
		}
	}

	private void ShowPanel(ContentPanel panel)
	{
		if (_overviewPanel != null) _overviewPanel.Visible = panel == ContentPanel.Overview;
		if (_helpPanel != null) _helpPanel.Visible = panel == ContentPanel.Help;
		if (_creditsPanel != null) _creditsPanel.Visible = panel == ContentPanel.Credits;
		if (_releaseNotesPanel != null) _releaseNotesPanel.Visible = panel == ContentPanel.ReleaseNotes;
		if (_optionsPanel != null) _optionsPanel.Visible = panel == ContentPanel.Options;
	}

	private void ApplyWindowSettings()
	{
		if (_fullscreenCheck == null || _resolutionOption == null)
		{
			return;
		}

		if (_fullscreenCheck.ButtonPressed)
		{
			WindowSettingsService.ApplyAndSave(new WindowSettingsService.WindowSettingsState(true, GetSelectedResolution()));
			if (_optionsStatusLabel != null)
			{
				_optionsStatusLabel.Text = "Applied fullscreen mode";
			}

			return;
		}

		Vector2I resolution = GetSelectedResolution();
		WindowSettingsService.ApplyAndSave(new WindowSettingsService.WindowSettingsState(false, resolution));
		if (_optionsStatusLabel != null)
		{
			_optionsStatusLabel.Text = $"Applied windowed mode at {resolution.X} x {resolution.Y}";
		}
	}

	private Vector2I GetSelectedResolution()
	{
		if (_resolutionOption == null || _resolutionOption.Selected < 0)
		{
			return new Vector2I(1600, 900);
		}

		Variant metadata = _resolutionOption.GetItemMetadata(_resolutionOption.Selected);
		string text;
		if (metadata.VariantType == Variant.Type.String)
		{
			text = metadata.AsString();
		}
		else
		{
			text = "1600x900";
		}
		if (WindowSettingsService.TryParseResolutionKey(text, out Vector2I resolution))
		{
			return resolution;
		}

		return new Vector2I(1600, 900);
	}

	private int FindResolutionIndex(Vector2I size)
	{
		if (_resolutionOption == null)
		{
			return -1;
		}

		string key = WindowSettingsService.FormatResolutionKey(size);
		for (int i = 0; i < _resolutionOption.ItemCount; i += 1)
		{
			Variant metadata = _resolutionOption.GetItemMetadata(i);
			if (metadata.VariantType == Variant.Type.String && metadata.AsString() == key)
			{
				return i;
			}
		}

		return -1;
	}

	private static IReadOnlyList<Vector2I> CommonResolutions => WindowSettingsService.GetCommonResolutions();
}
