using Godot;
using StarGen.App.Rendering;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Validation;

namespace StarGen.App.Viewer;

/// <summary>
/// C# object-viewer controller for the external-body viewing path.
/// Display and interaction helpers in ObjectViewer.Display.cs.
/// </summary>
public partial class ObjectViewer : Node3D
{
	/// <summary>
	/// Emitted when the user wants to return to the system viewer.
	/// </summary>
	[Signal]
	public delegate void BackToSystemRequestedEventHandler();

	/// <summary>
	/// Emitted when focus shifts to a moon. Null means focus returned to the primary body.
	/// </summary>
	[Signal]
	public delegate void MoonFocusedEventHandler(GodotObject moon);

	/// <summary>
	/// Placeholder edit signal to preserve scene contract as the edit flow migrates.
	/// </summary>
	[Signal]
	public delegate void BodyEditedEventHandler(GodotObject body, int starSeed);

	/// <summary>
	/// Whether the primary body should rotate.
	/// </summary>
	[Export]
	public bool AnimateRotation = true;

	/// <summary>
	/// Rotation speed multiplier for the rendered body.
	/// </summary>
	[Export]
	public float RotationSpeed = 1.0f;

	internal Label? _statusLabel;
	internal Control? _uiRoot;
	internal Control? _topBar;
	internal Control? _sidePanel;
	internal Node? _inspectorPanel;
	internal OptionButton? _typeOption;
	internal OptionButton? _presetOption;
	internal Label? _presetAssumptionsLabel;
	internal SpinBox? _seedInput;
	internal HBoxContainer? _populationContainer;
	internal OptionButton? _populationOption;
	internal Button? _generateButton;
	internal Button? _rerollButton;
	internal Button? _saveButton;
	internal Button? _loadButton;
	internal Label? _fileInfo;
	internal FileDialog? _saveFileDialog;
	internal FileDialog? _loadFileDialog;
	internal CameraController? _camera;
	internal Node3D? _cameraRig;
	internal Node3D? _cameraArm;
	internal BodyRenderer? _bodyRenderer;
	internal WorldEnvironment? _worldEnvironment;
	internal Button? _backButton;
	internal EditDialog? _editDialog;
	internal ObjectViewerMoonSystem? _moonSystem;
	internal CelestialBody? _currentBody;
	internal GodotObject? _gdCurrentBody;
	internal readonly Godot.Collections.Array _gdCurrentMoons = new();
	internal readonly Godot.Collections.Dictionary<string, GodotObject> _gdMoonById = new();
	internal readonly Godot.Collections.Array<CelestialBody> _currentMoons = [];
	internal float _primaryDisplayScale = 1.0f;
	internal int _sourceStarSeed;
	internal bool _navigatedFromSystem;
	internal Rect2 _renderAreaRect = new Rect2();

	/// <summary>
	/// Initializes the viewer state.
	/// </summary>
	public override void _Ready()
	{
		CacheNodeReferences();
		SetupViewport();
		SetupCamera();
		SetupMoonSystem();
		SetupControls();
		SetupTopMenu();
		ConnectSignals();
		SetGenerationControlsEnabled(false);
		SetFileControlsEnabled(false);
		SetStatus("Object viewer initialized");
	}

	/// <summary>
	/// Advances body rotation and moon animation.
	/// </summary>
	public override void _Process(double delta)
	{
		UpdatePanelAwareFraming();

		if (AnimateRotation && _bodyRenderer != null && _currentBody != null)
		{
			_bodyRenderer.RotateBody((float)delta, RotationSpeed);
		}

		if (_moonSystem != null && _moonSystem.HasMoons())
		{
			_moonSystem.UpdateOrbitalPositions((float)delta);
			if (_moonSystem.GetFocusedMoon() != null && _camera != null)
			{
				_camera.SetTargetPosition(_moonSystem.GetFocusedMoonPosition());
			}
		}
	}

	/// <summary>
	/// Handles click-to-focus for displayed moons.
	/// </summary>
	public override void _UnhandledInput(InputEvent @event)
	{
		if (_moonSystem == null || !_moonSystem.HasMoons() || _camera == null)
		{
			return;
		}

		if (@event is not InputEventMouseButton mouseEvent || !mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
		{
			return;
		}

		CelestialBody? clickedMoon = _moonSystem.DetectMoonClick(_camera, mouseEvent.Position);
		if (clickedMoon != null)
		{
			FocusOnMoon(clickedMoon);
			GetViewport()?.SetInputAsHandled();
		}
	}

	/// <summary>
	/// GDScript-compatible wrapper for external body display.
	/// </summary>
	public void display_external_body(Variant bodyVariant, Godot.Collections.Array moons, int starSeed = 0)
	{
		DisplayExternalBody(bodyVariant, moons, starSeed);
	}

	/// <summary>
	/// Displays an externally provided body and optional moons.
	/// </summary>
	public void DisplayExternalBody(Variant bodyVariant, Godot.Collections.Array moons, int starSeed = 0)
	{
		CelestialBody? body = ConvertVariantToCelestialBody(bodyVariant);
		if (body == null)
		{
			return;
		}

		_currentBody = body;
		_gdCurrentBody = bodyVariant.AsGodotObject();
		_sourceStarSeed = starSeed;
		_navigatedFromSystem = starSeed != 0;
		_currentMoons.Clear();
		_gdCurrentMoons.Clear();
		_gdMoonById.Clear();

		foreach (Variant moonVariant in moons)
		{
			CelestialBody? moon = ConvertVariantToCelestialBody(moonVariant);
			if (moon != null)
			{
				_currentMoons.Add(moon);
			}

			GodotObject? gdMoon = moonVariant.AsGodotObject();
			if (gdMoon != null)
			{
				_gdCurrentMoons.Add(gdMoon);
				if (moon != null)
				{
					_gdMoonById[moon.Id] = gdMoon;
				}
			}
		}

		if (_navigatedFromSystem)
		{
			ShowBackButton("<- Back to System", "Return to the system viewer");
		}
		else if (_backButton != null && _backButton.Visible)
		{
			ShowBackButton(_backButton.Text, _backButton.TooltipText);
		}
		else
		{
			HideBackButton();
		}
		SetGenerationControlsEnabled(false);
		SetFileControlsEnabled(true);
		DisplayBodyWithMoons(body, _currentMoons);

		string suffix;
		if (_currentMoons.Count > 0)
		{
			suffix = $" with {_currentMoons.Count} moon(s)";
		}
		else
		{
			suffix = string.Empty;
		}
		SetStatus($"Viewing: {body.Name}{suffix} (from system)");
	}

	/// <summary>
	/// GDScript-compatible status wrapper.
	/// </summary>
	public void set_status(string message)
	{
		SetStatus(message);
	}

	/// <summary>
	/// Updates the status label.
	/// </summary>
	public void SetStatus(string message)
	{
		if (_statusLabel != null)
		{
			_statusLabel.Text = message;
			_statusLabel.Modulate = new Color(0.7f, 0.7f, 0.7f, 1.0f);
		}
	}

	/// <summary>
	/// GDScript-compatible display clear wrapper.
	/// </summary>
	public void clear_display()
	{
		ClearDisplay();
	}

	/// <summary>
	/// Clears the current display.
	/// </summary>
	public void ClearDisplay()
	{
		_currentBody = null;
		_gdCurrentBody = null;
		_currentMoons.Clear();
		_gdCurrentMoons.Clear();
		_gdMoonById.Clear();

		_moonSystem?.Clear();
		_bodyRenderer?.Clear();
		DisableStarGlow();
		UpdateInspector();
		SetFileControlsEnabled(false);
		SetStatus("No object loaded");
	}

	/// <summary>
	/// Shows or hides the top-level back button with caller-provided text.
	/// </summary>
	public void SetBackNavigationVisibility(bool visible, string buttonText = "<- Back", string tooltipText = "Return")
	{
		if (visible)
		{
			ShowBackButton(buttonText, tooltipText);
			return;
		}

		HideBackButton();
	}

	/// <summary>
	/// Prepares the object viewer for standalone generation launched from the main menu.
	/// </summary>
	public void PrepareStandaloneGenerator(int seedValue, ObjectType defaultType)
	{
		_navigatedFromSystem = false;
		SetGenerationControlsEnabled(true);
		SetFileControlsEnabled(false);

		if (_seedInput != null)
		{
			_seedInput.Value = seedValue;
		}

		if (_typeOption != null)
		{
			_typeOption.Select((int)defaultType);
		}

		if (_presetOption != null)
		{
			_presetOption.Select(0);
		}

		GenerateObjectFromPreset(defaultType, seedValue);
		_navigatedFromSystem = false;
		ShowBackButton("<- Back to Menu", "Return to the main menu");
	}
}
