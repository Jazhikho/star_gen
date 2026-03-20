using Godot;
using Godot.Collections;
using StarGen.Domain.Population;

namespace StarGen.Domain.Concepts.Pipeline;

/// <summary>
/// Rich deterministic environment profile used as the upstream input for concept generation.
/// </summary>
public sealed class PlanetEnvironmentProfile
{
    public int Seed { get; set; }

    public string BodyId { get; set; } = string.Empty;

    public string BodyName { get; set; } = string.Empty;

    public string BodyType { get; set; } = string.Empty;

    public int HabitabilityScore { get; set; }

    public double AvgTemperatureK { get; set; }

    public double PressureAtm { get; set; }

    public double OceanCoverage { get; set; }

    public double LandCoverage { get; set; }

    public double IceCoverage { get; set; }

    public double GravityG { get; set; }

    public double TectonicActivity { get; set; }

    public double VolcanismLevel { get; set; }

    public double WeatherSeverity { get; set; }

    public double MagneticFieldStrength { get; set; }

    public double RadiationLevel { get; set; }

    public bool HasAtmosphere { get; set; }

    public bool HasLiquidWater { get; set; }

    public bool HasBreathableAtmosphere { get; set; }

    public bool IsMoon { get; set; }

    public string DominantBiome { get; set; } = string.Empty;

    public Dictionary<string, double> BiomeCoverage { get; set; } = new Dictionary<string, double>();

    public Dictionary ToDictionary()
    {
        Dictionary biomeCoverage = new();
        foreach (System.Collections.Generic.KeyValuePair<string, double> entry in BiomeCoverage)
        {
            biomeCoverage[entry.Key] = entry.Value;
        }

        return new Dictionary
        {
            ["seed"] = Seed,
            ["body_id"] = BodyId,
            ["body_name"] = BodyName,
            ["body_type"] = BodyType,
            ["habitability_score"] = HabitabilityScore,
            ["avg_temperature_k"] = AvgTemperatureK,
            ["pressure_atm"] = PressureAtm,
            ["ocean_coverage"] = OceanCoverage,
            ["land_coverage"] = LandCoverage,
            ["ice_coverage"] = IceCoverage,
            ["gravity_g"] = GravityG,
            ["tectonic_activity"] = TectonicActivity,
            ["volcanism_level"] = VolcanismLevel,
            ["weather_severity"] = WeatherSeverity,
            ["magnetic_field_strength"] = MagneticFieldStrength,
            ["radiation_level"] = RadiationLevel,
            ["has_atmosphere"] = HasAtmosphere,
            ["has_liquid_water"] = HasLiquidWater,
            ["has_breathable_atmosphere"] = HasBreathableAtmosphere,
            ["is_moon"] = IsMoon,
            ["dominant_biome"] = DominantBiome,
            ["biome_coverage"] = biomeCoverage,
        };
    }

    public static PlanetEnvironmentProfile FromDictionary(Dictionary data)
    {
        PlanetEnvironmentProfile profile = new();
        profile.Seed = ConceptSerializationUtils.ReadInt(data, "seed");
        profile.BodyId = ConceptSerializationUtils.ReadString(data, "body_id");
        profile.BodyName = ConceptSerializationUtils.ReadString(data, "body_name");
        profile.BodyType = ConceptSerializationUtils.ReadString(data, "body_type");
        profile.HabitabilityScore = ConceptSerializationUtils.ReadInt(data, "habitability_score");
        profile.AvgTemperatureK = ConceptSerializationUtils.ReadDouble(data, "avg_temperature_k");
        profile.PressureAtm = ConceptSerializationUtils.ReadDouble(data, "pressure_atm");
        profile.OceanCoverage = ConceptSerializationUtils.ReadDouble(data, "ocean_coverage");
        profile.LandCoverage = ConceptSerializationUtils.ReadDouble(data, "land_coverage");
        profile.IceCoverage = ConceptSerializationUtils.ReadDouble(data, "ice_coverage");
        profile.GravityG = ConceptSerializationUtils.ReadDouble(data, "gravity_g");
        profile.TectonicActivity = ConceptSerializationUtils.ReadDouble(data, "tectonic_activity");
        profile.VolcanismLevel = ConceptSerializationUtils.ReadDouble(data, "volcanism_level");
        profile.WeatherSeverity = ConceptSerializationUtils.ReadDouble(data, "weather_severity");
        profile.MagneticFieldStrength = ConceptSerializationUtils.ReadDouble(data, "magnetic_field_strength");
        profile.RadiationLevel = ConceptSerializationUtils.ReadDouble(data, "radiation_level");
        profile.HasAtmosphere = ConceptSerializationUtils.ReadBool(data, "has_atmosphere");
        profile.HasLiquidWater = ConceptSerializationUtils.ReadBool(data, "has_liquid_water");
        profile.HasBreathableAtmosphere = ConceptSerializationUtils.ReadBool(data, "has_breathable_atmosphere");
        profile.IsMoon = ConceptSerializationUtils.ReadBool(data, "is_moon");
        profile.DominantBiome = ConceptSerializationUtils.ReadString(data, "dominant_biome");
        Dictionary? biomeCoverage = ConceptSerializationUtils.ReadDictionary(data, "biome_coverage");
        if (biomeCoverage != null)
        {
            foreach (Variant key in biomeCoverage.Keys)
            {
                if (key.VariantType != Variant.Type.String)
                {
                    throw new System.InvalidOperationException("Expected string biome key.");
                }

                Variant value = biomeCoverage[key];
                if (value.VariantType == Variant.Type.Int)
                {
                    profile.BiomeCoverage[(string)key] = (int)value;
                    continue;
                }

                if (value.VariantType == Variant.Type.Float)
                {
                    profile.BiomeCoverage[(string)key] = (double)value;
                    continue;
                }

                throw new System.InvalidOperationException("Expected numeric biome coverage value.");
            }
        }

        return profile;
    }

    public static PlanetEnvironmentProfile FromPlanetProfile(PlanetProfile profile, int seed, string bodyName, string bodyType)
    {
        PlanetEnvironmentProfile environment = new();
        environment.Seed = seed;
        environment.BodyId = profile.BodyId;
        environment.BodyName = bodyName;
        environment.BodyType = bodyType;
        environment.HabitabilityScore = profile.HabitabilityScore;
        environment.AvgTemperatureK = profile.AvgTemperatureK;
        environment.PressureAtm = profile.PressureAtm;
        environment.OceanCoverage = profile.OceanCoverage;
        environment.LandCoverage = profile.LandCoverage;
        environment.IceCoverage = profile.IceCoverage;
        environment.GravityG = profile.GravityG;
        environment.TectonicActivity = profile.TectonicActivity;
        environment.VolcanismLevel = profile.VolcanismLevel;
        environment.WeatherSeverity = profile.WeatherSeverity;
        environment.MagneticFieldStrength = profile.MagneticFieldStrength;
        environment.RadiationLevel = profile.RadiationLevel;
        environment.HasAtmosphere = profile.HasAtmosphere;
        environment.HasLiquidWater = profile.HasLiquidWater;
        environment.HasBreathableAtmosphere = profile.HasBreathableAtmosphere;
        environment.IsMoon = profile.IsMoon;
        environment.DominantBiome = BiomeType.ToStringName(profile.GetDominantBiome());

        foreach (Variant biomeKey in profile.Biomes.Keys)
        {
            string biomeName = BiomeType.ToStringName((BiomeType.Type)(int)biomeKey);
            Variant value = profile.Biomes[biomeKey];
            if (value.VariantType == Variant.Type.Float)
            {
                environment.BiomeCoverage[biomeName] = (double)value;
            }
            else if (value.VariantType == Variant.Type.Int)
            {
                environment.BiomeCoverage[biomeName] = (int)value;
            }
        }

        return environment;
    }

    public bool SupportsBiology()
    {
        if (!HasLiquidWater)
        {
            return false;
        }

        if (!HasAtmosphere && !HasLiquidWater)
        {
            return false;
        }

        if (AvgTemperatureK < 180.0 || AvgTemperatureK > 390.0)
        {
            return false;
        }

        return HabitabilityScore >= 2;
    }
}
