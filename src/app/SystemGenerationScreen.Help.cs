using System.Text;
using Godot;
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

	private partial void CacheScienceHelpNodeReferences()
	{
		const string HeroRoot = "MarginContainer/ScrollContainer/Layout/HeroPanel/MarginContainer/HeroVBox/HeaderRow";
		_helpButton = GetNodeOrNull<Button>($"{HeroRoot}/HelpButton");
		_helpDialog = GetNodeOrNull<Window>("HelpDialog");
		_helpDialogText = GetNodeOrNull<RichTextLabel>("HelpDialog/MarginContainer/HelpVBox/HelpCard/MarginContainer/HelpDialogText");
		_helpDialogCloseButton = GetNodeOrNull<Button>("HelpDialog/MarginContainer/HelpVBox/ButtonRow/CloseButton");
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
            _helpButton.TooltipText = "Open plain-language help for system and star settings.\nThis guide includes examples of what changing each one does.";
        }

		if (_helpDialog != null)
		{
			_helpDialog.Visible = false;
		}

		if (_helpDialogText != null)
		{
			_helpDialogText.Text = BuildSystemHelpDialogBbCode();
		}
	}

	private void OnHelpPressed()
	{
		if (_helpDialog == null)
		{
			return;
		}

		_helpDialog.Size = new Vector2I(760, 580);
		if (_helpDialogText != null)
		{
			_helpDialogText.ScrollToLine(0);
		}

		_helpDialog.Visible = true;
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
		return $"{BuildSystemStudioBasicsBbCode()}\n\n{StellarScienceReferenceCatalog.BuildHelpPanelBbCode()}";
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
}
