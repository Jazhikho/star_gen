using System.IO;
using Godot;
using StarGen.Domain.Systems;
using StarGen.Services.Persistence;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Minimal system-viewer contract used by programmatic save/load helpers and tests.
/// </summary>
public interface ISystemViewerSaveLoadHost
{
    /// <summary>
    /// Returns the currently displayed system.
    /// </summary>
    SolarSystem? GetCurrentSystem();

    /// <summary>
    /// Updates the viewer status text.
    /// </summary>
    void SetStatus(string message);

    /// <summary>
    /// Updates the viewer error text.
    /// </summary>
    void SetError(string message);

    /// <summary>
    /// Displays a loaded system.
    /// </summary>
    void DisplaySystem(SolarSystem system);

    /// <summary>
    /// Updates the viewer seed display.
    /// </summary>
    void UpdateSeedDisplay(int seedValue);
}

/// <summary>
/// Handles solar-system viewer save/load flows.
/// </summary>
public partial class SystemViewerSaveLoad : RefCounted
{
    /// <summary>
    /// Opens a save dialog for the currently displayed system.
    /// </summary>
    public void OnSavePressed(SystemViewer viewer)
    {
        SolarSystem? currentSystem = viewer.GetCurrentSystem();
        if (currentSystem == null)
        {
            viewer.SetStatus("No system to save");
            return;
        }

        FileDialog dialog = new()
        {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Access = FileDialog.AccessEnum.Userdata,
            Filters = ["*.sgs ; StarGen System", "*.json ; JSON Debug"],
        };

        string defaultName = "system";
        if (!string.IsNullOrEmpty(currentSystem.Name))
        {
            defaultName = currentSystem.Name.ToLowerInvariant().Replace(" ", "_");
        }
        else if (currentSystem.Provenance != null)
        {
            defaultName = $"system_{currentSystem.Provenance.GenerationSeed}";
        }

        dialog.CurrentFile = $"{defaultName}.sgs";
        dialog.FileSelected += path => OnSaveFileSelected(viewer, path);
        dialog.Canceled += () => dialog.QueueFree();
        viewer.AddChild(dialog);
        dialog.PopupCentered(new Vector2I(800, 600));
    }

    /// <summary>
    /// Opens a load dialog for solar-system files.
    /// </summary>
    public void OnLoadPressed(SystemViewer viewer)
    {
        FileDialog dialog = new()
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Userdata,
            Filters = ["*.sgs ; StarGen System", "*.json ; JSON Debug"],
        };

        dialog.FileSelected += path => OnLoadFileSelected(viewer, path);
        dialog.Canceled += () => dialog.QueueFree();
        viewer.AddChild(dialog);
        dialog.PopupCentered(new Vector2I(800, 600));
    }

    /// <summary>
    /// Saves the current system after a file path has been chosen.
    /// </summary>
    public void OnSaveFileSelected(SystemViewer viewer, string path)
    {
        SolarSystem? currentSystem = viewer.GetCurrentSystem();
        if (currentSystem == null)
        {
            viewer.SetStatus("No system to save");
            return;
        }

        bool compress = !path.EndsWith(".json");
        string savePath = SystemPersistence.ResolveSavePath(path, compress);
        Error error = SystemPersistence.Save(currentSystem, savePath, compress);
        if (error == Error.Ok)
        {
            long size = SystemPersistence.GetFileSize(savePath);
            string sizeString = SystemPersistence.FormatFileSize(size);
            viewer.SetStatus($"Saved to {Path.GetFileName(savePath)} ({sizeString})");
            return;
        }

        viewer.SetError($"Failed to save: {error}");
    }

    /// <summary>
    /// Loads a system after a file path has been chosen.
    /// </summary>
    public void OnLoadFileSelected(SystemViewer viewer, string path)
    {
        SystemPersistenceLoadResult result = SystemPersistence.Load(path);
        if (!result.Success)
        {
            viewer.SetError($"Failed to load: {result.ErrorMessage}");
            return;
        }

        if (result.System == null)
        {
            viewer.SetError("Loaded file contains no system data");
            return;
        }

        viewer.DisplaySystem(result.System);
        if (result.System.Provenance != null)
        {
            viewer.UpdateSeedDisplay((int)result.System.Provenance.GenerationSeed);
        }

        viewer.SetStatus($"Loaded: {Path.GetFileName(path)}");
    }

    /// <summary>
    /// Saves the current system directly to a path.
    /// </summary>
    public Error SaveToPath(ISystemViewerSaveLoadHost viewer, string path, bool compress = true)
    {
        SolarSystem? currentSystem = viewer.GetCurrentSystem();
        if (currentSystem == null)
        {
            return Error.InvalidData;
        }

        return SystemPersistence.Save(currentSystem, SystemPersistence.ResolveSavePath(path, compress), compress);
    }

    /// <summary>
    /// Loads a system directly from a path.
    /// </summary>
    public SystemPersistenceLoadResult LoadFromPath(string path)
    {
        return SystemPersistence.Load(path);
    }

    /// <summary>
    /// Reads the current system from the viewer via its existing GDScript API.
    /// </summary>
}
