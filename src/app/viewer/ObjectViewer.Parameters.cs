using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;

namespace StarGen.App.Viewer;

/// <summary>
/// Shared use-case settings UI and standalone empty-state helpers for ObjectViewer.
/// </summary>
public partial class ObjectViewer
{
    private void SetupUseCaseControls()
    {
        Node? generationSectionNode = GetNodeOrNull<Node>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection");
        if (generationSectionNode is not VBoxContainer generationSection)
        {
            return;
        }

        if (_rulesetModeOption != null)
        {
            return;
        }

        HBoxContainer rulesetContainer = new HBoxContainer();
        rulesetContainer.Name = "RulesetContainer";
        Label rulesetLabel = new Label();
        rulesetLabel.Text = "Ruleset:";
        rulesetLabel.CustomMinimumSize = new Vector2(60.0f, 0.0f);
        rulesetContainer.AddChild(rulesetLabel);

        OptionButton rulesetModeOption = new OptionButton();
        rulesetModeOption.Name = "RulesetModeOption";
        rulesetModeOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        rulesetModeOption.AddItem("Default", (int)GenerationUseCaseSettings.RulesetModeType.Default);
        rulesetModeOption.AddItem("Traveller", (int)GenerationUseCaseSettings.RulesetModeType.Traveller);
        rulesetModeOption.ItemSelected += OnRulesetModeSelected;
        rulesetContainer.AddChild(rulesetModeOption);
        _rulesetModeOption = rulesetModeOption;

        CheckBox showTravellerReadoutsCheck = new CheckBox();
        showTravellerReadoutsCheck.Name = "ShowTravellerReadoutsCheck";
        showTravellerReadoutsCheck.Text = "Show Traveller / UWP Readouts";
        showTravellerReadoutsCheck.Toggled += enabled => _activeUseCaseSettings.ShowTravellerReadouts = enabled;
        _showTravellerReadoutsCheck = showTravellerReadoutsCheck;

        HBoxContainer lifeContainer = new HBoxContainer();
        lifeContainer.Name = "LifePermissivenessContainer";
        Label lifeLabel = new Label();
        lifeLabel.Text = "Life Potential:";
        lifeLabel.CustomMinimumSize = new Vector2(60.0f, 0.0f);
        lifeContainer.AddChild(lifeLabel);

        SpinBox lifePermissivenessInput = new SpinBox();
        lifePermissivenessInput.Name = "LifePermissivenessInput";
        lifePermissivenessInput.MinValue = 0.0;
        lifePermissivenessInput.MaxValue = 1.0;
        lifePermissivenessInput.Step = 0.05;
        lifePermissivenessInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        lifePermissivenessInput.ValueChanged += value => _activeUseCaseSettings.LifePermissiveness = value;
        lifeContainer.AddChild(lifePermissivenessInput);
        _lifePermissivenessInput = lifePermissivenessInput;

        HBoxContainer populationContainer = new HBoxContainer();
        populationContainer.Name = "PopulationPermissivenessContainer";
        Label populationLabel = new Label();
        populationLabel.Text = "Settlement Density:";
        populationLabel.CustomMinimumSize = new Vector2(60.0f, 0.0f);
        populationContainer.AddChild(populationLabel);

        SpinBox populationPermissivenessInput = new SpinBox();
        populationPermissivenessInput.Name = "PopulationPermissivenessInput";
        populationPermissivenessInput.MinValue = 0.0;
        populationPermissivenessInput.MaxValue = 1.0;
        populationPermissivenessInput.Step = 0.05;
        populationPermissivenessInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        populationPermissivenessInput.ValueChanged += value => _activeUseCaseSettings.PopulationPermissiveness = value;
        populationContainer.AddChild(populationPermissivenessInput);
        _populationPermissivenessInput = populationPermissivenessInput;

        Label useCaseAssumptionsLabel = new Label();
        useCaseAssumptionsLabel.Name = "UseCaseAssumptionsLabel";
        useCaseAssumptionsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        useCaseAssumptionsLabel.CustomMinimumSize = new Vector2(220.0f, 0.0f);
        useCaseAssumptionsLabel.AddThemeFontSizeOverride("font_size", 10);
        useCaseAssumptionsLabel.Modulate = new Color(0.6f, 0.7f, 0.8f, 1.0f);
        useCaseAssumptionsLabel.Text = "Ruleset and assumption settings are persisted with generated bodies so downstream system and export work can honor the same assumptions.";
        _useCaseAssumptionsLabel = useCaseAssumptionsLabel;

        int buttonIndex = generationSection.GetNode("ButtonContainer").GetIndex();
        generationSection.AddChild(rulesetContainer);
        generationSection.MoveChild(rulesetContainer, buttonIndex);
        generationSection.AddChild(showTravellerReadoutsCheck);
        generationSection.MoveChild(showTravellerReadoutsCheck, buttonIndex + 1);
        generationSection.AddChild(lifeContainer);
        generationSection.MoveChild(lifeContainer, buttonIndex + 2);
        generationSection.AddChild(populationContainer);
        generationSection.MoveChild(populationContainer, buttonIndex + 3);
        generationSection.AddChild(useCaseAssumptionsLabel);
        generationSection.MoveChild(useCaseAssumptionsLabel, buttonIndex + 4);

        ApplyUseCaseSettingsToControls(_activeUseCaseSettings);
    }

    private void SetupEmptyStateUi()
    {
        if (_uiRoot == null || _emptyStateLabel != null)
        {
            return;
        }

        Label emptyStateLabel = new Label();
        emptyStateLabel.Name = "EmptyStateLabel";
        emptyStateLabel.Text = "Set parameters in the side panel, then click Generate.";
        emptyStateLabel.AnchorLeft = 0.0f;
        emptyStateLabel.AnchorTop = 0.0f;
        emptyStateLabel.AnchorRight = 1.0f;
        emptyStateLabel.AnchorBottom = 1.0f;
        emptyStateLabel.OffsetLeft = 180.0f;
        emptyStateLabel.OffsetTop = 120.0f;
        emptyStateLabel.OffsetRight = -180.0f;
        emptyStateLabel.OffsetBottom = -120.0f;
        emptyStateLabel.HorizontalAlignment = HorizontalAlignment.Center;
        emptyStateLabel.VerticalAlignment = VerticalAlignment.Center;
        emptyStateLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        emptyStateLabel.CustomMinimumSize = new Vector2(280.0f, 0.0f);
        emptyStateLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
        emptyStateLabel.Modulate = new Color(0.74f, 0.78f, 0.84f, 0.9f);
        _uiRoot.AddChild(emptyStateLabel);
        _emptyStateLabel = emptyStateLabel;
        UpdateEmptyStateVisibility();
    }

    private void UpdateEmptyStateVisibility()
    {
        if (_emptyStateLabel == null)
        {
            return;
        }

        _emptyStateLabel.Visible = _currentBody == null && _startupState == ViewerStartupState.UnconfiguredStandalone;
    }

    private void SetFileControlState(bool saveEnabled, bool loadEnabled)
    {
        if (_saveButton != null)
        {
            _saveButton.Disabled = !saveEnabled;
        }

        if (_loadButton != null)
        {
            _loadButton.Disabled = !loadEnabled;
        }

        if (_fileInfo != null && !saveEnabled)
        {
            _fileInfo.Text = "No object selected";
        }
        else if (_fileInfo != null && _currentBody != null)
        {
            UpdateFileInfoForCurrentTarget();
        }

        if (_saveFileDialog != null)
        {
            _saveFileDialog.Visible = false;
        }

        if (_loadFileDialog != null)
        {
            _loadFileDialog.Visible = false;
        }
    }

    private void SetFileControlsEnabled(bool enabled)
    {
        SetFileControlState(enabled, enabled);
    }

    private void ApplyUseCaseSettingsToControls(GenerationUseCaseSettings settings)
    {
        _activeUseCaseSettings = settings.Clone();

        if (_rulesetModeOption != null)
        {
            _rulesetModeOption.Select((int)_activeUseCaseSettings.RulesetMode);
        }

        if (_showTravellerReadoutsCheck != null)
        {
            _showTravellerReadoutsCheck.ButtonPressed = _activeUseCaseSettings.ShowTravellerReadouts;
        }

        if (_lifePermissivenessInput != null)
        {
            _lifePermissivenessInput.Value = _activeUseCaseSettings.LifePermissiveness;
        }

        if (_populationPermissivenessInput != null)
        {
            _populationPermissivenessInput.Value = _activeUseCaseSettings.PopulationPermissiveness;
        }
    }

    private void TryApplyUseCaseSettingsFromBody(CelestialBody body)
    {
        Dictionary? snapshot = body.Provenance?.SpecSnapshot;
        if (snapshot == null || !snapshot.ContainsKey("use_case_settings"))
        {
            _activeUseCaseSettings = GenerationUseCaseSettings.CreateDefault();
            return;
        }

        Variant settingsVariant = snapshot["use_case_settings"];
        if (settingsVariant.VariantType == Variant.Type.Dictionary)
        {
            _activeUseCaseSettings = GenerationUseCaseSettings.FromDictionary((Dictionary)settingsVariant);
            return;
        }

        _activeUseCaseSettings = GenerationUseCaseSettings.CreateDefault();
    }

    private void OnRulesetModeSelected(long selectedId)
    {
        _activeUseCaseSettings.RulesetMode = (GenerationUseCaseSettings.RulesetModeType)selectedId;
        if (_activeUseCaseSettings.RulesetMode == GenerationUseCaseSettings.RulesetModeType.Traveller)
        {
            _activeUseCaseSettings.ApplyTravellerDefaults();
        }

        ApplyUseCaseSettingsToControls(_activeUseCaseSettings);
    }
}
