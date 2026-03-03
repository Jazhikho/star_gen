using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Serialization;
using StarGen.Domain.Celestial.Validation;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for celestial validation.
/// </summary>
[GlobalClass]
public partial class CSharpCelestialValidationBridge : RefCounted
{
    /// <summary>
    /// Validates a serialized celestial-body dictionary and returns a GDScript-friendly payload.
    /// </summary>
    public Dictionary ValidateBodyData(Dictionary bodyData)
    {
        CelestialBody? deserializedBody = CelestialSerializer.FromDictionary(bodyData);
        if (deserializedBody is null)
        {
            return BuildInvalidBodyPayload();
        }

        CelestialBody body = deserializedBody;
        ValidationResult result = CelestialValidator.Validate(body);
        return SerializeValidationResult(result);
    }

    private static Dictionary BuildInvalidBodyPayload()
    {
        Array errors = new();
        errors.Add(new Dictionary
        {
            ["field"] = "body",
            ["message"] = "Body data could not be deserialized for validation",
            ["severity"] = (int)ValidationError.SeverityLevel.Error,
        });

        return new Dictionary
        {
            ["errors"] = errors,
        };
    }

    private static Dictionary SerializeValidationResult(ValidationResult result)
    {
        Array errors = new();
        foreach (ValidationError error in result.Errors)
        {
            errors.Add(new Dictionary
            {
                ["field"] = error.Field,
                ["message"] = error.Message,
                ["severity"] = (int)error.Severity,
            });
        }

        return new Dictionary
        {
            ["errors"] = errors,
        };
    }
}
