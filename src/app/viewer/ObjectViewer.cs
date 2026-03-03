using Godot;
using StarGen.App.Rendering;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Math;

namespace StarGen.App.Viewer;

/// <summary>
/// C# object-viewer controller for the external-body viewing path.
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

	private Label? _statusLabel;
	private Node? _inspectorPanel;
	private OptionButton? _typeOption;
	private SpinBox? _seedInput;
	private HBoxContainer? _populationContainer;
	private OptionButton? _populationOption;
	private Button? _generateButton;
	private Button? _rerollButton;
	private Button? _saveButton;
	private Button? _loadButton;
	private Label? _fileInfo;
	private FileDialog? _saveFileDialog;
	private FileDialog? _loadFileDialog;
	private CameraController? _camera;
	private Node3D? _cameraRig;
	private Node3D? _cameraArm;
	private BodyRenderer? _bodyRenderer;
	private WorldEnvironment? _worldEnvironment;
	private Button? _backButton;
	private ObjectViewerMoonSystem? _moonSystem;
	private CelestialBody? _currentBody;
	private GodotObject? _gdCurrentBody;
	private readonly Godot.Collections.Array _gdCurrentMoons = new();
	private readonly Godot.Collections.Dictionary<string, GodotObject> _gdMoonById = new();
	private readonly Godot.Collections.Array<CelestialBody> _currentMoons = [];
	private float _primaryDisplayScale = 1.0f;
	private int _sourceStarSeed;
	private bool _navigatedFromSystem;

	/// <summary>
	/// Initializes the viewer state.
	/// </summary>
	public override void _Ready()
	{
		CacheNodeReferences();
		SetupViewport();
		SetupCamera();
		SetupMoonSystem();
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
		_navigatedFromSystem = true;
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

		ShowBackButton();
		SetGenerationControlsEnabled(false);
		SetFileControlsEnabled(false);
		DisplayBodyWithMoons(body, _currentMoons);

		string suffix = _currentMoons.Count > 0 ? $" with {_currentMoons.Count} moon(s)" : string.Empty;
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
	/// Clears the current display.
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
		SetStatus("No object loaded");
	}

	private void CacheNodeReferences()
	{
		_statusLabel = GetNodeOrNull<Label>("UI/TopBar/MarginContainer/HBoxContainer/StatusLabel");
		_inspectorPanel = GetNodeOrNull<Node>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer");
		_typeOption = GetNodeOrNull<OptionButton>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/TypeContainer/TypeOption");
		_seedInput = GetNodeOrNull<SpinBox>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SeedContainer/SeedInput");
		_populationContainer = GetNodeOrNull<HBoxContainer>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/PopulationContainer");
		_populationOption = GetNodeOrNull<OptionButton>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/PopulationContainer/PopulationOption");
		_generateButton = GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/ButtonContainer/GenerateButton");
		_rerollButton = GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/ButtonContainer/RerollButton");
		_saveButton = GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/FileSection/FileButtonContainer/SaveButton");
		_loadButton = GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/FileSection/FileButtonContainer/LoadButton");
		_fileInfo = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/FileSection/FileInfo");
		_saveFileDialog = GetNodeOrNull<FileDialog>("SaveFileDialog");
		_loadFileDialog = GetNodeOrNull<FileDialog>("LoadFileDialog");
		_cameraRig = GetNodeOrNull<Node3D>("CameraRig");
		_cameraArm = GetNodeOrNull<Node3D>("CameraRig/CameraArm");
		_camera = GetNodeOrNull<CameraController>("CameraRig/CameraArm/Camera3D");
		_bodyRenderer = GetNodeOrNull<BodyRenderer>("BodyRenderer");
		_worldEnvironment = GetNodeOrNull<WorldEnvironment>("Environment/WorldEnvironment");
	}

	private void SetupViewport()
	{
		Viewport? viewport = GetViewport();
		if (viewport != null)
		{
			viewport.UseHdr2D = true;
		}
	}

	private void SetupCamera()
	{
		if (_camera == null)
		{
			return;
		}

		_camera.SetDistance(10.0f);
		_camera.LookAt(Vector3.Zero, Vector3.Up);
	}

	private void SetupMoonSystem()
	{
		if (_bodyRenderer == null)
		{
			return;
		}

		_moonSystem = new ObjectViewerMoonSystem();
		_moonSystem.Setup(_bodyRenderer);
		_moonSystem.Connect("MoonFocused", Callable.From<Variant>(OnMoonFocusChangedVariant));
	}

	private void ConnectSignals()
	{
		if (_inspectorPanel is InspectorPanel typedInspectorPanel)
		{
			typedInspectorPanel.MoonSelected += OnInspectorMoonSelectedVariant;
		}
		else if (_inspectorPanel != null && _inspectorPanel.HasSignal("moon_selected"))
		{
			_inspectorPanel.Connect("moon_selected", Callable.From<Variant>(OnInspectorMoonSelectedVariant));
		}
	}

	private void DisplayBodyWithMoons(CelestialBody body, Godot.Collections.Array<CelestialBody> moons)
	{
		if (_bodyRenderer == null)
		{
			return;
		}

		_primaryDisplayScale = CalculateDisplayScale(body);
		_bodyRenderer.RenderBody(body, _primaryDisplayScale);

		AdjustLightingForBody(body);
		if (body.Type == CelestialType.Type.Star)
		{
			EnableStarGlow(body);
		}
		else
		{
			DisableStarGlow();
		}

		if (_moonSystem != null)
		{
			_moonSystem.SetPrimaryBody(body, _primaryDisplayScale);
			if (moons.Count > 0 && body.Type == CelestialType.Type.Planet)
			{
				_moonSystem.BuildMoonDisplay(moons, (float)body.Physical.AxialTiltDeg);
			}
			else
			{
				_moonSystem.Clear();
			}
		}

		FitCamera();
		UpdateInspector();
	}

	private float CalculateDisplayScale(CelestialBody body)
	{
		double radiusM = body.Physical.RadiusM;
		return body.Type switch
		{
			CelestialType.Type.Star => (float)(radiusM / Units.SolarRadiusMeters),
			CelestialType.Type.Planet or CelestialType.Type.Moon => (float)(radiusM / Units.EarthRadiusMeters),
			CelestialType.Type.Asteroid => (float)(radiusM / 1000.0),
			_ => 1.0f,
		};
	}

	private void FitCamera()
	{
		if (_camera == null || _moonSystem == null)
		{
			return;
		}

		float bodyRadius = _primaryDisplayScale;
		_camera.MinDistance = Mathf.Max(bodyRadius * 1.2f, 0.5f);
		_camera.SetTargetPosition(Vector3.Zero);
		_camera.SetDistance(_moonSystem.GetFramingDistance());
		_camera.FocusOnTarget();
	}

	private void FocusOnMoon(CelestialBody moon)
	{
		if (_moonSystem == null || !_moonSystem.FocusOnMoon(moon))
		{
			return;
		}

		if (_camera != null)
		{
			float moonRadius = _moonSystem.GetFocusedMoonDisplayRadius();
			_camera.MinDistance = Mathf.Max(moonRadius * 1.2f, 0.5f);
			_camera.SetTargetPosition(_moonSystem.GetFocusedMoonPosition());
			_camera.SetDistance(moonRadius * 4.0f);
		}

		UpdateInspector();
		SetStatus($"Focused: {moon.Name}");
	}

	private void FocusOnPlanet()
	{
		if (_moonSystem == null)
		{
			return;
		}

		_moonSystem.FocusOnPlanet();
		if (_camera != null)
		{
			float bodyRadius = _primaryDisplayScale;
			_camera.MinDistance = Mathf.Max(bodyRadius * 1.2f, 0.5f);
			_camera.SetTargetPosition(Vector3.Zero);
			_camera.SetDistance(_moonSystem.GetFramingDistance());
			_camera.FocusOnTarget();
		}

		UpdateInspector();
		if (_currentBody != null)
		{
			SetStatus($"Viewing: {_currentBody.Name}");
		}
	}

	private void UpdateInspector()
	{
		if (_inspectorPanel == null)
		{
			return;
		}

		if (_inspectorPanel is InspectorPanel typedInspectorPanel)
		{
			if (_moonSystem != null && _moonSystem.GetFocusedMoon() != null)
			{
				CelestialBody moon = _moonSystem.GetFocusedMoon()!;
				typedInspectorPanel.DisplayFocusedMoon(moon, _currentBody, _currentMoons, _gdCurrentMoons);
				return;
			}

			if (_currentBody != null)
			{
				typedInspectorPanel.DisplayBodyWithMoons(_currentBody, _currentMoons, _gdCurrentMoons);
				return;
			}

			typedInspectorPanel.Clear();
			return;
		}

		if (_moonSystem != null && _moonSystem.GetFocusedMoon() != null)
		{
			CelestialBody moon = _moonSystem.GetFocusedMoon()!;
			GodotObject? gdMoon = FindGdMoon(moon.Id);
			if (gdMoon != null && _gdCurrentBody != null)
			{
				_inspectorPanel.Call("display_focused_moon", gdMoon, _gdCurrentBody, _gdCurrentMoons);
				return;
			}
		}

		if (_gdCurrentBody != null)
		{
			_inspectorPanel.Call("display_body_with_moons", _gdCurrentBody, _gdCurrentMoons);
			return;
		}

		_inspectorPanel.Call("clear");
	}

	private GodotObject? FindGdMoon(string moonId)
	{
		return _gdMoonById.ContainsKey(moonId) ? (GodotObject)_gdMoonById[moonId] : null;
	}

	private void OnInspectorMoonSelectedVariant(Variant moonVariant)
	{
		if (moonVariant.VariantType == Variant.Type.Nil)
		{
			FocusOnPlanet();
			return;
		}

		CelestialBody? moon = ConvertVariantToCelestialBody(moonVariant);
		if (moon != null)
		{
			FocusOnMoon(moon);
		}
	}

	private void OnMoonFocusChangedVariant(Variant moonVariant)
	{
		EmitSignal(SignalName.MoonFocused, moonVariant);
	}

	private void ShowBackButton()
	{
		if (_backButton != null)
		{
			_backButton.Visible = true;
			return;
		}

		HBoxContainer? topBar = GetNodeOrNull<HBoxContainer>("UI/TopBar/MarginContainer/HBoxContainer");
		if (topBar == null)
		{
			return;
		}

		_backButton = new Button
		{
			Text = "<- Back to System",
			TooltipText = "Return to solar system viewer",
		};
		_backButton.Pressed += OnBackPressed;
		topBar.AddChild(_backButton);
		topBar.MoveChild(_backButton, 0);
	}

	private void OnBackPressed()
	{
		_navigatedFromSystem = false;
		if (_backButton != null)
		{
			_backButton.Visible = false;
		}

		EmitSignal(SignalName.BackToSystemRequested);
	}

	private void SetGenerationControlsEnabled(bool enabled)
	{
		if (_typeOption != null)
		{
			_typeOption.Disabled = !enabled;
		}

		if (_seedInput != null)
		{
			_seedInput.Editable = enabled;
		}

		if (_populationContainer != null)
		{
			_populationContainer.Visible = enabled;
		}

		if (_populationOption != null)
		{
			_populationOption.Disabled = !enabled;
		}

		if (_generateButton != null)
		{
			_generateButton.Disabled = !enabled;
		}

		if (_rerollButton != null)
		{
			_rerollButton.Disabled = !enabled;
		}
	}

	private void SetFileControlsEnabled(bool enabled)
	{
		if (_saveButton != null)
		{
			_saveButton.Disabled = !enabled;
		}

		if (_loadButton != null)
		{
			_loadButton.Disabled = !enabled;
		}

		if (_fileInfo != null && !enabled)
		{
			_fileInfo.Text = "C# object-viewer save/load is not wired yet";
		}

		if (_saveFileDialog != null)
		{
			_saveFileDialog.Visible = false;
		}

		if (_loadFileDialog != null)
		{
			_loadFileDialog.Visible = false;
		}
	}

	private void AdjustLightingForBody(CelestialBody body)
	{
		DirectionalLight3D? directionalLight = GetNodeOrNull<DirectionalLight3D>("Environment/DirectionalLight3D");
		if (directionalLight == null)
		{
			return;
		}

		directionalLight.LightEnergy = body.Type switch
		{
			CelestialType.Type.Star => 0.1f,
			CelestialType.Type.Asteroid => 1.0f,
			_ => 0.5f,
		};
	}

	private void EnableStarGlow(CelestialBody body)
	{
		if (_worldEnvironment?.Environment == null)
		{
			return;
		}

		Environment environment = _worldEnvironment.Environment;
		environment.GlowEnabled = true;
		environment.GlowIntensity = 1.0f;
		environment.GlowStrength = 1.2f;
		environment.GlowBloom = 0.5f;
		environment.GlowBlendMode = Environment.GlowBlendModeEnum.Screen;

		if (body.HasStellar() && body.Stellar != null)
		{
			float luminositySolar = (float)(body.Stellar.LuminosityWatts / 3.828e26);
			environment.GlowIntensity = Mathf.Clamp(luminositySolar, 0.5f, 2.0f);
		}
	}

	private void DisableStarGlow()
	{
		if (_worldEnvironment?.Environment != null)
		{
			_worldEnvironment.Environment.GlowEnabled = false;
		}
	}

	private static CelestialBody? ConvertVariantToCelestialBody(Variant bodyVariant)
	{
		if (bodyVariant.VariantType == Variant.Type.Nil)
		{
			return null;
		}

		GodotObject? godotObject = bodyVariant.AsGodotObject();
		if (godotObject is CelestialBody typedBody)
		{
			return typedBody;
		}

		if (godotObject != null && godotObject.HasMethod("to_dict"))
		{
			Variant dataVariant = godotObject.Call("to_dict");
			if (dataVariant.VariantType == Variant.Type.Dictionary)
			{
				return CelestialSerializer.FromDictionary((Godot.Collections.Dictionary)dataVariant);
			}
		}

		return null;
	}
}
