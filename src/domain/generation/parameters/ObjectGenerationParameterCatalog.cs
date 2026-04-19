using System.Collections.Generic;

namespace StarGen.Domain.Generation.Parameters;

/// <summary>
/// Direct object-level parameter help for Object Studio.
/// </summary>
public static class ObjectGenerationParameterCatalog
{
    private static readonly Dictionary<string, string> Tooltips = new()
    {
        ["ruleset_mode"] = "Realistic keeps StarGen's normal direct object generation.\nSpace Opera enables RPG-facing shortcuts like Traveller world-profile shaping where supported.",
        ["show_traveller_readouts"] = "Shows Universal World Profile code when the generated object has enough information to derive one.\nThis changes the readout, not the physical body by itself.",
        ["use_traveller_world_profile"] = "Build a Traveller-style world profile first, then map that profile into the generated planet.\nUse this when you want an RPG-facing mainworld rather than a purely physical roll.",
        ["traveller_size_code"] = "This directly biases the Traveller size code used before planet generation.\nLeave it on Auto if you want the preset and seed to decide.",
        ["traveller_atmosphere_code"] = "This directly biases the Traveller atmosphere code used before planet generation.\nUse it when you want a specific RPG-facing atmosphere class.",
        ["traveller_hydrographics_code"] = "This directly biases the Traveller hydrographics code used before planet generation.\nHigher values generally mean more surface water in the Traveller profile.",
        ["traveller_population_code"] = "This directly biases the Traveller population code used before planet generation.\nUse it when you want the RPG profile to target a specific settlement scale.",
        ["planet_orbit_mode"] = "Choose Bound for a normal orbiting planet.\nChoose Rogue for a free-floating world with no final parent orbit shown.",
        ["planet_class_bias"] = "This nudges the broad planet kind.\nRocky favors denser land-heavy worlds.\nWater-rich favors wetter or icier worlds.\nSub-Neptune favors puffier volatile-rich worlds.\nGas Giant favors very large gas-rich worlds.\nStripped Core favors denser worlds that lost more gas.",
        ["planet_composition_bias"] = "This nudges what the planet is mostly made of.\nRocky favors silicates and metal.\nIce or Water-rich favors more volatiles.\nGas Envelope favors thicker gas around the planet.",
        ["planet_envelope_override"] = "This directly nudges how much gas the planet keeps.\nThin keeps some air.\nRetained keeps a thicker envelope.\nStripped favors a denser planet with far less gas left.",
        ["planet_volatile_richness"] = "Volatiles are materials like water and other ices that are easier to lose or freeze.\nHigher richness makes oceans, ice, and thicker atmospheres easier to get.\nPoor richness makes drier worlds easier to get.",
        ["planet_hydrosphere_tendency"] = "Hydrosphere means surface water and ice.\nDry favors little surface water.\nMixed favors partial oceans.\nOceanic favors water-heavy worlds.",
        ["planet_size_category"] = "This pushes the final size band of the planet.\nLeave it on Auto if you want the seed and preset to decide.",
        ["planet_orbit_zone"] = "This pushes where the planet sits around its star.\nInner favors hotter close-in worlds.\nTemperate favors milder worlds.\nOuter favors colder distant worlds.",
        ["planet_atmosphere"] = "Auto lets seeded generation decide when an atmosphere makes sense.\nYes forces an atmosphere target.\nNo favors an airless result.",
        ["planet_rings"] = "Auto lets seeded generation decide when rings make sense.\nYes forces a ring target.\nNo leaves rings out.",
        ["planet_ring_complexity"] = "This nudges how simple or elaborate the ring system is if rings are present.\nLeave it on Auto if rings should stay seed-driven.",
        ["planet_surface_pressure"] = "This nudges surface air pressure.\nThin favors tenuous air.\nModerate favors Earth-like pressure.\nDense favors heavier atmospheres.",
        ["planet_ocean_coverage"] = "This nudges how much of the surface is covered by oceans.\nDry favors little open water.\nMixed favors partial oceans.\nOcean World favors water-heavy surfaces.",
        ["planet_ice_coverage"] = "This nudges how icy the surface is.\nIce-free favors warmer bare surfaces.\nSeasonal favors patchy or periodic ice.\nFrozen favors heavy ice cover.",
        ["planet_albedo_profile"] = "Albedo is how reflective the surface is.\nDark worlds absorb more light.\nBright worlds reflect more light.\nBalanced stays in the middle.",
        ["planet_volcanism"] = "This nudges geological activity.\nQuiet favors calmer worlds.\nActive favors more resurfacing and heat.\nExtreme favors strongly volcanic worlds.",
        ["planet_generate_moons"] = "Turn this on if the planet should also generate moons before opening the viewer.\nThe moons come from the same seed family as the planet.",
        ["planet_moon_target_count"] = "This sets a target moon count, not a guaranteed final count.\nThe generator still caps the result by planet size so small worlds do not get giant-planet moon systems.",
        ["planet_moon_captured_bias"] = "Captured moons are outsider bodies snagged by gravity instead of forming with the planet.\nTurn this on to bias the moon bundle toward more irregular captured satellites.",
        ["star_spectral_class"] = "A spectral class is the broad star type.\nO, B, A, F, G, K, and M are normal stars.\nL, T, and Y are brown dwarfs.",
        ["star_subclass"] = "The subclass fine-tunes the chosen spectral class from hotter to cooler within that class.\nLower numbers are usually hotter within the same letter class.",
        ["asteroid_type"] = "This chooses the asteroid's broad composition family.\nDifferent families change likely density, reflectivity, and overall character.",
        ["asteroid_large"] = "Turn this on to bias the asteroid toward the larger end of its class instead of a smaller fragment.",
        ["asteroid_orbit_band"] = "This pushes where the asteroid sits in the system.\nInner favors warmer inner-system material.\nMain Belt favors classic belt placement.\nOuter favors colder distant placement.",
        ["asteroid_density_profile"] = "This nudges how loose or dense the asteroid is.\nLoose favors rubble-pile style bodies.\nDense favors more compact rock or metal-rich bodies.",
        ["asteroid_albedo_profile"] = "This nudges how dark or bright the asteroid surface is.\nDark absorbs more light.\nBright reflects more light.",
        ["comet_family"] = "Jupiter-family comets are shorter-period comets shaped by the giant planets.\nLong-period comets come in from much farther out on stretched orbits.",
        ["comet_activity"] = "Active comets vent more gas and dust.\nDormant comets keep comet origins but show little activity.\nExtinct comets are mostly spent icy bodies.",
        ["comet_large"] = "Turn this on to bias the comet toward a larger nucleus.",
        ["advanced_controls"] = "Advanced controls expose explicit override keys used later by the editor.\nUse them only when you want to target a specific physical value instead of leaving the direct object profile to guide generation.",
    };

    public static string GetTooltip(string parameterId)
    {
        if (Tooltips.TryGetValue(parameterId, out string? tooltip))
        {
            return tooltip;
        }

        return string.Empty;
    }
}
