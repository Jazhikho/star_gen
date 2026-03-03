using Godot;
using StarGen.Domain.Galaxy;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Tracks the galaxy-viewer zoom level and emits transitions.
/// </summary>
public partial class ZoomStateMachine : RefCounted
{
    /// <summary>
    /// Emitted when the zoom level changes.
    /// </summary>
    [Signal]
    public delegate void LevelChangedEventHandler(int oldLevel, int newLevel);

    private int _currentLevel = (int)GalaxyCoordinates.ZoomLevel.Galaxy;

    /// <summary>
    /// Returns the current zoom level.
    /// </summary>
    public int GetCurrentLevel()
    {
        return _currentLevel;
    }

    /// <summary>
    /// Sets the current zoom level directly.
    /// </summary>
    public void SetLevel(int level)
    {
        if (level < (int)GalaxyCoordinates.ZoomLevel.Galaxy || level > (int)GalaxyCoordinates.ZoomLevel.Subsector)
        {
            return;
        }

        int previousLevel = _currentLevel;
        _currentLevel = level;
        if (previousLevel != _currentLevel)
        {
            EmitSignal(SignalName.LevelChanged, previousLevel, _currentLevel);
        }
    }

    /// <summary>
    /// Transitions to a specific zoom level.
    /// </summary>
    public void TransitionTo(int newLevel)
    {
        if (newLevel == _currentLevel)
        {
            return;
        }

        int previousLevel = _currentLevel;
        _currentLevel = newLevel;
        EmitSignal(SignalName.LevelChanged, previousLevel, newLevel);
    }

    /// <summary>
    /// Zooms in one level when possible.
    /// </summary>
    public void ZoomIn()
    {
        if (CanZoomIn())
        {
            TransitionTo(_currentLevel + 1);
        }
    }

    /// <summary>
    /// Zooms out one level when possible.
    /// </summary>
    public void ZoomOut()
    {
        if (CanZoomOut())
        {
            TransitionTo(_currentLevel - 1);
        }
    }

    /// <summary>
    /// Returns whether a deeper zoom level exists.
    /// </summary>
    public bool CanZoomIn()
    {
        return _currentLevel < (int)GalaxyCoordinates.ZoomLevel.Subsector;
    }

    /// <summary>
    /// Returns whether a wider zoom level exists.
    /// </summary>
    public bool CanZoomOut()
    {
        return _currentLevel > (int)GalaxyCoordinates.ZoomLevel.Galaxy;
    }
}
