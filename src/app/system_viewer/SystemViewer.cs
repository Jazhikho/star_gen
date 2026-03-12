using Godot;
using StarGen.Domain.Generation;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;
using StarGen.Domain.Generation.Parameters;
using System.Collections.Generic;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Main viewer scene for inspecting solar systems.
/// Core state, public API, and Godot lifecycle methods.
/// Setup in SystemViewer.Setup.cs. Rendering in SystemViewer.Rendering.cs.
/// Interaction and conversion in SystemViewer.Interaction.cs.
/// GDScript wrappers in SystemViewer.GdCompat.cs.
/// </summary>
public partial class SystemViewer : Node3D, ISystemViewerSaveLoadHost
{
    internal enum ViewerStartupState
    {
        ViewingExistingContent = 0,
        UnconfiguredStandalone = 1,
    }

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

    internal static readonly Vector3 InvalidPosition = new(1.0e20f, 1.0e20f, 1.0e20f);
    internal const string SystemBodyNodeScenePath = "res://src/app/system_viewer/SystemBodyNode.tscn";

    internal Label? _statusLabel;
    internal Control? _uiRoot;
    internal Control? _topBar;
    internal Control? _sidePanel;
    internal Button? _backButton;
    internal Node? _inspectorPanel;
    internal VBoxContainer? _generationSection;
    internal Label? _starCountLabel;
    internal SpinBox? _starCountSpin;
    internal SpinBox? _starCountMaxSpin;
    internal SpinBox? _seedInput;
    internal LineEdit? _spectralHintsInput;
    internal SpinBox? _systemAgeInput;
    internal SpinBox? _systemMetallicityInput;
    internal CheckBox? _includeBeltsCheck;
    internal CheckBox? _generatePopulationCheck;
    internal OptionButton? _rulesetModeOption;
    internal CheckBox? _showTravellerReadoutsCheck;
    internal SpinBox? _lifePermissivenessInput;
    internal SpinBox? _populationPermissivenessInput;
    internal OptionButton? _mainworldPolicyOption;
    internal Label? _generationAssumptionsLabel;
    internal VBoxContainer? _generationIssuesContainer;
    internal Button? _generateButton;
    internal Button? _rerollButton;
    internal Button? _saveButton;
    internal Button? _loadButton;
    internal CheckBox? _showOrbitsCheck;
    internal CheckBox? _showZonesCheck;
    internal Node? _cameraController;
    internal Node3D? _bodiesContainer;
    internal Node3D? _orbitsContainer;
    internal Node3D? _zonesContainer;
    internal Node? _orbitRenderer;
    internal Node3D? _beltRenderer;
    internal Label? _emptyStateLabel;
    internal PackedScene? _systemBodyNodeScene;

    internal SolarSystem? _currentSystem;
    internal SystemLayout? _currentLayout;
    internal string _selectedBodyId = string.Empty;
    internal string _selectedBeltId = string.Empty;
    internal readonly Dictionary<string, Node3D> _bodyNodes = new();
    internal bool _animationEnabled = true;
    internal bool _isUpdatingSystem;
    internal bool _isReady;
    internal int _sourceStarSeed;
    internal readonly SystemViewerSaveLoad _saveLoad = new();
    internal Rect2 _renderAreaRect = new Rect2();
    internal SolarSystemSpec? _currentSpec;
    internal GenerationParameterIssueSet _currentGenerationIssues = new();
    internal ViewerStartupState _startupState = ViewerStartupState.UnconfiguredStandalone;

    /// <summary>
    /// Reused scratch list for removing stale body node IDs during the animation update.
    /// Promoted to a field to avoid per-frame heap allocation.
    /// </summary>
    private readonly List<string> _staleBodyIds = new();

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
        SetupTopMenu();
        SetupTooltips();
        ConnectSignals();

        SetStatus("System viewer initialized");
        _isReady = true;
        ConfigureStandaloneGenerator((int)(_seedInput?.Value ?? 1.0));
    }

    /// <summary>
    /// Updates orbital animation each frame.
    /// </summary>
    public override void _Process(double delta)
    {
        UpdatePanelAwareFraming();

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

        _staleBodyIds.Clear();
        foreach (string bodyId in _bodyNodes.Keys)
        {
            Node3D bodyNode = _bodyNodes[bodyId];
            if (!IsInstanceValid(bodyNode))
            {
                _staleBodyIds.Add(bodyId);
                continue;
            }

            BodyLayout? layout = _currentLayout.GetBodyLayout(bodyId);
            if (layout != null)
            {
                bodyNode.GlobalPosition = layout.Position;
            }
        }

        foreach (string bodyId in _staleBodyIds)
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
        SolarSystemSpec spec = new(seedValue, minStars, maxStars);
        GenerateSystem(spec);
    }

    /// <summary>
    /// Generates a solar system from a fully specified generation spec.
    /// </summary>
    public void GenerateSystem(SolarSystemSpec spec)
    {
        if (_isUpdatingSystem)
        {
            return;
        }

        _currentGenerationIssues = SystemGenerationParameterValidator.Validate(spec);
        UpdateGenerationIssuesUi();
        if (_currentGenerationIssues.HasErrors())
        {
            SetError("System parameters contain blocking errors");
            return;
        }

        _currentSpec = spec;
        SetStatus($"Generating system with seed {spec.GenerationSeed}...");
        SolarSystem? system = SystemFixtureGenerator.GenerateSystem(spec, null);
        if (system == null)
        {
            SetError("Failed to generate system");
            return;
        }

        AppendTravellerGenerationIssues(system, spec);
        UpdateGenerationIssuesUi();
        DisplaySystem(system);
        if (_currentGenerationIssues.Issues.Count > 0)
        {
            SetStatus($"Generated with {_currentGenerationIssues.Issues.Count} advisory issue(s): {system.GetSummary()}");
            return;
        }

        SetStatus($"Generated: {system.GetSummary()}");
    }

    private void AppendTravellerGenerationIssues(SolarSystem system, SolarSystemSpec spec)
    {
        if (spec.UseCaseSettings.MainworldPolicy != GenerationUseCaseSettings.MainworldPolicyType.Require)
        {
            return;
        }

        TravellerMainworldSelector.SelectionResult selection = TravellerMainworldSelector.Select(system);
        if (!selection.HasCandidate())
        {
            _currentGenerationIssues.AddWarning("mainworld_policy", "Traveller mainworld requirement has no valid candidate in this generated system.");
        }
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
        _startupState = ViewerStartupState.ViewingExistingContent;
        _currentLayout = SystemDisplayLayout.CalculateLayout(system);
        SolarSystemSpec? currentSpec = ExtractCurrentSpec(system);
        if (currentSpec != null)
        {
            ApplySpecToControls(currentSpec);
        }

        ClearBodies();
        UpdateSaveButtonState();
        ClearOrbits();
        ClearBelts();
        ClearZones();

        CreateBeltVisualizations();
        CreateBodyNodes();
        CreateOrbitVisualizations();
        UpdateEmptyStateVisibility();

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
        UpdateEmptyStateVisibility();
        UpdateInspectorSystem();

        if (_startupState == ViewerStartupState.UnconfiguredStandalone)
        {
            SetStatus("Set parameters, then click Generate");
        }
        else
        {
            SetStatus("No system loaded");
        }
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
    /// Returns the current generation specification reflected by the viewer controls.
    /// </summary>
    public SolarSystemSpec? GetCurrentSpec()
    {
        return _currentSpec;
    }

    /// <summary>
    /// Prepares the viewer for standalone generation launched from the main menu.
    /// </summary>
    public void ConfigureStandaloneGenerator(int seedValue, SolarSystemSpec? initialSpec = null)
    {
        SolarSystemSpec seedSpec;
        if (initialSpec != null)
        {
            seedSpec = initialSpec;
        }
        else
        {
            seedSpec = new SolarSystemSpec(seedValue, 1, 1);
        }

        if (seedSpec.GenerationSeed == 0)
        {
            seedSpec.GenerationSeed = seedValue;
        }

        _startupState = ViewerStartupState.UnconfiguredStandalone;
        _sourceStarSeed = 0;
        SetGenerationSectionVisible(true);
        ApplySpecToControls(seedSpec);
        ClearDisplay();
    }

    /// <summary>
    /// Shows or hides the standalone generation section.
    /// </summary>
    public void SetGenerationSectionVisible(bool visible)
    {
        if (_generationSection != null)
        {
            _generationSection.Visible = visible;
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
    /// Updates the text and tooltip of the top-level back button.
    /// </summary>
    public void ConfigureBackNavigation(string buttonText, string tooltipText)
    {
        if (_backButton == null)
        {
            return;
        }

        _backButton.Text = buttonText;
        _backButton.TooltipText = tooltipText;
    }

    /// <summary>
    /// Returns the visible 3D render area after subtracting the persistent UI chrome.
    /// </summary>
    public Rect2 GetRenderAreaRect()
    {
        return _renderAreaRect;
    }
}
