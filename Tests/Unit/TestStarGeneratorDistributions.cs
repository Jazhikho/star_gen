#nullable enable annotations
#nullable disable warnings
using System;

namespace StarGen.Tests.Unit;

/// <summary>
/// Statistical tests for StarGenerator distributions against astrophysical expectations.
/// </summary>
public static class TestStarGeneratorDistributions
{
    /// <summary>
    /// Samples many random stars and checks the spectral-type mix is broadly realistic.
    /// </summary>
    public static void TestSpectralTypeDistributionReasonable()
    {
        GenerationStatsHarness.StarSpectralHistogram histogram = GenerationStatsHarness.SampleStarSpectralHistogram(1000, 4096);
        if (histogram.Total <= 0)
        {
            throw new InvalidOperationException("Sampling must produce at least one classified star");
        }

        double brownDwarfFraction = (double)(histogram.L + histogram.T + histogram.Y) / (double)histogram.Total;
        double mFraction = (double)histogram.M / (double)histogram.Total;
        double gkFraction = (double)(histogram.G + histogram.K) / (double)histogram.Total;
        double obafFraction = (double)(histogram.O + histogram.B + histogram.A + histogram.F) / (double)histogram.Total;
        double whiteDwarfFraction = (double)histogram.D / (double)histogram.Total;

        if (brownDwarfFraction < ScientificBenchmarks.BrownDwarfFractionMin || brownDwarfFraction > ScientificBenchmarks.BrownDwarfFractionMax)
        {
            throw new InvalidOperationException(
                $"Brown-dwarf fraction should match benchmark range [{ScientificBenchmarks.BrownDwarfFractionMin}, {ScientificBenchmarks.BrownDwarfFractionMax}], got {brownDwarfFraction}");
        }
        if (mFraction < ScientificBenchmarks.MDwarfFractionMin || mFraction > ScientificBenchmarks.MDwarfFractionMax)
        {
            throw new InvalidOperationException(
                $"M-dwarf fraction should match benchmark range [{ScientificBenchmarks.MDwarfFractionMin}, {ScientificBenchmarks.MDwarfFractionMax}], got {mFraction}");
        }
        if (gkFraction < ScientificBenchmarks.GkFractionMin || gkFraction > ScientificBenchmarks.GkFractionMax)
        {
            throw new InvalidOperationException(
                $"G+K fraction should match benchmark range [{ScientificBenchmarks.GkFractionMin}, {ScientificBenchmarks.GkFractionMax}], got {gkFraction}");
        }
        if (obafFraction < 0.0 || obafFraction > ScientificBenchmarks.ObafFractionMax)
        {
            throw new InvalidOperationException(
                $"OBAF fraction should remain within benchmark max {ScientificBenchmarks.ObafFractionMax}, got {obafFraction}");
        }
        if (whiteDwarfFraction < 0.0 || whiteDwarfFraction > ScientificBenchmarks.WhiteDwarfFractionMax)
        {
            throw new InvalidOperationException(
                $"White-dwarf fraction should remain within benchmark max {ScientificBenchmarks.WhiteDwarfFractionMax}, got {whiteDwarfFraction}");
        }
    }
}
