using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Generation.Traveller;

/// <summary>
/// System-facing Traveller helpers for trade codes, travel zones, route metrics, and body backfill.
/// </summary>
public static partial class TravellerWorldGenerator
{
    /// <summary>
    /// Generates Traveller trade codes from a world profile.
    /// </summary>
    public static TravellerTradeCodeSet GenerateTradeCodes(TravellerWorldProfile profile)
    {
        TravellerTradeCodeSet codes = new();

        if (profile.AtmosphereCode >= 4
            && profile.AtmosphereCode <= 9
            && profile.HydrographicsCode >= 4
            && profile.HydrographicsCode <= 8
            && profile.PopulationCode >= 5
            && profile.PopulationCode <= 7)
        {
            codes.AddCode("Ag");
        }

        if (profile.SizeCode == 0
            && profile.AtmosphereCode == 0
            && profile.HydrographicsCode == 0)
        {
            codes.AddCode("As");
        }

        if (profile.PopulationCode == 0
            && profile.GovernmentCode == 0
            && profile.LawCode == 0)
        {
            codes.AddCode("Ba");
        }

        if (profile.AtmosphereCode >= 2 && profile.HydrographicsCode == 0)
        {
            codes.AddCode("De");
        }

        if (profile.AtmosphereCode >= 10 && profile.HydrographicsCode >= 1)
        {
            codes.AddCode("Fl");
        }

        if (profile.AtmosphereCode >= 5
            && profile.HydrographicsCode >= 4
            && profile.HydrographicsCode <= 9
            && profile.PopulationCode >= 4
            && profile.PopulationCode <= 8)
        {
            codes.AddCode("Ga");
        }

        if (profile.PopulationCode >= 9)
        {
            codes.AddCode("Hi");
        }

        if (profile.TechLevelCode >= 12)
        {
            codes.AddCode("Ht");
        }

        if (profile.AtmosphereCode >= 0
            && profile.AtmosphereCode <= 1
            && profile.HydrographicsCode >= 1)
        {
            codes.AddCode("Ic");
        }

        if ((profile.AtmosphereCode >= 0 && profile.AtmosphereCode <= 2)
            || profile.AtmosphereCode == 4
            || profile.AtmosphereCode == 7
            || profile.AtmosphereCode == 9)
        {
            if (profile.PopulationCode >= 9)
            {
                codes.AddCode("In");
            }
        }

        if (profile.PopulationCode >= 1 && profile.PopulationCode <= 3)
        {
            codes.AddCode("Lo");
        }

        if (profile.TechLevelCode <= 5)
        {
            codes.AddCode("Lt");
        }

        if (profile.AtmosphereCode >= 0
            && profile.AtmosphereCode <= 3
            && profile.HydrographicsCode >= 0
            && profile.HydrographicsCode <= 3
            && profile.PopulationCode >= 6)
        {
            codes.AddCode("Na");
        }

        if (profile.PopulationCode >= 4 && profile.PopulationCode <= 6)
        {
            codes.AddCode("Ni");
        }

        if (profile.AtmosphereCode >= 2
            && profile.AtmosphereCode <= 5
            && profile.HydrographicsCode >= 0
            && profile.HydrographicsCode <= 3)
        {
            codes.AddCode("Po");
        }

        if ((profile.AtmosphereCode == 6 || profile.AtmosphereCode == 8)
            && profile.PopulationCode >= 6
            && profile.PopulationCode <= 8)
        {
            codes.AddCode("Ri");
        }

        if (profile.HydrographicsCode == 10)
        {
            codes.AddCode("Wa");
        }

        if (profile.AtmosphereCode == 0)
        {
            codes.AddCode("Va");
        }

        return codes;
    }

    /// <summary>
    /// Determines the supported travel-zone label for the supplied world profile.
    /// </summary>
    public static string DetermineTravelZone(TravellerWorldProfile profile)
    {
        if (profile.AtmosphereCode >= 10
            || profile.GovernmentCode == 0
            || profile.GovernmentCode == 7
            || profile.GovernmentCode == 10
            || profile.LawCode == 0
            || profile.LawCode >= 9)
        {
            return "Amber";
        }

        return string.Empty;
    }

    /// <summary>
    /// Builds a route profile from Traveller world data.
    /// </summary>
    public static TravellerRouteProfile BuildRouteProfile(
        TravellerWorldProfile profile,
        TravellerTradeCodeSet tradeCodes)
    {
        TravellerRouteProfile routeProfile = new();
        routeProfile.EstimatedPopulation = EstimatePopulationCount(profile.PopulationCode);
        routeProfile.PopulationCode = profile.PopulationCode;
        routeProfile.StarportCode = profile.StarportCode;
        routeProfile.TechLevelCode = profile.TechLevelCode;

        int importance = 0;
        if (profile.StarportCode == "A" || profile.StarportCode == "B")
        {
            importance += 1;
        }

        if (profile.StarportCode == "D" || profile.StarportCode == "E" || profile.StarportCode == "X")
        {
            importance -= 1;
        }

        if (profile.TechLevelCode >= 10)
        {
            importance += 1;
        }

        if (profile.PopulationCode >= 8)
        {
            importance += 1;
        }

        if (profile.PopulationCode <= 4)
        {
            importance -= 1;
        }

        if (tradeCodes.ContainsAny("Hi", "In", "Ht", "Ri"))
        {
            importance += 1;
        }

        if (tradeCodes.ContainsAny("Ba", "Lo"))
        {
            importance -= 1;
        }

        int maxJumpNumber = 0;
        if (!string.Equals(profile.StarportCode, "X", System.StringComparison.OrdinalIgnoreCase))
        {
            if (profile.TechLevelCode >= 12)
            {
                maxJumpNumber = 2;
            }
            else if (profile.TechLevelCode >= 9)
            {
                maxJumpNumber = 1;
            }
        }

        int routeWeight = System.Math.Max(0, importance);
        if (tradeCodes.ContainsAny("Ag", "Ga", "Wa"))
        {
            routeWeight += 1;
        }

        if (tradeCodes.ContainsAny("Hi", "In", "Ht", "Ri"))
        {
            routeWeight += 2;
        }

        routeProfile.Importance = importance;
        routeProfile.MaxJumpNumber = maxJumpNumber;
        routeProfile.RouteWeight = routeWeight;
        return routeProfile;
    }

    /// <summary>
    /// Applies Traveller-backed world values to an existing generated body.
    /// </summary>
    public static void ApplyToBody(
        CelestialBody body,
        TravellerWorldProfile profile,
        SeededRng rng)
    {
        if (body == null)
        {
            throw new System.ArgumentNullException(nameof(body));
        }

        if (profile == null)
        {
            throw new System.ArgumentNullException(nameof(profile));
        }

        SizeCategory.Category sizeCategory = MapSizeCodeToSizeCategory(profile.SizeCode);
        double radiusM = ResolveRadiusMeters(profile.SizeCode, rng);
        double densityKgM3 = body.Physical.GetDensityKgM3();
        if (densityKgM3 <= 0.0)
        {
            densityKgM3 = Generation.Tables.SizeTable.RandomDensity(sizeCategory, rng);
        }

        body.Physical.RadiusM = radiusM;
        body.Physical.MassKg = (4.0 / 3.0) * System.Math.PI * System.Math.Pow(radiusM, 3.0) * densityKgM3;

        if (profile.AtmosphereCode <= 0)
        {
            body.Atmosphere = null;
        }
        else
        {
            AtmosphereProps atmosphere = body.Atmosphere ?? new AtmosphereProps();
            atmosphere.SurfacePressurePa = ResolvePressurePa(profile.AtmosphereCode);
            atmosphere.ScaleHeightM = System.Math.Max(radiusM * 0.01, 1000.0);
            atmosphere.GreenhouseFactor = ResolveGreenhouseFactor(profile.AtmosphereCode);
            atmosphere.Composition = BuildAtmosphereComposition(profile.AtmosphereCode);
            body.Atmosphere = atmosphere;
        }

        SurfaceProps surface = body.Surface ?? new SurfaceProps();
        if (string.IsNullOrWhiteSpace(surface.SurfaceType))
        {
            surface.SurfaceType = "rocky";
        }

        double oceanCoverage = ResolveHydrographicsCoverage(profile.HydrographicsCode);
        double iceCoverage = ResolveIceCoverage(body, oceanCoverage);

        HydrosphereProps hydrosphere = surface.Hydrosphere ?? new HydrosphereProps();
        hydrosphere.OceanCoverage = oceanCoverage;
        hydrosphere.IceCoverage = iceCoverage;
        hydrosphere.WaterType = "water";
        if (hydrosphere.OceanDepthM <= 0.0)
        {
            hydrosphere.OceanDepthM = 2500.0 + (oceanCoverage * 2500.0);
        }
        surface.Hydrosphere = hydrosphere;

        CryosphereProps cryosphere = surface.Cryosphere ?? new CryosphereProps();
        cryosphere.PolarCapCoverage = iceCoverage;
        cryosphere.HasSubsurfaceOcean = iceCoverage >= 0.35 && oceanCoverage <= 0.05;
        cryosphere.IceType = "water_ice";
        surface.Cryosphere = cryosphere;

        body.Surface = surface;
    }

    /// <summary>
    /// Estimates a concrete population count from a Traveller population code.
    /// </summary>
    public static int EstimatePopulationCount(int populationCode)
    {
        if (populationCode <= 0)
        {
            return 0;
        }

        if (populationCode == 1)
        {
            return 5;
        }

        double estimated = 5.0 * System.Math.Pow(10.0, populationCode);
        if (estimated >= int.MaxValue)
        {
            return int.MaxValue;
        }

        return (int)System.Math.Round(estimated);
    }

    /// <summary>
    /// Maps a Traveller technology code back into the local technology enum.
    /// </summary>
    public static TechnologyLevel.Level MapTechLevelCodeToLevel(int techLevelCode)
    {
        if (techLevelCode <= 0)
        {
            return TechnologyLevel.Level.StoneAge;
        }

        if (techLevelCode == 1)
        {
            return TechnologyLevel.Level.BronzeAge;
        }

        if (techLevelCode == 2)
        {
            return TechnologyLevel.Level.IronAge;
        }

        if (techLevelCode == 3)
        {
            return TechnologyLevel.Level.Classical;
        }

        if (techLevelCode == 4)
        {
            return TechnologyLevel.Level.Medieval;
        }

        if (techLevelCode == 5)
        {
            return TechnologyLevel.Level.Renaissance;
        }

        if (techLevelCode == 6)
        {
            return TechnologyLevel.Level.Industrial;
        }

        if (techLevelCode == 7)
        {
            return TechnologyLevel.Level.Information;
        }

        if (techLevelCode <= 9)
        {
            return TechnologyLevel.Level.Spacefaring;
        }

        if (techLevelCode <= 11)
        {
            return TechnologyLevel.Level.Interstellar;
        }

        return TechnologyLevel.Level.Advanced;
    }

    /// <summary>
    /// Maps a Traveller government code into a local government model.
    /// </summary>
    public static Government BuildGovernmentFromCode(int governmentCode)
    {
        Government government = new Government();
        government.Regime = MapGovernmentCodeToRegime(governmentCode);

        switch (governmentCode)
        {
            case <= 0:
                government.CoercionCentralization = 0.05;
                government.AdministrativeCapacity = 0.05;
                government.PoliticalInclusiveness = 0.85;
                government.Legitimacy = 0.40;
                break;
            case 1:
                government.CoercionCentralization = 0.55;
                government.AdministrativeCapacity = 0.60;
                government.PoliticalInclusiveness = 0.20;
                government.Legitimacy = 0.60;
                break;
            case 2:
            case 4:
                government.CoercionCentralization = 0.30;
                government.AdministrativeCapacity = 0.65;
                government.PoliticalInclusiveness = 0.75;
                government.Legitimacy = 0.70;
                break;
            case 3:
            case 12:
                government.CoercionCentralization = 0.65;
                government.AdministrativeCapacity = 0.60;
                government.PoliticalInclusiveness = 0.25;
                government.Legitimacy = 0.60;
                break;
            case 5:
            case 8:
            case 9:
                government.CoercionCentralization = 0.70;
                government.AdministrativeCapacity = 0.80;
                government.PoliticalInclusiveness = 0.30;
                government.Legitimacy = 0.65;
                break;
            case 6:
            case 10:
            case 11:
            case 13:
                government.CoercionCentralization = 0.85;
                government.AdministrativeCapacity = 0.55;
                government.PoliticalInclusiveness = 0.10;
                government.Legitimacy = 0.55;
                break;
            case 7:
                government.CoercionCentralization = 0.25;
                government.AdministrativeCapacity = 0.15;
                government.PoliticalInclusiveness = 0.15;
                government.Legitimacy = 0.20;
                break;
            default:
                government.CoercionCentralization = 0.70;
                government.AdministrativeCapacity = 0.55;
                government.PoliticalInclusiveness = 0.20;
                government.Legitimacy = 0.50;
                break;
        }

        return government;
    }

    private static GovernmentType.Regime MapGovernmentCodeToRegime(int governmentCode)
    {
        return governmentCode switch
        {
            0 => GovernmentType.Regime.Tribal,
            1 => GovernmentType.Regime.Corporate,
            2 => GovernmentType.Regime.MassDemocracy,
            3 => GovernmentType.Regime.Oligarchic,
            4 => GovernmentType.Regime.Constitutional,
            5 => GovernmentType.Regime.Technocracy,
            6 => GovernmentType.Regime.MilitaryJunta,
            7 => GovernmentType.Regime.FailedState,
            8 => GovernmentType.Regime.BureaucraticEmpire,
            9 => GovernmentType.Regime.PatrimonialKingdom,
            10 => GovernmentType.Regime.AbsoluteMonarchy,
            11 => GovernmentType.Regime.OnePartyState,
            12 => GovernmentType.Regime.EliteRepublic,
            13 => GovernmentType.Regime.Theocracy,
            _ => GovernmentType.Regime.Theocracy,
        };
    }

    private static double ResolveGreenhouseFactor(int atmosphereCode)
    {
        return atmosphereCode switch
        {
            <= 1 => 1.0,
            <= 5 => 1.05,
            <= 9 => 1.10,
            10 => 1.15,
            11 => 1.30,
            12 => 1.35,
            13 => 1.20,
            _ => 1.10,
        };
    }

    private static Dictionary BuildAtmosphereComposition(int atmosphereCode)
    {
        Dictionary composition = new();
        switch (atmosphereCode)
        {
            case 1:
                composition["N2"] = 0.85;
                composition["CO2"] = 0.15;
                break;
            case 2:
            case 4:
            case 7:
            case 9:
                composition["N2"] = 0.70;
                composition["O2"] = 0.10;
                composition["CO2"] = 0.20;
                break;
            case 3:
            case 5:
            case 6:
            case 8:
            case 13:
            case 14:
                composition["N2"] = 0.78;
                composition["O2"] = 0.21;
                composition["Ar"] = 0.01;
                break;
            case 10:
                composition["CH4"] = 0.60;
                composition["N2"] = 0.40;
                break;
            case 11:
                composition["SO2"] = 0.55;
                composition["CO2"] = 0.45;
                break;
            case 12:
                composition["Cl2"] = 0.50;
                composition["SO2"] = 0.50;
                break;
            default:
                composition["N2"] = 0.90;
                composition["CO2"] = 0.10;
                break;
        }

        return composition;
    }

    private static double ResolveIceCoverage(CelestialBody body, double oceanCoverage)
    {
        if (body.HasSurface() && body.Surface != null)
        {
            if (body.Surface.TemperatureK <= 230.0)
            {
                return System.Math.Clamp(0.65 + ((0.25 - oceanCoverage) * 0.25), 0.0, 1.0);
            }

            if (body.Surface.TemperatureK <= 260.0)
            {
                return System.Math.Clamp(0.25 + ((0.20 - oceanCoverage) * 0.20), 0.0, 0.7);
            }
        }

        return 0.0;
    }
}
