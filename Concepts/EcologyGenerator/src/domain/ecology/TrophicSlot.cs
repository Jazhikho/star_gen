using System;
using System.Collections.Generic;

namespace StarGen.Domain.Ecology
{
    /// <summary>
    /// A slot/niche in the ecosystem that can be filled by organisms.
    /// Represents a functional role rather than a specific species.
    /// </summary>
    public class TrophicSlot
    {
        /// <summary>
        /// Unique identifier for this slot.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Position in energy hierarchy.
        /// </summary>
        public TrophicLevel Level { get; set; }

        /// <summary>
        /// Ecological role/strategy.
        /// </summary>
        public NicheType Niche { get; set; }

        /// <summary>
        /// Human-readable description of this ecological role.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Relative biomass capacity (arbitrary units, used for ratios).
        /// </summary>
        public float BiomassCapacity { get; set; }

        /// <summary>
        /// Energy transfer efficiency when feeding (0.05 - 0.20 typically).
        /// </summary>
        public float EnergyEfficiency { get; set; } = 0.10f;

        /// <summary>
        /// Minimum temperature tolerance in Kelvin.
        /// </summary>
        public float TemperatureToleranceMin { get; set; }

        /// <summary>
        /// Maximum temperature tolerance in Kelvin.
        /// </summary>
        public float TemperatureToleranceMax { get; set; }

        /// <summary>
        /// Minimum water requirement (0-1).
        /// </summary>
        public float WaterRequirement { get; set; }

        /// <summary>
        /// Whether this slot represents a generalist (many connections) or specialist (few).
        /// </summary>
        public bool IsGeneralist { get; set; }

        /// <summary>
        /// Size class (0 = microscopic, 1 = small, 2 = medium, 3 = large, 4 = megafauna).
        /// </summary>
        public int SizeClass { get; set; }

        /// <summary>
        /// IDs of slots this feeds on (prey).
        /// </summary>
        public List<Guid> PreySlotIds { get; set; } = new List<Guid>();

        /// <summary>
        /// IDs of slots that feed on this (predators).
        /// </summary>
        public List<Guid> PredatorSlotIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Creates a deep copy of this slot (without connection IDs).
        /// </summary>
        public TrophicSlot CloneWithoutConnections()
        {
            return new TrophicSlot
            {
                Id = Guid.NewGuid(),
                Level = this.Level,
                Niche = this.Niche,
                Description = this.Description,
                BiomassCapacity = this.BiomassCapacity,
                EnergyEfficiency = this.EnergyEfficiency,
                TemperatureToleranceMin = this.TemperatureToleranceMin,
                TemperatureToleranceMax = this.TemperatureToleranceMax,
                WaterRequirement = this.WaterRequirement,
                IsGeneralist = this.IsGeneralist,
                SizeClass = this.SizeClass,
                PreySlotIds = new List<Guid>(),
                PredatorSlotIds = new List<Guid>()
            };
        }
    }
}
