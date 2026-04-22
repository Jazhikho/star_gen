using Godot;
using StarGen.Domain.Generation;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Evaluates galaxy-origin fields for a star position inside a resolved galaxy specification.
/// </summary>
public static class GalaxyScientificFieldEvaluator
{
    /// <summary>
    /// Evaluates the downstream galaxy-origin context for a world-space position.
    /// </summary>
    public static GalaxyOriginContext Evaluate(Vector3 position, GalaxySpec galaxySpec)
    {
        GalaxyOriginContext context = new GalaxyOriginContext();
        GalaxyRealismProfile profile = galaxySpec.RealismProfile ?? new GalaxyRealismProfile();

        double radialDistance = System.Math.Sqrt((position.X * position.X) + (position.Z * position.Z));
        double normalizedRadius = 0.0;
        if (galaxySpec.RadiusPc > 0.0)
        {
            normalizedRadius = radialDistance / galaxySpec.RadiusPc;
        }

        double absoluteHeight = System.Math.Abs(position.Y);
        double normalizedHeight = 0.0;
        if (galaxySpec.HeightPc > 0.0)
        {
            normalizedHeight = absoluteHeight / galaxySpec.HeightPc;
        }

        context.ResolvedSubtype = galaxySpec.ResolvedSubtype;
        context.RegionKind = ResolveRegion(position, radialDistance, normalizedRadius, normalizedHeight, galaxySpec);
        context.AgeCohort = ResolveAgeCohort(context.RegionKind, profile);
        context.MetallicityPrior = CalculateMetallicity(position, radialDistance, normalizedHeight, galaxySpec, profile);
        context.AgeMeanGyr = ResolveAgeMeanGyr(context.AgeCohort, profile);
        context.AgeBias = ResolveAgeBias(context.AgeMeanGyr);
        context.GhzWeight = CalculateGhzWeight(radialDistance, profile);
        context.HazardWeight = CalculateHazardWeight(context.RegionKind, context.GhzWeight, normalizedHeight, profile);
        context.ClusterProbability = CalculateClusterProbability(context.RegionKind, profile);
        context.IsBarInfluenced = galaxySpec.IsBarred && context.RegionKind == GalaxyRegionKind.Bar;
        context.IsArmInfluenced = context.RegionKind == GalaxyRegionKind.SpiralArm;
        context.EnvironmentDensityIndex = profile.EnvironmentDensityIndex;
        context.HaloMassLog10Solar = profile.HaloMassLog10Solar;
        context.LocalDensityRatio = CalculateLocalDensityRatio(normalizedRadius, normalizedHeight, galaxySpec, context);
        context.LocalStarFormationEfficiency = CalculateLocalStarFormationEfficiency(context, profile);
        context.StellarProfile = ResolveLocalStellarProfile(galaxySpec, context);
        return context;
    }

    private static StellarGenerationProfile ResolveLocalStellarProfile(GalaxySpec galaxySpec, GalaxyOriginContext context)
    {
        StellarGenerationProfile profile = galaxySpec.StellarProfile.Clone();
        double multiplicityScale = profile.MultiplicityScale;
        multiplicityScale *= 0.85 + (context.ClusterProbability * 0.45);
        multiplicityScale *= 0.90 + (context.LocalStarFormationEfficiency * 0.35);

        if (context.RegionKind == GalaxyRegionKind.Halo || context.RegionKind == GalaxyRegionKind.DwarfEnvelope)
        {
            multiplicityScale *= 0.90;
        }
        else if (context.RegionKind == GalaxyRegionKind.SpiralArm || context.RegionKind == GalaxyRegionKind.IrregularBody)
        {
            multiplicityScale *= 1.08;
        }

        profile.MultiplicityScale = System.Math.Clamp(multiplicityScale, 0.35, 2.0);
        return profile;
    }

    private static GalaxyRegionKind ResolveRegion(
        Vector3 position,
        double radialDistance,
        double normalizedRadius,
        double normalizedHeight,
        GalaxySpec galaxySpec)
    {
        if (normalizedRadius < 0.08)
        {
            return GalaxyRegionKind.Core;
        }

        if (normalizedRadius < 0.16)
        {
            if (galaxySpec.IsBarred)
            {
                double barMajor = galaxySpec.BulgeRadiusPc * (1.6 + galaxySpec.BarStrength);
                double barMinor = galaxySpec.BulgeRadiusPc * 0.55;
                if (barMajor > 0.0 && barMinor > 0.0)
                {
                    double barDistance = ((position.X * position.X) / (barMajor * barMajor)) +
                        ((position.Z * position.Z) / (barMinor * barMinor));
                    if (barDistance <= 1.0)
                    {
                        return GalaxyRegionKind.Bar;
                    }
                }
            }

            return GalaxyRegionKind.Bulge;
        }

        if (galaxySpec.Type == GalaxySpec.GalaxyType.Elliptical)
        {
            if (normalizedRadius < 0.6)
            {
                return GalaxyRegionKind.Bulge;
            }

            return GalaxyRegionKind.Halo;
        }

        if (galaxySpec.Type == GalaxySpec.GalaxyType.Lenticular)
        {
            if (normalizedRadius < 0.65)
            {
                return GalaxyRegionKind.LenticularDisk;
            }

            return GalaxyRegionKind.Halo;
        }

        if (galaxySpec.Type == GalaxySpec.GalaxyType.Irregular)
        {
            if (galaxySpec.ResolvedSubtype == GalaxyResolvedSubtype.DwarfSpheroidal)
            {
                return GalaxyRegionKind.DwarfEnvelope;
            }

            if (normalizedRadius < 0.6)
            {
                return GalaxyRegionKind.IrregularBody;
            }

            return GalaxyRegionKind.DwarfEnvelope;
        }

        if (normalizedHeight > 0.65)
        {
            return GalaxyRegionKind.Halo;
        }

        if (normalizedRadius < 0.35)
        {
            if (IsNearSpiralArm(position, radialDistance, galaxySpec))
            {
                return GalaxyRegionKind.SpiralArm;
            }

            return GalaxyRegionKind.InnerDisk;
        }

        if (normalizedRadius < 0.82)
        {
            if (IsNearSpiralArm(position, radialDistance, galaxySpec))
            {
                return GalaxyRegionKind.SpiralArm;
            }

            return GalaxyRegionKind.OuterDisk;
        }

        return GalaxyRegionKind.Halo;
    }

    private static GalaxyAgeCohort ResolveAgeCohort(GalaxyRegionKind regionKind, GalaxyRealismProfile profile)
    {
        if (regionKind == GalaxyRegionKind.Core || regionKind == GalaxyRegionKind.Bulge)
        {
            return GalaxyAgeCohort.Ancient;
        }

        if (regionKind == GalaxyRegionKind.Bar)
        {
            return GalaxyAgeCohort.Old;
        }

        if (regionKind == GalaxyRegionKind.SpiralArm || regionKind == GalaxyRegionKind.IrregularBody)
        {
            if (profile.CharacteristicAgeGyr < 4.5)
            {
                return GalaxyAgeCohort.Young;
            }

            return GalaxyAgeCohort.Mature;
        }

        if (regionKind == GalaxyRegionKind.LenticularDisk || regionKind == GalaxyRegionKind.Halo || regionKind == GalaxyRegionKind.DwarfEnvelope)
        {
            return GalaxyAgeCohort.Old;
        }

        return GalaxyAgeCohort.Mature;
    }

    private static double CalculateMetallicity(
        Vector3 position,
        double radialDistance,
        double normalizedHeight,
        GalaxySpec galaxySpec,
        GalaxyRealismProfile profile)
    {
        double radialDistanceKpc = radialDistance / 1000.0;
        double metallicity = 1.15 + (profile.MetallicityGradientDexPerKpc * radialDistanceKpc);
        metallicity *= System.Math.Exp(-0.5 * normalizedHeight);

        if (galaxySpec.Type == GalaxySpec.GalaxyType.Elliptical)
        {
            metallicity *= 1.1;
        }
        else if (galaxySpec.Type == GalaxySpec.GalaxyType.Lenticular)
        {
            metallicity *= 0.95;
        }
        else if (galaxySpec.Type == GalaxySpec.GalaxyType.Irregular)
        {
            metallicity *= 0.75;
        }

        if (System.Math.Abs(position.Y) > galaxySpec.HeightPc * 0.5)
        {
            metallicity *= 0.85;
        }

        return System.Math.Clamp(metallicity, 0.1, 3.0);
    }

    private static double ResolveAgeMeanGyr(GalaxyAgeCohort ageCohort, GalaxyRealismProfile profile)
    {
        if (ageCohort == GalaxyAgeCohort.Young)
        {
            return 1.8 + (profile.CharacteristicAgeGyr * 0.15);
        }

        if (ageCohort == GalaxyAgeCohort.Mature)
        {
            return System.Math.Max(3.5, profile.CharacteristicAgeGyr * 0.8);
        }

        if (ageCohort == GalaxyAgeCohort.Old)
        {
            return System.Math.Max(6.5, profile.CharacteristicAgeGyr + 1.0);
        }

        return System.Math.Max(8.5, profile.CharacteristicAgeGyr + 2.5);
    }

    private static double ResolveAgeBias(double ageMeanGyr)
    {
        return System.Math.Clamp(ageMeanGyr / 5.5, 0.5, 2.1);
    }

    private static double CalculateGhzWeight(double radialDistance, GalaxyRealismProfile profile)
    {
        double innerDistance = (radialDistance - profile.GhzInnerRadiusPc) / profile.GhzTransitionWidthPc;
        double outerDistance = (profile.GhzOuterRadiusPc - radialDistance) / profile.GhzTransitionWidthPc;
        double innerWeight = 1.0 / (1.0 + System.Math.Exp(-innerDistance));
        double outerWeight = 1.0 / (1.0 + System.Math.Exp(-outerDistance));
        return System.Math.Clamp(innerWeight * outerWeight, 0.0, 1.0);
    }

    private static double CalculateHazardWeight(
        GalaxyRegionKind regionKind,
        double ghzWeight,
        double normalizedHeight,
        GalaxyRealismProfile profile)
    {
        double hazard = 0.35 + (profile.EnvironmentDensityIndex * 0.35);
        if (regionKind == GalaxyRegionKind.Core || regionKind == GalaxyRegionKind.Bulge)
        {
            hazard += 0.35;
        }
        else if (regionKind == GalaxyRegionKind.SpiralArm)
        {
            hazard += 0.18;
        }

        hazard -= ghzWeight * 0.2;
        hazard += normalizedHeight * 0.08;
        return System.Math.Clamp(hazard, 0.0, 1.0);
    }

    private static double CalculateClusterProbability(GalaxyRegionKind regionKind, GalaxyRealismProfile profile)
    {
        double clusterProbability = profile.StarFormationEfficiency;
        if (regionKind == GalaxyRegionKind.SpiralArm || regionKind == GalaxyRegionKind.IrregularBody)
        {
            clusterProbability += 0.25;
        }
        else if (regionKind == GalaxyRegionKind.OuterDisk)
        {
            clusterProbability += 0.08;
        }
        else if (regionKind == GalaxyRegionKind.Core || regionKind == GalaxyRegionKind.Halo)
        {
            clusterProbability -= 0.12;
        }

        return System.Math.Clamp(clusterProbability, 0.02, 0.9);
    }

    private static double CalculateLocalDensityRatio(
        double normalizedRadius,
        double normalizedHeight,
        GalaxySpec galaxySpec,
        GalaxyOriginContext context)
    {
        double radialFactor = System.Math.Exp(-2.0 * normalizedRadius);
        double verticalFactor = System.Math.Exp(-1.5 * normalizedHeight);
        double localDensity = (0.25 + radialFactor) * verticalFactor * galaxySpec.BulgeIntensity;

        if (context.RegionKind == GalaxyRegionKind.SpiralArm)
        {
            localDensity *= 1.6;
        }
        else if (context.RegionKind == GalaxyRegionKind.Bar)
        {
            localDensity *= 1.35;
        }
        else if (context.RegionKind == GalaxyRegionKind.Halo || context.RegionKind == GalaxyRegionKind.DwarfEnvelope)
        {
            localDensity *= 0.45;
        }

        return System.Math.Clamp(localDensity, 0.05, 10.0);
    }

    private static double CalculateLocalStarFormationEfficiency(GalaxyOriginContext context, GalaxyRealismProfile profile)
    {
        double efficiency = profile.StarFormationEfficiency;
        if (context.RegionKind == GalaxyRegionKind.SpiralArm || context.RegionKind == GalaxyRegionKind.IrregularBody)
        {
            efficiency += 0.12;
        }
        else if (context.RegionKind == GalaxyRegionKind.Bulge || context.RegionKind == GalaxyRegionKind.Core)
        {
            efficiency -= 0.05;
        }

        return System.Math.Clamp(efficiency, 0.05, 0.65);
    }

    private static bool IsNearSpiralArm(Vector3 position, double radialDistance, GalaxySpec galaxySpec)
    {
        if (galaxySpec.Type != GalaxySpec.GalaxyType.Spiral || galaxySpec.NumArms <= 0)
        {
            return false;
        }

        if (radialDistance < 100.0)
        {
            return true;
        }

        double theta = System.Math.Atan2(position.Z, position.X);
        double pitchTan = System.Math.Tan(Mathf.DegToRad((float)galaxySpec.ArmPitchAngleDeg));
        if (System.Math.Abs(pitchTan) < 0.001)
        {
            return false;
        }

        double logRadius = System.Math.Log(System.Math.Max(radialDistance, 1.0));
        for (int index = 0; index < galaxySpec.NumArms; index += 1)
        {
            double offset = index * Mathf.Tau / galaxySpec.NumArms;
            double armTheta = offset + (logRadius / pitchTan);
            double delta = WrapAngle(theta - armTheta);
            if (System.Math.Abs(delta) <= galaxySpec.ArmWidth)
            {
                return true;
            }
        }

        return false;
    }

    private static double WrapAngle(double angle)
    {
        double tau = System.Math.PI * 2.0;
        double wrapped = angle % tau;
        if (wrapped > System.Math.PI)
        {
            wrapped -= tau;
        }

        if (wrapped < -System.Math.PI)
        {
            wrapped += tau;
        }

        return wrapped;
    }
}
