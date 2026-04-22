#nullable enable annotations
#nullable disable warnings
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Parameters;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Systems;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for shared stellar-generation profile plumbing and metadata.
/// </summary>
public static class TestStellarGenerationProfile
{
    /// <summary>
    /// Tests that the profile round-trips through its dictionary payload.
    /// </summary>
    public static void TestRoundTrip()
    {
        StellarGenerationProfile profile = new StellarGenerationProfile
        {
            ImfForm = StellarImfForm.Chabrier,
            ImfVariationMode = StellarImfVariationMode.MetallicityAgeModulated,
            IsochroneModel = StellarIsochroneModel.Parsec,
            MultiplicityScale = 1.35,
        };

        StellarGenerationProfile rebuilt = StellarGenerationProfile.FromDictionary(profile.ToDictionary());
        DotNetNativeTestSuite.AssertEqual((int)profile.ImfForm, (int)rebuilt.ImfForm, "IMF form should round-trip");
        DotNetNativeTestSuite.AssertEqual((int)profile.ImfVariationMode, (int)rebuilt.ImfVariationMode, "IMF variation should round-trip");
        DotNetNativeTestSuite.AssertEqual((int)profile.IsochroneModel, (int)rebuilt.IsochroneModel, "Isochrone model should round-trip");
        DotNetNativeTestSuite.AssertEqual(profile.MultiplicityScale, rebuilt.MultiplicityScale, "Multiplicity scale should round-trip");
    }

    /// <summary>
    /// Tests that galaxy and system specs preserve the stellar profile.
    /// </summary>
    public static void TestProfilePropagatesThroughGalaxyAndSystemSpecs()
    {
        StellarGenerationProfile profile = new StellarGenerationProfile
        {
            ImfForm = StellarImfForm.Chabrier,
            ImfVariationMode = StellarImfVariationMode.MetallicityAgeModulated,
            IsochroneModel = StellarIsochroneModel.Parsec,
            MultiplicityScale = 1.20,
        };

        GalaxyConfig galaxyConfig = GalaxyConfig.CreateDefault();
        galaxyConfig.StellarProfile = profile.Clone();
        GalaxyConfig? rebuiltGalaxyConfig = GalaxyConfig.FromDictionary(galaxyConfig.ToDictionary());
        DotNetNativeTestSuite.AssertNotNull(rebuiltGalaxyConfig, "Galaxy config should deserialize");
        DotNetNativeTestSuite.AssertEqual((int)profile.ImfForm, (int)rebuiltGalaxyConfig!.StellarProfile.ImfForm, "Galaxy config should preserve IMF form");

        SolarSystemSpec systemSpec = new SolarSystemSpec(77, 1, 3);
        systemSpec.StellarProfile = profile.Clone();
        SolarSystemSpec rebuiltSystemSpec = SolarSystemSpec.FromDictionary(systemSpec.ToDictionary());
        DotNetNativeTestSuite.AssertEqual((int)profile.IsochroneModel, (int)rebuiltSystemSpec.StellarProfile.IsochroneModel, "System spec should preserve isochrone model");
        DotNetNativeTestSuite.AssertEqual(profile.MultiplicityScale, rebuiltSystemSpec.StellarProfile.MultiplicityScale, "System spec should preserve multiplicity scale");
    }

    /// <summary>
    /// Tests that every exposed stellar parameter has assumption text and valid source links.
    /// </summary>
    public static void TestReferenceCatalogCoversExposedParameters()
    {
        foreach (StellarScienceParameterReference reference in StellarScienceReferenceCatalog.GetParameterReferences())
        {
            DotNetNativeTestSuite.AssertTrue(reference.SourceIds.Count > 0, $"stellar science parameter '{reference.ParameterId}' should resolve at least one source");
        }

        foreach (GenerationParameterDefinition definition in GenerationParameterCatalog.GetGalaxyDefinitions())
        {
            if (!definition.Id.StartsWith("stellar_"))
            {
                continue;
            }

            DotNetNativeTestSuite.AssertTrue(!string.IsNullOrWhiteSpace(definition.AssumptionText), $"Galaxy definition should describe {definition.Id}");
            foreach (string sourceId in StellarScienceReferenceCatalog.GetParameterSourceIds(definition.Id))
            {
                DotNetNativeTestSuite.AssertNotNull(StellarScienceReferenceCatalog.GetSource(sourceId), $"Galaxy stellar source '{sourceId}' should resolve");
            }
        }

        foreach (GenerationParameterDefinition definition in GenerationParameterCatalog.GetSystemDefinitions())
        {
            if (!definition.Id.StartsWith("stellar_"))
            {
                continue;
            }

            DotNetNativeTestSuite.AssertTrue(!string.IsNullOrWhiteSpace(definition.AssumptionText), $"System definition should describe {definition.Id}");
            foreach (string sourceId in StellarScienceReferenceCatalog.GetParameterSourceIds(definition.Id))
            {
                DotNetNativeTestSuite.AssertNotNull(StellarScienceReferenceCatalog.GetSource(sourceId), $"System stellar source '{sourceId}' should resolve");
            }
        }

        foreach (string sourceId in StellarScienceReferenceCatalog.GetHelpPanelSourceIds())
        {
            DotNetNativeTestSuite.AssertNotNull(StellarScienceReferenceCatalog.GetSource(sourceId), $"Help-panel source '{sourceId}' should resolve");
        }
    }

    /// <summary>
    /// Tests that the stellar help copy explains terms and practical outcomes in plain language.
    /// </summary>
    public static void TestHelpCopyUsesPlainLanguage()
    {
        string helpText = StellarScienceReferenceCatalog.BuildHelpPanelBbCode();

        DotNetNativeTestSuite.AssertTrue(helpText.Contains("What changing it does"), "Stellar help should explain practical outcomes");
        DotNetNativeTestSuite.AssertTrue(helpText.Contains("red dwarfs"), "Stellar help should give non-experts a practical example");
        DotNetNativeTestSuite.AssertTrue(!helpText.Contains("galactic_formation.md"), "Stellar help should not cite the internal paper");
    }
}
