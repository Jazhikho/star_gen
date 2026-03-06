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
}
