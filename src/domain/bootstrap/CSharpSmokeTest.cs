namespace StarGen.Domain.Bootstrap;

/// <summary>
/// Minimal compile target used to verify the C# assembly is wired into the repo.
/// </summary>
public static class CSharpSmokeTest
{
    /// <summary>
    /// Returns a stable bootstrap marker for sanity checks.
    /// </summary>
    public static string GetBootstrapMessage()
    {
        return "StarGen C# bootstrap ready";
    }
}
