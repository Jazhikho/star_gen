using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace StarGen.Tests.Framework;

/// <summary>
/// C# test runner that executes the existing GDScript test scripts.
/// </summary>
public partial class DotNetTestRunner : RefCounted
{
    private static readonly HashSet<string> NativeCoveredScriptNames =
    [
        "TestStableHash",
        "TestSeedDeriver",
        "TestColorUtils",
        "TestMaterialFactory",
        "TestSystemScaleManager",
        "TestSystemDisplayLayout",
        "TestGalaxyConfig",
        "TestGalaxyCoordinates",
        "TestSpiralDensityModel",
        "TestDensitySampler",
        "TestZoomStateMachine",
        "TestRaycastUtils",
        "TestQuadrantSelector",
        "TestGridCursor",
        "TestGalaxyStar",
        "TestSubSectorGenerator",
        "TestStarPicker",
        "TestSubSectorNeighborhood",
        "TestStarSystemPreview",
        "TestGenerationRealismProfile",
        "TestHomePosition",
        "TestHierarchyNode",
        "TestStarGenerator",
        "TestPlanetGenerator",
        "TestMoonGenerator",
        "TestAsteroidGenerator",
        "TestRingSystemGenerator",
        "TestCelestialSerializer",
        "TestSystemSerializer",
        "TestOrbitalMechanics",
        "TestGalaxySystemGenerator",
        "TestOrbitHost",
        "TestSystemHierarchy",
        "TestGalaxySaveData",
        "TestGalaxyBodyOverrides",
        "TestSolarSystem",
        "TestSolarSystemSpec",
        "TestSystemCache",
        "TestSector",
        "TestGalaxy",
        "TestSystemViewerSaveLoad",
        "TestSystemViewer",
        "TestObjectViewer",
        "TestObjectViewerMoons",
        "TestMainApp",
        "TestMainAppNavigation",
        "TestGalaxyViewerUI",
        "TestGalaxyViewerHome",
        "TestGalaxySystemTransition",
        "TestGalaxyRandomization",
        "TestGalaxyStartup",
        "TestStarSystemPreviewIntegration",
    ];

    /// <summary>
    /// Emitted when a test starts.
    /// </summary>
    [Signal]
    public delegate void TestStartedEventHandler(string testName);

    /// <summary>
    /// Emitted when a test finishes.
    /// </summary>
    [Signal]
    public delegate void TestFinishedEventHandler(DotNetTestResult result);

    /// <summary>
    /// Emitted when all tests finish.
    /// </summary>
    [Signal]
    public delegate void AllTestsFinishedEventHandler();

    private readonly Godot.Collections.Array<DotNetTestResult> _results = [];
    private int _totalCount;
    private int _passCount;
    private int _failCount;
    private string _progressBuffer = string.Empty;

    /// <summary>
    /// Runs all provided GDScript test scripts.
    /// </summary>
    public async Task<Godot.Collections.Array<DotNetTestResult>> RunAll(
        Godot.Collections.Array testScripts,
        SceneTree? sceneTree = null)
    {
        _results.Clear();
        _totalCount = 0;
        _passCount = 0;
        _failCount = 0;
        _progressBuffer = string.Empty;

        DotNetNativeTestSuite.RunAll(this);

        foreach (Variant scriptVariant in testScripts)
        {
            GDScript? script = scriptVariant.AsGodotObject() as GDScript;
            if (script == null)
            {
                continue;
            }

            if (IsCoveredByNativeSuite(script))
            {
                continue;
            }

            await RunTestScriptAsync(script, sceneTree);
        }

        FlushProgress();
        EmitSignal(SignalName.AllTestsFinished);
        return _results;
    }

    /// <summary>
    /// Returns the number of failed tests.
    /// </summary>
    public int GetFailCount()
    {
        return _failCount;
    }

    /// <summary>
    /// Prints the same summary shape as the GDScript runner.
    /// </summary>
    public void PrintSummary()
    {
        string divider = new('=', 60);
        string separator = new('-', 60);

        GD.Print(string.Empty);
        GD.Print(string.Empty);
        GD.Print(divider);
        GD.Print("TEST SUMMARY");
        GD.Print(divider);

        if (_failCount > 0)
        {
            GD.Print(string.Empty);
            GD.Print("FAILED TESTS:");
            GD.Print(separator);
            foreach (DotNetTestResult result in _results)
            {
                if (!result.Passed)
                {
                    GD.Print($"[FAIL] {result.TestName} ({result.TimeMs:0.0}ms)");
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        GD.Print($"       -> {result.Message}");
                    }
                }
            }

            GD.Print(string.Empty);
        }

        GD.Print(separator);
        GD.Print($"Total: {_totalCount} | Passed: {_passCount} | Failed: {_failCount}");
        GD.Print(divider);
        GD.Print(_failCount > 0 ? "SOME TESTS FAILED" : "ALL TESTS PASSED");
        GD.Print(string.Empty);
        GD.Print("(Report complete.)");
    }

    /// <summary>
    /// Executes one native C# test using the same result pipeline as script-backed tests.
    /// </summary>
    internal void RunNativeTest(string fullName, System.Action testAction)
    {
        EmitSignal(SignalName.TestStarted, fullName);

        ulong startTime = Time.GetTicksMsec();
        string message = string.Empty;
        bool passed = true;

        try
        {
            testAction();
        }
        catch (System.Exception exception)
        {
            passed = false;
            message = exception.Message;
        }

        ulong endTime = Time.GetTicksMsec();
        float timeMs = (float)(endTime - startTime);
        RecordCompletedResult(new DotNetTestResult(fullName, passed, message, timeMs));
    }

    private async Task RunTestScriptAsync(GDScript script, SceneTree? sceneTree)
    {
        GodotObject? testInstance = InstantiateTest(script);
        if (testInstance == null)
        {
            GD.PushError($"Script does not produce a test instance: {script.ResourcePath}");
            return;
        }

        if (sceneTree != null)
        {
            testInstance.Set("runner_scene_tree", sceneTree);
        }

        List<string> testMethods = GetTestMethods(testInstance);
        if (testMethods.Count == 0)
        {
            return;
        }

        testInstance.Call("before_all");

        foreach (string methodName in testMethods)
        {
            await RunSingleTestAsync(testInstance, methodName, script.ResourcePath, sceneTree);
        }

        testInstance.Call("after_all");
    }

    private static GodotObject? InstantiateTest(GDScript script)
    {
        Variant instanceVariant = script.Call("new");
        return instanceVariant.VariantType == Variant.Type.Nil ? null : instanceVariant.AsGodotObject();
    }

    private static bool IsCoveredByNativeSuite(GDScript script)
    {
        string scriptName = script.ResourcePath.GetFile().GetBaseName();
        return NativeCoveredScriptNames.Contains(scriptName);
    }

    private static List<string> GetTestMethods(GodotObject instance)
    {
        List<string> methods = [];
        Variant methodListVariant = instance.Call("get_method_list");
        if (methodListVariant.VariantType != Variant.Type.Array)
        {
            return methods;
        }

        Godot.Collections.Array methodList = (Godot.Collections.Array)methodListVariant;
        foreach (Variant methodInfoVariant in methodList)
        {
            if (methodInfoVariant.VariantType != Variant.Type.Dictionary)
            {
                continue;
            }

            Godot.Collections.Dictionary methodInfo = (Godot.Collections.Dictionary)methodInfoVariant;
            if (!methodInfo.ContainsKey("name"))
            {
                continue;
            }

            Variant nameVariant = methodInfo["name"];
            if (nameVariant.VariantType != Variant.Type.String)
            {
                continue;
            }

            string methodName = (string)nameVariant;
            if (methodName.StartsWith("test_"))
            {
                methods.Add(methodName);
            }
        }

        return methods;
    }

    private async Task RunSingleTestAsync(
        GodotObject instance,
        string methodName,
        string scriptPath,
        SceneTree? sceneTree)
    {
        string fullName = $"{scriptPath.GetFile().GetBaseName()}::{methodName}";
        EmitSignal(SignalName.TestStarted, fullName);

        instance.Call("_reset_failure_state");
        instance.Call("before_each");

        if (sceneTree != null && NeedsDeferredFrames(scriptPath))
        {
            await ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
            await ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
            await ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
        }

        ulong startTime = Time.GetTicksMsec();
        Variant callResult = instance.Call(methodName);
        await AwaitIfNeeded(callResult);
        ulong endTime = Time.GetTicksMsec();

        instance.Call("after_each");
        RecordResult(instance, fullName, startTime, endTime);
    }

    private async Task AwaitIfNeeded(Variant callResult)
    {
        if (callResult.VariantType != Variant.Type.Object)
        {
            return;
        }

        GodotObject? awaitable = callResult.AsGodotObject();
        if (awaitable == null)
        {
            return;
        }

        if (awaitable.HasSignal("completed"))
        {
            await ToSignal(awaitable, "completed");
        }
    }

    private static bool NeedsDeferredFrames(string scriptPath)
    {
        return scriptPath.Contains("TestMainAppNavigation")
            || scriptPath.Contains("TestGalaxyViewerUI")
            || scriptPath.Contains("TestGalaxyRandomization")
            || scriptPath.Contains("TestWelcomeScreen")
            || scriptPath.Contains("TestGalaxyStartup")
            || scriptPath.Contains("TestStarSystemPreviewIntegration");
    }

    private void RecordResult(GodotObject instance, string fullName, ulong startTime, ulong endTime)
    {
        float timeMs = (float)(endTime - startTime);
        bool passed = !ReadBool(instance, "has_failed");
        string message = ReadString(instance, "get_failure_message");

        RecordCompletedResult(new DotNetTestResult(fullName, passed, message, timeMs));
    }

    /// <summary>
    /// Records an already-executed result and updates the aggregate counters.
    /// </summary>
    private void RecordCompletedResult(DotNetTestResult result)
    {
        _results.Add(result);
        _totalCount += 1;
        if (result.Passed)
        {
            _passCount += 1;
            if (result.TimeMs > 5000.0f)
            {
                GD.Print($"Running: {result.TestName} ({result.TimeMs / 1000.0f:0.0}s)");
            }

            _progressBuffer += ".";
            if (_progressBuffer.Length >= 50)
            {
                FlushProgress();
            }
        }
        else
        {
            _failCount += 1;
            FlushProgress();
            GD.Print($"[FAIL] {result.TestName} ({result.TimeMs:0.0}ms)");
            if (!string.IsNullOrEmpty(result.Message))
            {
                GD.Print($"       -> {result.Message}");
            }
        }

        EmitSignal(SignalName.TestFinished, result);
    }

    private static bool ReadBool(GodotObject instance, string methodName)
    {
        Variant value = instance.Call(methodName);
        return value.VariantType == Variant.Type.Bool && (bool)value;
    }

    private static string ReadString(GodotObject instance, string methodName)
    {
        Variant value = instance.Call(methodName);
        return value.VariantType == Variant.Type.String ? (string)value : string.Empty;
    }

    private void FlushProgress()
    {
        if (!string.IsNullOrEmpty(_progressBuffer))
        {
            GD.Print(_progressBuffer);
            _progressBuffer = string.Empty;
        }
    }
}
