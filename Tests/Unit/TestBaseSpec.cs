#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for BaseSpec and override functionality.
/// </summary>
public static class TestBaseSpec
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests creation with default values.
    /// </summary>
    public static void TestDefaultValues()
    {
        BaseSpec spec = new BaseSpec();
        if (spec.GenerationSeed != 0)
        {
            throw new InvalidOperationException($"Expected generation_seed 0, got {spec.GenerationSeed}");
        }
        if (spec.NameHint != "")
        {
            throw new InvalidOperationException($"Expected empty name_hint, got '{spec.NameHint}'");
        }
        if (spec.Overrides.Count != 0)
        {
            throw new InvalidOperationException($"Expected 0 overrides, got {spec.Overrides.Count}");
        }
    }

    /// <summary>
    /// Tests creation with all parameters.
    /// </summary>
    public static void TestInitialization()
    {
        Godot.Collections.Dictionary overrides = new Godot.Collections.Dictionary { ["physical.mass_kg"] = 5.0e24 };
        BaseSpec spec = new BaseSpec(12345, "Test Body", overrides);

        if (spec.GenerationSeed != 12345)
        {
            throw new InvalidOperationException($"Expected generation_seed 12345, got {spec.GenerationSeed}");
        }
        if (spec.NameHint != "Test Body")
        {
            throw new InvalidOperationException($"Expected name_hint 'Test Body', got '{spec.NameHint}'");
        }
        if (!spec.HasOverride("physical.mass_kg"))
        {
            throw new InvalidOperationException("Expected override for 'physical.mass_kg'");
        }
    }

    /// <summary>
    /// Tests override methods.
    /// </summary>
    public static void TestOverrides()
    {
        BaseSpec spec = new BaseSpec();

        if (spec.HasOverride("test.field"))
        {
            throw new InvalidOperationException("Should not have override initially");
        }
        if (spec.GetOverride("test.field", 100.0).AsDouble() != 100.0)
        {
            throw new InvalidOperationException($"Expected default 100.0, got {spec.GetOverride("test.field", 100.0)}");
        }

        spec.SetOverride("test.field", 50.0);
        if (!spec.HasOverride("test.field"))
        {
            throw new InvalidOperationException("Should have override after set");
        }
        if (spec.GetOverride("test.field", 100.0).AsDouble() != 50.0)
        {
            throw new InvalidOperationException($"Expected override 50.0, got {spec.GetOverride("test.field", 100.0)}");
        }

        spec.RemoveOverride("test.field");
        if (spec.HasOverride("test.field"))
        {
            throw new InvalidOperationException("Should not have override after remove");
        }
    }

    /// <summary>
    /// Tests typed override getters.
    /// </summary>
    public static void TestTypedOverrides()
    {
        BaseSpec spec = new BaseSpec();
        spec.SetOverride("float_field", 3.14);
        spec.SetOverride("int_field", 42);

        if (System.Math.Abs(spec.GetOverrideFloat("float_field", 0.0) - 3.14) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected float_field 3.14, got {spec.GetOverrideFloat("float_field", 0.0)}");
        }
        if (spec.GetOverrideInt("int_field", 0) != 42)
        {
            throw new InvalidOperationException($"Expected int_field 42, got {spec.GetOverrideInt("int_field", 0)}");
        }

        if (System.Math.Abs(spec.GetOverrideFloat("missing", 99.9) - 99.9) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected default 99.9, got {spec.GetOverrideFloat("missing", 99.9)}");
        }
        if (spec.GetOverrideInt("missing", 99) != 99)
        {
            throw new InvalidOperationException($"Expected default 99, got {spec.GetOverrideInt("missing", 99)}");
        }
    }

    /// <summary>
    /// Tests clear_overrides.
    /// </summary>
    public static void TestClearOverrides()
    {
        BaseSpec spec = new BaseSpec();
        spec.SetOverride("field1", 1);
        spec.SetOverride("field2", 2);

        if (spec.Overrides.Count != 2)
        {
            throw new InvalidOperationException($"Expected 2 overrides, got {spec.Overrides.Count}");
        }
        spec.ClearOverrides();
        if (spec.Overrides.Count != 0)
        {
            throw new InvalidOperationException($"Expected 0 overrides after clear, got {spec.Overrides.Count}");
        }
    }

    /// <summary>
    /// Tests StarSpec presets.
    /// </summary>
    public static void TestStarSpecPresets()
    {
        StarSpec sun = StarSpec.SunLike(100);
        if (sun.SpectralClass != (int)StarClass.SpectralClass.G)
        {
            throw new InvalidOperationException($"Expected spectral_class G, got {sun.SpectralClass}");
        }
        if (sun.Subclass != 2)
        {
            throw new InvalidOperationException($"Expected subclass 2, got {sun.Subclass}");
        }

        StarSpec red = StarSpec.RedDwarf(200);
        if (red.SpectralClass != (int)StarClass.SpectralClass.M)
        {
            throw new InvalidOperationException($"Expected spectral_class M, got {red.SpectralClass}");
        }
    }

    /// <summary>
    /// Tests PlanetSpec presets.
    /// </summary>
    public static void TestPlanetSpecPresets()
    {
        PlanetSpec earth = PlanetSpec.EarthLike(100);
        if (earth.SizeCategory != (int)SizeCategory.Category.Terrestrial)
        {
            throw new InvalidOperationException($"Expected size_category Terrestrial, got {earth.SizeCategory}");
        }
        if (earth.OrbitZone != (int)OrbitZone.Zone.Temperate)
        {
            throw new InvalidOperationException($"Expected orbit_zone Temperate, got {earth.OrbitZone}");
        }

        PlanetSpec jupiter = PlanetSpec.HotJupiter(200);
        if (jupiter.SizeCategory != (int)SizeCategory.Category.GasGiant)
        {
            throw new InvalidOperationException($"Expected size_category GasGiant, got {jupiter.SizeCategory}");
        }
        if (jupiter.OrbitZone != (int)OrbitZone.Zone.Hot)
        {
            throw new InvalidOperationException($"Expected orbit_zone Hot, got {jupiter.OrbitZone}");
        }
    }

    /// <summary>
    /// Tests MoonSpec presets.
    /// </summary>
    public static void TestMoonSpecPresets()
    {
        MoonSpec europa = MoonSpec.EuropaLike(100);
        if (!europa.HasSubsurfaceOcean.AsBool())
        {
            throw new InvalidOperationException("Expected has_subsurface_ocean true");
        }
        if (europa.IsCaptured)
        {
            throw new InvalidOperationException("Expected is_captured false");
        }

        MoonSpec captured = MoonSpec.Captured(200);
        if (!captured.IsCaptured)
        {
            throw new InvalidOperationException("Expected is_captured true");
        }
    }

    /// <summary>
    /// Tests AsteroidSpec presets.
    /// </summary>
    public static void TestAsteroidSpecPresets()
    {
        AsteroidSpec cType = AsteroidSpec.Carbonaceous(100);
        if (cType.AsteroidType != (int)AsteroidType.Type.CType)
        {
            throw new InvalidOperationException($"Expected asteroid_type C_TYPE, got {cType.AsteroidType}");
        }

        AsteroidSpec ceres = AsteroidSpec.CeresLike(200);
        if (!ceres.IsLarge)
        {
            throw new InvalidOperationException("Expected is_large true");
        }
    }

    /// <summary>
    /// Tests spec serialization round-trip.
    /// </summary>
    public static void TestSpecSerialization()
    {
        StarSpec original = new StarSpec(12345, StarClass.SpectralClass.G, 2, 1.0, 4.6e9, "Sol", new Godot.Collections.Dictionary { ["mass"] = 1.0 });
        Godot.Collections.Dictionary data = original.ToDictionary();
        StarSpec restored = StarSpec.FromDictionary(data);

        if (restored.GenerationSeed != original.GenerationSeed)
        {
            throw new InvalidOperationException($"Expected generation_seed {original.GenerationSeed}, got {restored.GenerationSeed}");
        }
        if (restored.SpectralClass != original.SpectralClass)
        {
            throw new InvalidOperationException($"Expected spectral_class {original.SpectralClass}, got {restored.SpectralClass}");
        }
        if (restored.Subclass != original.Subclass)
        {
            throw new InvalidOperationException($"Expected subclass {original.Subclass}, got {restored.Subclass}");
        }
        if (restored.NameHint != original.NameHint)
        {
            throw new InvalidOperationException($"Expected name_hint '{original.NameHint}', got '{restored.NameHint}'");
        }
        if (!restored.HasOverride("mass"))
        {
            throw new InvalidOperationException("Expected override for 'mass'");
        }
    }
}
