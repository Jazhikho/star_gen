using System;
using System.Collections.Generic;
using Godot;
using StarGen.App.Shared;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;

namespace StarGen.App;

/// <summary>
/// Production station studio that exposes station generation controls and live results.
/// </summary>
public partial class StationStudioScreen : Control
{
	[Signal]
	public delegate void back_requestedEventHandler();

	private OptionButton? _contextOption;
	private SpinBox? _seedSpinbox;
	private Button? _randomSeedButton;
	private HSlider? _densitySlider;
	private Label? _densityLabel;
	private SpinBox? _minStationsSpinbox;
	private SpinBox? _maxStationsSpinbox;
	private CheckBox? _allowUtilityCheck;
	private CheckBox? _allowOutpostsCheck;
	private CheckBox? _allowLargeCheck;
	private SpinBox? _colonyCountSpinbox;
	private CheckBox? _nativeCheck;
	private CheckBox? _nativeSpacefaringCheck;
	private HSlider? _resourceSlider;
	private Label? _resourceLabel;
	private SpinBox? _beltCountSpinbox;
	private Button? _generateButton;
	private Button? _backButton;
	private Label? _versionLabel;
	private RichTextLabel? _summaryLabel;
	private ItemList? _stationList;
	private RichTextLabel? _detailText;

	private StationGenerationResult? _currentResult;
	private readonly List<(string Type, GodotObject Data)> _allItems = new();

	/// <summary>
	/// Initializes controls and performs an initial generation pass.
	/// </summary>
	public override void _Ready()
	{
		CacheNodes();
		SetupUi();
		ConnectSignals();
		Generate();
	}

	private void CacheNodes()
	{
		_contextOption = GetNodeOrNull<OptionButton>("%ContextOption");
		_seedSpinbox = GetNodeOrNull<SpinBox>("%SeedSpinBox");
		_randomSeedButton = GetNodeOrNull<Button>("%RandomSeedButton");
		_densitySlider = GetNodeOrNull<HSlider>("%DensitySlider");
		_densityLabel = GetNodeOrNull<Label>("%DensityLabel");
		_minStationsSpinbox = GetNodeOrNull<SpinBox>("%MinStationsSpinBox");
		_maxStationsSpinbox = GetNodeOrNull<SpinBox>("%MaxStationsSpinBox");
		_allowUtilityCheck = GetNodeOrNull<CheckBox>("%AllowUtilityCheck");
		_allowOutpostsCheck = GetNodeOrNull<CheckBox>("%AllowOutpostsCheck");
		_allowLargeCheck = GetNodeOrNull<CheckBox>("%AllowLargeCheck");
		_colonyCountSpinbox = GetNodeOrNull<SpinBox>("%ColonyCountSpinBox");
		_nativeCheck = GetNodeOrNull<CheckBox>("%NativeCheck");
		_nativeSpacefaringCheck = GetNodeOrNull<CheckBox>("%NativeSpacefaringCheck");
		_resourceSlider = GetNodeOrNull<HSlider>("%ResourceSlider");
		_resourceLabel = GetNodeOrNull<Label>("%ResourceLabel");
		_beltCountSpinbox = GetNodeOrNull<SpinBox>("%BeltCountSpinBox");
		_generateButton = GetNodeOrNull<Button>("%GenerateButton");
		_backButton = GetNodeOrNull<Button>("%BackButton");
		_versionLabel = GetNodeOrNull<Label>("%VersionLabel");
		_summaryLabel = GetNodeOrNull<RichTextLabel>("%SummaryLabel");
		_stationList = GetNodeOrNull<ItemList>("%StationList");
		_detailText = GetNodeOrNull<RichTextLabel>("%DetailText");
	}

	private void SetupUi()
	{
		if (_versionLabel != null)
		{
			_versionLabel.Text = $"Version {UserFacingVersionHelper.GetDisplayVersion()}";
		}

		if (_contextOption != null)
		{
			_contextOption.Clear();
			_contextOption.AddItem("Auto-detect", -1);
			_contextOption.AddItem("Bridge System", (int)StationPlacementContext.Context.BridgeSystem);
			_contextOption.AddItem("Colony World", (int)StationPlacementContext.Context.ColonyWorld);
			_contextOption.AddItem("Native World", (int)StationPlacementContext.Context.NativeWorld);
			_contextOption.AddItem("Resource System", (int)StationPlacementContext.Context.ResourceSystem);
			_contextOption.AddItem("Strategic", (int)StationPlacementContext.Context.Strategic);
			_contextOption.AddItem("Scientific", (int)StationPlacementContext.Context.Scientific);
			_contextOption.AddItem("Other", (int)StationPlacementContext.Context.Other);
			_contextOption.Select(0);
		}

		if (_seedSpinbox != null)
		{
			_seedSpinbox.Value = GD.Randi() % 100000;
		}
		if (_densitySlider != null)
		{
			_densitySlider.Value = 1.0;
		}
		UpdateDensityLabel();
		if (_resourceSlider != null)
		{
			_resourceSlider.Value = 0.3;
		}
		UpdateResourceLabel();

		if (_allowUtilityCheck != null)
		{
			_allowUtilityCheck.ButtonPressed = true;
		}
		if (_allowOutpostsCheck != null)
		{
			_allowOutpostsCheck.ButtonPressed = true;
		}
		if (_allowLargeCheck != null)
		{
			_allowLargeCheck.ButtonPressed = true;
		}
		if (_nativeCheck != null)
		{
			_nativeCheck.ButtonPressed = false;
		}
		if (_nativeSpacefaringCheck != null)
		{
			_nativeSpacefaringCheck.ButtonPressed = false;
			_nativeSpacefaringCheck.Disabled = true;
		}
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
		if (_resourceSlider != null)
		{
			_resourceSlider.ValueChanged += _ => UpdateResourceLabel();
		}
		if (_nativeCheck != null)
		{
			_nativeCheck.Toggled += OnNativeToggled;
		}
		if (_stationList != null)
		{
			_stationList.ItemSelected += OnStationSelected;
		}
		if (_contextOption != null)
		{
			_contextOption.ItemSelected += _ => Generate();
		}
		if (_colonyCountSpinbox != null)
		{
			_colonyCountSpinbox.ValueChanged += _ => Generate();
		}
		if (_nativeSpacefaringCheck != null)
		{
			_nativeSpacefaringCheck.Toggled += _ => Generate();
		}
		if (_beltCountSpinbox != null)
		{
			_beltCountSpinbox.ValueChanged += _ => Generate();
		}
		if (_backButton != null)
		{
			_backButton.Pressed += () => EmitSignal(SignalName.back_requested);
		}
	}

	private void OnRandomizeSeed()
	{
		if (_seedSpinbox != null)
		{
			_seedSpinbox.Value = GD.Randi() % 100000;
		}
		Generate();
	}

	private void UpdateDensityLabel()
	{
		if (_densityLabel != null && _densitySlider != null)
		{
			_densityLabel.Text = $"{_densitySlider.Value:0.0}x";
		}
	}

	private void UpdateResourceLabel()
	{
		if (_resourceLabel != null && _resourceSlider != null)
		{
			_resourceLabel.Text = $"{_resourceSlider.Value * 100:0}%";
		}
	}

	private void OnNativeToggled(bool pressed)
	{
		if (_nativeSpacefaringCheck != null)
		{
			_nativeSpacefaringCheck.Disabled = !pressed;
			if (!pressed)
			{
				_nativeSpacefaringCheck.ButtonPressed = false;
			}
		}

		Generate();
	}

	private void Generate()
	{
		StationSystemContext context = BuildContext();
		StationSpec spec = BuildSpec();
		_currentResult = StationGenerator.Generate(context, spec, new SeededRng(spec.GenerationSeed));
		UpdateSummary();
		UpdateStationList();
		ClearDetail();
	}

	private StationSystemContext BuildContext()
	{
		StationSystemContext context = new StationSystemContext();
		context.SystemId = "station_studio_system";

		int selectedContext = -1;
		if (_contextOption != null)
		{
			selectedContext = _contextOption.GetSelectedId();
		}
		if (selectedContext == (int)StationPlacementContext.Context.BridgeSystem)
		{
			context.IsBridgeSystem = true;
		}

		int colonyCount = 0;
		if (_colonyCountSpinbox != null)
		{
			colonyCount = (int)_colonyCountSpinbox.Value;
		}
		context.ColonyWorldCount = colonyCount;
		context.HabitablePlanetCount = colonyCount;
		for (int index = 0; index < colonyCount; index++)
		{
			string planetId = $"planet_{index:D3}";
			context.ColonyPlanetIds.Add(planetId);
			context.PlanetIds.Add(planetId);
		}

		if (_nativeCheck != null && _nativeCheck.ButtonPressed)
		{
			context.NativeWorldCount = 1;
			string nativePlanet = "native_planet_001";
			context.NativePlanetIds.Add(nativePlanet);
			if (!context.PlanetIds.Contains(nativePlanet))
			{
				context.PlanetIds.Add(nativePlanet);
			}

			if (_nativeSpacefaringCheck != null && _nativeSpacefaringCheck.ButtonPressed)
			{
				context.HasSpacefaringNatives = true;
				context.HighestNativeTech = TechnologyLevel.Level.Spacefaring;
			}
			else
			{
				context.HasSpacefaringNatives = false;
				context.HighestNativeTech = TechnologyLevel.Level.Industrial;
			}
		}

		if (_resourceSlider != null)
		{
			context.ResourceRichness = _resourceSlider.Value;
		}
		else
		{
			context.ResourceRichness = 0.3;
		}

		if (_beltCountSpinbox != null)
		{
			context.AsteroidBeltCount = (int)_beltCountSpinbox.Value;
		}
		else
		{
			context.AsteroidBeltCount = 0;
		}

		if (context.ResourceRichness > 0.2)
		{
			for (int index = 0; index < 3; index++)
			{
				context.ResourceBodyIds.Add($"asteroid_{index:D3}");
			}
		}

		return context;
	}

	private StationSpec BuildSpec()
	{
		StationSpec spec = new StationSpec();
		if (_seedSpinbox != null)
		{
			spec.GenerationSeed = (int)_seedSpinbox.Value;
		}
		if (_densitySlider != null)
		{
			spec.PopulationDensity = _densitySlider.Value;
		}
		else
		{
			spec.PopulationDensity = 1.0;
		}
		if (_minStationsSpinbox != null)
		{
			spec.MinStations = (int)_minStationsSpinbox.Value;
		}
		if (_maxStationsSpinbox != null)
		{
			spec.MaxStations = (int)_maxStationsSpinbox.Value;
		}
		if (_allowUtilityCheck != null)
		{
			spec.AllowUtility = _allowUtilityCheck.ButtonPressed;
		}
		else
		{
			spec.AllowUtility = true;
		}
		if (_allowOutpostsCheck != null)
		{
			spec.AllowOutposts = _allowOutpostsCheck.ButtonPressed;
		}
		else
		{
			spec.AllowOutposts = true;
		}
		if (_allowLargeCheck != null)
		{
			spec.AllowLargeStations = _allowLargeCheck.ButtonPressed;
		}
		else
		{
			spec.AllowLargeStations = true;
		}

		int selectedContext = -1;
		if (_contextOption != null)
		{
			selectedContext = _contextOption.GetSelectedId();
		}
		if (selectedContext >= 0)
		{
			spec.ForceContext = (StationPlacementContext.Context)selectedContext;
		}

		spec.FoundingCivilizationId = "studio_civ";
		spec.FoundingCivilizationName = "Station Studio Civilization";
		return spec;
	}

	private void UpdateSummary()
	{
		if (_summaryLabel == null)
		{
			return;
		}
		if (_currentResult == null)
		{
			_summaryLabel.Text = "No generation result";
			return;
		}

		List<string> lines = new List<string>();
		StationPlacementRecommendation? recommendation = _currentResult.Recommendation;
		if (recommendation != null)
		{
			lines.Add($"[b]Context:[/b] {StationPlacementContext.ToStringName(recommendation.Context)}");
			string shouldHaveStationsText;
			if (recommendation.ShouldHaveStations)
			{
				shouldHaveStationsText = "Yes";
			}
			else
			{
				shouldHaveStationsText = "No";
			}
			lines.Add($"[b]Should Have Stations:[/b] {shouldHaveStationsText}");
		}

		lines.Add(string.Empty);
		lines.Add("[b]Generation Results:[/b]");
		lines.Add($"Seed: {_currentResult.GenerationSeed}");
		lines.Add($"Total Stations: {_currentResult.GetTotalCount()}");
		lines.Add($"  - Outposts: {_currentResult.Outposts.Count}");
		lines.Add($"  - Stations: {_currentResult.Stations.Count}");

		int totalPopulation = 0;
		foreach (Outpost outpost in _currentResult.Outposts)
		{
			totalPopulation += outpost.Population;
		}
		foreach (SpaceStation station in _currentResult.Stations)
		{
			totalPopulation += station.Population;
		}
		lines.Add($"Total Population: {FormatPopulation(totalPopulation)}");

		Dictionary<string, int> classCounts = new Dictionary<string, int>();
		foreach (Outpost outpost in _currentResult.Outposts)
		{
			string stationClass = StationClass.ToLetter(outpost.StationClass);
			classCounts[stationClass] = classCounts.GetValueOrDefault(stationClass, 0) + 1;
		}
		foreach (SpaceStation station in _currentResult.Stations)
		{
			string stationClass = StationClass.ToLetter(station.StationClass);
			classCounts[stationClass] = classCounts.GetValueOrDefault(stationClass, 0) + 1;
		}
		if (classCounts.Count > 0)
		{
			string classSummary = string.Empty;
			string[] displayOrder = { "U", "O", "B", "A", "S" };
			foreach (string stationClass in displayOrder)
			{
				if (classCounts.TryGetValue(stationClass, out int count))
				{
					classSummary += $"{stationClass}:{count} ";
				}
			}
			lines.Add($"By Class: {classSummary.Trim()}");
		}

		if (_currentResult.Warnings.Count > 0)
		{
			lines.Add(string.Empty);
			lines.Add("[color=yellow][b]Warnings:[/b][/color]");
			foreach (string warning in _currentResult.Warnings)
			{
				lines.Add($"  - {warning}");
			}
		}

		if (recommendation != null && recommendation.Reasoning.Count > 0)
		{
			lines.Add(string.Empty);
			lines.Add("[b]Reasoning:[/b]");
			foreach (string reason in recommendation.Reasoning)
			{
				lines.Add($"  • {reason}");
			}
		}

		_summaryLabel.Text = string.Join("\n", lines);
	}

	private void UpdateStationList()
	{
		_allItems.Clear();
		if (_stationList != null)
		{
			_stationList.Clear();
		}
		if (_currentResult == null)
		{
			return;
		}

		foreach (Outpost outpost in _currentResult.Outposts)
		{
			string status = string.Empty;
			if (!outpost.IsOperational)
			{
				status = " [DECOMM]";
			}

			string itemText = $"[{StationClass.ToLetter(outpost.StationClass)}] {outpost.Name} - {StationPurpose.ToStringName(outpost.PrimaryPurpose)} ({FormatPopulation(outpost.Population)}){status}";
			_stationList?.AddItem(itemText);
			_allItems.Add(("outpost", outpost));
		}

		foreach (SpaceStation station in _currentResult.Stations)
		{
			string status = string.Empty;
			if (!station.IsOperational)
			{
				status = " [DECOMM]";
			}

			string itemText = $"[{StationClass.ToLetter(station.StationClass)}] {station.Name} - {StationPurpose.ToStringName(station.PrimaryPurpose)} ({FormatPopulation(station.Population)}){status}";
			_stationList?.AddItem(itemText);
			_allItems.Add(("station", station));
		}
	}

	private void ClearDetail()
	{
		if (_detailText != null)
		{
			_detailText.Text = "Select a station to view details.";
		}
	}

	private void OnStationSelected(long index)
	{
		int selectedIndex = (int)index;
		if (selectedIndex < 0 || selectedIndex >= _allItems.Count)
		{
			ClearDetail();
			return;
		}

		(string itemType, GodotObject data) = _allItems[selectedIndex];
		if (itemType == "outpost")
		{
			ShowOutpostDetail((Outpost)data);
		}
		else
		{
			ShowStationDetail((SpaceStation)data);
		}
	}

	private void ShowOutpostDetail(Outpost outpost)
	{
		if (_detailText == null)
		{
			return;
		}

		List<string> lines = new List<string>();
		lines.Add($"[b][u]{outpost.Name}[/u][/b]");
		lines.Add(string.Empty);
		lines.Add($"[b]ID:[/b] {outpost.Id}");
		lines.Add($"[b]Class:[/b] {StationClass.ToLetter(outpost.StationClass)} ({StationClass.ToStringName(outpost.StationClass)})");
		lines.Add($"[b]Type:[/b] {StationType.ToStringName(outpost.StationType)}");
		lines.Add($"[b]Purpose:[/b] {StationPurpose.ToStringName(outpost.PrimaryPurpose)}");
		lines.Add($"[b]Context:[/b] {StationPlacementContext.ToStringName(outpost.PlacementContext)}");
		lines.Add(string.Empty);
		lines.Add($"[b]Population:[/b] {FormatPopulation(outpost.Population)}");
		lines.Add($"[b]Established:[/b] Year {outpost.EstablishedYear}");
		lines.Add($"[b]Age:[/b] {outpost.GetAge()} years");
		lines.Add(string.Empty);
		lines.Add($"[b]System:[/b] {outpost.SystemId}");
		if (!string.IsNullOrEmpty(outpost.OrbitingBodyId))
		{
			lines.Add($"[b]Orbiting:[/b] {outpost.OrbitingBodyId}");
		}
		lines.Add(string.Empty);
		lines.Add($"[b]Authority:[/b] {OutpostAuthority.ToStringName(outpost.Authority)}");
		lines.Add($"[b]Commander:[/b] {outpost.CommanderTitle}");
		if (outpost.HasParentOrganization())
		{
			lines.Add($"[b]Organization:[/b] {outpost.ParentOrganizationName}");
		}
		if (outpost.Services.Count > 0)
		{
			lines.Add(string.Empty);
			lines.Add("[b]Services:[/b]");
			foreach (StationService.Service service in outpost.Services)
			{
				lines.Add($"  • {StationService.ToStringName(service)}");
			}
		}
		if (!outpost.IsOperational)
		{
			lines.Add(string.Empty);
			lines.Add("[color=red][b]DECOMMISSIONED[/b][/color]");
			lines.Add($"Year: {outpost.DecommissionedYear}");
			lines.Add($"Reason: {outpost.DecommissionedReason}");
		}

		_detailText.Text = string.Join("\n", lines);
	}

	private void ShowStationDetail(SpaceStation station)
	{
		if (_detailText == null)
		{
			return;
		}

		List<string> lines = new List<string>();
		lines.Add($"[b][u]{station.Name}[/u][/b]");
		lines.Add(string.Empty);
		lines.Add($"[b]ID:[/b] {station.Id}");
		lines.Add($"[b]Class:[/b] {StationClass.ToLetter(station.StationClass)} ({StationClass.ToStringName(station.StationClass)})");
		lines.Add($"[b]Type:[/b] {StationType.ToStringName(station.StationType)}");
		lines.Add($"[b]Purpose:[/b] {StationPurpose.ToStringName(station.PrimaryPurpose)}");
		lines.Add($"[b]Context:[/b] {StationPlacementContext.ToStringName(station.PlacementContext)}");
		lines.Add(string.Empty);
		lines.Add($"[b]Population:[/b] {FormatPopulation(station.Population)}");
		lines.Add($"[b]Peak Population:[/b] {FormatPopulation(station.PeakPopulation)} (Year {station.PeakPopulationYear})");
		string growthState = station.GetGrowthState();
		if (growthState.Length > 0)
		{
			growthState = char.ToUpperInvariant(growthState[0]) + growthState.Substring(1);
		}
		lines.Add($"[b]Growth State:[/b] {growthState}");
		lines.Add($"[b]Established:[/b] Year {station.EstablishedYear}");
		lines.Add($"[b]Age:[/b] {station.GetAge()} years");
		lines.Add(string.Empty);
		lines.Add($"[b]System:[/b] {station.SystemId}");
		if (!string.IsNullOrEmpty(station.OrbitingBodyId))
		{
			lines.Add($"[b]Orbiting:[/b] {station.OrbitingBodyId}");
		}
		lines.Add(string.Empty);

		if (station.UsesOutpostGovernment())
		{
			lines.Add("[b]Governance:[/b] Outpost Authority");
			lines.Add($"[b]Authority:[/b] {OutpostAuthority.ToStringName(station.OutpostAuthority)}");
			lines.Add($"[b]Commander:[/b] {station.CommanderTitle}");
			if (station.HasParentOrganization())
			{
				lines.Add($"[b]Organization:[/b] {station.ParentOrganizationName}");
			}
		}
		else
		{
			lines.Add("[b]Governance:[/b] Colony Government");
			if (station.Government != null)
			{
				lines.Add($"[b]Regime:[/b] {GovernmentType.ToStringName(station.Government.Regime)}");
				lines.Add($"[b]Legitimacy:[/b] {station.Government.Legitimacy * 100:0}%");
				string stableText;
				if (station.IsPoliticallyStable())
				{
					stableText = "Yes";
				}
				else
				{
					stableText = "No";
				}
				lines.Add($"[b]Stable:[/b] {stableText}");
			}
		}

		if (!string.IsNullOrEmpty(station.FoundingCivilizationName))
		{
			lines.Add($"[b]Founded By:[/b] {station.FoundingCivilizationName}");
		}
		if (station.IsIndependent)
		{
			lines.Add($"[b]Independent:[/b] Yes (Year {station.IndependenceYear})");
		}
		if (station.Services.Count > 0)
		{
			lines.Add(string.Empty);
			lines.Add($"[b]Services ({station.Services.Count}):[/b]");
			List<string> serviceNames = new List<string>();
			foreach (StationService.Service service in station.Services)
			{
				serviceNames.Add(StationService.ToStringName(service));
			}
			for (int index = 0; index < serviceNames.Count; index += 3)
			{
				string row = "  ";
				for (int innerIndex = 0; innerIndex < 3 && index + innerIndex < serviceNames.Count; innerIndex++)
				{
					row += $"• {serviceNames[index + innerIndex]}  ";
				}
				lines.Add(row);
			}
		}
		if (station.History != null && station.History.GetAllEvents().Count > 0)
		{
			Godot.Collections.Array<HistoryEvent> allEvents = station.History.GetAllEvents();
			int showCount = Math.Min(5, allEvents.Count);
			lines.Add(string.Empty);
			lines.Add($"[b]History ({allEvents.Count} events):[/b]");
			for (int index = 0; index < showCount; index++)
			{
				HistoryEvent historyEvent = allEvents[index];
				lines.Add($"  Year {historyEvent.Year}: {historyEvent.Title}");
			}
		}
		if (!station.IsOperational)
		{
			lines.Add(string.Empty);
			lines.Add("[color=red][b]DECOMMISSIONED[/b][/color]");
			lines.Add($"Year: {station.DecommissionedYear}");
			lines.Add($"Reason: {station.DecommissionedReason}");
		}

		_detailText.Text = string.Join("\n", lines);
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
}
