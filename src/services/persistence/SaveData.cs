using System.IO;
using System.Text;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Services.Persistence;

/// <summary>
/// Efficient celestial-body save/load service that prefers regeneration payloads.
/// </summary>
public static class SaveData
{
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

    private const string BinaryNotYetPortedMessage =
        "Compressed .sgb format is not yet ported in C#; use JSON or the GDScript path for now.";

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

        Dictionary data = CreateSaveData(body, mode);
        return compress ? SaveCompressed(path, data) : SaveJson(path, data);
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
    /// Loads a celestial body from a save file.
    /// </summary>
    public static SaveDataLoadResult LoadBody(string path)
    {
        Dictionary data;
        string errorMessage;
        string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();

        if (extension == "sgb")
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
            return SaveDataLoadResult.CreateError(
                $"Save file version {version} is newer than supported version {SaveVersion}");
        }

        SaveMode saveMode = (SaveMode)GetInt(data, "save_mode", (int)SaveMode.Compact);
        CelestialBody? body = saveMode switch
        {
            SaveMode.Minimal => RegenerateBody(data),
            SaveMode.Compact => RegenerateBody(data),
            SaveMode.Full => DeserializeBody(data),
            _ => RegenerateBody(data),
        };

        if (body == null)
        {
            return SaveDataLoadResult.CreateError("Failed to reconstruct body from save data");
        }

        return SaveDataLoadResult.CreateSuccess(body);
    }

    /// <summary>
    /// Returns the size of a save file in bytes.
    /// </summary>
    public static long GetFileSize(string path)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);
        return File.Exists(globalPath) ? new FileInfo(globalPath).Length : 0L;
    }

    /// <summary>
    /// Formats a file size for UI display.
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

        return $"{(bytes / (1024.0 * 1024.0)):0.00} MB";
    }

    /// <summary>
    /// Builds the persisted payload for a body.
    /// </summary>
    private static Dictionary CreateSaveData(CelestialBody body, SaveMode mode)
    {
        Dictionary data = new()
        {
            ["version"] = SaveVersion,
            ["save_mode"] = (int)mode,
            ["timestamp"] = (long)Time.GetUnixTimeFromSystem(),
        };

        Dictionary modeData = mode switch
        {
            SaveMode.Minimal => CreateMinimalData(body),
            SaveMode.Compact => CreateCompactData(body),
            SaveMode.Full => CreateFullData(body),
            _ => CreateCompactData(body),
        };

        MergeInto(data, modeData);
        return data;
    }

    /// <summary>
    /// Builds the minimal regeneration payload.
    /// </summary>
    private static Dictionary CreateMinimalData(CelestialBody body)
    {
        Dictionary data = new()
        {
            ["id"] = body.Id,
            ["type"] = CelestialType.TypeToString(body.Type),
        };

        if (body.Provenance != null)
        {
            data["seed"] = body.Provenance.GenerationSeed;
        }

        return data;
    }

    /// <summary>
    /// Builds the compact regeneration payload.
    /// </summary>
    private static Dictionary CreateCompactData(CelestialBody body)
    {
        Dictionary data = CreateMinimalData(body);
        if (body.Provenance != null)
        {
            Dictionary specSnapshot = DuplicateDictionary(body.Provenance.SpecSnapshot);
            if (body.Type != CelestialType.Type.Star && specSnapshot.ContainsKey("context"))
            {
                data["context"] = specSnapshot["context"];
                specSnapshot.Remove("context");
            }

            data["spec"] = specSnapshot;
            data["generator_version"] = body.Provenance.GeneratorVersion;

            if (body.Type != CelestialType.Type.Star && !data.ContainsKey("context"))
            {
                data["context"] = GetDefaultContext(body.Type).ToDictionary();
            }
        }

        if (body.HasMeta("user_modifications"))
        {
            Variant modifications = body.GetMeta("user_modifications");
            if (modifications.VariantType == Variant.Type.Dictionary)
            {
                data["modifications"] = modifications;
            }
        }

        return data;
    }

    /// <summary>
    /// Builds the full-serialization payload.
    /// </summary>
    private static Dictionary CreateFullData(CelestialBody body)
    {
        return new Dictionary
        {
            ["id"] = body.Id,
            ["type"] = CelestialType.TypeToString(body.Type),
            ["body"] = CelestialSerializer.ToDictionary(body),
        };
    }

    /// <summary>
    /// Reconstructs a body from a compact or minimal payload.
    /// </summary>
    private static CelestialBody? RegenerateBody(Dictionary data)
    {
        string typeName = GetString(data, "type", "planet");
        if (!CelestialType.TryParse(typeName, out CelestialType.Type bodyType))
        {
            bodyType = CelestialType.Type.Planet;
        }

        long seedValue = GetLong(data, "seed", 0L);
        Dictionary specData = GetDictionary(data, "spec");
        Dictionary contextData = GetDictionary(data, "context");
        if (contextData.Count == 0 && specData.ContainsKey("context"))
        {
            Variant embeddedContext = specData["context"];
            if (embeddedContext.VariantType == Variant.Type.Dictionary)
            {
                contextData = (Dictionary)embeddedContext;
            }

            specData.Remove("context");
        }

        SeededRng rng = new(seedValue);
        CelestialBody? body = bodyType switch
        {
            CelestialType.Type.Star => GenerateStar(specData, seedValue, rng),
            CelestialType.Type.Planet => GeneratePlanet(specData, contextData, seedValue, rng),
            CelestialType.Type.Moon => GenerateMoon(specData, contextData, seedValue, rng),
            CelestialType.Type.Asteroid => GenerateAsteroid(specData, contextData, seedValue, rng),
            _ => null,
        };

        if (body == null)
        {
            return null;
        }

        if (data.ContainsKey("name"))
        {
            string customName = GetString(data, "name", string.Empty);
            if (!string.IsNullOrEmpty(customName))
            {
                body.Name = customName;
            }
        }

        if (data.ContainsKey("modifications") && data["modifications"].VariantType == Variant.Type.Dictionary)
        {
            ApplyModifications(body, (Dictionary)data["modifications"]);
        }

        return body;
    }

    /// <summary>
    /// Reconstructs a fully serialized body.
    /// </summary>
    private static CelestialBody? DeserializeBody(Dictionary data)
    {
        if (!data.ContainsKey("body") || data["body"].VariantType != Variant.Type.Dictionary)
        {
            return null;
        }

        return CelestialSerializer.FromDictionary((Dictionary)data["body"]);
    }

    /// <summary>
    /// Generates a star from a persisted payload.
    /// </summary>
    private static CelestialBody GenerateStar(Dictionary specData, long seedValue, SeededRng rng)
    {
        int generationSeed = ClampSeedToInt(seedValue);
        StarSpec spec = specData.Count == 0 ? StarSpec.Random(generationSeed) : StarSpec.FromDictionary(specData);
        return StarGenerator.Generate(spec, rng);
    }

    /// <summary>
    /// Generates a planet from a persisted payload.
    /// </summary>
    private static CelestialBody GeneratePlanet(
        Dictionary specData,
        Dictionary contextData,
        long seedValue,
        SeededRng rng)
    {
        int generationSeed = ClampSeedToInt(seedValue);
        PlanetSpec spec = specData.Count == 0 ? PlanetSpec.Random(generationSeed) : PlanetSpec.FromDictionary(specData);
        ParentContext context = ReconstructContext(contextData, CelestialType.Type.Planet);
        return PlanetGenerator.Generate(spec, context, rng);
    }

    /// <summary>
    /// Generates a moon from a persisted payload.
    /// </summary>
    private static CelestialBody? GenerateMoon(
        Dictionary specData,
        Dictionary contextData,
        long seedValue,
        SeededRng rng)
    {
        int generationSeed = ClampSeedToInt(seedValue);
        MoonSpec spec = specData.Count == 0 ? MoonSpec.Random(generationSeed) : MoonSpec.FromDictionary(specData);
        ParentContext context = ReconstructContext(contextData, CelestialType.Type.Moon);
        return MoonGenerator.Generate(spec, context, rng);
    }

    /// <summary>
    /// Generates an asteroid from a persisted payload.
    /// </summary>
    private static CelestialBody GenerateAsteroid(
        Dictionary specData,
        Dictionary contextData,
        long seedValue,
        SeededRng rng)
    {
        int generationSeed = ClampSeedToInt(seedValue);
        AsteroidSpec spec = specData.Count == 0 ? AsteroidSpec.Random(generationSeed) : AsteroidSpec.FromDictionary(specData);
        ParentContext context = ReconstructContext(contextData, CelestialType.Type.Asteroid);
        return AsteroidGenerator.Generate(spec, context, rng);
    }

    /// <summary>
    /// Reconstructs the stored parent context or uses the default for the body type.
    /// </summary>
    private static ParentContext ReconstructContext(Dictionary contextData, CelestialType.Type bodyType)
    {
        return contextData.Count > 0 ? ParentContext.FromDictionary(contextData) : GetDefaultContext(bodyType);
    }

    /// <summary>
    /// Returns the default context for bodies saved without explicit parent context.
    /// </summary>
    private static ParentContext GetDefaultContext(CelestialType.Type bodyType)
    {
        return bodyType switch
        {
            CelestialType.Type.Planet => ParentContext.SunLike(),
            CelestialType.Type.Moon => ParentContext.ForMoon(
                Units.SolarMassKg,
                StellarProps.SolarLuminosityWatts,
                5778.0,
                4.6e9,
                5.2 * Units.AuMeters,
                1.898e27,
                6.9911e7,
                5.0e8),
            CelestialType.Type.Asteroid => ParentContext.SunLike(2.7 * Units.AuMeters),
            _ => ParentContext.SunLike(),
        };
    }

    /// <summary>
    /// Applies user modifications to a regenerated body.
    /// </summary>
    private static void ApplyModifications(CelestialBody body, Dictionary modifications)
    {
        body.SetMeta("user_modifications", modifications);
    }

    /// <summary>
    /// Preserves the existing binary save contract without silently changing the file format.
    /// </summary>
    private static Error SaveCompressed(string path, Dictionary data)
    {
        GD.PushError(BinaryNotYetPortedMessage);
        return Error.Failed;
    }

    /// <summary>
    /// Saves an uncompressed JSON payload.
    /// </summary>
    private static Error SaveJson(string path, Dictionary data)
    {
        string savePath = path.EndsWith(".json") ? path : $"{Path.ChangeExtension(path, null)}.json";
        string globalPath = ProjectSettings.GlobalizePath(savePath);
        EnsureDirectoryExists(globalPath);
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
        errorMessage = File.Exists(globalPath)
            ? BinaryNotYetPortedMessage
            : $"Could not open file: {path}";
        if (File.Exists(globalPath))
        {
            GD.PushError(BinaryNotYetPortedMessage);
        }

        return new Dictionary();
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

        Variant parsed = Json.ParseString(json);
        if (parsed.VariantType != Variant.Type.Dictionary)
        {
            errorMessage = "Expected JSON object at root";
            return new Dictionary();
        }

        errorMessage = string.Empty;
        return (Dictionary)parsed;
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
    /// Clamps a stored seed to the current C# spec seed range.
    /// </summary>
    private static int ClampSeedToInt(long seedValue)
    {
        if (seedValue < int.MinValue)
        {
            return int.MinValue;
        }

        if (seedValue > int.MaxValue)
        {
            return int.MaxValue;
        }

        return (int)seedValue;
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
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.Float => (int)(double)value,
            _ => fallback,
        };
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
        return value.VariantType switch
        {
            Variant.Type.Int => (long)(int)value,
            Variant.Type.Float => (long)(double)value,
            _ => fallback,
        };
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
        return value.VariantType == Variant.Type.String ? (string)value : fallback;
    }

    /// <summary>
    /// Reads a dictionary payload value or returns an empty dictionary.
    /// </summary>
    private static Dictionary GetDictionary(Dictionary data, string key)
    {
        return data.ContainsKey(key) && data[key].VariantType == Variant.Type.Dictionary
            ? DuplicateDictionary((Dictionary)data[key])
            : new Dictionary();
    }
}
