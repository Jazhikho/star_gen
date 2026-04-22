using System.Collections.Generic;
using Godot;
using StarGen.App.Shared;
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
	public delegate void station_generation_requestedEventHandler();

	[Signal]
	public delegate void quit_requestedEventHandler();

	private enum ContentPanel
	{
		Overview,
		Help,
		Credits,
		Sources,
		ReleaseNotes,
		Options,
	}

	private Label? _versionLabel;
	private Label? _heroNoteLabel;
	private Button? _galaxyButton;
	private Button? _systemButton;
	private Button? _objectButton;
	private Button? _stationButton;
	private Button? _helpButton;
	private Button? _creditsButton;
	private Button? _sourcesButton;
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
	private CheckBox? _fullscreenCheck;
	private CheckBox? _showSeedControlsCheck;
	private CheckBox? _skipIntroCheck;
	private OptionButton? _resolutionOption;
	private Button? _applyOptionsButton;
	private Label? _optionsStatusLabel;
	private Window? _infoDialog;
	private RichTextLabel? _infoDialogText;
	private Window? _optionsDialog;
	private Button? _infoDialogCloseButton;
	private Button? _optionsDialogCloseButton;

	/// <summary>
	/// Initializes menu wiring and static content.
	/// </summary>
	public override void _Ready()
	{
		CacheNodeReferences();
		ConnectSignals();
		PopulateStaticText();
		PopulateResolutionOptions();
		RefreshOptionsState();
		ShowPanel(ContentPanel.Overview);
	}

	/// <summary>
	/// Synchronizes the options UI with the current window and studio-preference state.
	/// </summary>
	public void RefreshOptionsState()
	{
		if (_fullscreenCheck == null || _resolutionOption == null)
		{
			return;
		}

		WindowSettingsService.WindowSettingsState currentSettings = WindowSettingsService.LoadOrCaptureCurrent();
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

		StudioUiPreferencesService.StudioUiPreferences studioPreferences = StudioUiPreferencesService.LoadOrDefault();
		if (_showSeedControlsCheck != null)
		{
			_showSeedControlsCheck.ButtonPressed = studioPreferences.ShowSeedControls;
		}
		if (_skipIntroCheck != null)
		{
			_skipIntroCheck.ButtonPressed = studioPreferences.SkipIntro;
		}

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

			if (studioPreferences.ShowSeedControls)
			{
				_optionsStatusLabel.Text = $"{modeText}. All studio seeds are visible.";
			}
			else
			{
				_optionsStatusLabel.Text = $"{modeText}. All studio seeds are hidden and reroll on each launch.";
			}

			if (studioPreferences.SkipIntro)
			{
				_optionsStatusLabel.Text += " Intro is skipped on startup.";
			}
			else
			{
				_optionsStatusLabel.Text += " Intro plays on startup.";
			}
		}
	}

	private void CacheNodeReferences()
	{
		const string Root = "MarginContainer/ScrollContainer/Layout";
		_versionLabel = GetNodeOrNull<Label>($"{Root}/HeroPanel/MarginContainer/HeroVBox/TopRow/VersionLabel");
		_heroNoteLabel = GetNodeOrNull<Label>($"{Root}/HeroPanel/MarginContainer/HeroVBox/ReleaseNote");
		_galaxyButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardGalaxy/MarginContainer/VBoxContainer/GalaxyButton");
		_systemButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardSystem/MarginContainer/VBoxContainer/SystemButton");
		_objectButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardObject/MarginContainer/VBoxContainer/ObjectButton");
		_stationButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardStation/MarginContainer/VBoxContainer/StationButton");
		_helpButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/HelpButton");
		_creditsButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/CreditsButton");
		_sourcesButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/SourcesButton");
		_releaseNotesButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/ReleaseNotesButton");
		_optionsButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/SecondaryButtons/OptionsButton");
		_quitButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/UtilityRow/UtilityPanel/MarginContainer/UtilityVBox/QuitButton");
		_overviewPanel = GetNodeOrNull<Control>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OverviewPanel");
		_helpPanel = GetNodeOrNull<Control>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/HelpPanel");
		_creditsPanel = GetNodeOrNull<Control>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/CreditsPanel");
		_releaseNotesPanel = GetNodeOrNull<Control>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/ReleaseNotesPanel");
		_optionsPanel = GetNodeOrNull<Control>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel");
		_helpText = GetNodeOrNull<RichTextLabel>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/HelpPanel/HelpText");
		_creditsText = GetNodeOrNull<RichTextLabel>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/CreditsPanel/CreditsText");
		_releaseNotesText = GetNodeOrNull<RichTextLabel>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/ReleaseNotesPanel/ReleaseNotesText");
		_fullscreenCheck = GetNodeOrNull<CheckBox>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/FullscreenCheck");
		_showSeedControlsCheck = GetNodeOrNull<CheckBox>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/ShowSeedControlsCheck");
		_skipIntroCheck = GetNodeOrNull<CheckBox>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/SkipIntroCheck");
		_resolutionOption = GetNodeOrNull<OptionButton>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/ResolutionRow/ResolutionOption");
		_applyOptionsButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/ApplyOptionsButton");
		_optionsStatusLabel = GetNodeOrNull<Label>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/OptionsStatusLabel");
		_infoDialog = GetNodeOrNull<Window>("InfoDialog");
		_infoDialogText = GetNodeOrNull<RichTextLabel>("InfoDialog/MarginContainer/InfoVBox/InfoDialogText");
		_infoDialogCloseButton = GetNodeOrNull<Button>("InfoDialog/MarginContainer/InfoVBox/CloseButton");
		_optionsDialog = GetNodeOrNull<Window>("OptionsDialog");
		if (_fullscreenCheck == null)
		{
			_fullscreenCheck = GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/FullscreenCheck");
		}

		if (_showSeedControlsCheck == null)
		{
			_showSeedControlsCheck = GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/ShowSeedControlsCheck");
		}

		if (_skipIntroCheck == null)
		{
			_skipIntroCheck = GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/SkipIntroCheck");
		}

		if (_resolutionOption == null)
		{
			_resolutionOption = GetNodeOrNull<OptionButton>("OptionsDialog/MarginContainer/OptionsVBox/ResolutionRow/ResolutionOption");
		}

		if (_applyOptionsButton == null)
		{
			_applyOptionsButton = GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/ApplyOptionsButton");
		}

		if (_optionsStatusLabel == null)
		{
			_optionsStatusLabel = GetNodeOrNull<Label>("OptionsDialog/MarginContainer/OptionsVBox/OptionsStatusLabel");
		}

		_optionsDialogCloseButton = GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/CloseButton");
	}

	private void ConnectSignals()
	{
		if (_galaxyButton != null) _galaxyButton.Connect(Button.SignalName.Pressed, Callable.From(OnGalaxyButtonPressed));
		if (_systemButton != null) _systemButton.Connect(Button.SignalName.Pressed, Callable.From(OnSystemButtonPressed));
		if (_objectButton != null) _objectButton.Connect(Button.SignalName.Pressed, Callable.From(OnObjectButtonPressed));
		if (_stationButton != null) _stationButton.Connect(Button.SignalName.Pressed, Callable.From(OnStationButtonPressed));
		if (_helpButton != null) _helpButton.Connect(Button.SignalName.Pressed, Callable.From(OnHelpButtonPressed));
		if (_creditsButton != null) _creditsButton.Connect(Button.SignalName.Pressed, Callable.From(OnCreditsButtonPressed));
		if (_sourcesButton != null) _sourcesButton.Connect(Button.SignalName.Pressed, Callable.From(OnSourcesButtonPressed));
		if (_releaseNotesButton != null) _releaseNotesButton.Connect(Button.SignalName.Pressed, Callable.From(OnReleaseNotesButtonPressed));
		if (_optionsButton != null) _optionsButton.Connect(Button.SignalName.Pressed, Callable.From(OnOptionsButtonPressed));
		if (_quitButton != null) _quitButton.Connect(Button.SignalName.Pressed, Callable.From(OnQuitButtonPressed));
		if (_applyOptionsButton != null) _applyOptionsButton.Connect(Button.SignalName.Pressed, Callable.From(ApplyWindowSettings));
		if (_infoDialogCloseButton != null) _infoDialogCloseButton.Pressed += HideInfoDialog;
		if (_optionsDialogCloseButton != null) _optionsDialogCloseButton.Pressed += HideOptionsDialog;
		if (_infoDialog != null) _infoDialog.CloseRequested += HideInfoDialog;
		if (_optionsDialog != null) _optionsDialog.CloseRequested += HideOptionsDialog;
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
		string version = UserFacingVersionHelper.GetDisplayVersion();
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
				"- Galaxy Studio: Configure galaxy shape, scientific assumptions, and generation overrides before generating the galaxy viewer.\n\n" +
				"- System Studio: Set stellar counts, seed, and worldbuilding assumptions before opening the system viewer.\n\n" +
				"- Object Studio: Choose a star, planet, asteroid, or comet preset before launching the object viewer.\n\n" +
				"- Station Studio: Configure an individual station concept and review the current station workflow.\n\n" +
				"- Sources: Review the astronomy and worldbuilding references currently guiding the generator's assumptions and realism goals.";
		}

		if (_creditsText != null)
		{
			_creditsText.Text =
				"Credits\n\n" +
				"Design and direction: Jazhikho\n\n" +
				"AI assistance: OpenAI Codex / GPT models, Anthropic Claude, and Cursor were used under human direction for exploration, drafting, refactoring, testing support, UI copy iteration, documentation/provenance upkeep, and focused implementation assistance. Human review remained responsible for design, realism, licensing, and release decisions.\n\n" +
				"App icon: Galaxy icon by Freepik via Flaticon, used with attribution.\n\n" +
				"Music: \"Thus Spoke Zarathustra\" (Introduction / Sunrise), adapted from the Kevin MacLeod source archived on Free Music Archive.\n\n" +
				"Patreon support\n\n" +
				"Thank you to Leo for supporting StarGen on Patreon.\n\n" +
				"Astronomy and worldbuilding references can be reviewed from Sources on the main menu.";
		}

		if (_releaseNotesText != null)
		{
			_releaseNotesText.Text = GetReleaseNotesContent();
		}
	}

	private void OnGalaxyButtonPressed()
	{
		EmitSignal(SignalName.galaxy_generation_requested);
	}

	private void OnSystemButtonPressed()
	{
		EmitSignal(SignalName.system_generation_requested);
	}

	private void OnObjectButtonPressed()
	{
		EmitSignal(SignalName.object_generation_requested);
	}

	private void OnStationButtonPressed()
	{
		EmitSignal(SignalName.station_generation_requested);
	}

	private void OnHelpButtonPressed()
	{
		ShowPanel(ContentPanel.Help);
	}

	private void OnCreditsButtonPressed()
	{
		ShowPanel(ContentPanel.Credits);
	}

	private void OnSourcesButtonPressed()
	{
		ShowPanel(ContentPanel.Sources);
	}

	private void OnReleaseNotesButtonPressed()
	{
		ShowPanel(ContentPanel.ReleaseNotes);
	}

	private void OnOptionsButtonPressed()
	{
		ShowPanel(ContentPanel.Options);
	}

	private void OnQuitButtonPressed()
	{
		EmitSignal(SignalName.quit_requested);
	}

	/// <summary>
	/// Returns the full release notes text shown in the Release Notes panel.
	/// </summary>
	private static string GetReleaseNotesContent()
	{
		return
			"Version 0.9d\n\n" +
			"- Mainline remains focused on deterministic generation and viewing across Galaxy, System, Object, and Station workflows.\n" +
			"- Scientific assumptions, compatibility overrides, runtime toggles, and readout controls are now partitioned more clearly in the active studios.\n" +
			"- Planetary generation, habitable-zone handling, life support, and sentient-world baseline fields now follow the audited science passes added during the 0.9 hardening line.\n" +
			"- Galaxy, System, and Object viewers use scene-owned controls-panel geometry and more explicit input help.\n" +
			"- Mainline save and load affordances remain removed from the shipped runtime path.\n" +
			"- Release prep now includes a documented build path, a live acceptance checklist, and itch publishing guidance.\n\n" +
			"Version 0.8.0.0\n\n" +
			"- Replaced the timer-based splash with the root intro video, a clean fade into the StarGen logo, and skip behavior that still resolves through the branded transition.\n" +
			"- Moved startup audio onto the shared app audio controller and library so the intro track is an explicit exported resource instead of a runtime directory scan.\n" +
			"- Added a fade-to-black handoff from the splash into the main menu.\n" +
			"- Replaced the Station Studio placeholder with the production station generation flow and live detail views.\n" +
			"- Reworked Galaxy Studio around clearer shape controls, separated generation rules, and a stronger active-profile summary.\n" +
			"- Life Potential now shapes generation, while colonization settings have moved into simulation tooling.";
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
		if (panel == ContentPanel.Sources)
		{
			ShowFallbackPanel(panel);
			return;
		}

		if (!HasEmbeddedContentPanels())
		{
			ShowFallbackPanel(panel);
			return;
		}

		if (_overviewPanel != null) _overviewPanel.Visible = panel == ContentPanel.Overview;
		if (_helpPanel != null) _helpPanel.Visible = panel == ContentPanel.Help;
		if (_creditsPanel != null) _creditsPanel.Visible = panel == ContentPanel.Credits;
		if (_releaseNotesPanel != null) _releaseNotesPanel.Visible = panel == ContentPanel.ReleaseNotes;
		if (_optionsPanel != null) _optionsPanel.Visible = panel == ContentPanel.Options;
	}

	private bool HasEmbeddedContentPanels()
	{
		if (_overviewPanel != null)
		{
			return true;
		}

		if (_helpPanel != null)
		{
			return true;
		}

		if (_creditsPanel != null)
		{
			return true;
		}

		if (_releaseNotesPanel != null)
		{
			return true;
		}

		if (_optionsPanel != null)
		{
			return true;
		}

		return false;
	}

	private void ShowFallbackPanel(ContentPanel panel)
	{
		if (panel == ContentPanel.Overview)
		{
			if (_infoDialog != null)
			{
				_infoDialog.Hide();
			}

			if (_optionsDialog != null)
			{
				_optionsDialog.Hide();
			}

			return;
		}

		if (panel == ContentPanel.Help)
		{
			ShowInfoDialog("Help", _helpText?.Text ?? BuildHelpFallbackText());
			return;
		}

		if (panel == ContentPanel.Credits)
		{
			ShowInfoDialog("Credits", _creditsText?.Text ?? BuildCreditsFallbackText());
			return;
		}

		if (panel == ContentPanel.Sources)
		{
			ShowInfoDialog("Sources", BuildSourcesFallbackText());
			return;
		}

		if (panel == ContentPanel.ReleaseNotes)
		{
			ShowInfoDialog("Release Notes", _releaseNotesText?.Text ?? GetReleaseNotesContent());
			return;
		}

		if (panel == ContentPanel.Options)
		{
			ShowOptionsDialog();
		}
	}

	private void ShowInfoDialog(string title, string body)
	{
		if (_infoDialog == null || _infoDialogText == null)
		{
			return;
		}

		_infoDialog.Title = title;
		_infoDialogText.Text = body;
		ShowWindow(_infoDialog, new Vector2I(720, 520));
	}

	private void HideInfoDialog()
	{
		if (_infoDialog != null)
		{
			_infoDialog.Hide();
		}
	}

	private void ShowOptionsDialog()
	{
		RefreshOptionsState();
		if (_optionsDialog != null)
		{
			ShowWindow(_optionsDialog, new Vector2I(520, 320));
		}
	}

	private void HideOptionsDialog()
	{
		if (_optionsDialog != null)
		{
			_optionsDialog.Hide();
		}
	}

	private static void ShowWindow(Window window, Vector2I size)
	{
		if (window.IsInsideTree())
		{
			window.PopupCentered(size);
			return;
		}

		window.Size = size;
		window.Visible = true;
	}

	private static string BuildHelpFallbackText()
	{
		return
			"How to use StarGen\n\n" +
			"- Galaxy Studio: Configure galaxy shape, scientific assumptions, and generation overrides before generating the galaxy viewer.\n\n" +
			"- System Studio: Set stellar counts, seed, and worldbuilding assumptions before opening the system viewer.\n\n" +
			"- Object Studio: Choose a star, planet, asteroid, or comet preset before launching the object viewer.\n\n" +
			"- Station Studio: Configure an individual station concept and review the current station workflow.\n\n" +
			"- Sources: Review the astronomy and worldbuilding references currently guiding the generator's assumptions and realism goals.";
	}

	private static string BuildCreditsFallbackText()
	{
		return
			"Credits\n\n" +
			"Design and direction: Jazhikho\n\n" +
			"AI assistance: OpenAI Codex / GPT models, Anthropic Claude, and Cursor were used under human direction for exploration, drafting, refactoring, testing support, UI copy iteration, documentation/provenance upkeep, and focused implementation assistance. Human review remained responsible for design, realism, licensing, and release decisions.\n\n" +
			"App icon: Galaxy icon by Freepik via Flaticon, used with attribution.\n\n" +
			"Music: \"Thus Spoke Zarathustra\" (Introduction / Sunrise), adapted from the Kevin MacLeod source archived on Free Music Archive.\n\n" +
			"Patreon support\n\n" +
			"Thank you to Leo for supporting StarGen on Patreon.\n\n" +
			"Astronomy and worldbuilding references can be reviewed from Sources on the main menu.";
	}

	private static string BuildSourcesFallbackText()
	{
		return
			"Sources\n\n" +
			"StarGen's galaxy-morphology defaults are currently informed by astronomy reviews and structure papers, including:\n\n" +
			"- Bland-Hawthorn and Gerhard (2016) on Milky Way structure and context.\n" +
			"- van der Kruit and Freeman (2011) on galaxy disks.\n" +
			"- Wegg and Gerhard (2013) on the Galactic bulge.\n" +
			"- Conselice (2014) on galaxy structure over cosmic time.\n\n" +
			"These source notes are provided so you can review what the current assumptions are based on while the generator continues to be refined.";
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
			ApplyStudioPreferences();
			RefreshOptionsState();
			if (_optionsStatusLabel != null)
			{
				_optionsStatusLabel.Text = "Fullscreen active. All studio and intro preferences were saved.";
			}
			HideOptionsDialog();
			return;
		}

		Vector2I resolution = GetSelectedResolution();
		WindowSettingsService.ApplyAndSave(new WindowSettingsService.WindowSettingsState(false, resolution));
		ApplyStudioPreferences();
		RefreshOptionsState();
		if (_optionsStatusLabel != null)
		{
			_optionsStatusLabel.Text = $"Windowed at {resolution.X} x {resolution.Y}. All studio and intro preferences were saved.";
		}

		HideOptionsDialog();
	}

	private void ApplyStudioPreferences()
	{
		bool showSeedControls = false;
		bool skipIntro = false;
		if (_showSeedControlsCheck != null)
		{
			showSeedControls = _showSeedControlsCheck.ButtonPressed;
		}

		if (_skipIntroCheck != null)
		{
			skipIntro = _skipIntroCheck.ButtonPressed;
		}

		StudioUiPreferencesService.Save(new StudioUiPreferencesService.StudioUiPreferences(showSeedControls, skipIntro));
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
