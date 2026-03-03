using System;
using System.IO;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Jumplanes;
using StarGen.Services.Persistence;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Handles galaxy-viewer save/load state capture and restore flows.
/// </summary>
public partial class GalaxyViewerSaveLoad : RefCounted
{
    /// <summary>
    /// Saves the current viewer state for later restoration.
    /// </summary>
    public void SaveState(Node viewer)
    {
        ZoomStateMachine? zoomMachine = viewer.Call("get_zoom_machine").As<ZoomStateMachine>();
        if (zoomMachine != null)
        {
            viewer.Call("set_saved_zoom_level", zoomMachine.GetCurrentLevel());
        }
        else
        {
            viewer.Call("set_saved_zoom_level", (int)GalaxyCoordinates.ZoomLevel.Subsector);
        }

        QuadrantSelector? quadrantSelector = viewer.Call("get_quadrant_selector").As<QuadrantSelector>();
        if (quadrantSelector != null && quadrantSelector.HasSelection())
        {
            viewer.Call("set_saved_quadrant", quadrantSelector.SelectedCoords);
        }
        else
        {
            viewer.Call("set_saved_quadrant", default(Variant));
        }

        viewer.Call("set_saved_sector", viewer.Call("get_selected_sector_internal"));

        int savedLevel = GetInt(viewer.Call("get_saved_zoom_level"), (int)GalaxyCoordinates.ZoomLevel.Subsector);
        if (savedLevel == (int)GalaxyCoordinates.ZoomLevel.Subsector)
        {
            Node3D? starCamera = viewer.Call("get_star_camera").As<Node3D>();
            if (starCamera != null && starCamera.IsInsideTree())
            {
                viewer.Call("set_saved_star_camera_position", starCamera.GlobalPosition);
                viewer.Call("set_saved_star_camera_rotation", starCamera.Rotation);
            }
        }

        viewer.Call("set_saved_star_seed", viewer.Call("get_selected_star_seed_internal"));
        viewer.Call("set_saved_star_position", viewer.Call("get_selected_star_position_internal"));
    }

    /// <summary>
    /// Restores the previously saved viewer state.
    /// </summary>
    public void RestoreState(Node viewer)
    {
        int savedLevel = GetInt(viewer.Call("get_saved_zoom_level"), -1);
        if (savedLevel < 0)
        {
            viewer.Call("call_initialize_at_home");
            return;
        }

        JumpLaneResult? jumpLaneResult = viewer.Call("get_jump_lane_result").As<JumpLaneResult>();
        if (jumpLaneResult != null)
        {
            viewer.Call("set_jump_lane_result", jumpLaneResult);
        }

        Variant savedQuadrantVariant = viewer.Call("get_saved_quadrant");
        QuadrantSelector? quadrantSelector = viewer.Call("get_quadrant_selector").As<QuadrantSelector>();
        if (savedQuadrantVariant.VariantType == Variant.Type.Vector3I && quadrantSelector != null)
        {
            Vector3I quadrantCoords = (Vector3I)savedQuadrantVariant;
            GodotObject? quadrantCursor = viewer.Call("get_quadrant_cursor").AsGodotObject();
            if (quadrantCursor != null)
            {
                quadrantCursor.Set("position", quadrantCoords);
            }

            quadrantSelector.SetSelection(quadrantCoords);
        }

        Variant savedSectorVariant = viewer.Call("get_saved_sector");
        viewer.Call("set_selected_sector_internal", savedSectorVariant);
        if (savedSectorVariant.VariantType == Variant.Type.Vector3I)
        {
            GodotObject? sectorCursor = viewer.Call("get_sector_cursor").AsGodotObject();
            if (sectorCursor != null)
            {
                sectorCursor.Set("position", (Vector3I)savedSectorVariant);
            }
        }

        if (savedQuadrantVariant.VariantType == Variant.Type.Vector3I
            && (savedLevel == (int)GalaxyCoordinates.ZoomLevel.Sector || savedLevel == (int)GalaxyCoordinates.ZoomLevel.Subsector))
        {
            GodotObject? sectorRenderer = viewer.Call("get_sector_renderer").AsGodotObject();
            if (sectorRenderer != null)
            {
                sectorRenderer.Call("build_for_quadrant", (Vector3I)savedQuadrantVariant, viewer.Call("get_density_model"));
            }
        }

        ZoomStateMachine? zoomMachine = viewer.Call("get_zoom_machine").As<ZoomStateMachine>();
        if (zoomMachine != null)
        {
            zoomMachine.SetLevel(savedLevel);
        }

        viewer.Call("call_apply_zoom_level", savedLevel);

        if (savedLevel == (int)GalaxyCoordinates.ZoomLevel.Subsector)
        {
            Node3D? starCamera = viewer.Call("get_star_camera").As<Node3D>();
            if (starCamera != null && starCamera.IsInsideTree())
            {
                starCamera.GlobalPosition = GetVector3(viewer.Call("get_saved_star_camera_position"));
                starCamera.Rotation = GetVector3(viewer.Call("get_saved_star_camera_rotation"));

                GodotObject? neighborhoodRenderer = viewer.Call("get_neighborhood_renderer").AsGodotObject();
                if (neighborhoodRenderer != null)
                {
                    neighborhoodRenderer.Call(
                        "build_neighborhood",
                        starCamera.GlobalPosition,
                        viewer.Get("galaxy_seed"),
                        viewer.Call("get_density_model"),
                        viewer.Call("get_reference_density"));
                }
            }
        }

        int savedStarSeed = GetInt(viewer.Call("get_saved_star_seed"), 0);
        Vector3 savedStarPosition = GetVector3(viewer.Call("get_saved_star_position"));
        if (savedStarSeed != 0 && savedLevel == (int)GalaxyCoordinates.ZoomLevel.Subsector)
        {
            viewer.Call("call_apply_star_selection", savedStarPosition, savedStarSeed);
        }
        else
        {
            GodotObject? selectionIndicator = viewer.Call("get_selection_indicator").AsGodotObject();
            if (selectionIndicator != null)
            {
                selectionIndicator.Call("hide_indicator");
            }
        }

        viewer.Call("call_update_inspector");
        viewer.Call("set_status", "Returned to galaxy view");
    }

    /// <summary>
    /// Clears saved viewer state.
    /// </summary>
    public void ClearSavedState(Node viewer)
    {
        viewer.Call("set_saved_zoom_level", -1);
        viewer.Call("set_saved_quadrant", default(Variant));
        viewer.Call("set_saved_sector", default(Variant));
        viewer.Call("set_saved_star_camera_position", Vector3.Zero);
        viewer.Call("set_saved_star_camera_rotation", Vector3.Zero);
        viewer.Call("set_saved_star_seed", 0);
        viewer.Call("set_saved_star_position", Vector3.Zero);
    }

    /// <summary>
    /// Returns whether there is saved viewer state to restore.
    /// </summary>
    public bool HasSavedState(Node viewer)
    {
        return GetInt(viewer.Call("get_saved_zoom_level"), -1) >= 0;
    }

    /// <summary>
    /// Creates a galaxy-save payload from the current viewer state.
    /// </summary>
    public GalaxySaveData CreateSaveData(Node viewer)
    {
        GalaxySaveData data = GalaxySaveData.Create((long)Time.GetUnixTimeFromSystem());
        data.GalaxySeed = GetInt(viewer.Get("galaxy_seed"), data.GalaxySeed);

        GalaxyConfig? config = viewer.Call("get_galaxy_config").As<GalaxyConfig>();
        if (config != null)
        {
            data.SetConfig(config);
        }

        Galaxy? galaxy = viewer.Call("get_galaxy").As<Galaxy>();
        if (galaxy != null)
        {
            data.CachedSystemCount = galaxy.GetCachedSystemCount();
        }

        ZoomStateMachine? zoomMachine = viewer.Call("get_zoom_machine").As<ZoomStateMachine>();
        if (zoomMachine != null)
        {
            data.ZoomLevel = zoomMachine.GetCurrentLevel();
        }

        QuadrantSelector? quadrantSelector = viewer.Call("get_quadrant_selector").As<QuadrantSelector>();
        if (quadrantSelector != null && quadrantSelector.HasSelection())
        {
            if (quadrantSelector.SelectedCoords.VariantType == Variant.Type.Vector3I)
            {
                data.SelectedQuadrant = (Vector3I)quadrantSelector.SelectedCoords;
            }
        }

        Variant selectedSectorVariant = viewer.Call("get_selected_sector_internal");
        if (selectedSectorVariant.VariantType == Variant.Type.Vector3I)
        {
            data.SelectedSector = (Vector3I)selectedSectorVariant;
        }

        Node3D? starCamera = viewer.Call("get_star_camera").As<Node3D>();
        if (starCamera != null && starCamera.IsInsideTree())
        {
            data.CameraPosition = starCamera.GlobalPosition;
            data.CameraRotation = starCamera.Rotation;
        }

        int selectedStarSeed = GetInt(viewer.Call("get_selected_star_seed_internal"), 0);
        data.HasStarSelection = selectedStarSeed != 0;
        data.SelectedStarSeed = selectedStarSeed;
        data.SelectedStarPosition = GetVector3(viewer.Call("get_selected_star_position_internal"));

        JumpLaneRegion? jumpLaneRegion = viewer.Call("get_jump_lane_region").As<JumpLaneRegion>();
        if (jumpLaneRegion != null)
        {
            data.JumpLaneRegionData = jumpLaneRegion.ToDictionary();
        }

        JumpLaneResult? jumpLaneResult = viewer.Call("get_jump_lane_result").As<JumpLaneResult>();
        if (jumpLaneResult != null)
        {
            data.JumpLaneResultData = jumpLaneResult.ToDictionary();
        }

        Node? mainApp = viewer.GetParent()?.GetParent();
        if (mainApp != null && mainApp.HasMethod("get_body_overrides"))
        {
            Variant overridesVariant = mainApp.Call("get_body_overrides");
            if (overridesVariant.VariantType != Variant.Type.Nil)
            {
                data.SetBodyOverrides(overridesVariant.As<GalaxyBodyOverrides>());
            }
        }

        return data;
    }

    /// <summary>
    /// Applies save data to restore viewer state.
    /// </summary>
    public void ApplySaveData(Node viewer, GalaxySaveData data)
    {
        if (data.HasConfig())
        {
            GalaxyConfig? config = data.GetConfig();
            if (config != null)
            {
                viewer.Call("set_galaxy_config", config);
            }
        }

        int currentSeed = GetInt(viewer.Get("galaxy_seed"), data.GalaxySeed);
        if (data.GalaxySeed != currentSeed)
        {
            viewer.Call("call_change_galaxy_seed", data.GalaxySeed);
        }

        QuadrantSelector? quadrantSelector = viewer.Call("get_quadrant_selector").As<QuadrantSelector>();
        if (data.SelectedQuadrant.HasValue && quadrantSelector != null)
        {
            Vector3I quadrantCoords = data.SelectedQuadrant.Value;
            GodotObject? quadrantCursor = viewer.Call("get_quadrant_cursor").AsGodotObject();
            if (quadrantCursor != null)
            {
                quadrantCursor.Set("position", quadrantCoords);
            }

            quadrantSelector.SetSelection(quadrantCoords);
        }
        else if (quadrantSelector != null)
        {
            quadrantSelector.ClearSelection();
        }

        if (data.SelectedSector.HasValue)
        {
            viewer.Call("set_selected_sector_internal", data.SelectedSector.Value);
            GodotObject? sectorCursor = viewer.Call("get_sector_cursor").AsGodotObject();
            if (sectorCursor != null)
            {
                sectorCursor.Set("position", data.SelectedSector.Value);
            }
        }
        else
        {
            viewer.Call("set_selected_sector_internal", default(Variant));
        }

        if (data.SelectedQuadrant.HasValue
            && (data.ZoomLevel == (int)GalaxyCoordinates.ZoomLevel.Sector || data.ZoomLevel == (int)GalaxyCoordinates.ZoomLevel.Subsector))
        {
            GodotObject? sectorRenderer = viewer.Call("get_sector_renderer").AsGodotObject();
            if (sectorRenderer != null)
            {
                sectorRenderer.Call("build_for_quadrant", data.SelectedQuadrant.Value, viewer.Call("get_density_model"));
            }
        }

        ZoomStateMachine? zoomMachine = viewer.Call("get_zoom_machine").As<ZoomStateMachine>();
        if (zoomMachine != null)
        {
            zoomMachine.SetLevel(data.ZoomLevel);
        }

        viewer.Call("call_apply_zoom_level", data.ZoomLevel);

        if (data.ZoomLevel == (int)GalaxyCoordinates.ZoomLevel.Subsector)
        {
            Node3D? starCamera = viewer.Call("get_star_camera").As<Node3D>();
            if (starCamera != null && starCamera.IsInsideTree())
            {
                starCamera.GlobalPosition = data.CameraPosition;
                starCamera.Rotation = data.CameraRotation;

                GodotObject? neighborhoodRenderer = viewer.Call("get_neighborhood_renderer").AsGodotObject();
                if (neighborhoodRenderer != null)
                {
                    neighborhoodRenderer.Call(
                        "build_neighborhood",
                        starCamera.GlobalPosition,
                        viewer.Get("galaxy_seed"),
                        viewer.Call("get_density_model"),
                        viewer.Call("get_reference_density"));
                }
            }
        }

        if (data.HasStarSelection)
        {
            viewer.Call("set_selected_star_seed_internal", data.SelectedStarSeed);
            viewer.Call("set_selected_star_position_internal", data.SelectedStarPosition);

            GodotObject? selectionIndicator = viewer.Call("get_selection_indicator").AsGodotObject();
            if (selectionIndicator != null)
            {
                selectionIndicator.Call("show_at", data.SelectedStarPosition);
            }

            GodotObject? inspectorPanel = viewer.Call("get_inspector_panel").AsGodotObject();
            if (inspectorPanel != null)
            {
                inspectorPanel.Call("display_selected_star", data.SelectedStarPosition, data.SelectedStarSeed);
            }
        }
        else
        {
            viewer.Call("set_selected_star_seed_internal", 0);
            viewer.Call("set_selected_star_position_internal", Vector3.Zero);

            GodotObject? selectionIndicator = viewer.Call("get_selection_indicator").AsGodotObject();
            if (selectionIndicator != null)
            {
                selectionIndicator.Call("hide_indicator");
            }
        }

        if (data.JumpLaneRegionData.Count > 0)
        {
            JumpLaneRegion region = JumpLaneRegion.FromDictionary(data.JumpLaneRegionData);
            viewer.Call("set_jump_lane_region", region);
        }

        if (data.JumpLaneResultData.Count > 0)
        {
            JumpLaneResult result = JumpLaneResult.FromDictionary(data.JumpLaneResultData);
            viewer.Call("set_jump_lane_result", result);
        }

        viewer.Call("call_update_inspector");
    }

    /// <summary>
    /// Opens the save dialog for galaxy save files.
    /// </summary>
    public void OnSavePressed(Node viewer)
    {
        FileDialog dialog = new()
        {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Access = FileDialog.AccessEnum.Userdata,
            Filters = ["*.sgg ; StarGen Galaxy", "*.json ; JSON Debug"],
        };

        int galaxySeed = GetInt(viewer.Get("galaxy_seed"), 0);
        dialog.CurrentFile = $"galaxy_{galaxySeed}.sgg";
        dialog.FileSelected += path => OnSaveFileSelected(viewer, path);
        dialog.Canceled += () => dialog.QueueFree();
        viewer.AddChild(dialog);
        dialog.PopupCentered(new Vector2I(800, 600));
    }

    /// <summary>
    /// Opens the load dialog for galaxy save files.
    /// </summary>
    public void OnLoadPressed(Node viewer)
    {
        FileDialog dialog = new()
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Userdata,
            Filters = ["*.sgg ; StarGen Galaxy", "*.json ; JSON Debug"],
        };

        dialog.FileSelected += path => OnLoadFileSelected(viewer, path);
        dialog.Canceled += () => dialog.QueueFree();
        viewer.AddChild(dialog);
        dialog.PopupCentered(new Vector2I(800, 600));
    }

    /// <summary>
    /// Handles a chosen save path.
    /// </summary>
    public void OnSaveFileSelected(Node viewer, string path)
    {
        GalaxySaveData data = CreateSaveData(viewer);
        string error = path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? GalaxyPersistence.SaveJson(path, data)
            : GalaxyPersistence.SaveBinary(path, data);

        if (string.IsNullOrEmpty(error))
        {
            viewer.Call("set_status", $"Saved to {Path.GetFileName(path)}");
            return;
        }

        viewer.Call("set_status", $"Save failed: {error}");
        GD.PushError(error);
    }

    /// <summary>
    /// Handles a chosen load path.
    /// </summary>
    public void OnLoadFileSelected(Node viewer, string path)
    {
        GalaxySaveData? data = GalaxyPersistence.LoadAuto(path);
        if (data == null)
        {
            viewer.Call("set_status", "Failed to load file");
            return;
        }

        if (!data.IsValid())
        {
            viewer.Call("set_status", "Invalid save data");
            return;
        }

        ApplySaveData(viewer, data);
        viewer.Call("set_status", $"Loaded from {Path.GetFileName(path)}");
    }

    private static int GetInt(Variant value, int fallback)
    {
        return value.VariantType switch
        {
            Variant.Type.Int => (int)(long)value,
            Variant.Type.Float => (int)(double)value,
            _ => fallback,
        };
    }

    private static Vector3 GetVector3(Variant value)
    {
        return value.VariantType == Variant.Type.Vector3 ? (Vector3)value : Vector3.Zero;
    }
}
