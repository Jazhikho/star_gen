using System;
using Godot;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Rng;

namespace StarGen.App;

/// <summary>
/// Startup screen for galaxy generation with presets and exposed tuning controls.
/// </summary>
public partial class WelcomeScreen : Control
{
    [Signal]
    public delegate void start_new_galaxyEventHandler(GalaxyConfig config, int seedValue);

    [Signal]
    public delegate void load_galaxy_requestedEventHandler();

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

    private const string ArrowCollapsed = ">";
    private const string ArrowExpanded = "v";

    private SeededRng? _seededRng;
    private GodotObject? _seededRngObject;
    private bool _isUpdatingUi;
    private bool _typeSectionExpanded = true;
    private bool _structureSectionExpanded;
    private bool _sizeSectionExpanded;

    private Button? _startButton;
    private Button? _loadButton;
    private Button? _backButton;
    private Button? _quitButton;
    private Button? _randomizeButton;
    private OptionButton? _presetOption;
    private Button? _typeHeader;
    private MarginContainer? _typeContent;
    private OptionButton? _typeOption;
    private HBoxContainer? _armsRow;
    private HSlider? _armsSlider;
    private Label? _armsValue;
    private Button? _structureHeader;
    private MarginContainer? _structureContent;
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
    private Button? _sizeHeader;
    private MarginContainer? _sizeContent;
    private HSlider? _radiusSlider;
    private Label? _radiusValue;
    private HSlider? _diskLengthSlider;
    private Label? _diskLengthValue;
    private HSlider? _diskHeightSlider;
    private Label? _diskHeightValue;
    private HSlider? _densitySlider;
    private Label? _densityValue;
    private SpinBox? _seedSpin;
    private OptionButton? _rulesetModeOption;
    private CheckBox? _showTravellerReadoutsCheck;
    private SpinBox? _lifePermissivenessInput;
    private SpinBox? _populationPermissivenessInput;
    private OptionButton? _mainworldPolicyOption;
    private VBoxContainer? _settingsVBox;
    private Label? _assumptionsLabel;
    private VBoxContainer? _issuesContainer;
    private GenerationParameterIssueSet _currentIssues = new();

    /// <summary>
    /// Initializes UI wiring.
    /// </summary>
    public override void _Ready()
    {
        CacheNodeReferences();
        ApplyLayoutPolish();
        BuildParameterSupportUi();
        ConnectSignals();
        ApplyParameterTooltips();
        UpdateSectionVisibility();
        UpdateTypeSpecificControls();
        UpdateAllValueLabels();
        RefreshRandomSeedDisplay();
        RefreshValidationIssues();
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
        return new GalaxyConfig
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
    }

    /// <summary>
    /// GDScript-compatible config getter wrapper.
    /// </summary>
    public GalaxyConfig get_current_config()
    {
        return GetCurrentConfig();
    }

    /// <summary>
    /// Applies a typed galaxy configuration to the welcome-screen controls.
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
        _startButton = GetNodeOrNull<Button>("CenterContainer/MainPanel/MarginContainer/VBox/Buttons/StartButton");
        _loadButton = GetNodeOrNull<Button>("CenterContainer/MainPanel/MarginContainer/VBox/Buttons/LoadButton");
        _backButton = GetNodeOrNull<Button>("CenterContainer/MainPanel/MarginContainer/VBox/HeaderRow/BackButton");
        _quitButton = GetNodeOrNull<Button>("CenterContainer/MainPanel/MarginContainer/VBox/Buttons/QuitButton");
        _randomizeButton = GetNodeOrNull<Button>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SeedContainer/RandomizeButton");
        _presetOption = GetNodeOrNull<OptionButton>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/PresetContainer/PresetOption");
        _typeHeader = GetNodeOrNull<Button>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeHeader");
        _typeContent = GetNodeOrNull<MarginContainer>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeContent");
        _typeOption = GetNodeOrNull<OptionButton>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeContent/TypeVBox/TypeRow/TypeOption");
        _armsRow = GetNodeOrNull<HBoxContainer>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeContent/TypeVBox/ArmsRow");
        _armsSlider = GetNodeOrNull<HSlider>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeContent/TypeVBox/ArmsRow/ArmsSlider");
        _armsValue = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeContent/TypeVBox/ArmsRow/ArmsValue");
        _structureHeader = GetNodeOrNull<Button>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureHeader");
        _structureContent = GetNodeOrNull<MarginContainer>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent");
        _pitchSlider = GetNodeOrNull<HSlider>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/PitchRow/PitchSlider");
        _pitchValue = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/PitchRow/PitchValue");
        _amplitudeSlider = GetNodeOrNull<HSlider>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/AmplitudeRow/AmplitudeSlider");
        _amplitudeValue = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/AmplitudeRow/AmplitudeValue");
        _bulgeIntensitySlider = GetNodeOrNull<HSlider>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/BulgeIntensityRow/BulgeIntensitySlider");
        _bulgeIntensityValue = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/BulgeIntensityRow/BulgeIntensityValue");
        _bulgeRadiusSlider = GetNodeOrNull<HSlider>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/BulgeRadiusRow/BulgeRadiusSlider");
        _bulgeRadiusValue = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/BulgeRadiusRow/BulgeRadiusValue");
        _ellipticityRow = GetNodeOrNull<HBoxContainer>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/EllipticityRow");
        _ellipticitySlider = GetNodeOrNull<HSlider>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/EllipticityRow/EllipticitySlider");
        _ellipticityValue = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/EllipticityRow/EllipticityValue");
        _irregularityRow = GetNodeOrNull<HBoxContainer>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/IrregularityRow");
        _irregularitySlider = GetNodeOrNull<HSlider>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/IrregularityRow/IrregularitySlider");
        _irregularityValue = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/IrregularityRow/IrregularityValue");
        _sizeHeader = GetNodeOrNull<Button>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeHeader");
        _sizeContent = GetNodeOrNull<MarginContainer>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent");
        _radiusSlider = GetNodeOrNull<HSlider>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/RadiusRow/RadiusSlider");
        _radiusValue = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/RadiusRow/RadiusValue");
        _diskLengthSlider = GetNodeOrNull<HSlider>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DiskLengthRow/DiskLengthSlider");
        _diskLengthValue = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DiskLengthRow/DiskLengthValue");
        _diskHeightSlider = GetNodeOrNull<HSlider>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DiskHeightRow/DiskHeightSlider");
        _diskHeightValue = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DiskHeightRow/DiskHeightValue");
        _densitySlider = GetNodeOrNull<HSlider>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DensityRow/DensitySlider");
        _densityValue = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DensityRow/DensityValue");
        _seedSpin = GetNodeOrNull<SpinBox>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SeedContainer/SeedSpin");
        _settingsVBox = GetNodeOrNull<VBoxContainer>("CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox");
    }

    private void ConnectSignals()
    {
        if (_startButton != null) _startButton.Pressed += OnStartPressed;
        if (_loadButton != null) _loadButton.Pressed += OnLoadPressed;
        if (_backButton != null) _backButton.Pressed += OnBackPressed;
        if (_quitButton != null) _quitButton.Pressed += OnQuitPressed;
        if (_randomizeButton != null) _randomizeButton.Pressed += OnRandomizePressed;
        if (_presetOption != null) _presetOption.ItemSelected += OnPresetSelected;
        if (_typeHeader != null) _typeHeader.Pressed += OnTypeHeaderPressed;
        if (_structureHeader != null) _structureHeader.Pressed += OnStructureHeaderPressed;
        if (_sizeHeader != null) _sizeHeader.Pressed += OnSizeHeaderPressed;
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
        if (_lifePermissivenessInput != null) _lifePermissivenessInput.ValueChanged += _ => RefreshValidationIssues();
        if (_populationPermissivenessInput != null) _populationPermissivenessInput.ValueChanged += _ => RefreshValidationIssues();
        if (_mainworldPolicyOption != null) _mainworldPolicyOption.ItemSelected += _ => RefreshValidationIssues();
    }

    private void ApplyLayoutPolish()
    {
        if (_settingsVBox != null)
        {
            ApplyRowSpacing(_settingsVBox);
        }

        VBoxContainer? buttonsContainer = GetNodeOrNull<VBoxContainer>("CenterContainer/MainPanel/MarginContainer/VBox/Buttons");
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

    private void UpdateSectionVisibility()
    {
        if (_typeContent != null) _typeContent.Visible = _typeSectionExpanded;
        if (_typeHeader != null)
        {
            string arrow;
            if (_typeSectionExpanded)
            {
                arrow = ArrowExpanded;
            }
            else
            {
                arrow = ArrowCollapsed;
            }

            _typeHeader.Text = $"{arrow}  Galaxy Type";
        }
        if (_structureContent != null) _structureContent.Visible = _structureSectionExpanded;
        if (_structureHeader != null)
        {
            string arrow;
            if (_structureSectionExpanded)
            {
                arrow = ArrowExpanded;
            }
            else
            {
                arrow = ArrowCollapsed;
            }

            _structureHeader.Text = $"{arrow}  Structure";
        }
        if (_sizeContent != null) _sizeContent.Visible = _sizeSectionExpanded;
        if (_sizeHeader != null)
        {
            string arrow;
            if (_sizeSectionExpanded)
            {
                arrow = ArrowExpanded;
            }
            else
            {
                arrow = ArrowCollapsed;
            }

            _sizeHeader.Text = $"{arrow}  Size & Density";
        }
    }

    private void UpdateTypeSpecificControls()
    {
        int galaxyType = _typeOption?.Selected ?? (int)GalaxySpec.GalaxyType.Spiral;
        if (_armsRow != null) _armsRow.Visible = galaxyType == (int)GalaxySpec.GalaxyType.Spiral;
        if (_ellipticityRow != null) _ellipticityRow.Visible = galaxyType == (int)GalaxySpec.GalaxyType.Elliptical;
        if (_irregularityRow != null) _irregularityRow.Visible = galaxyType == (int)GalaxySpec.GalaxyType.Irregular;
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
            config.Type = GalaxySpec.GalaxyType.Elliptical;
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
            GD.PushError("WelcomeScreen: galaxy parameters contain blocking errors");
            return;
        }

        int seedValue;
        if (_seedSpin == null)
        {
            seedValue = GenerateRandomSeed();
        }
        else
        {
            seedValue = (int)_seedSpin.Value;
        }
        if (seedValue == 0) seedValue = GenerateRandomSeed();
        EmitSignal("start_new_galaxy", config, seedValue);
    }

    private void OnLoadPressed() => EmitSignal("load_galaxy_requested");
    private void OnBackPressed() => EmitSignal("back_requested");
    private void OnQuitPressed() => EmitSignal("quit_requested");

    private void OnRandomizePressed()
    {
        if (_seedSpin != null) _seedSpin.Value = GenerateRandomSeed();
        RefreshValidationIssues();
    }

    private void OnPresetSelected(long index) => ApplyPreset((int)index);

    private void OnTypeHeaderPressed()
    {
        _typeSectionExpanded = !_typeSectionExpanded;
        UpdateSectionVisibility();
    }

    private void OnStructureHeaderPressed()
    {
        _structureSectionExpanded = !_structureSectionExpanded;
        UpdateSectionVisibility();
    }

    private void OnSizeHeaderPressed()
    {
        _sizeSectionExpanded = !_sizeSectionExpanded;
        UpdateSectionVisibility();
    }

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

    private void BuildParameterSupportUi()
    {
        if (_settingsVBox == null || _issuesContainer != null)
        {
            return;
        }

        VBoxContainer useCaseSection = new VBoxContainer();
        useCaseSection.Name = "UseCaseSection";
        useCaseSection.AddThemeConstantOverride("separation", 8);

        Label useCaseHeader = new Label();
        useCaseHeader.Text = "Traveller / Ruleset";
        useCaseHeader.AddThemeFontSizeOverride("font_size", 12);
        useCaseHeader.Modulate = new Color(0.82f, 0.82f, 0.55f, 1.0f);
        useCaseSection.AddChild(useCaseHeader);

        HBoxContainer rulesetRow = new HBoxContainer();
        rulesetRow.AddThemeConstantOverride("separation", 12);
        Label rulesetLabel = new Label();
        rulesetLabel.Text = "Ruleset";
        rulesetLabel.CustomMinimumSize = new Vector2(110.0f, 0.0f);
        rulesetRow.AddChild(rulesetLabel);
        OptionButton rulesetModeOption = new OptionButton();
        rulesetModeOption.Name = "RulesetModeOption";
        rulesetModeOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        rulesetModeOption.AddItem("Default", (int)GenerationUseCaseSettings.RulesetModeType.Default);
        rulesetModeOption.AddItem("Traveller", (int)GenerationUseCaseSettings.RulesetModeType.Traveller);
        rulesetRow.AddChild(rulesetModeOption);
        _rulesetModeOption = rulesetModeOption;
        useCaseSection.AddChild(rulesetRow);

        CheckBox showTravellerReadoutsCheck = new CheckBox();
        showTravellerReadoutsCheck.Name = "ShowTravellerReadoutsCheck";
        showTravellerReadoutsCheck.Text = "Show Traveller / UWP Readouts";
        _showTravellerReadoutsCheck = showTravellerReadoutsCheck;
        useCaseSection.AddChild(showTravellerReadoutsCheck);

        Label advancedHeader = new Label();
        advancedHeader.Text = "Advanced Assumptions";
        advancedHeader.AddThemeFontSizeOverride("font_size", 12);
        advancedHeader.Modulate = new Color(0.82f, 0.82f, 0.55f, 1.0f);
        useCaseSection.AddChild(advancedHeader);

        HBoxContainer lifeRow = new HBoxContainer();
        lifeRow.AddThemeConstantOverride("separation", 12);
        Label lifeLabel = new Label();
        lifeLabel.Text = "Life Bias";
        lifeLabel.CustomMinimumSize = new Vector2(110.0f, 0.0f);
        lifeRow.AddChild(lifeLabel);
        SpinBox lifePermissivenessInput = new SpinBox();
        lifePermissivenessInput.Name = "LifePermissivenessInput";
        lifePermissivenessInput.MinValue = 0.0;
        lifePermissivenessInput.MaxValue = 1.0;
        lifePermissivenessInput.Step = 0.05;
        lifePermissivenessInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        lifeRow.AddChild(lifePermissivenessInput);
        _lifePermissivenessInput = lifePermissivenessInput;
        useCaseSection.AddChild(lifeRow);

        HBoxContainer populationRow = new HBoxContainer();
        populationRow.AddThemeConstantOverride("separation", 12);
        Label populationLabel = new Label();
        populationLabel.Text = "Pop. Bias";
        populationLabel.CustomMinimumSize = new Vector2(110.0f, 0.0f);
        populationRow.AddChild(populationLabel);
        SpinBox populationPermissivenessInput = new SpinBox();
        populationPermissivenessInput.Name = "PopulationPermissivenessInput";
        populationPermissivenessInput.MinValue = 0.0;
        populationPermissivenessInput.MaxValue = 1.0;
        populationPermissivenessInput.Step = 0.05;
        populationPermissivenessInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        populationRow.AddChild(populationPermissivenessInput);
        _populationPermissivenessInput = populationPermissivenessInput;
        useCaseSection.AddChild(populationRow);

        HBoxContainer mainworldRow = new HBoxContainer();
        mainworldRow.AddThemeConstantOverride("separation", 12);
        Label mainworldLabel = new Label();
        mainworldLabel.Text = "Mainworld";
        mainworldLabel.CustomMinimumSize = new Vector2(110.0f, 0.0f);
        mainworldRow.AddChild(mainworldLabel);
        OptionButton mainworldPolicyOption = new OptionButton();
        mainworldPolicyOption.Name = "MainworldPolicyOption";
        mainworldPolicyOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        mainworldPolicyOption.AddItem("None", (int)GenerationUseCaseSettings.MainworldPolicyType.None);
        mainworldPolicyOption.AddItem("Prefer", (int)GenerationUseCaseSettings.MainworldPolicyType.Prefer);
        mainworldPolicyOption.AddItem("Require", (int)GenerationUseCaseSettings.MainworldPolicyType.Require);
        mainworldRow.AddChild(mainworldPolicyOption);
        _mainworldPolicyOption = mainworldPolicyOption;
        useCaseSection.AddChild(mainworldRow);

        _settingsVBox.AddChild(useCaseSection);

        Label assumptionsLabel = new Label();
        assumptionsLabel.Name = "AssumptionsLabel";
        assumptionsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        assumptionsLabel.AddThemeFontSizeOverride("font_size", 10);
        assumptionsLabel.Modulate = new Color(0.65f, 0.72f, 0.8f, 1.0f);
        assumptionsLabel.Text = "The startup editor uses the same normalized GalaxyConfig and validation rules as the in-viewer galaxy inspector, including Traveller-oriented ruleset metadata.";
        _settingsVBox.AddChild(assumptionsLabel);
        _assumptionsLabel = assumptionsLabel;

        VBoxContainer issuesContainer = new VBoxContainer();
        issuesContainer.Name = "IssuesContainer";
        issuesContainer.AddThemeConstantOverride("separation", 2);
        _settingsVBox.AddChild(issuesContainer);
        _issuesContainer = issuesContainer;

        ApplyUseCaseSettingsToControls(GenerationUseCaseSettings.CreateDefault());
    }

    private void ApplyParameterTooltips()
    {
        ApplyTooltip("galaxy_type", _typeOption, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeContent/TypeVBox/TypeRow/TypeLabel");
        ApplyTooltip("num_arms", _armsSlider, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/TypeSection/TypeContent/TypeVBox/ArmsRow/ArmsLabel");
        ApplyTooltip("arm_pitch_angle_deg", _pitchSlider, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/PitchRow/PitchLabel");
        ApplyTooltip("arm_amplitude", _amplitudeSlider, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/AmplitudeRow/AmplitudeLabel");
        ApplyTooltip("bulge_intensity", _bulgeIntensitySlider, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/BulgeIntensityRow/BulgeIntensityLabel");
        ApplyTooltip("bulge_radius_pc", _bulgeRadiusSlider, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/BulgeRadiusRow/BulgeRadiusLabel");
        ApplyTooltip("ellipticity", _ellipticitySlider, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/EllipticityRow/EllipticityLabel");
        ApplyTooltip("irregularity_scale", _irregularitySlider, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/StructureSection/StructureContent/StructureVBox/IrregularityRow/IrregularityLabel");
        ApplyTooltip("radius_pc", _radiusSlider, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/RadiusRow/RadiusLabel");
        ApplyTooltip("disk_scale_length_pc", _diskLengthSlider, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DiskLengthRow/DiskLengthLabel");
        ApplyTooltip("disk_scale_height_pc", _diskHeightSlider, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DiskHeightRow/DiskHeightLabel");
        ApplyTooltip("star_density_multiplier", _densitySlider, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SizeSection/SizeContent/SizeVBox/DensityRow/DensityLabel");
        ApplyTooltip("galaxy_seed", _seedSpin, "CenterContainer/MainPanel/MarginContainer/VBox/ScrollContainer/SettingsVBox/SeedContainer/SeedLabel");
        ApplyDynamicTooltip(_rulesetModeOption, "ruleset_mode");
        ApplyDynamicTooltip(_showTravellerReadoutsCheck, "show_traveller_readouts");
        ApplyDynamicTooltip(_lifePermissivenessInput, "life_permissiveness");
        ApplyDynamicTooltip(_populationPermissivenessInput, "population_permissiveness");
        ApplyDynamicTooltip(_mainworldPolicyOption, "mainworld_policy");
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
        ApplyUseCaseSettingsToControls(config.UseCaseSettings);
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

        if (_populationPermissivenessInput != null)
        {
            settings.PopulationPermissiveness = _populationPermissivenessInput.Value;
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

        if (_populationPermissivenessInput != null)
        {
            _populationPermissivenessInput.Value = resolvedSettings.PopulationPermissiveness;
        }

        if (_mainworldPolicyOption != null)
        {
            _mainworldPolicyOption.Select((int)resolvedSettings.MainworldPolicy);
        }
    }

    private void OnRulesetModeSelected(long selectedId)
    {
        if (_isUpdatingUi)
        {
            RefreshValidationIssues();
            return;
        }

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

        RefreshValidationIssues();
    }

    private void RefreshValidationIssues()
    {
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
            Label cleanLabel = new Label();
            cleanLabel.Text = "No parameter issues.";
            cleanLabel.AddThemeFontSizeOverride("font_size", 10);
            cleanLabel.Modulate = new Color(0.55f, 0.75f, 0.55f, 1.0f);
            _issuesContainer.AddChild(cleanLabel);
            return;
        }

        foreach (GenerationParameterIssue issue in _currentIssues.Issues)
        {
            Label issueLabel = new Label();
            issueLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            issueLabel.AddThemeFontSizeOverride("font_size", 10);
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
