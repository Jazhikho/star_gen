using System;
using System.Collections.Generic;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Concepts.Disease;

/// <summary>
/// Deterministic outbreak generator used by the concept atlas.
/// </summary>
public static class DiseaseConceptGenerator
{
    private static readonly string[] PathogenTypes =
    {
        "Virus",
        "Bacteria",
        "Fungal spore",
        "Prion",
        "Xenoparasite",
    };

    private static readonly Dictionary<string, string[]> SymptomPool = new Dictionary<string, string[]>
    {
        ["Virus"] = new[] { "Fever", "Cough", "Pneumonia", "Organ failure" },
        ["Bacteria"] = new[] { "Inflammation", "Septic shock", "Meningitis", "Tissue necrosis" },
        ["Fungal spore"] = new[] { "Skin lesions", "Lung colonization", "Vision loss", "Systemic invasion" },
        ["Prion"] = new[] { "Memory gaps", "Ataxia", "Dementia", "Cortical failure" },
        ["Xenoparasite"] = new[] { "Nausea", "Cysts", "Neurological invasion", "Systemic collapse" },
    };

    /// <summary>
    /// Generates an outbreak snapshot from shared concept context.
    /// </summary>
    public static DiseaseConceptSnapshot Generate(ConceptContextSnapshot context)
    {
        SeededRng rng = new(context.Seed ^ 0x2DB62A1);
        DiseaseConceptSnapshot snapshot = new DiseaseConceptSnapshot();
        snapshot.PathogenType = Pick(PathogenTypes, rng);
        snapshot.Infectivity = ResolveInfectivity(context, snapshot.PathogenType, rng);
        snapshot.Severity = ResolveSeverity(context, rng);
        snapshot.Lethality = ResolveLethality(context, rng);
        snapshot.Mutability = Clamp01(0.12 + (context.RadiationLevel * 0.35) + rng.RandfRange(0.0f, 0.22f));
        snapshot.Resilience = Clamp01(0.24 + (context.WaterAvailability * 0.18) + rng.RandfRange(0.0f, 0.25f));
        snapshot.Symptoms = BuildSymptoms(snapshot, rng);
        snapshot.EnvironmentalDrivers = BuildDrivers(context, snapshot);
        RunSimulation(context, snapshot);
        return snapshot;
    }

    private static double ResolveInfectivity(ConceptContextSnapshot context, string pathogenType, SeededRng rng)
    {
        double baseValue = 0.22 + (context.WaterAvailability * 0.18) + (context.Population > 5000000 ? 0.12 : 0.04);
        if (pathogenType == "Virus")
        {
            baseValue += 0.16;
        }
        else if (pathogenType == "Fungal spore")
        {
            baseValue += 0.10;
        }

        return Clamp01(baseValue + rng.RandfRange(0.0f, 0.22f));
    }

    private static double ResolveSeverity(ConceptContextSnapshot context, SeededRng rng)
    {
        double baseValue = 0.18 + (context.RadiationLevel * 0.22);
        if (context.HabitabilityScore <= 4)
        {
            baseValue += 0.14;
        }

        return Clamp01(baseValue + rng.RandfRange(0.0f, 0.25f));
    }

    private static double ResolveLethality(ConceptContextSnapshot context, SeededRng rng)
    {
        double medTechFactor = ResolveMedicalTechnologyFactor(context.TechnologyLevel);
        double baseValue = 0.02 + (context.RadiationLevel * 0.08) + ((1.0 - medTechFactor) * 0.09);
        return Clamp01(baseValue + rng.RandfRange(0.0f, 0.08f));
    }

    private static double ResolveMedicalTechnologyFactor(TechnologyLevel.Level? techLevel)
    {
        if (!techLevel.HasValue)
        {
            return 0.25;
        }

        return techLevel.Value switch
        {
            TechnologyLevel.Level.StoneAge => 0.05,
            TechnologyLevel.Level.BronzeAge => 0.08,
            TechnologyLevel.Level.IronAge => 0.10,
            TechnologyLevel.Level.Classical => 0.14,
            TechnologyLevel.Level.Medieval => 0.16,
            TechnologyLevel.Level.Renaissance => 0.22,
            TechnologyLevel.Level.Industrial => 0.34,
            TechnologyLevel.Level.Atomic => 0.48,
            TechnologyLevel.Level.Information => 0.64,
            TechnologyLevel.Level.Spacefaring => 0.72,
            TechnologyLevel.Level.Interstellar => 0.80,
            TechnologyLevel.Level.Advanced => 0.88,
            _ => 0.25,
        };
    }

    private static List<string> BuildSymptoms(DiseaseConceptSnapshot snapshot, SeededRng rng)
    {
        List<string> symptoms = new List<string>();
        string[] pool = SymptomPool[snapshot.PathogenType];
        foreach (string symptom in pool)
        {
            bool include = false;
            if (symptoms.Count < 2)
            {
                include = true;
            }
            else if (snapshot.Severity > 0.45 && rng.Randf() < 0.6f)
            {
                include = true;
            }
            else if (snapshot.Lethality > 0.08 && rng.Randf() < 0.45f)
            {
                include = true;
            }

            if (include && !symptoms.Contains(symptom))
            {
                symptoms.Add(symptom);
            }
        }

        return symptoms;
    }

    private static List<string> BuildDrivers(ConceptContextSnapshot context, DiseaseConceptSnapshot snapshot)
    {
        List<string> drivers = new List<string>();
        if (context.Population > 5000000)
        {
            drivers.Add("Dense settlement networks accelerate transmission.");
        }
        else
        {
            drivers.Add("Sparse settlement patterns slow some transmission chains.");
        }

        if (context.RadiationLevel > 0.4)
        {
            drivers.Add("Elevated radiation increases mutation pressure.");
        }

        if (context.WaterAvailability > 0.6)
        {
            drivers.Add("Abundant water and humidity improve pathogen persistence.");
        }

        if (context.HabitabilityScore <= 4)
        {
            drivers.Add("Harsh conditions weaken public resilience and logistics.");
        }

        if (drivers.Count == 0)
        {
            drivers.Add("Moderate environmental conditions keep the outbreak locally manageable.");
        }

        if (snapshot.PathogenType == "Prion")
        {
            drivers.Add("Slow incubation hides spread until severe symptoms appear.");
        }

        return drivers;
    }

    private static void RunSimulation(ConceptContextSnapshot context, DiseaseConceptSnapshot snapshot)
    {
        int population = context.Population;
        if (population <= 0)
        {
            population = 1000000;
        }

        double medTech = ResolveMedicalTechnologyFactor(context.TechnologyLevel);
        double densityFactor = population > 10000000 ? 1.25 : population > 1000000 ? 1.08 : 0.92;
        double beta = snapshot.Infectivity * 0.42 * densityFactor * (1.0 - (medTech * 0.25));
        double gamma = 0.05 + ((1.0 - snapshot.Severity) * 0.08) + (medTech * 0.06);
        double mu = snapshot.Lethality * (0.03 + (snapshot.Severity * 0.09)) * (1.0 - (medTech * 0.45));

        double susceptible = population - System.Math.Max(10, population * 0.0002);
        double infected = System.Math.Max(10, population * 0.0002);
        double recovered = 0.0;
        double dead = 0.0;
        double totalInfected = infected;
        double peak = infected;
        int peakDay = 0;

        for (int day = 0; day < 240; day += 1)
        {
            double livingPopulation = susceptible + infected + recovered;
            if (livingPopulation <= 1.0)
            {
                break;
            }

            double newInfections = beta * infected * susceptible / livingPopulation;
            double recoveries = gamma * infected;
            double deaths = mu * infected;

            susceptible = System.Math.Max(0.0, susceptible - newInfections);
            infected = System.Math.Max(0.0, infected + newInfections - recoveries - deaths);
            recovered += recoveries;
            dead += deaths;
            totalInfected += newInfections;

            if (infected > peak)
            {
                peak = infected;
                peakDay = day + 1;
            }
        }

        snapshot.TotalInfected = (int)System.Math.Round(totalInfected);
        snapshot.TotalDeaths = (int)System.Math.Round(dead);
        snapshot.PeakInfected = (int)System.Math.Round(peak);
        snapshot.PeakDay = peakDay;
    }

    private static string Pick(IReadOnlyList<string> values, SeededRng rng)
    {
        return values[rng.RandiRange(0, values.Count - 1)];
    }

    private static double Clamp01(double value)
    {
        return System.Math.Clamp(value, 0.0, 1.0);
    }
}
