using System;
using System.Collections.Generic;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Concepts;
using StarGen.Domain.Population;
using StarGen.Domain.Systems;

namespace StarGen.Services.Concepts;

/// <summary>
/// Populates persisted concept state on generated or loaded StarGen world objects.
/// </summary>
public static class ConceptWorldStateGenerator
{
    /// <summary>
    /// Ensures persisted concept results exist for the full system and its bodies.
    /// </summary>
    public static void EnsureSystemConcepts(SolarSystem? system, int galaxySeed = 0)
    {
        if (system == null)
        {
            return;
        }

        foreach (CelestialBody body in system.Bodies.Values)
        {
            EnsureBodyConcepts(body, system, galaxySeed);
        }

        CelestialBody? aggregateBody = SelectAggregateBody(system);
        if (aggregateBody == null)
        {
            return;
        }

        if (aggregateBody.HasConceptResults())
        {
            CopyMissingResults(system.ConceptResults, aggregateBody.ConceptResults);
        }

        if (aggregateBody.PopulationData != null && aggregateBody.PopulationData.HasConceptResults())
        {
            CopyMissingResults(system.ConceptResults, aggregateBody.PopulationData.ConceptResults);
        }

        ConceptContextSnapshot systemSnapshot = ConceptContextBuilder.FromSystem(system, ResolveSystemSeed(system));
        EnsureConcept(system.ConceptResults, ConceptKind.Civilization, systemSnapshot);
        EnsureConcept(system.ConceptResults, ConceptKind.Language, systemSnapshot);
        EnsureConcept(system.ConceptResults, ConceptKind.Religion, systemSnapshot);
        if (systemSnapshot.Population > 0)
        {
            EnsureConcept(system.ConceptResults, ConceptKind.Disease, systemSnapshot);
        }
    }

    /// <summary>
    /// Ensures persisted concept results exist for a body and any attached populations.
    /// </summary>
    public static void EnsureBodyConcepts(CelestialBody? body, SolarSystem? system = null, int galaxySeed = 0)
    {
        if (body == null)
        {
            return;
        }

        ConceptContextSnapshot bodySnapshot = ConceptContextBuilder.FromBody(body, system, galaxySeed);
        EnsureConcept(body.ConceptResults, ConceptKind.Ecology, bodySnapshot);
        EnsureConcept(body.ConceptResults, ConceptKind.Evolution, bodySnapshot);

        PlanetPopulationData? populationData = body.PopulationData;
        if (populationData == null || !populationData.IsInhabited())
        {
            return;
        }

        EnsurePopulationConcepts(body, populationData, system, galaxySeed);
        CopyMissingResults(body.ConceptResults, populationData.ConceptResults);
    }

    private static void EnsurePopulationConcepts(
        CelestialBody body,
        PlanetPopulationData populationData,
        SolarSystem? system,
        int galaxySeed)
    {
        foreach (NativePopulation nativePopulation in populationData.NativePopulations)
        {
            if (!nativePopulation.IsExtant)
            {
                continue;
            }

            ConceptContextSnapshot nativeSnapshot = ConceptContextBuilder.FromNativePopulation(body, nativePopulation, system, galaxySeed);
            EnsureConcept(nativePopulation.ConceptResults, ConceptKind.Civilization, nativeSnapshot);
            EnsureConcept(nativePopulation.ConceptResults, ConceptKind.Religion, nativeSnapshot);
            EnsureConcept(nativePopulation.ConceptResults, ConceptKind.Language, nativeSnapshot);
            EnsureConcept(nativePopulation.ConceptResults, ConceptKind.Disease, nativeSnapshot);
            EnsureHistory(nativePopulation.History, nativePopulation.ConceptResults);
        }

        foreach (Colony colony in populationData.Colonies)
        {
            if (!colony.IsActive)
            {
                continue;
            }

            ConceptContextSnapshot colonySnapshot = ConceptContextBuilder.FromColony(body, colony, system, galaxySeed);
            EnsureConcept(colony.ConceptResults, ConceptKind.Civilization, colonySnapshot);
            EnsureConcept(colony.ConceptResults, ConceptKind.Religion, colonySnapshot);
            EnsureConcept(colony.ConceptResults, ConceptKind.Language, colonySnapshot);
            EnsureConcept(colony.ConceptResults, ConceptKind.Disease, colonySnapshot);
            EnsureHistory(colony.History, colony.ConceptResults);
        }

        Variant dominant = populationData.GetDominantPopulation();
        if (dominant.VariantType == Variant.Type.Nil)
        {
            return;
        }

        if (dominant.Obj is NativePopulation dominantNative)
        {
            CopyMissingResults(populationData.ConceptResults, dominantNative.ConceptResults);
        }
        else if (dominant.Obj is Colony dominantColony)
        {
            CopyMissingResults(populationData.ConceptResults, dominantColony.ConceptResults);
        }

        ConceptContextSnapshot bodySnapshot = ConceptContextBuilder.FromBody(body, system, galaxySeed);
        EnsureConcept(populationData.ConceptResults, ConceptKind.Disease, bodySnapshot);
    }

    private static void EnsureConcept(ConceptResultStore target, ConceptKind kind, ConceptContextSnapshot snapshot)
    {
        if (target.Has(kind))
        {
            return;
        }

        ConceptRunResult result = ConceptResultFactory.Run(new ConceptRunRequest
        {
            Kind = kind,
            Context = snapshot,
        });
        target.Set(kind, result);
    }

    private static void CopyMissingResults(ConceptResultStore destination, ConceptResultStore source)
    {
        IReadOnlyDictionary<ConceptKind, ConceptRunResult> values = source.GetAll();
        foreach (KeyValuePair<ConceptKind, ConceptRunResult> entry in values)
        {
            if (!destination.Has(entry.Key))
            {
                destination.Set(entry.Key, entry.Value);
            }
        }
    }

    private static CelestialBody? SelectAggregateBody(SolarSystem system)
    {
        CelestialBody? bestBody = null;
        int bestPopulation = -1;
        int bestHabitability = -1;

        foreach (CelestialBody body in system.Bodies.Values)
        {
            int population = 0;
            int habitability = -1;
            if (body.PopulationData != null)
            {
                population = body.PopulationData.GetTotalPopulation();
                if (body.PopulationData.Profile != null)
                {
                    habitability = body.PopulationData.Profile.HabitabilityScore;
                }
            }

            if (population > bestPopulation)
            {
                bestPopulation = population;
                bestHabitability = habitability;
                bestBody = body;
                continue;
            }

            if (population == bestPopulation && habitability > bestHabitability)
            {
                bestHabitability = habitability;
                bestBody = body;
            }
        }

        if (bestBody != null)
        {
            return bestBody;
        }

        foreach (CelestialBody planet in system.GetPlanets())
        {
            return planet;
        }

        foreach (CelestialBody moon in system.GetMoons())
        {
            return moon;
        }

        foreach (CelestialBody star in system.GetStars())
        {
            return star;
        }

        return null;
    }

    private static int ResolveSystemSeed(SolarSystem system)
    {
        if (system.Provenance != null)
        {
            return (int)system.Provenance.GenerationSeed;
        }

        return StableStringHash(system.Id);
    }

    private static void EnsureHistory(PopulationHistory history, ConceptResultStore results)
    {
        EnsureHistoryEvent(history, results, ConceptKind.Civilization, HistoryEvent.EventType.PoliticalChange, 0, 0.18);
        EnsureHistoryEvent(history, results, ConceptKind.Religion, HistoryEvent.EventType.CulturalShift, 1, 0.08);
        EnsureHistoryEvent(history, results, ConceptKind.Language, HistoryEvent.EventType.CulturalShift, 2, 0.05);
        EnsureHistoryEvent(history, results, ConceptKind.Disease, HistoryEvent.EventType.Plague, 3, -0.35);
    }

    private static void EnsureHistoryEvent(
        PopulationHistory history,
        ConceptResultStore results,
        ConceptKind kind,
        HistoryEvent.EventType eventType,
        int yearOffset,
        double magnitude)
    {
        ConceptRunResult? result = results.Get(kind);
        if (result == null)
        {
            return;
        }

        foreach (HistoryEvent existing in history.GetAllEvents())
        {
            if (existing.Metadata.ContainsKey("concept_kind")
                && existing.Metadata["concept_kind"].VariantType == Variant.Type.String
                && string.Equals((string)existing.Metadata["concept_kind"], kind.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        HistoryEvent historyEvent = history.AddNewEvent(
            eventType,
            yearOffset,
            result.Title,
            result.Summary,
            magnitude);
        historyEvent.Metadata["concept_kind"] = kind.ToString();
        historyEvent.Metadata["concept_subtitle"] = result.Subtitle;
        historyEvent.Metadata["concept_seed"] = result.Provenance.Seed;
    }

    private static int StableStringHash(string value)
    {
        const uint fnvOffset = 2166136261;
        const uint fnvPrime = 16777619;
        uint hash = fnvOffset;

        foreach (char character in value)
        {
            hash ^= character;
            hash *= fnvPrime;
        }

        return unchecked((int)hash);
    }
}
