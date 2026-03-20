using System;
using System.Collections.Generic;
using System.Globalization;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Concepts.Civilization;

/// <summary>
/// Deterministic civilisation generator used by the concept atlas and runtime concept pipeline.
/// </summary>
public static class CivilizationConceptGenerator
{
    private sealed class RegimeProfile
    {
        public GovernmentType.Regime Regime { get; }
        public int MinTechIndex { get; }
        public int MaxTechIndex { get; }
        public double Centralization { get; }
        public double Inclusiveness { get; }
        public double Capacity { get; }

        public RegimeProfile(GovernmentType.Regime regime, int minTechIndex, int maxTechIndex, double centralization, double inclusiveness, double capacity)
        {
            Regime = regime;
            MinTechIndex = minTechIndex;
            MaxTechIndex = maxTechIndex;
            Centralization = centralization;
            Inclusiveness = inclusiveness;
            Capacity = capacity;
        }
    }

    private static readonly RegimeProfile[] RegimeProfiles =
    {
        new RegimeProfile(GovernmentType.Regime.Tribal, 0, 2, 0.22, 0.54, 0.20),
        new RegimeProfile(GovernmentType.Regime.Chiefdom, 0, 3, 0.34, 0.44, 0.24),
        new RegimeProfile(GovernmentType.Regime.CityState, 1, 5, 0.44, 0.52, 0.34),
        new RegimeProfile(GovernmentType.Regime.Feudal, 2, 5, 0.50, 0.20, 0.28),
        new RegimeProfile(GovernmentType.Regime.PatrimonialKingdom, 2, 7, 0.62, 0.24, 0.42),
        new RegimeProfile(GovernmentType.Regime.BureaucraticEmpire, 3, 8, 0.82, 0.16, 0.72),
        new RegimeProfile(GovernmentType.Regime.AbsoluteMonarchy, 3, 8, 0.74, 0.18, 0.58),
        new RegimeProfile(GovernmentType.Regime.Constitutional, 5, 11, 0.58, 0.72, 0.62),
        new RegimeProfile(GovernmentType.Regime.Oligarchic, 4, 11, 0.64, 0.28, 0.56),
        new RegimeProfile(GovernmentType.Regime.EliteRepublic, 4, 11, 0.52, 0.42, 0.52),
        new RegimeProfile(GovernmentType.Regime.MassDemocracy, 6, 11, 0.54, 0.82, 0.68),
        new RegimeProfile(GovernmentType.Regime.OnePartyState, 6, 11, 0.78, 0.24, 0.70),
        new RegimeProfile(GovernmentType.Regime.MilitaryJunta, 4, 11, 0.76, 0.16, 0.48),
        new RegimeProfile(GovernmentType.Regime.PersonalistDict, 4, 11, 0.74, 0.14, 0.42),
        new RegimeProfile(GovernmentType.Regime.FailedState, 0, 11, 0.18, 0.12, 0.08),
        new RegimeProfile(GovernmentType.Regime.Corporate, 6, 11, 0.68, 0.30, 0.74),
        new RegimeProfile(GovernmentType.Regime.Theocracy, 2, 8, 0.70, 0.22, 0.46),
        new RegimeProfile(GovernmentType.Regime.Technocracy, 6, 11, 0.72, 0.34, 0.84),
    };

    private static readonly Dictionary<TechnologyLevel.Level, List<string>> TechnologiesByEra = new Dictionary<TechnologyLevel.Level, List<string>>
    {
        [TechnologyLevel.Level.StoneAge] = new List<string> { "fire control", "foraging memory", "ritual language" },
        [TechnologyLevel.Level.BronzeAge] = new List<string> { "crop cultivation", "pottery", "gift exchange", "boat building" },
        [TechnologyLevel.Level.IronAge] = new List<string> { "iron tools", "roads", "coinage", "codified law" },
        [TechnologyLevel.Level.Classical] = new List<string> { "astronomy", "medicine", "engineering", "rhetoric" },
        [TechnologyLevel.Level.Medieval] = new List<string> { "mills", "printing", "algebra", "siege craft" },
        [TechnologyLevel.Level.Renaissance] = new List<string> { "navigation", "optics", "accounting", "gunpowder logistics" },
        [TechnologyLevel.Level.Industrial] = new List<string> { "steam power", "railways", "germ theory", "industrial chemistry" },
        [TechnologyLevel.Level.Atomic] = new List<string> { "electrical grids", "radio", "pharmaceuticals", "mass media" },
        [TechnologyLevel.Level.Information] = new List<string> { "microelectronics", "global networks", "automation", "genomics" },
        [TechnologyLevel.Level.Spacefaring] = new List<string> { "launch systems", "life support", "orbital industry", "satellite governance" },
        [TechnologyLevel.Level.Interstellar] = new List<string> { "fusion", "quantum computing", "synthetic biology", "interstellar law" },
        [TechnologyLevel.Level.Advanced] = new List<string> { "nanotechnology", "post-scarcity fabrication", "general AI", "planetary engineering" },
    };

    private static readonly string[] LegitimacyFrames =
    {
        "ancestral continuity",
        "sacred mandate",
        "civic representation",
        "technocratic competence",
        "martial protection",
        "commercial prosperity",
        "ecological stewardship",
    };

    private static readonly string[] ValuePool =
    {
        "ritual continuity",
        "mercantile pragmatism",
        "frontier resilience",
        "collective duty",
        "scholarly prestige",
        "martial honour",
        "civic debate",
        "kinship patronage",
        "technocratic stewardship",
        "ceremonial legitimacy",
        "ecological reciprocity",
        "expansionist ambition",
    };

    /// <summary>
    /// Generates a civilisation snapshot from shared concept context.
    /// </summary>
    public static CivilizationConceptSnapshot Generate(ConceptContextSnapshot context)
    {
        SeededRng rng = new SeededRng(context.Seed ^ 0x63A1D29);
        TechnologyLevel.Level techLevel = ResolveTechLevel(context, rng);
        RegimeProfile regime = ResolveRegimeProfile(context, techLevel, rng);
        CivilizationConceptSnapshot snapshot = new CivilizationConceptSnapshot();
        snapshot.PolityName = ResolvePolityName(context, regime.Regime, rng);
        snapshot.RegimeName = GovernmentType.ToStringName(regime.Regime);
        snapshot.TechEra = TechnologyLevel.ToStringName(techLevel);
        snapshot.LegitimacyFrame = Pick(LegitimacyFrames, rng);
        snapshot.CoreTerrain = ResolveCoreTerrain(context);
        snapshot.Centralization = Clamp01(regime.Centralization + RandomRange(rng, -0.08, 0.08));
        snapshot.Inclusiveness = Clamp01(regime.Inclusiveness + RandomRange(rng, -0.08, 0.08));
        snapshot.AdministrativeCapacity = Clamp01(regime.Capacity + ResolveTechModifier(techLevel, 0.18) + RandomRange(rng, -0.05, 0.05));
        snapshot.ExternalPressure = ResolveExternalPressure(context, rng);
        snapshot.Innovation = ResolveInnovation(context, techLevel, rng);
        snapshot.Stability = ResolveStability(snapshot, rng);
        snapshot.EconomySectors = BuildEconomy(context, techLevel, rng);
        snapshot.CulturalValues = BuildValues(regime.Regime, context, rng);
        snapshot.KeyTechnologies = BuildTechnologyHighlights(techLevel, rng);
        snapshot.HistoricalMilestones = BuildMilestones(snapshot, regime.Regime, techLevel, context, rng);
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

        int index = rng.RandiRange((int)TechnologyLevel.Level.BronzeAge, (int)TechnologyLevel.Level.IronAge);
        return (TechnologyLevel.Level)index;
    }

    private static RegimeProfile ResolveRegimeProfile(ConceptContextSnapshot context, TechnologyLevel.Level techLevel, SeededRng rng)
    {
        int techIndex = (int)techLevel;
        if (context.Regime.HasValue)
        {
            foreach (RegimeProfile profile in RegimeProfiles)
            {
                if (profile.Regime == context.Regime.Value)
                {
                    return profile;
                }
            }
        }

        List<RegimeProfile> valid = new List<RegimeProfile>();
        foreach (RegimeProfile profile in RegimeProfiles)
        {
            if (techIndex >= profile.MinTechIndex && techIndex <= profile.MaxTechIndex)
            {
                valid.Add(profile);
            }
        }

        if (valid.Count == 0)
        {
            throw new InvalidOperationException("No valid civilisation regimes for technology level " + techLevel + ".");
        }

        List<RegimeProfile> weighted = new List<RegimeProfile>();
        foreach (RegimeProfile profile in valid)
        {
            int weight = 1;
            if (context.DominantBiome.Equals("Oceanic", StringComparison.OrdinalIgnoreCase) && profile.Regime == GovernmentType.Regime.CityState)
            {
                weight = 4;
            }
            else if (context.DominantBiome.Equals("Desert", StringComparison.OrdinalIgnoreCase) && profile.Regime == GovernmentType.Regime.PatrimonialKingdom)
            {
                weight = 4;
            }
            else if (techIndex >= (int)TechnologyLevel.Level.Information && (profile.Regime == GovernmentType.Regime.MassDemocracy || profile.Regime == GovernmentType.Regime.Technocracy || profile.Regime == GovernmentType.Regime.Corporate))
            {
                weight = 4;
            }
            else if (techIndex <= (int)TechnologyLevel.Level.IronAge && (profile.Regime == GovernmentType.Regime.Tribal || profile.Regime == GovernmentType.Regime.Chiefdom))
            {
                weight = 4;
            }

            for (int index = 0; index < weight; index += 1)
            {
                weighted.Add(profile);
            }
        }

        return weighted[rng.RandiRange(0, weighted.Count - 1)];
    }

    private static string ResolvePolityName(ConceptContextSnapshot context, GovernmentType.Regime regime, SeededRng rng)
    {
        string baseName = context.BodyName;
        if (string.IsNullOrWhiteSpace(baseName))
        {
            string[] roots = { "Aster", "Caldris", "Meru", "Thessa", "Varun", "Lyr", "Nemor", "Saur" };
            baseName = roots[rng.RandiRange(0, roots.Length - 1)];
        }

        string suffix = "Polity";
        if (regime == GovernmentType.Regime.MassDemocracy)
        {
            suffix = "Assembly";
        }
        else if (regime == GovernmentType.Regime.Constitutional)
        {
            suffix = "Commonwealth";
        }
        else if (regime == GovernmentType.Regime.Corporate)
        {
            suffix = "Combine";
        }
        else if (regime == GovernmentType.Regime.Technocracy)
        {
            suffix = "Directorate";
        }
        else if (regime == GovernmentType.Regime.Theocracy)
        {
            suffix = "Sacral State";
        }
        else if (regime == GovernmentType.Regime.MilitaryJunta)
        {
            suffix = "Command";
        }
        else if (regime == GovernmentType.Regime.BureaucraticEmpire)
        {
            suffix = "Imperium";
        }

        return baseName + " " + suffix;
    }

    private static string ResolveCoreTerrain(ConceptContextSnapshot context)
    {
        if (context.DominantBiome.Equals("Oceanic", StringComparison.OrdinalIgnoreCase))
        {
            return "coastal and riverine";
        }

        if (context.DominantBiome.Equals("Desert", StringComparison.OrdinalIgnoreCase))
        {
            return "arid caravan corridors";
        }

        if (context.DominantBiome.Equals("Forest", StringComparison.OrdinalIgnoreCase))
        {
            return "forested interior";
        }

        if (context.DominantBiome.Equals("Tundra", StringComparison.OrdinalIgnoreCase))
        {
            return "cold seasonal plain";
        }

        return "mixed temperate terrain";
    }

    private static double ResolveInnovation(ConceptContextSnapshot context, TechnologyLevel.Level techLevel, SeededRng rng)
    {
        double value = 0.22 + ResolveTechModifier(techLevel, 0.55);
        if (context.Population > 10000000)
        {
            value += 0.08;
        }

        if (context.DominantBiome.Equals("Desert", StringComparison.OrdinalIgnoreCase))
        {
            value += 0.04;
        }

        return Clamp01(value + RandomRange(rng, -0.06, 0.06));
    }

    private static double ResolveExternalPressure(ConceptContextSnapshot context, SeededRng rng)
    {
        double pressure = 0.18;
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

        return Clamp01(pressure + RandomRange(rng, -0.06, 0.14));
    }

    private static double ResolveStability(CivilizationConceptSnapshot snapshot, SeededRng rng)
    {
        double score = 0.42 + (snapshot.Centralization * 0.14) + (snapshot.Inclusiveness * 0.16) + (snapshot.AdministrativeCapacity * 0.18) + (snapshot.Innovation * 0.10) - (snapshot.ExternalPressure * 0.30);
        return Clamp01(score + RandomRange(rng, -0.06, 0.06));
    }

    private static List<string> BuildEconomy(ConceptContextSnapshot context, TechnologyLevel.Level techLevel, SeededRng rng)
    {
        List<string> sectors = new List<string>();
        if (context.DominantBiome.Equals("Oceanic", StringComparison.OrdinalIgnoreCase))
        {
            sectors.Add("maritime trade");
            sectors.Add("shipbuilding");
        }
        else if (context.DominantBiome.Equals("Desert", StringComparison.OrdinalIgnoreCase))
        {
            sectors.Add("caravan exchange");
            sectors.Add("irrigated agriculture");
        }
        else if (context.DominantBiome.Equals("Forest", StringComparison.OrdinalIgnoreCase))
        {
            sectors.Add("forest extraction");
            sectors.Add("river transport");
        }
        else
        {
            sectors.Add("agriculture");
            sectors.Add("regional trade");
        }

        if (techLevel >= TechnologyLevel.Level.Industrial)
        {
            sectors.Add("heavy industry");
        }

        if (techLevel >= TechnologyLevel.Level.Information)
        {
            sectors.Add("data services");
        }

        if (techLevel >= TechnologyLevel.Level.Spacefaring)
        {
            sectors.Add("orbital infrastructure");
        }

        if (rng.Randf() < 0.35f)
        {
            sectors.Add("prestige exchange");
        }

        return sectors;
    }

    private static List<string> BuildValues(GovernmentType.Regime regime, ConceptContextSnapshot context, SeededRng rng)
    {
        List<string> values = new List<string>();
        if (regime == GovernmentType.Regime.Theocracy)
        {
            values.Add("ceremonial legitimacy");
        }
        if (regime == GovernmentType.Regime.Corporate)
        {
            values.Add("mercantile pragmatism");
        }
        if (context.DominantBiome.Equals("Forest", StringComparison.OrdinalIgnoreCase))
        {
            values.Add("ecological reciprocity");
        }

        while (values.Count < 3)
        {
            string candidate = ValuePool[rng.RandiRange(0, ValuePool.Length - 1)];
            if (!values.Contains(candidate))
            {
                values.Add(candidate);
            }
        }

        return values;
    }

    private static List<string> BuildTechnologyHighlights(TechnologyLevel.Level techLevel, SeededRng rng)
    {
        List<string> pool = TechnologiesByEra[techLevel];
        List<string> highlights = new List<string>();
        while (highlights.Count < 3 && highlights.Count < pool.Count)
        {
            string candidate = pool[rng.RandiRange(0, pool.Count - 1)];
            if (!highlights.Contains(candidate))
            {
                highlights.Add(candidate);
            }
        }

        return highlights;
    }

    private static List<string> BuildMilestones(CivilizationConceptSnapshot snapshot, GovernmentType.Regime regime, TechnologyLevel.Level techLevel, ConceptContextSnapshot context, SeededRng rng)
    {
        List<string> milestones = new List<string>();
        milestones.Add("Founding era: authority consolidated around " + snapshot.LegitimacyFrame + ".");
        milestones.Add("Administrative buildout: " + snapshot.CoreTerrain + " required new coordination and record keeping.");

        if (techLevel >= TechnologyLevel.Level.Classical)
        {
            milestones.Add("Institutional era: " + snapshot.RegimeName + " codified law, taxation, and social obligations.");
        }

        if (snapshot.Stability < 0.45)
        {
            milestones.Add("Crisis cycle: repeated shocks created a habit of defensive reform and guarded succession.");
        }
        else
        {
            milestones.Add("Consolidation phase: stable rule allowed infrastructure and cultural memory to accumulate.");
        }

        if (techLevel >= TechnologyLevel.Level.Industrial)
        {
            milestones.Add("Acceleration era: " + snapshot.KeyTechnologies[0] + " reshaped labor, communication, and elite competition.");
        }

        if (GovernmentType.IsAuthoritarian(regime))
        {
            milestones.Add("Legitimacy remains tied to hierarchy, order, and managed dissent.");
        }
        else
        {
            milestones.Add("Legitimacy remains tied to negotiation, participation, and public consent rituals.");
        }

        if (context.Population > 25000000 && rng.Randf() < 0.60f)
        {
            milestones.Add("Metropolitan phase: scale forced new bargains between center, frontier, and specialist classes.");
        }

        return milestones;
    }

    private static List<string> BuildExternalPosture(CivilizationConceptSnapshot snapshot, TechnologyLevel.Level techLevel, SeededRng rng)
    {
        List<string> posture = new List<string>();
        if (snapshot.ExternalPressure >= 0.55)
        {
            posture.Add("Security doctrine prioritizes border resilience and rapid mobilization.");
        }
        else
        {
            posture.Add("Diplomatic posture favors exchange networks over coercive expansion.");
        }

        if (techLevel >= TechnologyLevel.Level.Spacefaring)
        {
            posture.Add("Off-world logistics shape prestige, alliance making, and elite competition.");
        }

        if (snapshot.Innovation >= 0.60)
        {
            posture.Add("Neighbors perceive the polity as technically adaptable and institutionally ambitious.");
        }
        else
        {
            posture.Add("Neighbors read the polity as tradition-bound but regionally persistent.");
        }

        posture.Add("Public ideology is narrated through " + snapshot.LegitimacyFrame + ".");
        return posture;
    }

    private static double ResolveTechModifier(TechnologyLevel.Level techLevel, double scale)
    {
        return ((int)techLevel / 11.0) * scale;
    }

    private static T Pick<T>(IReadOnlyList<T> values, SeededRng rng)
    {
        return values[rng.RandiRange(0, values.Count - 1)];
    }

    private static double RandomRange(SeededRng rng, double min, double max)
    {
        return min + ((max - min) * rng.Randf());
    }

    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
    }
}
