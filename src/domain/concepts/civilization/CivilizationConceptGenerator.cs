using System;
using System.Collections.Generic;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Concepts.Civilization;

/// <summary>
/// Deterministic civilisation generator used by the concept atlas.
/// </summary>
public static class CivilizationConceptGenerator
{
    private static readonly string[] CoastalEconomy =
    {
        "Maritime trade",
        "Port logistics",
        "Shipbuilding",
        "Fisheries",
    };

    private static readonly string[] DesertEconomy =
    {
        "Caravan exchange",
        "Irrigated agriculture",
        "Mineral extraction",
        "Water-right administration",
    };

    private static readonly string[] GenericEconomy =
    {
        "Agriculture",
        "Craft production",
        "Regional trade",
        "Infrastructure building",
    };

    private static readonly string[] LateEconomy =
    {
        "Heavy industry",
        "Scientific research",
        "Data services",
        "Interstellar logistics",
    };

    private static readonly string[] ValuePool =
    {
        "Ritual continuity",
        "Mercantile pragmatism",
        "Frontier resilience",
        "Collective duty",
        "Scholarly prestige",
        "Martial honour",
        "Civic debate",
        "Kinship patronage",
        "Technocratic stewardship",
        "Ceremonial legitimacy",
        "Ecological reciprocity",
        "Expansionist ambition",
    };

    private static readonly string[] LegitimacyFrames =
    {
        "ancestral continuity",
        "sacred mandate",
        "civic representation",
        "technocratic competence",
        "martial protection",
        "commercial prosperity",
    };

    /// <summary>
    /// Generates a civilisation snapshot from shared concept context.
    /// </summary>
    public static CivilizationConceptSnapshot Generate(ConceptContextSnapshot context)
    {
        SeededRng rng = new(context.Seed ^ 0x63A1D29);
        TechnologyLevel.Level techLevel = ResolveTechLevel(context, rng);
        GovernmentType.Regime regime = ResolveRegime(context, techLevel, rng);
        string polityName = ResolvePolityName(context, regime, rng);

        double centralization = ResolveCentralization(regime, techLevel, rng);
        double inclusiveness = ResolveInclusiveness(regime, techLevel, rng);
        double externalPressure = ResolveExternalPressure(context, rng);
        double innovation = ResolveInnovation(techLevel, context, rng);
        double stability = ResolveStability(centralization, inclusiveness, externalPressure, innovation, rng);

        CivilizationConceptSnapshot snapshot = new CivilizationConceptSnapshot();
        snapshot.PolityName = polityName;
        snapshot.RegimeName = GovernmentType.ToStringName(regime);
        snapshot.TechEra = TechnologyLevel.ToStringName(techLevel);
        snapshot.Stability = stability;
        snapshot.Centralization = centralization;
        snapshot.Inclusiveness = inclusiveness;
        snapshot.Innovation = innovation;
        snapshot.ExternalPressure = externalPressure;
        snapshot.LegitimacyFrame = Pick(LegitimacyFrames, rng);
        snapshot.EconomySectors = BuildEconomy(context, techLevel, rng);
        snapshot.CulturalValues = BuildCulturalValues(regime, context, rng);
        snapshot.HistoricalMilestones = BuildMilestones(snapshot, context, techLevel, rng);
        snapshot.ExternalPosture = BuildExternalPosture(snapshot, techLevel, rng);
        return snapshot;
    }

    private static TechnologyLevel.Level ResolveTechLevel(ConceptContextSnapshot context, SeededRng rng)
    {
        if (context.TechnologyLevel.HasValue)
        {
            return context.TechnologyLevel.Value;
        }

        if (context.Population > 100000000)
        {
            return TechnologyLevel.Level.Spacefaring;
        }

        if (context.Population > 10000000)
        {
            return TechnologyLevel.Level.Information;
        }

        if (context.Population > 1000000)
        {
            return TechnologyLevel.Level.Industrial;
        }

        if (context.HabitabilityScore >= 7)
        {
            return TechnologyLevel.Level.Classical;
        }

        int offset = rng.RandiRange(0, 2);
        return (TechnologyLevel.Level)System.Math.Clamp(offset + (int)TechnologyLevel.Level.BronzeAge, 0, TechnologyLevel.Count() - 1);
    }

    private static GovernmentType.Regime ResolveRegime(
        ConceptContextSnapshot context,
        TechnologyLevel.Level techLevel,
        SeededRng rng)
    {
        if (context.Regime.HasValue)
        {
            return context.Regime.Value;
        }

        List<GovernmentType.Regime> options = new List<GovernmentType.Regime>();
        if (techLevel <= TechnologyLevel.Level.IronAge)
        {
            options.Add(GovernmentType.Regime.Tribal);
            options.Add(GovernmentType.Regime.Chiefdom);
            options.Add(GovernmentType.Regime.CityState);
        }
        else if (techLevel <= TechnologyLevel.Level.Renaissance)
        {
            options.Add(GovernmentType.Regime.Feudal);
            options.Add(GovernmentType.Regime.PatrimonialKingdom);
            options.Add(GovernmentType.Regime.BureaucraticEmpire);
        }
        else if (techLevel <= TechnologyLevel.Level.Information)
        {
            options.Add(GovernmentType.Regime.Constitutional);
            options.Add(GovernmentType.Regime.EliteRepublic);
            options.Add(GovernmentType.Regime.Corporate);
            options.Add(GovernmentType.Regime.Theocracy);
        }
        else
        {
            options.Add(GovernmentType.Regime.MassDemocracy);
            options.Add(GovernmentType.Regime.Technocracy);
            options.Add(GovernmentType.Regime.OnePartyState);
            options.Add(GovernmentType.Regime.Corporate);
        }

        return Pick(options, rng);
    }

    private static string ResolvePolityName(
        ConceptContextSnapshot context,
        GovernmentType.Regime regime,
        SeededRng rng)
    {
        string baseName = context.BodyName;
        if (string.IsNullOrWhiteSpace(baseName))
        {
            string[] roots =
            {
                "Aster",
                "Caldris",
                "Meru",
                "Thessa",
                "Varun",
                "Lyr",
                "Nemor",
                "Saur",
            };
            baseName = Pick(roots, rng);
        }

        string suffix = regime switch
        {
            GovernmentType.Regime.MassDemocracy => "Assembly",
            GovernmentType.Regime.Constitutional => "Commonwealth",
            GovernmentType.Regime.Corporate => "Combine",
            GovernmentType.Regime.Technocracy => "Directorate",
            GovernmentType.Regime.Theocracy => "Sacral State",
            GovernmentType.Regime.MilitaryJunta => "Command",
            GovernmentType.Regime.BureaucraticEmpire => "Imperium",
            _ => "Polity",
        };

        return baseName + " " + suffix;
    }

    private static double ResolveCentralization(
        GovernmentType.Regime regime,
        TechnologyLevel.Level techLevel,
        SeededRng rng)
    {
        double baseValue = regime switch
        {
            GovernmentType.Regime.Tribal => 0.22,
            GovernmentType.Regime.Chiefdom => 0.34,
            GovernmentType.Regime.CityState => 0.42,
            GovernmentType.Regime.Feudal => 0.48,
            GovernmentType.Regime.BureaucraticEmpire => 0.82,
            GovernmentType.Regime.AbsoluteMonarchy => 0.76,
            GovernmentType.Regime.Constitutional => 0.58,
            GovernmentType.Regime.MassDemocracy => 0.55,
            GovernmentType.Regime.OnePartyState => 0.78,
            GovernmentType.Regime.Corporate => 0.69,
            GovernmentType.Regime.Technocracy => 0.73,
            GovernmentType.Regime.Theocracy => 0.71,
            _ => 0.50,
        };
        double techModifier = ((int)techLevel / (double)(TechnologyLevel.Count() - 1)) * 0.12;
        return Clamp01(baseValue + techModifier + rng.RandfRange(-0.08f, 0.08f));
    }

    private static double ResolveInclusiveness(
        GovernmentType.Regime regime,
        TechnologyLevel.Level techLevel,
        SeededRng rng)
    {
        double baseValue = regime switch
        {
            GovernmentType.Regime.Tribal => 0.48,
            GovernmentType.Regime.CityState => 0.54,
            GovernmentType.Regime.Constitutional => 0.72,
            GovernmentType.Regime.MassDemocracy => 0.82,
            GovernmentType.Regime.Technocracy => 0.44,
            GovernmentType.Regime.Corporate => 0.31,
            GovernmentType.Regime.Theocracy => 0.26,
            GovernmentType.Regime.MilitaryJunta => 0.18,
            GovernmentType.Regime.OnePartyState => 0.24,
            _ => 0.35,
        };
        double techModifier = 0.0;
        if (techLevel >= TechnologyLevel.Level.Information)
        {
            techModifier = 0.08;
        }

        return Clamp01(baseValue + techModifier + rng.RandfRange(-0.08f, 0.08f));
    }

    private static double ResolveExternalPressure(ConceptContextSnapshot context, SeededRng rng)
    {
        double pressure = 0.20;
        if (context.RadiationLevel > 0.45)
        {
            pressure += 0.20;
        }

        if (context.HabitabilityScore <= 4)
        {
            pressure += 0.18;
        }

        if (context.Population > 50000000)
        {
            pressure += 0.12;
        }

        return Clamp01(pressure + rng.RandfRange(-0.08f, 0.16f));
    }

    private static double ResolveInnovation(
        TechnologyLevel.Level techLevel,
        ConceptContextSnapshot context,
        SeededRng rng)
    {
        double baseValue = ((int)techLevel + 1) / (double)TechnologyLevel.Count();
        if (context.Population > 10000000)
        {
            baseValue += 0.08;
        }

        if (context.DominantBiome.Equals("Desert", StringComparison.OrdinalIgnoreCase))
        {
            baseValue += 0.04;
        }

        return Clamp01(baseValue + rng.RandfRange(-0.07f, 0.07f));
    }

    private static double ResolveStability(
        double centralization,
        double inclusiveness,
        double externalPressure,
        double innovation,
        SeededRng rng)
    {
        double score = 0.46 + (centralization * 0.18) + (inclusiveness * 0.16) + (innovation * 0.08) - (externalPressure * 0.28);
        return Clamp01(score + rng.RandfRange(-0.08f, 0.08f));
    }

    private static List<string> BuildEconomy(
        ConceptContextSnapshot context,
        TechnologyLevel.Level techLevel,
        SeededRng rng)
    {
        List<string> sectors = new List<string>();
        string biome = context.DominantBiome;
        if (biome.Equals("Oceanic", StringComparison.OrdinalIgnoreCase))
        {
            AddDistinct(sectors, Pick(CoastalEconomy, rng));
            AddDistinct(sectors, Pick(CoastalEconomy, rng));
        }
        else if (biome.Equals("Desert", StringComparison.OrdinalIgnoreCase))
        {
            AddDistinct(sectors, Pick(DesertEconomy, rng));
            AddDistinct(sectors, Pick(DesertEconomy, rng));
        }
        else
        {
            AddDistinct(sectors, Pick(GenericEconomy, rng));
            AddDistinct(sectors, Pick(GenericEconomy, rng));
        }

        if (techLevel >= TechnologyLevel.Level.Industrial)
        {
            AddDistinct(sectors, Pick(LateEconomy, rng));
        }

        if (techLevel >= TechnologyLevel.Level.Spacefaring)
        {
            AddDistinct(sectors, "Orbital infrastructure");
        }

        return sectors;
    }

    private static List<string> BuildCulturalValues(
        GovernmentType.Regime regime,
        ConceptContextSnapshot context,
        SeededRng rng)
    {
        List<string> values = new List<string>();
        if (regime == GovernmentType.Regime.Theocracy)
        {
            AddDistinct(values, "Ceremonial legitimacy");
        }

        if (regime == GovernmentType.Regime.Corporate)
        {
            AddDistinct(values, "Mercantile pragmatism");
        }

        if (context.DominantBiome.Equals("Forest", StringComparison.OrdinalIgnoreCase))
        {
            AddDistinct(values, "Ecological reciprocity");
        }

        while (values.Count < 3)
        {
            AddDistinct(values, Pick(ValuePool, rng));
        }

        return values;
    }

    private static List<string> BuildMilestones(
        CivilizationConceptSnapshot snapshot,
        ConceptContextSnapshot context,
        TechnologyLevel.Level techLevel,
        SeededRng rng)
    {
        List<string> milestones = new List<string>();
        milestones.Add("Founding era: regional authority consolidated around " + snapshot.LegitimacyFrame + ".");

        if (techLevel >= TechnologyLevel.Level.Classical)
        {
            milestones.Add("Administrative expansion: " + snapshot.RegimeName + " institutions codified taxation, law, and record keeping.");
        }

        if (techLevel >= TechnologyLevel.Level.Industrial)
        {
            milestones.Add("Industrial transition: " + Pick(LateEconomy, rng).ToLowerInvariant() + " reoriented labour and transport.");
        }

        if (context.Population > 25000000)
        {
            milestones.Add("Metropolitan era: population scale forced new bargains between centre and periphery.");
        }
        else
        {
            milestones.Add("Regional consolidation: the polity remained anchored to a small set of strategic settlements.");
        }

        return milestones;
    }

    private static List<string> BuildExternalPosture(
        CivilizationConceptSnapshot snapshot,
        TechnologyLevel.Level techLevel,
        SeededRng rng)
    {
        List<string> posture = new List<string>();
        if (snapshot.ExternalPressure >= 0.55)
        {
            posture.Add("Security doctrine prioritizes border resilience and rapid mobilization.");
        }
        else
        {
            posture.Add("Diplomatic posture favours exchange networks over coercive expansion.");
        }

        if (techLevel >= TechnologyLevel.Level.Spacefaring)
        {
            posture.Add("Off-world logistics shape prestige, alliance making, and elite competition.");
        }

        posture.Add("Public ideology is narrated through " + snapshot.LegitimacyFrame + ".");
        posture.Add("Neighbouring societies read the regime as " + DescribeExternalPerception(snapshot, rng) + ".");
        return posture;
    }

    private static string DescribeExternalPerception(CivilizationConceptSnapshot snapshot, SeededRng rng)
    {
        string[] perceptions =
        {
            "disciplined but adaptable",
            "ornate and prestige-driven",
            "technically formidable",
            "ritually anchored",
            "commercially opportunistic",
            "restless at its frontiers",
        };
        if (snapshot.Stability < 0.4)
        {
            return "internally strained despite its public confidence";
        }

        return Pick(perceptions, rng);
    }

    private static void AddDistinct(List<string> values, string value)
    {
        if (!values.Contains(value))
        {
            values.Add(value);
        }
    }

    private static T Pick<T>(IReadOnlyList<T> values, SeededRng rng)
    {
        int index = rng.RandiRange(0, values.Count - 1);
        return values[index];
    }

    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
    }
}
