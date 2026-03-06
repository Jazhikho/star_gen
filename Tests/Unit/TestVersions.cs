#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain;
using StarGen.Domain.Constants;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for version constants.
/// </summary>
public static class TestVersions
{
    /// <summary>
    /// Tests that GENERATOR_VERSION is a non-empty string.
    /// </summary>
    public static void TestGeneratorVersionIsNonEmptyString()
    {
        if (Versions.GeneratorVersion.Length == 0)
        {
            throw new InvalidOperationException("GENERATOR_VERSION should not be empty");
        }
    }

    /// <summary>
    /// Tests that GENERATOR_VERSION follows semver format (x.y.z).
    /// </summary>
    public static void TestGeneratorVersionIsSemverFormat()
    {
        string[] parts = Versions.GeneratorVersion.Split(".");
        if (parts.Length != 3)
        {
            throw new InvalidOperationException($"GENERATOR_VERSION should have 3 parts (x.y.z), got {parts.Length}");
        }

        foreach (string part in parts)
        {
            if (!int.TryParse(part, out int _))
            {
                throw new InvalidOperationException($"Each semver part should be a valid integer, got '{part}'");
            }
        }
    }

    /// <summary>
    /// Tests that SCHEMA_VERSION is a positive integer.
    /// </summary>
    public static void TestSchemaVersionIsPositive()
    {
        if (Versions.SchemaVersion <= 0)
        {
            throw new InvalidOperationException($"SCHEMA_VERSION should be positive, got {Versions.SchemaVersion}");
        }
    }
}
