#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for Outpost data model.
/// </summary>
public static class TestOutpost
{
    /// <summary>
    /// Creates a test outpost.
    /// </summary>
    private static Outpost CreateTestOutpost()
    {
        Outpost outpost = new Outpost();
        outpost.Id = "outpost_001";
        outpost.Name = "Waypoint Alpha";
        outpost.StationClass = StationClass.Class.U;
        outpost.StationType = StationType.Type.DeepSpace;
        outpost.PrimaryPurpose = StationPurpose.Purpose.Utility;
        outpost.PlacementContext = StationPlacementContext.Context.BridgeSystem;
        outpost.Authority = OutpostAuthority.Type.Franchise;
        outpost.ParentOrganizationId = "corp_001";
        outpost.ParentOrganizationName = "StarWay Services";
        outpost.Population = 150;
        outpost.EstablishedYear = -50;
        outpost.SystemId = "system_001";
        outpost.Services = new Array<StationService.Service> { StationService.Service.Refuel, StationService.Service.Repair, StationService.Service.Lodging };
        return outpost;
    }

    /// <summary>
    /// Tests default creation.
    /// </summary>
    public static void TestCreationDefault()
    {
        Outpost outpost = new Outpost();
        DotNetNativeTestSuite.AssertEqual("", outpost.Id, "Default ID should be empty");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, outpost.StationClass, "Default class should be O");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.Orbital, outpost.StationType, "Default type should be Orbital");
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Utility, outpost.PrimaryPurpose, "Default purpose should be Utility");
        DotNetNativeTestSuite.AssertEqual(0, outpost.Population, "Default population should be 0");
        DotNetNativeTestSuite.AssertTrue(outpost.IsOperational, "Default should be operational");
        DotNetNativeTestSuite.AssertNotEqual("", outpost.CommanderTitle, "Should have commander title");
    }

    /// <summary>
    /// Tests get_age for operational outpost.
    /// </summary>
    public static void TestGetAgeOperational()
    {
        Outpost outpost = CreateTestOutpost();
        outpost.EstablishedYear = -50;

        int age = outpost.GetAge(0);
        DotNetNativeTestSuite.AssertEqual(50, age, "Age should be 50");
    }

    /// <summary>
    /// Tests get_age for decommissioned outpost.
    /// </summary>
    public static void TestGetAgeDecommissioned()
    {
        Outpost outpost = CreateTestOutpost();
        outpost.EstablishedYear = -100;
        outpost.IsOperational = false;
        outpost.DecommissionedYear = -20;

        int age = outpost.GetAge(0);
        DotNetNativeTestSuite.AssertEqual(80, age, "Age should be 80");
    }

    /// <summary>
    /// Tests is_utility.
    /// </summary>
    public static void TestIsUtility()
    {
        Outpost outpost = CreateTestOutpost();
        outpost.StationClass = StationClass.Class.U;
        DotNetNativeTestSuite.AssertTrue(outpost.IsUtility(), "U class should be utility");

        outpost.StationClass = StationClass.Class.O;
        DotNetNativeTestSuite.AssertFalse(outpost.IsUtility(), "O class should not be utility");
    }

    /// <summary>
    /// Tests is_body_associated.
    /// </summary>
    public static void TestIsBodyAssociated()
    {
        Outpost outpost = CreateTestOutpost();
        outpost.StationType = StationType.Type.DeepSpace;
        DotNetNativeTestSuite.AssertFalse(outpost.IsBodyAssociated(), "Deep space should not be body associated");

        outpost.StationType = StationType.Type.Orbital;
        outpost.OrbitingBodyId = "";
        DotNetNativeTestSuite.AssertFalse(outpost.IsBodyAssociated(), "Orbital without body ID should not be body associated");

        outpost.OrbitingBodyId = "planet_001";
        DotNetNativeTestSuite.AssertTrue(outpost.IsBodyAssociated(), "Orbital with body ID should be body associated");
    }

    /// <summary>
    /// Tests has_parent_organization.
    /// </summary>
    public static void TestHasParentOrganization()
    {
        Outpost outpost = CreateTestOutpost();
        outpost.Authority = OutpostAuthority.Type.Corporate;
        outpost.ParentOrganizationId = "corp_001";
        DotNetNativeTestSuite.AssertTrue(outpost.HasParentOrganization(), "Corporate with org ID should have parent");

        outpost.Authority = OutpostAuthority.Type.Independent;
        DotNetNativeTestSuite.AssertFalse(outpost.HasParentOrganization(), "Independent should not have parent");

        outpost.Authority = OutpostAuthority.Type.Corporate;
        outpost.ParentOrganizationId = "";
        DotNetNativeTestSuite.AssertFalse(outpost.HasParentOrganization(), "Corporate without org ID should not have parent");
    }

    /// <summary>
    /// Tests service management.
    /// </summary>
    public static void TestServiceManagement()
    {
        Outpost outpost = new Outpost();
        outpost.Services = new Array<StationService.Service>();

        DotNetNativeTestSuite.AssertFalse(outpost.OffersService(StationService.Service.Refuel), "Should not offer Refuel initially");

        outpost.AddService(StationService.Service.Refuel);
        DotNetNativeTestSuite.AssertTrue(outpost.OffersService(StationService.Service.Refuel), "Should offer Refuel after adding");
        DotNetNativeTestSuite.AssertEqual(1, outpost.Services.Count, "Should have 1 service");

        outpost.AddService(StationService.Service.Refuel);
        DotNetNativeTestSuite.AssertEqual(1, outpost.Services.Count, "Adding duplicate should not increase count");

        outpost.RemoveService(StationService.Service.Refuel);
        DotNetNativeTestSuite.AssertFalse(outpost.OffersService(StationService.Service.Refuel), "Should not offer Refuel after removing");
        DotNetNativeTestSuite.AssertEqual(0, outpost.Services.Count, "Should have 0 services");
    }

    /// <summary>
    /// Tests set_population clamping.
    /// </summary>
    public static void TestSetPopulationClamping()
    {
        Outpost outpost = new Outpost();

        outpost.SetPopulation(5000);
        DotNetNativeTestSuite.AssertEqual(5000, outpost.Population, "Should set to 5000");

        outpost.SetPopulation(15000);
        DotNetNativeTestSuite.AssertEqual(Outpost.MaxPopulation, outpost.Population, "Should clamp to max");

        outpost.SetPopulation(-100);
        DotNetNativeTestSuite.AssertEqual(0, outpost.Population, "Should clamp negative to 0");
    }

    /// <summary>
    /// Tests record_decommissioning.
    /// </summary>
    public static void TestRecordDecommissioning()
    {
        Outpost outpost = CreateTestOutpost();
        DotNetNativeTestSuite.AssertTrue(outpost.IsOperational, "Should be operational initially");

        outpost.RecordDecommissioning(-10, "Resource depletion");

        DotNetNativeTestSuite.AssertFalse(outpost.IsOperational, "Should not be operational after decommissioning");
        DotNetNativeTestSuite.AssertEqual(-10, outpost.DecommissionedYear, "Decommissioned year should be -10");
        DotNetNativeTestSuite.AssertEqual("Resource depletion", outpost.DecommissionedReason, "Decommissioned reason should match");
    }

    /// <summary>
    /// Tests update_commander_title.
    /// </summary>
    public static void TestUpdateCommanderTitle()
    {
        Outpost outpost = new Outpost();

        outpost.Authority = OutpostAuthority.Type.Military;
        outpost.UpdateCommanderTitle();
        DotNetNativeTestSuite.AssertEqual("Base Commander", outpost.CommanderTitle, "Military should have Base Commander");

        outpost.Authority = OutpostAuthority.Type.Corporate;
        outpost.UpdateCommanderTitle();
        DotNetNativeTestSuite.AssertEqual("Station Manager", outpost.CommanderTitle, "Corporate should have Station Manager");
    }

    /// <summary>
    /// Tests get_summary.
    /// </summary>
    public static void TestGetSummary()
    {
        Outpost outpost = CreateTestOutpost();
        Godot.Collections.Dictionary summary = outpost.GetSummary();

        DotNetNativeTestSuite.AssertEqual("outpost_001", summary["id"].AsString(), "ID should match");
        DotNetNativeTestSuite.AssertEqual("Waypoint Alpha", summary["name"].AsString(), "Name should match");
        DotNetNativeTestSuite.AssertEqual("U", summary["class"].AsString(), "Class should be U");
        DotNetNativeTestSuite.AssertEqual(150, summary["population"].AsInt32(), "Population should be 150");
        DotNetNativeTestSuite.AssertTrue(summary["is_operational"].AsBool(), "Should be operational");
    }

    /// <summary>
    /// Tests validation - valid outpost.
    /// </summary>
    public static void TestValidationValid()
    {
        Outpost outpost = CreateTestOutpost();
        DotNetNativeTestSuite.AssertTrue(outpost.IsValid(), "Should be valid");
        DotNetNativeTestSuite.AssertEqual(0, outpost.Validate().Count, "Should have no errors");
    }

    /// <summary>
    /// Tests validation - missing ID.
    /// </summary>
    public static void TestValidationMissingId()
    {
        Outpost outpost = CreateTestOutpost();
        outpost.Id = "";

        Array<string> errors = outpost.Validate();
        DotNetNativeTestSuite.AssertGreaterThan(errors.Count, 0, "Should have errors");
        DotNetNativeTestSuite.AssertTrue(errors[0].Contains("ID is required"), "Should have ID error");
    }

    /// <summary>
    /// Tests validation - population over max.
    /// </summary>
    public static void TestValidationPopulationOverMax()
    {
        Outpost outpost = CreateTestOutpost();
        outpost.Population = 20000;

        Array<string> errors = outpost.Validate();
        DotNetNativeTestSuite.AssertGreaterThan(errors.Count, 0, "Should have errors");
        DotNetNativeTestSuite.AssertTrue(errors[0].Contains("exceeds"), "Should have exceeds error");
    }

    /// <summary>
    /// Tests validation - wrong class.
    /// </summary>
    public static void TestValidationWrongClass()
    {
        Outpost outpost = CreateTestOutpost();
        outpost.StationClass = StationClass.Class.A;

        Array<string> errors = outpost.Validate();
        DotNetNativeTestSuite.AssertGreaterThan(errors.Count, 0, "Should have errors");
        DotNetNativeTestSuite.AssertTrue(errors[0].Contains("U or O class"), "Should have class error");
    }

    /// <summary>
    /// Tests validation - orbital without body.
    /// </summary>
    public static void TestValidationOrbitalWithoutBody()
    {
        Outpost outpost = CreateTestOutpost();
        outpost.StationType = StationType.Type.Orbital;
        outpost.OrbitingBodyId = "";

        Array<string> errors = outpost.Validate();
        DotNetNativeTestSuite.AssertGreaterThan(errors.Count, 0, "Should have errors");
        DotNetNativeTestSuite.AssertTrue(errors[0].Contains("orbiting_body_id"), "Should have orbiting body error");
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestSerializationRoundTrip()
    {
        Outpost original = CreateTestOutpost();
        original.SecondaryPurposes = new Array<StationPurpose.Purpose> { StationPurpose.Purpose.Trade };
        original.Metadata = new Godot.Collections.Dictionary { { "custom_key", "custom_value" } };

        Godot.Collections.Dictionary data = original.ToDictionary();
        Outpost restored = Outpost.FromDictionary(data);

        DotNetNativeTestSuite.AssertEqual(original.Id, restored.Id, "ID should match");
        DotNetNativeTestSuite.AssertEqual(original.Name, restored.Name, "Name should match");
        DotNetNativeTestSuite.AssertEqual(original.StationClass, restored.StationClass, "StationClass should match");
        DotNetNativeTestSuite.AssertEqual(original.StationType, restored.StationType, "StationType should match");
        DotNetNativeTestSuite.AssertEqual(original.PrimaryPurpose, restored.PrimaryPurpose, "PrimaryPurpose should match");
        DotNetNativeTestSuite.AssertEqual(original.SecondaryPurposes.Count, restored.SecondaryPurposes.Count, "SecondaryPurposes count should match");
        DotNetNativeTestSuite.AssertEqual(original.Services.Count, restored.Services.Count, "Services count should match");
        DotNetNativeTestSuite.AssertEqual(original.PlacementContext, restored.PlacementContext, "PlacementContext should match");
        DotNetNativeTestSuite.AssertEqual(original.Authority, restored.Authority, "Authority should match");
        DotNetNativeTestSuite.AssertEqual(original.ParentOrganizationId, restored.ParentOrganizationId, "ParentOrganizationId should match");
        DotNetNativeTestSuite.AssertEqual(original.Population, restored.Population, "Population should match");
        DotNetNativeTestSuite.AssertEqual(original.EstablishedYear, restored.EstablishedYear, "EstablishedYear should match");
        DotNetNativeTestSuite.AssertEqual(original.SystemId, restored.SystemId, "SystemId should match");
        DotNetNativeTestSuite.AssertEqual(original.IsOperational, restored.IsOperational, "IsOperational should match");
        DotNetNativeTestSuite.AssertEqual("custom_value", restored.Metadata["custom_key"].AsString(), "Metadata should match");
    }

    /// <summary>
    /// Tests decommissioned outpost serialization.
    /// </summary>
    public static void TestDecommissionedSerialization()
    {
        Outpost original = CreateTestOutpost();
        original.RecordDecommissioning(-10, "Abandoned");

        Godot.Collections.Dictionary data = original.ToDictionary();
        Outpost restored = Outpost.FromDictionary(data);

        DotNetNativeTestSuite.AssertFalse(restored.IsOperational, "Should not be operational");
        DotNetNativeTestSuite.AssertEqual(-10, restored.DecommissionedYear, "DecommissionedYear should be -10");
        DotNetNativeTestSuite.AssertEqual("Abandoned", restored.DecommissionedReason, "DecommissionedReason should match");
    }

    /// <summary>
    /// Tests create_utility factory.
    /// </summary>
    public static void TestCreateUtility()
    {
        Outpost outpost = Outpost.CreateUtility("u001", "Pit Stop", "sys001");

        DotNetNativeTestSuite.AssertEqual("u001", outpost.Id, "ID should be u001");
        DotNetNativeTestSuite.AssertEqual("Pit Stop", outpost.Name, "Name should be Pit Stop");
        DotNetNativeTestSuite.AssertEqual("sys001", outpost.SystemId, "SystemId should be sys001");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.U, outpost.StationClass, "Class should be U");
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Utility, outpost.PrimaryPurpose, "Purpose should be Utility");
        DotNetNativeTestSuite.AssertEqual(StationPlacementContext.Context.BridgeSystem, outpost.PlacementContext, "Context should be BridgeSystem");
        DotNetNativeTestSuite.AssertGreaterThan(outpost.Services.Count, 0, "Should have services");
    }

    /// <summary>
    /// Tests create_mining factory.
    /// </summary>
    public static void TestCreateMining()
    {
        Outpost outpost = Outpost.CreateMining("m001", "Rock Crusher", "sys001", "asteroid001");

        DotNetNativeTestSuite.AssertEqual("m001", outpost.Id, "ID should be m001");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, outpost.StationClass, "Class should be O");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.Orbital, outpost.StationType, "Type should be Orbital");
        DotNetNativeTestSuite.AssertEqual("asteroid001", outpost.OrbitingBodyId, "OrbitingBodyId should be asteroid001");
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Mining, outpost.PrimaryPurpose, "Purpose should be Mining");
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Corporate, outpost.Authority, "Authority should be Corporate");
    }

    /// <summary>
    /// Tests create_science factory.
    /// </summary>
    public static void TestCreateScience()
    {
        Outpost outpost = Outpost.CreateScience("s001", "Deep Space Lab", "sys001");

        DotNetNativeTestSuite.AssertEqual("s001", outpost.Id, "ID should be s001");
        DotNetNativeTestSuite.AssertEqual(StationClass.Class.O, outpost.StationClass, "Class should be O");
        DotNetNativeTestSuite.AssertEqual(StationType.Type.DeepSpace, outpost.StationType, "Type should be DeepSpace");
        DotNetNativeTestSuite.AssertEqual(StationPurpose.Purpose.Science, outpost.PrimaryPurpose, "Purpose should be Science");
        DotNetNativeTestSuite.AssertEqual(OutpostAuthority.Type.Government, outpost.Authority, "Authority should be Government");
    }
}
