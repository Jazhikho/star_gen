using System.Collections.Generic;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Population;
using StarGen.Domain.Systems;
using StarGen.Domain.Systems.Fixtures;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Builds cached deterministic native-pressure summaries for nearby star systems.
/// </summary>
public static class GalaxyNativePressureCalculator
{
    private const float DefaultNeighborRadiusPc = 10.0f;

    /// <summary>
    /// Builds the nearby-system native-pressure summary for a focal star.
    /// </summary>
    public static NativeSystemPressureSummary BuildNearbySummary(
        Galaxy? galaxy,
        GalaxyStar? centerStar,
        GenerationUseCaseSettings? useCaseSettings)
    {
        if (galaxy == null || centerStar == null)
        {
            return NativeSystemPressureSummary.Empty.Clone();
        }

        Godot.Collections.Array<GalaxyStar> nearbyStars = galaxy.GetStarsInRadius(centerStar.Position, DefaultNeighborRadiusPc);
        List<(double DistancePc, GalaxyStar Star)> orderedNeighbors = new();
        foreach (GalaxyStar nearbyStar in nearbyStars)
        {
            if (nearbyStar.StarSeed == centerStar.StarSeed)
            {
                continue;
            }

            double distancePc = nearbyStar.Position.DistanceTo(centerStar.Position);
            if (distancePc <= 0.0 || distancePc > DefaultNeighborRadiusPc)
            {
                continue;
            }

            orderedNeighbors.Add((distancePc, nearbyStar));
        }

        orderedNeighbors.Sort((left, right) =>
        {
            int distanceComparison = left.DistancePc.CompareTo(right.DistancePc);
            if (distanceComparison != 0)
            {
                return distanceComparison;
            }

            return left.Star.StarSeed.CompareTo(right.Star.StarSeed);
        });

        int nearbyNativeWorldCount = 0;
        double rawSignal = 0.0;
        foreach ((double distancePc, GalaxyStar star) in orderedNeighbors)
        {
            NativeSystemPressureSummary summary = GetOrCreateSystemSummary(galaxy, star, useCaseSettings);
            if (summary.NativeWorldCount <= 0 || summary.PressureSignal <= 0.0)
            {
                continue;
            }

            nearbyNativeWorldCount += summary.NativeWorldCount;
            double distanceFactor = 1.0 - System.Math.Clamp(distancePc / DefaultNeighborRadiusPc, 0.0, 1.0);
            rawSignal += summary.PressureSignal * distanceFactor;
        }

        return new NativeSystemPressureSummary
        {
            NativeWorldCount = nearbyNativeWorldCount,
            PressureSignal = 1.0 - System.Math.Exp(-rawSignal * 0.9),
        };
    }

    /// <summary>
    /// Builds a cached native summary for a single generated system.
    /// </summary>
    public static NativeSystemPressureSummary BuildSystemSummary(SolarSystem? system)
    {
        if (system == null)
        {
            return NativeSystemPressureSummary.Empty.Clone();
        }

        List<CelestialBody> nativeWorlds = new();
        foreach (CelestialBody body in system.Bodies.Values)
        {
            if (!body.HasPopulationData() || body.PopulationData == null || !body.PopulationData.HasExtantNatives())
            {
                continue;
            }

            nativeWorlds.Add(body);
        }

        nativeWorlds.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));

        int nativeWorldCount = 0;
        double rawSignal = 0.0;
        foreach (CelestialBody body in nativeWorlds)
        {
            nativeWorldCount += 1;
            int nativePopulation = body.PopulationData!.GetNativePopulation();
            double populationFactor = 0.40 + (0.60 * NormalizePopulation(nativePopulation));
            rawSignal += populationFactor;
        }

        return new NativeSystemPressureSummary
        {
            NativeWorldCount = nativeWorldCount,
            PressureSignal = 1.0 - System.Math.Exp(-rawSignal * 0.70),
        };
    }

    private static NativeSystemPressureSummary GetOrCreateSystemSummary(
        Galaxy galaxy,
        GalaxyStar star,
        GenerationUseCaseSettings? useCaseSettings)
    {
        if (galaxy.HasCachedNativePressureSummary(star.StarSeed))
        {
            return galaxy.GetCachedNativePressureSummary(star.StarSeed) ?? NativeSystemPressureSummary.Empty.Clone();
        }

        SolarSystem? summarySystem = GenerateSummarySystem(star, useCaseSettings);
        NativeSystemPressureSummary summary = BuildSystemSummary(summarySystem);
        galaxy.CacheNativePressureSummary(star.StarSeed, summary);
        return summary.Clone();
    }

    private static SolarSystem? GenerateSummarySystem(GalaxyStar star, GenerationUseCaseSettings? useCaseSettings)
    {
        SolarSystemSpec spec = SolarSystemSpec.RandomSmall(star.StarSeed);
        spec.SystemMetallicity = star.Metallicity;
        spec.IncludeAsteroidBelts = false;
        spec.GeneratePopulation = true;
        if (useCaseSettings != null)
        {
            spec.UseCaseSettings = useCaseSettings.Clone();
        }

        return SystemFixtureGenerator.GenerateSystem(spec, true);
    }

    private static double NormalizePopulation(int nativePopulation)
    {
        if (nativePopulation <= 0)
        {
            return 0.0;
        }

        return System.Math.Clamp(System.Math.Log10(nativePopulation + 1.0) / 9.0, 0.0, 1.0);
    }
}
