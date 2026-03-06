#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Domain.Galaxy;

/// <summary>
/// Tests for SeedDeriver — determinism and hierarchy correctness.
/// </summary>
public static class TestSeedDeriver
{
    public static void TestQuadrantSeedDeterministic()
    {
        long a = SeedDeriver.DeriveQuadrantSeed(42, new Vector3I(3, 1, 7));
        long b = SeedDeriver.DeriveQuadrantSeed(42, new Vector3I(3, 1, 7));
        DotNetNativeTestSuite.AssertEqual(a, b, "Same galaxy seed + coords must give same quadrant seed");
    }

    public static void TestQuadrantSeedVariesWithCoords()
    {
        long a = SeedDeriver.DeriveQuadrantSeed(42, new Vector3I(0, 0, 0));
        long b = SeedDeriver.DeriveQuadrantSeed(42, new Vector3I(1, 0, 0));
        long c = SeedDeriver.DeriveQuadrantSeed(42, new Vector3I(0, 1, 0));
        long d = SeedDeriver.DeriveQuadrantSeed(42, new Vector3I(0, 0, 1));
        DotNetNativeTestSuite.AssertNotEqual(a, b, "Different x should give different seed");
        DotNetNativeTestSuite.AssertNotEqual(a, c, "Different y should give different seed");
        DotNetNativeTestSuite.AssertNotEqual(a, d, "Different z should give different seed");
    }

    public static void TestQuadrantSeedVariesWithGalaxySeed()
    {
        long a = SeedDeriver.DeriveQuadrantSeed(1, new Vector3I(5, 5, 5));
        long b = SeedDeriver.DeriveQuadrantSeed(2, new Vector3I(5, 5, 5));
        DotNetNativeTestSuite.AssertNotEqual(a, b, "Different galaxy seeds must give different quadrant seeds");
    }

    public static void TestSectorSeedDeterministic()
    {
        long a = SeedDeriver.DeriveSectorSeed(999, new Vector3I(4, 5, 6));
        long b = SeedDeriver.DeriveSectorSeed(999, new Vector3I(4, 5, 6));
        DotNetNativeTestSuite.AssertEqual(a, b, "Same quadrant seed + coords must give same sector seed");
    }

    public static void TestSectorSeedVariesWithLocalCoords()
    {
        long a = SeedDeriver.DeriveSectorSeed(999, new Vector3I(0, 0, 0));
        long b = SeedDeriver.DeriveSectorSeed(999, new Vector3I(9, 9, 9));
        DotNetNativeTestSuite.AssertNotEqual(a, b, "Different sector coords should give different seeds");
    }

    public static void TestSubsectorSeedDeterministic()
    {
        long a = SeedDeriver.DeriveSubsectorSeed(777, new Vector3I(2, 3, 4));
        long b = SeedDeriver.DeriveSubsectorSeed(777, new Vector3I(2, 3, 4));
        DotNetNativeTestSuite.AssertEqual(a, b, "Same sector seed + coords must give same subsector seed");
    }

    public static void TestStarSeedDeterministic()
    {
        long a = SeedDeriver.DeriveStarSeed(555, 0);
        long b = SeedDeriver.DeriveStarSeed(555, 0);
        DotNetNativeTestSuite.AssertEqual(a, b, "Same subsector seed + index must give same star seed");
    }

    public static void TestStarSeedVariesWithIndex()
    {
        long a = SeedDeriver.DeriveStarSeed(555, 0);
        long b = SeedDeriver.DeriveStarSeed(555, 1);
        long c = SeedDeriver.DeriveStarSeed(555, 2);
        DotNetNativeTestSuite.AssertNotEqual(a, b, "Different star indices should give different seeds");
        DotNetNativeTestSuite.AssertNotEqual(b, c, "Different star indices should give different seeds");
    }

    public static void TestFullChainDeterministic()
    {
        long a = SeedDeriver.DeriveSectorSeedFull(42, new Vector3I(3, 0, -2), new Vector3I(5, 5, 5));
        long b = SeedDeriver.DeriveSectorSeedFull(42, new Vector3I(3, 0, -2), new Vector3I(5, 5, 5));
        DotNetNativeTestSuite.AssertEqual(a, b, "Full chain must be deterministic");
    }

    public static void TestFullChainSubsectorDeterministic()
    {
        long a = SeedDeriver.DeriveSubsectorSeedFull(
            42, new Vector3I(1, 0, 1), new Vector3I(3, 3, 3), new Vector3I(7, 7, 7));
        long b = SeedDeriver.DeriveSubsectorSeedFull(
            42, new Vector3I(1, 0, 1), new Vector3I(3, 3, 3), new Vector3I(7, 7, 7));
        DotNetNativeTestSuite.AssertEqual(a, b, "Full subsector chain must be deterministic");
    }

    public static void TestFullChainVariesWithQuadrant()
    {
        long a = SeedDeriver.DeriveSectorSeedFull(42, new Vector3I(0, 0, 0), new Vector3I(5, 5, 5));
        long b = SeedDeriver.DeriveSectorSeedFull(42, new Vector3I(1, 0, 0), new Vector3I(5, 5, 5));
        DotNetNativeTestSuite.AssertNotEqual(a, b, "Different quadrants must produce different sector seeds");
    }

    public static void TestHierarchyIndependence()
    {
        long qSeed = SeedDeriver.DeriveQuadrantSeed(42, new Vector3I(5, 5, 5));
        long sSeedA = SeedDeriver.DeriveSectorSeed(qSeed, new Vector3I(0, 0, 0));
        long sSeedB = SeedDeriver.DeriveSectorSeed(qSeed, new Vector3I(0, 0, 1));

        DotNetNativeTestSuite.AssertNotEqual(qSeed, sSeedA, "Sector seed should differ from parent quadrant seed");
        DotNetNativeTestSuite.AssertNotEqual(sSeedA, sSeedB, "Adjacent sectors should have different seeds");
    }
}
