using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;
using System.Collections.Generic;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Main viewer scene for inspecting solar systems.
/// </summary>
public partial class SystemViewer : Node3D
{
    /// <summary>
    /// Emitted when a body should be opened in the object viewer.
    /// </summary>
    [Signal]
    public delegate void OpenBodyInViewerEventHandler(GodotObject body, Godot.Collections.Array moons, int starSeed);

    /// <summary>
    /// Emitted when the user wants to return to the galaxy viewer.
    /// </summary>
    [Signal]
    public delegate void BackToGalaxyRequestedEventHandler();

    private static readonly Vector3 InvalidPosition = new(1.0e20f, 1.0e20f, 1.0e20f);
    private const string SystemBodyNodeScenePath = "res://src/app/system_viewer/SystemBodyNodeCSharp.tscn";
    private const string GdSolarSystemScriptPath = "res://src/domain/system/SolarSystem.gd";
    private static readonly GDScript? GdSolarSystemScript = ResourceLoader.Load<GDScript>(GdSolarSystemScriptPath);

    private Label? _statusLabel;
    private Node? _inspectorPanel;
    private SpinBox? _starCountSpin;
    private SpinBox? _seedInput;
    private Button? _generateButton;
    private Button? _rerollButton;
    private Button? _saveButton;
    private Button? _loadButton;
    private CheckBox? _showOrbitsCheck;
    private CheckBox? _showZonesCheck;
    private Node? _cameraController;
    private Node3D? _bodiesContainer;
    private Node3D? _orbitsContainer;
    private Node3D? _zonesContainer;
    private Node? _orbitRenderer;
    private Node3D? _beltRenderer;
    private PackedScene? _systemBodyNodeScene;

    private SolarSystem? _currentSystem;
    private SystemLayout? _currentLayout;
    private string _selectedBodyId = string.Empty;
    private string _selectedBeltId = string.Empty;
    private readonly Dictionary<string, Node3D> _bodyNodes = new();
    private bool _animationEnabled = true;
    private bool _isUpdatingSystem;
    private bool _isReady;
    private int _sourceStarSeed;
    private readonly SystemViewerSaveLoad _saveLoad = new();

    /// <summary>
    /// Initializes the viewer state once the scene tree is ready.
    /// </summary>
    public override void _Ready()
    {
        CacheNodeReferences();
        SetupViewport();
        SetupCamera();
        SetupGenerationUi();
        SetupViewUi();
        SetupOrbitRenderer();
        SetupBeltRenderer();
        SetupSaveLoadUi();
        SetupTooltips();
        ConnectSignals();

        SetStatus("System viewer initialized");
        _isReady = true;
        OnGeneratePressed();
    }

    /// <summary>
    /// Updates orbital animation each frame.
    /// </summary>
    public override void _Process(double delta)
    {
        if (_animationEnabled && _currentLayout != null)
        {
            UpdateOrbitalAnimation((float)delta);
        }
    }

    /// <summary>
    /// Handles keyboard shortcuts routed by Godot.
    /// </summary>
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        HandleUnhandledKeyInput(@event);
    }

    /// <summary>
    /// Updates orbital animation for all bodies and orbit paths.
    /// </summary>
    public void UpdateOrbitalAnimation(float delta)
    {
        if (_isUpdatingSystem || _currentSystem == null || _currentLayout == null)
        {
            return;
        }

        SystemDisplayLayout.UpdateOrbits(_currentLayout, delta);

        List<string> toRemove = new();
        foreach (string bodyId in _bodyNodes.Keys)
        {
            Node3D bodyNode = _bodyNodes[bodyId];
            if (!IsInstanceValid(bodyNode))
            {
                toRemove.Add(bodyId);
                continue;
            }

            BodyLayout? layout = _currentLayout.GetBodyLayout(bodyId);
            if (layout != null)
            {
                bodyNode.GlobalPosition = layout.Position;
            }
        }

        foreach (string bodyId in toRemove)
        {
            _bodyNodes.Remove(bodyId);
        }

        if (_orbitRenderer is OrbitRenderer typedOrbitRenderer)
        {
            typedOrbitRenderer.UpdateOrbitPositions(BuildHostPositionsDictionary(_currentLayout.HostPositions));
        }
        else if (_orbitRenderer != null)
        {
            _orbitRenderer.Call("update_orbit_positions", BuildHostPositionsDictionary(_currentLayout.HostPositions));
        }

        if (_beltRenderer is BeltRenderer typedBeltRenderer)
        {
            typedBeltRenderer.UpdateBeltPositions(BuildHostPositionsDictionary(_currentLayout.HostPositions));
            typedBeltRenderer.UpdateBeltRotation(delta);
        }
    }

    /// <summary>
    /// Generates a solar system from UI settings.
    /// </summary>
    public void GenerateSystem(int seedValue, int minStars = 1, int maxStars = 1)
    {
        if (_isUpdatingSystem)
        {
            return;
        }

        SetStatus($"Generating system with seed {seedValue}...");
        SolarSystemSpec spec = new(seedValue, minStars, maxStars);
        SolarSystem? system = SystemFixtureGenerator.GenerateSystem(spec, true);
        if (system == null)
        {
            SetError("Failed to generate system");
            return;
        }

        DisplaySystem(system);
        SetStatus($"Generated: {system.GetSummary()}");
    }

    /// <summary>
    /// Displays a generated solar system.
    /// </summary>
    public void DisplaySystem(SolarSystem? system)
    {
        _isUpdatingSystem = true;

        if (system == null)
        {
            ClearDisplay();
            _isUpdatingSystem = false;
            return;
        }

        _currentSystem = system;
        _currentLayout = SystemDisplayLayout.CalculateLayout(system);

        ClearBodies();
        UpdateSaveButtonState();
        ClearOrbits();
        ClearBelts();
        ClearZones();

        CreateBeltVisualizations();
        CreateBodyNodes();
        CreateOrbitVisualizations();

        if (_showZonesCheck != null && _showZonesCheck.ButtonPressed)
        {
            CreateZoneVisualizations();
        }

        UpdateInspectorSystem();
        FitCameraToSystem();

        if (!string.IsNullOrEmpty(system.Name))
        {
            SetStatus($"Viewing: {system.Name}");
        }
        else
        {
            SetStatus($"Generated: {system.GetSummary()}");
        }

        _isUpdatingSystem = false;
    }

    /// <summary>
    /// Fits the camera to the current system extent.
    /// </summary>
    public void FitCameraToSystem()
    {
        if (_cameraController == null)
        {
            return;
        }

        if (_currentSystem == null || _currentLayout == null)
        {
            if (_cameraController is SystemCameraController typedCameraController)
            {
                typedCameraController.FocusOnOrigin();
            }
            else
            {
                _cameraController.Call("focus_on_origin");
            }

            return;
        }

        float maxExtent = _currentLayout.TotalExtent;
        if (maxExtent < 10.0f)
        {
            maxExtent = 20.0f;
        }

        float targetHeight = maxExtent * 2.0f;
        if (_cameraController is SystemCameraController typedCamera)
        {
            if (typedCamera.MaxHeight > 0.0f)
            {
                targetHeight = Mathf.Min(targetHeight, typedCamera.MaxHeight);
            }

            targetHeight = Mathf.Max(targetHeight, 20.0f);
            typedCamera.MinHeight = Mathf.Min(10.0f, targetHeight * 0.1f);
            if (targetHeight > typedCamera.MaxHeight)
            {
                typedCamera.MaxHeight = targetHeight * 1.5f;
            }

            typedCamera.ApplyViewState(Vector3.Zero, targetHeight, Mathf.DegToRad(60.0f), 0.0f);
            return;
        }

        Variant maxHeightVariant = _cameraController.Get("max_height");
        if (maxHeightVariant.VariantType == Variant.Type.Float && (float)(double)maxHeightVariant > 0.0f)
        {
            targetHeight = Mathf.Min(targetHeight, (float)(double)maxHeightVariant);
        }

        targetHeight = Mathf.Max(targetHeight, 20.0f);
        _cameraController.Set("min_height", Mathf.Min(10.0f, targetHeight * 0.1f));

        Variant maxHeightNow = _cameraController.Get("max_height");
        if (maxHeightNow.VariantType == Variant.Type.Float && targetHeight > (float)(double)maxHeightNow)
        {
            _cameraController.Set("max_height", targetHeight * 1.5f);
        }

        _cameraController.Set("_target_position", Vector3.Zero);
        _cameraController.Set("_target_height", targetHeight);
        _cameraController.Set("_height", targetHeight);
        _cameraController.Set("_target_pitch", Mathf.DegToRad(60.0f));
        _cameraController.Set("_yaw", 0.0f);
        _cameraController.Set("_smooth_target", Vector3.Zero);
    }

    /// <summary>
    /// Clears the current system display.
    /// </summary>
    public void ClearDisplay()
    {
        _currentSystem = null;
        _currentLayout = null;
        _selectedBodyId = string.Empty;
        _selectedBeltId = string.Empty;

        ClearBodies();
        ClearOrbits();
        ClearBelts();
        ClearZones();
        UpdateSaveButtonState();

        SetStatus("No system loaded");
    }

    /// <summary>
    /// Selects a body by identifier.
    /// </summary>
    public void SelectBody(string bodyId)
    {
        _selectedBeltId = string.Empty;

        if (!string.IsNullOrEmpty(_selectedBodyId) && _bodyNodes.ContainsKey(_selectedBodyId))
        {
            SetBodyNodeSelected(_bodyNodes[_selectedBodyId], false);
        }

        _selectedBodyId = bodyId;
        if (_bodyNodes.ContainsKey(bodyId))
        {
            Node3D node = _bodyNodes[bodyId];
            SetBodyNodeSelected(node, true);
            if (_cameraController is SystemCameraController typedCameraController)
            {
                typedCameraController.FocusOnPosition(node.GlobalPosition);
            }
            else
            {
                _cameraController?.Call("focus_on_position", node.GlobalPosition);
            }
        }

        if (_orbitRenderer is OrbitRenderer typedOrbitRenderer)
        {
            typedOrbitRenderer.HighlightOrbit(bodyId);
        }
        else
        {
            _orbitRenderer?.Call("highlight_orbit", bodyId);
        }
        UpdateInspectorBody();
        SetStatus($"Selected: {bodyId}");
    }

    /// <summary>
    /// Deselects the current body.
    /// </summary>
    public void DeselectBody()
    {
        _selectedBeltId = string.Empty;

        if (!string.IsNullOrEmpty(_selectedBodyId) && _bodyNodes.ContainsKey(_selectedBodyId))
        {
            SetBodyNodeSelected(_bodyNodes[_selectedBodyId], false);
        }

        _selectedBodyId = string.Empty;
        if (_orbitRenderer is OrbitRenderer typedOrbitRenderer)
        {
            typedOrbitRenderer.HighlightOrbit(string.Empty);
        }
        else
        {
            _orbitRenderer?.Call("highlight_orbit", string.Empty);
        }
        UpdateInspectorSystem();
    }

    /// <summary>
    /// Sets the status message.
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
    /// Sets an error status message.
    /// </summary>
    public void SetError(string message)
    {
        if (_statusLabel != null)
        {
            _statusLabel.Text = "Error: " + message;
            _statusLabel.Modulate = new Color(1.0f, 0.3f, 0.3f);
        }

        GD.PushError(message);
    }

    /// <summary>
    /// Returns the currently displayed system.
    /// </summary>
    public SolarSystem? GetCurrentSystem()
    {
        return _currentSystem;
    }

    /// <summary>
    /// Sets the source galaxy-star seed for this system.
    /// </summary>
    public void SetSourceStarSeed(int starSeed)
    {
        _sourceStarSeed = starSeed;
    }

    /// <summary>
    /// Updates the displayed seed UI value.
    /// </summary>
    public void UpdateSeedDisplay(int seedValue)
    {
        if (_seedInput != null)
        {
            _seedInput.Value = seedValue;
        }
    }

    /// <summary>
    /// Returns the save/load helper instance.
    /// </summary>
    public SystemViewerSaveLoad GetSaveLoad()
    {
        return _saveLoad;
    }

    /// <summary>
    /// Enables or disables orbital animation.
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        _animationEnabled = enabled;
    }

    /// <summary>
    /// GDScript-compatible wrapper for system generation.
    /// </summary>
    public void generate_system(int seedValue, int minStars = 1, int maxStars = 1)
    {
        GenerateSystem(seedValue, minStars, maxStars);
    }

    /// <summary>
    /// GDScript-compatible wrapper for system display.
    /// </summary>
    public void display_system(Variant systemVariant)
    {
        DisplaySystem(ConvertVariantToSolarSystem(systemVariant));
    }

    /// <summary>
    /// GDScript-compatible wrapper for camera fitting.
    /// </summary>
    public void fit_camera_to_system()
    {
        FitCameraToSystem();
    }

    /// <summary>
    /// GDScript-compatible wrapper for clearing the current display.
    /// </summary>
    public void clear_display()
    {
        ClearDisplay();
    }

    /// <summary>
    /// GDScript-compatible wrapper for body selection.
    /// </summary>
    public void select_body(string bodyId)
    {
        SelectBody(bodyId);
    }

    /// <summary>
    /// GDScript-compatible wrapper for body deselection.
    /// </summary>
    public void deselect_body()
    {
        DeselectBody();
    }

    /// <summary>
    /// GDScript-compatible wrapper for status updates.
    /// </summary>
    public void set_status(string message)
    {
        SetStatus(message);
    }

    /// <summary>
    /// GDScript-compatible wrapper for error updates.
    /// </summary>
    public void set_error(string message)
    {
        SetError(message);
    }

    /// <summary>
    /// GDScript-compatible wrapper for reading the displayed system.
    /// </summary>
    public GodotObject? get_current_system()
    {
        return ConvertSolarSystemToGdObject(GetCurrentSystem());
    }

    /// <summary>
    /// GDScript-compatible wrapper for star-seed context.
    /// </summary>
    public void set_source_star_seed(int starSeed)
    {
        SetSourceStarSeed(starSeed);
    }

    /// <summary>
    /// GDScript-compatible wrapper for seed-display updates.
    /// </summary>
    public void update_seed_display(int seedValue)
    {
        UpdateSeedDisplay(seedValue);
    }

    /// <summary>
    /// GDScript-compatible wrapper for save/load helper access.
    /// </summary>
    public SystemViewerSaveLoad get_save_load()
    {
        return GetSaveLoad();
    }

    /// <summary>
    /// GDScript-compatible wrapper for animation toggles.
    /// </summary>
    public void set_animation_enabled(bool enabled)
    {
        SetAnimationEnabled(enabled);
    }

    /// <summary>
    /// GDScript-compatible wrapper for the private generate callback.
    /// </summary>
    public void _on_generate_pressed()
    {
        OnGeneratePressed();
    }

    /// <summary>
    /// GDScript-compatible wrapper for the private reroll callback.
    /// </summary>
    public void _on_reroll_pressed()
    {
        OnRerollPressed();
    }

    /// <summary>
    /// GDScript-compatible wrapper for tooltip setup.
    /// </summary>
    public void _setup_tooltips()
    {
        SetupTooltips();
    }

    /// <summary>
    /// GDScript-compatible wrapper for keyboard shortcuts.
    /// </summary>
    public void _unhandled_key_input(InputEvent @event)
    {
        HandleUnhandledKeyInput(@event);
    }

    /// <summary>
    /// GDScript-compatible wrapper for major-asteroid node creation.
    /// </summary>
    public void _create_major_asteroid_node_at(Variant asteroidVariant, Vector3 displayPosition)
    {
        CelestialBody? asteroid = ConvertVariantToCelestialBody(asteroidVariant);
        if (asteroid != null)
        {
            CreateMajorAsteroidNodeAt(asteroid, displayPosition);
        }
    }

    /// <summary>
    /// GDScript-compatible wrapper for major-asteroid display mapping.
    /// </summary>
    public Vector3 _get_major_asteroid_display_position(Variant asteroidVariant)
    {
        CelestialBody? asteroid = ConvertVariantToCelestialBody(asteroidVariant);
        return asteroid != null ? GetMajorAsteroidDisplayPosition(asteroid) : InvalidPosition;
    }

    /// <summary>
    /// Caches scene-node references.
    /// </summary>
    private void CacheNodeReferences()
    {
        _statusLabel = GetNodeOrNull<Label>("UI/TopBar/MarginContainer/HBoxContainer/StatusLabel");
        _inspectorPanel = GetNodeOrNull<Node>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/InspectorPanel");
        _starCountSpin = GetNodeOrNull<SpinBox>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/StarCountContainer/StarCountSpin");
        _seedInput = GetNodeOrNull<SpinBox>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/SeedContainer/SeedInput");
        _generateButton = GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/ButtonContainer/GenerateButton");
        _rerollButton = GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/ButtonContainer/RerollButton");
        _saveButton = GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/SaveButton");
        _loadButton = GetNodeOrNull<Button>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/SaveLoadSection/ButtonContainer/LoadButton");
        _showOrbitsCheck = GetNodeOrNull<CheckBox>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/ViewSection/ShowOrbitsCheck");
        _showZonesCheck = GetNodeOrNull<CheckBox>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/ViewSection/ShowZonesCheck");
        _cameraController = GetNodeOrNull<Node>("CameraRig/Camera3D");
        _bodiesContainer = GetNodeOrNull<Node3D>("BodiesContainer");
        _orbitsContainer = GetNodeOrNull<Node3D>("OrbitsContainer");
        _zonesContainer = GetNodeOrNull<Node3D>("ZonesContainer");
        _systemBodyNodeScene = ResourceLoader.Load<PackedScene>(SystemBodyNodeScenePath);
    }

    /// <summary>
    /// Sets up viewport flags.
    /// </summary>
    private void SetupViewport()
    {
        Viewport? viewport = GetViewport();
        if (viewport != null)
        {
            viewport.UseHdr2D = true;
        }
    }

    /// <summary>
    /// Positions the camera at startup.
    /// </summary>
    private void SetupCamera()
    {
        if (_cameraController is SystemCameraController typedCameraController)
        {
            typedCameraController.FocusOnOrigin();
        }
        else
        {
            _cameraController?.Call("focus_on_origin");
        }
    }

    /// <summary>
    /// Initializes generation UI defaults.
    /// </summary>
    private void SetupGenerationUi()
    {
        if (_starCountSpin != null)
        {
            _starCountSpin.MinValue = 1;
            _starCountSpin.MaxValue = 10;
            _starCountSpin.Value = 1;
        }

        if (_seedInput != null)
        {
            _seedInput.Value = GD.Randi() % 1000000;
        }
    }

    /// <summary>
    /// Initializes view-toggle defaults.
    /// </summary>
    private void SetupViewUi()
    {
        if (_showOrbitsCheck != null)
        {
            _showOrbitsCheck.ButtonPressed = true;
        }

        if (_showZonesCheck != null)
        {
            _showZonesCheck.ButtonPressed = false;
        }
    }

    /// <summary>
    /// Creates the orbit renderer node.
    /// </summary>
    private void SetupOrbitRenderer()
    {
        if (_orbitsContainer == null)
        {
            return;
        }

        OrbitRenderer orbitRenderer = new();
        orbitRenderer.Name = "OrbitRenderer";
        _orbitsContainer.AddChild(orbitRenderer);
        _orbitRenderer = orbitRenderer;
    }

    /// <summary>
    /// Creates the belt renderer node.
    /// </summary>
    private void SetupBeltRenderer()
    {
        if (_bodiesContainer == null)
        {
            _beltRenderer = null;
            return;
        }

        BeltRenderer beltRenderer = new()
        {
            Name = "BeltRenderer",
        };
        beltRenderer.BeltClicked += OnBeltClicked;
        _bodiesContainer.AddChild(beltRenderer);
        _beltRenderer = beltRenderer;
    }

    /// <summary>
    /// Initializes save/load button state.
    /// </summary>
    private void SetupSaveLoadUi()
    {
        UpdateSaveButtonState();
    }

    /// <summary>
    /// Applies tooltip text to interactive controls.
    /// </summary>
    private void SetupTooltips()
    {
        if (_generateButton != null)
        {
            _generateButton.TooltipText = "Generate system with current settings";
        }

        if (_rerollButton != null)
        {
            _rerollButton.TooltipText = "Generate with a new random seed";
        }

        if (_starCountSpin != null)
        {
            _starCountSpin.TooltipText = "Number of stars in the system (1-10)";
        }

        if (_seedInput != null)
        {
            _seedInput.TooltipText = "Generation seed for deterministic results";
        }

        if (_showOrbitsCheck != null)
        {
            _showOrbitsCheck.TooltipText = "Toggle orbital path visibility";
        }

        if (_showZonesCheck != null)
        {
            _showZonesCheck.TooltipText = "Toggle habitable zone visibility";
        }

        if (_saveButton != null)
        {
            _saveButton.TooltipText = "Save current system to file (Ctrl+S)";
        }

        if (_loadButton != null)
        {
            _loadButton.TooltipText = "Load system from file (Ctrl+O)";
        }
    }

    /// <summary>
    /// Connects interactive UI signals.
    /// </summary>
    private void ConnectSignals()
    {
        if (_generateButton != null)
        {
            _generateButton.Pressed += OnGeneratePressed;
        }

        if (_rerollButton != null)
        {
            _rerollButton.Pressed += OnRerollPressed;
        }

        if (_showOrbitsCheck != null)
        {
            _showOrbitsCheck.Toggled += OnShowOrbitsToggled;
        }

        if (_showZonesCheck != null)
        {
            _showZonesCheck.Toggled += OnShowZonesToggled;
        }

        if (_saveButton != null)
        {
            _saveButton.Pressed += OnSavePressed;
        }

        if (_loadButton != null)
        {
            _loadButton.Pressed += OnLoadPressed;
        }

        if (_inspectorPanel is SystemInspectorPanel typedInspectorPanel)
        {
            typedInspectorPanel.OpenInViewerRequested += OnOpenBodyInViewer;
        }
        else if (_inspectorPanel != null && _inspectorPanel.HasSignal("open_in_viewer_requested"))
        {
            _inspectorPanel.Connect("open_in_viewer_requested", Callable.From<CelestialBody>(OnOpenBodyInViewer));
        }

        Button? backButton = GetNodeOrNull<Button>("UI/TopBar/MarginContainer/HBoxContainer/BackButton");
        if (backButton != null)
        {
            backButton.Pressed += OnBackPressed;
        }
    }

    /// <summary>
    /// Handles back-button presses.
    /// </summary>
    private void OnBackPressed()
    {
        EmitSignal(SignalName.BackToGalaxyRequested);
    }

    /// <summary>
    /// Handles keyboard shortcuts for the system viewer.
    /// </summary>
    private void HandleUnhandledKeyInput(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
        {
            return;
        }

        if (keyEvent.Keycode == Key.S && keyEvent.CtrlPressed)
        {
            OnSavePressed();
            GetViewport()?.SetInputAsHandled();
            return;
        }

        if (keyEvent.Keycode == Key.O && keyEvent.CtrlPressed)
        {
            OnLoadPressed();
            GetViewport()?.SetInputAsHandled();
            return;
        }

        if (keyEvent.Keycode == Key.Escape)
        {
            OnBackPressed();
            GetViewport()?.SetInputAsHandled();
        }
    }

    /// <summary>
    /// Handles generate-button presses.
    /// </summary>
    private void OnGeneratePressed()
    {
        int starCount = _starCountSpin != null ? (int)_starCountSpin.Value : 1;
        int seedValue = _seedInput != null ? (int)_seedInput.Value : (int)(GD.Randi() % 1000000);
        _sourceStarSeed = 0;
        GenerateSystem(seedValue, starCount, starCount);
    }

    /// <summary>
    /// Handles reroll-button presses.
    /// </summary>
    private void OnRerollPressed()
    {
        int newSeed = (int)(GD.Randi() % 1000000);
        if (_seedInput != null)
        {
            _seedInput.Value = newSeed;
        }

        OnGeneratePressed();
    }

    /// <summary>
    /// Handles orbit-visibility toggles.
    /// </summary>
    private void OnShowOrbitsToggled(bool enabled)
    {
        if (_orbitsContainer != null)
        {
            _orbitsContainer.Visible = enabled;
        }
    }

    /// <summary>
    /// Handles zone-visibility toggles.
    /// </summary>
    private void OnShowZonesToggled(bool enabled)
    {
        if (_zonesContainer != null)
        {
            _zonesContainer.Visible = enabled;
        }
    }

    /// <summary>
    /// Handles save-button presses.
    /// </summary>
    private void OnSavePressed()
    {
        _saveLoad.OnSavePressed(this);
    }

    /// <summary>
    /// Handles load-button presses.
    /// </summary>
    private void OnLoadPressed()
    {
        _saveLoad.OnLoadPressed(this);
    }

    /// <summary>
    /// Updates save-button availability.
    /// </summary>
    private void UpdateSaveButtonState()
    {
        if (_saveButton != null)
        {
            _saveButton.Disabled = _currentSystem == null;
        }
    }

    /// <summary>
    /// Creates viewer body nodes for all displayed bodies.
    /// </summary>
    private void CreateBodyNodes()
    {
        if (_currentSystem == null || _currentLayout == null || _bodiesContainer == null)
        {
            return;
        }

        foreach (CelestialBody star in _currentSystem.GetStars())
        {
            CreateBodyNodeFromLayout(star);
        }

        foreach (CelestialBody planet in _currentSystem.GetPlanets())
        {
            CreateBodyNodeFromLayout(planet);
        }

        if (_currentSystem.AsteroidBelts.Count > 0)
        {
            foreach (CelestialBody asteroid in _currentSystem.GetAsteroids())
            {
                Vector3 position = GetMajorAsteroidDisplayPosition(asteroid);
                if (position != InvalidPosition)
                {
                    CreateMajorAsteroidNodeAt(asteroid, position);
                }
            }
        }
    }

    /// <summary>
    /// Creates one body node from precomputed layout data.
    /// </summary>
    private void CreateBodyNodeFromLayout(CelestialBody body)
    {
        if (_currentLayout == null || _bodiesContainer == null || _systemBodyNodeScene == null)
        {
            return;
        }

        BodyLayout? layout = _currentLayout.GetBodyLayout(body.Id);
        if (layout == null)
        {
            GD.PushWarning($"No layout found for body: {body.Id}");
            return;
        }

        Node3D? bodyNode = _systemBodyNodeScene.Instantiate() as Node3D;
        if (bodyNode == null)
        {
            return;
        }

        if (bodyNode is SystemBodyNode typedBodyNode)
        {
            typedBodyNode.Setup(body, layout.DisplayRadius, layout.Position);
            typedBodyNode.BodySelected += OnBodyClicked;
        }
        else
        {
            bodyNode.Call("setup", body, layout.DisplayRadius, layout.Position);
            bodyNode.Connect("body_selected", Callable.From<string>(OnBodyClicked));
        }

        _bodiesContainer.AddChild(bodyNode);
        _bodyNodes[body.Id] = bodyNode;
    }

    /// <summary>
    /// Creates a major-asteroid node at a known display position.
    /// </summary>
    private void CreateMajorAsteroidNodeAt(CelestialBody asteroid, Vector3 displayPosition)
    {
        if (_systemBodyNodeScene == null || _bodiesContainer == null)
        {
            return;
        }

        float asteroidDisplayRadius = Mathf.Clamp(
            SystemDisplayLayout.CalculatePlanetDisplayRadius(asteroid.Physical.RadiusM) * 0.4f,
            0.08f,
            0.28f);

        Node3D? asteroidNode = _systemBodyNodeScene.Instantiate() as Node3D;
        if (asteroidNode == null)
        {
            return;
        }

        Node3D parentNode = _bodiesContainer;
        Vector3 localPosition = displayPosition;
        string beltId = GetMajorAsteroidBeltId(asteroid);
        if (_beltRenderer != null && !string.IsNullOrEmpty(beltId))
        {
            Node3D? beltRoot = _beltRenderer.GetNodeOrNull<Node3D>("Belt_" + beltId);
            if (beltRoot != null)
            {
                parentNode = beltRoot;
                localPosition = beltRoot.ToLocal(displayPosition);
            }
        }

        if (asteroidNode is SystemBodyNode typedAsteroidNode)
        {
            typedAsteroidNode.Setup(asteroid, asteroidDisplayRadius, localPosition);
            typedAsteroidNode.BodySelected += OnBodyClicked;
        }
        else
        {
            asteroidNode.Call("setup", asteroid, asteroidDisplayRadius, localPosition);
            asteroidNode.Connect("body_selected", Callable.From<string>(OnBodyClicked));
        }

        parentNode.AddChild(asteroidNode);
    }

    /// <summary>
    /// Returns the belt identifier for a major asteroid.
    /// </summary>
    private string GetMajorAsteroidBeltId(CelestialBody asteroid)
    {
        if (_currentSystem == null)
        {
            return string.Empty;
        }

        foreach (AsteroidBelt belt in _currentSystem.AsteroidBelts)
        {
            if (belt.MajorAsteroidIds.Contains(asteroid.Id))
            {
                return belt.Id;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Maps a major asteroid orbit into display coordinates.
    /// </summary>
    private Vector3 GetMajorAsteroidDisplayPosition(CelestialBody asteroid)
    {
        if (_currentSystem == null || _currentLayout == null || !asteroid.HasOrbital() || asteroid.Orbital == null)
        {
            return InvalidPosition;
        }

        AsteroidBelt? matchingBelt = null;
        foreach (AsteroidBelt belt in _currentSystem.AsteroidBelts)
        {
            if (belt.MajorAsteroidIds.Contains(asteroid.Id))
            {
                matchingBelt = belt;
                break;
            }
        }

        if (matchingBelt == null)
        {
            return InvalidPosition;
        }

        BeltLayout? beltLayout = _currentLayout.GetBeltLayout(matchingBelt.Id);
        if (beltLayout == null)
        {
            return InvalidPosition;
        }

        float asteroidAu = (float)(asteroid.Orbital.SemiMajorAxisM / Units.AuMeters);
        float bandAu = Mathf.Max(0.001f, beltLayout.OuterAu - beltLayout.InnerAu);
        float radialT = Mathf.Clamp((asteroidAu - beltLayout.InnerAu) / bandAu, 0.0f, 1.0f);
        float displayRadius = Mathf.Lerp(beltLayout.InnerDisplayRadius, beltLayout.OuterDisplayRadius, radialT);
        float angle = Mathf.DegToRad((float)asteroid.Orbital.MeanAnomalyDeg);
        float inclination = Mathf.Clamp(Mathf.Abs((float)asteroid.Orbital.InclinationDeg), 0.0f, beltLayout.MaxInclinationDeg);
        float yOffset = Mathf.Sin(Mathf.DegToRad(inclination)) * Mathf.Sin(angle) * displayRadius;

        return beltLayout.HostCenter + new Vector3(
            Mathf.Cos(angle) * displayRadius,
            yOffset,
            Mathf.Sin(angle) * displayRadius);
    }

    /// <summary>
    /// Creates rendered belt visuals.
    /// </summary>
    private void CreateBeltVisualizations()
    {
        if (_currentSystem == null || _currentLayout == null || _beltRenderer == null)
        {
            return;
        }

        int baseSeed = _currentSystem.Provenance != null ? (int)_currentSystem.Provenance.GenerationSeed : 0;
        if (_beltRenderer is BeltRenderer typedBeltRenderer)
        {
            typedBeltRenderer.RenderBelts(_currentSystem, _currentLayout, baseSeed);
        }
        else if (_beltRenderer.HasMethod("render_belts"))
        {
            _beltRenderer.Call("render_belts", _currentSystem, _currentLayout, baseSeed);
        }
    }

    /// <summary>
    /// Creates orbit path visualizations.
    /// </summary>
    private void CreateOrbitVisualizations()
    {
        if (_currentSystem == null || _currentLayout == null || _orbitRenderer == null)
        {
            return;
        }

        foreach (CelestialBody planet in _currentSystem.GetPlanets())
        {
            BodyLayout? bodyLayout = _currentLayout.GetBodyLayout(planet.Id);
            if (bodyLayout != null && bodyLayout.OrbitRadius > 0.0f)
            {
                CreateCircleOrbit(planet.Id, bodyLayout.OrbitCenter, bodyLayout.OrbitRadius, (int)planet.Type, bodyLayout.OrbitParentId);
            }
        }

        foreach (CelestialBody star in _currentSystem.GetStars())
        {
            BodyLayout? starOrbit = _currentLayout.GetStarOrbit(star.Id);
            if (starOrbit != null && starOrbit.IsOrbiting && starOrbit.OrbitRadius > 0.0f)
            {
                CreateCircleOrbit(star.Id + "_orbit", starOrbit.OrbitCenter, starOrbit.OrbitRadius, (int)CelestialType.Type.Star, starOrbit.OrbitParentId);
            }
        }

        CreateBeltEdgeOrbits();
    }

    /// <summary>
    /// Creates a circular orbit visualization.
    /// </summary>
    private void CreateCircleOrbit(string orbitId, Vector3 center, float radius, int bodyType, string parentId)
    {
        if (_orbitRenderer == null)
        {
            return;
        }

        Vector3[] points = new Vector3[65];
        for (int index = 0; index < points.Length; index += 1)
        {
            float angle = ((float)index / (points.Length - 1)) * Mathf.Tau;
            points[index] = center + new Vector3(Mathf.Cos(angle) * radius, 0.0f, Mathf.Sin(angle) * radius);
        }

        if (_orbitRenderer is OrbitRenderer typedOrbitRenderer)
        {
            typedOrbitRenderer.AddOrbit(orbitId, points, bodyType, parentId, center);
        }
        else
        {
            _orbitRenderer.Call("add_orbit", orbitId, points, bodyType, parentId, center);
        }
    }

    /// <summary>
    /// Creates inner and outer edge orbit lines for each belt.
    /// </summary>
    private void CreateBeltEdgeOrbits()
    {
        if (_currentLayout == null)
        {
            return;
        }

        foreach (BeltLayout beltLayout in _currentLayout.GetAllBelts())
        {
            CreateCircleOrbit(beltLayout.BeltId + "_inner_edge", beltLayout.HostCenter, beltLayout.InnerDisplayRadius, (int)CelestialType.Type.Asteroid, beltLayout.HostId);
            CreateCircleOrbit(beltLayout.BeltId + "_outer_edge", beltLayout.HostCenter, beltLayout.OuterDisplayRadius, (int)CelestialType.Type.Asteroid, beltLayout.HostId);
        }
    }

    /// <summary>
    /// Creates zone visualizations when enabled.
    /// </summary>
    private void CreateZoneVisualizations()
    {
    }

    /// <summary>
    /// Clears body nodes while preserving the belt renderer.
    /// </summary>
    private void ClearBodies()
    {
        _bodyNodes.Clear();
        if (_bodiesContainer == null)
        {
            return;
        }

        foreach (Node child in _bodiesContainer.GetChildren())
        {
            if (_beltRenderer != null && child == _beltRenderer)
            {
                continue;
            }

            child.QueueFree();
        }
    }

    /// <summary>
    /// Clears orbit visuals.
    /// </summary>
    private void ClearOrbits()
    {
        if (_orbitRenderer is OrbitRenderer typedOrbitRenderer)
        {
            typedOrbitRenderer.Clear();
        }
        else
        {
            _orbitRenderer?.Call("clear");
        }
    }

    /// <summary>
    /// Clears belt visuals.
    /// </summary>
    private void ClearBelts()
    {
        if (_beltRenderer is BeltRenderer typedBeltRenderer)
        {
            typedBeltRenderer.Clear();
        }
        else if (_beltRenderer != null && _beltRenderer.HasMethod("clear"))
        {
            _beltRenderer.Call("clear");
        }
    }

    /// <summary>
    /// Clears zone visuals.
    /// </summary>
    private void ClearZones()
    {
        if (_zonesContainer == null)
        {
            return;
        }

        foreach (Node child in _zonesContainer.GetChildren())
        {
            child.QueueFree();
        }
    }

    /// <summary>
    /// Handles body click callbacks.
    /// </summary>
    private void OnBodyClicked(string bodyId)
    {
        SelectBody(bodyId);
    }

    /// <summary>
    /// Updates the inspector to show system info.
    /// </summary>
    private void UpdateInspectorSystem()
    {
        if (_currentSystem == null || _inspectorPanel == null)
        {
            return;
        }

        if (_inspectorPanel is SystemInspectorPanel typedInspectorPanel)
        {
            typedInspectorPanel.DisplaySystem(_currentSystem);
            return;
        }

        GodotObject? gdSystem = ConvertSolarSystemToGdObject(_currentSystem);
        if (gdSystem != null)
        {
            _inspectorPanel.Call("display_system", gdSystem);
        }
    }

    /// <summary>
    /// Handles belt click callbacks.
    /// </summary>
    private void OnBeltClicked(string beltId)
    {
        if (!string.IsNullOrEmpty(_selectedBodyId) && _bodyNodes.ContainsKey(_selectedBodyId))
        {
            SetBodyNodeSelected(_bodyNodes[_selectedBodyId], false);
        }

        _selectedBodyId = string.Empty;
        _selectedBeltId = beltId;
        if (_orbitRenderer is OrbitRenderer typedOrbitRenderer)
        {
            typedOrbitRenderer.HighlightOrbit(string.Empty);
        }
        else
        {
            _orbitRenderer?.Call("highlight_orbit", string.Empty);
        }

        if (_inspectorPanel != null && _currentSystem != null)
        {
            if (_inspectorPanel is SystemInspectorPanel typedInspectorPanel)
            {
                foreach (AsteroidBelt belt in _currentSystem.AsteroidBelts)
                {
                    if (belt.Id == beltId)
                    {
                        typedInspectorPanel.DisplaySelectedBelt(belt, _currentSystem);
                        break;
                    }
                }
            }
            else
            {
                GodotObject? gdSystem = ConvertSolarSystemToGdObject(_currentSystem);
                foreach (AsteroidBelt belt in _currentSystem.AsteroidBelts)
                {
                    if (belt.Id == beltId && gdSystem != null)
                    {
                        _inspectorPanel.Call("display_selected_belt", belt, gdSystem);
                        break;
                    }
                }
            }
        }

        SetStatus($"Selected: {beltId}");
    }

    /// <summary>
    /// Sets the selected state for either a C# or GDScript body node.
    /// </summary>
    private static void SetBodyNodeSelected(Node3D node, bool selected)
    {
        if (node is SystemBodyNode typedNode)
        {
            typedNode.SetSelected(selected);
            return;
        }

        node.Call("set_selected", selected);
    }

    /// <summary>
    /// Updates the inspector to show the selected body.
    /// </summary>
    private void UpdateInspectorBody()
    {
        if (_inspectorPanel == null || _currentSystem == null || string.IsNullOrEmpty(_selectedBodyId))
        {
            return;
        }

        CelestialBody? body = _currentSystem.GetBody(_selectedBodyId);
        if (body != null)
        {
            if (_inspectorPanel is SystemInspectorPanel typedInspectorPanel)
            {
                typedInspectorPanel.DisplaySelectedBody(body);
                return;
            }

            _inspectorPanel.Call("display_selected_body", body);
        }
    }

    /// <summary>
    /// Handles open-in-viewer requests from the inspector.
    /// </summary>
    private void OnOpenBodyInViewer(CelestialBody body)
    {
        if (body == null)
        {
            return;
        }

        Godot.Collections.Array moons = new();
        if (_currentSystem != null && body.Type == CelestialType.Type.Planet)
        {
            foreach (CelestialBody moon in _currentSystem.GetMoonsOfPlanet(body.Id))
            {
                moons.Add(moon);
            }
        }

        EmitSignal(SignalName.OpenBodyInViewer, body, moons, _sourceStarSeed);
    }

    /// <summary>
    /// Converts host positions into a GDScript-friendly dictionary.
    /// </summary>
    private static Godot.Collections.Dictionary BuildHostPositionsDictionary(Godot.Collections.Dictionary<string, Vector3> hostPositions)
    {
        Godot.Collections.Dictionary dictionary = new();
        foreach (KeyValuePair<string, Vector3> pair in hostPositions)
        {
            dictionary[pair.Key] = pair.Value;
        }

        return dictionary;
    }

    /// <summary>
    /// Converts an external Variant into the C# SolarSystem model.
    /// </summary>
    private static SolarSystem? ConvertVariantToSolarSystem(Variant systemVariant)
    {
        if (systemVariant.VariantType == Variant.Type.Nil)
        {
            return null;
        }

        GodotObject? godotObject = systemVariant.AsGodotObject();
        if (godotObject is SolarSystem typedSystem)
        {
            return typedSystem;
        }

        if (godotObject != null && godotObject.HasMethod("to_dict"))
        {
            Variant dataVariant = godotObject.Call("to_dict");
            if (dataVariant.VariantType == Variant.Type.Dictionary)
            {
                return SystemSerializer.FromDictionary((Godot.Collections.Dictionary)dataVariant);
            }
        }

        return null;
    }

    /// <summary>
    /// Converts an external Variant into the C# CelestialBody model.
    /// </summary>
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

    /// <summary>
    /// Converts a C# SolarSystem into the GDScript runtime type.
    /// </summary>
    private static GodotObject? ConvertSolarSystemToGdObject(SolarSystem? system)
    {
        if (system == null || GdSolarSystemScript == null)
        {
            return null;
        }

        Variant gdSystemVariant = GdSolarSystemScript.Call("from_dict", SystemSerializer.ToDictionary(system));
        return gdSystemVariant.VariantType == Variant.Type.Nil ? null : gdSystemVariant.AsGodotObject();
    }

}
