using System.Collections.Generic;
using System.Globalization;
using Godot;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Represents a 100pc cube sector with lazily generated star data.
/// </summary>
public partial class Sector : RefCounted
{
    /// <summary>
    /// Subsectors per edge.
    /// </summary>
    public const int SubsectorsPerEdge = 10;

    private readonly global::System.WeakReference<Galaxy> _galaxy;
    private readonly Dictionary<string, List<GalaxyStar>> _starsBySubsector = new();
    private readonly List<GalaxyStar> _allStars = new();
    private bool _isGenerated;
    private Godot.Collections.Array<GalaxyStar>? _cachedGodotArray;

    /// <summary>
    /// Quadrant grid coordinates.
    /// </summary>
    public Vector3I QuadrantCoords;

    /// <summary>
    /// Sector-local coordinates inside the quadrant.
    /// </summary>
    public Vector3I SectorLocalCoords;

    /// <summary>
    /// World-space origin of the sector.
    /// </summary>
    public Vector3 WorldOrigin = Vector3.Zero;

    /// <summary>
    /// Deterministic seed for the sector.
    /// </summary>
    public long SectorSeed;

    /// <summary>
    /// Creates a new lazily generated sector.
    /// </summary>
    public Sector(Galaxy galaxy, Vector3I quadrantCoords, Vector3I sectorLocalCoords)
    {
        _galaxy = new global::System.WeakReference<Galaxy>(galaxy);
        QuadrantCoords = quadrantCoords;
        SectorLocalCoords = sectorLocalCoords;
        WorldOrigin = GalaxyCoordinates.SectorWorldOrigin(quadrantCoords, sectorLocalCoords);
        SectorSeed = SeedDeriver.DeriveSectorSeedFull(galaxy.GalaxySeed, quadrantCoords, sectorLocalCoords);
    }

    /// <summary>
    /// Returns all stars in the sector, generating them if needed.
    /// The Godot array wrapper is cached after first generation and reused on subsequent calls.
    /// </summary>
    public Godot.Collections.Array<GalaxyStar> GetStars()
    {
        EnsureGenerated();
        if (_cachedGodotArray == null)
        {
            _cachedGodotArray = ToGodotArray(_allStars);
        }

        return _cachedGodotArray;
    }

    /// <summary>
    /// Returns stars in a specific subsector.
    /// </summary>
    public Godot.Collections.Array<GalaxyStar> GetStarsInSubsector(Vector3I subsectorLocalCoords)
    {
        EnsureGenerated();
        string key = GetSubsectorKey(subsectorLocalCoords);
        if (_starsBySubsector.ContainsKey(key))
        {
            return ToGodotArray(_starsBySubsector[key]);
        }

        return new Godot.Collections.Array<GalaxyStar>();
    }

    /// <summary>
    /// Returns the total star count.
    /// </summary>
    public int GetStarCount()
    {
        EnsureGenerated();
        return _allStars.Count;
    }

    /// <summary>
    /// Returns whether the sector has generated star data.
    /// </summary>
    public bool IsGenerated()
    {
        return _isGenerated;
    }

    /// <summary>
    /// Clears and regenerates the sector star cache.
    /// </summary>
    public void Regenerate()
    {
        _isGenerated = false;
        _cachedGodotArray = null;
        _starsBySubsector.Clear();
        _allStars.Clear();
        EnsureGenerated();
    }

    /// <summary>
    /// Ensures star data has been generated.
    /// </summary>
    private void EnsureGenerated()
    {
        if (_isGenerated)
        {
            return;
        }

        if (!_galaxy.TryGetTarget(out Galaxy? galaxy) || galaxy == null)
        {
            _isGenerated = true;
            return;
        }

        GenerateAllSubsectors(galaxy);
        _isGenerated = true;
    }

    /// <summary>
    /// Generates all subsectors and indexes their stars.
    /// </summary>
    private void GenerateAllSubsectors(Galaxy galaxy)
    {
        for (int x = 0; x < SubsectorsPerEdge; x += 1)
        {
            for (int y = 0; y < SubsectorsPerEdge; y += 1)
            {
                for (int z = 0; z < SubsectorsPerEdge; z += 1)
                {
                    _starsBySubsector[GetSubsectorKey(new Vector3I(x, y, z))] = new List<GalaxyStar>();
                }
            }
        }

        SectorStarData starData = SubSectorGenerator.GenerateSectorStars(
            galaxy.GalaxySeed,
            QuadrantCoords,
            SectorLocalCoords,
            galaxy.DensityModel,
            galaxy.ReferenceDensity);

        for (int index = 0; index < starData.GetCount(); index += 1)
        {
            Vector3 position = starData.Positions[index];
            int seedValue = (int)starData.StarSeeds[index];
            GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(position, seedValue, galaxy.Spec);
            star.SectorQuadrant = QuadrantCoords;
            star.SectorLocal = SectorLocalCoords;

            Vector3 localPosition = position - WorldOrigin;
            Vector3I subsectorCoords = new(
                Mathf.Clamp((int)(localPosition.X / (float)GalaxyCoordinates.SubsectorSizePc), 0, 9),
                Mathf.Clamp((int)(localPosition.Y / (float)GalaxyCoordinates.SubsectorSizePc), 0, 9),
                Mathf.Clamp((int)(localPosition.Z / (float)GalaxyCoordinates.SubsectorSizePc), 0, 9));
            star.SubsectorCoords = subsectorCoords;

            string key = GetSubsectorKey(subsectorCoords);
            _starsBySubsector[key].Add(star);
            _allStars.Add(star);
        }
    }

    /// <summary>
    /// Returns the cache key for a subsector.
    /// </summary>
    private static string GetSubsectorKey(Vector3I coords)
    {
        return string.Concat(
            coords.X.ToString(CultureInfo.InvariantCulture),
            ",",
            coords.Y.ToString(CultureInfo.InvariantCulture),
            ",",
            coords.Z.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Converts a list to a Godot array.
    /// </summary>
    private static Godot.Collections.Array<GalaxyStar> ToGodotArray(List<GalaxyStar> stars)
    {
        Godot.Collections.Array<GalaxyStar> result = new();
        foreach (GalaxyStar star in stars)
        {
            result.Add(star);
        }

        return result;
    }
}
