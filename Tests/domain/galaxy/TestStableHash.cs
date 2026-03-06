#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using Godot.Collections;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Domain.Galaxy;

/// <summary>
/// Tests for StableHash — determinism, collision resistance, derivation.
/// </summary>
public static class TestStableHash
{
    public static void TestSameInputSameOutput()
    {
        long a = StableHash.HashIntegers(new Array<long> { 1, 2, 3 });
        long b = StableHash.HashIntegers(new Array<long> { 1, 2, 3 });
        DotNetNativeTestSuite.AssertEqual(a, b, "Identical inputs must produce identical hashes");
    }

    public static void TestDifferentInputsDifferentOutput()
    {
        long a = StableHash.HashIntegers(new Array<long> { 1, 2, 3 });
        long b = StableHash.HashIntegers(new Array<long> { 3, 2, 1 });
        DotNetNativeTestSuite.AssertNotEqual(a, b, "Different inputs should produce different hashes");
    }

    public static void TestOrderMatters()
    {
        long a = StableHash.HashIntegers(new Array<long> { 100, 200 });
        long b = StableHash.HashIntegers(new Array<long> { 200, 100 });
        DotNetNativeTestSuite.AssertNotEqual(a, b, "Input order must affect the hash");
    }

    public static void TestHashIsPositive()
    {
        long h = StableHash.HashIntegers(new Array<long> { 0, -1, 999999 });
        DotNetNativeTestSuite.AssertGreaterThan(h, -1L, "Hash should be non-negative (32-bit masked)");
    }

    public static void TestDeriveSeedDeterministic()
    {
        long a = StableHash.DeriveSeed(42, new Vector3I(10, 20, 30));
        long b = StableHash.DeriveSeed(42, new Vector3I(10, 20, 30));
        DotNetNativeTestSuite.AssertEqual(a, b, "Same parent + coords must give same child seed");
    }

    public static void TestDeriveSeedVariesWithCoords()
    {
        long a = StableHash.DeriveSeed(42, new Vector3I(0, 0, 0));
        long b = StableHash.DeriveSeed(42, new Vector3I(1, 0, 0));
        long c = StableHash.DeriveSeed(42, new Vector3I(0, 1, 0));
        DotNetNativeTestSuite.AssertNotEqual(a, b, "Different x should give different seed");
        DotNetNativeTestSuite.AssertNotEqual(a, c, "Different y should give different seed");
        DotNetNativeTestSuite.AssertNotEqual(b, c, "Different coords should give different seeds");
    }

    public static void TestDeriveSeedVariesWithParent()
    {
        long a = StableHash.DeriveSeed(1, new Vector3I(5, 5, 5));
        long b = StableHash.DeriveSeed(2, new Vector3I(5, 5, 5));
        DotNetNativeTestSuite.AssertNotEqual(a, b, "Different parent seeds must give different children");
    }

    public static void TestDeriveSeedIndexedDeterministic()
    {
        long a = StableHash.DeriveSeedIndexed(99, 7);
        long b = StableHash.DeriveSeedIndexed(99, 7);
        DotNetNativeTestSuite.AssertEqual(a, b, "Same parent + index must give same child seed");
    }

    public static void TestKnownValueStability()
    {
        long h = StableHash.HashIntegers(new Array<long> { 42 });
        DotNetNativeTestSuite.AssertGreaterThan(h, 0L, "Known-input hash must be positive");
        DotNetNativeTestSuite.AssertLessThan(h, 0xFFFFFFFFL + 1, "Known-input hash must fit 32 bits");
    }
}
