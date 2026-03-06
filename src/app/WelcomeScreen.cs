using System;
using Godot;
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

    /// <summary>
    /// Initializes UI wiring.
    /// </summary>
    public override void _Ready()
    {
        CacheNodeReferences();
        ConnectSignals();
        UpdateSectionVisibility();
        UpdateTypeSpecificControls();
        UpdateAllValueLabels();
        RefreshRandomSeedDisplay();
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
        switch ((Preset)preset)
        {
            case Preset.Custom: ApplyDefaultValues(); break;
            case Preset.MilkyWay: ApplyDefaultValues(); break;
            case Preset.Andromeda: ApplyAndromedaPreset(); break;
            case Preset.Whirlpool: ApplyWhirlpoolPreset(); break;
            case Preset.Sombrero: ApplySombreroPreset(); break;
            case Preset.LargeMagellanicCloud: ApplyLargeMagellanicCloudPreset(); break;
        }
        UpdateTypeSpecificControls();
        UpdateAllValueLabels();
        _isUpdatingUi = false;
    }

    private void ApplyDefaultValues()
    {
        SetType((int)GalaxySpec.GalaxyType.Spiral);
        SetSlider(_armsSlider, 4.0);
        SetSlider(_pitchSlider, 14.0);
        SetSlider(_amplitudeSlider, 0.65);
        SetSlider(_bulgeIntensitySlider, 0.8);
        SetSlider(_bulgeRadiusSlider, 1500.0);
        SetSlider(_ellipticitySlider, 0.3);
        SetSlider(_irregularitySlider, 0.5);
        SetSlider(_radiusSlider, 15000.0);
        SetSlider(_diskLengthSlider, 4000.0);
        SetSlider(_diskHeightSlider, 300.0);
        SetSlider(_densitySlider, 1.0);
    }

    private void ApplyAndromedaPreset()
    {
        SetType((int)GalaxySpec.GalaxyType.Spiral);
        SetSlider(_armsSlider, 2.0);
        SetSlider(_pitchSlider, 20.0);
        SetSlider(_amplitudeSlider, 0.55);
        SetSlider(_bulgeIntensitySlider, 1.0);
        SetSlider(_bulgeRadiusSlider, 2200.0);
        SetSlider(_radiusSlider, 22000.0);
        SetSlider(_diskLengthSlider, 5500.0);
        SetSlider(_diskHeightSlider, 400.0);
        SetSlider(_densitySlider, 1.2);
    }

    private void ApplyWhirlpoolPreset()
    {
        SetType((int)GalaxySpec.GalaxyType.Spiral);
        SetSlider(_armsSlider, 2.0);
        SetSlider(_pitchSlider, 18.0);
        SetSlider(_amplitudeSlider, 0.85);
        SetSlider(_bulgeIntensitySlider, 0.6);
        SetSlider(_bulgeRadiusSlider, 1200.0);
        SetSlider(_radiusSlider, 12000.0);
        SetSlider(_diskLengthSlider, 3500.0);
        SetSlider(_diskHeightSlider, 250.0);
        SetSlider(_densitySlider, 1.0);
    }

    private void ApplySombreroPreset()
    {
        SetType((int)GalaxySpec.GalaxyType.Elliptical);
        SetSlider(_bulgeIntensitySlider, 1.2);
        SetSlider(_bulgeRadiusSlider, 2500.0);
        SetSlider(_ellipticitySlider, 0.6);
        SetSlider(_radiusSlider, 15000.0);
        SetSlider(_diskLengthSlider, 4000.0);
        SetSlider(_diskHeightSlider, 450.0);
        SetSlider(_densitySlider, 1.3);
    }

    private void ApplyLargeMagellanicCloudPreset()
    {
        SetType((int)GalaxySpec.GalaxyType.Irregular);
        SetSlider(_bulgeIntensitySlider, 0.4);
        SetSlider(_bulgeRadiusSlider, 1000.0);
        SetSlider(_irregularitySlider, 0.7);
        SetSlider(_radiusSlider, 10000.0);
        SetSlider(_diskLengthSlider, 2500.0);
        SetSlider(_diskHeightSlider, 350.0);
        SetSlider(_densitySlider, 0.7);
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
        if (!config.IsValid())
        {
            GD.PushError("WelcomeScreen: config invalid, using default");
            config = GalaxyConfig.CreateDefault();
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
    }

    private void OnArmsChanged(double _value) { UpdateIntLabel(_armsValue, _armsSlider, string.Empty); MarkAsCustom(); }
    private void OnPitchChanged(double _value) { UpdateFloatLabel(_pitchValue, _pitchSlider, "0.0", " deg"); MarkAsCustom(); }
    private void OnAmplitudeChanged(double _value) { UpdateFloatLabel(_amplitudeValue, _amplitudeSlider, "0.00", string.Empty); MarkAsCustom(); }
    private void OnBulgeIntensityChanged(double _value) { UpdateFloatLabel(_bulgeIntensityValue, _bulgeIntensitySlider, "0.00", string.Empty); MarkAsCustom(); }
    private void OnBulgeRadiusChanged(double _value) { UpdateIntLabel(_bulgeRadiusValue, _bulgeRadiusSlider, " pc"); MarkAsCustom(); }
    private void OnEllipticityChanged(double _value) { UpdateFloatLabel(_ellipticityValue, _ellipticitySlider, "0.00", string.Empty); MarkAsCustom(); }
    private void OnIrregularityChanged(double _value) { UpdateFloatLabel(_irregularityValue, _irregularitySlider, "0.00", string.Empty); MarkAsCustom(); }
    private void OnRadiusChanged(double _value) { UpdateIntLabel(_radiusValue, _radiusSlider, " pc"); MarkAsCustom(); }
    private void OnDiskLengthChanged(double _value) { UpdateIntLabel(_diskLengthValue, _diskLengthSlider, " pc"); MarkAsCustom(); }
    private void OnDiskHeightChanged(double _value) { UpdateIntLabel(_diskHeightValue, _diskHeightSlider, " pc"); MarkAsCustom(); }
    private void OnDensityChanged(double _value) { UpdateFloatLabel(_densityValue, _densitySlider, "0.0", "x"); MarkAsCustom(); }
}
