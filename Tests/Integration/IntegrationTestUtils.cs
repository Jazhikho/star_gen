#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App;
using StarGen.App.GalaxyViewer;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Math;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

internal static class IntegrationTestUtils
{
    internal static PackedScene LoadPackedScene(string scenePath)
    {
        PackedScene? scene = ResourceLoader.Load<PackedScene>(scenePath);
        DotNetNativeTestSuite.AssertNotNull(scene, $"Scene should load: {scenePath}");
        return scene!;
    }

    internal static T InstantiateScene<T>(string scenePath) where T : Node
    {
        PackedScene scene = LoadPackedScene(scenePath);
        Node? node = scene.Instantiate();
        DotNetNativeTestSuite.AssertNotNull(node, $"Scene should instantiate: {scenePath}");
        DotNetNativeTestSuite.AssertTrue(node is T, $"Scene root should be {typeof(T).Name}: {scenePath}");
        return (T)node;
    }

    internal static MainApp CreateMainAppReady()
    {
        MainApp app = InstantiateScene<MainApp>("res://src/app/MainApp.tscn");
        app._Ready();
        return app;
    }

    internal static MainApp CreateMainAppReadyAndStarted()
    {
        MainApp app = CreateMainAppReady();
        app.start_galaxy_with_defaults();
        GalaxyViewer? viewer = app.get_galaxy_viewer();
        if (viewer != null)
        {
            viewer._Ready();
        }
        return app;
    }

    internal static CelestialBody CreateTestBody(
        string id = "test_body",
        string name = "Test Body",
        CelestialType.Type type = CelestialType.Type.Planet,
        double massKg = Units.EarthMassKg,
        double radiusM = Units.EarthRadiusMeters)
    {
        PhysicalProps physical = new(massKg, radiusM, 86400.0, 23.5);
        CelestialBody body = new(id, name, type, physical, Provenance.CreateCurrent(12345));
        return body;
    }

    internal static void CleanupNode(Node? node)
    {
        if (node == null || !GodotObject.IsInstanceValid(node))
        {
            return;
        }

        node.QueueFree();
    }
}
