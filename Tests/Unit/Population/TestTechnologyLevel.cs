#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Population;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit.Population;

/// <summary>
/// Tests for TechnologyLevel enum and helpers.
/// </summary>
public static class TestTechnologyLevel
{
    /// <summary>
    /// Tests to_string_name.
    /// </summary>
    public static void TestToStringName()
    {
        DotNetNativeTestSuite.AssertEqual("Stone Age", TechnologyLevel.ToStringName(TechnologyLevel.Level.StoneAge), "StoneAge string should match");
        DotNetNativeTestSuite.AssertEqual("Industrial", TechnologyLevel.ToStringName(TechnologyLevel.Level.Industrial), "Industrial string should match");
        DotNetNativeTestSuite.AssertEqual("Interstellar", TechnologyLevel.ToStringName(TechnologyLevel.Level.Interstellar), "Interstellar string should match");
    }

    /// <summary>
    /// Tests from_string.
    /// </summary>
    public static void TestFromString()
    {
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.StoneAge, TechnologyLevel.FromString("stone_age"), "Should parse stone_age");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Industrial, TechnologyLevel.FromString("Industrial"), "Should parse Industrial");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.StoneAge, TechnologyLevel.FromString("invalid"), "Invalid should return StoneAge");
    }

    /// <summary>
    /// Tests next_level.
    /// </summary>
    public static void TestNextLevel()
    {
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.BronzeAge, TechnologyLevel.NextLevel(TechnologyLevel.Level.StoneAge), "StoneAge next should be BronzeAge");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Atomic, TechnologyLevel.NextLevel(TechnologyLevel.Level.Industrial), "Industrial next should be Atomic");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Advanced, TechnologyLevel.NextLevel(TechnologyLevel.Level.Advanced), "Advanced should stay at Advanced");
    }

    /// <summary>
    /// Tests previous_level.
    /// </summary>
    public static void TestPreviousLevel()
    {
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.StoneAge, TechnologyLevel.PreviousLevel(TechnologyLevel.Level.BronzeAge), "BronzeAge previous should be StoneAge");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.StoneAge, TechnologyLevel.PreviousLevel(TechnologyLevel.Level.StoneAge), "StoneAge should stay at StoneAge");
    }

    /// <summary>
    /// Tests can_spaceflight.
    /// </summary>
    public static void TestCanSpaceflight()
    {
        DotNetNativeTestSuite.AssertFalse(TechnologyLevel.CanSpaceflight(TechnologyLevel.Level.StoneAge), "StoneAge should not have spaceflight");
        DotNetNativeTestSuite.AssertFalse(TechnologyLevel.CanSpaceflight(TechnologyLevel.Level.Industrial), "Industrial should not have spaceflight");
        DotNetNativeTestSuite.AssertFalse(TechnologyLevel.CanSpaceflight(TechnologyLevel.Level.Information), "Information should not have spaceflight");
        DotNetNativeTestSuite.AssertTrue(TechnologyLevel.CanSpaceflight(TechnologyLevel.Level.Spacefaring), "Spacefaring should have spaceflight");
        DotNetNativeTestSuite.AssertTrue(TechnologyLevel.CanSpaceflight(TechnologyLevel.Level.Interstellar), "Interstellar should have spaceflight");
    }

    /// <summary>
    /// Tests can_interstellar.
    /// </summary>
    public static void TestCanInterstellar()
    {
        DotNetNativeTestSuite.AssertFalse(TechnologyLevel.CanInterstellar(TechnologyLevel.Level.Spacefaring), "Spacefaring should not have interstellar");
        DotNetNativeTestSuite.AssertTrue(TechnologyLevel.CanInterstellar(TechnologyLevel.Level.Interstellar), "Interstellar should have interstellar");
        DotNetNativeTestSuite.AssertTrue(TechnologyLevel.CanInterstellar(TechnologyLevel.Level.Advanced), "Advanced should have interstellar");
    }

    /// <summary>
    /// Tests typical_years_to_reach increases monotonically.
    /// </summary>
    public static void TestTypicalYearsToReachIncreases()
    {
        int prevYears = -1;
        for (int i = 0; i < TechnologyLevel.Count(); i++)
        {
            TechnologyLevel.Level level = (TechnologyLevel.Level)i;
            int years = TechnologyLevel.TypicalYearsToReach(level);
            DotNetNativeTestSuite.AssertGreaterThan(years, prevYears - 1, "Years should generally increase");
            prevYears = years;
        }
    }

    /// <summary>
    /// Tests count.
    /// </summary>
    public static void TestCount()
    {
        DotNetNativeTestSuite.AssertEqual(12, TechnologyLevel.Count(), "Should have 12 technology levels");
    }

    /// <summary>
    /// Tests neutral core technology levels map to stable era bands.
    /// </summary>
    public static void TestCoreLevelToEra()
    {
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.StoneAge, TechnologyLevel.CoreLevelToEra(0), "Core 0 should map to Stone");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.StoneAge, TechnologyLevel.CoreLevelToEra(1), "Core 1 should map to Stone");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.BronzeAge, TechnologyLevel.CoreLevelToEra(2), "Core 2 should map to Bronze");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.IronAge, TechnologyLevel.CoreLevelToEra(3), "Core 3 should map to Iron");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Classical, TechnologyLevel.CoreLevelToEra(4), "Core 4 should map to Classical");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Medieval, TechnologyLevel.CoreLevelToEra(5), "Core 5 should map to Medieval");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Renaissance, TechnologyLevel.CoreLevelToEra(6), "Core 6 should map to Renaissance");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Industrial, TechnologyLevel.CoreLevelToEra(7), "Core 7 should map to Industrial");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Atomic, TechnologyLevel.CoreLevelToEra(8), "Core 8 should map to Atomic");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Information, TechnologyLevel.CoreLevelToEra(9), "Core 9 should map to Information");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Spacefaring, TechnologyLevel.CoreLevelToEra(11), "Core 11 should map to Spacefaring");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Interstellar, TechnologyLevel.CoreLevelToEra(15), "Core 15 should map to Interstellar");
        DotNetNativeTestSuite.AssertEqual(TechnologyLevel.Level.Advanced, TechnologyLevel.CoreLevelToEra(24), "Core 24 should map to Advanced");
    }

    /// <summary>
    /// Tests neutral core technology levels map to Traveller/Cepheus codes deterministically.
    /// </summary>
    public static void TestCoreLevelToTravellerTechLevel()
    {
        int[] expected =
        [
            0,
            1,
            1,
            2,
            3,
            4,
            5,
            6,
            6,
            7,
            8,
            9,
            10,
            10,
            11,
            12,
            12,
            13,
            13,
            14,
            14,
            15,
            15,
            15,
            15,
        ];

        for (int coreLevel = 0; coreLevel < expected.Length; coreLevel += 1)
        {
            DotNetNativeTestSuite.AssertEqual(
                expected[coreLevel],
                TechnologyLevel.CoreLevelToTravellerTechLevel(coreLevel),
                $"Core {coreLevel} should map to Traveller TL {expected[coreLevel]}");
        }
    }
}
