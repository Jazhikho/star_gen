using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Pure deterministic calculations for building planet-profile data from celestial bodies.
/// </summary>
public static class ProfileCalculations
{
    /// <summary>
    /// Earth's surface gravity in meters per second squared.
    /// </summary>
    public const double EarthGravity = 9.81;

    /// <summary>
    /// Earth's atmospheric pressure in Pascals.
    /// </summary>
    public const double EarthPressurePa = 101325.0;

    /// <summary>
    /// Earth's magnetic moment in ampere-square-meters.
    /// </summary>
    public const double EarthMagneticMoment = 8.0e22;

    /// <summary>
    /// Water freezing point in Kelvin.
    /// </summary>
    public const double WaterFreezeK = 273.15;

    /// <summary>
    /// Water boiling point in Kelvin at one atmosphere.
    /// </summary>
    public const double WaterBoilK = 373.15;

    /// <summary>
    /// Seconds per hour.
    /// </summary>
    public const double SecondsPerHour = 3600.0;

    /// <summary>
    /// Minimum pressure in Earth atmospheres to count as having an atmosphere.
    /// </summary>
    public const double MinAtmosphereAtm = 0.001;

    /// <summary>
    /// Calculates the derived 0-10 habitability score.
    /// </summary>
    public static int CalculateHabitabilityScore(
        double tempK,
        double pressureAtm,
        double gravityG,
        bool hasLiquidWater,
        bool hasBreathable,
        double radiationLevel,
        double oceanCoverage)
    {
        double score = 0.0;
        double tempC = tempK - 273.15;

        if (tempC >= -20.0 && tempC <= 50.0)
        {
            score += 1.0;
        }

        if (tempC >= 0.0 && tempC <= 40.0)
        {
            score += 1.0;
        }

        if (tempC >= 10.0 && tempC <= 30.0)
        {
            score += 1.0;
        }

        if (pressureAtm >= 0.1 && pressureAtm <= 5.0)
        {
            score += 1.0;
        }

        if (pressureAtm >= 0.5 && pressureAtm <= 2.0)
        {
            score += 1.0;
        }

        if (hasLiquidWater)
        {
            score += 1.0;
        }

        if (oceanCoverage >= 0.1 && oceanCoverage <= 0.9)
        {
            score += 1.0;
        }

        if (gravityG >= 0.5 && gravityG <= 1.5)
        {
            score += 1.0;
        }

        if (hasBreathable)
        {
            score += 1.0;
        }

        if (radiationLevel < 0.3)
        {
            score += 1.0;
        }

        return System.Math.Clamp((int)System.Math.Round(score), 0, 10);
    }

    /// <summary>
    /// Calculates weather severity from atmosphere and rotation.
    /// </summary>
    public static double CalculateWeatherSeverity(double pressureAtm, double rotationPeriodS, bool hasAtmosphere)
    {
        if (!hasAtmosphere || pressureAtm < MinAtmosphereAtm)
        {
            return 0.0;
        }

        double atmosphereFactor = System.Math.Clamp(pressureAtm * 0.3, 0.0, 0.5);
        double rotationFactor = 0.0;
        const double earthDay = 86400.0;

        if (System.Math.Abs(rotationPeriodS) > 0.0)
        {
            double dayRatio = earthDay / System.Math.Abs(rotationPeriodS);
            rotationFactor = System.Math.Clamp(dayRatio * 0.2, 0.0, 0.4);
        }

        if (System.Math.Abs(rotationPeriodS) > earthDay * 10.0)
        {
            rotationFactor = 0.3;
        }

        return System.Math.Clamp(atmosphereFactor + rotationFactor, 0.0, 1.0);
    }

    /// <summary>
    /// Calculates magnetic-field strength normalized to Earth.
    /// </summary>
    public static double CalculateMagneticStrength(double magneticMoment)
    {
        if (magneticMoment <= 0.0)
        {
            return 0.0;
        }

        return System.Math.Clamp(magneticMoment / EarthMagneticMoment, 0.0, 1.0);
    }

    /// <summary>
    /// Calculates surface radiation level from magnetic field and atmosphere.
    /// </summary>
    public static double CalculateRadiationLevel(double magneticMoment, double pressureAtm, bool hasAtmosphere)
    {
        double magneticProtection = System.Math.Clamp(magneticMoment / EarthMagneticMoment, 0.0, 1.0);
        double atmosphereProtection = 0.0;
        if (hasAtmosphere)
        {
            atmosphereProtection = System.Math.Clamp(pressureAtm * 0.5, 0.0, 0.5);
        }

        double totalProtection = 1.0 - ((1.0 - magneticProtection) * (1.0 - atmosphereProtection));
        return System.Math.Clamp(1.0 - totalProtection, 0.0, 1.0);
    }

    /// <summary>
    /// Estimates continent count from terrain properties and land coverage.
    /// </summary>
    public static int EstimateContinentCount(double tectonicActivity, double landCoverage, bool hasTerrain)
    {
        if (!hasTerrain || landCoverage < 0.01)
        {
            return 0;
        }

        int baseCount = 1;
        if (landCoverage > 0.15)
        {
            baseCount = 2;
        }

        if (landCoverage > 0.30)
        {
            baseCount = 3;
        }

        if (landCoverage > 0.50)
        {
            baseCount = 5;
        }

        if (landCoverage > 0.70)
        {
            baseCount = 7;
        }

        double tectonicModifier = 1.0 + tectonicActivity;
        return System.Math.Max(1, (int)System.Math.Round(baseCount * tectonicModifier));
    }

    /// <summary>
    /// Calculates climate-zone coverage from axial tilt and temperature.
    /// </summary>
    public static Array<Dictionary> CalculateClimateZones(double axialTiltDeg, double avgTempK, bool hasAtmosphere)
    {
        Array<Dictionary> zones = new();

        if (!hasAtmosphere)
        {
            zones.Add(new Dictionary { ["zone"] = (int)ClimateZone.Zone.Extreme, ["coverage"] = 1.0 });
            return zones;
        }

        if (avgTempK < 200.0)
        {
            zones.Add(new Dictionary { ["zone"] = (int)ClimateZone.Zone.Polar, ["coverage"] = 1.0 });
            return zones;
        }

        if (avgTempK > 400.0)
        {
            zones.Add(new Dictionary { ["zone"] = (int)ClimateZone.Zone.Arid, ["coverage"] = 1.0 });
            return zones;
        }

        double tiltFactor = System.Math.Clamp(axialTiltDeg / 23.5, 0.5, 2.0);
        double polar = System.Math.Clamp(0.1 * tiltFactor, 0.05, 0.25);
        double subpolar = System.Math.Clamp(0.1 * tiltFactor, 0.05, 0.15);
        double temperate = System.Math.Clamp(0.25 + (0.05 * tiltFactor), 0.15, 0.35);
        double subtropical = System.Math.Clamp(0.15, 0.1, 0.2);
        double tropical = System.Math.Max(1.0 - ((polar + subpolar + temperate + subtropical) * 2.0), 0.1);

        double tempC = avgTempK - 273.15;
        if (tempC < 0.0)
        {
            double coldFactor = System.Math.Clamp(-tempC / 30.0, 0.0, 1.0);
            polar += coldFactor * 0.2;
            tropical = System.Math.Max(tropical - (coldFactor * 0.2), 0.05);
        }
        else if (tempC > 30.0)
        {
            double hotFactor = System.Math.Clamp((tempC - 30.0) / 30.0, 0.0, 1.0);
            tropical += hotFactor * 0.2;
            polar = System.Math.Max(polar - (hotFactor * 0.1), 0.02);
        }

        zones.Add(new Dictionary { ["zone"] = (int)ClimateZone.Zone.Polar, ["coverage"] = polar * 2.0 });
        zones.Add(new Dictionary { ["zone"] = (int)ClimateZone.Zone.Subpolar, ["coverage"] = subpolar * 2.0 });
        zones.Add(new Dictionary { ["zone"] = (int)ClimateZone.Zone.Temperate, ["coverage"] = temperate * 2.0 });
        zones.Add(new Dictionary { ["zone"] = (int)ClimateZone.Zone.Subtropical, ["coverage"] = subtropical * 2.0 });
        zones.Add(new Dictionary { ["zone"] = (int)ClimateZone.Zone.Tropical, ["coverage"] = tropical });

        double total = 0.0;
        foreach (Dictionary zoneData in zones)
        {
            total += (double)zoneData["coverage"];
        }

        if (total > 0.0)
        {
            foreach (Dictionary zoneData in zones)
            {
                zoneData["coverage"] = (double)zoneData["coverage"] / total;
            }
        }

        return zones;
    }

    /// <summary>
    /// Calculates biome coverage from climate and surface data.
    /// </summary>
    public static Dictionary CalculateBiomes(
        Array<Dictionary> climateZones,
        double oceanCoverage,
        double iceCoverage,
        double volcanismLevel,
        bool hasLiquidWater,
        bool hasAtmosphere)
    {
        Dictionary biomes = new();
        if (oceanCoverage > 0.0)
        {
            biomes[(int)BiomeType.Type.Ocean] = oceanCoverage;
        }

        if (iceCoverage > 0.0)
        {
            biomes[(int)BiomeType.Type.IceSheet] = iceCoverage;
        }

        double land = System.Math.Max(0.0, 1.0 - oceanCoverage - iceCoverage);
        if (land <= 0.0)
        {
            return biomes;
        }

        if (!hasAtmosphere)
        {
            biomes[(int)BiomeType.Type.Barren] = land;
            return biomes;
        }

        foreach (Dictionary zoneData in climateZones)
        {
            ClimateZone.Zone zone = (ClimateZone.Zone)(int)zoneData["zone"];
            double coverage = (double)zoneData["coverage"];
            double landInZone = land * coverage;
            if (landInZone < 0.001)
            {
                continue;
            }

            switch (zone)
            {
                case ClimateZone.Zone.Polar:
                    AddBiome(biomes, BiomeType.Type.Tundra, landInZone);
                    break;
                case ClimateZone.Zone.Subpolar:
                    if (hasLiquidWater)
                    {
                        AddBiome(biomes, BiomeType.Type.Taiga, landInZone * 0.7);
                        AddBiome(biomes, BiomeType.Type.Tundra, landInZone * 0.3);
                    }
                    else
                    {
                        AddBiome(biomes, BiomeType.Type.Tundra, landInZone);
                    }

                    break;
                case ClimateZone.Zone.Temperate:
                    if (hasLiquidWater)
                    {
                        AddBiome(biomes, BiomeType.Type.Forest, landInZone * 0.5);
                        AddBiome(biomes, BiomeType.Type.Grassland, landInZone * 0.4);
                        AddBiome(biomes, BiomeType.Type.Wetland, landInZone * 0.1);
                    }
                    else
                    {
                        AddBiome(biomes, BiomeType.Type.Desert, landInZone * 0.6);
                        AddBiome(biomes, BiomeType.Type.Grassland, landInZone * 0.4);
                    }

                    break;
                case ClimateZone.Zone.Subtropical:
                    if (hasLiquidWater)
                    {
                        AddBiome(biomes, BiomeType.Type.Savanna, landInZone * 0.5);
                        AddBiome(biomes, BiomeType.Type.Forest, landInZone * 0.3);
                        AddBiome(biomes, BiomeType.Type.Desert, landInZone * 0.2);
                    }
                    else
                    {
                        AddBiome(biomes, BiomeType.Type.Desert, landInZone);
                    }

                    break;
                case ClimateZone.Zone.Tropical:
                    if (hasLiquidWater)
                    {
                        AddBiome(biomes, BiomeType.Type.Jungle, landInZone * 0.6);
                        AddBiome(biomes, BiomeType.Type.Savanna, landInZone * 0.3);
                        AddBiome(biomes, BiomeType.Type.Wetland, landInZone * 0.1);
                    }
                    else
                    {
                        AddBiome(biomes, BiomeType.Type.Desert, landInZone);
                    }

                    break;
                case ClimateZone.Zone.Arid:
                    AddBiome(biomes, BiomeType.Type.Desert, landInZone);
                    break;
                case ClimateZone.Zone.Extreme:
                    AddBiome(biomes, BiomeType.Type.Barren, landInZone);
                    break;
            }
        }

        if (volcanismLevel > 0.3)
        {
            AddBiome(biomes, BiomeType.Type.Volcanic, land * volcanismLevel * 0.15);
        }

        AddBiome(biomes, BiomeType.Type.Mountain, land * 0.1);
        return biomes;
    }

    /// <summary>
    /// Calculates resource abundances from surface composition and biome data.
    /// </summary>
    public static Dictionary CalculateResources(
        Dictionary surfaceComposition,
        Dictionary biomes,
        double volcanismLevel,
        bool hasLiquidWater,
        double oceanCoverage)
    {
        Dictionary resources = new();

        double waterAbundance = 0.0;
        if (hasLiquidWater)
        {
            waterAbundance = 0.5 + (oceanCoverage * 0.5);
        }
        else if (surfaceComposition.ContainsKey("water_ice"))
        {
            waterAbundance = (double)surfaceComposition["water_ice"] * 0.7;
        }

        if (waterAbundance > 0.0)
        {
            resources[(int)ResourceType.Type.Water] = System.Math.Clamp(waterAbundance, 0.0, 1.0);
        }

        if (surfaceComposition.ContainsKey("silicates"))
        {
            resources[(int)ResourceType.Type.Silicates] = System.Math.Clamp((double)surfaceComposition["silicates"], 0.0, 1.0);
        }
        else
        {
            resources[(int)ResourceType.Type.Silicates] = 0.5;
        }

        double metalAbundance = 0.0;
        if (surfaceComposition.ContainsKey("iron_oxides"))
        {
            metalAbundance += (double)surfaceComposition["iron_oxides"] * 1.5;
        }

        if (surfaceComposition.ContainsKey("metals"))
        {
            metalAbundance += (double)surfaceComposition["metals"];
        }

        if (metalAbundance > 0.0)
        {
            resources[(int)ResourceType.Type.Metals] = System.Math.Clamp(metalAbundance, 0.0, 1.0);
        }

        if (volcanismLevel > 0.2)
        {
            resources[(int)ResourceType.Type.RareElements] = System.Math.Clamp(volcanismLevel * 0.5, 0.0, 1.0);
        }

        if (volcanismLevel > 0.3)
        {
            resources[(int)ResourceType.Type.Radioactives] = System.Math.Clamp(volcanismLevel * 0.3, 0.0, 1.0);
        }

        if (surfaceComposition.ContainsKey("carbon_compounds"))
        {
            resources[(int)ResourceType.Type.Hydrocarbons] = System.Math.Clamp((double)surfaceComposition["carbon_compounds"] * 0.8, 0.0, 1.0);
        }

        double organicAbundance = 0.0;
        organicAbundance += GetBiomeCoverage(biomes, BiomeType.Type.Forest) * 0.8;
        organicAbundance += GetBiomeCoverage(biomes, BiomeType.Type.Jungle) * 1.0;
        organicAbundance += GetBiomeCoverage(biomes, BiomeType.Type.Wetland) * 0.6;
        if (hasLiquidWater)
        {
            organicAbundance += GetBiomeCoverage(biomes, BiomeType.Type.Ocean) * 0.3;
        }

        if (organicAbundance > 0.0)
        {
            resources[(int)ResourceType.Type.Organics] = System.Math.Clamp(organicAbundance, 0.0, 1.0);
        }

        if (surfaceComposition.ContainsKey("nitrogen_ice") || surfaceComposition.ContainsKey("methane_ice"))
        {
            double volatileValue = 0.0;
            if (surfaceComposition.ContainsKey("nitrogen_ice"))
            {
                volatileValue += (double)surfaceComposition["nitrogen_ice"];
            }

            if (surfaceComposition.ContainsKey("methane_ice"))
            {
                volatileValue += (double)surfaceComposition["methane_ice"];
            }

            resources[(int)ResourceType.Type.Volatiles] = System.Math.Clamp(volatileValue, 0.0, 1.0);
        }

        return resources;
    }

    /// <summary>
    /// Returns whether an atmosphere is breathable for humans.
    /// </summary>
    public static bool CheckBreathability(Dictionary composition, double pressureAtm)
    {
        if (composition.Count == 0)
        {
            return false;
        }

        if (pressureAtm < 0.5 || pressureAtm > 3.0)
        {
            return false;
        }

        double oxygenFraction = 0.0;
        if (composition.ContainsKey("O2"))
        {
            oxygenFraction = (double)composition["O2"];
        }
        else if (composition.ContainsKey("oxygen"))
        {
            oxygenFraction = (double)composition["oxygen"];
        }

        if (oxygenFraction < 0.18 || oxygenFraction > 0.25)
        {
            return false;
        }

        string[] toxicGases = { "CO2", "CO", "H2S", "SO2", "NH3", "Cl2" };
        foreach (string gas in toxicGases)
        {
            if (!composition.ContainsKey(gas))
            {
                continue;
            }

            double fraction = (double)composition[gas];
            double threshold;
            if (gas == "CO2")
            {
                threshold = 0.05;
            }
            else
            {
                threshold = 0.01;
            }
            if (fraction > threshold)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Calculates moon-specific tidal-heating factor.
    /// </summary>
    public static double CalculateTidalHeating(
        double parentMassKg,
        double orbitalDistanceM,
        double moonRadiusM,
        double eccentricity)
    {
        if (parentMassKg <= 0.0 || orbitalDistanceM <= 0.0 || moonRadiusM <= 0.0)
        {
            return 0.0;
        }

        const double ioParentMass = 1.898e27;
        const double ioDistance = 4.217e8;
        const double ioRadius = 1.821e6;
        const double ioEccentricity = 0.0041;

        double massFactor = System.Math.Pow(parentMassKg / ioParentMass, 2.0);
        double radiusFactor = System.Math.Pow(moonRadiusM / ioRadius, 5.0);
        double distanceFactor = System.Math.Pow(ioDistance / orbitalDistanceM, 6.0);
        double eccentricityFactor = System.Math.Pow(eccentricity / System.Math.Max(ioEccentricity, 0.001), 2.0);
        double heating = massFactor * radiusFactor * distanceFactor * eccentricityFactor;

        return System.Math.Clamp(heating, 0.0, 1.0);
    }

    /// <summary>
    /// Calculates parent-body radiation exposure for moons.
    /// </summary>
    public static double CalculateParentRadiation(double parentMassKg, double parentMagneticMoment, double orbitalDistanceM)
    {
        if (parentMassKg <= 0.0 || orbitalDistanceM <= 0.0)
        {
            return 0.0;
        }

        const double jupiterMass = 1.898e27;
        const double jupiterMagnetic = 1.5e20;
        const double ioDistance = 4.217e8;

        if (parentMassKg < jupiterMass * 0.01)
        {
            return 0.0;
        }

        double magneticFactor = parentMagneticMoment / jupiterMagnetic;
        double distanceFactor = System.Math.Pow(ioDistance / orbitalDistanceM, 2.0);
        double radiation = magneticFactor * distanceFactor * 0.5;

        return System.Math.Clamp(radiation, 0.0, 1.0);
    }

    /// <summary>
    /// Calculates eclipse factor for moons.
    /// </summary>
    public static double CalculateEclipseFactor(
        double parentRadiusM,
        double orbitalDistanceM,
        double orbitalPeriodS,
        double parentOrbitalPeriodS)
    {
        if (parentRadiusM <= 0.0 || orbitalDistanceM <= 0.0)
        {
            return 0.0;
        }

        if (orbitalPeriodS <= 0.0 || parentOrbitalPeriodS <= 0.0)
        {
            return 0.0;
        }

        double angularSize = 2.0 * parentRadiusM / orbitalDistanceM;
        double eclipsesPerOrbit = parentOrbitalPeriodS / orbitalPeriodS;
        double durationFactor = System.Math.Clamp(angularSize * 5.0, 0.0, 1.0);
        double frequencyFactor = System.Math.Clamp(eclipsesPerOrbit / 100.0, 0.0, 1.0);

        return System.Math.Clamp((durationFactor * 0.5) + (frequencyFactor * 0.5), 0.0, 1.0);
    }

    private static void AddBiome(Dictionary biomes, BiomeType.Type biome, double coverage)
    {
        int key = (int)biome;
        double existing;
        if (biomes.ContainsKey(key))
        {
            existing = (double)biomes[key];
        }
        else
        {
            existing = 0.0;
        }
        biomes[key] = existing + coverage;
    }

    private static double GetBiomeCoverage(Dictionary biomes, BiomeType.Type biome)
    {
        int key = (int)biome;
        if (biomes.ContainsKey(key))
        {
            return (double)biomes[key];
        }

        return 0.0;
    }
}
