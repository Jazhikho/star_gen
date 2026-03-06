using Godot;
using Godot.Collections;

namespace StarGen.Domain.Generation.Archetypes;

/// <summary>
/// Traveller UWP planet size-code mapping helpers.
/// </summary>
public static class TravellerSizeCode
{
    public const double DiamKm0Max = 800.0;
    public const double DiamKm1Min = 800.0;
    public const double DiamKm1Max = 2400.0;
    public const double DiamKm2Min = 2400.0;
    public const double DiamKm2Max = 4000.0;
    public const double DiamKm3Min = 4000.0;
    public const double DiamKm3Max = 5600.0;
    public const double DiamKm4Min = 5600.0;
    public const double DiamKm4Max = 7200.0;
    public const double DiamKm5Min = 7200.0;
    public const double DiamKm5Max = 8800.0;
    public const double DiamKm6Min = 8800.0;
    public const double DiamKm6Max = 10400.0;
    public const double DiamKm7Min = 10400.0;
    public const double DiamKm7Max = 12000.0;
    public const double DiamKm8Min = 12000.0;
    public const double DiamKm8Max = 13600.0;
    public const double DiamKm9Min = 13600.0;
    public const double DiamKm9Max = 15200.0;
    public const double DiamKmAMin = 15200.0;
    public const double DiamKmAMax = 16800.0;
    public const double DiamKmBMin = 16800.0;
    public const double DiamKmBMax = 18400.0;
    public const double DiamKmCMin = 18400.0;
    public const double DiamKmCMax = 40000.0;
    public const double DiamKmDMin = 40000.0;
    public const double DiamKmDMax = 120000.0;
    public const double DiamKmEMin = 120000.0;

    /// <summary>
    /// Returns the Traveller size code for a diameter in kilometers.
    /// </summary>
    public static object DiameterKmToCode(double diameterKm)
    {
        if (diameterKm < 0.0 || diameterKm < DiamKm0Max)
        {
            return 0;
        }

        if (diameterKm < DiamKm1Max)
        {
            return 1;
        }

        if (diameterKm < DiamKm2Max)
        {
            return 2;
        }

        if (diameterKm < DiamKm3Max)
        {
            return 3;
        }

        if (diameterKm < DiamKm4Max)
        {
            return 4;
        }

        if (diameterKm < DiamKm5Max)
        {
            return 5;
        }

        if (diameterKm < DiamKm6Max)
        {
            return 6;
        }

        if (diameterKm < DiamKm7Max)
        {
            return 7;
        }

        if (diameterKm < DiamKm8Max)
        {
            return 8;
        }

        if (diameterKm < DiamKm9Max)
        {
            return 9;
        }

        if (diameterKm < DiamKmAMax)
        {
            return "A";
        }

        if (diameterKm < DiamKmBMax)
        {
            return "B";
        }

        if (diameterKm < DiamKmCMax)
        {
            return "C";
        }

        if (diameterKm < DiamKmDMax)
        {
            return "D";
        }

        return "E";
    }

    /// <summary>
    /// Returns the diameter range for a Traveller size code.
    /// </summary>
    public static Dictionary<string, double> CodeToDiameterRange(object? code)
    {
        if (code is Variant variantCode)
        {
            if (variantCode.VariantType == Variant.Type.Int)
            {
                return CodeToDiameterRange((int)variantCode);
            }

            if (variantCode.VariantType == Variant.Type.String)
            {
                return CodeToDiameterRange((string)variantCode);
            }
        }

        if (code is int intCode)
        {
            return intCode switch
            {
                0 => new Dictionary<string, double> { ["min"] = 0.0, ["max"] = DiamKm0Max },
                1 => new Dictionary<string, double> { ["min"] = DiamKm1Min, ["max"] = DiamKm1Max },
                2 => new Dictionary<string, double> { ["min"] = DiamKm2Min, ["max"] = DiamKm2Max },
                3 => new Dictionary<string, double> { ["min"] = DiamKm3Min, ["max"] = DiamKm3Max },
                4 => new Dictionary<string, double> { ["min"] = DiamKm4Min, ["max"] = DiamKm4Max },
                5 => new Dictionary<string, double> { ["min"] = DiamKm5Min, ["max"] = DiamKm5Max },
                6 => new Dictionary<string, double> { ["min"] = DiamKm6Min, ["max"] = DiamKm6Max },
                7 => new Dictionary<string, double> { ["min"] = DiamKm7Min, ["max"] = DiamKm7Max },
                8 => new Dictionary<string, double> { ["min"] = DiamKm8Min, ["max"] = DiamKm8Max },
                9 => new Dictionary<string, double> { ["min"] = DiamKm9Min, ["max"] = DiamKm9Max },
                _ => new Dictionary<string, double>(),
            };
        }

        if (code is long longCode && longCode >= int.MinValue && longCode <= int.MaxValue)
        {
            return CodeToDiameterRange((int)longCode);
        }

        if (code is string stringCode)
        {
            return stringCode.ToUpperInvariant() switch
            {
                "A" => new Dictionary<string, double> { ["min"] = DiamKmAMin, ["max"] = DiamKmAMax },
                "B" => new Dictionary<string, double> { ["min"] = DiamKmBMin, ["max"] = DiamKmBMax },
                "C" => new Dictionary<string, double> { ["min"] = DiamKmCMin, ["max"] = DiamKmCMax },
                "D" => new Dictionary<string, double> { ["min"] = DiamKmDMin, ["max"] = DiamKmDMax },
                "E" => new Dictionary<string, double> { ["min"] = DiamKmEMin, ["max"] = -1.0 },
                _ => new Dictionary<string, double>(),
            };
        }

        return new Dictionary<string, double>();
    }

    /// <summary>
    /// Returns the code as a single UWP character.
    /// </summary>
    public static string ToStringUwp(object? code)
    {
        if (code is null)
        {
            return "?";
        }

        if (code is Variant variantCode)
        {
            return variantCode.VariantType switch
            {
                Variant.Type.Int => ((int)variantCode).ToString(),
                Variant.Type.String => (string)variantCode,
                _ => "?",
            };
        }

        if (code is string stringCode)
        {
            return stringCode;
        }

        if (code is int intCode)
        {
            return intCode.ToString();
        }

        if (code is long longCode)
        {
            return longCode.ToString();
        }

        return "?";
    }
}
