#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using StarGen.Domain.Generation.Parameters;

namespace StarGen.Tests.Quality;

/// <summary>
/// Verifies that science-facing tuning entries remain linked to controls, citations, and acquisition status.
/// </summary>
public static class TestScienceTuningRegistry
{
    public static void TestRegistryEntriesHaveControlsAndCitations()
    {
        foreach (ScienceTuningEntry entry in ScienceTuningRegistry.GetEntries())
        {
            if (string.IsNullOrWhiteSpace(entry.Id))
            {
                throw new InvalidOperationException("Science tuning entry is missing an id.");
            }

            if (entry.SourceIds.Count == 0)
            {
                throw new InvalidOperationException($"Science tuning entry '{entry.Id}' has no source ids.");
            }

            if (string.IsNullOrWhiteSpace(entry.ValueOrRange))
            {
                throw new InvalidOperationException($"Science tuning entry '{entry.Id}' has no value or range metadata.");
            }

            if (string.IsNullOrWhiteSpace(entry.PublicControlId))
            {
                throw new InvalidOperationException($"Science tuning entry '{entry.Id}' has no public control id.");
            }

            ParameterMaterialityRegistry.Entry? materiality = ParameterMaterialityRegistry.GetEntry(entry.PublicControlId);
            if (materiality == null)
            {
                throw new InvalidOperationException($"Science tuning entry '{entry.Id}' points to unsurfaced control '{entry.PublicControlId}'.");
            }

            if (string.IsNullOrWhiteSpace(GetTooltip(entry.PublicControlId)))
            {
                throw new InvalidOperationException($"Science tuning control '{entry.PublicControlId}' has no tooltip citation surface.");
            }

            foreach (string sourceId in entry.SourceIds)
            {
                if (!SourceResolves(sourceId))
                {
                    throw new InvalidOperationException($"Science tuning entry '{entry.Id}' references unresolved source '{sourceId}'.");
                }

                SourceAcquisitionRecord? acquisitionRecord = SourceAcquisitionRegistry.GetRecord(sourceId);
                if (acquisitionRecord == null)
                {
                    throw new InvalidOperationException($"Science tuning entry '{entry.Id}' references source '{sourceId}' without acquisition metadata.");
                }
            }
        }
    }

    public static void TestSourceAcquisitionMetadataUsedByNewControlsIsExplicit()
    {
        HashSet<string> sourceIds = new();
        foreach (ScienceTuningEntry entry in ScienceTuningRegistry.GetEntries())
        {
            foreach (string sourceId in entry.SourceIds)
            {
                sourceIds.Add(sourceId);
            }
        }

        foreach (string sourceId in sourceIds)
        {
            SourceAcquisitionRecord? acquisitionRecord = SourceAcquisitionRegistry.GetRecord(sourceId);
            if (acquisitionRecord == null)
            {
                throw new InvalidOperationException($"Source '{sourceId}' has no acquisition record.");
            }

            if (string.IsNullOrWhiteSpace(acquisitionRecord.AuditNote))
            {
                throw new InvalidOperationException($"Source '{sourceId}' has no acquisition audit note.");
            }

            if (acquisitionRecord.Status == SourceAcquisitionStatus.LocalSourceNote
                || acquisitionRecord.Status == SourceAcquisitionStatus.PendingReacquisition)
            {
                if (string.IsNullOrWhiteSpace(acquisitionRecord.TextNoteResPath))
                {
                    throw new InvalidOperationException($"Source '{sourceId}' is marked local or pending but has no text-note path.");
                }

                string absolutePath = ProjectSettings.GlobalizePath(acquisitionRecord.TextNoteResPath);
                if (!File.Exists(absolutePath))
                {
                    throw new InvalidOperationException($"Source '{sourceId}' points to a missing source note: {absolutePath}");
                }
            }

            if ((acquisitionRecord.Status == SourceAcquisitionStatus.AcquisitionBlocked
                    || acquisitionRecord.Status == SourceAcquisitionStatus.BackgroundCatalogOnly)
                && !string.IsNullOrWhiteSpace(acquisitionRecord.TextNoteResPath))
            {
                throw new InvalidOperationException($"Source '{sourceId}' is marked blocked/background-only but still has a text-note path.");
            }
        }
    }

    public static void TestScienceCatalogParametersHaveRegistryAndHelpCoverage()
    {
        VerifyGalaxyCatalogCoverage();
        VerifyStellarCatalogCoverage();
        VerifyPlanetaryCatalogCoverage();
        VerifyLifeCatalogCoverage();
        VerifySentientCatalogCoverage();
    }

    private static string GetTooltip(string parameterId)
    {
        string tooltip = GalaxyScienceReferenceCatalog.GetTooltipSummary(parameterId);
        if (!string.IsNullOrWhiteSpace(tooltip))
        {
            return tooltip;
        }

        tooltip = StellarScienceReferenceCatalog.GetTooltipSummary(parameterId);
        if (!string.IsNullOrWhiteSpace(tooltip))
        {
            return tooltip;
        }

        tooltip = PlanetaryScienceReferenceCatalog.GetTooltipSummary(parameterId);
        if (!string.IsNullOrWhiteSpace(tooltip))
        {
            return tooltip;
        }

        tooltip = LifeScienceReferenceCatalog.GetTooltipSummary(parameterId);
        if (!string.IsNullOrWhiteSpace(tooltip))
        {
            return tooltip;
        }

        return SentientScienceReferenceCatalog.GetTooltipSummary(parameterId);
    }

    private static bool SourceResolves(string sourceId)
    {
        return GalaxyScienceReferenceCatalog.GetSource(sourceId) != null
            || StellarScienceReferenceCatalog.GetSource(sourceId) != null
            || PlanetaryScienceReferenceCatalog.GetSource(sourceId) != null
            || LifeScienceReferenceCatalog.GetSource(sourceId) != null
            || SentientScienceReferenceCatalog.GetSource(sourceId) != null
            || ObjectScienceReferenceCatalog.GetSource(sourceId) != null;
    }

    private static void VerifyGalaxyCatalogCoverage()
    {
        foreach (GalaxyScienceParameterReference reference in GalaxyScienceReferenceCatalog.GetParameterReferences())
        {
            VerifyCatalogReference(reference.ParameterId, reference.SourceIds, GalaxyScienceReferenceCatalog.GetSciencePanelSourceIds());
        }
    }

    private static void VerifyStellarCatalogCoverage()
    {
        foreach (StellarScienceParameterReference reference in StellarScienceReferenceCatalog.GetParameterReferences())
        {
            VerifyCatalogReference(reference.ParameterId, reference.SourceIds, StellarScienceReferenceCatalog.GetHelpPanelSourceIds());
        }
    }

    private static void VerifyPlanetaryCatalogCoverage()
    {
        foreach (PlanetaryScienceParameterReference reference in PlanetaryScienceReferenceCatalog.GetParameterReferences())
        {
            VerifyCatalogReference(reference.ParameterId, reference.SourceIds, PlanetaryScienceReferenceCatalog.GetHelpPanelSourceIds());
        }
    }

    private static void VerifyLifeCatalogCoverage()
    {
        foreach (LifeScienceParameterReference reference in LifeScienceReferenceCatalog.GetParameterReferences())
        {
            VerifyCatalogReference(reference.ParameterId, reference.SourceIds, LifeScienceReferenceCatalog.GetHelpPanelSourceIds());
        }
    }

    private static void VerifySentientCatalogCoverage()
    {
        foreach (SentientScienceParameterReference reference in SentientScienceReferenceCatalog.GetParameterReferences())
        {
            VerifyCatalogReference(reference.ParameterId, reference.SourceIds, SentientScienceReferenceCatalog.GetHelpPanelSourceIds());
        }
    }

    private static void VerifyCatalogReference(
        string parameterId,
        IReadOnlyList<string> sourceIds,
        IReadOnlyList<string> helpPanelSourceIds)
    {
        if (string.IsNullOrWhiteSpace(GetTooltip(parameterId)))
        {
            throw new InvalidOperationException($"Science parameter '{parameterId}' has no tooltip.");
        }

        IReadOnlyList<ScienceTuningEntry> tuningEntries = ScienceTuningRegistry.GetEntriesForPublicControl(parameterId);
        if (tuningEntries.Count == 0)
        {
            throw new InvalidOperationException($"Science parameter '{parameterId}' has no science tuning registry entry.");
        }

        foreach (string sourceId in sourceIds)
        {
            if (!ContainsSourceId(helpPanelSourceIds, sourceId))
            {
                throw new InvalidOperationException($"Science parameter '{parameterId}' cites source '{sourceId}' but that source is not listed in its help panel.");
            }

            if (!RegistryEntriesContainSource(tuningEntries, sourceId))
            {
                throw new InvalidOperationException($"Science parameter '{parameterId}' cites source '{sourceId}' but the tuning registry entry does not include it.");
            }

            if (SourceAcquisitionRegistry.GetRecord(sourceId) == null)
            {
                throw new InvalidOperationException($"Science parameter '{parameterId}' cites source '{sourceId}' without source-acquisition metadata.");
            }
        }
    }

    private static bool ContainsSourceId(IReadOnlyList<string> sourceIds, string sourceId)
    {
        foreach (string candidate in sourceIds)
        {
            if (candidate == sourceId)
            {
                return true;
            }
        }

        return false;
    }

    private static bool RegistryEntriesContainSource(IReadOnlyList<ScienceTuningEntry> entries, string sourceId)
    {
        foreach (ScienceTuningEntry entry in entries)
        {
            foreach (string candidate in entry.SourceIds)
            {
                if (candidate == sourceId)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
