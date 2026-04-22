using Godot;
using StarGen.App.Components;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Celestial.Validation;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Traveller;
using StarGen.Domain.Population;

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

	private const string MoonSectionPath = "InspectorContainer/MoonSection";
	private const string BodySectionPath = "InspectorContainer/BodySection";
	private const string ParentSectionPath = "InspectorContainer/ParentSection";
	private const string OrbitSectionPath = "InspectorContainer/OrbitSection";
	private const string SurfaceSectionPath = "InspectorContainer/SurfaceSection";
	private const string AtmosphereSectionPath = "InspectorContainer/AtmosphereSection";
	private const string RingsSectionPath = "InspectorContainer/RingsSection";
	private const string TravellerSectionPath = "InspectorContainer/TravellerSection";
	private const string PopulationSectionPath = "InspectorContainer/PopulationSection";
	private const string ValidationSectionPath = "InspectorContainer/ValidationSection";

	private VBoxContainer? _inspectorContainer;
	private Label? _infoLabel;
	private Button? _backToBodyButton;
	private Button? _editButton;
	private bool _staticSignalsConnected;

	/// <summary>
	/// Caches the scene-owned inspector nodes.
	/// </summary>
	public override void _Ready()
	{
		EnsureSceneReferences();
		ConnectStaticSignals();
		Clear();
	}

	private void EnsureSceneReferences()
	{
		if (_inspectorContainer == null)
		{
			_inspectorContainer = GetNodeOrNull<VBoxContainer>("InspectorContainer");
		}

		if (_infoLabel == null)
		{
			_infoLabel = GetNodeOrNull<Label>("InspectorContainer/InfoLabel");
		}

		if (_backToBodyButton == null)
		{
			_backToBodyButton = GetNodeOrNull<Button>("InspectorContainer/BackToBodyButton");
		}

		if (_editButton == null)
		{
			_editButton = GetNodeOrNull<Button>("InspectorContainer/EditButton");
		}
	}

	private void ConnectStaticSignals()
	{
		if (_staticSignalsConnected)
		{
			return;
		}

		if (_backToBodyButton != null)
		{
			_backToBodyButton.Pressed += OnBackToBodyPressed;
		}

		if (_editButton != null)
		{
			_editButton.Pressed += OnEditButtonPressed;
		}

		_staticSignalsConnected = true;
	}

	/// <summary>
	/// GDScript-compatible clear wrapper.
	/// </summary>
	public void clear()
	{
		Clear();
	}

	/// <summary>
	/// Clears scene-owned section content and returns the inspector to its empty state.
	/// </summary>
	public void Clear()
	{
		EnsureSceneReferences();
		if (_inspectorContainer == null)
		{
			return;
		}

		ClearSection(MoonSectionPath);
		ClearSection(BodySectionPath);
		ClearSection(ParentSectionPath);
		ClearSection(OrbitSectionPath);
		ClearSection(SurfaceSectionPath);
		ClearSection(AtmosphereSectionPath);
		ClearSection(RingsSectionPath);
		ClearSection(TravellerSectionPath);
		ClearSection(PopulationSectionPath);
		ClearSection(ValidationSectionPath);
		HideInfoLabel();
		HideBackButton();
		HideEditButton();
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
		EnsureSceneReferences();
		if (_inspectorContainer == null)
		{
			return;
		}

		if (body == null)
		{
			ShowInfoLabel("No object loaded");
			return;
		}

		if (moons.Count > 0)
		{
			Godot.Collections.Array moonValues = originalMoonVariants ?? BuildVariantArray(moons);
			PopulateMoonSection(moons, moonValues, null);
		}

		PopulatePrimaryBodySections(body, "Body");
		ShowEditButton();
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
	/// Displays a focused moon view with a scene-owned back button.
	/// </summary>
	public void DisplayFocusedMoon(
		CelestialBody? moon,
		CelestialBody? planet,
		Godot.Collections.Array<CelestialBody> allMoons,
		Godot.Collections.Array? originalMoonVariants = null)
	{
		Clear();
		EnsureSceneReferences();
		if (_inspectorContainer == null || moon == null)
		{
			return;
		}

		ShowBackButton(planet);
		PopulatePrimaryBodySections(moon, $"Moon: {moon.Name}");
		if (allMoons.Count > 1)
		{
			Godot.Collections.Array moonValues = originalMoonVariants ?? BuildVariantArray(allMoons);
			PopulateMoonSection(allMoons, moonValues, moon);
		}

		if (planet != null)
		{
			PopulateParentSection(planet);
		}

		ShowEditButton();
	}

	private void PopulatePrimaryBodySections(CelestialBody body, string header)
	{
		PopulateBodySection(body, header, BodySectionPath);
		PopulateOrbitSection(body);
		PopulateSurfaceSection(body);
		PopulateAtmosphereSection(body);
		PopulateRingSection(body);
		PopulateTravellerSection(body);
		PopulatePopulationSection(body);
		PopulateValidationSection(body);
	}

	private void PopulateMoonSection(
		Godot.Collections.Array<CelestialBody> moons,
		Godot.Collections.Array originalMoonVariants,
		CelestialBody? focusedMoon)
	{
		SetSectionTitle(MoonSectionPath, $"Moons ({moons.Count})");
		VBoxContainer? content = GetSectionContent(MoonSectionPath);
		if (content == null)
		{
			return;
		}

		for (int index = 0; index < moons.Count; index += 1)
		{
			CelestialBody moon = moons[index];
			Button button = UiSceneTemplates.InstantiateActionButton();
			string buttonText = moon.Name;
			if (focusedMoon != null && moon.Id == focusedMoon.Id)
			{
				buttonText = $"* {moon.Name}";
			}

			button.Text = buttonText;
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
			content.AddChild(button);
		}

		SetSectionVisible(MoonSectionPath, true);
	}

	private void PopulateBodySection(CelestialBody body, string title, string sectionPath)
	{
		SetSectionTitle(sectionPath, title);
		string nameValue = body.Id;
		if (!string.IsNullOrEmpty(body.Name))
		{
			nameValue = body.Name;
		}

		AddPropertyToSection(sectionPath, "Name", nameValue);
		AddPropertyToSection(sectionPath, "Type", body.GetTypeString());
		AddPropertyToSection(sectionPath, "ID", body.Id);
		AddPhysicalSummaryToSection(sectionPath, body.Physical);

		if (body.HasStellar() && body.Stellar != null)
		{
			AddPropertyToSection(sectionPath, "Spectral Class", body.Stellar.SpectralClass);
			AddPropertyToSection(sectionPath, "Temperature", $"{body.Stellar.EffectiveTemperatureK:0} K");
		}
	}

	private void PopulateParentSection(CelestialBody parentBody)
	{
		string headerText = "Parent";
		if (!string.IsNullOrWhiteSpace(parentBody.Name))
		{
			headerText = $"Parent: {parentBody.Name}";
		}

		PopulateBodySection(parentBody, headerText, ParentSectionPath);
	}

	private void PopulateOrbitSection(CelestialBody body)
	{
		if (!body.HasOrbital() || body.Orbital == null)
		{
			return;
		}

		SetSectionTitle(OrbitSectionPath, "Orbit");
		AddPropertyToSection(OrbitSectionPath, "Semi-major Axis", FormatDistance(body.Orbital.SemiMajorAxisM));
		AddPropertyToSection(OrbitSectionPath, "Eccentricity", $"{body.Orbital.Eccentricity:0.0000}");
		AddPropertyToSection(OrbitSectionPath, "Periapsis", FormatDistance(body.Orbital.GetPeriapsisM()));
		AddPropertyToSection(OrbitSectionPath, "Apoapsis", FormatDistance(body.Orbital.GetApoapsisM()));
		AddPropertyToSection(OrbitSectionPath, "Inclination", $"{body.Orbital.InclinationDeg:0.00} deg");
		if (!string.IsNullOrWhiteSpace(body.Orbital.ParentId))
		{
			AddPropertyToSection(OrbitSectionPath, "Parent", body.Orbital.ParentId);
		}
	}

	private void PopulateSurfaceSection(CelestialBody body)
	{
		if (!body.HasSurface() || body.Surface == null)
		{
			return;
		}

		SetSectionTitle(SurfaceSectionPath, "Surface");
		AddPropertyToSection(SurfaceSectionPath, "Temperature", $"{body.Surface.TemperatureK:0.0} K");
		AddPropertyToSection(SurfaceSectionPath, "Albedo", $"{body.Surface.Albedo:0.00}");
		if (!string.IsNullOrWhiteSpace(body.Surface.SurfaceType))
		{
			AddPropertyToSection(SurfaceSectionPath, "Surface Type", body.Surface.SurfaceType);
		}

		AddPropertyToSection(SurfaceSectionPath, "Volcanism", $"{body.Surface.VolcanismLevel:0.00}");
		if (body.Surface.HasTerrain() && body.Surface.Terrain != null)
		{
			AddPropertyToSection(SurfaceSectionPath, "Terrain", body.Surface.Terrain.TerrainType);
			AddPropertyToSection(SurfaceSectionPath, "Elevation Range", FormatDistance(body.Surface.Terrain.ElevationRangeM));
			AddPropertyToSection(SurfaceSectionPath, "Tectonics", $"{body.Surface.Terrain.TectonicActivity:0.00}");
		}

		if (body.Surface.HasHydrosphere() && body.Surface.Hydrosphere != null)
		{
			AddPropertyToSection(SurfaceSectionPath, "Ocean Coverage", $"{body.Surface.Hydrosphere.OceanCoverage * 100.0:0.#}%");
			AddPropertyToSection(SurfaceSectionPath, "Ice Coverage", $"{body.Surface.Hydrosphere.IceCoverage * 100.0:0.#}%");
			AddPropertyToSection(SurfaceSectionPath, "Water Type", body.Surface.Hydrosphere.WaterType);
		}

		if (body.Surface.HasCryosphere() && body.Surface.Cryosphere != null)
		{
			AddPropertyToSection(SurfaceSectionPath, "Polar Caps", $"{body.Surface.Cryosphere.PolarCapCoverage * 100.0:0.#}%");
			if (body.Surface.Cryosphere.HasSubsurfaceOcean)
			{
				AddPropertyToSection(SurfaceSectionPath, "Subsurface Ocean", "Yes");
			}
			else
			{
				AddPropertyToSection(SurfaceSectionPath, "Subsurface Ocean", "No");
			}

			AddPropertyToSection(SurfaceSectionPath, "Cryovolcanism", $"{body.Surface.Cryosphere.CryovolcanismLevel:0.00}");
		}
	}

	private void PopulateAtmosphereSection(CelestialBody body)
	{
		if (!body.HasAtmosphere() || body.Atmosphere == null)
		{
			return;
		}

		SetSectionTitle(AtmosphereSectionPath, "Atmosphere");
		AddPropertyToSection(AtmosphereSectionPath, "Surface Pressure", $"{body.Atmosphere.SurfacePressurePa / 101325.0:0.###} atm");
		AddPropertyToSection(AtmosphereSectionPath, "Scale Height", FormatDistance(body.Atmosphere.ScaleHeightM));
		AddPropertyToSection(AtmosphereSectionPath, "Greenhouse", $"{body.Atmosphere.GreenhouseFactor:0.00}x");
		if (body.Atmosphere.Composition.Count > 0)
		{
			AddPropertyToSection(AtmosphereSectionPath, "Dominant Gas", body.Atmosphere.GetDominantGas());
		}
	}

	private void PopulateRingSection(CelestialBody body)
	{
		if (!body.HasRingSystem() || body.RingSystem == null)
		{
			return;
		}

		SetSectionTitle(RingsSectionPath, "Rings");
		AddPropertyToSection(RingsSectionPath, "Bands", body.RingSystem.GetBandCount().ToString());
		AddPropertyToSection(RingsSectionPath, "Inner Radius", FormatDistance(body.RingSystem.GetInnerRadiusM()));
		AddPropertyToSection(RingsSectionPath, "Outer Radius", FormatDistance(body.RingSystem.GetOuterRadiusM()));
		AddPropertyToSection(RingsSectionPath, "Plane Tilt", $"{body.RingSystem.InclinationDeg:0.0} deg");
	}

	private void PopulateTravellerSection(CelestialBody body)
	{
		if (body.Provenance == null || body.Provenance.SpecSnapshot.Count == 0)
		{
			return;
		}

		GenerationUseCaseSettings? settings = ResolveUseCaseSettings(body);
		bool shouldShowTravellerReadout = false;
		if (settings != null)
		{
			if (settings.ShowTravellerReadouts || settings.IsTravellerMode())
			{
				shouldShowTravellerReadout = true;
			}
		}
		else
		{
			shouldShowTravellerReadout = ResolveLegacyTravellerInspectorVisibility(body);
		}

		if (!shouldShowTravellerReadout)
		{
			return;
		}

		TravellerWorldProfile profile;
		TravellerWorldProfile? storedProfile = TravellerWorldGenerator.TryGetStoredProfile(body);
		if (storedProfile != null)
		{
			profile = storedProfile;
		}
		else
		{
			profile = TravellerWorldGenerator.DeriveFromBody(body);
		}

		SetSectionTitle(TravellerSectionPath, "Traveller");
		if (settings != null)
		{
			AddPropertyToSection(TravellerSectionPath, "Ruleset", GenerationUseCasePresentation.GetRulesetLabel(settings.RulesetMode));
		}

		AddPropertyToSection(TravellerSectionPath, "UWP", profile.ToUwpString());
		AddPropertyToSection(TravellerSectionPath, "Size Code", TravellerWorldProfile.ToHexDigit(profile.SizeCode));
		AddPropertyToSection(TravellerSectionPath, "Atmosphere Code", TravellerWorldProfile.ToHexDigit(profile.AtmosphereCode));
		AddPropertyToSection(TravellerSectionPath, "Hydrographics Code", TravellerWorldProfile.ToHexDigit(profile.HydrographicsCode));
		AddPropertyToSection(TravellerSectionPath, "Population Code", TravellerWorldProfile.ToHexDigit(profile.PopulationCode));
		AddPropertyToSection(TravellerSectionPath, "Government Code", TravellerWorldProfile.ToHexDigit(profile.GovernmentCode));
		AddPropertyToSection(TravellerSectionPath, "Law Code", TravellerWorldProfile.ToHexDigit(profile.LawCode));
		AddPropertyToSection(TravellerSectionPath, "Tech Level", TravellerWorldProfile.ToHexDigit(profile.TechLevelCode));

		TravellerTradeCodeSet? tradeCodes = TryGetStoredTradeCodes(body);
		if (tradeCodes != null)
		{
			AddPropertyToSection(TravellerSectionPath, "Trade Codes", tradeCodes.ToDisplayString());
		}

		string travelZone = TryGetStoredTravelZone(body);
		if (!string.IsNullOrEmpty(travelZone))
		{
			AddPropertyToSection(TravellerSectionPath, "Travel Zone", travelZone);
		}
	}

	private void PopulatePopulationSection(CelestialBody body)
	{
		if (!body.HasPopulationData() || body.PopulationData == null)
		{
			return;
		}

		if (!body.PopulationData.IsInhabited())
		{
			return;
		}

		SetSectionTitle(PopulationSectionPath, "Population");
		AddPropertyToSection(PopulationSectionPath, "Total Population", PropertyFormatter.FormatPopulation(body.PopulationData.GetTotalPopulation()));
		AddPropertyToSection(PopulationSectionPath, "Situation", PropertyFormatter.FormatPoliticalSituation(body.PopulationData.GetPoliticalSituation()));
		AddPropertyToSection(PopulationSectionPath, "Colonies", body.PopulationData.GetActiveColonyCount().ToString());
		AddPropertyToSection(PopulationSectionPath, "Native Groups", body.PopulationData.GetExtantNativeCount().ToString());
		AddPropertyToSection(PopulationSectionPath, "Dominant", body.PopulationData.GetDominantPopulationName());
		AddPropertyToSection(PopulationSectionPath, "Highest Tech", PropertyFormatter.FormatTechLevel(body.PopulationData.GetHighestTechLevel()));
		if (body.PopulationData.Profile != null)
		{
			AddPropertyToSection(PopulationSectionPath, "Habitability", PropertyFormatter.FormatHabitability(body.PopulationData.Profile.HabitabilityScore));
		}

		if (body.PopulationData.Suitability != null)
		{
			AddPropertyToSection(PopulationSectionPath, "Suitability", PropertyFormatter.FormatSuitability(body.PopulationData.Suitability.OverallScore));
		}

		SentientWorldProfile? sentientWorldProfile = body.PopulationData.GetSentientWorldProfile();
		if (sentientWorldProfile != null)
		{
			AddPropertyToSection(PopulationSectionPath, "Dominant Regime", PropertyFormatter.FormatRegime(sentientWorldProfile.DominantRegime));
			AddPropertyToSection(PopulationSectionPath, "Settlement Pattern", sentientWorldProfile.SettlementPattern);
			AddPropertyToSection(PopulationSectionPath, "Primary Settlement", sentientWorldProfile.PrimarySettlementRank);
			AddPropertyToSection(PopulationSectionPath, "Logistics Capacity", sentientWorldProfile.LogisticsCapacity);
			AddPropertyToSection(PopulationSectionPath, "Urbanization", PropertyFormatter.FormatPercent(sentientWorldProfile.UrbanizationShare));
			AddPropertyToSection(PopulationSectionPath, "Social Scale", PropertyFormatter.FormatPercent(sentientWorldProfile.SocialScale));
			AddPropertyToSection(PopulationSectionPath, "Surplus Base", PropertyFormatter.FormatPercent(sentientWorldProfile.SurplusBase));
			AddPropertyToSection(PopulationSectionPath, "Trade Connectivity", PropertyFormatter.FormatPercent(sentientWorldProfile.TradeConnectivity));
			AddPropertyToSection(PopulationSectionPath, "External Threat", PropertyFormatter.FormatPercent(sentientWorldProfile.ExternalThreat));
			AddPropertyToSection(PopulationSectionPath, "State Capacity", PropertyFormatter.FormatPercent(sentientWorldProfile.StateCapacity));
			AddPropertyToSection(PopulationSectionPath, "Fiscal Contract", PropertyFormatter.FormatPercent(sentientWorldProfile.FiscalContract));
			AddPropertyToSection(PopulationSectionPath, "Legal Centralization", PropertyFormatter.FormatPercent(sentientWorldProfile.LegalCentralization));
			AddPropertyToSection(PopulationSectionPath, "Legal Reach", PropertyFormatter.FormatPercent(sentientWorldProfile.LegalReach));
			AddPropertyToSection(PopulationSectionPath, "Restriction Pressure", PropertyFormatter.FormatPercent(sentientWorldProfile.RestrictionPressure));
			AddPropertyToSection(PopulationSectionPath, "Cultural Accumulation", PropertyFormatter.FormatPercent(sentientWorldProfile.CulturalAccumulation));
			AddPropertyToSection(PopulationSectionPath, "Tech Adoption", PropertyFormatter.FormatPercent(sentientWorldProfile.TechnologyAdoptionCapacity));
			AddPropertyToSection(PopulationSectionPath, "Factional Fragmentation", PropertyFormatter.FormatPercent(sentientWorldProfile.FactionalFragmentation));
			AddPropertyToSection(PopulationSectionPath, "Religious Centralization", PropertyFormatter.FormatPercent(sentientWorldProfile.ReligiousCentralization));
		}
	}

	private void PopulateValidationSection(CelestialBody body)
	{
		ValidationResult validation = CelestialValidator.Validate(body);
		SetSectionTitle(ValidationSectionPath, "Validation");
		if (validation.IsClean())
		{
			AddInfoToSection(ValidationSectionPath, "No validation issues");
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

			AddCustomControlToSection(ValidationSectionPath, label);
		}
	}

	private void AddPhysicalSummaryToSection(string sectionPath, PhysicalProps physical)
	{
		AddPropertyToSection(sectionPath, "Mass", $"{physical.MassKg:0.###e0} kg");
		AddPropertyToSection(sectionPath, "Radius", FormatDistance(physical.RadiusM));
		AddPropertyToSection(sectionPath, "Density", $"{physical.GetDensityKgM3():0.0} kg/m^3");
		AddPropertyToSection(sectionPath, "Gravity", $"{physical.GetSurfaceGravityMS2():0.00} m/s^2");
		AddPropertyToSection(sectionPath, "Escape Velocity", $"{physical.GetEscapeVelocityMS() / 1000.0:0.00} km/s");
	}

	private void AddPropertyToSection(string sectionPath, string labelText, string valueText)
	{
		HBoxContainer row = UiSceneTemplates.InstantiatePropertyRow();
		Label label = UiSceneTemplates.GetRequiredChild<Label>(row, "Key");
		Label value = UiSceneTemplates.GetRequiredChild<Label>(row, "Value");
		label.Text = $"{labelText}:";
		value.Text = valueText;
		AddCustomControlToSection(sectionPath, row);
	}

	private void AddInfoToSection(string sectionPath, string text)
	{
		Label label = UiSceneTemplates.InstantiateMessageLabel();
		label.Text = text;
		AddCustomControlToSection(sectionPath, label);
	}

	private void AddCustomControlToSection(string sectionPath, Control control)
	{
		VBoxContainer? content = GetSectionContent(sectionPath);
		if (content == null)
		{
			control.QueueFree();
			return;
		}

		content.AddChild(control);
		SetSectionVisible(sectionPath, true);
	}

	private void SetSectionTitle(string sectionPath, string title)
	{
		Label? titleLabel = GetNodeOrNull<Label>($"{sectionPath}/TitleLabel");
		if (titleLabel != null)
		{
			titleLabel.Text = title;
		}
	}

	private VBoxContainer? GetSectionContent(string sectionPath)
	{
		return GetNodeOrNull<VBoxContainer>($"{sectionPath}/Content");
	}

	private void SetSectionVisible(string sectionPath, bool visible)
	{
		Control? section = GetNodeOrNull<Control>(sectionPath);
		if (section != null)
		{
			section.Visible = visible;
		}
	}

	private void ClearSection(string sectionPath)
	{
		VBoxContainer? content = GetSectionContent(sectionPath);
		if (content != null)
		{
			ClearDynamicChildren(content);
		}

		SetSectionVisible(sectionPath, false);
	}

	private void ShowInfoLabel(string text)
	{
		if (_infoLabel == null)
		{
			return;
		}

		_infoLabel.Text = text;
		_infoLabel.Visible = true;
	}

	private void HideInfoLabel()
	{
		if (_infoLabel != null)
		{
			_infoLabel.Visible = false;
		}
	}

	private void ShowBackButton(CelestialBody? planet)
	{
		if (_backToBodyButton == null)
		{
			return;
		}

		if (planet == null || string.IsNullOrWhiteSpace(planet.Name))
		{
			_backToBodyButton.Text = "Back to Planet";
		}
		else
		{
			_backToBodyButton.Text = $"Back to {planet.Name}";
		}

		_backToBodyButton.Visible = true;
	}

	private void HideBackButton()
	{
		if (_backToBodyButton != null)
		{
			_backToBodyButton.Visible = false;
		}
	}

	private void ShowEditButton()
	{
		if (_editButton != null)
		{
			_editButton.Visible = true;
		}
	}

	private void HideEditButton()
	{
		if (_editButton != null)
		{
			_editButton.Visible = false;
		}
	}

	private void OnBackToBodyPressed()
	{
		EmitSignal(SignalName.MoonSelected, new Variant());
	}

	private void OnEditButtonPressed()
	{
		EmitSignal(SignalName.EditRequested);
	}

	private static void ClearDynamicChildren(Node parent)
	{
		Godot.Collections.Array<Node> children = [];
		foreach (Node child in parent.GetChildren())
		{
			children.Add(child);
		}

		foreach (Node child in children)
		{
			parent.RemoveChild(child);
			child.QueueFree();
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
