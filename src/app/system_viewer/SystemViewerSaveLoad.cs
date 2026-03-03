using System.IO;
using Godot;
using StarGen.Domain.Systems;
using StarGen.Services.Persistence;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Handles solar-system viewer save/load flows.
/// </summary>
public partial class SystemViewerSaveLoad : RefCounted
{
    /// <summary>
    /// Opens a save dialog for the currently displayed system.
    /// </summary>
    public void OnSavePressed(Node viewer)
    {
        SolarSystem? currentSystem = GetCurrentSystem(viewer);
        if (currentSystem == null)
        {
            SetStatus(viewer, "No system to save");
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
    public void OnLoadPressed(Node viewer)
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
    public void OnSaveFileSelected(Node viewer, string path)
    {
        SolarSystem? currentSystem = GetCurrentSystem(viewer);
        if (currentSystem == null)
        {
            SetStatus(viewer, "No system to save");
            return;
        }

        bool compress = !path.EndsWith(".json");
        Error error = SystemPersistence.Save(currentSystem, path, compress);
        if (error == Error.Ok)
        {
            long size = SystemPersistence.GetFileSize(path);
            string sizeString = SystemPersistence.FormatFileSize(size);
            SetStatus(viewer, $"Saved to {Path.GetFileName(path)} ({sizeString})");
            return;
        }

        SetError(viewer, $"Failed to save: {error}");
    }

    /// <summary>
    /// Loads a system after a file path has been chosen.
    /// </summary>
    public void OnLoadFileSelected(Node viewer, string path)
    {
        SystemPersistenceLoadResult result = SystemPersistence.Load(path);
        if (!result.Success)
        {
            SetError(viewer, $"Failed to load: {result.ErrorMessage}");
            return;
        }

        if (result.System == null)
        {
            SetError(viewer, "Loaded file contains no system data");
            return;
        }

        DisplaySystem(viewer, result.System);
        if (result.System.Provenance != null)
        {
            UpdateSeedDisplay(viewer, (int)result.System.Provenance.GenerationSeed);
        }

        SetStatus(viewer, $"Loaded: {Path.GetFileName(path)}");
    }

    /// <summary>
    /// Saves the current system directly to a path.
    /// </summary>
    public Error SaveToPath(Node viewer, string path, bool compress = true)
    {
        SolarSystem? currentSystem = GetCurrentSystem(viewer);
        if (currentSystem == null)
        {
            return Error.InvalidData;
        }

        return SystemPersistence.Save(currentSystem, path, compress);
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
    private static SolarSystem? GetCurrentSystem(Node viewer)
    {
        if (viewer is SystemViewer typedViewer)
        {
            return typedViewer.GetCurrentSystem();
        }

        Variant systemVariant = viewer.Call("get_current_system");
        if (systemVariant.VariantType == Variant.Type.Nil)
        {
            return null;
        }

        return systemVariant.As<SolarSystem>();
    }

    /// <summary>
    /// Updates the viewer status text for either runtime path.
    /// </summary>
    private static void SetStatus(Node viewer, string message)
    {
        if (viewer is SystemViewer typedViewer)
        {
            typedViewer.SetStatus(message);
            return;
        }

        viewer.Call("set_status", message);
    }

    /// <summary>
    /// Updates the viewer error text for either runtime path.
    /// </summary>
    private static void SetError(Node viewer, string message)
    {
        if (viewer is SystemViewer typedViewer)
        {
            typedViewer.SetError(message);
            return;
        }

        viewer.Call("set_error", message);
    }

    /// <summary>
    /// Displays a loaded system for either runtime path.
    /// </summary>
    private static void DisplaySystem(Node viewer, SolarSystem system)
    {
        if (viewer is SystemViewer typedViewer)
        {
            typedViewer.DisplaySystem(system);
            return;
        }

        viewer.Call("display_system", system);
    }

    /// <summary>
    /// Updates the seed input for either runtime path.
    /// </summary>
    private static void UpdateSeedDisplay(Node viewer, int seedValue)
    {
        if (viewer is SystemViewer typedViewer)
        {
            typedViewer.UpdateSeedDisplay(seedValue);
            return;
        }

        viewer.Call("update_seed_display", seedValue);
    }
}
