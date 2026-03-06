using System;
using System.Collections.Generic;
using Godot;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;

namespace StarGen.App.Prototypes;

/// <summary>
/// Prototype scene demonstrating station and outpost generation.
/// Allows exploring different system contexts and generation parameters.
/// </summary>
public partial class StationGeneratorPrototype : Control
{
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
    private RichTextLabel? _summaryLabel;
    private ItemList? _stationList;
    private RichTextLabel? _detailText;

    private StationGenerationResult? _currentResult;
    private readonly List<(string Type, GodotObject Data)> _allItems = new();

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
        _maxStationsSpinbox = GetNodeOrNull<SpinBox>("%MaxStationsSpinbox");
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
        _summaryLabel = GetNodeOrNull<RichTextLabel>("%SummaryLabel");
        _stationList = GetNodeOrNull<ItemList>("%StationList");
        _detailText = GetNodeOrNull<RichTextLabel>("%DetailText");
    }

    private void SetupUi()
    {
        if (_contextOption == null)
            return;
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

        if (_seedSpinbox != null)
            _seedSpinbox.Value = GD.Randi() % 100000;
        if (_densitySlider != null)
            _densitySlider.Value = 1.0;
        UpdateDensityLabel();
        if (_resourceSlider != null)
            _resourceSlider.Value = 0.3;
        UpdateResourceLabel();

        if (_allowUtilityCheck != null)
            _allowUtilityCheck.ButtonPressed = true;
        if (_allowOutpostsCheck != null)
            _allowOutpostsCheck.ButtonPressed = true;
        if (_allowLargeCheck != null)
            _allowLargeCheck.ButtonPressed = true;
        if (_nativeCheck != null)
            _nativeCheck.ButtonPressed = false;
        if (_nativeSpacefaringCheck != null)
        {
            _nativeSpacefaringCheck.ButtonPressed = false;
            _nativeSpacefaringCheck.Disabled = true;
        }
    }

    private void ConnectSignals()
    {
        if (_generateButton != null)
            _generateButton.Pressed += Generate;
        if (_randomSeedButton != null)
            _randomSeedButton.Pressed += OnRandomizeSeed;
        if (_densitySlider != null)
            _densitySlider.ValueChanged += _ => UpdateDensityLabel();
        if (_resourceSlider != null)
            _resourceSlider.ValueChanged += _ => UpdateResourceLabel();
        if (_nativeCheck != null)
            _nativeCheck.Toggled += OnNativeToggled;
        if (_stationList != null)
            _stationList.ItemSelected += OnStationSelected;
        if (_contextOption != null)
            _contextOption.ItemSelected += _ => Generate();
        if (_colonyCountSpinbox != null)
            _colonyCountSpinbox.ValueChanged += _ => Generate();
        if (_nativeSpacefaringCheck != null)
            _nativeSpacefaringCheck.Toggled += _ => Generate();
        if (_beltCountSpinbox != null)
            _beltCountSpinbox.ValueChanged += _ => Generate();
    }

    private void OnRandomizeSeed()
    {
        if (_seedSpinbox != null)
            _seedSpinbox.Value = GD.Randi() % 100000;
        Generate();
    }

    private void UpdateDensityLabel()
    {
        if (_densityLabel != null && _densitySlider != null)
            _densityLabel.Text = $"{_densitySlider.Value:0.1}x";
    }

    private void UpdateResourceLabel()
    {
        if (_resourceLabel != null && _resourceSlider != null)
            _resourceLabel.Text = $"{_resourceSlider.Value * 100:0}%";
    }

    private void OnNativeToggled(bool pressed)
    {
        if (_nativeSpacefaringCheck != null)
            _nativeSpacefaringCheck.Disabled = !pressed;
        if (!pressed && _nativeSpacefaringCheck != null)
            _nativeSpacefaringCheck.ButtonPressed = false;
        Generate();
    }

    private void Generate()
    {
        StationSystemContext ctx = BuildContext();
        StationSpec spec = BuildSpec();
        _currentResult = StationGenerator.Generate(ctx, spec, new SeededRng(spec.GenerationSeed));
        UpdateSummary();
        UpdateStationList();
        ClearDetail();
    }

    private StationSystemContext BuildContext()
    {
        var ctx = new StationSystemContext();
        ctx.SystemId = "prototype_system";

        int selectedContext = -1;
        if (_contextOption != null)
        {
            selectedContext = _contextOption.GetSelectedId();
        }

        if (selectedContext == (int)StationPlacementContext.Context.BridgeSystem)
        {
            ctx.IsBridgeSystem = true;
        }

        int colonyCount = 0;
        if (_colonyCountSpinbox != null)
        {
            colonyCount = (int)_colonyCountSpinbox.Value;
        }
        ctx.ColonyWorldCount = colonyCount;
        ctx.HabitablePlanetCount = colonyCount;
        for (int i = 0; i < colonyCount; i++)
        {
            string planetId = $"planet_{i:D3}";
            ctx.ColonyPlanetIds.Add(planetId);
            ctx.PlanetIds.Add(planetId);
        }

        if (_nativeCheck != null && _nativeCheck.ButtonPressed)
        {
            ctx.NativeWorldCount = 1;
            string nativePlanet = "native_planet_001";
            ctx.NativePlanetIds.Add(nativePlanet);
            if (!ctx.PlanetIds.Contains(nativePlanet))
                ctx.PlanetIds.Add(nativePlanet);
            if (_nativeSpacefaringCheck != null && _nativeSpacefaringCheck.ButtonPressed)
            {
                ctx.HasSpacefaringNatives = true;
                ctx.HighestNativeTech = TechnologyLevel.Level.Spacefaring;
            }
            else
            {
                ctx.HasSpacefaringNatives = false;
                ctx.HighestNativeTech = TechnologyLevel.Level.Industrial;
            }
        }

        if (_resourceSlider != null)
        {
            ctx.ResourceRichness = _resourceSlider.Value;
        }
        else
        {
            ctx.ResourceRichness = 0.3;
        }

        if (_beltCountSpinbox != null)
        {
            ctx.AsteroidBeltCount = (int)_beltCountSpinbox.Value;
        }
        else
        {
            ctx.AsteroidBeltCount = 0;
        }
        if (ctx.ResourceRichness > 0.2)
        {
            for (int i = 0; i < 3; i++)
                ctx.ResourceBodyIds.Add($"asteroid_{i:D3}");
        }

        return ctx;
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

        spec.FoundingCivilizationId = "proto_civ";
        spec.FoundingCivilizationName = "Prototype Civilization";
        return spec;
    }

    private void UpdateSummary()
    {
        if (_summaryLabel == null)
            return;
        if (_currentResult == null)
        {
            _summaryLabel.Text = "No generation result";
            return;
        }

        List<string> lines = new List<string>();
        StationPlacementRecommendation? rec = _currentResult.Recommendation;
        if (rec != null)
        {
            lines.Add($"[b]Context:[/b] {StationPlacementContext.ToStringName(rec.Context)}");
            string hasStationsStr;
            if (rec.ShouldHaveStations)
            {
                hasStationsStr = "Yes";
            }
            else
            {
                hasStationsStr = "No";
            }

            lines.Add($"[b]Should Have Stations:[/b] {hasStationsStr}");
        }

        lines.Add("");
        lines.Add("[b]Generation Results:[/b]");
        lines.Add($"Seed: {_currentResult.GenerationSeed}");
        lines.Add($"Total Stations: {_currentResult.GetTotalCount()}");
        lines.Add($"  - Outposts: {_currentResult.Outposts.Count}");
        lines.Add($"  - Stations: {_currentResult.Stations.Count}");

        int totalPop = 0;
        foreach (Outpost o in _currentResult.Outposts)
            totalPop += o.Population;
        foreach (SpaceStation s in _currentResult.Stations)
            totalPop += s.Population;
        lines.Add($"Total Population: {FormatPopulation(totalPop)}");

        Dictionary<string, int> classCounts = new Dictionary<string, int>();
        foreach (Outpost o in _currentResult.Outposts)
        {
            string cls = StationClass.ToLetter(o.StationClass);
            classCounts[cls] = classCounts.GetValueOrDefault(cls, 0) + 1;
        }
        foreach (SpaceStation s in _currentResult.Stations)
        {
            string cls = StationClass.ToLetter(s.StationClass);
            classCounts[cls] = classCounts.GetValueOrDefault(cls, 0) + 1;
        }
        if (classCounts.Count > 0)
        {
            string classStr = "";
            foreach (string cls in new[] { "U", "O", "B", "A", "S" })
            {
                if (classCounts.TryGetValue(cls, out int count))
                    classStr += $"{cls}:{count} ";
            }
            lines.Add($"By Class: {classStr.Trim()}");
        }

        if (_currentResult.Warnings.Count > 0)
        {
            lines.Add("");
            lines.Add("[color=yellow][b]Warnings:[/b][/color]");
            foreach (string warning in _currentResult.Warnings)
                lines.Add($"  - {warning}");
        }

        if (rec != null && rec.Reasoning.Count > 0)
        {
            lines.Add("");
            lines.Add("[b]Reasoning:[/b]");
            foreach (string reason in rec.Reasoning)
                lines.Add($"  • {reason}");
        }

        _summaryLabel.Text = string.Join("\n", lines);
    }

    private void UpdateStationList()
    {
        _allItems.Clear();
        if (_stationList != null)
            _stationList.Clear();
        if (_currentResult == null)
            return;

        foreach (Outpost outpost in _currentResult.Outposts)
        {
            string status = "";
            if (!outpost.IsOperational)
            {
                status = " [DECOMM]";
            }

            string text = $"[{StationClass.ToLetter(outpost.StationClass)}] {outpost.Name} - {StationPurpose.ToStringName(outpost.PrimaryPurpose)} ({FormatPopulation(outpost.Population)}){status}";
            _stationList?.AddItem(text);
            _allItems.Add(("outpost", outpost));
        }

        foreach (SpaceStation station in _currentResult.Stations)
        {
            string status = "";
            if (!station.IsOperational)
            {
                status = " [DECOMM]";
            }

            string text = $"[{StationClass.ToLetter(station.StationClass)}] {station.Name} - {StationPurpose.ToStringName(station.PrimaryPurpose)} ({FormatPopulation(station.Population)}){status}";
            _stationList?.AddItem(text);
            _allItems.Add(("station", station));
        }
    }

    private void ClearDetail()
    {
        if (_detailText != null)
            _detailText.Text = "Select a station to view details.";
    }

    private void OnStationSelected(long index)
    {
        int i = (int)index;
        if (i < 0 || i >= _allItems.Count)
        {
            ClearDetail();
            return;
        }

        (string type, GodotObject data) = _allItems[i];
        if (type == "outpost")
            ShowOutpostDetail((Outpost)data);
        else
            ShowStationDetail((SpaceStation)data);
    }

    private void ShowOutpostDetail(Outpost outpost)
    {
        if (_detailText == null)
        {
            return;
        }

        List<string> lines = new List<string>();
        lines.Add($"[b][u]{outpost.Name}[/u][/b]");
        lines.Add("");
        lines.Add($"[b]ID:[/b] {outpost.Id}");
        lines.Add($"[b]Class:[/b] {StationClass.ToLetter(outpost.StationClass)} ({StationClass.ToStringName(outpost.StationClass)})");
        lines.Add($"[b]Type:[/b] {StationType.ToStringName(outpost.StationType)}");
        lines.Add($"[b]Purpose:[/b] {StationPurpose.ToStringName(outpost.PrimaryPurpose)}");
        lines.Add($"[b]Context:[/b] {StationPlacementContext.ToStringName(outpost.PlacementContext)}");
        lines.Add("");
        lines.Add($"[b]Population:[/b] {FormatPopulation(outpost.Population)}");
        lines.Add($"[b]Established:[/b] Year {outpost.EstablishedYear}");
        lines.Add($"[b]Age:[/b] {outpost.GetAge()} years");
        lines.Add("");
        lines.Add($"[b]System:[/b] {outpost.SystemId}");
        if (!string.IsNullOrEmpty(outpost.OrbitingBodyId))
            lines.Add($"[b]Orbiting:[/b] {outpost.OrbitingBodyId}");
        lines.Add("");
        lines.Add($"[b]Authority:[/b] {OutpostAuthority.ToStringName(outpost.Authority)}");
        lines.Add($"[b]Commander:[/b] {outpost.CommanderTitle}");
        if (outpost.HasParentOrganization())
            lines.Add($"[b]Organization:[/b] {outpost.ParentOrganizationName}");
        if (outpost.Services.Count > 0)
        {
            lines.Add("");
            lines.Add("[b]Services:[/b]");
            foreach (StationService.Service service in outpost.Services)
                lines.Add($"  • {StationService.ToStringName(service)}");
        }
        if (!outpost.IsOperational)
        {
            lines.Add("");
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
        lines.Add("");
        lines.Add($"[b]ID:[/b] {station.Id}");
        lines.Add($"[b]Class:[/b] {StationClass.ToLetter(station.StationClass)} ({StationClass.ToStringName(station.StationClass)})");
        lines.Add($"[b]Type:[/b] {StationType.ToStringName(station.StationType)}");
        lines.Add($"[b]Purpose:[/b] {StationPurpose.ToStringName(station.PrimaryPurpose)}");
        lines.Add($"[b]Context:[/b] {StationPlacementContext.ToStringName(station.PlacementContext)}");
        lines.Add("");
        lines.Add($"[b]Population:[/b] {FormatPopulation(station.Population)}");
        lines.Add($"[b]Peak Population:[/b] {FormatPopulation(station.PeakPopulation)} (Year {station.PeakPopulationYear})");
        string growthState = station.GetGrowthState();
        if (growthState.Length > 0)
            growthState = char.ToUpperInvariant(growthState[0]) + growthState.Substring(1);
        lines.Add($"[b]Growth State:[/b] {growthState}");
        lines.Add($"[b]Established:[/b] Year {station.EstablishedYear}");
        lines.Add($"[b]Age:[/b] {station.GetAge()} years");
        lines.Add("");
        lines.Add($"[b]System:[/b] {station.SystemId}");
        if (!string.IsNullOrEmpty(station.OrbitingBodyId))
            lines.Add($"[b]Orbiting:[/b] {station.OrbitingBodyId}");
        lines.Add("");
        if (station.UsesOutpostGovernment())
        {
            lines.Add("[b]Governance:[/b] Outpost Authority");
            lines.Add($"[b]Authority:[/b] {OutpostAuthority.ToStringName(station.OutpostAuthority)}");
            lines.Add($"[b]Commander:[/b] {station.CommanderTitle}");
            if (station.HasParentOrganization())
                lines.Add($"[b]Organization:[/b] {station.ParentOrganizationName}");
        }
        else
        {
            lines.Add("[b]Governance:[/b] Colony Government");
            if (station.Government != null)
            {
                lines.Add($"[b]Regime:[/b] {GovernmentType.ToStringName(station.Government.Regime)}");
                lines.Add($"[b]Legitimacy:[/b] {station.Government.Legitimacy * 100:0}%");
                string stableStr;
                if (station.IsPoliticallyStable())
                {
                    stableStr = "Yes";
                }
                else
                {
                    stableStr = "No";
                }

                lines.Add($"[b]Stable:[/b] {stableStr}");
            }
        }
        if (!string.IsNullOrEmpty(station.FoundingCivilizationName))
            lines.Add($"[b]Founded By:[/b] {station.FoundingCivilizationName}");
        if (station.IsIndependent)
            lines.Add($"[b]Independent:[/b] Yes (Year {station.IndependenceYear})");
        if (station.Services.Count > 0)
        {
            lines.Add("");
            lines.Add($"[b]Services ({station.Services.Count}):[/b]");
            List<string> names = new List<string>();
            foreach (StationService.Service s in station.Services)
                names.Add(StationService.ToStringName(s));
            for (int i = 0; i < names.Count; i += 3)
            {
                string row = "  ";
                for (int j = 0; j < 3 && i + j < names.Count; j++)
                    row += $"• {names[i + j]}  ";
                lines.Add(row);
            }
        }
        if (station.History != null && station.History.GetAllEvents().Count > 0)
        {
            Godot.Collections.Array<HistoryEvent> allEvents = station.History.GetAllEvents();
            int showCount = Math.Min(5, allEvents.Count);
            lines.Add("");
            lines.Add($"[b]History ({allEvents.Count} events):[/b]");
            for (int k = 0; k < showCount; k++)
            {
                HistoryEvent evt = allEvents[k];
                lines.Add($"  Year {evt.Year}: {evt.Title}");
            }
        }
        if (!station.IsOperational)
        {
            lines.Add("");
            lines.Add("[color=red][b]DECOMMISSIONED[/b][/color]");
            lines.Add($"Year: {station.DecommissionedYear}");
            lines.Add($"Reason: {station.DecommissionedReason}");
        }
        _detailText.Text = string.Join("\n", lines);
    }

    private static string FormatPopulation(int pop)
    {
        if (pop >= 1_000_000)
            return $"{pop / 1_000_000.0:0.00}M";
        if (pop >= 1_000)
            return $"{pop / 1_000.0:0.1}K";
        return pop.ToString();
    }
}
