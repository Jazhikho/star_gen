namespace StarGen.App.Shared;

/// <summary>
/// Shared labels and descriptions for life and settlement permissiveness scales.
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
		return
			$"Lower values make {subject} rare. Mid-low values keep it less common but still plentiful. " +
			"0.50-0.74 matches Traveller-normal assumptions, while 0.75+ leans into space-opera density.";
	}
}
