using System;
using System.Collections.Generic;
using System.Globalization;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Concepts.Disease;

/// <summary>
/// Deterministic outbreak generator used by the concept atlas and runtime concept pipeline.
/// </summary>
public static class DiseaseConceptGenerator
{
    private sealed class Traits
    {
        public double Infectivity { get; set; }
        public double Severity { get; set; }
        public double Lethality { get; set; }
        public double Mutability { get; set; }
        public double Resilience { get; set; }
        public double Incubation { get; set; }
        public double Airborne { get; set; }
        public double ImmuneEvasion { get; set; }
    }

    /// <summary>
    /// Generates an outbreak snapshot from shared concept context.
    /// </summary>
    public static DiseaseConceptSnapshot Generate(ConceptContextSnapshot context)
    {
        SeededRng rng = new SeededRng(context.Seed ^ 0x2DB62A1);
        string pathogenId = ResolvePathogenId(context, rng);
        Traits traits = GenerateTraits(context, pathogenId, rng);
        int hostPopulation = ResolvePopulationSize(context);
        List<string> symptoms = BuildSymptoms(pathogenId, traits, rng);
        List<string> mutationEvents = new List<string>();
        RunSimulation(context, pathogenId, traits, hostPopulation, symptoms, mutationEvents, rng, out int totalInfected, out int totalDeaths, out int peakInfected, out int peakDay);

        DiseaseConceptSnapshot snapshot = new DiseaseConceptSnapshot();
        snapshot.PathogenType = ResolvePathogenName(pathogenId);
        snapshot.HostEnvironment = ResolveHostEnvironment(context);
        snapshot.Infectivity = traits.Infectivity;
        snapshot.Severity = traits.Severity;
        snapshot.Lethality = traits.Lethality;
        snapshot.Mutability = traits.Mutability;
        snapshot.Resilience = traits.Resilience;
        snapshot.Incubation = traits.Incubation;
        snapshot.Airborne = traits.Airborne;
        snapshot.ImmuneEvasion = traits.ImmuneEvasion;
        snapshot.PopulationSize = hostPopulation;
        snapshot.Symptoms = symptoms;
        snapshot.TotalInfected = totalInfected;
        snapshot.TotalDeaths = totalDeaths;
        snapshot.PeakInfected = peakInfected;
        snapshot.PeakDay = peakDay;
        snapshot.EnvironmentalDrivers = BuildDrivers(context, pathogenId, hostPopulation);
        snapshot.MutationEvents = mutationEvents;
        return snapshot;
    }

    private static string ResolvePathogenId(ConceptContextSnapshot context, SeededRng rng)
    {
        List<string> options = new List<string> { "virus", "bacteria", "fungal", "prion", "parasite" };
        if (context.TechnologyLevel.HasValue && context.TechnologyLevel.Value >= TechnologyLevel.Level.Information)
        {
            options.Add("nanobot");
        }

        if (context.DominantBiome.Equals("Forest", StringComparison.OrdinalIgnoreCase) || context.DominantBiome.Equals("Wetland", StringComparison.OrdinalIgnoreCase))
        {
            if (rng.Randf() < 0.25f)
            {
                return "parasite";
            }
        }

        if (context.DominantBiome.Equals("Oceanic", StringComparison.OrdinalIgnoreCase) && rng.Randf() < 0.20f)
        {
            return "fungal";
        }

        return options[rng.RandiRange(0, options.Count - 1)];
    }

    private static string ResolvePathogenName(string pathogenId)
    {
        foreach (DiseasePathogenKind pathogen in DiseaseConceptData.PathogenKinds)
        {
            if (pathogen.Id == pathogenId)
            {
                return pathogen.DisplayName;
            }
        }

        throw new InvalidOperationException("Unknown pathogen id '" + pathogenId + "'.");
    }

    private static Traits GenerateTraits(ConceptContextSnapshot context, string pathogenId, SeededRng rng)
    {
        double humidity = Clamp01(context.WaterAvailability);
        double radiation = Clamp01(context.RadiationLevel);
        double temperature = Clamp01((context.AvgTemperatureK - 180.0) / 180.0);
        double medTech = ResolveMedicalCapability(context.TechnologyLevel);
        double hygiene = ResolveHygiene(context);
        double density = ResolvePopulationDensity(context);
        Traits traits = new Traits();

        if (pathogenId == "virus")
        {
            traits.Infectivity = Clamp01(RandomRange(rng, 0.30, 0.70) + humidity * 0.20 + density * 0.15);
            traits.Severity = Clamp01(RandomRange(rng, 0.10, 0.50) + radiation * 0.20);
            traits.Lethality = Clamp01(RandomRange(rng, 0.01, 0.15) + radiation * 0.15 - medTech * 0.10);
            traits.Mutability = Clamp01(RandomRange(rng, 0.20, 0.60) + radiation * 0.20);
            traits.Resilience = Clamp01(RandomRange(rng, 0.10, 0.40) + temperature * 0.15);
            traits.Incubation = Clamp01(RandomRange(rng, 0.30, 0.70) - temperature * 0.10);
            traits.Airborne = Clamp01(RandomRange(rng, 0.20, 0.80) + humidity * 0.10 - temperature * 0.05);
            traits.ImmuneEvasion = Clamp01(RandomRange(rng, 0.05, 0.30) + radiation * 0.15);
        }
        else if (pathogenId == "bacteria")
        {
            traits.Infectivity = Clamp01(RandomRange(rng, 0.20, 0.60) + humidity * 0.25 + (1.0 - hygiene) * 0.20);
            traits.Severity = Clamp01(RandomRange(rng, 0.20, 0.60) + temperature * 0.10);
            traits.Lethality = Clamp01(RandomRange(rng, 0.02, 0.20) - medTech * 0.15);
            traits.Mutability = Clamp01(RandomRange(rng, 0.10, 0.40) + radiation * 0.10);
            traits.Resilience = Clamp01(RandomRange(rng, 0.30, 0.70) + temperature * 0.10 + humidity * 0.10);
            traits.Incubation = Clamp01(RandomRange(rng, 0.20, 0.50));
            traits.Airborne = Clamp01(RandomRange(rng, 0.05, 0.30));
            traits.ImmuneEvasion = Clamp01(RandomRange(rng, 0.10, 0.40) + (1.0 - medTech) * 0.10);
        }
        else if (pathogenId == "fungal")
        {
            traits.Infectivity = Clamp01(RandomRange(rng, 0.15, 0.50) + humidity * 0.35);
            traits.Severity = Clamp01(RandomRange(rng, 0.10, 0.40) + humidity * 0.15);
            traits.Lethality = Clamp01(RandomRange(rng, 0.01, 0.10) + temperature * 0.05);
            traits.Mutability = Clamp01(RandomRange(rng, 0.05, 0.25));
            traits.Resilience = Clamp01(RandomRange(rng, 0.40, 0.80) + humidity * 0.15);
            traits.Incubation = Clamp01(RandomRange(rng, 0.50, 0.90));
            traits.Airborne = Clamp01(RandomRange(rng, 0.10, 0.50) + humidity * 0.20);
            traits.ImmuneEvasion = Clamp01(RandomRange(rng, 0.05, 0.20));
        }
        else if (pathogenId == "prion")
        {
            traits.Infectivity = Clamp01(RandomRange(rng, 0.05, 0.25));
            traits.Severity = Clamp01(RandomRange(rng, 0.60, 0.95));
            traits.Lethality = Clamp01(RandomRange(rng, 0.40, 0.80));
            traits.Mutability = Clamp01(RandomRange(rng, 0.01, 0.10));
            traits.Resilience = Clamp01(RandomRange(rng, 0.70, 0.95));
            traits.Incubation = Clamp01(RandomRange(rng, 0.70, 0.99));
            traits.Airborne = Clamp01(RandomRange(rng, 0.00, 0.05));
            traits.ImmuneEvasion = Clamp01(RandomRange(rng, 0.50, 0.90));
        }
        else if (pathogenId == "nanobot")
        {
            traits.Infectivity = Clamp01(RandomRange(rng, 0.30, 0.70) + ResolveConnectivity(context) * 0.20);
            traits.Severity = Clamp01(RandomRange(rng, 0.20, 0.70));
            traits.Lethality = Clamp01(RandomRange(rng, 0.05, 0.30));
            traits.Mutability = Clamp01(RandomRange(rng, 0.30, 0.70) + radiation * 0.12);
            traits.Resilience = Clamp01(RandomRange(rng, 0.50, 0.90) - humidity * 0.10);
            traits.Incubation = Clamp01(RandomRange(rng, 0.10, 0.30));
            traits.Airborne = Clamp01(RandomRange(rng, 0.10, 0.40));
            traits.ImmuneEvasion = Clamp01(RandomRange(rng, 0.30, 0.70) + radiation * 0.10);
        }
        else
        {
            traits.Infectivity = Clamp01(RandomRange(rng, 0.20, 0.55) + temperature * 0.15 + humidity * 0.10);
            traits.Severity = Clamp01(RandomRange(rng, 0.30, 0.70));
            traits.Lethality = Clamp01(RandomRange(rng, 0.05, 0.25));
            traits.Mutability = Clamp01(RandomRange(rng, 0.05, 0.30));
            traits.Resilience = Clamp01(RandomRange(rng, 0.30, 0.60) + temperature * 0.20);
            traits.Incubation = Clamp01(RandomRange(rng, 0.40, 0.80));
            traits.Airborne = Clamp01(RandomRange(rng, 0.00, 0.10));
            traits.ImmuneEvasion = Clamp01(RandomRange(rng, 0.20, 0.60) + temperature * 0.10);
        }

        return traits;
    }

    private static List<string> BuildSymptoms(string pathogenId, Traits traits, SeededRng rng)
    {
        List<string> symptoms = new List<string>();
        foreach (DiseaseSymptomDescriptor descriptor in DiseaseConceptData.SymptomPools[pathogenId])
        {
            double probability = 0.30;
            if (descriptor.Phase == "early")
            {
                probability = 0.70;
            }
            else if (descriptor.Phase == "mid")
            {
                probability = 0.45;
            }
            else if (descriptor.Phase == "late")
            {
                probability = 0.25;
            }

            if (GetTrait(traits, descriptor.TraitKey) >= descriptor.Threshold && rng.Randf() < probability)
            {
                symptoms.Add(descriptor.Name + " (" + descriptor.Phase + ")");
            }
        }

        if (symptoms.Count == 0)
        {
            DiseaseSymptomDescriptor fallback = DiseaseConceptData.SymptomPools[pathogenId][0];
            symptoms.Add(fallback.Name + " (" + fallback.Phase + ")");
        }

        return symptoms;
    }

    private static void RunSimulation(ConceptContextSnapshot context, string pathogenId, Traits traits, int population, List<string> symptoms, List<string> mutationEvents, SeededRng rng, out int totalInfected, out int totalDeaths, out int peakInfected, out int peakDay)
    {
        double susceptible = population - System.Math.Max(1, population * 0.0001);
        double exposed = System.Math.Max(1, population * 0.0001);
        double infected = 0.0;
        double recovered = 0.0;
        double dead = 0.0;
        double peak = 0.0;
        int peakIndex = 0;
        double density = ResolvePopulationDensity(context);
        double medTech = ResolveMedicalCapability(context.TechnologyLevel);
        double hygiene = ResolveHygiene(context);
        double transmission = traits.Infectivity * 0.8 * (1.0 + density * 0.6) * (1.0 + ResolveConnectivity(context) * 0.3);
        double sigma = Lerp(0.05, 0.5, 1.0 - traits.Incubation);
        double gamma = Lerp(0.03, 0.15, 1.0 - traits.Severity) / (1.0 + (1.0 - hygiene) * 0.2);
        double mu = traits.Lethality * Lerp(0.02, 0.12, traits.Severity) * (1.0 - medTech * 0.5);
        double totalFlow = 0.0;

        for (int day = 0; day <= 360; day += 1)
        {
            double livingPopulation = susceptible + exposed + infected + recovered;
            if (livingPopulation <= 0.0)
            {
                break;
            }

            if (infected > peak)
            {
                peak = infected;
                peakIndex = day;
            }

            if (day > 0 && day % 14 == 0 && rng.Randf() < traits.Mutability * 0.6f)
            {
                string[] keys = { "infectivity", "severity", "lethality", "mutability", "resilience", "incubation", "airborne", "immuneEvasion" };
                string traitKey = keys[rng.RandiRange(0, keys.Length - 1)];
                double delta = (rng.Randf() - 0.5) * 0.16 * (1.0 + context.RadiationLevel * 0.5);
                SetTrait(traits, traitKey, GetTrait(traits, traitKey) + delta);
                mutationEvents.Add("Day " + day.ToString(CultureInfo.InvariantCulture) + ": " + traitKey + " shifts to "
                    + (GetTrait(traits, traitKey) * 100.0).ToString("0.0", CultureInfo.InvariantCulture) + "%.");
                string emergentSymptom = TryEmergentSymptom(pathogenId, traits, symptoms, rng);
                if (!string.IsNullOrEmpty(emergentSymptom))
                {
                    symptoms.Add(emergentSymptom);
                    mutationEvents.Add("Day " + day.ToString(CultureInfo.InvariantCulture) + ": emergent symptom " + emergentSymptom + ".");
                }
            }

            double newExposed = (transmission * susceptible * infected) / livingPopulation;
            double newInfected = sigma * exposed;
            double newRecovered = gamma * infected;
            double newDead = mu * infected;
            susceptible = System.Math.Max(0.0, susceptible - newExposed);
            exposed = System.Math.Max(0.0, exposed + newExposed - newInfected);
            infected = System.Math.Max(0.0, infected + newInfected - newRecovered - newDead);
            recovered += newRecovered;
            dead += newDead;
            totalFlow += newInfected;
        }

        totalInfected = (int)System.Math.Round(totalFlow);
        totalDeaths = (int)System.Math.Round(dead);
        peakInfected = (int)System.Math.Round(peak);
        peakDay = peakIndex;
    }

    private static string TryEmergentSymptom(string pathogenId, Traits traits, List<string> symptoms, SeededRng rng)
    {
        if (rng.Randf() >= 0.30f)
        {
            return string.Empty;
        }

        foreach (DiseaseSymptomDescriptor descriptor in DiseaseConceptData.SymptomPools[pathogenId])
        {
            bool alreadyPresent = false;
            foreach (string symptom in symptoms)
            {
                if (symptom.StartsWith(descriptor.Name, StringComparison.Ordinal))
                {
                    alreadyPresent = true;
                    break;
                }
            }

            if (!alreadyPresent && GetTrait(traits, descriptor.TraitKey) >= descriptor.Threshold)
            {
                return descriptor.Name + " (" + descriptor.Phase + ")";
            }
        }

        return string.Empty;
    }

    private static List<string> BuildDrivers(ConceptContextSnapshot context, string pathogenId, int population)
    {
        List<string> drivers = new List<string>();
        drivers.Add("Host environment: " + ResolveHostEnvironment(context) + ".");
        if (population > 5000000)
        {
            drivers.Add("Dense settlement networks accelerate transmission.");
        }
        else
        {
            drivers.Add("Sparse host corridors slow some transmission chains.");
        }

        if (context.RadiationLevel > 0.40)
        {
            drivers.Add("Elevated radiation increases mutation pressure.");
        }

        if (context.WaterAvailability > 0.60)
        {
            drivers.Add("High moisture improves persistence outside the host.");
        }

        if (context.HabitabilityScore <= 4)
        {
            drivers.Add("Harsh conditions weaken logistics and medical resilience.");
        }

        if (pathogenId == "prion")
        {
            drivers.Add("Slow onset delays detection until neurological damage is advanced.");
        }

        if (pathogenId == "nanobot")
        {
            drivers.Add("Industrial and digital infrastructure create additional spread vectors.");
        }

        return drivers;
    }

    private static string ResolveHostEnvironment(ConceptContextSnapshot context)
    {
        if (context.DominantBiome.Equals("Desert", StringComparison.OrdinalIgnoreCase))
        {
            return "arid settlements";
        }

        if (context.DominantBiome.Equals("Oceanic", StringComparison.OrdinalIgnoreCase) || context.DominantBiome.Equals("Reef", StringComparison.OrdinalIgnoreCase))
        {
            return "maritime and littoral hosts";
        }

        if (context.DominantBiome.Equals("Forest", StringComparison.OrdinalIgnoreCase) || context.DominantBiome.Equals("Wetland", StringComparison.OrdinalIgnoreCase))
        {
            return "humid canopy and riverine hosts";
        }

        if (context.DominantBiome.Equals("Tundra", StringComparison.OrdinalIgnoreCase))
        {
            return "cold-weather host corridors";
        }

        return "temperate mixed hosts";
    }

    private static int ResolvePopulationSize(ConceptContextSnapshot context)
    {
        if (context.Population > 0)
        {
            return context.Population;
        }

        if (context.EcologyState != null && context.EcologyState.Status == Concepts.ConceptRunStatus.Generated)
        {
            double inferredHosts = context.EcologyState.Snapshot.Biomass * 2800.0 * (0.25 + context.EcologyState.Snapshot.Productivity);
            inferredHosts = System.Math.Clamp(inferredHosts, 25000.0, 50000000.0);
            return (int)System.Math.Round(inferredHosts);
        }

        return 1000000;
    }

    private static double ResolvePopulationDensity(ConceptContextSnapshot context)
    {
        if (context.Population <= 0)
        {
            return 0.18;
        }

        return Clamp01(System.Math.Log10(context.Population + 1) / 9.0);
    }

    private static double ResolveConnectivity(ConceptContextSnapshot context)
    {
        double connectivity = 0.15;
        if (context.TechnologyLevel.HasValue)
        {
            connectivity += ((int)context.TechnologyLevel.Value / 11.0) * 0.55;
        }

        if (context.Population > 10000000)
        {
            connectivity += 0.15;
        }

        return Clamp01(connectivity);
    }

    private static double ResolveHygiene(ConceptContextSnapshot context)
    {
        double hygiene = 0.20;
        if (context.TechnologyLevel.HasValue)
        {
            hygiene += ((int)context.TechnologyLevel.Value / 11.0) * 0.45;
        }

        if (context.HabitabilityScore >= 6)
        {
            hygiene += 0.10;
        }

        return Clamp01(hygiene);
    }

    private static double ResolveMedicalCapability(TechnologyLevel.Level? techLevel)
    {
        if (!techLevel.HasValue)
        {
            return 0.08;
        }

        return Clamp01((int)techLevel.Value / 11.0);
    }

    private static double GetTrait(Traits traits, string traitKey)
    {
        if (traitKey == "infectivity") { return traits.Infectivity; }
        if (traitKey == "severity") { return traits.Severity; }
        if (traitKey == "lethality") { return traits.Lethality; }
        if (traitKey == "mutability") { return traits.Mutability; }
        if (traitKey == "resilience") { return traits.Resilience; }
        if (traitKey == "incubation") { return traits.Incubation; }
        if (traitKey == "airborne") { return traits.Airborne; }
        if (traitKey == "immuneEvasion") { return traits.ImmuneEvasion; }
        throw new InvalidOperationException("Unknown disease trait key '" + traitKey + "'.");
    }

    private static void SetTrait(Traits traits, string traitKey, double value)
    {
        double clamped = Clamp01(value);
        if (traitKey == "infectivity") { traits.Infectivity = clamped; return; }
        if (traitKey == "severity") { traits.Severity = clamped; return; }
        if (traitKey == "lethality") { traits.Lethality = clamped; return; }
        if (traitKey == "mutability") { traits.Mutability = clamped; return; }
        if (traitKey == "resilience") { traits.Resilience = clamped; return; }
        if (traitKey == "incubation") { traits.Incubation = clamped; return; }
        if (traitKey == "airborne") { traits.Airborne = clamped; return; }
        if (traitKey == "immuneEvasion") { traits.ImmuneEvasion = clamped; return; }
        throw new InvalidOperationException("Unknown disease trait key '" + traitKey + "'.");
    }

    private static double RandomRange(SeededRng rng, double min, double max) => min + ((max - min) * rng.Randf());
    private static double Clamp01(double value) => System.Math.Clamp(value, 0.0, 1.0);
    private static double Lerp(double start, double end, double alpha) => start + ((end - start) * Clamp01(alpha));
}
