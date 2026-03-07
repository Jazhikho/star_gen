#nullable enable annotations
#nullable disable warnings
using System;
using System.IO;
using Godot;
using Godot.Collections;
using StarGen.App.GalaxyViewer;
using StarGen.App.Rendering;
using StarGen.App.SystemViewer;
using StarGen.App.Viewer;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Galaxy;
using StarGen.Domain.Generation;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Generation.Generators;
using StarGen.Domain.Generation.Specs;
using StarGen.Domain.Math;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;
using StarGen.Services.Persistence;
using StarGen.Tests.Integration;

namespace StarGen.Tests.Framework;

public static partial class DotNetNativeTestSuite
{
    /// <summary>
    /// Runs the headless-safe integration tests.
    /// </summary>
    public static void RunHeadlessIntegrationTests(DotNetTestRunner runner)
    {
        TestCelestialPersistence.RunAll(runner);
        TestGalaxyPersistence.RunAll(runner);
        TestSaveLoad.RunAll(runner);
        TestSystemPersistence.RunAll(runner);
        TestPopulationGoldenMasters.RunAll(runner);
        TestPopulationIntegration.RunAll(runner);
        TestGenerationParameters.RunAll(runner);
        TestWindowSettingsService.RunAll(runner);
        TestObjectViewerMoons.RunAll(runner);
        TestViewerLayoutHelper.RunAll(runner);
        TestGalaxyViewerUI.RunAll(runner);
        TestObjectViewer.RunAll(runner);
        TestSystemViewer.RunAll(runner);
        TestSystemViewerSaveLoad.RunAll(runner);
    }

    /// <summary>
    /// Runs the interactive-only integration tests.
    /// </summary>
    public static void RunSceneOnlyIntegrationTests(DotNetTestRunner runner)
    {
        TestGalaxyRandomization.RunAll(runner);
        TestGalaxyStartup.RunAll(runner);
        TestGalaxySystemTransition.RunAll(runner);
        TestGalaxyViewerHome.RunAll(runner);
        TestMainApp.RunAll(runner);
        TestMainAppNavigation.RunAll(runner);
        TestStarSystemPreviewIntegration.RunAll(runner);
        TestSystemCameraController.RunAll(runner);
        TestWelcomeScreen.RunAll(runner);
    }
}
