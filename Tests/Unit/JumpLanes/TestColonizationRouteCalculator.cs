#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Jumplanes;
using StarGen.Domain.Population;

namespace StarGen.Tests.Unit.JumpLanes;

/// <summary>
/// Tests for ColonizationRouteCalculator.
/// </summary>
public static class TestColonizationRouteCalculator
{
    /// <summary>
    /// Tests that a strong interstellar exporter colonizes a nearby viable empty system.
    /// </summary>
    public static void TestInterstellarExporterColonizesNearbyViableTarget()
    {
        ColonizationRouteCalculator calculator = new();
        JumpLaneRegion region = new();
        JumpLaneSystem exporter = MakeSystem("1001", 0.0, 0.0, 0.0, 900000, true, 1.0, 0.4, 200000, 10.0, (int)TechnologyLevel.Level.Advanced);
        JumpLaneSystem target = MakeSystem("2001", 4.0, 0.0, 0.0, 0, false, 0.0, 1.0, 1500000, 0.0, -1);
        JumpLaneSystem distant = MakeSystem("2002", 20.0, 0.0, 0.0, 0, false, 0.0, 1.0, 2000000, 0.0, -1);
        region.AddSystem(exporter);
        region.AddSystem(target);
        region.AddSystem(distant);

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 1)
        {
            throw new InvalidOperationException($"Expected 1 connection, got {result.GetTotalConnections()}");
        }

        JumpLaneConnection connection = result.Connections[0];
        if (connection.SourceId != "1001" || connection.DestinationId != "2001")
        {
            throw new InvalidOperationException("Expected the exporter to connect directly to the nearby viable target");
        }

        if (connection.Type != JumpLaneConnection.ConnectionType.Green)
        {
            throw new InvalidOperationException($"Expected green direct colonization route, got {connection.Type}");
        }

        JumpLaneSystem? colonizedTarget = result.GetSystem("2001");
        if (colonizedTarget == null)
        {
            throw new InvalidOperationException("Expected colonized target to be registered");
        }

        if (colonizedTarget.Population <= 0)
        {
            throw new InvalidOperationException("Expected colonized target to gain a positive route population");
        }

        if (result.IsOrphan("1001"))
        {
            throw new InvalidOperationException("Exporter should not remain orphaned after founding a colony route");
        }

        if (!result.IsOrphan("2002"))
        {
            throw new InvalidOperationException("Unreached distant system should remain orphaned");
        }
    }

    /// <summary>
    /// Tests that systems without interstellar export capability do not form colonization routes.
    /// </summary>
    public static void TestLowTechSystemsDoNotColonize()
    {
        ColonizationRouteCalculator calculator = new();
        JumpLaneRegion region = new();
        JumpLaneSystem exporter = MakeSystem("2001", 0.0, 0.0, 0.0, 750000, true, 0.9, 0.4, 250000, 0.0, (int)TechnologyLevel.Level.Information);
        JumpLaneSystem target = MakeSystem("2002", 4.0, 0.0, 0.0, 0, false, 0.0, 0.9, 1500000, 0.0, -1);
        region.AddSystem(exporter);
        region.AddSystem(target);

        JumpLaneResult result = calculator.Calculate(region);

        if (result.GetTotalConnections() != 0)
        {
            throw new InvalidOperationException($"Expected 0 connections, got {result.GetTotalConnections()}");
        }

        if (!result.IsOrphan("2001") || !result.IsOrphan("2002"))
        {
            throw new InvalidOperationException("Both systems should remain orphaned when the source lacks interstellar capability");
        }
    }

    /// <summary>
    /// Tests that repeated calculation with the same region yields the same route network.
    /// </summary>
    public static void TestColonizationRoutesAreDeterministic()
    {
        ColonizationRouteCalculator calculator = new();
        JumpLaneRegion region = new();
        region.AddSystem(MakeSystem("3001", 0.0, 0.0, 0.0, 850000, true, 0.95, 0.3, 300000, 10.0, (int)TechnologyLevel.Level.Advanced));
        region.AddSystem(MakeSystem("3002", 3.0, 0.0, 0.0, 0, false, 0.0, 0.85, 1400000, 0.0, -1));
        region.AddSystem(MakeSystem("3003", 6.5, 0.0, 0.0, 0, false, 0.0, 0.80, 1200000, 0.0, -1));

        JumpLaneResult first = calculator.Calculate(region);
        JumpLaneResult second = calculator.Calculate(region);

        if (first.GetTotalConnections() != second.GetTotalConnections())
        {
            throw new InvalidOperationException("Deterministic colonization routing should keep connection counts stable");
        }

        if (first.GetTotalOrphans() != second.GetTotalOrphans())
        {
            throw new InvalidOperationException("Deterministic colonization routing should keep orphan counts stable");
        }

        JumpLaneSystem? firstSecondSystem = first.GetSystem("3002");
        JumpLaneSystem? secondSecondSystem = second.GetSystem("3002");
        if (firstSecondSystem == null || secondSecondSystem == null)
        {
            throw new InvalidOperationException("Expected repeated calculations to register the same systems");
        }

        if (firstSecondSystem.Population != secondSecondSystem.Population)
        {
            throw new InvalidOperationException("Deterministic colonization routing should keep colonized target populations stable");
        }
    }

    private static JumpLaneSystem MakeSystem(
        string id,
        double x,
        double y,
        double z,
        int population,
        bool canExportColonists,
        double exportPressure,
        double colonyTargetScore,
        int colonyTargetCapacity,
        double colonizationRangePc,
        int routeTechnologyLevel)
    {
        JumpLaneSystem system = new(id, new Vector3((float)x, (float)y, (float)z), population);
        system.CanExportColonists = canExportColonists;
        system.ExportPressure = exportPressure;
        system.ColonyTargetScore = colonyTargetScore;
        system.ColonyTargetCapacity = colonyTargetCapacity;
        system.ColonizationRangePc = colonizationRangePc;
        system.RouteTechnologyLevel = routeTechnologyLevel;
        return system;
    }
}
