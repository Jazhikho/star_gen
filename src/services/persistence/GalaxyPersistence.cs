using System.IO;
using System.Text;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;

namespace StarGen.Services.Persistence;

/// <summary>
/// File I/O service for galaxy save payloads.
/// </summary>
public static class GalaxyPersistence
{
    /// <summary>
    /// Default file extension for binary galaxy saves.
    /// </summary>
    public const string BinaryExtension = "sgg";

    /// <summary>
    /// Default file extension for JSON galaxy saves.
    /// </summary>
    public const string JsonExtension = "json";

    private const string BinaryNotYetPortedMessage =
        "Binary .sgg format is not yet ported in C#; use JSON or the GDScript path for now.";

    /// <summary>
    /// Saves galaxy data to a JSON file.
    /// </summary>
    public static string SaveJson(string path, GalaxySaveData? data)
    {
        if (data == null)
        {
            return "No data to save";
        }

        if (!data.IsValid())
        {
            return "Invalid galaxy save data";
        }

        string globalPath = ProjectSettings.GlobalizePath(path);
        EnsureDirectoryExists(globalPath);
        string json = Json.Stringify(data.ToDictionary(), "  ");
        File.WriteAllText(globalPath, json, Encoding.UTF8);
        return string.Empty;
    }

    /// <summary>
    /// Loads galaxy data from a JSON file.
    /// </summary>
    public static GalaxySaveData? LoadJson(string path)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        if (!File.Exists(globalPath))
        {
            return null;
        }

        string json = File.ReadAllText(globalPath, Encoding.UTF8);
        Variant parsed = Json.ParseString(json);
        if (parsed.VariantType != Variant.Type.Dictionary)
        {
            GD.PushError("Invalid JSON structure");
            return null;
        }

        return GalaxySaveData.FromDictionary((Dictionary)parsed);
    }

    /// <summary>
    /// Preserves the current contract for binary saves without silently changing the format.
    /// </summary>
    public static string SaveBinary(string path, GalaxySaveData? data)
    {
        if (data == null)
        {
            return "No data to save";
        }

        if (!data.IsValid())
        {
            return "Invalid galaxy save data";
        }

        return BinaryNotYetPortedMessage;
    }

    /// <summary>
    /// Preserves the current contract for binary loads without silently changing the format.
    /// </summary>
    public static GalaxySaveData? LoadBinary(string path)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        if (!File.Exists(globalPath))
        {
            GD.PushError($"File not found: {path}");
            return null;
        }

        GD.PushError(BinaryNotYetPortedMessage);
        return null;
    }

    /// <summary>
    /// Loads galaxy data by file extension.
    /// </summary>
    public static GalaxySaveData? LoadAuto(string path)
    {
        string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
        return extension == JsonExtension ? LoadJson(path) : LoadBinary(path);
    }

    /// <summary>
    /// Returns the recommended file-dialog filter.
    /// </summary>
    public static string GetFileFilter()
    {
        return "*.sgg ; StarGen Galaxy, *.json ; JSON Debug";
    }

    /// <summary>
    /// Ensures the target directory exists before writing.
    /// </summary>
    private static void EnsureDirectoryExists(string globalPath)
    {
        string? directoryPath = Path.GetDirectoryName(globalPath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}
