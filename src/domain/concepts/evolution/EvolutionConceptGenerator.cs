using System.Collections.Generic;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Concepts.Evolution;

/// <summary>
/// Deterministic trait-lineage generator used by the concept atlas.
/// </summary>
public static class EvolutionConceptGenerator
{
    private static readonly string[] NameStarts =
    {
        "Aru", "Bel", "Cer", "Dru", "Ela", "Kor", "Lysa", "Moro", "Sae", "Talu",
    };

    private static readonly string[] NameEnds =
    {
        "th", "rix", "len", "vor", "mii", "syl", "dar", "qun", "esh", "nox",
    };

    /// <summary>
    /// Generates an evolution snapshot from shared concept context.
    /// </summary>
    public static EvolutionConceptSnapshot Generate(ConceptContextSnapshot context)
    {
        SeededRng rng = new(context.Seed ^ 0x77E221F);
        EvolutionConceptSnapshot snapshot = new EvolutionConceptSnapshot();
        snapshot.EnvironmentLabel = ResolveEnvironment(context);
        snapshot.EvolutionSpanMya = 12.0 + rng.RandiRange(0, 180);
        snapshot.BodyPlan = ResolveBodyPlan(context, rng);
        snapshot.Locomotion = ResolveLocomotion(context, rng);
        snapshot.Diet = ResolveDiet(context, rng);
        snapshot.Senses = ResolveSenses(context, rng);
        snapshot.Sociality = ResolveSociality(context, rng);
        snapshot.SpeciesName = BuildSpeciesName(rng);
        snapshot.Traits = BuildTraits(snapshot, context);
        snapshot.LineageMilestones = BuildMilestones(snapshot);
        snapshot.AdaptationFit = ResolveAdaptationFit(context, rng);
        snapshot.DefenseScore = ResolveDefense(snapshot, rng);
        snapshot.MobilityScore = ResolveMobility(snapshot, rng);
        snapshot.CognitionScore = ResolveCognition(snapshot, rng);
        return snapshot;
    }

    private static string ResolveEnvironment(ConceptContextSnapshot context)
    {
        if (context.DominantBiome.Equals("Oceanic", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Shallow-water littoral";
        }

        if (context.DominantBiome.Equals("Desert", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Arid open terrain";
        }

        if (context.DominantBiome.Equals("Forest", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Dense forest canopy";
        }

        if (context.DominantBiome.Equals("Tundra", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Cold seasonal plain";
        }

        return "Mixed temperate biosphere";
    }

    private static string ResolveBodyPlan(ConceptContextSnapshot context, SeededRng rng)
    {
        if (context.GravityG > 1.2)
        {
            return "Low-slung segmented frame";
        }

        if (context.DominantBiome.Equals("Oceanic", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Streamlined bilateral swimmer";
        }

        string[] options =
        {
            "Bilateral vertebrate analogue",
            "Segmented exoskeletal crawler",
            "Flexible hydrostatic hunter",
            "Feathered glider lineage",
        };
        return Pick(options, rng);
    }

    private static string ResolveLocomotion(ConceptContextSnapshot context, SeededRng rng)
    {
        if (context.DominantBiome.Equals("Oceanic", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Undulating swimming and reef maneuvering";
        }

        if (context.DominantBiome.Equals("Forest", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Climbing with short glide descents";
        }

        if (context.DominantBiome.Equals("Desert", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Burrowing sprints and dune traversal";
        }

        string[] options =
        {
            "Cursorial running",
            "Perching and bounded leaps",
            "Crawling with sudden ambush bursts",
        };
        return Pick(options, rng);
    }

    private static string ResolveDiet(ConceptContextSnapshot context, SeededRng rng)
    {
        if (context.WaterAvailability > 0.6)
        {
            return "Opportunistic omnivore with seasonal filter-feeding stages";
        }

        string[] options =
        {
            "Grazing herbivore with occasional scavenging",
            "Predatory ambush feeder",
            "Omnivore exploiting fungi, seeds, and small fauna",
        };
        return Pick(options, rng);
    }

    private static string ResolveSenses(ConceptContextSnapshot context, SeededRng rng)
    {
        if (context.RadiationLevel > 0.45)
        {
            return "Radiation-aware electroreception with deep-shade vision";
        }

        string[] options =
        {
            "Wide-band color vision and vibration sensing",
            "Chemosensory tracking with low-light eyesight",
            "Pressure-wave sensing with acute hearing",
        };
        return Pick(options, rng);
    }

    private static string ResolveSociality(ConceptContextSnapshot context, SeededRng rng)
    {
        if (context.Population > 1000000)
        {
            return "Colony-forming pack behavior with cooperative nesting";
        }

        string[] options =
        {
            "Loose seasonal groups",
            "Pair-bonded territorial family units",
            "Solitary adults with communal juvenile nurseries",
        };
        return Pick(options, rng);
    }

    private static string BuildSpeciesName(SeededRng rng)
    {
        return Pick(NameStarts, rng) + Pick(NameEnds, rng);
    }

    private static List<string> BuildTraits(EvolutionConceptSnapshot snapshot, ConceptContextSnapshot context)
    {
        List<string> traits = new List<string>();
        traits.Add(snapshot.BodyPlan);
        traits.Add(snapshot.Locomotion);
        traits.Add(snapshot.Diet);

        if (context.DominantBiome.Equals("Forest", System.StringComparison.OrdinalIgnoreCase))
        {
            traits.Add("Color-shifting integument");
            traits.Add("Arboreal grip appendages");
        }
        else if (context.DominantBiome.Equals("Oceanic", System.StringComparison.OrdinalIgnoreCase))
        {
            traits.Add("Lateral-line analogue");
            traits.Add("Schooling coordination signals");
        }
        else if (context.DominantBiome.Equals("Desert", System.StringComparison.OrdinalIgnoreCase))
        {
            traits.Add("Desiccation resistance");
            traits.Add("Subsurface heat avoidance");
        }
        else
        {
            traits.Add("Generalist body insulation");
            traits.Add("Flexible reproductive timing");
        }

        traits.Add(snapshot.Senses);
        traits.Add(snapshot.Sociality);
        return traits;
    }

    private static List<string> BuildMilestones(EvolutionConceptSnapshot snapshot)
    {
        List<string> milestones = new List<string>();
        milestones.Add("+" + (snapshot.EvolutionSpanMya * 0.15).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " Mya: baseline multicellular lineage stabilizes.");
        milestones.Add("+" + (snapshot.EvolutionSpanMya * 0.38).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " Mya: " + snapshot.BodyPlan.ToLowerInvariant() + " emerges.");
        milestones.Add("+" + (snapshot.EvolutionSpanMya * 0.61).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " Mya: " + snapshot.Locomotion.ToLowerInvariant() + " becomes dominant.");
        milestones.Add("+" + snapshot.EvolutionSpanMya.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " Mya: " + snapshot.SpeciesName + " profile reaches current niche balance.");
        return milestones;
    }

    private static double ResolveAdaptationFit(ConceptContextSnapshot context, SeededRng rng)
    {
        double score = 0.48 + (context.HabitabilityScore * 0.035) + (context.WaterAvailability * 0.12);
        if (context.RadiationLevel > 0.45)
        {
            score -= 0.12;
        }

        return System.Math.Clamp(score + rng.RandfRange(-0.08f, 0.08f), 0.0, 1.0);
    }

    private static double ResolveDefense(EvolutionConceptSnapshot snapshot, SeededRng rng)
    {
        double score = 0.36;
        if (snapshot.Traits.Exists(trait => trait.Contains("Color-shifting")))
        {
            score += 0.12;
        }

        if (snapshot.Traits.Exists(trait => trait.Contains("Desiccation")))
        {
            score += 0.08;
        }

        return System.Math.Clamp(score + rng.RandfRange(0.0f, 0.22f), 0.0, 1.0);
    }

    private static double ResolveMobility(EvolutionConceptSnapshot snapshot, SeededRng rng)
    {
        double score = 0.42;
        if (snapshot.Locomotion.Contains("swimming"))
        {
            score += 0.20;
        }

        if (snapshot.Locomotion.Contains("glide"))
        {
            score += 0.18;
        }

        if (snapshot.Locomotion.Contains("running"))
        {
            score += 0.15;
        }

        return System.Math.Clamp(score + rng.RandfRange(0.0f, 0.12f), 0.0, 1.0);
    }

    private static double ResolveCognition(EvolutionConceptSnapshot snapshot, SeededRng rng)
    {
        double score = 0.24;
        if (snapshot.Sociality.Contains("cooperative") || snapshot.Sociality.Contains("communal"))
        {
            score += 0.22;
        }

        if (snapshot.Senses.Contains("vision") || snapshot.Senses.Contains("electroreception"))
        {
            score += 0.12;
        }

        return System.Math.Clamp(score + rng.RandfRange(0.0f, 0.18f), 0.0, 1.0);
    }

    private static string Pick(IReadOnlyList<string> values, SeededRng rng)
    {
        return values[rng.RandiRange(0, values.Count - 1)];
    }
}
