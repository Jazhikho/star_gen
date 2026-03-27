#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Concepts;
using StarGen.Domain.Concepts.Pipeline;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Tables;
using StarGen.Domain.Math;
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

    private const int TargetWorldCount = 10000;
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

    private sealed class ScoreStats
    {
        public int HabitabilityScore;
        public int TotalWorlds;
        public int BiosphereWorlds;
        public int SentientWorlds;
        public int SettledWorlds;
        public int ColonyWorlds;
    }

    private sealed class RemainingWorldStats
    {
        public int WorldCount;
        public int WetWorldCount;
        public int AtmosphereWorldCount;
        public int BreathableAtmosphereWorldCount;
        public int MoonWorldCount;
        public int PlanetWorldCount;
        public int MinHabitability = int.MaxValue;
        public int MaxHabitability = int.MinValue;
        public double HabitabilitySum;
        public double MinTemperatureK = double.MaxValue;
        public double MaxTemperatureK = double.MinValue;
        public double TemperatureSumK;
        public double MinPressureAtm = double.MaxValue;
        public double MaxPressureAtm = double.MinValue;
        public double PressureSumAtm;
        public double MinGravityG = double.MaxValue;
        public double MaxGravityG = double.MinValue;
        public double GravitySumG;
    }

    private sealed class SizeBlockedWorld
    {
        public string BodyId = string.Empty;
        public string BodyType = string.Empty;
        public string BlockReason = string.Empty;
        public string SizeCategory = string.Empty;
        public double MassEarth;
        public int HabitabilityScore;
        public double TemperatureK;
    }

    private sealed class ScenarioResult
    {
        public string Key = string.Empty;
        public string Label = string.Empty;
        public int TotalWorlds;
        public int RemainingWorlds;
        public int WetWorlds;
        public int BiologySupportWorlds;
        public int WaterlessWorlds;
        public int WetHabitabilityBlockedWorlds;
        public int WetRadiationBlockedWorlds;
        public int WetTooColdWorlds;
        public int WetTooHotWorlds;
        public int NativeCandidateWorlds;
        public int NativeEcologyCapableCandidateWorlds;
        public int NativeApprovedWorlds;
        public int NativeRollRejectedWorlds;
        public int NativeEcologyBlockedWorlds;
        public int NativeCandidateTemperatureBlockedWorlds;
        public int ColonyCandidateWorlds;
        public int ColonyApprovedWorlds;
        public int BiosphereWorlds;
        public double ExpectedBiosphereWorlds;
        public double ExpectedEcologyCapableBiosphereWorlds;
        public double NativeCandidateProbabilitySum;
        public double NativeCandidateRollSum;
        public int SentientWorlds;
        public int SettledWorlds;
        public int ColonyWorlds;
        public double ExpectedColonyWorlds;
        public double ColonyCandidateProbabilitySum;
        public double ColonyCandidateRollSum;
        public int SizeBlockedWorlds;
        public int GasGiantBlockedWorlds;
        public int IceGiantBlockedWorlds;
        public List<SizeBlockedWorld> SizeBlockedWorldDetails = new();
        public RemainingWorldStats RemainingStats = new();
        public List<ScoreStats> ScoreBuckets = new();
    }

    private sealed class WorldSample
    {
        public string BodyId = string.Empty;
        public string BodyType = string.Empty;
        public PlanetProfile Profile = new();
        public ColonySuitability Suitability = new();
        public int GenerationSeed;
        public bool IsBlockedBySize;
        public string BlockReason = string.Empty;
        public string SizeCategory = string.Empty;
        public double MassEarth;
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
            ScoreBuckets = CreateScoreBuckets(),
        };

        GenerationUseCaseSettings settings = GenerationUseCaseSettings.CreateDefault();
        settings.LifePermissiveness = permissiveness;

        foreach (WorldSample sample in worldSamples)
        {
            AddScenarioResult(result, sample, settings);
        }

        return result;
    }

    private static List<ScoreStats> CreateScoreBuckets()
    {
        List<ScoreStats> buckets = new List<ScoreStats>();
        for (int score = 0; score <= 10; score += 1)
        {
            ScoreStats stats = new ScoreStats();
            stats.HabitabilityScore = score;
            buckets.Add(stats);
        }

        return buckets;
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
        sample.BodyId = body.Id;
        sample.BodyType = body.GetTypeString();
        sample.Profile = PlanetProfile.FromDictionary(populationData.Profile.ToDictionary());
        sample.Suitability = ColonySuitability.FromDictionary(populationData.Suitability.ToDictionary());
        sample.GenerationSeed = populationData.GenerationSeed;
        sample.MassEarth = body.Physical.MassKg / Units.EarthMassKg;
        sample.SizeCategory = ResolveSizeCategoryLabel(sample.MassEarth);
        sample.IsBlockedBySize = IsBlockedBySizeCategory(sample.SizeCategory);
        if (sample.IsBlockedBySize)
        {
            if (sample.SizeCategory == "GasGiant")
            {
                sample.BlockReason = "Gas giant";
            }
            else
            {
                sample.BlockReason = "Ice giant";
            }
        }
        samples.Add(sample);
    }

    private static void AddScenarioResult(
        ScenarioResult result,
        WorldSample sample,
        GenerationUseCaseSettings settings)
    {
        result.TotalWorlds += 1;
        if (sample.IsBlockedBySize)
        {
            RecordBlockedWorld(result, sample);
            return;
        }

        PlanetEnvironmentProfile environmentProfile = PlanetEnvironmentProfile.FromPlanetProfile(
            sample.Profile,
            sample.GenerationSeed,
            sample.Profile.BodyId,
            "Planet");
        BiologySupportEvaluator.Assessment biologyAssessment = BiologySupportEvaluator.Evaluate(environmentProfile, settings);
        bool supportsBiology = biologyAssessment.IsSupported;
        bool nativeLifeExists = PopulationLikelihood.ShouldGenerateNatives(sample.Profile, sample.GenerationSeed, settings);
        bool colonyExists = PopulationLikelihood.ShouldGenerateColony(sample.Profile, sample.Suitability, sample.GenerationSeed, settings);
        double nativeLikelihood = PopulationLikelihood.EstimateNativeLikelihood(sample.Profile, settings);
        double colonyLikelihood = PopulationLikelihood.EstimateColonyLikelihood(sample.Profile, sample.Suitability, settings);
        double nativeRoll = PopulationLikelihood.DeriveRollValue(sample.GenerationSeed, PopulationLikelihood.NativeRollSalt);
        double colonyRoll = PopulationLikelihood.DeriveRollValue(sample.GenerationSeed, PopulationLikelihood.ColonyRollSalt);
        PlanetPopulationData populationData = PopulationGenerator.GenerateFromProfile(
            PlanetProfile.FromDictionary(sample.Profile.ToDictionary()),
            sample.GenerationSeed,
            generateNatives: nativeLifeExists,
            generateColonies: colonyExists,
            currentYear: 0,
            existingSuitability: ColonySuitability.FromDictionary(sample.Suitability.ToDictionary()),
            useCaseSettings: settings);
        PlanetProfile profile = populationData.Profile;
        result.RemainingWorlds += 1;
        UpdateRemainingWorldStats(result.RemainingStats, profile);
        if (sample.Profile.HasLiquidWater)
        {
            result.WetWorlds += 1;
        }

        if (supportsBiology)
        {
            result.BiologySupportWorlds += 1;
        }
        else
        {
            if (biologyAssessment.Reason == BiologySupportEvaluator.FailureReason.NoLiquidWater)
            {
                result.WaterlessWorlds += 1;
            }
            else if (biologyAssessment.Reason == BiologySupportEvaluator.FailureReason.LowHabitability)
            {
                result.WetHabitabilityBlockedWorlds += 1;
            }
            else if (biologyAssessment.Reason == BiologySupportEvaluator.FailureReason.HighRadiation)
            {
                result.WetRadiationBlockedWorlds += 1;
            }
            else if (biologyAssessment.Reason == BiologySupportEvaluator.FailureReason.TooCold)
            {
                result.WetTooColdWorlds += 1;
            }
            else if (biologyAssessment.Reason == BiologySupportEvaluator.FailureReason.TooHot)
            {
                result.WetTooHotWorlds += 1;
            }
        }

        if (nativeLikelihood > 0.0)
        {
            result.NativeCandidateWorlds += 1;
            result.NativeCandidateProbabilitySum += nativeLikelihood;
            result.NativeCandidateRollSum += nativeRoll;
            if (supportsBiology)
            {
                result.NativeEcologyCapableCandidateWorlds += 1;
                result.ExpectedEcologyCapableBiosphereWorlds += nativeLikelihood;
            }
            else if (environmentProfile.AvgTemperatureK < 180.0 || environmentProfile.AvgTemperatureK > 390.0)
            {
                result.NativeCandidateTemperatureBlockedWorlds += 1;
            }

            if (nativeLifeExists)
            {
                result.NativeApprovedWorlds += 1;
                if (!supportsBiology)
                {
                    result.NativeEcologyBlockedWorlds += 1;
                }
            }
            else
            {
                result.NativeRollRejectedWorlds += 1;
            }
        }

        if (colonyLikelihood > 0.0)
        {
            result.ColonyCandidateWorlds += 1;
            result.ColonyCandidateProbabilitySum += colonyLikelihood;
            result.ColonyCandidateRollSum += colonyRoll;
            if (colonyExists)
            {
                result.ColonyApprovedWorlds += 1;
            }
        }

        result.ExpectedBiosphereWorlds += nativeLikelihood;
        result.ExpectedColonyWorlds += colonyLikelihood;
        ScoreStats scoreStats = GetScoreBucket(result.ScoreBuckets, profile.HabitabilityScore);
        scoreStats.TotalWorlds += 1;

        bool hasBiosphere = populationData.EcologyState != null
            && populationData.EcologyState.Status == ConceptRunStatus.Generated;
        if (hasBiosphere)
        {
            result.BiosphereWorlds += 1;
            scoreStats.BiosphereWorlds += 1;
        }

        bool hasSentientLife = populationData.SentienceAssessment != null
            && populationData.SentienceAssessment.Status == ConceptRunStatus.Generated
            && populationData.SentienceAssessment.HasSentientLife;
        if (hasSentientLife)
        {
            result.SentientWorlds += 1;
            scoreStats.SentientWorlds += 1;
        }

        bool hasSettlements = populationData.HasExtantNatives() || populationData.HasActiveColonies();
        if (hasSettlements)
        {
            result.SettledWorlds += 1;
            scoreStats.SettledWorlds += 1;
        }

        if (populationData.HasActiveColonies())
        {
            result.ColonyWorlds += 1;
            scoreStats.ColonyWorlds += 1;
        }
    }

    private static ScoreStats GetScoreBucket(List<ScoreStats> buckets, int habitabilityScore)
    {
        int clampedScore = Math.Clamp(habitabilityScore, 0, 10);
        foreach (ScoreStats bucket in buckets)
        {
            if (bucket.HabitabilityScore == clampedScore)
            {
                return bucket;
            }
        }

        return buckets[buckets.Count - 1];
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
            lines.Add($"- Size-blocked worlds (gas/ice giants): {scenario.SizeBlockedWorlds} ({FormatPercent(scenario.SizeBlockedWorlds, scenario.TotalWorlds)})");
            lines.Add($"- Remaining evaluated worlds: {scenario.RemainingWorlds}");
            lines.Add($"- Gas giants blocked: {scenario.GasGiantBlockedWorlds}");
            lines.Add($"- Ice giants blocked: {scenario.IceGiantBlockedWorlds}");
            lines.Add($"- Remaining worlds (planets): {scenario.RemainingStats.PlanetWorldCount}");
            lines.Add($"- Remaining worlds (moons): {scenario.RemainingStats.MoonWorldCount}");
            lines.Add($"- Remaining worlds with water: {scenario.RemainingStats.WetWorldCount}");
            lines.Add($"- Remaining worlds with atmosphere: {scenario.RemainingStats.AtmosphereWorldCount}");
            lines.Add($"- Remaining worlds with breathable atmosphere: {scenario.RemainingStats.BreathableAtmosphereWorldCount}");
            lines.Add($"- Remaining worlds habitability min/max/avg: {FormatRangeWithAverageInt(scenario.RemainingStats.MinHabitability, scenario.RemainingStats.MaxHabitability, scenario.RemainingStats.HabitabilitySum, scenario.RemainingStats.WorldCount)}");
            lines.Add($"- Remaining worlds temperature K min/max/avg: {FormatRangeWithAverageDouble(scenario.RemainingStats.MinTemperatureK, scenario.RemainingStats.MaxTemperatureK, scenario.RemainingStats.TemperatureSumK, scenario.RemainingStats.WorldCount)}");
            lines.Add($"- Remaining worlds pressure atm min/max/avg: {FormatRangeWithAverageDouble(scenario.RemainingStats.MinPressureAtm, scenario.RemainingStats.MaxPressureAtm, scenario.RemainingStats.PressureSumAtm, scenario.RemainingStats.WorldCount)}");
            lines.Add($"- Remaining worlds gravity g min/max/avg: {FormatRangeWithAverageDouble(scenario.RemainingStats.MinGravityG, scenario.RemainingStats.MaxGravityG, scenario.RemainingStats.GravitySumG, scenario.RemainingStats.WorldCount)}");
            lines.Add($"- Wet worlds: {scenario.WetWorlds}");
            lines.Add($"- Ecology-capable worlds: {scenario.BiologySupportWorlds}");
            lines.Add($"- Waterless worlds: {scenario.WaterlessWorlds}");
            lines.Add($"- Wet worlds blocked by habitability: {scenario.WetHabitabilityBlockedWorlds}");
            lines.Add($"- Wet worlds blocked by radiation: {scenario.WetRadiationBlockedWorlds}");
            lines.Add($"- Wet worlds blocked as too cold: {scenario.WetTooColdWorlds}");
            lines.Add($"- Wet worlds blocked as too hot: {scenario.WetTooHotWorlds}");
            lines.Add($"- Native-life candidates: {scenario.NativeCandidateWorlds}");
            lines.Add($"- Expected biospheres from probability sum: {scenario.ExpectedBiosphereWorlds:0.0}");
            lines.Add($"- Ecology-capable native candidates: {scenario.NativeEcologyCapableCandidateWorlds}");
            lines.Add($"- Expected biospheres within ecology-capable worlds: {scenario.ExpectedEcologyCapableBiosphereWorlds:0.0}");
            lines.Add($"- Worlds approved by native roll: {scenario.NativeApprovedWorlds}");
            lines.Add($"- Native candidates avg probability: {FormatAverage(scenario.NativeCandidateProbabilitySum, scenario.NativeCandidateWorlds)}");
            lines.Add($"- Native candidates avg roll: {FormatAverage(scenario.NativeCandidateRollSum, scenario.NativeCandidateWorlds)}");
            lines.Add($"- Native candidates rejected by roll: {scenario.NativeRollRejectedWorlds}");
            lines.Add($"- Native approvals blocked by ecology support: {scenario.NativeEcologyBlockedWorlds}");
            lines.Add($"- Native candidates blocked by ecology temperature gate: {scenario.NativeCandidateTemperatureBlockedWorlds}");
            lines.Add($"- Worlds with biospheres: {scenario.BiosphereWorlds} ({FormatPercent(scenario.BiosphereWorlds, scenario.RemainingWorlds)})");
            lines.Add($"- Colony candidates: {scenario.ColonyCandidateWorlds}");
            lines.Add($"- Expected colony worlds from probability sum: {scenario.ExpectedColonyWorlds:0.0}");
            lines.Add($"- Worlds approved by colony roll: {scenario.ColonyApprovedWorlds}");
            lines.Add($"- Colony candidates avg probability: {FormatAverage(scenario.ColonyCandidateProbabilitySum, scenario.ColonyCandidateWorlds)}");
            lines.Add($"- Colony candidates avg roll: {FormatAverage(scenario.ColonyCandidateRollSum, scenario.ColonyCandidateWorlds)}");
            lines.Add($"- Worlds with sentient native life: {scenario.SentientWorlds} ({FormatPercent(scenario.SentientWorlds, scenario.RemainingWorlds)})");
            lines.Add($"- Worlds with any settlement: {scenario.SettledWorlds} ({FormatPercent(scenario.SettledWorlds, scenario.RemainingWorlds)})");
            lines.Add($"- Worlds with colonies: {scenario.ColonyWorlds} ({FormatPercent(scenario.ColonyWorlds, scenario.RemainingWorlds)})");
            lines.Add(string.Empty);
            lines.Add("| Habitability Score | Total | Biospheres | Sentient | Settled | Colonies | Distribution |");
            lines.Add("| --- | ---: | ---: | ---: | ---: | ---: | --- |");

            foreach (ScoreStats scoreStats in scenario.ScoreBuckets)
            {
                lines.Add(
                    $"| {scoreStats.HabitabilityScore} | {scoreStats.TotalWorlds} | {scoreStats.BiosphereWorlds} | {scoreStats.SentientWorlds} | {scoreStats.SettledWorlds} | {scoreStats.ColonyWorlds} | {BuildBar(scoreStats.TotalWorlds, scenario.RemainingWorlds)} |");
            }

            lines.Add(string.Empty);
            lines.Add("### Size-blocked worlds");
            lines.Add(string.Empty);
            lines.Add("| Body ID | Type | Block | Size Category | Mass (Earth) | Habitability | Temp K |");
            lines.Add("| --- | --- | --- | --- | ---: | ---: | ---: |");
            foreach (SizeBlockedWorld blockedWorld in scenario.SizeBlockedWorldDetails)
            {
                lines.Add($"| {blockedWorld.BodyId} | {blockedWorld.BodyType} | {blockedWorld.BlockReason} | {blockedWorld.SizeCategory} | {blockedWorld.MassEarth:0.000} | {blockedWorld.HabitabilityScore} | {blockedWorld.TemperatureK:0.0} |");
            }
            lines.Add(string.Empty);
        }

        return string.Join("\n", lines);
    }

    private static string BuildCsvReport(List<ScenarioResult> scenarios)
    {
        List<string> lines = new List<string>();
        lines.Add("scenario_key,scenario_label,total_worlds,remaining_worlds,size_blocked_worlds,gas_giant_blocked_worlds,ice_giant_blocked_worlds,wet_worlds,biology_support_worlds,waterless_worlds,wet_habitability_blocked_worlds,wet_radiation_blocked_worlds,wet_too_cold_worlds,wet_too_hot_worlds,native_candidate_worlds,native_ecology_capable_candidate_worlds,native_approved_worlds,native_roll_rejected_worlds,native_ecology_blocked_worlds,native_candidate_temperature_blocked_worlds,expected_biosphere_worlds,expected_ecology_capable_biosphere_worlds,native_candidate_avg_probability,native_candidate_avg_roll,colony_candidate_worlds,colony_approved_worlds,expected_colony_worlds,colony_candidate_avg_probability,colony_candidate_avg_roll,habitability_score,score_worlds,biosphere_worlds,sentient_worlds,settled_worlds,colony_worlds");
        foreach (ScenarioResult scenario in scenarios)
        {
            foreach (ScoreStats scoreStats in scenario.ScoreBuckets)
            {
                lines.Add(
                    $"{scenario.Key},{EscapeCsv(scenario.Label)},{scenario.TotalWorlds},{scenario.RemainingWorlds},{scenario.SizeBlockedWorlds},{scenario.GasGiantBlockedWorlds},{scenario.IceGiantBlockedWorlds},{scenario.WetWorlds},{scenario.BiologySupportWorlds},{scenario.WaterlessWorlds},{scenario.WetHabitabilityBlockedWorlds},{scenario.WetRadiationBlockedWorlds},{scenario.WetTooColdWorlds},{scenario.WetTooHotWorlds},{scenario.NativeCandidateWorlds},{scenario.NativeEcologyCapableCandidateWorlds},{scenario.NativeApprovedWorlds},{scenario.NativeRollRejectedWorlds},{scenario.NativeEcologyBlockedWorlds},{scenario.NativeCandidateTemperatureBlockedWorlds},{scenario.ExpectedBiosphereWorlds:0.000},{scenario.ExpectedEcologyCapableBiosphereWorlds:0.000},{FormatAverage(scenario.NativeCandidateProbabilitySum, scenario.NativeCandidateWorlds)},{FormatAverage(scenario.NativeCandidateRollSum, scenario.NativeCandidateWorlds)},{scenario.ColonyCandidateWorlds},{scenario.ColonyApprovedWorlds},{scenario.ExpectedColonyWorlds:0.000},{FormatAverage(scenario.ColonyCandidateProbabilitySum, scenario.ColonyCandidateWorlds)},{FormatAverage(scenario.ColonyCandidateRollSum, scenario.ColonyCandidateWorlds)},{scoreStats.HabitabilityScore},{scoreStats.TotalWorlds},{scoreStats.BiosphereWorlds},{scoreStats.SentientWorlds},{scoreStats.SettledWorlds},{scoreStats.ColonyWorlds}");
            }
        }

        return string.Join("\n", lines);
    }

    private static Dictionary BuildJsonPayload(List<ScenarioResult> scenarios)
    {
        Array<Dictionary> scenarioArray = new Array<Dictionary>();
        foreach (ScenarioResult scenario in scenarios)
        {
            Array<Dictionary> scoreArray = new Array<Dictionary>();
            foreach (ScoreStats scoreStats in scenario.ScoreBuckets)
            {
                scoreArray.Add(new Dictionary
                {
                    ["habitability_score"] = scoreStats.HabitabilityScore,
                    ["total_worlds"] = scoreStats.TotalWorlds,
                    ["biosphere_worlds"] = scoreStats.BiosphereWorlds,
                    ["sentient_worlds"] = scoreStats.SentientWorlds,
                    ["settled_worlds"] = scoreStats.SettledWorlds,
                    ["colony_worlds"] = scoreStats.ColonyWorlds,
                });
            }

            Array<Dictionary> blockedArray = new Array<Dictionary>();
            foreach (SizeBlockedWorld blockedWorld in scenario.SizeBlockedWorldDetails)
            {
                blockedArray.Add(new Dictionary
                {
                    ["body_id"] = blockedWorld.BodyId,
                    ["body_type"] = blockedWorld.BodyType,
                    ["block_reason"] = blockedWorld.BlockReason,
                    ["size_category"] = blockedWorld.SizeCategory,
                    ["mass_earth"] = blockedWorld.MassEarth,
                    ["habitability_score"] = blockedWorld.HabitabilityScore,
                    ["temperature_k"] = blockedWorld.TemperatureK,
                });
            }

            scenarioArray.Add(new Dictionary
            {
                ["key"] = scenario.Key,
                ["label"] = scenario.Label,
                ["total_worlds"] = scenario.TotalWorlds,
                ["remaining_worlds"] = scenario.RemainingWorlds,
                ["size_blocked_worlds"] = scenario.SizeBlockedWorlds,
                ["gas_giant_blocked_worlds"] = scenario.GasGiantBlockedWorlds,
                ["ice_giant_blocked_worlds"] = scenario.IceGiantBlockedWorlds,
                ["wet_worlds"] = scenario.WetWorlds,
                ["biology_support_worlds"] = scenario.BiologySupportWorlds,
                ["waterless_worlds"] = scenario.WaterlessWorlds,
                ["wet_habitability_blocked_worlds"] = scenario.WetHabitabilityBlockedWorlds,
                ["wet_radiation_blocked_worlds"] = scenario.WetRadiationBlockedWorlds,
                ["wet_too_cold_worlds"] = scenario.WetTooColdWorlds,
                ["wet_too_hot_worlds"] = scenario.WetTooHotWorlds,
                ["native_candidate_worlds"] = scenario.NativeCandidateWorlds,
                ["native_ecology_capable_candidate_worlds"] = scenario.NativeEcologyCapableCandidateWorlds,
                ["native_approved_worlds"] = scenario.NativeApprovedWorlds,
                ["native_roll_rejected_worlds"] = scenario.NativeRollRejectedWorlds,
                ["native_ecology_blocked_worlds"] = scenario.NativeEcologyBlockedWorlds,
                ["native_candidate_temperature_blocked_worlds"] = scenario.NativeCandidateTemperatureBlockedWorlds,
                ["expected_biosphere_worlds"] = scenario.ExpectedBiosphereWorlds,
                ["expected_ecology_capable_biosphere_worlds"] = scenario.ExpectedEcologyCapableBiosphereWorlds,
                ["native_candidate_avg_probability"] = FormatAverage(scenario.NativeCandidateProbabilitySum, scenario.NativeCandidateWorlds),
                ["native_candidate_avg_roll"] = FormatAverage(scenario.NativeCandidateRollSum, scenario.NativeCandidateWorlds),
                ["biosphere_worlds"] = scenario.BiosphereWorlds,
                ["colony_candidate_worlds"] = scenario.ColonyCandidateWorlds,
                ["colony_approved_worlds"] = scenario.ColonyApprovedWorlds,
                ["expected_colony_worlds"] = scenario.ExpectedColonyWorlds,
                ["colony_candidate_avg_probability"] = FormatAverage(scenario.ColonyCandidateProbabilitySum, scenario.ColonyCandidateWorlds),
                ["colony_candidate_avg_roll"] = FormatAverage(scenario.ColonyCandidateRollSum, scenario.ColonyCandidateWorlds),
                ["sentient_worlds"] = scenario.SentientWorlds,
                ["settled_worlds"] = scenario.SettledWorlds,
                ["colony_worlds"] = scenario.ColonyWorlds,
                ["score_buckets"] = scoreArray,
                ["size_blocked_world_details"] = blockedArray,
                ["remaining_world_stats"] = BuildRemainingWorldStatsDictionary(scenario.RemainingStats),
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

    private static string FormatAverage(double totalValue, int count)
    {
        if (count <= 0)
        {
            return "0.000";
        }

        return (totalValue / count).ToString("0.000");
    }

    private static string ResolveSizeCategoryLabel(double massEarth)
    {
        SizeCategory.Category category = SizeTable.CategoryFromMass(massEarth);
        return category.ToString();
    }

    private static bool IsBlockedBySizeCategory(string sizeCategory)
    {
        if (sizeCategory == SizeCategory.Category.MiniNeptune.ToString())
        {
            return true;
        }

        if (sizeCategory == SizeCategory.Category.NeptuneClass.ToString())
        {
            return true;
        }

        if (sizeCategory == SizeCategory.Category.GasGiant.ToString())
        {
            return true;
        }

        return false;
    }

    private static void RecordBlockedWorld(ScenarioResult result, WorldSample sample)
    {
        result.SizeBlockedWorlds += 1;
        if (sample.SizeCategory == SizeCategory.Category.GasGiant.ToString())
        {
            result.GasGiantBlockedWorlds += 1;
        }
        else
        {
            result.IceGiantBlockedWorlds += 1;
        }

        SizeBlockedWorld blockedWorld = new SizeBlockedWorld();
        blockedWorld.BodyId = sample.BodyId;
        blockedWorld.BodyType = sample.BodyType;
        blockedWorld.BlockReason = sample.BlockReason;
        blockedWorld.SizeCategory = sample.SizeCategory;
        blockedWorld.MassEarth = sample.MassEarth;
        blockedWorld.HabitabilityScore = sample.Profile.HabitabilityScore;
        blockedWorld.TemperatureK = sample.Profile.AvgTemperatureK;
        result.SizeBlockedWorldDetails.Add(blockedWorld);
    }

    private static void UpdateRemainingWorldStats(RemainingWorldStats stats, PlanetProfile profile)
    {
        stats.WorldCount += 1;
        if (profile.HasLiquidWater)
        {
            stats.WetWorldCount += 1;
        }

        if (profile.HasAtmosphere)
        {
            stats.AtmosphereWorldCount += 1;
        }

        if (profile.HasBreathableAtmosphere)
        {
            stats.BreathableAtmosphereWorldCount += 1;
        }

        if (profile.IsMoon)
        {
            stats.MoonWorldCount += 1;
        }
        else
        {
            stats.PlanetWorldCount += 1;
        }

        stats.HabitabilitySum += profile.HabitabilityScore;
        if (profile.HabitabilityScore < stats.MinHabitability)
        {
            stats.MinHabitability = profile.HabitabilityScore;
        }

        if (profile.HabitabilityScore > stats.MaxHabitability)
        {
            stats.MaxHabitability = profile.HabitabilityScore;
        }

        stats.TemperatureSumK += profile.AvgTemperatureK;
        if (profile.AvgTemperatureK < stats.MinTemperatureK)
        {
            stats.MinTemperatureK = profile.AvgTemperatureK;
        }

        if (profile.AvgTemperatureK > stats.MaxTemperatureK)
        {
            stats.MaxTemperatureK = profile.AvgTemperatureK;
        }

        stats.PressureSumAtm += profile.PressureAtm;
        if (profile.PressureAtm < stats.MinPressureAtm)
        {
            stats.MinPressureAtm = profile.PressureAtm;
        }

        if (profile.PressureAtm > stats.MaxPressureAtm)
        {
            stats.MaxPressureAtm = profile.PressureAtm;
        }

        stats.GravitySumG += profile.GravityG;
        if (profile.GravityG < stats.MinGravityG)
        {
            stats.MinGravityG = profile.GravityG;
        }

        if (profile.GravityG > stats.MaxGravityG)
        {
            stats.MaxGravityG = profile.GravityG;
        }
    }

    private static string FormatRangeWithAverageInt(int minimum, int maximum, double sum, int count)
    {
        if (count <= 0)
        {
            return "n/a";
        }

        return $"{minimum} / {maximum} / {(sum / count):0.000}";
    }

    private static string FormatRangeWithAverageDouble(double minimum, double maximum, double sum, int count)
    {
        if (count <= 0)
        {
            return "n/a";
        }

        return $"{minimum:0.000} / {maximum:0.000} / {(sum / count):0.000}";
    }

    private static Dictionary BuildRemainingWorldStatsDictionary(RemainingWorldStats stats)
    {
        Dictionary dictionary = new Dictionary();
        dictionary["world_count"] = stats.WorldCount;
        dictionary["wet_world_count"] = stats.WetWorldCount;
        dictionary["atmosphere_world_count"] = stats.AtmosphereWorldCount;
        dictionary["breathable_atmosphere_world_count"] = stats.BreathableAtmosphereWorldCount;
        dictionary["moon_world_count"] = stats.MoonWorldCount;
        dictionary["planet_world_count"] = stats.PlanetWorldCount;
        if (stats.WorldCount <= 0)
        {
            dictionary["min_habitability"] = 0;
            dictionary["max_habitability"] = 0;
            dictionary["avg_habitability"] = 0.0;
            dictionary["min_temperature_k"] = 0.0;
            dictionary["max_temperature_k"] = 0.0;
            dictionary["avg_temperature_k"] = 0.0;
            dictionary["min_pressure_atm"] = 0.0;
            dictionary["max_pressure_atm"] = 0.0;
            dictionary["avg_pressure_atm"] = 0.0;
            dictionary["min_gravity_g"] = 0.0;
            dictionary["max_gravity_g"] = 0.0;
            dictionary["avg_gravity_g"] = 0.0;
            return dictionary;
        }

        dictionary["min_habitability"] = stats.MinHabitability;
        dictionary["max_habitability"] = stats.MaxHabitability;
        dictionary["avg_habitability"] = stats.HabitabilitySum / stats.WorldCount;
        dictionary["min_temperature_k"] = stats.MinTemperatureK;
        dictionary["max_temperature_k"] = stats.MaxTemperatureK;
        dictionary["avg_temperature_k"] = stats.TemperatureSumK / stats.WorldCount;
        dictionary["min_pressure_atm"] = stats.MinPressureAtm;
        dictionary["max_pressure_atm"] = stats.MaxPressureAtm;
        dictionary["avg_pressure_atm"] = stats.PressureSumAtm / stats.WorldCount;
        dictionary["min_gravity_g"] = stats.MinGravityG;
        dictionary["max_gravity_g"] = stats.MaxGravityG;
        dictionary["avg_gravity_g"] = stats.GravitySumG / stats.WorldCount;
        return dictionary;
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
