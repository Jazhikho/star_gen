#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestSplashScreen
{
    private const string ScenePath = "res://src/app/SplashScreen.tscn";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestSplashScreen::test_splash_uses_user_facing_release_version", TestSplashUsesUserFacingReleaseVersion);
    }

    private static void TestSplashUsesUserFacingReleaseVersion()
    {
        SplashScreen splash = IntegrationTestUtils.InstantiateScene<SplashScreen>(ScenePath);
        try
        {
            splash._Ready();

            Label? versionLabel = splash.GetNodeOrNull<Label>("MarginContainer/ScrollContainer/Layout/WordmarkBlock/VersionLabel");
            DotNetNativeTestSuite.AssertNotNull(versionLabel, "Splash screen should expose the release version label");
            DotNetNativeTestSuite.AssertTrue(versionLabel!.Text.Contains("0.8.0.0"), "Splash screen should show the upcoming public release label");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(splash);
        }
    }
}
