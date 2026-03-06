#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for Provenance tracking.
/// </summary>
public static class TestProvenance
{
    /// <summary>
    /// Tests creation with default values.
    /// </summary>
    public static void TestDefaultValues()
    {
        Provenance prov = new Provenance();
        if (prov.GenerationSeed != 0)
        {
            throw new InvalidOperationException($"Expected generation_seed 0, got {prov.GenerationSeed}");
        }
        if (prov.GeneratorVersion != "")
        {
            throw new InvalidOperationException($"Expected empty generator_version, got '{prov.GeneratorVersion}'");
        }
        if (prov.SchemaVersion != 0)
        {
            throw new InvalidOperationException($"Expected schema_version 0, got {prov.SchemaVersion}");
        }
        if (prov.CreatedTimestamp != 0)
        {
            throw new InvalidOperationException($"Expected created_timestamp 0, got {prov.CreatedTimestamp}");
        }
        if (prov.SpecSnapshot.Count != 0)
        {
            throw new InvalidOperationException($"Expected empty spec_snapshot, got {prov.SpecSnapshot.Count} entries");
        }
    }

    /// <summary>
    /// Tests creation with specific values.
    /// </summary>
    public static void TestInitialization()
    {
        Godot.Collections.Dictionary spec = new Godot.Collections.Dictionary { ["preset"] = "earth-like" };
        Provenance prov = new Provenance(12345, "0.1.0", 1, 1700000000, spec);
        if (prov.GenerationSeed != 12345)
        {
            throw new InvalidOperationException($"Expected generation_seed 12345, got {prov.GenerationSeed}");
        }
        if (prov.GeneratorVersion != "0.1.0")
        {
            throw new InvalidOperationException($"Expected generator_version '0.1.0', got '{prov.GeneratorVersion}'");
        }
        if (prov.SchemaVersion != 1)
        {
            throw new InvalidOperationException($"Expected schema_version 1, got {prov.SchemaVersion}");
        }
        if (prov.CreatedTimestamp != 1700000000)
        {
            throw new InvalidOperationException($"Expected created_timestamp 1700000000, got {prov.CreatedTimestamp}");
        }
        if (prov.SpecSnapshot["preset"].AsString() != "earth-like")
        {
            throw new InvalidOperationException($"Expected preset 'earth-like', got '{prov.SpecSnapshot["preset"]}'");
        }
    }

    /// <summary>
    /// Tests create_current uses current version constants.
    /// </summary>
    public static void TestCreateCurrent()
    {
        Godot.Collections.Dictionary spec = new Godot.Collections.Dictionary { ["test"] = "value" };
        Provenance prov = Provenance.CreateCurrent(99999, spec);

        if (prov.GenerationSeed != 99999)
        {
            throw new InvalidOperationException($"Expected generation_seed 99999, got {prov.GenerationSeed}");
        }
        if (prov.GeneratorVersion != Versions.GeneratorVersion)
        {
            throw new InvalidOperationException($"Expected generator_version '{Versions.GeneratorVersion}', got '{prov.GeneratorVersion}'");
        }
        if (prov.SchemaVersion != Versions.SchemaVersion)
        {
            throw new InvalidOperationException($"Expected schema_version {Versions.SchemaVersion}, got {prov.SchemaVersion}");
        }
        if (prov.CreatedTimestamp <= 0)
        {
            throw new InvalidOperationException($"Expected positive created_timestamp, got {prov.CreatedTimestamp}");
        }
        if (prov.SpecSnapshot["test"].AsString() != "value")
        {
            throw new InvalidOperationException($"Expected test 'value', got '{prov.SpecSnapshot["test"]}'");
        }
    }

    /// <summary>
    /// Tests round-trip serialization.
    /// </summary>
    public static void TestRoundTrip()
    {
        Godot.Collections.Dictionary spec = new Godot.Collections.Dictionary { ["preset"] = "mars", ["size"] = "medium" };
        Provenance original = new Provenance(54321, "0.2.0", 2, 1700000000, spec);
        Godot.Collections.Dictionary data = original.ToDictionary();
        Provenance restored = Provenance.FromDictionary(data);

        if (restored.GenerationSeed != original.GenerationSeed)
        {
            throw new InvalidOperationException($"Expected generation_seed {original.GenerationSeed}, got {restored.GenerationSeed}");
        }
        if (restored.GeneratorVersion != original.GeneratorVersion)
        {
            throw new InvalidOperationException($"Expected generator_version '{original.GeneratorVersion}', got '{restored.GeneratorVersion}'");
        }
        if (restored.SchemaVersion != original.SchemaVersion)
        {
            throw new InvalidOperationException($"Expected schema_version {original.SchemaVersion}, got {restored.SchemaVersion}");
        }
        if (restored.CreatedTimestamp != original.CreatedTimestamp)
        {
            throw new InvalidOperationException($"Expected created_timestamp {original.CreatedTimestamp}, got {restored.CreatedTimestamp}");
        }
        if (restored.SpecSnapshot["preset"].AsString() != "mars")
        {
            throw new InvalidOperationException($"Expected preset 'mars', got '{restored.SpecSnapshot["preset"]}'");
        }
        if (restored.SpecSnapshot["size"].AsString() != "medium")
        {
            throw new InvalidOperationException($"Expected size 'medium', got '{restored.SpecSnapshot["size"]}'");
        }
    }

    /// <summary>
    /// Tests from_dict handles empty dictionary.
    /// </summary>
    public static void TestFromDictEmpty()
    {
        Provenance prov = Provenance.FromDictionary(new Dictionary());
        if (prov != null)
        {
            throw new InvalidOperationException("Expected null from empty dictionary");
        }
    }

    /// <summary>
    /// Tests backward compatibility with old "seed" field name.
    /// </summary>
    public static void TestBackwardCompatibilitySeedField()
    {
        Godot.Collections.Dictionary oldData = new Godot.Collections.Dictionary
        {
            ["seed"] = 12345,
            ["generator_version"] = "0.0.1",
            ["schema_version"] = 0,
            ["created_timestamp"] = 1600000000,
            ["spec_snapshot"] = new Godot.Collections.Dictionary()
        };

        Provenance prov = Provenance.FromDictionary(oldData);
        if (prov.GenerationSeed != 0)
        {
            throw new InvalidOperationException($"Expected generation_seed 0 (fallback), got {prov.GenerationSeed}");
        }
    }
}
