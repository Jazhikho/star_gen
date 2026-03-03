using Godot;
using Godot.Collections;

namespace StarGen.Domain.Celestial.Components;

/// <summary>
/// Ring system properties containing multiple bands.
/// </summary>
public partial class RingSystemProps : RefCounted
{
    /// <summary>
    /// Array of ring bands from inner to outer.
    /// </summary>
    public Array<RingBand> Bands;

    /// <summary>
    /// Total mass of the ring system in kilograms.
    /// </summary>
    public double TotalMassKg;

    /// <summary>
    /// Inclination of the ring plane relative to the equator.
    /// </summary>
    public double InclinationDeg;

    /// <summary>
    /// Creates a new ring-system component.
    /// </summary>
    public RingSystemProps(
        Array<RingBand>? bands = null,
        double totalMassKg = 0.0,
        double inclinationDeg = 0.0)
    {
        Bands = CloneBands(bands);
        TotalMassKg = totalMassKg;
        InclinationDeg = inclinationDeg;
    }

    /// <summary>
    /// Returns the innermost radius of the ring system.
    /// </summary>
    public double GetInnerRadiusM()
    {
        if (Bands.Count == 0)
        {
            return 0.0;
        }

        double minRadius = Bands[0].InnerRadiusM;
        foreach (RingBand band in Bands)
        {
            if (band.InnerRadiusM < minRadius)
            {
                minRadius = band.InnerRadiusM;
            }
        }

        return minRadius;
    }

    /// <summary>
    /// Returns the outermost radius of the ring system.
    /// </summary>
    public double GetOuterRadiusM()
    {
        if (Bands.Count == 0)
        {
            return 0.0;
        }

        double maxRadius = Bands[0].OuterRadiusM;
        foreach (RingBand band in Bands)
        {
            if (band.OuterRadiusM > maxRadius)
            {
                maxRadius = band.OuterRadiusM;
            }
        }

        return maxRadius;
    }

    /// <summary>
    /// Returns the total width of the ring system.
    /// </summary>
    public double GetTotalWidthM() => GetOuterRadiusM() - GetInnerRadiusM();

    /// <summary>
    /// Returns the number of bands in the system.
    /// </summary>
    public int GetBandCount() => Bands.Count;

    /// <summary>
    /// Adds a band to the system.
    /// </summary>
    public void AddBand(RingBand band) => Bands.Add(band);

    /// <summary>
    /// Gets a band by index.
    /// </summary>
    public RingBand? GetBand(int index)
    {
        if (index < 0 || index >= Bands.Count)
        {
            return null;
        }

        return Bands[index];
    }

    /// <summary>
    /// Converts this component to a dictionary for serialization.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array bandsArray = new();
        foreach (RingBand band in Bands)
        {
            bandsArray.Add(band.ToDictionary());
        }

        return new Dictionary
        {
            ["bands"] = bandsArray,
            ["total_mass_kg"] = TotalMassKg,
            ["inclination_deg"] = InclinationDeg,
        };
    }

    /// <summary>
    /// Creates a ring-system component from a dictionary.
    /// </summary>
    public static RingSystemProps FromDictionary(Dictionary data)
    {
        Array<RingBand> parsedBands = new();
        if (data.ContainsKey("bands"))
        {
            foreach (Variant bandData in (Array)data["bands"])
            {
                parsedBands.Add(RingBand.FromDictionary((Dictionary)bandData));
            }
        }

        return new RingSystemProps(
            parsedBands,
            GetDouble(data, "total_mass_kg", 0.0),
            GetDouble(data, "inclination_deg", 0.0));
    }

    private static Array<RingBand> CloneBands(Array<RingBand>? source)
    {
        Array<RingBand> clone = new();
        if (source == null)
        {
            return clone;
        }

        foreach (RingBand band in source)
        {
            clone.Add(band);
        }

        return clone;
    }

    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        return data.ContainsKey(key) ? (double)data[key] : fallback;
    }
}
