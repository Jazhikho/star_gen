using System;
using System.Collections.Generic;
using System.Linq;

namespace StarGen.Domain.Ecology
{
    /// <summary>
    /// Validation and constraint checking for ecology generation.
    /// </summary>
    public static class EcologyConstraints
    {
        // Temperature bounds (Kelvin)
        public const float MIN_VIABLE_TEMP = 200f;
        public const float MAX_VIABLE_TEMP = 450f;
        public const float OPTIMAL_TEMP_MIN = 273f;
        public const float OPTIMAL_TEMP_MAX = 323f;

        // Energy transfer bounds
        public const float MIN_ENERGY_EFFICIENCY = 0.05f;
        public const float MAX_ENERGY_EFFICIENCY = 0.20f;
        public const float TYPICAL_ENERGY_EFFICIENCY = 0.10f;

        // Biomass ratios (each level should be ~10% of level below)
        public const float MIN_BIOMASS_RATIO = 0.05f;
        public const float MAX_BIOMASS_RATIO = 0.25f;

        // Slot count bounds per level
        public const int MIN_PRODUCERS = 1;
        public const int MAX_PRODUCERS = 12;
        public const int MIN_PRIMARY_CONSUMERS = 1;
        public const int MAX_PRIMARY_CONSUMERS = 10;
        public const int MIN_SECONDARY_CONSUMERS = 0;
        public const int MAX_SECONDARY_CONSUMERS = 8;
        public const int MIN_TERTIARY_CONSUMERS = 0;
        public const int MAX_TERTIARY_CONSUMERS = 5;
        public const int MIN_APEX_PREDATORS = 0;
        public const int MAX_APEX_PREDATORS = 3;
        public const int MIN_DECOMPOSERS = 1;
        public const int MAX_DECOMPOSERS = 4;

        /// <summary>
        /// Validates an environment spec for ecology generation.
        /// Returns list of validation errors (empty = valid).
        /// </summary>
        public static List<string> ValidateEnvironmentSpec(EnvironmentSpec spec)
        {
            var errors = new List<string>();

            if (spec == null)
            {
                errors.Add("EnvironmentSpec cannot be null");
                return errors;
            }

            if (spec.TemperatureMin < 0)
            {
                errors.Add("TemperatureMin cannot be negative: " + spec.TemperatureMin);
            }

            if (spec.TemperatureMax < spec.TemperatureMin)
            {
                errors.Add("TemperatureMax (" + spec.TemperatureMax + ") must be >= TemperatureMin (" + spec.TemperatureMin + ")");
            }

            if (spec.WaterAvailability < 0 || spec.WaterAvailability > 1)
            {
                errors.Add("WaterAvailability must be 0-1: " + spec.WaterAvailability);
            }

            if (spec.LightLevel < 0 || spec.LightLevel > 1)
            {
                errors.Add("LightLevel must be 0-1: " + spec.LightLevel);
            }

            if (spec.NutrientLevel < 0 || spec.NutrientLevel > 1)
            {
                errors.Add("NutrientLevel must be 0-1: " + spec.NutrientLevel);
            }

            if (spec.Gravity <= 0)
            {
                errors.Add("Gravity must be positive: " + spec.Gravity);
            }

            if (spec.RadiationLevel < 0 || spec.RadiationLevel > 1)
            {
                errors.Add("RadiationLevel must be 0-1: " + spec.RadiationLevel);
            }

            if (spec.OxygenLevel < 0 || spec.OxygenLevel > 1)
            {
                errors.Add("OxygenLevel must be 0-1: " + spec.OxygenLevel);
            }

            if (spec.SeasonalVariation < 0 || spec.SeasonalVariation > 1)
            {
                errors.Add("SeasonalVariation must be 0-1: " + spec.SeasonalVariation);
            }

            return errors;
        }

        /// <summary>
        /// Validates a generated ecology web for internal consistency.
        /// </summary>
        public static List<string> ValidateEcologyWeb(EcologyWeb web)
        {
            var errors = new List<string>();

            if (web == null)
            {
                errors.Add("EcologyWeb cannot be null");
                return errors;
            }

            // Must have producers
            var producers = web.GetSlotsByLevel(TrophicLevel.Producer);
            if (producers.Count == 0)
            {
                errors.Add("Ecology must have at least one producer");
            }

            // Must have decomposers
            var decomposers = web.GetSlotsByLevel(TrophicLevel.Decomposer);
            if (decomposers.Count == 0)
            {
                errors.Add("Ecology must have at least one decomposer");
            }

            // Validate connections reference valid slots
            var slotIds = new HashSet<Guid>(web.Slots.Select(s => s.Id));
            foreach (var conn in web.Connections)
            {
                if (!slotIds.Contains(conn.SourceSlotId))
                {
                    errors.Add("Connection references non-existent source slot: " + conn.SourceSlotId);
                }
                if (!slotIds.Contains(conn.TargetSlotId))
                {
                    errors.Add("Connection references non-existent target slot: " + conn.TargetSlotId);
                }
            }

            // Validate no self-feeding
            foreach (var conn in web.Connections)
            {
                if (conn.SourceSlotId == conn.TargetSlotId)
                {
                    errors.Add("Slot cannot feed on itself: " + conn.SourceSlotId);
                }
            }

            // Validate consumers have prey
            foreach (var slot in web.Slots)
            {
                if (slot.Level != TrophicLevel.Producer && slot.Level != TrophicLevel.Decomposer)
                {
                    if (slot.PreySlotIds.Count == 0)
                    {
                        errors.Add("Consumer slot has no prey: " + slot.Id + " (" + slot.Description + ")");
                    }
                }
            }

            // Validate biomass pyramid (roughly)
            errors.AddRange(ValidateBiomassPyramid(web));

            return errors;
        }

        /// <summary>
        /// Validates that biomass follows roughly the 10% rule.
        /// </summary>
        private static List<string> ValidateBiomassPyramid(EcologyWeb web)
        {
            var errors = new List<string>();

            float producerBiomass = web.GetSlotsByLevel(TrophicLevel.Producer).Sum(s => s.BiomassCapacity);
            float primaryBiomass = web.GetSlotsByLevel(TrophicLevel.PrimaryConsumer).Sum(s => s.BiomassCapacity);
            float secondaryBiomass = web.GetSlotsByLevel(TrophicLevel.SecondaryConsumer).Sum(s => s.BiomassCapacity);
            float tertiaryBiomass = web.GetSlotsByLevel(TrophicLevel.TertiaryConsumer).Sum(s => s.BiomassCapacity);
            float apexBiomass = web.GetSlotsByLevel(TrophicLevel.ApexPredator).Sum(s => s.BiomassCapacity);

            if (producerBiomass > 0 && primaryBiomass > 0)
            {
                float ratio = primaryBiomass / producerBiomass;
                if (ratio > MAX_BIOMASS_RATIO)
                {
                    errors.Add("Primary consumer biomass ratio too high: " + ratio.ToString("F2") + " (max " + MAX_BIOMASS_RATIO + ")");
                }
            }

            if (primaryBiomass > 0 && secondaryBiomass > 0)
            {
                float ratio = secondaryBiomass / primaryBiomass;
                if (ratio > MAX_BIOMASS_RATIO)
                {
                    errors.Add("Secondary consumer biomass ratio too high: " + ratio.ToString("F2") + " (max " + MAX_BIOMASS_RATIO + ")");
                }
            }

            // Tertiary and apex can have more flexible ratios due to rarity
            _ = tertiaryBiomass;
            _ = apexBiomass;

            return errors;
        }

        /// <summary>
        /// Calculates habitability score for an environment (0-1).
        /// </summary>
        public static float CalculateHabitability(EnvironmentSpec spec)
        {
            float score = 1.0f;

            // Temperature factor
            float avgTemp = spec.GetAverageTemperature();
            if (avgTemp < MIN_VIABLE_TEMP || avgTemp > MAX_VIABLE_TEMP)
            {
                return 0f;
            }

            float tempOptimality = 1.0f;
            if (avgTemp < OPTIMAL_TEMP_MIN)
            {
                tempOptimality = System.Math.Max(0, (avgTemp - MIN_VIABLE_TEMP) / (OPTIMAL_TEMP_MIN - MIN_VIABLE_TEMP));
            }
            else if (avgTemp > OPTIMAL_TEMP_MAX)
            {
                tempOptimality = System.Math.Max(0, (MAX_VIABLE_TEMP - avgTemp) / (MAX_VIABLE_TEMP - OPTIMAL_TEMP_MAX));
            }
            score *= tempOptimality;

            // Water factor (critical)
            score *= (float)System.Math.Pow(spec.WaterAvailability, 0.5);

            // Radiation penalty
            score *= 1.0f - spec.RadiationLevel * 0.8f;

            // Gravity penalty for extremes
            if (spec.Gravity < 0.3f || spec.Gravity > 3.0f)
            {
                score *= 0.5f;
            }

            return System.Math.Clamp(score, 0f, 1f);
        }

        /// <summary>
        /// Calculates base productivity potential for an environment.
        /// </summary>
        public static float CalculateProductivity(EnvironmentSpec spec)
        {
            float habitability = CalculateHabitability(spec);
            if (habitability <= 0)
            {
                return 0f;
            }

            // Light-based productivity (photosynthesis)
            float photoProductivity = spec.LightLevel * spec.WaterAvailability * spec.NutrientLevel;

            // Chemosynthesis potential (for low-light environments)
            float chemoProductivity = spec.NutrientLevel * 0.3f * (1.0f - spec.LightLevel);

            float baseProductivity = System.Math.Max(photoProductivity, chemoProductivity);

            // Scale by habitability
            return baseProductivity * habitability;
        }
    }
}
