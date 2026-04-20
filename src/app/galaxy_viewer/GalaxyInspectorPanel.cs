using System;
using Godot;
using StarGen.App.Components;
using StarGen.App.Shared;
using StarGen.App.Viewer;
using StarGen.Domain.Colonization;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;
using StarGen.Domain.Galaxy;
using StarGen.Services.Persistence;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// C# inspector panel for galaxy and star-selection details.
/// </summary>
public partial class GalaxyInspectorPanel : VBoxContainer
{
	/// <summary>
	/// Emitted when the user requests jump-route calculation.
	/// </summary>
	[Signal]
	public delegate void CalculateJumpRoutesRequestedEventHandler();

	/// <summary>
	/// Emitted when jump-route visibility is toggled.
	/// </summary>
	[Signal]
	public delegate void JumpRoutesVisibilityToggledEventHandler(bool showRoutes);

	/// <summary>
	/// Emitted when the user requests to open the selected star system.
	/// </summary>
	[Signal]
	public delegate void OpenSystemRequestedEventHandler(int starSeed, Vector3 worldPosition);

	/// <summary>
	/// Emitted when the user requests galaxy regeneration from the current editable config.
	/// </summary>
	[Signal]
	public delegate void ApplyGalaxyConfigRequestedEventHandler();

	private int _selectedStarSeed;
	private Vector3 _selectedStarPosition = Vector3.Zero;
	private StarSystemPreviewData? _currentPreview;
	private bool _showSeedControls;

	private VBoxContainer? _overviewContainer;
	private VBoxContainer? _configEditorContainer;
	private VBoxContainer? _profileSummaryContainer;
	private OptionButton? _galaxyTypeOption;
	private SpinBox? _numArmsInput;
	private SpinBox? _armPitchInput;
	private SpinBox? _armAmplitudeInput;
	private SpinBox? _bulgeIntensityInput;
	private SpinBox? _bulgeRadiusInput;
	private SpinBox? _radiusInput;
	private SpinBox? _diskLengthInput;
	private SpinBox? _diskHeightInput;
	private SpinBox? _densityInput;
	private SpinBox? _ellipticityInput;
	private SpinBox? _irregularityInput;
	private OptionButton? _rulesetModeOption;
	private CheckBox? _showTravellerReadoutsCheck;
	private SpinBox? _lifePermissivenessInput;
	private OptionButton? _mainworldPolicyOption;
	private SpinBox? _colonizationPermissivenessInput;
	private VBoxContainer? _configIssuesContainer;
	private VBoxContainer? _selectionContainer;
	private VBoxContainer? _previewContainer;
	private Control? _configSection;
	private Control? _overviewSection;
	private Control? _colonizationSection;
	private Button? _openSystemButton;
	private Button? _calculateRoutesButton;
	private CheckBox? _showRoutesCheck;
	private Label? _jumpRoutesProgressLabel;
	private ProgressBar? _jumpRoutesProgressBar;
	private bool _isCalculating;
	private GalaxyConfig? _editableConfig;

	/// <summary>
	/// Binds the scene-authored inspector UI.
	/// </summary>
	public override void _Ready()
	{
		CacheUi();
		InitializeUi();
	}

	/// <summary>
	/// GDScript-compatible wrapper for overview display.
	/// </summary>
	public void display_galaxy(Variant specVariant, int zoomLevel)
	{
		DisplayGalaxy(ConvertVariantToGalaxySpec(specVariant), zoomLevel);
	}

	/// <summary>
	/// Displays galaxy overview information.
	/// </summary>
	public void DisplayGalaxy(GalaxySpec? spec, int zoomLevel)
	{
		ClearContainer(_overviewContainer);
		if (_overviewContainer == null)
		{
			return;
		}

		if (spec == null)
		{
			AddProperty(_overviewContainer, "Status", "No galaxy loaded");
			return;
		}

		AddProperty(_overviewContainer, "Type", GetGalaxyTypeName(spec.Type));
		AddProperty(_overviewContainer, "Seed", spec.GalaxySeed.ToString());
		AddProperty(_overviewContainer, "Radius", $"{spec.RadiusPc / 1000.0:0.0} kpc");
		AddProperty(_overviewContainer, "Height", $"{spec.HeightPc / 1000.0:0.0} kpc");
		AddProperty(_overviewContainer, "Spiral Arms", spec.NumArms.ToString());
		AddProperty(_overviewContainer, "Arm Pitch", $"{spec.ArmPitchAngleDeg:0.0} deg");
		AddUseCaseOverview(_overviewContainer, _editableConfig?.UseCaseSettings);
		AddProperty(_overviewContainer, "View", GetZoomLevelName(zoomLevel));
	}

	/// <summary>
	/// Compatibility overload accepting enum zoom-level values.
	/// </summary>
	public void DisplayGalaxy(GalaxySpec? spec, GalaxyCoordinates.ZoomLevel zoomLevel)
	{
		DisplayGalaxy(spec, (int)zoomLevel);
	}

	/// <summary>
	/// GDScript-compatible wrapper for zoom-level updates.
	/// </summary>
	public void update_zoom_level(int zoomLevel)
	{
		UpdateZoomLevel(zoomLevel);
	}

	/// <summary>
	/// Updates the zoom-level display row.
	/// </summary>
	public void UpdateZoomLevel(int zoomLevel)
	{
		if (_overviewContainer == null)
		{
			return;
		}

		foreach (Node child in _overviewContainer.GetChildren())
		{
			if (child is not HBoxContainer row)
			{
				continue;
			}

			Label? keyLabel = row.GetNodeOrNull<Label>("Key");
			Label? valueLabel = row.GetNodeOrNull<Label>("Value");
			if (keyLabel != null && valueLabel != null && keyLabel.Text == "View:")
			{
				valueLabel.Text = GetZoomLevelName(zoomLevel);
				return;
			}
		}

		AddProperty(_overviewContainer, "View", GetZoomLevelName(zoomLevel));
	}

	/// <summary>
	/// Displays the live overview details for the active galaxy location.
	/// </summary>
	public void DisplayOverview(GalaxySpec? spec, Vector3 worldPosition, float density)
	{
		ClearContainer(_selectionContainer);
		if (_selectionContainer == null)
		{
			return;
		}

		if (spec == null)
		{
			AddProperty(_selectionContainer, "Status", "No galaxy loaded");
			return;
		}

		GalaxyInspectorSelectionFormatter.SelectionLocationSummary locationSummary =
			GalaxyInspectorSelectionFormatter.Build(worldPosition);

		AddProperty(_selectionContainer, "Type", GetGalaxyTypeName(spec.Type));
		if (_showSeedControls)
		{
			AddProperty(_selectionContainer, "Seed", spec.GalaxySeed.ToString());
		}
		AddProperty(_selectionContainer, "Quadrant", FormatVector3I(locationSummary.Quadrant));
		AddProperty(_selectionContainer, "Local", FormatVector3I(locationSummary.LocalGrid));
		AddProperty(_selectionContainer, "Density", density.ToString("0.0000"));
		AddProperty(_selectionContainer, "Azimuth", $"{locationSummary.AzimuthDegrees:0.0} deg");
		AddProperty(_selectionContainer, "Inclination", $"{locationSummary.InclinationDegrees:0.0} deg");
		AddProperty(
			_selectionContainer,
			"Distance from Core",
			GalaxyInspectorSelectionFormatter.FormatDistanceFromCore(locationSummary.DistanceFromCorePc));
	}

	/// <summary>
	/// Displays selected star information.
	/// </summary>
	public void display_selected_star(Vector3 worldPosition, int starSeed)
	{
		DisplaySelectedStar(worldPosition, starSeed);
	}

	/// <summary>
	/// Displays selected star information.
	/// </summary>
	public void DisplaySelectedStar(Vector3 worldPosition, int starSeed)
	{
		_selectedStarSeed = starSeed;
		_currentPreview = null;
		_selectedStarPosition = worldPosition;

		if (_openSystemButton != null)
		{
			_openSystemButton.Visible = true;
		}

		ClearContainer(_previewContainer);
		if (_previewContainer != null)
		{
			AddProperty(_previewContainer, "Status", "Generating preview...");
		}
	}

	/// <summary>
	/// GDScript-compatible preview wrapper.
	/// </summary>
	public void display_system_preview(Variant previewVariant)
	{
		DisplaySystemPreview(ConvertVariantToPreviewData(previewVariant));
	}

	/// <summary>
	/// Displays a generated system preview.
	/// </summary>
	public void DisplaySystemPreview(StarSystemPreviewData? preview)
	{
		_currentPreview = preview;
		ClearContainer(_previewContainer);
		if (_previewContainer == null)
		{
			return;
		}

		if (preview == null)
		{
			AddProperty(_previewContainer, "Status", "Preview unavailable");
			return;
		}

		AddProperty(_previewContainer, "Stars", BuildStarPreviewSummary(preview));
		AddProperty(_previewContainer, "Bodies", $"{preview.PlanetCount} planets, {preview.MoonCount} moons, {preview.BeltCount} belts");
		AddProperty(_previewContainer, "Settlement", BuildSettlementPreviewSummary(preview));
	}

	/// <summary>
	/// GDScript-compatible clear wrapper.
	/// </summary>
	public void clear_selection()
	{
		ClearSelection();
	}

	/// <summary>
	/// Clears selection and preview state.
	/// </summary>
	public void ClearSelection()
	{
		ClearContainer(_previewContainer);
		ClearStarSelection();
		if (_previewContainer != null)
		{
			AddProperty(_previewContainer, "Status", "No system selected");
		}
	}

	/// <summary>
	/// GDScript-compatible calculating-state wrapper.
	/// </summary>
	public void set_jump_routes_calculating(bool calculating)
	{
		SetJumpRoutesCalculating(calculating);
	}

	/// <summary>
	/// Updates the calculating state for jump-route controls.
	/// </summary>
	public void SetJumpRoutesCalculating(bool calculating)
	{
		_isCalculating = calculating;
		if (_calculateRoutesButton != null)
		{
			_calculateRoutesButton.Disabled = calculating;
			if (calculating)
			{
				_calculateRoutesButton.Text = "Simulating...";
			}
			else
			{
				_calculateRoutesButton.Text = "Run Colonization Simulation";
			}
		}

		if (_jumpRoutesProgressLabel != null)
		{
			_jumpRoutesProgressLabel.Visible = calculating;
			if (calculating)
			{
				_jumpRoutesProgressLabel.Text = "Preparing colonization simulation...";
			}
		}

		if (_jumpRoutesProgressBar != null)
		{
			_jumpRoutesProgressBar.Visible = calculating;
			if (calculating)
			{
				_jumpRoutesProgressBar.MaxValue = 1.0;
				_jumpRoutesProgressBar.Value = 0.0;
			}
		}
	}

	/// <summary>
	/// GDScript-compatible availability wrapper.
	/// </summary>
	public void set_jump_routes_available(bool available)
	{
		SetJumpRoutesAvailable(available);
	}

	/// <summary>
	/// Updates jump-route availability state.
	/// </summary>
	public void SetJumpRoutesAvailable(bool available)
	{
		if (_showRoutesCheck != null)
		{
			_showRoutesCheck.Disabled = !available;
		}
		if (_calculateRoutesButton != null)
		{
			_calculateRoutesButton.Disabled = false;
			if (available)
			{
				_calculateRoutesButton.Text = "Rerun Colonization Simulation";
			}
			else
			{
				_calculateRoutesButton.Text = "Run Colonization Simulation";
			}
		}
		_isCalculating = false;

		if (_jumpRoutesProgressLabel != null)
		{
			_jumpRoutesProgressLabel.Visible = false;
		}

		if (_jumpRoutesProgressBar != null)
		{
			_jumpRoutesProgressBar.Visible = false;
		}
	}

	/// <summary>
	/// Updates the visible jump-route calculation progress.
	/// </summary>
	public void SetJumpRoutesProgress(int completed, int total)
	{
		SetJumpRoutesProgress("Building jump routes", completed, total);
	}

	/// <summary>
	/// Updates the visible jump-route calculation progress for a named stage.
	/// </summary>
	public void SetJumpRoutesProgress(string stageLabel, int completed, int total)
	{
		int safeTotal;
		if (total > 0)
		{
			safeTotal = total;
		}
		else
		{
			safeTotal = 1;
		}

		int safeCompleted;
		if (completed >= 0)
		{
			safeCompleted = completed;
		}
		else
		{
			safeCompleted = 0;
		}

		if (_jumpRoutesProgressLabel != null)
		{
			_jumpRoutesProgressLabel.Visible = true;
			_jumpRoutesProgressLabel.Text = $"{stageLabel}: {safeCompleted}/{safeTotal}";
		}

		if (_jumpRoutesProgressBar != null)
		{
			_jumpRoutesProgressBar.Visible = true;
			_jumpRoutesProgressBar.MaxValue = safeTotal;
			_jumpRoutesProgressBar.Value = safeCompleted;
		}
	}

	/// <summary>
	/// Updates the jump-route progress indicator for a coarse pipeline stage.
	/// </summary>
	public void SetJumpRoutesStage(string stageLabel, int stageIndex, int stageCount)
	{
		SetJumpRoutesProgress(stageLabel, stageIndex, stageCount);
	}

	/// <summary>
	/// GDScript-compatible checkbox-state wrapper.
	/// </summary>
	public bool get_show_routes_checked()
	{
		return GetShowRoutesChecked();
	}

	/// <summary>
	/// Returns whether jump routes are currently toggled on.
	/// </summary>
	public bool GetShowRoutesChecked()
	{
		return _showRoutesCheck?.ButtonPressed ?? true;
	}

	/// <summary>
	/// Returns whether a star is selected.
	/// </summary>
	public bool has_star_selected()
	{
		return _selectedStarSeed != 0;
	}

	/// <summary>
	/// PascalCase compatibility alias.
	/// </summary>
	public bool HasStarSelected() => has_star_selected();

	/// <summary>
	/// Returns the selected star seed.
	/// </summary>
	public int get_selected_star_seed()
	{
		return _selectedStarSeed;
	}

	/// <summary>
	/// PascalCase compatibility alias.
	/// </summary>
	public int GetSelectedStarSeed() => get_selected_star_seed();

	/// <summary>
	/// Returns the selected star position.
	/// </summary>
	public Vector3 get_selected_star_position()
	{
		return _selectedStarPosition;
	}

	/// <summary>
	/// PascalCase compatibility alias.
	/// </summary>
	public Vector3 GetSelectedStarPosition() => get_selected_star_position();

	/// <summary>
	/// Returns the current preview data.
	/// </summary>
	public StarSystemPreviewData? get_current_preview()
	{
		return _currentPreview;
	}

	/// <summary>
	/// Returns whether the legacy active-profile section is visible.
	/// </summary>
	public bool IsConfigSectionVisible()
	{
		return _configSection?.Visible ?? false;
	}

	/// <summary>
	/// Returns whether the legacy overview section is visible.
	/// </summary>
	public bool IsOverviewSectionVisible()
	{
		return _overviewSection?.Visible ?? false;
	}

	/// <summary>
	/// Returns whether the legacy jump-route tools section is visible.
	/// </summary>
	public bool IsColonizationSectionVisible()
	{
		return _colonizationSection?.Visible ?? false;
	}

	private void CacheUi()
	{
		_configSection = GetNodeOrNull<Control>("ConfigSection");
		_overviewSection = GetNodeOrNull<Control>("OverviewSection");
		_colonizationSection = GetNodeOrNull<Control>("ColonizationSection");
		_configEditorContainer = GetNodeOrNull<VBoxContainer>("ConfigSection/Content/ConfigEditorContainer");
		_profileSummaryContainer = GetNodeOrNull<VBoxContainer>("ConfigSection/Content/ConfigEditorContainer/ProfileSummaryContainer");
		_configIssuesContainer = GetNodeOrNull<VBoxContainer>("ConfigSection/Content/ConfigEditorContainer/ConfigIssuesContainer");
		_overviewContainer = GetNodeOrNull<VBoxContainer>("OverviewSection/Content");
		_selectionContainer = GetNodeOrNull<VBoxContainer>("SelectionSection/Content");
		_previewContainer = GetNodeOrNull<VBoxContainer>("PreviewSection/Content");
		_openSystemButton = GetNodeOrNull<Button>("PreviewSection/OpenSystemButton");
		_colonizationPermissivenessInput = GetNodeOrNull<SpinBox>("ColonizationSection/Content/ColonizationSettingsRow/ColonizationPermissivenessInput");
		_calculateRoutesButton = GetNodeOrNull<Button>("ColonizationSection/Content/CalculateRoutesButton");
		_showRoutesCheck = GetNodeOrNull<CheckBox>("ColonizationSection/Content/ShowRoutesCheck");
		_jumpRoutesProgressLabel = GetNodeOrNull<Label>("ColonizationSection/Content/JumpRoutesProgressLabel");
		_jumpRoutesProgressBar = GetNodeOrNull<ProgressBar>("ColonizationSection/Content/JumpRoutesProgressBar");

		if (_openSystemButton != null)
		{
			_openSystemButton.Pressed += OnOpenSystemPressed;
		}

		if (_calculateRoutesButton != null)
		{
			_calculateRoutesButton.Pressed += OnCalculateRoutesPressed;
		}

		if (_showRoutesCheck != null)
		{
			_showRoutesCheck.Toggled += OnShowRoutesToggled;
		}
	}

	private void InitializeUi()
	{
		if (_selectionContainer == null
			|| _previewContainer == null
			|| _profileSummaryContainer == null
			|| _configIssuesContainer == null)
		{
			throw new InvalidOperationException("GalaxyInspectorPanel scene is missing required inspector nodes.");
		}

		if (_configSection != null)
		{
			_configSection.Visible = false;
		}

		if (_overviewSection != null)
		{
			_overviewSection.Visible = false;
		}

		if (_colonizationSection != null)
		{
			_colonizationSection.Visible = false;
		}

		_showSeedControls = StudioUiPreferencesService.LoadOrDefault().ShowSeedControls;
		ClearContainer(_selectionContainer);
		ClearContainer(_previewContainer);
		AddProperty(_selectionContainer, "Status", "No galaxy loaded");
		AddProperty(_previewContainer, "Status", "No system selected");
		SetConfigIssues(new GenerationParameterIssueSet());
		RebuildProfileSummary();
	}

	/// <summary>
	/// Applies a galaxy config to the editor controls.
	/// </summary>
	public void SetEditableConfig(GalaxyConfig? config)
	{
		if (config == null)
		{
			return;
		}

		_editableConfig = GalaxyConfig.FromDictionary(config.ToDictionary()) ?? config;
		RebuildProfileSummary();
	}

	/// <summary>
	/// Returns the current editable galaxy config.
	/// </summary>
	public GalaxyConfig GetEditableConfig()
	{
		if (_editableConfig == null)
		{
			return GalaxyConfig.CreateDefault();
		}

		return GalaxyConfig.FromDictionary(_editableConfig.ToDictionary()) ?? _editableConfig;
	}

	/// <summary>
	/// Displays current config validation issues.
	/// </summary>
	public void SetConfigIssues(GenerationParameterIssueSet issues)
	{
		if (_configIssuesContainer == null)
		{
			return;
		}

		foreach (Node child in _configIssuesContainer.GetChildren())
		{
			child.QueueFree();
		}

		if (issues.Issues.Count == 0)
		{
			Label cleanLabel = UiSceneTemplates.InstantiateMessageLabel();
			cleanLabel.Text = "No parameter issues.";
			cleanLabel.Modulate = new Color(0.55f, 0.75f, 0.55f, 1.0f);
			_configIssuesContainer.AddChild(cleanLabel);
			return;
		}

		foreach (GenerationParameterIssue issue in issues.Issues)
		{
			Label issueLabel = UiSceneTemplates.InstantiateMessageLabel();
			if (issue.Severity == GenerationParameterIssue.IssueSeverity.Error)
			{
				issueLabel.Modulate = new Color(1.0f, 0.45f, 0.45f, 1.0f);
				issueLabel.Text = $"Error: {issue.Message}";
			}
			else
			{
				issueLabel.Modulate = new Color(0.85f, 0.7f, 0.3f, 1.0f);
				issueLabel.Text = $"Warning: {issue.Message}";
			}

			_configIssuesContainer.AddChild(issueLabel);
		}
	}

	private void ClearStarSelection()
	{
		_selectedStarSeed = 0;
		_selectedStarPosition = Vector3.Zero;
		_currentPreview = null;
		if (_openSystemButton != null)
		{
			_openSystemButton.Visible = false;
		}
	}

	private void OnOpenSystemPressed()
	{
		if (_selectedStarSeed != 0)
		{
			EmitSignal(SignalName.OpenSystemRequested, _selectedStarSeed, _selectedStarPosition);
		}
	}

	private void OnCalculateRoutesPressed()
	{
		if (!_isCalculating)
		{
			EmitSignal(SignalName.CalculateJumpRoutesRequested);
		}
	}

	private void OnShowRoutesToggled(bool enabled)
	{
		EmitSignal(SignalName.JumpRoutesVisibilityToggled, enabled);
	}

	private void RebuildProfileSummary()
	{
		if (_profileSummaryContainer == null)
		{
			return;
		}

		ClearContainer(_profileSummaryContainer);
		if (_editableConfig == null)
		{
			AddProperty(_profileSummaryContainer, "Status", "No galaxy profile loaded");
			return;
		}

		AddProperty(_profileSummaryContainer, "Type", _editableConfig.GetTypeName());
		if (_editableConfig.Type == GalaxySpec.GalaxyType.Spiral)
		{
			AddProperty(_profileSummaryContainer, "Arms", _editableConfig.NumArms.ToString());
			AddProperty(_profileSummaryContainer, "Pitch", $"{_editableConfig.ArmPitchAngleDeg:0.0} deg");
			AddProperty(_profileSummaryContainer, "Amplitude", _editableConfig.ArmAmplitude.ToString("0.00"));
		}
		else if (_editableConfig.Type == GalaxySpec.GalaxyType.Elliptical)
		{
			AddProperty(_profileSummaryContainer, "Ellipticity", _editableConfig.Ellipticity.ToString("0.00"));
		}
		else
		{
			AddProperty(_profileSummaryContainer, "Irregularity", _editableConfig.IrregularityScale.ToString("0.00"));
		}

		AddProperty(_profileSummaryContainer, "Radius", $"{_editableConfig.RadiusPc / 1000.0:0.0} kpc");
		AddProperty(_profileSummaryContainer, "Disk Length", $"{_editableConfig.DiskScaleLengthPc:0} pc");
		AddProperty(_profileSummaryContainer, "Disk Height", $"{_editableConfig.DiskScaleHeightPc:0} pc");
		AddProperty(_profileSummaryContainer, "Density", $"{_editableConfig.StarDensityMultiplier:0.0}x");
		AddProperty(_profileSummaryContainer, "Bulge Intensity", _editableConfig.BulgeIntensity.ToString("0.00"));
		AddProperty(_profileSummaryContainer, "Bulge Radius", $"{_editableConfig.BulgeRadiusPc:0} pc");
		AddUseCaseOverview(_profileSummaryContainer, _editableConfig.UseCaseSettings);
	}

	private void SetConfigControlsEditable(bool editable)
	{
		SetControlEditable(_galaxyTypeOption, editable);
		SetControlEditable(_numArmsInput, editable);
		SetControlEditable(_armPitchInput, editable);
		SetControlEditable(_armAmplitudeInput, editable);
		SetControlEditable(_bulgeIntensityInput, editable);
		SetControlEditable(_bulgeRadiusInput, editable);
		SetControlEditable(_radiusInput, editable);
		SetControlEditable(_diskLengthInput, editable);
		SetControlEditable(_diskHeightInput, editable);
		SetControlEditable(_densityInput, editable);
		SetControlEditable(_ellipticityInput, editable);
		SetControlEditable(_irregularityInput, editable);
		SetControlEditable(_rulesetModeOption, editable);
		SetControlEditable(_showTravellerReadoutsCheck, editable);
		SetControlEditable(_lifePermissivenessInput, editable);
		SetControlEditable(_mainworldPolicyOption, editable);
	}

	private static void SetControlEditable(Control? control, bool editable)
	{
		if (control == null)
		{
			return;
		}

		if (control is BaseButton typedButton)
		{
			typedButton.Disabled = !editable;
			return;
		}

		if (control is OptionButton typedOptionButton)
		{
			typedOptionButton.Disabled = !editable;
			return;
		}

		if (control is SpinBox typedSpinBox)
		{
			typedSpinBox.Editable = editable;
			return;
		}
	}

	private GenerationUseCaseSettings BuildUseCaseSettingsFromControls()
	{
		GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
		if (_rulesetModeOption != null)
		{
			settings.RulesetMode = (GenerationUseCaseSettings.RulesetModeType)_rulesetModeOption.Selected;
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
			settings.MainworldPolicy = (GenerationUseCaseSettings.MainworldPolicyType)_mainworldPolicyOption.Selected;
		}

		return settings;
	}

	private void ApplyUseCaseSettingsToControls(GenerationUseCaseSettings? settings)
	{
		GenerationUseCaseSettings resolvedSettings = settings?.Clone() ?? GenerationUseCaseSettings.CreateDefault();
		if (_rulesetModeOption != null)
		{
			_rulesetModeOption.Select((int)resolvedSettings.RulesetMode);
		}

		if (_showTravellerReadoutsCheck != null)
		{
			_showTravellerReadoutsCheck.ButtonPressed = resolvedSettings.ShowTravellerReadouts;
		}

		if (_lifePermissivenessInput != null)
		{
			_lifePermissivenessInput.Value = resolvedSettings.LifePermissiveness;
		}

		if (_mainworldPolicyOption != null)
		{
			_mainworldPolicyOption.Select((int)resolvedSettings.MainworldPolicy);
		}
	}

	private void OnRulesetModeSelected(long selectedId)
	{
		if ((GenerationUseCaseSettings.RulesetModeType)selectedId == GenerationUseCaseSettings.RulesetModeType.Traveller)
		{
			if (_showTravellerReadoutsCheck != null)
			{
				_showTravellerReadoutsCheck.ButtonPressed = true;
			}

			if (_mainworldPolicyOption != null)
			{
				_mainworldPolicyOption.Select((int)GenerationUseCaseSettings.MainworldPolicyType.Require);
			}
		}
	}

	private void AddUseCaseOverview(VBoxContainer container, GenerationUseCaseSettings? settings)
	{
		GenerationUseCaseSettings resolvedSettings = settings?.Clone() ?? GenerationUseCaseSettings.CreateDefault();
		string readoutText;
		if (resolvedSettings.ShowTravellerReadouts)
		{
			readoutText = "On";
		}
		else
		{
			readoutText = "Off";
		}

		AddProperty(container, "Ruleset", GenerationUseCasePresentation.GetRulesetLabel(resolvedSettings.RulesetMode));
		AddProperty(container, "Traveller Readouts", readoutText);
		AddProperty(
			container,
			"Life Potential",
			$"{resolvedSettings.LifePermissiveness:0.00} {PermissivenessScaleHelper.GetBandLabel(resolvedSettings.LifePermissiveness)}");
		AddProperty(container, "Mainworld Policy", resolvedSettings.MainworldPolicy.ToString());
	}

	/// <summary>
	/// Returns the current colonization-simulation settings from the tool UI.
	/// </summary>
	public ColonizationSimulationSettings GetColonizationSimulationSettings()
	{
		ColonizationSimulationSettings settings = ColonizationSimulationSettings.CreateDefault();
		if (_colonizationPermissivenessInput != null)
		{
			settings.ExpansionPermissiveness = _colonizationPermissivenessInput.Value;
		}

		return settings;
	}

	/// <summary>
	/// Applies colonization-simulation settings to the tool UI.
	/// </summary>
	public void SetColonizationSimulationSettings(ColonizationSimulationSettings settings)
	{
		if (_colonizationPermissivenessInput != null)
		{
			_colonizationPermissivenessInput.Value = settings.ExpansionPermissiveness;
		}
	}

	private static void ClearContainer(VBoxContainer? container)
	{
		if (container == null)
		{
			return;
		}

		foreach (Node child in container.GetChildren())
		{
			child.QueueFree();
		}
	}

	private static void AddProperty(VBoxContainer container, string key, string value)
	{
		HBoxContainer row = UiSceneTemplates.InstantiatePropertyRow();
		Label keyLabel = UiSceneTemplates.GetRequiredChild<Label>(row, "Key");
		Label valueLabel = UiSceneTemplates.GetRequiredChild<Label>(row, "Value");
		keyLabel.Text = key + ":";
		valueLabel.Text = value;
		container.AddChild(row);
	}

	private static string GetGalaxyTypeName(GalaxySpec.GalaxyType galaxyType)
	{
		return galaxyType switch
		{
			GalaxySpec.GalaxyType.Spiral => "Spiral",
			GalaxySpec.GalaxyType.Elliptical => "Elliptical",
			GalaxySpec.GalaxyType.Lenticular => "Lenticular",
			GalaxySpec.GalaxyType.Irregular => "Irregular",
			_ => "Unknown",
		};
	}

	private static string GetZoomLevelName(int zoomLevel)
	{
		return zoomLevel switch
		{
			(int)GalaxyCoordinates.ZoomLevel.Galaxy => "Galaxy",
			(int)GalaxyCoordinates.ZoomLevel.Quadrant => "Quadrant",
			(int)GalaxyCoordinates.ZoomLevel.Sector => "Sector",
			(int)GalaxyCoordinates.ZoomLevel.Subsector => "Star Field",
			_ => "Unknown",
		};
	}

	private static string FormatVector3I(Vector3I value)
	{
		return $"({value.X}, {value.Y}, {value.Z})";
	}

	private static string BuildStarPreviewSummary(StarSystemPreviewData preview)
	{
		if (preview.StarCount <= 0)
		{
			return "Unknown";
		}

		string spectralSummary;
		if (preview.SpectralClasses.Length == 0)
		{
			spectralSummary = "type unknown";
		}
		else
		{
			int maxSpectralCount = Math.Min(preview.SpectralClasses.Length, 3);
			string[] entries = new string[maxSpectralCount];
			for (int index = 0; index < maxSpectralCount; index += 1)
			{
				entries[index] = preview.SpectralClasses[index];
			}

			spectralSummary = string.Join(", ", entries);
			if (preview.SpectralClasses.Length > maxSpectralCount)
			{
				spectralSummary += ", ...";
			}
		}

		if (preview.StarCount == 1)
		{
			return $"1 ({spectralSummary})";
		}

		return $"{preview.StarCount} ({spectralSummary})";
	}

	private static string BuildSettlementPreviewSummary(StarSystemPreviewData preview)
	{
		if (!preview.IsInhabited)
		{
			return "Uninhabited";
		}

		return $"Inhabited ({PropertyFormatter.FormatPopulation(preview.TotalPopulation)})";
	}

	private static GalaxySpec? ConvertVariantToGalaxySpec(Variant specVariant)
	{
		if (specVariant.VariantType == Variant.Type.Nil)
		{
			return null;
		}

		GodotObject? godotObject = specVariant.AsGodotObject();
		if (godotObject is GalaxySpec typedSpec)
		{
			return typedSpec;
		}

		if (godotObject != null && godotObject.HasMethod("to_dict"))
		{
			Variant dictVariant = godotObject.Call("to_dict");
			if (dictVariant.VariantType == Variant.Type.Dictionary)
			{
				return GalaxySpec.FromDictionary((Godot.Collections.Dictionary)dictVariant);
			}
		}

		return null;
	}

	private static StarSystemPreviewData? ConvertVariantToPreviewData(Variant previewVariant)
	{
		if (previewVariant.VariantType == Variant.Type.Nil)
		{
			return null;
		}

		GodotObject? godotObject = previewVariant.AsGodotObject();
		if (godotObject is StarSystemPreviewData typedPreview)
		{
			return typedPreview;
		}

		if (godotObject == null)
		{
			return null;
		}

		StarSystemPreviewData preview = new();
		preview.StarSeed = GetIntProperty(godotObject, "star_seed", 0);
		preview.WorldPosition = GetVector3Property(godotObject, "world_position", Vector3.Zero);
		preview.StarCount = GetIntProperty(godotObject, "star_count", 0);
		preview.SpectralClasses = GetStringArrayProperty(godotObject, "spectral_classes");
		preview.StarTemperatures = GetFloatArrayProperty(godotObject, "star_temperatures");
		preview.PlanetCount = GetIntProperty(godotObject, "planet_count", 0);
		preview.MoonCount = GetIntProperty(godotObject, "moon_count", 0);
		preview.BeltCount = GetIntProperty(godotObject, "belt_count", 0);
		preview.Metallicity = GetDoubleProperty(godotObject, "metallicity", 1.0);
		preview.TotalPopulation = GetIntProperty(godotObject, "total_population", 0);
		preview.IsInhabited = GetBoolProperty(godotObject, "is_inhabited", false);
		return preview;
	}

	private static int GetIntProperty(GodotObject source, string propertyName, int fallback)
	{
		Variant value = source.Get(propertyName);
		return value.VariantType switch
		{
			Variant.Type.Int => (int)value,
			Variant.Type.Float => (int)(double)value,
			_ => fallback,
		};
	}

	private static double GetDoubleProperty(GodotObject source, string propertyName, double fallback)
	{
		Variant value = source.Get(propertyName);
		return value.VariantType switch
		{
			Variant.Type.Float => (double)value,
			Variant.Type.Int => (int)value,
			_ => fallback,
		};
	}

	private static bool GetBoolProperty(GodotObject source, string propertyName, bool fallback)
	{
		Variant value = source.Get(propertyName);
		if (value.VariantType == Variant.Type.Bool)
		{
			return (bool)value;
		}

		return fallback;
	}

	private static Vector3 GetVector3Property(GodotObject source, string propertyName, Vector3 fallback)
	{
		Variant value = source.Get(propertyName);
		if (value.VariantType == Variant.Type.Vector3)
		{
			return (Vector3)value;
		}

		return fallback;
	}

	private static string[] GetStringArrayProperty(GodotObject source, string propertyName)
	{
		Variant value = source.Get(propertyName);
		if (value.VariantType != Variant.Type.Array)
		{
			return global::System.Array.Empty<string>();
		}

		Godot.Collections.Array array = (Godot.Collections.Array)value;
		string[] result = new string[array.Count];
		for (int index = 0; index < array.Count; index++)
		{
			Variant item = array[index];
			if (item.VariantType == Variant.Type.String)
			{
				result[index] = (string)item;
			}
			else
			{
				result[index] = "?";
			}
		}

		return result;
	}

	private static float[] GetFloatArrayProperty(GodotObject source, string propertyName)
	{
		Variant value = source.Get(propertyName);
		if (value.VariantType != Variant.Type.Array)
		{
			return global::System.Array.Empty<float>();
		}

		Godot.Collections.Array array = (Godot.Collections.Array)value;
		float[] result = new float[array.Count];
		for (int index = 0; index < array.Count; index++)
		{
			Variant item = array[index];
			result[index] = item.VariantType switch
			{
				Variant.Type.Float => (float)(double)item,
				Variant.Type.Int => (int)item,
				_ => 0.0f,
			};
		}

		return result;
	}
}
