using System.IO;
using System.IO.Compression;
using System.Text;
using Godot;
using StarGen.Domain.Systems;

namespace StarGen.Services.Persistence;

/// <summary>
/// File I/O service for solar-system persistence.
/// </summary>
public static class SystemPersistence
{
    /// <summary>
    /// Compressed binary extension.
    /// </summary>
    public const string BinaryExtension = "sgs";

    /// <summary>
    /// JSON debug extension.
    /// </summary>
    public const string JsonExtension = "json";

    /// <summary>
    /// Saves a solar system to disk.
    /// </summary>
    public static Error Save(SolarSystem? system, string path, bool compress = true)
    {
        if (system == null)
        {
            return Error.InvalidParameter;
        }

        string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
        return extension == JsonExtension || !compress
            ? SaveJson(SystemSerializer.ToDictionary(system), path)
            : SaveCompressed(SystemSerializer.ToDictionary(system), path);
    }

    /// <summary>
    /// Loads a solar system from disk.
    /// </summary>
    public static SystemPersistenceLoadResult Load(string path)
    {
        SystemPersistenceLoadResult result = new();
        string globalPath = ProjectSettings.GlobalizePath(path);
        if (!File.Exists(globalPath))
        {
            result.ErrorMessage = $"File not found: {path}";
            return result;
        }

        string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
        Godot.Collections.Dictionary data = extension == JsonExtension
            ? LoadJson(path, result)
            : LoadCompressed(path, result);

        if (data.Count == 0)
        {
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                result.ErrorMessage = "Failed to parse file";
            }

            return result;
        }

        result.System = SystemSerializer.FromDictionary(data);
        if (result.System == null)
        {
            result.ErrorMessage = "Failed to deserialize system";
            return result;
        }

        result.Success = true;
        return result;
    }

    /// <summary>
    /// Returns the size of a persisted system file in bytes.
    /// </summary>
    public static long GetFileSize(string path)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        return File.Exists(globalPath) ? new FileInfo(globalPath).Length : 0L;
    }

    /// <summary>
    /// Formats a file size for display.
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        if (bytes < 1024L)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024L * 1024L)
        {
            return $"{(bytes / 1024.0):0.0} KB";
        }

        return $"{(bytes / (1024.0 * 1024.0)):0.0} MB";
    }

    /// <summary>
    /// Saves a payload as JSON.
    /// </summary>
    private static Error SaveJson(Godot.Collections.Dictionary data, string path)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        EnsureDirectoryExists(globalPath);
        string json = Json.Stringify(data, "\t");
        File.WriteAllText(globalPath, json, Encoding.UTF8);
        return Error.Ok;
    }

    /// <summary>
    /// Saves a payload as GZIP-compressed JSON.
    /// </summary>
    private static Error SaveCompressed(Godot.Collections.Dictionary data, string path)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        EnsureDirectoryExists(globalPath);
        string json = Json.Stringify(data);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        using FileStream fileStream = File.Create(globalPath);
        using GZipStream gzipStream = new(fileStream, CompressionLevel.SmallestSize);
        gzipStream.Write(bytes, 0, bytes.Length);
        return Error.Ok;
    }

    /// <summary>
    /// Loads and parses a JSON payload.
    /// </summary>
    private static Godot.Collections.Dictionary LoadJson(string path, SystemPersistenceLoadResult result)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        string json = File.ReadAllText(globalPath, Encoding.UTF8);
        Variant parsed = Json.ParseString(json);
        if (parsed.VariantType != Variant.Type.Dictionary)
        {
            result.ErrorMessage = "Invalid JSON structure";
            return new Godot.Collections.Dictionary();
        }

        return (Godot.Collections.Dictionary)parsed;
    }

    /// <summary>
    /// Loads and parses a GZIP-compressed JSON payload.
    /// </summary>
    private static Godot.Collections.Dictionary LoadCompressed(string path, SystemPersistenceLoadResult result)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        try
        {
            using FileStream fileStream = File.OpenRead(globalPath);
            using GZipStream gzipStream = new(fileStream, CompressionMode.Decompress);
            using StreamReader reader = new(gzipStream, Encoding.UTF8);
            string json = reader.ReadToEnd();
            Variant parsed = Json.ParseString(json);
            if (parsed.VariantType != Variant.Type.Dictionary)
            {
                result.ErrorMessage = "Invalid JSON structure";
                return new Godot.Collections.Dictionary();
            }

            return (Godot.Collections.Dictionary)parsed;
        }
        catch (InvalidDataException)
        {
            result.ErrorMessage = "Decompression failed";
            return new Godot.Collections.Dictionary();
        }
    }

    /// <summary>
    /// Ensures the target directory for a file path exists.
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
