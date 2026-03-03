using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;

namespace StarGen.Domain.Generation.Generators;

/// <summary>
/// Shared utility functions for generation helpers.
/// </summary>
public static class GeneratorUtils
{
    /// <summary>
    /// Formats a generated celestial-body identifier.
    /// </summary>
    public static string GenerateIdFromRandomPart(string bodyType, int randomPart)
    {
        int normalizedRandomPart = Mathf.Abs(randomPart) % 1_000_000;
        return $"{bodyType}_{normalizedRandomPart:D6}";
    }

    /// <summary>
    /// Creates a provenance record from a generation seed and spec snapshot.
    /// </summary>
    public static Provenance CreateProvenance(int generationSeed, Dictionary specSnapshot)
    {
        return Provenance.CreateCurrent(generationSeed, specSnapshot);
    }
}
