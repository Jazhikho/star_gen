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

        _planetSection = GetNodeOrNull<VBoxContainer>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetSection");
        _travellerSection = GetNodeOrNull<VBoxContainer>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/TravellerSection");
        _moonSection = GetNodeOrNull<VBoxContainer>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/MoonSection");
        _starSection = GetNodeOrNull<VBoxContainer>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StarSection");
        _asteroidSection = GetNodeOrNull<VBoxContainer>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/AsteroidSection");
        _advancedSection = GetNodeOrNull<VBoxContainer>("MarginContainer/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/AdvancedSection");

        _planetSizeCategoryOption = GetRequiredOptionButton("PlanetSizeCategoryRow", "PlanetSizeCategoryOption");
        _planetOrbitZoneOption = GetRequiredOptionButton("PlanetOrbitZoneRow", "PlanetOrbitZoneOption");
        _planetAtmosphereOption = GetRequiredOptionButton("PlanetAtmosphereRow", "PlanetAtmosphereOption");
        _planetRingsOption = GetRequiredOptionButton("PlanetRingsRow", "PlanetRingsOption");
        _planetRingComplexityOption = GetRequiredOptionButton("PlanetRingComplexityRow", "PlanetRingComplexityOption");

        _useTravellerWorldProfileCheck = GetRequiredCheckBox("UseTravellerWorldProfileRow", "UseTravellerWorldProfileCheck");
        _travellerSizeCodeOption = GetRequiredOptionButton("TravellerSizeCodeRow", "TravellerSizeCodeOption");
        _travellerAtmosphereCodeOption = GetRequiredOptionButton("TravellerAtmosphereCodeRow", "TravellerAtmosphereCodeOption");
        _travellerHydrographicsCodeOption = GetRequiredOptionButton("TravellerHydrographicsCodeRow", "TravellerHydrographicsCodeOption");
        _travellerPopulationCodeOption = GetRequiredOptionButton("TravellerPopulationCodeRow", "TravellerPopulationCodeOption");

        _moonSizeCategoryOption = GetRequiredOptionButton("MoonSizeCategoryRow", "MoonSizeCategoryOption");
        _moonCapturedCheck = GetRequiredCheckBox("MoonCapturedRow", "MoonCapturedCheck");
        _moonAtmosphereOption = GetRequiredOptionButton("MoonAtmosphereRow", "MoonAtmosphereOption");
        _moonOceanOption = GetRequiredOptionButton("MoonOceanRow", "MoonOceanOption");

        _starSpectralClassOption = GetRequiredOptionButton("StarSpectralClassRow", "StarSpectralClassOption");

        _asteroidTypeOption = GetRequiredOptionButton("AsteroidTypeRow", "AsteroidTypeOption");
        _asteroidLargeCheck = GetRequiredCheckBox("AsteroidLargeRow", "AsteroidLargeCheck");

        _lifePermissivenessInput = GetRequiredSlider("LifePermissivenessRow", "LifePermissivenessInput");
        _lifePermissivenessValueLabel = GetRequiredLabel("LifePermissivenessRow", "LifePermissivenessValue");
        _populationPermissivenessInput = GetRequiredSlider("PopulationPermissivenessRow", "PopulationPermissivenessInput");
        _populationPermissivenessValueLabel = GetRequiredLabel("PopulationPermissivenessRow", "PopulationPermissivenessValue");

        CacheOptionalOverride("StarSubclass");
        CacheOptionalOverride("StarMetallicity");
        CacheOptionalOverride("StarAgeGyr");
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
        _typeOption.AddItem("Moon", (int)ObjectViewer.ObjectType.Moon);
        _typeOption.AddItem("Asteroid", (int)ObjectViewer.ObjectType.Asteroid);
        _rulesetModeOption.Clear();
        _rulesetModeOption.AddItem(GenerationUseCasePresentation.RealisticRulesetLabel, (int)GenerationUseCaseSettings.RulesetModeType.Default);
        _rulesetModeOption.AddItem("Traveller", (int)GenerationUseCaseSettings.RulesetModeType.Traveller);
        PopulatePlanetSection();
        PopulateTravellerSection();
        PopulateMoonSection();
        PopulateStarSection();
        PopulateAsteroidSection();
        PopulateAdvancedSection();
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

            lines.Add($"Ruleset {GenerationUseCasePresentation.GetRulesetLabel(request.UseCaseSettings.RulesetMode)}");
            lines.Add($"Traveller Readouts {(request.UseCaseSettings.ShowTravellerReadouts ? "On" : "Off")}");
            lines.Add($"Life Potential {PermissivenessScaleHelper.GetBandLabel(request.UseCaseSettings.LifePermissiveness)}");
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
        SetEnhancedRowVisible("PopulationPermissivenessRow", false);
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
        RefreshSummary();
    }

    private void UpdatePermissivenessValueLabels()
    {
        if (_lifePermissivenessInput != null && _lifePermissivenessValueLabel != null)
        {
            _lifePermissivenessValueLabel.Text =
                $"{_lifePermissivenessInput.Value:0.00} {PermissivenessScaleHelper.GetBandLabel(_lifePermissivenessInput.Value)}";
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

    private void PopulatePlanetSection()
    {
        PopulateAutoSizeOptions(_planetSizeCategoryOption);
        PopulateAutoOrbitZoneOptions(_planetOrbitZoneOption);
        PopulateAutoBoolOptions(_planetAtmosphereOption);
        ApplyTriStateTooltip("PlanetAtmosphereRow", _planetAtmosphereOption, "atmosphere");
        PopulateAutoBoolOptions(_planetRingsOption);
        ApplyTriStateTooltip("PlanetRingsRow", _planetRingsOption, "rings");
        PopulateAutoRingComplexityOptions(_planetRingComplexityOption);
    }

    private void PopulateTravellerSection()
    {
        PopulateTravellerCodeOptions(_travellerSizeCodeOption, "size");
        PopulateTravellerCodeOptions(_travellerAtmosphereCodeOption, "atmosphere");
        PopulateTravellerCodeOptions(_travellerHydrographicsCodeOption, "hydrographics");
        PopulateTravellerCodeOptions(_travellerPopulationCodeOption, "population");
    }

    private void PopulateMoonSection()
    {
        PopulateAutoSizeOptions(_moonSizeCategoryOption);
        PopulateAutoBoolOptions(_moonAtmosphereOption);
        ApplyTriStateTooltip("MoonAtmosphereRow", _moonAtmosphereOption, "atmosphere");
        PopulateAutoBoolOptions(_moonOceanOption);
        ApplyTriStateTooltip("MoonOceanRow", _moonOceanOption, "subsurface ocean");
    }

    private void PopulateStarSection()
    {
        PopulateStarClassOptions(_starSpectralClassOption);
    }

    private void PopulateAsteroidSection()
    {
        PopulateAsteroidTypeOptions(_asteroidTypeOption);
    }

    private void PopulateAdvancedSection()
    {
        ApplyPermissivenessTooltip("LifePermissivenessRow", _lifePermissivenessInput, _lifePermissivenessValueLabel, "life");
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
        label.CustomMinimumSize = new Vector2(112.0f, 0.0f);
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _rows[rowName] = row;
        _rowLabels[rowName] = label;
        return row;
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

    private void ApplyPermissivenessTooltip(string rowName, Control? input, Label? valueLabel, string subject)
    {
        if (_rowLabels.TryGetValue(rowName, out Label? label))
        {
            label.TooltipText = PermissivenessScaleHelper.GetTooltipText(subject);
        }

        if (input != null && _rowLabels.TryGetValue(rowName, out Label? rowLabel))
        {
            input.TooltipText = rowLabel.TooltipText;
        }

        if (valueLabel != null && input != null)
        {
            valueLabel.TooltipText = input.TooltipText;
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
}
