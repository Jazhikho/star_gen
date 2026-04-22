using Godot;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;

namespace StarGen.App;

/// <summary>
/// Life-model control wiring for the system-generation studio.
/// </summary>
public partial class SystemGenerationScreen
{
	private OptionButton? _lifeFrameworkOption;
	private OptionButton? _abiogenesisModelOption;
	private OptionButton? _complexLifeModelOption;
	private OptionButton? _civilizationModelOption;
	private OptionButton? _environmentalWindowWeightOption;

	private partial void CacheLifeNodeReferences()
	{
		const string Root = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox";
		_lifeFrameworkOption = GetNodeOrNull<OptionButton>($"{Root}/LifeFrameworkRow/LifeFrameworkOption");
		_abiogenesisModelOption = GetNodeOrNull<OptionButton>($"{Root}/AbiogenesisModelRow/AbiogenesisModelOption");
		_complexLifeModelOption = GetNodeOrNull<OptionButton>($"{Root}/ComplexLifeModelRow/ComplexLifeModelOption");
		_civilizationModelOption = GetNodeOrNull<OptionButton>($"{Root}/CivilizationModelRow/CivilizationModelOption");
		_environmentalWindowWeightOption = GetNodeOrNull<OptionButton>($"{Root}/EnvironmentalWindowWeightRow/EnvironmentalWindowWeightOption");
	}

	private partial void ConnectLifeSignals()
	{
		if (_lifeFrameworkOption != null) _lifeFrameworkOption.ItemSelected += _ => RefreshSummary();
		if (_abiogenesisModelOption != null) _abiogenesisModelOption.ItemSelected += _ => RefreshSummary();
		if (_complexLifeModelOption != null) _complexLifeModelOption.ItemSelected += _ => RefreshSummary();
		if (_civilizationModelOption != null) _civilizationModelOption.ItemSelected += _ => RefreshSummary();
		if (_environmentalWindowWeightOption != null) _environmentalWindowWeightOption.ItemSelected += _ => RefreshSummary();
	}

	private partial void ApplyLifeDefaults()
	{
		ApplyLifeSettingsToControls(GenerationUseCaseSettings.CreateDefault());
	}

	private partial void ApplyLifeParameterTooltips()
	{
		const string Root = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/LifeSection/LifeContent/LifeVBox";
		ApplyLifeTooltip("life_framework", _lifeFrameworkOption, $"{Root}/LifeFrameworkRow/LifeFrameworkLabel");
		ApplyLifeTooltip("abiogenesis_model", _abiogenesisModelOption, $"{Root}/AbiogenesisModelRow/AbiogenesisModelLabel");
		ApplyLifeTooltip("complex_life_model", _complexLifeModelOption, $"{Root}/ComplexLifeModelRow/ComplexLifeModelLabel");
		ApplyLifeTooltip("civilization_model", _civilizationModelOption, $"{Root}/CivilizationModelRow/CivilizationModelLabel");
		ApplyLifeTooltip("environmental_window_weight", _environmentalWindowWeightOption, $"{Root}/EnvironmentalWindowWeightRow/EnvironmentalWindowWeightLabel");
	}

	private partial void ApplyLifeSettingsFromControls(GenerationUseCaseSettings settings)
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
	}

	private partial void ApplyTravellerLifeDefaultsToControls()
	{
		SetOptionId(_lifeFrameworkOption, (int)GenerationUseCaseSettings.LifeFrameworkType.RapidBiospheres);
		SetOptionId(_abiogenesisModelOption, (int)GenerationUseCaseSettings.AbiogenesisModelType.FollowFramework);
		SetOptionId(_complexLifeModelOption, (int)GenerationUseCaseSettings.ComplexLifeModelType.FollowFramework);
		SetOptionId(_civilizationModelOption, (int)GenerationUseCaseSettings.CivilizationModelType.FollowFramework);
		SetOptionId(_environmentalWindowWeightOption, (int)GenerationUseCaseSettings.EnvironmentalWindowWeightType.FollowFramework);
	}

	private void ApplyLifeSettingsToControls(GenerationUseCaseSettings settings)
	{
		SetOptionId(_lifeFrameworkOption, (int)settings.LifeFramework);
		SetOptionId(_abiogenesisModelOption, (int)settings.AbiogenesisModel);
		SetOptionId(_complexLifeModelOption, (int)settings.ComplexLifeModel);
		SetOptionId(_civilizationModelOption, (int)settings.CivilizationModel);
		SetOptionId(_environmentalWindowWeightOption, (int)settings.EnvironmentalWindowWeight);
	}

	private void ApplyLifeTooltip(string parameterId, Control? inputControl, string labelPath)
	{
		string tooltip = GetLifeParameterAssumption(parameterId);
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

	private static string GetLifeParameterAssumption(string parameterId)
	{
		return GenerationParameterCatalog.FindSystemDefinition(parameterId)?.AssumptionText ?? string.Empty;
	}

	private static void SetOptionId(OptionButton? optionButton, int itemId)
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
}
