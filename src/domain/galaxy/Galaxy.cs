using System.Collections.Generic;
using Godot;
using StarGen.Domain.Systems;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Top-level pure-data container for a procedurally generated galaxy.
/// </summary>
public partial class Galaxy : RefCounted
{
    private readonly Dictionary<string, Sector> _sectors = new();
    private readonly Dictionary<int, SolarSystem> _systemsCache = new();

    /// <summary>
    /// Galaxy master seed.
    /// </summary>
    public int GalaxySeed { get; set; }

    /// <summary>
    /// Derived galaxy specification.
    /// </summary>
    public GalaxySpec Spec { get; set; }

    /// <summary>
    /// Source galaxy configuration.
    /// </summary>
    public GalaxyConfig Config { get; set; }

    /// <summary>
    /// Density model matching the galaxy type.
    /// </summary>
    public DensityModelInterface DensityModel { get; set; }

    /// <summary>
    /// Reference density near the solar-neighborhood radius.
    /// </summary>
    public float ReferenceDensity { get; set; }

    /// <summary>
    /// Creates a new galaxy from configuration and seed.
    /// </summary>
    public Galaxy(GalaxyConfig? config, int seed)
    {
        GalaxySeed = seed;
        Config = config ?? GalaxyConfig.CreateDefault();
        Spec = GalaxySpec.CreateFromConfig(Config, GalaxySeed);
        DensityModel = DensityModelInterface.CreateForSpec(Spec);
        ReferenceDensity = ComputeReferenceDensity();
    }

    /// <summary>
    /// Creates a default Milky-Way-like galaxy.
    /// </summary>
    public static Galaxy CreateDefault(int seed)
    {
        return new Galaxy(GalaxyConfig.CreateMilkyWay(), seed);
    }

    /// <summary>
    /// Returns the sector at the supplied coordinates, creating it if needed.
    /// </summary>
    public Sector GetSector(Vector3I quadrantCoords, Vector3I sectorLocalCoords)
    {
        string key = GetSectorKey(quadrantCoords, sectorLocalCoords);
        if (_sectors.ContainsKey(key))
        {
            return _sectors[key];
        }

        Sector sector = new(this, quadrantCoords, sectorLocalCoords);
        _sectors[key] = sector;
        return sector;
    }

    /// <summary>
    /// Returns the sector containing the supplied world-space position.
    /// </summary>
    public Sector GetSectorAtPosition(Vector3 position)
    {
        HierarchyCoords hierarchy = GalaxyCoordinates.ParsecToHierarchy(position);
        return GetSector(hierarchy.QuadrantCoords, hierarchy.SectorLocalCoords);
    }

    /// <summary>
    /// Returns all stars in a sector.
    /// </summary>
    public Godot.Collections.Array<GalaxyStar> GetStarsInSector(Vector3I quadrantCoords, Vector3I sectorLocalCoords)
    {
        return GetSector(quadrantCoords, sectorLocalCoords).GetStars();
    }

    /// <summary>
    /// Returns all stars in a subsector.
    /// </summary>
    public Godot.Collections.Array<GalaxyStar> GetStarsInSubsector(
        Vector3I quadrantCoords,
        Vector3I sectorLocalCoords,
        Vector3I subsectorLocalCoords)
    {
        return GetSector(quadrantCoords, sectorLocalCoords).GetStarsInSubsector(subsectorLocalCoords);
    }

    /// <summary>
    /// Returns all stars within a radius of a world-space position.
    /// </summary>
    public Godot.Collections.Array<GalaxyStar> GetStarsInRadius(Vector3 center, float radiusPc)
    {
        Godot.Collections.Array<GalaxyStar> result = new();
        Vector3 minPosition = center - (Vector3.One * radiusPc);
        Vector3 maxPosition = center + (Vector3.One * radiusPc);
        HierarchyCoords minHierarchy = GalaxyCoordinates.ParsecToHierarchy(minPosition);
        HierarchyCoords maxHierarchy = GalaxyCoordinates.ParsecToHierarchy(maxPosition);

        for (int qx = minHierarchy.QuadrantCoords.X; qx <= maxHierarchy.QuadrantCoords.X; qx += 1)
        {
            for (int qy = minHierarchy.QuadrantCoords.Y; qy <= maxHierarchy.QuadrantCoords.Y; qy += 1)
            {
                for (int qz = minHierarchy.QuadrantCoords.Z; qz <= maxHierarchy.QuadrantCoords.Z; qz += 1)
                {
                    Vector3I quadrant = new(qx, qy, qz);
                    Vector3I sectorMin;
                    if (quadrant == minHierarchy.QuadrantCoords)
                    {
                        sectorMin = minHierarchy.SectorLocalCoords;
                    }
                    else
                    {
                        sectorMin = Vector3I.Zero;
                    }

                    Vector3I sectorMax;
                    if (quadrant == maxHierarchy.QuadrantCoords)
                    {
                        sectorMax = maxHierarchy.SectorLocalCoords;
                    }
                    else
                    {
                        sectorMax = new Vector3I(9, 9, 9);
                    }

                    for (int sx = sectorMin.X; sx <= sectorMax.X; sx += 1)
                    {
                        for (int sy = sectorMin.Y; sy <= sectorMax.Y; sy += 1)
                        {
                            for (int sz = sectorMin.Z; sz <= sectorMax.Z; sz += 1)
                            {
                                Godot.Collections.Array<GalaxyStar> stars = GetStarsInSector(quadrant, new Vector3I(sx, sy, sz));
                                foreach (GalaxyStar star in stars)
                                {
                                    if (star.Position.DistanceTo(center) <= radiusPc)
                                    {
                                        result.Add(star);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Caches a generated solar system.
    /// </summary>
    public void CacheSystem(int starSeed, SolarSystem system)
    {
        _systemsCache[starSeed] = system;
    }

    /// <summary>
    /// Returns a cached solar system if present.
    /// </summary>
    public SolarSystem? GetCachedSystem(int starSeed)
    {
        if (_systemsCache.ContainsKey(starSeed))
        {
            return _systemsCache[starSeed];
        }

        return null;
    }

    /// <summary>
    /// Returns whether a system is cached.
    /// </summary>
    public bool HasCachedSystem(int starSeed)
    {
        return _systemsCache.ContainsKey(starSeed);
    }

    /// <summary>
    /// Clears the sector and system caches.
    /// </summary>
    public void ClearCache()
    {
        _sectors.Clear();
        _systemsCache.Clear();
    }

    /// <summary>
    /// Returns the number of cached sectors.
    /// </summary>
    public int GetCachedSectorCount()
    {
        return _sectors.Count;
    }

    /// <summary>
    /// Returns the number of cached systems.
    /// </summary>
    public int GetCachedSystemCount()
    {
        return _systemsCache.Count;
    }

    /// <summary>
    /// Converts the galaxy to a persistence payload.
    /// </summary>
    public Godot.Collections.Dictionary ToDictionary()
    {
        return new Godot.Collections.Dictionary
        {
            ["seed"] = GalaxySeed,
            ["config"] = Config.ToDictionary(),
        };
    }

    /// <summary>
    /// Rebuilds a galaxy from a persistence payload.
    /// </summary>
    public static Galaxy FromDictionary(Godot.Collections.Dictionary data)
    {
        int seed = DomainDictionaryUtils.GetInt(data, "seed", 42);
        GalaxyConfig config;
        if (data.ContainsKey("config") && data["config"].VariantType == Variant.Type.Dictionary)
        {
            GalaxyConfig? parsed = GalaxyConfig.FromDictionary((Godot.Collections.Dictionary)data["config"]);
            if (parsed == null)
            {
                GD.PushError("Galaxy.FromDictionary: GalaxyConfig.FromDictionary returned null — falling back to default galaxy configuration.");
                config = GalaxyConfig.CreateDefault();
            }
            else
            {
                config = parsed;
            }
        }
        else
        {
            GD.PushError("Galaxy.FromDictionary: 'config' key missing or wrong type in payload — falling back to default galaxy configuration.");
            config = GalaxyConfig.CreateDefault();
        }

        return new Galaxy(config, seed);
    }

    /// <summary>
    /// Computes the reference density at the solar-neighborhood radius.
    /// </summary>
    private float ComputeReferenceDensity()
    {
        return DensityModel.GetDensity(new Vector3(8000.0f, 0.0f, 0.0f));
    }

    /// <summary>
    /// Returns the cache key for a sector.
    /// </summary>
    private static string GetSectorKey(Vector3I quadrantCoords, Vector3I sectorLocalCoords)
    {
        return $"{quadrantCoords.X},{quadrantCoords.Y},{quadrantCoords.Z}:{sectorLocalCoords.X},{sectorLocalCoords.Y},{sectorLocalCoords.Z}";
    }

}
