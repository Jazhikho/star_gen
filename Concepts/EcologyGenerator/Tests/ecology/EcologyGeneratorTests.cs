using System;
using System.Linq;
using StarGen.Domain.Ecology;

namespace StarGen.Tests.Ecology
{
    /// <summary>
    /// Unit tests for EcologyGenerator.
    /// </summary>
    public class EcologyGeneratorTests
    {
        public void TestGenerateCreatesValidWeb()
        {
            EnvironmentSpec spec = CreateDefaultSpec();
            EcologyRng rng = new EcologyRng(spec.Seed);

            EcologyWeb web = EcologyGenerator.Generate(spec, rng);

            Assert(web != null, "Web should not be null");
            Assert(web!.Slots.Count > 0, "Web should have slots");
            Assert(web!.Connections.Count > 0, "Web should have connections");
        }

        public void TestGenerateAlwaysHasProducers()
        {
            EnvironmentSpec spec = CreateDefaultSpec();
            EcologyRng rng = new EcologyRng(spec.Seed);

            EcologyWeb web = EcologyGenerator.Generate(spec, rng);

            var producers = web.GetSlotsByLevel(TrophicLevel.Producer);
            Assert(producers.Count >= 1, "Should have at least 1 producer, got " + producers.Count);
        }

        public void TestGenerateAlwaysHasDecomposers()
        {
            EnvironmentSpec spec = CreateDefaultSpec();
            EcologyRng rng = new EcologyRng(spec.Seed);

            EcologyWeb web = EcologyGenerator.Generate(spec, rng);

            var decomposers = web.GetSlotsByLevel(TrophicLevel.Decomposer);
            Assert(decomposers.Count >= 1, "Should have at least 1 decomposer, got " + decomposers.Count);
        }

        public void TestGenerateConsumersHavePrey()
        {
            EnvironmentSpec spec = CreateDefaultSpec();
            EcologyRng rng = new EcologyRng(spec.Seed);

            EcologyWeb web = EcologyGenerator.Generate(spec, rng);

            foreach (TrophicSlot slot in web.Slots)
            {
                if (slot.Level != TrophicLevel.Producer && slot.Level != TrophicLevel.Decomposer)
                {
                    Assert(slot.PreySlotIds.Count > 0, "Consumer " + slot.Description + " should have prey");
                }
            }
        }

        public void TestGenerateBiomassPyramidValid()
        {
            EnvironmentSpec spec = CreateDefaultSpec();
            spec.WaterAvailability = 0.8f;
            spec.LightLevel = 1.0f;
            EcologyRng rng = new EcologyRng(spec.Seed);

            EcologyWeb web = EcologyGenerator.Generate(spec, rng);

            float producerBiomass = web.GetSlotsByLevel(TrophicLevel.Producer).Sum(s => s.BiomassCapacity);
            float primaryBiomass = web.GetSlotsByLevel(TrophicLevel.PrimaryConsumer).Sum(s => s.BiomassCapacity);

            if (primaryBiomass > 0)
            {
                float ratio = primaryBiomass / producerBiomass;
                Assert(ratio <= 0.25f, "Primary/Producer biomass ratio should be <= 0.25, got " + ratio.ToString("F2"));
            }
        }

        public void TestGenerateLowProductivityEnvironment()
        {
            EnvironmentSpec spec = CreateDefaultSpec();
            spec.WaterAvailability = 0.05f;
            spec.LightLevel = 0.1f;
            spec.NutrientLevel = 0.1f;
            EcologyRng rng = new EcologyRng(spec.Seed);

            EcologyWeb web = EcologyGenerator.Generate(spec, rng);

            Assert(web.Slots.Count >= 2, "Even barren environments should have at least 2 slots");
            Assert(web.TotalProductivity < 0.1f, "Low resources should mean low productivity, got " + web.TotalProductivity);
        }

        public void TestGenerateHighProductivityEnvironment()
        {
            EnvironmentSpec spec = CreateDefaultSpec();
            spec.WaterAvailability = 0.9f;
            spec.LightLevel = 1.0f;
            spec.NutrientLevel = 0.9f;
            EcologyRng rng = new EcologyRng(spec.Seed);

            EcologyWeb web = EcologyGenerator.Generate(spec, rng);

            Assert(web.Slots.Count > 10, "High productivity should create complex ecology, got " + web.Slots.Count + " slots");
        }

        public void TestGenerateStoresProvenance()
        {
            EnvironmentSpec spec = CreateDefaultSpec();
            spec.Seed = 99999;
            EcologyRng rng = new EcologyRng(spec.Seed);

            EcologyWeb web = EcologyGenerator.Generate(spec, rng);

            Assert(web.SourceSpec != null, "Should store source spec");
            Assert(web.SourceSpec!.Seed == 99999, "Should store correct seed");
            Assert(web.GeneratorVersion == EcologyGenerator.GENERATOR_VERSION, "Should store generator version");
        }

        public void TestGenerateNoSelfFeedingConnections()
        {
            EnvironmentSpec spec = CreateDefaultSpec();
            EcologyRng rng = new EcologyRng(spec.Seed);

            EcologyWeb web = EcologyGenerator.Generate(spec, rng);

            foreach (TrophicConnection conn in web.Connections)
            {
                Assert(conn.SourceSlotId != conn.TargetSlotId, "No slot should feed on itself");
            }
        }

        public void TestGenerateConnectionsReferenceValidSlots()
        {
            EnvironmentSpec spec = CreateDefaultSpec();
            EcologyRng rng = new EcologyRng(spec.Seed);

            EcologyWeb web = EcologyGenerator.Generate(spec, rng);
            var slotIds = web.Slots.Select(s => s.Id).ToHashSet();

            foreach (TrophicConnection conn in web.Connections)
            {
                Assert(slotIds.Contains(conn.SourceSlotId), "Connection source " + conn.SourceSlotId + " should exist in slots");
                Assert(slotIds.Contains(conn.TargetSlotId), "Connection target " + conn.TargetSlotId + " should exist in slots");
            }
        }

        private EnvironmentSpec CreateDefaultSpec()
        {
            EnvironmentSpec spec = new EnvironmentSpec
            {
                Seed = 12345,
                TemperatureMin = 273f,
                TemperatureMax = 310f,
                WaterAvailability = 0.5f,
                LightLevel = 1.0f,
                NutrientLevel = 0.5f,
                Gravity = 1.0f,
                RadiationLevel = 0.1f,
                OxygenLevel = 0.21f,
                SeasonalVariation = 0.3f,
                Biome = BiomeType.Grassland
            };
            return spec;
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

