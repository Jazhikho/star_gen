#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Concepts;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Population;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Baselines;

/// <summary>
/// Generates a reproducible world-distribution baseline for life and settlement permissiveness tuning.
/// </summary>
public partial class LifeDistributionBaselineRunner : Node
{
    [Signal]
    public delegate void RunCompletedEventHandler(int exitCode);

    private const int TargetWorldCount = 1000;
    private const int SeedBase = 120000;

    private static readonly double[] ScenarioValues =
    {
        0.0,
        0.5,
        1.0,
    };

    private static readonly string[] ScenarioKeys =
    {
        "strict",
        "neutral",
        "space_opera",
    };

    private static readonly string[] ScenarioLabels =
    {
        "0.00 Strict",
        "0.50 Neutral",
        "1.00 Space Opera",
    };

    private sealed class BandStats
    {
        public string Label = string.Empty;
        public int MinimumHabitability;
        public int MaximumHabitability;
        public int TotalWorlds;
        public int BiosphereWorlds;
        public int SentientWorlds;
        public int SettledWorlds;
        public int ColonyWorlds;
    }

    private sealed class ScenarioResult
    {
        public string Key = string.Empty;
        public string Label = string.Empty;
        public int TotalWorlds;
        public int BiosphereWorlds;
        public int SentientWorlds;
        public int SettledWorlds;
        public int ColonyWorlds;
        public List<BandStats> Bands = new();
    }

    private sealed class WorldSample
    {
        public PlanetProfile Profile = new();
        public ColonySuitability Suitability = new();
        public int GenerationSeed;
    }

    public void start_headless()
    {
        CallDeferred(MethodName.RunBaseline);
    }

    private void RunBaseline()
    {
        try
        {
            List<WorldSample> worldSamples = CollectWorldSamples();
            List<ScenarioResult> scenarios = new();
            for (int index = 0; index < ScenarioValues.Length; index += 1)
            {
                ScenarioResult result = SampleScenario(ScenarioKeys[index], ScenarioLabels[index], ScenarioValues[index], worldSamples);
                scenarios.Add(result);
            }

            string outputDirectory = ProjectSettings.GlobalizePath("res://Tests/Baselines/Artifacts");
            DirAccess.MakeDirRecursiveAbsolute(outputDirectory);

            string markdownPath = outputDirectory + "/LifeDistributionBaseline.md";
            string csvPath = outputDirectory + "/LifeDistributionBaseline.csv";
            string jsonPath = outputDirectory + "/LifeDistributionBaseline.json";

            WriteTextFile(markdownPath, BuildMarkdownReport(scenarios));
            WriteTextFile(csvPath, BuildCsvReport(scenarios));
            WriteTextFile(jsonPath, Json.Stringify(BuildJsonPayload(scenarios), "\t"));

            GD.Print("Life distribution baseline written to:");
            GD.Print(markdownPath);
            GD.Print(csvPath);
            GD.Print(jsonPath);
            EmitSignal(SignalName.RunCompleted, 0);
        }
        catch (Exception exception)
        {
            GD.PushError("LifeDistributionBaselineRunner failed: " + exception.Message);
            GD.PushError(exception.StackTrace ?? string.Empty);
            EmitSignal(SignalName.RunCompleted, 1);
        }
    }

    private static List<WorldSample> CollectWorldSamples()
    {
        List<WorldSample> samples = new();
        GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
        int systemIndex = 0;
        while (samples.Count < TargetWorldCount)
        {
            int seed = SeedBase + systemIndex;
            GalaxySpec galaxySpec = GalaxySpec.CreateMilkyWay(seed);
            Vector3 worldPosition = new Vector3(8000.0f + systemIndex, 0.0f, 0.0f);
            GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(worldPosition, seed, galaxySpec);
            SolarSystem? system = GalaxySystemGenerator.GenerateSystem(star, includeAsteroids: false, enablePopulation: true, overrides: null, useCaseSettings: settings);
            systemIndex += 1;
            if (system == null)
            {
                continue;
            }

            foreach (CelestialBody body in system.GetPlanets())
            {
                AddWorldSample(samples, body);
                if (samples.Count >= TargetWorldCount)
                {
                    break;
                }
            }

            if (samples.Count >= TargetWorldCount)
            {
                continue;
            }

            foreach (CelestialBody body in system.GetMoons())
            {
                AddWorldSample(samples, body);
                if (samples.Count >= TargetWorldCount)
                {
                    break;
                }
            }
        }

        return samples;
    }

    private static ScenarioResult SampleScenario(string key, string label, double permissiveness, List<WorldSample> worldSamples)
    {
        ScenarioResult result = new ScenarioResult
        {
            Key = key,
            Label = label,
            Bands = CreateBands(),
        };

        GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
        settings.LifePermissiveness = permissiveness;
        settings.PopulationPermissiveness = permissiveness;

        foreach (WorldSample sample in worldSamples)
        {
            AddScenarioResult(result, sample, settings);
        }

        return result;
    }

    private static List<BandStats> CreateBands()
    {
        return new List<BandStats>
        {
            new BandStats { Label = "Hostile (0-1)", MinimumHabitability = 0, MaximumHabitability = 1 },
            new BandStats { Label = "Marginal (2-3)", MinimumHabitability = 2, MaximumHabitability = 3 },
            new BandStats { Label = "Viable (4-5)", MinimumHabitability = 4, MaximumHabitability = 5 },
            new BandStats { Label = "Habitable (6-7)", MinimumHabitability = 6, MaximumHabitability = 7 },
            new BandStats { Label = "Prime (8-10)", MinimumHabitability = 8, MaximumHabitability = 10 },
        };
    }

    private static void AddWorldSample(List<WorldSample> samples, CelestialBody body)
    {
        if (body.PopulationData == null || body.PopulationData.Profile == null)
        {
            return;
        }

        PlanetPopulationData populationData = body.PopulationData;
        if (populationData.Suitability == null)
        {
            return;
        }

        WorldSample sample = new WorldSample();
        sample.Profile = PlanetProfile.FromDictionary(populationData.Profile.ToDictionary());
        sample.Suitability = ColonySuitability.FromDictionary(populationData.Suitability.ToDictionary());
        sample.GenerationSeed = populationData.GenerationSeed;
        samples.Add(sample);
    }

    private static void AddScenarioResult(
        ScenarioResult result,
        WorldSample sample,
        GenerationUseCaseSettings settings)
    {
        bool nativeLifeExists = PopulationLikelihood.ShouldGenerateNatives(sample.Profile, sample.GenerationSeed, settings);
        bool colonyExists = PopulationLikelihood.ShouldGenerateColony(sample.Profile, sample.Suitability, sample.GenerationSeed, settings);
        PlanetPopulationData populationData = PopulationGenerator.GenerateFromProfile(
            PlanetProfile.FromDictionary(sample.Profile.ToDictionary()),
            sample.GenerationSeed,
            generateNatives: nativeLifeExists,
            generateColonies: colonyExists,
            currentYear: 0,
            existingSuitability: ColonySuitability.FromDictionary(sample.Suitability.ToDictionary()),
            useCaseSettings: settings);
        PlanetProfile profile = populationData.Profile;

        result.TotalWorlds += 1;
        BandStats band = GetBand(result.Bands, profile.HabitabilityScore);
        band.TotalWorlds += 1;

        bool hasBiosphere = populationData.EcologyState != null
            && populationData.EcologyState.Status == ConceptRunStatus.Generated;
        if (hasBiosphere)
        {
            result.BiosphereWorlds += 1;
            band.BiosphereWorlds += 1;
        }

        bool hasSentientLife = populationData.SentienceAssessment != null
            && populationData.SentienceAssessment.Status == ConceptRunStatus.Generated
            && populationData.SentienceAssessment.HasSentientLife;
        if (hasSentientLife)
        {
            result.SentientWorlds += 1;
            band.SentientWorlds += 1;
        }

        bool hasSettlements = populationData.HasExtantNatives() || populationData.HasActiveColonies();
        if (hasSettlements)
        {
            result.SettledWorlds += 1;
            band.SettledWorlds += 1;
        }

        if (populationData.HasActiveColonies())
        {
            result.ColonyWorlds += 1;
            band.ColonyWorlds += 1;
        }
    }

    private static BandStats GetBand(List<BandStats> bands, int habitabilityScore)
    {
        foreach (BandStats band in bands)
        {
            if (habitabilityScore >= band.MinimumHabitability && habitabilityScore <= band.MaximumHabitability)
            {
                return band;
            }
        }

        return bands[bands.Count - 1];
    }

    private static string BuildMarkdownReport(List<ScenarioResult> scenarios)
    {
        List<string> lines = new List<string>();
        lines.Add("# Life Distribution Baseline");
        lines.Add(string.Empty);
        lines.Add("Generated by `godot-mono.exe --path . --headless --script res://Tests/Baselines/RunLifeDistributionBaseline.gd`.");
        lines.Add(string.Empty);
        lines.Add($"Each scenario evaluates the same first {TargetWorldCount} generated planets and moons from deterministic seeds.");
        lines.Add(string.Empty);

        foreach (ScenarioResult scenario in scenarios)
        {
            lines.Add("## " + scenario.Label);
            lines.Add(string.Empty);
            lines.Add($"- Total worlds: {scenario.TotalWorlds}");
            lines.Add($"- Worlds with biospheres: {scenario.BiosphereWorlds} ({FormatPercent(scenario.BiosphereWorlds, scenario.TotalWorlds)})");
            lines.Add($"- Worlds with sentient native life: {scenario.SentientWorlds} ({FormatPercent(scenario.SentientWorlds, scenario.TotalWorlds)})");
            lines.Add($"- Worlds with any settlement: {scenario.SettledWorlds} ({FormatPercent(scenario.SettledWorlds, scenario.TotalWorlds)})");
            lines.Add($"- Worlds with colonies: {scenario.ColonyWorlds} ({FormatPercent(scenario.ColonyWorlds, scenario.TotalWorlds)})");
            lines.Add(string.Empty);
            lines.Add("| Habitability Band | Total | Biospheres | Sentient | Settled | Colonies | Distribution |");
            lines.Add("| --- | ---: | ---: | ---: | ---: | ---: | --- |");

            foreach (BandStats band in scenario.Bands)
            {
                lines.Add(
                    $"| {band.Label} | {band.TotalWorlds} | {band.BiosphereWorlds} | {band.SentientWorlds} | {band.SettledWorlds} | {band.ColonyWorlds} | {BuildBar(band.TotalWorlds, scenario.TotalWorlds)} |");
            }

            lines.Add(string.Empty);
        }

        return string.Join("\n", lines);
    }

    private static string BuildCsvReport(List<ScenarioResult> scenarios)
    {
        List<string> lines = new List<string>();
        lines.Add("scenario_key,scenario_label,band,total_worlds,biosphere_worlds,sentient_worlds,settled_worlds,colony_worlds");
        foreach (ScenarioResult scenario in scenarios)
        {
            foreach (BandStats band in scenario.Bands)
            {
                lines.Add(
                    $"{scenario.Key},{EscapeCsv(scenario.Label)},{EscapeCsv(band.Label)},{band.TotalWorlds},{band.BiosphereWorlds},{band.SentientWorlds},{band.SettledWorlds},{band.ColonyWorlds}");
            }
        }

        return string.Join("\n", lines);
    }

    private static Dictionary BuildJsonPayload(List<ScenarioResult> scenarios)
    {
        Array<Dictionary> scenarioArray = new Array<Dictionary>();
        foreach (ScenarioResult scenario in scenarios)
        {
            Array<Dictionary> bandArray = new Array<Dictionary>();
            foreach (BandStats band in scenario.Bands)
            {
                bandArray.Add(new Dictionary
                {
                    ["label"] = band.Label,
                    ["minimum_habitability"] = band.MinimumHabitability,
                    ["maximum_habitability"] = band.MaximumHabitability,
                    ["total_worlds"] = band.TotalWorlds,
                    ["biosphere_worlds"] = band.BiosphereWorlds,
                    ["sentient_worlds"] = band.SentientWorlds,
                    ["settled_worlds"] = band.SettledWorlds,
                    ["colony_worlds"] = band.ColonyWorlds,
                });
            }

            scenarioArray.Add(new Dictionary
            {
                ["key"] = scenario.Key,
                ["label"] = scenario.Label,
                ["total_worlds"] = scenario.TotalWorlds,
                ["biosphere_worlds"] = scenario.BiosphereWorlds,
                ["sentient_worlds"] = scenario.SentientWorlds,
                ["settled_worlds"] = scenario.SettledWorlds,
                ["colony_worlds"] = scenario.ColonyWorlds,
                ["bands"] = bandArray,
            });
        }

        return new Dictionary
        {
            ["sample_size_per_scenario"] = TargetWorldCount,
            ["seed_base"] = SeedBase,
            ["scenarios"] = scenarioArray,
        };
    }

    private static string BuildBar(int value, int total)
    {
        if (total <= 0)
        {
            return string.Empty;
        }

        const int maxWidth = 24;
        double ratio = (double)value / total;
        int width = (int)Math.Round(ratio * maxWidth);
        if (width < 1 && value > 0)
        {
            width = 1;
        }

        return new string('#', width);
    }

    private static string FormatPercent(int value, int total)
    {
        if (total <= 0)
        {
            return "0.0%";
        }

        double percent = ((double)value / total) * 100.0;
        return percent.ToString("0.0") + "%";
    }

    private static string EscapeCsv(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private static void WriteTextFile(string path, string content)
    {
        using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(content);
    }
}
