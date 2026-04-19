#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.App.Audio;
using StarGen.Services.Persistence;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

/// <summary>
/// Non-visual integration tests for shared application audio wiring.
/// </summary>
public static class TestAppAudio
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestAppAudio::test_main_app_exposes_shared_intro_audio", TestMainAppExposesSharedIntroAudio);
        runner.RunNativeTest("TestAppAudio::test_main_app_respects_skip_intro_preference", TestMainAppRespectsSkipIntroPreference);
    }

    private static void TestMainAppExposesSharedIntroAudio()
    {
        MainApp app = IntegrationTestUtils.CreateMainAppReady();

        AppAudioController? audioController = app.GetNodeOrNull<AppAudioController>("AudioController");
        SplashScreen? splashScreen = app.GetNodeOrNull<SplashScreen>("ViewerContainer/SplashScreen");
        AudioStreamPlayer? musicPlayer = app.GetNodeOrNull<AudioStreamPlayer>("AudioController/MusicPlayer");
        AudioStreamPlayer? uiPlayer = app.GetNodeOrNull<AudioStreamPlayer>("AudioController/UiPlayer");
        AudioStreamPlayer? legacySplashPlayer = app.GetNodeOrNull<AudioStreamPlayer>("ViewerContainer/SplashScreen/IntroMusicPlayer");

        DotNetNativeTestSuite.AssertNotNull(audioController, "MainApp should own a shared audio controller node");
        DotNetNativeTestSuite.AssertNotNull(splashScreen, "MainApp should still create the splash screen");
        DotNetNativeTestSuite.AssertNotNull(musicPlayer, "Audio controller should expose a shared music player");
        DotNetNativeTestSuite.AssertNotNull(uiPlayer, "Audio controller should expose a shared UI player");
        DotNetNativeTestSuite.AssertNull(legacySplashPlayer, "Splash screen should no longer own a private intro music player");

        audioController!._Ready();

        DotNetNativeTestSuite.AssertNotNull(audioController.Library, "Audio controller should have a shared audio library resource");
        DotNetNativeTestSuite.AssertTrue(audioController.HasCue(AppAudioCueId.IntroMusic), "Shared audio library should configure the intro music cue");
        DotNetNativeTestSuite.AssertNotNull(audioController.Library!.GetStream(AppAudioCueId.IntroMusic), "Intro music cue should point at a real audio stream");

        IntegrationTestUtils.CleanupNode(app);
    }

    private static void TestMainAppRespectsSkipIntroPreference()
    {
        StudioUiPreferencesService.Save(new StudioUiPreferencesService.StudioUiPreferences(false, true));

        try
        {
            MainApp app = IntegrationTestUtils.CreateMainAppReady();
            SplashScreen? splashScreen = app.GetNodeOrNull<SplashScreen>("ViewerContainer/SplashScreen");
            MainMenuScreen? mainMenuScreen = app.GetNodeOrNull<MainMenuScreen>("ViewerContainer/MainMenuScreen");

            DotNetNativeTestSuite.AssertNull(splashScreen, "Splash screen should not be attached when Skip Intro is enabled");
            DotNetNativeTestSuite.AssertNotNull(mainMenuScreen, "Main menu should open immediately when Skip Intro is enabled");

            IntegrationTestUtils.CleanupNode(app);
        }
        finally
        {
            StudioUiPreferencesService.Save(StudioUiPreferencesService.CreateDefault());
        }
    }
}
