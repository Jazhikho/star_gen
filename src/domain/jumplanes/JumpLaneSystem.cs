using Godot;
using Godot.Collections;
using StarGen.Domain.Utils;

namespace StarGen.Domain.Jumplanes;

/// <summary>
/// System record used for jump-lane calculations.
/// </summary>
public partial class JumpLaneSystem : RefCounted
{
    /// <summary>
    /// Unique system identifier.
    /// </summary>
    public string Id = string.Empty;

    /// <summary>
    /// Position in parsecs.
    /// </summary>
    public Vector3 Position = Vector3.Zero;

    /// <summary>
    /// Population, where zero means unpopulated.
    /// </summary>
    public int Population;

    /// <summary>
    /// Synthetic population when used as a bridge, or -1 when unset.
    /// </summary>
    public int FalsePopulation = -1;

    /// <summary>
    /// Whether this system is acting as a bridge.
    /// </summary>
    public bool IsBridge;

    /// <summary>
    /// Creates a new jump-lane system.
    /// </summary>
    public JumpLaneSystem(string id = "", Vector3 position = default, int population = 0)
    {
        Id = id;
        Position = position;
        Population = population;
    }

    /// <summary>
    /// Returns the effective population used by sorting and linking.
    /// </summary>
    public int GetEffectivePopulation()
    {
        if (FalsePopulation >= 0)
        {
            return FalsePopulation;
        }

        return Population;
    }

    /// <summary>
    /// Returns whether the system is populated.
    /// </summary>
    public bool IsPopulated()
    {
        return Population > 0;
    }

    /// <summary>
    /// Calculates distance to another system in parsecs.
    /// </summary>
    public double DistanceTo(JumpLaneSystem other)
    {
        return Position.DistanceTo(other.Position);
    }

    /// <summary>
    /// Marks this system as a bridge with a synthetic population.
    /// </summary>
    public void MakeBridge(int higherPopulation)
    {
        IsBridge = true;
        FalsePopulation = higherPopulation - 10000;
        if (FalsePopulation < 0)
        {
            FalsePopulation = 0;
        }
    }

    /// <summary>
    /// Converts the system to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            ["id"] = Id,
            ["position"] = new Dictionary
            {
                ["x"] = Position.X,
                ["y"] = Position.Y,
                ["z"] = Position.Z,
            },
            ["population"] = Population,
            ["false_population"] = FalsePopulation,
            ["is_bridge"] = IsBridge,
        };
    }

    /// <summary>
    /// Creates a system from a dictionary payload.
    /// </summary>
    public static JumpLaneSystem FromDictionary(Dictionary data)
    {
        Vector3 position = Vector3.Zero;
        if (data.ContainsKey("position") && data["position"].VariantType == Variant.Type.Dictionary)
        {
            Dictionary positionData = (Dictionary)data["position"];
            position = new Vector3(
                (float)DomainDictionaryUtils.GetDouble(positionData, "x", 0.0),
                (float)DomainDictionaryUtils.GetDouble(positionData, "y", 0.0),
                (float)DomainDictionaryUtils.GetDouble(positionData, "z", 0.0));
        }

        JumpLaneSystem system = new(
            DomainDictionaryUtils.GetString(data, "id", string.Empty),
            position,
            DomainDictionaryUtils.GetInt(data, "population", 0));
        system.FalsePopulation = DomainDictionaryUtils.GetInt(data, "false_population", -1);
        system.IsBridge = DomainDictionaryUtils.GetBool(data, "is_bridge", false);
        return system;
    }

}
