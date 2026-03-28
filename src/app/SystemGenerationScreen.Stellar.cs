using Godot;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;

namespace StarGen.App;

/// <summary>
/// Stellar-model control wiring for the system-generation studio.
/// </summary>
public partial class SystemGenerationScreen
{
	private OptionButton? _stellarImfFormOption;
	private OptionButton? _stellarImfVariationModeOption;
	private OptionButton? _stellarIsochroneModelOption;
	private HSlider? _stellarMultiplicityScaleInput;
	private Label? _stellarMultiplicityScaleValueLabel;

	private partial void CacheStellarNodeReferences()
	{
		const string Root = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox";
		_stellarImfFormOption = GetNodeOrNull<OptionButton>($"{Root}/StellarSection/StellarContent/StellarVBox/ImfFormRow/ImfFormOption");
		_stellarImfVariationModeOption = GetNodeOrNull<OptionButton>($"{Root}/StellarSection/StellarContent/StellarVBox/ImfVariationRow/ImfVariationOption");
		_stellarIsochroneModelOption = GetNodeOrNull<OptionButton>($"{Root}/StellarSection/StellarContent/StellarVBox/IsochroneRow/IsochroneOption");
		_stellarMultiplicityScaleInput = GetNodeOrNull<HSlider>($"{Root}/StellarSection/StellarContent/StellarVBox/MultiplicityRow/MultiplicityInput");
		_stellarMultiplicityScaleValueLabel = GetNodeOrNull<Label>($"{Root}/StellarSection/StellarContent/StellarVBox/MultiplicityRow/MultiplicityValue");
	}

	private partial void ConnectStellarSignals()
	{
		if (_stellarImfFormOption != null) _stellarImfFormOption.ItemSelected += _ => RefreshSummary();
		if (_stellarImfVariationModeOption != null) _stellarImfVariationModeOption.ItemSelected += _ => RefreshSummary();
		if (_stellarIsochroneModelOption != null) _stellarIsochroneModelOption.ItemSelected += _ => RefreshSummary();
		if (_stellarMultiplicityScaleInput != null) _stellarMultiplicityScaleInput.ValueChanged += OnStellarMultiplicityScaleChanged;
	}

	private partial void ApplyStellarDefaults()
	{
		ApplyStellarProfileToControls(StellarGenerationProfile.CreateDefault());
		UpdateStellarMultiplicityScaleLabel();
	}

	private partial void ApplyStellarParameterTooltips()
	{
		const string Root = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/StellarSection/StellarContent/StellarVBox";
		ApplyStellarTooltip("stellar_imf_form", _stellarImfFormOption, $"{Root}/ImfFormRow/ImfFormLabel");
		ApplyStellarTooltip("stellar_imf_variation_mode", _stellarImfVariationModeOption, $"{Root}/ImfVariationRow/ImfVariationLabel");
		ApplyStellarTooltip("stellar_isochrone_model", _stellarIsochroneModelOption, $"{Root}/IsochroneRow/IsochroneLabel");
		ApplyStellarTooltip("stellar_multiplicity_scale", _stellarMultiplicityScaleInput, $"{Root}/MultiplicityRow/MultiplicityLabel");
	}

	private partial StellarGenerationProfile BuildStellarProfileFromControls()
	{
		StellarGenerationProfile profile = StellarGenerationProfile.CreateDefault();
		if (_stellarImfFormOption != null)
		{
			profile.ImfForm = (StellarImfForm)_stellarImfFormOption.GetSelectedId();
		}

		if (_stellarImfVariationModeOption != null)
		{
			profile.ImfVariationMode = (StellarImfVariationMode)_stellarImfVariationModeOption.GetSelectedId();
		}

		if (_stellarIsochroneModelOption != null)
		{
			profile.IsochroneModel = (StellarIsochroneModel)_stellarIsochroneModelOption.GetSelectedId();
		}

		if (_stellarMultiplicityScaleInput != null)
		{
			profile.MultiplicityScale = _stellarMultiplicityScaleInput.Value;
		}

		return profile;
	}

	private partial string BuildStellarProfileSummary(StellarGenerationProfile profile)
	{
		string imfLabel = "Kroupa";
		if (profile.ImfForm == StellarImfForm.Chabrier)
		{
			imfLabel = "Chabrier";
		}

		string shiftLabel = "canonical";
		if (profile.ImfVariationMode == StellarImfVariationMode.MetallicityAgeModulated)
		{
			shiftLabel = "metallicity and age";
		}

		string modelLabel = "MIST";
		if (profile.IsochroneModel == StellarIsochroneModel.Parsec)
		{
			modelLabel = "PARSEC";
		}

		return $"Stars IMF {imfLabel} | Shift {shiftLabel} | Model {modelLabel} | Companions {profile.MultiplicityScale:0.00}x";
	}

	private void ApplyStellarTooltip(string parameterId, Control? inputControl, string labelPath)
	{
		string tooltip = GetStellarParameterAssumption(parameterId);
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

	private static string GetStellarParameterAssumption(string parameterId)
	{
		foreach (GenerationParameterDefinition definition in GenerationParameterCatalog.GetSystemDefinitions())
		{
			if (definition.Id == parameterId)
			{
				return definition.AssumptionText;
			}
		}

		return string.Empty;
	}

	private void ApplyStellarProfileToControls(StellarGenerationProfile profile)
	{
		SelectOptionId(_stellarImfFormOption, (int)profile.ImfForm);
		SelectOptionId(_stellarImfVariationModeOption, (int)profile.ImfVariationMode);
		SelectOptionId(_stellarIsochroneModelOption, (int)profile.IsochroneModel);
		if (_stellarMultiplicityScaleInput != null)
		{
			_stellarMultiplicityScaleInput.Value = profile.MultiplicityScale;
		}

		UpdateStellarMultiplicityScaleLabel();
	}

	private void OnStellarMultiplicityScaleChanged(double _value)
	{
		UpdateStellarMultiplicityScaleLabel();
		RefreshSummary();
	}

	private void UpdateStellarMultiplicityScaleLabel()
	{
		if (_stellarMultiplicityScaleInput != null && _stellarMultiplicityScaleValueLabel != null)
		{
			_stellarMultiplicityScaleValueLabel.Text = $"{_stellarMultiplicityScaleInput.Value:0.00}x";
		}
	}

	private static void SelectOptionId(OptionButton? optionButton, int itemId)
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
