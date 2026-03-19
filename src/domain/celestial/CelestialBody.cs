using Godot;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Concepts;
using StarGen.Domain.Population;

namespace StarGen.Domain.Celestial;

/// <summary>
/// Core data model for a celestial body.
/// </summary>
public partial class CelestialBody : RefCounted
{
    /// <summary>
    /// Unique identifier for this body.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of the body.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Type of celestial body.
    /// </summary>
    public CelestialType.Type Type { get; set; }

    /// <summary>
    /// Physical properties for this body.
    /// </summary>
    public PhysicalProps Physical { get; set; }

    /// <summary>
    /// Orbital properties.
    /// </summary>
    public OrbitalProps? Orbital { get; set; }

    /// <summary>
    /// Stellar properties.
    /// </summary>
    public StellarProps? Stellar { get; set; }

    /// <summary>
    /// Surface properties.
    /// </summary>
    public SurfaceProps? Surface { get; set; }

    /// <summary>
    /// Atmospheric properties.
    /// </summary>
    public AtmosphereProps? Atmosphere { get; set; }

    /// <summary>
    /// Ring-system properties.
    /// </summary>
    public RingSystemProps? RingSystem { get; set; }

    /// <summary>
    /// Population data for the body when generated or loaded.
    /// </summary>
    public PlanetPopulationData? PopulationData { get; set; }

    /// <summary>
    /// Persisted concept results associated with this body.
    /// </summary>
    public ConceptResultStore ConceptResults { get; set; }

    /// <summary>
    /// Generation provenance information.
    /// </summary>
    public Provenance? Provenance { get; set; }

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
        ConceptResults = new ConceptResultStore();
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
    /// Returns whether this body has persisted concept results.
    /// </summary>
    public bool HasConceptResults() => !ConceptResults.IsEmpty();

    /// <summary>
    /// Returns the body type as a string.
    /// </summary>
    public string GetTypeString() => CelestialType.TypeToString(Type);
}
