using Godot;
using Godot.Collections;

namespace StarGen.Domain.Systems;

/// <summary>
/// Session cache for generated solar systems.
/// </summary>
public partial class SystemCache : RefCounted
{
    /// <summary>
    /// Cached systems keyed by deterministic star seed.
    /// </summary>
    private readonly Dictionary<string, SolarSystem> _cache = new();

    /// <summary>
    /// Returns a cached system by star seed.
    /// </summary>
    public SolarSystem? GetSystem(int starSeed)
    {
        string key = starSeed.ToString();
        return _cache.ContainsKey(key) ? _cache[key] : null;
    }

    /// <summary>
    /// Stores a system in the cache.
    /// </summary>
    public void PutSystem(int starSeed, SolarSystem system)
    {
        string key = starSeed.ToString();
        _cache[key] = system;
    }

    /// <summary>
    /// Returns whether a system is already cached.
    /// </summary>
    public bool HasSystem(int starSeed)
    {
        string key = starSeed.ToString();
        return _cache.ContainsKey(key);
    }

    /// <summary>
    /// Returns the number of cached systems.
    /// </summary>
    public int GetCacheSize()
    {
        return _cache.Count;
    }

    /// <summary>
    /// Removes a single cached system.
    /// </summary>
    public void Evict(int starSeed)
    {
        string key = starSeed.ToString();
        if (_cache.ContainsKey(key))
        {
            _cache.Remove(key);
        }
    }

    /// <summary>
    /// Clears all cached systems.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }
}
