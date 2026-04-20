using System.Text;
using System.Collections.Generic;
using Godot;
using StarGen.App.Shared;
using StarGen.Domain.Generation.Parameters;

namespace StarGen.App;

/// <summary>
/// Plain-language help popup for the system-generation studio.
/// </summary>
public partial class SystemGenerationScreen
{
	private Button? _helpButton;
	private Window? _helpDialog;
	private RichTextLabel? _helpDialogText;
	private Button? _helpDialogCloseButton;
	private Button? _systemSourcesButton;
	private Button? _stellarSourcesButton;
	private Button? _planetarySourcesButton;
	private Button? _lifeSourcesButton;

	private partial void CacheScienceHelpNodeReferences()
	{
		const string HeroRoot = "MarginContainer/ScrollContainer/Layout/HeroPanel/MarginContainer/HeroVBox/HeaderRow";
		const string ParameterRoot = "MarginContainer/ScrollContainer/Layout/MainPanel/MarginContainer/VBox/StudioRow/SettingsPanel/MarginContainer/SettingsVBox/ScrollContainer/ParameterVBox";
		_helpButton = GetNodeOrNull<Button>($"{HeroRoot}/HelpButton");
		_helpDialog = GetNodeOrNull<Window>("HelpDialog");
		_helpDialogText = GetNodeOrNull<RichTextLabel>("HelpDialog/MarginContainer/HelpVBox/HelpCard/MarginContainer/HelpDialogText");
		_helpDialogCloseButton = GetNodeOrNull<Button>("HelpDialog/MarginContainer/HelpVBox/ButtonRow/CloseButton");
		_systemSourcesButton = GetNodeOrNull<Button>($"{ParameterRoot}/SystemHeaderRow/SystemSourcesButton");
		_stellarSourcesButton = GetNodeOrNull<Button>($"{ParameterRoot}/StellarSection/StellarHeaderRow/StellarSourcesButton");
		_planetarySourcesButton = GetNodeOrNull<Button>($"{ParameterRoot}/PlanetarySection/PlanetaryHeaderRow/PlanetarySourcesButton");
		_lifeSourcesButton = GetNodeOrNull<Button>($"{ParameterRoot}/LifeSection/LifeHeaderRow/LifeSourcesButton");
	}

	private partial void ConnectScienceHelpSignals()
	{
		if (_helpButton != null)
		{
			_helpButton.Pressed += OnHelpPressed;
		}

		if (_helpDialogCloseButton != null)
		{
			_helpDialogCloseButton.Pressed += HideHelpDialog;
		}

		if (_helpDialog != null)
		{
			_helpDialog.CloseRequested += HideHelpDialog;
		}
	}

	private partial void InitializeScienceHelpUi()
	{
        if (_helpButton != null)
        {
            _helpButton.TooltipText = "Open plain-language help for system, star, and planet settings.\nThis guide includes examples of what changing each one does.";
        }

		if (_helpDialog != null)
		{
			_helpDialog.Visible = false;
		}

		if (_helpDialogText != null)
		{
			_helpDialogText.Text = BuildSystemHelpDialogBbCode();
		}

		ApplySectionSourceTooltips();
	}

	private void OnHelpPressed()
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

	private static string BuildSystemHelpDialogBbCode()
	{
		return $"{BuildSystemStudioBasicsBbCode()}\n\n{StellarScienceReferenceCatalog.BuildHelpPanelBbCode()}\n\n{PlanetaryScienceReferenceCatalog.BuildHelpPanelBbCode()}\n\n{LifeScienceReferenceCatalog.BuildHelpPanelBbCode()}";
	}

	private static string BuildSystemStudioBasicsBbCode()
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("[b][color=#f0c46a]System controls[/color][/b]");
		builder.AppendLine("[color=#c8d6e5]This studio decides what kind of star system you start with before planets and moons are built around it.[/color]");
		builder.AppendLine();
		builder.AppendLine("[b]Min Stars and Max Stars[/b]");
		builder.AppendLine("[color=#9cc4ff]What it means:[/color] This sets the allowed star count range.");
		builder.AppendLine("[color=#9cc4ff]What changing it does:[/color] Raising the maximum allows binaries, triples, and larger systems. Very high counts are rarer and usually make the system more crowded and complicated.");
		builder.AppendLine();
        builder.AppendLine("[b]Spectral Hints[/b]");
        builder.AppendLine("[color=#9cc4ff]What it means:[/color] A spectral class is a rough star type. O, B, A, F, G, K, and M are normal stars. L, T, and Y are brown dwarfs, which are too small to shine like true stars.");
        builder.AppendLine("[color=#9cc4ff]What changing it does:[/color] Adding hints pushes the generator toward those types. More M hints favors cool red dwarfs. L, T, or Y hints favor brown dwarfs. More A or B hints favors hotter, brighter stars.");
        builder.AppendLine();
        builder.AppendLine("[b]System Age[/b]");
        builder.AppendLine("[color=#9cc4ff]What it means:[/color] Age is how long the stars in the system have been evolving.");
        builder.AppendLine("[color=#9cc4ff]What changing it does:[/color] Older systems can now include stars that have swollen into subgiants or giants, and old low- or medium-mass stars can cool down into white dwarfs. Younger systems lean toward hotter, more active stars and newer surroundings.");
		builder.AppendLine();
		builder.AppendLine("[b]System Metallicity[/b]");
		builder.AppendLine("[color=#9cc4ff]What it means:[/color] In astronomy, metals are all elements heavier than hydrogen and helium. That includes the ingredients used to make dust, rock, and planets.");
		builder.AppendLine("[color=#9cc4ff]What changing it does:[/color] Higher metallicity usually means more raw material for rocky worlds and dust. Lower metallicity means a cleaner, more gas-heavy environment with fewer heavy ingredients.");
		return builder.ToString().TrimEnd();
	}

	private void ApplySectionSourceTooltips()
	{
		ApplySectionTooltip(
			_systemSourcesButton,
			"Sources for System Controls:\n- This section mixes deterministic generator controls with direct age and metallicity targets.\n- Seed, star-count bounds, spectral hints, and belt inclusion are generator constraints rather than literature-backed model choices.\n- The age and metallicity science that those targets feed into is sourced in the Stellar and Planetary Priors sections below.");

		ApplySectionTooltip(
			_stellarSourcesButton,
			BuildStellarSectionSourceTooltip(
				"Stellar Priors",
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
				"Planetary Priors",
				new[]
				{
					"planet_mass_radius_model",
					"planet_envelope_loss_model",
					"planet_gas_giant_formation_model",
					"planet_metallicity_coupling_strength",
					"planet_rogue_planet_allowance",
					"planet_moon_formation_bias",
					"planet_minor_body_outer_system_bias",
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
	}

	private static void ApplySectionTooltip(Control? control, string tooltipText)
	{
		if (control == null)
		{
			return;
		}

		control.TooltipText = tooltipText;
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
		StringBuilder builder = new();
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
