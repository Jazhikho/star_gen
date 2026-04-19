using Godot;
using StarGen.App.Viewer;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;

namespace StarGen.App;

/// <summary>
/// Planet-local life-model controls for Object Studio.
/// </summary>
public partial class ObjectGenerationScreen
{
    private CheckBox? _forceLifeOnSupportableWorldsCheck;

    private void CacheObjectLifeNodeReferences()
    {
        const string Root = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox";
        _lifeSection = GetNodeOrNull<VBoxContainer>("MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection");
        _lifeFrameworkOption = GetNodeOrNull<OptionButton>($"{Root}/LifeFrameworkRow/LifeFrameworkOption");
        _abiogenesisModelOption = GetNodeOrNull<OptionButton>($"{Root}/AbiogenesisModelRow/AbiogenesisModelOption");
        _complexLifeModelOption = GetNodeOrNull<OptionButton>($"{Root}/ComplexLifeModelRow/ComplexLifeModelOption");
        _civilizationModelOption = GetNodeOrNull<OptionButton>($"{Root}/CivilizationModelRow/CivilizationModelOption");
        _environmentalWindowWeightOption = GetNodeOrNull<OptionButton>($"{Root}/EnvironmentalWindowWeightRow/EnvironmentalWindowWeightOption");
        _forceLifeOnSupportableWorldsCheck = GetNodeOrNull<CheckBox>($"{Root}/ForceLifeOnSupportableWorldsRow/ForceLifeOnSupportableWorldsCheck");
    }

    private void PopulateObjectLifeSection()
    {
        PopulateLifeFrameworkOptions(_lifeFrameworkOption);
        PopulateAbiogenesisOptions(_abiogenesisModelOption);
        PopulateComplexLifeOptions(_complexLifeModelOption);
        PopulateCivilizationOptions(_civilizationModelOption);
        PopulateEnvironmentalWindowOptions(_environmentalWindowWeightOption);
        ApplyObjectLifeParameterTooltips();
    }

    private void ConnectObjectLifeSignals()
    {
        ConnectOptionToSummary(_lifeFrameworkOption);
        ConnectOptionToSummary(_abiogenesisModelOption);
        ConnectOptionToSummary(_complexLifeModelOption);
        ConnectOptionToSummary(_civilizationModelOption);
        ConnectOptionToSummary(_environmentalWindowWeightOption);
        if (_forceLifeOnSupportableWorldsCheck != null)
        {
            _forceLifeOnSupportableWorldsCheck.Toggled += _ => RefreshSummary();
        }
    }

    private void ApplyObjectLifeDefaults()
    {
        ApplyObjectLifeSettingsToControls(GenerationUseCaseSettings.CreateDefault());
    }

    private void ApplyObjectLifeSettingsFromControls(GenerationUseCaseSettings settings)
    {
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

        if (_forceLifeOnSupportableWorldsCheck != null)
        {
            settings.ForceLifeOnSupportableWorlds = _forceLifeOnSupportableWorldsCheck.ButtonPressed;
        }
    }

    private void ApplyTravellerObjectLifeDefaultsToControls()
    {
        SelectOptionById(_lifeFrameworkOption, (int)GenerationUseCaseSettings.LifeFrameworkType.RapidBiospheres);
        SelectOptionById(_abiogenesisModelOption, (int)GenerationUseCaseSettings.AbiogenesisModelType.FollowFramework);
        SelectOptionById(_complexLifeModelOption, (int)GenerationUseCaseSettings.ComplexLifeModelType.FollowFramework);
        SelectOptionById(_civilizationModelOption, (int)GenerationUseCaseSettings.CivilizationModelType.FollowFramework);
        SelectOptionById(_environmentalWindowWeightOption, (int)GenerationUseCaseSettings.EnvironmentalWindowWeightType.FollowFramework);
        if (_forceLifeOnSupportableWorldsCheck != null)
        {
            _forceLifeOnSupportableWorldsCheck.ButtonPressed = false;
        }
    }

    private void ApplyObjectLifeSettingsToControls(GenerationUseCaseSettings settings)
    {
        SelectOptionById(_lifeFrameworkOption, (int)settings.LifeFramework);
        SelectOptionById(_abiogenesisModelOption, (int)settings.AbiogenesisModel);
        SelectOptionById(_complexLifeModelOption, (int)settings.ComplexLifeModel);
        SelectOptionById(_civilizationModelOption, (int)settings.CivilizationModel);
        SelectOptionById(_environmentalWindowWeightOption, (int)settings.EnvironmentalWindowWeight);
        if (_forceLifeOnSupportableWorldsCheck != null)
        {
            _forceLifeOnSupportableWorldsCheck.ButtonPressed = settings.ForceLifeOnSupportableWorlds;
        }
    }

    private void ApplyObjectLifeParameterTooltips()
    {
        const string Root = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox";
        ApplyObjectLifeTooltip("life_framework", _lifeFrameworkOption, $"{Root}/LifeFrameworkRow/LifeFrameworkLabel");
        ApplyObjectLifeTooltip("abiogenesis_model", _abiogenesisModelOption, $"{Root}/AbiogenesisModelRow/AbiogenesisModelLabel");
        ApplyObjectLifeTooltip("complex_life_model", _complexLifeModelOption, $"{Root}/ComplexLifeModelRow/ComplexLifeModelLabel");
        ApplyObjectLifeTooltip("civilization_model", _civilizationModelOption, $"{Root}/CivilizationModelRow/CivilizationModelLabel");
        ApplyObjectLifeTooltip("environmental_window_weight", _environmentalWindowWeightOption, $"{Root}/EnvironmentalWindowWeightRow/EnvironmentalWindowWeightLabel");
        ApplyObjectLifeOverrideTooltip($"{Root}/ForceLifeOnSupportableWorldsRow/ForceLifeOnSupportableWorldsLabel");
    }

    private void ApplyObjectLifeTooltip(string parameterId, Control? inputControl, string labelPath)
    {
        string tooltip = LifeScienceReferenceCatalog.GetTooltipSummary(parameterId);
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

    private string BuildObjectLifeSummary()
    {
        if (GetSelectedObjectType() != ObjectViewer.ObjectType.Planet)
        {
            return string.Empty;
        }

        string framework = LifeScienceReferenceCatalog.GetFrameworkLabel((GenerationUseCaseSettings.LifeFrameworkType)(_lifeFrameworkOption?.GetSelectedId() ?? 0));
        string forceLife = _forceLifeOnSupportableWorldsCheck != null && _forceLifeOnSupportableWorldsCheck.ButtonPressed ? " | Force Life On" : string.Empty;
        return $"Life {framework}{forceLife}";
    }

    private void ApplyObjectLifeOverrideTooltip(string labelPath)
    {
        string tooltip = "Generation override, not a scientific model.\nWhen enabled, this planet keeps native life if it already passes the biology support gate.\nIt still does not create life on worlds that fail support.";
        if (_forceLifeOnSupportableWorldsCheck != null)
        {
            _forceLifeOnSupportableWorldsCheck.TooltipText = tooltip;
        }

        Label? label = GetNodeOrNull<Label>(labelPath);
        if (label != null)
        {
            label.TooltipText = tooltip;
        }
    }

    private void PopulateLifeFrameworkOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Earth-Anchored Composite", (int)GenerationUseCaseSettings.LifeFrameworkType.EarthAnchoredComposite);
        optionButton.AddItem("Rapid Biospheres", (int)GenerationUseCaseSettings.LifeFrameworkType.RapidBiospheres);
        optionButton.AddItem("Environmental Windows", (int)GenerationUseCaseSettings.LifeFrameworkType.EnvironmentalWindows);
        optionButton.AddItem("Rare Complex Life", (int)GenerationUseCaseSettings.LifeFrameworkType.RareComplexLife);
    }

    private void PopulateAbiogenesisOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Follow Framework", (int)GenerationUseCaseSettings.AbiogenesisModelType.FollowFramework);
        optionButton.AddItem("Rapid Start", (int)GenerationUseCaseSettings.AbiogenesisModelType.RapidStart);
        optionButton.AddItem("Conservative", (int)GenerationUseCaseSettings.AbiogenesisModelType.Conservative);
    }

    private void PopulateComplexLifeOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Follow Framework", (int)GenerationUseCaseSettings.ComplexLifeModelType.FollowFramework);
        optionButton.AddItem("Earth-Anchored Composite", (int)GenerationUseCaseSettings.ComplexLifeModelType.EarthAnchoredComposite);
        optionButton.AddItem("Environmental Windows", (int)GenerationUseCaseSettings.ComplexLifeModelType.EnvironmentalWindows);
        optionButton.AddItem("Rare Earth Filters", (int)GenerationUseCaseSettings.ComplexLifeModelType.RareEarthFilters);
    }

    private void PopulateCivilizationOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Follow Framework", (int)GenerationUseCaseSettings.CivilizationModelType.FollowFramework);
        optionButton.AddItem("Earth-Anchored Composite", (int)GenerationUseCaseSettings.CivilizationModelType.EarthAnchoredComposite);
        optionButton.AddItem("Rare Civilizations", (int)GenerationUseCaseSettings.CivilizationModelType.RareCivilizations);
        optionButton.AddItem("Technosphere Oxygen Bottleneck", (int)GenerationUseCaseSettings.CivilizationModelType.TechnosphereOxygenBottleneck);
    }

    private void PopulateEnvironmentalWindowOptions(OptionButton? optionButton)
    {
        if (optionButton == null)
        {
            return;
        }

        optionButton.Clear();
        optionButton.AddItem("Follow Framework", (int)GenerationUseCaseSettings.EnvironmentalWindowWeightType.FollowFramework);
        optionButton.AddItem("Low", (int)GenerationUseCaseSettings.EnvironmentalWindowWeightType.Low);
        optionButton.AddItem("Moderate", (int)GenerationUseCaseSettings.EnvironmentalWindowWeightType.Moderate);
        optionButton.AddItem("High", (int)GenerationUseCaseSettings.EnvironmentalWindowWeightType.High);
    }
}
