using System.Collections.Generic;
using System.IO;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;

namespace StarGen.Services.Persistence;

/// <summary>
/// File I/O service for celestial-body persistence.
/// </summary>
public static class CelestialPersistence
{
    /// <summary>
    /// Default directory for saved celestial bodies.
    /// </summary>
    public const string DefaultSaveDir = "user://celestial_bodies/";

    /// <summary>
    /// Saves a celestial body to a JSON file.
    /// </summary>
    public static Error SaveBody(CelestialBody? body, string filePath)
    {
        if (body == null)
        {
            return Error.InvalidParameter;
        }

        string globalPath = ProjectSettings.GlobalizePath(filePath);
        string? directoryPath = Path.GetDirectoryName(globalPath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string json = CelestialSerializer.ToJson(body, true);
        File.WriteAllText(globalPath, json);
        return Error.Ok;
    }

    /// <summary>
    /// Loads a celestial body from a JSON file.
    /// Returns null if the file is missing, unreadable, or JSON is invalid.
    /// </summary>
    public static CelestialBody? LoadBody(string filePath)
    {
        string globalPath = ProjectSettings.GlobalizePath(filePath);
        if (!File.Exists(globalPath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(globalPath);
            return CelestialSerializer.FromJson(json);
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Returns a default save path for a celestial body.
    /// </summary>
    public static string GetDefaultPath(CelestialBody body)
    {
        string fileName;
        if (string.IsNullOrEmpty(body.Id))
        {
            fileName = "unnamed";
        }
        else
        {
            fileName = body.Id;
        }
        fileName = fileName.Replace(" ", "_").ToLowerInvariant();
        return DefaultSaveDir + fileName + ".json";
    }

    /// <summary>
    /// Lists saved celestial-body JSON files in the target directory.
    /// </summary>
    public static Godot.Collections.Array<string> ListSavedBodies(string? dirPath = null)
    {
        string effectiveDir;
        if (string.IsNullOrEmpty(dirPath))
        {
            effectiveDir = DefaultSaveDir;
        }
        else
        {
            effectiveDir = dirPath;
        }
        string globalPath = ProjectSettings.GlobalizePath(effectiveDir);
        Godot.Collections.Array<string> results = new();

        if (!Directory.Exists(globalPath))
        {
            return results;
        }

        IEnumerable<string> files = Directory.EnumerateFiles(globalPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string file in files)
        {
            string relativePath = effectiveDir.TrimEnd('/', '\\') + "/" + Path.GetFileName(file);
            results.Add(relativePath);
        }

        return results;
    }

    /// <summary>
    /// Deletes a saved celestial-body file.
    /// </summary>
    public static Error DeleteBody(string filePath)
    {
        string globalPath = ProjectSettings.GlobalizePath(filePath);
        if (!File.Exists(globalPath))
        {
            return Error.FileNotFound;
        }

        File.Delete(globalPath);
        return Error.Ok;
    }
}
