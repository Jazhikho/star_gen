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
        Godot.Collections.Dictionary payload = SystemSerializer.ToDictionary(system);
        if (extension == JsonExtension || !compress)
        {
            return SaveJson(payload, path);
        }

        return SaveCompressed(payload, path);
    }

    /// <summary>
    /// Loads a solar system from disk.
    /// On failure, ErrorMessage is set; Success is false and System may be null.
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

        try
        {
            string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            Godot.Collections.Dictionary data;
            if (extension == JsonExtension)
            {
                data = LoadJson(path, result);
            }
            else
            {
                data = LoadCompressed(path, result);
            }

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
        catch (System.Exception ex)
        {
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    /// <summary>
    /// Returns the size of a persisted system file in bytes.
    /// </summary>
    public static long GetFileSize(string path)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        if (File.Exists(globalPath))
        {
            return new FileInfo(globalPath).Length;
        }

        return 0L;
    }

    /// <summary>
    /// Formats a file size for display.
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        return PersistenceUtils.FormatFileSize(bytes);
    }

    /// <summary>
    /// Saves a payload as JSON.
    /// </summary>
    private static Error SaveJson(Godot.Collections.Dictionary data, string path)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        PersistenceUtils.EnsureDirectoryExists(globalPath);
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
        PersistenceUtils.EnsureDirectoryExists(globalPath);
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
        Json parser = new();
        if (parser.Parse(json) != Error.Ok || parser.Data.VariantType != Variant.Type.Dictionary)
        {
            result.ErrorMessage = "Invalid JSON structure";
            return new Godot.Collections.Dictionary();
        }

        return (Godot.Collections.Dictionary)parser.Data;
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
            Json parser = new();
            if (parser.Parse(json) != Error.Ok || parser.Data.VariantType != Variant.Type.Dictionary)
            {
                result.ErrorMessage = "Invalid JSON structure";
                return new Godot.Collections.Dictionary();
            }

            return (Godot.Collections.Dictionary)parser.Data;
        }
        catch (InvalidDataException)
        {
            result.ErrorMessage = "Decompression failed";
            return new Godot.Collections.Dictionary();
        }
    }

}
