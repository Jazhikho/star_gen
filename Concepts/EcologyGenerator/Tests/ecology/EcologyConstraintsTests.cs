using System;
using System.Linq;
using StarGen.Domain.Ecology;

namespace StarGen.Tests.Ecology
{
    /// <summary>
    /// Unit tests for EcologyConstraints validation.
    /// </summary>
    public class EcologyConstraintsTests
    {
        public void TestValidateEnvironmentSpecAcceptsValid()
        {
            EnvironmentSpec spec = new EnvironmentSpec
            {
                TemperatureMin = 273f,
                TemperatureMax = 310f,
                WaterAvailability = 0.5f,
                LightLevel = 1.0f,
                NutrientLevel = 0.5f,
                Gravity = 1.0f,
                RadiationLevel = 0.1f,
                OxygenLevel = 0.21f,
                SeasonalVariation = 0.3f
            };

            var errors = EcologyConstraints.ValidateEnvironmentSpec(spec);

            Assert(errors.Count == 0, "Valid spec should have no errors, got: " + string.Join(", ", errors));
        }

        public void TestValidateEnvironmentSpecRejectsNegativeTemp()
        {
            EnvironmentSpec spec = new EnvironmentSpec
            {
                TemperatureMin = -10f
            };

            var errors = EcologyConstraints.ValidateEnvironmentSpec(spec);

            Assert(errors.Any(e => e.Contains("TemperatureMin")), "Should reject negative temperature");
        }

        public void TestValidateEnvironmentSpecRejectsInvalidTempRange()
        {
            EnvironmentSpec spec = new EnvironmentSpec
            {
                TemperatureMin = 350f,
                TemperatureMax = 300f
            };

            var errors = EcologyConstraints.ValidateEnvironmentSpec(spec);

            Assert(errors.Any(e => e.Contains("TemperatureMax")), "Should reject max < min temperature");
        }

        public void TestValidateEnvironmentSpecRejectsInvalidWater()
        {
            EnvironmentSpec spec = new EnvironmentSpec
            {
                WaterAvailability = 1.5f
            };

            var errors = EcologyConstraints.ValidateEnvironmentSpec(spec);

            Assert(errors.Any(e => e.Contains("WaterAvailability")), "Should reject water > 1.0");
        }

        public void TestValidateEnvironmentSpecRejectsZeroGravity()
        {
            EnvironmentSpec spec = new EnvironmentSpec
            {
                Gravity = 0f
            };

            var errors = EcologyConstraints.ValidateEnvironmentSpec(spec);

            Assert(errors.Any(e => e.Contains("Gravity")), "Should reject zero gravity");
        }

        public void TestValidateEcologyWebRejectsNoProducers()
        {
            EcologyWeb web = new EcologyWeb();
            web.Slots.Add(new TrophicSlot
            {
                Level = TrophicLevel.PrimaryConsumer
            });
            web.Slots.Add(new TrophicSlot
            {
                Level = TrophicLevel.Decomposer
            });

            var errors = EcologyConstraints.ValidateEcologyWeb(web);

            Assert(errors.Any(e => e.Contains("producer")), "Should reject web without producers");
        }

        public void TestValidateEcologyWebRejectsNoDecomposers()
        {
            EcologyWeb web = new EcologyWeb();
            web.Slots.Add(new TrophicSlot
            {
                Level = TrophicLevel.Producer
            });
            web.Slots.Add(new TrophicSlot
            {
                Level = TrophicLevel.PrimaryConsumer
            });

            var errors = EcologyConstraints.ValidateEcologyWeb(web);

            Assert(errors.Any(e => e.Contains("decomposer")), "Should reject web without decomposers");
        }

        public void TestCalculateHabitabilityOptimalConditions()
        {
            EnvironmentSpec spec = new EnvironmentSpec
            {
                TemperatureMin = 280f,
                TemperatureMax = 300f,
                WaterAvailability = 1.0f,
                RadiationLevel = 0f,
                Gravity = 1.0f
            };

            float habitability = EcologyConstraints.CalculateHabitability(spec);

            Assert(habitability > 0.8f, "Optimal conditions should have high habitability, got " + habitability);
        }

        public void TestCalculateHabitabilityExtremeTemperature()
        {
            EnvironmentSpec spec = new EnvironmentSpec
            {
                TemperatureMin = 500f,
                TemperatureMax = 600f,
                WaterAvailability = 1.0f
            };

            float habitability = EcologyConstraints.CalculateHabitability(spec);

            Assert(habitability == 0f, "Extreme temperature should have zero habitability, got " + habitability);
        }

        public void TestCalculateHabitabilityNoWater()
        {
            EnvironmentSpec spec = new EnvironmentSpec
            {
                TemperatureMin = 280f,
                TemperatureMax = 300f,
                WaterAvailability = 0f
            };

            float habitability = EcologyConstraints.CalculateHabitability(spec);

            Assert(habitability == 0f, "No water should have zero habitability, got " + habitability);
        }

        public void TestCalculateProductivityHighLight()
        {
            EnvironmentSpec spec = new EnvironmentSpec
            {
                TemperatureMin = 280f,
                TemperatureMax = 300f,
                WaterAvailability = 0.8f,
                LightLevel = 1.0f,
                NutrientLevel = 0.8f,
                RadiationLevel = 0f,
                Gravity = 1.0f
            };

            float productivity = EcologyConstraints.CalculateProductivity(spec);

            Assert(productivity > 0.5f, "Good conditions should have high productivity, got " + productivity);
        }

        public void TestCalculateProductivityChemosynthesis()
        {
            EnvironmentSpec spec = new EnvironmentSpec
            {
                TemperatureMin = 280f,
                TemperatureMax = 300f,
                WaterAvailability = 0.8f,
                LightLevel = 0f,
                NutrientLevel = 0.8f,
                RadiationLevel = 0f,
                Gravity = 1.0f
            };

            float productivity = EcologyConstraints.CalculateProductivity(spec);

            Assert(productivity > 0f, "Chemosynthesis should still provide productivity, got " + productivity);
        }

        private void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }
    }
}

