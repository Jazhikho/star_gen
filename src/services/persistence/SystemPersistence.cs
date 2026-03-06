using System.IO;
using System.IO.Compression;
using System.Text;
using Godot;
using Godot.Collections;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;

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
    /// Current save schema version.
    /// </summary>
    public const int SaveVersion = 1;

    /// <summary>
    /// Supported save modes for system payloads.
    /// </summary>
    public enum SaveMode
    {
        Compact = 1,
        Full = 2,
    }

    /// <summary>
    /// Saves a solar system to disk.
    /// </summary>
    public static Error Save(SolarSystem? system, string path, bool compress = true)
    {
        if (system == null)
        {
            return Error.InvalidParameter;
        }

        string savePath = ResolveSavePath(path, compress);
        Dictionary payload = CreateSavePayload(system);
        if (!compress)
        {
            return SaveJson(payload, savePath);
        }

        return SaveCompressed(payload, savePath);
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
            Dictionary data;
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

            SolarSystem? system = DeserializePayload(data);
            if (system == null)
            {
                result.ErrorMessage = "Failed to deserialize system";
                return result;
            }

            result.System = system;
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
    /// Returns the save path with the expected extension applied.
    /// </summary>
    public static string ResolveSavePath(string path, bool compress = true)
    {
        if (!compress)
        {
            if (path.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            return $"{Path.ChangeExtension(path, null)}.{JsonExtension}";
        }

        if (path.EndsWith(".sgs", System.StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return $"{Path.ChangeExtension(path, null)}.{BinaryExtension}";
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
    /// Builds either a compact regeneration payload or a full snapshot.
    /// </summary>
    private static Dictionary CreateSavePayload(SolarSystem system)
    {
        if (CanUseCompactSave(system))
        {
            return CreateCompactPayload(system);
        }

        return CreateFullPayload(system);
    }

    /// <summary>
    /// Returns whether the system can be reconstructed from provenance alone.
    /// </summary>
    private static bool CanUseCompactSave(SolarSystem system)
    {
        return system.Provenance != null
            && system.Provenance.GenerationSeed != 0
            && system.Provenance.SpecSnapshot.Count > 0;
    }

    /// <summary>
    /// Builds the compact regeneration payload.
    /// </summary>
    private static Dictionary CreateCompactPayload(SolarSystem system)
    {
        return new Dictionary
        {
            ["version"] = SaveVersion,
            ["save_mode"] = (int)SaveMode.Compact,
            ["timestamp"] = (long)Time.GetUnixTimeFromSystem(),
            ["type"] = "system_save",
            ["id"] = system.Id,
            ["name"] = system.Name,
            ["seed"] = system.Provenance!.GenerationSeed,
            ["spec"] = DuplicateDictionary(system.Provenance.SpecSnapshot),
            ["generator_version"] = system.Provenance.GeneratorVersion,
        };
    }

    /// <summary>
    /// Builds the full snapshot payload.
    /// </summary>
    private static Dictionary CreateFullPayload(SolarSystem system)
    {
        return new Dictionary
        {
            ["version"] = SaveVersion,
            ["save_mode"] = (int)SaveMode.Full,
            ["timestamp"] = (long)Time.GetUnixTimeFromSystem(),
            ["type"] = "system_save",
            ["system"] = SystemSerializer.ToDictionary(system),
        };
    }

    /// <summary>
    /// Deserializes either the new wrapped payload or the legacy direct system payload.
    /// </summary>
    private static SolarSystem? DeserializePayload(Dictionary data)
    {
        if (IsLegacySystemPayload(data))
        {
            return SystemSerializer.FromDictionary(data);
        }

        int version = GetInt(data, "version", SaveVersion);
        if (version > SaveVersion)
        {
            return null;
        }

        SaveMode saveMode = (SaveMode)GetInt(data, "save_mode", (int)SaveMode.Compact);
        if (saveMode == SaveMode.Full)
        {
            return DeserializeFullPayload(data);
        }

        return RegenerateSystem(data);
    }

    /// <summary>
    /// Returns whether the payload is an older direct system dictionary.
    /// </summary>
    private static bool IsLegacySystemPayload(Dictionary data)
    {
        string type = GetString(data, "type", string.Empty);
        if (type == "solar_system")
        {
            return true;
        }

        return data.ContainsKey("hierarchy") || data.ContainsKey("bodies");
    }

    /// <summary>
    /// Reconstructs a system from a full snapshot payload.
    /// </summary>
    private static SolarSystem? DeserializeFullPayload(Dictionary data)
    {
        if (!data.ContainsKey("system") || data["system"].VariantType != Variant.Type.Dictionary)
        {
            return null;
        }

        return SystemSerializer.FromDictionary((Dictionary)data["system"]);
    }

    /// <summary>
    /// Reconstructs a system from a seed and spec snapshot.
    /// </summary>
    private static SolarSystem? RegenerateSystem(Dictionary data)
    {
        Dictionary specData = GetDictionary(data, "spec");
        int seedValue = GetInt(data, "seed", GetInt(specData, "generation_seed", 0));

        SolarSystemSpec spec;
        if (specData.Count == 0)
        {
            spec = new SolarSystemSpec(seedValue, 1, 1);
        }
        else
        {
            spec = SolarSystemSpec.FromDictionary(specData);
        }

        if (spec.GenerationSeed == 0 && seedValue != 0)
        {
            spec.GenerationSeed = seedValue;
        }

        SolarSystem? system = SystemFixtureGenerator.GenerateSystem(spec);
        if (system == null)
        {
            return null;
        }

        string savedId = GetString(data, "id", string.Empty);
        if (!string.IsNullOrEmpty(savedId))
        {
            system.Id = savedId;
        }

        string savedName = GetString(data, "name", string.Empty);
        if (!string.IsNullOrEmpty(savedName))
        {
            system.Name = savedName;
        }

        return system;
    }

    /// <summary>
    /// Saves a payload as JSON.
    /// </summary>
    private static Error SaveJson(Dictionary data, string path)
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
    private static Error SaveCompressed(Dictionary data, string path)
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
    private static Dictionary LoadJson(string path, SystemPersistenceLoadResult result)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        string json = File.ReadAllText(globalPath, Encoding.UTF8);
        Json parser = new();
        if (parser.Parse(json) != Error.Ok || parser.Data.VariantType != Variant.Type.Dictionary)
        {
            result.ErrorMessage = "Invalid JSON structure";
            return new Dictionary();
        }

        return (Dictionary)parser.Data;
    }

    /// <summary>
    /// Loads and parses a GZIP-compressed JSON payload.
    /// </summary>
    private static Dictionary LoadCompressed(string path, SystemPersistenceLoadResult result)
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
                return new Dictionary();
            }

            return (Dictionary)parser.Data;
        }
        catch (InvalidDataException)
        {
            result.ErrorMessage = "Decompression failed";
            return new Dictionary();
        }
    }

    /// <summary>
    /// Reads an integer value from a dictionary.
    /// </summary>
    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        switch (value.VariantType)
        {
            case Variant.Type.Int:
                return (int)value;
            case Variant.Type.Float:
                return (int)(double)value;
            case Variant.Type.String:
                {
                    if (int.TryParse((string)value, out int parsed))
                    {
                        return parsed;
                    }
                    return fallback;
                }
            default:
                return fallback;
        }
    }

    /// <summary>
    /// Reads a string value from a dictionary.
    /// </summary>
    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.String)
        {
            return (string)value;
        }

        return fallback;
    }

    /// <summary>
    /// Reads a dictionary payload value or returns an empty dictionary.
    /// </summary>
    private static Dictionary GetDictionary(Dictionary data, string key)
    {
        if (data.ContainsKey(key) && data[key].VariantType == Variant.Type.Dictionary)
        {
            return DuplicateDictionary((Dictionary)data[key]);
        }

        return new Dictionary();
    }

    /// <summary>
    /// Duplicates a dictionary payload shallowly.
    /// </summary>
    private static Dictionary DuplicateDictionary(Dictionary source)
    {
        Dictionary clone = new();
        foreach (Variant key in source.Keys)
        {
            clone[key] = source[key];
        }

        return clone;
    }
}
