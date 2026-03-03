using System;

namespace StarGen.Domain.Ecology
{
    /// <summary>
    /// Represents an energy flow connection between two trophic slots.
    /// </summary>
    public class TrophicConnection
    {
        /// <summary>
        /// The slot being consumed (prey/resource).
        /// </summary>
        public Guid SourceSlotId { get; set; }

        /// <summary>
        /// The slot doing the consuming (predator/consumer).
        /// </summary>
        public Guid TargetSlotId { get; set; }

        /// <summary>
        /// Relative strength of this feeding relationship (0-1).
        /// Higher = more important food source.
        /// </summary>
        public float Strength { get; set; } = 0.5f;

        /// <summary>
        /// Whether this is an obligate relationship (essential for survival).
        /// </summary>
        public bool IsObligate { get; set; } = false;
    }
}
