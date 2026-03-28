#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;
using StarGen.Domain.Galaxy;
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
        foreach (GenerationParameterDefinition definition in GenerationParameterCatalog.GetGalaxyDefinitions())
        {
            if (string.IsNullOrWhiteSpace(definition.AssumptionText))
            {
                continue;
            }

            if (definition.Id == "ruleset_mode" || definition.Id == "show_traveller_readouts" || definition.Id == "life_permissiveness" || definition.Id == "mainworld_policy" || definition.Id == "galaxy_seed")
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
}
