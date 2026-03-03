using Godot;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Systems.AsteroidFields;

/// <summary>
/// Generates sampled asteroid-belt field data from a specification and deterministic RNG.
/// </summary>
public static class BeltFieldGenerator
{
    /// <summary>
    /// Generates one complete belt field.
    /// </summary>
    public static BeltFieldData GenerateField(BeltFieldSpec spec, SeededRng rng)
    {
        BeltFieldData belt = new()
        {
            Spec = spec,
            GenerationSeed = (int)rng.GetInitialSeed(),
        };

        for (int index = 0; index < spec.AsteroidCount; index += 1)
        {
            belt.Asteroids.Add(GenerateSingleAsteroid(spec, rng));
        }

        foreach (BeltAsteroidData major in ProcessMajorAsteroids(spec))
        {
            belt.Asteroids.Add(major);
        }

        return belt;
    }

    /// <summary>
    /// Generates one background asteroid sample.
    /// </summary>
    private static BeltAsteroidData GenerateSingleAsteroid(BeltFieldSpec spec, SeededRng rng)
    {
        BeltAsteroidData asteroid = new()
        {
            IsMajor = false,
            SemiMajorAxisAu = SampleSemiMajorAxis(spec, rng),
            Eccentricity = SampleEccentricity(spec, rng),
            InclinationRad = SampleInclination(spec, rng),
            LongitudeAscendingNodeRad = rng.Randf() * Mathf.Tau,
            ArgumentPeriapsisRad = rng.Randf() * Mathf.Tau,
        };

        double effectiveLongitude = SampleEffectiveLongitude(spec, rng);
        double rawAnomaly = effectiveLongitude - asteroid.ArgumentPeriapsisRad;
        asteroid.TrueAnomalyRad = WrapTau(rawAnomaly);
        asteroid.BodyRadiusKm = SampleBodySize(spec, rng);
        asteroid.PositionAu = BeltOrbitalMath.OrbitalElementsToPosition(
            asteroid.SemiMajorAxisAu,
            asteroid.Eccentricity,
            asteroid.InclinationRad,
            asteroid.LongitudeAscendingNodeRad,
            asteroid.ArgumentPeriapsisRad,
            asteroid.TrueAnomalyRad);
        return asteroid;
    }

    /// <summary>
    /// Converts major-input rows to generated asteroid entries.
    /// </summary>
    private static Godot.Collections.Array<BeltAsteroidData> ProcessMajorAsteroids(BeltFieldSpec spec)
    {
        Godot.Collections.Array<BeltAsteroidData> result = new();
        foreach (BeltMajorAsteroidInput input in spec.MajorAsteroidInputs)
        {
            BeltAsteroidData asteroid = new()
            {
                IsMajor = true,
                BodyId = input.BodyId,
                BodyRadiusKm = input.BodyRadiusKm,
                AsteroidType = input.AsteroidType,
                SemiMajorAxisAu = input.SemiMajorAxisM / BeltOrbitalMath.AuMeters,
                Eccentricity = input.Eccentricity,
                InclinationRad = Mathf.DegToRad((float)input.InclinationDeg),
                LongitudeAscendingNodeRad = Mathf.DegToRad((float)input.LongitudeAscendingNodeDeg),
                ArgumentPeriapsisRad = Mathf.DegToRad((float)input.ArgumentPeriapsisDeg),
            };

            double meanAnomalyRad = Mathf.DegToRad((float)input.MeanAnomalyDeg);
            asteroid.TrueAnomalyRad = BeltOrbitalMath.MeanAnomalyToTrueAnomaly(meanAnomalyRad, input.Eccentricity);
            asteroid.PositionAu = BeltOrbitalMath.OrbitalElementsToPosition(
                asteroid.SemiMajorAxisAu,
                asteroid.Eccentricity,
                asteroid.InclinationRad,
                asteroid.LongitudeAscendingNodeRad,
                asteroid.ArgumentPeriapsisRad,
                asteroid.TrueAnomalyRad);
            result.Add(asteroid);
        }

        return result;
    }

    private static double SampleSemiMajorAxis(BeltFieldSpec spec, SeededRng rng)
    {
        double beltWidth = spec.OuterRadiusAu - spec.InnerRadiusAu;
        double maxDensity = RadialDensity(0.5, spec.RadialConcentration);

        for (int attempt = 0; attempt < 1000; attempt += 1)
        {
            double t = rng.Randf();
            double threshold = rng.Randf() * maxDensity;
            if (threshold < RadialDensity(t, spec.RadialConcentration))
            {
                double radius = spec.InnerRadiusAu + (t * beltWidth);
                if (!IsInGap(radius, spec))
                {
                    return radius;
                }
            }
        }

        return (spec.InnerRadiusAu + spec.OuterRadiusAu) * 0.5;
    }

    private static double RadialDensity(double t, double concentration)
    {
        if (t <= 0.0 || t >= 1.0)
        {
            return 0.0;
        }

        return System.Math.Pow(t, concentration) * System.Math.Pow(1.0 - t, concentration);
    }

    private static bool IsInGap(double radiusAu, BeltFieldSpec spec)
    {
        int gapCount = System.Math.Min(spec.GapCentersAu.Count, spec.GapHalfWidthsAu.Count);
        for (int index = 0; index < gapCount; index += 1)
        {
            if (System.Math.Abs(radiusAu - spec.GapCentersAu[index]) < spec.GapHalfWidthsAu[index])
            {
                return true;
            }
        }

        return false;
    }

    private static double SampleEccentricity(BeltFieldSpec spec, SeededRng rng)
    {
        double u = rng.Randf();
        return spec.MaxEccentricity * u * u;
    }

    private static double SampleInclination(BeltFieldSpec spec, SeededRng rng)
    {
        double maxInclinationRad = Mathf.DegToRad((float)spec.MaxInclinationDeg);
        double u = rng.Randf();
        return maxInclinationRad * u * u;
    }

    private static double SampleEffectiveLongitude(BeltFieldSpec spec, SeededRng rng)
    {
        if (spec.ClusterCount <= 0 || spec.ClusterLongitudesRad.Count == 0)
        {
            return rng.Randf() * Mathf.Tau;
        }

        if (rng.Randf() < spec.ClusterFraction)
        {
            int clusterIndex = (int)(rng.Randi() % (uint)spec.ClusterLongitudesRad.Count);
            return SampleVonMises(spec.ClusterLongitudesRad[clusterIndex], spec.ClusterConcentration, rng);
        }

        return rng.Randf() * Mathf.Tau;
    }

    private static double SampleVonMises(double mu, double kappa, SeededRng rng)
    {
        if (kappa <= 0.0)
        {
            return rng.Randf() * Mathf.Tau;
        }

        double stdDev = 1.0 / System.Math.Sqrt(kappa);
        return WrapTau(rng.Randfn((float)mu, (float)stdDev));
    }

    private static double SampleBodySize(BeltFieldSpec spec, SeededRng rng)
    {
        double alpha = spec.SizePowerLawExponent;
        double minRadius = spec.MinBodyRadiusKm;
        double maxRadius = spec.MaxBodyRadiusKm;
        if (System.Math.Abs(alpha - 1.0) < 0.001)
        {
            double logMin = System.Math.Log(minRadius);
            double logMax = System.Math.Log(maxRadius);
            return System.Math.Exp(logMin + (rng.Randf() * (logMax - logMin)));
        }

        double u = rng.Randf();
        double exponent = 1.0 - alpha;
        double result = System.Math.Pow(
            ((System.Math.Pow(maxRadius, exponent) - System.Math.Pow(minRadius, exponent)) * u) + System.Math.Pow(minRadius, exponent),
            1.0 / exponent);
        return System.Math.Clamp(result, minRadius, maxRadius);
    }

    private static double WrapTau(double angle)
    {
        double tau = Mathf.Tau;
        return ((angle % tau) + tau) % tau;
    }
}
