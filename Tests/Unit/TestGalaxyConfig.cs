#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Concepts.Pipeline;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for galaxy scientific configuration and metadata.
/// </summary>
public static class TestGalaxyConfig
{
    /// <summary>
    /// Tests that the default galaxy config ships scientific defaults.
    /// </summary>
    public static void TestCreateDefaultReturnsScientificMilkyWayConfig()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();

        DotNetNativeTestSuite.AssertTrue(config.IsValid(), "default config should be valid");
        DotNetNativeTestSuite.AssertEqual((int)GalaxySpec.GalaxyType.Spiral, (int)config.Type, "default family should be spiral");
        DotNetNativeTestSuite.AssertEqual((int)GalaxySubtypeMode.IntermediateType, (int)config.SubtypeMode, "Milky Way preset should bias toward an intermediate spiral");
        DotNetNativeTestSuite.AssertEqual((int)GalaxyBarMode.PreferBarred, (int)config.BarMode, "Milky Way preset should prefer a bar");
        DotNetNativeTestSuite.AssertEqual((int)GalaxyArmMechanism.GrandDesign, (int)config.ArmMechanismPreference, "Milky Way preset should default to grand-design arms");
        DotNetNativeTestSuite.AssertEqual(2600.0, config.ThinDiskScaleLengthPc, "Milky Way preset should carry the Bland-Hawthorn/Gerhard thin-disk scale length");
        DotNetNativeTestSuite.AssertEqual(2000.0, config.ThickDiskScaleLengthPc, "Milky Way preset should carry the Bland-Hawthorn/Gerhard thick-disk scale length");
        DotNetNativeTestSuite.AssertEqual(300.0, config.ThinDiskScaleHeightPc, "Milky Way preset should carry the thin-disk scale height");
        DotNetNativeTestSuite.AssertEqual(900.0, config.ThickDiskScaleHeightPc, "Milky Way preset should carry the thick-disk scale height");
        DotNetNativeTestSuite.AssertEqual(4500.0, config.BarHalfLengthPc, "Milky Way preset should expose bar half-length separately from bulge radius");
        DotNetNativeTestSuite.AssertEqual(8200.0, config.SolarGalactocentricRadiusPc, "Milky Way preset should carry the solar-circle radius");
        DotNetNativeTestSuite.AssertEqual(240.0, config.CircularVelocityAtSolarRadiusKmS, "Milky Way preset should carry the circular velocity at the solar circle");
        DotNetNativeTestSuite.AssertEqual(5.0e10, config.StellarMassSolar, "Milky Way preset should carry the stellar mass scale");
    }

    /// <summary>
    /// Tests that scientific fields survive dictionary round-trip.
    /// </summary>
    public static void TestScientificFieldsRoundTrip()
    {
        GalaxyConfig original = GalaxyConfig.CreateMilkyWay();
        original.Type = GalaxySpec.GalaxyType.Lenticular;
        original.SubtypeMode = GalaxySubtypeMode.LateType;
        original.BarMode = GalaxyBarMode.PreferUnbarred;
        original.HaloMassLog10Solar = 12.6;
        original.EnvironmentDensityIndex = 0.65;
        original.GhzInnerRadiusPc = 5200.0;
        original.GhzOuterRadiusPc = 13200.0;
        original.GhzTransitionWidthPc = 2400.0;
        original.MetallicityGradientDexPerKpc = -0.035;
        original.StarFormationEfficiency = 0.09;
        original.ThinDiskScaleLengthPc = 2700.0;
        original.ThickDiskScaleLengthPc = 2100.0;
        original.ThinDiskScaleHeightPc = 320.0;
        original.ThickDiskScaleHeightPc = 950.0;
        original.BarHalfLengthPc = 4300.0;
        original.SolarGalactocentricRadiusPc = 8300.0;
        original.CircularVelocityAtSolarRadiusKmS = 235.0;
        original.StellarMassSolar = 4.8e10;
        original.StellarProfile.ImfForm = StellarImfForm.Chabrier;
        original.StellarProfile.IsochroneModel = StellarIsochroneModel.Parsec;
        original.StellarProfile.MultiplicityScale = 1.15;

        Dictionary data = original.ToDictionary();
        GalaxyConfig? restored = GalaxyConfig.FromDictionary(data);

        DotNetNativeTestSuite.AssertNotNull(restored, "config should deserialize");
        DotNetNativeTestSuite.AssertEqual((int)original.Type, (int)restored!.Type, "family should round-trip");
        DotNetNativeTestSuite.AssertEqual((int)original.SubtypeMode, (int)restored.SubtypeMode, "subtype mode should round-trip");
        DotNetNativeTestSuite.AssertEqual((int)original.BarMode, (int)restored.BarMode, "bar mode should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.HaloMassLog10Solar, restored.HaloMassLog10Solar, "halo mass should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.GhzOuterRadiusPc, restored.GhzOuterRadiusPc, "GHZ outer radius should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.MetallicityGradientDexPerKpc, restored.MetallicityGradientDexPerKpc, "metallicity gradient should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.ThinDiskScaleLengthPc, restored.ThinDiskScaleLengthPc, "thin-disk scale length should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.ThickDiskScaleLengthPc, restored.ThickDiskScaleLengthPc, "thick-disk scale length should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.ThinDiskScaleHeightPc, restored.ThinDiskScaleHeightPc, "thin-disk scale height should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.ThickDiskScaleHeightPc, restored.ThickDiskScaleHeightPc, "thick-disk scale height should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.BarHalfLengthPc, restored.BarHalfLengthPc, "bar half-length should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.SolarGalactocentricRadiusPc, restored.SolarGalactocentricRadiusPc, "solar-circle radius should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.CircularVelocityAtSolarRadiusKmS, restored.CircularVelocityAtSolarRadiusKmS, "solar-circle circular velocity should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.StellarMassSolar, restored.StellarMassSolar, "stellar mass scale should round-trip");
        DotNetNativeTestSuite.AssertEqual((int)original.StellarProfile.ImfForm, (int)restored.StellarProfile.ImfForm, "stellar IMF should round-trip");
        DotNetNativeTestSuite.AssertEqual(original.StellarProfile.MultiplicityScale, restored.StellarProfile.MultiplicityScale, "stellar multiplicity should round-trip");
    }

    /// <summary>
    /// Tests that scientific validation rejects broken halo or GHZ ranges.
    /// </summary>
    public static void TestScientificValidationRejectsBrokenRanges()
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        config.HaloMassLog10Solar = 8.5;
        DotNetNativeTestSuite.AssertTrue(!config.IsValid(), "halo mass below range should be invalid");

        config = GalaxyConfig.CreateMilkyWay();
        config.GhzOuterRadiusPc = config.GhzInnerRadiusPc;
        DotNetNativeTestSuite.AssertTrue(!config.IsValid(), "GHZ outer radius must exceed inner radius");

        config = GalaxyConfig.CreateMilkyWay();
        config.ThickDiskScaleHeightPc = 250.0;
        DotNetNativeTestSuite.AssertTrue(!config.IsValid(), "thick-disk scale height below range should be invalid");
    }

    /// <summary>
    /// Tests that applying config to spec resolves the scientific profile.
    /// </summary>
    public static void TestApplyToSpecCreatesResolvedScientificProfile()
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        config.Type = GalaxySpec.GalaxyType.Lenticular;
        config.SubtypeMode = GalaxySubtypeMode.LateType;
        config.BarMode = GalaxyBarMode.PreferBarred;

        GalaxySpec spec = new GalaxySpec
        {
            GalaxySeed = 12345,
        };

        config.ApplyToSpec(spec);

        DotNetNativeTestSuite.AssertEqual((int)GalaxySpec.GalaxyType.Lenticular, (int)spec.Type, "resolved spec should keep the selected family");
        DotNetNativeTestSuite.AssertEqual((int)GalaxyResolvedSubtype.LenticularS0a, (int)spec.ResolvedSubtype, "late lenticular should resolve to S0/a");
        DotNetNativeTestSuite.AssertTrue(spec.IsBarred, "preferred barred lenticular should resolve as barred");
        DotNetNativeTestSuite.AssertEqual(0, spec.NumArms, "lenticular profiles should not expose active spiral arms");
        DotNetNativeTestSuite.AssertNotNull(spec.RealismProfile, "resolved realism profile should be stored");
    }

    /// <summary>
    /// Tests that Milky-Way structural parameters are first-class fields and affect regional classification.
    /// </summary>
    public static void TestMilkyWayStructuralSchemaUsesBlandParameters()
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        GalaxySpec spec = GalaxySpec.CreateFromConfig(config, 15001);

        DotNetNativeTestSuite.AssertEqual(2600.0, spec.ThinDiskScaleLengthPc, "spec should carry thin-disk scale length");
        DotNetNativeTestSuite.AssertEqual(2000.0, spec.ThickDiskScaleLengthPc, "spec should carry thick-disk scale length");
        DotNetNativeTestSuite.AssertEqual(300.0, spec.ThinDiskScaleHeightPc, "spec should carry thin-disk scale height");
        DotNetNativeTestSuite.AssertEqual(900.0, spec.ThickDiskScaleHeightPc, "spec should carry thick-disk scale height");
        DotNetNativeTestSuite.AssertEqual(4500.0, spec.BarHalfLengthPc, "spec should carry bar half-length");
        DotNetNativeTestSuite.AssertEqual(8200.0, spec.SolarGalactocentricRadiusPc, "spec should carry solar-circle radius");
        DotNetNativeTestSuite.AssertEqual(240.0, spec.CircularVelocityAtSolarRadiusKmS, "spec should carry circular velocity at the solar circle");
        DotNetNativeTestSuite.AssertEqual(5.0e10, spec.StellarMassSolar, "spec should carry stellar mass scale");
        DotNetNativeTestSuite.AssertEqual("partly implemented", spec.RealismProfile.MilkyWayStructureSourceStatus, "profile should record partial Bland-Hawthorn/Gerhard implementation status");

        GalaxyOriginContext barContext = GalaxyScientificFieldEvaluator.Evaluate(new Vector3(3200.0f, 0.0f, 0.0f), spec);
        DotNetNativeTestSuite.AssertEqual((int)GalaxyRegionKind.Bar, (int)barContext.RegionKind, "bar half-length should classify elongated inner-bar positions as bar influenced");
        DotNetNativeTestSuite.AssertTrue(barContext.IsBarInfluenced, "bar-region context should carry bar influence");
        DotNetNativeTestSuite.AssertEqual(8200.0, barContext.SolarGalactocentricRadiusPc, "origin context should carry solar-circle radius");
        DotNetNativeTestSuite.AssertEqual(240.0, barContext.CircularVelocityAtSolarRadiusKmS, "origin context should carry solar-circle circular velocity");

        Dictionary specData = spec.ToDictionary();
        GalaxySpec restored = GalaxySpec.FromDictionary(specData);
        DotNetNativeTestSuite.AssertEqual(spec.ThickDiskScaleHeightPc, restored.ThickDiskScaleHeightPc, "spec thick-disk field should survive round-trip");
        DotNetNativeTestSuite.AssertEqual(spec.BarHalfLengthPc, restored.BarHalfLengthPc, "spec bar half-length should survive round-trip");
        DotNetNativeTestSuite.AssertEqual(spec.RealismProfile.MilkyWayStructureSourceStatus, restored.RealismProfile.MilkyWayStructureSourceStatus, "profile source status should survive round-trip");
    }

    /// <summary>
    /// Tests that galaxy-family names include the new lenticular and irregular labels.
    /// </summary>
    public static void TestGetTypeNameIncludesExpandedFamilies()
    {
        GalaxyConfig config = GalaxyConfig.CreateDefault();
        config.Type = GalaxySpec.GalaxyType.Lenticular;
        DotNetNativeTestSuite.AssertEqual("Lenticular", config.GetTypeName(), "lenticular label should be available");

        config.Type = GalaxySpec.GalaxyType.Irregular;
        DotNetNativeTestSuite.AssertEqual("Irregular / Dwarf", config.GetTypeName(), "irregular label should reflect the dwarf-capable family");
    }

    /// <summary>
    /// Tests that every exposed galaxy-science parameter has assumption text and valid sources.
    /// </summary>
    public static void TestGalaxyScienceReferenceCatalogCoversExposedParameters()
    {
        foreach (GalaxyScienceParameterReference reference in GalaxyScienceReferenceCatalog.GetParameterReferences())
        {
            DotNetNativeTestSuite.AssertTrue(reference.SourceIds.Count > 0, $"galaxy science parameter '{reference.ParameterId}' should resolve at least one source");
        }

        foreach (GenerationParameterDefinition definition in GenerationParameterCatalog.GetGalaxyDefinitions())
        {
            if (definition.Classification != GenerationParameterClassification.GenerationPrior)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(definition.AssumptionText))
            {
                continue;
            }

            DotNetNativeTestSuite.AssertTrue(!string.IsNullOrWhiteSpace(definition.AssumptionText), $"assumption text should exist for {definition.Id}");
            foreach (string sourceId in GalaxyScienceReferenceCatalog.GetParameterSourceIds(definition.Id))
            {
                DotNetNativeTestSuite.AssertNotNull(GalaxyScienceReferenceCatalog.GetSource(sourceId), $"source '{sourceId}' should resolve");
            }
        }

        foreach (string sourceId in GalaxyScienceReferenceCatalog.GetSciencePanelSourceIds())
        {
            DotNetNativeTestSuite.AssertNotNull(GalaxyScienceReferenceCatalog.GetSource(sourceId), $"science panel source '{sourceId}' should resolve");
        }
    }

    /// <summary>
    /// Tests that spiral arm controls materially change how much of the disk resolves as arm structure.
    /// </summary>
    public static void TestArmControlsChangeArmCoverage()
    {
        GalaxySpec sparseSpec = BuildSpiralSpec(2, 12.0, 11001);
        GalaxySpec richSpec = BuildSpiralSpec(6, 28.0, 11001);

        int sparseArmSamples = CountArmSamples(sparseSpec, 7200.0, 180);
        int richArmSamples = CountArmSamples(richSpec, 7200.0, 180);

        DotNetNativeTestSuite.AssertTrue(richArmSamples > sparseArmSamples, "more open multi-arm spirals should cover more sampled disk positions with arm influence");
    }

    /// <summary>
    /// Tests that denser environments bias automatic spiral subtype resolution toward earlier forms.
    /// </summary>
    public static void TestEnvironmentDensityBiasChangesResolvedSubtype()
    {
        GalaxyConfig quietConfig = GalaxyConfig.CreateMilkyWay();
        quietConfig.Type = GalaxySpec.GalaxyType.Spiral;
        quietConfig.SubtypeMode = GalaxySubtypeMode.Automatic;
        quietConfig.HaloMassLog10Solar = 11.4;
        quietConfig.EnvironmentDensityIndex = 0.05;

        GalaxyConfig crowdedConfig = GalaxyConfig.CreateMilkyWay();
        crowdedConfig.Type = GalaxySpec.GalaxyType.Spiral;
        crowdedConfig.SubtypeMode = GalaxySubtypeMode.Automatic;
        crowdedConfig.HaloMassLog10Solar = 11.4;
        crowdedConfig.EnvironmentDensityIndex = 0.95;

        GalaxyRealismProfile quietProfile = GalaxyRealismProfileBuilder.Build(quietConfig, 12001);
        GalaxyRealismProfile crowdedProfile = GalaxyRealismProfileBuilder.Build(crowdedConfig, 12001);

        DotNetNativeTestSuite.AssertTrue((int)crowdedProfile.ResolvedSubtype < (int)quietProfile.ResolvedSubtype, "crowded environments should bias automatic spiral resolution toward earlier types");
    }

    /// <summary>
    /// Tests that ellipticity materially changes the resolved axis ratio for elliptical galaxies.
    /// </summary>
    public static void TestEllipticityChangesEllipticalAxisRatio()
    {
        GalaxySpec rounderSpec = BuildEllipticalSpec(0.05, 13001);
        GalaxySpec flatterSpec = BuildEllipticalSpec(0.35, 13001);

        EllipticalDensityModel rounderModel = new EllipticalDensityModel(rounderSpec);
        EllipticalDensityModel flatterModel = new EllipticalDensityModel(flatterSpec);

        DotNetNativeTestSuite.AssertTrue(flatterModel.GetAxisRatio() < rounderModel.GetAxisRatio(), "higher ellipticity should lower the resolved minor-axis ratio");
    }

    /// <summary>
    /// Tests that steeper metallicity gradients materially reduce outer-disk metallicity.
    /// </summary>
    public static void TestMetallicityGradientChangesOuterDiskChemistry()
    {
        GalaxySpec steepSpec = BuildSpiralSpec(4, 18.0, 14001);
        steepSpec.MetallicityGradientDexPerKpc = -0.08;
        steepSpec.RealismProfile!.MetallicityGradientDexPerKpc = -0.08;

        GalaxySpec shallowSpec = BuildSpiralSpec(4, 18.0, 14001);
        shallowSpec.MetallicityGradientDexPerKpc = -0.015;
        shallowSpec.RealismProfile!.MetallicityGradientDexPerKpc = -0.015;

        Vector3 outerDiskPosition = new Vector3(11000.0f, 0.0f, 0.0f);
        GalaxyOriginContext steepContext = GalaxyScientificFieldEvaluator.Evaluate(outerDiskPosition, steepSpec);
        GalaxyOriginContext shallowContext = GalaxyScientificFieldEvaluator.Evaluate(outerDiskPosition, shallowSpec);

        DotNetNativeTestSuite.AssertTrue(steepContext.MetallicityPrior < shallowContext.MetallicityPrior, "steeper metallicity gradients should reduce outer-disk metallicity more strongly");
    }

    /// <summary>
    /// Tests that the galaxy help copy explains both terms and practical outcomes in plain language.
    /// </summary>
    public static void TestGalaxyScienceHelpUsesPlainLanguage()
    {
        string helpText = GalaxyScienceReferenceCatalog.BuildSciencePanelBbCode();

        DotNetNativeTestSuite.AssertTrue(helpText.Contains("What changing it does"), "Galaxy help should explain practical outcomes");
        DotNetNativeTestSuite.AssertTrue(helpText.Contains("heavy elements"), "Galaxy help should define metallicity in plain language");
        DotNetNativeTestSuite.AssertTrue(helpText.Contains("Grand design"), "Galaxy help should explain spiral arm terms");
        DotNetNativeTestSuite.AssertTrue(!helpText.Contains("galactic_formation.md"), "Galaxy help should not cite the internal paper");
    }

    /// <summary>
    /// Tests that the life-model controls have plain-language help and resolvable sources.
    /// </summary>
    public static void TestLifeScienceReferenceCatalogCoversLifeModels()
    {
        foreach (LifeScienceParameterReference reference in new[]
        {
            new LifeScienceParameterReference("life_framework", LifeScienceReferenceCatalog.GetTooltipSummary("life_framework"), LifeScienceReferenceCatalog.GetParameterSourceIds("life_framework")),
            new LifeScienceParameterReference("abiogenesis_model", LifeScienceReferenceCatalog.GetTooltipSummary("abiogenesis_model"), LifeScienceReferenceCatalog.GetParameterSourceIds("abiogenesis_model")),
            new LifeScienceParameterReference("complex_life_model", LifeScienceReferenceCatalog.GetTooltipSummary("complex_life_model"), LifeScienceReferenceCatalog.GetParameterSourceIds("complex_life_model")),
            new LifeScienceParameterReference("civilization_model", LifeScienceReferenceCatalog.GetTooltipSummary("civilization_model"), LifeScienceReferenceCatalog.GetParameterSourceIds("civilization_model")),
            new LifeScienceParameterReference("environmental_window_weight", LifeScienceReferenceCatalog.GetTooltipSummary("environmental_window_weight"), LifeScienceReferenceCatalog.GetParameterSourceIds("environmental_window_weight")),
        })
        {
            DotNetNativeTestSuite.AssertTrue(reference.SourceIds.Count > 0, $"life science parameter '{reference.ParameterId}' should resolve at least one source");
        }

        GenerationParameterDefinition? lifeFrameworkDefinition = null;
        GenerationParameterDefinition? abiogenesisDefinition = null;
        GenerationParameterDefinition? civilizationDefinition = null;
        foreach (GenerationParameterDefinition definition in GenerationParameterCatalog.GetGalaxyDefinitions())
        {
            if (definition.Id == "life_framework")
            {
                lifeFrameworkDefinition = definition;
            }

            if (definition.Id == "abiogenesis_model")
            {
                abiogenesisDefinition = definition;
            }

            if (definition.Id == "civilization_model")
            {
                civilizationDefinition = definition;
            }
        }

        DotNetNativeTestSuite.AssertNotNull(lifeFrameworkDefinition, "Galaxy parameters should expose a life-framework selector");
        DotNetNativeTestSuite.AssertNotNull(abiogenesisDefinition, "Galaxy parameters should expose an abiogenesis selector");
        DotNetNativeTestSuite.AssertNotNull(civilizationDefinition, "Galaxy parameters should expose a civilization selector");
        DotNetNativeTestSuite.AssertTrue(!string.IsNullOrWhiteSpace(lifeFrameworkDefinition!.AssumptionText), "Life framework selector should have help text");
        DotNetNativeTestSuite.AssertTrue(lifeFrameworkDefinition.AssumptionText.Contains("Earth-Anchored Composite"), "Life framework help should explain the composite option");
        DotNetNativeTestSuite.AssertTrue(abiogenesisDefinition!.AssumptionText.Contains("Rapid Start"), "Abiogenesis help should explain the rapid-start option");
        DotNetNativeTestSuite.AssertTrue(civilizationDefinition!.AssumptionText.Contains("Technosphere"), "Civilization help should explain the technosphere bottleneck option");

        foreach (string sourceId in LifeScienceReferenceCatalog.GetParameterSourceIds("life_framework"))
        {
            DotNetNativeTestSuite.AssertNotNull(LifeScienceReferenceCatalog.GetSource(sourceId), $"life-framework source '{sourceId}' should resolve");
        }

        string helpText = LifeScienceReferenceCatalog.BuildHelpPanelBbCode();
        DotNetNativeTestSuite.AssertTrue(helpText.Contains("What changing it does"), "Life help should explain practical outcomes");
        DotNetNativeTestSuite.AssertTrue(helpText.Contains("What this composite assumes"), "Life help should explain the composite assumptions explicitly");
        DotNetNativeTestSuite.AssertTrue(helpText.Contains("Earth-Anchored Composite"), "Life help should use the Earth-Anchored Composite label");
        DotNetNativeTestSuite.AssertTrue(helpText.Contains("Rapid Biospheres"), "Life help should include the rapid-biosphere model");
        DotNetNativeTestSuite.AssertTrue(helpText.Contains("Rare Complex Life"), "Life help should include the rare-complex-life model");
        DotNetNativeTestSuite.AssertFalse(helpText.Contains("Earth History"), "Life help should not use the old Earth History label");
    }

    /// <summary>
    /// Tests that life models materially change biology and civilization gating for the same world.
    /// </summary>
    public static void TestLifeModelsChangeBiologyAssessment()
    {
        PlanetEnvironmentProfile environment = new PlanetEnvironmentProfile
        {
            Seed = 42,
            BodyId = "life_test_world",
            BodyName = "Life Test World",
            BodyType = "Planet",
            HabitabilityScore = 9,
            AvgTemperatureK = 289.0,
            StellarAgeYears = 5.4e9,
            PressureAtm = 1.0,
            OceanCoverage = 0.58,
            LandCoverage = 0.32,
            IceCoverage = 0.04,
            GravityG = 1.0,
            VolcanismLevel = 0.22,
            WeatherSeverity = 0.18,
            MagneticFieldStrength = 0.72,
            RadiationLevel = 0.12,
            StellarFluxEarth = 1.02,
            HabitableZoneInnerAu = 0.95,
            HabitableZoneOuterAu = 1.65,
            HabitableZoneAlignment = 0.92,
            XuvExposure = 0.10,
            TidalHeatingFactor = 0.0,
            ParentRadiationExposure = 0.0,
            HasAtmosphere = true,
            HasLiquidWater = true,
            HasBreathableAtmosphere = true,
            HasMagneticField = true,
            IsMoon = false,
        };
        environment.BiomeCoverage["ocean"] = 0.58;
        environment.BiomeCoverage["temperate"] = 0.26;
        environment.BiomeCoverage["arid"] = 0.08;

        GenerationUseCaseSettings rapidSettings = GenerationUseCaseSettings.CreateDefault();
        rapidSettings.LifeFramework = GenerationUseCaseSettings.LifeFrameworkType.RapidBiospheres;
        rapidSettings.LifePermissiveness = GenerationUseCaseSettings.GetRecommendedLifePermissiveness(rapidSettings.LifeFramework);

        GenerationUseCaseSettings rareSettings = GenerationUseCaseSettings.CreateDefault();
        rareSettings.LifeFramework = GenerationUseCaseSettings.LifeFrameworkType.RareComplexLife;
        rareSettings.LifePermissiveness = GenerationUseCaseSettings.GetRecommendedLifePermissiveness(rareSettings.LifeFramework);
        rareSettings.CivilizationModel = GenerationUseCaseSettings.CivilizationModelType.RareCivilizations;

        BiologySupportEvaluator.Assessment rapidAssessment = BiologySupportEvaluator.Evaluate(environment, rapidSettings);
        BiologySupportEvaluator.Assessment rareAssessment = BiologySupportEvaluator.Evaluate(environment, rareSettings);

        DotNetNativeTestSuite.AssertTrue(rapidAssessment.IsSupported, "Rapid-biosphere model should still support a good temperate world");
        DotNetNativeTestSuite.AssertTrue(rareAssessment.IsSupported, "Rare-complex-life model should still allow a prime habitable world");
        DotNetNativeTestSuite.AssertTrue(rapidAssessment.AbiogenesisChance > rareAssessment.AbiogenesisChance, "Rapid-biosphere model should make simple life easier to start");
        DotNetNativeTestSuite.AssertTrue(rapidAssessment.ComplexLifeChance > rareAssessment.ComplexLifeChance, "Rapid-biosphere model should remain more permissive than rare-complex-life for complex ecosystems");
        DotNetNativeTestSuite.AssertTrue(rapidAssessment.SentienceChance > rareAssessment.SentienceChance, "Rapid-biosphere model should permit higher sentience odds than rare-complex-life");
        DotNetNativeTestSuite.AssertTrue(rapidAssessment.CivilizationChance > rareAssessment.CivilizationChance, "Rapid-biosphere model should remain more permissive than rare-complex-life for civilizations");
    }

    private static GalaxySpec BuildSpiralSpec(int numArms, double pitchAngleDeg, int seed)
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        config.Type = GalaxySpec.GalaxyType.Spiral;
        config.NumArms = numArms;
        config.ArmPitchAngleDeg = pitchAngleDeg;
        config.ArmAmplitude = 0.40;

        GalaxySpec spec = new GalaxySpec
        {
            GalaxySeed = seed,
        };
        config.ApplyToSpec(spec);
        return spec;
    }

    private static GalaxySpec BuildEllipticalSpec(double ellipticity, int seed)
    {
        GalaxyConfig config = GalaxyConfig.CreateMilkyWay();
        config.Type = GalaxySpec.GalaxyType.Elliptical;
        config.Ellipticity = ellipticity;

        GalaxySpec spec = new GalaxySpec
        {
            GalaxySeed = seed,
        };
        config.ApplyToSpec(spec);
        return spec;
    }

    private static int CountArmSamples(GalaxySpec spec, double radiusPc, int sampleCount)
    {
        int hits = 0;
        for (int index = 0; index < sampleCount; index += 1)
        {
            double theta = (Math.PI * 2.0 * index) / sampleCount;
            Vector3 sample = new Vector3(
                (float)(Math.Cos(theta) * radiusPc),
                0.0f,
                (float)(Math.Sin(theta) * radiusPc));
            GalaxyOriginContext context = GalaxyScientificFieldEvaluator.Evaluate(sample, spec);
            if (context.RegionKind == GalaxyRegionKind.SpiralArm)
            {
                hits += 1;
            }
        }

        return hits;
    }
}
