using System;
using Godot;
using StarGen.App.Components;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Traveller;
using OrbitZoneArchetype = StarGen.Domain.Generation.Archetypes.OrbitZone;
using RingComplexityArchetype = StarGen.Domain.Generation.Archetypes.RingComplexity;
using SizeCategoryArchetype = StarGen.Domain.Generation.Archetypes.SizeCategory;

namespace StarGen.App.Viewer;

/// <summary>
/// C# inspector panel for the external object-viewer path.
/// </summary>
public partial class InspectorPanel : VBoxContainer
{
	/// <summary>
	/// Emitted when the user selects a moon or clears moon focus.
	/// </summary>
	[Signal]
	public delegate void MoonSelectedEventHandler(Variant moon);

	/// <summary>
	/// Emitted when the user requests editing for the currently displayed body.
	/// </summary>
	[Signal]
	public delegate void EditRequestedEventHandler();

	private VBoxContainer? _inspectorContainer;
	private VBoxContainer? _currentSectionContent;

	/// <summary>
	/// Caches the dynamic content container.
	/// </summary>
	public override void _Ready()
	{
		EnsureInspectorContainer();
	}

	private void EnsureInspectorContainer()
	{
		if (_inspectorContainer == null)
		{
			_inspectorContainer = GetNodeOrNull<VBoxContainer>("InspectorContainer");
		}
	}

	/// <summary>
	/// GDScript-compatible clear wrapper.
	/// </summary>
	public void clear()
	{
		Clear();
	}

	/// <summary>
	/// Clears dynamic inspector content.
	/// </summary>
	public void Clear()
	{
		EnsureInspectorContainer();
		if (_inspectorContainer == null)
		{
			return;
		}

		foreach (Node child in _inspectorContainer.GetChildren())
		{
			child.QueueFree();
		}

		_currentSectionContent = null;
	}

	/// <summary>
	/// GDScript-compatible body display wrapper.
	/// </summary>
	public void display_body_with_moons(Variant bodyVariant, Godot.Collections.Array moons)
	{
		CelestialBody? body = ConvertVariantToCelestialBody(bodyVariant);
		Godot.Collections.Array<CelestialBody> typedMoons = ConvertVariantArrayToBodies(moons);
		DisplayBodyWithMoons(body, typedMoons, moons);
	}

	/// <summary>
	/// Displays a body and optional moon list.
	/// </summary>
	public void DisplayBodyWithMoons(
		CelestialBody? body,
		Godot.Collections.Array<CelestialBody> moons,
		Godot.Collections.Array? originalMoonVariants = null)
	{
		Clear();
		EnsureInspectorContainer();
		if (_inspectorContainer == null)
		{
			return;
		}

		if (body == null)
		{
			AddInfoLabel("No object loaded");
			return;
		}

		if (moons.Count > 0)
		{
			AddMoonListSection(moons, originalMoonVariants ?? BuildVariantArray(moons), null);
		}

		AddBodySummarySection(body);
	}

	/// <summary>
	/// GDScript-compatible focused-moon display wrapper.
	/// </summary>
	public void display_focused_moon(Variant moonVariant, Variant planetVariant, Godot.Collections.Array allMoons)
	{
		CelestialBody? moon = ConvertVariantToCelestialBody(moonVariant);
		CelestialBody? planet = ConvertVariantToCelestialBody(planetVariant);
		Godot.Collections.Array<CelestialBody> typedMoons = ConvertVariantArrayToBodies(allMoons);
		DisplayFocusedMoon(moon, planet, typedMoons, allMoons);
	}

	/// <summary>
	/// Displays a focused moon view with a back button.
	/// </summary>
	public void DisplayFocusedMoon(
		CelestialBody? moon,
		CelestialBody? planet,
		Godot.Collections.Array<CelestialBody> allMoons,
		Godot.Collections.Array? originalMoonVariants = null)
	{
		Clear();
		EnsureInspectorContainer();
		if (_inspectorContainer == null || moon == null)
		{
			return;
		}

		AddBackToPlanetButton(planet);
		AddBodySummarySection(moon, $"Moon: {moon.Name}");

		if (allMoons.Count > 1)
		{
			AddMoonListSection(allMoons, originalMoonVariants ?? BuildVariantArray(allMoons), moon);
		}

		if (planet != null)
		{
			AddBodySummarySection(planet, $"Parent: {planet.Name}");
		}
	}

	private void AddBodySummarySection(CelestialBody body, string? headerOverride = null)
	{
		BeginSection(headerOverride ?? "Body");
		string nameValue;
		if (string.IsNullOrEmpty(body.Name))
		{
			nameValue = body.Id;
		}
		else
		{
			nameValue = body.Name;
		}

		AddProperty("Name", nameValue);
		AddProperty("Type", body.GetTypeString());
		AddProperty("ID", body.Id);
		AddPhysicalSummary(body.Physical);

		if (body.HasStellar() && body.Stellar != null)
		{
			AddProperty("Spectral Class", body.Stellar.SpectralClass);
			AddProperty("Temperature", $"{body.Stellar.EffectiveTemperatureK:0} K");
		}

		AddOrbitalSummary(body);
		AddSurfaceSummary(body);
		AddAtmosphereSummary(body);
		AddRingSummary(body);
		AddTravellerReadout(body);
		AddPopulationSummary(body);
		AddValidationSummary(body);
		AddEditButton();
	}

	private void AddPhysicalSummary(PhysicalProps physical)
	{
		AddProperty("Mass", $"{physical.MassKg:0.###e0} kg");
		AddProperty("Radius", FormatDistance(physical.RadiusM));
		AddProperty("Density", $"{physical.GetDensityKgM3():0.0} kg/m^3");
		AddProperty("Gravity", $"{physical.GetSurfaceGravityMS2():0.00} m/s^2");
		AddProperty("Escape Velocity", $"{physical.GetEscapeVelocityMS() / 1000.0:0.00} km/s");
	}

	private void AddMoonListSection(
		Godot.Collections.Array<CelestialBody> moons,
		Godot.Collections.Array originalMoonVariants,
		CelestialBody? focusedMoon)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		VBoxContainer section = UiSceneTemplates.InstantiateSection();
		Label title = UiSceneTemplates.GetRequiredChild<Label>(section, "TitleLabel");
		VBoxContainer content = UiSceneTemplates.GetRequiredChild<VBoxContainer>(section, "Content");
		title.Text = $"Moons ({moons.Count})";

		for (int index = 0; index < moons.Count; index++)
		{
			CelestialBody moon = moons[index];
			Button button = UiSceneTemplates.InstantiateActionButton();
			if (focusedMoon != null && moon.Id == focusedMoon.Id)
			{
				button.Text = $"* {moon.Name}";
			}
			else
			{
				button.Text = moon.Name;
			}
			button.Flat = true;
			button.Alignment = HorizontalAlignment.Left;
			Variant emitValue;
			if (index < originalMoonVariants.Count)
			{
				emitValue = (Variant)originalMoonVariants[index];
			}
			else
			{
				emitValue = Variant.From((GodotObject?)null);
			}
			button.Pressed += () => EmitSignal(SignalName.MoonSelected, emitValue);
			button.Alignment = HorizontalAlignment.Left;
			content.AddChild(button);
		}

		_inspectorContainer.AddChild(section);
	}

	private void AddBackToPlanetButton(CelestialBody? planet)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		Button button = UiSceneTemplates.InstantiateActionButton();
		if (planet == null)
		{
			button.Text = "Back to Planet";
		}
		else
		{
			button.Text = $"Back to {planet.Name}";
		}
		button.Pressed += () => EmitSignal(SignalName.MoonSelected, new Variant());
		_inspectorContainer.AddChild(button);
	}

	private void BeginSection(string title)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		VBoxContainer section = UiSceneTemplates.InstantiateSection();
		Label titleLabel = UiSceneTemplates.GetRequiredChild<Label>(section, "TitleLabel");
		VBoxContainer content = UiSceneTemplates.GetRequiredChild<VBoxContainer>(section, "Content");
		titleLabel.Text = title;
		_inspectorContainer.AddChild(section);
		_currentSectionContent = content;
	}

	private void AddProperty(string labelText, string valueText)
	{
		VBoxContainer? targetContainer = _currentSectionContent ?? _inspectorContainer;
		if (targetContainer == null)
		{
			return;
		}

		HBoxContainer row = UiSceneTemplates.InstantiatePropertyRow();
		Label label = UiSceneTemplates.GetRequiredChild<Label>(row, "Key");
		Label value = UiSceneTemplates.GetRequiredChild<Label>(row, "Value");
		label.Text = $"{labelText}:";
		value.Text = valueText;
		targetContainer.AddChild(row);
	}

	private void AddInfoLabel(string text)
	{
		VBoxContainer? targetContainer = _currentSectionContent ?? _inspectorContainer;
		if (targetContainer == null)
		{
			return;
		}

		Label label = UiSceneTemplates.InstantiateMessageLabel();
		label.Text = text;
		targetContainer.AddChild(label);
	}

	private void AddTravellerReadout(CelestialBody body)
	{
		if (_inspectorContainer == null || body.Provenance == null || body.Provenance.SpecSnapshot.Count == 0)
		{
			return;
		}

		GenerationUseCaseSettings? settings = ResolveUseCaseSettings(body);
		bool shouldShowTravellerReadout = settings != null
			? settings.ShowTravellerReadouts || settings.IsTravellerMode()
			: ResolveLegacyTravellerInspectorVisibility(body);
		if (!shouldShowTravellerReadout)
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

		BeginSection("Traveller");
		if (settings != null)
		{
			AddProperty("Ruleset", GenerationUseCasePresentation.GetRulesetLabel(settings.RulesetMode));
		}
		AddProperty("UWP", profile.ToUwpString());
		AddProperty("Size Code", TravellerWorldProfile.ToHexDigit(profile.SizeCode));
		AddProperty("Atmosphere Code", TravellerWorldProfile.ToHexDigit(profile.AtmosphereCode));
		AddProperty("Hydrographics Code", TravellerWorldProfile.ToHexDigit(profile.HydrographicsCode));
		AddProperty("Population Code", TravellerWorldProfile.ToHexDigit(profile.PopulationCode));
		AddProperty("Government Code", TravellerWorldProfile.ToHexDigit(profile.GovernmentCode));
		AddProperty("Law Code", TravellerWorldProfile.ToHexDigit(profile.LawCode));
		AddProperty("Tech Level", TravellerWorldProfile.ToHexDigit(profile.TechLevelCode));
		TravellerTradeCodeSet? tradeCodes = TryGetStoredTradeCodes(body);
		if (tradeCodes != null)
		{
			AddProperty("Trade Codes", tradeCodes.ToDisplayString());
		}

		string travelZone = TryGetStoredTravelZone(body);
		if (!string.IsNullOrEmpty(travelZone))
		{
			AddProperty("Travel Zone", travelZone);
		}
	}

	private static GenerationUseCaseSettings? ResolveUseCaseSettings(CelestialBody body)
	{
		if (body.Provenance == null || !body.Provenance.SpecSnapshot.ContainsKey("use_case_settings"))
		{
			return null;
		}

		Variant settingsVariant = body.Provenance.SpecSnapshot["use_case_settings"];
		if (settingsVariant.VariantType != Variant.Type.Dictionary)
		{
			return null;
		}

		return GenerationUseCaseSettings.FromDictionary((Godot.Collections.Dictionary)settingsVariant);
	}

	private static bool ResolveLegacyTravellerInspectorVisibility(CelestialBody body)
	{
		if (body.Provenance == null)
		{
			return false;
		}

		if (body.Provenance.SpecSnapshot.ContainsKey("show_traveller_readouts"))
		{
			Variant showVariant = body.Provenance.SpecSnapshot["show_traveller_readouts"];
			if (showVariant.VariantType == Variant.Type.Bool && (bool)showVariant)
			{
				return true;
			}
		}

		if (!body.Provenance.SpecSnapshot.ContainsKey("ruleset_mode"))
		{
			return false;
		}

		Variant rulesetVariant = body.Provenance.SpecSnapshot["ruleset_mode"];
		if (rulesetVariant.VariantType != Variant.Type.Int)
		{
			return false;
		}

		int rulesetValue = (int)rulesetVariant;
		return rulesetValue == (int)GenerationUseCaseSettings.RulesetModeType.Traveller;
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

	private static string TryGetStoredTravelZone(CelestialBody body)
	{
		if (body.Provenance == null || !body.Provenance.SpecSnapshot.ContainsKey("traveller_travel_zone"))
		{
			return string.Empty;
		}

		Variant travelZoneVariant = body.Provenance.SpecSnapshot["traveller_travel_zone"];
		if (travelZoneVariant.VariantType == Variant.Type.String)
		{
			return (string)travelZoneVariant;
		}

		return string.Empty;
	}

	private void AddOrbitalSummary(CelestialBody body)
	{
		if (_inspectorContainer == null || !body.HasOrbital() || body.Orbital == null)
		{
			return;
		}

		BeginSection("Orbit");
		AddProperty("Semi-major Axis", FormatDistance(body.Orbital.SemiMajorAxisM));
		AddProperty("Eccentricity", $"{body.Orbital.Eccentricity:0.0000}");
		AddProperty("Periapsis", FormatDistance(body.Orbital.GetPeriapsisM()));
		AddProperty("Apoapsis", FormatDistance(body.Orbital.GetApoapsisM()));
		AddProperty("Inclination", $"{body.Orbital.InclinationDeg:0.00} deg");
		if (!string.IsNullOrWhiteSpace(body.Orbital.ParentId))
		{
			AddProperty("Parent", body.Orbital.ParentId);
		}
	}

	private void AddSurfaceSummary(CelestialBody body)
	{
		if (_inspectorContainer == null || !body.HasSurface() || body.Surface == null)
		{
			return;
		}

		BeginSection("Surface");
		AddProperty("Temperature", $"{body.Surface.TemperatureK:0.0} K");
		AddProperty("Albedo", $"{body.Surface.Albedo:0.00}");
		if (!string.IsNullOrWhiteSpace(body.Surface.SurfaceType))
		{
			AddProperty("Surface Type", body.Surface.SurfaceType);
		}
		AddProperty("Volcanism", $"{body.Surface.VolcanismLevel:0.00}");

		if (body.Surface.HasTerrain() && body.Surface.Terrain != null)
		{
			AddProperty("Terrain", body.Surface.Terrain.TerrainType);
			AddProperty("Elevation Range", FormatDistance(body.Surface.Terrain.ElevationRangeM));
			AddProperty("Tectonics", $"{body.Surface.Terrain.TectonicActivity:0.00}");
		}

		if (body.Surface.HasHydrosphere() && body.Surface.Hydrosphere != null)
		{
			AddProperty("Ocean Coverage", $"{body.Surface.Hydrosphere.OceanCoverage * 100.0:0.#}%");
			AddProperty("Ice Coverage", $"{body.Surface.Hydrosphere.IceCoverage * 100.0:0.#}%");
			AddProperty("Water Type", body.Surface.Hydrosphere.WaterType);
		}

		if (body.Surface.HasCryosphere() && body.Surface.Cryosphere != null)
		{
			AddProperty("Polar Caps", $"{body.Surface.Cryosphere.PolarCapCoverage * 100.0:0.#}%");
			AddProperty("Subsurface Ocean", body.Surface.Cryosphere.HasSubsurfaceOcean ? "Yes" : "No");
			AddProperty("Cryovolcanism", $"{body.Surface.Cryosphere.CryovolcanismLevel:0.00}");
		}
	}

	private void AddAtmosphereSummary(CelestialBody body)
	{
		if (_inspectorContainer == null || !body.HasAtmosphere() || body.Atmosphere == null)
		{
			return;
		}

		BeginSection("Atmosphere");
		AddProperty("Surface Pressure", $"{body.Atmosphere.SurfacePressurePa / 101325.0:0.###} atm");
		AddProperty("Scale Height", FormatDistance(body.Atmosphere.ScaleHeightM));
		AddProperty("Greenhouse", $"{body.Atmosphere.GreenhouseFactor:0.00}x");
		if (body.Atmosphere.Composition.Count > 0)
		{
			AddProperty("Dominant Gas", body.Atmosphere.GetDominantGas());
		}
	}

	private void AddRingSummary(CelestialBody body)
	{
		if (_inspectorContainer == null || !body.HasRingSystem() || body.RingSystem == null)
		{
			return;
		}

		BeginSection("Rings");
		AddProperty("Bands", body.RingSystem.GetBandCount().ToString());
		AddProperty("Inner Radius", FormatDistance(body.RingSystem.GetInnerRadiusM()));
		AddProperty("Outer Radius", FormatDistance(body.RingSystem.GetOuterRadiusM()));
		AddProperty("Plane Tilt", $"{body.RingSystem.InclinationDeg:0.0} deg");
	}

	private void AddPopulationSummary(CelestialBody body)
	{
		if (_inspectorContainer == null || !body.HasPopulationData() || body.PopulationData == null)
		{
			return;
		}

		BeginSection("Population");
		AddProperty("Total Population", PropertyFormatter.FormatPopulation(body.PopulationData.GetTotalPopulation()));
		AddProperty("Situation", body.PopulationData.GetPoliticalSituation());
		AddProperty("Colonies", body.PopulationData.GetActiveColonyCount().ToString());
		AddProperty("Native Groups", body.PopulationData.GetExtantNativeCount().ToString());
		AddProperty("Dominant", body.PopulationData.GetDominantPopulationName());
		if (body.PopulationData.Profile != null)
		{
			AddProperty("Habitability", body.PopulationData.Profile.HabitabilityScore.ToString());
		}
		if (body.PopulationData.Suitability != null)
		{
			AddProperty("Suitability", body.PopulationData.Suitability.OverallScore.ToString());
		}
	}

	private void AddValidationSummary(CelestialBody body)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		ValidationResult validation = CelestialValidator.Validate(body);
		BeginSection("Validation");
		if (validation.IsClean())
		{
			AddInfoLabel("No validation issues");
			return;
		}

		foreach (ValidationError warning in validation.Errors)
		{
			Label label = UiSceneTemplates.InstantiateMessageLabel();
			if (warning.Severity == ValidationError.SeverityLevel.Error)
			{
				label.Text = $"Error: {warning.Message}";
				label.Modulate = new Color(1.0f, 0.45f, 0.45f, 1.0f);
			}
			else
			{
				label.Text = $"Warning: {warning.Message}";
				label.Modulate = new Color(0.85f, 0.7f, 0.3f, 1.0f);
			}

			if (_currentSectionContent != null)
			{
				_currentSectionContent.AddChild(label);
			}
		}
	}

	private void AddEditButton()
	{
		VBoxContainer? targetContainer = _currentSectionContent ?? _inspectorContainer;
		if (targetContainer == null)
		{
			return;
		}

		Button button = UiSceneTemplates.InstantiateActionButton();
		button.Text = "Open Parameter Editor";
		button.TooltipText = "Edit and regenerate this body using validated parameters";
		button.Pressed += () => EmitSignal(SignalName.EditRequested);
		targetContainer.AddChild(button);
	}

	private static string FormatDistance(double meters)
	{
		if (meters >= 1.0e9)
		{
			return $"{meters / 1.0e9:0.###} Gm";
		}

		if (meters >= 1.0e6)
		{
			return $"{meters / 1.0e6:0.###} Mm";
		}

		if (meters >= 1000.0)
		{
			return $"{meters / 1000.0:0.###} km";
		}

		return $"{meters:0.###} m";
	}

	private static string FormatOptionalBool(Variant value)
	{
		if (value.VariantType == Variant.Type.Nil)
		{
			return "Auto";
		}

        if (value.VariantType == Variant.Type.Bool)
        {
            if ((bool)value)
            {
                return "Yes";
            }

            return "None";
        }

		return value.ToString();
	}

	private static string FormatSizeCategory(Variant value)
	{
		if (value.VariantType != Variant.Type.Int)
		{
			return value.ToString();
		}

		int intValue = (int)value;
		if (!Enum.IsDefined(typeof(SizeCategoryArchetype.Category), intValue))
		{
			return intValue.ToString();
		}

		return ((SizeCategoryArchetype.Category)intValue).ToString();
	}

	private static string FormatOrbitZone(Variant value)
	{
		if (value.VariantType != Variant.Type.Int)
		{
			return value.ToString();
		}

		int intValue = (int)value;
		if (!Enum.IsDefined(typeof(OrbitZoneArchetype.Zone), intValue))
		{
			return intValue.ToString();
		}

		return ((OrbitZoneArchetype.Zone)intValue).ToString();
	}

	private static string FormatRingComplexity(Variant value)
	{
		if (value.VariantType != Variant.Type.Int)
		{
			return value.ToString();
		}

		int intValue = (int)value;
		if (intValue < 0)
		{
			return "Auto";
		}

		if (!Enum.IsDefined(typeof(RingComplexityArchetype.Level), intValue))
		{
			return intValue.ToString();
		}

		return ((RingComplexityArchetype.Level)intValue).ToString();
	}

	private static CelestialBody? ConvertVariantToCelestialBody(Variant value)
	{
		if (value.VariantType == Variant.Type.Nil)
		{
			return null;
		}

		GodotObject? godotObject = value.AsGodotObject();
		if (godotObject is CelestialBody typedBody)
		{
			return typedBody;
		}

		if (godotObject != null && godotObject.HasMethod("to_dict"))
		{
			Variant dataVariant = godotObject.Call("to_dict");
			if (dataVariant.VariantType == Variant.Type.Dictionary)
			{
				return CelestialSerializer.FromDictionary((Godot.Collections.Dictionary)dataVariant);
			}
		}

		return null;
	}

	private static Godot.Collections.Array<CelestialBody> ConvertVariantArrayToBodies(Godot.Collections.Array values)
	{
		Godot.Collections.Array<CelestialBody> bodies = [];
		foreach (Variant value in values)
		{
			CelestialBody? body = ConvertVariantToCelestialBody(value);
			if (body != null)
			{
				bodies.Add(body);
			}
		}

		return bodies;
	}

	private static Godot.Collections.Array BuildVariantArray(Godot.Collections.Array<CelestialBody> bodies)
	{
		Godot.Collections.Array values = new();
		foreach (CelestialBody body in bodies)
		{
			values.Add(body);
		}

		return values;
	}
}
