using Godot;
using StarGen.Domain.Celestial;

namespace StarGen.App.SystemViewer;

/// <summary>
/// GDScript-compatible snake_case wrappers for SystemViewer public API.
/// All methods delegate to their PascalCase counterparts.
/// </summary>
public partial class SystemViewer
{
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
        if (asteroid != null)
        {
            return GetMajorAsteroidDisplayPosition(asteroid);
        }

        return InvalidPosition;
    }
}
