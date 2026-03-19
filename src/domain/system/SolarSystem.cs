using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Concepts;

namespace StarGen.Domain.Systems;

/// <summary>
/// Main container for a complete solar system.
/// Stats in SolarSystem.Stats.cs. Serialization in SolarSystem.Serialization.cs.
/// </summary>
public partial class SolarSystem : Godot.RefCounted
{
    /// <summary>
    /// Unique system identifier.
    /// </summary>
    public string Id = string.Empty;

    /// <summary>
    /// Display name for the system.
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    /// Stellar hierarchy for the system.
    /// </summary>
    public SystemHierarchy Hierarchy = new();

    /// <summary>
    /// All celestial bodies keyed by identifier.
    /// </summary>
    public Dictionary<string, CelestialBody> Bodies = new();

    /// <summary>
    /// Identifiers of star bodies.
    /// </summary>
    public Array<string> StarIds = new();

    /// <summary>
    /// Identifiers of planet bodies.
    /// </summary>
    public Array<string> PlanetIds = new();

    /// <summary>
    /// Identifiers of moon bodies.
    /// </summary>
    public Array<string> MoonIds = new();

    /// <summary>
    /// Identifiers of asteroid bodies.
    /// </summary>
    public Array<string> AsteroidIds = new();

    /// <summary>
    /// Asteroid belts in the system.
    /// </summary>
    public Array<AsteroidBelt> AsteroidBelts = new();

    /// <summary>
    /// Computed orbit hosts in the system.
    /// </summary>
    public Array<OrbitHost> OrbitHosts = new();

    /// <summary>
    /// Generation provenance.
    /// </summary>
    public Provenance? Provenance;

    /// <summary>
    /// Persisted concept results for system-level aggregate views.
    /// </summary>
    public ConceptResultStore ConceptResults = new();

    /// <summary>
    /// Creates a new solar-system container.
    /// </summary>
    public SolarSystem(string id = "", string name = "")
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Adds a body and updates the type-specific indexes.
    /// </summary>
    public void AddBody(CelestialBody? body)
    {
        if (body == null || string.IsNullOrEmpty(body.Id))
        {
            return;
        }

        Bodies[body.Id] = body;
        switch (body.Type)
        {
            case CelestialType.Type.Star:
                AppendUnique(StarIds, body.Id);
                break;
            case CelestialType.Type.Planet:
                AppendUnique(PlanetIds, body.Id);
                break;
            case CelestialType.Type.Moon:
                AppendUnique(MoonIds, body.Id);
                break;
            case CelestialType.Type.Asteroid:
                AppendUnique(AsteroidIds, body.Id);
                break;
        }
    }

    /// <summary>
    /// Returns a body by identifier.
    /// </summary>
    public CelestialBody? GetBody(string bodyId)
    {
        if (Bodies.ContainsKey(bodyId))
        {
            return Bodies[bodyId];
        }

        return null;
    }

    /// <summary>
    /// Returns all star bodies.
    /// </summary>
    public Array<CelestialBody> GetStars()
    {
        return GetBodiesByIds(StarIds);
    }

    /// <summary>
    /// Returns all planet bodies.
    /// </summary>
    public Array<CelestialBody> GetPlanets()
    {
        return GetBodiesByIds(PlanetIds);
    }

    /// <summary>
    /// Returns all moon bodies.
    /// </summary>
    public Array<CelestialBody> GetMoons()
    {
        return GetBodiesByIds(MoonIds);
    }

    /// <summary>
    /// Returns all moons orbiting a given planet.
    /// </summary>
    public Array<CelestialBody> GetMoonsOfPlanet(string planetId)
    {
        Array<CelestialBody> result = new();
        foreach (string moonId in MoonIds)
        {
            CelestialBody? moon = GetBody(moonId);
            if (moon == null || !moon.HasOrbital())
            {
                continue;
            }

            if (moon.Orbital!.ParentId == planetId)
            {
                result.Add(moon);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns all asteroid bodies.
    /// </summary>
    public Array<CelestialBody> GetAsteroids()
    {
        return GetBodiesByIds(AsteroidIds);
    }

    /// <summary>
    /// Adds an asteroid belt.
    /// </summary>
    public void AddAsteroidBelt(AsteroidBelt? belt)
    {
        if (belt != null)
        {
            AsteroidBelts.Add(belt);
        }
    }

    /// <summary>
    /// Adds an orbit host.
    /// </summary>
    public void AddOrbitHost(OrbitHost? host)
    {
        if (host != null)
        {
            OrbitHosts.Add(host);
        }
    }

    /// <summary>
    /// Returns an orbit host by hierarchy-node identifier.
    /// </summary>
    public OrbitHost? GetOrbitHost(string nodeId)
    {
        foreach (OrbitHost host in OrbitHosts)
        {
            if (host.NodeId == nodeId)
            {
                return host;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns whether the system contains at least one valid star.
    /// </summary>
    public bool IsValid()
    {
        return Hierarchy.IsValid() && StarIds.Count > 0;
    }

    /// <summary>
    /// Returns whether this system has persisted concept results.
    /// </summary>
    public bool HasConceptResults()
    {
        return !ConceptResults.IsEmpty();
    }

    /// <summary>
    /// Appends an item only when it is not already present.
    /// </summary>
    private static void AppendUnique(Array<string> target, string value)
    {
        if (!target.Contains(value))
        {
            target.Add(value);
        }
    }

    /// <summary>
    /// Returns bodies resolved from a typed id list.
    /// </summary>
    private Array<CelestialBody> GetBodiesByIds(Array<string> ids)
    {
        Array<CelestialBody> result = new();
        foreach (string bodyId in ids)
        {
            CelestialBody? body = GetBody(bodyId);
            if (body != null)
            {
                result.Add(body);
            }
        }

        return result;
    }
}
