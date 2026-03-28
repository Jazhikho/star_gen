using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.App.Components;
using StarGen.App.Shared;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Parameters;
using StarGen.Domain.Systems;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Parameter-editor setup and spec normalization helpers for SystemViewer.
/// </summary>
public partial class SystemViewer
{
    private void BuildParameterEditorUi()
    {
        if (_generationSection == null)
        {
            return;
        }

        _starCountMaxSpin = GetNodeOrNull<SpinBox>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/StarCountMaxContainer/StarCountMaxSpin")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing StarCountMaxSpin.");
        _spectralHintsInput = GetNodeOrNull<LineEdit>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SpectralHintsContainer/SpectralHintsInput")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing SpectralHintsInput.");
        _systemAgeInput = GetNodeOrNull<SpinBox>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SystemAgeContainer/SystemAgeInput")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing SystemAgeInput.");
        _systemMetallicityInput = GetNodeOrNull<SpinBox>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SystemMetallicityContainer/SystemMetallicityInput")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing SystemMetallicityInput.");
        _includeBeltsCheck = GetNodeOrNull<CheckBox>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/IncludeBeltsCheck")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing IncludeBeltsCheck.");
        _generatePopulationCheck = GetNodeOrNull<CheckBox>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/GeneratePopulationCheck")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing GeneratePopulationCheck.");
        _rulesetModeOption = GetNodeOrNull<OptionButton>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/RulesetModeContainer/RulesetModeOption")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing RulesetModeOption.");
        _showTravellerReadoutsCheck = GetNodeOrNull<CheckBox>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/ShowTravellerReadoutsCheck")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing ShowTravellerReadoutsCheck.");
        _lifePermissivenessInput = GetNodeOrNull<HSlider>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/LifePermissivenessContainer/LifePermissivenessInput")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing LifePermissivenessInput.");
        _lifePermissivenessValueLabel = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/LifePermissivenessContainer/LifePermissivenessValueLabel")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing LifePermissivenessValueLabel.");
        _populationPermissivenessInput = GetNodeOrNull<HSlider>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/PopulationPermissivenessContainer/PopulationPermissivenessInput");
        _populationPermissivenessValueLabel = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/PopulationPermissivenessContainer/PopulationPermissivenessValueLabel");
        _mainworldPolicyOption = GetNodeOrNull<OptionButton>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/MainworldPolicyContainer/MainworldPolicyOption")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing MainworldPolicyOption.");
        _generationAssumptionsLabel = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/GenerationAssumptionsLabel")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing GenerationAssumptionsLabel.");
        _generationIssuesContainer = GetNodeOrNull<VBoxContainer>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/GenerationIssuesContainer")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing GenerationIssuesContainer.");

        Label starCountMaxLabel = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/StarCountMaxContainer/StarCountMaxLabel")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing StarCountMaxLabel.");
        Label spectralHintsLabel = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SpectralHintsContainer/SpectralHintsLabel")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing SpectralHintsLabel.");
        Label systemAgeLabel = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SystemAgeContainer/SystemAgeLabel")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing SystemAgeLabel.");
        Label metallicityLabel = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SystemMetallicityContainer/SystemMetallicityLabel")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing SystemMetallicityLabel.");
        Label rulesetModeLabel = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/RulesetModeContainer/RulesetModeLabel")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing RulesetModeLabel.");
        Label lifePermissivenessLabel = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/LifePermissivenessContainer/LifePermissivenessLabel")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing LifePermissivenessLabel.");
        Label? populationPermissivenessLabel = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/PopulationPermissivenessContainer/PopulationPermissivenessLabel");
        Label mainworldPolicyLabel = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/MainworldPolicyContainer/MainworldPolicyLabel")
            ?? throw new System.InvalidOperationException("SystemViewer scene is missing MainworldPolicyLabel.");
        string starCountMaxAssumption = GetSystemAssumption("star_count_max");
        string spectralHintsAssumption = GetSystemAssumption("spectral_class_hints");
        string systemAgeAssumption = GetSystemAssumption("system_age_years");
        string systemMetallicityAssumption = GetSystemAssumption("system_metallicity");
        string includeBeltsAssumption = GetSystemAssumption("include_asteroid_belts");
        string generatePopulationAssumption = GetSystemAssumption("generate_population");
        string rulesetModeAssumption = GetSystemAssumption("ruleset_mode");
        string showTravellerAssumption = GetSystemAssumption("show_traveller_readouts");
        string lifeAssumption = PermissivenessScaleHelper.GetTooltipText("life");
        string mainworldAssumption = GetSystemAssumption("mainworld_policy");

        starCountMaxLabel.TooltipText = starCountMaxAssumption;
        _starCountMaxSpin.TooltipText = starCountMaxAssumption;
        spectralHintsLabel.TooltipText = spectralHintsAssumption;
        _spectralHintsInput.TooltipText = spectralHintsAssumption;
        systemAgeLabel.TooltipText = systemAgeAssumption;
        _systemAgeInput.TooltipText = systemAgeAssumption;
        metallicityLabel.TooltipText = systemMetallicityAssumption;
        _systemMetallicityInput.TooltipText = systemMetallicityAssumption;
        _includeBeltsCheck.TooltipText = includeBeltsAssumption;
        _generatePopulationCheck.TooltipText = generatePopulationAssumption;
        rulesetModeLabel.TooltipText = rulesetModeAssumption;
        _rulesetModeOption.TooltipText = rulesetModeAssumption;
        _showTravellerReadoutsCheck.TooltipText = showTravellerAssumption;
        lifePermissivenessLabel.TooltipText = lifeAssumption;
        _lifePermissivenessInput.TooltipText = lifeAssumption;
        _lifePermissivenessValueLabel.TooltipText = lifeAssumption;
        if (populationPermissivenessLabel != null)
        {
            populationPermissivenessLabel.GetParent<Control>()?.Hide();
        }
        mainworldPolicyLabel.TooltipText = mainworldAssumption;
        _mainworldPolicyOption.TooltipText = mainworldAssumption;
        if (_rulesetModeOption.ItemCount == 0)
        {
            _rulesetModeOption.AddItem(GenerationUseCasePresentation.RealisticRulesetLabel, (int)GenerationUseCaseSettings.RulesetModeType.Default);
            _rulesetModeOption.AddItem("Traveller", (int)GenerationUseCaseSettings.RulesetModeType.Traveller);
        }

        if (_mainworldPolicyOption.ItemCount == 0)
        {
            _mainworldPolicyOption.AddItem("None", (int)GenerationUseCaseSettings.MainworldPolicyType.None);
            _mainworldPolicyOption.AddItem("Prefer", (int)GenerationUseCaseSettings.MainworldPolicyType.Prefer);
            _mainworldPolicyOption.AddItem("Require", (int)GenerationUseCaseSettings.MainworldPolicyType.Require);
        }
    }

    private string GetSystemAssumption(string parameterId)
    {
        List<GenerationParameterDefinition> definitions = GenerationParameterCatalog.GetSystemDefinitions();
        foreach (GenerationParameterDefinition definition in definitions)
        {
            if (definition.Id == parameterId)
            {
                return definition.AssumptionText;
            }
        }

        return string.Empty;
    }

    private SolarSystemSpec BuildCurrentSpecFromControls()
    {
        int seedValue = 1;
        if (_seedInput != null)
        {
            seedValue = (int)_seedInput.Value;
        }

        int minStars = 1;
        if (_starCountSpin != null)
        {
            minStars = (int)_starCountSpin.Value;
        }

        int maxStars = minStars;
        if (_starCountMaxSpin != null)
        {
            maxStars = (int)_starCountMaxSpin.Value;
        }

        SolarSystemSpec spec = new SolarSystemSpec(seedValue, minStars, maxStars);
        if (_currentSpec != null)
        {
            spec.GalaxyContext = _currentSpec.GalaxyContext.Clone();
            spec.StellarProfile = _currentSpec.StellarProfile.Clone();
        }

        if (_spectralHintsInput != null)
        {
            spec.SpectralClassHints = ParseSpectralHints(_spectralHintsInput.Text);
        }

        if (_systemAgeInput != null)
        {
            if (_systemAgeInput.Value >= 0.0)
            {
                spec.SystemAgeYears = _systemAgeInput.Value * 1.0e9;
            }
            else
            {
                spec.SystemAgeYears = -1.0;
            }
        }

        if (_systemMetallicityInput != null)
        {
            if (_systemMetallicityInput.Value >= 0.0)
            {
                spec.SystemMetallicity = _systemMetallicityInput.Value;
            }
            else
            {
                spec.SystemMetallicity = -1.0;
            }
        }

        if (_includeBeltsCheck != null)
        {
            spec.IncludeAsteroidBelts = _includeBeltsCheck.ButtonPressed;
        }

        if (_generatePopulationCheck != null)
        {
            spec.GeneratePopulation = _generatePopulationCheck.ButtonPressed;
        }

        spec.UseCaseSettings = BuildUseCaseSettingsFromControls();

        return spec;
    }

    private void ApplySpecToControls(SolarSystemSpec spec)
    {
        _currentSpec = spec;
        if (_seedInput != null)
        {
            _seedInput.Value = spec.GenerationSeed;
        }

        if (_starCountSpin != null)
        {
            _starCountSpin.Value = spec.StarCountMin;
        }

        if (_starCountMaxSpin != null)
        {
            _starCountMaxSpin.Value = spec.StarCountMax;
        }

        if (_spectralHintsInput != null)
        {
            _spectralHintsInput.Text = FormatSpectralHints(spec.SpectralClassHints);
        }

        if (_systemAgeInput != null)
        {
            if (spec.SystemAgeYears >= 0.0)
            {
                _systemAgeInput.Value = spec.SystemAgeYears / 1.0e9;
            }
            else
            {
                _systemAgeInput.Value = -1.0;
            }
        }

        if (_systemMetallicityInput != null)
        {
            if (spec.SystemMetallicity >= 0.0)
            {
                _systemMetallicityInput.Value = spec.SystemMetallicity;
            }
            else
            {
                _systemMetallicityInput.Value = -1.0;
            }
        }

        if (_includeBeltsCheck != null)
        {
            _includeBeltsCheck.ButtonPressed = spec.IncludeAsteroidBelts;
        }

        if (_generatePopulationCheck != null)
        {
            _generatePopulationCheck.ButtonPressed = spec.GeneratePopulation;
        }

        ApplyUseCaseSettingsToControls(spec.UseCaseSettings);

        _currentGenerationIssues = SystemGenerationParameterValidator.Validate(spec);
        UpdateGenerationIssuesUi();
    }

    private SolarSystemSpec? ExtractCurrentSpec(SolarSystem? system)
    {
        if (system != null && system.Provenance != null && system.Provenance.SpecSnapshot.Count > 0)
        {
            return SolarSystemSpec.FromDictionary(system.Provenance.SpecSnapshot);
        }

        return _currentSpec;
    }

    private void UpdateGenerationIssuesUi()
    {
        if (_generationIssuesContainer == null)
        {
            return;
        }

        foreach (Node child in _generationIssuesContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (_currentGenerationIssues.Issues.Count == 0)
        {
            Label cleanLabel = UiSceneTemplates.InstantiateMessageLabel();
            cleanLabel.Text = "No parameter issues.";
            cleanLabel.Modulate = new Color(0.55f, 0.75f, 0.55f, 1.0f);
            _generationIssuesContainer.AddChild(cleanLabel);
            return;
        }

        foreach (GenerationParameterIssue issue in _currentGenerationIssues.Issues)
        {
            Label issueLabel = UiSceneTemplates.InstantiateMessageLabel();
            string prefix = "Warning";
            issueLabel.Modulate = new Color(0.85f, 0.7f, 0.3f, 1.0f);
            if (issue.Severity == GenerationParameterIssue.IssueSeverity.Error)
            {
                prefix = "Error";
                issueLabel.Modulate = new Color(1.0f, 0.45f, 0.45f, 1.0f);
            }

            issueLabel.Text = $"{prefix}: {issue.Message}";
            _generationIssuesContainer.AddChild(issueLabel);
        }
    }

    private GenerationUseCaseSettings BuildUseCaseSettingsFromControls()
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

        if (_mainworldPolicyOption != null)
        {
            settings.MainworldPolicy = (GenerationUseCaseSettings.MainworldPolicyType)_mainworldPolicyOption.GetSelectedId();
        }

        return settings;
    }

    private void ApplyUseCaseSettingsToControls(GenerationUseCaseSettings settings)
    {
        if (_rulesetModeOption != null)
        {
            _rulesetModeOption.Select((int)settings.RulesetMode);
        }

        if (_showTravellerReadoutsCheck != null)
        {
            _showTravellerReadoutsCheck.ButtonPressed = settings.ShowTravellerReadouts;
        }

        if (_lifePermissivenessInput != null)
        {
            _lifePermissivenessInput.Value = settings.LifePermissiveness;
        }

        if (_mainworldPolicyOption != null)
        {
            _mainworldPolicyOption.Select((int)settings.MainworldPolicy);
        }

        UpdatePermissivenessValueLabels();
    }

    private void ApplyTravellerDefaultsToControls()
    {
        if (_showTravellerReadoutsCheck != null)
        {
            _showTravellerReadoutsCheck.ButtonPressed = true;
        }

        if (_mainworldPolicyOption != null)
        {
            _mainworldPolicyOption.Select((int)GenerationUseCaseSettings.MainworldPolicyType.Require);
        }

        if (_generatePopulationCheck != null)
        {
            _generatePopulationCheck.ButtonPressed = true;
        }

        if (_lifePermissivenessInput != null)
        {
            if (System.Math.Abs(_lifePermissivenessInput.Value - GenerationUseCaseSettings.NeutralPermissiveness) < 0.001)
            {
                _lifePermissivenessInput.Value = GenerationUseCaseSettings.TravellerLifePermissiveness;
            }
        }

    }

    private void RefreshGenerationValidationFromControls()
    {
        SolarSystemSpec spec = BuildCurrentSpecFromControls();
        _currentGenerationIssues = SystemGenerationParameterValidator.Validate(spec);
        UpdatePermissivenessValueLabels();
        UpdateGenerationIssuesUi();
    }

    private void UpdatePermissivenessValueLabels()
    {
        if (_lifePermissivenessInput != null && _lifePermissivenessValueLabel != null)
        {
            _lifePermissivenessValueLabel.Text =
                $"{_lifePermissivenessInput.Value:0.00} {PermissivenessScaleHelper.GetBandLabel(_lifePermissivenessInput.Value)}";
        }

    }

    private static Array<int> ParseSpectralHints(string text)
    {
        Array<int> result = new Array<int>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return result;
        }

        string[] parts = text.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
        foreach (string rawPart in parts)
        {
            string part = rawPart.ToUpperInvariant();
            if (part == "O")
            {
                result.Add((int)StarClass.SpectralClass.O);
                continue;
            }

            if (part == "B")
            {
                result.Add((int)StarClass.SpectralClass.B);
                continue;
            }

            if (part == "A")
            {
                result.Add((int)StarClass.SpectralClass.A);
                continue;
            }

            if (part == "F")
            {
                result.Add((int)StarClass.SpectralClass.F);
                continue;
            }

            if (part == "G")
            {
                result.Add((int)StarClass.SpectralClass.G);
                continue;
            }

            if (part == "K")
            {
                result.Add((int)StarClass.SpectralClass.K);
                continue;
            }

            if (part == "M")
            {
                result.Add((int)StarClass.SpectralClass.M);
            }
        }

        return result;
    }

    private static string FormatSpectralHints(Array<int> hints)
    {
        List<string> parts = new List<string>();
        foreach (int hint in hints)
        {
            if (System.Enum.IsDefined(typeof(StarClass.SpectralClass), hint))
            {
                parts.Add(((StarClass.SpectralClass)hint).ToString());
            }
        }

        return string.Join(",", parts);
    }
}
