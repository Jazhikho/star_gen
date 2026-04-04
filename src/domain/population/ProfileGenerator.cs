using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Generation;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;

namespace StarGen.Domain.Population;

/// <summary>
/// Builds planet-profile data from celestial bodies and parent context.
/// </summary>
public static class ProfileGenerator
{
    /// <summary>
    /// Generates a planet profile from a celestial body.
    /// </summary>
    public static PlanetProfile Generate(CelestialBody body, ParentContext context, CelestialBody? parentBody = null)
    {
        PlanetProfile profile = new()
        {
            BodyId = body.Id,
            IsMoon = body.Type == CelestialType.Type.Moon,
            StellarAgeYears = context.StellarAgeYears,
        };

        ExtractPhysicalProperties(profile, body);
        ExtractAtmosphereProperties(profile, body);
        ExtractSurfaceProperties(profile, body);
        CalculateDerivedProperties(profile, body, context);

        if (profile.IsMoon && parentBody != null)
        {
            CalculateMoonProperties(profile, body, parentBody, context);
        }

        if (IsGasGiantForProfile(body))
        {
            profile.ClimateZones = new Array<Dictionary>();
            profile.Biomes = new Dictionary
            {
                [(int)BiomeType.Type.GasGiant] = 1.0,
            };
        }
        else
        {
            profile.ClimateZones = ProfileCalculations.CalculateClimateZones(
                profile.AxialTiltDeg,
                profile.AvgTemperatureK,
                profile.HasAtmosphere);
            profile.Biomes = ProfileCalculations.CalculateBiomes(
                profile.ClimateZones,
                profile.OceanCoverage,
                profile.IceCoverage,
                profile.VolcanismLevel,
                profile.HasLiquidWater,
                profile.HasAtmosphere);
        }

        Dictionary surfaceComposition;
        if (body.HasSurface())
        {
            surfaceComposition = body.Surface!.SurfaceComposition;
        }
        else
        {
            surfaceComposition = new Dictionary();
        }
        profile.Resources = ProfileCalculations.CalculateResources(
            surfaceComposition,
            profile.Biomes,
            profile.VolcanismLevel,
            profile.HasLiquidWater,
            profile.OceanCoverage);

        profile.ContinentCount = ProfileCalculations.EstimateContinentCount(
            profile.TectonicActivity,
            profile.LandCoverage,
            body.HasSurface() && body.Surface!.HasTerrain());

        profile.HabitabilityScore = ProfileCalculations.CalculateHabitabilityScore(
            profile.AvgTemperatureK,
            profile.PressureAtm,
            profile.GravityG,
            profile.HasLiquidWater,
            profile.HasBreathableAtmosphere,
            profile.RadiationLevel,
            profile.OceanCoverage);

        return profile;
    }

    private static void ExtractPhysicalProperties(PlanetProfile profile, CelestialBody body)
    {
        profile.GravityG = body.Physical.GetSurfaceGravityMS2() / ProfileCalculations.EarthGravity;
        profile.DayLengthHours = System.Math.Abs(body.Physical.RotationPeriodS) / ProfileCalculations.SecondsPerHour;
        profile.AxialTiltDeg = body.Physical.AxialTiltDeg;
        profile.MagneticFieldStrength = ProfileCalculations.CalculateMagneticStrength(body.Physical.MagneticMoment);
        profile.HasMagneticField = profile.MagneticFieldStrength > 0.1;
    }

    private static void ExtractAtmosphereProperties(PlanetProfile profile, CelestialBody body)
    {
        if (!body.HasAtmosphere())
        {
            profile.HasAtmosphere = false;
            profile.PressureAtm = 0.0;
            profile.GreenhouseFactor = 1.0;
            profile.HasBreathableAtmosphere = false;
            return;
        }

        profile.PressureAtm = body.Atmosphere!.SurfacePressurePa / ProfileCalculations.EarthPressurePa;
        profile.HasAtmosphere = profile.PressureAtm > ProfileCalculations.MinAtmosphereAtm;
        profile.GreenhouseFactor = body.Atmosphere.GreenhouseFactor;
        profile.HasBreathableAtmosphere = ProfileCalculations.CheckBreathability(
            body.Atmosphere.Composition,
            profile.PressureAtm);
    }

    private static void ExtractSurfaceProperties(PlanetProfile profile, CelestialBody body)
    {
        if (!body.HasSurface())
        {
            profile.AvgTemperatureK = 0.0;
            profile.Albedo = 0.0;
            profile.VolcanismLevel = 0.0;
            profile.OceanCoverage = 0.0;
            profile.LandCoverage = 0.0;
            profile.IceCoverage = 0.0;
            profile.MaxElevationKm = 0.0;
            profile.TectonicActivity = 0.0;
            profile.HasLiquidWater = false;
            return;
        }

        profile.AvgTemperatureK = body.Surface!.TemperatureK;
        profile.Albedo = body.Surface.Albedo;
        profile.VolcanismLevel = body.Surface.VolcanismLevel;

        if (body.Surface.HasHydrosphere())
        {
            profile.OceanCoverage = body.Surface.Hydrosphere!.OceanCoverage;
            profile.HasLiquidWater = profile.OceanCoverage > 0.0;
        }
        else
        {
            profile.OceanCoverage = 0.0;
            profile.HasLiquidWater = false;
        }

        if (body.Surface.HasCryosphere())
        {
            profile.IceCoverage = body.Surface.Cryosphere!.PolarCapCoverage;
            if (body.Surface.Cryosphere.HasSubsurfaceOcean)
            {
                profile.HasLiquidWater = true;
            }
        }
        else
        {
            profile.IceCoverage = 0.0;
        }

        profile.LandCoverage = System.Math.Clamp(1.0 - profile.OceanCoverage - profile.IceCoverage, 0.0, 1.0);

        if (body.Surface.HasTerrain())
        {
            profile.MaxElevationKm = body.Surface.Terrain!.ElevationRangeM / 1000.0;
            profile.TectonicActivity = body.Surface.Terrain.TectonicActivity;
        }
        else
        {
            profile.MaxElevationKm = 0.0;
            profile.TectonicActivity = 0.0;
        }
    }

    private static void CalculateDerivedProperties(PlanetProfile profile, CelestialBody body, ParentContext context)
    {
        profile.WeatherSeverity = ProfileCalculations.CalculateWeatherSeverity(
            profile.PressureAtm,
            body.Physical.RotationPeriodS,
            profile.HasAtmosphere);
        double baseRadiation = ProfileCalculations.CalculateRadiationLevel(
            body.Physical.MagneticMoment,
            profile.PressureAtm,
            profile.HasAtmosphere);
        double orbitalDistanceAu = context.OrbitalDistanceFromStarM / Units.AuMeters;
        profile.StellarFluxEarth = 0.0;
        if (context.StellarLuminosityWatts > 0.0 && orbitalDistanceAu > 0.0)
        {
            double luminositySolar = context.StellarLuminosityWatts / StellarProps.SolarLuminosityWatts;
            profile.StellarFluxEarth = luminositySolar / System.Math.Max(orbitalDistanceAu * orbitalDistanceAu, 0.01);
        }

        profile.HabitableZoneInnerAu = OrbitalMechanics.CalculateHabitableZoneInner(
            context.StellarLuminosityWatts,
            context.StellarTemperatureK) / Units.AuMeters;
        profile.HabitableZoneOuterAu = OrbitalMechanics.CalculateHabitableZoneOuter(
            context.StellarLuminosityWatts,
            context.StellarTemperatureK) / Units.AuMeters;
        profile.HabitableZoneAlignment = CalculateHabitableZoneAlignment(
            orbitalDistanceAu,
            profile.HabitableZoneInnerAu,
            profile.HabitableZoneOuterAu);
        profile.XuvExposure = CalculateXuvExposure(profile, context);
        double atmosphericShielding = profile.HasAtmosphere ? 0.35 : 1.0;
        double magneticShielding = profile.HasMagneticField ? 0.45 : 1.0;
        double xuvPenalty = profile.XuvExposure * atmosphericShielding * magneticShielding * 0.55;
        profile.RadiationLevel = System.Math.Clamp(baseRadiation + xuvPenalty, 0.0, 1.0);

        profile.IsTidallyLocked = false;
        if (body.HasOrbital() && !profile.IsMoon)
        {
            double orbitalPeriod = body.Orbital!.GetOrbitalPeriodS(context.StellarMassKg);
            double rotationPeriod = System.Math.Abs(body.Physical.RotationPeriodS);
            if (orbitalPeriod > 0.0 && rotationPeriod > 0.0)
            {
                double difference = System.Math.Abs(rotationPeriod - orbitalPeriod);
                profile.IsTidallyLocked = difference < orbitalPeriod * 0.01;
            }
        }
    }

    private static void CalculateMoonProperties(
        PlanetProfile profile,
        CelestialBody body,
        CelestialBody parentBody,
        ParentContext context)
    {
        if (!body.HasOrbital())
        {
            return;
        }

        double orbitalDistance = body.Orbital!.SemiMajorAxisM;
        double eccentricity = body.Orbital.Eccentricity;
        double moonRadius = body.Physical.RadiusM;
        double parentMass = parentBody.Physical.MassKg;
        double parentRadius = parentBody.Physical.RadiusM;
        double parentMagnetic = parentBody.Physical.MagneticMoment;

        double moonOrbitalPeriod = body.Orbital.GetOrbitalPeriodS(parentMass);
        double rotationPeriod = System.Math.Abs(body.Physical.RotationPeriodS);
        if (moonOrbitalPeriod > 0.0 && rotationPeriod > 0.0)
        {
            double difference = System.Math.Abs(rotationPeriod - moonOrbitalPeriod);
            profile.IsTidallyLocked = difference < moonOrbitalPeriod * 0.01;
        }

        profile.TidalHeatingFactor = ProfileCalculations.CalculateTidalHeating(
            parentMass,
            orbitalDistance,
            moonRadius,
            eccentricity);
        profile.ParentRadiationExposure = ProfileCalculations.CalculateParentRadiation(
            parentMass,
            parentMagnetic,
            orbitalDistance);
        profile.RadiationLevel = System.Math.Clamp(
            profile.RadiationLevel + (profile.ParentRadiationExposure * 0.5),
            0.0,
            1.0);

        double parentOrbitalPeriod = 0.0;
        if (parentBody.HasOrbital())
        {
            parentOrbitalPeriod = parentBody.Orbital!.GetOrbitalPeriodS(context.StellarMassKg);
        }

        profile.EclipseFactor = ProfileCalculations.CalculateEclipseFactor(
            parentRadius,
            orbitalDistance,
            moonOrbitalPeriod,
            parentOrbitalPeriod);
    }

    private static double CalculateHabitableZoneAlignment(double orbitAu, double innerAu, double outerAu)
    {
        if (orbitAu <= 0.0 || innerAu <= 0.0 || outerAu <= innerAu)
        {
            return 0.0;
        }

        if (orbitAu >= innerAu && orbitAu <= outerAu)
        {
            return 1.0;
        }

        double innerDecayFloor = innerAu * 0.45;
        double outerDecayCeiling = outerAu * 1.80;
        if (orbitAu < innerAu)
        {
            return System.Math.Clamp((orbitAu - innerDecayFloor) / System.Math.Max(innerAu - innerDecayFloor, 0.01), 0.0, 1.0);
        }

        return System.Math.Clamp((outerDecayCeiling - orbitAu) / System.Math.Max(outerDecayCeiling - outerAu, 0.01), 0.0, 1.0);
    }

    private static double CalculateXuvExposure(PlanetProfile profile, ParentContext context)
    {
        double orbitAu = context.OrbitalDistanceFromStarM / Units.AuMeters;
        if (orbitAu <= 0.0 || context.StellarMassKg <= 0.0)
        {
            return 0.0;
        }

        double massSolar = context.StellarMassKg / Units.SolarMassKg;
        double ageFactor = 1.0;
        if (context.StellarAgeYears > 0.0)
        {
            ageFactor = System.Math.Clamp(System.Math.Pow(1.6e9 / context.StellarAgeYears, 0.20), 0.65, 1.8);
        }

        double lowMassActivityFactor;
        if (massSolar < 0.85)
        {
            lowMassActivityFactor = System.Math.Clamp(1.35 - (massSolar * 0.38), 1.0, 1.32);
        }
        else
        {
            lowMassActivityFactor = System.Math.Clamp(1.0 - ((massSolar - 0.85) * 0.08), 0.84, 1.0);
        }

        double fluxFactor = System.Math.Pow(System.Math.Max(profile.StellarFluxEarth, 0.02), 0.35);
        return System.Math.Clamp(ageFactor * lowMassActivityFactor * fluxFactor, 0.0, 1.8);
    }

    private static bool IsGasGiantForProfile(CelestialBody body)
    {
        if (body.Physical == null || body.HasSurface())
        {
            return false;
        }

        double massEarth = body.Physical.MassKg / Units.EarthMassKg;
        return massEarth >= 10.0;
    }
}
