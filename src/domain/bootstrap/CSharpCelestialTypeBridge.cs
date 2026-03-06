using Godot;
using StarGen.Domain.Celestial;

namespace StarGen.Domain.Bootstrap;

/// <summary>
/// GDScript-facing bridge for celestial type conversions.
/// </summary>
[GlobalClass]
public partial class CSharpCelestialTypeBridge : RefCounted
{
    /// <summary>
    /// Converts a celestial-body type enum to its display string.
    /// </summary>
    public string TypeToString(int typeValue) => CelestialType.TypeToString((CelestialType.Type)typeValue);

    /// <summary>
    /// Parses a celestial-body type string.
    /// </summary>
    public int StringToType(string typeName)
    {
        if (CelestialType.TryParse(typeName, out CelestialType.Type parsedType))
        {
            return (int)parsedType;
        }

        return -1;
    }
}
