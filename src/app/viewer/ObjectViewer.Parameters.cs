using Godot;
using Godot.Collections;
using StarGen.App.Shared;
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
        if (_rulesetModeOption == null
            || _showTravellerReadoutsCheck == null
            || _lifePermissivenessInput == null
            || _useCaseAssumptionsLabel == null)
        {
            return;
        }

        if (_rulesetModeOption.ItemCount == 0)
        {
            _rulesetModeOption.AddItem(GenerationUseCasePresentation.RealisticRulesetLabel, (int)GenerationUseCaseSettings.RulesetModeType.Default);
            _rulesetModeOption.AddItem("Space Opera", (int)GenerationUseCaseSettings.RulesetModeType.Traveller);
        }

        _rulesetModeOption.ItemSelected += OnRulesetModeSelected;
        _showTravellerReadoutsCheck.Toggled += OnShowTravellerReadoutsToggled;
        _lifePermissivenessInput.ValueChanged += OnLifePermissivenessChanged;
        ApplyUseCaseSettingsToControls(_activeUseCaseSettings);
        _populationPermissivenessInput?.GetParent<Control>()?.Hide();
    }

    private void SetupEmptyStateUi()
    {
        if (_emptyStateLabel == null)
        {
            throw new System.InvalidOperationException("ObjectViewer scene is missing EmptyStateLabel.");
        }

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
        bool persistenceEnabled = ReleaseEditionService.CanUseSaveLoad();
        if (_fileSection != null)
        {
            _fileSection.Visible = persistenceEnabled;
        }

        if (_saveButton != null)
        {
            _saveButton.Disabled = !persistenceEnabled || !saveEnabled;
        }

        if (_loadButton != null)
        {
            _loadButton.Disabled = !persistenceEnabled || !loadEnabled;
        }

        if (_fileInfo != null && !persistenceEnabled)
        {
            _fileInfo.Text = ReleaseEditionService.GetPersistenceDisabledMessage();
        }
        else if (_fileInfo != null && !saveEnabled)
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

    private void OnShowTravellerReadoutsToggled(bool enabled)
    {
        _activeUseCaseSettings.ShowTravellerReadouts = enabled;
    }

    private void OnLifePermissivenessChanged(double value)
    {
        _activeUseCaseSettings.LifePermissiveness = value;
    }

    private void OnPopulationPermissivenessChanged(double value) { }
}
