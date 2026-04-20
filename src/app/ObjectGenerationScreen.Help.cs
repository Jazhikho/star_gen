using System.Text;
using Godot;
using StarGen.App.Shared;
using StarGen.Domain.Generation.Parameters;

namespace StarGen.App;

/// <summary>
/// Plain-language help popup for Object Studio.
/// </summary>
public partial class ObjectGenerationScreen
{
    private Button? _helpButton;
    private Window? _helpDialog;
    private RichTextLabel? _helpDialogText;
    private Button? _helpDialogCloseButton;

    private void CacheHelpNodeReferences()
    {
        const string HeroRoot = "MarginContainer/ScrollContainer/Layout/HeroPanel/MarginContainer/HeroVBox/HeaderRow";
        _helpButton = GetNodeOrNull<Button>($"{HeroRoot}/HelpButton");
        _helpDialog = GetNodeOrNull<Window>("HelpDialog");
        _helpDialogText = GetNodeOrNull<RichTextLabel>("HelpDialog/MarginContainer/HelpVBox/HelpCard/MarginContainer/HelpDialogText");
        _helpDialogCloseButton = GetNodeOrNull<Button>("HelpDialog/MarginContainer/HelpVBox/ButtonRow/CloseButton");
    }

    private void ConnectObjectHelpSignals()
    {
        if (_helpButton != null)
        {
            _helpButton.Pressed += OnObjectHelpPressed;
        }

        if (_helpDialogCloseButton != null)
        {
            _helpDialogCloseButton.Pressed += HideObjectHelpDialog;
        }

        if (_helpDialog != null)
        {
            _helpDialog.CloseRequested += HideObjectHelpDialog;
        }
    }

    private void InitializeObjectHelpUi()
    {
        if (_helpButton != null)
        {
            _helpButton.TooltipText = "Open plain-language help for direct object settings.\nThis guide explains what direct locks change and why some combinations conflict.";
        }

        if (_helpDialog != null)
        {
            _helpDialog.Visible = false;
        }

        if (_helpDialogText != null)
        {
            _helpDialogText.Text = BuildObjectHelpDialogBbCode();
        }
    }

    private void OnObjectHelpPressed()
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

    private void HideObjectHelpDialog()
    {
        if (_helpDialog != null)
        {
            _helpDialog.Visible = false;
        }
    }

    private static string BuildObjectHelpDialogBbCode()
    {
        return $"{BuildObjectStudioBasicsBbCode()}\n\n{ObjectScienceReferenceCatalog.BuildHelpPanelBbCode()}\n\n{LifeScienceReferenceCatalog.BuildHelpPanelBbCode()}";
    }

    private static string BuildObjectStudioBasicsBbCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("[b][color=#f0c46a]Direct object controls[/color][/b]");
        builder.AppendLine("[color=#c8d6e5]Use Object Studio when you want one specific body, not a population trend. The controls here can override what StarGen would normally pick downstream.[/color]");
        builder.AppendLine();
        builder.AppendLine("[b]Planets[/b]");
        builder.AppendLine("[color=#9cc4ff]What it means:[/color] Planet controls let you directly target class, composition, atmosphere, hydrosphere, rings, moons, and life assumptions for one world.");
        builder.AppendLine("[color=#9cc4ff]What changing it does:[/color] These settings write straight into the generated `PlanetSpec`, so the viewer opens with the exact direct target you asked for whenever the generator can honor it.");
        builder.AppendLine();
        builder.AppendLine("[b]Stars, asteroids, and comets[/b]");
        builder.AppendLine("[color=#9cc4ff]What it means:[/color] These controls bias one object directly instead of changing aggregate system behavior.");
        builder.AppendLine("[color=#9cc4ff]What changing it does:[/color] Spectral class, asteroid family, comet family, and similar choices reshape the generated object itself, not the rest of a larger system.");
        builder.AppendLine();
        builder.AppendLine("[b]Preset Notes[/b]");
        builder.AppendLine("[color=#9cc4ff]What they mean:[/color] The notes panel now does two jobs: it explains what the current preset assumes, and it flags direct-setting combinations that fight known physical constraints.");
        builder.AppendLine("[color=#9cc4ff]What changing it does:[/color] If you ignore the warning, Object Studio still honors your direct lock. The note is there so you know when you are overriding the more realistic downstream path.");
        return builder.ToString().TrimEnd();
    }
}
