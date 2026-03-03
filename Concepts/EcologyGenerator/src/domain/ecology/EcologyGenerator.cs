using System;
using System.Collections.Generic;
using System.Linq;

namespace StarGen.Domain.Ecology
{
    /// <summary>
    /// Deterministic procedural generator for ecology webs.
    /// </summary>
    public static class EcologyGenerator
    {
        public const string GENERATOR_VERSION = "1.0.0";

        /// <summary>
        /// Generates a complete ecology web from an environment specification.
        /// </summary>
        public static EcologyWeb Generate(EnvironmentSpec spec, EcologyRng rng)
        {
            List<string> errors = EcologyConstraints.ValidateEnvironmentSpec(spec);
            if (errors.Count > 0)
            {
                throw new ArgumentException("Invalid spec: " + string.Join("; ", errors));
            }

            EcologyWeb web = new EcologyWeb
            {
                SourceSpec = spec.Clone(),
                GeneratorVersion = GENERATOR_VERSION
            };

            float productivity = EcologyConstraints.CalculateProductivity(spec);
            web.TotalProductivity = productivity;

            if (productivity <= 0.01f)
            {
                GenerateMinimalEcology(web, spec, rng);
            }
            else
            {
                GenerateProducers(web, spec, rng, productivity);
                GeneratePrimaryConsumers(web, spec, rng);
                GenerateSecondaryConsumers(web, spec, rng);
                GenerateTertiaryConsumers(web, spec, rng);
                GenerateApexPredators(web, spec, rng);
                GenerateDecomposers(web, spec, rng);
            }

            BuildFoodWeb(web, rng);
            CalculateMetrics(web);

            return web;
        }

        private static void GenerateMinimalEcology(EcologyWeb web, EnvironmentSpec spec, EcologyRng rng)
        {
            TrophicSlot producer = CreateSlot(TrophicLevel.Producer, NicheType.Chemosynthesizer, spec, rng);
            producer.Description = "Extremophile chemotroph";
            producer.BiomassCapacity = 10f;
            web.Slots.Add(producer);

            TrophicSlot decomposer = CreateSlot(TrophicLevel.Decomposer, NicheType.Reducer, spec, rng);
            decomposer.Description = "Microbial reducer";
            decomposer.BiomassCapacity = 5f;
            web.Slots.Add(decomposer);
        }

        private static void GenerateProducers(EcologyWeb web, EnvironmentSpec spec, EcologyRng rng, float productivity)
        {
            int count = CalculateSlotCount(
                productivity,
                EcologyConstraints.MIN_PRODUCERS,
                EcologyConstraints.MAX_PRODUCERS,
                rng
            );

            bool hasPhotosynthesis = spec.LightLevel > 0.2f;
            bool hasChemosynthesis = spec.NutrientLevel > 0.3f && spec.LightLevel < 0.5f;

            List<NicheType> nichePool = new List<NicheType>();
            if (hasPhotosynthesis)
            {
                nichePool.Add(NicheType.Photosynthesizer);
            }
            if (hasChemosynthesis)
            {
                nichePool.Add(NicheType.Chemosynthesizer);
            }
            if (spec.GetAverageTemperature() > 350f)
            {
                nichePool.Add(NicheType.Thermosynthesizer);
            }
            if (nichePool.Count == 0)
            {
                nichePool.Add(NicheType.Chemosynthesizer);
            }

            float totalBiomass = productivity * 1000f;

            for (int i = 0; i < count; i++)
            {
                NicheType niche = nichePool[rng.NextIntRange(0, nichePool.Count)];
                TrophicSlot slot = CreateSlot(TrophicLevel.Producer, niche, spec, rng);
                slot.Description = GenerateProducerDescription(niche, spec, rng);
                slot.BiomassCapacity = totalBiomass / count * rng.NextFloatRange(0.5f, 1.5f);
                web.Slots.Add(slot);
            }
        }

        private static void GeneratePrimaryConsumers(EcologyWeb web, EnvironmentSpec spec, EcologyRng rng)
        {
            float producerBiomass = web.GetSlotsByLevel(TrophicLevel.Producer).Sum(s => s.BiomassCapacity);
            float availableBiomass = producerBiomass * rng.NextFloatRange(0.08f, 0.15f);

            int count = CalculateSlotCount(
                availableBiomass / 100f,
                EcologyConstraints.MIN_PRIMARY_CONSUMERS,
                EcologyConstraints.MAX_PRIMARY_CONSUMERS,
                rng
            );

            NicheType[] nichePool = new NicheType[] { NicheType.Grazer, NicheType.Browser, NicheType.FilterFeeder };

            for (int i = 0; i < count; i++)
            {
                NicheType niche = rng.PickRandom(nichePool);
                TrophicSlot slot = CreateSlot(TrophicLevel.PrimaryConsumer, niche, spec, rng);
                slot.Description = GenerateConsumerDescription(niche, 1, spec, rng);
                slot.BiomassCapacity = availableBiomass / count * rng.NextFloatRange(0.5f, 1.5f);
                slot.SizeClass = rng.NextIntRange(0, 3);
                web.Slots.Add(slot);
            }
        }

        private static void GenerateSecondaryConsumers(EcologyWeb web, EnvironmentSpec spec, EcologyRng rng)
        {
            float primaryBiomass = web.GetSlotsByLevel(TrophicLevel.PrimaryConsumer).Sum(s => s.BiomassCapacity);
            if (primaryBiomass < 10f)
            {
                return;
            }

            float availableBiomass = primaryBiomass * rng.NextFloatRange(0.08f, 0.15f);

            int count = CalculateSlotCount(
                availableBiomass / 50f,
                EcologyConstraints.MIN_SECONDARY_CONSUMERS,
                EcologyConstraints.MAX_SECONDARY_CONSUMERS,
                rng
            );

            NicheType[] nichePool = new NicheType[]
            {
                NicheType.ActiveHunter,
                NicheType.AmbushPredator,
                NicheType.Omnivore,
                NicheType.Parasite
            };

            for (int i = 0; i < count; i++)
            {
                NicheType niche = rng.PickRandom(nichePool);
                TrophicSlot slot = CreateSlot(TrophicLevel.SecondaryConsumer, niche, spec, rng);
                slot.Description = GenerateConsumerDescription(niche, 2, spec, rng);
                slot.BiomassCapacity = availableBiomass / count * rng.NextFloatRange(0.5f, 1.5f);
                slot.SizeClass = rng.NextIntRange(1, 4);
                web.Slots.Add(slot);
            }
        }

        private static void GenerateTertiaryConsumers(EcologyWeb web, EnvironmentSpec spec, EcologyRng rng)
        {
            float secondaryBiomass = web.GetSlotsByLevel(TrophicLevel.SecondaryConsumer).Sum(s => s.BiomassCapacity);
            if (secondaryBiomass < 5f)
            {
                return;
            }

            float availableBiomass = secondaryBiomass * rng.NextFloatRange(0.08f, 0.15f);

            int count = CalculateSlotCount(
                availableBiomass / 20f,
                EcologyConstraints.MIN_TERTIARY_CONSUMERS,
                EcologyConstraints.MAX_TERTIARY_CONSUMERS,
                rng
            );

            NicheType[] nichePool = new NicheType[] { NicheType.ActiveHunter, NicheType.AmbushPredator };

            for (int i = 0; i < count; i++)
            {
                NicheType niche = rng.PickRandom(nichePool);
                TrophicSlot slot = CreateSlot(TrophicLevel.TertiaryConsumer, niche, spec, rng);
                slot.Description = GenerateConsumerDescription(niche, 3, spec, rng);
                slot.BiomassCapacity = availableBiomass / count * rng.NextFloatRange(0.5f, 1.5f);
                slot.SizeClass = rng.NextIntRange(2, 5);
                web.Slots.Add(slot);
            }
        }

        private static void GenerateApexPredators(EcologyWeb web, EnvironmentSpec spec, EcologyRng rng)
        {
            float preyBiomass = web.GetSlotsByLevel(TrophicLevel.SecondaryConsumer).Sum(s => s.BiomassCapacity)
                              + web.GetSlotsByLevel(TrophicLevel.TertiaryConsumer).Sum(s => s.BiomassCapacity);
            if (preyBiomass < 3f)
            {
                return;
            }

            float availableBiomass = preyBiomass * rng.NextFloatRange(0.05f, 0.12f);

            int count = CalculateSlotCount(
                availableBiomass / 10f,
                EcologyConstraints.MIN_APEX_PREDATORS,
                EcologyConstraints.MAX_APEX_PREDATORS,
                rng
            );

            for (int i = 0; i < count; i++)
            {
                NicheType niche = rng.NextBool(0.7f) ? NicheType.ActiveHunter : NicheType.AmbushPredator;
                TrophicSlot slot = CreateSlot(TrophicLevel.ApexPredator, niche, spec, rng);
                slot.Description = "Apex " + (niche == NicheType.ActiveHunter ? "hunter" : "ambush predator");
                slot.BiomassCapacity = availableBiomass / count * rng.NextFloatRange(0.5f, 1.5f);
                slot.SizeClass = rng.NextIntRange(3, 5);
                slot.EnergyEfficiency = rng.NextFloatRange(0.12f, 0.18f);
                web.Slots.Add(slot);
            }
        }

        private static void GenerateDecomposers(EcologyWeb web, EnvironmentSpec spec, EcologyRng rng)
        {
            float totalBiomass = web.GetTotalBiomass();

            int count = CalculateSlotCount(
                totalBiomass / 500f,
                EcologyConstraints.MIN_DECOMPOSERS,
                EcologyConstraints.MAX_DECOMPOSERS,
                rng
            );

            NicheType[] nichePool = new NicheType[]
            {
                NicheType.Detritivore,
                NicheType.Saprophyte,
                NicheType.Reducer
            };

            for (int i = 0; i < count; i++)
            {
                NicheType niche = rng.PickRandom(nichePool);
                TrophicSlot slot = CreateSlot(TrophicLevel.Decomposer, niche, spec, rng);
                slot.Description = GenerateDecomposerDescription(niche, rng);
                slot.BiomassCapacity = totalBiomass * 0.05f / count * rng.NextFloatRange(0.5f, 1.5f);
                slot.SizeClass = rng.NextIntRange(0, 2);
                web.Slots.Add(slot);
            }
        }

        private static TrophicSlot CreateSlot(TrophicLevel level, NicheType niche, EnvironmentSpec spec, EcologyRng rng)
        {
            float tempRange = spec.GetTemperatureRange();
            float avgTemp = spec.GetAverageTemperature();

            return new TrophicSlot
            {
                Level = level,
                Niche = niche,
                EnergyEfficiency = rng.NextFloatRange(
                    EcologyConstraints.MIN_ENERGY_EFFICIENCY,
                    EcologyConstraints.MAX_ENERGY_EFFICIENCY
                ),
                TemperatureToleranceMin = avgTemp - tempRange * rng.NextFloatRange(0.5f, 1.5f) - 10f,
                TemperatureToleranceMax = avgTemp + tempRange * rng.NextFloatRange(0.5f, 1.5f) + 10f,
                WaterRequirement = spec.WaterAvailability * rng.NextFloatRange(0.3f, 1.0f),
                IsGeneralist = rng.NextBool(0.4f)
            };
        }

        private static int CalculateSlotCount(float factor, int min, int max, EcologyRng rng)
        {
            float normalized = System.Math.Clamp(factor, 0f, 1f);
            int baseCount = (int)(min + (max - min) * normalized);
            int variation = rng.NextIntRange(-1, 2);
            return System.Math.Clamp(baseCount + variation, min, max);
        }

        private static void BuildFoodWeb(EcologyWeb web, EcologyRng rng)
        {
            Dictionary<TrophicLevel, List<TrophicSlot>> slotsByLevel = new Dictionary<TrophicLevel, List<TrophicSlot>>();
            foreach (TrophicLevel level in Enum.GetValues(typeof(TrophicLevel)))
            {
                slotsByLevel[level] = web.GetSlotsByLevel(level);
            }

            ConnectLevels(web, slotsByLevel[TrophicLevel.PrimaryConsumer], slotsByLevel[TrophicLevel.Producer], rng);

            List<TrophicSlot> secondaryPrey = new List<TrophicSlot>(slotsByLevel[TrophicLevel.PrimaryConsumer]);
            ConnectLevels(web, slotsByLevel[TrophicLevel.SecondaryConsumer], secondaryPrey, rng);
            ConnectOmnivores(web, slotsByLevel[TrophicLevel.SecondaryConsumer], slotsByLevel[TrophicLevel.Producer], rng);

            List<TrophicSlot> tertiaryPrey = new List<TrophicSlot>(slotsByLevel[TrophicLevel.PrimaryConsumer]);
            tertiaryPrey.AddRange(slotsByLevel[TrophicLevel.SecondaryConsumer]);
            ConnectLevels(web, slotsByLevel[TrophicLevel.TertiaryConsumer], tertiaryPrey, rng);

            List<TrophicSlot> apexPrey = new List<TrophicSlot>(slotsByLevel[TrophicLevel.SecondaryConsumer]);
            apexPrey.AddRange(slotsByLevel[TrophicLevel.TertiaryConsumer]);
            if (apexPrey.Count == 0)
            {
                apexPrey.AddRange(slotsByLevel[TrophicLevel.PrimaryConsumer]);
            }
            ConnectLevels(web, slotsByLevel[TrophicLevel.ApexPredator], apexPrey, rng);

            List<TrophicSlot> allNonDecomposers = web.Slots.Where(s => s.Level != TrophicLevel.Decomposer).ToList();
            foreach (TrophicSlot decomposer in slotsByLevel[TrophicLevel.Decomposer])
            {
                int connectionCount = rng.NextIntRange(2, System.Math.Min(allNonDecomposers.Count + 1, 6));
                TrophicSlot[] shuffled = allNonDecomposers.ToArray();
                rng.Shuffle(shuffled);

                for (int i = 0; i < connectionCount && i < shuffled.Length; i++)
                {
                    CreateConnection(web, shuffled[i], decomposer, rng);
                }
            }
        }

        private static void ConnectLevels(EcologyWeb web, List<TrophicSlot> consumers, List<TrophicSlot> prey, EcologyRng rng)
        {
            if (prey.Count == 0)
            {
                return;
            }

            foreach (TrophicSlot consumer in consumers)
            {
                int minConnections = consumer.IsGeneralist ? 2 : 1;
                int maxConnections = consumer.IsGeneralist ? System.Math.Min(prey.Count, 5) : System.Math.Min(prey.Count, 3);
                int connectionCount = rng.NextIntRange(minConnections, maxConnections + 1);

                TrophicSlot[] shuffledPrey = prey.ToArray();
                rng.Shuffle(shuffledPrey);

                for (int i = 0; i < connectionCount && i < shuffledPrey.Length; i++)
                {
                    CreateConnection(web, shuffledPrey[i], consumer, rng);
                }
            }
        }

        private static void ConnectOmnivores(EcologyWeb web, List<TrophicSlot> consumers, List<TrophicSlot> producers, EcologyRng rng)
        {
            if (producers.Count == 0)
            {
                return;
            }

            foreach (TrophicSlot consumer in consumers.Where(c => c.Niche == NicheType.Omnivore))
            {
                int connectionCount = rng.NextIntRange(1, System.Math.Min(producers.Count + 1, 3));
                TrophicSlot[] shuffled = producers.ToArray();
                rng.Shuffle(shuffled);

                for (int i = 0; i < connectionCount && i < shuffled.Length; i++)
                {
                    CreateConnection(web, shuffled[i], consumer, rng);
                }
            }
        }

        private static void CreateConnection(EcologyWeb web, TrophicSlot source, TrophicSlot target, EcologyRng rng)
        {
            if (web.Connections.Any(c => c.SourceSlotId == source.Id && c.TargetSlotId == target.Id))
            {
                return;
            }

            TrophicConnection connection = new TrophicConnection
            {
                SourceSlotId = source.Id,
                TargetSlotId = target.Id,
                Strength = rng.NextFloatRange(0.2f, 1.0f),
                IsObligate = !target.IsGeneralist && rng.NextBool(0.3f)
            };

            web.Connections.Add(connection);
            source.PredatorSlotIds.Add(target.Id);
            target.PreySlotIds.Add(source.Id);
        }

        private static void CalculateMetrics(EcologyWeb web)
        {
            float maxPossibleConnections = web.Slots.Count * (web.Slots.Count - 1) / 2f;
            if (maxPossibleConnections > 0)
            {
                web.ComplexityScore = System.Math.Min(1f, web.Connections.Count / (maxPossibleConnections * 0.3f));
            }
            else
            {
                web.ComplexityScore = 0f;
            }

            float avgPreyCount = web.Slots
                .Where(s => s.Level != TrophicLevel.Producer && s.Level != TrophicLevel.Decomposer)
                .Select(s => (float)s.PreySlotIds.Count)
                .DefaultIfEmpty(0)
                .Average();

            float redundancyScore = System.Math.Min(1f, avgPreyCount / 3f);

            int actualLevels = Enum.GetValues(typeof(TrophicLevel))
                .Cast<TrophicLevel>()
                .Count(level => web.GetSlotsByLevel(level).Count > 0);
            float balanceScore = (float)actualLevels / 6f;

            web.StabilityScore = (redundancyScore + balanceScore) / 2f;
        }

        private static string GenerateProducerDescription(NicheType niche, EnvironmentSpec spec, EcologyRng rng)
        {
            string[] photoDescriptors = new string[] { "Surface", "Canopy", "Understory", "Floating", "Benthic" };
            string[] chemoDescriptors = new string[] { "Vent", "Seep", "Subsurface", "Mineral" };
            string[] thermoDescriptors = new string[] { "Thermal pool", "Volcanic", "Hydrothermal" };

            if (niche == NicheType.Photosynthesizer)
            {
                return rng.PickRandom(photoDescriptors) + " phototroph";
            }
            if (niche == NicheType.Chemosynthesizer)
            {
                return rng.PickRandom(chemoDescriptors) + " chemotroph";
            }
            if (niche == NicheType.Thermosynthesizer)
            {
                return rng.PickRandom(thermoDescriptors) + " thermophile";
            }
            _ = spec;
            return "Primary producer";
        }

        private static string GenerateConsumerDescription(NicheType niche, int tier, EnvironmentSpec spec, EcologyRng rng)
        {
            string[] sizes = new string[] { "Micro", "Small", "Medium", "Large", "Mega" };
            int minIndex = System.Math.Max(0, tier - 1);
            int maxIndex = System.Math.Min(sizes.Length, tier + 2);
            int sizeIndex = rng.NextIntRange(minIndex, maxIndex);
            string sizePrefix = sizes[sizeIndex];

            _ = spec;

            if (niche == NicheType.Grazer)
            {
                return sizePrefix + " grazer";
            }
            if (niche == NicheType.Browser)
            {
                return sizePrefix + " browser";
            }
            if (niche == NicheType.FilterFeeder)
            {
                return sizePrefix + " filter feeder";
            }
            if (niche == NicheType.ActiveHunter)
            {
                return sizePrefix + " active predator";
            }
            if (niche == NicheType.AmbushPredator)
            {
                return sizePrefix + " ambush predator";
            }
            if (niche == NicheType.Scavenger)
            {
                return sizePrefix + " scavenger";
            }
            if (niche == NicheType.Parasite)
            {
                return sizePrefix + " parasite";
            }
            if (niche == NicheType.Omnivore)
            {
                return sizePrefix + " omnivore";
            }
            return "Consumer (tier " + tier + ")";
        }

        private static string GenerateDecomposerDescription(NicheType niche, EcologyRng rng)
        {
            if (niche == NicheType.Detritivore)
            {
                if (rng.NextBool(0.5f))
                {
                    return "Detritivore colony";
                }
                return "Scavenging detritivore";
            }
            if (niche == NicheType.Saprophyte)
            {
                if (rng.NextBool(0.5f))
                {
                    return "Fungal decomposer";
                }
                return "Saprophytic mat";
            }
            if (niche == NicheType.Reducer)
            {
                if (rng.NextBool(0.5f))
                {
                    return "Bacterial reducer";
                }
                return "Microbial consortium";
            }
            return "Decomposer";
        }
    }
}
