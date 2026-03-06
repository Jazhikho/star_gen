#nullable enable annotations
#nullable disable warnings
using System;
using Godot;
using StarGen.App.GalaxyViewer;
using StarGen.Domain.Galaxy;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Domain.Galaxy;

/// <summary>
/// Tests for ZoomStateMachine — state transitions and guards.
/// </summary>
public static class TestZoomStateMachine
{
    private static ZoomStateMachine _machine;

    private static void BeforeEach()
    {
        _machine = new ZoomStateMachine();
    }

    public static void TestInitialLevelIsGalaxy()
    {
        BeforeEach();
        DotNetNativeTestSuite.AssertEqual(
            (int)GalaxyCoordinates.ZoomLevel.Galaxy,
            _machine.GetCurrentLevel(),
            "Should start at galaxy level");
    }

    public static void TestZoomInFromGalaxy()
    {
        BeforeEach();
        _machine.ZoomIn();
        DotNetNativeTestSuite.AssertEqual(
            (int)GalaxyCoordinates.ZoomLevel.Quadrant,
            _machine.GetCurrentLevel(),
            "Zoom in from galaxy should go to quadrant");
    }

    public static void TestZoomOutFromGalaxyDoesNothing()
    {
        BeforeEach();
        _machine.ZoomOut();
        DotNetNativeTestSuite.AssertEqual(
            (int)GalaxyCoordinates.ZoomLevel.Galaxy,
            _machine.GetCurrentLevel(),
            "Zoom out from galaxy should stay at galaxy");
    }

    public static void TestZoomInTwice()
    {
        BeforeEach();
        _machine.ZoomIn();
        _machine.ZoomIn();
        DotNetNativeTestSuite.AssertEqual(
            (int)GalaxyCoordinates.ZoomLevel.Sector,
            _machine.GetCurrentLevel(),
            "Two zoom-ins should reach sector level");
    }

    public static void TestZoomInThreeTimes()
    {
        BeforeEach();
        _machine.ZoomIn();
        _machine.ZoomIn();
        _machine.ZoomIn();
        DotNetNativeTestSuite.AssertEqual(
            (int)GalaxyCoordinates.ZoomLevel.Subsector,
            _machine.GetCurrentLevel(),
            "Three zoom-ins should reach subsector level");
    }

    public static void TestCannotZoomPastSubsector()
    {
        BeforeEach();
        _machine.ZoomIn();
        _machine.ZoomIn();
        _machine.ZoomIn();
        _machine.ZoomIn();
        DotNetNativeTestSuite.AssertEqual(
            (int)GalaxyCoordinates.ZoomLevel.Subsector,
            _machine.GetCurrentLevel(),
            "Should not zoom past subsector");
    }

    public static void TestZoomOutFromQuadrant()
    {
        BeforeEach();
        _machine.ZoomIn();
        _machine.ZoomOut();
        DotNetNativeTestSuite.AssertEqual(
            (int)GalaxyCoordinates.ZoomLevel.Galaxy,
            _machine.GetCurrentLevel(),
            "Zoom out from quadrant should return to galaxy");
    }

    public static void TestCanZoomInFromGalaxy()
    {
        BeforeEach();
        DotNetNativeTestSuite.AssertTrue(_machine.CanZoomIn(), "Should be able to zoom in from galaxy");
    }

    public static void TestCannotZoomOutFromGalaxy()
    {
        BeforeEach();
        DotNetNativeTestSuite.AssertFalse(_machine.CanZoomOut(), "Should not be able to zoom out from galaxy");
    }

    public static void TestCanZoomInFromSector()
    {
        BeforeEach();
        _machine.ZoomIn();
        _machine.ZoomIn();
        DotNetNativeTestSuite.AssertTrue(_machine.CanZoomIn(), "Should be able to zoom in from sector");
    }

    public static void TestCannotZoomInFromSubsector()
    {
        BeforeEach();
        _machine.ZoomIn();
        _machine.ZoomIn();
        _machine.ZoomIn();
        DotNetNativeTestSuite.AssertFalse(_machine.CanZoomIn(), "Should not be able to zoom in from subsector");
    }

    public static void TestTransitionToSpecificLevel()
    {
        BeforeEach();
        _machine.TransitionTo((int)GalaxyCoordinates.ZoomLevel.Sector);
        DotNetNativeTestSuite.AssertEqual(
            (int)GalaxyCoordinates.ZoomLevel.Sector,
            _machine.GetCurrentLevel(),
            "Direct transition should set level");
    }

    public static void TestTransitionToSameLevelIsNoop()
    {
        BeforeEach();
        _machine.TransitionTo((int)GalaxyCoordinates.ZoomLevel.Galaxy);
        DotNetNativeTestSuite.AssertEqual(
            (int)GalaxyCoordinates.ZoomLevel.Galaxy,
            _machine.GetCurrentLevel(),
            "Transition to current level should be a no-op");
    }
}
