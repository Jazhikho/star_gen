using System;
using System.Collections.Generic;

namespace StarGen.Domain.Generation.Science;

/// <summary>
/// Registry of science-hardening engines and competing model families.
/// </summary>
public static class ScienceHardeningEngineCatalog
{
    /// <summary>
    /// Returns all completion-critical science engine descriptors.
    /// </summary>
    public static IReadOnlyList<ScienceHardeningEngineDescriptor> GetAll()
    {
        return EngineDescriptors;
    }

    /// <summary>
    /// Returns the descriptor for a required engine ID.
    /// </summary>
    public static ScienceHardeningEngineDescriptor GetRequired(string id)
    {
        foreach (ScienceHardeningEngineDescriptor descriptor in EngineDescriptors)
        {
            if (descriptor.Id == id)
            {
                return descriptor;
            }
        }

        throw new InvalidOperationException($"Science hardening engine '{id}' is not registered.");
    }

    /// <summary>
    /// Returns whether a domain has at least one engine descriptor.
    /// </summary>
    public static bool HasDomain(string domain)
    {
        foreach (ScienceHardeningEngineDescriptor descriptor in EngineDescriptors)
        {
            if (descriptor.Domain == domain)
            {
                return true;
            }
        }

        return false;
    }

    private static readonly ScienceHardeningEngineDescriptor[] EngineDescriptors =
    {
        new(
            "kopparapu_2014_mass_corrected_hz_diagnostic",
            "planet_habitability",
            "Kopparapu2013;Kopparapu2014;ChenKipping2017;Otegi2020",
            "Planet-mass HZ correction is recorded after mass/radius generation so orbit generation remains deterministic and save-compatible.",
            new[]
            {
                "kasting_1993_conservative_band",
                "kopparapu_2013_conservative_band",
                "kopparapu_2013_optimistic_band",
            },
            true),
        new(
            "mutual_hill_amd_packing_stability_diagnostic",
            "orbital_stability",
            "Obertas2017;HeEtAl2020;Petit2018;Petit2020;Laskar2017;Tamayo2020;FangMargot2013;ObertasTamayo2023",
            "Stability diagnostics avoid N-body integration during generation; active placement still uses the existing mutual-Hill scaffold unless a reviewed engine is selected later.",
            new[]
            {
                "mutual_hill_spacing_proxy",
                "amd_screen_diagnostic",
                "dynamical_packing_diagnostic",
                "resonance_proximity_diagnostic",
            },
            true),
        new(
            "moon_channel_architecture_diagnostic",
            "moon_formation",
            "Ronnet2020;Sasaki2010;Szulagyi2018;JewittHaghighipour2007;MalamudPerets2019;NakajimaEtAl2022;HellerBarnes2013",
            "Moon channels are deterministic architecture proxies, not hydrodynamic CPD, impact, tidal-evolution, or capture simulations.",
            new[]
            {
                "regular_cpd_pebble_accretion",
                "captured_irregular",
                "terrestrial_giant_impact_candidate",
                "habitable_edge_diagnostic",
            },
            true),
        new(
            "small_body_population_export_diagnostic",
            "small_body_population",
            "DeMeoCarry2014;BauerEtAl2017;KavelaarsEtAl2023;BernardinelliEtAl2022;NapierEtAl2023",
            "Reservoir records expose source-marked families and settlement/export readiness before calibrated survey-bias population counts drive station placement.",
            new[]
            {
                "reservoir_family_proxy",
                "survey_bias_size_luminosity_diagnostic",
                "station_habitat_followup_surface",
            },
            true),
        new(
            "galaxy_family_dynamics_diagnostic",
            "galaxy_dynamics",
            "BlandHawthornGerhard2016;Bovy2017;KhoperskovEtAl2024;HuntVasiliev2025;Hayden2014;Kennicutt1998",
            "Galaxy dynamics remain behavior-gated because calibrated potentials and non-Milky-Way comparison models can alter placement, routes, and populations.",
            new[]
            {
                "milky_way_analog_preset",
                "family_proxy_comparison_preset",
                "affect_region_context",
                "affect_placement",
            },
            true),
        new(
            "human_audit_required_sentient_population_proxy",
            "sentient_population",
            "HamiltonEtAl2020;BettencourtEtAl2007;ArvidssonEtAl2023;Knez2023;Stokey2020;Comin2013;CominMestieri2013;BallandEtAl2022;Chowdhury2022",
            "Population, technology, law, culture, religion, and governance outputs remain human-audit-required proxies, not final cultural or historical authority.",
            new[]
            {
                "aggregate_diffusion_proxy",
                "per_technology_diffusion_followup",
                "jurisdiction_hierarchy_followup",
                "state_capacity_human_audit_surface",
            },
            true),
    };
}
