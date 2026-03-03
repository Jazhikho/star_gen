using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for celestial serialization entry points.
/// </summary>
[GlobalClass]
public partial class CSharpCelestialSerializationBridge : RefCounted
{
    /// <summary>
    /// Normalizes a serialized celestial-body dictionary through the C# serializer path.
    /// </summary>
    public Dictionary NormalizeBodyData(Dictionary bodyData)
    {
        CelestialBody? body = CelestialSerializer.FromDictionary(bodyData);
        if (body is null)
        {
            return new Dictionary();
        }

        return CelestialSerializer.ToDictionary(body);
    }

    /// <summary>
    /// Serializes a celestial-body dictionary to JSON using the C# serializer path.
    /// </summary>
    public string ToJsonFromBodyData(Dictionary bodyData, bool pretty = true)
    {
        CelestialBody? body = CelestialSerializer.FromDictionary(bodyData);
        if (body is null)
        {
            return string.Empty;
        }

        return CelestialSerializer.ToJson(body, pretty);
    }

    /// <summary>
    /// Parses JSON into a celestial-body dictionary using the C# serializer path.
    /// </summary>
    public Dictionary ParseJsonToBodyData(string jsonString)
    {
        CelestialBody? body = CelestialSerializer.FromJson(jsonString);
        if (body is null)
        {
            return new Dictionary();
        }

        return CelestialSerializer.ToDictionary(body);
    }
}
