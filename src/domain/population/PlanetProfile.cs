using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Derived snapshot of a planet's habitability-related properties.
/// </summary>
public partial class PlanetProfile : RefCounted
{
    /// <summary>
    /// Source celestial body identifier.
    /// </summary>
    public string BodyId = string.Empty;

    /// <summary>
    /// Primary 0-10 human-habitability score.
    /// </summary>
    public int HabitabilityScore;

    /// <summary>
    /// Average surface temperature in Kelvin.
    /// </summary>
    public double AvgTemperatureK;

    /// <summary>
    /// Atmospheric pressure in Earth atmospheres.
    /// </summary>
    public double PressureAtm;

    /// <summary>
    /// Fraction of surface covered by liquid.
    /// </summary>
    public double OceanCoverage;

    /// <summary>
    /// Fraction of surface covered by land.
    /// </summary>
    public double LandCoverage;

    /// <summary>
    /// Fraction of surface covered by ice.
    /// </summary>
    public double IceCoverage;

    /// <summary>
    /// Estimated number of continents.
    /// </summary>
    public int ContinentCount;

    /// <summary>
    /// Maximum elevation range in kilometers.
    /// </summary>
    public double MaxElevationKm;

    /// <summary>
    /// Day length in hours.
    /// </summary>
    public double DayLengthHours;

    /// <summary>
    /// Axial tilt in degrees.
    /// </summary>
    public double AxialTiltDeg;

    /// <summary>
    /// Surface gravity relative to Earth.
    /// </summary>
    public double GravityG;

    /// <summary>
    /// Tectonic activity level.
    /// </summary>
    public double TectonicActivity;

    /// <summary>
    /// Volcanic activity level.
    /// </summary>
    public double VolcanismLevel;

    /// <summary>
    /// Weather severity level.
    /// </summary>
    public double WeatherSeverity;

    /// <summary>
    /// Magnetic-field strength, normalized.
    /// </summary>
    public double MagneticFieldStrength;

    /// <summary>
    /// Surface radiation level, normalized.
    /// </summary>
    public double RadiationLevel;

    /// <summary>
    /// Bond albedo.
    /// </summary>
    public double Albedo;

    /// <summary>
    /// Greenhouse warming factor.
    /// </summary>
    public double GreenhouseFactor = 1.0;

    /// <summary>
    /// Climate zones with coverage fractions.
    /// </summary>
    public Array<Dictionary> ClimateZones = new();

    /// <summary>
    /// Biome distribution keyed by biome enum integer.
    /// </summary>
    public Dictionary Biomes = new();

    /// <summary>
    /// Resource abundances keyed by resource enum integer.
    /// </summary>
    public Dictionary Resources = new();

    /// <summary>
    /// Whether the body is tidally locked.
    /// </summary>
    public bool IsTidallyLocked;

    /// <summary>
    /// Whether the body has a significant atmosphere.
    /// </summary>
    public bool HasAtmosphere;

    /// <summary>
    /// Whether the body has a protective magnetic field.
    /// </summary>
    public bool HasMagneticField;

    /// <summary>
    /// Whether liquid water is present.
    /// </summary>
    public bool HasLiquidWater;

    /// <summary>
    /// Whether the atmosphere is breathable to humans.
    /// </summary>
    public bool HasBreathableAtmosphere;

    /// <summary>
    /// Whether the body is a moon.
    /// </summary>
    public bool IsMoon;

    /// <summary>
    /// Moon-specific tidal-heating factor.
    /// </summary>
    public double TidalHeatingFactor;

    /// <summary>
    /// Moon-specific parent-radiation exposure.
    /// </summary>
    public double ParentRadiationExposure;

    /// <summary>
    /// Moon-specific eclipse factor.
    /// </summary>
    public double EclipseFactor;

    /// <summary>
    /// Returns the derived habitability category.
    /// </summary>
    public HabitabilityCategory.Category GetHabitabilityCategory()
    {
        return HabitabilityCategory.FromScore(HabitabilityScore);
    }

    /// <summary>
    /// Returns the habitability category as a display string.
    /// </summary>
    public string GetHabitabilityCategoryString()
    {
        return HabitabilityCategory.ToStringName(GetHabitabilityCategory());
    }

    /// <summary>
    /// Returns the average temperature in Celsius.
    /// </summary>
    public double GetTemperatureCelsius() => AvgTemperatureK - 273.15;

    /// <summary>
    /// Returns the habitable land fraction.
    /// </summary>
    public double GetHabitableSurface() => System.Math.Max(0.0, LandCoverage - (IceCoverage * 0.5));

    /// <summary>
    /// Returns the dominant biome.
    /// </summary>
    public BiomeType.Type GetDominantBiome()
    {
        double maxCoverage = 0.0;
        BiomeType.Type dominant = BiomeType.Type.Barren;

        foreach (Variant biomeKey in Biomes.Keys)
        {
            double coverage = GetDouble(Biomes, biomeKey, 0.0);
            if (coverage > maxCoverage)
            {
                maxCoverage = coverage;
                dominant = (BiomeType.Type)KeyToInt(biomeKey);
            }
        }

        return dominant;
    }

    /// <summary>
    /// Returns the most abundant resource.
    /// </summary>
    public ResourceType.Type GetPrimaryResource()
    {
        double maxAbundance = 0.0;
        ResourceType.Type primary = ResourceType.Type.Silicates;

        foreach (Variant resourceKey in Resources.Keys)
        {
            double abundance = GetDouble(Resources, resourceKey, 0.0);
            if (abundance > maxAbundance)
            {
                maxAbundance = abundance;
                primary = (ResourceType.Type)KeyToInt(resourceKey);
            }
        }

        return primary;
    }

    /// <summary>
    /// Returns whether native life emergence is plausible.
    /// </summary>
    public bool CanSupportNativeLife()
    {
        if (!HasLiquidWater && OceanCoverage < 0.01)
        {
            return false;
        }

        if (AvgTemperatureK < 200.0 || AvgTemperatureK > 400.0)
        {
            return false;
        }

        if (PressureAtm < 0.01)
        {
            return false;
        }

        return HabitabilityScore >= 3;
    }

    /// <summary>
    /// Returns whether colonization is feasible.
    /// </summary>
    public bool IsColonizable() => HabitabilityScore >= 1;

    /// <summary>
    /// Converts the profile to a dictionary.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<Dictionary> climateZoneData = new();
        foreach (Dictionary zoneData in ClimateZones)
        {
            climateZoneData.Add(new Dictionary
            {
                ["zone"] = KeyToInt(zoneData["zone"]),
                ["coverage"] = (double)zoneData["coverage"],
            });
        }

        Dictionary biomesData = new();
        foreach (Variant biomeKey in Biomes.Keys)
        {
            biomesData[KeyToInt(biomeKey)] = GetDouble(Biomes, biomeKey, 0.0);
        }

        Dictionary resourcesData = new();
        foreach (Variant resourceKey in Resources.Keys)
        {
            resourcesData[KeyToInt(resourceKey)] = GetDouble(Resources, resourceKey, 0.0);
        }

        return new Dictionary
        {
            ["body_id"] = BodyId,
            ["habitability_score"] = HabitabilityScore,
            ["avg_temperature_k"] = AvgTemperatureK,
            ["pressure_atm"] = PressureAtm,
            ["ocean_coverage"] = OceanCoverage,
            ["land_coverage"] = LandCoverage,
            ["ice_coverage"] = IceCoverage,
            ["continent_count"] = ContinentCount,
            ["max_elevation_km"] = MaxElevationKm,
            ["day_length_hours"] = DayLengthHours,
            ["axial_tilt_deg"] = AxialTiltDeg,
            ["gravity_g"] = GravityG,
            ["tectonic_activity"] = TectonicActivity,
            ["volcanism_level"] = VolcanismLevel,
            ["weather_severity"] = WeatherSeverity,
            ["magnetic_field_strength"] = MagneticFieldStrength,
            ["radiation_level"] = RadiationLevel,
            ["albedo"] = Albedo,
            ["greenhouse_factor"] = GreenhouseFactor,
            ["climate_zones"] = climateZoneData,
            ["biomes"] = biomesData,
            ["resources"] = resourcesData,
            ["is_tidally_locked"] = IsTidallyLocked,
            ["has_atmosphere"] = HasAtmosphere,
            ["has_magnetic_field"] = HasMagneticField,
            ["has_liquid_water"] = HasLiquidWater,
            ["has_breathable_atmosphere"] = HasBreathableAtmosphere,
            ["is_moon"] = IsMoon,
            ["tidal_heating_factor"] = TidalHeatingFactor,
            ["parent_radiation_exposure"] = ParentRadiationExposure,
            ["eclipse_factor"] = EclipseFactor,
        };
    }

    /// <summary>
    /// Creates a profile from a dictionary payload.
    /// </summary>
    public static PlanetProfile FromDictionary(Dictionary data)
    {
        PlanetProfile profile = new()
        {
            BodyId = GetString(data, "body_id", string.Empty),
            HabitabilityScore = GetInt(data, "habitability_score", 0),
            AvgTemperatureK = GetDouble(data, "avg_temperature_k", 0.0),
            PressureAtm = GetDouble(data, "pressure_atm", 0.0),
            OceanCoverage = GetDouble(data, "ocean_coverage", 0.0),
            LandCoverage = GetDouble(data, "land_coverage", 0.0),
            IceCoverage = GetDouble(data, "ice_coverage", 0.0),
            ContinentCount = GetInt(data, "continent_count", 0),
            MaxElevationKm = GetDouble(data, "max_elevation_km", 0.0),
            DayLengthHours = GetDouble(data, "day_length_hours", 0.0),
            AxialTiltDeg = GetDouble(data, "axial_tilt_deg", 0.0),
            GravityG = GetDouble(data, "gravity_g", 0.0),
            TectonicActivity = GetDouble(data, "tectonic_activity", 0.0),
            VolcanismLevel = GetDouble(data, "volcanism_level", 0.0),
            WeatherSeverity = GetDouble(data, "weather_severity", 0.0),
            MagneticFieldStrength = GetDouble(data, "magnetic_field_strength", 0.0),
            RadiationLevel = GetDouble(data, "radiation_level", 0.0),
            Albedo = GetDouble(data, "albedo", 0.0),
            GreenhouseFactor = GetDouble(data, "greenhouse_factor", 1.0),
            IsTidallyLocked = GetBool(data, "is_tidally_locked", false),
            HasAtmosphere = GetBool(data, "has_atmosphere", false),
            HasMagneticField = GetBool(data, "has_magnetic_field", false),
            HasLiquidWater = GetBool(data, "has_liquid_water", false),
            HasBreathableAtmosphere = GetBool(data, "has_breathable_atmosphere", false),
            IsMoon = GetBool(data, "is_moon", false),
            TidalHeatingFactor = GetDouble(data, "tidal_heating_factor", 0.0),
            ParentRadiationExposure = GetDouble(data, "parent_radiation_exposure", 0.0),
            EclipseFactor = GetDouble(data, "eclipse_factor", 0.0),
        };

        if (data.ContainsKey("climate_zones"))
        {
            foreach (Dictionary zoneData in (Array)data["climate_zones"])
            {
                profile.ClimateZones.Add(new Dictionary
                {
                    ["zone"] = GetInt(zoneData, "zone", 0),
                    ["coverage"] = GetDouble(zoneData, "coverage", 0.0),
                });
            }
        }

        if (data.ContainsKey("biomes"))
        {
            Dictionary biomesData = (Dictionary)data["biomes"];
            foreach (Variant biomeKey in biomesData.Keys)
            {
                profile.Biomes[KeyToInt(biomeKey)] = GetDouble(biomesData, biomeKey, 0.0);
            }
        }

        if (data.ContainsKey("resources"))
        {
            Dictionary resourcesData = (Dictionary)data["resources"];
            foreach (Variant resourceKey in resourcesData.Keys)
            {
                profile.Resources[KeyToInt(resourceKey)] = GetDouble(resourcesData, resourceKey, 0.0);
            }
        }

        return profile;
    }

    private static int KeyToInt(Variant key)
    {
        return key.VariantType switch
        {
            Variant.Type.Int => (int)key,
            Variant.Type.String => int.Parse((string)key),
            _ => 0,
        };
    }

    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        if (data.ContainsKey(key))
        {
            return (bool)data[key];
        }

        return fallback;
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (data.ContainsKey(key))
        {
            return (double)data[key];
        }

        return fallback;
    }

    private static double GetDouble(Dictionary data, Variant key, double fallback)
    {
        if (data.ContainsKey(key))
        {
            return (double)data[key];
        }

        return fallback;
    }

    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (data.ContainsKey(key))
        {
            return (int)data[key];
        }

        return fallback;
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (data.ContainsKey(key))
        {
            return (string)data[key];
        }

        return fallback;
    }
}
