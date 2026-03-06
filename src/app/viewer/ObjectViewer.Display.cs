using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Math;

namespace StarGen.App.Viewer;

/// <summary>
/// Setup, rendering, and interaction helpers for ObjectViewer.
/// </summary>
public partial class ObjectViewer
{
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
		_bodyRenderer = GetNodeOrNull<App.Rendering.BodyRenderer>("BodyRenderer");
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

		_camera.SetTargetPosition(Vector3.Zero);
		_camera.FocusOnTarget();
		_camera.SetDistance(10.0f);
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
		if (body.Type == CelestialType.Type.Star)
		{
			return (float)(radiusM / Units.SolarRadiusMeters);
		}

		if (body.Type == CelestialType.Type.Planet || body.Type == CelestialType.Type.Moon)
		{
			return (float)(radiusM / Units.EarthRadiusMeters);
		}

		if (body.Type == CelestialType.Type.Asteroid)
		{
			return (float)(radiusM / 1000.0);
		}

		return 1.0f;
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
		UpdateFileInfoForCurrentTarget();
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
		UpdateFileInfoForCurrentTarget();
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
		if (_gdMoonById.ContainsKey(moonId))
		{
			return (GodotObject)_gdMoonById[moonId];
		}

		return null;
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

	private void ShowBackButton(string buttonText = "<- Back to System", string tooltipText = "Return to solar system viewer")
	{
		if (_backButton != null)
		{
			_backButton.Text = buttonText;
			_backButton.TooltipText = tooltipText;
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
			Text = buttonText,
			TooltipText = tooltipText,
		};
		_backButton.Pressed += OnBackPressed;
		topBar.AddChild(_backButton);
		topBar.MoveChild(_backButton, 0);
	}

	private void HideBackButton()
	{
		if (_backButton != null)
		{
			_backButton.Visible = false;
		}
	}

	private void OnBackPressed()
	{
		_navigatedFromSystem = false;
		HideBackButton();

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
			_fileInfo.Text = "No object selected";
		}
		else if (_fileInfo != null)
		{
			UpdateFileInfoForCurrentTarget();
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

		if (body.Type == CelestialType.Type.Star)
		{
			directionalLight.LightEnergy = 0.1f;
		}
		else if (body.Type == CelestialType.Type.Asteroid)
		{
			directionalLight.LightEnergy = 1.0f;
		}
		else
		{
			directionalLight.LightEnergy = 0.5f;
		}
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
