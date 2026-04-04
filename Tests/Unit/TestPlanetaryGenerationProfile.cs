#nullable enable annotations
#nullable disable warnings
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for shared planetary-generation profile plumbing and metadata.
/// </summary>
public static class TestPlanetaryGenerationProfile
{
    /// <summary>
    /// Tests that the shared planetary profile round-trips through its dictionary payload.
    /// </summary>
    public static void TestRoundTrip()
    {
        PlanetaryGenerationProfile profile = new PlanetaryGenerationProfile
        {
            MassRadiusModel = PlanetMassRadiusModel.ChenKipping,
            EnvelopeLossModel = PlanetEnvelopeLossModel.CorePowered,
            GasGiantFormationModel = GasGiantFormationModel.PebbleAssisted,
            MetallicityCouplingStrength = PlanetMetallicityCouplingStrength.Strong,
            MoonFormationBias = PlanetMoonFormationBias.CapturedRich,
            RoguePlanetAllowance = PlanetRoguePlanetAllowance.Standard,
            MinorBodyOuterSystemBias = PlanetMinorBodyOuterSystemBias.CometLeaning,
            SolidMassScalar = 1.35,
            GasMassScalar = 1.55,
            DiskLifetimeMyr = 5.2,
            SnowLineScalar = 1.15,
            OxidationScalar = 0.92,
            MigrationStrength = 1.30,
            ImpactStirring = 1.18,
        };

        PlanetaryGenerationProfile rebuilt = PlanetaryGenerationProfile.FromDictionary(profile.ToDictionary());

        DotNetNativeTestSuite.AssertEqual((int)profile.MassRadiusModel, (int)rebuilt.MassRadiusModel, "Mass-radius model should round-trip");
        DotNetNativeTestSuite.AssertEqual((int)profile.EnvelopeLossModel, (int)rebuilt.EnvelopeLossModel, "Envelope-loss model should round-trip");
        DotNetNativeTestSuite.AssertEqual((int)profile.GasGiantFormationModel, (int)rebuilt.GasGiantFormationModel, "Gas-giant model should round-trip");
        DotNetNativeTestSuite.AssertEqual((int)profile.MetallicityCouplingStrength, (int)rebuilt.MetallicityCouplingStrength, "Metallicity coupling should round-trip");
        DotNetNativeTestSuite.AssertEqual((int)profile.MoonFormationBias, (int)rebuilt.MoonFormationBias, "Moon-formation bias should round-trip");
        DotNetNativeTestSuite.AssertEqual((int)profile.RoguePlanetAllowance, (int)rebuilt.RoguePlanetAllowance, "Rogue allowance should round-trip");
        DotNetNativeTestSuite.AssertEqual((int)profile.MinorBodyOuterSystemBias, (int)rebuilt.MinorBodyOuterSystemBias, "Outer-system bias should round-trip");
        DotNetNativeTestSuite.AssertEqual(profile.SolidMassScalar, rebuilt.SolidMassScalar, "Solid-mass scalar should round-trip");
        DotNetNativeTestSuite.AssertEqual(profile.GasMassScalar, rebuilt.GasMassScalar, "Gas-mass scalar should round-trip");
    }

    /// <summary>
    /// Tests that derived planetary-system state round-trips through serialization.
    /// </summary>
    public static void TestDerivedStateRoundTrip()
    {
        SolarSystemSpec spec = new SolarSystemSpec(41, 1, 1);
        spec.SystemMetallicity = 1.18;
        spec.SystemAgeYears = 5.0e9;
        spec.PlanetaryProfile = new PlanetaryGenerationProfile
        {
            EnvelopeLossModel = PlanetEnvelopeLossModel.Photoevaporation,
            GasGiantFormationModel = GasGiantFormationModel.PebbleAssisted,
            SolidMassScalar = 1.25,
            GasMassScalar = 1.40,
            SnowLineScalar = 1.10,
            MigrationStrength = 1.20,
        };

        Array<CelestialBody> stars = new Array<CelestialBody> { CreateTestStar() };
        PlanetarySystemState state = PlanetarySystemState.Build(spec, stars);
        PlanetarySystemState rebuilt = PlanetarySystemState.FromDictionary(state.ToDictionary());

        DotNetNativeTestSuite.AssertEqual((int)state.Profile.EnvelopeLossModel, (int)rebuilt.Profile.EnvelopeLossModel, "Derived state should preserve envelope-loss model");
        DotNetNativeTestSuite.AssertEqual(state.SnowLineAu, rebuilt.SnowLineAu, "Derived state should preserve the snow line");
        DotNetNativeTestSuite.AssertEqual(state.GasGiantWeight, rebuilt.GasGiantWeight, "Derived state should preserve gas-giant weighting");
        DotNetNativeTestSuite.AssertEqual(state.HabitableZoneInnerAu, rebuilt.HabitableZoneInnerAu, "Derived state should preserve habitable-zone inner edge");
        DotNetNativeTestSuite.AssertEqual(state.HabitableZoneOuterAu, rebuilt.HabitableZoneOuterAu, "Derived state should preserve habitable-zone outer edge");
        DotNetNativeTestSuite.AssertEqual(state.VolatileDeliveryScalar, rebuilt.VolatileDeliveryScalar, "Derived state should preserve volatile delivery");
        DotNetNativeTestSuite.AssertEqual(state.XuvActivityScalar, rebuilt.XuvActivityScalar, "Derived state should preserve XUV activity");
    }

    /// <summary>
    /// Tests that galaxy and system specs preserve the shared planetary profile.
    /// </summary>
    public static void TestProfilePropagatesThroughGalaxyAndSystemSpecs()
    {
        PlanetaryGenerationProfile profile = new PlanetaryGenerationProfile
        {
            EnvelopeLossModel = PlanetEnvelopeLossModel.CorePowered,
            GasGiantFormationModel = GasGiantFormationModel.Mixed,
            MetallicityCouplingStrength = PlanetMetallicityCouplingStrength.Strong,
            RoguePlanetAllowance = PlanetRoguePlanetAllowance.Standard,
        };

        GalaxyConfig galaxyConfig = GalaxyConfig.CreateDefault();
        galaxyConfig.PlanetaryProfile = profile.Clone();
        GalaxyConfig? rebuiltGalaxyConfig = GalaxyConfig.FromDictionary(galaxyConfig.ToDictionary());
        DotNetNativeTestSuite.AssertNotNull(rebuiltGalaxyConfig, "Galaxy config should deserialize");
        DotNetNativeTestSuite.AssertEqual((int)profile.EnvelopeLossModel, (int)rebuiltGalaxyConfig!.PlanetaryProfile.EnvelopeLossModel, "Galaxy config should preserve the planetary profile");

        SolarSystemSpec systemSpec = new SolarSystemSpec(77, 1, 3);
        systemSpec.PlanetaryProfile = profile.Clone();
        SolarSystemSpec rebuiltSystemSpec = SolarSystemSpec.FromDictionary(systemSpec.ToDictionary());
        DotNetNativeTestSuite.AssertEqual((int)profile.RoguePlanetAllowance, (int)rebuiltSystemSpec.PlanetaryProfile.RoguePlanetAllowance, "System spec should preserve rogue-planet allowance");
    }

    /// <summary>
    /// Tests that the reference catalog covers every exposed aggregate planetary parameter.
    /// </summary>
    public static void TestReferenceCatalogCoversExposedParameters()
    {
        foreach (GenerationParameterDefinition definition in GenerationParameterCatalog.GetGalaxyDefinitions())
        {
            if (!definition.Id.StartsWith("planet_"))
            {
                continue;
            }

            DotNetNativeTestSuite.AssertTrue(!string.IsNullOrWhiteSpace(definition.AssumptionText), $"Galaxy definition should describe {definition.Id}");
            DotNetNativeTestSuite.AssertTrue(!string.IsNullOrWhiteSpace(PlanetaryScienceReferenceCatalog.GetTooltipSummary(definition.Id)), $"Planetary tooltip summary should exist for {definition.Id}");
        }

        foreach (GenerationParameterDefinition definition in GenerationParameterCatalog.GetSystemDefinitions())
        {
            if (!definition.Id.StartsWith("planet_"))
            {
                continue;
            }

            DotNetNativeTestSuite.AssertTrue(!string.IsNullOrWhiteSpace(definition.AssumptionText), $"System definition should describe {definition.Id}");
            DotNetNativeTestSuite.AssertTrue(!string.IsNullOrWhiteSpace(PlanetaryScienceReferenceCatalog.GetTooltipSummary(definition.Id)), $"System tooltip summary should exist for {definition.Id}");
        }

        foreach (PlanetaryScienceParameterReference reference in PlanetaryScienceReferenceCatalog.GetParameterReferences())
        {
            foreach (string sourceId in reference.SourceIds)
            {
                DotNetNativeTestSuite.AssertNotNull(PlanetaryScienceReferenceCatalog.GetSource(sourceId), $"Planetary source '{sourceId}' should resolve");
            }
        }
    }

    /// <summary>
    /// Tests that the help copy explains practical outcome changes in plain language.
    /// </summary>
    public static void TestHelpCopyUsesPlainLanguage()
    {
        string helpText = PlanetaryScienceReferenceCatalog.BuildHelpPanelBbCode();

        DotNetNativeTestSuite.AssertTrue(helpText.Contains("What changing it does"), "Planetary help should explain practical outcomes");
        DotNetNativeTestSuite.AssertTrue(helpText.Contains("rocky worlds"), "Planetary help should mention practical world outcomes");
        DotNetNativeTestSuite.AssertTrue(helpText.Contains("system state"), "Planetary help should explain the aggregate retrofit approach");
    }

    private static CelestialBody CreateTestStar()
    {
        StarSpec spec = StarSpec.SunLike(12345);
        SeededRng rng = new SeededRng(12345);
        CelestialBody star = StarGenerator.Generate(spec, rng);
        star.Id = "test_star";
        if (star.Stellar == null)
        {
            throw new System.InvalidOperationException("Generated test star is missing stellar properties.");
        }

        return star;
    }
}
