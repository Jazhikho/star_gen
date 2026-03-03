using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Population;

namespace StarGen.Domain.Systems;

/// <summary>
/// Main container for a complete solar system.
/// </summary>
public partial class SolarSystem : RefCounted
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
        return Bodies.ContainsKey(bodyId) ? Bodies[bodyId] : null;
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
    /// Returns the total body count.
    /// </summary>
    public int GetBodyCount()
    {
        return Bodies.Count;
    }

    /// <summary>
    /// Returns the total star count.
    /// </summary>
    public int GetStarCount()
    {
        return StarIds.Count;
    }

    /// <summary>
    /// Returns the total planet count.
    /// </summary>
    public int GetPlanetCount()
    {
        return PlanetIds.Count;
    }

    /// <summary>
    /// Returns the total moon count.
    /// </summary>
    public int GetMoonCount()
    {
        return MoonIds.Count;
    }

    /// <summary>
    /// Returns the total asteroid count.
    /// </summary>
    public int GetAsteroidCount()
    {
        return AsteroidIds.Count;
    }

    /// <summary>
    /// Returns total active population across all bodies.
    /// </summary>
    public int GetTotalPopulation()
    {
        return GetPopulationMetric(PopulationMetric.Total);
    }

    /// <summary>
    /// Returns total extant native population across all bodies.
    /// </summary>
    public int GetNativePopulation()
    {
        return GetPopulationMetric(PopulationMetric.Native);
    }

    /// <summary>
    /// Returns total active colony population across all bodies.
    /// </summary>
    public int GetColonyPopulation()
    {
        return GetPopulationMetric(PopulationMetric.Colony);
    }

    /// <summary>
    /// Returns whether the system contains any active population.
    /// </summary>
    public bool IsInhabited()
    {
        return GetTotalPopulation() > 0;
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
    /// Returns a short summary string for diagnostics.
    /// </summary>
    public string GetSummary()
    {
        string label = string.IsNullOrEmpty(Name) ? Id : Name;
        return $"{label}: {GetStarCount()} stars, {GetPlanetCount()} planets, {GetMoonCount()} moons, {GetAsteroidCount()} asteroids, {AsteroidBelts.Count} belts";
    }

    /// <summary>
    /// Converts the system to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = new()
        {
            ["id"] = Id,
            ["name"] = Name,
            ["hierarchy"] = Hierarchy != null ? Hierarchy.ToDictionary() : new Dictionary(),
            ["star_ids"] = CloneArray(StarIds),
            ["planet_ids"] = CloneArray(PlanetIds),
            ["moon_ids"] = CloneArray(MoonIds),
            ["asteroid_ids"] = CloneArray(AsteroidIds),
        };

        Dictionary bodiesData = new();
        foreach (string bodyId in Bodies.Keys)
        {
            bodiesData[bodyId] = CelestialSerializer.ToDictionary(Bodies[bodyId]);
        }

        data["bodies"] = bodiesData;

        Array<Dictionary> beltsData = new();
        foreach (AsteroidBelt belt in AsteroidBelts)
        {
            beltsData.Add(belt.ToDictionary());
        }

        data["asteroid_belts"] = beltsData;

        Array<Dictionary> hostsData = new();
        foreach (OrbitHost host in OrbitHosts)
        {
            hostsData.Add(host.ToDictionary());
        }

        data["orbit_hosts"] = hostsData;

        if (Provenance != null)
        {
            data["provenance"] = Provenance.ToDictionary();
        }

        return data;
    }

    /// <summary>
    /// Creates a system from a dictionary payload.
    /// </summary>
    public static SolarSystem FromDictionary(Dictionary data)
    {
        SolarSystem system = new(
            GetString(data, "id", string.Empty),
            GetString(data, "name", string.Empty));

        if (data.ContainsKey("hierarchy") && data["hierarchy"].VariantType == Variant.Type.Dictionary)
        {
            system.Hierarchy = SystemHierarchy.FromDictionary((Dictionary)data["hierarchy"]);
        }

        if (data.ContainsKey("bodies") && data["bodies"].VariantType == Variant.Type.Dictionary)
        {
            Dictionary bodiesData = (Dictionary)data["bodies"];
            foreach (Variant bodyKey in bodiesData.Keys)
            {
                if (bodyKey.VariantType != Variant.Type.String)
                {
                    continue;
                }

                Variant bodyValue = bodiesData[bodyKey];
                if (bodyValue.VariantType != Variant.Type.Dictionary)
                {
                    continue;
                }

                CelestialBody? body = CelestialSerializer.FromDictionary((Dictionary)bodyValue);
                if (body != null)
                {
                    system.Bodies[(string)bodyKey] = body;
                }
            }
        }

        PopulateStringArray(data, "star_ids", system.StarIds);
        PopulateStringArray(data, "planet_ids", system.PlanetIds);
        PopulateStringArray(data, "moon_ids", system.MoonIds);
        PopulateStringArray(data, "asteroid_ids", system.AsteroidIds);

        if (data.ContainsKey("asteroid_belts") && data["asteroid_belts"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["asteroid_belts"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    system.AsteroidBelts.Add(AsteroidBelt.FromDictionary((Dictionary)value));
                }
            }
        }

        if (data.ContainsKey("orbit_hosts") && data["orbit_hosts"].VariantType == Variant.Type.Array)
        {
            foreach (Variant value in (Array)data["orbit_hosts"])
            {
                if (value.VariantType == Variant.Type.Dictionary)
                {
                    system.OrbitHosts.Add(OrbitHost.FromDictionary((Dictionary)value));
                }
            }
        }

        if (data.ContainsKey("provenance") && data["provenance"].VariantType == Variant.Type.Dictionary)
        {
            system.Provenance = Provenance.FromDictionary((Dictionary)data["provenance"]);
        }

        return system;
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

    /// <summary>
    /// Returns a population aggregate across all bodies.
    /// </summary>
    private int GetPopulationMetric(PopulationMetric metric)
    {
        int total = 0;
        foreach (CelestialBody body in Bodies.Values)
        {
            if (!body.HasPopulationData())
            {
                continue;
            }

            RefCounted populationData = body.PopulationData!;
            if (populationData is PlanetPopulationData typedPopulationData)
            {
                total += metric switch
                {
                    PopulationMetric.Total => typedPopulationData.GetTotalPopulation(),
                    PopulationMetric.Native => typedPopulationData.GetNativePopulation(),
                    PopulationMetric.Colony => typedPopulationData.GetColonyPopulation(),
                    _ => 0,
                };
                continue;
            }

            string methodName = metric switch
            {
                PopulationMetric.Total => "get_total_population",
                PopulationMetric.Native => "get_native_population",
                PopulationMetric.Colony => "get_colony_population",
                _ => string.Empty,
            };
            if (!string.IsNullOrEmpty(methodName) && populationData.HasMethod(methodName))
            {
                total += VariantToInt(populationData.Call(methodName));
            }
        }

        return total;
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
    /// Clones a string array for payload serialization.
    /// </summary>
    private static Array<string> CloneArray(Array<string> source)
    {
        Array<string> clone = new();
        foreach (string value in source)
        {
            clone.Add(value);
        }

        return clone;
    }

    /// <summary>
    /// Populates a typed string array from a payload key.
    /// </summary>
    private static void PopulateStringArray(Dictionary data, string key, Array<string> target)
    {
        if (!data.ContainsKey(key) || data[key].VariantType != Variant.Type.Array)
        {
            return;
        }

        foreach (Variant value in (Array)data[key])
        {
            if (value.VariantType == Variant.Type.String)
            {
                target.Add((string)value);
            }
        }
    }

    /// <summary>
    /// Reads a string value from a dictionary.
    /// </summary>
    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType == Variant.Type.String ? (string)value : fallback;
    }

    /// <summary>
    /// Converts a variant to an integer when possible.
    /// </summary>
    private static int VariantToInt(Variant value)
    {
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.Float => (int)(double)value,
            Variant.Type.String => int.TryParse((string)value, out int parsed) ? parsed : 0,
            _ => 0,
        };
    }

    /// <summary>
    /// Population aggregate categories.
    /// </summary>
    private enum PopulationMetric
    {
        Total,
        Native,
        Colony,
    }
}
