using System.Collections.Generic;
using StarGen.Domain.Population;

namespace StarGen.Domain.Concepts.Disease;

internal sealed class DiseasePathogenKind
{
    public string Id { get; }

    public string DisplayName { get; }

    public TechnologyLevel.Level? MinimumTech { get; }

    public DiseasePathogenKind(string id, string displayName, TechnologyLevel.Level? minimumTech)
    {
        Id = id;
        DisplayName = displayName;
        MinimumTech = minimumTech;
    }
}

internal sealed class DiseaseSymptomDescriptor
{
    public string Name { get; }

    public int SeverityBand { get; }

    public string TraitKey { get; }

    public double Threshold { get; }

    public string Phase { get; }

    public DiseaseSymptomDescriptor(string name, int severityBand, string traitKey, double threshold, string phase)
    {
        Name = name;
        SeverityBand = severityBand;
        TraitKey = traitKey;
        Threshold = threshold;
        Phase = phase;
    }
}

internal static class DiseaseConceptData
{
    public static readonly DiseasePathogenKind[] PathogenKinds =
    {
        new DiseasePathogenKind("virus", "Virus", null),
        new DiseasePathogenKind("bacteria", "Bacteria", null),
        new DiseasePathogenKind("fungal", "Fungal Spore", null),
        new DiseasePathogenKind("prion", "Prion", null),
        new DiseasePathogenKind("nanobot", "Nanobot Swarm", TechnologyLevel.Level.Information),
        new DiseasePathogenKind("parasite", "Xenoparasite", null),
    };

    public static readonly Dictionary<string, List<DiseaseSymptomDescriptor>> SymptomPools = new Dictionary<string, List<DiseaseSymptomDescriptor>>
    {
        ["virus"] = new List<DiseaseSymptomDescriptor>
        {
            new DiseaseSymptomDescriptor("Fever", 1, "severity", 0.10, "early"),
            new DiseaseSymptomDescriptor("Cough", 1, "airborne", 0.20, "early"),
            new DiseaseSymptomDescriptor("Headache", 1, "severity", 0.15, "early"),
            new DiseaseSymptomDescriptor("Pneumonia", 2, "airborne", 0.40, "mid"),
            new DiseaseSymptomDescriptor("High fever", 2, "severity", 0.35, "mid"),
            new DiseaseSymptomDescriptor("Organ failure", 3, "lethality", 0.30, "late"),
            new DiseaseSymptomDescriptor("Cytokine storm", 3, "immuneEvasion", 0.50, "late"),
        },
        ["bacteria"] = new List<DiseaseSymptomDescriptor>
        {
            new DiseaseSymptomDescriptor("Inflammation", 1, "severity", 0.10, "early"),
            new DiseaseSymptomDescriptor("Fever", 1, "severity", 0.15, "early"),
            new DiseaseSymptomDescriptor("Septic shock", 2, "severity", 0.45, "mid"),
            new DiseaseSymptomDescriptor("Meningitis", 2, "severity", 0.50, "mid"),
            new DiseaseSymptomDescriptor("Gangrene", 3, "lethality", 0.35, "late"),
            new DiseaseSymptomDescriptor("Multi-organ sepsis", 3, "lethality", 0.50, "late"),
        },
        ["fungal"] = new List<DiseaseSymptomDescriptor>
        {
            new DiseaseSymptomDescriptor("Skin lesions", 1, "infectivity", 0.10, "early"),
            new DiseaseSymptomDescriptor("Cough", 1, "airborne", 0.15, "early"),
            new DiseaseSymptomDescriptor("Lung colonization", 2, "airborne", 0.35, "mid"),
            new DiseaseSymptomDescriptor("Vision loss", 2, "severity", 0.40, "mid"),
            new DiseaseSymptomDescriptor("Brain fungemia", 3, "immuneEvasion", 0.40, "late"),
            new DiseaseSymptomDescriptor("Systemic failure", 3, "lethality", 0.40, "late"),
        },
        ["prion"] = new List<DiseaseSymptomDescriptor>
        {
            new DiseaseSymptomDescriptor("Insomnia", 1, "severity", 0.10, "early"),
            new DiseaseSymptomDescriptor("Memory gaps", 1, "severity", 0.20, "early"),
            new DiseaseSymptomDescriptor("Dementia", 2, "severity", 0.40, "mid"),
            new DiseaseSymptomDescriptor("Ataxia", 2, "severity", 0.50, "mid"),
            new DiseaseSymptomDescriptor("Brain sponging", 3, "lethality", 0.40, "late"),
            new DiseaseSymptomDescriptor("Cortical death", 3, "lethality", 0.60, "late"),
        },
        ["nanobot"] = new List<DiseaseSymptomDescriptor>
        {
            new DiseaseSymptomDescriptor("Metallic taste", 1, "infectivity", 0.10, "early"),
            new DiseaseSymptomDescriptor("Micro-tremors", 1, "severity", 0.20, "early"),
            new DiseaseSymptomDescriptor("Neural override", 2, "immuneEvasion", 0.40, "mid"),
            new DiseaseSymptomDescriptor("Tissue rewrite", 2, "mutability", 0.50, "mid"),
            new DiseaseSymptomDescriptor("Full conversion", 3, "lethality", 0.40, "late"),
            new DiseaseSymptomDescriptor("System collapse", 3, "lethality", 0.50, "late"),
        },
        ["parasite"] = new List<DiseaseSymptomDescriptor>
        {
            new DiseaseSymptomDescriptor("Nausea", 1, "severity", 0.15, "early"),
            new DiseaseSymptomDescriptor("Abdominal pain", 1, "severity", 0.15, "early"),
            new DiseaseSymptomDescriptor("Organ cysts", 2, "resilience", 0.40, "mid"),
            new DiseaseSymptomDescriptor("Anemia", 2, "severity", 0.40, "mid"),
            new DiseaseSymptomDescriptor("Brain parasites", 3, "immuneEvasion", 0.50, "late"),
            new DiseaseSymptomDescriptor("Systemic failure", 3, "lethality", 0.50, "late"),
        },
    };
}
