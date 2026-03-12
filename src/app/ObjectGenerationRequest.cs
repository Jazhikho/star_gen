using Godot;
using StarGen.Domain.Generation;
using StarGen.App.Viewer;

namespace StarGen.App;

/// <summary>
/// App-layer request describing a standalone object-generation launch.
/// </summary>
public partial class ObjectGenerationRequest : RefCounted
{
    /// <summary>
    /// Selected object type.
    /// </summary>
    public ObjectViewer.ObjectType ObjectType { get; set; } = ObjectViewer.ObjectType.Planet;

    /// <summary>
    /// Deterministic generation seed.
    /// </summary>
    public int SeedValue { get; set; }

    /// <summary>
    /// Selected preset identifier within the object type.
    /// </summary>
    public int PresetId { get; set; }

    /// <summary>
    /// Active use-case settings to persist into generated content.
    /// </summary>
    public GenerationUseCaseSettings UseCaseSettings { get; set; } = GenerationUseCaseSettings.CreateDefault();

    /// <summary>
    /// Returns a defensive copy of the request.
    /// </summary>
    public ObjectGenerationRequest Clone()
    {
        return new ObjectGenerationRequest
        {
            ObjectType = ObjectType,
            SeedValue = SeedValue,
            PresetId = PresetId,
            UseCaseSettings = UseCaseSettings.Clone(),
        };
    }
}
