using Godot;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for version constants.
/// </summary>
[GlobalClass]
public partial class CSharpVersionsBridge : RefCounted
{
    /// <summary>
    /// Returns the current generator version.
    /// </summary>
    public string GetGeneratorVersion() => Constants.Versions.GeneratorVersion;

    /// <summary>
    /// Returns the current schema version.
    /// </summary>
    public int GetSchemaVersion() => Constants.Versions.SchemaVersion;
}
