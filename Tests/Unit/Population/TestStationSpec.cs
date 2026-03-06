#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for StationSpec.
/// </summary>
public static class TestStationSpec
{
    /// <summary>
    /// Tests default creation.
    /// </summary>
    public static void TestCreationDefault()
    {
        StationSpec spec = new StationSpec();

        DotNetNativeTestSuite.AssertTrue(spec.GenerateStations, "GenerateStations should be true");
        DotNetNativeTestSuite.AssertTrue(spec.AllowUtility, "AllowUtility should be true");
        DotNetNativeTestSuite.AssertTrue(spec.AllowOutposts, "AllowOutposts should be true");
        DotNetNativeTestSuite.AssertTrue(spec.AllowLargeStations, "AllowLargeStations should be true");
        DotNetNativeTestSuite.AssertTrue(spec.AllowDeepSpace, "AllowDeepSpace should be true");
        DotNetNativeTestSuite.AssertTrue(spec.AllowBeltStations, "AllowBeltStations should be true");
        DotNetNativeTestSuite.AssertEqual(0, spec.MinStations, "MinStations should be 0");
        DotNetNativeTestSuite.AssertEqual(0, spec.MaxStations, "MaxStations should be 0");
        DotNetNativeTestSuite.AssertFloatEqual(1.0f, spec.PopulationDensity, 0.001f, "PopulationDensity should be 1.0");
        DotNetNativeTestSuite.AssertNull(spec.ForceContext, "ForceContext should be null");
    }

    /// <summary>
    /// Tests minimal factory.
    /// </summary>
    public static void TestMinimalFactory()
    {
        StationSpec spec = StationSpec.Minimal();

        DotNetNativeTestSuite.AssertTrue(spec.GenerateStations, "GenerateStations should be true");
        DotNetNativeTestSuite.AssertFalse(spec.AllowLargeStations, "AllowLargeStations should be false");
        DotNetNativeTestSuite.AssertEqual(2, spec.MaxStations, "MaxStations should be 2");
        DotNetNativeTestSuite.AssertLessThan(spec.PopulationDensity, 1.0f, "PopulationDensity should be < 1.0");
    }

    /// <summary>
    /// Tests standard factory.
    /// </summary>
    public static void TestStandardFactory()
    {
        StationSpec spec = StationSpec.Standard();

        DotNetNativeTestSuite.AssertTrue(spec.GenerateStations, "GenerateStations should be true");
        DotNetNativeTestSuite.AssertTrue(spec.AllowUtility, "AllowUtility should be true");
        DotNetNativeTestSuite.AssertTrue(spec.AllowOutposts, "AllowOutposts should be true");
        DotNetNativeTestSuite.AssertTrue(spec.AllowLargeStations, "AllowLargeStations should be true");
    }

    /// <summary>
    /// Tests dense factory.
    /// </summary>
    public static void TestDenseFactory()
    {
        StationSpec spec = StationSpec.Dense();

        DotNetNativeTestSuite.AssertGreaterThan(spec.PopulationDensity, 1.0f, "PopulationDensity should be > 1.0");
        DotNetNativeTestSuite.AssertGreaterThan(spec.MinStations, 0, "MinStations should be > 0");
    }

    /// <summary>
    /// Tests for_context factory.
    /// </summary>
    public static void TestForContextFactory()
    {
        StationSpec spec = StationSpec.ForContext(StationPlacementContext.Context.BridgeSystem);

        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.BridgeSystem, spec.ForceContext, "ForceContext should be BridgeSystem");
    }

    /// <summary>
    /// Tests is_purpose_allowed with no restrictions.
    /// </summary>
    public static void TestIsPurposeAllowedUnrestricted()
    {
        StationSpec spec = new StationSpec();

        DotNetNativeTestSuite.AssertTrue(spec.IsPurposeAllowed(StationPurpose.Purpose.Utility), "Utility should be allowed");
        DotNetNativeTestSuite.AssertTrue(spec.IsPurposeAllowed(StationPurpose.Purpose.Mining), "Mining should be allowed");
        DotNetNativeTestSuite.AssertTrue(spec.IsPurposeAllowed(StationPurpose.Purpose.Military), "Military should be allowed");
    }

    /// <summary>
    /// Tests is_purpose_allowed with required purposes.
    /// </summary>
    public static void TestIsPurposeAllowedRequired()
    {
        StationSpec spec = new StationSpec();
        spec.RequiredPurposes = new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Mining, StationPurpose.Purpose.Science };

        DotNetNativeTestSuite.AssertTrue(spec.IsPurposeAllowed(StationPurpose.Purpose.Mining), "Mining should be allowed");
        DotNetNativeTestSuite.AssertTrue(spec.IsPurposeAllowed(StationPurpose.Purpose.Science), "Science should be allowed");
        DotNetNativeTestSuite.AssertFalse(spec.IsPurposeAllowed(StationPurpose.Purpose.Utility), "Utility should not be allowed");
    }

    /// <summary>
    /// Tests is_purpose_allowed with excluded purposes.
    /// </summary>
    public static void TestIsPurposeAllowedExcluded()
    {
        StationSpec spec = new StationSpec();
        spec.ExcludedPurposes = new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Military };

        DotNetNativeTestSuite.AssertTrue(spec.IsPurposeAllowed(StationPurpose.Purpose.Mining), "Mining should be allowed");
        DotNetNativeTestSuite.AssertFalse(spec.IsPurposeAllowed(StationPurpose.Purpose.Military), "Military should not be allowed");
    }

    /// <summary>
    /// Tests is_class_allowed.
    /// </summary>
    public static void TestIsClassAllowed()
    {
        StationSpec spec = new StationSpec();

        DotNetNativeTestSuite.AssertTrue(spec.IsClassAllowed(StationClass.Class.U), "U should be allowed");
        DotNetNativeTestSuite.AssertTrue(spec.IsClassAllowed(StationClass.Class.O), "O should be allowed");
        DotNetNativeTestSuite.AssertTrue(spec.IsClassAllowed(StationClass.Class.B), "B should be allowed");

        spec.AllowUtility = false;
        DotNetNativeTestSuite.AssertFalse(spec.IsClassAllowed(StationClass.Class.U), "U should not be allowed");

        spec.AllowLargeStations = false;
        DotNetNativeTestSuite.AssertFalse(spec.IsClassAllowed(StationClass.Class.B), "B should not be allowed");
        DotNetNativeTestSuite.AssertFalse(spec.IsClassAllowed(StationClass.Class.A), "A should not be allowed");
    }

    /// <summary>
    /// Tests validation - valid spec.
    /// </summary>
    public static void TestValidationValid()
    {
        StationSpec spec = new StationSpec();
        DotNetNativeTestSuite.AssertTrue(spec.IsValid(), "Should be valid");
        DotNetNativeTestSuite.AssertEqual(0, spec.Validate().Count, "Should have no errors");
    }

    /// <summary>
    /// Tests validation - min > max stations.
    /// </summary>
    public static void TestValidationMinMaxStations()
    {
        StationSpec spec = new StationSpec();
        spec.MinStations = 10;
        spec.MaxStations = 5;

        Array<string> errors = spec.Validate();
        DotNetNativeTestSuite.AssertGreaterThan(errors.Count, 0, "Should have errors");
        DotNetNativeTestSuite.AssertTrue(errors[0].Contains("min_stations"), "Should have min_stations error");
    }

    /// <summary>
    /// Tests validation - negative population density.
    /// </summary>
    public static void TestValidationNegativeDensity()
    {
        StationSpec spec = new StationSpec();
        spec.PopulationDensity = -1.0f;

        Array<string> errors = spec.Validate();
        DotNetNativeTestSuite.AssertGreaterThan(errors.Count, 0, "Should have errors");
        DotNetNativeTestSuite.AssertTrue(errors[0].Contains("population_density"), "Should have population_density error");
    }

    /// <summary>
    /// Tests validation - invalid decommission chance.
    /// </summary>
    public static void TestValidationInvalidDecommission()
    {
        StationSpec spec = new StationSpec();
        spec.DecommissionChance = 1.5f;

        Array<string> errors = spec.Validate();
        DotNetNativeTestSuite.AssertGreaterThan(errors.Count, 0, "Should have errors");
        DotNetNativeTestSuite.AssertTrue(errors[0].Contains("decommission_chance"), "Should have decommission_chance error");
    }

    /// <summary>
    /// Tests validation - invalid year range.
    /// </summary>
    public static void TestValidationInvalidYears()
    {
        StationSpec spec = new StationSpec();
        spec.MinEstablishedYear = 100;
        spec.MaxEstablishedYear = -100;

        Array<string> errors = spec.Validate();
        DotNetNativeTestSuite.AssertGreaterThan(errors.Count, 0, "Should have errors");
        DotNetNativeTestSuite.AssertTrue(errors[0].ToLower().Contains("year"), "Should have year error");
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        StationSpec original = new StationSpec();
        original.GenerationSeed = 12345;
        original.GenerateStations = true;
        original.ForceContext = StationPlacementContext.Context.ColonyWorld;
        original.MinStations = 2;
        original.MaxStations = 10;
        original.AllowUtility = false;
        original.PopulationDensity = 1.5f;
        original.DecommissionChance = 0.1f;
        original.RequiredPurposes = new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Trade };
        original.ExcludedPurposes = new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Military };
        original.IdPrefix = "test_station";
        original.FoundingCivilizationId = "civ_001";

        Godot.Collections.Dictionary data = original.ToDictionary();
        StationSpec restored = StationSpec.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.GenerationSeed, restored.GenerationSeed, "GenerationSeed should match");
        DotNetNativeTestSuite.AssertEqual(original.GenerateStations, restored.GenerateStations, "GenerateStations should match");
        DotNetNativeTestSuite.AssertEqual((int)original.ForceContext, (int)restored.ForceContext, "ForceContext should match");
        DotNetNativeTestSuite.AssertEqual(original.MinStations, restored.MinStations, "MinStations should match");
        DotNetNativeTestSuite.AssertEqual(original.MaxStations, restored.MaxStations, "MaxStations should match");
        DotNetNativeTestSuite.AssertEqual(original.AllowUtility, restored.AllowUtility, "AllowUtility should match");
        DotNetNativeTestSuite.AssertFloatEqual(original.PopulationDensity, restored.PopulationDensity, 0.001f, "PopulationDensity should match");
        DotNetNativeTestSuite.AssertFloatEqual(original.DecommissionChance, restored.DecommissionChance, 0.001f, "DecommissionChance should match");
        DotNetNativeTestSuite.AssertEqual(original.RequiredPurposes.Count, restored.RequiredPurposes.Count, "RequiredPurposes count should match");
        DotNetNativeTestSuite.AssertEqual(original.ExcludedPurposes.Count, restored.ExcludedPurposes.Count, "ExcludedPurposes count should match");
        DotNetNativeTestSuite.AssertEqual(original.IdPrefix, restored.IdPrefix, "IdPrefix should match");
        DotNetNativeTestSuite.AssertEqual(original.FoundingCivilizationId, restored.FoundingCivilizationId, "FoundingCivilizationId should match");
    }

    /// <summary>
    /// Tests serialization without force_context.
    /// </summary>
    public static void TestSerializationNoForceContext()
    {
        StationSpec original = new StationSpec();
        original.ForceContext = null;

        Godot.Collections.Dictionary data = original.ToDictionary();
        StationSpec restored = StationSpec.FromDictionary(data);

        DotNetNativeTestSuite.AssertNull(restored.ForceContext, "ForceContext should be null");
    }
}
