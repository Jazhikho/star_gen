using Godot;
using StarGen.App.Viewer;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Domain.Math;
using StarGen.Domain.Population;
using StarGen.Domain.Systems;
using System.Globalization;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Side panel for the solar-system viewer.
/// </summary>
public partial class SystemInspectorPanel : VBoxContainer
{
    /// <summary>
    /// Emitted when the user requests to open a body in the object viewer.
    /// </summary>
    [Signal]
    public delegate void OpenInViewerRequestedEventHandler(CelestialBody body);

    private VBoxContainer? _overviewSection;
    private VBoxContainer? _bodySection;
    private Button? _openViewerButton;
    private SolarSystem? _currentSystem;
    private CelestialBody? _selectedBody;

    /// <summary>
    /// Builds the panel UI on scene entry.
    /// </summary>
    public override void _Ready()
    {
        BuildUi();
    }

    /// <summary>
    /// Displays system overview information.
    /// </summary>
    public void DisplaySystem(SolarSystem? system, SolarSystemSpec? spec = null)
    {
        EnsureUi();

        _currentSystem = system;
        _selectedBody = null;
        ResetOpenViewerButtonState();
        ClearSectionContent(_overviewSection);
        ClearSectionContent(_bodySection);
        AddProperty(_bodySection, "Status", "Click a body to inspect");

        if (system == null)
        {
            AddProperty(_overviewSection, "Status", "No system generated");
            return;
        }

        AddProperty(_overviewSection, "Name", system.Name);
        AddProperty(_overviewSection, "Stars", system.GetStarCount().ToString(CultureInfo.InvariantCulture));
        AddProperty(_overviewSection, "Planets", system.GetPlanetCount().ToString(CultureInfo.InvariantCulture));
        AddProperty(_overviewSection, "Moons", system.GetMoonCount().ToString(CultureInfo.InvariantCulture));
        AddProperty(_overviewSection, "Asteroids", system.GetAsteroidCount().ToString(CultureInfo.InvariantCulture));
        AddProperty(_overviewSection, "Asteroid Belts", system.AsteroidBelts.Count.ToString(CultureInfo.InvariantCulture));

        AddSeparator(_overviewSection);
        AddHeader(_overviewSection, "Population");
        if (system.IsInhabited())
        {
            AddProperty(_overviewSection, "Inhabited", "Yes");
            AddProperty(_overviewSection, "Total Pop.", PropertyFormatter.FormatPopulation(system.GetTotalPopulation()));
            AddProperty(_overviewSection, "  Native", PropertyFormatter.FormatPopulation(system.GetNativePopulation()));
            AddProperty(_overviewSection, "  Colony", PropertyFormatter.FormatPopulation(system.GetColonyPopulation()));
        }
        else
        {
            AddProperty(_overviewSection, "Inhabited", "No");
            AddProperty(_overviewSection, "Total Pop.", "0");
        }

        if (system.GetStars().Count > 0)
        {
            AddSeparator(_overviewSection);
            AddHeader(_overviewSection, "Stars");
            foreach (CelestialBody star in system.GetStars())
            {
                AddProperty(_overviewSection, star.Name, FormatStarInfo(star));
            }
        }

        if (system.OrbitHosts.Count > 0)
        {
            AddSeparator(_overviewSection);
            AddHeader(_overviewSection, "Orbit Hosts");
            foreach (OrbitHost host in system.OrbitHosts)
            {
                string hostInfo = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} ({1:0.00} - {2:0.00} AU)",
                    host.GetTypeString(),
                    host.InnerStabilityM / Units.AuMeters,
                    host.OuterStabilityM / Units.AuMeters);
                AddProperty(_overviewSection, host.NodeId, hostInfo);
            }
        }

        if (system.AsteroidBelts.Count > 0)
        {
            AddSeparator(_overviewSection);
            AddHeader(_overviewSection, "Asteroid Belts");
            foreach (AsteroidBelt belt in system.AsteroidBelts)
            {
                string beltInfo = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} | {1:0.00}-{2:0.00} AU | center {3:0.00} AU | majors {4}",
                    belt.GetCompositionString(),
                    belt.InnerRadiusM / Units.AuMeters,
                    belt.OuterRadiusM / Units.AuMeters,
                    belt.GetCenterAu(),
                    belt.GetMajorAsteroidCount());
                AddProperty(_overviewSection, belt.Name, beltInfo);
            }
        }

        if (spec != null && (spec.UseCaseSettings.IsTravellerMode() || spec.UseCaseSettings.ShowTravellerReadouts))
        {
            AddTravellerSummary(system, spec);
        }
    }

    /// <summary>
    /// Displays details for a selected body.
    /// </summary>
    public void DisplaySelectedBody(CelestialBody? body)
    {
        EnsureUi();

        _selectedBody = body;
        ResetOpenViewerButtonState();
        ClearSectionContent(_bodySection);

        if (body == null)
        {
            AddProperty(_bodySection, "Status", "Click a body to inspect");
            RemoveOpenViewerButton();
            return;
        }

        AddProperty(_bodySection, "Name", body.Name);
        AddProperty(_bodySection, "Type", GetTypeDisplay(body.Type));
        AddProperty(_bodySection, "ID", body.Id);

        AddSeparator(_bodySection);
        AddHeader(_bodySection, "Physical");
        AddPhysicalProperties(body);

        if (body.HasOrbital())
        {
            AddSeparator(_bodySection);
            AddHeader(_bodySection, "Orbital");
            AddOrbitalProperties(body);
        }

        if (body.HasStellar())
        {
            AddSeparator(_bodySection);
            AddHeader(_bodySection, "Stellar");
            AddStellarProperties(body);
        }

        if (body.HasAtmosphere())
        {
            AddSeparator(_bodySection);
            AddHeader(_bodySection, "Atmosphere");
            AddAtmosphereProperties(body);
        }

        if (body.HasPopulationData())
        {
            AddSeparator(_bodySection);
            AddHeader(_bodySection, "Population");
            AddPopulationSummary(body);
        }

        AddOpenViewerButton();
    }

    /// <summary>
    /// Displays details for a selected asteroid belt.
    /// </summary>
    public void DisplaySelectedBelt(AsteroidBelt? belt, SolarSystem? system)
    {
        EnsureUi();

        _selectedBody = null;
        ResetOpenViewerButtonState();
        ClearSectionContent(_bodySection);

        if (belt == null)
        {
            AddProperty(_bodySection, "Status", "Click a body to inspect");
            RemoveOpenViewerButton();
            return;
        }

        AddProperty(_bodySection, "Name", belt.Name);
        AddProperty(_bodySection, "Type", "Asteroid Belt");
        AddProperty(_bodySection, "ID", belt.Id);

        AddSeparator(_bodySection);
        AddHeader(_bodySection, "Orbital Extent");
        AddProperty(_bodySection, "Inner Edge", string.Format(CultureInfo.InvariantCulture, "{0:0.0000} AU", belt.InnerRadiusM / Units.AuMeters));
        AddProperty(_bodySection, "Outer Edge", string.Format(CultureInfo.InvariantCulture, "{0:0.0000} AU", belt.OuterRadiusM / Units.AuMeters));
        AddProperty(_bodySection, "Center", string.Format(CultureInfo.InvariantCulture, "{0:0.0000} AU", belt.GetCenterAu()));
        AddProperty(_bodySection, "Width", string.Format(CultureInfo.InvariantCulture, "{0:0.0000} AU", belt.GetWidthAu()));

        AddSeparator(_bodySection);
        AddHeader(_bodySection, "Properties");
        AddProperty(_bodySection, "Composition", belt.GetCompositionString());
        AddProperty(_bodySection, "Total Mass", FormatMassKg(belt.TotalMassKg));
        AddProperty(_bodySection, "Major Bodies", belt.GetMajorAsteroidCount().ToString(CultureInfo.InvariantCulture));

        if (system != null && belt.MajorAsteroidIds.Count > 0)
        {
            AddSeparator(_bodySection);
            AddHeader(_bodySection, "Major Asteroids");
            foreach (string asteroidId in belt.MajorAsteroidIds)
            {
                CelestialBody? asteroid = system.GetBody(asteroidId);
                if (asteroid == null)
                {
                    continue;
                }

                double radiusKm = asteroid.Physical.RadiusM / 1000.0;
                AddProperty(
                    _bodySection,
                    asteroid.Name,
                    string.Format(CultureInfo.InvariantCulture, "{0:0} km radius", radiusKm));
            }
        }

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
        ResetOpenViewerButtonState();
        ClearSectionContent(_overviewSection);
        ClearSectionContent(_bodySection);
        AddProperty(_overviewSection, "Status", "No system generated");
        AddProperty(_bodySection, "Status", "Click a body to inspect");
    }

    /// <summary>
    /// Builds the UI structure if needed.
    /// </summary>
    private void BuildUi()
    {
        if (_overviewSection != null && _bodySection != null)
        {
            return;
        }

        _overviewSection = CreateSection("System Overview");
        AddChild(_overviewSection);
        AddChild(new HSeparator());

        _bodySection = CreateSection("Selected Body");
        AddChild(_bodySection);
    }

    /// <summary>
    /// Adds physical property rows for a body.
    /// </summary>
    private void AddPhysicalProperties(CelestialBody body)
    {
        PhysicalProps physical = body.Physical;
        AddProperty(_bodySection, "Mass", PropertyFormatter.FormatMass(physical.MassKg, body.Type));
        AddProperty(_bodySection, "Radius", PropertyFormatter.FormatRadius(physical.RadiusM, body.Type));
        AddProperty(
            _bodySection,
            "Density",
            string.Format(CultureInfo.InvariantCulture, "{0:0.0} kg/m^3", physical.GetDensityKgM3()));

        if (physical.RotationPeriodS != 0.0)
        {
            double hours = System.Math.Abs(physical.RotationPeriodS) / 3600.0;
            string retrograde;
            if (physical.RotationPeriodS < 0.0)
            {
                retrograde = " (retrograde)";
            }
            else
            {
                retrograde = string.Empty;
            }
            AddProperty(
                _bodySection,
                "Rotation",
                string.Format(CultureInfo.InvariantCulture, "{0:0.0} hours{1}", hours, retrograde));
        }

        if (physical.AxialTiltDeg != 0.0)
        {
            AddProperty(
                _bodySection,
                "Axial Tilt",
                string.Format(CultureInfo.InvariantCulture, "{0:0.0}\u00B0", physical.AxialTiltDeg));
        }
    }

    /// <summary>
    /// Formats a mass value for belt display.
    /// </summary>
    private static string FormatMassKg(double massKg)
    {
        if (massKg >= 1.0e24)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.00} M\u2295", massKg / Units.EarthMassKg);
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0.00e+0} kg", massKg);
    }

    /// <summary>
    /// Adds orbital property rows for a body.
    /// </summary>
    private void AddOrbitalProperties(CelestialBody body)
    {
        if (!body.HasOrbital() || body.Orbital == null)
        {
            return;
        }

        OrbitalProps orbital = body.Orbital;
        double semiMajorAxisAu = orbital.SemiMajorAxisM / Units.AuMeters;
        if (semiMajorAxisAu > 0.01)
        {
            AddProperty(
                _bodySection,
                "Semi-major Axis",
                string.Format(CultureInfo.InvariantCulture, "{0:0.0000} AU", semiMajorAxisAu));
        }
        else
        {
            AddProperty(
                _bodySection,
                "Semi-major Axis",
                string.Format(CultureInfo.InvariantCulture, "{0:0} km", orbital.SemiMajorAxisM / 1000.0));
        }

        AddProperty(_bodySection, "Eccentricity", string.Format(CultureInfo.InvariantCulture, "{0:0.0000}", orbital.Eccentricity));
        AddProperty(_bodySection, "Inclination", string.Format(CultureInfo.InvariantCulture, "{0:0.00}\u00B0", orbital.InclinationDeg));
        if (!string.IsNullOrEmpty(orbital.ParentId))
        {
            AddProperty(_bodySection, "Orbits", orbital.ParentId);
        }
    }

    /// <summary>
    /// Adds stellar property rows for a body.
    /// </summary>
    private void AddStellarProperties(CelestialBody body)
    {
        if (!body.HasStellar() || body.Stellar == null)
        {
            return;
        }

        StellarProps stellar = body.Stellar;
        AddProperty(_bodySection, "Spectral Class", stellar.SpectralClass);
        AddProperty(_bodySection, "Temperature", string.Format(CultureInfo.InvariantCulture, "{0} K", (int)stellar.EffectiveTemperatureK));
        AddProperty(
            _bodySection,
            "Luminosity",
            string.Format(CultureInfo.InvariantCulture, "{0:0.0000} L\u2609", stellar.LuminosityWatts / StellarProps.SolarLuminosityWatts));

        if (stellar.AgeYears > 0.0)
        {
            AddProperty(
                _bodySection,
                "Age",
                string.Format(CultureInfo.InvariantCulture, "{0:0.00} Gyr", stellar.AgeYears / 1.0e9));
        }
    }

    /// <summary>
    /// Adds atmosphere property rows for a body.
    /// </summary>
    private void AddAtmosphereProperties(CelestialBody body)
    {
        if (!body.HasAtmosphere() || body.Atmosphere == null)
        {
            return;
        }

        AtmosphereProps atmosphere = body.Atmosphere;
        AddProperty(
            _bodySection,
            "Pressure",
            string.Format(CultureInfo.InvariantCulture, "{0:0.0000} atm", atmosphere.SurfacePressurePa / 101325.0));

        if (body.HasSurface() && body.Surface != null && body.Surface.TemperatureK > 0.0)
        {
            AddProperty(
                _bodySection,
                "Temperature",
                string.Format(CultureInfo.InvariantCulture, "{0:0} K", body.Surface.TemperatureK));
        }

        if (atmosphere.GreenhouseFactor > 1.0)
        {
            AddProperty(
                _bodySection,
                "Greenhouse",
                string.Format(CultureInfo.InvariantCulture, "{0:0.00}x", atmosphere.GreenhouseFactor));
        }
    }

    /// <summary>
    /// Adds population summary rows for a body.
    /// </summary>
    private void AddPopulationSummary(CelestialBody body)
    {
        if (body.PopulationData == null)
        {
            return;
        }

        PlanetPopulationData populationData = body.PopulationData;
        if (populationData.Profile != null)
        {
            AddProperty(
                _bodySection,
                "Habitability",
                PropertyFormatter.FormatHabitability(populationData.Profile.HabitabilityScore));
        }

        if (populationData.Suitability != null)
        {
            AddProperty(
                _bodySection,
                "Suitability",
                PropertyFormatter.FormatSuitability(populationData.Suitability.OverallScore));
        }

        AddProperty(
            _bodySection,
            "Status",
            PropertyFormatter.FormatPoliticalSituation(GetPoliticalSituation(populationData)));

        int totalPopulation = populationData.GetTotalPopulation();
        if (totalPopulation > 0)
        {
            AddProperty(_bodySection, "Total Pop.", PropertyFormatter.FormatPopulation(totalPopulation));
            AddProperty(_bodySection, "Dominant", GetDominantPopulationName(populationData));
        }
    }

    /// <summary>
    /// Adds Traveller-oriented mainworld readiness summary rows.
    /// </summary>
    private void AddTravellerSummary(SolarSystem system, SolarSystemSpec spec)
    {
        AddSeparator(_overviewSection);
        AddHeader(_overviewSection, "Traveller");
        AddProperty(_overviewSection, "Ruleset", GetRulesetLabel(spec.UseCaseSettings.RulesetMode));
        AddProperty(_overviewSection, "Mainworld Policy", GetMainworldPolicyLabel(spec.UseCaseSettings.MainworldPolicy));

        TravellerMainworldSelector.SelectionResult selection = TravellerMainworldSelector.Select(system);
        if (!selection.HasCandidate() || selection.Body == null)
        {
            AddProperty(_overviewSection, "Mainworld", "No candidate");
            AddProperty(_overviewSection, "Readiness", "Mainworld candidate missing");
            AddProperty(_overviewSection, "Reason", selection.Reason);
            return;
        }

        AddProperty(_overviewSection, "Mainworld", selection.Body.Name);
        AddProperty(_overviewSection, "Reason", selection.Reason);
        AddProperty(_overviewSection, "Population", PropertyFormatter.FormatPopulation(selection.Population));
        AddProperty(_overviewSection, "Habitability", PropertyFormatter.FormatHabitability(selection.HabitabilityScore));
        AddProperty(_overviewSection, "Suitability", PropertyFormatter.FormatSuitability(selection.SuitabilityScore));
    }

    /// <summary>
    /// Derives the current political situation for display.
    /// </summary>
    private static string GetPoliticalSituation(PlanetPopulationData populationData)
    {
        int nativePopulation = populationData.GetNativePopulation();
        int colonyPopulation = populationData.GetColonyPopulation();
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

        foreach (Colony colony in populationData.Colonies)
        {
            if (colony.IsActive && colony.HasHostileNativeRelations())
            {
                return "conflict";
            }
        }

        return "coexisting";
    }

    /// <summary>
    /// Returns the largest active population name.
    /// </summary>
    private static string GetDominantPopulationName(PlanetPopulationData populationData)
    {
        string dominantName = "Unknown";
        int dominantCount = 0;

        foreach (NativePopulation nativePopulation in populationData.NativePopulations)
        {
            if (nativePopulation.IsExtant && nativePopulation.Population > dominantCount)
            {
                dominantCount = nativePopulation.Population;
                dominantName = nativePopulation.Name;
            }
        }

        foreach (Colony colony in populationData.Colonies)
        {
            if (colony.IsActive && colony.Population > dominantCount)
            {
                dominantCount = colony.Population;
                dominantName = colony.Name;
            }
        }

        return dominantName;
    }

    /// <summary>
    /// Formats star info for overview display.
    /// </summary>
    private static string FormatStarInfo(CelestialBody star)
    {
        if (star.HasStellar() && star.Stellar != null)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} ({1:0} K)",
                star.Stellar.SpectralClass,
                star.Stellar.EffectiveTemperatureK);
        }

        return "Unknown type";
    }

    /// <summary>
    /// Gets the display string for a body type.
    /// </summary>
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

    private static string GetRulesetLabel(GenerationUseCaseSettings.RulesetModeType rulesetMode)
    {
        if (rulesetMode == GenerationUseCaseSettings.RulesetModeType.Traveller)
        {
            return "Traveller";
        }

        return "Default";
    }

    private static string GetMainworldPolicyLabel(GenerationUseCaseSettings.MainworldPolicyType policy)
    {
        if (policy == GenerationUseCaseSettings.MainworldPolicyType.Prefer)
        {
            return "Prefer";
        }

        if (policy == GenerationUseCaseSettings.MainworldPolicyType.Require)
        {
            return "Require";
        }

        return "None";
    }

    /// <summary>
    /// Creates a section container with a header label.
    /// </summary>
    private static VBoxContainer CreateSection(string title)
    {
        VBoxContainer section = new();
        section.AddThemeConstantOverride("separation", 4);

        Label header = new();
        header.Text = title;
        header.AddThemeFontSizeOverride("font_size", 14);
        header.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
        section.AddChild(header);

        return section;
    }

    /// <summary>
    /// Clears all section content while preserving the header.
    /// </summary>
    private static void ClearSectionContent(VBoxContainer? section)
    {
        if (section == null)
        {
            return;
        }

        while (section.GetChildCount() > 1)
        {
            Node child = section.GetChild(section.GetChildCount() - 1);
            section.RemoveChild(child);
            child.QueueFree();
        }
    }

    /// <summary>
    /// Adds one property row to a section.
    /// </summary>
    private static void AddProperty(VBoxContainer? section, string labelText, string valueText)
    {
        if (section == null)
        {
            return;
        }

        HBoxContainer row = new();

        Label label = new();
        label.Text = labelText + ":";
        label.CustomMinimumSize = new Vector2(100.0f, 0.0f);
        label.AddThemeFontSizeOverride("font_size", 12);
        label.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        row.AddChild(label);

        Label value = new();
        value.Text = valueText;
        value.AddThemeFontSizeOverride("font_size", 12);
        value.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        value.AutowrapMode = TextServer.AutowrapMode.Word;
        row.AddChild(value);

        section.AddChild(row);
    }

    /// <summary>
    /// Adds a sub-header label to a section.
    /// </summary>
    private static void AddHeader(VBoxContainer? section, string text)
    {
        if (section == null)
        {
            return;
        }

        Label header = new();
        header.Text = text;
        header.AddThemeFontSizeOverride("font_size", 12);
        header.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.5f));
        section.AddChild(header);
    }

    /// <summary>
    /// Adds a separator to a section.
    /// </summary>
    private static void AddSeparator(VBoxContainer? section)
    {
        if (section != null)
        {
            section.AddChild(new HSeparator());
        }
    }

    /// <summary>
    /// Adds the open-in-viewer button.
    /// </summary>
    private void AddOpenViewerButton()
    {
        RemoveOpenViewerButton();
        if (_bodySection == null)
        {
            return;
        }

        Control spacer = new()
        {
            CustomMinimumSize = new Vector2(0.0f, 5.0f),
        };
        _bodySection.AddChild(spacer);

        _openViewerButton = new Button
        {
            Text = "Open in Object Viewer",
            TooltipText = "View this body in detail",
        };
        _openViewerButton.Pressed += OnOpenViewerPressed;
        _bodySection.AddChild(_openViewerButton);
    }

    /// <summary>
    /// Removes the open-in-viewer button.
    /// </summary>
    private void RemoveOpenViewerButton()
    {
        if (_openViewerButton == null)
        {
            return;
        }

        if (!GodotObject.IsInstanceValid(_openViewerButton))
        {
            _openViewerButton = null;
            return;
        }

        _openViewerButton.Pressed -= OnOpenViewerPressed;
        _openViewerButton.QueueFree();
        _openViewerButton = null;
    }

    /// <summary>
    /// Clears the tracked open-in-viewer button reference before section rebuilds.
    /// </summary>
    private void ResetOpenViewerButtonState()
    {
        if (_openViewerButton == null)
        {
            return;
        }

        if (!GodotObject.IsInstanceValid(_openViewerButton))
        {
            _openViewerButton = null;
            return;
        }

        _openViewerButton.Pressed -= OnOpenViewerPressed;
        _openViewerButton = null;
    }

    /// <summary>
    /// Handles the open-in-viewer button press.
    /// </summary>
    private void OnOpenViewerPressed()
    {
        if (_selectedBody != null)
        {
            EmitSignal(SignalName.OpenInViewerRequested, _selectedBody);
        }
    }

    /// <summary>
    /// Ensures the panel UI exists before display calls.
    /// </summary>
    private void EnsureUi()
    {
        BuildUi();
    }
}
