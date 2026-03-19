using System;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Concepts;
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

	/// <summary>
	/// Emitted when the user requests the concept atlas for the current body or moon.
	/// </summary>
	[Signal]
	public delegate void OpenConceptAtlasRequestedEventHandler();

	private VBoxContainer? _inspectorContainer;

	/// <summary>
	/// Caches the dynamic content container.
	/// </summary>
	public override void _Ready()
	{
		_inspectorContainer = GetNodeOrNull<VBoxContainer>("InspectorContainer");
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
		if (_inspectorContainer == null)
		{
			return;
		}

		foreach (Node child in _inspectorContainer.GetChildren())
		{
			child.QueueFree();
		}
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
		AddSectionHeader(headerOverride ?? "Body");
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
		AddWorldProfileSummary(body);
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
		AddGenerationSnapshot(body);
		AddTravellerReadout(body);
		AddPopulationSummary(body);
		AddConceptSummary(body);
		AddConceptHistorySummary(body);
		AddValidationSummary(body);
		AddEditButton();
	}

	private void AddWorldProfileSummary(CelestialBody body)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

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

		AddSectionHeader("World Profile");
		AddProperty("UWP", profile.ToUwpString());
		AddProperty("Starport", profile.StarportCode);
		AddProperty(
			"Atmosphere",
			TravellerWorldProfile.ToHexDigit(profile.AtmosphereCode) + " " + TravellerWorldGenerator.DescribeAtmosphereCode(profile.AtmosphereCode));
		AddProperty("Hydrographics", $"{TravellerWorldProfile.ToHexDigit(profile.HydrographicsCode)} ({profile.HydrographicsCode * 10}% nominal)");
		AddProperty("Population", TravellerWorldProfile.ToHexDigit(profile.PopulationCode));
		AddProperty("Government", TravellerWorldProfile.ToHexDigit(profile.GovernmentCode));
		AddProperty("Law", TravellerWorldProfile.ToHexDigit(profile.LawCode));
		AddProperty("Tech Level", TravellerWorldProfile.ToHexDigit(profile.TechLevelCode));
		AddProperty("Gravity (g)", $"{body.Physical.GetSurfaceGravityMS2() / 9.80665:0.00} g");
		if (body.HasSurface() && body.Surface != null)
		{
			double celsius = body.Surface.TemperatureK - 273.15;
			AddProperty("Climate", $"{body.Surface.TemperatureK:0.0} K / {celsius:0.0} C");
		}

		if (body.HasPopulationData() && body.PopulationData != null && body.PopulationData.Suitability != null)
		{
			AddProperty("Suitability", body.PopulationData.Suitability.OverallScore.ToString());
		}
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

		VBoxContainer section = new();
		section.AddThemeConstantOverride("separation", 2);

		Label title = new();
		title.Text = $"Moons ({moons.Count})";
		title.AddThemeFontSizeOverride("font_size", 14);
		section.AddChild(title);

		for (int index = 0; index < moons.Count; index++)
		{
			CelestialBody moon = moons[index];
			Button button = new();
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
			section.AddChild(button);
		}

		_inspectorContainer.AddChild(section);
	}

	private void AddBackToPlanetButton(CelestialBody? planet)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		Button button = new();
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

	private void AddSectionHeader(string title)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		Label label = new();
		label.Text = title;
		label.AddThemeFontSizeOverride("font_size", 14);
		_inspectorContainer.AddChild(label);
	}

	private void AddProperty(string labelText, string valueText)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		HBoxContainer row = new();

		Label label = new();
		label.Text = $"{labelText}:";
		label.CustomMinimumSize = new Vector2(120.0f, 0.0f);
		row.AddChild(label);

		Label value = new();
		value.Text = valueText;
		value.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		row.AddChild(value);

		_inspectorContainer.AddChild(row);
	}

	private void AddInfoLabel(string text)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		Label label = new();
		label.Text = text;
		_inspectorContainer.AddChild(label);
	}

	private void AddGenerationSnapshot(CelestialBody body)
	{
		if (_inspectorContainer == null || body.Provenance == null || body.Provenance.SpecSnapshot.Count == 0)
		{
			return;
		}

		AddSectionHeader("Generation Targets");
		if (body.Provenance.SpecSnapshot.ContainsKey("spec_type"))
		{
			AddProperty("Spec", body.Provenance.SpecSnapshot["spec_type"].ToString());
		}

		if (body.Provenance.SpecSnapshot.ContainsKey("size_category"))
		{
			AddProperty("Size Target", FormatSizeCategory(body.Provenance.SpecSnapshot["size_category"]));
		}

		if (body.Provenance.SpecSnapshot.ContainsKey("orbit_zone"))
		{
			AddProperty("Orbit Target", FormatOrbitZone(body.Provenance.SpecSnapshot["orbit_zone"]));
		}

		if (body.Provenance.SpecSnapshot.ContainsKey("spectral_class"))
		{
			AddProperty("Spectral Target", body.Provenance.SpecSnapshot["spectral_class"].ToString());
		}

		if (body.Provenance.SpecSnapshot.ContainsKey("has_atmosphere"))
		{
			AddProperty("Atmosphere Target", FormatOptionalBool(body.Provenance.SpecSnapshot["has_atmosphere"]));
		}

		if (body.Provenance.SpecSnapshot.ContainsKey("has_rings"))
		{
			AddProperty("Rings Target", FormatOptionalBool(body.Provenance.SpecSnapshot["has_rings"]));
		}

		if (body.Provenance.SpecSnapshot.ContainsKey("ring_complexity"))
		{
			AddProperty("Ring Complexity", FormatRingComplexity(body.Provenance.SpecSnapshot["ring_complexity"]));
		}

		if (body.Provenance.SpecSnapshot.ContainsKey("is_captured"))
		{
			AddProperty("Captured Target", (bool)body.Provenance.SpecSnapshot["is_captured"] ? "Yes" : "No");
		}

		if (body.Provenance.SpecSnapshot.ContainsKey("has_subsurface_ocean"))
		{
			AddProperty("Ocean Target", FormatOptionalBool(body.Provenance.SpecSnapshot["has_subsurface_ocean"]));
		}
	}

	private void AddTravellerReadout(CelestialBody body)
	{
		if (_inspectorContainer == null || body.Provenance == null || body.Provenance.SpecSnapshot.Count == 0)
		{
			return;
		}

		if (!body.Provenance.SpecSnapshot.ContainsKey("use_case_settings"))
		{
			return;
		}

		Variant settingsVariant = body.Provenance.SpecSnapshot["use_case_settings"];
		if (settingsVariant.VariantType != Variant.Type.Dictionary)
		{
			return;
		}

		GenerationUseCaseSettings settings = GenerationUseCaseSettings.FromDictionary((Godot.Collections.Dictionary)settingsVariant);
		if (!settings.ShowTravellerReadouts && !settings.IsTravellerMode())
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

		AddSectionHeader("Traveller");
		AddProperty("Ruleset", settings.IsTravellerMode() ? "Traveller" : "Default");
		AddProperty("UWP", profile.ToUwpString());
		AddProperty("Size Code", TravellerWorldProfile.ToHexDigit(profile.SizeCode));
		AddProperty("Atmosphere Code", TravellerWorldProfile.ToHexDigit(profile.AtmosphereCode));
		AddProperty("Hydrographics Code", TravellerWorldProfile.ToHexDigit(profile.HydrographicsCode));
		AddProperty("Population Code", TravellerWorldProfile.ToHexDigit(profile.PopulationCode));
		AddProperty("Government Code", TravellerWorldProfile.ToHexDigit(profile.GovernmentCode));
		AddProperty("Law Code", TravellerWorldProfile.ToHexDigit(profile.LawCode));
		AddProperty("Tech Level", TravellerWorldProfile.ToHexDigit(profile.TechLevelCode));
	}

	private void AddOrbitalSummary(CelestialBody body)
	{
		if (_inspectorContainer == null || !body.HasOrbital() || body.Orbital == null)
		{
			return;
		}

		AddSectionHeader("Orbit");
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

		AddSectionHeader("Surface");
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

		AddSectionHeader("Atmosphere");
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

		AddSectionHeader("Rings");
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

		AddSectionHeader("Population");
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

	private void AddConceptSummary(CelestialBody body)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		bool hasPopulationConcepts = body.PopulationData != null && body.PopulationData.HasConceptResults();
		if (!body.HasConceptResults() && !hasPopulationConcepts)
		{
			return;
		}

		AddSectionHeader("Concept Summary");
		AddConceptProperties(body.ConceptResults);
		if (body.PopulationData != null)
		{
			AddConceptProperties(body.PopulationData.ConceptResults);
		}
	}

	private void AddConceptProperties(ConceptResultStore store)
	{
		foreach (ConceptKind kind in store.GetAll().Keys)
		{
			ConceptRunResult? result = store.Get(kind);
			if (result == null)
			{
				continue;
			}

			string label = kind.ToString();
			string value;
			if (!string.IsNullOrEmpty(result.Subtitle))
			{
				value = result.Subtitle;
			}
			else
			{
				value = result.Title;
			}

			AddProperty(label, value);
		}
	}

	private void AddConceptHistorySummary(CelestialBody body)
	{
		if (_inspectorContainer == null || body.PopulationData == null)
		{
			return;
		}

		Godot.Collections.Array<StarGen.Domain.Population.HistoryEvent> events = new();
		foreach (StarGen.Domain.Population.NativePopulation nativePopulation in body.PopulationData.NativePopulations)
		{
			foreach (StarGen.Domain.Population.HistoryEvent historyEvent in nativePopulation.History.GetAllEvents())
			{
				if (historyEvent.Metadata.ContainsKey("concept_kind"))
				{
					events.Add(historyEvent);
				}
			}
		}

		foreach (StarGen.Domain.Population.Colony colony in body.PopulationData.Colonies)
		{
			foreach (StarGen.Domain.Population.HistoryEvent historyEvent in colony.History.GetAllEvents())
			{
				if (historyEvent.Metadata.ContainsKey("concept_kind"))
				{
					events.Add(historyEvent);
				}
			}
		}

		if (events.Count == 0)
		{
			return;
		}

		AddSectionHeader("History Highlights");
		int startIndex = Mathf.Max(0, events.Count - 4);
		for (int index = startIndex; index < events.Count; index += 1)
		{
			StarGen.Domain.Population.HistoryEvent historyEvent = events[index];
			string label = StarGen.Domain.Population.HistoryEvent.TypeToString(historyEvent.Type);
			AddProperty(label, historyEvent.Title);
		}
	}

	private void AddValidationSummary(CelestialBody body)
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		ValidationResult validation = CelestialValidator.Validate(body);
		AddSectionHeader("Validation");
		if (validation.IsClean())
		{
			AddInfoLabel("No validation issues");
			return;
		}

		foreach (ValidationError warning in validation.Errors)
		{
			Label label = new Label();
			label.AutowrapMode = TextServer.AutowrapMode.Word;
			label.CustomMinimumSize = new Vector2(220.0f, 0.0f);
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

			_inspectorContainer.AddChild(label);
		}
	}

	private void AddEditButton()
	{
		if (_inspectorContainer == null)
		{
			return;
		}

		Button button = new Button();
		button.Text = "Open Parameter Editor";
		button.TooltipText = "Edit and regenerate this body using validated parameters";
		button.Pressed += () => EmitSignal(SignalName.EditRequested);
		_inspectorContainer.AddChild(button);

		Button conceptAtlasButton = new Button();
		conceptAtlasButton.Text = "Open Concept Atlas";
		conceptAtlasButton.TooltipText = "Explore concept modules seeded from the current body or moon";
		conceptAtlasButton.Pressed += () => EmitSignal(SignalName.OpenConceptAtlasRequested);
		_inspectorContainer.AddChild(conceptAtlasButton);
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
