using System;
using Godot;
using StarGen.App.Components;
using StarGen.App.Shared;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Rng;
using StarGen.Services.Persistence;

namespace StarGen.App;

/// <summary>
/// Galaxy-generation studio screen with presets and exposed tuning controls.
/// </summary>
public partial class GalaxyGenerationScreen : Control
{
	[Signal]
	public delegate void start_new_galaxyEventHandler(GalaxyConfig config, int seedValue);

	[Signal]
	public delegate void back_requestedEventHandler();

	[Signal]
	public delegate void quit_requestedEventHandler();

	private enum Preset
	{
		Custom = 0,
		MilkyWay = 1,
		Andromeda = 2,
		Whirlpool = 3,
		Sombrero = 4,
		LargeMagellanicCloud = 5,
	}

	private SeededRng? _seededRng;
	private GodotObject? _seededRngObject;
	private bool _isUpdatingUi;

	private Button? _startButton;
	private Button? _loadButton;
	private Button? _backButton;
	private Button? _quitButton;
	private Button? _randomizeButton;
	private OptionButton? _presetOption;
	private Label? _versionLabel;
	private Label? _summaryLabel;
	private OptionButton? _typeOption;
	private HBoxContainer? _armsRow;
	private HSlider? _armsSlider;
	private Label? _armsValue;
	private HSlider? _pitchSlider;
	private Label? _pitchValue;
	private HSlider? _amplitudeSlider;
	private Label? _amplitudeValue;
	private HSlider? _bulgeIntensitySlider;
	private Label? _bulgeIntensityValue;
	private HSlider? _bulgeRadiusSlider;
	private Label? _bulgeRadiusValue;
	private HBoxContainer? _ellipticityRow;
	private HSlider? _ellipticitySlider;
	private Label? _ellipticityValue;
	private HBoxContainer? _irregularityRow;
	private HSlider? _irregularitySlider;
	private Label? _irregularityValue;
	private HSlider? _radiusSlider;
	private Label? _radiusValue;
	private HSlider? _diskLengthSlider;
	private Label? _diskLengthValue;
	private HSlider? _diskHeightSlider;
	private Label? _diskHeightValue;
	private HSlider? _densitySlider;
	private Label? _densityValue;
	private SpinBox? _seedSpin;
	private HBoxContainer? _seedContainer;
	private OptionButton? _rulesetModeOption;
	private BaseButton? _showTravellerReadoutsCheck;
	private BaseButton? _forceLifeOnSupportableWorldsCheck;
	private HBoxContainer? _mainworldPolicyRow;
	private OptionButton? _mainworldPolicyOption;
	private HBoxContainer? _temperateWorldBiasRow;
	private HSlider? _temperateWorldBiasSlider;
	private Label? _temperateWorldBiasValue;
	private HBoxContainer? _harshWorldBiasRow;
	private HSlider? _harshWorldBiasSlider;
	private Label? _harshWorldBiasValue;
	private HBoxContainer? _terrestrialWorldBiasRow;
	private HSlider? _terrestrialWorldBiasSlider;
	private Label? _terrestrialWorldBiasValue;
	private HBoxContainer? _nativeLifeBiasRow;
	private HSlider? _nativeLifeBiasSlider;
	private Label? _nativeLifeBiasValue;
	private OptionButton? _lifeFrameworkOption;
	private OptionButton? _abiogenesisModelOption;
	private OptionButton? _complexLifeModelOption;
	private OptionButton? _civilizationModelOption;
	private OptionButton? _environmentalWindowWeightOption;
	private HSlider? _populationPermissivenessInput;
	private Label? _populationPermissivenessValueLabel;
	private HBoxContainer? _populationPermissivenessRow;
	private Button? _advancedAssumptionsInfoButton;
	private VBoxContainer? _settingsVBox;
	private VBoxContainer? _rulesVBox;
	private BoxContainer? _studioRow;
	private Control? _settingsPanel;
	private Control? _rulesPanel;
	private Control? _summaryPanel;
	private Label? _assumptionsLabel;
	private VBoxContainer? _issuesContainer;
	private GenerationParameterIssueSet _currentIssues = new();
	private bool _showSeedControls;

	private const string HeroRootPath = "MarginContainer/ScrollContainer/Layout/HeroPanel/MarginContainer/HeroVBox";
	private const string StudioRootPath = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox";
	private const string ParameterRootPath = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox";
	private const string RulesRootPath = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/RulesPanel/MarginContainer/RulesVBox/ScrollContainer/RulesContent";
	private const string SummaryRootPath = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SummaryPanel/MarginContainer/SummaryVBox";
	private const float GalaxyStudioCompactBreakpoint = 1080.0f;

	/// <summary>
	/// Initializes UI wiring.
	/// </summary>
	public override void _Ready()
	{
		CacheNodeReferences();
		ApplyLayoutPolish();
		ConnectSignals();
		ApplyParameterTooltips();
		ApplyVersionLabel();
		ApplySciencePanelText();
		ApplyUseCaseSettingsToControls(GenerationUseCaseSettings.CreateDefault());
		UpdateTypeSpecificControls();
		UpdateAllValueLabels();
		ApplySeedVisibilityPreference(rerollHiddenSeed: true);
		RefreshValidationIssues();
		ApplyResponsiveLayout();
		Resized += ApplyResponsiveLayout;
	}

	/// <summary>
	/// Sets the deterministic RNG used by the startup screen.
	/// </summary>
	public void SetSeededRng(SeededRng? rng)
	{
		_seededRng = rng;
		_seededRngObject = null;
		RefreshRandomSeedDisplay();
	}

	/// <summary>
	/// GDScript-compatible RNG injection wrapper.
	/// </summary>
	public void set_seeded_rng(Variant rngVariant)
	{
		_seededRng = null;
		if (rngVariant.VariantType == Variant.Type.Nil)
		{
			_seededRngObject = null;
		}
		else
		{
			_seededRngObject = rngVariant.AsGodotObject();
		}
		RefreshRandomSeedDisplay();
	}

	/// <summary>
	/// Returns the current generation config represented by the UI.
	/// </summary>
	public GalaxyConfig GetCurrentConfig()
	{
		GalaxyConfig config = new GalaxyConfig
		{
			Type = (GalaxySpec.GalaxyType)(_typeOption?.Selected ?? (int)GalaxySpec.GalaxyType.Spiral),
			NumArms = (int)(_armsSlider?.Value ?? 4.0),
			ArmPitchAngleDeg = _pitchSlider?.Value ?? 14.0,
			ArmAmplitude = _amplitudeSlider?.Value ?? 0.65,
			BulgeIntensity = _bulgeIntensitySlider?.Value ?? 0.8,
			BulgeRadiusPc = _bulgeRadiusSlider?.Value ?? 1500.0,
			RadiusPc = _radiusSlider?.Value ?? 15000.0,
			DiskScaleLengthPc = _diskLengthSlider?.Value ?? 4000.0,
			DiskScaleHeightPc = _diskHeightSlider?.Value ?? 300.0,
			StarDensityMultiplier = _densitySlider?.Value ?? 1.0,
			Ellipticity = _ellipticitySlider?.Value ?? 0.3,
			IrregularityScale = _irregularitySlider?.Value ?? 0.5,
			UseCaseSettings = BuildUseCaseSettingsFromControls(),
		};
		ApplyScientificValuesToConfig(config);
		return config;
	}

	/// <summary>
	/// GDScript-compatible config getter wrapper.
	/// </summary>
	public GalaxyConfig get_current_config()
	{
		return GetCurrentConfig();
	}

	/// <summary>
	/// Applies a typed galaxy configuration to the galaxy-studio controls.
	/// </summary>
	public void SetCurrentConfig(GalaxyConfig config)
	{
		_isUpdatingUi = true;
		ApplyConfig(config);
		UpdateTypeSpecificControls();
		UpdateAllValueLabels();
		_isUpdatingUi = false;
		RefreshValidationIssues();
	}

	/// <summary>
	/// GDScript-compatible config setter wrapper.
	/// </summary>
	public void set_current_config(GalaxyConfig config)
	{
		SetCurrentConfig(config);
	}

	/// <summary>
	/// Returns the current validation results for the startup configuration.
	/// </summary>
	public GenerationParameterIssueSet GetCurrentIssues()
	{
		return _currentIssues;
	}

	/// <summary>
	/// Refreshes the seed field with a deterministic random value.
	/// </summary>
	public void RefreshRandomSeedDisplay()
	{
		if (_seedSpin != null)
		{
			_seedSpin.Value = GenerateRandomSeed();
		}
	}

	/// <summary>
	/// GDScript-compatible refresh wrapper.
	/// </summary>
	public void refresh_random_seed_display()
	{
		RefreshRandomSeedDisplay();
	}

	/// <summary>
	/// Applies the global studio seed-visibility preference and optionally rerolls hidden seeds.
	/// </summary>
	public void ApplySeedVisibilityPreference(bool rerollHiddenSeed)
	{
		StudioUiPreferencesService.StudioUiPreferences preferences = StudioUiPreferencesService.LoadOrDefault();
		_showSeedControls = preferences.ShowSeedControls;

		if (_seedContainer != null)
		{
			_seedContainer.Visible = _showSeedControls;
		}

		if (_showSeedControls)
		{
			if (_seedSpin != null && _seedSpin.Value <= 0.0)
			{
				_seedSpin.Value = GenerateRandomSeed();
			}
		}
		else if (rerollHiddenSeed)
		{
			RefreshRandomSeedDisplay();
		}

		RefreshValidationIssues();
	}

	/// <summary>
	/// Controls the visibility of the back and quit buttons for menu-driven navigation.
	/// </summary>
	public void SetNavigationVisibility(bool showBackButton, bool showQuitButton)
	{
		if (_backButton != null)
		{
			_backButton.Visible = showBackButton;
		}

		if (_quitButton != null)
		{
			_quitButton.Visible = showQuitButton;
		}
	}

	/// <summary>
	/// GDScript-compatible visibility wrapper.
	/// </summary>
	public void set_navigation_visibility(bool showBackButton, bool showQuitButton)
	{
		SetNavigationVisibility(showBackButton, showQuitButton);
	}

	private void CacheNodeReferences()
	{
		_studioRow = GetNodeOrNull<BoxContainer>($"{StudioRootPath}/StudioRow");
		_settingsPanel = GetNodeOrNull<Control>($"{StudioRootPath}/StudioRow/SettingsPanel");
		_rulesPanel = GetNodeOrNull<Control>($"{StudioRootPath}/StudioRow/RulesPanel");
		_summaryPanel = GetNodeOrNull<Control>($"{StudioRootPath}/StudioRow/SummaryPanel");
		_versionLabel = GetNodeOrNull<Label>($"{HeroRootPath}/HeaderRow/VersionLabel");
		_summaryLabel = GetNodeOrNull<Label>($"{SummaryRootPath}/SummaryScroll/SummaryContent/SummaryLabel");
		_assumptionsLabel = GetNodeOrNull<Label>($"{SummaryRootPath}/SummaryScroll/SummaryContent/AssumptionsLabel");
		_issuesContainer = GetNodeOrNull<VBoxContainer>($"{SummaryRootPath}/SummaryScroll/SummaryContent/IssuesContainer");
		_startButton = GetNodeOrNull<Button>($"{SummaryRootPath}/Buttons/StartButton");
		_loadButton = GetNodeOrNull<Button>($"{SummaryRootPath}/Buttons/LoadButton");
		_quitButton = GetNodeOrNull<Button>($"{SummaryRootPath}/Buttons/QuitButton");
		_backButton = GetNodeOrNull<Button>($"{HeroRootPath}/HeaderRow/BackButton");
		_seedContainer = GetNodeOrNull<HBoxContainer>($"{ParameterRootPath}/SeedContainer");
		_randomizeButton = GetNodeOrNull<Button>($"{ParameterRootPath}/SeedContainer/RandomizeButton");
		_presetOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/PresetContainer/PresetOption");
		_typeOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/TypeSection/TypeContent/TypeVBox/TypeRow/TypeOption");
		_armsRow = GetNodeOrNull<HBoxContainer>($"{ParameterRootPath}/TypeSection/TypeContent/TypeVBox/ArmsRow");
		_armsSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/TypeSection/TypeContent/TypeVBox/ArmsRow/ArmsSlider");
		_armsValue = GetNodeOrNull<Label>($"{ParameterRootPath}/TypeSection/TypeContent/TypeVBox/ArmsRow/ArmsValue");
		_pitchSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/PitchRow/PitchSlider");
		_pitchValue = GetNodeOrNull<Label>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/PitchRow/PitchValue");
		_amplitudeSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/AmplitudeRow/AmplitudeSlider");
		_amplitudeValue = GetNodeOrNull<Label>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/AmplitudeRow/AmplitudeValue");
		_bulgeIntensitySlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/BulgeIntensityRow/BulgeIntensitySlider");
		_bulgeIntensityValue = GetNodeOrNull<Label>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/BulgeIntensityRow/BulgeIntensityValue");
		_bulgeRadiusSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/BulgeRadiusRow/BulgeRadiusSlider");
		_bulgeRadiusValue = GetNodeOrNull<Label>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/BulgeRadiusRow/BulgeRadiusValue");
		_ellipticityRow = GetNodeOrNull<HBoxContainer>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/EllipticityRow");
		_ellipticitySlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/EllipticityRow/EllipticitySlider");
		_ellipticityValue = GetNodeOrNull<Label>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/EllipticityRow/EllipticityValue");
		_irregularityRow = GetNodeOrNull<HBoxContainer>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/IrregularityRow");
		_irregularitySlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/IrregularityRow/IrregularitySlider");
		_irregularityValue = GetNodeOrNull<Label>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/IrregularityRow/IrregularityValue");
		_radiusSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/RadiusRow/RadiusSlider");
		_radiusValue = GetNodeOrNull<Label>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/RadiusRow/RadiusValue");
		_diskLengthSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/DiskLengthRow/DiskLengthSlider");
		_diskLengthValue = GetNodeOrNull<Label>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/DiskLengthRow/DiskLengthValue");
		_diskHeightSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/DiskHeightRow/DiskHeightSlider");
		_diskHeightValue = GetNodeOrNull<Label>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/DiskHeightRow/DiskHeightValue");
		_densitySlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/DensityRow/DensitySlider");
		_densityValue = GetNodeOrNull<Label>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/DensityRow/DensityValue");
		_seedSpin = GetNodeOrNull<SpinBox>($"{ParameterRootPath}/SeedContainer/SeedSpin");
		_settingsVBox = GetNodeOrNull<VBoxContainer>(ParameterRootPath);
		_rulesVBox = GetNodeOrNull<VBoxContainer>(RulesRootPath);
		_rulesetModeOption = GetNodeOrNull<OptionButton>($"{RulesRootPath}/UseCaseSection/RulesetRow/RulesetModeOption");
		_showTravellerReadoutsCheck = GetNodeOrNull<BaseButton>($"{RulesRootPath}/UseCaseSection/ShowTravellerReadoutsCheck");
		_forceLifeOnSupportableWorldsCheck = GetNodeOrNull<BaseButton>($"{RulesRootPath}/UseCaseSection/ForceLifeOnSupportableWorldsCheck");
		_mainworldPolicyRow = GetNodeOrNull<HBoxContainer>($"{RulesRootPath}/UseCaseSection/MainworldPolicyRow");
		_mainworldPolicyOption = GetNodeOrNull<OptionButton>($"{RulesRootPath}/UseCaseSection/MainworldPolicyRow/MainworldPolicyOption");
		_temperateWorldBiasRow = GetNodeOrNull<HBoxContainer>($"{RulesRootPath}/UseCaseSection/TemperateWorldBiasRow");
		_temperateWorldBiasSlider = GetNodeOrNull<HSlider>($"{RulesRootPath}/UseCaseSection/TemperateWorldBiasRow/TemperateWorldBiasSlider");
		_temperateWorldBiasValue = GetNodeOrNull<Label>($"{RulesRootPath}/UseCaseSection/TemperateWorldBiasRow/TemperateWorldBiasValue");
		_harshWorldBiasRow = GetNodeOrNull<HBoxContainer>($"{RulesRootPath}/UseCaseSection/HarshWorldBiasRow");
		_harshWorldBiasSlider = GetNodeOrNull<HSlider>($"{RulesRootPath}/UseCaseSection/HarshWorldBiasRow/HarshWorldBiasSlider");
		_harshWorldBiasValue = GetNodeOrNull<Label>($"{RulesRootPath}/UseCaseSection/HarshWorldBiasRow/HarshWorldBiasValue");
		_terrestrialWorldBiasRow = GetNodeOrNull<HBoxContainer>($"{RulesRootPath}/UseCaseSection/TerrestrialWorldBiasRow");
		_terrestrialWorldBiasSlider = GetNodeOrNull<HSlider>($"{RulesRootPath}/UseCaseSection/TerrestrialWorldBiasRow/TerrestrialWorldBiasSlider");
		_terrestrialWorldBiasValue = GetNodeOrNull<Label>($"{RulesRootPath}/UseCaseSection/TerrestrialWorldBiasRow/TerrestrialWorldBiasValue");
		_nativeLifeBiasRow = GetNodeOrNull<HBoxContainer>($"{RulesRootPath}/UseCaseSection/NativeLifeBiasRow");
		_nativeLifeBiasSlider = GetNodeOrNull<HSlider>($"{RulesRootPath}/UseCaseSection/NativeLifeBiasRow/NativeLifeBiasSlider");
		_nativeLifeBiasValue = GetNodeOrNull<Label>($"{RulesRootPath}/UseCaseSection/NativeLifeBiasRow/NativeLifeBiasValue");
		_advancedAssumptionsInfoButton = GetNodeOrNull<Button>($"{RulesRootPath}/UseCaseSection/AdvancedHeaderRow/AdvancedAssumptionsInfoButton");
		_lifeFrameworkOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/LifeFrameworkRow/LifeFrameworkOption");
		_abiogenesisModelOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/AbiogenesisModelRow/AbiogenesisModelOption");
		_complexLifeModelOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/ComplexLifeModelRow/ComplexLifeModelOption");
		_civilizationModelOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/CivilizationModelRow/CivilizationModelOption");
		_environmentalWindowWeightOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/EnvironmentalWindowWeightRow/EnvironmentalWindowWeightOption");
		_populationPermissivenessRow = GetNodeOrNull<HBoxContainer>($"{RulesRootPath}/UseCaseSection/PopulationRow");
		_populationPermissivenessInput = GetNodeOrNull<HSlider>($"{RulesRootPath}/UseCaseSection/PopulationRow/PopulationPermissivenessInput");
		_populationPermissivenessValueLabel = GetNodeOrNull<Label>($"{RulesRootPath}/UseCaseSection/PopulationPermissivenessValue");
		CacheScienceNodeReferences();
	}

	private void ConnectSignals()
	{
		if (_startButton != null) _startButton.Pressed += OnStartPressed;
		if (_backButton != null) _backButton.Pressed += OnBackPressed;
		if (_quitButton != null) _quitButton.Pressed += OnQuitPressed;
		if (_randomizeButton != null) _randomizeButton.Pressed += OnRandomizePressed;
		if (_presetOption != null) _presetOption.ItemSelected += OnPresetSelected;
		if (_typeOption != null) _typeOption.ItemSelected += OnTypeChanged;
		ConnectSlider(_armsSlider, OnArmsChanged);
		ConnectSlider(_pitchSlider, OnPitchChanged);
		ConnectSlider(_amplitudeSlider, OnAmplitudeChanged);
		ConnectSlider(_bulgeIntensitySlider, OnBulgeIntensityChanged);
		ConnectSlider(_bulgeRadiusSlider, OnBulgeRadiusChanged);
		ConnectSlider(_ellipticitySlider, OnEllipticityChanged);
		ConnectSlider(_irregularitySlider, OnIrregularityChanged);
		ConnectSlider(_radiusSlider, OnRadiusChanged);
		ConnectSlider(_diskLengthSlider, OnDiskLengthChanged);
		ConnectSlider(_diskHeightSlider, OnDiskHeightChanged);
		ConnectSlider(_densitySlider, OnDensityChanged);
		if (_seedSpin != null) _seedSpin.ValueChanged += _ => RefreshValidationIssues();
		if (_rulesetModeOption != null) _rulesetModeOption.ItemSelected += OnRulesetModeSelected;
		if (_showTravellerReadoutsCheck != null) _showTravellerReadoutsCheck.Toggled += _ => RefreshValidationIssues();
		if (_forceLifeOnSupportableWorldsCheck != null) _forceLifeOnSupportableWorldsCheck.Toggled += _ => RefreshValidationIssues();
		if (_mainworldPolicyOption != null) _mainworldPolicyOption.ItemSelected += _ => RefreshValidationIssues();
		ConnectSlider(_temperateWorldBiasSlider, OnCompatibilityPressureChanged);
		ConnectSlider(_harshWorldBiasSlider, OnCompatibilityPressureChanged);
		ConnectSlider(_terrestrialWorldBiasSlider, OnCompatibilityPressureChanged);
		ConnectSlider(_nativeLifeBiasSlider, OnCompatibilityPressureChanged);
		ConnectSlider(_populationPermissivenessInput, OnCompatibilityPressureChanged);
		if (_lifeFrameworkOption != null) _lifeFrameworkOption.ItemSelected += _ => OnLifeModelChanged();
		if (_abiogenesisModelOption != null) _abiogenesisModelOption.ItemSelected += _ => OnLifeModelChanged();
		if (_complexLifeModelOption != null) _complexLifeModelOption.ItemSelected += _ => OnLifeModelChanged();
		if (_civilizationModelOption != null) _civilizationModelOption.ItemSelected += _ => OnLifeModelChanged();
		if (_environmentalWindowWeightOption != null) _environmentalWindowWeightOption.ItemSelected += _ => OnLifeModelChanged();
		ConnectScienceSignals();
	}

	private void ApplyLayoutPolish()
	{
		if (_summaryLabel != null)
		{
			_summaryLabel.Visible = true;
		}

		if (_assumptionsLabel != null)
		{
			_assumptionsLabel.Visible = true;
		}

		if (_settingsVBox != null)
		{
			ApplyRowSpacing(_settingsVBox);
		}

		if (_rulesVBox != null)
		{
			ApplyRowSpacing(_rulesVBox);
		}

		if (_populationPermissivenessRow != null)
		{
			_populationPermissivenessRow.Visible = false;
		}

		if (_populationPermissivenessValueLabel != null)
		{
			_populationPermissivenessValueLabel.Visible = false;
		}

		HideSpaceOperaOverrideRows();

		if (_loadButton != null)
		{
			_loadButton.Visible = false;
		}

		ApplyScienceLayoutPolish();

		VBoxContainer? buttonsContainer = GetNodeOrNull<VBoxContainer>($"{SummaryRootPath}/Buttons");
		if (buttonsContainer != null)
		{
			foreach (Node child in buttonsContainer.GetChildren())
			{
				if (child is Button typedButton)
				{
					typedButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				}
			}
		}
	}

	private static void ApplyRowSpacing(Node root)
	{
		foreach (Node child in root.GetChildren())
		{
			if (child is HBoxContainer row)
			{
				row.AddThemeConstantOverride("separation", 12);
			}

			ApplyRowSpacing(child);
		}
	}

	private static void ConnectSlider(Godot.Range? slider, Action<double> callback)
	{
		if (slider != null) slider.ValueChanged += value => callback(value);
	}

	private void ApplyVersionLabel()
	{
		if (_versionLabel != null)
		{
			string version = UserFacingVersionHelper.GetDisplayVersion();
			_versionLabel.Text = $"Version {version}";
		}
	}

	private void ApplyResponsiveLayout()
	{
		StudioScreenLayoutHelper.ApplyResponsiveStudioLayout(
			this,
			_studioRow,
			_settingsPanel,
			_rulesPanel,
			_summaryPanel,
			GalaxyStudioCompactBreakpoint);
	}

	private void UpdateTypeSpecificControls()
	{
		int galaxyType = _typeOption?.Selected ?? (int)GalaxySpec.GalaxyType.Spiral;
		if (_armsRow != null) _armsRow.Visible = galaxyType == (int)GalaxySpec.GalaxyType.Spiral;
		if (_ellipticityRow != null) _ellipticityRow.Visible = galaxyType == (int)GalaxySpec.GalaxyType.Elliptical;
		if (_irregularityRow != null) _irregularityRow.Visible = galaxyType == (int)GalaxySpec.GalaxyType.Irregular;
		UpdateScienceTypeSpecificControls(galaxyType);
	}

	private void UpdateAllValueLabels()
	{
		UpdateIntLabel(_armsValue, _armsSlider, string.Empty);
		UpdateFloatLabel(_pitchValue, _pitchSlider, "0.0", " deg");
		UpdateFloatLabel(_amplitudeValue, _amplitudeSlider, "0.00", string.Empty);
		UpdateFloatLabel(_bulgeIntensityValue, _bulgeIntensitySlider, "0.00", string.Empty);
		UpdateIntLabel(_bulgeRadiusValue, _bulgeRadiusSlider, " pc");
		UpdateFloatLabel(_ellipticityValue, _ellipticitySlider, "0.00", string.Empty);
		UpdateFloatLabel(_irregularityValue, _irregularitySlider, "0.00", string.Empty);
		UpdateIntLabel(_radiusValue, _radiusSlider, " pc");
		UpdateIntLabel(_diskLengthValue, _diskLengthSlider, " pc");
		UpdateIntLabel(_diskHeightValue, _diskHeightSlider, " pc");
		UpdateFloatLabel(_densityValue, _densitySlider, "0.0", "x");
		UpdateScienceValueLabels();
	}

	private void ApplyPreset(int preset)
	{
		_isUpdatingUi = true;
		ApplyConfig(BuildPresetConfig((Preset)preset));
		UpdateTypeSpecificControls();
		UpdateAllValueLabels();
		_isUpdatingUi = false;
		RefreshValidationIssues();
	}

	private static GalaxyConfig BuildPresetConfig(Preset preset)
	{
		GalaxyConfig config = GalaxyConfig.CreateDefault();
		ApplyScientificPresetValues(preset, config);
		if (preset == Preset.Andromeda)
		{
			config.Type = GalaxySpec.GalaxyType.Spiral;
			config.NumArms = 2;
			config.ArmPitchAngleDeg = 20.0;
			config.ArmAmplitude = 0.55;
			config.BulgeIntensity = 1.0;
			config.BulgeRadiusPc = 2200.0;
			config.RadiusPc = 22000.0;
			config.DiskScaleLengthPc = 5500.0;
			config.DiskScaleHeightPc = 400.0;
			config.StarDensityMultiplier = 1.2;
			return config;
		}

		if (preset == Preset.Whirlpool)
		{
			config.Type = GalaxySpec.GalaxyType.Spiral;
			config.NumArms = 2;
			config.ArmPitchAngleDeg = 18.0;
			config.ArmAmplitude = 0.85;
			config.BulgeIntensity = 0.6;
			config.BulgeRadiusPc = 1200.0;
			config.RadiusPc = 12000.0;
			config.DiskScaleLengthPc = 3500.0;
			config.DiskScaleHeightPc = 250.0;
			return config;
		}

		if (preset == Preset.Sombrero)
		{
			config.Type = GalaxySpec.GalaxyType.Lenticular;
			config.BulgeIntensity = 1.2;
			config.BulgeRadiusPc = 2500.0;
			config.Ellipticity = 0.6;
			config.RadiusPc = 15000.0;
			config.DiskScaleLengthPc = 4000.0;
			config.DiskScaleHeightPc = 450.0;
			config.StarDensityMultiplier = 1.3;
			return config;
		}

		if (preset == Preset.LargeMagellanicCloud)
		{
			config.Type = GalaxySpec.GalaxyType.Irregular;
			config.BulgeIntensity = 0.4;
			config.BulgeRadiusPc = 1000.0;
			config.IrregularityScale = 0.7;
			config.RadiusPc = 10000.0;
			config.DiskScaleLengthPc = 2500.0;
			config.DiskScaleHeightPc = 350.0;
			config.StarDensityMultiplier = 0.7;
		}

		return config;
	}

	private void MarkAsCustom()
	{
		if (!_isUpdatingUi && _presetOption != null && _presetOption.Selected != (int)Preset.Custom)
		{
			_presetOption.Select((int)Preset.Custom);
		}
	}

	private int GenerateRandomSeed()
	{
		if (_seededRng != null) return _seededRng.RandiRange(1, 999999);
		if (_seededRngObject != null && _seededRngObject.HasMethod("randi"))
		{
			Variant rawVariant = _seededRngObject.Call("randi");
			long rawValue = rawVariant.VariantType switch
			{
				Variant.Type.Int => (int)rawVariant,
				Variant.Type.Float => (long)(double)rawVariant,
				_ => 1L,
			};
			long capped = Math.Abs(rawValue) % 1000000L;
			if (capped == 0L)
			{
				return 1;
			}

			return (int)capped;
		}

		return 12345;
	}

	private void SetType(int typeValue)
	{
		if (_typeOption != null) _typeOption.Select(typeValue);
	}

	private static void SetSlider(Godot.Range? slider, double value)
	{
		if (slider != null) slider.Value = value;
	}

	private static void UpdateIntLabel(Label? label, Godot.Range? slider, string suffix)
	{
		if (label != null && slider != null) label.Text = $"{(int)slider.Value}{suffix}";
	}

	private static void UpdateFloatLabel(Label? label, Godot.Range? slider, string format, string suffix)
	{
		if (label != null && slider != null) label.Text = $"{slider.Value.ToString(format)}{suffix}";
	}

	private void OnStartPressed()
	{
		GalaxyConfig config = GetCurrentConfig();
		RefreshValidationIssues();
		if (_currentIssues.HasErrors())
		{
			return;
		}

		int seedValue;
		if (!_showSeedControls)
		{
			seedValue = GenerateRandomSeed();
			if (_seedSpin != null)
			{
				_seedSpin.Value = seedValue;
			}
		}
		else if (_seedSpin == null)
		{
			seedValue = GenerateRandomSeed();
		}
		else
		{
			seedValue = (int)_seedSpin.Value;
		}
		EmitSignal("start_new_galaxy", config, seedValue);
	}

	private void OnBackPressed() => EmitSignal("back_requested");
	private void OnQuitPressed() => EmitSignal("quit_requested");

	private void OnRandomizePressed()
	{
		if (_seedSpin != null) _seedSpin.Value = GenerateRandomSeed();
		RefreshValidationIssues();
	}

	private void OnPresetSelected(long index) => ApplyPreset((int)index);

	private void OnTypeChanged(long _index)
	{
		UpdateTypeSpecificControls();
		MarkAsCustom();
		RefreshValidationIssues();
	}

	private void OnArmsChanged(double _value) { UpdateIntLabel(_armsValue, _armsSlider, string.Empty); MarkAsCustom(); RefreshValidationIssues(); }
	private void OnPitchChanged(double _value) { UpdateFloatLabel(_pitchValue, _pitchSlider, "0.0", " deg"); MarkAsCustom(); RefreshValidationIssues(); }
	private void OnAmplitudeChanged(double _value) { UpdateFloatLabel(_amplitudeValue, _amplitudeSlider, "0.00", string.Empty); MarkAsCustom(); RefreshValidationIssues(); }
	private void OnBulgeIntensityChanged(double _value) { UpdateFloatLabel(_bulgeIntensityValue, _bulgeIntensitySlider, "0.00", string.Empty); MarkAsCustom(); RefreshValidationIssues(); }
	private void OnBulgeRadiusChanged(double _value) { UpdateIntLabel(_bulgeRadiusValue, _bulgeRadiusSlider, " pc"); MarkAsCustom(); RefreshValidationIssues(); }
	private void OnEllipticityChanged(double _value) { UpdateFloatLabel(_ellipticityValue, _ellipticitySlider, "0.00", string.Empty); MarkAsCustom(); RefreshValidationIssues(); }
	private void OnIrregularityChanged(double _value) { UpdateFloatLabel(_irregularityValue, _irregularitySlider, "0.00", string.Empty); MarkAsCustom(); RefreshValidationIssues(); }
	private void OnRadiusChanged(double _value) { UpdateIntLabel(_radiusValue, _radiusSlider, " pc"); MarkAsCustom(); RefreshValidationIssues(); }
	private void OnDiskLengthChanged(double _value) { UpdateIntLabel(_diskLengthValue, _diskLengthSlider, " pc"); MarkAsCustom(); RefreshValidationIssues(); }
	private void OnDiskHeightChanged(double _value) { UpdateIntLabel(_diskHeightValue, _diskHeightSlider, " pc"); MarkAsCustom(); RefreshValidationIssues(); }
	private void OnDensityChanged(double _value) { UpdateFloatLabel(_densityValue, _densitySlider, "0.0", "x"); MarkAsCustom(); RefreshValidationIssues(); }

	private void ApplyParameterTooltips()
	{
		if (_rulesetModeOption != null && _rulesetModeOption.ItemCount <= 2)
		{
			GenerationUseCasePresentation.PopulateRulesetOptions(_rulesetModeOption);
		}

		ApplyTooltip("galaxy_type", _typeOption, $"{ParameterRootPath}/TypeSection/TypeContent/TypeVBox/TypeRow/TypeLabel");
		ApplyTooltip("num_arms", _armsSlider, $"{ParameterRootPath}/TypeSection/TypeContent/TypeVBox/ArmsRow/ArmsLabel");
		ApplyTooltip("arm_pitch_angle_deg", _pitchSlider, $"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/PitchRow/PitchLabel");
		ApplyTooltip("arm_amplitude", _amplitudeSlider, $"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/AmplitudeRow/AmplitudeLabel");
		ApplyTooltip("bulge_intensity", _bulgeIntensitySlider, $"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/BulgeIntensityRow/BulgeIntensityLabel");
		ApplyTooltip("bulge_radius_pc", _bulgeRadiusSlider, $"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/BulgeRadiusRow/BulgeRadiusLabel");
		ApplyTooltip("ellipticity", _ellipticitySlider, $"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/EllipticityRow/EllipticityLabel");
		ApplyTooltip("irregularity_scale", _irregularitySlider, $"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/IrregularityRow/IrregularityLabel");
		ApplyTooltip("radius_pc", _radiusSlider, $"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/RadiusRow/RadiusLabel");
		ApplyTooltip("disk_scale_length_pc", _diskLengthSlider, $"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/DiskLengthRow/DiskLengthLabel");
		ApplyTooltip("disk_scale_height_pc", _diskHeightSlider, $"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/DiskHeightRow/DiskHeightLabel");
		ApplyTooltip("star_density_multiplier", _densitySlider, $"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/DensityRow/DensityLabel");
		ApplyTooltip("galaxy_seed", _seedSpin, $"{ParameterRootPath}/SeedContainer/SeedLabel");
		ApplyDynamicTooltip(_rulesetModeOption, "ruleset_mode");
		ApplyDynamicTooltip(_showTravellerReadoutsCheck, "show_traveller_readouts");
		if (_forceLifeOnSupportableWorldsCheck != null)
		{
			_forceLifeOnSupportableWorldsCheck.TooltipText = "Generation override, not a scientific model.\nWhen enabled, supportable worlds keep native life instead of losing it to the later life-roll.\nWorlds that fail the biology support gate still stay lifeless.";
		}
		ApplyTooltip("life_framework", _lifeFrameworkOption, $"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/LifeFrameworkRow/LifeFrameworkLabel");
		ApplyTooltip("abiogenesis_model", _abiogenesisModelOption, $"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/AbiogenesisModelRow/AbiogenesisModelLabel");
		ApplyTooltip("complex_life_model", _complexLifeModelOption, $"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/ComplexLifeModelRow/ComplexLifeModelLabel");
		ApplyTooltip("civilization_model", _civilizationModelOption, $"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/CivilizationModelRow/CivilizationModelLabel");
		ApplyTooltip("environmental_window_weight", _environmentalWindowWeightOption, $"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/EnvironmentalWindowWeightRow/EnvironmentalWindowWeightLabel");

		if (_advancedAssumptionsInfoButton != null)
		{
			_advancedAssumptionsInfoButton.TooltipText = PermissivenessScaleHelper.GetAdvancedLegendTooltip();
		}

		if (_mainworldPolicyOption != null)
		{
			_mainworldPolicyOption.TooltipText = "This tells Space Opera generation whether it should ignore, prefer, or require a strong mainworld candidate.\nRequire makes the generator push harder for one clearly playable focal world.";
		}

		ApplyCompatibilityTooltip(_temperateWorldBiasSlider, _temperateWorldBiasValue, "Higher values fill more good temperate slots with worlds.\nLower values leave more of those slots empty.");
		ApplyCompatibilityTooltip(_harshWorldBiasSlider, _harshWorldBiasValue, "Higher values allow more hot, cold, and otherwise harsh worlds to survive slot filling.\nLower values prune harsh slots more aggressively.");
		ApplyCompatibilityTooltip(_terrestrialWorldBiasSlider, _terrestrialWorldBiasValue, "Higher values favor rocky and super-Earth mainworld candidates over mini-Neptunes and giants.\nLower values relax that bias.");
		ApplyCompatibilityTooltip(_nativeLifeBiasSlider, _nativeLifeBiasValue, "Higher values make supportable worlds more likely to keep native life.\nLower values make life rarer even when the world can support it.");
		ApplyCompatibilityTooltip(_populationPermissivenessInput, _populationPermissivenessValueLabel, "Higher values make colonies and inhabited outposts more common.\nLower values make settled worlds sparser.");

		ApplyScienceParameterTooltips();
	}

	private void ApplyTooltip(string parameterId, Control? inputControl, string labelPath)
	{
		string tooltip = GetParameterAssumption(parameterId);
		if (inputControl != null)
		{
			inputControl.TooltipText = tooltip;
		}

		Label? label = GetNodeOrNull<Label>(labelPath);
		if (label != null)
		{
			label.TooltipText = tooltip;
		}
	}

	private string GetParameterAssumption(string parameterId)
	{
		foreach (GenerationParameterDefinition definition in GenerationParameterCatalog.GetGalaxyDefinitions())
		{
			if (definition.Id == parameterId)
			{
				return definition.AssumptionText;
			}
		}

		return string.Empty;
	}

	private void ApplyDynamicTooltip(Control? control, string parameterId)
	{
		if (control == null)
		{
			return;
		}

		control.TooltipText = GetParameterAssumption(parameterId);
	}

	private void ApplyConfig(GalaxyConfig config)
	{
		SetType((int)config.Type);
		SetSlider(_armsSlider, config.NumArms);
		SetSlider(_pitchSlider, config.ArmPitchAngleDeg);
		SetSlider(_amplitudeSlider, config.ArmAmplitude);
		SetSlider(_bulgeIntensitySlider, config.BulgeIntensity);
		SetSlider(_bulgeRadiusSlider, config.BulgeRadiusPc);
		SetSlider(_ellipticitySlider, config.Ellipticity);
		SetSlider(_irregularitySlider, config.IrregularityScale);
		SetSlider(_radiusSlider, config.RadiusPc);
		SetSlider(_diskLengthSlider, config.DiskScaleLengthPc);
		SetSlider(_diskHeightSlider, config.DiskScaleHeightPc);
		SetSlider(_densitySlider, config.StarDensityMultiplier);
		ApplyScienceConfig(config);
		ApplyUseCaseSettingsToControls(config.UseCaseSettings);
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

		if (_forceLifeOnSupportableWorldsCheck != null)
		{
			settings.ForceLifeOnSupportableWorlds = _forceLifeOnSupportableWorldsCheck.ButtonPressed;
		}

		if (_lifeFrameworkOption != null)
		{
			settings.LifeFramework = (GenerationUseCaseSettings.LifeFrameworkType)_lifeFrameworkOption.GetSelectedId();
			settings.LifePermissiveness = GenerationUseCaseSettings.GetRecommendedLifePermissiveness(settings.LifeFramework);
		}

		if (_abiogenesisModelOption != null)
		{
			settings.AbiogenesisModel = (GenerationUseCaseSettings.AbiogenesisModelType)_abiogenesisModelOption.GetSelectedId();
		}

		if (_complexLifeModelOption != null)
		{
			settings.ComplexLifeModel = (GenerationUseCaseSettings.ComplexLifeModelType)_complexLifeModelOption.GetSelectedId();
		}

		if (_civilizationModelOption != null)
		{
			settings.CivilizationModel = (GenerationUseCaseSettings.CivilizationModelType)_civilizationModelOption.GetSelectedId();
		}

		if (_environmentalWindowWeightOption != null)
		{
			settings.EnvironmentalWindowWeight = (GenerationUseCaseSettings.EnvironmentalWindowWeightType)_environmentalWindowWeightOption.GetSelectedId();
		}

		if (_mainworldPolicyOption != null)
		{
			settings.MainworldPolicy = (GenerationUseCaseSettings.MainworldPolicyType)_mainworldPolicyOption.GetSelectedId();
		}

		if (_temperateWorldBiasSlider != null)
		{
			settings.CompatibilityTemperateSlotFillMultiplier = _temperateWorldBiasSlider.Value;
		}

		if (_harshWorldBiasSlider != null)
		{
			settings.CompatibilityHarshSlotFillMultiplier = _harshWorldBiasSlider.Value;
		}

		if (_terrestrialWorldBiasSlider != null)
		{
			settings.CompatibilityTerrestrialWorldWeightMultiplier = _terrestrialWorldBiasSlider.Value;
		}

		if (_nativeLifeBiasSlider != null)
		{
			settings.CompatibilityNativeLifeProbabilityMultiplier = _nativeLifeBiasSlider.Value;
		}

		if (_populationPermissivenessInput != null)
		{
			settings.CompatibilityColonyProbabilityMultiplier = _populationPermissivenessInput.Value;
		}

		if (settings.GetCompatibilityProfile().UsesUwpLikeReadouts)
		{
			settings.ShowTravellerReadouts = true;
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

		if (_forceLifeOnSupportableWorldsCheck != null)
		{
			_forceLifeOnSupportableWorldsCheck.ButtonPressed = resolvedSettings.ForceLifeOnSupportableWorlds;
		}

		if (_lifeFrameworkOption != null)
		{
			SetOptionSelection(_lifeFrameworkOption, (int)resolvedSettings.LifeFramework);
		}

		if (_abiogenesisModelOption != null)
		{
			SetOptionSelection(_abiogenesisModelOption, (int)resolvedSettings.AbiogenesisModel);
		}

		if (_complexLifeModelOption != null)
		{
			SetOptionSelection(_complexLifeModelOption, (int)resolvedSettings.ComplexLifeModel);
		}

		if (_civilizationModelOption != null)
		{
			SetOptionSelection(_civilizationModelOption, (int)resolvedSettings.CivilizationModel);
		}

		if (_environmentalWindowWeightOption != null)
		{
			SetOptionSelection(_environmentalWindowWeightOption, (int)resolvedSettings.EnvironmentalWindowWeight);
		}

		if (_mainworldPolicyOption != null)
		{
			SetOptionSelection(_mainworldPolicyOption, (int)resolvedSettings.MainworldPolicy);
		}

		SetCompatibilitySliderValue(_temperateWorldBiasSlider, _temperateWorldBiasValue, resolvedSettings.CompatibilityTemperateSlotFillMultiplier);
		SetCompatibilitySliderValue(_harshWorldBiasSlider, _harshWorldBiasValue, resolvedSettings.CompatibilityHarshSlotFillMultiplier);
		SetCompatibilitySliderValue(_terrestrialWorldBiasSlider, _terrestrialWorldBiasValue, resolvedSettings.CompatibilityTerrestrialWorldWeightMultiplier);
		SetCompatibilitySliderValue(_nativeLifeBiasSlider, _nativeLifeBiasValue, resolvedSettings.CompatibilityNativeLifeProbabilityMultiplier);
		SetCompatibilitySliderValue(_populationPermissivenessInput, _populationPermissivenessValueLabel, resolvedSettings.CompatibilityColonyProbabilityMultiplier);
		UpdateCompatibilityOverrideVisibility(resolvedSettings);
	}

	private void OnRulesetModeSelected(long selectedId)
	{
		if (_isUpdatingUi)
		{
			RefreshValidationIssues();
			return;
		}

		GenerationUseCaseSettings rulesetDefaults = GenerationUseCaseSettings.CreateDefault();
		rulesetDefaults.RulesetMode = (GenerationUseCaseSettings.RulesetModeType)selectedId;
		rulesetDefaults.ApplyRulesetDefaults();
		RpgCompatibilityProfile compatibilityProfile = rulesetDefaults.GetCompatibilityProfile();
		if (compatibilityProfile.IsActive)
		{
			if (_showTravellerReadoutsCheck != null)
			{
				_showTravellerReadoutsCheck.ButtonPressed = compatibilityProfile.UsesUwpLikeReadouts;
			}

			ApplyRulesetDefaultsToControls(rulesetDefaults);
		}
		else
		{
			UpdateCompatibilityOverrideVisibility(rulesetDefaults);
		}

		RefreshValidationIssues();
	}

	private void RefreshSummary()
	{
		GalaxyConfig config = GetCurrentConfig();
		GenerationUseCaseSettings settings = config.UseCaseSettings;

		if (_summaryLabel != null)
		{
			System.Collections.Generic.List<string> lines = new();
			lines.Add(BuildScienceSummary(config));
			lines.Add($"Structure Radius {config.RadiusPc / 1000.0:0.0} kpc | Disk {config.DiskScaleLengthPc:0}/{config.DiskScaleHeightPc:0} pc | Density {config.StarDensityMultiplier:0.0}x");
			lines.Add($"Ruleset {GenerationUseCasePresentation.GetRulesetLabel(settings.RulesetMode)}");
			string readoutVisibility = "Hidden";
			if (settings.ShowTravellerReadouts)
			{
				readoutVisibility = "Visible";
			}

			lines.Add($"UWP Code {readoutVisibility}");
			lines.Add($"Life Framework: {LifeScienceReferenceCatalog.GetFrameworkLabel(settings.LifeFramework)}");
			lines.Add($"Abiogenesis: {LifeScienceReferenceCatalog.GetAbiogenesisLabel(settings.AbiogenesisModel)} | Complex Life: {LifeScienceReferenceCatalog.GetComplexLifeLabel(settings.ComplexLifeModel)}");
			lines.Add($"Civilization: {LifeScienceReferenceCatalog.GetCivilizationLabel(settings.CivilizationModel)} | Window Weight: {LifeScienceReferenceCatalog.GetEnvironmentalWindowWeightLabel(settings.EnvironmentalWindowWeight)}");
			lines.Add($"Force Life On Supportable Worlds: {(settings.ForceLifeOnSupportableWorlds ? "On" : "Off")}");
			if (settings.RulesetMode == GenerationUseCaseSettings.RulesetModeType.Traveller)
			{
				lines.Add($"Space Opera Overrides: Mainworld {settings.MainworldPolicy} | Temperate {settings.CompatibilityTemperateSlotFillMultiplier:0.00}x | Harsh {settings.CompatibilityHarshSlotFillMultiplier:0.00}x");
				lines.Add($"Mainworld Class {settings.CompatibilityTerrestrialWorldWeightMultiplier:0.00}x | Native Life {settings.CompatibilityNativeLifeProbabilityMultiplier:0.00}x | Settlements {settings.CompatibilityColonyProbabilityMultiplier:0.00}x");
			}
			if (_showSeedControls && _seedSpin != null)
			{
				lines.Add($"Seed {(int)_seedSpin.Value}");
			}

			_summaryLabel.Text = string.Join("\n", lines);
		}

		if (_assumptionsLabel != null)
		{
			ApplyScienceAssumptionSummary();
		}
	}

	private void ApplyTravellerDefaultsToControls()
	{
		GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
		settings.RulesetMode = GenerationUseCaseSettings.RulesetModeType.Traveller;
		settings.ApplyRulesetDefaults();
		ApplyRulesetDefaultsToControls(settings);
	}

	private void ApplyRulesetDefaultsToControls(GenerationUseCaseSettings settings)
	{
		if (_lifeFrameworkOption != null)
		{
			SetOptionSelection(_lifeFrameworkOption, (int)settings.LifeFramework);
		}

		if (_abiogenesisModelOption != null)
		{
			SetOptionSelection(_abiogenesisModelOption, (int)settings.AbiogenesisModel);
		}

		if (_complexLifeModelOption != null)
		{
			SetOptionSelection(_complexLifeModelOption, (int)settings.ComplexLifeModel);
		}

		if (_civilizationModelOption != null)
		{
			SetOptionSelection(_civilizationModelOption, (int)settings.CivilizationModel);
		}

		if (_environmentalWindowWeightOption != null)
		{
			SetOptionSelection(_environmentalWindowWeightOption, (int)settings.EnvironmentalWindowWeight);
		}

		if (_forceLifeOnSupportableWorldsCheck != null)
		{
			_forceLifeOnSupportableWorldsCheck.ButtonPressed = settings.ForceLifeOnSupportableWorlds;
		}

		if (_mainworldPolicyOption != null)
		{
			SetOptionSelection(_mainworldPolicyOption, (int)settings.MainworldPolicy);
		}

		SetCompatibilitySliderValue(_temperateWorldBiasSlider, _temperateWorldBiasValue, settings.CompatibilityTemperateSlotFillMultiplier);
		SetCompatibilitySliderValue(_harshWorldBiasSlider, _harshWorldBiasValue, settings.CompatibilityHarshSlotFillMultiplier);
		SetCompatibilitySliderValue(_terrestrialWorldBiasSlider, _terrestrialWorldBiasValue, settings.CompatibilityTerrestrialWorldWeightMultiplier);
		SetCompatibilitySliderValue(_nativeLifeBiasSlider, _nativeLifeBiasValue, settings.CompatibilityNativeLifeProbabilityMultiplier);
		SetCompatibilitySliderValue(_populationPermissivenessInput, _populationPermissivenessValueLabel, settings.CompatibilityColonyProbabilityMultiplier);
		UpdateCompatibilityOverrideVisibility(settings);
	}

	private void OnLifeModelChanged()
	{
		MarkAsCustom();
		RefreshValidationIssues();
	}

	private void OnCompatibilityPressureChanged(double _value)
	{
		UpdateCompatibilityValueLabels();
		RefreshValidationIssues();
	}

	private void UpdateCompatibilityOverrideVisibility(GenerationUseCaseSettings settings)
	{
		bool showSpaceOperaOverrides = settings.RulesetMode == GenerationUseCaseSettings.RulesetModeType.Traveller;
		SetRowVisible(_mainworldPolicyRow, showSpaceOperaOverrides);
		SetRowVisible(_temperateWorldBiasRow, showSpaceOperaOverrides);
		SetRowVisible(_harshWorldBiasRow, showSpaceOperaOverrides);
		SetRowVisible(_terrestrialWorldBiasRow, showSpaceOperaOverrides);
		SetRowVisible(_nativeLifeBiasRow, showSpaceOperaOverrides);
		SetRowVisible(_populationPermissivenessRow, showSpaceOperaOverrides);
		if (_populationPermissivenessValueLabel != null)
		{
			_populationPermissivenessValueLabel.Visible = showSpaceOperaOverrides;
		}

		UpdateCompatibilityValueLabels();
	}

	private void UpdateCompatibilityValueLabels()
	{
		UpdateCompatibilityValueLabel(_temperateWorldBiasSlider, _temperateWorldBiasValue);
		UpdateCompatibilityValueLabel(_harshWorldBiasSlider, _harshWorldBiasValue);
		UpdateCompatibilityValueLabel(_terrestrialWorldBiasSlider, _terrestrialWorldBiasValue);
		UpdateCompatibilityValueLabel(_nativeLifeBiasSlider, _nativeLifeBiasValue);
		UpdateCompatibilityValueLabel(_populationPermissivenessInput, _populationPermissivenessValueLabel);
	}

	private static void UpdateCompatibilityValueLabel(Godot.Range? slider, Label? label)
	{
		if (slider == null || label == null)
		{
			return;
		}

		label.Text = $"{slider.Value:0.00}x";
	}

	private static void SetCompatibilitySliderValue(Godot.Range? slider, Label? label, double value)
	{
		if (slider != null)
		{
			slider.Value = value;
		}

		UpdateCompatibilityValueLabel(slider, label);
	}

	private static void SetRowVisible(Control? row, bool isVisible)
	{
		if (row != null)
		{
			row.Visible = isVisible;
		}
	}

	private void HideSpaceOperaOverrideRows()
	{
		SetRowVisible(_mainworldPolicyRow, false);
		SetRowVisible(_temperateWorldBiasRow, false);
		SetRowVisible(_harshWorldBiasRow, false);
		SetRowVisible(_terrestrialWorldBiasRow, false);
		SetRowVisible(_nativeLifeBiasRow, false);
		SetRowVisible(_populationPermissivenessRow, false);
	}

	private static void ApplyCompatibilityTooltip(Control? inputControl, Control? valueLabel, string tooltip)
	{
		if (inputControl != null)
		{
			inputControl.TooltipText = tooltip;
		}

		if (valueLabel != null)
		{
			valueLabel.TooltipText = tooltip;
		}
	}

	private void RefreshValidationIssues()
	{
		RefreshSummary();

		int seedValue = 1;
		if (_seedSpin != null)
		{
			seedValue = (int)_seedSpin.Value;
		}

		_currentIssues = GalaxyGenerationParameterValidator.Validate(seedValue, GetCurrentConfig());
		if (_issuesContainer == null)
		{
			return;
		}

		foreach (Node child in _issuesContainer.GetChildren())
		{
			child.QueueFree();
		}

		if (_currentIssues.Issues.Count == 0)
		{
			Label cleanLabel = UiSceneTemplates.InstantiateMessageLabel();
			cleanLabel.Text = "No parameter issues.";
			cleanLabel.Modulate = new Color(0.55f, 0.75f, 0.55f, 1.0f);
			_issuesContainer.AddChild(cleanLabel);
			return;
		}

		foreach (GenerationParameterIssue issue in _currentIssues.Issues)
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

			_issuesContainer.AddChild(issueLabel);
		}
	}
}
