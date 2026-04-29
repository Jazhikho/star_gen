using Godot;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Resolves a scientifically grounded galaxy profile from family-lock inputs and realism tunables.
/// </summary>
public static class GalaxyRealismProfileBuilder
{
    /// <summary>
    /// Builds the resolved realism profile for the supplied configuration and seed.
    /// </summary>
    public static GalaxyRealismProfile Build(GalaxyConfig config, int galaxySeed)
    {
        SeededRng rng = new(galaxySeed ^ 0x5F3759DF);
        GalaxyRealismProfile profile = new GalaxyRealismProfile();
        profile.Family = config.Type;
        profile.HaloMassLog10Solar = System.Math.Clamp(config.HaloMassLog10Solar, 9.0, 13.8);
        profile.EnvironmentDensityIndex = System.Math.Clamp(config.EnvironmentDensityIndex, 0.0, 1.0);
        profile.StarFormationEfficiency = System.Math.Clamp(config.StarFormationEfficiency, 0.05, 0.65);
        profile.ClusterMassFunctionSlope = 2.0;
        profile.ClusterDissolutionTimescaleMyr = 10.0;

        if (config.Type == GalaxySpec.GalaxyType.Spiral)
        {
            BuildSpiralProfile(config, rng, profile);
        }
        else if (config.Type == GalaxySpec.GalaxyType.Elliptical)
        {
            BuildEllipticalProfile(config, rng, profile);
        }
        else if (config.Type == GalaxySpec.GalaxyType.Lenticular)
        {
            BuildLenticularProfile(config, rng, profile);
        }
        else
        {
            BuildIrregularProfile(config, rng, profile);
        }

        profile.GhzInnerRadiusPc = System.Math.Max(2000.0, config.GhzInnerRadiusPc);
        profile.GhzOuterRadiusPc = System.Math.Max(profile.GhzInnerRadiusPc + 1000.0, config.GhzOuterRadiusPc);
        profile.GhzTransitionWidthPc = System.Math.Max(500.0, config.GhzTransitionWidthPc);
        profile.MetallicityGradientDexPerKpc = System.Math.Clamp(config.MetallicityGradientDexPerKpc, -0.12, -0.005);
        return profile;
    }

    /// <summary>
    /// Applies the resolved realism profile back onto the generated galaxy specification.
    /// </summary>
    public static void ApplyToSpec(GalaxyConfig config, GalaxyRealismProfile profile, GalaxySpec spec)
    {
        spec.Type = config.Type;
        spec.SubtypeMode = config.SubtypeMode;
        spec.BarMode = config.BarMode;
        spec.ArmMechanismPreference = config.ArmMechanismPreference;
        spec.ResolvedSubtype = profile.ResolvedSubtype;
        spec.HaloMassLog10Solar = profile.HaloMassLog10Solar;
        spec.EnvironmentDensityIndex = profile.EnvironmentDensityIndex;
        spec.ArmMechanism = profile.ArmMechanism;
        spec.IsBarred = profile.IsBarred;
        spec.BarStrength = profile.BarStrength;
        spec.SersicIndex = profile.SersicIndex;
        spec.EffectiveRadiusPc = profile.EffectiveRadiusPc;
        spec.BulgeToTotal = profile.BulgeToTotal;
        spec.GhzInnerRadiusPc = profile.GhzInnerRadiusPc;
        spec.GhzOuterRadiusPc = profile.GhzOuterRadiusPc;
        spec.GhzTransitionWidthPc = profile.GhzTransitionWidthPc;
        spec.MetallicityGradientDexPerKpc = profile.MetallicityGradientDexPerKpc;
        spec.StarFormationEfficiency = profile.StarFormationEfficiency;
        spec.ClusterMassFunctionSlope = profile.ClusterMassFunctionSlope;
        spec.ClusterDissolutionTimescaleMyr = profile.ClusterDissolutionTimescaleMyr;
        spec.RealismProfile = profile;
        spec.StellarProfile = config.StellarProfile.Clone();

        spec.NumArms = config.NumArms;
        spec.ArmPitchAngleDeg = config.ArmPitchAngleDeg;
        spec.ArmAmplitude = config.ArmAmplitude;
        spec.BulgeIntensity = config.BulgeIntensity;
        spec.DiskScaleLengthPc = config.DiskScaleLengthPc;
        spec.DiskScaleHeightPc = config.DiskScaleHeightPc;
        spec.BulgeRadiusPc = config.BulgeRadiusPc;
        spec.RadiusPc = config.RadiusPc;
        spec.Ellipticity = config.Ellipticity;
        spec.IrregularityScale = config.IrregularityScale;
        spec.HeightPc = ResolveHeight(config, profile);
        spec.BulgeHeightPc = ResolveBulgeHeight(config, profile);
        spec.ArmWidth = ResolveArmWidth(profile);
        if (spec.Type == GalaxySpec.GalaxyType.Lenticular)
        {
            spec.NumArms = 0;
            spec.ArmAmplitude = 0.0;
        }
    }

    /// <summary>
    /// Returns a descriptive label for the resolved subtype.
    /// </summary>
    public static string GetSubtypeLabel(GalaxyResolvedSubtype subtype)
    {
        if (subtype == GalaxyResolvedSubtype.SpiralSa)
        {
            return "Sa";
        }

        if (subtype == GalaxyResolvedSubtype.SpiralSb)
        {
            return "Sb";
        }

        if (subtype == GalaxyResolvedSubtype.SpiralSc)
        {
            return "Sc";
        }

        if (subtype == GalaxyResolvedSubtype.SpiralSd)
        {
            return "Sd";
        }

        if (subtype == GalaxyResolvedSubtype.EllipticalDwarf)
        {
            return "Dwarf Elliptical";
        }

        if (subtype == GalaxyResolvedSubtype.EllipticalIntermediate)
        {
            return "Intermediate Elliptical";
        }

        if (subtype == GalaxyResolvedSubtype.EllipticalGiant)
        {
            return "Giant Elliptical";
        }

        if (subtype == GalaxyResolvedSubtype.LenticularS0)
        {
            return "S0";
        }

        if (subtype == GalaxyResolvedSubtype.LenticularS0a)
        {
            return "S0/a";
        }

        if (subtype == GalaxyResolvedSubtype.IrregularMagellanic)
        {
            return "Magellanic Irregular";
        }

        if (subtype == GalaxyResolvedSubtype.DwarfIrregular)
        {
            return "Dwarf Irregular";
        }

        if (subtype == GalaxyResolvedSubtype.DwarfSpheroidal)
        {
            return "Dwarf Spheroidal";
        }

        return "Scientific Default";
    }

    private static void BuildSpiralProfile(GalaxyConfig config, SeededRng rng, GalaxyRealismProfile profile)
    {
        double morphologyBias = GetSubtypeBias(config.SubtypeMode, profile.HaloMassLog10Solar, profile.EnvironmentDensityIndex);
        if (morphologyBias < 0.25)
        {
            profile.ResolvedSubtype = GalaxyResolvedSubtype.SpiralSd;
            profile.BulgeToTotal = 0.08;
            profile.SersicIndex = 1.5;
        }
        else if (morphologyBias < 0.50)
        {
            profile.ResolvedSubtype = GalaxyResolvedSubtype.SpiralSc;
            profile.BulgeToTotal = 0.15;
            profile.SersicIndex = 1.8;
        }
        else if (morphologyBias < 0.75)
        {
            profile.ResolvedSubtype = GalaxyResolvedSubtype.SpiralSb;
            profile.BulgeToTotal = 0.28;
            profile.SersicIndex = 2.2;
        }
        else
        {
            profile.ResolvedSubtype = GalaxyResolvedSubtype.SpiralSa;
            profile.BulgeToTotal = 0.42;
            profile.SersicIndex = 3.0;
        }

        profile.IsBarred = ResolveBarState(config.BarMode, 0.65, rng);
        // Diaz-Garcia et al. (2016) measure bar strength with Q_b and show that bar structure
        // co-varies with host-galaxy morphology and bulge prominence. Tuning: this
        // `0.35 + 0.6 * BulgeToTotal` blend is a StarGen calibration for the resolved profile,
        // not a published law, and remains pending human verification in the science-audit pass.
        profile.BarStrength = profile.IsBarred ? 0.35 + (profile.BulgeToTotal * 0.6) : 0.0;
        profile.ArmMechanism = ResolveArmMechanism(config.ArmMechanismPreference, profile.ResolvedSubtype, rng);
        profile.EffectiveRadiusPc = ResolveDiskScaleLength(profile.HaloMassLog10Solar, profile.ResolvedSubtype);
        profile.CharacteristicAgeGyr = 4.5 + (profile.BulgeToTotal * 6.0);
    }

    private static void BuildEllipticalProfile(GalaxyConfig config, SeededRng rng, GalaxyRealismProfile profile)
    {
        double mass = profile.HaloMassLog10Solar;
        if (mass < 10.4)
        {
            profile.ResolvedSubtype = GalaxyResolvedSubtype.EllipticalDwarf;
            profile.SersicIndex = 2.0 + (rng.Randf() * 0.8);
            profile.BulgeToTotal = 1.0;
            profile.CharacteristicAgeGyr = 8.0;
        }
        else if (mass < 12.2)
        {
            profile.ResolvedSubtype = GalaxyResolvedSubtype.EllipticalIntermediate;
            // Kormendy et al. (2009) treat Sersic profiles as the main-body description for
            // elliptical galaxies and distinguish lower-luminosity/coreless ellipticals from
            // giant/core systems. Tuning: the `3.2 + rand * 1.2` range is StarGen's compact
            // intermediate-elliptical band within that framework and remains pending human
            // verification of the specific range choice.
            profile.SersicIndex = 3.2 + (rng.Randf() * 1.2);
            profile.BulgeToTotal = 1.0;
            profile.CharacteristicAgeGyr = 10.0;
        }
        else
        {
            profile.ResolvedSubtype = GalaxyResolvedSubtype.EllipticalGiant;
            // Kormendy et al. (2009) describe giant ellipticals as a distinct structural family
            // whose main bodies are still well fit by Sersic functions. Tuning: the
            // `4.0 + rand * 1.5` range is StarGen's giant-elliptical calibration band rather
            // than a direct transcription of a published fit, and remains pending human
            // verification in the audit pass.
            profile.SersicIndex = 4.0 + (rng.Randf() * 1.5);
            profile.BulgeToTotal = 1.0;
            profile.CharacteristicAgeGyr = 11.5;
        }

        profile.ArmMechanism = GalaxyArmMechanism.Auto;
        profile.IsBarred = false;
        profile.BarStrength = 0.0;
        profile.EffectiveRadiusPc = ResolveEllipticalEffectiveRadius(profile.HaloMassLog10Solar, rng);
    }

    private static void BuildLenticularProfile(GalaxyConfig config, SeededRng rng, GalaxyRealismProfile profile)
    {
        if (config.SubtypeMode == GalaxySubtypeMode.LateType)
        {
            profile.ResolvedSubtype = GalaxyResolvedSubtype.LenticularS0a;
            profile.BulgeToTotal = 0.35;
            profile.SersicIndex = 2.4;
        }
        else
        {
            profile.ResolvedSubtype = GalaxyResolvedSubtype.LenticularS0;
            profile.BulgeToTotal = 0.48;
            profile.SersicIndex = 3.1;
        }

        profile.IsBarred = ResolveBarState(config.BarMode, 0.45, rng);
        profile.BarStrength = profile.IsBarred ? 0.28 + (rng.Randf() * 0.18) : 0.0;
        profile.ArmMechanism = GalaxyArmMechanism.Auto;
        profile.EffectiveRadiusPc = ResolveDiskScaleLength(profile.HaloMassLog10Solar, GalaxyResolvedSubtype.SpiralSa) * 0.9;
        profile.CharacteristicAgeGyr = 8.5 + (profile.EnvironmentDensityIndex * 2.0);
    }

    private static void BuildIrregularProfile(GalaxyConfig config, SeededRng rng, GalaxyRealismProfile profile)
    {
        double mass = profile.HaloMassLog10Solar;
        if (mass < 9.8)
        {
            profile.ResolvedSubtype = GalaxyResolvedSubtype.DwarfSpheroidal;
            profile.SersicIndex = 1.0;
            profile.BulgeToTotal = 0.9;
            profile.CharacteristicAgeGyr = 9.0;
        }
        else if (mass < 10.5)
        {
            profile.ResolvedSubtype = GalaxyResolvedSubtype.DwarfIrregular;
            profile.SersicIndex = 1.1;
            profile.BulgeToTotal = 0.05;
            profile.CharacteristicAgeGyr = 4.0;
        }
        else
        {
            profile.ResolvedSubtype = GalaxyResolvedSubtype.IrregularMagellanic;
            profile.SersicIndex = 1.0;
            profile.BulgeToTotal = 0.03;
            profile.CharacteristicAgeGyr = 3.0 + (rng.Randf() * 2.0);
        }

        profile.ArmMechanism = GalaxyArmMechanism.Auto;
        profile.IsBarred = false;
        profile.BarStrength = 0.0;
        profile.EffectiveRadiusPc = 1200.0 + (System.Math.Max(0.0, mass - 9.0) * 900.0);
    }

    private static double GetSubtypeBias(GalaxySubtypeMode subtypeMode, double haloMassLog10Solar, double environmentDensityIndex)
    {
        if (subtypeMode == GalaxySubtypeMode.EarlyType)
        {
            return 0.9;
        }

        if (subtypeMode == GalaxySubtypeMode.IntermediateType)
        {
            return 0.55;
        }

        if (subtypeMode == GalaxySubtypeMode.LateType)
        {
            return 0.15;
        }

        double massBias = (haloMassLog10Solar - 9.5) / 4.0;
        double environmentBias = environmentDensityIndex * 0.35;
        return System.Math.Clamp((massBias * 0.7) + environmentBias, 0.0, 1.0);
    }

    private static bool ResolveBarState(GalaxyBarMode barMode, double defaultProbability, SeededRng rng)
    {
        if (barMode == GalaxyBarMode.PreferBarred)
        {
            return true;
        }

        if (barMode == GalaxyBarMode.PreferUnbarred)
        {
            return false;
        }

        return rng.Randf() <= defaultProbability;
    }

    private static GalaxyArmMechanism ResolveArmMechanism(
        GalaxyArmMechanism preference,
        GalaxyResolvedSubtype subtype,
        SeededRng rng)
    {
        if (preference != GalaxyArmMechanism.Auto)
        {
            return preference;
        }

        if (subtype == GalaxyResolvedSubtype.SpiralSa || subtype == GalaxyResolvedSubtype.SpiralSb)
        {
            if (rng.Randf() < 0.55f)
            {
                return GalaxyArmMechanism.GrandDesign;
            }

            return GalaxyArmMechanism.MultiArmed;
        }

        if (subtype == GalaxyResolvedSubtype.SpiralSc)
        {
            if (rng.Randf() < 0.35f)
            {
                return GalaxyArmMechanism.GrandDesign;
            }

            return GalaxyArmMechanism.MultiArmed;
        }

        return GalaxyArmMechanism.Flocculent;
    }

    private static double ResolveDiskScaleLength(double haloMassLog10Solar, GalaxyResolvedSubtype subtype)
    {
        double baseLengthPc = 2500.0 + System.Math.Max(0.0, haloMassLog10Solar - 10.0) * 1200.0;
        if (subtype == GalaxyResolvedSubtype.SpiralSa)
        {
            return baseLengthPc * 1.15;
        }

        if (subtype == GalaxyResolvedSubtype.SpiralSb)
        {
            return baseLengthPc;
        }

        if (subtype == GalaxyResolvedSubtype.SpiralSc)
        {
            return baseLengthPc * 0.9;
        }

        return baseLengthPc * 0.78;
    }

    private static double ResolveEllipticalEffectiveRadius(double haloMassLog10Solar, SeededRng rng)
    {
        double logRadiusPc = 3.2 + ((haloMassLog10Solar - 11.0) * 0.32) + (rng.Randfn(0.0f, 0.06f));
        return System.Math.Pow(10.0, logRadiusPc);
    }

    private static double ResolveHeight(GalaxyConfig config, GalaxyRealismProfile profile)
    {
        if (config.Type == GalaxySpec.GalaxyType.Elliptical)
        {
            return profile.EffectiveRadiusPc * 0.95;
        }

        if (config.Type == GalaxySpec.GalaxyType.Lenticular)
        {
            return System.Math.Max(500.0, config.RadiusPc / 18.0);
        }

        if (config.Type == GalaxySpec.GalaxyType.Irregular)
        {
            return System.Math.Max(800.0, config.RadiusPc / 9.0);
        }

        return System.Math.Max(500.0, config.RadiusPc / 15.0);
    }

    private static double ResolveBulgeHeight(GalaxyConfig config, GalaxyRealismProfile profile)
    {
        if (config.Type == GalaxySpec.GalaxyType.Elliptical)
        {
            return profile.EffectiveRadiusPc * 0.75;
        }

        if (config.Type == GalaxySpec.GalaxyType.Lenticular)
        {
            return config.BulgeRadiusPc * 0.58;
        }

        if (config.Type == GalaxySpec.GalaxyType.Irregular)
        {
            return config.BulgeRadiusPc * 0.80;
        }

        return config.BulgeRadiusPc * 0.53;
    }

    private static double ResolveArmWidth(GalaxyRealismProfile profile)
    {
        if (profile.ArmMechanism == GalaxyArmMechanism.GrandDesign)
        {
            return 0.28;
        }

        if (profile.ArmMechanism == GalaxyArmMechanism.MultiArmed)
        {
            return 0.40;
        }

        return 0.62;
    }
}
