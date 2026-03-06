#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for GalaxySaveData.
/// </summary>
public static class TestGalaxySaveData
{
    /// <summary>
    /// Tests create sets timestamp.
    /// </summary>
    public static void TestCreateSetsTimestamp()
    {
        GalaxySaveData data = GalaxySaveData.Create(1000);

        if (data.SavedAt <= 0)
        {
            throw new InvalidOperationException("Should have timestamp");
        }
    }

    /// <summary>
    /// Tests default values.
    /// </summary>
    public static void TestDefaultValues()
    {
        GalaxySaveData data = new GalaxySaveData();

        DotNetNativeTestSuite.AssertEqual(GalaxySaveData.FormatVersion, data.Version, "Should have current version");
        DotNetNativeTestSuite.AssertEqual(42, data.GalaxySeed, "Should have default seed");
        DotNetNativeTestSuite.AssertEqual((int)GalaxyCoordinates.ZoomLevel.SubSector, (int)data.ZoomLevel, "Should have default zoom");
    }

    /// <summary>
    /// Tests is valid with defaults.
    /// </summary>
    public static void TestIsValidWithDefaults()
    {
        GalaxySaveData data = new GalaxySaveData();

        if (!data.IsValid())
        {
            throw new InvalidOperationException("Default data should be valid");
        }
    }

    /// <summary>
    /// Tests is valid rejects zero seed.
    /// </summary>
    public static void TestIsValidRejectsZeroSeed()
    {
        GalaxySaveData data = new GalaxySaveData();
        data.GalaxySeed = 0;

        if (data.IsValid())
        {
            throw new InvalidOperationException("Zero seed should be invalid");
        }
    }

    /// <summary>
    /// Tests is valid rejects invalid zoom.
    /// </summary>
    public static void TestIsValidRejectsInvalidZoom()
    {
        GalaxySaveData data = new GalaxySaveData();
        data.ZoomLevel = (GalaxyCoordinates.ZoomLevel)(-1);

        if (data.IsValid())
        {
            throw new InvalidOperationException("Negative zoom should be invalid");
        }
    }

    /// <summary>
    /// Tests to dict contains required fields.
    /// </summary>
    public static void TestToDictContainsRequiredFields()
    {
        GalaxySaveData data = GalaxySaveData.Create(1000);
        data.GalaxySeed = 12345;
        data.ZoomLevel = GalaxyCoordinates.ZoomLevel.Quadrant;

        Godot.Collections.Dictionary dict = data.ToDictionary();

        if (!dict.ContainsKey("version"))
        {
            throw new InvalidOperationException("Should have version");
        }
        if (!dict.ContainsKey("galaxy_seed"))
        {
            throw new InvalidOperationException("Should have galaxy_seed");
        }
        if (!dict.ContainsKey("zoom_level"))
        {
            throw new InvalidOperationException("Should have zoom_level");
        }
        if (!dict.ContainsKey("saved_at"))
        {
            throw new InvalidOperationException("Should have saved_at");
        }
    }

    /// <summary>
    /// Tests round trip basic.
    /// </summary>
    public static void TestRoundTripBasic()
    {
        GalaxySaveData original = GalaxySaveData.Create(0);
        original.GalaxySeed = 99999;
        original.ZoomLevel = GalaxyCoordinates.ZoomLevel.Sector;

        Godot.Collections.Dictionary dict = original.ToDictionary();
        GalaxySaveData restored = GalaxySaveData.FromDictionary(dict);

        DotNetNativeTestSuite.AssertNotNull(restored, "Should deserialize");
        DotNetNativeTestSuite.AssertEqual(original.GalaxySeed, restored.GalaxySeed, "Seed should match");
        DotNetNativeTestSuite.AssertEqual((int)original.ZoomLevel, (int)restored.ZoomLevel, "Zoom should match");
    }

    /// <summary>
    /// Tests round trip with quadrant.
    /// </summary>
    public static void TestRoundTripWithQuadrant()
    {
        GalaxySaveData original = GalaxySaveData.Create(0);
        original.SelectedQuadrant = new Vector3I(7, 0, 3);

        Godot.Collections.Dictionary dict = original.ToDictionary();
        GalaxySaveData restored = GalaxySaveData.FromDictionary(dict);

        DotNetNativeTestSuite.AssertNotNull(restored.SelectedQuadrant, "Should have quadrant");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(7, 0, 3), restored.SelectedQuadrant, "Quadrant should match");
    }

    /// <summary>
    /// Tests round trip with sector.
    /// </summary>
    public static void TestRoundTripWithSector()
    {
        GalaxySaveData original = GalaxySaveData.Create(0);
        original.SelectedSector = new Vector3I(5, 2, 8);

        Godot.Collections.Dictionary dict = original.ToDictionary();
        GalaxySaveData restored = GalaxySaveData.FromDictionary(dict);

        DotNetNativeTestSuite.AssertNotNull(restored.SelectedSector, "Should have sector");
        DotNetNativeTestSuite.AssertEqual(new Vector3I(5, 2, 8), restored.SelectedSector, "Sector should match");
    }

    /// <summary>
    /// Tests round trip with camera.
    /// </summary>
    public static void TestRoundTripWithCamera()
    {
        GalaxySaveData original = GalaxySaveData.Create(0);
        original.CameraPosition = new Vector3(8000.5f, 20.3f, 150.7f);
        original.CameraRotation = new Vector3(0.1f, 0.5f, 0.0f);

        Godot.Collections.Dictionary dict = original.ToDictionary();
        GalaxySaveData restored = GalaxySaveData.FromDictionary(dict);

        if (!restored.CameraPosition.IsEqualApprox(original.CameraPosition))
        {
            throw new InvalidOperationException("Camera position should match");
        }
        if (!restored.CameraRotation.IsEqualApprox(original.CameraRotation))
        {
            throw new InvalidOperationException("Camera rotation should match");
        }
    }

    /// <summary>
    /// Tests round trip with star selection.
    /// </summary>
    public static void TestRoundTripWithStarSelection()
    {
        GalaxySaveData original = GalaxySaveData.Create(0);
        original.HasStarSelection = true;
        original.SelectedStarSeed = 55555;
        original.SelectedStarPosition = new Vector3(8001.2f, 19.8f, 155.3f);

        Godot.Collections.Dictionary dict = original.ToDictionary();
        GalaxySaveData restored = GalaxySaveData.FromDictionary(dict);

        if (!restored.HasStarSelection)
        {
            throw new InvalidOperationException("Should have star selection");
        }
        DotNetNativeTestSuite.AssertEqual(55555, restored.SelectedStarSeed, "Star seed should match");
        if (!restored.SelectedStarPosition.IsEqualApprox(original.SelectedStarPosition))
        {
            throw new InvalidOperationException("Star position should match");
        }
    }

    /// <summary>
    /// Tests null quadrant serializes.
    /// </summary>
    public static void TestNullQuadrantSerializes()
    {
        GalaxySaveData original = GalaxySaveData.Create(0);
        original.SelectedQuadrant = null;

        Godot.Collections.Dictionary dict = original.ToDictionary();
        GalaxySaveData restored = GalaxySaveData.FromDictionary(dict);

        DotNetNativeTestSuite.AssertNull(restored.SelectedQuadrant, "Null quadrant should deserialize as null");
    }

    /// <summary>
    /// Tests from dict returns null for invalid.
    /// </summary>
    public static void TestFromDictReturnsNullForInvalid()
    {
        Godot.Collections.Dictionary invalid = new Godot.Collections.Dictionary { { "foo", "bar" } };
        GalaxySaveData result = GalaxySaveData.FromDictionary(invalid);

        DotNetNativeTestSuite.AssertNull(result, "Should return null for invalid dict");
    }

    /// <summary>
    /// Tests from dict returns null for empty.
    /// </summary>
    public static void TestFromDictReturnsNullForEmpty()
    {
        Godot.Collections.Dictionary empty = new Godot.Collections.Dictionary();
        GalaxySaveData result = GalaxySaveData.FromDictionary(empty);

        DotNetNativeTestSuite.AssertNull(result, "Should return null for empty dict");
    }

    /// <summary>
    /// Tests get summary.
    /// </summary>
    public static void TestGetSummary()
    {
        GalaxySaveData data = GalaxySaveData.Create(1000);
        data.GalaxySeed = 42;
        data.ZoomLevel = GalaxyCoordinates.ZoomLevel.SubSector;

        string summary = data.GetSummary();

        if (!summary.Contains("42"))
        {
            throw new InvalidOperationException("Summary should contain seed");
        }
        if (!summary.Contains("Star Field"))
        {
            throw new InvalidOperationException("Summary should contain zoom level");
        }
    }

    /// <summary>
    /// Tests vector3 conversion.
    /// </summary>
    public static void TestVector3Conversion()
    {
        Vector3 original = new Vector3(1.5f, 2.7f, 3.9f);
        Godot.Collections.Array arr = GalaxySaveData.Vector3ToArray(original);
        Vector3 restored = GalaxySaveData.ArrayToVector3(arr);

        if (!restored.IsEqualApprox(original))
        {
            throw new InvalidOperationException("Vector3 should round-trip");
        }
    }

    /// <summary>
    /// Tests vector3i conversion.
    /// </summary>
    public static void TestVector3iConversion()
    {
        Vector3I original = new Vector3I(5, -3, 8);
        Godot.Collections.Array arr = GalaxySaveData.Vector3iToArray(original);
        Vector3I restored = GalaxySaveData.ArrayToVector3i(arr);

        DotNetNativeTestSuite.AssertEqual(original, restored, "Vector3i should round-trip");
    }

    /// <summary>
    /// Tests body overrides default empty.
    /// </summary>
    public static void TestBodyOverridesDefaultEmpty()
    {
        GalaxySaveData data = GalaxySaveData.Create(0);
        if (data.HasBodyOverrides())
        {
            throw new InvalidOperationException("Should not have body overrides by default");
        }
        GalaxyBodyOverrides overrides = data.GetBodyOverrides();
        if (!overrides.IsEmpty())
        {
            throw new InvalidOperationException("Body overrides should be empty");
        }
    }

    /// <summary>
    /// Tests body overrides round trip.
    /// </summary>
    public static void TestBodyOverridesRoundTrip()
    {
        GalaxyBodyOverrides overrides = new GalaxyBodyOverrides();
        Godot.Collections.Dictionary bodyDict = new Godot.Collections.Dictionary
        {
            { "id", "p1" },
            { "name", "Planet One" },
            { "type", "planet" },
            { "physical", new Godot.Collections.Dictionary { { "mass_kg", 1e24 }, { "radius_m", 1e6 } } }
        };
        overrides.SetOverrideDict(100, "p1", bodyDict);

        GalaxySaveData data = GalaxySaveData.Create(0);
        data.SetBodyOverrides(overrides);
        if (!data.HasBodyOverrides())
        {
            throw new InvalidOperationException("Should have body overrides");
        }

        Godot.Collections.Dictionary dict = data.ToDictionary();
        if (!dict.ContainsKey("body_overrides_data"))
        {
            throw new InvalidOperationException("Should have body_overrides_data");
        }

        GalaxySaveData restored = GalaxySaveData.FromDictionary(dict);
        DotNetNativeTestSuite.AssertNotNull(restored, "Should deserialize");
        if (!restored.HasBodyOverrides())
        {
            throw new InvalidOperationException("Restored should have body overrides");
        }
        GalaxyBodyOverrides restoredOverrides = restored.GetBodyOverrides();
        if (!restoredOverrides.HasAnyFor(100))
        {
            throw new InvalidOperationException("Should have overrides for seed 100");
        }
        if (restoredOverrides.GetOverrideDict(100, "p1").Count == 0)
        {
            throw new InvalidOperationException("Should have override dict for p1");
        }
    }

    /// <summary>
    /// Tests body overrides absent field legacy save.
    /// </summary>
    public static void TestBodyOverridesAbsentFieldLegacySave()
    {
        Godot.Collections.Dictionary dict = new Godot.Collections.Dictionary
        {
            { "version", 1 },
            { "galaxy_seed", 42 }
        };
        GalaxySaveData data = GalaxySaveData.FromDictionary(dict);
        DotNetNativeTestSuite.AssertNotNull(data, "Should deserialize legacy save");
        if (data.HasBodyOverrides())
        {
            throw new InvalidOperationException("Legacy save should not have body overrides");
        }
        if (!data.GetBodyOverrides().IsEmpty())
        {
            throw new InvalidOperationException("Body overrides should be empty");
        }
    }

    /// <summary>
    /// Tests set body overrides null clears.
    /// </summary>
    public static void TestSetBodyOverridesNullClears()
    {
        GalaxySaveData data = GalaxySaveData.Create(0);
        data.BodyOverridesData = new Godot.Collections.Dictionary { { "1", new Godot.Collections.Dictionary { { "id", "x" } } } };
        data.SetBodyOverrides(null);
        if (data.BodyOverridesData.Count != 0)
        {
            throw new InvalidOperationException("Body overrides data should be empty");
        }
        if (data.HasBodyOverrides())
        {
            throw new InvalidOperationException("Should not have body overrides");
        }
    }
}
