using System;
using System.Collections.Generic;
using System.Globalization;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Concepts.Evolution;

/// <summary>
/// Deterministic evolutionary trait-lineage generator used by the concept atlas and runtime concept pipeline.
/// </summary>
public static class EvolutionConceptGenerator
{
    private sealed class Node
    {
        public string Id { get; }
        public string Label { get; }
        public string[] Requirements { get; }

        public Node(string id, string label, params string[] requirements)
        {
            Id = id;
            Label = label;
            Requirements = requirements;
        }
    }

    private sealed class EnvironmentProfile
    {
        public string Key { get; }
        public string Label { get; }
        public Dictionary<string, int> Pressures { get; }

        public EnvironmentProfile(string key, string label, Dictionary<string, int> pressures)
        {
            Key = key;
            Label = label;
            Pressures = pressures;
        }
    }

    private static readonly Node[] Nodes =
    {
        new Node("prokaryote", "prokaryotic cell"),
        new Node("eukaryote", "eukaryotic cell", "prokaryote"),
        new Node("multicellular", "multicellularity", "eukaryote"),
        new Node("cell_diff", "cell differentiation", "multicellular"),
        new Node("bilateral_sym", "bilateral symmetry", "multicellular"),
        new Node("radial_sym", "radial symmetry", "multicellular"),
        new Node("coelom", "coelom", "bilateral_sym"),
        new Node("segmentation", "segmentation", "bilateral_sym"),
        new Node("endoskeleton", "endoskeleton", "coelom"),
        new Node("exoskeleton", "exoskeleton", "coelom"),
        new Node("limbs", "limbs and appendages", "endoskeleton"),
        new Node("flagella", "flagellar swimming", "prokaryote"),
        new Node("undulation", "undulating swimming", "endoskeleton"),
        new Node("walking", "walking and running", "limbs"),
        new Node("burrowing", "burrowing", "limbs"),
        new Node("climbing", "climbing", "walking"),
        new Node("gliding", "gliding", "limbs"),
        new Node("flight", "powered flight", "gliding"),
        new Node("photosynthesis", "photosynthesis", "prokaryote"),
        new Node("filter_feeding", "filter feeding", "multicellular"),
        new Node("herbivory", "herbivory", "cell_diff"),
        new Node("carnivory", "carnivory", "cell_diff"),
        new Node("omnivory", "omnivory", "herbivory", "carnivory"),
        new Node("chemoreception", "chemoreception", "prokaryote"),
        new Node("mechanoreception", "mechanoreception", "eukaryote"),
        new Node("photoreception", "photoreception", "eukaryote"),
        new Node("camera_eye", "camera eyes", "photoreception", "endoskeleton"),
        new Node("compound_eye", "compound eyes", "photoreception", "exoskeleton"),
        new Node("electroreception", "electroreception", "mechanoreception"),
        new Node("echolocation", "echolocation", "mechanoreception"),
        new Node("color_vision", "color vision", "camera_eye"),
        new Node("sexual_repro", "sexual reproduction", "eukaryote"),
        new Node("internal_fert", "internal fertilization", "sexual_repro"),
        new Node("oviparity", "oviparity", "internal_fert"),
        new Node("viviparity", "viviparity", "internal_fert"),
        new Node("parental_care", "parental care", "oviparity"),
        new Node("aerobic_metab", "aerobic metabolism", "prokaryote"),
        new Node("ectothermy", "ectothermy", "aerobic_metab"),
        new Node("endothermy", "endothermy", "aerobic_metab", "endoskeleton"),
        new Node("desiccation_res", "desiccation resistance", "aerobic_metab"),
        new Node("camouflage", "camouflage", "cell_diff"),
        new Node("armor", "armor plates", "exoskeleton"),
        new Node("venom", "venom systems", "carnivory"),
        new Node("regeneration", "regeneration", "cell_diff"),
        new Node("brain", "centralized brain", "mechanoreception", "endoskeleton"),
        new Node("assoc_learning", "associative learning", "brain"),
        new Node("tool_use", "tool use", "assoc_learning", "limbs"),
        new Node("abstract_reason", "abstract reasoning", "tool_use", "brain"),
        new Node("social_living", "social groups", "parental_care"),
        new Node("cooperative_hunt", "cooperative hunting", "social_living", "carnivory"),
        new Node("comm_complex", "complex communication", "social_living", "brain"),
        new Node("language", "symbolic language", "comm_complex", "abstract_reason"),
        new Node("culture", "cultural transmission", "tool_use", "social_living", "language"),
    };

    private static readonly string[] StartingNodes =
    {
        "prokaryote",
        "chemoreception",
        "photosynthesis",
    };

    private static readonly Dictionary<string, string[]> PressureTraits = new Dictionary<string, string[]>
    {
        ["predation"] = new[] { "coelom", "endoskeleton", "camera_eye", "camouflage", "armor", "venom", "cooperative_hunt", "walking", "flight" },
        ["cold"] = new[] { "endothermy", "desiccation_res", "parental_care", "internal_fert" },
        ["dark"] = new[] { "electroreception", "echolocation", "mechanoreception", "color_vision" },
        ["drought"] = new[] { "desiccation_res", "burrowing", "ectothermy" },
        ["competition"] = new[] { "carnivory", "omnivory", "sexual_repro", "internal_fert", "oviparity", "parental_care", "social_living", "brain", "assoc_learning", "comm_complex", "tool_use", "abstract_reason", "language", "culture" },
        ["verticality"] = new[] { "endoskeleton", "limbs", "climbing", "gliding", "flight", "color_vision", "brain" },
        ["aquatic"] = new[] { "flagella", "undulation", "filter_feeding", "electroreception" },
    };

    /// <summary>
    /// Generates an evolution snapshot from shared concept context.
    /// </summary>
    public static EvolutionConceptSnapshot Generate(ConceptContextSnapshot context)
    {
        SeededRng rng = new SeededRng(context.Seed ^ 0x77E221F);
        EnvironmentProfile environment = ResolveEnvironment(context);
        HashSet<string> unlocked = new HashSet<string>(StringComparer.Ordinal);
        foreach (string node in StartingNodes)
        {
            unlocked.Add(node);
        }

        double totalMya = 80.0 + rng.RandiRange(0, 420);
        int steps = 6;
        for (int step = 0; step < steps; step += 1)
        {
            EvolveStep(unlocked, environment, totalMya / steps, rng);
        }

        EvolutionConceptSnapshot snapshot = new EvolutionConceptSnapshot();
        snapshot.SpeciesName = BuildSpeciesName(rng);
        snapshot.EnvironmentLabel = environment.Label;
        snapshot.EvolutionSpanMya = totalMya;
        snapshot.BodyPlan = ResolveBodyPlan(unlocked);
        snapshot.Locomotion = ResolveLocomotion(unlocked);
        snapshot.Diet = ResolveDiet(unlocked);
        snapshot.Senses = ResolveSenses(unlocked);
        snapshot.Sociality = ResolveSociality(unlocked);
        snapshot.Reproduction = ResolveReproduction(unlocked);
        snapshot.Communication = ResolveCommunication(unlocked);
        snapshot.Metabolism = ResolveMetabolism(unlocked);
        snapshot.Integument = ResolveIntegument(unlocked);
        snapshot.UnlockedNodes = new List<string>(unlocked);
        snapshot.UnlockedNodes.Sort(StringComparer.Ordinal);
        snapshot.Traits = BuildTraits(snapshot);
        snapshot.LineageMilestones = BuildMilestones(snapshot, unlocked);
        snapshot.AdaptationFit = ResolveAdaptationFit(context, unlocked, environment);
        snapshot.DefenseScore = ResolveDefense(unlocked);
        snapshot.MobilityScore = ResolveMobility(unlocked);
        snapshot.CognitionScore = ResolveCognition(unlocked);
        return snapshot;
    }

    private static EnvironmentProfile ResolveEnvironment(ConceptContextSnapshot context)
    {
        if (context.DominantBiome.Equals("Oceanic", StringComparison.OrdinalIgnoreCase) || context.DominantBiome.Equals("Reef", StringComparison.OrdinalIgnoreCase))
        {
            return new EnvironmentProfile("ocean_shallow", "Shallow ocean", new Dictionary<string, int> { ["aquatic"] = 5, ["predation"] = 4, ["competition"] = 5, ["dark"] = 1 });
        }
        if (context.DominantBiome.Equals("Forest", StringComparison.OrdinalIgnoreCase))
        {
            return new EnvironmentProfile("forest", "Dense forest canopy", new Dictionary<string, int> { ["predation"] = 4, ["verticality"] = 5, ["competition"] = 5, ["dark"] = 2 });
        }
        if (context.DominantBiome.Equals("Desert", StringComparison.OrdinalIgnoreCase))
        {
            return new EnvironmentProfile("desert", "Arid open terrain", new Dictionary<string, int> { ["drought"] = 5, ["competition"] = 2, ["predation"] = 2, ["cold"] = 1 });
        }
        if (context.DominantBiome.Equals("Tundra", StringComparison.OrdinalIgnoreCase))
        {
            return new EnvironmentProfile("tundra", "Cold seasonal plain", new Dictionary<string, int> { ["cold"] = 5, ["predation"] = 2, ["competition"] = 2, ["dark"] = 1 });
        }
        return new EnvironmentProfile("grassland", "Mixed temperate biosphere", new Dictionary<string, int> { ["predation"] = 5, ["competition"] = 4, ["drought"] = 2 });
    }

    private static void EvolveStep(HashSet<string> unlocked, EnvironmentProfile environment, double mya, SeededRng rng)
    {
        List<Node> unlockable = new List<Node>();
        foreach (Node node in Nodes)
        {
            if (!unlocked.Contains(node.Id) && CanUnlock(node, unlocked))
            {
                unlockable.Add(node);
            }
        }

        int gains = System.Math.Max(1, (int)System.Math.Round((mya / 18.0) + rng.RandfRange(-0.5f, 1.8f)));
        for (int index = 0; index < gains && unlockable.Count > 0; index += 1)
        {
            Node best = unlockable[0];
            double bestScore = -1.0;
            foreach (Node node in unlockable)
            {
                double score = rng.Randf() * 0.2;
                foreach (KeyValuePair<string, string[]> entry in PressureTraits)
                {
                    if (Contains(entry.Value, node.Id) && environment.Pressures.ContainsKey(entry.Key))
                    {
                        score += environment.Pressures[entry.Key] * (0.18 + (rng.Randf() * 0.12));
                    }
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = node;
                }
            }

            unlocked.Add(best.Id);
            unlockable.Remove(best);
            unlockable.RemoveAll(node => !CanUnlock(node, unlocked));
            foreach (Node node in Nodes)
            {
                if (!unlocked.Contains(node.Id) && CanUnlock(node, unlocked) && !unlockable.Contains(node))
                {
                    unlockable.Add(node);
                }
            }
        }
    }

    private static bool CanUnlock(Node node, HashSet<string> unlocked)
    {
        foreach (string requirement in node.Requirements)
        {
            if (!unlocked.Contains(requirement))
            {
                return false;
            }
        }

        return true;
    }

    private static bool Contains(string[] values, string value)
    {
        foreach (string candidate in values)
        {
            if (candidate == value)
            {
                return true;
            }
        }

        return false;
    }

    private static string ResolveBodyPlan(HashSet<string> unlocked)
    {
        if (unlocked.Contains("endoskeleton") && unlocked.Contains("bilateral_sym"))
        {
            return "bilateral vertebrate analogue";
        }
        if (unlocked.Contains("exoskeleton"))
        {
            return "segmented exoskeletal lineage";
        }
        if (unlocked.Contains("radial_sym"))
        {
            return "radial hydrostatic lineage";
        }
        return "simple multicellular body plan";
    }

    private static string ResolveLocomotion(HashSet<string> unlocked)
    {
        if (unlocked.Contains("flight")) { return "powered flight"; }
        if (unlocked.Contains("gliding")) { return "gliding and aerial descent"; }
        if (unlocked.Contains("climbing")) { return "climbing and arboreal movement"; }
        if (unlocked.Contains("walking")) { return "walking and running"; }
        if (unlocked.Contains("undulation")) { return "undulating swimming"; }
        if (unlocked.Contains("burrowing")) { return "burrowing"; }
        return "limited microbial or sessile movement";
    }

    private static string ResolveDiet(HashSet<string> unlocked)
    {
        if (unlocked.Contains("omnivory")) { return "opportunistic omnivore"; }
        if (unlocked.Contains("carnivory")) { return "predatory carnivore"; }
        if (unlocked.Contains("herbivory")) { return "grazing herbivore"; }
        if (unlocked.Contains("filter_feeding")) { return "filter feeder"; }
        return "autotrophic or absorptive feeder";
    }

    private static string ResolveSenses(HashSet<string> unlocked)
    {
        List<string> senses = new List<string>();
        if (unlocked.Contains("color_vision")) { senses.Add("color vision"); }
        if (unlocked.Contains("camera_eye")) { senses.Add("camera eyes"); }
        if (unlocked.Contains("compound_eye")) { senses.Add("compound eyes"); }
        if (unlocked.Contains("electroreception")) { senses.Add("electroreception"); }
        if (unlocked.Contains("echolocation")) { senses.Add("echolocation"); }
        if (senses.Count == 0) { senses.Add("chemoreception"); }
        return string.Join(", ", senses);
    }

    private static string ResolveSociality(HashSet<string> unlocked)
    {
        if (unlocked.Contains("culture")) { return "culture-bearing social groups"; }
        if (unlocked.Contains("social_living")) { return "social groups"; }
        if (unlocked.Contains("cooperative_hunt")) { return "cooperative packs"; }
        return "loosely social or solitary adults";
    }

    private static string ResolveReproduction(HashSet<string> unlocked)
    {
        if (unlocked.Contains("viviparity")) { return "live birth"; }
        if (unlocked.Contains("oviparity")) { return "egg-laying"; }
        if (unlocked.Contains("sexual_repro")) { return "sexual reproduction"; }
        return "asexual division";
    }

    private static string ResolveCommunication(HashSet<string> unlocked)
    {
        if (unlocked.Contains("language")) { return "symbolic language"; }
        if (unlocked.Contains("comm_complex")) { return "complex multi-modal signaling"; }
        return "basic signal exchange";
    }

    private static string ResolveMetabolism(HashSet<string> unlocked)
    {
        if (unlocked.Contains("endothermy")) { return "endothermic"; }
        if (unlocked.Contains("ectothermy")) { return "ectothermic"; }
        return "basic aerobic metabolism";
    }

    private static string ResolveIntegument(HashSet<string> unlocked)
    {
        if (unlocked.Contains("armor")) { return "armored exterior"; }
        if (unlocked.Contains("camouflage")) { return "camouflaged skin"; }
        if (unlocked.Contains("endoskeleton")) { return "soft tissue over internal support"; }
        return "simple membrane";
    }

    private static string BuildSpeciesName(SeededRng rng)
    {
        string[] first = { "Aru", "Bel", "Cer", "Dru", "Ela", "Kor", "Lysa", "Moro", "Sae", "Talu" };
        string[] second = { "th", "rix", "len", "vor", "mii", "syl", "dar", "qun", "esh", "nox" };
        return first[rng.RandiRange(0, first.Length - 1)] + second[rng.RandiRange(0, second.Length - 1)];
    }

    private static List<string> BuildTraits(EvolutionConceptSnapshot snapshot)
    {
        return new List<string>
        {
            snapshot.BodyPlan,
            snapshot.Locomotion,
            snapshot.Diet,
            snapshot.Senses,
            snapshot.Sociality,
            snapshot.Reproduction,
            snapshot.Communication,
            snapshot.Metabolism,
            snapshot.Integument,
        };
    }

    private static List<string> BuildMilestones(EvolutionConceptSnapshot snapshot, HashSet<string> unlocked)
    {
        List<string> milestones = new List<string>();
        milestones.Add("+" + (snapshot.EvolutionSpanMya * 0.18).ToString("0.0", CultureInfo.InvariantCulture) + " Mya: multicellular baseline stabilizes.");
        milestones.Add("+" + (snapshot.EvolutionSpanMya * 0.42).ToString("0.0", CultureInfo.InvariantCulture) + " Mya: " + snapshot.BodyPlan.ToLowerInvariant() + " emerges.");
        milestones.Add("+" + (snapshot.EvolutionSpanMya * 0.68).ToString("0.0", CultureInfo.InvariantCulture) + " Mya: " + snapshot.Locomotion.ToLowerInvariant() + " becomes dominant.");
        if (unlocked.Contains("language"))
        {
            milestones.Add("+" + snapshot.EvolutionSpanMya.ToString("0.0", CultureInfo.InvariantCulture) + " Mya: symbolic communication appears in the lineage.");
        }
        else
        {
            milestones.Add("+" + snapshot.EvolutionSpanMya.ToString("0.0", CultureInfo.InvariantCulture) + " Mya: the lineage reaches its current niche balance.");
        }

        return milestones;
    }

    private static double ResolveAdaptationFit(ConceptContextSnapshot context, HashSet<string> unlocked, EnvironmentProfile environment)
    {
        double score = 0.35 + (context.HabitabilityScore * 0.04);
        if (environment.Pressures.ContainsKey("aquatic") && unlocked.Contains("undulation"))
        {
            score += 0.10;
        }
        if (environment.Pressures.ContainsKey("verticality") && (unlocked.Contains("climbing") || unlocked.Contains("gliding")))
        {
            score += 0.10;
        }
        if (environment.Pressures.ContainsKey("drought") && unlocked.Contains("desiccation_res"))
        {
            score += 0.10;
        }
        return System.Math.Clamp(score, 0.0, 1.0);
    }

    private static double ResolveDefense(HashSet<string> unlocked)
    {
        double score = 0.20;
        if (unlocked.Contains("camouflage")) { score += 0.14; }
        if (unlocked.Contains("armor")) { score += 0.18; }
        if (unlocked.Contains("venom")) { score += 0.12; }
        if (unlocked.Contains("regeneration")) { score += 0.12; }
        return System.Math.Clamp(score, 0.0, 1.0);
    }

    private static double ResolveMobility(HashSet<string> unlocked)
    {
        double score = 0.10;
        if (unlocked.Contains("walking")) { score += 0.20; }
        if (unlocked.Contains("climbing")) { score += 0.18; }
        if (unlocked.Contains("gliding")) { score += 0.16; }
        if (unlocked.Contains("flight")) { score += 0.24; }
        if (unlocked.Contains("undulation")) { score += 0.16; }
        return System.Math.Clamp(score, 0.0, 1.0);
    }

    private static double ResolveCognition(HashSet<string> unlocked)
    {
        double score = 0.08;
        if (unlocked.Contains("brain")) { score += 0.18; }
        if (unlocked.Contains("assoc_learning")) { score += 0.16; }
        if (unlocked.Contains("tool_use")) { score += 0.20; }
        if (unlocked.Contains("abstract_reason")) { score += 0.22; }
        if (unlocked.Contains("language")) { score += 0.18; }
        return System.Math.Clamp(score, 0.0, 1.0);
    }
}
