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
        _statusLabel = GetNodeOrNull<Label>("UI/TopBar/MarginContainer/TopBarVBox/HeaderRow/StatusLabel");
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
        SetupEmptyStateUi();
        UpdateSaveButtonState();
    }

    /// <summary>
    /// Creates the empty-state placeholder shown before the first standalone generation.
    /// </summary>
    private void SetupEmptyStateUi()
    {
        if (_uiRoot == null || _emptyStateLabel != null)
        {
            return;
        }

        Label emptyStateLabel = new Label();
        emptyStateLabel.Name = "EmptyStateLabel";
        emptyStateLabel.Text = "Set parameters in the side panel, then click Generate.";
        emptyStateLabel.HorizontalAlignment = HorizontalAlignment.Center;
        emptyStateLabel.VerticalAlignment = VerticalAlignment.Center;
        emptyStateLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        emptyStateLabel.CustomMinimumSize = new Vector2(280.0f, 0.0f);
        emptyStateLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        emptyStateLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        emptyStateLabel.AnchorLeft = 0.0f;
        emptyStateLabel.AnchorTop = 0.0f;
        emptyStateLabel.AnchorRight = 1.0f;
        emptyStateLabel.AnchorBottom = 1.0f;
        emptyStateLabel.OffsetLeft = 180.0f;
        emptyStateLabel.OffsetTop = 120.0f;
        emptyStateLabel.OffsetRight = -180.0f;
        emptyStateLabel.OffsetBottom = -120.0f;
        emptyStateLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
        emptyStateLabel.Modulate = new Color(0.74f, 0.78f, 0.84f, 0.9f);
        _uiRoot.AddChild(emptyStateLabel);
        _emptyStateLabel = emptyStateLabel;
        UpdateEmptyStateVisibility();
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

        if (_rulesetModeOption != null)
        {
            _rulesetModeOption.TooltipText = GetSystemAssumption("ruleset_mode");
        }

        if (_showTravellerReadoutsCheck != null)
        {
            _showTravellerReadoutsCheck.TooltipText = GetSystemAssumption("show_traveller_readouts");
        }

        if (_lifePermissivenessInput != null)
        {
            _lifePermissivenessInput.TooltipText = PermissivenessScaleHelper.GetTooltipText("life");
        }

        if (_populationPermissivenessInput != null)
        {
            _populationPermissivenessInput.TooltipText = PermissivenessScaleHelper.GetTooltipText("settlement");
        }

        if (_mainworldPolicyOption != null)
        {
            _mainworldPolicyOption.TooltipText = GetSystemAssumption("mainworld_policy");
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

        if (_rulesetModeOption != null)
        {
            _rulesetModeOption.ItemSelected += OnRulesetModeSelected;
        }

        if (_showTravellerReadoutsCheck != null)
        {
            _showTravellerReadoutsCheck.Toggled += _ => RefreshGenerationValidationFromControls();
        }

        if (_lifePermissivenessInput != null)
        {
            _lifePermissivenessInput.ValueChanged += _ => RefreshGenerationValidationFromControls();
        }

        if (_populationPermissivenessInput != null)
        {
            _populationPermissivenessInput.ValueChanged += _ => RefreshGenerationValidationFromControls();
        }

        if (_mainworldPolicyOption != null)
        {
            _mainworldPolicyOption.ItemSelected += _ => RefreshGenerationValidationFromControls();
        }

        if (_generatePopulationCheck != null)
        {
            _generatePopulationCheck.Toggled += _ => RefreshGenerationValidationFromControls();
        }

        if (_inspectorPanel is SystemInspectorPanel typedInspectorPanel)
        {
            typedInspectorPanel.OpenInViewerRequested += OnOpenBodyInViewer;
        }
        else if (_inspectorPanel != null && _inspectorPanel.HasSignal("open_in_viewer_requested"))
        {
            _inspectorPanel.Connect("open_in_viewer_requested", Callable.From<CelestialBody>(OnOpenBodyInViewer));
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
