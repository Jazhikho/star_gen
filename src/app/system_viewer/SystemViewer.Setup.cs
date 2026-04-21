using Godot;
using StarGen.App.Shared;
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
        _backButton = GetNodeOrNull<Button>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/BackButton");
        _statusLabel = GetNodeOrNull<Label>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/StatusLabel");
        _inspectorPanel = GetNodeOrNull<Node>("UI/SidePanel/MarginContainer/ScrollContainer/VBoxContainer/InspectorPanel");
        _optionsDialog = GetNodeOrNull<Window>("OptionsDialog");
        _fullscreenCheck = GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/FullscreenCheck");
        _showSeedControlsCheck = GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/ShowSeedControlsCheck");
        _skipIntroCheck = GetNodeOrNull<CheckBox>("OptionsDialog/MarginContainer/OptionsVBox/SkipIntroCheck");
        _resolutionOption = GetNodeOrNull<OptionButton>("OptionsDialog/MarginContainer/OptionsVBox/ResolutionRow/ResolutionOption");
        _applyOptionsButton = GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/ButtonRow/ApplyOptionsButton");
        _optionsStatusLabel = GetNodeOrNull<Label>("OptionsDialog/MarginContainer/OptionsVBox/OptionsStatusLabel");
        _optionsDialogCloseButton = GetNodeOrNull<Button>("OptionsDialog/MarginContainer/OptionsVBox/ButtonRow/CloseButton");
        _cameraPanel = GetNodeOrNull<Control>("UI/CameraPanel");
        _cameraPanelHeaderButton = GetNodeOrNull<Button>("UI/CameraPanel/CameraPanelVBox/CameraPanelHeaderButton");
        _cameraPanelContent = GetNodeOrNull<Control>("UI/CameraPanel/CameraPanelVBox/CameraPanelContent");
        _emptyStateLabel = GetNodeOrNull<Label>("UI/EmptyStateLabel");
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
    /// Initializes viewer defaults for a detached standalone instance.
    /// </summary>
    private void SetupGenerationUi()
    {
        if (_currentSpec == null)
        {
            ApplySpecToControls(new SolarSystemSpec((int)(GD.Randi() % 1000000), 1, 1));
        }
    }

    /// <summary>
    /// Initializes view-toggle defaults.
    /// </summary>
    private void SetupViewUi()
    {
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
        SetupEmptyStateUi();
    }

    /// <summary>
    /// Creates the empty-state placeholder shown before the first standalone generation.
    /// </summary>
	private void SetupEmptyStateUi()
	{
		if (_emptyStateLabel == null)
		{
			throw new System.InvalidOperationException("SystemViewer scene is missing EmptyStateLabel.");
		}

		UpdateEmptyStateVisibility();
	}

    /// <summary>
    /// Applies tooltip text to interactive controls.
    /// </summary>
	private void SetupTooltips()
	{
		if (_backButton != null)
		{
			_backButton.TooltipText = _backNavigationTooltip;
        }

        if (_populationPermissivenessInput != null)
        {
            _populationPermissivenessInput.TooltipText = PermissivenessScaleHelper.GetTooltipText("settlement");
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

        if (_backButton != null)
        {
            _backButton.Pressed += OnBackPressed;
        }

        if (_inspectorPanel is SystemInspectorPanel typedInspectorPanel)
        {
            typedInspectorPanel.OpenInViewerRequested += OnOpenBodyInViewer;
            typedInspectorPanel.FocusBodyRequested += OnFocusBodyRequested;
            typedInspectorPanel.FocusBeltRequested += OnFocusBeltRequested;
        }
        else if (_inspectorPanel != null && _inspectorPanel.HasSignal("open_in_viewer_requested"))
        {
            _inspectorPanel.Connect("open_in_viewer_requested", Callable.From<CelestialBody>(OnOpenBodyInViewer));

            if (_inspectorPanel.HasSignal("focus_body_requested"))
            {
                _inspectorPanel.Connect("focus_body_requested", Callable.From<CelestialBody>(OnFocusBodyRequested));
            }

            if (_inspectorPanel.HasSignal("focus_belt_requested"))
            {
                _inspectorPanel.Connect("focus_belt_requested", Callable.From<string>(OnFocusBeltRequested));
            }
        }

        if (_cameraPanelHeaderButton != null)
        {
            _cameraPanelHeaderButton.Pressed += ToggleCameraPanel;
            InitializeCameraPanelLayout();
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
