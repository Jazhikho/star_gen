using Godot;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Importance-samples galaxy point clouds from the density models for previews and rendering.
/// </summary>
public static class DensitySampler
{
    /// <summary>
    /// Safety cap used by rejection sampling.
    /// </summary>
    public const int MaxAttemptsPerPoint = 30;

    /// <summary>
    /// Samples a galaxy into bulge and disk point populations.
    /// </summary>
    public static GalaxySample SampleGalaxy(GalaxySpec spec, int numPoints, SeededRng rng)
    {
        GalaxySample sample;
        if (spec.Type == GalaxySpec.GalaxyType.Spiral)
        {
            sample = SampleSpiralGalaxy(spec, numPoints, rng);
            return ApplyDynamicsPlacementAdjustment(spec, sample);
        }

        if (spec.Type == GalaxySpec.GalaxyType.Elliptical)
        {
            sample = SampleEllipticalGalaxy(spec, numPoints, rng);
            return ApplyDynamicsPlacementAdjustment(spec, sample);
        }

        if (spec.Type == GalaxySpec.GalaxyType.Lenticular)
        {
            sample = SampleLenticularGalaxy(spec, numPoints, rng);
            return ApplyDynamicsPlacementAdjustment(spec, sample);
        }

        if (spec.Type == GalaxySpec.GalaxyType.Irregular)
        {
            sample = SampleIrregularGalaxy(spec, numPoints, rng);
            return ApplyDynamicsPlacementAdjustment(spec, sample);
        }

        sample = SampleSpiralGalaxy(spec, numPoints, rng);
        return ApplyDynamicsPlacementAdjustment(spec, sample);
    }

    private static GalaxySample ApplyDynamicsPlacementAdjustment(GalaxySpec spec, GalaxySample sample)
    {
        if (spec.DynamicsBehaviorMode != GalaxyDynamicsBehaviorMode.AffectPlacement)
        {
            return sample;
        }

        double densityPressure = System.Math.Clamp(spec.DynamicsDiagnostic.LocalTotalMassDensitySolarPerPc3 / 0.10, 0.75, 1.60);
        double radialScale = 0.985 - ((densityPressure - 1.0) * 0.06);
        double verticalScale = 1.015 + ((densityPressure - 1.0) * 0.04);
        AdjustPlacementPopulation(sample.BulgePoints, radialScale, verticalScale);
        AdjustPlacementPopulation(sample.DiskPoints, radialScale, verticalScale);
        return sample;
    }

    private static void AdjustPlacementPopulation(Vector3[] points, double radialScale, double verticalScale)
    {
        for (int index = 0; index < points.Length; index += 1)
        {
            Vector3 point = points[index];
            points[index] = new Vector3(
                (float)(point.X * radialScale),
                (float)(point.Y * verticalScale),
                (float)(point.Z * radialScale));
        }
    }

    /// <summary>
    /// Samples a spiral galaxy's bulge and disk.
    /// </summary>
    private static GalaxySample SampleSpiralGalaxy(GalaxySpec spec, int numPoints, SeededRng rng)
    {
        GalaxySample sample = new();
        SpiralDensityModel densityModel = new(spec);
        float bulgeFraction = GetBulgeFractionSpiral(spec);
        int bulgeCount = Mathf.RoundToInt(numPoints * bulgeFraction);
        int diskCount = numPoints - bulgeCount;
        sample.BulgePoints = SampleGaussianBulge(spec, bulgeCount, rng);
        sample.DiskPoints = SampleSpiralDisk(spec, densityModel, diskCount, rng);
        return sample;
    }

    /// <summary>
    /// Samples an elliptical galaxy as a single ellipsoidal population.
    /// </summary>
    private static GalaxySample SampleEllipticalGalaxy(GalaxySpec spec, int numPoints, SeededRng rng)
    {
        GalaxySample sample = new();
        EllipticalDensityModel densityModel = new(spec);
        sample.BulgePoints = SampleEllipsoid(densityModel, numPoints, rng);
        sample.DiskPoints = System.Array.Empty<Vector3>();
        return sample;
    }

    /// <summary>
    /// Samples a lenticular galaxy as a bulge plus smooth disk with no active spiral-arm filtering.
    /// </summary>
    private static GalaxySample SampleLenticularGalaxy(GalaxySpec spec, int numPoints, SeededRng rng)
    {
        GalaxySample sample = new GalaxySample();
        LenticularDensityModel densityModel = new LenticularDensityModel(spec);
        int bulgeCount = Mathf.RoundToInt(numPoints * Mathf.Clamp((float)spec.BulgeToTotal, 0.2f, 0.75f));
        int diskCount = numPoints - bulgeCount;
        sample.BulgePoints = SampleGaussianBulge(spec, bulgeCount, rng);
        sample.DiskPoints = SampleSmoothDisk(spec, densityModel, diskCount, rng);
        return sample;
    }

    /// <summary>
    /// Samples an irregular galaxy as core and outer populations from the same 3D density.
    /// </summary>
    private static GalaxySample SampleIrregularGalaxy(GalaxySpec spec, int numPoints, SeededRng rng)
    {
        GalaxySample sample = new();
        IrregularDensityModel densityModel = new(spec);
        int coreCount = Mathf.RoundToInt(numPoints * 0.3f);
        int outerCount = numPoints - coreCount;
        sample.BulgePoints = SampleIrregularBlob(densityModel, coreCount, rng, true);
        sample.DiskPoints = SampleIrregularBlob(densityModel, outerCount, rng, false);
        return sample;
    }

    /// <summary>
    /// Estimates the spiral bulge fraction from the density parameters.
    /// </summary>
    private static float GetBulgeFractionSpiral(GalaxySpec spec)
    {
        double bulgeVolume = spec.BulgeIntensity
            * Mathf.Tau
            * spec.BulgeRadiusPc
            * spec.BulgeRadiusPc
            * System.Math.Sqrt(Mathf.Tau)
            * spec.BulgeHeightPc;
        double averageArmModulation = 1.0 - (spec.ArmAmplitude * 0.4);
        double thinDiskVolume = spec.ThinDiskScaleLengthPc
            * spec.ThinDiskScaleLengthPc
            * spec.ThinDiskScaleHeightPc
            * 0.88;
        double thickDiskVolume = spec.ThickDiskScaleLengthPc
            * spec.ThickDiskScaleLengthPc
            * spec.ThickDiskScaleHeightPc
            * 0.12;
        double diskVolume = averageArmModulation
            * Mathf.Tau
            * (thinDiskVolume + thickDiskVolume)
            * 2.0;
        double total = bulgeVolume + diskVolume;
        if (total <= 0.0)
        {
            return 0.5f;
        }

        return Mathf.Clamp((float)(bulgeVolume / total), 0.05f, 0.5f);
    }

    /// <summary>
    /// Samples a Gaussian bulge centered at the origin.
    /// </summary>
    private static Vector3[] SampleGaussianBulge(GalaxySpec spec, int count, SeededRng rng)
    {
        Vector3[] points = new Vector3[count];
        for (int index = 0; index < count; index += 1)
        {
            points[index] = new Vector3(
                rng.Randfn(0.0f, (float)spec.BulgeRadiusPc),
                rng.Randfn(0.0f, (float)spec.BulgeHeightPc),
                rng.Randfn(0.0f, (float)spec.BulgeRadiusPc));
        }

        return points;
    }

    /// <summary>
    /// Samples a spiral disk using arm-based rejection sampling.
    /// </summary>
    private static Vector3[] SampleSpiralDisk(
        GalaxySpec spec,
        SpiralDensityModel densityModel,
        int count,
        SeededRng rng)
    {
        Vector3[] points = new Vector3[count];
        int accepted = 0;
        int maxAttempts = count * MaxAttemptsPerPoint;
        int attempt = 0;

        while (accepted < count && attempt < maxAttempts)
        {
            attempt += 1;
            float radialDistance = SampleGamma2((float)spec.ThinDiskScaleLengthPc, rng);
            if (radialDistance > spec.RadiusPc)
            {
                continue;
            }

            float theta = rng.Randf() * Mathf.Tau;
            float height = SampleThinOrThickDiskHeight(spec, rng);
            if (Mathf.Abs(height) > spec.HeightPc)
            {
                continue;
            }

            float x = radialDistance * Mathf.Cos(theta);
            float z = radialDistance * Mathf.Sin(theta);
            float armFactor = densityModel.GetArmFactor(radialDistance, x, z);
            if (rng.Randf() <= armFactor)
            {
                points[accepted] = new Vector3(x, height, z);
                accepted += 1;
            }
        }

        if (accepted < count)
        {
            System.Array.Resize(ref points, accepted);
        }

        return points;
    }

    private static float SampleThinOrThickDiskHeight(GalaxySpec spec, SeededRng rng)
    {
        if (rng.Randf() < 0.12f)
        {
            return SampleLaplace((float)spec.ThickDiskScaleHeightPc, rng);
        }

        return SampleLaplace((float)spec.ThinDiskScaleHeightPc, rng);
    }

    /// <summary>
    /// Samples a 3D Gaussian ellipsoid aligned with the elliptical density model.
    /// </summary>
    private static Vector3[] SampleEllipsoid(EllipticalDensityModel densityModel, int count, SeededRng rng)
    {
        Vector3[] points = new Vector3[count];
        float sigmaMajor = densityModel.GetEffectiveRadius();
        float sigmaMinor = sigmaMajor * densityModel.GetAxisRatio();

        for (int index = 0; index < count; index += 1)
        {
            points[index] = new Vector3(
                rng.Randfn(0.0f, sigmaMajor),
                rng.Randfn(0.0f, sigmaMinor),
                rng.Randfn(0.0f, sigmaMajor));
        }

        return points;
    }

    /// <summary>
    /// Samples an irregular 3D blob with rejection based on the noise-modulated density.
    /// </summary>
    private static Vector3[] SampleIrregularBlob(
        IrregularDensityModel densityModel,
        int count,
        SeededRng rng,
        bool isCore)
    {
        Vector3[] points = new Vector3[count];
        int accepted = 0;
        int maxAttempts = count * MaxAttemptsPerPoint;
        float scaleRadius = densityModel.GetScaleRadius();
        float peakDensity = densityModel.GetPeakDensity();
        Vector3 centerOffset = densityModel.GetCenterOffset();
        float scaleFactor;
        if (isCore)
        {
            scaleFactor = 0.6f;
        }
        else
        {
            scaleFactor = 1.5f;
        }

        float sampleScale = scaleRadius * scaleFactor;
        float maxRadius = sampleScale * 4.0f;
        int attempt = 0;

        while (accepted < count && attempt < maxAttempts)
        {
            attempt += 1;
            float radialDistance = SampleGamma2(sampleScale, rng);
            if (radialDistance > maxRadius)
            {
                continue;
            }

            float theta = rng.Randf() * Mathf.Tau;
            float cosPhi = (2.0f * rng.Randf()) - 1.0f;
            float sinPhi = Mathf.Sqrt(1.0f - (cosPhi * cosPhi));
            Vector3 position = new(
                radialDistance * sinPhi * Mathf.Cos(theta),
                radialDistance * cosPhi,
                radialDistance * sinPhi * Mathf.Sin(theta));
            position += centerOffset;

            float density = densityModel.GetDensity(position);
            float acceptance = Mathf.Clamp(density / peakDensity, 0.0f, 1.0f);
            if (rng.Randf() <= acceptance)
            {
                points[accepted] = position;
                accepted += 1;
            }
        }

        if (accepted < count)
        {
            System.Array.Resize(ref points, accepted);
        }

        return points;
    }

    /// <summary>
    /// Samples a smooth disk without spiral-arm rejection filtering.
    /// </summary>
    private static Vector3[] SampleSmoothDisk(
        GalaxySpec spec,
        DensityModelInterface densityModel,
        int count,
        SeededRng rng)
    {
        Vector3[] points = new Vector3[count];
        int accepted = 0;
        int attempt = 0;
        int maxAttempts = count * MaxAttemptsPerPoint;
        float peakDensity = densityModel.GetPeakDensity();
        while (accepted < count && attempt < maxAttempts)
        {
            attempt += 1;
            float radialDistance = SampleGamma2((float)spec.DiskScaleLengthPc, rng);
            if (radialDistance > spec.RadiusPc)
            {
                continue;
            }

            float theta = rng.Randf() * Mathf.Tau;
            float height = SampleLaplace((float)spec.DiskScaleHeightPc, rng);
            if (Mathf.Abs(height) > spec.HeightPc)
            {
                continue;
            }

            Vector3 position = new Vector3(
                radialDistance * Mathf.Cos(theta),
                height,
                radialDistance * Mathf.Sin(theta));
            float acceptance = Mathf.Clamp(densityModel.GetDensity(position) / peakDensity, 0.0f, 1.0f);
            if (rng.Randf() <= acceptance)
            {
                points[accepted] = position;
                accepted += 1;
            }
        }

        if (accepted < count)
        {
            System.Array.Resize(ref points, accepted);
        }

        return points;
    }

    /// <summary>
    /// Samples a Gamma(shape=2, scale) value.
    /// </summary>
    private static float SampleGamma2(float scale, SeededRng rng)
    {
        float u1 = Mathf.Max(rng.Randf(), 1.0e-10f);
        float u2 = Mathf.Max(rng.Randf(), 1.0e-10f);
        return -scale * Mathf.Log(u1 * u2);
    }

    /// <summary>
    /// Samples a Laplace(0, scale) value.
    /// </summary>
    private static float SampleLaplace(float scale, SeededRng rng)
    {
        float u = rng.Randf() - 0.5f;
        float absTwiceU = Mathf.Min(2.0f * Mathf.Abs(u), 1.0f - 1.0e-10f);
        return -scale * Mathf.Sign(u) * Mathf.Log(1.0f - absTwiceU);
    }
}
