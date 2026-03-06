#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for SpaceStation data model.
/// </summary>
public static class TestSpaceStation
{
    /// <summary>
    /// Creates a small test station (O-class).
    /// </summary>
    private static SpaceStation CreateSmallStation()
    {
        SpaceStation station = new SpaceStation();
        station.Id = "station_001";
        station.Name = "Outpost Gamma";
        station.StationClass = StationClass.Class.O;
        station.StationType = StationType.Type.Orbital;
        station.PrimaryPurpose = StationPurpose.Purpose.Mining;
        station.PlacementContext = StationPlacementContext.Context.ResourceSystem;
        station.OutpostAuthority = OutpostAuthority.Type.Corporate;
        station.ParentOrganizationId = "corp_001";
        station.Population = 5000;
        station.EstablishedYear = -100;
        station.SystemId = "system_001";
        station.OrbitingBodyId = "planet_001";
        station.Services = new Array<StationService.Service> { StationService.Service.Refuel, StationService.Service.Storage };
        return station;
    }

    /// <summary>
    /// Creates a large test station (A-class).
    /// </summary>
    private static SpaceStation CreateLargeStation()
    {
        SpaceStation station = new SpaceStation();
        station.Id = "station_002";
        station.Name = "Central Hub";
        station.StationClass = StationClass.Class.A;
        station.StationType = StationType.Type.Orbital;
        station.PrimaryPurpose = StationPurpose.Purpose.Trade;
        station.PlacementContext = StationPlacementContext.Context.ColonyWorld;
        station.Population = 500000;
        station.PeakPopulation = 600000;
        station.PeakPopulationYear = -20;
        station.EstablishedYear = -200;
        station.SystemId = "system_002";
        station.OrbitingBodyId = "planet_002";
        station.FoundingCivilizationId = "civ_001";
        station.FoundingCivilizationName = "United Colonies";

        Array<StationService.Service> services = StationService.BasicUtilityServices();
        services.Add(StationService.Service.Shipyard);
        services.Add(StationService.Service.Banking);
        station.Services = services;

        station.Government = new Government();
        station.Government.Regime = GovernmentType.Regime.Constitutional;
        station.Government.Legitimacy = 0.85f;

        station.History = new PopulationHistory();
        station.History.AddNewEvent(
            HistoryEvent.EventType.Founding,
            -200,
            "Station Founded",
            "Central Hub established as trade station"
        );

        return station;
    }

    /// <summary>
    /// Tests default creation.
    /// </summary>
    public static void TestCreationDefault()
    {
        SpaceStation station = new SpaceStation();
        DotNetNativeTestSuite.AssertEqual("", station.Id, "Default ID should be empty");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, station.StationClass, "Default class should be O");
        DotNetNativeTestSuite.AssertEqual(0, station.Population, "Default population should be 0");
        DotNetNativeTestSuite.AssertTrue(station.IsOperational, "Default should be operational");
        DotNetNativeTestSuite.AssertNull(station.Government, "Default government should be null");
        DotNetNativeTestSuite.AssertNull(station.History, "Default history should be null");
    }

    /// <summary>
    /// Tests get_age for operational station.
    /// </summary>
    public static void TestGetAgeOperational()
    {
        SpaceStation station = CreateSmallStation();
        station.EstablishedYear = -100;

        DotNetNativeTestSuite.AssertEqual(100, station.GetAge(0), "Age should be 100");
    }

    /// <summary>
    /// Tests get_age for decommissioned station.
    /// </summary>
    public static void TestGetAgeDecommissioned()
    {
        SpaceStation station = CreateSmallStation();
        station.EstablishedYear = -100;
        station.IsOperational = false;
        station.DecommissionedYear = -30;

        DotNetNativeTestSuite.AssertEqual(70, station.GetAge(0), "Age should be 70");
    }

    /// <summary>
    /// Tests update_class_from_population - staying small.
    /// </summary>
    public static void TestUpdateClassSmall()
    {
        SpaceStation station = new SpaceStation();
        station.StationClass = StationClass.Class.O;
        station.Population = 5000;

        station.UpdateClassFromPopulation();

        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, station.StationClass, "Should remain O class");
        DotNetNativeTestSuite.AssertNull(station.Government, "Government should be null");
        DotNetNativeTestSuite.AssertNull(station.History, "History should be null");
    }

    /// <summary>
    /// Tests update_class_from_population - growing to B-class.
    /// </summary>
    public static void TestUpdateClassGrowthToB()
    {
        SpaceStation station = new SpaceStation();
        station.StationClass = StationClass.Class.O;
        station.Population = 50000;

        station.UpdateClassFromPopulation();

        DotNetNativeTestSuite.AssertEqual(StationClass.Class.B, station.StationClass, "Should grow to B class");
        DotNetNativeTestSuite.AssertNotNull(station.Government, "Government should be created");
        DotNetNativeTestSuite.AssertNotNull(station.History, "History should be created");
    }

    /// <summary>
    /// Tests update_class_from_population - growing to A-class.
    /// </summary>
    public static void TestUpdateClassGrowthToA()
    {
        SpaceStation station = new SpaceStation();
        station.StationClass = StationClass.Class.O;
        station.Population = 500000;

        station.UpdateClassFromPopulation();

        DotNetNativeTestSuite.AssertEqual(StationClass.Class.A, station.StationClass, "Should grow to A class");
        DotNetNativeTestSuite.AssertNotNull(station.Government, "Government should be created");
    }

    /// <summary>
    /// Tests update_class_from_population - growing to S-class.
    /// </summary>
    public static void TestUpdateClassGrowthToS()
    {
        SpaceStation station = new SpaceStation();
        station.StationClass = StationClass.Class.O;
        station.Population = 2000000;

        station.UpdateClassFromPopulation();

        DotNetNativeTestSuite.AssertEqual(StationClass.Class.S, station.StationClass, "Should grow to S class");
    }

    /// <summary>
    /// Tests update_class_from_population - shrinking preserves data.
    /// </summary>
    public static void TestUpdateClassShrinkPreservesData()
    {
        SpaceStation station = CreateLargeStation();
        DotNetNativeTestSuite.AssertNotNull(station.Government, "Government should exist initially");
        DotNetNativeTestSuite.AssertNotNull(station.History, "History should exist initially");

        station.Population = 5000;
        station.UpdateClassFromPopulation();

        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, station.StationClass, "Should shrink to O class");
        DotNetNativeTestSuite.AssertNotNull(station.Government, "Government should be preserved");
        DotNetNativeTestSuite.AssertNotNull(station.History, "History should be preserved");
    }

    /// <summary>
    /// Tests uses_outpost_government.
    /// </summary>
    public static void TestUsesOutpostGovernment()
    {
        SpaceStation station = new SpaceStation();

        station.StationClass = StationClass.Class.U;
        DotNetNativeTestSuite.AssertTrue(station.UsesOutpostGovernment(), "U class should use outpost government");

        station.StationClass = StationClass.Class.O;
        DotNetNativeTestSuite.AssertTrue(station.UsesOutpostGovernment(), "O class should use outpost government");

        station.StationClass = StationClass.Class.B;
        DotNetNativeTestSuite.AssertFalse(station.UsesOutpostGovernment(), "B class should not use outpost government");
    }

    /// <summary>
    /// Tests uses_colony_government.
    /// </summary>
    public static void TestUsesColonyGovernment()
    {
        SpaceStation station = new SpaceStation();

        station.StationClass = StationClass.Class.O;
        DotNetNativeTestSuite.AssertFalse(station.UsesColonyGovernment(), "O class should not use colony government");

        station.StationClass = StationClass.Class.B;
        DotNetNativeTestSuite.AssertTrue(station.UsesColonyGovernment(), "B class should use colony government");

        station.StationClass = StationClass.Class.A;
        DotNetNativeTestSuite.AssertTrue(station.UsesColonyGovernment(), "A class should use colony government");

        station.StationClass = StationClass.Class.S;
        DotNetNativeTestSuite.AssertTrue(station.UsesColonyGovernment(), "S class should use colony government");
    }

    /// <summary>
    /// Tests get_regime.
    /// </summary>
    public static void TestGetRegime()
    {
        SpaceStation station = CreateLargeStation();
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Constitutional, station.GetRegime(), "Should return Constitutional");

        SpaceStation small = CreateSmallStation();
        DotNetNativeTestSuite.AssertEqual(GovernmentType.Regime.Constitutional, small.GetRegime(), "Small stations should return default regime");
    }

    /// <summary>
    /// Tests is_politically_stable.
    /// </summary>
    public static void TestIsPoliticallyStable()
    {
        SpaceStation station = CreateLargeStation();
        station.Government.Legitimacy = 0.85f;
        DotNetNativeTestSuite.AssertTrue(station.IsPoliticallyStable(), "High legitimacy should be stable");

        station.Government.Legitimacy = 0.1f;
        DotNetNativeTestSuite.AssertFalse(station.IsPoliticallyStable(), "Low legitimacy should be unstable");

        SpaceStation small = CreateSmallStation();
        DotNetNativeTestSuite.AssertTrue(small.IsPoliticallyStable(), "Small stations should always be stable");
    }

    /// <summary>
    /// Tests service management.
    /// </summary>
    public static void TestServiceManagement()
    {
        SpaceStation station = new SpaceStation();
        station.Services = new Array<StationService.Service>();

        DotNetNativeTestSuite.AssertFalse(station.OffersService(StationService.Service.Shipyard), "Should not offer Shipyard initially");

        station.AddService(StationService.Service.Shipyard);
        DotNetNativeTestSuite.AssertTrue(station.OffersService(StationService.Service.Shipyard), "Should offer Shipyard after adding");

        station.AddService(StationService.Service.Shipyard);
        DotNetNativeTestSuite.AssertEqual(1, station.Services.Count, "Adding duplicate should not increase count");

        station.RemoveService(StationService.Service.Shipyard);
        DotNetNativeTestSuite.AssertFalse(station.OffersService(StationService.Service.Shipyard), "Should not offer Shipyard after removing");
    }

    /// <summary>
    /// Tests set_population with class update.
    /// </summary>
    public static void TestSetPopulation()
    {
        SpaceStation station = new SpaceStation();
        station.StationClass = StationClass.Class.O;

        station.SetPopulation(5000);
        DotNetNativeTestSuite.AssertEqual(5000, station.Population, "Should set to 5000");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, station.StationClass, "Should remain O class");

        station.SetPopulation(200000);
        DotNetNativeTestSuite.AssertEqual(200000, station.Population, "Should set to 200000");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.A, station.StationClass, "Should grow to A class");

        station.SetPopulation(-100);
        DotNetNativeTestSuite.AssertEqual(0, station.Population, "Should clamp negative to 0");
    }

    /// <summary>
    /// Tests update_peak_population.
    /// </summary>
    public static void TestUpdatePeakPopulation()
    {
        SpaceStation station = CreateLargeStation();
        station.Population = 700000;
        station.PeakPopulation = 600000;

        station.UpdatePeakPopulation(-5);

        DotNetNativeTestSuite.AssertEqual(700000, station.PeakPopulation, "Peak should update to 700000");
        DotNetNativeTestSuite.AssertEqual(-5, station.PeakPopulationYear, "Peak year should be -5");
    }

    /// <summary>
    /// Tests get_growth_state.
    /// </summary>
    public static void TestGetGrowthState()
    {
        SpaceStation station = CreateLargeStation();

        station.Population = 600000;
        station.PeakPopulation = 600000;
        DotNetNativeTestSuite.AssertEqual("growing", station.GetGrowthState(), "Should be growing");

        station.Population = 400000;
        DotNetNativeTestSuite.AssertEqual("stable", station.GetGrowthState(), "Should be stable");

        station.Population = 200000;
        DotNetNativeTestSuite.AssertEqual("declining", station.GetGrowthState(), "Should be declining");

        station.IsOperational = false;
        DotNetNativeTestSuite.AssertEqual("abandoned", station.GetGrowthState(), "Should be abandoned");
    }

    /// <summary>
    /// Tests record_decommissioning.
    /// </summary>
    public static void TestRecordDecommissioning()
    {
        SpaceStation station = CreateSmallStation();
        DotNetNativeTestSuite.AssertTrue(station.IsOperational, "Should be operational initially");

        station.RecordDecommissioning(-10, "Structural failure");

        DotNetNativeTestSuite.AssertFalse(station.IsOperational, "Should not be operational after decommissioning");
        DotNetNativeTestSuite.AssertEqual(-10, station.DecommissionedYear, "DecommissionedYear should be -10");
        DotNetNativeTestSuite.AssertEqual("Structural failure", station.DecommissionedReason, "DecommissionedReason should match");
    }

    /// <summary>
    /// Tests record_independence.
    /// </summary>
    public static void TestRecordIndependence()
    {
        SpaceStation station = CreateLargeStation();
        DotNetNativeTestSuite.AssertFalse(station.IsIndependent, "Should not be independent initially");

        station.RecordIndependence(-30);

        DotNetNativeTestSuite.AssertTrue(station.IsIndependent, "Should be independent");
        DotNetNativeTestSuite.AssertEqual(-30, station.IndependenceYear, "IndependenceYear should be -30");
    }

    /// <summary>
    /// Tests get_summary for small station.
    /// </summary>
    public static void TestGetSummarySmall()
    {
        SpaceStation station = CreateSmallStation();
        Godot.Collections.Dictionary summary = station.GetSummary();

        DotNetNativeTestSuite.AssertEqual("station_001", summary["id"].AsString(), "ID should match");
        DotNetNativeTestSuite.AssertEqual("O", summary["class"].AsString(), "Class should be O");
        DotNetNativeTestSuite.AssertEqual(5000, summary["population"].AsInt32(), "Population should be 5000");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("authority"), "Should have authority");
        DotNetNativeTestSuite.AssertFalse(summary.ContainsKey("regime"), "Should not have regime");
    }

    /// <summary>
    /// Tests get_summary for large station.
    /// </summary>
    public static void TestGetSummaryLarge()
    {
        SpaceStation station = CreateLargeStation();
        Godot.Collections.Dictionary summary = station.GetSummary();

        DotNetNativeTestSuite.AssertEqual("station_002", summary["id"].AsString(), "ID should match");
        DotNetNativeTestSuite.AssertEqual("A", summary["class"].AsString(), "Class should be A");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("regime"), "Should have regime");
        DotNetNativeTestSuite.AssertTrue(summary.ContainsKey("is_independent"), "Should have is_independent");
        DotNetNativeTestSuite.AssertFalse(summary.ContainsKey("authority"), "Should not have authority");
    }

    /// <summary>
    /// Tests validation - valid small station.
    /// </summary>
    public static void TestValidationValidSmall()
    {
        SpaceStation station = CreateSmallStation();
        DotNetNativeTestSuite.AssertTrue(station.IsValid(), "Small station should be valid");
    }

    /// <summary>
    /// Tests validation - valid large station.
    /// </summary>
    public static void TestValidationValidLarge()
    {
        SpaceStation station = CreateLargeStation();
        DotNetNativeTestSuite.AssertTrue(station.IsValid(), "Large station should be valid");
    }

    /// <summary>
    /// Tests validation - missing ID.
    /// </summary>
    public static void TestValidationMissingId()
    {
        SpaceStation station = CreateSmallStation();
        station.Id = "";

        Array<string> errors = station.Validate();
        DotNetNativeTestSuite.AssertGreaterThan(errors.Count, 0, "Should have errors");
        DotNetNativeTestSuite.AssertTrue(errors[0].Contains("ID is required"), "Should have ID error");
    }

    /// <summary>
    /// Tests validation - class mismatch.
    /// </summary>
    public static void TestValidationClassMismatch()
    {
        SpaceStation station = new SpaceStation();
        station.Id = "test";
        station.StationType = StationType.Type.DeepSpace;
        station.StationClass = StationClass.Class.A;
        station.Population = 5000;

        Array<string> errors = station.Validate();
        DotNetNativeTestSuite.AssertGreaterThan(errors.Count, 0, "Should have errors");

        bool foundClassError = false;
        foreach (string error in errors)
        {
            if (error.Contains("does not match"))
            {
                foundClassError = true;
                break;
            }
        }
        DotNetNativeTestSuite.AssertTrue(foundClassError, "Should have class mismatch error");
    }

    /// <summary>
    /// Tests validation - large station without government.
    /// </summary>
    public static void TestValidationLargeWithoutGovernment()
    {
        SpaceStation station = new SpaceStation();
        station.Id = "test";
        station.StationType = StationType.Type.DeepSpace;
        station.StationClass = StationClass.Class.B;
        station.Population = 50000;
        station.Government = null;

        Array<string> errors = station.Validate();
        DotNetNativeTestSuite.AssertGreaterThan(errors.Count, 0, "Should have errors");

        bool foundGovError = false;
        foreach (string error in errors)
        {
            if (error.Contains("government"))
            {
                foundGovError = true;
                break;
            }
        }
        DotNetNativeTestSuite.AssertTrue(foundGovError, "Should have government error");
    }

    /// <summary>
    /// Tests serialization round-trip for small station.
    /// </summary>
    public static void TestSerializationSmallStation()
    {
        SpaceStation original = CreateSmallStation();
        original.SecondaryPurposes = new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Trade };
        original.Metadata = new Godot.Collections.Dictionary { { "custom", "value" } };

        Godot.Collections.Dictionary data = original.ToDictionary();
        SpaceStation restored = SpaceStation.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Id, restored.Id, "ID should match");
        DotNetNativeTestSuite.AssertEqual(original.Name, restored.Name, "Name should match");
        DotNetNativeTestSuite.AssertEqual(original.StationClass, restored.StationClass, "StationClass should match");
        DotNetNativeTestSuite.AssertEqual(original.StationType, restored.StationType, "StationType should match");
        DotNetNativeTestSuite.AssertEqual(original.PrimaryPurpose, restored.PrimaryPurpose, "PrimaryPurpose should match");
        DotNetNativeTestSuite.AssertEqual(original.SecondaryPurposes.Count, restored.SecondaryPurposes.Count, "SecondaryPurposes count should match");
        DotNetNativeTestSuite.AssertEqual(original.Services.Count, restored.Services.Count, "Services count should match");
        DotNetNativeTestSuite.AssertEqual(original.OutpostAuthority, restored.OutpostAuthority, "OutpostAuthority should match");
        DotNetNativeTestSuite.AssertEqual(original.ParentOrganizationId, restored.ParentOrganizationId, "ParentOrganizationId should match");
        DotNetNativeTestSuite.AssertEqual(original.Population, restored.Population, "Population should match");
        DotNetNativeTestSuite.AssertEqual(original.EstablishedYear, restored.EstablishedYear, "EstablishedYear should match");
        DotNetNativeTestSuite.AssertEqual("value", restored.Metadata["custom"].AsString(), "Metadata should match");
    }

    /// <summary>
    /// Tests serialization round-trip for large station.
    /// </summary>
    public static void TestSerializationLargeStation()
    {
        SpaceStation original = CreateLargeStation();

        Godot.Collections.Dictionary data = original.ToDictionary();
        SpaceStation restored = SpaceStation.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Id, restored.Id, "ID should match");
        DotNetNativeTestSuite.AssertEqual(original.StationClass, restored.StationClass, "StationClass should match");
        DotNetNativeTestSuite.AssertEqual(original.Population, restored.Population, "Population should match");
        DotNetNativeTestSuite.AssertEqual(original.PeakPopulation, restored.PeakPopulation, "PeakPopulation should match");
        DotNetNativeTestSuite.AssertEqual(original.FoundingCivilizationId, restored.FoundingCivilizationId, "FoundingCivilizationId should match");

        DotNetNativeTestSuite.AssertNotNull(restored.Government, "Government should be restored");
        DotNetNativeTestSuite.AssertEqual(original.Government.Regime, restored.Government.Regime, "Regime should match");
        DotNetNativeTestSuite.AssertFloatEqual(original.Government.Legitimacy, restored.Government.Legitimacy, 0.001f, "Legitimacy should match");

        DotNetNativeTestSuite.AssertNotNull(restored.History, "History should be restored");
        DotNetNativeTestSuite.AssertEqual(original.History.Size(), restored.History.Size(), "History size should match");
    }

    /// <summary>
    /// Tests serialization of decommissioned station.
    /// </summary>
    public static void TestSerializationDecommissioned()
    {
        SpaceStation original = CreateSmallStation();
        original.RecordDecommissioning(-10, "Destroyed");

        Godot.Collections.Dictionary data = original.ToDictionary();
        SpaceStation restored = SpaceStation.FromDictionary(data);

        DotNetNativeTestSuite.AssertFalse(restored.IsOperational, "Should not be operational");
        DotNetNativeTestSuite.AssertEqual(-10, restored.DecommissionedYear, "DecommissionedYear should be -10");
        DotNetNativeTestSuite.AssertEqual("Destroyed", restored.DecommissionedReason, "DecommissionedReason should match");
    }

    /// <summary>
    /// Tests serialization of independent station.
    /// </summary>
    public static void TestSerializationIndependent()
    {
        SpaceStation original = CreateLargeStation();
        original.RecordIndependence(-50);

        Godot.Collections.Dictionary data = original.ToDictionary();
        SpaceStation restored = SpaceStation.FromDictionary(data);

        DotNetNativeTestSuite.AssertTrue(restored.IsIndependent, "Should be independent");
        DotNetNativeTestSuite.AssertEqual(-50, restored.IndependenceYear, "IndependenceYear should be -50");
    }

    /// <summary>
    /// Tests create_orbital factory.
    /// </summary>
    public static void TestCreateOrbital()
    {
        SpaceStation station = SpaceStation.CreateOrbital("orb001", "Orbital One", "sys001", "planet001");

        DotNetNativeTestSuite.AssertEqual("orb001", station.Id, "ID should be orb001");
        DotNetNativeTestSuite.AssertEqual("Orbital One", station.Name, "Name should be Orbital One");
        DotNetNativeTestSuite.AssertEqual("sys001", station.SystemId, "SystemId should be sys001");
        DotNetNativeTestSuite.AssertEqual("planet001", station.OrbitingBodyId, "OrbitingBodyId should be planet001");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.Orbital, station.StationType, "Type should be Orbital");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.ColonyWorld, station.PlacementContext, "Context should be ColonyWorld");
        DotNetNativeTestSuite.AssertGreaterThan(station.Services.Count, 0, "Should have services");
    }

    /// <summary>
    /// Tests create_deep_space factory.
    /// </summary>
    public static void TestCreateDeepSpace()
    {
        SpaceStation station = SpaceStation.CreateDeepSpace("ds001", "Deep Station", "sys001");

        DotNetNativeTestSuite.AssertEqual("ds001", station.Id, "ID should be ds001");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.DeepSpace, station.StationType, "Type should be DeepSpace");
        DotNetNativeTestSuite.AssertEqual("", station.OrbitingBodyId, "OrbitingBodyId should be empty");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.ResourceSystem, station.PlacementContext, "Context should be ResourceSystem");
    }

    /// <summary>
    /// Tests utility station class selection.
    /// </summary>
    public static void TestUtilityClassSelection()
    {
        SpaceStation station = new SpaceStation();
        station.PrimaryPurpose = StationPurpose.Purpose.Utility;
        station.Population = 5000;

        station.UpdateClassFromPopulation();

        DotNetNativeTestSuite.AssertEqual(StationClass.Class.U, station.StationClass, "Utility purpose should result in U class");
    }
}
