using System;
using StarGen.Domain.Population;
using StdMath = System.Math;

namespace StarGen.Domain.Population.StationDesign;

/// <summary>
/// Maps station metadata into deterministic detailed-design inputs.
/// </summary>
public static class DesignMapping
{
    /// <summary>
    /// Derives a design template from station purpose and class.
    /// </summary>
    public static DesignTemplate DeriveTemplate(
        StationPurpose.Purpose purpose,
        StationClass.Class stationClass)
    {
        if (purpose == StationPurpose.Purpose.Utility || purpose == StationPurpose.Purpose.Communications)
        {
            if (stationClass == StationClass.Class.U || stationClass == StationClass.Class.O)
            {
                return DesignTemplate.Waystation;
            }

            return DesignTemplate.Scout;
        }

        if (purpose == StationPurpose.Purpose.Trade)
        {
            if (stationClass == StationClass.Class.U || stationClass == StationClass.Class.O)
            {
                return DesignTemplate.HighportD;
            }

            if (stationClass == StationClass.Class.B)
            {
                return DesignTemplate.HighportC;
            }

            if (stationClass == StationClass.Class.A)
            {
                return DesignTemplate.HighportB;
            }

            return DesignTemplate.HighportA;
        }

        if (purpose == StationPurpose.Purpose.Military)
        {
            if (stationClass == StationClass.Class.U || stationClass == StationClass.Class.O)
            {
                return DesignTemplate.Defense;
            }

            return DesignTemplate.Naval;
        }

        if (purpose == StationPurpose.Purpose.Science)
        {
            if (stationClass == StationClass.Class.U || stationClass == StationClass.Class.O)
            {
                return DesignTemplate.Research;
            }

            return DesignTemplate.Scout;
        }

        if (purpose == StationPurpose.Purpose.Mining)
        {
            return DesignTemplate.Mining;
        }

        if (purpose == StationPurpose.Purpose.Residential || purpose == StationPurpose.Purpose.Administrative)
        {
            if (stationClass == StationClass.Class.U || stationClass == StationClass.Class.O)
            {
                return DesignTemplate.Waystation;
            }

            if (stationClass == StationClass.Class.B)
            {
                return DesignTemplate.Trade;
            }

            return DesignTemplate.Freeport;
        }

        if (purpose == StationPurpose.Purpose.Industrial)
        {
            if (stationClass == StationClass.Class.U || stationClass == StationClass.Class.O)
            {
                return DesignTemplate.Mining;
            }

            return DesignTemplate.Trade;
        }

        if (purpose == StationPurpose.Purpose.Medical)
        {
            if (stationClass == StationClass.Class.U || stationClass == StationClass.Class.O)
            {
                return DesignTemplate.Research;
            }

            return DesignTemplate.Freeport;
        }

        return DesignTemplate.Custom;
    }

    /// <summary>
    /// Derives detailed-design hull tonnage from station population and class.
    /// </summary>
    public static int DeriveHullTonnage(
        DesignTemplate template,
        StationClass.Class stationClass,
        int population)
    {
        if (!ComponentCatalog.Templates.TryGetValue(template, out (string Name, int MinHull, int MaxHull, string Description) templateData))
        {
            throw new ArgumentException($"Unknown design template: {template}");
        }

        (double MinSpan, double MaxSpan) span = GetClassSpan(stationClass);
        double normalizedPopulation = NormalizePopulation(stationClass, population);
        double spanProgress = span.MinSpan + ((span.MaxSpan - span.MinSpan) * normalizedPopulation);
        double hullRange = templateData.MaxHull - templateData.MinHull;
        double rawHull = templateData.MinHull + (hullRange * spanProgress);
        int roundedHull = (int)(StdMath.Ceiling(rawHull / 100.0) * 100.0);
        return StdMath.Clamp(roundedHull, templateData.MinHull, templateData.MaxHull);
    }

    /// <summary>
    /// Derives the preferred hull configuration from station location and template.
    /// </summary>
    public static HullConfiguration DeriveHullConfiguration(
        StationType.Type stationType,
        DesignTemplate template)
    {
        if (stationType == StationType.Type.AsteroidBelt
            && (template == DesignTemplate.Mining || template == DesignTemplate.Trade))
        {
            return HullConfiguration.Asteroid;
        }

        if (stationType == StationType.Type.Lagrange)
        {
            return HullConfiguration.Platform;
        }

        if (stationType == StationType.Type.DeepSpace)
        {
            if (template == DesignTemplate.Waystation
                || template == DesignTemplate.Defense
                || template == DesignTemplate.Naval)
            {
                return HullConfiguration.Platform;
            }

            return HullConfiguration.Cylinder;
        }

        if (stationType == StationType.Type.Orbital)
        {
            if (template == DesignTemplate.Trade
                || template == DesignTemplate.HighportA
                || template == DesignTemplate.HighportB
                || template == DesignTemplate.HighportC
                || template == DesignTemplate.HighportD
                || template == DesignTemplate.Freeport)
            {
                return HullConfiguration.Ring;
            }

            if (template == DesignTemplate.Research || template == DesignTemplate.Scout)
            {
                return HullConfiguration.Cylinder;
            }
        }

        return HullConfiguration.Sphere;
    }

    private static (double MinSpan, double MaxSpan) GetClassSpan(StationClass.Class stationClass)
    {
        if (stationClass == StationClass.Class.U)
        {
            return (0.10, 0.20);
        }

        if (stationClass == StationClass.Class.O)
        {
            return (0.15, 0.35);
        }

        if (stationClass == StationClass.Class.B)
        {
            return (0.35, 0.60);
        }

        if (stationClass == StationClass.Class.A)
        {
            return (0.60, 0.85);
        }

        return (1.0, 1.0);
    }

    private static double NormalizePopulation(StationClass.Class stationClass, int population)
    {
        if (stationClass == StationClass.Class.S)
        {
            return 1.0;
        }

        int minPopulation = GetClassMinPopulation(stationClass);
        int maxPopulation = GetClassMaxPopulation(stationClass);
        if (maxPopulation <= minPopulation)
        {
            return 1.0;
        }

        double normalized = (population - minPopulation) / (double)(maxPopulation - minPopulation);
        return StdMath.Clamp(normalized, 0.0, 1.0);
    }

    private static int GetClassMinPopulation(StationClass.Class stationClass)
    {
        if (stationClass == StationClass.Class.B)
        {
            return StationClass.OutpostMax + 1;
        }

        if (stationClass == StationClass.Class.A)
        {
            return StationClass.BaseMax + 1;
        }

        if (stationClass == StationClass.Class.S)
        {
            return StationClass.AnchorMax + 1;
        }

        return 0;
    }

    private static int GetClassMaxPopulation(StationClass.Class stationClass)
    {
        if (stationClass == StationClass.Class.B)
        {
            return StationClass.BaseMax;
        }

        if (stationClass == StationClass.Class.A)
        {
            return StationClass.AnchorMax;
        }

        if (stationClass == StationClass.Class.S)
        {
            return StationClass.AnchorMax + 1;
        }

        return StationClass.OutpostMax;
    }
}
