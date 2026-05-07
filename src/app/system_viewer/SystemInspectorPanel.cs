using Godot;
using StarGen.App.Components;
using StarGen.App.Viewer;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Traveller;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;
using System.Collections.Generic;
using System.Globalization;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Side panel for the solar-system viewer.
/// Keeps the viewer focused on system preview and selection handoff rather than duplicating object-level detail.
/// </summary>
public partial class SystemInspectorPanel : VBoxContainer
{
    [Signal]
    public delegate void OpenInViewerRequestedEventHandler(CelestialBody body);

    [Signal]
    public delegate void FocusBodyRequestedEventHandler(CelestialBody body);

    [Signal]
    public delegate void FocusBeltRequestedEventHandler(string beltId);

    private VBoxContainer? _overviewSection;
    private VBoxContainer? _bodySection;
    private Button? _openViewerButton;
    private SolarSystem? _currentSystem;
    private CelestialBody? _selectedBody;
    private string _selectedBeltId = string.Empty;

    public override void _Ready()
    {
        CacheUi();
    }

    /// <summary>
    /// Displays system-level preview information and clears the current selection.
    /// </summary>
    public void DisplaySystem(SolarSystem? system, SolarSystemSpec? spec = null)
    {
        EnsureUi();

        _currentSystem = system;
        _selectedBody = null;
        _selectedBeltId = string.Empty;
        ResetOpenViewerButtonState();
        RenderOverview();
        RenderSelectionPrompt();
    }

    /// <summary>
    /// Displays a selected body summary and keeps the overview in sync with the current selection.
    /// </summary>
    public void DisplaySelectedBody(CelestialBody? body)
    {
        EnsureUi();

        _selectedBody = body;
        _selectedBeltId = string.Empty;
        ResetOpenViewerButtonState();
        RenderOverview();
        ClearSectionContent(_bodySection);

        if (body == null)
        {
            RenderSelectionPrompt();
            return;
        }

        AddProperty(_bodySection, "Name", GetBodyDisplayName(body));
        AddProperty(_bodySection, "Type", GetTypeDisplay(body.Type));
        AddProperty(_bodySection, "Class", DescribeBodyClass(body));

        if (body.HasOrbital() && body.Orbital != null && !string.IsNullOrWhiteSpace(body.Orbital.ParentId))
        {
            AddProperty(_bodySection, "Orbits", ResolveOrbitHostName(body.Orbital.ParentId));
            AddProperty(
                _bodySection,
                "Distance",
                string.Format(CultureInfo.InvariantCulture, "{0:0.0000} AU", body.Orbital.SemiMajorAxisM / Units.AuMeters));
        }

        if (body.Type == CelestialType.Type.Planet && _currentSystem != null)
        {
            int moonCount = _currentSystem.GetMoonsOfPlanet(body.Id).Count;
            AddProperty(_bodySection, "Moons", moonCount.ToString(CultureInfo.InvariantCulture));
        }

        AddPopulationSelectionSummary(body);
        AddTravellerSelectionSummary(body);
        AddOpenViewerButton();
    }

    /// <summary>
    /// Displays a selected belt summary.
    /// </summary>
    public void DisplaySelectedBelt(AsteroidBelt? belt, SolarSystem? system)
    {
        EnsureUi();

        if (system != null)
        {
            _currentSystem = system;
        }

        _selectedBody = null;
        _selectedBeltId = belt?.Id ?? string.Empty;
        ResetOpenViewerButtonState();
        RenderOverview();
        ClearSectionContent(_bodySection);

        if (belt == null)
        {
            RenderSelectionPrompt();
            return;
        }

        AddProperty(_bodySection, "Name", GetBeltDisplayName(belt));
        AddProperty(_bodySection, "Type", "Asteroid Belt");
        AddProperty(_bodySection, "Class", belt.GetCompositionString() + " belt");
        AddProperty(_bodySection, "Center", string.Format(CultureInfo.InvariantCulture, "{0:0.0000} AU", belt.GetCenterAu()));
        AddProperty(_bodySection, "Width", string.Format(CultureInfo.InvariantCulture, "{0:0.0000} AU", belt.GetWidthAu()));
        if (!string.IsNullOrWhiteSpace(belt.ReservoirSubfamily))
        {
            AddProperty(_bodySection, "Reservoir", FormatReservoirSubfamily(belt.ReservoirSubfamily));
        }

        string reservoirSummary = GetReservoirFamilySummary(belt.Id);
        if (!string.IsNullOrWhiteSpace(reservoirSummary))
        {
            AddProperty(_bodySection, "Reservoir Families", reservoirSummary);
        }

        AddProperty(_bodySection, "Major Bodies", belt.GetMajorAsteroidCount().ToString(CultureInfo.InvariantCulture));
        RemoveOpenViewerButton();
    }

    /// <summary>
    /// Clears all displayed information.
    /// </summary>
    public void Clear()
    {
        EnsureUi();

        _currentSystem = null;
        _selectedBody = null;
        _selectedBeltId = string.Empty;
        ResetOpenViewerButtonState();
        RenderOverview();
        RenderSelectionPrompt();
    }

    private void CacheUi()
    {
        _overviewSection = GetNodeOrNull<VBoxContainer>("OverviewSection/Content");
        _bodySection = GetNodeOrNull<VBoxContainer>("SelectedBodySection/Content");
        _openViewerButton = GetNodeOrNull<Button>("SelectedBodySection/OpenViewerButton");
        if (_openViewerButton != null)
        {
            _openViewerButton.Pressed += OnOpenViewerPressed;
            _openViewerButton.Visible = false;
        }
    }

    private void RenderOverview()
    {
        ClearSectionContent(_overviewSection);
        if (_currentSystem == null)
        {
            AddProperty(_overviewSection, "Status", "No system generated");
            return;
        }

        AddProperty(_overviewSection, "Stars", _currentSystem.GetStarCount().ToString(CultureInfo.InvariantCulture));
        AddProperty(
            _overviewSection,
            "Bodies",
            string.Format(
                CultureInfo.InvariantCulture,
                "{0} planets, {1} moons, {2} belts, {3} reservoirs",
                _currentSystem.GetPlanetCount(),
                _currentSystem.GetMoonCount(),
                _currentSystem.AsteroidBelts.Count,
                _currentSystem.SmallBodyReservoirs.Count));
        AddProperty(_overviewSection, "Settlement", GetSystemSettlementSummary(_currentSystem));

        AddSeparator(_overviewSection);
        AddHeader(_overviewSection, "System Preview");
        AddOrbitPreviewRows(_currentSystem);
    }

    private void RenderSelectionPrompt()
    {
        ClearSectionContent(_bodySection);
        AddProperty(_bodySection, "Status", "Select a star or orbit entry");
        RemoveOpenViewerButton();
    }

    private void AddOrbitPreviewRows(SolarSystem system)
    {
        AddHeader(_overviewSection, "Stars");
        Godot.Collections.Array<CelestialBody> stars = system.GetStars();
        for (int index = 0; index < stars.Count; index += 1)
        {
            CelestialBody star = stars[index];
            string label = $"Star {index + 1}: {DescribeBodyClass(star)}";
            AddBodyFocusButton(label, star, star.Id == _selectedBody?.Id);
        }

        List<OrbitPreviewEntry> orbitEntries = BuildOrbitPreviewEntries(system);
        if (orbitEntries.Count == 0)
        {
            AddInfoLabel(_overviewSection, "No orbiting bodies");
            return;
        }

        AddSeparator(_overviewSection);
        AddHeader(_overviewSection, "Orbits");
        for (int index = 0; index < orbitEntries.Count; index += 1)
        {
            OrbitPreviewEntry entry = orbitEntries[index];
            if (entry.Body != null)
            {
                string label = $"Orbit {index + 1}: {GetBodyDisplayName(entry.Body)} - {DescribeBodyClass(entry.Body)}";
                AddBodyFocusButton(label, entry.Body, entry.Body.Id == _selectedBody?.Id);
            }
            else if (entry.Belt != null)
            {
                string label = $"Orbit {index + 1}: {GetBeltDisplayName(entry.Belt)} - {entry.Belt.GetCompositionString()} belt";
                AddBeltFocusButton(label, entry.Belt, entry.Belt.Id == _selectedBeltId);
            }
        }

        AddReservoirPreviewRows(system);
    }

    private void AddReservoirPreviewRows(SolarSystem system)
    {
        if (system.SmallBodyReservoirs.Count == 0)
        {
            return;
        }

        AddSeparator(_overviewSection);
        AddHeader(_overviewSection, "Small-Body Reservoirs");
        Dictionary<string, List<SmallBodyReservoir>> grouped = GroupReservoirsByAnchor(system.SmallBodyReservoirs);
        foreach (string anchorBeltId in grouped.Keys)
        {
            string anchorName = anchorBeltId;
            AsteroidBelt? anchorBelt = FindBeltById(anchorBeltId);
            if (anchorBelt != null)
            {
                anchorName = GetBeltDisplayName(anchorBelt);
            }

            AddInfoLabel(_overviewSection, $"{anchorName}: {FormatReservoirFamilySummary(grouped[anchorBeltId])}");
        }
    }

    private void AddBodyFocusButton(string text, CelestialBody body, bool selected)
    {
        if (_overviewSection == null)
        {
            return;
        }

        Button button = UiSceneTemplates.InstantiateActionButton();
        button.Alignment = HorizontalAlignment.Left;
        button.Text = selected ? "* " + text : text;
        button.TooltipText = "Focus " + GetBodyDisplayName(body);
        button.Pressed += () => EmitSignal(SignalName.FocusBodyRequested, body);
        _overviewSection.AddChild(button);
    }

    private void AddBeltFocusButton(string text, AsteroidBelt belt, bool selected)
    {
        if (_overviewSection == null)
        {
            return;
        }

        Button button = UiSceneTemplates.InstantiateActionButton();
        button.Alignment = HorizontalAlignment.Left;
        button.Text = selected ? "* " + text : text;
        button.TooltipText = "Focus " + GetBeltDisplayName(belt);
        button.Pressed += () => EmitSignal(SignalName.FocusBeltRequested, belt.Id);
        _overviewSection.AddChild(button);
    }

    private static List<OrbitPreviewEntry> BuildOrbitPreviewEntries(SolarSystem system)
    {
        List<OrbitPreviewEntry> entries = new();
        foreach (CelestialBody planet in system.GetPlanets())
        {
            double distance = 0.0;
            if (planet.HasOrbital() && planet.Orbital != null)
            {
                distance = planet.Orbital.SemiMajorAxisM;
            }

            entries.Add(new OrbitPreviewEntry(distance, planet, null));
        }

        foreach (AsteroidBelt belt in system.AsteroidBelts)
        {
            entries.Add(new OrbitPreviewEntry(belt.GetCenterM(), null, belt));
        }

        entries.Sort(static (left, right) => left.DistanceM.CompareTo(right.DistanceM));
        return entries;
    }

    private string GetReservoirFamilySummary(string anchorBeltId)
    {
        if (_currentSystem == null)
        {
            return string.Empty;
        }

        List<SmallBodyReservoir> reservoirs = new();
        foreach (SmallBodyReservoir reservoir in _currentSystem.SmallBodyReservoirs)
        {
            if (reservoir.AnchorBeltId == anchorBeltId)
            {
                reservoirs.Add(reservoir);
            }
        }

        return FormatReservoirFamilySummary(reservoirs);
    }

    private static Dictionary<string, List<SmallBodyReservoir>> GroupReservoirsByAnchor(Godot.Collections.Array<SmallBodyReservoir> reservoirs)
    {
        Dictionary<string, List<SmallBodyReservoir>> grouped = new();
        foreach (SmallBodyReservoir reservoir in reservoirs)
        {
            string key = reservoir.AnchorBeltId;
            if (string.IsNullOrWhiteSpace(key))
            {
                key = reservoir.Id;
            }

            if (!grouped.ContainsKey(key))
            {
                grouped[key] = new List<SmallBodyReservoir>();
            }

            grouped[key].Add(reservoir);
        }

        return grouped;
    }

    private static string FormatReservoirFamilySummary(List<SmallBodyReservoir> reservoirs)
    {
        if (reservoirs.Count == 0)
        {
            return string.Empty;
        }

        reservoirs.Sort(static (left, right) => right.RelativeWeight.CompareTo(left.RelativeWeight));
        List<string> labels = new();
        int limit = System.Math.Min(3, reservoirs.Count);
        for (int index = 0; index < limit; index += 1)
        {
            SmallBodyReservoir reservoir = reservoirs[index];
            string familyLabel = FormatReservoirFamily(reservoir.ReservoirFamily);
            labels.Add(string.Format(CultureInfo.InvariantCulture, "{0} {1:0}%", familyLabel, reservoir.RelativeWeight * 100.0));
        }

        if (reservoirs.Count > limit)
        {
            labels.Add($"+{reservoirs.Count - limit} more");
        }

        return string.Join(", ", labels);
    }

    private void AddPopulationSelectionSummary(CelestialBody body)
    {
        if (!body.HasPopulationData() || body.PopulationData == null)
        {
            return;
        }

        if (!body.PopulationData.IsInhabited())
        {
            AddProperty(_bodySection, "Settlement", "Uninhabited");
            return;
        }

        AddProperty(
            _bodySection,
            "Settlement",
            PropertyFormatter.FormatPopulation(body.PopulationData.GetTotalPopulation()) + " inhabited");
        AddProperty(_bodySection, "Situation", PropertyFormatter.FormatPoliticalSituation(GetPoliticalSituation(body)));
    }

    private void AddTravellerSelectionSummary(CelestialBody body)
    {
        if (body.Type != CelestialType.Type.Planet && body.Type != CelestialType.Type.Moon)
        {
            return;
        }

        TravellerWorldProfile profile;
        TravellerWorldProfile? stored = TravellerWorldGenerator.TryGetStoredProfile(body);
        if (stored != null)
        {
            profile = stored;
        }
        else
        {
            profile = TravellerWorldGenerator.DeriveFromBody(body);
        }

        AddProperty(_bodySection, "UWP", profile.ToUwpString());
        TravellerTradeCodeSet? tradeCodes = TryGetStoredTradeCodes(body);
        if (tradeCodes != null)
        {
            AddProperty(_bodySection, "Trade Codes", tradeCodes.ToDisplayString());
        }
    }

    private string GetSystemSettlementSummary(SolarSystem system)
    {
        if (!system.IsInhabited())
        {
            return "Uninhabited";
        }

        return PropertyFormatter.FormatPopulation(system.GetTotalPopulation()) + " inhabited";
    }

    private string GetBodyDisplayName(CelestialBody body)
    {
        if (string.IsNullOrWhiteSpace(body.Name))
        {
            return body.Id;
        }

        return body.Name;
    }

    private string GetBeltDisplayName(AsteroidBelt belt)
    {
        if (_currentSystem == null)
        {
            return "Asteroid Belt";
        }

        List<AsteroidBelt> sortedBelts = new();
        foreach (AsteroidBelt candidate in _currentSystem.AsteroidBelts)
        {
            sortedBelts.Add(candidate);
        }

        sortedBelts.Sort(static (left, right) => left.GetCenterM().CompareTo(right.GetCenterM()));
        for (int index = 0; index < sortedBelts.Count; index += 1)
        {
            if (sortedBelts[index].Id == belt.Id)
            {
                return $"Asteroid Belt {index + 1}";
            }
        }

        return "Asteroid Belt";
    }

    private static string FormatReservoirSubfamily(string subfamily)
    {
        string label = subfamily.Replace("_", " ");
        label = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(label);
        return label.Replace("Tno", "TNO");
    }

    private static string FormatReservoirFamily(string family)
    {
        string label = family.Replace("_", " ");
        label = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(label);
        return label.Replace("Tno", "TNO");
    }

    private string ResolveOrbitHostName(string parentId)
    {
        if (_currentSystem == null || string.IsNullOrWhiteSpace(parentId))
        {
            return parentId;
        }

        CelestialBody? parentBody = _currentSystem.GetBody(parentId);
        if (parentBody != null)
        {
            return GetBodyDisplayName(parentBody);
        }

        AsteroidBelt? parentBelt = FindBeltById(parentId);
        if (parentBelt != null)
        {
            return GetBeltDisplayName(parentBelt);
        }

        return parentId;
    }

    private AsteroidBelt? FindBeltById(string beltId)
    {
        if (_currentSystem == null)
        {
            return null;
        }

        foreach (AsteroidBelt belt in _currentSystem.AsteroidBelts)
        {
            if (belt.Id == beltId)
            {
                return belt;
            }
        }

        return null;
    }

    private static string DescribeBodyClass(CelestialBody body)
    {
        if (body.HasStellar() && body.Stellar != null && !string.IsNullOrWhiteSpace(body.Stellar.SpectralClass))
        {
            return body.Stellar.SpectralClass;
        }

        if (body.HasSurface() && body.Surface != null && !string.IsNullOrWhiteSpace(body.Surface.SurfaceType))
        {
            return body.Surface.SurfaceType;
        }

        if (body.HasAtmosphere() && body.Atmosphere != null && body.Atmosphere.SurfacePressurePa < 100.0)
        {
            return "Airless world";
        }

        double earthRadii = body.Physical.RadiusM / Units.EarthRadiusMeters;
        if (body.Type == CelestialType.Type.Moon)
        {
            if (earthRadii >= 0.75)
            {
                return "Large moon";
            }

            return "Rocky moon";
        }

        if (earthRadii >= 6.0)
        {
            return "Giant planet";
        }

        if (earthRadii >= 3.0)
        {
            return "Large planet";
        }

        if (earthRadii >= 1.25)
        {
            return "Terrestrial world";
        }

        return body.GetTypeString();
    }

    private static TravellerTradeCodeSet? TryGetStoredTradeCodes(CelestialBody body)
    {
        if (body.Provenance == null || !body.Provenance.SpecSnapshot.ContainsKey("traveller_trade_codes"))
        {
            return null;
        }

        Variant codesVariant = body.Provenance.SpecSnapshot["traveller_trade_codes"];
        if (codesVariant.VariantType != Variant.Type.Dictionary)
        {
            return null;
        }

        return TravellerTradeCodeSet.FromDictionary((Godot.Collections.Dictionary)codesVariant);
    }

    private static string GetPoliticalSituation(CelestialBody body)
    {
        if (body.PopulationData == null)
        {
            return "uninhabited";
        }

        int nativePopulation = body.PopulationData.GetNativePopulation();
        int colonyPopulation = body.PopulationData.GetColonyPopulation();
        if (nativePopulation <= 0 && colonyPopulation <= 0)
        {
            return "uninhabited";
        }

        if (nativePopulation > 0 && colonyPopulation <= 0)
        {
            return "native_only";
        }

        if (nativePopulation <= 0 && colonyPopulation > 0)
        {
            return "colony_only";
        }

        return "coexisting";
    }

    private static string GetTypeDisplay(CelestialType.Type bodyType)
    {
        return bodyType switch
        {
            CelestialType.Type.Star => "Star",
            CelestialType.Type.Planet => "Planet",
            CelestialType.Type.Moon => "Moon",
            CelestialType.Type.Asteroid => "Asteroid",
            _ => "Unknown",
        };
    }

    private static void ClearSectionContent(VBoxContainer? section)
    {
        if (section == null)
        {
            return;
        }

        foreach (Node child in section.GetChildren())
        {
            child.QueueFree();
        }
    }

    private static void AddProperty(VBoxContainer? section, string labelText, string valueText)
    {
        if (section == null)
        {
            return;
        }

        HBoxContainer row = UiSceneTemplates.InstantiatePropertyRow();
        Label label = UiSceneTemplates.GetRequiredChild<Label>(row, "Key");
        Label value = UiSceneTemplates.GetRequiredChild<Label>(row, "Value");
        label.Text = labelText + ":";
        value.Text = valueText;
        section.AddChild(row);
    }

    private static void AddHeader(VBoxContainer? section, string text)
    {
        if (section == null)
        {
            return;
        }

        Label header = UiSceneTemplates.InstantiateSubheaderLabel();
        header.Text = text;
        section.AddChild(header);
    }

    private static void AddSeparator(VBoxContainer? section)
    {
        if (section != null)
        {
            section.AddChild(UiSceneTemplates.InstantiateDivider());
        }
    }

    private static void AddInfoLabel(VBoxContainer? section, string text)
    {
        if (section == null)
        {
            return;
        }

        Label label = UiSceneTemplates.InstantiateMessageLabel();
        label.Text = text;
        section.AddChild(label);
    }

    private void AddOpenViewerButton()
    {
        if (_openViewerButton != null)
        {
            _openViewerButton.Visible = true;
        }
    }

    private void RemoveOpenViewerButton()
    {
        if (_openViewerButton != null)
        {
            _openViewerButton.Visible = false;
        }
    }

    private void ResetOpenViewerButtonState()
    {
        if (_openViewerButton != null)
        {
            _openViewerButton.Visible = false;
        }
    }

    private void OnOpenViewerPressed()
    {
        if (_selectedBody != null)
        {
            EmitSignal(SignalName.OpenInViewerRequested, _selectedBody);
        }
    }

    private void EnsureUi()
    {
        if (_overviewSection == null || _bodySection == null || _openViewerButton == null)
        {
            CacheUi();
        }

        if (_overviewSection == null || _bodySection == null || _openViewerButton == null)
        {
            throw new System.InvalidOperationException("SystemInspectorPanel scene is missing required inspector nodes.");
        }
    }

    private readonly struct OrbitPreviewEntry
    {
        public OrbitPreviewEntry(double distanceM, CelestialBody? body, AsteroidBelt? belt)
        {
            DistanceM = distanceM;
            Body = body;
            Belt = belt;
        }

        public double DistanceM { get; }

        public CelestialBody? Body { get; }

        public AsteroidBelt? Belt { get; }
    }
}
