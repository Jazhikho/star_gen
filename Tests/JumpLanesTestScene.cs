#nullable enable annotations
#nullable disable warnings
using Godot;
using StarGen.Tests.Framework;

namespace StarGen.Tests;

/// <summary>
/// Test scene for running jump-lane tests with visual output.
/// </summary>
public partial class JumpLanesTestScene : Control
{
    private readonly DotNetTestRunner _runner = new();
    private Label? _statusLabel;
    private RichTextLabel? _resultsText;

    public override void _Ready()
    {
        _statusLabel = GetNodeOrNull<Label>("VBoxContainer/Status");
        _resultsText = GetNodeOrNull<RichTextLabel>("VBoxContainer/Results");
        _runner.Connect(DotNetTestRunner.SignalName.TestFinished, Callable.From<DotNetTestResult>(OnTestFinished));
        RunTests();
    }

    private async void RunTests()
    {
        if (_resultsText == null)
        {
            return;
        }

        _resultsText.Clear();
        _resultsText.AppendText("[b]Jump Lanes Test Suite[/b]\n");
        _resultsText.AppendText(new string('=', 50) + "\n\n");

        DotNetNativeTestSuite.RunJumpLanesTests(_runner);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        ShowSummary();
    }

    private void OnTestFinished(DotNetTestResult result)
    {
        if (_resultsText == null)
        {
            return;
        }

        if (result.Passed)
        {
            _resultsText.AppendText($"[color=green]PASS[/color] {result.TestName} ({result.TimeMs:0.1}ms)\n");
            return;
        }

        _resultsText.AppendText($"[color=red]FAIL[/color] {result.TestName} ({result.TimeMs:0.1}ms)\n");
        _resultsText.AppendText($"  [color=red]{result.Message}[/color]\n");
    }

    private void ShowSummary()
    {
        if (_resultsText == null)
        {
            return;
        }

        _resultsText.AppendText("\n" + new string('=', 50) + "\n");
        _resultsText.AppendText("[b]Summary:[/b] ");

        int total = _runner.GetTotalCount();
        int passed = _runner.GetPassCount();
        int failed = _runner.GetFailCount();

        if (_statusLabel != null)
        {
            if (failed == 0)
            {
                _statusLabel.Text = "All tests passed!";
            }
            else
            {
                _statusLabel.Text = $"{failed} test(s) failed";
            }
        }

        if (failed == 0)
        {
            _resultsText.AppendText($"[color=green]{passed}/{total} passed[/color]\n");
            return;
        }

        _resultsText.AppendText($"[color=red]{passed}/{total} passed, {failed} failed[/color]\n");
    }
}
