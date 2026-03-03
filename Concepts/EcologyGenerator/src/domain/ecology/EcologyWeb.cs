using System;
using System.Collections.Generic;
using System.Linq;

namespace StarGen.Domain.Ecology
{
    /// <summary>
    /// Complete ecosystem web containing all trophic slots and their connections.
    /// </summary>
    public class EcologyWeb
    {
        /// <summary>
        /// Unique identifier for this ecology.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The environment spec used to generate this web (provenance).
        /// </summary>
        public EnvironmentSpec? SourceSpec { get; set; }

        /// <summary>
        /// All trophic slots in this ecosystem.
        /// </summary>
        public List<TrophicSlot> Slots { get; set; } = new List<TrophicSlot>();

        /// <summary>
        /// All feeding relationships between slots.
        /// </summary>
        public List<TrophicConnection> Connections { get; set; } = new List<TrophicConnection>();

        /// <summary>
        /// Total primary productivity (energy input) in arbitrary units.
        /// </summary>
        public float TotalProductivity { get; set; }

        /// <summary>
        /// Complexity score (0-1) based on connection density.
        /// </summary>
        public float ComplexityScore { get; set; }

        /// <summary>
        /// Stability score (0-1) based on redundancy and balance.
        /// </summary>
        public float StabilityScore { get; set; }

        /// <summary>
        /// Generator version used (provenance).
        /// </summary>
        public string GeneratorVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Gets all slots at a specific trophic level.
        /// </summary>
        public List<TrophicSlot> GetSlotsByLevel(TrophicLevel level)
        {
            return Slots.Where(s => s.Level == level).ToList();
        }

        /// <summary>
        /// Gets a slot by its ID, or null if not found.
        /// </summary>
        public TrophicSlot? GetSlotById(Guid id)
        {
            return Slots.FirstOrDefault(s => s.Id == id);
        }

        /// <summary>
        /// Gets all connections where the given slot is prey.
        /// </summary>
        public List<TrophicConnection> GetPredatorConnections(Guid slotId)
        {
            return Connections.Where(c => c.SourceSlotId == slotId).ToList();
        }

        /// <summary>
        /// Gets all connections where the given slot is predator.
        /// </summary>
        public List<TrophicConnection> GetPreyConnections(Guid slotId)
        {
            return Connections.Where(c => c.TargetSlotId == slotId).ToList();
        }

        /// <summary>
        /// Calculates total biomass across all slots.
        /// </summary>
        public float GetTotalBiomass()
        {
            return Slots.Sum(s => s.BiomassCapacity);
        }

        /// <summary>
        /// Calculates the connection density (connections per slot).
        /// </summary>
        public float GetConnectionDensity()
        {
            if (Slots.Count == 0) return 0f;
            return (float)Connections.Count / Slots.Count;
        }

        /// <summary>
        /// Gets the maximum food chain length in this web.
        /// </summary>
        public int GetMaxChainLength()
        {
            var producers = GetSlotsByLevel(TrophicLevel.Producer);
            if (producers.Count == 0) return 0;

            int maxLength = 0;
            foreach (var producer in producers)
            {
                int length = GetChainLengthFrom(producer.Id, new HashSet<Guid>());
                maxLength = System.Math.Max(maxLength, length);
            }
            return maxLength;
        }

        private int GetChainLengthFrom(Guid slotId, HashSet<Guid> visited)
        {
            if (visited.Contains(slotId)) return 0;
            visited.Add(slotId);

            var predatorConnections = GetPredatorConnections(slotId);
            if (predatorConnections.Count == 0) return 1;

            int maxSubChain = 0;
            foreach (var conn in predatorConnections)
            {
                int subLength = GetChainLengthFrom(conn.TargetSlotId, new HashSet<Guid>(visited));
                maxSubChain = System.Math.Max(maxSubChain, subLength);
            }
            return 1 + maxSubChain;
        }
    }
}
