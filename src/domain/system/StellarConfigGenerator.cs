using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Constants;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Systems;

/// <summary>
/// Generates stellar configurations for solar systems.
/// </summary>
public static class StellarConfigGenerator
{
    private static readonly float[] SeparationWeights = { 15.0f, 50.0f, 35.0f };

    private const double CloseBinaryMinAu = 0.01;
    private const double CloseBinaryMaxAu = 1.0;
    private const double ModerateBinaryMinAu = 1.0;
    private const double ModerateBinaryMaxAu = 50.0;
    private const double WideBinaryMinAu = 50.0;
    private const double WideBinaryMaxAu = 10000.0;
    private const double CloseEccMax = 0.6;
    private const double ModerateEccMax = 0.8;
    private const double WideEccMax = 0.9;

    /// <summary>
    /// Generates a stellar configuration from a system specification.
    /// </summary>
    public static SolarSystem? Generate(SolarSystemSpec spec, SeededRng rng)
    {
        int starCount = DetermineStarCount(spec, rng);
        if (starCount < 1 || starCount > 10)
        {
            GD.PushError($"Invalid star count: {starCount}");
            return null;
        }

        string systemName;
        if (string.IsNullOrEmpty(spec.NameHint))
        {
            systemName = $"System-{spec.GenerationSeed}";
        }
        else
        {
            systemName = spec.NameHint;
        }

        SolarSystem system = new(
            GenerateSystemId(rng),
            systemName);

        Array<CelestialBody> stars = GenerateStars(spec, starCount, rng);
        if (stars.Count == 0)
        {
            GD.PushError("Failed to generate stars");
            return null;
        }

        foreach (CelestialBody star in stars)
        {
            system.AddBody(star);
        }

        SystemHierarchy? hierarchy = BuildHierarchy(stars, rng);
        if (hierarchy == null || !hierarchy.IsValid())
        {
            GD.PushError("Failed to build hierarchy");
            return null;
        }

        system.Hierarchy = hierarchy;
        CalculateOrbitHosts(system, stars);
        system.Provenance = new Provenance(
            spec.GenerationSeed,
            Versions.GeneratorVersion,
            Versions.SchemaVersion,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            spec.ToDictionary());
        return system;
    }

    /// <summary>
    /// Determines the number of stars to generate.
    /// </summary>
    private static int DetermineStarCount(SolarSystemSpec spec, SeededRng rng)
    {
        Variant overrideCount = spec.GetOverride("star_count", default);
        if (!overrideCount.Equals(default(Variant)) && overrideCount.VariantType == Variant.Type.Int)
        {
            return System.Math.Clamp((int)overrideCount, 1, 10);
        }

        if (spec.StarCountMin == spec.StarCountMax)
        {
            return spec.StarCountMin;
        }

        List<int> options = new();
        List<float> weights = new();
        for (int count = spec.StarCountMin; count <= spec.StarCountMax; count += 1)
        {
            options.Add(count);
            weights.Add((float)(1.0 / System.Math.Pow(2.0, count - 1)));
        }

        int? selected = rng.WeightedChoice(options, weights);
        return selected ?? spec.StarCountMin;
    }

    /// <summary>
    /// Generates the stars in the system.
    /// </summary>
    private static Array<CelestialBody> GenerateStars(SolarSystemSpec spec, int count, SeededRng rng)
    {
        Array<CelestialBody> stars = new();
        for (int index = 0; index < count; index += 1)
        {
            int starSeed = unchecked((int)rng.Randi());
            SeededRng starRng = new(starSeed);

            StarSpec starSpec;
            if (index < spec.SpectralClassHints.Count)
            {
                starSpec = new StarSpec(
                    starSeed,
                    spec.SpectralClassHints[index],
                    -1,
                    spec.SystemMetallicity,
                    spec.SystemAgeYears);
            }
            else
            {
                starSpec = new StarSpec(
                    starSeed,
                    -1,
                    -1,
                    spec.SystemMetallicity,
                    spec.SystemAgeYears);
            }

            CelestialBody star = StarGenerator.Generate(starSpec, starRng);
            star.Id = $"star_{index}";
            if (string.IsNullOrEmpty(star.Name))
            {
                star.Name = GenerateStarName(index, count);
            }

            stars.Add(star);
        }

        return stars;
    }

    /// <summary>
    /// Generates a default name for a star in a multi-star system.
    /// </summary>
    private static string GenerateStarName(int index, int total)
    {
        if (total == 1)
        {
            return "Primary";
        }

        string[] labels =
        {
            "Alpha", "Beta", "Gamma", "Delta", "Epsilon",
            "Zeta", "Eta", "Theta", "Iota", "Kappa",
        };
        if (index < labels.Length)
        {
            return labels[index];
        }

        return $"Star {index + 1}";
    }

    /// <summary>
    /// Builds the stellar hierarchy.
    /// </summary>
    private static SystemHierarchy? BuildHierarchy(Array<CelestialBody> stars, SeededRng rng)
    {
        if (stars.Count == 0)
        {
            return null;
        }

        List<HierarchyNode> nodes = new();
        for (int index = 0; index < stars.Count; index += 1)
        {
            nodes.Add(HierarchyNode.CreateStar($"node_star_{index}", stars[index].Id));
        }

        if (nodes.Count == 1)
        {
            return new SystemHierarchy(nodes[0]);
        }

        int barycenterIndex = 0;
        while (nodes.Count > 1)
        {
            int indexA = rng.RandiRange(0, nodes.Count - 1);
            HierarchyNode nodeA = nodes[indexA];
            nodes.RemoveAt(indexA);

            int indexB = rng.RandiRange(0, nodes.Count - 1);
            HierarchyNode nodeB = nodes[indexB];
            nodes.RemoveAt(indexB);

            double separationM = GenerateBinarySeparation(nodeA, nodeB, rng);
            double eccentricity = GenerateBinaryEccentricity(separationM, rng);
            double massA = GetNodeMass(nodeA, stars);
            double massB = GetNodeMass(nodeB, stars);
            double periodS = OrbitalMechanics.CalculateOrbitalPeriod(separationM, massA + massB);

            HierarchyNode barycenter = HierarchyNode.CreateBarycenter(
                $"node_barycenter_{barycenterIndex}",
                nodeA,
                nodeB,
                separationM,
                eccentricity);
            barycenter.OrbitalPeriodS = periodS;
            barycenterIndex += 1;
            nodes.Add(barycenter);
        }

        return new SystemHierarchy(nodes[0]);
    }

    /// <summary>
    /// Returns the total mass of a hierarchy node.
    /// </summary>
    private static double GetNodeMass(HierarchyNode node, Array<CelestialBody> stars)
    {
        if (node.IsStar())
        {
            foreach (CelestialBody star in stars)
            {
                if (star.Id == node.StarId)
                {
                    return star.Physical.MassKg;
                }
            }

            return Units.SolarMassKg;
        }

        double total = 0.0;
        foreach (HierarchyNode child in node.Children)
        {
            total += GetNodeMass(child, stars);
        }

        return total;
    }

    /// <summary>
    /// Returns the total luminosity of a hierarchy node.
    /// </summary>
    private static double GetNodeLuminosity(HierarchyNode node, Array<CelestialBody> stars)
    {
        if (node.IsStar())
        {
            foreach (CelestialBody star in stars)
            {
                if (star.Id == node.StarId && star.HasStellar())
                {
                    return star.Stellar!.LuminosityWatts;
                }
            }

            return StellarProps.SolarLuminosityWatts;
        }

        double total = 0.0;
        foreach (HierarchyNode child in node.Children)
        {
            total += GetNodeLuminosity(child, stars);
        }

        return total;
    }

    /// <summary>
    /// Generates binary separation.
    /// </summary>
    private static double GenerateBinarySeparation(HierarchyNode nodeA, HierarchyNode nodeB, SeededRng rng)
    {
        int[] categories = { 0, 1, 2 };
        int category = rng.WeightedChoice(categories, SeparationWeights);

        double minAu;
        double maxAu;
        switch (category)
        {
            case 0:
                minAu = CloseBinaryMinAu;
                maxAu = CloseBinaryMaxAu;
                break;
            case 1:
                minAu = ModerateBinaryMinAu;
                maxAu = ModerateBinaryMaxAu;
                break;
            default:
                minAu = WideBinaryMinAu;
                maxAu = WideBinaryMaxAu;
                break;
        }

        if (nodeA.IsBarycenter() || nodeB.IsBarycenter())
        {
            double innerSeparationAu = 0.0;
            if (nodeA.IsBarycenter())
            {
                innerSeparationAu = System.Math.Max(innerSeparationAu, nodeA.SeparationM / Units.AuMeters);
            }

            if (nodeB.IsBarycenter())
            {
                innerSeparationAu = System.Math.Max(innerSeparationAu, nodeB.SeparationM / Units.AuMeters);
            }

            minAu = System.Math.Max(minAu, innerSeparationAu * 3.0);
            maxAu = System.Math.Max(maxAu, minAu * 10.0);
        }

        double logMin = System.Math.Log(minAu);
        double logMax = System.Math.Log(maxAu);
        double logSeparation = rng.RandfRange((float)logMin, (float)logMax);
        return System.Math.Exp(logSeparation) * Units.AuMeters;
    }

    /// <summary>
    /// Generates binary eccentricity.
    /// </summary>
    private static double GenerateBinaryEccentricity(double separationM, SeededRng rng)
    {
        double separationAu = separationM / Units.AuMeters;
        double maxEccentricity;
        if (separationAu < CloseBinaryMaxAu)
        {
            maxEccentricity = CloseEccMax;
        }
        else if (separationAu < ModerateBinaryMaxAu)
        {
            maxEccentricity = ModerateEccMax;
        }
        else
        {
            maxEccentricity = WideEccMax;
        }

        double raw = rng.Randf();
        return raw * raw * maxEccentricity;
    }

    /// <summary>
    /// Calculates orbit hosts for all hierarchy nodes.
    /// </summary>
    private static void CalculateOrbitHosts(SolarSystem system, Array<CelestialBody> stars)
    {
        foreach (HierarchyNode node in system.Hierarchy.GetAllNodes())
        {
            OrbitHost? host = CreateOrbitHostForNode(node, stars, system.Hierarchy);
            if (host != null && host.HasValidZone())
            {
                system.AddOrbitHost(host);
            }
        }
    }

    /// <summary>
    /// Creates an orbit host for a hierarchy node.
    /// </summary>
    private static OrbitHost? CreateOrbitHostForNode(HierarchyNode node, Array<CelestialBody> stars, SystemHierarchy hierarchy)
    {
        OrbitHost host;
        if (node.IsStar())
        {
            host = new OrbitHost(node.Id, OrbitHost.HostType.SType);
            CelestialBody? star = FindStarById(node.StarId, stars);
            if (star == null)
            {
                return null;
            }

            host.CombinedMassKg = star.Physical.MassKg;
            if (star.HasStellar())
            {
                host.CombinedLuminosityWatts = star.Stellar!.LuminosityWatts;
                host.EffectiveTemperatureK = star.Stellar.EffectiveTemperatureK;
            }

            host.InnerStabilityM = star.Physical.RadiusM * 3.0;
            HierarchyNode? parentBarycenter = FindParentBarycenter(node, hierarchy.Root);
            if (parentBarycenter != null)
            {
                double siblingMass = GetSiblingMass(node, parentBarycenter, stars);
                double massRatio;
                if (host.CombinedMassKg > 0.0)
                {
                    massRatio = siblingMass / host.CombinedMassKg;
                }
                else
                {
                    massRatio = 1.0;
                }
                host.OuterStabilityM = OrbitalMechanics.CalculateStypeStabilityLimit(
                    parentBarycenter.SeparationM,
                    massRatio,
                    parentBarycenter.Eccentricity);
            }
            else
            {
                host.OuterStabilityM = OrbitalMechanics.CalculateOuterStabilityLimitM(host.CombinedMassKg, 100.0);
            }
        }
        else
        {
            host = new OrbitHost(node.Id, OrbitHost.HostType.PType);
            host.CombinedMassKg = GetNodeMass(node, stars);
            host.CombinedLuminosityWatts = GetNodeLuminosity(node, stars);

            double totalLuminosity = 0.0;
            double weightedTemperature = 0.0;
            foreach (string starId in node.GetAllStarIds())
            {
                CelestialBody? star = FindStarById(starId, stars);
                if (star != null && star.HasStellar())
                {
                    double luminosity = star.Stellar!.LuminosityWatts;
                    totalLuminosity += luminosity;
                    weightedTemperature += luminosity * star.Stellar.EffectiveTemperatureK;
                }
            }

            if (totalLuminosity > 0.0)
            {
                host.EffectiveTemperatureK = weightedTemperature / totalLuminosity;
            }

            double childMassRatio = 1.0;
            if (node.Children.Count >= 2)
            {
                double mass0 = GetNodeMass(node.Children[0], stars);
                double mass1 = GetNodeMass(node.Children[1], stars);
                if (mass0 > 0.0)
                {
                    childMassRatio = mass1 / mass0;
                }
            }

            host.InnerStabilityM = OrbitalMechanics.CalculatePtypeStabilityLimit(
                node.SeparationM,
                childMassRatio,
                node.Eccentricity);
            HierarchyNode? parentBarycenter = FindParentBarycenter(node, hierarchy.Root);
            if (parentBarycenter != null)
            {
                double siblingMass = GetSiblingMass(node, parentBarycenter, stars);
                double massRatio;
                if (host.CombinedMassKg > 0.0)
                {
                    massRatio = siblingMass / host.CombinedMassKg;
                }
                else
                {
                    massRatio = 1.0;
                }
                host.OuterStabilityM = OrbitalMechanics.CalculateStypeStabilityLimit(
                    parentBarycenter.SeparationM,
                    massRatio,
                    parentBarycenter.Eccentricity);
            }
            else
            {
                host.OuterStabilityM = OrbitalMechanics.CalculateOuterStabilityLimitM(host.CombinedMassKg, 200.0);
            }
        }

        host.CalculateZones();
        return host;
    }

    /// <summary>
    /// Finds a star in a star array by id.
    /// </summary>
    private static CelestialBody? FindStarById(string starId, Array<CelestialBody> stars)
    {
        foreach (CelestialBody star in stars)
        {
            if (star.Id == starId)
            {
                return star;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the parent barycenter of a node.
    /// </summary>
    private static HierarchyNode? FindParentBarycenter(HierarchyNode target, HierarchyNode? current)
    {
        if (current == null || current.IsStar())
        {
            return null;
        }

        foreach (HierarchyNode child in current.Children)
        {
            if (child.Id == target.Id)
            {
                return current;
            }
        }

        foreach (HierarchyNode child in current.Children)
        {
            HierarchyNode? found = FindParentBarycenter(target, child);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the mass of a node's sibling inside a barycenter.
    /// </summary>
    private static double GetSiblingMass(HierarchyNode node, HierarchyNode parent, Array<CelestialBody> stars)
    {
        foreach (HierarchyNode child in parent.Children)
        {
            if (child.Id != node.Id)
            {
                return GetNodeMass(child, stars);
            }
        }

        return Units.SolarMassKg;
    }

    /// <summary>
    /// Generates a unique system id.
    /// </summary>
    private static string GenerateSystemId(SeededRng rng)
    {
        return $"system_{rng.Randi()}";
    }
}
