#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Generation.Traveller;
using StarGen.Domain.Jumplanes;

namespace StarGen.Tests.Unit.JumpLanes;

/// <summary>
/// Tests for TravellerRouteCalculator.
/// </summary>
public static class TestTravellerRouteCalculator
{
    public static void TestJ1Equals2Parsecs()
    {
        TravellerRouteCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeTravellerSystem("agri", 0.0, 0.0, 0.0, 1, "Ag"));
        region.AddSystem(MakeTravellerSystem("rich", 2.0, 0.0, 0.0, 1, "Hi", "Ri"));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 Traveller route at exactly 2 pc, got {result.GetTotalConnections()}");
        }

        if (result.Connections[0].Type != JumpLaneConnection.ConnectionType.Green)
        {
            throw new InvalidOperationException("J1 Traveller route at 2 pc should be green");
        }
    }

    public static void TestBeyondJ1RangeDoesNotConnect()
    {
        TravellerRouteCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeTravellerSystem("agri", 0.0, 0.0, 0.0, 1, "Ag"));
        region.AddSystem(MakeTravellerSystem("rich", 2.1, 0.0, 0.0, 1, "Hi", "Ri"));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 0)
        {
            throw new InvalidOperationException("Traveller J1 routes should not connect past 2 pc");
        }
    }

    public static void TestJ2Uses4ParsecRangeAndTradeCodes()
    {
        TravellerRouteCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeTravellerSystem("industrial", 0.0, 0.0, 0.0, 2, "In"));
        region.AddSystem(MakeTravellerSystem("non_industrial", 4.0, 0.0, 0.0, 2, "Ni"));

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 Traveller J2 route at 4 pc, got {result.GetTotalConnections()}");
        }

        if (result.Connections[0].Type != JumpLaneConnection.ConnectionType.Orange)
        {
            throw new InvalidOperationException("Traveller J2 route at 4 pc should be orange");
        }
    }

    private static JumpLaneSystem MakeTravellerSystem(
        string id,
        double x,
        double y,
        double z,
        int maxJumpNumber,
        params string[] tradeCodes)
    {
        TravellerTradeCodeSet tradeCodeSet = new TravellerTradeCodeSet();
        foreach (string tradeCode in tradeCodes)
        {
            tradeCodeSet.AddCode(tradeCode);
        }

        JumpLaneSystem system = new(
            id,
            new Vector3((float)x, (float)y, (float)z),
            1000000);
        system.TravellerProfile = new TravellerSystemProfile
        {
            MainworldBodyId = id + "_mainworld",
            MainworldName = id,
            SelectionReason = "Fixture",
            WorldProfile = new TravellerWorldProfile
            {
                StarportCode = "B",
                SizeCode = 7,
                AtmosphereCode = 6,
                HydrographicsCode = 7,
                PopulationCode = 8,
                GovernmentCode = 4,
                LawCode = 5,
                TechLevelCode = 12,
            },
            TradeCodes = tradeCodeSet,
            RouteProfile = new TravellerRouteProfile
            {
                EstimatedPopulation = 1000000,
                PopulationCode = 8,
                StarportCode = "B",
                TechLevelCode = 12,
                Importance = 1,
                MaxJumpNumber = maxJumpNumber,
                RouteWeight = 2,
            },
        };
        return system;
    }
}
