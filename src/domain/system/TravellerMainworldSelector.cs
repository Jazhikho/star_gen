using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Population;

namespace StarGen.Domain.Systems;

/// <summary>
/// Selects a deterministic Traveller-oriented mainworld candidate from a system.
/// </summary>
public static class TravellerMainworldSelector
{
    /// <summary>
    /// Mainworld selection result with readiness details for inspector display.
    /// </summary>
    public sealed class SelectionResult
    {
        /// <summary>
        /// Selected candidate body, when present.
        /// </summary>
        public CelestialBody? Body { get; init; }

        /// <summary>
        /// Explanation of how the candidate was chosen.
        /// </summary>
        public string Reason { get; init; } = string.Empty;

        /// <summary>
        /// Total population on the selected body.
        /// </summary>
        public int Population { get; init; }

        /// <summary>
        /// Habitability score on the selected body, when available.
        /// </summary>
        public int HabitabilityScore { get; init; }

        /// <summary>
        /// Suitability score on the selected body, when available.
        /// </summary>
        public int SuitabilityScore { get; init; }

        /// <summary>
        /// Returns whether a candidate was found.
        /// </summary>
        public bool HasCandidate()
        {
            return Body != null;
        }
    }

    /// <summary>
    /// Selects the best mainworld candidate from the generated system.
    /// </summary>
    public static SelectionResult Select(SolarSystem? system)
    {
        if (system == null)
        {
            return new SelectionResult
            {
                Reason = "No system generated",
            };
        }

        List<CelestialBody> candidates = GetCandidates(system);
        if (candidates.Count == 0)
        {
            return new SelectionResult
            {
                Reason = "No planets or moons are available for mainworld selection",
            };
        }

        CelestialBody? inhabitedCandidate = SelectBest(candidates, body => GetPopulation(body) > 0);
        if (inhabitedCandidate != null)
        {
            return BuildResult(inhabitedCandidate, "Selected inhabited world with the strongest population signal");
        }

        CelestialBody? scoredCandidate = SelectBest(candidates, body => GetHabitabilityScore(body) > 0 || GetSuitabilityScore(body) > 0);
        if (scoredCandidate != null)
        {
            return BuildResult(scoredCandidate, "Selected best habitability candidate");
        }

        CelestialBody? terrestrialCandidate = SelectBest(candidates, IsTerrestrialCandidate);
        if (terrestrialCandidate != null)
        {
            return BuildResult(terrestrialCandidate, "Selected first stable terrestrial candidate");
        }

        return new SelectionResult
        {
            Reason = "No plausible mainworld candidate was found",
        };
    }

    private static List<CelestialBody> GetCandidates(SolarSystem system)
    {
        List<CelestialBody> candidates = new();
        foreach (CelestialBody planet in system.GetPlanets())
        {
            candidates.Add(planet);
        }

        foreach (CelestialBody moon in system.GetMoons())
        {
            candidates.Add(moon);
        }

        return candidates;
    }

    private static CelestialBody? SelectBest(List<CelestialBody> candidates, System.Predicate<CelestialBody> predicate)
    {
        List<CelestialBody> filtered = new();
        foreach (CelestialBody candidate in candidates)
        {
            if (predicate(candidate))
            {
                filtered.Add(candidate);
            }
        }

        if (filtered.Count == 0)
        {
            return null;
        }

        filtered.Sort(CompareCandidates);
        return filtered[0];
    }

    private static int CompareCandidates(CelestialBody left, CelestialBody right)
    {
        int populationComparison = GetPopulation(right).CompareTo(GetPopulation(left));
        if (populationComparison != 0)
        {
            return populationComparison;
        }

        int habitabilityComparison = GetHabitabilityScore(right).CompareTo(GetHabitabilityScore(left));
        if (habitabilityComparison != 0)
        {
            return habitabilityComparison;
        }

        int suitabilityComparison = GetSuitabilityScore(right).CompareTo(GetSuitabilityScore(left));
        if (suitabilityComparison != 0)
        {
            return suitabilityComparison;
        }

        int distanceComparison = GetStableDistance(left).CompareTo(GetStableDistance(right));
        if (distanceComparison != 0)
        {
            return distanceComparison;
        }

        return string.CompareOrdinal(left.Id, right.Id);
    }

    private static SelectionResult BuildResult(CelestialBody body, string reason)
    {
        return new SelectionResult
        {
            Body = body,
            Reason = reason,
            Population = GetPopulation(body),
            HabitabilityScore = GetHabitabilityScore(body),
            SuitabilityScore = GetSuitabilityScore(body),
        };
    }

    private static int GetPopulation(CelestialBody body)
    {
        PlanetPopulationData? populationData = body.PopulationData;
        if (populationData == null)
        {
            return 0;
        }

        return populationData.GetTotalPopulation();
    }

    private static int GetHabitabilityScore(CelestialBody body)
    {
        PlanetPopulationData? populationData = body.PopulationData;
        if (populationData?.Profile == null)
        {
            return 0;
        }

        return populationData.Profile.HabitabilityScore;
    }

    private static int GetSuitabilityScore(CelestialBody body)
    {
        PlanetPopulationData? populationData = body.PopulationData;
        if (populationData?.Suitability == null)
        {
            return 0;
        }

        return populationData.Suitability.OverallScore;
    }

    private static double GetStableDistance(CelestialBody body)
    {
        if (body.HasOrbital() && body.Orbital != null)
        {
            return body.Orbital.SemiMajorAxisM;
        }

        return double.MaxValue;
    }

    private static bool IsTerrestrialCandidate(CelestialBody body)
    {
        if (body.Type != CelestialType.Type.Planet && body.Type != CelestialType.Type.Moon)
        {
            return false;
        }

        Dictionary? specSnapshot = body.Provenance?.SpecSnapshot;
        if (specSnapshot != null && specSnapshot.ContainsKey("size_category"))
        {
            Variant sizeValue = specSnapshot["size_category"];
            if (sizeValue.VariantType == Variant.Type.Int)
            {
                SizeCategory.Category category = (SizeCategory.Category)(int)sizeValue;
                return SizeCategory.IsRocky(category);
            }
        }

        double diameterKm = body.Physical.RadiusM * 2.0 / 1000.0;
        string sizeToken = TravellerSizeCode.ToStringUwp(TravellerSizeCode.DiameterKmToCode(diameterKm));
        return sizeToken != "D" && sizeToken != "E";
    }
}
