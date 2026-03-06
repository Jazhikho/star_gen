#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation;
using StarGen.Services.Persistence;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

/// <summary>
/// Integration tests for CelestialPersistence.
/// </summary>
public static class TestCelestialPersistence
{
    private const string TestFilePath = "user://test_celestial_bodies/test_body.json";

    /// <summary>
    /// Runs all tests in this suite.
    /// </summary>
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest(
            "TestCelestialPersistence::test_save_and_load",
            TestSaveAndLoad);
        runner.RunNativeTest(
            "TestCelestialPersistence::test_load_nonexistent_file",
            TestLoadNonexistentFile);
        runner.RunNativeTest(
            "TestCelestialPersistence::test_delete",
            TestDelete);
        runner.RunNativeTest(
            "TestCelestialPersistence::test_default_path",
            TestDefaultPath);
        runner.RunNativeTest(
            "TestCelestialPersistence::test_full_round_trip_integrity",
            TestFullRoundTripIntegrity);
    }

    /// <summary>
    /// Creates a test body.
    /// </summary>
    private static CelestialBody CreateTestBody()
    {
        PhysicalProps physical = new(5.972e24, 6.371e6, 86400.0, 23.5);
        Provenance provenance = Provenance.CreateCurrent(42);
        CelestialBody body = new(
            "persistence_test_001",
            "Persistence Test Planet",
            CelestialType.Type.Planet,
            physical,
            provenance
        );
        body.Orbital = new OrbitalProps(1.5e11, 0.02, 1.5);
        body.Surface = new SurfaceProps(288.0, 0.3, "rocky");
        return body;
    }

    /// <summary>
    /// Clean up test file after each test.
    /// </summary>
    private static void CleanupTestFile()
    {
        if (FileAccess.FileExists(TestFilePath))
        {
            CelestialPersistence.DeleteBody(TestFilePath);
        }
    }

    /// <summary>
    /// Tests save and load round-trip.
    /// </summary>
    private static void TestSaveAndLoad()
    {
        try
        {
            CelestialBody original = CreateTestBody();

            Error saveResult = CelestialPersistence.SaveBody(original, TestFilePath);
            DotNetNativeTestSuite.AssertEqual(Error.Ok, saveResult, "Save should succeed");

            CelestialBody loaded = CelestialPersistence.LoadBody(TestFilePath);
            DotNetNativeTestSuite.AssertNotNull(loaded, "Load should return body");

            DotNetNativeTestSuite.AssertEqual(original.Id, loaded.Id);
            DotNetNativeTestSuite.AssertEqual(original.Name, loaded.Name);
            DotNetNativeTestSuite.AssertEqual(original.Type, loaded.Type);
            DotNetNativeTestSuite.AssertFloatNear(original.Physical.MassKg, loaded.Physical.MassKg, 1.0, "Mass should match");
            DotNetNativeTestSuite.AssertFloatNear(original.Physical.RadiusM, loaded.Physical.RadiusM, 1.0, "Radius should match");
        }
        finally
        {
            CleanupTestFile();
        }
    }

    /// <summary>
    /// Tests load from non-existent file returns null.
    /// </summary>
    private static void TestLoadNonexistentFile()
    {
        CelestialBody body = CelestialPersistence.LoadBody("user://does_not_exist.json");
        DotNetNativeTestSuite.AssertNull(body);
    }

    /// <summary>
    /// Tests delete removes file.
    /// </summary>
    private static void TestDelete()
    {
        try
        {
            CelestialBody body = CreateTestBody();
            CelestialPersistence.SaveBody(body, TestFilePath);

            DotNetNativeTestSuite.AssertTrue(FileAccess.FileExists(TestFilePath));

            Error deleteResult = CelestialPersistence.DeleteBody(TestFilePath);
            DotNetNativeTestSuite.AssertEqual(Error.Ok, deleteResult);
            DotNetNativeTestSuite.AssertFalse(FileAccess.FileExists(TestFilePath));
        }
        finally
        {
            CleanupTestFile();
        }
    }

    /// <summary>
    /// Tests default path generation.
    /// </summary>
    private static void TestDefaultPath()
    {
        CelestialBody body = CreateTestBody();
        string path = CelestialPersistence.GetDefaultPath(body);

        DotNetNativeTestSuite.AssertTrue(path.EndsWith(".json"));
        DotNetNativeTestSuite.AssertTrue(path.Contains(body.Id.ToLower()));
    }

    /// <summary>
    /// Tests full persistence round-trip verifies all data.
    /// </summary>
    private static void TestFullRoundTripIntegrity()
    {
        try
        {
            CelestialBody original = CreateTestBody();
            Godot.Collections.Dictionary atmoComp = new Godot.Collections.Dictionary { { "N2", 0.78 }, { "O2", 0.21 } };
            original.Atmosphere = new AtmosphereProps(101325.0, 8500.0, atmoComp, 1.0);
            original.Surface.Terrain = new TerrainProps(15000.0, 0.5, 0.2, 0.6, 0.4, "mixed");

            Godot.Collections.Dictionary bandComp = new Godot.Collections.Dictionary { { "ice", 1.0 } };
            RingBand band = new RingBand(1.0e8, 2.0e8, 0.5, bandComp, 1.0, "Main");
            Godot.Collections.Array<RingBand> bands = new Godot.Collections.Array<RingBand> { band };
            original.RingSystem = new RingSystemProps(bands, 1.0e17);

            CelestialPersistence.SaveBody(original, TestFilePath);
            CelestialBody loaded = CelestialPersistence.LoadBody(TestFilePath);

            DotNetNativeTestSuite.AssertTrue(loaded.HasOrbital());
            DotNetNativeTestSuite.AssertTrue(loaded.HasSurface());
            DotNetNativeTestSuite.AssertTrue(loaded.HasAtmosphere());
            DotNetNativeTestSuite.AssertTrue(loaded.HasRingSystem());

            DotNetNativeTestSuite.AssertFloatNear(original.Orbital.SemiMajorAxisM, loaded.Orbital.SemiMajorAxisM, 1.0, "Semi-major axis should match");
            DotNetNativeTestSuite.AssertFloatNear(original.Orbital.Eccentricity, loaded.Orbital.Eccentricity, 0.001, "Eccentricity should match");

            DotNetNativeTestSuite.AssertFloatNear(original.Surface.TemperatureK, loaded.Surface.TemperatureK, 0.01, "Temperature should match");
            DotNetNativeTestSuite.AssertFloatNear(original.Surface.Albedo, loaded.Surface.Albedo, 0.001, "Albedo should match");

            DotNetNativeTestSuite.AssertFloatNear(original.Atmosphere.SurfacePressurePa, loaded.Atmosphere.SurfacePressurePa, 1.0, "Pressure should match");
            DotNetNativeTestSuite.AssertTrue(loaded.Atmosphere.Composition.ContainsKey("N2"));

            DotNetNativeTestSuite.AssertTrue(loaded.Surface.HasTerrain());
            DotNetNativeTestSuite.AssertFloatNear(original.Surface.Terrain.ElevationRangeM, loaded.Surface.Terrain.ElevationRangeM, 1.0, "Elevation range should match");

            DotNetNativeTestSuite.AssertEqual(1, loaded.RingSystem.GetBandCount());
            DotNetNativeTestSuite.AssertEqual("Main", loaded.RingSystem.GetBand(0).Name);

            DotNetNativeTestSuite.AssertEqual(original.Provenance.GenerationSeed, loaded.Provenance.GenerationSeed);
        }
        finally
        {
            CleanupTestFile();
        }
    }
}
