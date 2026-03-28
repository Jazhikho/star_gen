using Godot;
using Godot.Collections;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Captures galaxy-derived context that should propagate into star and system generation.
/// </summary>
public partial class GalaxyOriginContext : RefCounted
{
    /// <summary>
    /// Resolved host-galaxy subtype.
    /// </summary>
    public GalaxyResolvedSubtype ResolvedSubtype { get; set; } = GalaxyResolvedSubtype.SpiralSb;

    /// <summary>
    /// Region class of the star inside the host galaxy.
    /// </summary>
    public GalaxyRegionKind RegionKind { get; set; } = GalaxyRegionKind.InnerDisk;

    /// <summary>
    /// Age cohort derived from the local galactic environment.
    /// </summary>
    public GalaxyAgeCohort AgeCohort { get; set; } = GalaxyAgeCohort.Mature;

    /// <summary>
    /// Metallicity prior relative to solar for downstream generation.
    /// </summary>
    public double MetallicityPrior { get; set; } = 1.0;

    /// <summary>
    /// Legacy age-bias scalar kept for compatibility with existing downstream logic.
    /// </summary>
    public double AgeBias { get; set; } = 1.0;

    /// <summary>
    /// Mean stellar age prior in gigayears.
    /// </summary>
    public double AgeMeanGyr { get; set; } = 5.5;

    /// <summary>
    /// Soft galactic habitable-zone weighting for this region.
    /// </summary>
    public double GhzWeight { get; set; } = 0.5;

    /// <summary>
    /// Relative hazard weighting for energetic or crowded regions.
    /// </summary>
    public double HazardWeight { get; set; } = 0.5;

    /// <summary>
    /// Probability that the star belongs to a young or dissolving cluster scaffold.
    /// </summary>
    public double ClusterProbability { get; set; } = 0.1;

    /// <summary>
    /// Whether the local environment is influenced by a central bar.
    /// </summary>
    public bool IsBarInfluenced { get; set; }

    /// <summary>
    /// Whether the local environment is influenced by a spiral arm.
    /// </summary>
    public bool IsArmInfluenced { get; set; }

    /// <summary>
    /// Normalized environment-density index inherited from the host-galaxy profile.
    /// </summary>
    public double EnvironmentDensityIndex { get; set; } = 0.25;

    /// <summary>
    /// Halo-mass proxy inherited from the host-galaxy profile.
    /// </summary>
    public double HaloMassLog10Solar { get; set; } = 12.0;

    /// <summary>
    /// Local density ratio relative to the galaxy reference density.
    /// </summary>
    public double LocalDensityRatio { get; set; } = 1.0;

    /// <summary>
    /// Local star-formation-efficiency prior.
    /// </summary>
    public double LocalStarFormationEfficiency { get; set; } = 0.1;

    /// <summary>
    /// Creates a detached copy of the context.
    /// </summary>
    public GalaxyOriginContext Clone()
    {
        return new GalaxyOriginContext
        {
            ResolvedSubtype = ResolvedSubtype,
            RegionKind = RegionKind,
            AgeCohort = AgeCohort,
            MetallicityPrior = MetallicityPrior,
            AgeBias = AgeBias,
            AgeMeanGyr = AgeMeanGyr,
            GhzWeight = GhzWeight,
            HazardWeight = HazardWeight,
            ClusterProbability = ClusterProbability,
            IsBarInfluenced = IsBarInfluenced,
            IsArmInfluenced = IsArmInfluenced,
            EnvironmentDensityIndex = EnvironmentDensityIndex,
            HaloMassLog10Solar = HaloMassLog10Solar,
            LocalDensityRatio = LocalDensityRatio,
            LocalStarFormationEfficiency = LocalStarFormationEfficiency,
        };
    }

    /// <summary>
    /// Creates a dictionary payload for persistence and provenance.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["resolved_subtype"] = (int)ResolvedSubtype,
            ["region_kind"] = (int)RegionKind,
            ["age_cohort"] = (int)AgeCohort,
            ["metallicity_prior"] = MetallicityPrior,
            ["age_bias"] = AgeBias,
            ["age_mean_gyr"] = AgeMeanGyr,
            ["ghz_weight"] = GhzWeight,
            ["hazard_weight"] = HazardWeight,
            ["cluster_probability"] = ClusterProbability,
            ["is_bar_influenced"] = IsBarInfluenced,
            ["is_arm_influenced"] = IsArmInfluenced,
            ["environment_density_index"] = EnvironmentDensityIndex,
            ["halo_mass_log10_solar"] = HaloMassLog10Solar,
            ["local_density_ratio"] = LocalDensityRatio,
            ["local_star_formation_efficiency"] = LocalStarFormationEfficiency,
        };
    }

    /// <summary>
    /// Rebuilds a context object from a serialized payload.
    /// </summary>
    public static GalaxyOriginContext FromDictionary(Dictionary data)
    {
        GalaxyOriginContext context = new GalaxyOriginContext();
        int subtypeValue = DomainDictionaryUtils.GetInt(data, "resolved_subtype", (int)GalaxyResolvedSubtype.SpiralSb);
        if (System.Enum.IsDefined(typeof(GalaxyResolvedSubtype), subtypeValue))
        {
            context.ResolvedSubtype = (GalaxyResolvedSubtype)subtypeValue;
        }

        int regionValue = DomainDictionaryUtils.GetInt(data, "region_kind", (int)GalaxyRegionKind.InnerDisk);
        if (System.Enum.IsDefined(typeof(GalaxyRegionKind), regionValue))
        {
            context.RegionKind = (GalaxyRegionKind)regionValue;
        }

        int ageCohortValue = DomainDictionaryUtils.GetInt(data, "age_cohort", (int)GalaxyAgeCohort.Mature);
        if (System.Enum.IsDefined(typeof(GalaxyAgeCohort), ageCohortValue))
        {
            context.AgeCohort = (GalaxyAgeCohort)ageCohortValue;
        }

        context.MetallicityPrior = DomainDictionaryUtils.GetDouble(data, "metallicity_prior", 1.0);
        context.AgeBias = DomainDictionaryUtils.GetDouble(data, "age_bias", 1.0);
        context.AgeMeanGyr = DomainDictionaryUtils.GetDouble(data, "age_mean_gyr", 5.5);
        context.GhzWeight = DomainDictionaryUtils.GetDouble(data, "ghz_weight", 0.5);
        context.HazardWeight = DomainDictionaryUtils.GetDouble(data, "hazard_weight", 0.5);
        context.ClusterProbability = DomainDictionaryUtils.GetDouble(data, "cluster_probability", 0.1);
        context.IsBarInfluenced = DomainDictionaryUtils.GetBool(data, "is_bar_influenced", false);
        context.IsArmInfluenced = DomainDictionaryUtils.GetBool(data, "is_arm_influenced", false);
        context.EnvironmentDensityIndex = DomainDictionaryUtils.GetDouble(data, "environment_density_index", 0.25);
        context.HaloMassLog10Solar = DomainDictionaryUtils.GetDouble(data, "halo_mass_log10_solar", 12.0);
        context.LocalDensityRatio = DomainDictionaryUtils.GetDouble(data, "local_density_ratio", 1.0);
        context.LocalStarFormationEfficiency = DomainDictionaryUtils.GetDouble(data, "local_star_formation_efficiency", 0.1);
        return context;
    }
}
