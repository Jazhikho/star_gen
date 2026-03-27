#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.App.Shared;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestSplashScreen
{
    private const string ScenePath = "res://src/app/SplashScreen.tscn";

    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestSplashScreen::test_splash_uses_user_facing_release_version", TestSplashUsesUserFacingReleaseVersion);
        runner.RunNativeTest("TestSplashScreen::test_splash_wires_video_logo_and_music_nodes", TestSplashWiresVideoLogoAndMusicNodes);
    }

    private static void TestSplashUsesUserFacingReleaseVersion()
    {
        SplashScreen splash = IntegrationTestUtils.InstantiateScene<SplashScreen>(ScenePath);
        try
        {
            splash._Ready();

            Label? versionLabel = splash.GetNodeOrNull<Label>("CenterStage/StageVBox/LogoVBox/VersionLabel");
            DotNetNativeTestSuite.AssertNotNull(versionLabel, "Splash screen should expose the release version label");
            DotNetNativeTestSuite.AssertTrue(
                versionLabel!.Text.Contains(UserFacingVersionHelper.GetDisplayVersion()),
                "Splash screen should show the upcoming public release label");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(splash);
        }
    }

    private static void TestSplashWiresVideoLogoAndMusicNodes()
    {
        SplashScreen splash = IntegrationTestUtils.InstantiateScene<SplashScreen>(ScenePath);
        try
        {
            splash._Ready();

            Control? mediaFrame = splash.GetNodeOrNull<Control>("CenterStage/StageVBox/MediaFrame");
            Control? videoLayer = splash.GetNodeOrNull<Control>("CenterStage/StageVBox/MediaFrame/VideoLayer");
            VideoStreamPlayer? videoPlayer = splash.GetNodeOrNull<VideoStreamPlayer>("CenterStage/StageVBox/MediaFrame/VideoLayer/VideoPlayer");
            CenterContainer? logoLayer = splash.GetNodeOrNull<CenterContainer>("CenterStage/StageVBox/MediaFrame/LogoLayer");
            TextureRect? logoTexture = splash.GetNodeOrNull<TextureRect>("CenterStage/StageVBox/MediaFrame/LogoLayer/LogoTexture");
            AudioStreamPlayer? introMusicPlayer = splash.GetNodeOrNull<AudioStreamPlayer>("IntroMusicPlayer");
            Label? skipLabel = splash.GetNodeOrNull<Label>("SkipLabel");

            DotNetNativeTestSuite.AssertNotNull(mediaFrame, "Splash screen should define the shared media frame");
            DotNetNativeTestSuite.AssertNotNull(videoLayer, "Splash screen should define the intro video layer");
            DotNetNativeTestSuite.AssertNotNull(videoPlayer, "Splash screen should define the intro video player");
            DotNetNativeTestSuite.AssertNotNull(logoLayer, "Splash screen should define the logo transition layer");
            DotNetNativeTestSuite.AssertNotNull(logoTexture, "Splash screen should define the logo texture surface");
            DotNetNativeTestSuite.AssertNotNull(introMusicPlayer, "Splash screen should keep an audio hook ready for intro music");
            DotNetNativeTestSuite.AssertNotNull(skipLabel, "Splash screen should expose skip guidance");
            DotNetNativeTestSuite.AssertNotNull(videoPlayer!.Stream, "Splash screen should load the root intro video");
            DotNetNativeTestSuite.AssertNotNull(logoTexture!.Texture, "Splash screen should load the StarGen logo texture");
            DotNetNativeTestSuite.AssertNotNull(introMusicPlayer!.Stream, "Splash screen should load the root intro music");
            DotNetNativeTestSuite.AssertTrue(Mathf.IsEqualApprox(1.0f, videoPlayer.AnchorRight), "Intro video should fill the shared media frame width");
            DotNetNativeTestSuite.AssertTrue(Mathf.IsEqualApprox(1.0f, videoPlayer.AnchorBottom), "Intro video should fill the shared media frame height");
            DotNetNativeTestSuite.AssertTrue(Mathf.IsEqualApprox(512.0f, mediaFrame!.CustomMinimumSize.Y), "Intro video should share the logo height");
            DotNetNativeTestSuite.AssertTrue(
                Mathf.IsEqualApprox(512.0f * (16.0f / 9.0f), mediaFrame.CustomMinimumSize.X),
                "Intro video should preserve the source aspect ratio while matching the logo height");
            DotNetNativeTestSuite.AssertTrue(Mathf.IsZeroApprox(logoLayer!.Modulate.A), "Logo layer should start hidden until the video finishes");
            DotNetNativeTestSuite.AssertFalse(Mathf.IsZeroApprox(videoLayer!.Modulate.A), "Video layer should start visible before the logo fade");
            DotNetNativeTestSuite.AssertTrue(skipLabel!.Text.Contains("skip"), "Skip guidance should remain visible during the intro");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(splash);
        }
    }
}
