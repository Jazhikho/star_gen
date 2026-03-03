using Godot;
using Godot.Collections;

namespace StarGen.Domain.Systems;

/// <summary>
/// Result of orbit-slot generation for a single orbit host.
/// </summary>
public partial class OrbitSlotGenerationResult : RefCounted
{
    /// <summary>
    /// Generated orbit slots.
    /// </summary>
    public Array<OrbitSlot> Slots = new();

    /// <summary>
    /// Identifier of the orbit host these slots belong to.
    /// </summary>
    public string OrbitHostId = string.Empty;

    /// <summary>
    /// Whether slot generation succeeded.
    /// </summary>
    public bool Success;
}
