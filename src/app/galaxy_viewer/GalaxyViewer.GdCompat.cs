using Godot;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// GDScript-compatible wrappers for GalaxyViewer methods used by legacy tests.
/// </summary>
public partial class GalaxyViewer
{
    /// <summary>
    /// GDScript-compatible seed field bridge.
    /// </summary>
    public int galaxy_seed
    {
        get => GalaxySeed;
        set => GalaxySeed = value;
    }

    /// <summary>
    /// Returns whether startup should begin at home.
    /// </summary>
    public bool get_start_at_home()
    {
        return StartAtHome;
    }

    /// <summary>
    /// Returns the active zoom level.
    /// </summary>
    public int get_zoom_level()
    {
        if (_zoomMachine == null)
        {
            return (int)GalaxyCoordinates.ZoomLevel.Galaxy;
        }

        return _zoomMachine.GetCurrentLevel();
    }

    /// <summary>
    /// Returns the active inspector panel.
    /// </summary>
    public GodotObject? get_inspector_panel()
    {
        return _inspectorPanel;
    }

    /// <summary>
    /// Returns the current galaxy spec.
    /// </summary>
    public GalaxySpec? get_spec()
    {
        return _spec;
    }

    /// <summary>
    /// Handles legacy key-input calls.
    /// </summary>
    public void _handle_key_input(InputEventKey keyEvent)
    {
        HandleKeyInput(keyEvent);
    }

    /// <summary>
    /// Navigates back to home position.
    /// </summary>
    public void navigate_to_home()
    {
        InitializeAtHome();
    }

    /// <summary>
    /// Clears saved state.
    /// </summary>
    public void clear_saved_state()
    {
        _saveLoad.ClearSavedState(this);
    }

    /// <summary>
    /// Test helper to emulate selecting a star.
    /// </summary>
    public void simulate_star_selected(int starSeed, Vector3 worldPosition)
    {
        ApplyStarSelection(worldPosition, starSeed);
    }

    /// <summary>
    /// Test helper to emulate deselecting a star.
    /// </summary>
    public void simulate_star_deselected()
    {
        ClearStarSelection();
    }

    /// <summary>
    /// Test helper to emulate opening a selected system.
    /// </summary>
    public void simulate_open_selected_system()
    {
        TryOpenSelectedSystem();
    }

    /// <summary>
    /// Applies a new galaxy seed and rebuilds dependent state.
    /// </summary>
    public void ChangeGalaxySeed(int newSeed)
    {
        GalaxySeed = newSeed;
        _galaxyConfig ??= GalaxyConfig.CreateDefault();
        _galaxy = new Domain.Galaxy.Galaxy(_galaxyConfig, GalaxySeed);
        _spec = _galaxy.Spec;
        _jumpRoutePopulationCache.Clear();
        InvalidateJumpRoutes();
        BuildStaticRenderers();
        UpdateSeedDisplay();
        UpdateInspectorState();
        EmitSignal(SignalName.GalaxySeedChanged, GalaxySeed);
    }

    /// <summary>
    /// Sets a typed galaxy configuration.
    /// </summary>
    public void SetGalaxyConfig(GalaxyConfig config)
    {
        _galaxyConfig = config;
        _galaxy = new Domain.Galaxy.Galaxy(_galaxyConfig, GalaxySeed);
        _spec = _galaxy.Spec;
        _jumpRoutePopulationCache.Clear();
        InvalidateJumpRoutes();
        UpdateInspectorState();
    }

    /// <summary>
    /// Applies a new galaxy seed and rebuilds dependent state.
    /// </summary>
    public void call_change_galaxy_seed(int newSeed)
    {
        ChangeGalaxySeed(newSeed);
    }
}
