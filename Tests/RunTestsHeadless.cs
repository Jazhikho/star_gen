#nullable enable annotations
#nullable disable warnings
using System.Threading.Tasks;
using Godot;

namespace StarGen.Tests;

/// <summary>
/// C# half of the headless runner, loaded via the GDScript launcher.
/// </summary>
public partial class RunTestsHeadless : SceneTree
{
    private const string CSharpTestScenePath = "res://Tests/TestSceneCSharp.tscn";

    private TestSceneCSharp? _csharpHarness;

    public override void _Initialize()
    {
        GD.Print("");
        GD.Print("StarGen Test Suite (Headless)");
        GD.Print("==============================");
        GD.Print("");
        GD.Print("Running tests...");

        CallDeferred(MethodName.StartCSharpHarness);
    }

    private void StartCSharpHarness()
    {
        _ = RunCSharpHarnessAsync();
    }

    private async Task RunCSharpHarnessAsync()
    {
        await Task.Yield();
        PackedScene? scene = ResourceLoader.Load<PackedScene>(CSharpTestScenePath);
        if (scene == null)
        {
            GD.PushError("Failed to load C# test harness scene: " + CSharpTestScenePath);
            Quit(1);
            return;
        }

        TestSceneCSharp? harness = scene.Instantiate<TestSceneCSharp>();
        if (harness == null)
        {
            GD.PushError("Failed to instantiate C# test harness scene: " + CSharpTestScenePath);
            Quit(1);
            return;
        }

        _csharpHarness = harness;
        Root.AddChild(harness);
        harness.Connect(TestSceneCSharp.SignalName.RunCompleted, Callable.From<int>(OnCSharpHeadlessCompleted));
        harness.start_headless();
    }

    private void CleanupCSharpHarness()
    {
        if (_csharpHarness == null)
        {
            return;
        }

        _csharpHarness.QueueFree();
        _csharpHarness = null;
    }

    private async void OnCSharpHeadlessCompleted(int exitCode)
    {
        CleanupCSharpHarness();
        foreach (Node child in Root.GetChildren())
        {
            child.QueueFree();
        }

        await ToSignal(this, SignalName.ProcessFrame);
        await ToSignal(this, SignalName.ProcessFrame);
        await ToSignal(this, SignalName.ProcessFrame);
        Quit(exitCode);
    }
}
