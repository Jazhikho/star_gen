using System;
using System.Collections.Generic;
using Godot;
using StarGen.App.Shared;
using StarGen.App.Viewer;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
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
    private OptionButton? _planetSizeCategoryOption;
    private OptionButton? _planetOrbitZoneOption;
    private OptionButton? _planetAtmosphereOption;
    private OptionButton? _planetRingsOption;
    private OptionButton? _planetRingComplexityOption;

    private VBoxContainer? _travellerSection;
    private CheckBox? _useTravellerWorldProfileCheck;
    private OptionButton? _travellerSizeCodeOption;
    private OptionButton? _travellerAtmosphereCodeOption;
    private OptionButton? _travellerHydrographicsCodeOption;
    private OptionButton? _travellerPopulationCodeOption;

    private VBoxContainer? _moonSection;
    private OptionButton? _moonSizeCategoryOption;
    private CheckBox? _moonCapturedCheck;
    private OptionButton? _moonAtmosphereOption;
    private OptionButton? _moonOceanOption;

    private VBoxContainer? _starSection;
    private OptionButton? _starSpectralClassOption;

    private VBoxContainer? _asteroidSection;
    private OptionButton? _asteroidTypeOption;
    private CheckBox? _asteroidLargeCheck;

    private VBoxContainer? _advancedSection;

    private void BuildEnhancedParameterUi()
    {
        if (_parameterVBox == null || _typeOption != null)
        {
            return;
        }

        _typeOption = AddEnhancedOptionRow("TypeRow", "Type");
        _typeOption.Name = "TypeOption";
        _typeOption.AddItem("Star", (int)ObjectViewer.ObjectType.Star);
        _typeOption.AddItem("Planet", (int)ObjectViewer.ObjectType.Planet);
        _typeOption.AddItem("Moon", (int)ObjectViewer.ObjectType.Moon);
        _typeOption.AddItem("Asteroid", (int)ObjectViewer.ObjectType.Asteroid);

        _presetOption = AddEnhancedOptionRow("PresetRow", "Preset");
        _presetOption.Name = "PresetOption";
        _nameInput = AddEnhancedLineEditRow("NameRow", "Name");
        _nameInput.Name = "NameInput";

        _seedInput = AddEnhancedSpinRow("SeedRow", "Seed", 1.0, 999999.0, 1.0);
        _seedInput.Name = "SeedInput";
        _seedRow = _seedInput.GetParent() as HBoxContainer;

        _rulesetModeOption = AddEnhancedOptionRow("RulesetRow", "Ruleset");
        _rulesetModeOption.Name = "RulesetModeOption";
        _rulesetModeOption.AddItem("Default", (int)GenerationUseCaseSettings.RulesetModeType.Default);
        _rulesetModeOption.AddItem("Traveller", (int)GenerationUseCaseSettings.RulesetModeType.Traveller);

        _showTravellerReadoutsCheck = AddEnhancedCheckRow("ShowTravellerReadoutsRow", "Traveller Readouts");
        _showTravellerReadoutsCheck.Name = "ShowTravellerReadoutsCheck";
        _showAdvancedControlsCheck = AddEnhancedCheckRow("ShowAdvancedControlsRow", "Advanced Controls");
        _showAdvancedControlsCheck.Name = "ShowAdvancedControlsCheck";

        BuildPlanetSection();
        BuildTravellerSection();
        BuildMoonSection();
        BuildStarSection();
        BuildAsteroidSection();
        BuildAdvancedSection();

        RebuildEnhancedPresetOptions();
    }

    private void ConnectEnhancedSignals()
    {
        if (_startButton != null)
        {
            _startButton.Pressed += OnStartPressed;
        }

        if (_loadButton != null)
        {
            _loadButton.Pressed += () => EmitSignal(SignalName.load_object_requested);
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

        if (_lifePermissivenessInput != null)
        {
            _lifePermissivenessInput.ValueChanged += OnLifePermissivenessChanged;
        }

        if (_populationPermissivenessInput != null)
        {
            _populationPermissivenessInput.ValueChanged += OnPopulationPermissivenessChanged;
        }

        ConnectOptionToSummary(_planetSizeCategoryOption);
        ConnectOptionToSummary(_planetOrbitZoneOption);
        ConnectOptionToSummary(_planetAtmosphereOption);
        ConnectOptionToSummary(_planetRingsOption);
        ConnectOptionToSummary(_planetRingComplexityOption);
        ConnectOptionToSummary(_travellerSizeCodeOption);
        ConnectOptionToSummary(_travellerAtmosphereCodeOption);
        ConnectOptionToSummary(_travellerHydrographicsCodeOption);
        ConnectOptionToSummary(_travellerPopulationCodeOption);
        ConnectOptionToSummary(_moonSizeCategoryOption);
        ConnectOptionToSummary(_moonAtmosphereOption);
        ConnectOptionToSummary(_moonOceanOption);
        ConnectOptionToSummary(_starSpectralClassOption);
        ConnectOptionToSummary(_asteroidTypeOption);

        if (_moonCapturedCheck != null)
        {
            _moonCapturedCheck.Toggled += _ => RefreshSummary();
        }

        if (_asteroidLargeCheck != null)
        {
            _asteroidLargeCheck.Toggled += _ => RefreshSummary();
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

        if (_seedInput != null)
        {
            _seedInput.Value = 12345.0;
        }

        if (_rulesetModeOption != null)
        {
            SelectOptionById(_rulesetModeOption, (int)GenerationUseCaseSettings.RulesetModeType.Default);
        }

        if (_showTravellerReadoutsCheck != null)
        {
            _showTravellerReadoutsCheck.ButtonPressed = false;
        }

        if (_showAdvancedControlsCheck != null)
        {
            _showAdvancedControlsCheck.ButtonPressed = false;
        }

        if (_useTravellerWorldProfileCheck != null)
        {
            _useTravellerWorldProfileCheck.ButtonPressed = false;
        }

        if (_lifePermissivenessInput != null)
        {
            _lifePermissivenessInput.Value = GenerationUseCaseSettings.NeutralPermissiveness;
        }

        if (_populationPermissivenessInput != null)
        {
            _populationPermissivenessInput.Value = GenerationUseCaseSettings.NeutralPermissiveness;
        }

        ResetEnhancedOptionalInputs();
        RebuildEnhancedPresetOptions();
        RefreshEnhancedFieldPresentation();
        RefreshEnhancedParameterVisibility();
        UpdatePermissivenessValueLabels();
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

            ApplyTravellerDefaultsToPermissivenessControls();
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

            lines.Add($"Ruleset {request.UseCaseSettings.RulesetMode}");
            lines.Add($"Traveller Readouts {(request.UseCaseSettings.ShowTravellerReadouts ? "On" : "Off")}");
            lines.Add($"Life Potential {PermissivenessScaleHelper.GetBandLabel(request.UseCaseSettings.LifePermissiveness)}");
            lines.Add($"Settlement Density {PermissivenessScaleHelper.GetBandLabel(request.UseCaseSettings.PopulationPermissiveness)}");

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
            _assumptionsLabel.Text = string.Empty;
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

        if (_lifePermissivenessInput != null)
        {
            settings.LifePermissiveness = _lifePermissivenessInput.Value;
        }

        if (_populationPermissivenessInput != null)
        {
            settings.PopulationPermissiveness = _populationPermissivenessInput.Value;
        }

        if (settings.IsTravellerMode())
        {
            settings.ShowTravellerReadouts = true;
        }

        return settings;
    }

    private void RefreshEnhancedParameterVisibility()
    {
        ObjectViewer.ObjectType objectType = GetSelectedObjectType();
        bool showAdvanced = _showAdvancedControlsCheck != null && _showAdvancedControlsCheck.ButtonPressed;
        bool travellerMode = _rulesetModeOption != null
            && _rulesetModeOption.GetSelectedId() == (int)GenerationUseCaseSettings.RulesetModeType.Traveller;
        bool useTravellerProfile = travellerMode
            && _useTravellerWorldProfileCheck != null
            && _useTravellerWorldProfileCheck.ButtonPressed;

        SetEnhancedRowVisible("SeedRow", _showSeedControls);
        SetEnhancedSectionVisible(_planetSection, objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedSectionVisible(_travellerSection, objectType == ObjectViewer.ObjectType.Planet && travellerMode);
        SetEnhancedSectionVisible(_moonSection, objectType == ObjectViewer.ObjectType.Moon);
        SetEnhancedSectionVisible(_starSection, objectType == ObjectViewer.ObjectType.Star);
        SetEnhancedSectionVisible(_asteroidSection, objectType == ObjectViewer.ObjectType.Asteroid);
        SetEnhancedSectionVisible(_advancedSection, showAdvanced);

        SetEnhancedRowVisible("PlanetSizeCategoryRow", !useTravellerProfile && objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("PlanetAtmosphereRow", !useTravellerProfile && objectType == ObjectViewer.ObjectType.Planet);
        SetEnhancedRowVisible("StarSubclassRow", objectType == ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("StarMetallicityRow", objectType == ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("StarAgeGyrRow", objectType == ObjectViewer.ObjectType.Star);

        SetEnhancedRowVisible("MassOverrideRow", showAdvanced);
        SetEnhancedRowVisible("RadiusOverrideRow", showAdvanced);
        SetEnhancedRowVisible("RotationOverrideRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("AxialTiltOverrideRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("SemiMajorAxisOverrideRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("EccentricityOverrideRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("InclinationOverrideRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("SurfacePressureOverrideRow", showAdvanced && (objectType == ObjectViewer.ObjectType.Planet || objectType == ObjectViewer.ObjectType.Moon));
        SetEnhancedRowVisible("AlbedoOverrideRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("VolcanismOverrideRow", showAdvanced && (objectType == ObjectViewer.ObjectType.Planet || objectType == ObjectViewer.ObjectType.Moon));
        SetEnhancedRowVisible("TemperatureOverrideRow", showAdvanced && objectType == ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("LuminosityOverrideRow", showAdvanced && objectType == ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("LifePermissivenessRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
        SetEnhancedRowVisible("PopulationPermissivenessRow", showAdvanced && objectType != ObjectViewer.ObjectType.Star);
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

        if (_showAdvancedControlsCheck != null && _showAdvancedControlsCheck.ButtonPressed)
        {
            AddEnhancedIssueLabel("Advanced overrides match the editor override keys.");
        }
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

    private void OnLifePermissivenessChanged(double _value)
    {
        UpdatePermissivenessValueLabels();
        RefreshSummary();
    }

    private void OnPopulationPermissivenessChanged(double _value)
    {
        UpdatePermissivenessValueLabels();
        RefreshSummary();
    }

    private void UpdatePermissivenessValueLabels()
    {
        if (_lifePermissivenessInput != null && _lifePermissivenessValueLabel != null)
        {
            _lifePermissivenessValueLabel.Text =
                $"{_lifePermissivenessInput.Value:0.00} {PermissivenessScaleHelper.GetBandLabel(_lifePermissivenessInput.Value)}";
        }

        if (_populationPermissivenessInput != null && _populationPermissivenessValueLabel != null)
        {
            _populationPermissivenessValueLabel.Text =
                $"{_populationPermissivenessInput.Value:0.00} {PermissivenessScaleHelper.GetBandLabel(_populationPermissivenessInput.Value)}";
        }
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

    private void BuildPlanetSection()
    {
        _planetSection = AddEnhancedSection("PlanetSection", "Planet Profile");
        _planetSizeCategoryOption = AddEnhancedSectionOptionRow(_planetSection, "PlanetSizeCategoryRow", "Size Category");
        PopulateAutoSizeOptions(_planetSizeCategoryOption);
        _planetOrbitZoneOption = AddEnhancedSectionOptionRow(_planetSection, "PlanetOrbitZoneRow", "Orbit Zone");
        PopulateAutoOrbitZoneOptions(_planetOrbitZoneOption);
        _planetAtmosphereOption = AddEnhancedSectionOptionRow(_planetSection, "PlanetAtmosphereRow", "Atmosphere");
        PopulateAutoBoolOptions(_planetAtmosphereOption);
        ApplyTriStateTooltip("PlanetAtmosphereRow", _planetAtmosphereOption, "atmosphere");
        _planetRingsOption = AddEnhancedSectionOptionRow(_planetSection, "PlanetRingsRow", "Rings");
        PopulateAutoBoolOptions(_planetRingsOption);
        ApplyTriStateTooltip("PlanetRingsRow", _planetRingsOption, "rings");
        _planetRingComplexityOption = AddEnhancedSectionOptionRow(_planetSection, "PlanetRingComplexityRow", "Ring Complexity");
        PopulateAutoRingComplexityOptions(_planetRingComplexityOption);
    }

    private void BuildTravellerSection()
    {
        _travellerSection = AddEnhancedSection("TravellerSection", "Traveller World Profile");
        _useTravellerWorldProfileCheck = AddEnhancedSectionCheckRow(_travellerSection, "UseTravellerWorldProfileRow", "Traveller Worldgen");
        _travellerSizeCodeOption = AddEnhancedSectionOptionRow(_travellerSection, "TravellerSizeCodeRow", "Size Code");
        PopulateTravellerCodeOptions(_travellerSizeCodeOption, "size");
        _travellerAtmosphereCodeOption = AddEnhancedSectionOptionRow(_travellerSection, "TravellerAtmosphereCodeRow", "Atmosphere");
        PopulateTravellerCodeOptions(_travellerAtmosphereCodeOption, "atmosphere");
        _travellerHydrographicsCodeOption = AddEnhancedSectionOptionRow(_travellerSection, "TravellerHydrographicsCodeRow", "Hydrographics");
        PopulateTravellerCodeOptions(_travellerHydrographicsCodeOption, "hydrographics");
        _travellerPopulationCodeOption = AddEnhancedSectionOptionRow(_travellerSection, "TravellerPopulationCodeRow", "Population");
        PopulateTravellerCodeOptions(_travellerPopulationCodeOption, "population");
    }

    private void BuildMoonSection()
    {
        _moonSection = AddEnhancedSection("MoonSection", "Moon Profile");
        _moonSizeCategoryOption = AddEnhancedSectionOptionRow(_moonSection, "MoonSizeCategoryRow", "Size Category");
        PopulateAutoSizeOptions(_moonSizeCategoryOption);
        _moonCapturedCheck = AddEnhancedSectionCheckRow(_moonSection, "MoonCapturedRow", "Captured Moon");
        _moonAtmosphereOption = AddEnhancedSectionOptionRow(_moonSection, "MoonAtmosphereRow", "Atmosphere");
        PopulateAutoBoolOptions(_moonAtmosphereOption);
        ApplyTriStateTooltip("MoonAtmosphereRow", _moonAtmosphereOption, "atmosphere");
        _moonOceanOption = AddEnhancedSectionOptionRow(_moonSection, "MoonOceanRow", "Subsurface Ocean");
        PopulateAutoBoolOptions(_moonOceanOption);
        ApplyTriStateTooltip("MoonOceanRow", _moonOceanOption, "subsurface ocean");
    }

    private void BuildStarSection()
    {
        _starSection = AddEnhancedSection("StarSection", "Star Profile");
        _starSpectralClassOption = AddEnhancedSectionOptionRow(_starSection, "StarSpectralClassRow", "Spectral Class");
        PopulateStarClassOptions(_starSpectralClassOption);
        AddEnhancedOptionalSpinRow(_starSection, "StarSubclass", "Subclass", 0.0, 9.0, 1.0);
        AddEnhancedOptionalSpinRow(_starSection, "StarMetallicity", "Metallicity", 0.01, 3.0, 0.01);
        AddEnhancedOptionalSpinRow(_starSection, "StarAgeGyr", "Age (Gyr)", 0.001, 15.0, 0.01);
    }

    private void BuildAsteroidSection()
    {
        _asteroidSection = AddEnhancedSection("AsteroidSection", "Asteroid Profile");
        _asteroidTypeOption = AddEnhancedSectionOptionRow(_asteroidSection, "AsteroidTypeRow", "Asteroid Type");
        PopulateAsteroidTypeOptions(_asteroidTypeOption);
        _asteroidLargeCheck = AddEnhancedSectionCheckRow(_asteroidSection, "AsteroidLargeRow", "Large Body");
    }

    private void BuildAdvancedSection()
    {
        _advancedSection = AddEnhancedSection("AdvancedSection", "Advanced Overrides");
        Label legendLabel = new Label();
        legendLabel.Text = PermissivenessScaleHelper.GetLegendText();
        legendLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        legendLabel.CustomMinimumSize = new Vector2(220.0f, 0.0f);
        legendLabel.AddThemeFontSizeOverride("font_size", 10);
        legendLabel.Modulate = new Color(0.62f, 0.7f, 0.8f, 1.0f);
        _advancedSection.AddChild(legendLabel);
        _lifePermissivenessInput = AddEnhancedPermissivenessRow(_advancedSection, "LifePermissivenessRow", "Life Potential", "life");
        _populationPermissivenessInput = AddEnhancedPermissivenessRow(_advancedSection, "PopulationPermissivenessRow", "Settlement Density", "settlement");
        AddEnhancedOptionalSpinRow(_advancedSection, "MassOverride", "Mass", 0.00001, 5000.0, 0.0001);
        AddEnhancedOptionalSpinRow(_advancedSection, "RadiusOverride", "Radius", 0.001, 500.0, 0.001);
        AddEnhancedOptionalSpinRow(_advancedSection, "RotationOverride", "Rotation Period (hrs)", 0.1, 10000.0, 0.1);
        AddEnhancedOptionalSpinRow(_advancedSection, "AxialTiltOverride", "Axial Tilt (deg)", 0.0, 180.0, 0.1);
        AddEnhancedOptionalSpinRow(_advancedSection, "SemiMajorAxisOverride", "Semi-major Axis (AU)", 0.001, 1000.0, 0.001);
        AddEnhancedOptionalSpinRow(_advancedSection, "EccentricityOverride", "Eccentricity", 0.0, 0.99, 0.001);
        AddEnhancedOptionalSpinRow(_advancedSection, "InclinationOverride", "Inclination (deg)", 0.0, 180.0, 0.1);
        AddEnhancedOptionalSpinRow(_advancedSection, "SurfacePressureOverride", "Surface Pressure (atm)", 0.0, 20.0, 0.001);
        AddEnhancedOptionalSpinRow(_advancedSection, "AlbedoOverride", "Albedo", 0.0, 1.0, 0.001);
        AddEnhancedOptionalSpinRow(_advancedSection, "VolcanismOverride", "Volcanism", 0.0, 1.0, 0.01);
        AddEnhancedOptionalSpinRow(_advancedSection, "TemperatureOverride", "Temperature (K)", 100.0, 50000.0, 1.0);
        AddEnhancedOptionalSpinRow(_advancedSection, "LuminosityOverride", "Luminosity (Solar)", 0.0001, 1000000.0, 0.0001);
    }

    private HSlider AddEnhancedPermissivenessRow(VBoxContainer section, string rowName, string labelText, string subject)
    {
        HBoxContainer row = CreateEnhancedRow(rowName, labelText);
        if (_rowLabels.TryGetValue(rowName, out Label? label))
        {
            label.CustomMinimumSize = new Vector2(112.0f, 0.0f);
            label.TooltipText = PermissivenessScaleHelper.GetTooltipText(subject);
        }

        HSlider slider = new HSlider();
        slider.MinValue = 0.0;
        slider.MaxValue = 1.0;
        slider.Step = 0.05;
        slider.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        if (_rowLabels.TryGetValue(rowName, out Label? sliderLabel))
        {
            slider.TooltipText = sliderLabel.TooltipText;
        }

        row.AddChild(slider);
        Label valueLabel = new Label();
        valueLabel.CustomMinimumSize = new Vector2(156.0f, 0.0f);
        valueLabel.HorizontalAlignment = HorizontalAlignment.Right;
        valueLabel.TooltipText = slider.TooltipText;
        row.AddChild(valueLabel);

        if (rowName == "LifePermissivenessRow")
        {
            _lifePermissivenessValueLabel = valueLabel;
        }
        else
        {
            _populationPermissivenessValueLabel = valueLabel;
        }

        section.AddChild(row);
        return slider;
    }

    private string BuildEnhancedAssumptionText()
    {
        if (ShouldUseTravellerWorldGeneration())
        {
            return "Traveller world profile is generated before launch.";
        }

        if (_showAdvancedControlsCheck != null && _showAdvancedControlsCheck.ButtonPressed)
        {
            return "Advanced mode exposes the same override surface used by the editor.";
        }

        return "Preset and ruleset choices persist into the generated body.";
    }

    private string BuildEnhancedAssumptionTooltip()
    {
        if (ShouldUseTravellerWorldGeneration())
        {
            return "Traveller mode builds a world profile first, then maps that profile into the body generator while keeping the result deterministic.";
        }

        if (_showAdvancedControlsCheck != null && _showAdvancedControlsCheck.ButtonPressed)
        {
            return "Advanced controls use the same override keys as the object editor, so creation and later editing stay aligned.";
        }

        return "Preset assumptions and use-case settings are persisted into the generated body so downstream inspection and save/load flows stay aligned.";
    }

    private void AddEnhancedIssueLabel(string text)
    {
        if (_issuesContainer == null || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        Label noteLabel = new Label();
        noteLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        noteLabel.CustomMinimumSize = new Vector2(220.0f, 0.0f);
        noteLabel.Modulate = new Color(0.85f, 0.7f, 0.3f, 1.0f);
        noteLabel.Text = text;
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

    private HBoxContainer CreateEnhancedRow(string rowName, string labelText)
    {
        HBoxContainer row = new HBoxContainer();
        row.Name = rowName;
        row.AddThemeConstantOverride("separation", 10);
        Label label = new Label();
        label.Name = $"{rowName}Label";
        label.Text = labelText;
        label.CustomMinimumSize = new Vector2(112.0f, 0.0f);
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        row.AddChild(label);
        _rows[rowName] = row;
        _rowLabels[rowName] = label;
        return row;
    }

    private OptionButton AddEnhancedOptionRow(string rowName, string labelText)
    {
        HBoxContainer row = CreateEnhancedRow(rowName, labelText);
        OptionButton optionButton = new OptionButton();
        optionButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(optionButton);
        _parameterVBox!.AddChild(row);
        return optionButton;
    }

    private SpinBox AddEnhancedSpinRow(string rowName, string labelText, double minValue, double maxValue, double step)
    {
        HBoxContainer row = CreateEnhancedRow(rowName, labelText);
        SpinBox spinBox = new SpinBox();
        spinBox.MinValue = minValue;
        spinBox.MaxValue = maxValue;
        spinBox.Step = step;
        spinBox.Rounded = step >= 1.0;
        spinBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(spinBox);
        _parameterVBox!.AddChild(row);
        return spinBox;
    }

    private LineEdit AddEnhancedLineEditRow(string rowName, string labelText)
    {
        HBoxContainer row = CreateEnhancedRow(rowName, labelText);
        LineEdit input = new LineEdit();
        input.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(input);
        _parameterVBox!.AddChild(row);
        return input;
    }

    private CheckBox AddEnhancedCheckRow(string rowName, string labelText)
    {
        HBoxContainer row = CreateEnhancedRow(rowName, labelText);
        CheckBox checkBox = new CheckBox();
        row.AddChild(checkBox);
        _parameterVBox!.AddChild(row);
        return checkBox;
    }

    private VBoxContainer AddEnhancedSection(string name, string title)
    {
        VBoxContainer section = new VBoxContainer();
        section.Name = name;
        section.AddThemeConstantOverride("separation", 8);
        Label label = new Label();
        label.Text = title;
        label.AddThemeFontSizeOverride("font_size", 16);
        section.AddChild(label);
        section.AddChild(new HSeparator());
        _parameterVBox!.AddChild(section);
        return section;
    }

    private OptionButton AddEnhancedSectionOptionRow(VBoxContainer section, string rowName, string labelText)
    {
        HBoxContainer row = CreateEnhancedRow(rowName, labelText);
        OptionButton optionButton = new OptionButton();
        optionButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(optionButton);
        section.AddChild(row);
        return optionButton;
    }

    private SpinBox AddEnhancedSectionSpinRow(VBoxContainer section, string rowName, string labelText, double minValue, double maxValue, double step)
    {
        HBoxContainer row = CreateEnhancedRow(rowName, labelText);
        SpinBox spinBox = new SpinBox();
        spinBox.MinValue = minValue;
        spinBox.MaxValue = maxValue;
        spinBox.Step = step;
        spinBox.Rounded = step >= 1.0;
        spinBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(spinBox);
        section.AddChild(row);
        return spinBox;
    }

    private CheckBox AddEnhancedSectionCheckRow(VBoxContainer section, string rowName, string labelText)
    {
        HBoxContainer row = CreateEnhancedRow(rowName, labelText);
        CheckBox checkBox = new CheckBox();
        row.AddChild(checkBox);
        section.AddChild(row);
        return checkBox;
    }

    private void AddEnhancedOptionalSpinRow(VBoxContainer section, string key, string labelText, double minValue, double maxValue, double step)
    {
        string rowName = $"{key}Row";
        HBoxContainer row = CreateEnhancedRow(rowName, labelText);
        CheckBox toggle = new CheckBox();
        toggle.Name = $"{key}Toggle";
        toggle.Text = "Set";
        row.AddChild(toggle);
        SpinBox input = new SpinBox();
        input.Name = $"{key}Input";
        input.MinValue = minValue;
        input.MaxValue = maxValue;
        input.Step = step;
        input.Rounded = step >= 1.0;
        input.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        input.Editable = false;
        row.AddChild(input);
        _optionalToggles[key] = toggle;
        _optionalInputs[key] = input;
        section.AddChild(row);
    }

    private void PopulateAutoSizeOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

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

        optionButton.AddItem("Auto", -1);
        foreach (OrbitZone.Zone zone in Enum.GetValues<OrbitZone.Zone>())
        {
            optionButton.AddItem(OrbitZone.ToStringName(zone), (int)zone);
        }
    }

    private void PopulateAutoRingComplexityOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

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

        optionButton.AddItem("Auto", -1);
        optionButton.AddItem("Yes", 1);
        optionButton.AddItem("No", 0);
    }

    private void ApplyTriStateTooltip(string rowName, OptionButton? optionButton, string subject)
    {
        if (optionButton == null)
        {
            return;
        }

        string tooltip = $"Auto lets seeded generation decide when {subject} makes sense. Yes forces it on. No leaves it out.";
        optionButton.TooltipText = tooltip;
        if (_rowLabels.TryGetValue(rowName, out Label? label))
        {
            label.TooltipText = tooltip;
        }
    }

    private void ApplyTravellerDefaultsToPermissivenessControls()
    {
        if (_lifePermissivenessInput != null
            && System.Math.Abs(_lifePermissivenessInput.Value - GenerationUseCaseSettings.NeutralPermissiveness) < 0.001)
        {
            _lifePermissivenessInput.Value = GenerationUseCaseSettings.TravellerLifePermissiveness;
        }

        if (_populationPermissivenessInput != null
            && System.Math.Abs(_populationPermissivenessInput.Value - GenerationUseCaseSettings.NeutralPermissiveness) < 0.001)
        {
            _populationPermissivenessInput.Value = GenerationUseCaseSettings.TravellerPopulationPermissiveness;
        }
    }

    private void PopulateTravellerCodeOptions(OptionButton? optionButton, string kind)
    {
        if (optionButton == null)
        {
            return;
        }

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
}
