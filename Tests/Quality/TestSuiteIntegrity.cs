#nullable enable annotations
#nullable disable warnings
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Godot;

namespace StarGen.Tests.Quality;

/// <summary>
/// Meta-tests that keep the C# suite from regressing into obvious fake-pass patterns.
/// </summary>
public static class TestSuiteIntegrity
{
    private static readonly Regex FakePassPattern = new Regex(
        @"AssertTrue\s*\(\s*true\b|AssertFalse\s*\(\s*false\b",
        RegexOptions.Compiled);

    public static void TestNoBlatantFakePassAssertions()
    {
        string testsPath = ProjectSettings.GlobalizePath("res://Tests");
        if (!Directory.Exists(testsPath))
        {
            throw new InvalidOperationException($"Tests directory not found: {testsPath}");
        }

        List<string> offenders = new List<string>();
        string[] testFiles = Directory.GetFiles(testsPath, "*.cs", SearchOption.AllDirectories);
        foreach (string testFile in testFiles)
        {
            string contents = File.ReadAllText(testFile);
            if (!FakePassPattern.IsMatch(contents))
            {
                continue;
            }

            offenders.Add(Path.GetFileName(testFile));
        }

        if (offenders.Count > 0)
        {
            string joined = string.Join(", ", offenders);
            throw new InvalidOperationException($"Found fake-pass assertions in C# tests: {joined}");
        }
    }
}
