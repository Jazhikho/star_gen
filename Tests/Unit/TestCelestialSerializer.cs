#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Serialization;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for CelestialSerializer.
/// </summary>
public static class TestCelestialSerializer
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Creates a fully populated celestial body for testing.
    /// </summary>
    private static CelestialBody CreateFullBody()
    {
        PhysicalProps physical = new PhysicalProps(5.972e24, 6.371e6, 86400.0, 23.5);
        Provenance provenance = Provenance.CreateCurrent(12345, new Godot.Collections.Dictionary { ["preset"] = "earth" });

        CelestialBody body = new CelestialBody(
            "earth_001",
            "Earth-like",
            CelestialType.Type.Planet,
            physical,
            provenance
        );

        body.Orbital = new OrbitalProps(1.496e11, 0.017, 0.0, 0.0, 0.0, 0.0, "sun_001");
        body.Surface = new SurfaceProps(288.0, 0.306, "terrestrial");
        body.Atmosphere = new AtmosphereProps(
            101325.0, 8500.0, new Godot.Collections.Dictionary { ["N2"] = 0.78, ["O2"] = 0.21, ["Ar"] = 0.01 }, 1.0
        );

        body.Surface.Terrain = new TerrainProps(20000.0, 0.4, 0.1, 0.7, 0.5, "continental");
        body.Surface.Hydrosphere = new HydrosphereProps(0.71, 3688.0, 0.03, 35.0);
        body.Surface.Cryosphere = new CryosphereProps(0.05, 500.0, false, 0.0, 0.0);

        return body;
    }

    /// <summary>
    /// Tests to_dict produces required fields.
    /// </summary>
    public static void TestToDictHasRequiredFields()
    {
        CelestialBody body = CreateFullBody();
        Godot.Collections.Dictionary data = CelestialSerializer.ToDict(body);

        if (!data.ContainsKey("schema_version"))
        {
            throw new InvalidOperationException("Expected schema_version field");
        }
        if (!data.ContainsKey("id"))
        {
            throw new InvalidOperationException("Expected id field");
        }
        if (!data.ContainsKey("name"))
        {
            throw new InvalidOperationException("Expected name field");
        }
        if (!data.ContainsKey("type"))
        {
            throw new InvalidOperationException("Expected type field");
        }
        if (!data.ContainsKey("physical"))
        {
            throw new InvalidOperationException("Expected physical field");
        }
        if (!data.ContainsKey("provenance"))
        {
            throw new InvalidOperationException("Expected provenance field");
        }
    }

    /// <summary>
    /// Tests to_dict includes optional components.
    /// </summary>
    public static void TestToDictIncludesOptionalComponents()
    {
        CelestialBody body = CreateFullBody();
        Godot.Collections.Dictionary data = CelestialSerializer.ToDict(body);

        if (!data.ContainsKey("orbital"))
        {
            throw new InvalidOperationException("Expected orbital field");
        }
        if (!data.ContainsKey("surface"))
        {
            throw new InvalidOperationException("Expected surface field");
        }
        if (!data.ContainsKey("atmosphere"))
        {
            throw new InvalidOperationException("Expected atmosphere field");
        }
    }

    /// <summary>
    /// Tests to_dict excludes null components.
    /// </summary>
    public static void TestToDictExcludesNullComponents()
    {
        PhysicalProps physical = new PhysicalProps(1.0e24, 5.0e6);
        CelestialBody body = new CelestialBody("test", "Test", CelestialType.Type.Asteroid, physical);
        Godot.Collections.Dictionary data = CelestialSerializer.ToDict(body);

        if (data.ContainsKey("orbital"))
        {
            throw new InvalidOperationException("Should not have orbital field");
        }
        if (data.ContainsKey("surface"))
        {
            throw new InvalidOperationException("Should not have surface field");
        }
        if (data.ContainsKey("atmosphere"))
        {
            throw new InvalidOperationException("Should not have atmosphere field");
        }
        if (data.ContainsKey("ring_system"))
        {
            throw new InvalidOperationException("Should not have ring_system field");
        }
        if (data.ContainsKey("stellar"))
        {
            throw new InvalidOperationException("Should not have stellar field");
        }
    }

    /// <summary>
    /// Tests from_dict restores identity.
    /// </summary>
    public static void TestFromDictRestoresIdentity()
    {
        CelestialBody body = CreateFullBody();
        Godot.Collections.Dictionary data = CelestialSerializer.ToDict(body);
        CelestialBody restored = CelestialSerializer.FromDictionary(data);

        if (restored.Id != body.Id)
        {
            throw new InvalidOperationException($"Expected id '{body.Id}', got '{restored.Id}'");
        }
        if (restored.Name != body.Name)
        {
            throw new InvalidOperationException($"Expected name '{body.Name}', got '{restored.Name}'");
        }
        if (restored.Type != body.Type)
        {
            throw new InvalidOperationException($"Expected type {body.Type}, got {restored.Type}");
        }
    }

    /// <summary>
    /// Tests from_dict restores physical properties.
    /// </summary>
    public static void TestFromDictRestoresPhysical()
    {
        CelestialBody body = CreateFullBody();
        Godot.Collections.Dictionary data = CelestialSerializer.ToDict(body);
        CelestialBody restored = CelestialSerializer.FromDictionary(data);

        if (System.Math.Abs(restored.Physical.MassKg - body.Physical.MassKg) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected mass {body.Physical.MassKg}, got {restored.Physical.MassKg}");
        }
        if (System.Math.Abs(restored.Physical.RadiusM - body.Physical.RadiusM) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected radius {body.Physical.RadiusM}, got {restored.Physical.RadiusM}");
        }
    }

    /// <summary>
    /// Tests from_dict restores orbital properties.
    /// </summary>
    public static void TestFromDictRestoresOrbital()
    {
        CelestialBody body = CreateFullBody();
        Godot.Collections.Dictionary data = CelestialSerializer.ToDict(body);
        CelestialBody restored = CelestialSerializer.FromDictionary(data);

        if (!restored.HasOrbital())
        {
            throw new InvalidOperationException("Expected orbital properties");
        }
        if (System.Math.Abs(restored.Orbital.SemiMajorAxisM - body.Orbital.SemiMajorAxisM) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected semi_major_axis_m {body.Orbital.SemiMajorAxisM}, got {restored.Orbital.SemiMajorAxisM}");
        }
        if (restored.Orbital.ParentId != body.Orbital.ParentId)
        {
            throw new InvalidOperationException($"Expected parent_id '{body.Orbital.ParentId}', got '{restored.Orbital.ParentId}'");
        }
    }

    /// <summary>
    /// Tests from_dict restores atmosphere.
    /// </summary>
    public static void TestFromDictRestoresAtmosphere()
    {
        CelestialBody body = CreateFullBody();
        Godot.Collections.Dictionary data = CelestialSerializer.ToDict(body);
        CelestialBody restored = CelestialSerializer.FromDictionary(data);

        if (!restored.HasAtmosphere())
        {
            throw new InvalidOperationException("Expected atmosphere");
        }
        if (System.Math.Abs(restored.Atmosphere.SurfacePressurePa - body.Atmosphere.SurfacePressurePa) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected surface_pressure_pa {body.Atmosphere.SurfacePressurePa}, got {restored.Atmosphere.SurfacePressurePa}");
        }
        if (!restored.Atmosphere.Composition.ContainsKey("N2"))
        {
            throw new InvalidOperationException("Expected N2 in composition");
        }
    }

    /// <summary>
    /// Tests from_dict restores surface sub-components.
    /// </summary>
    public static void TestFromDictRestoresSurfaceComponents()
    {
        CelestialBody body = CreateFullBody();
        Godot.Collections.Dictionary data = CelestialSerializer.ToDict(body);
        CelestialBody restored = CelestialSerializer.FromDictionary(data);

        if (!restored.HasSurface())
        {
            throw new InvalidOperationException("Expected surface");
        }
        if (!restored.Surface.HasTerrain())
        {
            throw new InvalidOperationException("Expected terrain");
        }
        if (!restored.Surface.HasHydrosphere())
        {
            throw new InvalidOperationException("Expected hydrosphere");
        }
        if (!restored.Surface.HasCryosphere())
        {
            throw new InvalidOperationException("Expected cryosphere");
        }

        if (System.Math.Abs(restored.Surface.Terrain.ElevationRangeM - 20000.0) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected elevation_range_m 20000.0, got {restored.Surface.Terrain.ElevationRangeM}");
        }
        if (System.Math.Abs(restored.Surface.Hydrosphere.OceanCoverage - 0.71) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected ocean_coverage 0.71, got {restored.Surface.Hydrosphere.OceanCoverage}");
        }
        if (System.Math.Abs(restored.Surface.Cryosphere.PolarCapCoverage - 0.05) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected polar_cap_coverage 0.05, got {restored.Surface.Cryosphere.PolarCapCoverage}");
        }
    }

    /// <summary>
    /// Tests stellar properties serialization for stars.
    /// </summary>
    public static void TestStellarRoundTrip()
    {
        PhysicalProps physical = new PhysicalProps(1.989e30, 6.957e8);
        Provenance provenance = Provenance.CreateCurrent(42);
        CelestialBody body = new CelestialBody(
            "sun_001", "Sun", CelestialType.Type.Star, physical, provenance
        );
        body.Stellar = new StellarProps(3.828e26, 5778.0, "G2V", "main_sequence", 1.0, 4.6e9);

        string jsonString = CelestialSerializer.ToJson(body);
        CelestialBody restored = CelestialSerializer.FromJson(jsonString);

        if (restored == null)
        {
            throw new InvalidOperationException("Expected non-null restored body");
        }
        if (!restored.HasStellar())
        {
            throw new InvalidOperationException("Expected stellar properties");
        }
        if (System.Math.Abs(restored.Stellar.LuminosityWatts - body.Stellar.LuminosityWatts) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected luminosity {body.Stellar.LuminosityWatts}, got {restored.Stellar.LuminosityWatts}");
        }
        if (restored.Stellar.SpectralClass != body.Stellar.SpectralClass)
        {
            throw new InvalidOperationException($"Expected spectral_class '{body.Stellar.SpectralClass}', got '{restored.Stellar.SpectralClass}'");
        }
    }

    /// <summary>
    /// Tests from_dict restores provenance.
    /// </summary>
    public static void TestFromDictRestoresProvenance()
    {
        CelestialBody body = CreateFullBody();
        Godot.Collections.Dictionary data = CelestialSerializer.ToDict(body);
        CelestialBody restored = CelestialSerializer.FromDictionary(data);

        if (restored.Provenance == null)
        {
            throw new InvalidOperationException("Expected non-null provenance");
        }
        if (restored.Provenance.GenerationSeed != body.Provenance.GenerationSeed)
        {
            throw new InvalidOperationException($"Expected generation_seed {body.Provenance.GenerationSeed}, got {restored.Provenance.GenerationSeed}");
        }
        if (restored.Provenance.GeneratorVersion != body.Provenance.GeneratorVersion)
        {
            throw new InvalidOperationException($"Expected generator_version '{body.Provenance.GeneratorVersion}', got '{restored.Provenance.GeneratorVersion}'");
        }
    }

    /// <summary>
    /// Tests JSON round-trip.
    /// </summary>
    public static void TestJsonRoundTrip()
    {
        CelestialBody body = CreateFullBody();
        string jsonString = CelestialSerializer.ToJson(body);
        CelestialBody restored = CelestialSerializer.FromJson(jsonString);

        if (restored == null)
        {
            throw new InvalidOperationException("Expected non-null restored body");
        }
        if (restored.Id != body.Id)
        {
            throw new InvalidOperationException($"Expected id '{body.Id}', got '{restored.Id}'");
        }
        if (restored.Name != body.Name)
        {
            throw new InvalidOperationException($"Expected name '{body.Name}', got '{restored.Name}'");
        }
        if (System.Math.Abs(restored.Physical.MassKg - body.Physical.MassKg) > DefaultTolerance)
        {
            throw new InvalidOperationException($"Expected mass {body.Physical.MassKg}, got {restored.Physical.MassKg}");
        }
    }

    /// <summary>
    /// Tests from_dict handles missing optional fields.
    /// </summary>
    public static void TestFromDictHandlesMissingFields()
    {
        Godot.Collections.Dictionary data = new Godot.Collections.Dictionary
        {
            ["id"] = "minimal_001",
            ["name"] = "Minimal",
            ["type"] = "Planet",
            ["physical"] = new Godot.Collections.Dictionary { ["mass_kg"] = 1.0e24, ["radius_m"] = 5.0e6 }
        };
        CelestialBody body = CelestialSerializer.FromDictionary(data);

        if (body == null)
        {
            throw new InvalidOperationException("Expected non-null body");
        }
        if (body.Id != "minimal_001")
        {
            throw new InvalidOperationException($"Expected id 'minimal_001', got '{body.Id}'");
        }
        if (body.HasOrbital())
        {
            throw new InvalidOperationException("Should not have orbital");
        }
        if (body.HasSurface())
        {
            throw new InvalidOperationException("Should not have surface");
        }
    }

    /// <summary>
    /// Tests from_json handles invalid JSON.
    /// </summary>
    public static void TestFromJsonHandlesInvalid()
    {
        CelestialBody body = CelestialSerializer.FromJson("not valid json");
        if (body != null)
        {
            throw new InvalidOperationException("Expected null for invalid JSON");
        }
    }

    /// <summary>
    /// Tests from_dict handles empty dictionary.
    /// </summary>
    public static void TestFromDictHandlesEmpty()
    {
        CelestialBody body = CelestialSerializer.FromDictionary(new Dictionary());
        if (body != null)
        {
            throw new InvalidOperationException("Expected null for empty dictionary");
        }
    }

    /// <summary>
    /// Tests schema_version is included in output.
    /// </summary>
    public static void TestSchemaVersionIncluded()
    {
        PhysicalProps physical = new PhysicalProps(1.0e24, 5.0e6);
        CelestialBody body = new CelestialBody("test", "Test", CelestialType.Type.Planet, physical);
        Godot.Collections.Dictionary data = CelestialSerializer.ToDict(body);

        if (data["schema_version"].AsInt32() != Versions.SchemaVersion)
        {
            throw new InvalidOperationException($"Expected schema_version {Versions.SchemaVersion}, got {data["schema_version"]}");
        }
    }
}
