#nullable enable annotations
#nullable disable warnings
using Godot;

namespace StarGen.Tests;

/// <summary>
/// Test scene that runs all tests and displays results in the console.
/// Run this scene directly or use --headless mode.
/// </summary>
public partial class TestScene : Node
{
	private const string CSharpTestScenePath = "res://Tests/TestSceneCSharp.tscn";

	private TestSceneCSharp? _csharpHarness;

	public override void _Ready()
	{
		GD.Print("");
		GD.Print("StarGen Test Suite");
		GD.Print("==================");
		GD.Print("");
		GD.Print("Running tests...");
		CallDeferred(MethodName.RunCSharpHarness);
	}

	private void RunCSharpHarness()
	{
		PackedScene? scene = ResourceLoader.Load<PackedScene>(CSharpTestScenePath);
		if (scene == null)
		{
			GD.PushError($"Failed to load C# test harness scene: {CSharpTestScenePath}");
			SceneTreeTimer timer = GetTree().CreateTimer(5.0);
			timer.Timeout += () => GetTree().Quit(1);
			return;
		}

		TestSceneCSharp? harness = scene.Instantiate<TestSceneCSharp>();
		if (harness == null)
		{
			GD.PushError("Failed to instantiate C# test harness scene: " + CSharpTestScenePath);
			SceneTreeTimer timer = GetTree().CreateTimer(5.0);
			timer.Timeout += () => GetTree().Quit(1);
			return;
		}

		_csharpHarness = harness;
		AddChild(harness);
		harness.Connect(TestSceneCSharp.SignalName.RunCompleted, Callable.From<int>(OnCSharpRunCompleted));
		harness.start_interactive();
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

	private async void OnCSharpRunCompleted(int exitCode)
	{
		CleanupCSharpHarness();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		SceneTreeTimer timer = GetTree().CreateTimer(5.0);
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
		GetTree().Quit(exitCode);
	}
}
