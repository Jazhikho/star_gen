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
			string version = ProjectSettings.GetSetting("application/config/version", "0.7.0.0").AsString();
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
				"- Object Studio: Choose a star, planet, moon, or asteroid preset before launching the object viewer.\n\n" +
				"- Station Studio: Open the in-progress station tool to configure individualized space stations.\n\n" +
				"- Concept Atlas: Open the standalone concept tool in development from the main menu or inspector surfaces to explore ecology, civilisation, language, religion, disease, and evolution layers. The long-term goal is realistic, user-adjustable worldbuilding models grounded in the same seed-driven context.";
		}

		if (_creditsText != null)
		{
			_creditsText.Text =
				"Credits\n\n" +
				"Design and direction: Jazhikho\n\n" +
				"AI assistance: OpenAI Codex / GPT models and Anthropic Claude were used under human direction for exploration, drafting, refactoring, testing support, UI copy iteration, and documentation/provenance upkeep. Human review remained responsible for design, realism, licensing, and release decisions.\n\n" +
				"App icon (Galaxy): Freepik (Flaticon). Used under Flaticon License; attribution required. See Docs/Assets.md.\n\n" +
				"StarGen uses astronomy and worldbuilding references for its generation parameters. See the project's Sources folder for further reading.";
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
				"Version 0.7.0.0\n\n" +
			"- Current showcase branch milestone: the user-facing `0.7.0.0` label remains in place while the concept tools stay presented as a standalone atlas in development.\n" +
			"- Refined the showcase presentation with a Generation Studios entry for Concept Atlas, clearer help and credits copy, sentence-case concept display text, and scroll-safe atlas navigation.\n" +
			"- Broader simulation and persistence integration for concept layers is deferred until applicability rules, realism controls, and tuning are ready.\n" +
			"- Cultural, religious, language, civilisation, and species-facing outputs remain subject to explicit human audit before public release sign-off.\n\n" +
				"Version 0.6.1.0\n\n" +
			"- Post-review showcase patch cycle: removed automatic concept generation from the normal generation, save/load, preview, and viewer pipelines so the Concept Atlas remains a standalone feature for now.\n" +
			"- Replaced the system-view `Concept Layers` overview with populated-world shortcuts that jump the current selection and camera.\n" +
			"- Keeps the atlas context-aware and sandbox-friendly while avoiding misleading main-pipeline concept coverage on worlds that should not yet generate those layers.\n" +
			"- Cultural, religious, language, civilisation, and species-facing outputs remain subject to explicit human audit before public release sign-off.\n\n" +
				"Version 0.6.0.0\n\n" +
			"- Release 1 showcase milestone: every selected concept prototype is now accessible inside StarGen through the Concept Atlas and relevant viewer inspection surfaces.\n" +
			"- Keeps the digital-humanities framing subtle in-app while making the atlas a reliable demonstration surface for ecology, civilisation, language, religion, disease, and evolution layers.\n" +
			"- Marks the atlas as a tool in development, intended to grow into realistic, user-adjustable worldbuilding models.\n" +
			"- Cultural, religious, language, civilisation, and species-facing outputs remain subject to explicit human audit before public release sign-off.\n\n" +
				"Version 0.5.8.0\n\n" +
			"- Added Concept Atlas launch points from galaxy, system, and object inspection surfaces so showcase visitors can move directly from generated worlds into the concept layers.\n" +
			"- Added context-aware atlas return navigation so the atlas sends users back to the originating viewer instead of always resetting to the main menu.\n" +
			"- Added regression coverage for inspector-driven atlas launch paths across the main app and the viewer surfaces.\n\n" +
				"Version 0.5.7.0\n\n" +
			"- Folded the evolution concept into the Concept Atlas with deterministic lineage, trait, and species-profile generation driven by environmental pressures.\n" +
			"- Added atlas-specific deterministic regression coverage for the evolution presenter.\n\n" +
				"Version 0.5.6.0\n\n" +
			"- Folded the disease concept into the Concept Atlas with deterministic outbreak traits, symptom bundles, and epidemic summary metrics derived from world and population context.\n" +
			"- Added atlas-specific deterministic regression coverage for the disease presenter.\n\n" +
				"Version 0.5.5.0\n\n" +
			"- Folded the language concept into the Concept Atlas with deterministic phonology, grammar, lexicon, and sample utterance generation for showcase-ready cultural presentation.\n" +
			"- Added atlas-specific deterministic regression coverage for the language presenter.\n\n" +
				"Version 0.5.4.0\n\n" +
			"- Folded the civilisation concept into the Concept Atlas with deterministic polity, economy, culture, and timeline summaries seeded from StarGen population context.\n" +
			"- Added atlas-specific deterministic regression coverage for the civilisation presenter.\n\n" +
				"Version 0.5.3.0\n\n" +
			"- Folded the religion concept into the Concept Atlas with deterministic belief-system generation, doctrine and landscape summaries, and context-seeded atlas readouts.\n" +
			"- Added atlas-specific deterministic regression coverage for the religion presenter.\n\n" +
				"Version 0.5.2.0\n\n" +
			"- Folded the ecology concept into the Concept Atlas with deterministic environment-to-food-web generation, trophic metrics, and highlighted niche summaries.\n" +
			"- Added atlas-specific concept regression coverage for the ecology presenter.\n\n" +
				"Version 0.5.1.0\n\n" +
			"- Added the first Concept Atlas shell, including shared concept context/provenance plumbing, a manual sandbox input surface, and a main-menu entry point for the concept fold-in work.\n" +
			"- Added initial regression coverage for the new Concept Atlas menu and navigation path.\n\n" +
				"Version 0.5.0.0\n\n" +
			"- First public release since 0.3.0, rolling up the internal 0.4.x work into a single release build.\n" +
			"- Adds config-first galaxy, system, and object studios; Traveller-aligned launch settings and UWP/world-profile readouts; and broad UI/navigation polish.\n" +
			"- Folds detailed station design into the main population framework with deterministic presets, classification, persistence, export, regression fixtures, and a Station Studio entry point marked as in-progress.\n\n" +
				"Version 0.4.3.5\n\n" +
			"- Fixed the main-menu scene/script mismatch after the recent layout rewrite so the studio buttons and utility actions work again.\n" +
			"- Added direct main-menu integration coverage for mode-button signal wiring and fallback utility dialogs.\n\n" +
				"Version 0.4.3.4\n\n" +
			"- Reformatted the AI provenance log into readable sectioned entries, documented the 640 px minimum supported UI width, and aligned wrap-ready controls with sensible minimum widths.\n" +
			"- Fixed object-viewer main-menu return routing after recent menu changes and updated the stale system-viewer Traveller-controls regression to match the current slider UI.\n" +
			"- Added wrap minimum sizes across the main menu, splash, studios, station placeholder, object viewer file info, and edit-dialog validation labels so wrapped text stays readable instead of stretching layouts.\n\n" +
				"Version 0.4.3.3\n\n" +
			"- Removed the studio launch-summary clutter, tightened studio chrome at smaller window sizes, and kept launch actions reachable in the footer.\n" +
			"- Hid studio seeds behind the Options preference, rerolled hidden seeds on fresh launches, and exposed the placeholder Station Studio entry point on the main menu.\n" +
			"- Kept the galaxy viewer on a single `New Galaxy...` path and marked windowed-resolution behavior for post-release build verification.\n\n" +
			"Version 0.4.3.2\n\n" +
			"- Moved viewer menus below the header row, removed duplicate header-level return affordances from the object/system viewers, and kept return navigation menu-scoped.\n" +
			"- Fixed Traveller object-generation edge cases so fully auto Traveller worlds avoid the blank all-zero profile, optional feature controls read `Auto / Yes / No`, and viewer summaries show `None` instead of `Forbidden`.\n" +
			"- Renamed permissiveness controls to `Life Potential` and `Settlement Density`, applied Traveller-leaning defaults across galaxy/system/object flows, and added regression coverage for the updated navigation and Traveller launch paths.\n\n" +
			"Version 0.4.3.1\n\n" +
			"- Reworked the launch studio layouts to stack responsively on narrower windows and added summary-panel scrolling.\n" +
			"- Moved more explanatory copy into tooltips, trimmed oversized labels, and reduced fixed chrome across the menu and splash screens.\n" +
			"- Added shared studio-layout regression coverage so the responsive structure stays intact.\n\n" +
			"Version 0.4.3.0\n\n" +
			"- Reworked object generation into an explicit spec builder, added Traveller planet profile generation/UWP output, and moved UWP world-profile readouts to the top of the inspector.\n\n" +
			"Version 0.4.2.3\n\n" +
			"- Fixed window and fullscreen settings to apply to the active root window immediately.\n" +
			"- Relaxed the galaxy studio split layout so the parameter panel has more breathing room at typical desktop widths.\n" +
			"- Added direct window-application regression coverage for windowed and fullscreen mode changes.\n\n" +
			"Version 0.4.2.2\n\n" +
			"- Moved galaxy parameter editing fully into the Galaxy Generation Studio.\n" +
			"- Converted the galaxy viewer profile area into a read-only active-profile summary and added a dedicated main-menu return action.\n" +
			"- Expanded the object viewer inspector to surface richer world, orbit, population, and Traveller context already present in generated bodies.\n\n" +
			"Version 0.4.2.1\n\n" +
			"- Fixed compact station-design reloads to preserve non-default design spec fields.\n" +
			"- Fixed detailed hull-band sizing to use the generated station class.\n" +
			"- Added regression coverage for legacy scalar reloads and explicit small-class hull mapping.\n\n" +
			"Version 0.4.2.0\n\n" +
			"- Folded detailed station design into the main station framework.\n" +
			"- Added deterministic classification, persistence, export, and regression coverage for station designs.\n" +
			"- Retired the SpaceStationBuilder prototype and synced roadmap and project structure docs.\n\n" +
			"Version 0.4.1.1\n\n" +
			"- Added a regression test for galaxy star snapshots surviving garbage collection.\n" +
			"- Consolidated the 0.4.0 MVP scope and Traveller integration notes into the active docs set.\n\n" +
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
			"- Galaxy Studio: Configure a galaxy profile first, then open the galaxy viewer to explore sectors and star systems.\n\n" +
			"- System Studio: Set stellar counts, seed, and Traveller assumptions before opening the system viewer.\n\n" +
			"- Object Studio: Choose a star, planet, moon, or asteroid preset before launching the object viewer.\n\n" +
			"- Station Studio: Open the in-progress station tool to configure individualized space stations.\n\n" +
			"- Concept Atlas: Open the standalone concept tool in development from the main menu or inspector surfaces to explore ecology, civilisation, language, religion, disease, and evolution layers. The long-term goal is realistic, user-adjustable worldbuilding models grounded in the same seed-driven context.";
	}

	private static string BuildCreditsFallbackText()
	{
		return
			"Credits\n\n" +
			"Design and direction: Jazhikho\n\n" +
			"AI assistance: OpenAI Codex / GPT models and Anthropic Claude were used under human direction for exploration, drafting, refactoring, testing support, UI copy iteration, and documentation/provenance upkeep. Human review remained responsible for design, realism, licensing, and release decisions.\n\n" +
			"App icon (Galaxy): Freepik (Flaticon). Used under Flaticon License; attribution required. See Docs/Assets.md.\n\n" +
			"StarGen uses astronomy and worldbuilding references for its generation parameters. See the project's Sources folder for further reading.";
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
