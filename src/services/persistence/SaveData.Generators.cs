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
/// Regeneration, generation, and payload-construction helpers for SaveData.
/// </summary>
public static partial class SaveData
{
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

        Dictionary modeData;
        if (mode == SaveMode.Minimal)
        {
            modeData = CreateMinimalData(body);
        }
        else if (mode == SaveMode.Full)
        {
            modeData = CreateFullData(body);
        }
        else
        {
            modeData = CreateCompactData(body);
        }

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
        CelestialBody? body = null;
        if (bodyType == CelestialType.Type.Star)
        {
            body = GenerateStar(specData, seedValue, rng);
        }
        else if (bodyType == CelestialType.Type.Planet)
        {
            body = GeneratePlanet(specData, contextData, seedValue, rng);
        }
        else if (bodyType == CelestialType.Type.Moon)
        {
            body = GenerateMoon(specData, contextData, seedValue, rng);
        }
        else if (bodyType == CelestialType.Type.Asteroid)
        {
            body = GenerateAsteroid(specData, contextData, seedValue, rng);
        }

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
        StarSpec spec;
        if (specData.Count == 0)
        {
            spec = StarSpec.Random(generationSeed);
        }
        else
        {
            spec = StarSpec.FromDictionary(specData);
            if (spec.GenerationSeed == 0)
            {
                spec.GenerationSeed = generationSeed;
            }
        }
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
        PlanetSpec spec;
        if (specData.Count == 0)
        {
            spec = PlanetSpec.Random(generationSeed);
        }
        else
        {
            spec = PlanetSpec.FromDictionary(specData);
            if (spec.GenerationSeed == 0)
            {
                spec.GenerationSeed = generationSeed;
            }
        }
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
        MoonSpec spec;
        if (specData.Count == 0)
        {
            spec = MoonSpec.Random(generationSeed);
        }
        else
        {
            spec = MoonSpec.FromDictionary(specData);
            if (spec.GenerationSeed == 0)
            {
                spec.GenerationSeed = generationSeed;
            }
        }
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
        AsteroidSpec spec;
        if (specData.Count == 0)
        {
            spec = AsteroidSpec.Random(generationSeed);
        }
        else
        {
            spec = AsteroidSpec.FromDictionary(specData);
            if (spec.GenerationSeed == 0)
            {
                spec.GenerationSeed = generationSeed;
            }
        }
        ParentContext context = ReconstructContext(contextData, CelestialType.Type.Asteroid);
        return AsteroidGenerator.Generate(spec, context, rng);
    }

    /// <summary>
    /// Reconstructs the stored parent context or uses the default for the body type.
    /// </summary>
    private static ParentContext ReconstructContext(Dictionary contextData, CelestialType.Type bodyType)
    {
        if (contextData.Count > 0)
        {
            return ParentContext.FromDictionary(contextData);
        }

        return GetDefaultContext(bodyType);
    }

    /// <summary>
    /// Returns the default context for bodies saved without explicit parent context.
    /// </summary>
    private static ParentContext GetDefaultContext(CelestialType.Type bodyType)
    {
        if (bodyType == CelestialType.Type.Moon)
        {
            return ParentContext.ForMoon(
                Units.SolarMassKg,
                StellarProps.SolarLuminosityWatts,
                5778.0,
                4.6e9,
                5.2 * Units.AuMeters,
                1.898e27,
                6.9911e7,
                5.0e8);
        }

        if (bodyType == CelestialType.Type.Asteroid)
        {
            return ParentContext.SunLike(2.7 * Units.AuMeters);
        }

        return ParentContext.SunLike();
    }

    /// <summary>
    /// Applies user modifications to a regenerated body.
    /// </summary>
    private static void ApplyModifications(CelestialBody body, Dictionary modifications)
    {
        body.SetMeta("user_modifications", modifications);
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
}
