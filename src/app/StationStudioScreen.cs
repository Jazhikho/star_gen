using Godot;

namespace StarGen.App;

/// <summary>
/// Lightweight placeholder for the upcoming dedicated station studio flow.
/// </summary>
public partial class StationStudioScreen : Control
{
	[Signal]
	public delegate void back_requestedEventHandler();

	private Label? _versionLabel;
	private Button? _backButton;

	/// <summary>
	/// Initializes the placeholder UI.
	/// </summary>
	public override void _Ready()
	{
		_versionLabel = GetNodeOrNull<Label>("MarginContainer/MainPanel/MarginContainer/VBox/HeaderRow/VersionLabel");
		_backButton = GetNodeOrNull<Button>("MarginContainer/MainPanel/MarginContainer/VBox/BackButton");

		if (_versionLabel != null)
		{
			string version = ProjectSettings.GetSetting("application/config/version", "0.4.3.4").AsString();
			_versionLabel.Text = $"Version {version}";
		}

		if (_backButton != null)
		{
			_backButton.Pressed += () => EmitSignal(SignalName.back_requested);
		}
	}
}
