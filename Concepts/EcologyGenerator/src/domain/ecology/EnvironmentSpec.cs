using System;

namespace StarGen.Domain.Ecology
{
    /// <summary>
    /// Input specification describing environmental conditions for ecology generation.
    /// All factor ranges are 0.0 to 1.0 unless otherwise noted.
    /// </summary>
    public class EnvironmentSpec
    {
        /// <summary>
        /// Minimum temperature in Kelvin.
        /// </summary>
        public float TemperatureMin { get; set; } = 273f;

        /// <summary>
        /// Maximum temperature in Kelvin.
        /// </summary>
        public float TemperatureMax { get; set; } = 310f;

        /// <summary>
        /// Water availability factor (0 = none, 1 = abundant).
        /// </summary>
        public float WaterAvailability { get; set; } = 0.5f;

        /// <summary>
        /// Light level for photosynthesis (0 = none, 1 = Earth-like).
        /// </summary>
        public float LightLevel { get; set; } = 1.0f;

        /// <summary>
        /// Nutrient availability in substrate (0 = barren, 1 = rich).
        /// </summary>
        public float NutrientLevel { get; set; } = 0.5f;

        /// <summary>
        /// Surface gravity relative to Earth (1.0 = Earth).
        /// </summary>
        public float Gravity { get; set; } = 1.0f;

        /// <summary>
        /// Background radiation level (0 = none, 1 = lethal without adaptation).
        /// </summary>
        public float RadiationLevel { get; set; } = 0.1f;

        /// <summary>
        /// Atmospheric oxygen fraction (0-1).
        /// </summary>
        public float OxygenLevel { get; set; } = 0.21f;

        /// <summary>
        /// Seasonal variation intensity (0 = stable, 1 = extreme).
        /// </summary>
        public float SeasonalVariation { get; set; } = 0.3f;

        /// <summary>
        /// Primary biome classification.
        /// </summary>
        public BiomeType Biome { get; set; } = BiomeType.Grassland;

        /// <summary>
        /// Seed for deterministic generation.
        /// </summary>
        public ulong Seed { get; set; } = 0;

        /// <summary>
        /// Generator version for provenance tracking.
        /// </summary>
        public string GeneratorVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Creates a deep copy of this spec.
        /// </summary>
        public EnvironmentSpec Clone()
        {
            return new EnvironmentSpec
            {
                TemperatureMin = this.TemperatureMin,
                TemperatureMax = this.TemperatureMax,
                WaterAvailability = this.WaterAvailability,
                LightLevel = this.LightLevel,
                NutrientLevel = this.NutrientLevel,
                Gravity = this.Gravity,
                RadiationLevel = this.RadiationLevel,
                OxygenLevel = this.OxygenLevel,
                SeasonalVariation = this.SeasonalVariation,
                Biome = this.Biome,
                Seed = this.Seed,
                GeneratorVersion = this.GeneratorVersion
            };
        }

        /// <summary>
        /// Calculates the average temperature in Kelvin.
        /// </summary>
        public float GetAverageTemperature()
        {
            return (TemperatureMin + TemperatureMax) / 2f;
        }

        /// <summary>
        /// Calculates temperature range (variability).
        /// </summary>
        public float GetTemperatureRange()
        {
            return TemperatureMax - TemperatureMin;
        }
    }
}
