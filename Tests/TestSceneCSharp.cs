using System.Threading.Tasks;
using Godot;
using StarGen.Tests.Framework;

namespace StarGen.Tests;

/// <summary>
/// Parallel C# test harness that runs the existing GDScript test scripts.
/// </summary>
public partial class TestSceneCSharp : Node
{
	/// <summary>
	/// Emitted when the run completes with the final exit code.
	/// </summary>
	[Signal]
	public delegate void RunCompletedEventHandler(int exitCode);

	private readonly DotNetTestRunner _runner = new();

	/// <summary>
	/// Starts a full test run using the provided GDScript test list.
	/// </summary>
	public void start_run(Godot.Collections.Array testScripts, SceneTree sceneTree)
	{
		_ = RunAsync(testScripts, sceneTree);
	}

	private async Task RunAsync(Godot.Collections.Array testScripts, SceneTree sceneTree)
	{
		GD.Print("Using C# test harness");
		GD.Print(string.Empty);
		await _runner.RunAll(testScripts, sceneTree);
		_runner.PrintSummary();
		EmitSignal(SignalName.RunCompleted, _runner.GetFailCount() == 0 ? 0 : 1);
	}
}
