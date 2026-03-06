#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.App.SystemViewer;
using StarGen.Tests.Framework;

namespace StarGen.Tests.Integration;

public static class TestSystemCameraController
{
    public static void RunAll(DotNetTestRunner runner)
    {
        runner.RunNativeTest("TestSystemCameraController::test_camera_default_position", TestCameraDefaultPosition);
        runner.RunNativeTest("TestSystemCameraController::test_camera_default_height", TestCameraDefaultHeight);
        runner.RunNativeTest("TestSystemCameraController::test_focus_on_origin", TestFocusOnOrigin);
        runner.RunNativeTest("TestSystemCameraController::test_focus_on_position", TestFocusOnPosition);
        runner.RunNativeTest("TestSystemCameraController::test_set_height_clamps_min", TestSetHeightClampsMin);
        runner.RunNativeTest("TestSystemCameraController::test_set_height_clamps_max", TestSetHeightClampsMax);
        runner.RunNativeTest("TestSystemCameraController::test_set_height_valid", TestSetHeightValid);
        runner.RunNativeTest("TestSystemCameraController::test_camera_stays_above_ground", TestCameraStaysAboveGround);
        runner.RunNativeTest("TestSystemCameraController::test_camera_looks_at_origin", TestCameraLooksAtOrigin);
        runner.RunNativeTest("TestSystemCameraController::test_camera_moved_signal", TestCameraMovedSignal);
        runner.RunNativeTest("TestSystemCameraController::test_rapid_height_changes_no_nan", TestRapidHeightChangesNoNan);
        runner.RunNativeTest("TestSystemCameraController::test_focus_zero_distance", TestFocusZeroDistance);
        runner.RunNativeTest("TestSystemCameraController::test_focus_negative_distance", TestFocusNegativeDistance);
    }

    private static SystemCameraController CreateCamera()
    {
        SystemCameraController camera = new();
        camera._Ready();
        return camera;
    }

    private static void Advance(SystemCameraController camera, int frames = 1, float delta = 1.0f / 60.0f)
    {
        for (int index = 0; index < frames; index += 1)
        {
            camera._Process(delta);
        }
    }

    private static void TestCameraDefaultPosition()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            Advance(camera);
            DotNetNativeTestSuite.AssertTrue(camera.Position.Y > 0.0f, $"Camera should be above origin: {camera.Position.Y}");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }

    private static void TestCameraDefaultHeight()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            Advance(camera);
            DotNetNativeTestSuite.AssertFloatEqual(20.0f, camera.get_height(), 0.1f, "Default height should be 20");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }

    private static void TestFocusOnOrigin()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            camera.set_height(50.0f);
            Advance(camera, 10);
            camera.focus_on_origin();
            Advance(camera, 300);
            DotNetNativeTestSuite.AssertFloatEqual(20.0f, camera.get_height(), 2.0f, "Focus on origin should restore default height");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }

    private static void TestFocusOnPosition()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            Vector3 target = new(5.0f, 0.0f, 3.0f);
            camera.focus_on_position(target, 10.0f);
            Advance(camera, 60);
            Vector2 cameraXZ = new(camera.Position.X, camera.Position.Z);
            Vector2 targetXZ = new(target.X, target.Z);
            float distance = cameraXZ.DistanceTo(targetXZ);
            DotNetNativeTestSuite.AssertTrue(distance < 30.0f, $"Camera should be near target: {distance}");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }

    private static void TestSetHeightClampsMin()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            camera.set_height(0.01f);
            DotNetNativeTestSuite.AssertTrue(camera.get_height() >= camera.min_height, "Height should clamp to min");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }

    private static void TestSetHeightClampsMax()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            camera.set_height(9999.0f);
            DotNetNativeTestSuite.AssertTrue(camera.get_height() <= camera.max_height, "Height should clamp to max");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }

    private static void TestSetHeightValid()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            camera.set_height(35.0f);
            DotNetNativeTestSuite.AssertFloatEqual(35.0f, camera.get_height(), 0.01f, "Height should set");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }

    private static void TestCameraStaysAboveGround()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            Advance(camera, 30);
            DotNetNativeTestSuite.AssertTrue(camera.Position.Y > 0.0f, $"Camera should stay above ground: {camera.Position.Y}");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }

    private static void TestCameraLooksAtOrigin()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            Advance(camera, 2);
            DotNetNativeTestSuite.AssertTrue(camera.Position.Y > 0.0f, "Camera should remain above origin");
            DotNetNativeTestSuite.AssertTrue(camera.Position.Z > 0.0f, "Camera should stay offset from origin in Z");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }

    private static void TestCameraMovedSignal()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            bool received = false;
            float height = 0.0f;
            camera.CameraMoved += (_, newHeight) =>
            {
                received = true;
                height = newHeight;
            };
            Advance(camera, 10);
            DotNetNativeTestSuite.AssertTrue(received, "CameraMoved signal should emit");
            DotNetNativeTestSuite.AssertTrue(height > 0.0f, $"Height from signal should be positive: {height}");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }

    private static void TestRapidHeightChangesNoNan()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            float[] heights = [1.0f, 100.0f, 0.5f, 50.0f, 200.0f, 2.0f];
            foreach (float height in heights)
            {
                camera.set_height(height);
                Advance(camera);
                DotNetNativeTestSuite.AssertFalse(float.IsNaN(camera.Position.X), $"X should not be NaN for {height}");
                DotNetNativeTestSuite.AssertFalse(float.IsNaN(camera.Position.Y), $"Y should not be NaN for {height}");
                DotNetNativeTestSuite.AssertFalse(float.IsNaN(camera.Position.Z), $"Z should not be NaN for {height}");
            }
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }

    private static void TestFocusZeroDistance()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            camera.focus_on_position(Vector3.Zero, 0.0f);
            Advance(camera);
            DotNetNativeTestSuite.AssertTrue(camera.get_height() >= camera.min_height, "Zero-distance focus should keep valid height");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }

    private static void TestFocusNegativeDistance()
    {
        SystemCameraController camera = CreateCamera();
        try
        {
            camera.focus_on_position(new Vector3(10.0f, 0.0f, 10.0f), -5.0f);
            Advance(camera);
            DotNetNativeTestSuite.AssertTrue(camera.get_height() > 0.0f, "Negative distance should not break camera height");
        }
        finally
        {
            IntegrationTestUtils.CleanupNode(camera);
        }
    }
}
