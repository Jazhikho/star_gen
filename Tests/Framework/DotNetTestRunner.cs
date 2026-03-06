#nullable enable annotations
#nullable disable warnings
using System.Threading.Tasks;
using Godot;

namespace StarGen.Tests.Framework;

/// <summary>
/// C# test runner for the primary native test suites.
/// </summary>
public partial class DotNetTestRunner : RefCounted
{
    private const int ConsoleProgressInterval = 25;
    private const string RunLogPath = "user://test_run_progress.log";

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
    private FileAccess? _runLogFile;

    /// <summary>
    /// Runs the headless-safe suite manifest.
    /// </summary>
    public Task<Godot.Collections.Array<DotNetTestResult>> RunHeadless()
    {
        ResetRunState();
        TestRegistry.RunHeadlessSuites(this);
        CompleteRun();
        return Task.FromResult(_results);
    }

    /// <summary>
    /// Runs the full interactive suite manifest.
    /// </summary>
    public Task<Godot.Collections.Array<DotNetTestResult>> RunInteractive()
    {
        ResetRunState();
        TestRegistry.RunInteractiveSuites(this);
        CompleteRun();
        return Task.FromResult(_results);
    }

    /// <summary>
    /// Returns the number of failed tests.
    /// </summary>
    public int GetFailCount()
    {
        return _failCount;
    }

    /// <summary>
    /// Returns the total executed test count.
    /// </summary>
    public int GetTotalCount()
    {
        return _totalCount;
    }

    /// <summary>
    /// Returns the number of passing tests.
    /// </summary>
    public int GetPassCount()
    {
        return _passCount;
    }

    /// <summary>
    /// Prints the same summary shape as the legacy runner.
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
        if (_failCount > 0)
        {
            GD.Print("SOME TESTS FAILED");
        }
        else
        {
            GD.Print("ALL TESTS PASSED");
        }

        GD.Print(string.Empty);
        GD.Print("(Report complete.)");
        GD.Print($"Detailed test log: {ProjectSettings.GlobalizePath(RunLogPath)}");
    }

    /// <summary>
    /// Executes one native C# test using the shared result pipeline.
    /// </summary>
    internal void RunNativeTest(string fullName, System.Action testAction)
    {
        EmitSignal(SignalName.TestStarted, fullName);
        WriteLogLine($"START {fullName}");

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

    private void ResetRunState()
    {
        _results.Clear();
        _totalCount = 0;
        _passCount = 0;
        _failCount = 0;
        _progressBuffer = string.Empty;
        InitializeRunLog();
    }

    private void CompleteRun()
    {
        FlushProgress();
        CloseRunLog();
        EmitSignal(SignalName.AllTestsFinished);
    }

    private void RecordCompletedResult(DotNetTestResult result)
    {
        _results.Add(result);
        _totalCount += 1;
        if (result.Passed)
        {
            _passCount += 1;
            WriteLogLine($"PASS {result.TestName} ({result.TimeMs:0.0}ms)");
            if ((_totalCount % ConsoleProgressInterval) == 0)
            {
                GD.Print($"Completed {_totalCount} tests. Passed: {_passCount}. Failed: {_failCount}.");
            }
        }
        else
        {
            _failCount += 1;
            WriteLogLine($"FAIL {result.TestName} ({result.TimeMs:0.0}ms) :: {result.Message}");
            GD.Print($"[FAIL] {result.TestName} ({result.TimeMs:0.0}ms)");
            if (!string.IsNullOrEmpty(result.Message))
            {
                GD.Print($"       -> {result.Message}");
            }
        }

        EmitSignal(SignalName.TestFinished, result);
    }

    private void FlushProgress()
    {
        if (!string.IsNullOrEmpty(_progressBuffer))
        {
            GD.Print(_progressBuffer);
            _progressBuffer = string.Empty;
        }
    }

    private void InitializeRunLog()
    {
        CloseRunLog();
        _runLogFile = FileAccess.Open(RunLogPath, FileAccess.ModeFlags.Write);
        if (_runLogFile == null)
        {
            return;
        }

        WriteLogLine("StarGen test run started");
        WriteLogLine(string.Empty);
    }

    private void CloseRunLog()
    {
        if (_runLogFile == null)
        {
            return;
        }

        _runLogFile.Flush();
        _runLogFile = null;
    }

    private void WriteLogLine(string line)
    {
        if (_runLogFile == null)
        {
            return;
        }

        _runLogFile.StoreLine(line);
        _runLogFile.Flush();
    }
}
