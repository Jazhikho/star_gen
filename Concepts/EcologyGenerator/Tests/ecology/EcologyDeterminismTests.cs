using System;
using System.Linq;
using StarGen.Domain.Ecology;

namespace StarGen.Tests.Ecology
{
    /// <summary>
    /// Determinism tests for ecology generation.
    /// Same seed + spec must produce identical results.
    /// </summary>
    public class EcologyDeterminismTests
    {
        public void TestSameSeedProducesSameSlotCount()
        {
            EnvironmentSpec spec = CreateSpec(12345);

            EcologyWeb web1 = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));
            EcologyWeb web2 = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));

            Assert(web1.Slots.Count == web2.Slots.Count, "Same seed should produce same slot count: " + web1.Slots.Count + " vs " + web2.Slots.Count);
        }

        public void TestSameSeedProducesSameConnectionCount()
        {
            EnvironmentSpec spec = CreateSpec(12345);

            EcologyWeb web1 = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));
            EcologyWeb web2 = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));

            Assert(web1.Connections.Count == web2.Connections.Count, "Same seed should produce same connection count: " + web1.Connections.Count + " vs " + web2.Connections.Count);
        }

        public void TestSameSeedProducesSameTotalBiomass()
        {
            EnvironmentSpec spec = CreateSpec(12345);

            EcologyWeb web1 = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));
            EcologyWeb web2 = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));

            float biomass1 = web1.GetTotalBiomass();
            float biomass2 = web2.GetTotalBiomass();

            Assert(System.Math.Abs(biomass1 - biomass2) < 0.001f, "Same seed should produce same total biomass: " + biomass1 + " vs " + biomass2);
        }

        public void TestSameSeedProducesSameSlotDescriptions()
        {
            EnvironmentSpec spec = CreateSpec(12345);

            EcologyWeb web1 = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));
            EcologyWeb web2 = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));

            var descriptions1 = web1.Slots.Select(s => s.Description).OrderBy(d => d).ToList();
            var descriptions2 = web2.Slots.Select(s => s.Description).OrderBy(d => d).ToList();

            Assert(descriptions1.SequenceEqual(descriptions2), "Same seed should produce same slot descriptions");
        }

        public void TestSameSeedProducesSameMetrics()
        {
            EnvironmentSpec spec = CreateSpec(12345);

            EcologyWeb web1 = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));
            EcologyWeb web2 = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));

            Assert(System.Math.Abs(web1.ComplexityScore - web2.ComplexityScore) < 0.001f, "Same seed should produce same complexity: " + web1.ComplexityScore + " vs " + web2.ComplexityScore);
            Assert(System.Math.Abs(web1.StabilityScore - web2.StabilityScore) < 0.001f, "Same seed should produce same stability: " + web1.StabilityScore + " vs " + web2.StabilityScore);
        }

        public void TestDifferentSeedsProduceDifferentResults()
        {
            EnvironmentSpec spec1 = CreateSpec(12345);
            EnvironmentSpec spec2 = CreateSpec(54321);

            EcologyWeb web1 = EcologyGenerator.Generate(spec1, new EcologyRng(spec1.Seed));
            EcologyWeb web2 = EcologyGenerator.Generate(spec2, new EcologyRng(spec2.Seed));

            bool slotsDiffer = web1.Slots.Count != web2.Slots.Count;
            bool connectionsDiffer = web1.Connections.Count != web2.Connections.Count;
            bool biomassDiffer = System.Math.Abs(web1.GetTotalBiomass() - web2.GetTotalBiomass()) > 1f;

            Assert(slotsDiffer || connectionsDiffer || biomassDiffer, "Different seeds should produce different results");
        }

        public void TestRngDeterminism()
        {
            EcologyRng rng1 = new EcologyRng(42);
            EcologyRng rng2 = new EcologyRng(42);

            for (int i = 0; i < 100; i++)
            {
                float v1 = rng1.NextFloat();
                float v2 = rng2.NextFloat();
                Assert(v1 == v2, "RNG should be deterministic at iteration " + i + ": " + v1 + " vs " + v2);
            }
        }

        public void TestRngRangeValues()
        {
            EcologyRng rng = new EcologyRng(42);

            for (int i = 0; i < 100; i++)
            {
                float value = rng.NextFloatRange(0.5f, 1.5f);
                Assert(value >= 0.5f && value <= 1.5f, "NextFloatRange should respect bounds: " + value);
            }
        }

        public void TestRngIntRangeValues()
        {
            EcologyRng rng = new EcologyRng(42);

            for (int i = 0; i < 100; i++)
            {
                int value = rng.NextIntRange(5, 10);
                Assert(value >= 5 && value < 10, "NextIntRange should respect bounds [5,10): " + value);
            }
        }

        public void TestMultipleGenerationsWithSameSeed()
        {
            EnvironmentSpec spec = CreateSpec(99999);

            EcologyWeb[] webs = new EcologyWeb[5];
            for (int i = 0; i < 5; i++)
            {
                webs[i] = EcologyGenerator.Generate(spec, new EcologyRng(spec.Seed));
            }

            for (int i = 1; i < 5; i++)
            {
                Assert(webs[0].Slots.Count == webs[i].Slots.Count, "Generation " + i + " slot count should match generation 0");
                Assert(webs[0].Connections.Count == webs[i].Connections.Count, "Generation " + i + " connection count should match generation 0");
            }
        }

        private EnvironmentSpec CreateSpec(ulong seed)
        {
            EnvironmentSpec spec = new EnvironmentSpec
            {
                Seed = seed,
                TemperatureMin = 273f,
                TemperatureMax = 310f,
                WaterAvailability = 0.6f,
                LightLevel = 0.9f,
                NutrientLevel = 0.5f,
                Gravity = 1.0f,
                RadiationLevel = 0.1f,
                OxygenLevel = 0.21f,
                SeasonalVariation = 0.3f,
                Biome = BiomeType.Forest
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

