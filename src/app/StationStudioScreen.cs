using System.Collections.Generic;
using Godot;
using StarGen.App.Shared;
using StarGen.Domain.Population;
using StarGen.Domain.Population.StationDesign;
using StarGen.Domain.Rng;

namespace StarGen.App;

/// <summary>
/// Production station studio that exposes single-station generation controls and RPG presentation options.
/// </summary>
public partial class StationStudioScreen : Control
{
	[Signal]
	public delegate void back_requestedEventHandler();

	private BoxContainer? _studioRow;
	private Control? _settingsPanel;
	private Control? _rpgPanel;
	private Control? _resultPanel;
	private OptionButton? _contextOption;
	private OptionButton? _purposeOption;
	private OptionButton? _classOption;
	private OptionButton? _locationOption;
	private OptionButton? _populationModeOption;
	private SpinBox? _seedSpinbox;
	private Button? _randomSeedButton;
	private HSlider? _densitySlider;
	private Label? _densityLabel;
	private OptionButton? _rpgSystemOption;
	private OptionButton? _rpgReadoutOption;
	private RichTextLabel? _rpgOverrideLabel;
	private Button? _generateButton;
	private Button? _backButton;
	private Button? _helpButton;
	private Window? _helpDialog;
	private RichTextLabel? _helpDialogText;
	private Button? _helpDialogCloseButton;
	private Label? _versionLabel;
	private RichTextLabel? _systemInfoLabel;

	private StationGenerationResult? _currentResult;
	private SpaceStation? _currentStation;
	private GenerationChoices? _currentChoices;

	/// <summary>
	/// Initializes controls without generating a station.
	/// </summary>
	public override void _Ready()
	{
		CacheNodes();
		SetupUi();
		ConnectSignals();
		ApplyResponsiveLayout();
		UpdateRpgOverridePanel();
		ShowInitialSystemInfo();
	}

	/// <summary>
	/// Keeps the studio layout responsive when the app window changes size.
	/// </summary>
	public override void _Notification(int what)
	{
		if (what == NotificationResized)
		{
			ApplyResponsiveLayout();
		}
	}

	private void CacheNodes()
	{
		_studioRow = GetNodeOrNull<BoxContainer>("%StudioRow");
		_settingsPanel = GetNodeOrNull<Control>("%SettingsPanel");
		_rpgPanel = GetNodeOrNull<Control>("%RpgPanel");
		_resultPanel = GetNodeOrNull<Control>("%ResultPanel");
		_contextOption = GetNodeOrNull<OptionButton>("%ContextOption");
		_purposeOption = GetNodeOrNull<OptionButton>("%PurposeOption");
		_classOption = GetNodeOrNull<OptionButton>("%ClassOption");
		_locationOption = GetNodeOrNull<OptionButton>("%LocationOption");
		_populationModeOption = GetNodeOrNull<OptionButton>("%PopulationModeOption");
		_seedSpinbox = GetNodeOrNull<SpinBox>("%SeedSpinBox");
		_randomSeedButton = GetNodeOrNull<Button>("%RandomSeedButton");
		_densitySlider = GetNodeOrNull<HSlider>("%DensitySlider");
		_densityLabel = GetNodeOrNull<Label>("%DensityLabel");
		_rpgSystemOption = GetNodeOrNull<OptionButton>("%RpgSystemOption");
		_rpgReadoutOption = GetNodeOrNull<OptionButton>("%RpgReadoutOption");
		_rpgOverrideLabel = GetNodeOrNull<RichTextLabel>("%RpgOverrideLabel");
		_generateButton = GetNodeOrNull<Button>("%GenerateButton");
		_backButton = GetNodeOrNull<Button>("%BackButton");
		_helpButton = GetNodeOrNull<Button>("%HelpButton");
		_helpDialog = GetNodeOrNull<Window>("%HelpDialog");
		_helpDialogText = GetNodeOrNull<RichTextLabel>("%HelpDialogText");
		_helpDialogCloseButton = GetNodeOrNull<Button>("%CloseButton");
		_versionLabel = GetNodeOrNull<Label>("%VersionLabel");
		_systemInfoLabel = GetNodeOrNull<RichTextLabel>("%SystemInfoLabel");
	}

	private void SetupUi()
	{
		if (_versionLabel != null)
		{
			_versionLabel.Text = $"Version {UserFacingVersionHelper.GetDisplayVersion()}";
		}

		PopulateContextOptions();
		PopulatePurposeOptions();
		PopulateClassOptions();
		PopulateLocationOptions();
		PopulatePopulationModeOptions();
		PopulateRpgOptions();

		if (_seedSpinbox != null)
		{
			_seedSpinbox.Value = GD.Randi() % 100000;
		}

		if (_helpDialog != null)
		{
			_helpDialog.Visible = false;
		}

		if (_helpDialogText != null)
		{
			_helpDialogText.Text = BuildHelpText();
		}

		UpdateDensityLabel();
	}

	private void ConnectSignals()
	{
		if (_generateButton != null)
		{
			_generateButton.Pressed += Generate;
		}
		if (_randomSeedButton != null)
		{
			_randomSeedButton.Pressed += OnRandomizeSeed;
		}
		if (_densitySlider != null)
		{
			_densitySlider.ValueChanged += _ => UpdateDensityLabel();
		}
		if (_rpgSystemOption != null)
		{
			_rpgSystemOption.ItemSelected += _ => OnRpgOptionsChanged();
		}
		if (_rpgReadoutOption != null)
		{
			_rpgReadoutOption.ItemSelected += _ => OnRpgOptionsChanged();
		}
		if (_backButton != null)
		{
			_backButton.Pressed += () => EmitSignal(SignalName.back_requested);
		}
		if (_helpButton != null)
		{
			_helpButton.Pressed += ShowHelpDialog;
		}
		if (_helpDialogCloseButton != null)
		{
			_helpDialogCloseButton.Pressed += HideHelpDialog;
		}
		if (_helpDialog != null)
		{
			_helpDialog.CloseRequested += HideHelpDialog;
		}
	}

	private void PopulateContextOptions()
	{
		if (_contextOption == null)
		{
			return;
		}

		_contextOption.Clear();
		_contextOption.AddItem("Random", -1);
		_contextOption.AddItem("Trade highport", (int)StationPlacementContext.Context.ColonyWorld);
		_contextOption.AddItem("Waystation", (int)StationPlacementContext.Context.BridgeSystem);
		_contextOption.AddItem("Resource platform", (int)StationPlacementContext.Context.ResourceSystem);
		_contextOption.AddItem("Strategic defense", (int)StationPlacementContext.Context.Strategic);
		_contextOption.AddItem("Research station", (int)StationPlacementContext.Context.Scientific);
		_contextOption.AddItem("Native contact station", (int)StationPlacementContext.Context.NativeWorld);
		_contextOption.AddItem("Free-space station", (int)StationPlacementContext.Context.Other);
		_contextOption.Select(0);
	}

	private void PopulatePurposeOptions()
	{
		if (_purposeOption == null)
		{
			return;
		}

		_purposeOption.Clear();
		_purposeOption.AddItem("Random", -1);
		_purposeOption.AddItem("Trade", (int)StationPurpose.Purpose.Trade);
		_purposeOption.AddItem("Residential", (int)StationPurpose.Purpose.Residential);
		_purposeOption.AddItem("Industrial", (int)StationPurpose.Purpose.Industrial);
		_purposeOption.AddItem("Military", (int)StationPurpose.Purpose.Military);
		_purposeOption.AddItem("Science", (int)StationPurpose.Purpose.Science);
		_purposeOption.AddItem("Mining", (int)StationPurpose.Purpose.Mining);
		_purposeOption.AddItem("Utility", (int)StationPurpose.Purpose.Utility);
		_purposeOption.AddItem("Administrative", (int)StationPurpose.Purpose.Administrative);
		_purposeOption.AddItem("Medical", (int)StationPurpose.Purpose.Medical);
		_purposeOption.AddItem("Communications", (int)StationPurpose.Purpose.Communications);
		_purposeOption.Select(0);
	}

	private void PopulateClassOptions()
	{
		if (_classOption == null)
		{
			return;
		}

		_classOption.Clear();
		_classOption.AddItem("Random", -1);
		_classOption.AddItem("Utility", (int)StationClass.Class.U);
		_classOption.AddItem("Outpost", (int)StationClass.Class.O);
		_classOption.AddItem("Base", (int)StationClass.Class.B);
		_classOption.AddItem("Anchor", (int)StationClass.Class.A);
		_classOption.AddItem("Super", (int)StationClass.Class.S);
		_classOption.Select(0);
	}

	private void PopulateLocationOptions()
	{
		if (_locationOption == null)
		{
			return;
		}

		_locationOption.Clear();
		_locationOption.AddItem("Random", -1);
		_locationOption.AddItem("Orbital", (int)StationType.Type.Orbital);
		_locationOption.AddItem("Deep Space", (int)StationType.Type.DeepSpace);
		_locationOption.AddItem("Lagrange Point", (int)StationType.Type.Lagrange);
		_locationOption.AddItem("Asteroid Belt", (int)StationType.Type.AsteroidBelt);
		_locationOption.Select(0);
	}

	private void PopulatePopulationModeOptions()
	{
		if (_populationModeOption == null)
		{
			return;
		}

		_populationModeOption.Clear();
		_populationModeOption.AddItem("Random", -1);
		_populationModeOption.AddItem("Use scale slider", 0);
		_populationModeOption.Select(0);
	}

	private void PopulateRpgOptions()
	{
		if (_rpgSystemOption != null)
		{
			_rpgSystemOption.Clear();
			_rpgSystemOption.AddItem("Traveller-compatible", 0);
			_rpgSystemOption.AddItem("Other RPG (WIP)", 1);
			_rpgSystemOption.Select(0);
		}

		if (_rpgReadoutOption != null)
		{
			_rpgReadoutOption.Clear();
			_rpgReadoutOption.AddItem("Station facts only", 0);
			_rpgReadoutOption.AddItem("Construction readout", 1);
			_rpgReadoutOption.Select(1);
		}
	}

	private void ApplyResponsiveLayout()
	{
		StudioScreenLayoutHelper.ApplyResponsiveStudioLayout(
			this,
			_studioRow,
			_settingsPanel,
			_rpgPanel,
			_resultPanel);
	}

	private void OnRandomizeSeed()
	{
		if (_seedSpinbox != null)
		{
			_seedSpinbox.Value = GD.Randi() % 100000;
		}
	}

	private void UpdateDensityLabel()
	{
		if (_densityLabel != null && _densitySlider != null)
		{
			_densityLabel.Text = $"{_densitySlider.Value:0.0}x";
		}
	}

	private void Generate()
	{
		int seed = GetSeed();
		SeededRng choiceRng = new(seed + 7919);
		GenerationChoices choices = ResolveChoices(choiceRng);
		StationSystemContext context = BuildContext(choices);
		StationSpec spec = BuildSpec(seed, choices);
		_currentResult = StationGenerator.Generate(context, spec, new SeededRng(spec.GenerationSeed));
		_currentStation = ResolveGeneratedStation(_currentResult);
		_currentChoices = choices;
		UpdateRpgOverridePanel();
		UpdateSystemInfo();
	}

	private int GetSeed()
	{
		if (_seedSpinbox == null)
		{
			return 0;
		}

		return (int)_seedSpinbox.Value;
	}

	private GenerationChoices ResolveChoices(SeededRng rng)
	{
		GenerationChoices choices = new();
		choices.Context = ResolveContextChoice(rng);
		choices.Purpose = ResolvePurposeChoice(rng);
		choices.StationClass = ResolveClassChoice(rng);
		choices.StationType = ResolveLocationChoice(rng);
		choices.PopulationDensity = ResolvePopulationDensity(rng);
		return choices;
	}

	private StationPlacementContext.Context ResolveContextChoice(SeededRng rng)
	{
		if (_contextOption != null && _contextOption.GetSelectedId() >= 0)
		{
			return (StationPlacementContext.Context)_contextOption.GetSelectedId();
		}

		StationPlacementContext.Context[] options =
		{
			StationPlacementContext.Context.ColonyWorld,
			StationPlacementContext.Context.BridgeSystem,
			StationPlacementContext.Context.ResourceSystem,
			StationPlacementContext.Context.Strategic,
			StationPlacementContext.Context.Scientific,
			StationPlacementContext.Context.NativeWorld,
			StationPlacementContext.Context.Other,
		};
		return options[rng.RandiRange(0, options.Length - 1)];
	}

	private StationPurpose.Purpose ResolvePurposeChoice(SeededRng rng)
	{
		if (_purposeOption != null && _purposeOption.GetSelectedId() >= 0)
		{
			return (StationPurpose.Purpose)_purposeOption.GetSelectedId();
		}

		StationPurpose.Purpose[] options =
		{
			StationPurpose.Purpose.Trade,
			StationPurpose.Purpose.Residential,
			StationPurpose.Purpose.Industrial,
			StationPurpose.Purpose.Military,
			StationPurpose.Purpose.Science,
			StationPurpose.Purpose.Mining,
			StationPurpose.Purpose.Utility,
			StationPurpose.Purpose.Administrative,
			StationPurpose.Purpose.Medical,
			StationPurpose.Purpose.Communications,
		};
		return options[rng.RandiRange(0, options.Length - 1)];
	}

	private StationClass.Class ResolveClassChoice(SeededRng rng)
	{
		if (_classOption != null && _classOption.GetSelectedId() >= 0)
		{
			return (StationClass.Class)_classOption.GetSelectedId();
		}

		StationClass.Class[] options =
		{
			StationClass.Class.U,
			StationClass.Class.O,
			StationClass.Class.B,
			StationClass.Class.A,
			StationClass.Class.S,
		};
		return options[rng.RandiRange(0, options.Length - 1)];
	}

	private StationType.Type ResolveLocationChoice(SeededRng rng)
	{
		if (_locationOption != null && _locationOption.GetSelectedId() >= 0)
		{
			return (StationType.Type)_locationOption.GetSelectedId();
		}

		StationType.Type[] options =
		{
			StationType.Type.Orbital,
			StationType.Type.DeepSpace,
			StationType.Type.Lagrange,
			StationType.Type.AsteroidBelt,
		};
		return options[rng.RandiRange(0, options.Length - 1)];
	}

	private double ResolvePopulationDensity(SeededRng rng)
	{
		if (_populationModeOption != null && _populationModeOption.GetSelectedId() == 0)
		{
			if (_densitySlider != null)
			{
				return _densitySlider.Value;
			}

			return 1.0;
		}

		return rng.RandfRange(0.5f, 2.7f);
	}

	private StationSystemContext BuildContext(GenerationChoices choices)
	{
		StationSystemContext context = new();
		context.SystemId = "station_studio_system";

		if (choices.Context == StationPlacementContext.Context.BridgeSystem)
		{
			context.IsBridgeSystem = true;
			context.PlanetIds.Add("route_marker_001");
		}
		else if (choices.Context == StationPlacementContext.Context.ColonyWorld)
		{
			context.ColonyWorldCount = 1;
			context.HabitablePlanetCount = 1;
			context.ColonyPlanetIds.Add("mainworld_001");
			context.PlanetIds.Add("mainworld_001");
		}
		else if (choices.Context == StationPlacementContext.Context.NativeWorld)
		{
			context.NativeWorldCount = 1;
			context.NativePlanetIds.Add("native_world_001");
			context.PlanetIds.Add("native_world_001");
			context.HasSpacefaringNatives = true;
			context.HighestNativeTech = TechnologyLevel.Level.Spacefaring;
		}
		else if (choices.Context == StationPlacementContext.Context.ResourceSystem
			|| choices.StationType == StationType.Type.AsteroidBelt)
		{
			context.ResourceRichness = 0.7;
			context.AsteroidBeltCount = 1;
			context.ResourceBodyIds.Add("belt_cluster_001");
		}
		else
		{
			context.ResourceRichness = 0.3;
			context.PlanetIds.Add("navigation_body_001");
		}

		return context;
	}

	private StationSpec BuildSpec(int seed, GenerationChoices choices)
	{
		StationSpec spec = new();
		spec.GenerationSeed = seed;
		spec.MinStations = 1;
		spec.MaxStations = 1;
		spec.AllowUtility = false;
		spec.AllowOutposts = false;
		spec.AllowLargeStations = true;
		spec.AllowDeepSpace = true;
		spec.AllowBeltStations = true;
		spec.GenerateDetailedDesign = ShouldGenerateDetailedDesign();
		spec.ForceContext = choices.Context;
		spec.ForceStationClass = choices.StationClass;
		spec.ForceStationType = choices.StationType;
		spec.PopulationDensity = choices.PopulationDensity;
		spec.FoundingCivilizationId = "station_studio_civ";
		spec.FoundingCivilizationName = "Station Studio Civilization";
		spec.RequiredPurposes.Add(choices.Purpose);
		return spec;
	}

	private bool ShouldGenerateDetailedDesign()
	{
		return ShouldShowConstructionReadout();
	}

	private static SpaceStation? ResolveGeneratedStation(StationGenerationResult result)
	{
		if (result.Stations.Count > 0)
		{
			return result.Stations[0];
		}

		return null;
	}

	private void ShowInitialSystemInfo()
	{
		if (_systemInfoLabel != null)
		{
			_systemInfoLabel.Text = "Set the station inputs and RPG display options, then press Generate Station.";
		}
	}

	private void UpdateRpgOverridePanel()
	{
		if (_rpgOverrideLabel == null)
		{
			return;
		}

		List<string> lines = new();
		lines.Add("[b]RPG Overrides[/b]");
		lines.Add("Work in progress. The station is generated from RPG-agnostic inputs; this panel only changes compatibility presentation.");
		lines.Add(string.Empty);
		lines.Add($"[b]Selected System:[/b] {GetSelectedRpgSystemText()}");
		lines.Add($"[b]Readout:[/b] {GetSelectedRpgReadoutText()}");
		lines.Add(string.Empty);
		lines.Add("Current support: Traveller-compatible construction terms derived from StarGen's own station design model.");
		lines.Add("Other RPG systems are placeholders until rules, terminology, and licensing constraints are reviewed by a human.");

		if (_currentStation != null && _currentStation.DetailedDesign != null && ShouldShowConstructionReadout())
		{
			lines.Add(string.Empty);
			lines.Add("[b]Current Compatibility Summary:[/b]");
			lines.Add($"Template: {ComponentCatalog.Templates[_currentStation.DetailedDesign.Spec.Template].Name}");
			lines.Add($"Hull: {_currentStation.DetailedDesign.Spec.HullTons:N0} tons");
			lines.Add($"Crew: {_currentStation.DetailedDesign.Crew.Total:N0}");
			lines.Add($"Cost: {FormatCredits(_currentStation.DetailedDesign.Cost.Total)}");
		}

		_rpgOverrideLabel.Text = string.Join("\n", lines);
	}

	private string GetSelectedRpgSystemText()
	{
		if (_rpgSystemOption == null || _rpgSystemOption.GetSelectedId() == 0)
		{
			return "Traveller-compatible";
		}

		return "Other RPG (WIP)";
	}

	private string GetSelectedRpgReadoutText()
	{
		if (ShouldShowConstructionReadout())
		{
			return "Construction readout";
		}

		return "Station facts only";
	}

	private bool ShouldShowConstructionReadout()
	{
		if (_rpgReadoutOption == null)
		{
			return true;
		}

		if (_rpgReadoutOption.GetSelectedId() != 1)
		{
			return false;
		}

		if (_rpgSystemOption != null && _rpgSystemOption.GetSelectedId() != 0)
		{
			return false;
		}

		return true;
	}

	private void OnRpgOptionsChanged()
	{
		UpdateRpgOverridePanel();
		if (_currentStation != null)
		{
			UpdateSystemInfo();
		}
	}

	private void UpdateSystemInfo()
	{
		if (_systemInfoLabel == null)
		{
			return;
		}

		if (_currentStation == null || _currentChoices == null)
		{
			_systemInfoLabel.Text = "No station generated.";
			return;
		}

		List<string> lines = new();
		AppendResolvedInputs(lines, _currentChoices);
		AppendStationFacts(lines, _currentStation);
		AppendGovernance(lines, _currentStation);
		AppendServices(lines, _currentStation);
		AppendPopulationProfile(lines, _currentStation.GetSentientWorldProfile());
		if (ShouldShowConstructionReadout())
		{
			AppendDesignSummary(lines, _currentStation.DetailedDesign);
		}
		AppendWarnings(lines);

		_systemInfoLabel.Text = string.Join("\n", lines);
	}

	private static void AppendResolvedInputs(List<string> lines, GenerationChoices choices)
	{
		lines.Add("[b]Resolved Inputs[/b]");
		lines.Add($"Role: {StationPlacementContext.ToStringName(choices.Context)}");
		lines.Add($"Purpose: {StationPurpose.ToStringName(choices.Purpose)}");
		lines.Add($"Class: {StationClass.ToLetter(choices.StationClass)} - {StationClass.ToStringName(choices.StationClass)}");
		lines.Add($"Location: {StationType.ToStringName(choices.StationType)}");
		lines.Add($"Population Scale: {choices.PopulationDensity:0.00}x");
		lines.Add(string.Empty);
	}

	private static void AppendStationFacts(List<string> lines, SpaceStation station)
	{
		lines.Add("[b]Station[/b]");
		lines.Add($"{station.Name}");
		lines.Add($"ID: {station.Id}");
		lines.Add($"Class: {StationClass.ToLetter(station.StationClass)} - {StationClass.ToStringName(station.StationClass)}");
		lines.Add($"Purpose: {StationPurpose.ToStringName(station.PrimaryPurpose)}");
		lines.Add($"Context: {StationPlacementContext.ToStringName(station.PlacementContext)}");
		lines.Add($"Location: {StationType.ToStringName(station.StationType)}");
		if (!string.IsNullOrEmpty(station.OrbitingBodyId))
		{
			lines.Add($"Orbiting: {station.OrbitingBodyId}");
		}
		lines.Add($"Population: {FormatPopulation(station.Population)}");
		lines.Add($"Peak Population: {FormatPopulation(station.PeakPopulation)}");
		lines.Add($"Growth State: {FormatTitle(station.GetGrowthState())}");
		lines.Add($"Established: Year {station.EstablishedYear}");
		lines.Add($"Age: {station.GetAge()} years");
		lines.Add(string.Empty);
	}

	private static void AppendGovernance(List<string> lines, SpaceStation station)
	{
		lines.Add("[b]Governance[/b]");
		if (station.UsesOutpostGovernment())
		{
			lines.Add("Mode: Station Authority");
			lines.Add($"Authority: {OutpostAuthority.ToStringName(station.OutpostAuthority)}");
			lines.Add($"Commander: {station.CommanderTitle}");
			if (station.HasParentOrganization())
			{
				lines.Add($"Organization: {station.ParentOrganizationName}");
			}
		}
		else
		{
			lines.Add("Mode: Population Government");
			if (station.Government != null)
			{
				lines.Add($"Regime: {GovernmentType.ToStringName(station.Government.Regime)}");
				lines.Add($"Legitimacy: {station.Government.Legitimacy * 100:0}%");
				lines.Add($"Administrative Capacity: {station.Government.AdministrativeCapacity * 100:0}%");
			}
		}

		if (!string.IsNullOrEmpty(station.FoundingCivilizationName))
		{
			lines.Add($"Founded By: {station.FoundingCivilizationName}");
		}
		lines.Add(string.Empty);
	}

	private static void AppendServices(List<string> lines, SpaceStation station)
	{
		if (station.Services.Count <= 0)
		{
			return;
		}

		lines.Add($"[b]Services ({station.Services.Count})[/b]");
		foreach (StationService.Service service in station.Services)
		{
			lines.Add($"- {StationService.ToStringName(service)}");
		}
		lines.Add(string.Empty);
	}

	private static void AppendPopulationProfile(List<string> lines, SentientWorldProfile? profile)
	{
		if (profile == null)
		{
			return;
		}

		lines.Add("[b]Population Profile[/b]");
		lines.Add($"Total Population: {FormatPopulation(profile.TotalPopulation)}");
		lines.Add($"Highest Tech: {TechnologyLevel.ToStringName(profile.HighestTechLevel)}");
		lines.Add($"Dominant Regime: {GovernmentType.ToStringName(profile.DominantRegime)}");
		lines.Add($"Settlement Pattern: {profile.SettlementPattern}");
		lines.Add($"Primary Settlement: {profile.PrimarySettlementRank}");
		lines.Add($"Logistics: {profile.LogisticsCapacity}");
		lines.Add($"Urbanization: {profile.UrbanizationShare * 100:0}%");
		lines.Add($"Social Scale: {FormatScore(profile.SocialScale)}");
		lines.Add($"Trade Connectivity: {FormatScore(profile.TradeConnectivity)}");
		lines.Add($"State Capacity: {FormatScore(profile.StateCapacity)}");
		lines.Add($"Economic Complexity: {FormatScore(profile.EconomicComplexity)}");
		if (profile.HumanAuditRequired)
		{
			lines.Add("Human audit required for population-culture interpretation before release claims.");
		}
		lines.Add(string.Empty);
	}

	private static void AppendDesignSummary(List<string> lines, DesignResult? design)
	{
		if (design == null)
		{
			lines.Add("[b]RPG-Compatible Construction[/b]");
			lines.Add("No construction readout generated.");
			return;
		}

		lines.Add("[b]RPG-Compatible Construction[/b]");
		lines.Add($"Template: {ComponentCatalog.Templates[design.Spec.Template].Name}");
		lines.Add($"Configuration: {ComponentCatalog.HullConfigurations[design.Spec.Configuration].DisplayName}");
		lines.Add($"Hull: {design.Spec.HullTons:N0} tons");
		lines.Add($"Structure HP: {design.StructureHitPoints}");
		lines.Add($"Hardpoints: {design.HardpointsUsed}/{design.Hardpoints}");
		lines.Add($"Armor: {design.EffectiveArmorPoints} effective points");
		lines.Add($"Power: {design.Power.Demand}/{design.Power.Output} PP");
		lines.Add($"Berths: {design.BerthsAvailable:N0}");
		lines.Add($"Crew: {design.Crew.Total:N0}");
		lines.Add($"Used Tonnage: {design.Tonnage.Used:N0}/{design.Spec.HullTons:N0}");
		lines.Add($"Cost: {FormatCredits(design.Cost.Total)}");
		lines.Add("Compatibility note: verify RPG-system terminology and licensing before release.");
		lines.Add(string.Empty);
	}

	private void AppendWarnings(List<string> lines)
	{
		if (_currentResult == null || _currentResult.Warnings.Count == 0)
		{
			return;
		}

		lines.Add("[color=yellow][b]Warnings[/b][/color]");
		foreach (string warning in _currentResult.Warnings)
		{
			lines.Add($"- {warning}");
		}
	}

	private void ShowHelpDialog()
	{
		if (_helpDialog == null)
		{
			return;
		}

		HelpDialogLayoutHelper.Open(_helpDialog, 760, 580);
		if (_helpDialogText != null)
		{
			_helpDialogText.ScrollToLine(0);
		}
	}

	private void HideHelpDialog()
	{
		if (_helpDialog != null)
		{
			_helpDialog.Visible = false;
		}
	}

	private static string BuildHelpText()
	{
		return "[b]Station Studio[/b]\n"
			+ "This studio generates exactly one space station after you press Generate Station. Changing inputs does not regenerate automatically.\n\n"
			+ "[b]Station Inputs[/b]\n"
			+ "The left column is RPG-system agnostic. Each major parameter can be left random or set directly: role, purpose, station class, location, and population scale.\n\n"
			+ "[b]RPG Overrides[/b]\n"
			+ "The middle column controls compatibility presentation. Traveller-compatible construction is currently the only implemented readout. Other RPG systems are work in progress pending rules, terminology, and licensing review.\n\n"
			+ "[b]System Information[/b]\n"
			+ "The right column stays empty until generation and then shows resolved inputs, station facts, governance, services, construction, and population profile in one report.";
	}

	private static string FormatPopulation(int population)
	{
		if (population >= 1_000_000)
		{
			return $"{population / 1_000_000.0:0.00}M";
		}
		if (population >= 1_000)
		{
			return $"{population / 1_000.0:0.1}K";
		}

		return population.ToString();
	}

	private static string FormatScore(double value)
	{
		return $"{value * 100.0:0}%";
	}

	private static string FormatCredits(long credits)
	{
		if (credits >= 1_000_000_000)
		{
			return $"Cr {credits / 1_000_000_000.0:F2}B";
		}

		if (credits >= 1_000_000)
		{
			return $"Cr {credits / 1_000_000.0:F2}M";
		}

		if (credits >= 1_000)
		{
			return $"Cr {credits / 1_000.0:F1}K";
		}

		return $"Cr {credits}";
	}

	private static string FormatTitle(string value)
	{
		if (value.Length == 0)
		{
			return value;
		}

		return char.ToUpperInvariant(value[0]) + value.Substring(1);
	}

	private sealed class GenerationChoices
	{
		public StationPlacementContext.Context Context;
		public StationPurpose.Purpose Purpose;
		public StationClass.Class StationClass;
		public StationType.Type StationType;
		public double PopulationDensity = 1.0;
	}
}
