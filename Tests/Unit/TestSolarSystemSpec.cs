#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for SolarSystemSpec.
/// </summary>
public static class TestSolarSystemSpec
{
    private const double DefaultTolerance = 0.1;

    /// <summary>
    /// Tests basic construction.
    /// </summary>
    public static void TestConstruction()
    {
        SolarSystemSpec spec = new SolarSystemSpec(12345, 1, 3);

        if (spec.GenerationSeed != 12345)
        {
            throw new InvalidOperationException("Expected generation_seed 12345");
        }
        if (spec.StarCountMin != 1)
        {
            throw new InvalidOperationException("Expected star_count_min 1");
        }
        if (spec.StarCountMax != 3)
        {
            throw new InvalidOperationException("Expected star_count_max 3");
        }
    }

    /// <summary>
    /// Tests clamping of star count.
    /// </summary>
    public static void TestStarCountClamping()
    {
        SolarSystemSpec spec = new SolarSystemSpec(123, 0, 20);

        if (spec.StarCountMin != 1)
        {
            throw new InvalidOperationException("Min should be clamped to 1");
        }
        if (spec.StarCountMax != 10)
        {
            throw new InvalidOperationException("Max should be clamped to 10");
        }
    }

    /// <summary>
    /// Tests min > max handling.
    /// </summary>
    public static void TestStarCountMinGreaterThanMax()
    {
        SolarSystemSpec spec = new SolarSystemSpec(123, 5, 3);

        if (spec.StarCountMin != 5)
        {
            throw new InvalidOperationException("Expected star_count_min 5");
        }
        if (spec.StarCountMax != 5)
        {
            throw new InvalidOperationException("Max should be at least min");
        }
    }

    /// <summary>
    /// Tests single star preset.
    /// </summary>
    public static void TestSingleStarPreset()
    {
        SolarSystemSpec spec = SolarSystemSpec.SingleStar(999);

        if (spec.GenerationSeed != 999)
        {
            throw new InvalidOperationException("Expected generation_seed 999");
        }
        if (spec.StarCountMin != 1)
        {
            throw new InvalidOperationException("Expected star_count_min 1");
        }
        if (spec.StarCountMax != 1)
        {
            throw new InvalidOperationException("Expected star_count_max 1");
        }
    }

    /// <summary>
    /// Tests binary preset.
    /// </summary>
    public static void TestBinaryPreset()
    {
        SolarSystemSpec spec = SolarSystemSpec.Binary(888);

        if (spec.StarCountMin != 2)
        {
            throw new InvalidOperationException("Expected star_count_min 2");
        }
        if (spec.StarCountMax != 2)
        {
            throw new InvalidOperationException("Expected star_count_max 2");
        }
    }

    /// <summary>
    /// Tests random small preset.
    /// </summary>
    public static void TestRandomSmallPreset()
    {
        SolarSystemSpec spec = SolarSystemSpec.RandomSmall(777);

        if (spec.StarCountMin != 1)
        {
            throw new InvalidOperationException("Expected star_count_min 1");
        }
        if (spec.StarCountMax != 3)
        {
            throw new InvalidOperationException("Expected star_count_max 3");
        }
    }

    /// <summary>
    /// Tests random preset.
    /// </summary>
    public static void TestRandomPreset()
    {
        SolarSystemSpec spec = SolarSystemSpec.Random(666);

        if (spec.StarCountMin != 1)
        {
            throw new InvalidOperationException("Expected star_count_min 1");
        }
        if (spec.StarCountMax != 10)
        {
            throw new InvalidOperationException("Expected star_count_max 10");
        }
    }

    /// <summary>
    /// Tests sun like preset.
    /// </summary>
    public static void TestSunLikePreset()
    {
        SolarSystemSpec spec = SolarSystemSpec.SunLike(555);

        if (spec.StarCountMin != 1)
        {
            throw new InvalidOperationException("Expected star_count_min 1");
        }
        if (spec.StarCountMax != 1)
        {
            throw new InvalidOperationException("Expected star_count_max 1");
        }
        if (spec.SpectralClassHints.Count != 1)
        {
            throw new InvalidOperationException("Expected 1 spectral class hint");
        }
        if (spec.SpectralClassHints[0] != (int)StarClass.SpectralClass.G)
        {
            throw new InvalidOperationException("Expected G class hint");
        }
    }

    /// <summary>
    /// Tests alpha centauri like preset.
    /// </summary>
    public static void TestAlphaCentauriLikePreset()
    {
        SolarSystemSpec spec = SolarSystemSpec.AlphaCentauriLike(444);

        if (spec.StarCountMin != 3)
        {
            throw new InvalidOperationException("Expected star_count_min 3");
        }
        if (spec.StarCountMax != 3)
        {
            throw new InvalidOperationException("Expected star_count_max 3");
        }
        if (spec.SpectralClassHints.Count != 3)
        {
            throw new InvalidOperationException("Expected 3 spectral class hints");
        }
    }

    /// <summary>
    /// Tests override functionality.
    /// </summary>
    public static void TestOverrides()
    {
        SolarSystemSpec spec = new SolarSystemSpec(123, 1, 1);

        if (spec.HasOverride("star_count"))
        {
            throw new InvalidOperationException("Should not have override initially");
        }

        spec.SetOverride("star_count", 2);

        if (!spec.HasOverride("star_count"))
        {
            throw new InvalidOperationException("Should have override after setting");
        }
        if (spec.GetOverride("star_count", 1).AsInt32() != 2)
        {
            throw new InvalidOperationException("Override value should be 2");
        }
    }

    /// <summary>
    /// Tests system age and metallicity defaults.
    /// </summary>
    public static void TestSystemAgeMetallicityDefaults()
    {
        SolarSystemSpec spec = new SolarSystemSpec(123, 1, 1);

        if (System.Math.Abs(spec.SystemAgeYears - (-1.0)) > DefaultTolerance)
        {
            throw new InvalidOperationException("Default age should be -1 (random)");
        }
        if (System.Math.Abs(spec.SystemMetallicity - (-1.0)) > DefaultTolerance)
        {
            throw new InvalidOperationException("Default metallicity should be -1 (random)");
        }
    }

    /// <summary>
    /// Tests system age and metallicity can be set.
    /// </summary>
    public static void TestSystemAgeMetallicitySetting()
    {
        SolarSystemSpec spec = new SolarSystemSpec(123, 1, 1);
        spec.SystemAgeYears = 5.0e9;
        spec.SystemMetallicity = 1.2;

        if (System.Math.Abs(spec.SystemAgeYears - 5.0e9) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected age 5.0e9, got {spec.SystemAgeYears}");
        }
        if (System.Math.Abs(spec.SystemMetallicity - 1.2) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected metallicity 1.2, got {spec.SystemMetallicity}");
        }
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestRoundTrip()
    {
        SolarSystemSpec original = new SolarSystemSpec(12345, 2, 5);
        original.NameHint = "Test System";
        original.SpectralClassHints = new Array<int> { (int)StarClass.SpectralClass.G, (int)StarClass.SpectralClass.K };
        original.SystemAgeYears = 4.5e9;
        original.SystemMetallicity = 0.8;
        original.SetOverride("test", "value");

        Godot.Collections.Dictionary data = original.ToDictionary();
        SolarSystemSpec restored = SolarSystemSpec.FromDictionary(data);

        if (restored.GenerationSeed != original.GenerationSeed)
        {
            throw new InvalidOperationException("Generation seed should match");
        }
        if (restored.NameHint != original.NameHint)
        {
            throw new InvalidOperationException("Name hint should match");
        }
        if (restored.StarCountMin != original.StarCountMin)
        {
            throw new InvalidOperationException("Star count min should match");
        }
        if (restored.StarCountMax != original.StarCountMax)
        {
            throw new InvalidOperationException("Star count max should match");
        }
        if (restored.SpectralClassHints.Count != 2)
        {
            throw new InvalidOperationException("Expected 2 spectral class hints");
        }
        if (System.Math.Abs(restored.SystemAgeYears - original.SystemAgeYears) > DefaultTolerance)
        {
            throw new InvalidOperationException("System age should match");
        }
        if (System.Math.Abs(restored.SystemMetallicity - original.SystemMetallicity) > DefaultTolerance)
        {
            throw new InvalidOperationException("System metallicity should match");
        }
        if (!restored.HasOverride("test"))
        {
            throw new InvalidOperationException("Should have test override");
        }
    }
}
