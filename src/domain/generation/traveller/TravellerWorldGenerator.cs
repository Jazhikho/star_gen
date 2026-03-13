using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Tables;
using StarGen.Domain.Math;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Traveller;

/// <summary>
/// Builds Traveller-style UWP profiles and maps them onto supported planet-generation fields.
/// Source rules: Traveller SRD world creation.
/// </summary>
public static class TravellerWorldGenerator
{
    private const double EarthAtmPa = 101325.0;

    /// <summary>
    /// Generates a deterministic Traveller world profile with optional fixed values.
    /// Fields set to negative values are rolled from the SRD tables.
    /// </summary>
    public static TravellerWorldProfile GeneratePlanetProfile(
        int requestedSizeCode,
        int requestedAtmosphereCode,
        int requestedHydrographicsCode,
        int requestedPopulationCode,
        OrbitZone.Zone orbitZone,
        SeededRng rng)
    {
        bool allAuto = requestedSizeCode < 0
            && requestedAtmosphereCode < 0
            && requestedHydrographicsCode < 0
            && requestedPopulationCode < 0;

        for (int attempt = 0; attempt < 2; attempt += 1)
        {
            TravellerWorldProfile profile = new TravellerWorldProfile();
            profile.SizeCode = ResolveCode(requestedSizeCode, Roll2D6(rng) - 2, 0, 10);
            profile.AtmosphereCode = ResolveCode(
                requestedAtmosphereCode,
                Roll2D6(rng) - 7 + profile.SizeCode,
                0,
                15);

            int hydroRoll = Roll2D6(rng) - 7 + profile.SizeCode;
            if (requestedHydrographicsCode < 0)
            {
                if (profile.SizeCode <= 1)
                {
                    hydroRoll = 0;
                }

                if (profile.AtmosphereCode == 0
                    || profile.AtmosphereCode == 1
                    || profile.AtmosphereCode == 10
                    || profile.AtmosphereCode == 11
                    || profile.AtmosphereCode == 12)
                {
                    hydroRoll -= 4;
                }

                if (orbitZone == OrbitZone.Zone.Hot)
                {
                    hydroRoll -= 2;
                }
            }

            profile.HydrographicsCode = ResolveCode(requestedHydrographicsCode, hydroRoll, 0, 10);
            profile.PopulationCode = ResolveCode(requestedPopulationCode, Roll2D6(rng) - 2, 0, 12);

            if (profile.PopulationCode <= 0)
            {
                profile.GovernmentCode = 0;
                profile.LawCode = 0;
                profile.TechLevelCode = 0;
                profile.StarportCode = "X";
            }
            else
            {
                profile.GovernmentCode = System.Math.Clamp(Roll2D6(rng) - 7 + profile.PopulationCode, 0, 15);
                profile.LawCode = System.Math.Clamp(Roll2D6(rng) - 7 + profile.GovernmentCode, 0, 15);
                profile.StarportCode = RollStarport(rng);
                profile.TechLevelCode = ResolveTechLevel(profile, rng);
            }

            if (!allAuto)
            {
                return profile;
            }

            if (!IsAllZeroProfile(profile))
            {
                return profile;
            }
        }

        TravellerWorldProfile fallbackProfile = new TravellerWorldProfile();
        fallbackProfile.SizeCode = 1;
        fallbackProfile.AtmosphereCode = 0;
        fallbackProfile.HydrographicsCode = 0;
        fallbackProfile.PopulationCode = 0;
        fallbackProfile.GovernmentCode = 0;
        fallbackProfile.LawCode = 0;
        fallbackProfile.TechLevelCode = 0;
        fallbackProfile.StarportCode = "X";
        return fallbackProfile;
    }

    /// <summary>
    /// Applies a Traveller world profile to a planet specification using supported generator fields.
    /// </summary>
    public static void ApplyToPlanetSpec(
        PlanetSpec spec,
        TravellerWorldProfile profile,
        SeededRng rng)
    {
        if (spec == null)
        {
            throw new System.ArgumentNullException(nameof(spec));
        }

        if (profile == null)
        {
            throw new System.ArgumentNullException(nameof(profile));
        }

        SizeCategory.Category sizeCategory = MapSizeCodeToSizeCategory(profile.SizeCode);
        spec.SizeCategory = (int)sizeCategory;

        double radiusM = ResolveRadiusMeters(profile.SizeCode, rng);
        double densityKgM3 = SizeTable.RandomDensity(sizeCategory, rng);
        double massKg = (4.0 / 3.0) * System.Math.PI * System.Math.Pow(radiusM, 3.0) * densityKgM3;

        spec.SetOverride("physical.radius_m", radiusM);
        spec.SetOverride("physical.mass_kg", massKg);
        spec.SetOverride("physical.density_kg_m3", densityKgM3);

        if (profile.AtmosphereCode <= 0)
        {
            spec.HasAtmosphere = false;
            spec.SetOverride("atmosphere.surface_pressure_pa", 0.0);
        }
        else
        {
            spec.HasAtmosphere = true;
            spec.SetOverride("atmosphere.surface_pressure_pa", ResolvePressurePa(profile.AtmosphereCode));
        }

        double oceanCoverage = ResolveHydrographicsCoverage(profile.HydrographicsCode);
        spec.SetOverride("surface.hydrosphere.ocean_coverage", oceanCoverage);

        double iceCoverage = 0.0;
        if (spec.OrbitZone == (int)OrbitZone.Zone.Cold)
        {
            iceCoverage = System.Math.Clamp(0.2 + ((10.0 - profile.HydrographicsCode) * 0.04), 0.0, 0.8);
        }
        spec.SetOverride("surface.hydrosphere.ice_coverage", iceCoverage);
        ApplyConditionalPlanetDefaults(spec, profile, rng);
    }

    /// <summary>
    /// Applies non-UWP defaults that still make sense for the generated world conditions.
    /// </summary>
    public static void ApplyConditionalPlanetDefaults(
        PlanetSpec spec,
        TravellerWorldProfile profile,
        SeededRng rng)
    {
        if (spec == null)
        {
            throw new System.ArgumentNullException(nameof(spec));
        }

        if (profile == null)
        {
            throw new System.ArgumentNullException(nameof(profile));
        }

        if (spec.HasRings.VariantType == Variant.Type.Nil)
        {
            bool shouldHaveRings = ShouldGenerateRings(spec, profile, rng);
            spec.HasRings = shouldHaveRings;
        }

        if (spec.RingComplexity < 0)
        {
            if (spec.HasRings.VariantType == Variant.Type.Bool && (bool)spec.HasRings)
            {
                spec.RingComplexity = ChooseRingComplexity(profile, rng);
            }
        }
    }

    /// <summary>
    /// Returns the stored Traveller profile from provenance, if present.
    /// </summary>
    public static TravellerWorldProfile? TryGetStoredProfile(CelestialBody body)
    {
        if (body.Provenance == null)
        {
            return null;
        }

        if (!body.Provenance.SpecSnapshot.ContainsKey("traveller_world_profile"))
        {
            return null;
        }

        Variant profileVariant = body.Provenance.SpecSnapshot["traveller_world_profile"];
        if (profileVariant.VariantType != Variant.Type.Dictionary)
        {
            return null;
        }

        return TravellerWorldProfile.FromDictionary((Dictionary)profileVariant);
    }

    /// <summary>
    /// Derives a Traveller-style world profile from the current body state.
    /// </summary>
    public static TravellerWorldProfile DeriveFromBody(CelestialBody body)
    {
        TravellerWorldProfile profile = new TravellerWorldProfile();
        profile.SizeCode = DeriveSizeCode(body);
        profile.AtmosphereCode = DeriveAtmosphereCode(body);
        profile.HydrographicsCode = DeriveHydrographicsCode(body);
        profile.PopulationCode = DerivePopulationCode(body);
        profile.GovernmentCode = DeriveGovernmentCode(body, profile.PopulationCode);
        profile.LawCode = DeriveLawCode(body, profile.GovernmentCode);
        profile.TechLevelCode = DeriveTechLevelCode(body);
        profile.StarportCode = DeriveStarportCode(body, profile.PopulationCode);
        return profile;
    }

    /// <summary>
    /// Returns a stable atmosphere label used in inspector readouts.
    /// </summary>
    public static string DescribeAtmosphereCode(int code)
    {
        return code switch
        {
            0 => "None",
            1 => "Trace",
            2 => "Very Thin, Tainted",
            3 => "Very Thin",
            4 => "Thin, Tainted",
            5 => "Thin",
            6 => "Standard",
            7 => "Standard, Tainted",
            8 => "Dense",
            9 => "Dense, Tainted",
            10 => "Exotic",
            11 => "Corrosive",
            12 => "Insidious",
            13 => "Dense, High",
            14 => "Thin, Low",
            15 => "Unusual",
            _ => "Unknown",
        };
    }

    private static int ResolveCode(int requestedCode, int rolledCode, int minValue, int maxValue)
    {
        if (requestedCode >= 0)
        {
            return System.Math.Clamp(requestedCode, minValue, maxValue);
        }

        return System.Math.Clamp(rolledCode, minValue, maxValue);
    }

    private static bool IsAllZeroProfile(TravellerWorldProfile profile)
    {
        if (profile.SizeCode != 0)
        {
            return false;
        }

        if (profile.AtmosphereCode != 0)
        {
            return false;
        }

        if (profile.HydrographicsCode != 0)
        {
            return false;
        }

        return profile.PopulationCode == 0;
    }

    private static bool ShouldGenerateRings(PlanetSpec spec, TravellerWorldProfile profile, SeededRng rng)
    {
        if (profile.SizeCode <= 3)
        {
            return false;
        }

        int threshold = 16;
        if (profile.SizeCode >= 9)
        {
            threshold = 8;
        }
        else if (profile.SizeCode >= 7)
        {
            threshold = 11;
        }
        else if (profile.SizeCode >= 5)
        {
            threshold = 13;
        }

        if (spec.OrbitZone == (int)OrbitZone.Zone.Cold)
        {
            threshold -= 2;
        }

        if (threshold < 4)
        {
            threshold = 4;
        }

        return rng.RandiRange(1, 20) >= threshold;
    }

    private static int ChooseRingComplexity(TravellerWorldProfile profile, SeededRng rng)
    {
        int roll = rng.RandiRange(1, 6);
        if (profile.SizeCode >= 9)
        {
            if (roll >= 5)
            {
                return (int)RingComplexity.Level.Complex;
            }

            if (roll >= 3)
            {
                return (int)RingComplexity.Level.Simple;
            }

            return (int)RingComplexity.Level.Trace;
        }

        if (roll >= 5)
        {
            return (int)RingComplexity.Level.Simple;
        }

        return (int)RingComplexity.Level.Trace;
    }

    private static int ResolveTechLevel(TravellerWorldProfile profile, SeededRng rng)
    {
        int techLevel = rng.RandiRange(1, 6);
        techLevel += GetStarportTechDm(profile.StarportCode);
        techLevel += GetSizeTechDm(profile.SizeCode);
        techLevel += GetAtmosphereTechDm(profile.AtmosphereCode);
        techLevel += GetHydrographicsTechDm(profile.HydrographicsCode);
        techLevel += GetPopulationTechDm(profile.PopulationCode);
        techLevel += GetGovernmentTechDm(profile.GovernmentCode);
        return System.Math.Clamp(techLevel, 0, 15);
    }

    private static int GetStarportTechDm(string starportCode)
    {
        string code = starportCode.Trim().ToUpperInvariant();
        if (code == "A")
        {
            return 6;
        }

        if (code == "B")
        {
            return 4;
        }

        if (code == "C")
        {
            return 2;
        }

        if (code == "X")
        {
            return -4;
        }

        return 0;
    }

    private static int GetSizeTechDm(int sizeCode)
    {
        if (sizeCode == 0 || sizeCode == 1)
        {
            return 2;
        }

        if (sizeCode == 2 || sizeCode == 3 || sizeCode == 4)
        {
            return 1;
        }

        return 0;
    }

    private static int GetAtmosphereTechDm(int atmosphereCode)
    {
        if (atmosphereCode <= 3)
        {
            return 1;
        }

        if (atmosphereCode >= 10)
        {
            return 1;
        }

        return 0;
    }

    private static int GetHydrographicsTechDm(int hydroCode)
    {
        if (hydroCode == 0)
        {
            return 1;
        }

        if (hydroCode == 9)
        {
            return 1;
        }

        if (hydroCode == 10)
        {
            return 2;
        }

        return 0;
    }

    private static int GetPopulationTechDm(int populationCode)
    {
        if (populationCode >= 1 && populationCode <= 5)
        {
            return 1;
        }

        if (populationCode == 9)
        {
            return 1;
        }

        if (populationCode == 10)
        {
            return 2;
        }

        if (populationCode == 11)
        {
            return 3;
        }

        if (populationCode == 12)
        {
            return 4;
        }

        return 0;
    }

    private static int GetGovernmentTechDm(int governmentCode)
    {
        if (governmentCode == 0)
        {
            return 1;
        }

        if (governmentCode == 5)
        {
            return 1;
        }

        if (governmentCode == 7)
        {
            return 2;
        }

        if (governmentCode == 13 || governmentCode == 14)
        {
            return -2;
        }

        return 0;
    }

    private static string RollStarport(SeededRng rng)
    {
        int roll = Roll2D6(rng);
        if (roll <= 2)
        {
            return "X";
        }

        if (roll <= 4)
        {
            return "E";
        }

        if (roll <= 6)
        {
            return "D";
        }

        if (roll <= 8)
        {
            return "C";
        }

        if (roll <= 10)
        {
            return "B";
        }

        return "A";
    }

    private static int Roll2D6(SeededRng rng)
    {
        return rng.RandiRange(1, 6) + rng.RandiRange(1, 6);
    }

    private static SizeCategory.Category MapSizeCodeToSizeCategory(int sizeCode)
    {
        if (sizeCode <= 2)
        {
            return SizeCategory.Category.Dwarf;
        }

        if (sizeCode <= 5)
        {
            return SizeCategory.Category.SubTerrestrial;
        }

        if (sizeCode <= 8)
        {
            return SizeCategory.Category.Terrestrial;
        }

        if (sizeCode <= 10)
        {
            return SizeCategory.Category.SuperEarth;
        }

        if (sizeCode <= 12)
        {
            return SizeCategory.Category.MiniNeptune;
        }

        return SizeCategory.Category.NeptuneClass;
    }

    private static double ResolveRadiusMeters(int sizeCode, SeededRng rng)
    {
        object lookupCode;
        if (sizeCode < 10)
        {
            lookupCode = sizeCode;
        }
        else
        {
            lookupCode = TravellerWorldProfile.ToHexDigit(sizeCode);
        }

        Dictionary<string, double> range = TravellerSizeCode.CodeToDiameterRange(lookupCode);
        double minKm = 0.0;
        if (range.TryGetValue("min", out double minValue))
        {
            minKm = minValue;
        }

        double maxKm = minKm;
        if (range.TryGetValue("max", out double maxValue))
        {
            maxKm = maxValue;
        }

        if (maxKm <= minKm)
        {
            maxKm = minKm + 400.0;
        }

        double diameterKm = rng.RandfRange((float)minKm, (float)maxKm);
        return diameterKm * 500.0;
    }

    private static double ResolvePressurePa(int atmosphereCode)
    {
        return atmosphereCode switch
        {
            0 => 0.0,
            1 => 0.05 * EarthAtmPa,
            2 => 0.25 * EarthAtmPa,
            3 => 0.25 * EarthAtmPa,
            4 => 0.55 * EarthAtmPa,
            5 => 0.55 * EarthAtmPa,
            6 => 1.0 * EarthAtmPa,
            7 => 1.0 * EarthAtmPa,
            8 => 2.0 * EarthAtmPa,
            9 => 2.0 * EarthAtmPa,
            10 => 0.9 * EarthAtmPa,
            11 => 3.0 * EarthAtmPa,
            12 => 3.0 * EarthAtmPa,
            13 => 3.0 * EarthAtmPa,
            14 => 0.4 * EarthAtmPa,
            15 => 1.2 * EarthAtmPa,
            _ => EarthAtmPa,
        };
    }

    private static double ResolveHydrographicsCoverage(int hydrographicsCode)
    {
        return hydrographicsCode switch
        {
            <= 0 => 0.0,
            1 => 0.10,
            2 => 0.20,
            3 => 0.30,
            4 => 0.40,
            5 => 0.50,
            6 => 0.60,
            7 => 0.70,
            8 => 0.80,
            9 => 0.90,
            _ => 0.98,
        };
    }

    private static int DeriveSizeCode(CelestialBody body)
    {
        double diameterKm = body.Physical.RadiusM * 2.0 / 1000.0;
        object code = TravellerSizeCode.DiameterKmToCode(diameterKm);
        string codeString = TravellerSizeCode.ToStringUwp(code);
        return TravellerWorldProfile.ParseHexDigit(codeString);
    }

    private static int DeriveAtmosphereCode(CelestialBody body)
    {
        if (!body.HasAtmosphere() || body.Atmosphere == null)
        {
            return 0;
        }

        double pressureAtm = body.Atmosphere.SurfacePressurePa / EarthAtmPa;
        string dominantGas = body.Atmosphere.GetDominantGas().ToUpperInvariant();
        bool tainted = dominantGas == "CO2"
            || dominantGas == "SO2"
            || dominantGas == "CH4";

        if (pressureAtm < 0.001)
        {
            return 0;
        }

        if (pressureAtm < 0.1)
        {
            return 1;
        }

        if (pressureAtm <= 0.42)
        {
            if (tainted)
            {
                return 2;
            }

            return 3;
        }

        if (pressureAtm <= 0.7)
        {
            if (tainted)
            {
                return 4;
            }

            return 5;
        }

        if (pressureAtm <= 1.49)
        {
            if (tainted)
            {
                return 7;
            }

            return 6;
        }

        if (pressureAtm <= 2.49)
        {
            if (tainted)
            {
                return 9;
            }

            return 8;
        }

        if (pressureAtm <= 3.0)
        {
            return 13;
        }

        return 10;
    }

    private static int DeriveHydrographicsCode(CelestialBody body)
    {
        if (!body.HasSurface() || body.Surface == null)
        {
            return 0;
        }

        double coverage = 0.0;
        if (body.Surface.Hydrosphere != null)
        {
            coverage += body.Surface.Hydrosphere.OceanCoverage;
        }

        if (body.Surface.Cryosphere != null)
        {
            coverage += body.Surface.Cryosphere.PolarCapCoverage * 0.5;
        }

        coverage = System.Math.Clamp(coverage, 0.0, 1.0);
        return System.Math.Clamp((int)System.Math.Round(coverage * 10.0), 0, 10);
    }

    private static int DerivePopulationCode(CelestialBody body)
    {
        if (!body.HasPopulationData() || body.PopulationData == null)
        {
            TravellerWorldProfile? storedProfile = TryGetStoredProfile(body);
            if (storedProfile != null)
            {
                return storedProfile.PopulationCode;
            }

            return 0;
        }

        long population = body.PopulationData.GetTotalPopulation();
        if (population <= 0)
        {
            return 0;
        }

        int exponent = (int)System.Math.Floor(System.Math.Log10(population));
        return System.Math.Clamp(exponent, 0, 12);
    }

    private static int DeriveGovernmentCode(CelestialBody body, int populationCode)
    {
        if (populationCode <= 0)
        {
            return 0;
        }

        if (body.HasPopulationData() && body.PopulationData != null)
        {
            Variant dominant = body.PopulationData.GetDominantPopulation();
            if (dominant.Obj is Colony colony)
            {
                return MapGovernmentRegime(colony.Government.Regime);
            }

            if (dominant.Obj is NativePopulation nativePopulation)
            {
                return MapGovernmentRegime(nativePopulation.Government.Regime);
            }
        }

        TravellerWorldProfile? storedProfile = TryGetStoredProfile(body);
        if (storedProfile != null)
        {
            return storedProfile.GovernmentCode;
        }

        return 0;
    }

    private static int DeriveLawCode(CelestialBody body, int governmentCode)
    {
        if (governmentCode <= 0)
        {
            TravellerWorldProfile? storedProfile = TryGetStoredProfile(body);
            if (storedProfile != null)
            {
                return storedProfile.LawCode;
            }

            return 0;
        }

        if (body.HasPopulationData() && body.PopulationData != null)
        {
            Variant dominant = body.PopulationData.GetDominantPopulation();
            if (dominant.Obj is Colony colony)
            {
                return MapLawLevel(colony.Government);
            }

            if (dominant.Obj is NativePopulation nativePopulation)
            {
                return MapLawLevel(nativePopulation.Government);
            }
        }

        TravellerWorldProfile? stored = TryGetStoredProfile(body);
        if (stored != null)
        {
            return stored.LawCode;
        }

        return 0;
    }

    private static int DeriveTechLevelCode(CelestialBody body)
    {
        if (body.HasPopulationData() && body.PopulationData != null)
        {
            return MapTechLevel(body.PopulationData.GetHighestTechLevel());
        }

        TravellerWorldProfile? storedProfile = TryGetStoredProfile(body);
        if (storedProfile != null)
        {
            return storedProfile.TechLevelCode;
        }

        return 0;
    }

    private static string DeriveStarportCode(CelestialBody body, int populationCode)
    {
        TravellerWorldProfile? storedProfile = TryGetStoredProfile(body);
        if (storedProfile != null)
        {
            return storedProfile.StarportCode;
        }

        if (populationCode <= 0)
        {
            return "X";
        }

        if (populationCode >= 9)
        {
            return "B";
        }

        if (populationCode >= 6)
        {
            return "C";
        }

        if (populationCode >= 3)
        {
            return "D";
        }

        return "E";
    }

    private static int MapGovernmentRegime(GovernmentType.Regime regime)
    {
        return regime switch
        {
            GovernmentType.Regime.Tribal => 0,
            GovernmentType.Regime.Chiefdom => 1,
            GovernmentType.Regime.CityState => 2,
            GovernmentType.Regime.Feudal => 7,
            GovernmentType.Regime.Corporate => 1,
            GovernmentType.Regime.MassDemocracy => 2,
            GovernmentType.Regime.Oligarchic => 3,
            GovernmentType.Regime.Constitutional => 4,
            GovernmentType.Regime.Technocracy => 5,
            GovernmentType.Regime.MilitaryJunta => 6,
            GovernmentType.Regime.PersonalistDict => 6,
            GovernmentType.Regime.FailedState => 7,
            GovernmentType.Regime.BureaucraticEmpire => 8,
            GovernmentType.Regime.PatrimonialKingdom => 9,
            GovernmentType.Regime.AbsoluteMonarchy => 10,
            GovernmentType.Regime.OnePartyState => 11,
            GovernmentType.Regime.EliteRepublic => 12,
            GovernmentType.Regime.Theocracy => 13,
            _ => 0,
        };
    }

    private static int MapLawLevel(Government government)
    {
        double coercion = government.CoercionCentralization;
        double capacity = government.AdministrativeCapacity;
        double inclusiveness = government.PoliticalInclusiveness;
        double score = (coercion * 8.0) + (capacity * 4.0) + ((1.0 - inclusiveness) * 3.0);
        return System.Math.Clamp((int)System.Math.Round(score), 0, 15);
    }

    private static int MapTechLevel(TechnologyLevel.Level techLevel)
    {
        return techLevel switch
        {
            TechnologyLevel.Level.StoneAge => 0,
            TechnologyLevel.Level.BronzeAge => 1,
            TechnologyLevel.Level.IronAge => 2,
            TechnologyLevel.Level.Classical => 3,
            TechnologyLevel.Level.Medieval => 4,
            TechnologyLevel.Level.Renaissance => 5,
            TechnologyLevel.Level.Industrial => 6,
            TechnologyLevel.Level.Atomic => 6,
            TechnologyLevel.Level.Information => 7,
            TechnologyLevel.Level.Spacefaring => 8,
            TechnologyLevel.Level.Interstellar => 10,
            TechnologyLevel.Level.Advanced => 12,
            _ => 0,
        };
    }
}
