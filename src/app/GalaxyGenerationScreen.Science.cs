using System.Collections.Generic;
using Godot;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;
using StarGen.Domain.Galaxy;
using StarGen.App.Shared;

namespace StarGen.App;

/// <summary>
/// Scientific galaxy-parameter and stellar-profile wiring for the galaxy-generation studio.
/// </summary>
public partial class GalaxyGenerationScreen
{
    private OptionButton? _subtypeModeOption;
    private OptionButton? _barModeOption;
    private OptionButton? _armMechanismOption;
    private HSlider? _haloMassSlider;
    private Label? _haloMassValue;
    private HSlider? _environmentSlider;
    private Label? _environmentValue;
    private HSlider? _starFormationEfficiencySlider;
    private Label? _starFormationEfficiencyValue;
    private HBoxContainer? _pitchRow;
    private HBoxContainer? _amplitudeRow;
    private HBoxContainer? _diskLengthRow;
    private HBoxContainer? _diskHeightRow;
    private HSlider? _ghzInnerSlider;
    private Label? _ghzInnerValue;
    private HSlider? _ghzOuterSlider;
    private Label? _ghzOuterValue;
    private HSlider? _ghzWidthSlider;
    private Label? _ghzWidthValue;
    private HSlider? _metallicityGradientSlider;
    private Label? _metallicityGradientValue;
    private OptionButton? _stellarImfFormOption;
    private OptionButton? _stellarImfVariationModeOption;
    private OptionButton? _stellarIsochroneModelOption;
    private HSlider? _stellarMultiplicityScaleSlider;
    private Label? _stellarMultiplicityScaleValue;
    private OptionButton? _planetMassRadiusModelOption;
    private OptionButton? _planetEnvelopeLossModelOption;
    private OptionButton? _planetHabitableZoneModelOption;
    private OptionButton? _planetGasGiantFormationModelOption;
    private OptionButton? _planetMetallicityCouplingOption;
    private OptionButton? _planetRogueAllowanceOption;
    private OptionButton? _planetMoonFormationBiasOption;
    private OptionButton? _planetMinorBodyOuterBiasOption;
    private Button? _helpButton;
    private Window? _helpDialog;
    private RichTextLabel? _helpDialogText;
    private Button? _helpDialogCloseButton;
    private Button? _typeSourcesButton;
    private Button? _scienceSourcesButton;
    private Button? _structureSourcesButton;
    private Button? _sizeSourcesButton;
    private Button? _lifeSourcesButton;
    private Button? _stellarSourcesButton;
    private Button? _planetarySourcesButton;

    private void CacheScienceNodeReferences()
    {
        _subtypeModeOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/TypeSection/TypeContent/TypeVBox/SubtypeModeRow/SubtypeModeOption");
        _barModeOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/TypeSection/TypeContent/TypeVBox/BarModeRow/BarModeOption");
        _armMechanismOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/TypeSection/TypeContent/TypeVBox/ArmMechanismRow/ArmMechanismOption");
        _haloMassSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/ScienceSection/ScienceContent/ScienceVBox/HaloMassRow/HaloMassSlider");
        _haloMassValue = GetNodeOrNull<Label>($"{ParameterRootPath}/ScienceSection/ScienceContent/ScienceVBox/HaloMassRow/HaloMassValue");
        _environmentSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/ScienceSection/ScienceContent/ScienceVBox/EnvironmentRow/EnvironmentSlider");
        _environmentValue = GetNodeOrNull<Label>($"{ParameterRootPath}/ScienceSection/ScienceContent/ScienceVBox/EnvironmentRow/EnvironmentValue");
        _starFormationEfficiencySlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/ScienceSection/ScienceContent/ScienceVBox/StarFormationEfficiencyRow/StarFormationEfficiencySlider");
        _starFormationEfficiencyValue = GetNodeOrNull<Label>($"{ParameterRootPath}/ScienceSection/ScienceContent/ScienceVBox/StarFormationEfficiencyRow/StarFormationEfficiencyValue");
        _pitchRow = GetNodeOrNull<HBoxContainer>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/PitchRow");
        _amplitudeRow = GetNodeOrNull<HBoxContainer>($"{ParameterRootPath}/StructureSection/StructureContent/StructureVBox/AmplitudeRow");
        _diskLengthRow = GetNodeOrNull<HBoxContainer>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/DiskLengthRow");
        _diskHeightRow = GetNodeOrNull<HBoxContainer>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/DiskHeightRow");
        _ghzInnerSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/GhzInnerRow/GhzInnerSlider");
        _ghzInnerValue = GetNodeOrNull<Label>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/GhzInnerRow/GhzInnerValue");
        _ghzOuterSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/GhzOuterRow/GhzOuterSlider");
        _ghzOuterValue = GetNodeOrNull<Label>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/GhzOuterRow/GhzOuterValue");
        _ghzWidthSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/GhzWidthRow/GhzWidthSlider");
        _ghzWidthValue = GetNodeOrNull<Label>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/GhzWidthRow/GhzWidthValue");
        _metallicityGradientSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/MetallicityGradientRow/MetallicityGradientSlider");
        _metallicityGradientValue = GetNodeOrNull<Label>($"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/MetallicityGradientRow/MetallicityGradientValue");
        _stellarImfFormOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/StellarSection/StellarContent/StellarVBox/ImfFormRow/ImfFormOption");
        _stellarImfVariationModeOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/StellarSection/StellarContent/StellarVBox/ImfVariationRow/ImfVariationOption");
        _stellarIsochroneModelOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/StellarSection/StellarContent/StellarVBox/IsochroneRow/IsochroneOption");
        _stellarMultiplicityScaleSlider = GetNodeOrNull<HSlider>($"{ParameterRootPath}/StellarSection/StellarContent/StellarVBox/MultiplicityRow/MultiplicitySlider");
        _stellarMultiplicityScaleValue = GetNodeOrNull<Label>($"{ParameterRootPath}/StellarSection/StellarContent/StellarVBox/MultiplicityRow/MultiplicityValue");
        _planetMassRadiusModelOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/MassRadiusRow/MassRadiusOption");
        _planetEnvelopeLossModelOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/EnvelopeLossRow/EnvelopeLossOption");
        _planetHabitableZoneModelOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/HabitableZoneRow/HabitableZoneOption");
        _planetGasGiantFormationModelOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/GasGiantFormationRow/GasGiantFormationOption");
        _planetMetallicityCouplingOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/MetallicityCouplingRow/MetallicityCouplingOption");
        _planetRogueAllowanceOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/RogueAllowanceRow/RogueAllowanceOption");
        _planetMoonFormationBiasOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/MoonFormationBiasRow/MoonFormationBiasOption");
        _planetMinorBodyOuterBiasOption = GetNodeOrNull<OptionButton>($"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/OuterBodyBiasRow/OuterBodyBiasOption");
        _helpButton = GetNodeOrNull<Button>($"{HeroRootPath}/HeaderRow/HelpButton");
        _helpDialog = GetNodeOrNull<Window>("HelpDialog");
        _helpDialogText = GetNodeOrNull<RichTextLabel>("HelpDialog/MarginContainer/HelpVBox/HelpCard/MarginContainer/HelpDialogText");
        _helpDialogCloseButton = GetNodeOrNull<Button>("HelpDialog/MarginContainer/HelpVBox/ButtonRow/CloseButton");
        _typeSourcesButton = GetNodeOrNull<Button>($"{ParameterRootPath}/TypeSection/TypeHeaderRow/TypeSourcesButton");
        _scienceSourcesButton = GetNodeOrNull<Button>($"{ParameterRootPath}/ScienceSection/ScienceHeaderRow/ScienceSourcesButton");
        _structureSourcesButton = GetNodeOrNull<Button>($"{ParameterRootPath}/StructureSection/StructureHeaderRow/StructureSourcesButton");
        _sizeSourcesButton = GetNodeOrNull<Button>($"{ParameterRootPath}/SizeSection/SizeHeaderRow/SizeSourcesButton");
        _lifeSourcesButton = GetNodeOrNull<Button>($"{ParameterRootPath}/LifeSection/LifeHeaderRow/LifeSourcesButton");
        _stellarSourcesButton = GetNodeOrNull<Button>($"{ParameterRootPath}/StellarSection/StellarHeaderRow/StellarSourcesButton");
        _planetarySourcesButton = GetNodeOrNull<Button>($"{ParameterRootPath}/PlanetarySection/PlanetaryHeaderRow/PlanetarySourcesButton");
    }

    private void ConnectScienceSignals()
    {
        if (_subtypeModeOption != null) _subtypeModeOption.ItemSelected += _ => OnScienceControlChanged();
        if (_barModeOption != null) _barModeOption.ItemSelected += _ => OnScienceControlChanged();
        if (_armMechanismOption != null) _armMechanismOption.ItemSelected += _ => OnScienceControlChanged();
        if (_stellarImfFormOption != null) _stellarImfFormOption.ItemSelected += _ => OnScienceControlChanged();
        if (_stellarImfVariationModeOption != null) _stellarImfVariationModeOption.ItemSelected += _ => OnScienceControlChanged();
        if (_stellarIsochroneModelOption != null) _stellarIsochroneModelOption.ItemSelected += _ => OnScienceControlChanged();
        if (_planetMassRadiusModelOption != null) _planetMassRadiusModelOption.ItemSelected += _ => OnScienceControlChanged();
        if (_planetEnvelopeLossModelOption != null) _planetEnvelopeLossModelOption.ItemSelected += _ => OnScienceControlChanged();
        if (_planetHabitableZoneModelOption != null) _planetHabitableZoneModelOption.ItemSelected += _ => OnScienceControlChanged();
        if (_planetGasGiantFormationModelOption != null) _planetGasGiantFormationModelOption.ItemSelected += _ => OnScienceControlChanged();
        if (_planetMetallicityCouplingOption != null) _planetMetallicityCouplingOption.ItemSelected += _ => OnScienceControlChanged();
        if (_planetRogueAllowanceOption != null) _planetRogueAllowanceOption.ItemSelected += _ => OnScienceControlChanged();
        if (_planetMoonFormationBiasOption != null) _planetMoonFormationBiasOption.ItemSelected += _ => OnScienceControlChanged();
        if (_planetMinorBodyOuterBiasOption != null) _planetMinorBodyOuterBiasOption.ItemSelected += _ => OnScienceControlChanged();
        ConnectSlider(_haloMassSlider, OnHaloMassChanged);
        ConnectSlider(_environmentSlider, OnEnvironmentChanged);
        ConnectSlider(_starFormationEfficiencySlider, OnStarFormationEfficiencyChanged);
        ConnectSlider(_ghzInnerSlider, OnGhzInnerChanged);
        ConnectSlider(_ghzOuterSlider, OnGhzOuterChanged);
        ConnectSlider(_ghzWidthSlider, OnGhzWidthChanged);
        ConnectSlider(_metallicityGradientSlider, OnMetallicityGradientChanged);
        ConnectSlider(_stellarMultiplicityScaleSlider, OnStellarMultiplicityChanged);
        if (_helpButton != null) _helpButton.Pressed += OnScienceInfoPressed;
        if (_helpDialogCloseButton != null) _helpDialogCloseButton.Pressed += HideHelpDialog;
        if (_helpDialog != null) _helpDialog.CloseRequested += HideHelpDialog;
    }

    private void ApplyScienceLayoutPolish()
    {
        if (_helpDialog != null)
        {
            _helpDialog.Visible = false;
        }
    }

    private void ApplySciencePanelText()
    {
        if (_helpDialogText != null)
        {
            _helpDialogText.Text = BuildHelpDialogBbCode();
        }
    }

    private void ApplyScienceParameterTooltips()
    {
        ApplyTooltip("subtype_mode", _subtypeModeOption, $"{ParameterRootPath}/TypeSection/TypeContent/TypeVBox/SubtypeModeRow/SubtypeModeLabel");
        ApplyTooltip("bar_mode", _barModeOption, $"{ParameterRootPath}/TypeSection/TypeContent/TypeVBox/BarModeRow/BarModeLabel");
        ApplyTooltip("arm_mechanism_preference", _armMechanismOption, $"{ParameterRootPath}/TypeSection/TypeContent/TypeVBox/ArmMechanismRow/ArmMechanismLabel");
        ApplyTooltip("halo_mass_log10_solar", _haloMassSlider, $"{ParameterRootPath}/ScienceSection/ScienceContent/ScienceVBox/HaloMassRow/HaloMassLabel");
        ApplyTooltip("environment_density_index", _environmentSlider, $"{ParameterRootPath}/ScienceSection/ScienceContent/ScienceVBox/EnvironmentRow/EnvironmentLabel");
        ApplyTooltip("star_formation_efficiency", _starFormationEfficiencySlider, $"{ParameterRootPath}/ScienceSection/ScienceContent/ScienceVBox/StarFormationEfficiencyRow/StarFormationEfficiencyLabel");
        ApplyTooltip("ghz_inner_radius_pc", _ghzInnerSlider, $"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/GhzInnerRow/GhzInnerLabel");
        ApplyTooltip("ghz_outer_radius_pc", _ghzOuterSlider, $"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/GhzOuterRow/GhzOuterLabel");
        ApplyTooltip("ghz_transition_width_pc", _ghzWidthSlider, $"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/GhzWidthRow/GhzWidthLabel");
        ApplyTooltip("metallicity_gradient_dex_per_kpc", _metallicityGradientSlider, $"{ParameterRootPath}/SizeSection/SizeContent/SizeVBox/MetallicityGradientRow/MetallicityGradientLabel");
        ApplyTooltip("stellar_imf_form", _stellarImfFormOption, $"{ParameterRootPath}/StellarSection/StellarContent/StellarVBox/ImfFormRow/ImfFormLabel");
        ApplyTooltip("stellar_imf_variation_mode", _stellarImfVariationModeOption, $"{ParameterRootPath}/StellarSection/StellarContent/StellarVBox/ImfVariationRow/ImfVariationLabel");
        ApplyTooltip("stellar_isochrone_model", _stellarIsochroneModelOption, $"{ParameterRootPath}/StellarSection/StellarContent/StellarVBox/IsochroneRow/IsochroneLabel");
        ApplyTooltip("stellar_multiplicity_scale", _stellarMultiplicityScaleSlider, $"{ParameterRootPath}/StellarSection/StellarContent/StellarVBox/MultiplicityRow/MultiplicityLabel");
        ApplyTooltip("planet_mass_radius_model", _planetMassRadiusModelOption, $"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/MassRadiusRow/MassRadiusLabel");
        ApplyTooltip("planet_envelope_loss_model", _planetEnvelopeLossModelOption, $"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/EnvelopeLossRow/EnvelopeLossLabel");
        ApplyTooltip("planet_habitable_zone_model", _planetHabitableZoneModelOption, $"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/HabitableZoneRow/HabitableZoneLabel");
        ApplyTooltip("planet_gas_giant_formation_model", _planetGasGiantFormationModelOption, $"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/GasGiantFormationRow/GasGiantFormationLabel");
        ApplyTooltip("planet_metallicity_coupling_strength", _planetMetallicityCouplingOption, $"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/MetallicityCouplingRow/MetallicityCouplingLabel");
        ApplyTooltip("planet_rogue_planet_allowance", _planetRogueAllowanceOption, $"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/RogueAllowanceRow/RogueAllowanceLabel");
        ApplyTooltip("planet_moon_formation_bias", _planetMoonFormationBiasOption, $"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/MoonFormationBiasRow/MoonFormationBiasLabel");
        ApplyTooltip("planet_minor_body_outer_system_bias", _planetMinorBodyOuterBiasOption, $"{ParameterRootPath}/PlanetarySection/PlanetaryContent/PlanetaryVBox/OuterBodyBiasRow/OuterBodyBiasLabel");
        ApplyTooltip("life_framework", _lifeFrameworkOption, $"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/LifeFrameworkRow/LifeFrameworkLabel");
        ApplyTooltip("abiogenesis_model", _abiogenesisModelOption, $"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/AbiogenesisModelRow/AbiogenesisModelLabel");
        ApplyTooltip("complex_life_model", _complexLifeModelOption, $"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/ComplexLifeModelRow/ComplexLifeModelLabel");
        ApplyTooltip("civilization_model", _civilizationModelOption, $"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/CivilizationModelRow/CivilizationModelLabel");
        ApplyTooltip("environmental_window_weight", _environmentalWindowWeightOption, $"{ParameterRootPath}/LifeSection/LifeContent/LifeVBox/EnvironmentalWindowWeightRow/EnvironmentalWindowWeightLabel");
        if (_helpButton != null)
        {
            _helpButton.TooltipText = "Open plain-language help.\nThis guide explains what these galaxy, star, and planet settings actually change.";
        }

        ApplyScienceSectionSourceTooltips();
    }

    private void UpdateScienceTypeSpecificControls(int galaxyType)
    {
        bool isSpiral = galaxyType == (int)GalaxySpec.GalaxyType.Spiral;
        bool isElliptical = galaxyType == (int)GalaxySpec.GalaxyType.Elliptical;
        bool isLenticular = galaxyType == (int)GalaxySpec.GalaxyType.Lenticular;
        bool isDiskFamily = isSpiral || isLenticular;

        if (_pitchRow != null) _pitchRow.Visible = isSpiral;
        if (_amplitudeRow != null) _amplitudeRow.Visible = isSpiral;
        if (_diskLengthRow != null) _diskLengthRow.Visible = isDiskFamily;
        if (_diskHeightRow != null) _diskHeightRow.Visible = isDiskFamily;
        if (_ellipticityRow != null) _ellipticityRow.Visible = isElliptical || isLenticular;
    }

    private void UpdateScienceValueLabels()
    {
        UpdateFloatLabel(_haloMassValue, _haloMassSlider, "0.0", string.Empty);
        UpdateFloatLabel(_environmentValue, _environmentSlider, "0.00", string.Empty);
        UpdateFloatLabel(_starFormationEfficiencyValue, _starFormationEfficiencySlider, "0.00", string.Empty);
        UpdateIntLabel(_ghzInnerValue, _ghzInnerSlider, " pc");
        UpdateIntLabel(_ghzOuterValue, _ghzOuterSlider, " pc");
        UpdateIntLabel(_ghzWidthValue, _ghzWidthSlider, " pc");
        UpdateFloatLabel(_metallicityGradientValue, _metallicityGradientSlider, "0.000", " dex/kpc");
        UpdateFloatLabel(_stellarMultiplicityScaleValue, _stellarMultiplicityScaleSlider, "0.00", "x");
    }

    private void ApplyScientificValuesToConfig(GalaxyConfig config)
    {
        if (_subtypeModeOption != null)
        {
            config.SubtypeMode = (GalaxySubtypeMode)_subtypeModeOption.GetItemId(_subtypeModeOption.Selected);
        }

        if (_barModeOption != null)
        {
            config.BarMode = (GalaxyBarMode)_barModeOption.GetItemId(_barModeOption.Selected);
        }

        if (_armMechanismOption != null)
        {
            config.ArmMechanismPreference = (GalaxyArmMechanism)_armMechanismOption.GetItemId(_armMechanismOption.Selected);
        }

        if (_haloMassSlider != null) config.HaloMassLog10Solar = _haloMassSlider.Value;
        if (_environmentSlider != null) config.EnvironmentDensityIndex = _environmentSlider.Value;
        if (_starFormationEfficiencySlider != null) config.StarFormationEfficiency = _starFormationEfficiencySlider.Value;
        if (_ghzInnerSlider != null) config.GhzInnerRadiusPc = _ghzInnerSlider.Value;
        if (_ghzOuterSlider != null) config.GhzOuterRadiusPc = _ghzOuterSlider.Value;
        if (_ghzWidthSlider != null) config.GhzTransitionWidthPc = _ghzWidthSlider.Value;
        if (_metallicityGradientSlider != null) config.MetallicityGradientDexPerKpc = _metallicityGradientSlider.Value;
        config.StellarProfile = BuildStellarProfileFromControls();
        config.PlanetaryProfile = BuildPlanetaryProfileFromControls();
    }

    private void ApplyScienceConfig(GalaxyConfig config)
    {
        SetOptionSelection(_subtypeModeOption, (int)config.SubtypeMode);
        SetOptionSelection(_barModeOption, (int)config.BarMode);
        SetOptionSelection(_armMechanismOption, (int)config.ArmMechanismPreference);
        SetSlider(_haloMassSlider, config.HaloMassLog10Solar);
        SetSlider(_environmentSlider, config.EnvironmentDensityIndex);
        SetSlider(_starFormationEfficiencySlider, config.StarFormationEfficiency);
        SetSlider(_ghzInnerSlider, config.GhzInnerRadiusPc);
        SetSlider(_ghzOuterSlider, config.GhzOuterRadiusPc);
        SetSlider(_ghzWidthSlider, config.GhzTransitionWidthPc);
        SetSlider(_metallicityGradientSlider, config.MetallicityGradientDexPerKpc);
        ApplyStellarProfileToControls(config.StellarProfile);
        ApplyPlanetaryProfileToControls(config.PlanetaryProfile);
    }

    private static void ApplyScientificPresetValues(Preset preset, GalaxyConfig config)
    {
        if (preset == Preset.Andromeda)
        {
            config.SubtypeMode = GalaxySubtypeMode.EarlyType;
            config.BarMode = GalaxyBarMode.PreferBarred;
            config.ArmMechanismPreference = GalaxyArmMechanism.GrandDesign;
            config.HaloMassLog10Solar = 12.4;
            config.EnvironmentDensityIndex = 0.35;
            config.StarFormationEfficiency = 0.11;
            config.GhzInnerRadiusPc = 5000.0;
            config.GhzOuterRadiusPc = 16000.0;
            config.GhzTransitionWidthPc = 2500.0;
            config.MetallicityGradientDexPerKpc = -0.040;
            return;
        }

        if (preset == Preset.Whirlpool)
        {
            config.SubtypeMode = GalaxySubtypeMode.LateType;
            config.BarMode = GalaxyBarMode.PreferUnbarred;
            config.ArmMechanismPreference = GalaxyArmMechanism.GrandDesign;
            config.HaloMassLog10Solar = 11.6;
            config.EnvironmentDensityIndex = 0.20;
            config.StarFormationEfficiency = 0.18;
            config.GhzInnerRadiusPc = 3000.0;
            config.GhzOuterRadiusPc = 9000.0;
            config.GhzTransitionWidthPc = 1800.0;
            config.MetallicityGradientDexPerKpc = -0.055;
            return;
        }

        if (preset == Preset.Sombrero)
        {
            config.Type = GalaxySpec.GalaxyType.Lenticular;
            config.SubtypeMode = GalaxySubtypeMode.Automatic;
            config.BarMode = GalaxyBarMode.PreferUnbarred;
            config.HaloMassLog10Solar = 12.3;
            config.EnvironmentDensityIndex = 0.55;
            config.StarFormationEfficiency = 0.07;
            config.GhzInnerRadiusPc = 4500.0;
            config.GhzOuterRadiusPc = 11000.0;
            config.GhzTransitionWidthPc = 1800.0;
            config.MetallicityGradientDexPerKpc = -0.035;
            config.StellarProfile.MultiplicityScale = 0.9;
            return;
        }

        if (preset == Preset.LargeMagellanicCloud)
        {
            config.SubtypeMode = GalaxySubtypeMode.LateType;
            config.HaloMassLog10Solar = 10.1;
            config.EnvironmentDensityIndex = 0.15;
            config.StarFormationEfficiency = 0.22;
            config.GhzInnerRadiusPc = 2500.0;
            config.GhzOuterRadiusPc = 7000.0;
            config.GhzTransitionWidthPc = 1800.0;
            config.MetallicityGradientDexPerKpc = -0.030;
            config.StellarProfile.ImfVariationMode = StellarImfVariationMode.MetallicityAgeModulated;
            config.StellarProfile.MultiplicityScale = 1.1;
        }
    }

    private string BuildScienceSummary(GalaxyConfig config)
    {
        int seedValue = 1;
        if (_seedSpin != null)
        {
            seedValue = (int)_seedSpin.Value;
        }

        GalaxyRealismProfile profile = GalaxyRealismProfileBuilder.Build(config, seedValue);
        string galaxySummary = GalaxyScienceReferenceCatalog.BuildProfileSummary(config, profile);
        string diagnosticsSummary = BuildResolvedGalacticDiagnosticsSummary(profile);
        string stellarSummary = BuildStellarProfileSummary(config.StellarProfile);
        string planetarySummary = BuildPlanetaryProfileSummary(config.PlanetaryProfile);
        return $"{galaxySummary}\n{diagnosticsSummary}\n{stellarSummary}\n{planetarySummary}";
    }

    private static string BuildResolvedGalacticDiagnosticsSummary(GalaxyRealismProfile profile)
    {
        GalaxyMassComponentBudget budget = profile.MassComponentBudget;
        GalaxyRotationCurveDiagnostic rotation = profile.RotationCurveDiagnostic;
        GalaxyDynamicsDiagnostic dynamics = profile.DynamicsDiagnostic;
        string analogSummary = dynamics.AnalogCalibrationMode;
        string comparisonSummary = dynamics.NonMilkyWayComparisonStatus;
        if (comparisonSummary.Contains("needs"))
        {
            comparisonSummary = "comparison sources needed";
        }

        return "Resolved Galactic Diagnostics: "
            + $"Mass Budget baryonic {budget.BaryonicMassSolar:0.00e0} Msun, dark halo {budget.DarkMatterHaloMassSolar:0.00e0} Msun | "
            + $"Rotation reference {rotation.ReferenceVelocityKmS:0} km/s, inner {rotation.InnerVelocityKmS:0} km/s, outer {rotation.OuterVelocityKmS:0} km/s | "
            + $"Bar Dynamics pattern speed {dynamics.BarPatternSpeedKmSPerKpc:0.0} km/s/kpc, corotation {dynamics.CorotationRadiusPc:0} pc | "
            + $"Local Mass Budget density {dynamics.LocalTotalMassDensitySolarPerPc3:0.000} Msun/pc^3 | "
            + $"Analog Calibration {analogSummary}, {comparisonSummary} | "
            + "Dynamics Mode Diagnostics Only disabled; future controls Affect Region Context and Affect Placement remain review-gated.";
    }

    private void ApplyScienceAssumptionSummary()
    {
        if (_assumptionsLabel != null)
        {
            _assumptionsLabel.Text = "The Help button explains what each science term means in plain language. The defaults are research-backed starting points, not universal laws.";
            _assumptionsLabel.Visible = true;
        }
    }

    private void OnScienceControlChanged()
    {
        MarkAsCustom();
        RefreshValidationIssues();
    }

    private void OnHaloMassChanged(double _value)
    {
        UpdateFloatLabel(_haloMassValue, _haloMassSlider, "0.0", string.Empty);
        OnScienceControlChanged();
    }

    private void OnEnvironmentChanged(double _value)
    {
        UpdateFloatLabel(_environmentValue, _environmentSlider, "0.00", string.Empty);
        OnScienceControlChanged();
    }

    private void OnStarFormationEfficiencyChanged(double _value)
    {
        UpdateFloatLabel(_starFormationEfficiencyValue, _starFormationEfficiencySlider, "0.00", string.Empty);
        OnScienceControlChanged();
    }

    private void OnGhzInnerChanged(double _value)
    {
        UpdateIntLabel(_ghzInnerValue, _ghzInnerSlider, " pc");
        OnScienceControlChanged();
    }

    private void OnGhzOuterChanged(double _value)
    {
        UpdateIntLabel(_ghzOuterValue, _ghzOuterSlider, " pc");
        OnScienceControlChanged();
    }

    private void OnGhzWidthChanged(double _value)
    {
        UpdateIntLabel(_ghzWidthValue, _ghzWidthSlider, " pc");
        OnScienceControlChanged();
    }

    private void OnMetallicityGradientChanged(double _value)
    {
        UpdateFloatLabel(_metallicityGradientValue, _metallicityGradientSlider, "0.000", " dex/kpc");
        OnScienceControlChanged();
    }

    private void OnStellarMultiplicityChanged(double _value)
    {
        UpdateFloatLabel(_stellarMultiplicityScaleValue, _stellarMultiplicityScaleSlider, "0.00", "x");
        OnScienceControlChanged();
    }

    private void OnScienceInfoPressed()
    {
        if (_helpDialog == null)
        {
            return;
        }

        HelpDialogLayoutHelper.Open(_helpDialog);
        if (_helpDialogText != null)
        {
            _helpDialogText.ScrollToLine(0);
        }
    }

    private void HideHelpDialog()
    {
        if (_helpDialog != null)
        {
            _helpDialog.Visible = false;
        }
    }

    private StellarGenerationProfile BuildStellarProfileFromControls()
    {
        StellarGenerationProfile profile = StellarGenerationProfile.CreateDefault();

        if (_stellarImfFormOption != null)
        {
            profile.ImfForm = (StellarImfForm)_stellarImfFormOption.GetItemId(_stellarImfFormOption.Selected);
        }

        if (_stellarImfVariationModeOption != null)
        {
            profile.ImfVariationMode = (StellarImfVariationMode)_stellarImfVariationModeOption.GetItemId(_stellarImfVariationModeOption.Selected);
        }

        if (_stellarIsochroneModelOption != null)
        {
            profile.IsochroneModel = (StellarIsochroneModel)_stellarIsochroneModelOption.GetItemId(_stellarIsochroneModelOption.Selected);
        }

        if (_stellarMultiplicityScaleSlider != null)
        {
            profile.MultiplicityScale = _stellarMultiplicityScaleSlider.Value;
        }

        return profile;
    }

    private void ApplyStellarProfileToControls(StellarGenerationProfile profile)
    {
        SetOptionSelection(_stellarImfFormOption, (int)profile.ImfForm);
        SetOptionSelection(_stellarImfVariationModeOption, (int)profile.ImfVariationMode);
        SetOptionSelection(_stellarIsochroneModelOption, (int)profile.IsochroneModel);
        SetSlider(_stellarMultiplicityScaleSlider, profile.MultiplicityScale);
    }

    private static string BuildStellarProfileSummary(StellarGenerationProfile profile)
    {
        string imfLabel = "Kroupa";
        if (profile.ImfForm == StellarImfForm.Chabrier)
        {
            imfLabel = "Chabrier";
        }

        string variationLabel = "canonical";
        if (profile.ImfVariationMode == StellarImfVariationMode.MetallicityAgeModulated)
        {
            variationLabel = "metallicity and age modulated";
        }

        string isochroneLabel = "MIST";
        if (profile.IsochroneModel == StellarIsochroneModel.Parsec)
        {
            isochroneLabel = "PARSEC";
        }

        return $"Stellar model: IMF {imfLabel} | Variation {variationLabel} | Tracks {isochroneLabel} | Multiplicity {profile.MultiplicityScale:0.00}x";
    }

    private static string BuildHelpDialogBbCode()
    {
        string diagnosticsHelp = "[b]Resolved Galactic Diagnostics[/b]\n"
            + "The structural schema fields describe the galaxy shape and scale used by generation. "
            + "The diagnostic dynamics fields describe mass, rotation, bar, corotation, and local-density readouts. "
            + "active generation behavior remains gated by the Dynamics Mode control: Diagnostics Only is the default, while Affect Region Context and Affect Placement require explicit review. "
            + "Bland-Hawthorn, Bovy, Khoperskov, Hunt, Kennicutt, and future comparison sources are tracked separately; non-Sb family rows still surface comparison sources needed when calibration is incomplete.";
        return $"{GalaxyScienceReferenceCatalog.BuildSciencePanelBbCode()}\n\n{diagnosticsHelp}\n\n{LifeScienceReferenceCatalog.BuildHelpPanelBbCode()}\n\n{StellarScienceReferenceCatalog.BuildHelpPanelBbCode()}\n\n{PlanetaryScienceReferenceCatalog.BuildHelpPanelBbCode()}";
    }

    private PlanetaryGenerationProfile BuildPlanetaryProfileFromControls()
    {
        PlanetaryGenerationProfile profile = PlanetaryGenerationProfile.CreateDefault();
        if (_planetMassRadiusModelOption != null)
        {
            profile.MassRadiusModel = (PlanetMassRadiusModel)_planetMassRadiusModelOption.GetSelectedId();
        }

        if (_planetEnvelopeLossModelOption != null)
        {
            profile.EnvelopeLossModel = (PlanetEnvelopeLossModel)_planetEnvelopeLossModelOption.GetSelectedId();
        }

        if (_planetHabitableZoneModelOption != null)
        {
            profile.HabitableZoneModel = (PlanetHabitableZoneModel)_planetHabitableZoneModelOption.GetSelectedId();
        }

        if (_planetGasGiantFormationModelOption != null)
        {
            profile.GasGiantFormationModel = (GasGiantFormationModel)_planetGasGiantFormationModelOption.GetSelectedId();
        }

        if (_planetMetallicityCouplingOption != null)
        {
            profile.MetallicityCouplingStrength = (PlanetMetallicityCouplingStrength)_planetMetallicityCouplingOption.GetSelectedId();
        }

        if (_planetRogueAllowanceOption != null)
        {
            profile.RoguePlanetAllowance = (PlanetRoguePlanetAllowance)_planetRogueAllowanceOption.GetSelectedId();
        }

        if (_planetMoonFormationBiasOption != null)
        {
            profile.MoonFormationBias = (PlanetMoonFormationBias)_planetMoonFormationBiasOption.GetSelectedId();
        }

        if (_planetMinorBodyOuterBiasOption != null)
        {
            profile.MinorBodyOuterSystemBias = (PlanetMinorBodyOuterSystemBias)_planetMinorBodyOuterBiasOption.GetSelectedId();
        }

        return profile;
    }

    private void ApplyPlanetaryProfileToControls(PlanetaryGenerationProfile profile)
    {
        SetOptionSelection(_planetMassRadiusModelOption, (int)profile.MassRadiusModel);
        SetOptionSelection(_planetEnvelopeLossModelOption, (int)profile.EnvelopeLossModel);
        SetOptionSelection(_planetHabitableZoneModelOption, (int)profile.HabitableZoneModel);
        SetOptionSelection(_planetGasGiantFormationModelOption, (int)profile.GasGiantFormationModel);
        SetOptionSelection(_planetMetallicityCouplingOption, (int)profile.MetallicityCouplingStrength);
        SetOptionSelection(_planetRogueAllowanceOption, (int)profile.RoguePlanetAllowance);
        SetOptionSelection(_planetMoonFormationBiasOption, (int)profile.MoonFormationBias);
        SetOptionSelection(_planetMinorBodyOuterBiasOption, (int)profile.MinorBodyOuterSystemBias);
    }

    private static string BuildPlanetaryProfileSummary(PlanetaryGenerationProfile profile)
    {
        return $"Planet model: Size {profile.MassRadiusModel} | Loss {profile.EnvelopeLossModel} | HZ {profile.HabitableZoneModel} | Giants {profile.GasGiantFormationModel} | Metallicity {profile.MetallicityCouplingStrength} | Rogue {profile.RoguePlanetAllowance} | Moons {profile.MoonFormationBias}";
    }

    private static void SetOptionSelection(OptionButton? optionButton, int itemId)
    {
        if (optionButton == null)
        {
            return;
        }

        for (int index = 0; index < optionButton.ItemCount; index += 1)
        {
            if (optionButton.GetItemId(index) == itemId)
            {
                optionButton.Select(index);
                return;
            }
        }
    }

    private void ApplyScienceSectionSourceTooltips()
    {
        ApplySectionTooltip(
            _typeSourcesButton,
            BuildGalaxySectionSourceTooltip(
                "Galaxy Type",
                new[]
                {
                    "galaxy_type",
                    "subtype_mode",
                    "bar_mode",
                    "arm_mechanism_preference",
                }));

        ApplySectionTooltip(
            _scienceSourcesButton,
            BuildGalaxySectionSourceTooltip(
                "Scientific Priors",
                new[]
                {
                    "halo_mass_log10_solar",
                    "environment_density_index",
                    "star_formation_efficiency",
                }));

        ApplySectionTooltip(
            _structureSourcesButton,
            BuildGalaxySectionSourceTooltip(
                "Structure",
                new[]
                {
                    "num_arms",
                    "arm_pitch_angle_deg",
                    "arm_amplitude",
                    "bulge_intensity",
                    "bulge_radius_pc",
                    "ellipticity",
                    "irregularity_scale",
                }));

        ApplySectionTooltip(
            _sizeSourcesButton,
            BuildGalaxySectionSourceTooltip(
                "Size",
                new[]
                {
                    "radius_pc",
                    "disk_scale_length_pc",
                    "disk_scale_height_pc",
                    "star_density_multiplier",
                    "ghz_inner_radius_pc",
                    "ghz_outer_radius_pc",
                    "ghz_transition_width_pc",
                    "metallicity_gradient_dex_per_kpc",
                }));

        ApplySectionTooltip(
            _lifeSourcesButton,
            BuildLifeSectionSourceTooltip(
                "Life Models",
                new[]
                {
                    "life_framework",
                    "abiogenesis_model",
                    "complex_life_model",
                    "civilization_model",
                    "environmental_window_weight",
                }));

        ApplySectionTooltip(
            _stellarSourcesButton,
            BuildStellarSectionSourceTooltip(
                "Stellar",
                new[]
                {
                    "stellar_imf_form",
                    "stellar_imf_variation_mode",
                    "stellar_isochrone_model",
                    "stellar_multiplicity_scale",
                }));

        ApplySectionTooltip(
            _planetarySourcesButton,
            BuildPlanetarySectionSourceTooltip(
                "Planetary",
                new[]
                {
                    "planet_mass_radius_model",
                    "planet_envelope_loss_model",
                    "planet_habitable_zone_model",
                    "planet_gas_giant_formation_model",
                    "planet_metallicity_coupling_strength",
                    "planet_rogue_planet_allowance",
                    "planet_moon_formation_bias",
                    "planet_minor_body_outer_system_bias",
                }));
    }

    private static void ApplySectionTooltip(Control? control, string tooltipText)
    {
        if (control == null)
        {
            return;
        }

        control.TooltipText = tooltipText;
    }

    private static string BuildGalaxySectionSourceTooltip(string sectionLabel, IReadOnlyList<string> parameterIds)
    {
        List<string> citations = CollectUniqueSourceCitations(
            parameterIds,
            GalaxyScienceReferenceCatalog.GetParameterSourceIds,
            static sourceId =>
            {
                GalaxyScienceSource? source = GalaxyScienceReferenceCatalog.GetSource(sourceId);
                if (source == null)
                {
                    return string.Empty;
                }

                return source.Citation;
            });

        return BuildSectionTooltipText(sectionLabel, citations);
    }

    private static string BuildStellarSectionSourceTooltip(string sectionLabel, IReadOnlyList<string> parameterIds)
    {
        List<string> citations = CollectUniqueSourceCitations(
            parameterIds,
            StellarScienceReferenceCatalog.GetParameterSourceIds,
            static sourceId =>
            {
                StellarScienceSource? source = StellarScienceReferenceCatalog.GetSource(sourceId);
                if (source == null)
                {
                    return string.Empty;
                }

                return source.Citation;
            });

        return BuildSectionTooltipText(sectionLabel, citations);
    }

    private static string BuildPlanetarySectionSourceTooltip(string sectionLabel, IReadOnlyList<string> parameterIds)
    {
        List<string> citations = CollectUniqueSourceCitations(
            parameterIds,
            PlanetaryScienceReferenceCatalog.GetParameterSourceIds,
            static sourceId =>
            {
                PlanetaryScienceSource? source = PlanetaryScienceReferenceCatalog.GetSource(sourceId);
                if (source == null)
                {
                    return string.Empty;
                }

                return source.Citation;
            });

        return BuildSectionTooltipText(sectionLabel, citations);
    }

    private static string BuildLifeSectionSourceTooltip(string sectionLabel, IReadOnlyList<string> parameterIds)
    {
        List<string> citations = CollectUniqueSourceCitations(
            parameterIds,
            LifeScienceReferenceCatalog.GetParameterSourceIds,
            static sourceId =>
            {
                LifeScienceSource? source = LifeScienceReferenceCatalog.GetSource(sourceId);
                if (source == null)
                {
                    return string.Empty;
                }

                return source.Citation;
            });

        return BuildSectionTooltipText(sectionLabel, citations);
    }

    private static List<string> CollectUniqueSourceCitations(
        IReadOnlyList<string> parameterIds,
        System.Func<string, IReadOnlyList<string>> sourceIdResolver,
        System.Func<string, string> citationResolver)
    {
        List<string> citations = new();
        HashSet<string> seenSourceIds = new();
        foreach (string parameterId in parameterIds)
        {
            IReadOnlyList<string> sourceIds = sourceIdResolver(parameterId);
            foreach (string sourceId in sourceIds)
            {
                if (!seenSourceIds.Add(sourceId))
                {
                    continue;
                }

                string citation = citationResolver(sourceId);
                if (string.IsNullOrWhiteSpace(citation))
                {
                    continue;
                }

                citations.Add(citation);
            }
        }

        return citations;
    }

    private static string BuildSectionTooltipText(string sectionLabel, IReadOnlyList<string> citations)
    {
        System.Text.StringBuilder builder = new();
        builder.Append("Sources for ");
        builder.Append(sectionLabel);
        builder.Append(':');

        if (citations.Count == 0)
        {
            builder.Append("\nNo linked external source notes for this section yet.");
            return builder.ToString();
        }

        foreach (string citation in citations)
        {
            builder.Append("\n- ");
            builder.Append(citation);
        }

        return builder.ToString();
    }
}
