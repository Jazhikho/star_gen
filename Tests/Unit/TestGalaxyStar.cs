#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for GalaxyStar class.
/// </summary>
public static class TestGalaxyStar
{
    /// <summary>
    /// Tests basic creation.
    /// </summary>
    public static void TestBasicCreation()
    {
        GalaxyStar star = new GalaxyStar(new Vector3(100.0f, 50.0f, 200.0f), 12345);
        DotNetNativeTestSuite.AssertEqual(12345, star.StarSeed, "Seed should match");
        if (!star.Position.IsEqualApprox(new Vector3(100.0f, 50.0f, 200.0f)))
        {
            throw new InvalidOperationException("Position should match");
        }
        DotNetNativeTestSuite.AssertEqual(1.0, star.Metallicity, "Default metallicity should be 1.0");
        DotNetNativeTestSuite.AssertEqual(1.0, star.AgeBias, "Default age bias should be 1.0");
    }

    /// <summary>
    /// Tests create with derived properties.
    /// </summary>
    public static void TestCreateWithDerivedProperties()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);
        GalaxyStar star = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(1000.0f, 0.0f, 0.0f), 99999, spec
        );
        DotNetNativeTestSuite.AssertEqual(99999, star.StarSeed, "Seed should match");
        if (star.Metallicity <= 0.5)
        {
            throw new InvalidOperationException("Metallicity should be derived");
        }
        if (star.Metallicity >= 5.0)
        {
            throw new InvalidOperationException("Metallicity should be reasonable");
        }
    }

    /// <summary>
    /// Tests metallicity gradient radial.
    /// </summary>
    public static void TestMetallicityGradientRadial()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);

        GalaxyStar centerStar = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(500.0f, 0.0f, 0.0f), 1, spec
        );

        GalaxyStar solarStar = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(8000.0f, 0.0f, 0.0f), 2, spec
        );

        GalaxyStar outerStar = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(15000.0f, 0.0f, 0.0f), 3, spec
        );

        if (centerStar.Metallicity <= solarStar.Metallicity)
        {
            throw new InvalidOperationException("Center star should have higher metallicity than solar-distance star");
        }
        if (solarStar.Metallicity <= outerStar.Metallicity)
        {
            throw new InvalidOperationException("Solar-distance star should have higher metallicity than outer star");
        }
    }

    /// <summary>
    /// Tests metallicity gradient vertical.
    /// </summary>
    public static void TestMetallicityGradientVertical()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);

        GalaxyStar diskStar = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(5000.0f, 0.0f, 0.0f), 1, spec
        );

        GalaxyStar haloStar = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(5000.0f, 2000.0f, 0.0f), 2, spec
        );

        if (diskStar.Metallicity <= haloStar.Metallicity)
        {
            throw new InvalidOperationException("Disk star should have higher metallicity than halo star");
        }
    }

    /// <summary>
    /// Tests age bias bulge.
    /// </summary>
    public static void TestAgeBiasBulge()
    {
        GalaxySpec spec = GalaxySpec.CreateMilkyWay(42);

        GalaxyStar bulgeStar = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(500.0f, 200.0f, 0.0f), 1, spec
        );

        GalaxyStar diskStar = GalaxyStar.CreateWithDerivedProperties(
            new Vector3(10000.0f, 0.0f, 0.0f), 2, spec
        );

        if (bulgeStar.AgeBias <= diskStar.AgeBias)
        {
            throw new InvalidOperationException("Bulge star should have higher age bias than disk star");
        }
    }

    /// <summary>
    /// Tests distance helpers.
    /// </summary>
    public static void TestDistanceHelpers()
    {
        GalaxyStar star = new GalaxyStar(new Vector3(3.0f, 4.0f, 0.0f), 1);
        DotNetNativeTestSuite.AssertEqual(5.0, star.GetDistanceFromCenter(), "Distance from center should be 5");
        DotNetNativeTestSuite.AssertEqual(3.0, star.GetRadialDistance(), "Radial distance should be 3");
        DotNetNativeTestSuite.AssertEqual(4.0, star.GetHeight(), "Height should be 4");
    }

    /// <summary>
    /// Tests to string.
    /// </summary>
    public static void TestToString()
    {
        GalaxyStar star = new GalaxyStar(new Vector3(100.0f, 0.0f, 0.0f), 42);
        string s = star.ToString();
        if (!s.Contains("42"))
        {
            throw new InvalidOperationException("String should contain seed");
        }
    }
}
