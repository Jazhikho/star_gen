using Godot.Collections;
using StarGen.Domain.Population;
using StarGen.Domain.Systems;

namespace StarGen.Domain.Colonization;

/// <summary>
/// Applies persisted colonization outcomes to a native-only generated system.
/// </summary>
public static class ColonizationSimulationOverlay
{
    private const string SimulationMarkerKey = "simulation_colonization";

    /// <summary>
    /// Rebuilds simulated colony state for a generated system.
    /// </summary>
    public static void ApplyToSystem(SolarSystem? system, int starSeed, Galaxy.Galaxy? galaxy, int currentYear = 0)
    {
        if (system == null || galaxy == null)
        {
            return;
        }

        string destinationSystemId = starSeed.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Array<ColonizationSettlementRecord> settlements = galaxy.GetColonizationSettlementsForDestination(destinationSystemId);
        ApplyToSystem(system, settlements, currentYear);
    }

    /// <summary>
    /// Rebuilds simulated colony state from explicit settlement records.
    /// </summary>
    public static void ApplyToSystem(SolarSystem? system, Array<ColonizationSettlementRecord> settlements, int currentYear = 0)
    {
        if (system == null)
        {
            return;
        }

        foreach (Celestial.CelestialBody body in system.Bodies.Values)
        {
            if (body.PopulationData == null)
            {
                continue;
            }

            ClearSimulatedColonies(body.PopulationData);
        }

        foreach (ColonizationSettlementRecord settlement in settlements)
        {
            Celestial.CelestialBody? body = system.GetBody(settlement.DestinationBodyId);
            if (body == null || body.PopulationData == null)
            {
                continue;
            }

            Colony colony = CreateColonyFromSettlement(settlement, currentYear);
            body.PopulationData.Colonies.Add(colony);
            body.PopulationData.Population = body.PopulationData.GetTotalPopulation();
            body.PopulationData.IsActive = body.PopulationData.GetTotalPopulation() > 0;
        }
    }

    private static void ClearSimulatedColonies(PlanetPopulationData populationData)
    {
        Array<Colony> retained = new Array<Colony>();
        foreach (Colony colony in populationData.Colonies)
        {
            if (!colony.Metadata.ContainsKey(SimulationMarkerKey))
            {
                retained.Add(colony);
            }
        }

        populationData.Colonies = retained;
        populationData.Population = populationData.GetTotalPopulation();
        populationData.IsActive = populationData.GetTotalPopulation() > 0;
    }

    private static Colony CreateColonyFromSettlement(ColonizationSettlementRecord settlement, int currentYear)
    {
        int foundingYear = currentYear - (settlement.Epoch * 100);
        Colony colony = new Colony
        {
            Id = $"sim_colony_{settlement.DestinationSystemId}_{settlement.DestinationBodyId}",
            Name = settlement.FoundingCivilizationName + " Colony",
            BodyId = settlement.DestinationBodyId,
            FoundingCivilizationId = settlement.FoundingCivilizationId,
            FoundingCivilizationName = settlement.FoundingCivilizationName,
            FoundingYear = foundingYear,
            Population = settlement.Population,
            PeakPopulation = settlement.Population,
            PeakPopulationYear = foundingYear,
            TechLevel = (TechnologyLevel.Level)settlement.TechnologyLevel,
            IsActive = true,
        };
        colony.Metadata[SimulationMarkerKey] = true;
        colony.Metadata["source_system_id"] = settlement.SourceSystemId;
        colony.Metadata["source_body_id"] = settlement.SourceBodyId;
        colony.Metadata["epoch"] = settlement.Epoch;
        colony.Metadata["route_distance_pc"] = settlement.RouteDistancePc;
        return colony;
    }
}
