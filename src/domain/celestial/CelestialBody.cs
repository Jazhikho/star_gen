using Godot;
using StarGen.Domain.Celestial.Components;

namespace StarGen.Domain.Celestial;

/// <summary>
/// Core data model for a celestial body.
/// </summary>
public partial class CelestialBody : RefCounted
{
    /// <summary>
    /// Unique identifier for this body.
    /// </summary>
    public string Id;

    /// <summary>
    /// Display name of the body.
    /// </summary>
    public string Name;

    /// <summary>
    /// Type of celestial body.
    /// </summary>
    public CelestialType.Type Type;

    /// <summary>
    /// Physical properties for this body.
    /// </summary>
    public PhysicalProps Physical;

    /// <summary>
    /// Orbital properties.
    /// </summary>
    public OrbitalProps? Orbital;

    /// <summary>
    /// Stellar properties.
    /// </summary>
    public StellarProps? Stellar;

    /// <summary>
    /// Surface properties.
    /// </summary>
    public SurfaceProps? Surface;

    /// <summary>
    /// Atmospheric properties.
    /// </summary>
    public AtmosphereProps? Atmosphere;

    /// <summary>
    /// Ring-system properties.
    /// </summary>
    public RingSystemProps? RingSystem;

    /// <summary>
    /// Population data. The dedicated C# population model has not been ported yet.
    /// </summary>
    public RefCounted? PopulationData;

    /// <summary>
    /// Generation provenance information.
    /// </summary>
    public Provenance? Provenance;

    /// <summary>
    /// Creates a new celestial-body instance.
    /// </summary>
    public CelestialBody(
        string id = "",
        string name = "",
        CelestialType.Type type = CelestialType.Type.Planet,
        PhysicalProps? physical = null,
        Provenance? provenance = null)
    {
        Id = id;
        Name = name;
        Type = type;
        Physical = physical ?? new PhysicalProps();
        Provenance = provenance;
        Orbital = null;
        Stellar = null;
        Surface = null;
        Atmosphere = null;
        RingSystem = null;
        PopulationData = null;
    }

    /// <summary>
    /// Returns whether this body has orbital data.
    /// </summary>
    public bool HasOrbital() => Orbital != null;

    /// <summary>
    /// Returns whether this body has stellar data.
    /// </summary>
    public bool HasStellar() => Stellar != null;

    /// <summary>
    /// Returns whether this body has surface data.
    /// </summary>
    public bool HasSurface() => Surface != null;

    /// <summary>
    /// Returns whether this body has atmospheric data.
    /// </summary>
    public bool HasAtmosphere() => Atmosphere != null;

    /// <summary>
    /// Returns whether this body has ring data.
    /// </summary>
    public bool HasRingSystem() => RingSystem != null;

    /// <summary>
    /// Returns whether this body has population data.
    /// </summary>
    public bool HasPopulationData() => PopulationData != null;

    /// <summary>
    /// Returns the body type as a string.
    /// </summary>
    public string GetTypeString() => CelestialType.TypeToString(Type);
}
