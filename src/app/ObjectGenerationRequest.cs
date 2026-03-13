using Godot;
using Godot.Collections;
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
    /// Optional explicit generator spec payload for studio-driven launches.
    /// </summary>
    public Dictionary SpecData { get; set; } = new Dictionary();

    /// <summary>
    /// Optional Traveller world-profile payload used for planet-focused readouts and mapping.
    /// </summary>
    public Dictionary TravellerWorldProfileData { get; set; } = new Dictionary();

    /// <summary>
    /// Whether the studio exposed the seed to the user.
    /// </summary>
    public bool ShowSeed { get; set; }

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
            SpecData = (Dictionary)SpecData.Duplicate(true),
            TravellerWorldProfileData = (Dictionary)TravellerWorldProfileData.Duplicate(true),
            ShowSeed = ShowSeed,
        };
    }
}
