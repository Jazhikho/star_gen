using Godot;
using Godot.Collections;

namespace StarGen.Domain.Generation.Traveller;

/// <summary>
/// Typed set of Traveller trade codes with deterministic ordering.
/// </summary>
public partial class TravellerTradeCodeSet : RefCounted
{
    /// <summary>
    /// Ordered trade codes.
    /// </summary>
    public Array<string> Codes { get; set; } = new();

    /// <summary>
    /// Adds a trade code when not already present and keeps ordering stable.
    /// </summary>
    public void AddCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        string normalized = code.Trim();
        if (Codes.Contains(normalized))
        {
            return;
        }

        Codes.Add(normalized);
        SortCodes();
    }

    /// <summary>
    /// Returns whether the set contains the supplied code.
    /// </summary>
    public bool Contains(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return Codes.Contains(code.Trim());
    }

    /// <summary>
    /// Returns whether any stored code matches the supplied list.
    /// </summary>
    public bool ContainsAny(params string[] codes)
    {
        foreach (string code in codes)
        {
            if (Contains(code))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns a comma-delimited display string.
    /// </summary>
    public string ToDisplayString()
    {
        if (Codes.Count == 0)
        {
            return "None";
        }

        System.Text.StringBuilder builder = new();
        for (int index = 0; index < Codes.Count; index += 1)
        {
            if (index > 0)
            {
                builder.Append(", ");
            }

            builder.Append(Codes[index]);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Converts the set to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<string> codes = new();
        foreach (string code in Codes)
        {
            codes.Add(code);
        }

        return new Dictionary
        {
            ["codes"] = codes,
        };
    }

    /// <summary>
    /// Rebuilds a trade-code set from a dictionary payload.
    /// </summary>
    public static TravellerTradeCodeSet FromDictionary(Dictionary data)
    {
        TravellerTradeCodeSet set = new();
        if (!data.ContainsKey("codes") || data["codes"].VariantType != Variant.Type.Array)
        {
            return set;
        }

        foreach (Variant value in (Array)data["codes"])
        {
            if (value.VariantType == Variant.Type.String)
            {
                set.AddCode((string)value);
            }
        }

        return set;
    }

    private void SortCodes()
    {
        string[] sorted = new string[Codes.Count];
        for (int index = 0; index < Codes.Count; index += 1)
        {
            sorted[index] = Codes[index];
        }

        System.Array.Sort(sorted, System.StringComparer.Ordinal);
        Codes.Clear();
        foreach (string code in sorted)
        {
            Codes.Add(code);
        }
    }
}
