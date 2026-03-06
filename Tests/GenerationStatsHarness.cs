#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;

namespace StarGen.Tests;

/// <summary>
/// Deterministic ensemble-sampling helpers for stochastic generator tests.
/// </summary>
public static class GenerationStatsHarness
{
    public sealed class StarSpectralHistogram
    {
        public int O;
        public int B;
        public int A;
        public int F;
        public int G;
        public int K;
        public int M;
        public int Total;
    }

    public sealed class PlanetDistributionStats
    {
        public int TotalPlanets;
        public int HotJupiters;
        public int InnerTotal;
        public int InnerLarge;
        public int OuterTotal;
        public int OuterLarge;
    }

    public static StarSpectralHistogram SampleStarSpectralHistogram(int seedBase, int count)
    {
        StarSpectralHistogram histogram = new();
        for (int index = 0; index < count; index += 1)
        {
            StarSpec spec = StarSpec.Random(seedBase + index);
            CelestialBody star = StarGenerator.Generate(spec, new SeededRng(spec.GenerationSeed));
            if (star.Stellar == null)
            {
                continue;
            }

            string spectral = star.Stellar.SpectralClass;
            if (string.IsNullOrEmpty(spectral))
            {
                continue;
            }

            histogram.Total += 1;
            char letter = char.ToUpperInvariant(spectral[0]);
            if (letter == 'O')
            {
                histogram.O += 1;
                continue;
            }

            if (letter == 'B')
            {
                histogram.B += 1;
                continue;
            }

            if (letter == 'A')
            {
                histogram.A += 1;
                continue;
            }

            if (letter == 'F')
            {
                histogram.F += 1;
                continue;
            }

            if (letter == 'G')
            {
                histogram.G += 1;
                continue;
            }

            if (letter == 'K')
            {
                histogram.K += 1;
                continue;
            }

            if (letter == 'M')
            {
                histogram.M += 1;
            }
        }

        return histogram;
    }

    public static PlanetDistributionStats SampleSystemPlanetStats(int seedBase, int systemCount)
    {
        PlanetDistributionStats stats = new();
        GalaxySpec galaxySpec = GalaxySpec.CreateMilkyWay(seedBase);

        for (int index = 0; index < systemCount; index += 1)
        {
            int starSeed = seedBase + index;
            Vector3 position = new(8000.0f + index, 0.0f, 0.0f);
            GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(position, starSeed, galaxySpec);
            SolarSystem? system = GalaxySystemGenerator.GenerateSystem(star, includeAsteroids: false, enablePopulation: false);
            if (system == null)
            {
                continue;
            }

            foreach (CelestialBody planet in system.GetPlanets())
            {
                if (planet.Orbital == null)
                {
                    continue;
                }

                double distanceAu = planet.Orbital.SemiMajorAxisM / Units.AuMeters;
                double massEarth = planet.Physical.MassKg / Units.EarthMassKg;
                bool isLarge = massEarth >= 10.0;

                stats.TotalPlanets += 1;

                if (distanceAu < 0.1 && massEarth >= 50.0)
                {
                    stats.HotJupiters += 1;
                }

                if (distanceAu < 1.0)
                {
                    stats.InnerTotal += 1;
                    if (isLarge)
                    {
                        stats.InnerLarge += 1;
                    }
                }
                else if (distanceAu > 5.0)
                {
                    stats.OuterTotal += 1;
                    if (isLarge)
                    {
                        stats.OuterLarge += 1;
                    }
                }
            }
        }

        return stats;
    }
}
