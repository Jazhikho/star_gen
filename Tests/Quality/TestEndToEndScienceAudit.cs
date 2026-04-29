#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Godot;

namespace StarGen.Tests.Quality;

/// <summary>
/// Verifies that the first-pass end-to-end science audit artifacts stay tracked and documented.
/// </summary>
public static class TestEndToEndScienceAudit
{
    public static void TestFirstPassArtifactsAndInlineCommentsExist()
    {
        string auditPath = ProjectSettings.GlobalizePath("res://Docs/EndToEndScienceAudit.md");
        if (!File.Exists(auditPath))
        {
            throw new InvalidOperationException($"Tracked audit document not found: {auditPath}");
        }

        string auditContents = File.ReadAllText(auditPath);
        EnsureContains(auditContents, "First-pass gap status", "Docs/EndToEndScienceAudit.md");
        EnsureContains(auditContents, "Bar strength coefficient", "Docs/EndToEndScienceAudit.md");
        EnsureContains(auditContents, "inline-cited (human verification pending)", "Docs/EndToEndScienceAudit.md");
        EnsureContains(auditContents, "Second-pass inline citation sweep", "Docs/EndToEndScienceAudit.md");
        EnsureContains(auditContents, "Source-acquisition batch for one-pass human review", "Docs/EndToEndScienceAudit.md");
        EnsureContains(auditContents, "Source grounding cleanup 2026-04-24", "Docs/EndToEndScienceAudit.md");
        EnsureContains(auditContents, "Workstream B demographics intake (2026-04-26)", "Docs/EndToEndScienceAudit.md");
        EnsureContains(auditContents, "CuiEtAl2026.txt", "Docs/EndToEndScienceAudit.md");
        EnsureContains(auditContents, "Workstreams C through K source intake (2026-04-26)", "Docs/EndToEndScienceAudit.md");
        EnsureContains(auditContents, "Workstreams C–K second anchor batch (2026-04-26)", "Docs/EndToEndScienceAudit.md");
        EnsureContains(auditContents, "KunimotoEtAl2022.txt", "Docs/EndToEndScienceAudit.md");

        VerifyFileContains(
            "res://src/domain/galaxy/GalaxyRealismProfileBuilder.cs",
            new[]
            {
                "Diaz-Garcia et al. (2016)",
                "Kormendy et al. (2009)",
                "pending human verification",
                "Tuning:",
            });
        VerifyFileContains(
            "res://src/domain/generation/generators/StellarIsochroneApproximator.cs",
            new[]
            {
                "Hurley et al. (2000)",
                "pending human verification",
                "Tuning:",
            });
        VerifyFileContains(
            "res://src/domain/galaxy/SpiralDensityModel.cs",
            new[]
            {
                "Hart et al. (2017)",
                "Lingard et al. (2021)",
                "Tuning:",
            });
        VerifyFileContains(
            "res://src/domain/system/StellarConfigGenerator.cs",
            new[]
            {
                "Tokovinin (2021)",
                "Moe and Di Stefano (2017)",
                "Tuning:",
            });
        VerifyFileContains(
            "res://src/domain/generation/generators/StellarMassSampler.cs",
            new[]
            {
                "Li et al. (2023)",
                "Tuning:",
            });
        VerifyFileContains(
            "res://src/domain/generation/PlanetarySystemState.cs",
            new[]
            {
                "Tanaka, Takeuchi, and Ward (2002)",
                "Pascucci et al. (2016)",
                "Fischer and Valenti (2005)",
                "Fernandes et al. (2019)",
                "pending human verification",
                "Tuning:",
            });
        VerifyFileContains(
            "res://src/domain/system/SystemPlanetGenerator.cs",
            new[]
            {
                "Petigura et al. (2013)",
                "Fulton et al. (2017)",
                "Kopparapu et al. (2013, 2014)",
                "Ronnet and Johansen (2020)",
                "Sasaki et al. (2010)",
                "Szulagyi et al. (2018)",
                "Tuning:",
            });
        VerifyFileContains(
            "res://src/domain/population/BiologySupportEvaluator.cs",
            new[]
            {
                "speculative biochemistry branch",
                "Lineweaver and Davis (2002)",
                "Forgan and Rice (2010)",
                "pending human verification",
                "Tuning:",
            });

        string[] expectedSourceNotes =
        {
            "res://Sources/Texts/DiazGarcia2016.txt",
            "res://Sources/Texts/Hurley2000.txt",
            "res://Sources/Texts/Kormendy2009.txt",
            "res://Sources/Texts/TanakaTakeuchiWard2002.txt",
            "res://Sources/Texts/Bains2004.txt",
            "res://Sources/Texts/Behroozi2019.txt",
            "res://Sources/Texts/BlandHawthornGerhard2016.txt",
            "res://Sources/Texts/Chabrier2003.txt",
            "res://Sources/Texts/Choi2016.txt",
            "res://Sources/Texts/Conselice2014.txt",
            "res://Sources/Texts/DucheneKraus2013.txt",
            "res://Sources/Texts/Hayden2014.txt",
            "res://Sources/Texts/Kennicutt1998.txt",
            "res://Sources/Texts/Kroupa2001.txt",
            "res://Sources/Texts/Raghavan2010.txt",
            "res://Sources/Texts/WeggGerhard2013.txt",
            "res://Sources/Texts/Laskar2017.txt",
            "res://Sources/Texts/Petit2018.txt",
            "res://Sources/Texts/Obertas2017.txt",
            "res://Sources/Texts/Petit2020.txt",
            "res://Sources/Texts/Tamayo2020.txt",
            "res://Sources/Texts/Rice2023.txt",
            "res://Sources/Texts/Outland2020.txt",
            "res://Sources/Texts/Ronnet2020.txt",
            "res://Sources/Texts/Sasaki2010.txt",
            "res://Sources/Texts/Szulagyi2018.txt",
            "res://Sources/Texts/Chowdhury2022.txt",
            "res://Sources/Texts/Comin2013.txt",
            "res://Sources/Texts/CominMestieri2013.txt",
            "res://Sources/Texts/Stokey2020.txt",
            "res://Sources/Texts/CuiEtAl2026.txt",
            "res://Sources/Texts/MentCharbonneau2023.txt",
            "res://Sources/Texts/WanderleyEtAl2025.txt",
            "res://Sources/Texts/GillisEtAl2026.txt",
            "res://Sources/Texts/VanZandtEtAl2025.txt",
            "res://Sources/Texts/KunimotoEtAl2022.txt",
            "res://Sources/Texts/ChatterjeeEtAl2026.txt",
            "res://Sources/Texts/NakajimaEtAl2022.txt",
            "res://Sources/Texts/KavelaarsEtAl2023.txt",
            "res://Sources/Texts/KhoperskovEtAl2024.txt",
            "res://Sources/Texts/HuntVasiliev2025.txt",
            "res://Sources/Texts/HeEtAl2020.txt",
            "res://Sources/Texts/ObertasTamayo2023.txt",
            "res://Sources/Texts/ChabrierLenoble2023.txt",
            "res://Sources/Texts/KarakatsanisMamassis2023.txt",
            "res://Sources/Texts/BainsEtAl2024.txt",
            "res://Sources/Texts/BergstenEtAl2023.txt",
            "res://Sources/Texts/LuquePalle2022.txt",
            "res://Sources/Texts/VissapragadaEtAl2022.txt",
            "res://Sources/Texts/BiassoniEtAl2023.txt",
            "res://Sources/Texts/BenistyEtAl2021.txt",
            "res://Sources/Texts/MalamudPerets2019.txt",
            "res://Sources/Texts/NapierEtAl2023.txt",
            "res://Sources/Texts/BernardinelliEtAl2022.txt",
            "res://Sources/Texts/GarmaOehmichenEtAl2022.txt",
            "res://Sources/Texts/FangMargot2013.txt",
            "res://Sources/Texts/StevensonEtAl2023.txt",
            "res://Sources/Texts/HamiltonEtAl2016.txt",
            "res://Sources/Texts/PetkowskiEtAl2020.txt",
        };
        foreach (string sourceNote in expectedSourceNotes)
        {
            string absolutePath = ProjectSettings.GlobalizePath(sourceNote);
            if (!File.Exists(absolutePath))
            {
                throw new InvalidOperationException($"Science-audit source note missing: {absolutePath}");
            }
        }

        VerifySourceGroundingCleanup();
    }

    private static void VerifyFileContains(string resPath, IReadOnlyList<string> expectedSnippets)
    {
        string absolutePath = ProjectSettings.GlobalizePath(resPath);
        if (!File.Exists(absolutePath))
        {
            throw new InvalidOperationException($"Expected file not found: {absolutePath}");
        }

        string contents = File.ReadAllText(absolutePath);
        foreach (string snippet in expectedSnippets)
        {
            EnsureContains(contents, snippet, absolutePath);
        }
    }

    private static void EnsureContains(string contents, string expectedSnippet, string label)
    {
        if (!contents.Contains(expectedSnippet, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected snippet `{expectedSnippet}` was not found in {label}.");
        }
    }

    private static void EnsureDoesNotContain(string contents, string rejectedSnippet, string label)
    {
        if (contents.Contains(rejectedSnippet, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Rejected snippet `{rejectedSnippet}` was found in {label}.");
        }
    }

    private static void VerifySourceGroundingCleanup()
    {
        VerifyFileContains(
            "res://Sources/Texts/ChenKipping2017.txt",
            new[]
            {
                "Human reviewer concern",
                "AI response / disposition",
                "Implementation status",
                "underutilized",
                "should not be used alone for atmosphere, habitability, volatile history, formation pathway, photoevaporation state, or detailed composition",
            });
        VerifyFileContains(
            "res://Sources/Texts/Balbi2023.txt",
            new[]
            {
                "Human reviewer concern",
                "AI response / disposition",
                "Implementation status",
                "partly implemented",
            });
        VerifyFileContains(
            "res://Sources/Texts/BlandHawthornGerhard2016.txt",
            new[]
            {
                "Human reviewer concern",
                "AI response / disposition",
                "Implementation status",
                "reviewed but underutilized",
            });

        string[] draftNotes =
        {
            "res://Sources/Texts/Laskar2017.txt",
            "res://Sources/Texts/Petit2018.txt",
            "res://Sources/Texts/Obertas2017.txt",
            "res://Sources/Texts/Petit2020.txt",
            "res://Sources/Texts/Tamayo2020.txt",
            "res://Sources/Texts/Rice2023.txt",
            "res://Sources/Texts/Outland2020.txt",
            "res://Sources/Texts/Ronnet2020.txt",
            "res://Sources/Texts/Sasaki2010.txt",
            "res://Sources/Texts/Szulagyi2018.txt",
            "res://Sources/Texts/Chowdhury2022.txt",
            "res://Sources/Texts/Comin2013.txt",
            "res://Sources/Texts/CominMestieri2013.txt",
            "res://Sources/Texts/Stokey2020.txt",
            "res://Sources/Texts/CuiEtAl2026.txt",
            "res://Sources/Texts/MentCharbonneau2023.txt",
            "res://Sources/Texts/WanderleyEtAl2025.txt",
            "res://Sources/Texts/GillisEtAl2026.txt",
            "res://Sources/Texts/VanZandtEtAl2025.txt",
            "res://Sources/Texts/KunimotoEtAl2022.txt",
            "res://Sources/Texts/ChatterjeeEtAl2026.txt",
            "res://Sources/Texts/NakajimaEtAl2022.txt",
            "res://Sources/Texts/KavelaarsEtAl2023.txt",
            "res://Sources/Texts/KhoperskovEtAl2024.txt",
            "res://Sources/Texts/HuntVasiliev2025.txt",
            "res://Sources/Texts/HeEtAl2020.txt",
            "res://Sources/Texts/ObertasTamayo2023.txt",
            "res://Sources/Texts/ChabrierLenoble2023.txt",
            "res://Sources/Texts/KarakatsanisMamassis2023.txt",
            "res://Sources/Texts/BainsEtAl2024.txt",
            "res://Sources/Texts/BergstenEtAl2023.txt",
            "res://Sources/Texts/LuquePalle2022.txt",
            "res://Sources/Texts/VissapragadaEtAl2022.txt",
            "res://Sources/Texts/BiassoniEtAl2023.txt",
            "res://Sources/Texts/BenistyEtAl2021.txt",
            "res://Sources/Texts/MalamudPerets2019.txt",
            "res://Sources/Texts/NapierEtAl2023.txt",
            "res://Sources/Texts/BernardinelliEtAl2022.txt",
            "res://Sources/Texts/GarmaOehmichenEtAl2022.txt",
            "res://Sources/Texts/FangMargot2013.txt",
            "res://Sources/Texts/StevensonEtAl2023.txt",
            "res://Sources/Texts/HamiltonEtAl2016.txt",
            "res://Sources/Texts/PetkowskiEtAl2020.txt",
        };
        foreach (string draftNote in draftNotes)
        {
            VerifyDraftSourceNote(draftNote);
        }

        VerifyBibliographyTextLinksExist();
        string bibliographyPathForCleanup = ProjectSettings.GlobalizePath("res://Sources/AnnotatedBibliography.md");
        string bibliographyForCleanup = File.ReadAllText(bibliographyPathForCleanup);
        EnsureDoesNotContain(bibliographyForCleanup, "CanupWard2006.txt", bibliographyPathForCleanup);
        VerifyFileContains(
            "res://Sources/AnnotatedBibliography.md",
            new[]
            {
                "specific `10 mutual Hill radii` multiplier as an authoritative source-backed rule",
                "reviewed but underutilized",
                "BauerEtAl2017",
                "SavvidouEtAl2023",
                "EscuderoEtAl2023",
                "HamiltonEtAl2020",
                "Knez2023",
                "ChacuaEtAl2024",
                "VanKleefEtAl2023",
            });

        string planetaryCatalogPath = ProjectSettings.GlobalizePath("res://src/domain/generation/parameters/PlanetaryScienceReferenceCatalog.cs");
        string planetaryCatalog = File.ReadAllText(planetaryCatalogPath);
        EnsureDoesNotContain(planetaryCatalog, "canupward2006", planetaryCatalogPath);
        EnsureContains(planetaryCatalog, "ronnet2020", planetaryCatalogPath);
        EnsureContains(planetaryCatalog, "sasaki2010", planetaryCatalogPath);
        EnsureContains(planetaryCatalog, "szulagyi2018", planetaryCatalogPath);
    }

    private static void VerifyBibliographyTextLinksExist()
    {
        string bibliographyPath = ProjectSettings.GlobalizePath("res://Sources/AnnotatedBibliography.md");
        string bibliographyContents = File.ReadAllText(bibliographyPath);
        MatchCollection matches = Regex.Matches(bibliographyContents, @"\[Texts/([^\]]+\.txt)\]\(Texts/([^)]+\.txt)\)");
        foreach (Match match in matches)
        {
            string linkedTextPath = match.Groups[2].Value;
            string resPath = $"res://Sources/Texts/{linkedTextPath}";
            string absolutePath = ProjectSettings.GlobalizePath(resPath);
            if (!File.Exists(absolutePath))
            {
                throw new InvalidOperationException($"Bibliography references missing source note: {absolutePath}");
            }
        }
    }

    private static void VerifyDraftSourceNote(string resPath)
    {
        string absolutePath = ProjectSettings.GlobalizePath(resPath);
        if (!File.Exists(absolutePath))
        {
            throw new InvalidOperationException($"Expected file not found: {absolutePath}");
        }

        string contents = File.ReadAllText(absolutePath);
        if (contents.Contains("AI-assisted draft source note", StringComparison.Ordinal))
        {
            EnsureVerificationMarker(contents, absolutePath);
            EnsureContains(contents, "AI response / disposition", absolutePath);
            EnsureContains(contents, "Implementation status", absolutePath);
            EnsureContains(contents, "Follow-up action", absolutePath);
            return;
        }

        EnsureContains(contents, "## Source metadata", absolutePath);
        EnsureVerificationMarker(contents, absolutePath);
        EnsureContains(contents, "## StarGen applicability", absolutePath);
        EnsureContains(contents, "## Parameters, constraints, and mechanics implications", absolutePath);
        EnsureContains(contents, "## Conflicts and follow-up work", absolutePath);
    }

    private static void EnsureVerificationMarker(string contents, string label)
    {
        if (contents.Contains("Human verification required", StringComparison.OrdinalIgnoreCase)
            || contents.Contains("Human review required", StringComparison.OrdinalIgnoreCase)
            || contents.Contains("verify", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        throw new InvalidOperationException($"Expected a human-verification marker was not found in {label}.");
    }
}
