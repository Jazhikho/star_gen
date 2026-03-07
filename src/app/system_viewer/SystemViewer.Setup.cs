using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Systems;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Initialization, node-caching, UI setup, and signal-wiring for SystemViewer.
/// </summary>
public partial class SystemViewer
{
    /// <summary>
    /// Caches scene-node references.
    /// </summary>
    private void CacheNodeReferences()
    {
        _uiRoot = GetNodeOrNull<Control>("UI");
        _topBar = GetNodeOrNull<Control>("UI/TopBar");
        _sidePanel = GetNodeOrNull<Control>("UI/SidePanel");
        _statusLabel = GetNodeOrNull<Label>("UI/TopBar/MarginContainer/HBoxContainer/StatusLabel");
        _backButton = GetNodeOrNull<Button>("UI/TopBar/MarginContainer/HBoxContainer/BackButton");
        _inspectorPanel = GetNodeOrNull<Node>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/InspectorPanel");
        _generationSection = GetNodeOrNull<VBoxContainer>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection");
        _starCountLabel = GetNodeOrNull<Label>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/GenerationSection/StarCountContainer/StarCountLabel");
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

        BuildParameterEditorUi();
        if (_currentSpec == null)
        {
            ApplySpecToControls(new SolarSystemSpec((int)(_seedInput?.Value ?? 1.0), 1, 1));
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

        if (_backButton != null)
        {
            _backButton.Pressed += OnBackPressed;
        }
    }

    /// <summary>
    /// Updates camera framing to account for the left panel and top bar.
    /// </summary>
    private void UpdatePanelAwareFraming()
    {
        _renderAreaRect = StarGen.App.Shared.ViewerLayoutHelper.ComputeRenderRect(GetViewport(), _topBar, _sidePanel);
        Vector2 framingOffset = StarGen.App.Shared.ViewerLayoutHelper.ComputeNormalizedCenterOffset(GetViewport(), _renderAreaRect);
        if (_cameraController is SystemCameraController typedCameraController)
        {
            typedCameraController.SetFramingOffset(framingOffset);
        }
    }
}
