#nullable enable annotations
#nullable disable warnings
using System.Threading.Tasks;
using Godot;
using StarGen.Tests.Framework;

namespace StarGen.Tests;

/// <summary>
/// Primary C# test harness for headless and interactive suite execution.
/// </summary>
public partial class TestSceneCSharp : Node
{
	/// <summary>
	/// Emitted when a test starts.
	/// </summary>
	[Signal]
	public delegate void TestStartedEventHandler(string testName);

	/// <summary>
	/// Emitted when a test finishes.
	/// </summary>
	[Signal]
	public delegate void TestFinishedEventHandler(string testName, bool passed, float timeMs, string message);

	/// <summary>
	/// Emitted when the run completes with the final exit code.
	/// </summary>
	[Signal]
	public delegate void RunCompletedEventHandler(int exitCode);

	private readonly DotNetTestRunner _runner = new();

	public override void _Ready()
	{
		_runner.Connect(
			DotNetTestRunner.SignalName.TestStarted,
			Callable.From<string>(OnRunnerTestStarted));
		_runner.Connect(
			DotNetTestRunner.SignalName.TestFinished,
			Callable.From<DotNetTestResult>(OnRunnerTestFinished));
	}

	/// <summary>
	/// Starts the headless-safe test suite.
	/// </summary>
	public void start_headless()
	{
		_ = RunHeadlessAsync();
	}

	/// <summary>
	/// Starts the full interactive test suite.
	/// </summary>
	public void start_interactive()
	{
		_ = RunInteractiveAsync();
	}

	private async Task RunHeadlessAsync()
	{
		GD.Print("Using C# test harness");
		GD.Print(string.Empty);
		await _runner.RunHeadless();
		_runner.PrintSummary();
		int exitCode = 0;
		if (_runner.GetFailCount() != 0)
		{
			exitCode = 1;
		}

		EmitSignal(SignalName.RunCompleted, exitCode);
	}

	private async Task RunInteractiveAsync()
	{
		GD.Print("Using C# test harness");
		GD.Print(string.Empty);
		await _runner.RunInteractive();
		_runner.PrintSummary();
		int exitCode = 0;
		if (_runner.GetFailCount() != 0)
		{
			exitCode = 1;
		}

		EmitSignal(SignalName.RunCompleted, exitCode);
	}

	private void OnRunnerTestStarted(string testName)
	{
		EmitSignal(SignalName.TestStarted, testName);
	}

	private void OnRunnerTestFinished(DotNetTestResult result)
	{
		EmitSignal(SignalName.TestFinished, result.TestName, result.Passed, result.TimeMs, result.Message);
	}
}
