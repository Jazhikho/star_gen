using System.IO;
using System.IO.Compression;
using System.Text;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;

namespace StarGen.Services.Persistence;

/// <summary>
/// Efficient celestial-body save/load service that prefers regeneration payloads.
/// Regeneration and generator helpers in SaveData.Generators.cs.
/// </summary>
public static partial class SaveData
{
    /// <summary>
    /// Legacy generic compressed body extension.
    /// </summary>
    public const string LegacyBinaryExtension = "sgb";

    /// <summary>
    /// Compressed asteroid-body extension.
    /// </summary>
    public const string AsteroidBinaryExtension = "sga";

    /// <summary>
    /// Compressed planet/moon-body extension.
    /// </summary>
    public const string WorldBinaryExtension = "sgp";

    /// <summary>
    /// Compressed star-body extension.
    /// </summary>
    public const string StarBinaryExtension = "sgt";

    /// <summary>
    /// JSON debug extension.
    /// </summary>
    public const string JsonExtension = "json";

    /// <summary>
    /// Supported save modes.
    /// </summary>
    public enum SaveMode
    {
        Minimal = 0,
        Compact = 1,
        Full = 2,
    }

    /// <summary>
    /// Current save-data schema version.
    /// </summary>
    public const int SaveVersion = 1;

    private static readonly byte[] CompressedMagic = Encoding.ASCII.GetBytes("SGB1");

    /// <summary>
    /// Saves a celestial body using the selected save mode.
    /// </summary>
    public static Error SaveBody(
        CelestialBody? body,
        string path,
        SaveMode mode = SaveMode.Compact,
        bool compress = true)
    {
        if (body == null)
        {
            return Error.InvalidParameter;
        }

        string savePath = ResolveSavePath(body, path, compress);
        Dictionary data = CreateSaveData(body, mode);
        if (compress)
        {
            return SaveCompressed(savePath, data);
        }

        return SaveJson(savePath, data);
    }

    /// <summary>
    /// Saves an edited body using full serialization.
    /// </summary>
    public static Error SaveEditedBody(
        CelestialBody? body,
        string path,
        bool compress = true)
    {
        return SaveBody(body, path, SaveMode.Full, compress);
    }

    /// <summary>
    /// Returns the preferred compressed extension for a body type.
    /// </summary>
    public static string GetPreferredBinaryExtension(CelestialType.Type bodyType)
    {
        return bodyType switch
        {
            CelestialType.Type.Star => StarBinaryExtension,
            CelestialType.Type.Asteroid => AsteroidBinaryExtension,
            CelestialType.Type.Planet => WorldBinaryExtension,
            CelestialType.Type.Moon => WorldBinaryExtension,
            _ => LegacyBinaryExtension,
        };
    }

    /// <summary>
    /// Returns the preferred file-dialog filters for a specific body type.
    /// </summary>
    public static string[] GetFileFilters(CelestialType.Type? bodyType = null, bool includeLegacy = false)
    {
        string binaryFilter;
        if (!bodyType.HasValue)
        {
            if (includeLegacy)
            {
                return
                [
                    "*.sgt ; StarGen Star",
                    "*.sgp ; StarGen Planet or Moon",
                    "*.sga ; StarGen Asteroid",
                    "*.sgb ; StarGen Legacy Body",
                    "*.json ; JSON Debug",
                ];
            }

            return
            [
                "*.sgt ; StarGen Star",
                "*.sgp ; StarGen Planet or Moon",
                "*.sga ; StarGen Asteroid",
                "*.json ; JSON Debug",
            ];
        }

        binaryFilter = bodyType.Value switch
        {
            CelestialType.Type.Star => "*.sgt ; StarGen Star",
            CelestialType.Type.Asteroid => "*.sga ; StarGen Asteroid",
            CelestialType.Type.Planet => "*.sgp ; StarGen Planet or Moon",
            CelestialType.Type.Moon => "*.sgp ; StarGen Planet or Moon",
            _ => "*.sgb ; StarGen Body",
        };

        if (includeLegacy)
        {
            return [binaryFilter, "*.sgb ; StarGen Legacy Body", "*.json ; JSON Debug"];
        }

        return [binaryFilter, "*.json ; JSON Debug"];
    }

    /// <summary>
    /// Returns the resolved save path with the expected extension applied.
    /// </summary>
    public static string ResolveSavePath(CelestialBody body, string path, bool compress = true)
    {
        if (!compress)
        {
            if (path.EndsWith(".json"))
            {
                return path;
            }

            return $"{Path.ChangeExtension(path, null)}.json";
        }

        string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
        if (IsKnownBinaryExtension(extension))
        {
            return path;
        }

        string preferredExtension = GetPreferredBinaryExtension(body.Type);
        return $"{Path.ChangeExtension(path, null)}.{preferredExtension}";
    }

    /// <summary>
    /// Loads a celestial body from a save file.
    /// On failure, ErrorMessage is set and the result's Success is false.
    /// </summary>
    public static SaveDataLoadResult LoadBody(string path)
    {
        try
        {
            Dictionary data;
            string errorMessage;
            string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();

            if (IsKnownBinaryExtension(extension))
            {
                data = LoadCompressed(path, out errorMessage);
            }
            else
            {
                data = LoadJson(path, out errorMessage);
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return SaveDataLoadResult.CreateError(errorMessage);
            }

            if (data.Count == 0)
            {
                return SaveDataLoadResult.CreateError("File is empty or invalid");
            }

            int version = GetInt(data, "version", 0);
            if (version > SaveVersion)
            {
                string msg = $"Save file version {version} is newer than supported version {SaveVersion}";
                return SaveDataLoadResult.CreateError(msg);
            }

            SaveMode saveMode = (SaveMode)GetInt(data, "save_mode", (int)SaveMode.Compact);
            StarGen.Domain.Celestial.CelestialBody? body;
            if (saveMode == SaveMode.Full)
            {
                body = DeserializeBody(data);
            }
            else
            {
                body = RegenerateBody(data);
            }

            if (body == null)
            {
                return SaveDataLoadResult.CreateError("Failed to reconstruct body from save data");
            }

            return SaveDataLoadResult.CreateSuccess(body);
        }
        catch (System.Exception)
        {
            return SaveDataLoadResult.CreateError("Load failed");
        }
    }

    /// <summary>
    /// Returns the size of a save file in bytes.
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
    /// Formats a file size for UI display.
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        return PersistenceUtils.FormatFileSize(bytes);
    }

    /// <summary>
    /// Preserves the existing binary save contract without silently changing the file format.
    /// </summary>
    private static Error SaveCompressed(string path, Dictionary data)
    {
        try
        {
            string savePath;
            string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            if (IsKnownBinaryExtension(extension))
            {
                savePath = path;
            }
            else
            {
                savePath = $"{Path.ChangeExtension(path, null)}.{LegacyBinaryExtension}";
            }

            string globalPath = ProjectSettings.GlobalizePath(savePath);
            PersistenceUtils.EnsureDirectoryExists(globalPath);

            string json = Json.Stringify(data);
            byte[] payload = Encoding.UTF8.GetBytes(json);

            using FileStream stream = File.Create(globalPath);
            stream.Write(CompressedMagic, 0, CompressedMagic.Length);
            using GZipStream gzip = new(stream, CompressionLevel.Optimal, leaveOpen: false);
            gzip.Write(payload, 0, payload.Length);
            return Error.Ok;
        }
        catch (System.Exception)
        {
            return Error.Failed;
        }
    }

    /// <summary>
    /// Saves an uncompressed JSON payload.
    /// </summary>
    private static Error SaveJson(string path, Dictionary data)
    {
        string savePath;
        if (path.EndsWith(".json"))
        {
            savePath = path;
        }
        else
        {
            savePath = $"{Path.ChangeExtension(path, null)}.json";
        }
        string globalPath = ProjectSettings.GlobalizePath(savePath);
        PersistenceUtils.EnsureDirectoryExists(globalPath);
        string json = Json.Stringify(data, "\t");
        File.WriteAllText(globalPath, json, Encoding.UTF8);
        return Error.Ok;
    }

    /// <summary>
    /// Preserves the existing binary load contract without silently changing the file format.
    /// </summary>
    private static Dictionary LoadCompressed(string path, out string errorMessage)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        if (!File.Exists(globalPath))
        {
            errorMessage = $"Could not open file: {path}";
            return new Dictionary();
        }

        try
        {
            using FileStream stream = File.OpenRead(globalPath);
            byte[] header = new byte[CompressedMagic.Length];
            int bytesRead = stream.Read(header, 0, header.Length);
            if (bytesRead != CompressedMagic.Length)
            {
                errorMessage = "Invalid file format";
                return new Dictionary();
            }

            for (int index = 0; index < CompressedMagic.Length; index += 1)
            {
                if (header[index] != CompressedMagic[index])
                {
                    errorMessage = "Invalid file format";
                    return new Dictionary();
                }
            }

            using GZipStream gzip = new(stream, CompressionMode.Decompress, leaveOpen: false);
            using MemoryStream output = new();
            gzip.CopyTo(output);

            string json = Encoding.UTF8.GetString(output.ToArray());
            if (string.IsNullOrEmpty(json))
            {
                errorMessage = "Invalid file format";
                return new Dictionary();
            }

            Json parser = new();
            Error parseError = parser.Parse(json);
            if (parseError != Error.Ok)
            {
                errorMessage = "Invalid JSON";
                return new Dictionary();
            }

            if (parser.Data.VariantType != Variant.Type.Dictionary)
            {
                errorMessage = "Invalid file format";
                return new Dictionary();
            }

            errorMessage = string.Empty;
            return (Dictionary)parser.Data;
        }
        catch (InvalidDataException)
        {
            errorMessage = "Invalid file format";
            return new Dictionary();
        }
        catch (System.Exception ex)
        {
            errorMessage = ex.Message;
            return new Dictionary();
        }

    }

    /// <summary>
    /// Loads and parses a JSON payload.
    /// </summary>
    private static Dictionary LoadJson(string path, out string errorMessage)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        if (!File.Exists(globalPath))
        {
            errorMessage = $"Could not open file: {path}";
            return new Dictionary();
        }

        string json = File.ReadAllText(globalPath, Encoding.UTF8);
        if (string.IsNullOrEmpty(json))
        {
            errorMessage = "File is empty";
            return new Dictionary();
        }

        Json parser = new();
        Error parseError = parser.Parse(json);
        if (parseError != Error.Ok)
        {
            errorMessage = "Invalid JSON";
            return new Dictionary();
        }

        if (parser.Data.VariantType != Variant.Type.Dictionary)
        {
            errorMessage = "Invalid file format";
            return new Dictionary();
        }

        errorMessage = string.Empty;
        return (Dictionary)parser.Data;
    }

    /// <summary>
    /// Merges source keys into a destination dictionary.
    /// </summary>
    private static void MergeInto(Dictionary destination, Dictionary source)
    {
        foreach (Variant key in source.Keys)
        {
            destination[key] = source[key];
        }
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

    /// <summary>
    /// Reads an integer value from a payload.
    /// </summary>
    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Int)
        {
            return (int)value;
        }

        if (value.VariantType == Variant.Type.Float)
        {
            return (int)(double)value;
        }

        return fallback;
    }

    /// <summary>
    /// Reads a 64-bit integer value from a payload.
    /// </summary>
    private static long GetLong(Dictionary data, string key, long fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.Int)
        {
            return (long)(int)value;
        }

        if (value.VariantType == Variant.Type.Float)
        {
            return (long)(double)value;
        }

        return fallback;
    }

    /// <summary>
    /// Reads a string value from a payload.
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
    /// Returns whether the extension is one of the supported compressed body formats.
    /// </summary>
    private static bool IsKnownBinaryExtension(string extension)
    {
        return extension == LegacyBinaryExtension
            || extension == AsteroidBinaryExtension
            || extension == WorldBinaryExtension
            || extension == StarBinaryExtension;
    }
}
