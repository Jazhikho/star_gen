using Godot.Collections;

namespace StarGen.Domain.Systems;

/// <summary>
/// Result payload for pre-reserving orbit slots for belts.
/// </summary>
public partial class BeltReservationResult : Godot.RefCounted
{
    /// <summary>
    /// Generated belt definitions.
    /// </summary>
    public Array<AsteroidBelt> Belts = new();

    /// <summary>
    /// Orbit-slot identifiers reserved for belts.
    /// </summary>
    public Array<string> ReservedSlotIds = new();
}
