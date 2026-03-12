using System.Collections.Generic;
using Godot;
using Godot.Collections;
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
        if (_generationSection == null || _spectralHintsInput != null)
        {
            return;
        }

        if (_starCountLabel != null)
        {
            _starCountLabel.Text = "Min Stars:";
        }

        HBoxContainer maxStarsRow = CreateGenerationRow("Max Stars:", out Label maxStarsLabel);
        maxStarsRow.Name = "StarCountMaxContainer";
        maxStarsLabel.TooltipText = GetSystemAssumption("star_count_max");
        SpinBox maxStarsSpin = new SpinBox();
        maxStarsSpin.Name = "StarCountMaxSpin";
        maxStarsSpin.MinValue = 1.0;
        maxStarsSpin.MaxValue = 10.0;
        maxStarsSpin.Step = 1.0;
        maxStarsSpin.Rounded = true;
        maxStarsSpin.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        maxStarsSpin.TooltipText = GetSystemAssumption("star_count_max");
        maxStarsRow.AddChild(maxStarsSpin);
        _starCountMaxSpin = maxStarsSpin;
        _generationSection.AddChild(maxStarsRow);

        HBoxContainer spectralHintsRow = CreateGenerationRow("Spectral:", out Label spectralLabel);
        spectralHintsRow.Name = "SpectralHintsContainer";
        spectralLabel.TooltipText = GetSystemAssumption("spectral_class_hints");
        LineEdit spectralInput = new LineEdit();
        spectralInput.Name = "SpectralHintsInput";
        spectralInput.PlaceholderText = "G,K,M";
        spectralInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        spectralInput.TooltipText = GetSystemAssumption("spectral_class_hints");
        spectralHintsRow.AddChild(spectralInput);
        _spectralHintsInput = spectralInput;
        _generationSection.AddChild(spectralHintsRow);

        HBoxContainer ageRow = CreateGenerationRow("Age:", out Label ageLabel);
        ageRow.Name = "SystemAgeContainer";
        ageLabel.TooltipText = GetSystemAssumption("system_age_years");
        SpinBox ageInput = new SpinBox();
        ageInput.Name = "SystemAgeInput";
        ageInput.MinValue = -1.0;
        ageInput.MaxValue = 13.0;
        ageInput.Step = 0.1;
        ageInput.Suffix = " Gyr";
        ageInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        ageInput.TooltipText = GetSystemAssumption("system_age_years");
        ageRow.AddChild(ageInput);
        _systemAgeInput = ageInput;
        _generationSection.AddChild(ageRow);

        HBoxContainer metallicityRow = CreateGenerationRow("Metallicity:", out Label metallicityLabel);
        metallicityRow.Name = "SystemMetallicityContainer";
        metallicityLabel.TooltipText = GetSystemAssumption("system_metallicity");
        SpinBox metallicityInput = new SpinBox();
        metallicityInput.Name = "SystemMetallicityInput";
        metallicityInput.MinValue = -1.0;
        metallicityInput.MaxValue = 5.0;
        metallicityInput.Step = 0.05;
        metallicityInput.Suffix = " Zsun";
        metallicityInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        metallicityInput.TooltipText = GetSystemAssumption("system_metallicity");
        metallicityRow.AddChild(metallicityInput);
        _systemMetallicityInput = metallicityInput;
        _generationSection.AddChild(metallicityRow);

        CheckBox includeBeltsCheck = new CheckBox();
        includeBeltsCheck.Name = "IncludeBeltsCheck";
        includeBeltsCheck.Text = "Generate Asteroid Belts";
        includeBeltsCheck.TooltipText = GetSystemAssumption("include_asteroid_belts");
        _includeBeltsCheck = includeBeltsCheck;
        _generationSection.AddChild(includeBeltsCheck);

        CheckBox generatePopulationCheck = new CheckBox();
        generatePopulationCheck.Name = "GeneratePopulationCheck";
        generatePopulationCheck.Text = "Generate Population";
        generatePopulationCheck.TooltipText = GetSystemAssumption("generate_population");
        _generatePopulationCheck = generatePopulationCheck;
        _generationSection.AddChild(generatePopulationCheck);

        Label basicHeader = new Label();
        basicHeader.Name = "TravellerBasicHeader";
        basicHeader.Text = "Traveller / Ruleset";
        basicHeader.AddThemeFontSizeOverride("font_size", 12);
        basicHeader.Modulate = new Color(0.82f, 0.82f, 0.55f, 1.0f);
        _generationSection.AddChild(basicHeader);

        HBoxContainer rulesetRow = CreateGenerationRow("Ruleset:", out Label rulesetLabel);
        rulesetRow.Name = "RulesetModeContainer";
        rulesetLabel.TooltipText = GetSystemAssumption("ruleset_mode");
        OptionButton rulesetModeOption = new OptionButton();
        rulesetModeOption.Name = "RulesetModeOption";
        rulesetModeOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        rulesetModeOption.AddItem("Default", (int)GenerationUseCaseSettings.RulesetModeType.Default);
        rulesetModeOption.AddItem("Traveller", (int)GenerationUseCaseSettings.RulesetModeType.Traveller);
        rulesetRow.AddChild(rulesetModeOption);
        _rulesetModeOption = rulesetModeOption;
        _generationSection.AddChild(rulesetRow);

        CheckBox showTravellerReadoutsCheck = new CheckBox();
        showTravellerReadoutsCheck.Name = "ShowTravellerReadoutsCheck";
        showTravellerReadoutsCheck.Text = "Show Traveller / UWP Readouts";
        showTravellerReadoutsCheck.TooltipText = GetSystemAssumption("show_traveller_readouts");
        _showTravellerReadoutsCheck = showTravellerReadoutsCheck;
        _generationSection.AddChild(showTravellerReadoutsCheck);

        Label advancedHeader = new Label();
        advancedHeader.Name = "TravellerAdvancedHeader";
        advancedHeader.Text = "Advanced Assumptions";
        advancedHeader.AddThemeFontSizeOverride("font_size", 12);
        advancedHeader.Modulate = new Color(0.82f, 0.82f, 0.55f, 1.0f);
        _generationSection.AddChild(advancedHeader);

        HBoxContainer lifeRow = CreateGenerationRow("Life Bias:", out Label lifeLabel);
        lifeRow.Name = "LifePermissivenessContainer";
        lifeLabel.TooltipText = GetSystemAssumption("life_permissiveness");
        SpinBox lifePermissivenessInput = new SpinBox();
        lifePermissivenessInput.Name = "LifePermissivenessInput";
        lifePermissivenessInput.MinValue = 0.0;
        lifePermissivenessInput.MaxValue = 1.0;
        lifePermissivenessInput.Step = 0.05;
        lifePermissivenessInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        lifePermissivenessInput.TooltipText = GetSystemAssumption("life_permissiveness");
        lifeRow.AddChild(lifePermissivenessInput);
        _lifePermissivenessInput = lifePermissivenessInput;
        _generationSection.AddChild(lifeRow);

        HBoxContainer populationRow = CreateGenerationRow("Pop. Bias:", out Label populationLabel);
        populationRow.Name = "PopulationPermissivenessContainer";
        populationLabel.TooltipText = GetSystemAssumption("population_permissiveness");
        SpinBox populationPermissivenessInput = new SpinBox();
        populationPermissivenessInput.Name = "PopulationPermissivenessInput";
        populationPermissivenessInput.MinValue = 0.0;
        populationPermissivenessInput.MaxValue = 1.0;
        populationPermissivenessInput.Step = 0.05;
        populationPermissivenessInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        populationPermissivenessInput.TooltipText = GetSystemAssumption("population_permissiveness");
        populationRow.AddChild(populationPermissivenessInput);
        _populationPermissivenessInput = populationPermissivenessInput;
        _generationSection.AddChild(populationRow);

        HBoxContainer mainworldRow = CreateGenerationRow("Mainworld:", out Label mainworldLabel);
        mainworldRow.Name = "MainworldPolicyContainer";
        mainworldLabel.TooltipText = GetSystemAssumption("mainworld_policy");
        OptionButton mainworldPolicyOption = new OptionButton();
        mainworldPolicyOption.Name = "MainworldPolicyOption";
        mainworldPolicyOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        mainworldPolicyOption.AddItem("None", (int)GenerationUseCaseSettings.MainworldPolicyType.None);
        mainworldPolicyOption.AddItem("Prefer", (int)GenerationUseCaseSettings.MainworldPolicyType.Prefer);
        mainworldPolicyOption.AddItem("Require", (int)GenerationUseCaseSettings.MainworldPolicyType.Require);
        mainworldRow.AddChild(mainworldPolicyOption);
        _mainworldPolicyOption = mainworldPolicyOption;
        _generationSection.AddChild(mainworldRow);

        Label assumptionsLabel = new Label();
        assumptionsLabel.Name = "GenerationAssumptionsLabel";
        assumptionsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        assumptionsLabel.AddThemeFontSizeOverride("font_size", 10);
        assumptionsLabel.Modulate = new Color(0.6f, 0.7f, 0.8f, 1.0f);
        assumptionsLabel.Text = "Targets bias star generation and shared chemistry, but orbit slots and body placement remain generator-driven in 0.4.0.";
        _generationAssumptionsLabel = assumptionsLabel;
        _generationSection.AddChild(assumptionsLabel);

        VBoxContainer issuesContainer = new VBoxContainer();
        issuesContainer.Name = "GenerationIssuesContainer";
        issuesContainer.AddThemeConstantOverride("separation", 2);
        _generationIssuesContainer = issuesContainer;
        _generationSection.AddChild(issuesContainer);
    }

    private static HBoxContainer CreateGenerationRow(string labelText, out Label label)
    {
        HBoxContainer row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 8);
        label = new Label();
        label.Text = labelText;
        label.CustomMinimumSize = new Vector2(90.0f, 0.0f);
        row.AddChild(label);
        return row;
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
            Label cleanLabel = new Label();
            cleanLabel.Text = "No parameter issues.";
            cleanLabel.AddThemeFontSizeOverride("font_size", 10);
            cleanLabel.Modulate = new Color(0.55f, 0.75f, 0.55f, 1.0f);
            _generationIssuesContainer.AddChild(cleanLabel);
            return;
        }

        foreach (GenerationParameterIssue issue in _currentGenerationIssues.Issues)
        {
            Label issueLabel = new Label();
            issueLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            issueLabel.AddThemeFontSizeOverride("font_size", 10);
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

        if (_populationPermissivenessInput != null)
        {
            settings.PopulationPermissiveness = _populationPermissivenessInput.Value;
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

        if (_populationPermissivenessInput != null)
        {
            _populationPermissivenessInput.Value = settings.PopulationPermissiveness;
        }

        if (_mainworldPolicyOption != null)
        {
            _mainworldPolicyOption.Select((int)settings.MainworldPolicy);
        }
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
    }

    private void RefreshGenerationValidationFromControls()
    {
        SolarSystemSpec spec = BuildCurrentSpecFromControls();
        _currentGenerationIssues = SystemGenerationParameterValidator.Validate(spec);
        UpdateGenerationIssuesUi();
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
