using Godot;

namespace StarGen.Tests.Framework;

/// <summary>
/// Result of a single test execution in the C# test harness.
/// </summary>
public partial class DotNetTestResult : RefCounted
{
    /// <summary>
    /// Name of the executed test.
    /// </summary>
    public string TestName = string.Empty;

    /// <summary>
    /// Whether the test passed.
    /// </summary>
    public bool Passed = true;

    /// <summary>
    /// Failure message when the test fails.
    /// </summary>
    public string Message = string.Empty;

    /// <summary>
    /// Execution time in milliseconds.
    /// </summary>
    public float TimeMs;

    /// <summary>
    /// Creates a populated test result.
    /// </summary>
    public DotNetTestResult()
    {
    }

    /// <summary>
    /// Creates a populated test result.
    /// </summary>
    public DotNetTestResult(string testName, bool passed, string message, float timeMs)
    {
        TestName = testName;
        Passed = passed;
        Message = message;
        TimeMs = timeMs;
    }
}
