using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Systems;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Input handling, inspector updates, belt/body click callbacks,
/// and domain-to-GDObject conversion helpers for SystemViewer.
/// </summary>
public partial class SystemViewer
{
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
        SolarSystemSpec spec = BuildCurrentSpecFromControls();
        _startupState = ViewerStartupState.ViewingExistingContent;
        _sourceStarSeed = 0;
        GenerateSystem(spec);
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
    /// Updates the standalone empty-state banner.
    /// </summary>
    private void UpdateEmptyStateVisibility()
    {
        if (_emptyStateLabel == null)
        {
            return;
        }

        _emptyStateLabel.Visible = _currentSystem == null && _startupState == ViewerStartupState.UnconfiguredStandalone;
    }

    /// <summary>
    /// Handles body click callbacks.
    /// </summary>
    private void OnBodyClicked(string bodyId)
    {
        SelectBody(bodyId);
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
                GD.PushError($"SystemViewer: inspector panel is not a SystemInspectorPanel; belt selection for '{beltId}' cannot be forwarded.");
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
    /// Updates the inspector to show system info.
    /// </summary>
    private void UpdateInspectorSystem()
    {
        if (_inspectorPanel == null)
        {
            return;
        }

        if (_inspectorPanel is SystemInspectorPanel typedInspectorPanel)
        {
            typedInspectorPanel.DisplaySystem(_currentSystem, _currentSpec);
            return;
        }

        GD.PushError("SystemViewer: inspector panel is not a SystemInspectorPanel; system display cannot be forwarded.");
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
        foreach (System.Collections.Generic.KeyValuePair<string, Vector3> pair in hostPositions)
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
    /// Applies ruleset defaults when the user switches to Traveller mode.
    /// </summary>
    private void OnRulesetModeSelected(long selectedId)
    {
        if (selectedId == (long)GenerationUseCaseSettings.RulesetModeType.Traveller)
        {
            ApplyTravellerDefaultsToControls();
        }

        RefreshGenerationValidationFromControls();
    }

}
