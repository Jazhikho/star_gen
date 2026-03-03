using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Recommendation produced by station-placement rules.
/// </summary>
public partial class StationPlacementRecommendation : RefCounted
{
    /// <summary>
    /// Primary placement context.
    /// </summary>
    public StationPlacementContext.Context Context = StationPlacementContext.Context.Other;

    /// <summary>
    /// Whether the system should have stations.
    /// </summary>
    public bool ShouldHaveStations;

    /// <summary>
    /// Recommended utility-station count.
    /// </summary>
    public int UtilityStationCount;

    /// <summary>
    /// Recommended outpost count.
    /// </summary>
    public int OutpostCount;

    /// <summary>
    /// Recommended large-station count.
    /// </summary>
    public int LargeStationCount;

    /// <summary>
    /// Recommended station purposes.
    /// </summary>
    public Array<StationPurpose.Purpose> RecommendedPurposes = new();

    /// <summary>
    /// Candidate body ids for orbital stations.
    /// </summary>
    public Array<string> OrbitalCandidates = new();

    /// <summary>
    /// Whether deep-space stations are appropriate.
    /// </summary>
    public bool AllowDeepSpace;

    /// <summary>
    /// Whether asteroid-belt stations are appropriate.
    /// </summary>
    public bool AllowBeltStations;

    /// <summary>
    /// Reasoning strings for debugging or display.
    /// </summary>
    public Array<string> Reasoning = new();

    /// <summary>
    /// Converts the recommendation to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<int> purposes = new();
        foreach (StationPurpose.Purpose purpose in RecommendedPurposes)
        {
            purposes.Add((int)purpose);
        }

        Array<string> orbitalCandidates = new();
        foreach (string bodyId in OrbitalCandidates)
        {
            orbitalCandidates.Add(bodyId);
        }

        Array<string> reasoning = new();
        foreach (string reason in Reasoning)
        {
            reasoning.Add(reason);
        }

        return new Dictionary
        {
            ["context"] = (int)Context,
            ["should_have_stations"] = ShouldHaveStations,
            ["utility_station_count"] = UtilityStationCount,
            ["outpost_count"] = OutpostCount,
            ["large_station_count"] = LargeStationCount,
            ["recommended_purposes"] = purposes,
            ["orbital_candidates"] = orbitalCandidates,
            ["allow_deep_space"] = AllowDeepSpace,
            ["allow_belt_stations"] = AllowBeltStations,
            ["reasoning"] = reasoning,
        };
    }
}
