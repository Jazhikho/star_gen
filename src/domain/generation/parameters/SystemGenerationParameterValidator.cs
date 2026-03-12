using System;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Systems;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Validates system-generation inputs before generation runs.
/// </summary>
public static class SystemGenerationParameterValidator
{
    /// <summary>
    /// Validates a solar-system spec and returns blocking errors and advisory warnings.
    /// </summary>
    public static GenerationParameterIssueSet Validate(SolarSystemSpec spec)
    {
        GenerationParameterIssueSet issues = new();
        if (spec.GenerationSeed <= 0)
        {
            issues.AddError("generation_seed", "Seed must be greater than zero.");
        }

        if (spec.StarCountMin < 1 || spec.StarCountMin > 10)
        {
            issues.AddError("star_count_min", "Minimum star count must be between 1 and 10.");
        }

        if (spec.StarCountMax < 1 || spec.StarCountMax > 10)
        {
            issues.AddError("star_count_max", "Maximum star count must be between 1 and 10.");
        }

        if (spec.StarCountMin > spec.StarCountMax)
        {
            issues.AddError("star_count_max", "Maximum star count must be greater than or equal to minimum star count.");
        }

        if (spec.StarCountMax >= 4)
        {
            issues.AddWarning("star_count_max", "High-multiplicity systems are allowed, but they are much rarer and produce more chaotic layouts.");
        }

        if (spec.SpectralClassHints.Count > spec.StarCountMax)
        {
            issues.AddWarning("spectral_class_hints", "Extra spectral hints beyond the generated star count will be ignored.");
        }

        foreach (int spectralClass in spec.SpectralClassHints)
        {
            if (!Enum.IsDefined(typeof(StarClass.SpectralClass), spectralClass))
            {
                issues.AddError("spectral_class_hints", "Spectral hints must use known stellar classes.");
                break;
            }
        }

        if (spec.SystemAgeYears > 0.0 && spec.SystemAgeYears < 1.0e6)
        {
            issues.AddError("system_age_years", "System age must be at least one million years when specified.");
        }

        if (spec.SystemAgeYears > 1.2e10)
        {
            issues.AddWarning("system_age_years", "Very old systems are allowed, but they push beyond the common Milky Way disk population.");
        }

        if (spec.SystemMetallicity > 0.0 && spec.SystemMetallicity < 0.01)
        {
            issues.AddWarning("system_metallicity", "Extremely metal-poor systems are allowed, but they strongly suppress planet-friendly chemistry.");
        }

        if (spec.SystemMetallicity > 0.0 && spec.SystemMetallicity > 3.0)
        {
            issues.AddWarning("system_metallicity", "Very high metallicity values are allowed, but they bend realism for most observed stars.");
        }

        if (spec.GeneratePopulation && spec.StarCountMax >= 5)
        {
            issues.AddWarning("generate_population", "Population generation on very high-multiplicity systems is supported, but habitability outcomes become less intuitive.");
        }

        if (spec.UseCaseSettings.IsTravellerMode() && !spec.GeneratePopulation)
        {
            issues.AddWarning("generate_population", "Traveller mode works best with population enabled so mainworld and UWP-oriented readouts have meaningful data.");
        }

        if (spec.UseCaseSettings.ShowTravellerReadouts && !spec.UseCaseSettings.IsTravellerMode())
        {
            issues.AddWarning("show_traveller_readouts", "Traveller readouts are enabled while the default ruleset remains active; values shown will be derived mappings only.");
        }

        if (spec.UseCaseSettings.MainworldPolicy == GenerationUseCaseSettings.MainworldPolicyType.Require
            && !spec.UseCaseSettings.IsTravellerMode())
        {
            issues.AddWarning("mainworld_policy", "Requiring a mainworld is mainly intended for Traveller-oriented flows.");
        }

        return issues;
    }
}
