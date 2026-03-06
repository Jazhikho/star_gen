using System;
using System.IO;
using Godot;
using StarGen.App;
using StarGen.Services.Persistence;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Jumplanes;

namespace StarGen.App.GalaxyViewer;

/// <summary>
/// Minimal saved-state contract used by the galaxy-viewer save/load helper and its tests.
/// </summary>
public interface IGalaxyViewerSavedStateHost
{
    /// <summary>
    /// Returns the saved zoom level.
    /// </summary>
    int GetSavedZoomLevel();

    /// <summary>
    /// Stores the saved zoom level.
    /// </summary>
    void SetSavedZoomLevel(int level);

    /// <summary>
    /// Returns the saved quadrant, if present.
    /// </summary>
    Vector3I? GetSavedQuadrant();

    /// <summary>
    /// Stores the saved quadrant.
    /// </summary>
    void SetSavedQuadrant(Vector3I? value);

    /// <summary>
    /// Returns the saved sector, if present.
    /// </summary>
    Vector3I? GetSavedSector();

    /// <summary>
    /// Stores the saved sector.
    /// </summary>
    void SetSavedSector(Vector3I? value);

    /// <summary>
    /// Returns the saved star-camera position.
    /// </summary>
    Vector3 GetSavedStarCameraPosition();

    /// <summary>
    /// Stores the saved star-camera position.
    /// </summary>
    void SetSavedStarCameraPosition(Vector3 value);

    /// <summary>
    /// Returns the saved star-camera rotation.
    /// </summary>
    Vector3 GetSavedStarCameraRotation();

    /// <summary>
    /// Stores the saved star-camera rotation.
    /// </summary>
    void SetSavedStarCameraRotation(Vector3 value);

    /// <summary>
    /// Returns the saved star seed.
    /// </summary>
    int GetSavedStarSeed();

    /// <summary>
    /// Stores the saved star seed.
    /// </summary>
    void SetSavedStarSeed(int value);

    /// <summary>
    /// Returns the saved star position.
    /// </summary>
    Vector3 GetSavedStarPosition();

    /// <summary>
    /// Stores the saved star position.
    /// </summary>
    void SetSavedStarPosition(Vector3 value);
}

/// <summary>
/// Handles galaxy-viewer save/load state capture and restore flows.
/// </summary>
public partial class GalaxyViewerSaveLoad : RefCounted
{
    /// <summary>
    /// Saves the current viewer state for later restoration.
    /// </summary>
    public void SaveState(GalaxyViewer viewer)
    {
        ZoomStateMachine? zoomMachine = viewer.GetZoomMachine();
        if (zoomMachine != null)
        {
            viewer.SetSavedZoomLevel(zoomMachine.GetCurrentLevel());
        }
        else
        {
            viewer.SetSavedZoomLevel((int)GalaxyCoordinates.ZoomLevel.Subsector);
        }

        QuadrantSelector? quadrantSelector = viewer.GetQuadrantSelector();
        if (quadrantSelector != null
            && quadrantSelector.HasSelection()
            && quadrantSelector.SelectedCoords.VariantType == Variant.Type.Vector3I)
        {
            viewer.SetSavedQuadrant((Vector3I)quadrantSelector.SelectedCoords);
        }
        else
        {
            viewer.SetSavedQuadrant(null);
        }

        viewer.SetSavedSector(viewer.GetSelectedSector());

        if (viewer.GetSavedZoomLevel() == (int)GalaxyCoordinates.ZoomLevel.Subsector)
        {
            StarViewCamera? starCamera = viewer.GetStarCamera();
            if (starCamera != null && starCamera.IsInsideTree())
            {
                viewer.SetSavedStarCameraPosition(starCamera.GlobalPosition);
                viewer.SetSavedStarCameraRotation(starCamera.Rotation);
            }
        }

        viewer.SetSavedStarSeed(viewer.GetSelectedStarSeed());
        viewer.SetSavedStarPosition(viewer.GetSelectedStarPosition());
    }

    /// <summary>
    /// Restores the previously saved viewer state.
    /// </summary>
    public void RestoreState(GalaxyViewer viewer)
    {
        int savedLevel = viewer.GetSavedZoomLevel();
        if (savedLevel < 0)
        {
            viewer.InitializeAtHomeState();
            return;
        }

        JumpLaneResult? jumpLaneResult = viewer.GetJumpLaneResult();
        if (jumpLaneResult != null)
        {
            viewer.SetJumpLaneResult(jumpLaneResult);
        }

        Vector3I? savedQuadrant = viewer.GetSavedQuadrant();
        QuadrantSelector? quadrantSelector = viewer.GetQuadrantSelector();
        if (savedQuadrant.HasValue && quadrantSelector != null)
        {
            GridCursor? quadrantCursor = viewer.GetQuadrantCursor();
            if (quadrantCursor != null)
            {
                quadrantCursor.Position = savedQuadrant.Value;
            }

            quadrantSelector.SetSelection(Variant.CreateFrom(savedQuadrant.Value));
        }
        else if (quadrantSelector != null)
        {
            quadrantSelector.ClearSelection();
        }

        Vector3I? savedSector = viewer.GetSavedSector();
        viewer.SetSelectedSector(savedSector);
        if (savedSector.HasValue)
        {
            GridCursor? sectorCursor = viewer.GetSectorCursor();
            if (sectorCursor != null)
            {
                sectorCursor.Position = savedSector.Value;
            }
        }

        if (savedQuadrant.HasValue
            && (savedLevel == (int)GalaxyCoordinates.ZoomLevel.Sector
                || savedLevel == (int)GalaxyCoordinates.ZoomLevel.Subsector))
        {
            SectorRenderer? sectorRenderer = viewer.GetSectorRenderer();
            DensityModelInterface? densityModel = viewer.GetDensityModel();
            if (sectorRenderer != null && densityModel != null)
            {
                sectorRenderer.BuildForQuadrant(savedQuadrant.Value, densityModel);
            }
        }

        ZoomStateMachine? zoomMachine = viewer.GetZoomMachine();
        if (zoomMachine != null)
        {
            zoomMachine.SetLevel(savedLevel);
        }

        viewer.ApplyZoomLevelState(savedLevel);

        if (savedLevel == (int)GalaxyCoordinates.ZoomLevel.Subsector)
        {
            StarViewCamera? starCamera = viewer.GetStarCamera();
            if (starCamera != null && starCamera.IsInsideTree())
            {
                starCamera.GlobalPosition = viewer.GetSavedStarCameraPosition();
                starCamera.Rotation = viewer.GetSavedStarCameraRotation();

                NeighborhoodRenderer? neighborhoodRenderer = viewer.GetNeighborhoodRenderer();
                DensityModelInterface? densityModel = viewer.GetDensityModel();
                if (neighborhoodRenderer != null && densityModel != null)
                {
                    neighborhoodRenderer.BuildNeighborhood(
                        starCamera.GlobalPosition,
                        viewer.GalaxySeed,
                        densityModel,
                        viewer.GetReferenceDensity());
                }
            }
        }

        int savedStarSeed = viewer.GetSavedStarSeed();
        Vector3 savedStarPosition = viewer.GetSavedStarPosition();
        if (savedStarSeed != 0 && savedLevel == (int)GalaxyCoordinates.ZoomLevel.Subsector)
        {
            viewer.ApplyStarSelectionState(savedStarPosition, savedStarSeed);
        }
        else
        {
            SelectionIndicator? selectionIndicator = viewer.GetSelectionIndicator();
            if (selectionIndicator != null)
            {
                selectionIndicator.HideIndicator();
            }
        }

        viewer.UpdateInspectorState();
        viewer.SetStatus("Returned to galaxy view");
    }

    /// <summary>
    /// Clears saved viewer state.
    /// </summary>
    public void ClearSavedState(IGalaxyViewerSavedStateHost viewer)
    {
        viewer.SetSavedZoomLevel(-1);
        viewer.SetSavedQuadrant(null);
        viewer.SetSavedSector(null);
        viewer.SetSavedStarCameraPosition(Vector3.Zero);
        viewer.SetSavedStarCameraRotation(Vector3.Zero);
        viewer.SetSavedStarSeed(0);
        viewer.SetSavedStarPosition(Vector3.Zero);
    }

    /// <summary>
    /// Returns whether there is saved viewer state to restore.
    /// </summary>
    public bool HasSavedState(IGalaxyViewerSavedStateHost viewer)
    {
        return viewer.GetSavedZoomLevel() >= 0;
    }

    /// <summary>
    /// Creates a galaxy-save payload from the current viewer state.
    /// </summary>
    public GalaxySaveData CreateSaveData(GalaxyViewer viewer)
    {
        GalaxySaveData data = GalaxySaveData.Create((long)Time.GetUnixTimeFromSystem());
        data.GalaxySeed = viewer.GalaxySeed;

        GalaxyConfig? config = viewer.GetGalaxyConfig();
        if (config != null)
        {
            data.SetConfig(config);
        }

        Galaxy? galaxy = viewer.GetGalaxy();
        if (galaxy != null)
        {
            data.CachedSystemCount = galaxy.GetCachedSystemCount();
        }

        ZoomStateMachine? zoomMachine = viewer.GetZoomMachine();
        if (zoomMachine != null)
        {
            data.ZoomLevel = (GalaxyCoordinates.ZoomLevel)zoomMachine.GetCurrentLevel();
        }

        QuadrantSelector? quadrantSelector = viewer.GetQuadrantSelector();
        if (quadrantSelector != null
            && quadrantSelector.HasSelection()
            && quadrantSelector.SelectedCoords.VariantType == Variant.Type.Vector3I)
        {
            data.SelectedQuadrant = (Vector3I)quadrantSelector.SelectedCoords;
        }

        data.SelectedSector = viewer.GetSelectedSector();

        StarViewCamera? starCamera = viewer.GetStarCamera();
        if (starCamera != null && starCamera.IsInsideTree())
        {
            data.CameraPosition = starCamera.GlobalPosition;
            data.CameraRotation = starCamera.Rotation;
        }

        int selectedStarSeed = viewer.GetSelectedStarSeed();
        data.HasStarSelection = selectedStarSeed != 0;
        data.SelectedStarSeed = selectedStarSeed;
        data.SelectedStarPosition = viewer.GetSelectedStarPosition();

        JumpLaneRegion? jumpLaneRegion = viewer.GetJumpLaneRegion();
        if (jumpLaneRegion != null)
        {
            data.JumpLaneRegionData = jumpLaneRegion.ToDictionary();
        }

        JumpLaneResult? jumpLaneResult = viewer.GetJumpLaneResult();
        if (jumpLaneResult != null)
        {
            data.JumpLaneResultData = jumpLaneResult.ToDictionary();
        }

        MainApp? mainApp = viewer.GetParent()?.GetParent() as MainApp;
        if (mainApp != null)
        {
            data.SetBodyOverrides(mainApp.GetBodyOverrides());
        }

        return data;
    }

    /// <summary>
    /// Applies save data to restore viewer state.
    /// </summary>
    public void ApplySaveData(GalaxyViewer viewer, GalaxySaveData data)
    {
        if (data.HasConfig())
        {
            GalaxyConfig? config = data.GetConfig();
            if (config != null)
            {
                viewer.SetGalaxyConfig(config);
            }
        }

        if (data.GalaxySeed != viewer.GalaxySeed)
        {
            viewer.ChangeGalaxySeed(data.GalaxySeed);
        }

        QuadrantSelector? quadrantSelector = viewer.GetQuadrantSelector();
        if (data.SelectedQuadrant.HasValue && quadrantSelector != null)
        {
            Vector3I quadrantCoords = data.SelectedQuadrant.Value;
            GridCursor? quadrantCursor = viewer.GetQuadrantCursor();
            if (quadrantCursor != null)
            {
                quadrantCursor.Position = quadrantCoords;
            }

            quadrantSelector.SetSelection(Variant.CreateFrom(quadrantCoords));
        }
        else if (quadrantSelector != null)
        {
            quadrantSelector.ClearSelection();
        }

        viewer.SetSelectedSector(data.SelectedSector);
        if (data.SelectedSector.HasValue)
        {
            GridCursor? sectorCursor = viewer.GetSectorCursor();
            if (sectorCursor != null)
            {
                sectorCursor.Position = data.SelectedSector.Value;
            }
        }

        if (data.SelectedQuadrant.HasValue
            && (data.ZoomLevel == GalaxyCoordinates.ZoomLevel.Sector
                || data.ZoomLevel == GalaxyCoordinates.ZoomLevel.Subsector))
        {
            SectorRenderer? sectorRenderer = viewer.GetSectorRenderer();
            DensityModelInterface? densityModel = viewer.GetDensityModel();
            if (sectorRenderer != null && densityModel != null)
            {
                sectorRenderer.BuildForQuadrant(data.SelectedQuadrant.Value, densityModel);
            }
        }

        ZoomStateMachine? zoomMachine = viewer.GetZoomMachine();
        if (zoomMachine != null)
        {
            zoomMachine.SetLevel((int)data.ZoomLevel);
        }

        viewer.ApplyZoomLevelState((int)data.ZoomLevel);

        if (data.ZoomLevel == GalaxyCoordinates.ZoomLevel.Subsector)
        {
            StarViewCamera? starCamera = viewer.GetStarCamera();
            if (starCamera != null && starCamera.IsInsideTree())
            {
                starCamera.GlobalPosition = data.CameraPosition;
                starCamera.Rotation = data.CameraRotation;

                NeighborhoodRenderer? neighborhoodRenderer = viewer.GetNeighborhoodRenderer();
                DensityModelInterface? densityModel = viewer.GetDensityModel();
                if (neighborhoodRenderer != null && densityModel != null)
                {
                    neighborhoodRenderer.BuildNeighborhood(
                        starCamera.GlobalPosition,
                        viewer.GalaxySeed,
                        densityModel,
                        viewer.GetReferenceDensity());
                }
            }
        }

        if (data.HasStarSelection)
        {
            viewer.SetSelectedStarState(data.SelectedStarSeed, data.SelectedStarPosition);
            SelectionIndicator? selectionIndicator = viewer.GetSelectionIndicator();
            if (selectionIndicator != null)
            {
                selectionIndicator.ShowAt(data.SelectedStarPosition);
            }

            GalaxyInspectorPanel? inspectorPanel = viewer.GetInspectorPanel();
            if (inspectorPanel != null)
            {
                inspectorPanel.DisplaySelectedStar(data.SelectedStarPosition, data.SelectedStarSeed);
            }
        }
        else
        {
            viewer.ClearSelectedStarState();
            SelectionIndicator? selectionIndicator = viewer.GetSelectionIndicator();
            if (selectionIndicator != null)
            {
                selectionIndicator.HideIndicator();
            }
        }

        viewer.SetJumpLaneRegion(null);
        viewer.SetJumpLaneResult(null);

        if (data.JumpLaneRegionData.Count > 0)
        {
            viewer.SetJumpLaneRegion(JumpLaneRegion.FromDictionary(data.JumpLaneRegionData));
        }

        if (data.JumpLaneResultData.Count > 0)
        {
            viewer.SetJumpLaneResult(JumpLaneResult.FromDictionary(data.JumpLaneResultData));
        }

        viewer.UpdateInspectorState();
        viewer.RefreshJumpRoutePresentationState();
    }

    /// <summary>
    /// Opens the save dialog for galaxy save files.
    /// </summary>
    public void OnSavePressed(GalaxyViewer viewer)
    {
        FileDialog dialog = new()
        {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Access = FileDialog.AccessEnum.Userdata,
            Filters = ["*.sgg ; StarGen Galaxy", "*.json ; JSON Debug"],
        };

        dialog.CurrentFile = $"galaxy_{viewer.GalaxySeed}.sgg";
        dialog.FileSelected += path => OnSaveFileSelected(viewer, path);
        dialog.Canceled += dialog.QueueFree;
        viewer.AddChild(dialog);
        dialog.PopupCentered(new Vector2I(800, 600));
    }

    /// <summary>
    /// Opens the load dialog for galaxy save files.
    /// </summary>
    public void OnLoadPressed(GalaxyViewer viewer)
    {
        FileDialog dialog = new()
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Userdata,
            Filters = ["*.sgg ; StarGen Galaxy", "*.json ; JSON Debug"],
        };

        dialog.FileSelected += path => OnLoadFileSelected(viewer, path);
        dialog.Canceled += dialog.QueueFree;
        viewer.AddChild(dialog);
        dialog.PopupCentered(new Vector2I(800, 600));
    }

    /// <summary>
    /// Handles a chosen save path.
    /// </summary>
    public void OnSaveFileSelected(GalaxyViewer viewer, string path)
    {
        GalaxySaveData data = CreateSaveData(viewer);
        string error;
        if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            error = GalaxyPersistence.SaveJson(path, data);
        }
        else
        {
            error = GalaxyPersistence.SaveBinary(path, data);
        }

        if (string.IsNullOrEmpty(error))
        {
            viewer.SetStatus($"Saved to {Path.GetFileName(path)}");
            return;
        }

        viewer.SetStatus($"Save failed: {error}");
        GD.PushError(error);
    }

    /// <summary>
    /// Handles a chosen load path.
    /// </summary>
    public void OnLoadFileSelected(GalaxyViewer viewer, string path)
    {
        GalaxySaveData? data = GalaxyPersistence.LoadAuto(path);
        if (data == null)
        {
            viewer.SetStatus("Failed to load file");
            return;
        }

        if (!data.IsValid())
        {
            viewer.SetStatus("Invalid save data");
            return;
        }

        ApplySaveData(viewer, data);
        viewer.SetStatus($"Loaded from {Path.GetFileName(path)}");
    }
}
