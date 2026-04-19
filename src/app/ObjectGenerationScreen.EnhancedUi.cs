using System;
using System.Collections.Generic;
using Godot;
using StarGen.App.Components;
using StarGen.App.Shared;
using StarGen.App.Viewer;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Parameters;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Traveller;

namespace StarGen.App;

public partial class ObjectGenerationScreen
{
    private readonly Dictionary<string, HBoxContainer> _rows = new();
    private readonly Dictionary<string, Label> _rowLabels = new();
    private readonly Dictionary<string, CheckBox> _optionalToggles = new();
    private readonly Dictionary<string, SpinBox> _optionalInputs = new();

    private CheckBox? _showAdvancedControlsCheck;
    private LineEdit? _nameInput;

    private VBoxContainer? _planetSection;
    private VBoxContainer? _lifeSection;
    private OptionButton? _planetOrbitModeOption;
    private OptionButton? _planetClassBiasOption;
    private OptionButton? _planetCompositionBiasOption;
    private OptionButton? _planetEnvelopeOverrideOption;
    private OptionButton? _planetVolatileRichnessOption;
    private OptionButton? _planetHydrosphereTendencyOption;
    private OptionButton? _planetSizeCategoryOption;
    private OptionButton? _planetOrbitZoneOption;
    private OptionButton? _planetAtmosphereOption;
    private OptionButton? _planetRingsOption;
    private OptionButton? _planetRingComplexityOption;
    private OptionButton? _planetSurfacePressureOption;
    private OptionButton? _planetOceanCoverageOption;
    private OptionButton? _planetIceCoverageOption;
    private OptionButton? _planetAlbedoProfileOption;
    private OptionButton? _planetVolcanismOption;
    private CheckBox? _planetGenerateMoonCheck;
    private OptionButton? _moonTargetCountOption;
    private OptionButton? _lifeFrameworkOption;
    private OptionButton? _abiogenesisModelOption;
    private OptionButton? _complexLifeModelOption;
    private OptionButton? _civilizationModelOption;
    private OptionButton? _environmentalWindowWeightOption;

    private VBoxContainer? _travellerSection;
    private CheckBox? _useTravellerWorldProfileCheck;
    private OptionButton? _travellerSizeCodeOption;
    private OptionButton? _travellerAtmosphereCodeOption;
    private OptionButton? _travellerHydrographicsCodeOption;
    private OptionButton? _travellerPopulationCodeOption;

    private CheckBox? _moonCapturedCheck;

    private VBoxContainer? _starSection;
    private OptionButton? _starSpectralClassOption;
    private OptionButton? _starSubclassOption;

    private VBoxContainer? _asteroidSection;
    private OptionButton? _asteroidTypeOption;
    private CheckBox? _asteroidLargeCheck;
    private OptionButton? _asteroidOrbitBandOption;
    private OptionButton? _asteroidDensityProfileOption;
    private OptionButton? _asteroidAlbedoProfileOption;

    private VBoxContainer? _cometSection;
    private OptionButton? _cometFamilyOption;
    private OptionButton? _cometActivityOption;
    private CheckBox? _cometLargeCheck;

    private VBoxContainer? _advancedSection;

    private void BuildEnhancedParameterUi()
    {
        if (_parameterVBox == null)
        {
            return;
        }

        CacheEnhancedSceneReferences();
        PopulateEnhancedStaticOptions();
        RebuildEnhancedPresetOptions();
    }

    private void CacheEnhancedSceneReferences()
    {
        if (_typeOption != null)
        {
            return;
        }

        _rows.Clear();
        _rowLabels.Clear();
        _optionalToggles.Clear();
        _optionalInputs.Clear();

        _typeOption = GetRequiredOptionButton("TypeRow", "TypeOption");
        _presetOption = GetRequiredOptionButton("PresetRow", "PresetOption");
        _nameInput = GetRequiredLineEdit("NameRow", "NameInput");
        _seedInput = GetRequiredSpinBox("SeedRow", "SeedInput");
        _seedRow = GetRequiredRow("SeedRow");
        _rulesetModeOption = GetRequiredOptionButton("RulesetRow", "RulesetModeOption");
        _showTravellerReadoutsCheck = GetRequiredCheckBox("ShowTravellerReadoutsRow", "ShowTravellerReadoutsCheck");
        _showAdvancedControlsCheck = GetRequiredCheckBox("ShowAdvancedControlsRow", "ShowAdvancedControlsCheck");

        _planetSection = GetOptionalSection("PlanetSection");
        _lifeSection = GetOptionalSection("LifeSection");
        _travellerSection = GetOptionalSection("TravellerSection");
        _starSection = GetOptionalSection("StarSection");
        _asteroidSection = GetOptionalSection("AsteroidSection");
        _cometSection = GetOptionalSection("CometSection");
        _advancedSection = GetOptionalSection("AdvancedSection");

        _planetOrbitModeOption = GetRequiredOptionButton("PlanetOrbitModeRow", "PlanetOrbitModeOption");
        _planetClassBiasOption = GetRequiredOptionButton("PlanetClassBiasRow", "PlanetClassBiasOption");
        _planetCompositionBiasOption = GetRequiredOptionButton("PlanetCompositionBiasRow", "PlanetCompositionBiasOption");
        _planetEnvelopeOverrideOption = GetRequiredOptionButton("PlanetEnvelopeOverrideRow", "PlanetEnvelopeOverrideOption");
        _planetVolatileRichnessOption = GetRequiredOptionButton("PlanetVolatileRichnessRow", "PlanetVolatileRichnessOption");
        _planetHydrosphereTendencyOption = GetRequiredOptionButton("PlanetHydrosphereTendencyRow", "PlanetHydrosphereTendencyOption");
        _planetSizeCategoryOption = GetRequiredOptionButton("PlanetSizeCategoryRow", "PlanetSizeCategoryOption");
        _planetOrbitZoneOption = GetRequiredOptionButton("PlanetOrbitZoneRow", "PlanetOrbitZoneOption");
        _planetAtmosphereOption = GetRequiredOptionButton("PlanetAtmosphereRow", "PlanetAtmosphereOption");
        _planetRingsOption = GetRequiredOptionButton("PlanetRingsRow", "PlanetRingsOption");
        _planetRingComplexityOption = GetRequiredOptionButton("PlanetRingComplexityRow", "PlanetRingComplexityOption");
        _planetSurfacePressureOption = GetRequiredOptionButton("PlanetSurfacePressureRow", "PlanetSurfacePressureOption");
        _planetOceanCoverageOption = GetRequiredOptionButton("PlanetOceanCoverageRow", "PlanetOceanCoverageOption");
        _planetIceCoverageOption = GetRequiredOptionButton("PlanetIceCoverageRow", "PlanetIceCoverageOption");
        _planetAlbedoProfileOption = GetRequiredOptionButton("PlanetAlbedoProfileRow", "PlanetAlbedoProfileOption");
        _planetVolcanismOption = GetRequiredOptionButton("PlanetVolcanismRow", "PlanetVolcanismOption");
        _planetGenerateMoonCheck = GetRequiredCheckBox("PlanetGenerateMoonRow", "PlanetGenerateMoonCheck");
        _moonTargetCountOption = GetRequiredOptionButton("MoonTargetCountRow", "MoonTargetCountOption");

        _useTravellerWorldProfileCheck = GetRequiredCheckBox("UseTravellerWorldProfileRow", "UseTravellerWorldProfileCheck");
        _travellerSizeCodeOption = GetRequiredOptionButton("TravellerSizeCodeRow", "TravellerSizeCodeOption");
        _travellerAtmosphereCodeOption = GetRequiredOptionButton("TravellerAtmosphereCodeRow", "TravellerAtmosphereCodeOption");
        _travellerHydrographicsCodeOption = GetRequiredOptionButton("TravellerHydrographicsCodeRow", "TravellerHydrographicsCodeOption");
        _travellerPopulationCodeOption = GetRequiredOptionButton("TravellerPopulationCodeRow", "TravellerPopulationCodeOption");

        _moonCapturedCheck = GetRequiredCheckBox("MoonCapturedRow", "MoonCapturedCheck");

        _starSpectralClassOption = GetRequiredOptionButton("StarSpectralClassRow", "StarSpectralClassOption");
        _starSubclassOption = GetRequiredOptionButton("StarSubclassRow", "StarSubclassOption");

        _asteroidTypeOption = GetRequiredOptionButton("AsteroidTypeRow", "AsteroidTypeOption");
        _asteroidLargeCheck = GetRequiredCheckBox("AsteroidLargeRow", "AsteroidLargeCheck");
        _asteroidOrbitBandOption = GetRequiredOptionButton("AsteroidOrbitBandRow", "AsteroidOrbitBandOption");
        _asteroidDensityProfileOption = GetRequiredOptionButton("AsteroidDensityProfileRow", "AsteroidDensityProfileOption");
        _asteroidAlbedoProfileOption = GetRequiredOptionButton("AsteroidAlbedoProfileRow", "AsteroidAlbedoProfileOption");

        _cometFamilyOption = GetRequiredOptionButton("CometFamilyRow", "CometFamilyOption");
        _cometActivityOption = GetRequiredOptionButton("CometActivityRow", "CometActivityOption");
        _cometLargeCheck = GetRequiredCheckBox("CometLargeRow", "CometLargeCheck");

        CacheOptionalOverride("MassOverride");
        CacheOptionalOverride("RadiusOverride");
        CacheOptionalOverride("RotationOverride");
        CacheOptionalOverride("AxialTiltOverride");
        CacheOptionalOverride("SemiMajorAxisOverride");
        CacheOptionalOverride("EccentricityOverride");
        CacheOptionalOverride("InclinationOverride");
        CacheOptionalOverride("SurfacePressureOverride");
        CacheOptionalOverride("AlbedoOverride");
        CacheOptionalOverride("VolcanismOverride");
        CacheOptionalOverride("TemperatureOverride");
        CacheOptionalOverride("LuminosityOverride");
    }

    private void PopulateEnhancedStaticOptions()
    {
        if (_typeOption == null || _rulesetModeOption == null)
        {
            throw new InvalidOperationException("ObjectGenerationScreen scene is missing required parameter controls.");
        }

        _typeOption.Clear();
        _typeOption.AddItem("Star", (int)ObjectViewer.ObjectType.Star);
        _typeOption.AddItem("Planet", (int)ObjectViewer.ObjectType.Planet);
        _typeOption.AddItem("Asteroid", (int)ObjectViewer.ObjectType.Asteroid);
        _typeOption.AddItem("Comet", (int)ObjectViewer.ObjectType.Comet);
        _rulesetModeOption.Clear();
        _rulesetModeOption.AddItem(GenerationUseCasePresentation.RealisticRulesetLabel, (int)GenerationUseCaseSettings.RulesetModeType.Default);
		_rulesetModeOption.AddItem("Space Opera", (int)GenerationUseCaseSettings.RulesetModeType.Traveller);
        PopulatePlanetSection();
        PopulateObjectLifeSection();
        PopulateTravellerSection();
        PopulateStarSection();
        PopulateAsteroidSection();
        PopulateCometSection();
        PopulateAdvancedSection();
    }

    private void ConnectEnhancedSignals()
    {
        if (_startButton != null)
        {
            _startButton.Pressed += OnStartPressed;
        }

        if (_backButton != null)
        {
            _backButton.Pressed += () => EmitSignal(SignalName.back_requested);
        }

        if (_typeOption != null)
        {
            _typeOption.ItemSelected += _ => OnTypeChanged();
        }

        if (_presetOption != null)
        {
            _presetOption.ItemSelected += _ => OnPresetChanged();
        }

        if (_nameInput != null)
        {
            _nameInput.TextChanged += _ => RefreshSummary();
        }

        if (_seedInput != null)
        {
            _seedInput.ValueChanged += _ => RefreshSummary();
        }

        if (_rulesetModeOption != null)
        {
            _rulesetModeOption.ItemSelected += OnRulesetModeSelected;
        }

        if (_showTravellerReadoutsCheck != null)
        {
            _showTravellerReadoutsCheck.Toggled += _ => RefreshSummary();
        }

        if (_showAdvancedControlsCheck != null)
        {
            _showAdvancedControlsCheck.Toggled += _ => OnEnhancedAdvancedToggled();
        }

        if (_useTravellerWorldProfileCheck != null)
        {
            _useTravellerWorldProfileCheck.Toggled += _ => OnEnhancedTravellerProfileToggled();
        }

        if (_planetOrbitModeOption != null)
        {
            _planetOrbitModeOption.ItemSelected += _ =>
            {
                RefreshEnhancedParameterVisibility();
                RefreshSummary();
            };
        }
        ConnectOptionToSummary(_planetClassBiasOption);
        ConnectOptionToSummary(_planetCompositionBiasOption);
        ConnectOptionToSummary(_planetEnvelopeOverrideOption);
        ConnectOptionToSummary(_planetVolatileRichnessOption);
        ConnectOptionToSummary(_planetHydrosphereTendencyOption);
        ConnectOptionToSummary(_planetSizeCategoryOption);
        ConnectOptionToSummary(_planetOrbitZoneOption);
        ConnectOptionToSummary(_planetAtmosphereOption);
        ConnectOptionToSummary(_planetRingsOption);
        ConnectOptionToSummary(_planetRingComplexityOption);
        ConnectOptionToSummary(_planetSurfacePressureOption);
        ConnectOptionToSummary(_planetOceanCoverageOption);
        ConnectOptionToSummary(_planetIceCoverageOption);
        ConnectOptionToSummary(_planetAlbedoProfileOption);
        ConnectOptionToSummary(_planetVolcanismOption);
        ConnectOptionToSummary(_moonTargetCountOption);
        ConnectObjectLifeSignals();
        ConnectOptionToSummary(_travellerSizeCodeOption);
        ConnectOptionToSummary(_travellerAtmosphereCodeOption);
        ConnectOptionToSummary(_travellerHydrographicsCodeOption);
        ConnectOptionToSummary(_travellerPopulationCodeOption);
        ConnectOptionToSummary(_starSpectralClassOption);
        ConnectOptionToSummary(_starSubclassOption);
        ConnectOptionToSummary(_asteroidTypeOption);
        ConnectOptionToSummary(_asteroidOrbitBandOption);
        ConnectOptionToSummary(_asteroidDensityProfileOption);
        ConnectOptionToSummary(_asteroidAlbedoProfileOption);
        ConnectOptionToSummary(_cometFamilyOption);
        ConnectOptionToSummary(_cometActivityOption);

        if (_moonCapturedCheck != null)
        {
            _moonCapturedCheck.Toggled += _ => RefreshSummary();
        }

        if (_planetGenerateMoonCheck != null)
        {
            _planetGenerateMoonCheck.Toggled += _ =>
            {
                RefreshEnhancedParameterVisibility();
                RefreshSummary();
            };
        }

        if (_asteroidLargeCheck != null)
        {
            _asteroidLargeCheck.Toggled += _ => RefreshSummary();
        }

        if (_cometLargeCheck != null)
        {
            _cometLargeCheck.Toggled += _ => RefreshSummary();
        }

        foreach (KeyValuePair<string, CheckBox> entry in _optionalToggles)
        {
            string key = entry.Key;
            entry.Value.Toggled += _ => OnEnhancedOptionalToggleChanged(key);
        }

        foreach (KeyValuePair<string, SpinBox> entry in _optionalInputs)
        {
            entry.Value.ValueChanged += _ => RefreshSummary();
        }
    }

    private void ApplyEnhancedDefaults()
    {
        if (_typeOption != null)
        {
            SelectOptionById(_typeOption, (int)ObjectViewer.ObjectType.Planet);
        }

        if (_rulesetModeOption != null)
        {
            SelectOptionById(_rulesetModeOption, (int)GenerationUseCaseSettings.RulesetModeType.Default);
        }

        ResetEnhancedOptionalInputs();
        ApplyObjectLifeDefaults();
        RebuildEnhancedPresetOptions();
        RefreshEnhancedFieldPresentation();
        RefreshEnhancedParameterVisibility();
    }

    private void OnEnhancedTypeChanged()
    {
        RebuildEnhancedPresetOptions();
        RefreshEnhancedFieldPresentation();
        RefreshEnhancedParameterVisibility();
        RefreshSummary();
    }

    private void OnEnhancedRulesetModeSelected(long selectedId)
    {
        if (selectedId == (long)GenerationUseCaseSettings.RulesetModeType.Traveller)
        {
            if (_showTravellerReadoutsCheck != null)
            {
                _showTravellerReadoutsCheck.ButtonPressed = true;
            }

            if (_useTravellerWorldProfileCheck != null && GetSelectedObjectType() == ObjectViewer.ObjectType.Planet)
            {
                _useTravellerWorldProfileCheck.ButtonPressed = true;
            }

            ApplyTravellerObjectLifeDefaultsToControls();

        }
        else
        {
            if (_showTravellerReadoutsCheck != null)
            {
                _showTravellerReadoutsCheck.ButtonPressed = false;
            }

            if (_useTravellerWorldProfileCheck != null)
            {
                _useTravellerWorldProfileCheck.ButtonPressed = false;
            }
        }

        RefreshEnhancedParameterVisibility();
        RefreshSummary();
    }

    private void RefreshEnhancedSummary()
    {
        ObjectGenerationRequest request = BuildEnhancedRequest();
        if (_summaryLabel != null)
        {
            List<string> lines = new();
            lines.Add($"Type {request.ObjectType}");
            lines.Add($"Preset {GetSelectedPresetLabel()}");
            if (!string.IsNullOrWhiteSpace(_nameInput?.Text))
            {
                lines.Add($"Name {_nameInput.Text}");
            }

            if (_showSeedControls)
            {
                lines.Add($"Seed {request.SeedValue}");
            }

            lines.Add($"Ruleset {GenerationUseCasePresentation.GetRulesetLabel(request.UseCaseSettings.RulesetMode)}");
            lines.Add($"Show UWP Code {(request.UseCaseSettings.ShowTravellerReadouts ? "On" : "Off")}");
            string lifeSummary = BuildObjectLifeSummary();
            if (!string.IsNullOrWhiteSpace(lifeSummary))
            {
                lines.Add(lifeSummary);
            }
            if (_planetGenerateMoonCheck != null && _planetGenerateMoonCheck.ButtonPressed)
            {
                lines.Add($"Moon Target {GetSelectedMoonTargetLabel()}");
                if (_moonCapturedCheck != null)
                {
                    if (_moonCapturedCheck.ButtonPressed)
                    {
                        lines.Add("Moon Bias Captured");
                    }
                    else
                    {
                        lines.Add("Moon Bias Regular");
                    }
                }
            }

            if (request.TravellerWorldProfileData.Count > 0)
            {
                TravellerWorldProfile profile = TravellerWorldProfile.FromDictionary(request.TravellerWorldProfileData);
                lines.Add($"UWP {profile.ToUwpString()}");
                lines.Add($"Atmosphere {TravellerWorldGenerator.DescribeAtmosphereCode(profile.AtmosphereCode)}");
            }

            _summaryLabel.Text = string.Join("\n", lines);
        }

        if (_assumptionsLabel != null)
        {
            _assumptionsLabel.Text = BuildEnhancedAssumptionText();
            _assumptionsLabel.TooltipText = BuildEnhancedAssumptionTooltip();
        }

        RefreshEnhancedIssuesUi();
    }

    private GenerationUseCaseSettings BuildEnhancedUseCaseSettingsFromControls()
    {
        GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
        if (_rulesetModeOption != null)
        {
            settings.RulesetMode = (GenerationUseCaseSettings.RulesetModeType)_rulesetModeOption.GetSelectedId();
        }

        if (_showTravellerReadoutsCheck != null)
        {
            settings.ShowTravellerReadouts = _showTravellerReadoutsCheck.ButtonPressed;
        }

        if (settings.IsTravellerMode())
        {
            settings.ShowTravellerReadouts = true;
        }
        else
        {
            settings.ShowTravellerReadouts = false;
        }

        if (GetSelectedObjectType() == ObjectViewer.ObjectType.Planet)
        {
            ApplyObjectLifeSettingsFromControls(settings);
        }

        return settings;
    }

    private void RefreshEnhancedParameterVisibility()
    {
        ObjectViewer.ObjectType objectType = GetSelectedObjectType();
        bool showAdvanced = _showAdvancedControlsCheck != null && _showAdvancedControlsCheck.ButtonPressed;
        bool travellerMode = IsTravellerModeSelected();
        bool useTravellerProfile = travellerMode
            && _useTravellerWorldProfileCheck != null
            && _useTravellerWorldProfileCheck.ButtonPressed;

        SetEnhancedRowVisible("SeedRow", _showSeedControls);
        SetEnhancedSectionVisible(_planetSection, objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedSectionVisible(_lifeSection, objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedSectionVisible(_travellerSection, objectType == ObjectViewer.ObjectType.Planet && travellerMode);
        SetEnhancedSectionVisible(_starSection, objectType == ObjectViewer.ObjectType.Star);
        SetEnhancedSectionVisible(_asteroidSection, objectType == ObjectViewer.ObjectType.Asteroid);
        SetEnhancedSectionVisible(_cometSection, objectType == ObjectViewer.ObjectType.Comet);
        SetEnhancedSectionVisible(_advancedSection, showAdvanced);

        SetEnhancedRowVisible("ShowTravellerReadoutsRow", travellerMode);
        SetEnhancedRowVisible("PlanetOrbitModeRow", objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetClassBiasRow", objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetCompositionBiasRow", objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetEnvelopeOverrideRow", objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetVolatileRichnessRow", objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetHydrosphereTendencyRow", objectType == ObjectViewer.ObjectType.Planet);
        bool rogueMode = objectType == ObjectViewer.ObjectType.Planet
            && _planetOrbitModeOption != null
            && _planetOrbitModeOption.GetSelectedId() == (int)PlanetOrbitMode.Rogue;
        SetEnhancedRowVisible("PlanetSizeCategoryRow", !useTravellerProfile && objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetOrbitZoneRow", !useTravellerProfile && objectType == ObjectViewer.ObjectType.Planet && !rogueMode);
        SetEnhancedRowVisible("PlanetAtmosphereRow", !useTravellerProfile && objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetSurfacePressureRow", objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetOceanCoverageRow", objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetIceCoverageRow", objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetAlbedoProfileRow", objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetVolcanismRow", objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetGenerateMoonRow", objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible(
            "MoonTargetCountRow",
            objectType == ObjectViewer.ObjectType.Planet
                && _planetGenerateMoonCheck != null
                && _planetGenerateMoonCheck.ButtonPressed);
        SetEnhancedRowVisible(
            "MoonCapturedRow",
            objectType == ObjectViewer.ObjectType.Planet
                && _planetGenerateMoonCheck != null
                && _planetGenerateMoonCheck.ButtonPressed);
        SetEnhancedRowVisible("StarSubclassRow", objectType == ObjectViewer.ObjectType.Star);

        SetEnhancedRowVisible("MassOverrideRow", showAdvanced);
        SetEnhancedRowVisible("RadiusOverrideRow", showAdvanced);
        SetEnhancedRowVisible("RotationOverrideRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("AxialTiltOverrideRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("SemiMajorAxisOverrideRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("EccentricityOverrideRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("InclinationOverrideRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("SurfacePressureOverrideRow", showAdvanced && objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("AlbedoOverrideRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("VolcanismOverrideRow", showAdvanced && objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("TemperatureOverrideRow", showAdvanced && objectType == ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("LuminosityOverrideRow", showAdvanced && objectType == ObjectViewer.ObjectType.Star);
    }

    private void RefreshEnhancedFieldPresentation()
    {
        ObjectViewer.ObjectType objectType = GetSelectedObjectType();
        if (objectType == ObjectViewer.ObjectType.Star)
        {
            SetEnhancedRowLabel("MassOverrideRow", "Mass (solar)");
            SetEnhancedRowLabel("RadiusOverrideRow", "Radius (solar)");
            return;
        }

        if (objectType == ObjectViewer.ObjectType.Asteroid)
        {
            SetEnhancedRowLabel("MassOverrideRow", "Mass (10^15 kg)");
            SetEnhancedRowLabel("RadiusOverrideRow", "Radius (km)");
            return;
        }

        if (objectType == ObjectViewer.ObjectType.Comet)
        {
            SetEnhancedRowLabel("MassOverrideRow", "Mass (10^15 kg)");
            SetEnhancedRowLabel("RadiusOverrideRow", "Radius (km)");
            return;
        }

        SetEnhancedRowLabel("MassOverrideRow", "Mass (earth)");
        SetEnhancedRowLabel("RadiusOverrideRow", "Radius (earth)");
    }

    private void RefreshEnhancedIssuesUi()
    {
        if (_issuesContainer == null)
        {
            return;
        }

        foreach (Node child in _issuesContainer.GetChildren())
        {
            child.QueueFree();
        }

        AddEnhancedIssueLabel(GetPresetAssumptionText(GetSelectedObjectType(), _presetOption?.GetSelectedId() ?? 0));
        if (ShouldUseTravellerWorldGeneration())
        {
            AddEnhancedIssueLabel("Traveller profile uses SRD-style UWP rules before body generation.");
        }

        if (_planetGenerateMoonCheck != null && _planetGenerateMoonCheck.ButtonPressed)
        {
            AddEnhancedIssueLabel($"Planet launch will also generate {GetSelectedMoonTargetDescription()} using the same seed family, then cap the final count by planet size.");
        }

        if (_showAdvancedControlsCheck != null && _showAdvancedControlsCheck.ButtonPressed)
        {
            AddEnhancedIssueLabel("Advanced overrides match the editor override keys.");
        }

        AddPlanetScienceIssueNotes();
    }

    private void OnEnhancedAdvancedToggled()
    {
        RefreshEnhancedParameterVisibility();
        RefreshSummary();
    }

    private void OnEnhancedTravellerProfileToggled()
    {
        RefreshEnhancedParameterVisibility();
        RefreshSummary();
    }

    private void OnPopulationPermissivenessChanged(double _value)
    {
        RefreshSummary();
    }

    private void OnEnhancedOptionalToggleChanged(string key)
    {
        if (_optionalInputs.TryGetValue(key, out SpinBox? input)
            && _optionalToggles.TryGetValue(key, out CheckBox? toggle))
        {
            input.Editable = toggle.ButtonPressed;
        }

        RefreshSummary();
    }

    private void PopulatePlanetSection()
    {
        PopulatePlanetOrbitModeOptions(_planetOrbitModeOption);
        PopulatePlanetClassBiasOptions(_planetClassBiasOption);
        PopulatePlanetCompositionBiasOptions(_planetCompositionBiasOption);
        PopulatePlanetEnvelopeOverrideOptions(_planetEnvelopeOverrideOption);
        PopulatePlanetVolatileRichnessOptions(_planetVolatileRichnessOption);
        PopulatePlanetHydrosphereTendencyOptions(_planetHydrosphereTendencyOption);
        PopulateAutoSizeOptions(_planetSizeCategoryOption);
        PopulateAutoOrbitZoneOptions(_planetOrbitZoneOption);
        PopulateAutoBoolOptions(_planetAtmosphereOption);
        PopulateAutoBoolOptions(_planetRingsOption);
        PopulateAutoRingComplexityOptions(_planetRingComplexityOption);
        PopulateProfileLevelOptions(_planetSurfacePressureOption, "Auto", "Thin", "Moderate", "Dense");
        PopulateProfileLevelOptions(_planetOceanCoverageOption, "Auto", "Dry", "Mixed", "Ocean World");
        PopulateProfileLevelOptions(_planetIceCoverageOption, "Auto", "Ice-free", "Seasonal", "Frozen");
        PopulateProfileLevelOptions(_planetAlbedoProfileOption, "Auto", "Dark", "Balanced", "Bright");
        PopulateProfileLevelOptions(_planetVolcanismOption, "Auto", "Quiet", "Active", "Extreme");
        PopulateMoonTargetCountOptions(_moonTargetCountOption);
        ApplyCatalogTooltip("planet_orbit_mode", "PlanetOrbitModeRow", _planetOrbitModeOption);
        ApplyCatalogTooltip("planet_class_bias", "PlanetClassBiasRow", _planetClassBiasOption);
        ApplyCatalogTooltip("planet_composition_bias", "PlanetCompositionBiasRow", _planetCompositionBiasOption);
        ApplyCatalogTooltip("planet_envelope_override", "PlanetEnvelopeOverrideRow", _planetEnvelopeOverrideOption);
        ApplyCatalogTooltip("planet_volatile_richness", "PlanetVolatileRichnessRow", _planetVolatileRichnessOption);
        ApplyCatalogTooltip("planet_hydrosphere_tendency", "PlanetHydrosphereTendencyRow", _planetHydrosphereTendencyOption);
        ApplyCatalogTooltip("planet_size_category", "PlanetSizeCategoryRow", _planetSizeCategoryOption);
        ApplyCatalogTooltip("planet_orbit_zone", "PlanetOrbitZoneRow", _planetOrbitZoneOption);
        ApplyCatalogTooltip("planet_atmosphere", "PlanetAtmosphereRow", _planetAtmosphereOption);
        ApplyCatalogTooltip("planet_rings", "PlanetRingsRow", _planetRingsOption);
        ApplyCatalogTooltip("planet_ring_complexity", "PlanetRingComplexityRow", _planetRingComplexityOption);
        ApplyCatalogTooltip("planet_surface_pressure", "PlanetSurfacePressureRow", _planetSurfacePressureOption);
        ApplyCatalogTooltip("planet_ocean_coverage", "PlanetOceanCoverageRow", _planetOceanCoverageOption);
        ApplyCatalogTooltip("planet_ice_coverage", "PlanetIceCoverageRow", _planetIceCoverageOption);
        ApplyCatalogTooltip("planet_albedo_profile", "PlanetAlbedoProfileRow", _planetAlbedoProfileOption);
        ApplyCatalogTooltip("planet_volcanism", "PlanetVolcanismRow", _planetVolcanismOption);
        ApplyCatalogTooltip("planet_generate_moons", "PlanetGenerateMoonRow", _planetGenerateMoonCheck);
        ApplyCatalogTooltip("planet_moon_target_count", "MoonTargetCountRow", _moonTargetCountOption);
        ApplyCatalogTooltip("planet_moon_captured_bias", "MoonCapturedRow", _moonCapturedCheck);
    }

    private void PopulateTravellerSection()
    {
        PopulateTravellerCodeOptions(_travellerSizeCodeOption, "size");
        PopulateTravellerCodeOptions(_travellerAtmosphereCodeOption, "atmosphere");
        PopulateTravellerCodeOptions(_travellerHydrographicsCodeOption, "hydrographics");
        PopulateTravellerCodeOptions(_travellerPopulationCodeOption, "population");
        ApplyCatalogTooltip("use_traveller_world_profile", "UseTravellerWorldProfileRow", _useTravellerWorldProfileCheck);
        ApplyCatalogTooltip("traveller_size_code", "TravellerSizeCodeRow", _travellerSizeCodeOption);
        ApplyCatalogTooltip("traveller_atmosphere_code", "TravellerAtmosphereCodeRow", _travellerAtmosphereCodeOption);
        ApplyCatalogTooltip("traveller_hydrographics_code", "TravellerHydrographicsCodeRow", _travellerHydrographicsCodeOption);
        ApplyCatalogTooltip("traveller_population_code", "TravellerPopulationCodeRow", _travellerPopulationCodeOption);
    }

    private void PopulateStarSection()
    {
        PopulateStarClassOptions(_starSpectralClassOption);
        PopulateStarSubclassOptions(_starSubclassOption);
        ApplyCatalogTooltip("star_spectral_class", "StarSpectralClassRow", _starSpectralClassOption);
        ApplyCatalogTooltip("star_subclass", "StarSubclassRow", _starSubclassOption);
    }

    private void PopulateAsteroidSection()
    {
        PopulateAsteroidTypeOptions(_asteroidTypeOption);
        PopulateProfileLevelOptions(_asteroidOrbitBandOption, "Auto", "Inner", "Main Belt", "Outer");
        PopulateProfileLevelOptions(_asteroidDensityProfileOption, "Auto", "Loose", "Typical", "Dense");
        PopulateProfileLevelOptions(_asteroidAlbedoProfileOption, "Auto", "Dark", "Balanced", "Bright");
        ApplyCatalogTooltip("asteroid_type", "AsteroidTypeRow", _asteroidTypeOption);
        ApplyCatalogTooltip("asteroid_large", "AsteroidLargeRow", _asteroidLargeCheck);
        ApplyCatalogTooltip("asteroid_orbit_band", "AsteroidOrbitBandRow", _asteroidOrbitBandOption);
        ApplyCatalogTooltip("asteroid_density_profile", "AsteroidDensityProfileRow", _asteroidDensityProfileOption);
        ApplyCatalogTooltip("asteroid_albedo_profile", "AsteroidAlbedoProfileRow", _asteroidAlbedoProfileOption);
    }

    private void PopulateCometSection()
    {
        PopulateProfileLevelOptions(_cometFamilyOption, "Auto", "Jupiter-family", "Long-period");
        PopulateProfileLevelOptions(_cometActivityOption, "Auto", "Active", "Dormant", "Extinct");
        ApplyCatalogTooltip("comet_family", "CometFamilyRow", _cometFamilyOption);
        ApplyCatalogTooltip("comet_activity", "CometActivityRow", _cometActivityOption);
        ApplyCatalogTooltip("comet_large", "CometLargeRow", _cometLargeCheck);
    }

    private void PopulateAdvancedSection()
    {
        SetEnhancedRowLabel("ShowTravellerReadoutsRow", "Show UWP Code");
        ApplyCatalogTooltip("advanced_controls", "ShowAdvancedControlsRow", _showAdvancedControlsCheck);
        ApplyCatalogTooltip("ruleset_mode", "RulesetRow", _rulesetModeOption);
        ApplyCatalogTooltip("show_traveller_readouts", "ShowTravellerReadoutsRow", _showTravellerReadoutsCheck);
    }

    private void AddPlanetScienceIssueNotes()
    {
        if (GetSelectedObjectType() != ObjectViewer.ObjectType.Planet)
        {
            return;
        }

        Variant atmospherePreference = GetTriStatePreference(_planetAtmosphereOption);
        bool atmosphereForcedOff = atmospherePreference.VariantType == Variant.Type.Bool && !(bool)atmospherePreference;
        PlanetEnvelopeOverride envelopeOverride = (PlanetEnvelopeOverride)(_planetEnvelopeOverrideOption?.GetSelectedId() ?? (int)PlanetEnvelopeOverride.Auto);
        PlanetHydrosphereTendency hydrosphereTendency = (PlanetHydrosphereTendency)(_planetHydrosphereTendencyOption?.GetSelectedId() ?? (int)PlanetHydrosphereTendency.Auto);
        int oceanCoverageId = _planetOceanCoverageOption?.GetSelectedId() ?? -1;
        int surfacePressureId = _planetSurfacePressureOption?.GetSelectedId() ?? -1;
        PlanetClassBias classBias = (PlanetClassBias)(_planetClassBiasOption?.GetSelectedId() ?? (int)PlanetClassBias.Auto);
        GenerationUseCaseSettings.CivilizationModelType civilizationModel =
            (GenerationUseCaseSettings.CivilizationModelType)(_civilizationModelOption?.GetSelectedId()
            ?? (int)GenerationUseCaseSettings.CivilizationModelType.FollowFramework);

        IReadOnlyList<ObjectScienceIssueNote> notes = ObjectScienceReferenceCatalog.BuildPlanetIssueNotes(
            atmosphereForcedOff,
            envelopeOverride,
            hydrosphereTendency,
            oceanCoverageId,
            surfacePressureId,
            classBias,
            civilizationModel);

        foreach (ObjectScienceIssueNote note in notes)
        {
            AddEnhancedIssueLabel(note.Text, note.Tooltip);
        }
    }

    private string BuildEnhancedAssumptionText()
    {
        if (ShouldUseTravellerWorldGeneration())
        {
            return "Traveller world profile is generated before launch.";
        }

        if (_planetGenerateMoonCheck != null && _planetGenerateMoonCheck.ButtonPressed)
        {
            return $"Planet launch targets {GetSelectedMoonTargetLabel().ToLowerInvariant()} and caps the final moon count by planet size.";
        }

        if (GetSelectedObjectType() == ObjectViewer.ObjectType.Planet)
        {
            return "Direct planet controls and local life settings persist into the generated world.";
        }

        if (_showAdvancedControlsCheck != null && _showAdvancedControlsCheck.ButtonPressed)
        {
            return "Advanced mode exposes direct physical override keys for this one object.";
        }

        return "Direct object controls persist into the generated body.";
    }

    private string BuildEnhancedAssumptionTooltip()
    {
        if (ShouldUseTravellerWorldGeneration())
        {
            return "Traveller mode builds a world profile first, then maps that profile into the body generator while keeping the result deterministic.";
        }

        if (_planetGenerateMoonCheck != null && _planetGenerateMoonCheck.ButtonPressed)
        {
            return $"The planet generator will also create {GetSelectedMoonTargetDescription()} from the same seed family. The target count is capped by the generated planet size so small worlds do not end up with giant-planet moon counts. Captured mode biases those moons toward irregular outsider satellites rather than regular formed-with-the-planet moons.";
        }

        if (GetSelectedObjectType() == ObjectViewer.ObjectType.Planet)
        {
            return "Planet life settings apply only to this one world. Conflict notes call out when direct atmosphere, hydrosphere, envelope, moon, or civilization choices pull against the reviewed science behind the normal generator.";
        }

        if (_showAdvancedControlsCheck != null && _showAdvancedControlsCheck.ButtonPressed)
        {
            return "Advanced controls use the same override keys as the object editor, so creation and later editing stay aligned for this one object.";
        }

        return "Preset assumptions and direct object settings are persisted into the generated body so downstream inspection stays aligned.";
    }

    private void AddEnhancedIssueLabel(string text)
    {
        AddEnhancedIssueLabel(text, string.Empty);
    }

    private void AddEnhancedIssueLabel(string text, string tooltipText)
    {
        if (_issuesContainer == null || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        Label noteLabel = UiSceneTemplates.InstantiateMessageLabel();
        noteLabel.Modulate = new Color(0.85f, 0.7f, 0.3f, 1.0f);
        noteLabel.Text = text;
        noteLabel.TooltipText = tooltipText;
        _issuesContainer.AddChild(noteLabel);
    }

    private void ResetEnhancedOptionalInputs()
    {
        foreach (KeyValuePair<string, CheckBox> entry in _optionalToggles)
        {
            entry.Value.ButtonPressed = false;
        }

        foreach (KeyValuePair<string, SpinBox> entry in _optionalInputs)
        {
            entry.Value.Editable = false;
        }
    }

    private void ConnectOptionToSummary(OptionButton? optionButton)
    {
        if (optionButton != null)
        {
            optionButton.ItemSelected += _ => RefreshSummary();
        }
    }

    private void SetEnhancedSectionVisible(Control? control, bool visible)
    {
        if (control != null)
        {
            control.Visible = visible;
        }
    }

    private void SetEnhancedRowVisible(string key, bool visible)
    {
        if (_rows.TryGetValue(key, out HBoxContainer? row))
        {
            row.Visible = visible;
        }
    }

    private void SetEnhancedRowLabel(string rowName, string text)
    {
        if (_rowLabels.TryGetValue(rowName, out Label? label))
        {
            label.Text = text;
        }
    }

    private bool IsTravellerModeSelected()
    {
        return _rulesetModeOption != null
            && _rulesetModeOption.GetSelectedId() == (int)GenerationUseCaseSettings.RulesetModeType.Traveller;
    }

    private void CacheOptionalOverride(string key)
    {
        HBoxContainer row = GetRequiredRow($"{key}Row");
        _optionalToggles[key] = GetRequiredChild<CheckBox>(row, $"{key}Toggle");
        _optionalInputs[key] = GetRequiredChild<SpinBox>(row, $"{key}Input");
    }

    private HBoxContainer GetRequiredRow(string rowName)
    {
        HBoxContainer? row = FindChild(rowName, true, false) as HBoxContainer;
        if (row == null)
        {
            throw new InvalidOperationException($"ObjectGenerationScreen is missing row '{rowName}'.");
        }

        row.AddThemeConstantOverride("separation", 10);
        Label label = GetRequiredChild<Label>(row, $"{rowName}Label");
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _rows[rowName] = row;
        _rowLabels[rowName] = label;
        return row;
    }

    private VBoxContainer? GetOptionalSection(string sectionName)
    {
        return FindChild(sectionName, true, false) as VBoxContainer;
    }

    private OptionButton GetRequiredOptionButton(string rowName, string controlName)
    {
        HBoxContainer row = GetRequiredRow(rowName);
        return GetRequiredChild<OptionButton>(row, controlName);
    }

    private SpinBox GetRequiredSpinBox(string rowName, string controlName)
    {
        HBoxContainer row = GetRequiredRow(rowName);
        return GetRequiredChild<SpinBox>(row, controlName);
    }

    private LineEdit GetRequiredLineEdit(string rowName, string controlName)
    {
        HBoxContainer row = GetRequiredRow(rowName);
        return GetRequiredChild<LineEdit>(row, controlName);
    }

    private CheckBox GetRequiredCheckBox(string rowName, string controlName)
    {
        HBoxContainer row = GetRequiredRow(rowName);
        return GetRequiredChild<CheckBox>(row, controlName);
    }

    private HSlider GetRequiredSlider(string rowName, string controlName)
    {
        HBoxContainer row = GetRequiredRow(rowName);
        return GetRequiredChild<HSlider>(row, controlName);
    }

    private Label GetRequiredLabel(string rowName, string controlName)
    {
        HBoxContainer row = GetRequiredRow(rowName);
        return GetRequiredChild<Label>(row, controlName);
    }

    private static T GetRequiredChild<T>(Node parent, string childName) where T : Node
    {
        T? child = parent.GetNodeOrNull<T>(childName);
        if (child == null)
        {
            throw new InvalidOperationException($"ObjectGenerationScreen is missing child '{childName}' under '{parent.Name}'.");
        }

        return child;
    }

    private void ApplyCatalogTooltip(string parameterId, string rowName, Control? input)
    {
        string tooltip = ObjectGenerationParameterCatalog.GetTooltip(parameterId);
        if (_rowLabels.TryGetValue(rowName, out Label? label))
        {
            label.TooltipText = tooltip;
        }

        if (input != null)
        {
            input.TooltipText = tooltip;
        }
    }

    private void PopulateAutoSizeOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", -1);
        foreach (SizeCategory.Category category in Enum.GetValues<SizeCategory.Category>())
        {
            optionButton.AddItem(SizeCategory.ToStringName(category), (int)category);
        }
    }

    private void PopulateAutoOrbitZoneOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", -1);
        foreach (OrbitZone.Zone zone in Enum.GetValues<OrbitZone.Zone>())
        {
            optionButton.AddItem(OrbitZone.ToStringName(zone), (int)zone);
        }
    }

    private void PopulatePlanetOrbitModeOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", (int)PlanetOrbitMode.Auto);
        optionButton.AddItem("Bound", (int)PlanetOrbitMode.Bound);
        optionButton.AddItem("Rogue", (int)PlanetOrbitMode.Rogue);
    }

    private void PopulatePlanetClassBiasOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", (int)PlanetClassBias.Auto);
        optionButton.AddItem("Rocky", (int)PlanetClassBias.Rocky);
        optionButton.AddItem("Water-rich", (int)PlanetClassBias.WaterRich);
        optionButton.AddItem("Sub-Neptune", (int)PlanetClassBias.SubNeptune);
        optionButton.AddItem("Gas Giant", (int)PlanetClassBias.GasGiant);
        optionButton.AddItem("Stripped Core", (int)PlanetClassBias.StrippedCore);
    }

    private void PopulatePlanetCompositionBiasOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", (int)PlanetCompositionBias.Auto);
        optionButton.AddItem("Rocky", (int)PlanetCompositionBias.Rocky);
        optionButton.AddItem("Ice or Water-rich", (int)PlanetCompositionBias.IcyWaterRich);
        optionButton.AddItem("Gas Envelope", (int)PlanetCompositionBias.GasEnvelope);
    }

    private void PopulatePlanetEnvelopeOverrideOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", (int)PlanetEnvelopeOverride.Auto);
        optionButton.AddItem("Thin", (int)PlanetEnvelopeOverride.Thin);
        optionButton.AddItem("Retained", (int)PlanetEnvelopeOverride.Retained);
        optionButton.AddItem("Stripped", (int)PlanetEnvelopeOverride.Stripped);
    }

    private void PopulatePlanetVolatileRichnessOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", (int)PlanetVolatileRichness.Auto);
        optionButton.AddItem("Poor", (int)PlanetVolatileRichness.Poor);
        optionButton.AddItem("Moderate", (int)PlanetVolatileRichness.Moderate);
        optionButton.AddItem("Rich", (int)PlanetVolatileRichness.Rich);
    }

    private void PopulatePlanetHydrosphereTendencyOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", (int)PlanetHydrosphereTendency.Auto);
        optionButton.AddItem("Dry", (int)PlanetHydrosphereTendency.Dry);
        optionButton.AddItem("Mixed", (int)PlanetHydrosphereTendency.Mixed);
        optionButton.AddItem("Oceanic", (int)PlanetHydrosphereTendency.Oceanic);
    }

    private void PopulateAutoRingComplexityOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", -1);
        foreach (RingComplexity.Level level in Enum.GetValues<RingComplexity.Level>())
        {
            optionButton.AddItem(RingComplexity.ToStringName(level), (int)level);
        }
    }

    private void PopulateStarClassOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", -1);
        foreach (StarClass.SpectralClass spectralClass in Enum.GetValues<StarClass.SpectralClass>())
        {
            optionButton.AddItem(StarClass.ToLetter(spectralClass), (int)spectralClass);
        }
    }

    private void PopulateAsteroidTypeOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", -1);
        foreach (AsteroidType.Type asteroidType in Enum.GetValues<AsteroidType.Type>())
        {
            optionButton.AddItem(AsteroidType.ToStringName(asteroidType), (int)asteroidType);
        }
    }

    private void PopulateAutoBoolOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", -1);
        optionButton.AddItem("Yes", 1);
        optionButton.AddItem("No", 0);
    }

    private void PopulateProfileLevelOptions(OptionButton? optionButton, params string[] labels)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        for (int index = 0; index < labels.Length; index++)
        {
            optionButton.AddItem(labels[index], index - 1);
        }
    }

    private void PopulateMoonTargetCountOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", -1);
        for (int targetCount = 1; targetCount <= 12; targetCount += 1)
        {
            optionButton.AddItem(targetCount.ToString(), targetCount);
        }
    }

    private void PopulateStarSubclassOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", -1);
        for (int subclass = 0; subclass <= 9; subclass++)
        {
            optionButton.AddItem(subclass.ToString(), subclass);
        }
    }

    private void PopulateTravellerCodeOptions(OptionButton? optionButton, string kind)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Auto", -1);
        int maxCode = 10;
        if (kind == "atmosphere" || kind == "population")
        {
            maxCode = 15;
        }

        for (int code = 0; code <= maxCode; code++)
        {
            string token = TravellerWorldProfile.ToHexDigit(code);
            string label = token;
            if (kind == "atmosphere")
            {
                label = $"{token} {TravellerWorldGenerator.DescribeAtmosphereCode(code)}";
            }

            optionButton.AddItem(label, code);
        }
    }

    private string GetSelectedMoonTargetLabel()
    {
        if (_moonTargetCountOption == null)
        {
            return "Auto";
        }

        int selectedId = _moonTargetCountOption.GetSelectedId();
        if (selectedId < 1)
        {
            return "Auto";
        }

        return selectedId.ToString();
    }

    private string GetSelectedMoonTargetDescription()
    {
        if (_moonTargetCountOption == null)
        {
            return "an automatic moon count";
        }

        int selectedId = _moonTargetCountOption.GetSelectedId();
        if (selectedId < 1)
        {
            return "an automatic moon count based on planet size";
        }

        if (selectedId == 1)
        {
            return "1 moon";
        }

        return $"{selectedId} moons";
    }
}
