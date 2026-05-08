using Godot;
using Godot.Collections;
using StarGen.Domain.Population;

namespace StarGen.Domain.Generation;

/// <summary>
/// Starfinder-only adapter payload derived from the neutral sentient-world profile.
/// </summary>
public sealed class StarfinderWorldbuildingProfile
{
    /// <summary>
    /// Starfinder-facing technology tier.
    /// </summary>
    public string TechnologyTier { get; init; } = string.Empty;

    /// <summary>
    /// Compact social accord signal.
    /// </summary>
    public string AccordSignal { get; init; } = string.Empty;

    /// <summary>
    /// Compact religion signal.
    /// </summary>
    public string ReligionSignal { get; init; } = string.Empty;

    /// <summary>
    /// Starfinder-only magic prevalence signal.
    /// </summary>
    public string MagicPrevalence { get; init; } = string.Empty;

    /// <summary>
    /// Converts this adapter payload to a dictionary.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["technology_tier"] = TechnologyTier,
            ["accord_signal"] = AccordSignal,
            ["religion_signal"] = ReligionSignal,
            ["magic_prevalence"] = MagicPrevalence,
        };
    }

    /// <summary>
    /// Creates an adapter payload from serialized data.
    /// </summary>
    public static StarfinderWorldbuildingProfile FromDictionary(Dictionary data)
    {
        return new StarfinderWorldbuildingProfile
        {
            TechnologyTier = GetString(data, "technology_tier", string.Empty),
            AccordSignal = GetString(data, "accord_signal", string.Empty),
            ReligionSignal = GetString(data, "religion_signal", string.Empty),
            MagicPrevalence = GetString(data, "magic_prevalence", string.Empty),
        };
    }

    /// <summary>
    /// Builds the Starfinder-only adapter payload when the Starfinder ruleset is active.
    /// </summary>
    public static StarfinderWorldbuildingProfile? Build(
        SentientWorldProfile? profile,
        GenerationUseCaseSettings settings)
    {
        if (profile == null)
        {
            return null;
        }

        if (settings.RulesetMode != GenerationUseCaseSettings.RulesetModeType.Starfinder)
        {
            return null;
        }

        return new StarfinderWorldbuildingProfile
        {
            TechnologyTier = ResolveTechnologyTier(profile.CoreTechLevel),
            AccordSignal = ResolveAccordSignal(profile),
            ReligionSignal = ResolveReligionSignal(profile),
            MagicPrevalence = ResolveMagicPrevalence(profile),
        };
    }

    private static string ResolveTechnologyTier(int coreTechLevel)
    {
        if (coreTechLevel <= 6)
        {
            return "Primitive";
        }

        if (coreTechLevel <= 8)
        {
            return "Industrial";
        }

        if (coreTechLevel <= 9)
        {
            return "Digital";
        }

        if (coreTechLevel <= 11)
        {
            return "Spacefaring";
        }

        if (coreTechLevel <= 15)
        {
            return "Interstellar";
        }

        return "Advanced";
    }

    private static string ResolveAccordSignal(SentientWorldProfile profile)
    {
        double accord = profile.InternalLegitimacy
            + profile.ExternalLegitimacy
            - profile.ExternalThreat
            - (profile.FactionalFragmentation * 0.50);
        if (accord >= 1.05)
        {
            return "Integrated";
        }

        if (accord >= 0.70)
        {
            return "Cooperative";
        }

        if (accord >= 0.35)
        {
            return "Mixed";
        }

        if (profile.ExternalThreat >= 0.50 || profile.FactionalFragmentation >= 0.45)
        {
            return "Tense";
        }

        return "Isolated";
    }

    private static string ResolveReligionSignal(SentientWorldProfile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.ReligionStructure))
        {
            return "Unspecified";
        }

        return profile.ReligionStructure;
    }

    private static string ResolveMagicPrevalence(SentientWorldProfile profile)
    {
        double score = 0.35
            + (profile.CulturalAccumulation * 0.20)
            + (profile.ReligiousCentralization * 0.18)
            + (profile.TradeConnectivity * 0.12)
            + (profile.ExternalThreat * 0.10);
        if (profile.ReligionStructure == "Suppressed")
        {
            score -= 0.18;
        }

        if (profile.ReligionStructure == "State-Aligned")
        {
            score += 0.12;
        }

        if (score >= 0.72)
        {
            return "High";
        }

        if (score >= 0.46)
        {
            return "Moderate";
        }

        return "Low";
    }

    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        if (value.VariantType == Variant.Type.String)
        {
            return (string)value;
        }

        return fallback;
    }
}
