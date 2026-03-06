#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for StationService enum and utilities.
/// </summary>
public static class TestStationService
{
    /// <summary>
    /// Tests to_string_name returns correct values.
    /// </summary>
    public static void TestToStringNameReturnsCorrectValues()
    {
        DotNetNativeTestSuite.AssertEqual("Refuel", StationService.ToStringName(StationService.Service.Refuel), "Refuel name should match");
        DotNetNativeTestSuite.AssertEqual("Repair", StationService.ToStringName(StationService.Service.Repair), "Repair name should match");
        DotNetNativeTestSuite.AssertEqual("Trade", StationService.ToStringName(StationService.Service.Trade), "Trade name should match");
        DotNetNativeTestSuite.AssertEqual("Medical", StationService.ToStringName(StationService.Service.Medical), "Medical name should match");
        DotNetNativeTestSuite.AssertEqual("Customs", StationService.ToStringName(StationService.Service.Customs), "Customs name should match");
        DotNetNativeTestSuite.AssertEqual("Entertainment", StationService.ToStringName(StationService.Service.Entertainment), "Entertainment name should match");
        DotNetNativeTestSuite.AssertEqual("Lodging", StationService.ToStringName(StationService.Service.Lodging), "Lodging name should match");
        DotNetNativeTestSuite.AssertEqual("Shipyard", StationService.ToStringName(StationService.Service.Shipyard), "Shipyard name should match");
        DotNetNativeTestSuite.AssertEqual("Banking", StationService.ToStringName(StationService.Service.Banking), "Banking name should match");
        DotNetNativeTestSuite.AssertEqual("Communications", StationService.ToStringName(StationService.Service.Communications), "Communications name should match");
        DotNetNativeTestSuite.AssertEqual("Storage", StationService.ToStringName(StationService.Service.Storage), "Storage name should match");
        DotNetNativeTestSuite.AssertEqual("Security", StationService.ToStringName(StationService.Service.Security), "Security name should match");
    }

    /// <summary>
    /// Tests from_string parses correctly.
    /// </summary>
    public static void TestFromStringParsesCorrectly()
    {
        DotNetNativeTestSuite.AssertEqual(StationService.Service.Refuel, StationService.FromString("Refuel"), "Should parse Refuel");
        DotNetNativeTestSuite.AssertEqual(StationService.Service.Repair, StationService.FromString("Repair"), "Should parse Repair");
        DotNetNativeTestSuite.AssertEqual(StationService.Service.Trade, StationService.FromString("Trade"), "Should parse Trade");
        DotNetNativeTestSuite.AssertEqual(StationService.Service.Shipyard, StationService.FromString("Shipyard"), "Should parse Shipyard");
    }

    /// <summary>
    /// Tests from_string is case insensitive.
    /// </summary>
    public static void TestFromStringIsCaseInsensitive()
    {
        DotNetNativeTestSuite.AssertEqual(StationService.Service.Refuel, StationService.FromString("REFUEL"), "Should parse REFUEL");
        DotNetNativeTestSuite.AssertEqual(StationService.Service.Repair, StationService.FromString("repair"), "Should parse repair");
    }

    /// <summary>
    /// Tests from_string returns default for unknown.
    /// </summary>
    public static void TestFromStringReturnsDefaultForUnknown()
    {
        DotNetNativeTestSuite.AssertEqual(StationService.Service.Refuel, StationService.FromString("unknown"), "Unknown should return Refuel");
        DotNetNativeTestSuite.AssertEqual(StationService.Service.Refuel, StationService.FromString(""), "Empty should return Refuel");
    }

    /// <summary>
    /// Tests basic_utility_services returns expected.
    /// </summary>
    public static void TestBasicUtilityServicesReturnsExpected()
    {
        Array<StationService.Service> services = StationService.BasicUtilityServices();
        DotNetNativeTestSuite.AssertTrue(services.Count >= 3, "Should have at least 3 basic services");
        DotNetNativeTestSuite.AssertTrue(services.Contains(StationService.Service.Refuel), "Should contain Refuel");
        DotNetNativeTestSuite.AssertTrue(services.Contains(StationService.Service.Repair), "Should contain Repair");
        DotNetNativeTestSuite.AssertTrue(services.Contains(StationService.Service.Trade), "Should contain Trade");
    }

    /// <summary>
    /// Tests advanced_services returns expected.
    /// </summary>
    public static void TestAdvancedServicesReturnsExpected()
    {
        Array<StationService.Service> services = StationService.AdvancedServices();
        DotNetNativeTestSuite.AssertTrue(services.Count > 0, "Should have advanced services");
        DotNetNativeTestSuite.AssertTrue(services.Contains(StationService.Service.Shipyard), "Should contain Shipyard");
    }

    /// <summary>
    /// Tests common_services returns expected.
    /// </summary>
    public static void TestCommonServicesReturnsExpected()
    {
        Array<StationService.Service> services = StationService.CommonServices();
        DotNetNativeTestSuite.AssertTrue(services.Count > 0, "Should have common services");
        DotNetNativeTestSuite.AssertTrue(services.Contains(StationService.Service.Refuel), "Should contain Refuel");
    }

    /// <summary>
    /// Tests requires_major_infrastructure.
    /// </summary>
    public static void TestRequiresMajorInfrastructure()
    {
        DotNetNativeTestSuite.AssertTrue(StationService.RequiresMajorInfrastructure(StationService.Service.Shipyard), "Shipyard should require major infrastructure");
        DotNetNativeTestSuite.AssertTrue(StationService.RequiresMajorInfrastructure(StationService.Service.Banking), "Banking should require major infrastructure");
        DotNetNativeTestSuite.AssertTrue(StationService.RequiresMajorInfrastructure(StationService.Service.Entertainment), "Entertainment should require major infrastructure");
        DotNetNativeTestSuite.AssertFalse(StationService.RequiresMajorInfrastructure(StationService.Service.Refuel), "Refuel should not require major infrastructure");
        DotNetNativeTestSuite.AssertFalse(StationService.RequiresMajorInfrastructure(StationService.Service.Repair), "Repair should not require major infrastructure");
    }

    /// <summary>
    /// Tests count returns correct value.
    /// </summary>
    public static void TestCountReturnsCorrectValue()
    {
        DotNetNativeTestSuite.AssertEqual(12, StationService.Count(), "Should have 12 station services");
    }

    /// <summary>
    /// Tests roundtrip string conversion.
    /// </summary>
    public static void TestRoundtripStringConversion()
    {
        for (int i = 0; i < StationService.Count(); i += 1)
        {
            StationService.Service service = (StationService.Service)i;
            string nameStr = StationService.ToStringName(service);
            StationService.Service parsed = StationService.FromString(nameStr);
            DotNetNativeTestSuite.AssertEqual(service, parsed, $"Roundtrip failed for service {i}");
        }
    }
}
