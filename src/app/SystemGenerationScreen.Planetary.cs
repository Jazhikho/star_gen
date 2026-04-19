using Godot;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;

namespace StarGen.App;

/// <summary>
/// Aggregate planetary-model control wiring for the system-generation studio.
/// </summary>
public partial class SystemGenerationScreen
{
	private OptionButton? _planetMassRadiusModelOption;
	private OptionButton? _planetEnvelopeLossModelOption;
	private OptionButton? _planetGasGiantFormationModelOption;
	private OptionButton? _planetMetallicityCouplingOption;
	private OptionButton? _planetRogueAllowanceOption;
	private OptionButton? _planetMoonFormationBiasOption;
	private OptionButton? _planetMinorBodyOuterBiasOption;

	private void CachePlanetaryNodeReferences()
	{
		const string Root = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryContent/PlanetaryVBox";
		_planetMassRadiusModelOption = GetNodeOrNull<OptionButton>($"{Root}/MassRadiusRow/MassRadiusOption");
		_planetEnvelopeLossModelOption = GetNodeOrNull<OptionButton>($"{Root}/EnvelopeLossRow/EnvelopeLossOption");
		_planetGasGiantFormationModelOption = GetNodeOrNull<OptionButton>($"{Root}/GasGiantFormationRow/GasGiantFormationOption");
		_planetMetallicityCouplingOption = GetNodeOrNull<OptionButton>($"{Root}/MetallicityCouplingRow/MetallicityCouplingOption");
		_planetRogueAllowanceOption = GetNodeOrNull<OptionButton>($"{Root}/RogueAllowanceRow/RogueAllowanceOption");
		_planetMoonFormationBiasOption = GetNodeOrNull<OptionButton>($"{Root}/MoonFormationBiasRow/MoonFormationBiasOption");
		_planetMinorBodyOuterBiasOption = GetNodeOrNull<OptionButton>($"{Root}/OuterBodyBiasRow/OuterBodyBiasOption");
	}

	private void ConnectPlanetarySignals()
	{
		if (_planetMassRadiusModelOption != null) _planetMassRadiusModelOption.ItemSelected += _ => RefreshSummary();
		if (_planetEnvelopeLossModelOption != null) _planetEnvelopeLossModelOption.ItemSelected += _ => RefreshSummary();
		if (_planetGasGiantFormationModelOption != null) _planetGasGiantFormationModelOption.ItemSelected += _ => RefreshSummary();
		if (_planetMetallicityCouplingOption != null) _planetMetallicityCouplingOption.ItemSelected += _ => RefreshSummary();
		if (_planetRogueAllowanceOption != null) _planetRogueAllowanceOption.ItemSelected += _ => RefreshSummary();
		if (_planetMoonFormationBiasOption != null) _planetMoonFormationBiasOption.ItemSelected += _ => RefreshSummary();
		if (_planetMinorBodyOuterBiasOption != null) _planetMinorBodyOuterBiasOption.ItemSelected += _ => RefreshSummary();
	}

	private void ApplyPlanetaryDefaults()
	{
		ApplyPlanetaryProfileToControls(PlanetaryGenerationProfile.CreateDefault());
	}

	private void ApplyPlanetaryParameterTooltips()
	{
		const string Root = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox/PlanetarySection/PlanetaryContent/PlanetaryVBox";
		ApplyPlanetaryTooltip("planet_mass_radius_model", _planetMassRadiusModelOption, $"{Root}/MassRadiusRow/MassRadiusLabel");
		ApplyPlanetaryTooltip("planet_envelope_loss_model", _planetEnvelopeLossModelOption, $"{Root}/EnvelopeLossRow/EnvelopeLossLabel");
		ApplyPlanetaryTooltip("planet_gas_giant_formation_model", _planetGasGiantFormationModelOption, $"{Root}/GasGiantFormationRow/GasGiantFormationLabel");
		ApplyPlanetaryTooltip("planet_metallicity_coupling_strength", _planetMetallicityCouplingOption, $"{Root}/MetallicityCouplingRow/MetallicityCouplingLabel");
		ApplyPlanetaryTooltip("planet_rogue_planet_allowance", _planetRogueAllowanceOption, $"{Root}/RogueAllowanceRow/RogueAllowanceLabel");
		ApplyPlanetaryTooltip("planet_moon_formation_bias", _planetMoonFormationBiasOption, $"{Root}/MoonFormationBiasRow/MoonFormationBiasLabel");
		ApplyPlanetaryTooltip("planet_minor_body_outer_system_bias", _planetMinorBodyOuterBiasOption, $"{Root}/OuterBodyBiasRow/OuterBodyBiasLabel");
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
		SelectOptionId(_planetMassRadiusModelOption, (int)profile.MassRadiusModel);
		SelectOptionId(_planetEnvelopeLossModelOption, (int)profile.EnvelopeLossModel);
		SelectOptionId(_planetGasGiantFormationModelOption, (int)profile.GasGiantFormationModel);
		SelectOptionId(_planetMetallicityCouplingOption, (int)profile.MetallicityCouplingStrength);
		SelectOptionId(_planetRogueAllowanceOption, (int)profile.RoguePlanetAllowance);
		SelectOptionId(_planetMoonFormationBiasOption, (int)profile.MoonFormationBias);
		SelectOptionId(_planetMinorBodyOuterBiasOption, (int)profile.MinorBodyOuterSystemBias);
	}

	private static string BuildPlanetaryProfileSummary(PlanetaryGenerationProfile profile)
	{
		return $"Planet model: Size {profile.MassRadiusModel} | Loss {profile.EnvelopeLossModel} | Giants {profile.GasGiantFormationModel} | Metallicity {profile.MetallicityCouplingStrength} | Rogue {profile.RoguePlanetAllowance} | Moons {profile.MoonFormationBias}";
	}

	private void ApplyPlanetaryTooltip(string parameterId, Control? inputControl, string labelPath)
	{
		string tooltip = GetPlanetaryParameterAssumption(parameterId);
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

	private static string GetPlanetaryParameterAssumption(string parameterId)
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
}
