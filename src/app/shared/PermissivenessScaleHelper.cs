namespace StarGen.App.Shared;

/// <summary>
/// Shared labels and descriptions for the generation-side life permissiveness scale.
/// </summary>
public static class PermissivenessScaleHelper
{
	/// <summary>
	/// Returns a compact legend for the permissiveness scale.
	/// </summary>
	public static string GetLegendText()
	{
		return "Rare | Less common, still plentiful | Traveller normal | Space opera";
	}

	/// <summary>
	/// Returns the shared tooltip text for the advanced-assumptions info button.
	/// </summary>
	public static string GetAdvancedLegendTooltip()
	{
		return
			"Rare to space opera is a worldbuilding permissiveness scale, not a realism score.\n\n" +
			"Life Potential changes how strict native biosphere emergence is. " +
			"Colonization settings now live in the simulation tools instead of generation.";
	}

	/// <summary>
	/// Returns the named band for the provided slider value.
	/// </summary>
	public static string GetBandLabel(double value)
	{
		if (value < 0.25)
		{
			return "Rare";
		}

		if (value < 0.50)
		{
			return "Less common, still plentiful";
		}

		if (value < 0.75)
		{
			return "Traveller normal";
		}

		return "Space opera";
	}

	/// <summary>
	/// Returns tooltip text explaining how the scale bands map to generation intent.
	/// </summary>
	public static string GetTooltipText(string subject)
	{
		if (subject == "life")
		{
			return
				"Low values require near-Earthlike conditions before native life is likely.\n\n" +
				"High values allow life on marginal but still biologically plausible worlds, including more permissive ocean and subsurface cases.";
		}

		if (subject == "expansion")
		{
			return
				"Low values keep expansion focused on the best worlds.\n\n" +
				"High values make sealed habitats, moons, and other harsh but workable locations much more likely to attract settlements during colonization simulation.";
		}

		return
			$"Lower values make {subject} rare. Mid-low values keep it less common but still plentiful. " +
			"0.50-0.74 matches Traveller-normal assumptions, while 0.75+ leans into space-opera density.";
	}
}
