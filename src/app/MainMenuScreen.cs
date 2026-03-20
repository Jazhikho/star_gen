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
	public delegate void concept_atlas_requestedEventHandler();

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
	private Button? _conceptAtlasButton;
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
	private CheckButton? _fullscreenCheck;
	private CheckButton? _showSeedControlsCheck;
	private OptionButton? _resolutionOption;
	private Button? _applyOptionsButton;
	private Label? _optionsStatusLabel;
	private Window? _infoDialog;
	private RichTextLabel? _infoDialogText;
	private Window? _optionsDialog;

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

		StudioUiPreferencesService.StudioUiPreferences studioPreferences = StudioUiPreferencesService.LoadOrDefault();
		if (_showSeedControlsCheck != null)
		{
			_showSeedControlsCheck.ButtonPressed = studioPreferences.ShowSeedControls;
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
				_optionsStatusLabel.Text = $"{modeText}. Studio seeds are visible.";
			}
			else
			{
				_optionsStatusLabel.Text = $"{modeText}. Studio seeds are hidden and reroll on each launch.";
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
		_conceptAtlasButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/ModesPanel/MarginContainer/ModesVBox/ModeCards/CardConceptAtlas/MarginContainer/VBoxContainer/ConceptAtlasButton");
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
		_fullscreenCheck = GetNodeOrNull<CheckButton>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/FullscreenCheck");
		_showSeedControlsCheck = GetNodeOrNull<CheckButton>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/ShowSeedControlsCheck");
		_resolutionOption = GetNodeOrNull<OptionButton>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/ResolutionRow/ResolutionOption");
		_applyOptionsButton = GetNodeOrNull<Button>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/ApplyOptionsButton");
		_optionsStatusLabel = GetNodeOrNull<Label>($"{Root}/HBoxContainer/UtilityRow/ContentPanel/MarginContainer/ContentStack/OptionsPanel/OptionsVBox/OptionsStatusLabel");
	}

	private void ConnectSignals()
	{
		if (_galaxyButton != null) _galaxyButton.Connect(Button.SignalName.Pressed, Callable.From(OnGalaxyButtonPressed));
		if (_systemButton != null) _systemButton.Connect(Button.SignalName.Pressed, Callable.From(OnSystemButtonPressed));
		if (_objectButton != null) _objectButton.Connect(Button.SignalName.Pressed, Callable.From(OnObjectButtonPressed));
		if (_stationButton != null) _stationButton.Connect(Button.SignalName.Pressed, Callable.From(OnStationButtonPressed));
		if (_conceptAtlasButton != null) _conceptAtlasButton.Connect(Button.SignalName.Pressed, Callable.From(OnConceptAtlasButtonPressed));
		if (_helpButton != null) _helpButton.Connect(Button.SignalName.Pressed, Callable.From(OnHelpButtonPressed));
		if (_creditsButton != null) _creditsButton.Connect(Button.SignalName.Pressed, Callable.From(OnCreditsButtonPressed));
		if (_sourcesButton != null) _sourcesButton.Connect(Button.SignalName.Pressed, Callable.From(OnSourcesButtonPressed));
		if (_releaseNotesButton != null) _releaseNotesButton.Connect(Button.SignalName.Pressed, Callable.From(OnReleaseNotesButtonPressed));
		if (_optionsButton != null) _optionsButton.Connect(Button.SignalName.Pressed, Callable.From(OnOptionsButtonPressed));
		if (_quitButton != null) _quitButton.Connect(Button.SignalName.Pressed, Callable.From(OnQuitButtonPressed));
		if (_applyOptionsButton != null) _applyOptionsButton.Connect(Button.SignalName.Pressed, Callable.From(ApplyWindowSettings));
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
				"- Galaxy Studio: Configure galaxy shape, generation rules, and worldbuilding assumptions before generating the galaxy viewer.\n\n" +
				"- System Studio: Set stellar counts, seed, and worldbuilding assumptions before opening the system viewer.\n\n" +
				"- Object Studio: Choose a star, planet, moon, or asteroid preset before launching the object viewer.\n\n" +
				"- Station Studio: Configure an individual station concept and review the current station workflow.\n\n" +
				"- Concept Atlas: Explore ecology, civilisation, language, religion, disease, and evolution layers as a worldbuilding tool in development.\n\n" +
				"- Sources: Review the astronomy and worldbuilding references currently guiding the generator's assumptions.";
		}

		if (_creditsText != null)
		{
			_creditsText.Text =
				"Credits\n\n" +
				"Design and direction: Jazhikho\n\n" +
				"AI assistance: OpenAI Codex / GPT models, Anthropic Claude, and Cursor were used under human direction for exploration, drafting, refactoring, testing support, UI copy iteration, documentation/provenance upkeep, and focused implementation assistance. Human review remained responsible for design, realism, licensing, and release decisions.\n\n" +
				"App icon: Galaxy icon by Freepik via Flaticon, used with attribution.\n\n" +
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

	private void OnConceptAtlasButtonPressed()
	{
		EmitSignal(SignalName.concept_atlas_requested);
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
			"Version 0.8.0.0\n\n" +
			"- Reworked Galaxy Studio around clearer shape controls, separated generation rules, and a stronger active-profile summary.\n" +
			"- Life Potential and Settlement Density now change generation behavior instead of acting like decorative labels.\n" +
			"- Added in-app source notes for the astronomy references guiding galaxy morphology and related assumptions.\n\n" +
			"Version 0.7.0.0\n\n" +
			"- Added the Concept Atlas as a standalone tool for ecology, civilisation, language, religion, disease, and evolution exploration.\n" +
			"- Added context-aware Atlas launch points from viewer inspection surfaces.\n\n" +
			"Version 0.6.0.0\n\n" +
			"- Brought the first showcase set of concept tools into StarGen.\n" +
			"- Framed the atlas as a worldbuilding tool in development rather than a final simulation layer.\n\n" +
			"Version 0.5.0.0\n\n" +
			"- Expanded the current studio lineup with galaxy, system, object, and station entry points.\n" +
			"- Added Traveller-aligned launch settings, world-profile readouts, and broad navigation polish.\n" +
			"- Introduced the station studio entry point and deterministic station-design support.";
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
		EnsureInfoDialog();
		if (_infoDialog == null || _infoDialogText == null)
		{
			return;
		}

		_infoDialog.Title = title;
		_infoDialogText.Text = body;
		_infoDialog.PopupCentered(new Vector2I(720, 520));
	}

	private void EnsureInfoDialog()
	{
		if (_infoDialog != null)
		{
			return;
		}

		Window dialog = new();
		dialog.Name = "InfoDialog";
		dialog.Title = "Information";
		dialog.MinSize = new Vector2I(520, 360);
		dialog.Transient = true;
		dialog.Exclusive = true;

		MarginContainer margin = new();
		margin.AnchorRight = 1.0f;
		margin.AnchorBottom = 1.0f;
		margin.OffsetLeft = 0.0f;
		margin.OffsetTop = 0.0f;
		margin.OffsetRight = 0.0f;
		margin.OffsetBottom = 0.0f;
		margin.AddThemeConstantOverride("margin_left", 16);
		margin.AddThemeConstantOverride("margin_top", 16);
		margin.AddThemeConstantOverride("margin_right", 16);
		margin.AddThemeConstantOverride("margin_bottom", 16);

		VBoxContainer vbox = new();
		vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		vbox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		vbox.AddThemeConstantOverride("separation", 12);

		RichTextLabel text = new();
		text.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		text.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		text.FitContent = false;
		text.ScrollActive = true;
		text.SelectionEnabled = true;
		text.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		text.CustomMinimumSize = new Vector2(360.0f, 0.0f);

		Button closeButton = new();
		closeButton.Text = "Close";
		closeButton.Pressed += dialog.Hide;

		vbox.AddChild(text);
		vbox.AddChild(closeButton);
		margin.AddChild(vbox);
		dialog.AddChild(margin);
		AddChild(dialog);

		_infoDialog = dialog;
		_infoDialogText = text;
	}

	private void ShowOptionsDialog()
	{
		EnsureOptionsDialog();
		RefreshOptionsState();
		if (_optionsDialog != null)
		{
			_optionsDialog.PopupCentered(new Vector2I(520, 320));
		}
	}

	private void EnsureOptionsDialog()
	{
		if (_optionsDialog != null)
		{
			return;
		}

		Window dialog = new();
		dialog.Name = "OptionsDialog";
		dialog.Title = "Options";
		dialog.MinSize = new Vector2I(460, 260);
		dialog.Transient = true;
		dialog.Exclusive = true;

		MarginContainer margin = new();
		margin.AnchorRight = 1.0f;
		margin.AnchorBottom = 1.0f;
		margin.OffsetLeft = 0.0f;
		margin.OffsetTop = 0.0f;
		margin.OffsetRight = 0.0f;
		margin.OffsetBottom = 0.0f;
		margin.AddThemeConstantOverride("margin_left", 16);
		margin.AddThemeConstantOverride("margin_top", 16);
		margin.AddThemeConstantOverride("margin_right", 16);
		margin.AddThemeConstantOverride("margin_bottom", 16);

		VBoxContainer optionsVBox = new();
		optionsVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		optionsVBox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		optionsVBox.AddThemeConstantOverride("separation", 10);

		CheckButton fullscreenCheck = new();
		fullscreenCheck.Text = "Fullscreen";

		CheckButton showSeedControlsCheck = new();
		showSeedControlsCheck.Text = "Show studio seed controls";

		HBoxContainer resolutionRow = new();
		resolutionRow.AddThemeConstantOverride("separation", 10);

		Label resolutionLabel = new();
		resolutionLabel.Text = "Resolution";
		resolutionLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

		OptionButton resolutionOption = new();
		resolutionOption.CustomMinimumSize = new Vector2(180.0f, 0.0f);

		Button applyButton = new();
		applyButton.Text = "Apply Display Settings";

		Label statusLabel = new();
		statusLabel.AutowrapMode = TextServer.AutowrapMode.Word;
		statusLabel.CustomMinimumSize = new Vector2(220.0f, 0.0f);

		Button closeButton = new();
		closeButton.Text = "Close";
		closeButton.Pressed += dialog.Hide;

		resolutionRow.AddChild(resolutionLabel);
		resolutionRow.AddChild(resolutionOption);
		optionsVBox.AddChild(fullscreenCheck);
		optionsVBox.AddChild(showSeedControlsCheck);
		optionsVBox.AddChild(resolutionRow);
		optionsVBox.AddChild(applyButton);
		optionsVBox.AddChild(statusLabel);
		optionsVBox.AddChild(closeButton);
		margin.AddChild(optionsVBox);
		dialog.AddChild(margin);
		AddChild(dialog);

		_optionsDialog = dialog;
		_fullscreenCheck = fullscreenCheck;
		_showSeedControlsCheck = showSeedControlsCheck;
		_resolutionOption = resolutionOption;
		_applyOptionsButton = applyButton;
		_optionsStatusLabel = statusLabel;

		PopulateResolutionOptions();
		if (_applyOptionsButton != null)
		{
			_applyOptionsButton.Pressed += ApplyWindowSettings;
		}

		if (_fullscreenCheck != null)
		{
			_fullscreenCheck.Toggled += enabled =>
			{
				if (_resolutionOption != null)
				{
					_resolutionOption.Disabled = enabled;
				}
			};
		}
	}

	private static string BuildHelpFallbackText()
	{
		return
			"How to use StarGen\n\n" +
			"- Galaxy Studio: Configure galaxy shape, generation rules, and worldbuilding assumptions before generating the galaxy viewer.\n\n" +
			"- System Studio: Set stellar counts, seed, and worldbuilding assumptions before opening the system viewer.\n\n" +
			"- Object Studio: Choose a star, planet, moon, or asteroid preset before launching the object viewer.\n\n" +
			"- Station Studio: Configure an individual station concept and review the current station workflow.\n\n" +
			"- Concept Atlas: Explore ecology, civilisation, language, religion, disease, and evolution layers as a worldbuilding tool in development.\n\n" +
			"- Sources: Review the astronomy and worldbuilding references currently guiding the generator's assumptions.";
	}

	private static string BuildCreditsFallbackText()
	{
		return
			"Credits\n\n" +
			"Design and direction: Jazhikho\n\n" +
			"AI assistance: OpenAI Codex / GPT models, Anthropic Claude, and Cursor were used under human direction for exploration, drafting, refactoring, testing support, UI copy iteration, documentation/provenance upkeep, and focused implementation assistance. Human review remained responsible for design, realism, licensing, and release decisions.\n\n" +
			"App icon: Galaxy icon by Freepik via Flaticon, used with attribution.\n\n" +
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
			if (_optionsStatusLabel != null)
			{
				_optionsStatusLabel.Text = "Applied fullscreen mode";
			}

			return;
		}

		Vector2I resolution = GetSelectedResolution();
		WindowSettingsService.ApplyAndSave(new WindowSettingsService.WindowSettingsState(false, resolution));
		ApplyStudioPreferences();
		if (_optionsStatusLabel != null)
		{
			_optionsStatusLabel.Text = $"Applied windowed mode at {resolution.X} x {resolution.Y}";
		}
	}

	private void ApplyStudioPreferences()
	{
		bool showSeedControls = false;
		if (_showSeedControlsCheck != null)
		{
			showSeedControls = _showSeedControlsCheck.ButtonPressed;
		}

		StudioUiPreferencesService.Save(new StudioUiPreferencesService.StudioUiPreferences(showSeedControls));
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
