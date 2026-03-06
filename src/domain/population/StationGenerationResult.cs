using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Result of station generation for a system.
/// </summary>
public partial class StationGenerationResult : RefCounted
{
    /// <summary>
    /// Generated outposts.
    /// </summary>
    public Array<Outpost> Outposts = new();

    /// <summary>
    /// Generated larger stations.
    /// </summary>
    public Array<SpaceStation> Stations = new();

    /// <summary>
    /// Recommendation used during generation.
    /// </summary>
    public StationPlacementRecommendation? Recommendation;

    /// <summary>
    /// Initial generation seed.
    /// </summary>
    public int GenerationSeed;

    /// <summary>
    /// Warnings encountered during generation.
    /// </summary>
    public Array<string> Warnings = new();

    /// <summary>
    /// Returns the total generated count.
    /// </summary>
    public int GetTotalCount()
    {
        return Outposts.Count + Stations.Count;
    }

    /// <summary>
    /// Returns a copy of the station array.
    /// </summary>
    public Array<SpaceStation> GetAllStations()
    {
        Array<SpaceStation> copy = new();
        foreach (SpaceStation station in Stations)
        {
            copy.Add(station);
        }

        return copy;
    }

    /// <summary>
    /// Returns stations orbiting a specific body.
    /// </summary>
    public Array<SpaceStation> GetStationsForBody(string bodyId)
    {
        Array<SpaceStation> matches = new();
        foreach (SpaceStation station in Stations)
        {
            if (station.OrbitingBodyId == bodyId)
            {
                matches.Add(station);
            }
        }

        return matches;
    }

    /// <summary>
    /// Returns outposts orbiting a specific body.
    /// </summary>
    public Array<Outpost> GetOutpostsForBody(string bodyId)
    {
        Array<Outpost> matches = new();
        foreach (Outpost outpost in Outposts)
        {
            if (outpost.OrbitingBodyId == bodyId)
            {
                matches.Add(outpost);
            }
        }

        return matches;
    }

    /// <summary>
    /// Converts the result to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<Dictionary> outposts = new();
        foreach (Outpost outpost in Outposts)
        {
            outposts.Add(outpost.ToDictionary());
        }

        Array<Dictionary> stations = new();
        foreach (SpaceStation station in Stations)
        {
            stations.Add(station.ToDictionary());
        }

        Array<string> warnings = new();
        foreach (string warning in Warnings)
        {
            warnings.Add(warning);
        }

        Dictionary recommendationData;
        if (Recommendation != null)
        {
            recommendationData = Recommendation.ToDictionary();
        }
        else
        {
            recommendationData = new Dictionary();
        }

        return new Dictionary
        {
            ["outposts"] = outposts,
            ["stations"] = stations,
            ["generation_seed"] = GenerationSeed,
            ["warnings"] = warnings,
            ["recommendation"] = recommendationData,
        };
    }

    /// <summary>
    /// Legacy alias for dictionary conversion.
    /// </summary>
    public Dictionary ToDict() => ToDictionary();
}
