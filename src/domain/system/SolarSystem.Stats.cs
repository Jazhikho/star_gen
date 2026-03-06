using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Population;

namespace StarGen.Domain.Systems;

/// <summary>
/// Body counts, population aggregates, and summary helpers for SolarSystem.
/// </summary>
public partial class SolarSystem
{
    private enum PopulationMetric
    {
        Total,
        Native,
        Colony,
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
    /// Returns a short summary string for diagnostics.
    /// </summary>
    public string GetSummary()
    {
        string label;
        if (string.IsNullOrEmpty(Name))
        {
            label = Id;
        }
        else
        {
            label = Name;
        }
        return $"{label}: {GetStarCount()} stars, {GetPlanetCount()} planets, {GetMoonCount()} moons, {GetAsteroidCount()} asteroids, {AsteroidBelts.Count} belts";
    }

    /// <summary>
    /// Returns a population aggregate across all bodies.
    /// </summary>
    private int GetPopulationMetric(PopulationMetric metric)
    {
        int total = 0;
        foreach (CelestialBody body in Bodies.Values)
        {
            if (!body.HasPopulationData() || body.PopulationData == null)
            {
                continue;
            }

            PlanetPopulationData populationData = body.PopulationData;
            if (metric == PopulationMetric.Total)
            {
                total += populationData.GetTotalPopulation();
            }
            else if (metric == PopulationMetric.Native)
            {
                total += populationData.GetNativePopulation();
            }
            else if (metric == PopulationMetric.Colony)
            {
                total += populationData.GetColonyPopulation();
            }
        }

        return total;
    }
}
