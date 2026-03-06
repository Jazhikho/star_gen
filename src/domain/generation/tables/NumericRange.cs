using Godot.Collections;

namespace StarGen.Domain.Generation.Tables;

/// <summary>
/// Numeric min/max range with implicit conversions for legacy tuple/dictionary call sites.
/// </summary>
public readonly struct NumericRange
{
    public double Min { get; }
    public double Max { get; }

    public NumericRange(double min, double max)
    {
        Min = min;
        Max = max;
    }

    public static implicit operator (double Min, double Max)(NumericRange range)
    {
        return (range.Min, range.Max);
    }

    public static implicit operator NumericRange((double Min, double Max) range)
    {
        return new NumericRange(range.Min, range.Max);
    }

    public static implicit operator Dictionary(NumericRange range)
    {
        return new Dictionary
        {
            ["min"] = range.Min,
            ["max"] = range.Max,
        };
    }

    public static implicit operator NumericRange(Dictionary range)
    {
        double min = 0.0;
        if (range.ContainsKey("min"))
        {
            min = (double)range["min"];
        }

        double max = 0.0;
        if (range.ContainsKey("max"))
        {
            max = (double)range["max"];
        }

        return new NumericRange(min, max);
    }
}
