using Godot;
using Godot.Collections;

namespace StarGen.Domain.Systems;

/// <summary>
/// Node in the stellar hierarchy tree.
/// </summary>
public partial class HierarchyNode : RefCounted
{
    /// <summary>
    /// Hierarchy node categories.
    /// </summary>
    public enum NodeType
    {
        Star,
        Barycenter,
    }

    /// <summary>
    /// Unique node identifier.
    /// </summary>
    public string Id = string.Empty;

    /// <summary>
    /// Node type.
    /// </summary>
    public NodeType Type = NodeType.Star;

    /// <summary>
    /// Referenced star identifier for leaf nodes.
    /// </summary>
    public string StarId = string.Empty;

    /// <summary>
    /// Child nodes for barycenters.
    /// </summary>
    public Array<HierarchyNode> Children = new();

    /// <summary>
    /// Separation between children in meters.
    /// </summary>
    public double SeparationM;

    /// <summary>
    /// Binary eccentricity.
    /// </summary>
    public double Eccentricity;

    /// <summary>
    /// Binary orbital period in seconds.
    /// </summary>
    public double OrbitalPeriodS;

    /// <summary>
    /// Creates a new hierarchy node.
    /// </summary>
    public HierarchyNode(string id = "", NodeType type = NodeType.Star)
    {
        Id = id;
        Type = type;
    }

    /// <summary>
    /// Creates a star leaf node.
    /// </summary>
    public static HierarchyNode CreateStar(string id, string starId)
    {
        HierarchyNode node = new(id, NodeType.Star)
        {
            StarId = starId,
        };
        return node;
    }

    /// <summary>
    /// Creates a barycenter node.
    /// </summary>
    public static HierarchyNode CreateBarycenter(
        string id,
        HierarchyNode childA,
        HierarchyNode childB,
        double separationM,
        double eccentricity = 0.0)
    {
        HierarchyNode node = new(id, NodeType.Barycenter)
        {
            SeparationM = separationM,
            Eccentricity = eccentricity,
        };
        node.Children.Add(childA);
        node.Children.Add(childB);
        return node;
    }

    /// <summary>
    /// Returns whether this is a star leaf node.
    /// </summary>
    public bool IsStar()
    {
        return Type == NodeType.Star;
    }

    /// <summary>
    /// Returns whether this is a barycenter node.
    /// </summary>
    public bool IsBarycenter()
    {
        return Type == NodeType.Barycenter;
    }

    /// <summary>
    /// Returns all star identifiers in this subtree.
    /// </summary>
    public Array<string> GetAllStarIds()
    {
        Array<string> ids = new();
        if (IsStar())
        {
            ids.Add(StarId);
            return ids;
        }

        foreach (HierarchyNode child in Children)
        {
            foreach (string starId in child.GetAllStarIds())
            {
                ids.Add(starId);
            }
        }

        return ids;
    }

    /// <summary>
    /// Returns the number of stars in this subtree.
    /// </summary>
    public int GetStarCount()
    {
        if (IsStar())
        {
            return 1;
        }

        int count = 0;
        foreach (HierarchyNode child in Children)
        {
            count += child.GetStarCount();
        }

        return count;
    }

    /// <summary>
    /// Returns the maximum subtree depth.
    /// </summary>
    public int GetDepth()
    {
        if (IsStar())
        {
            return 1;
        }

        int maxChildDepth = 0;
        foreach (HierarchyNode child in Children)
        {
            maxChildDepth = System.Math.Max(maxChildDepth, child.GetDepth());
        }

        return maxChildDepth + 1;
    }

    /// <summary>
    /// Finds a node by identifier in this subtree.
    /// </summary>
    public HierarchyNode? FindNode(string targetId)
    {
        if (Id == targetId)
        {
            return this;
        }

        if (IsBarycenter())
        {
            foreach (HierarchyNode child in Children)
            {
                HierarchyNode? found = child.FindNode(targetId);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Converts the node to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = new()
        {
            ["id"] = Id,
            ["node_type"] = IsStar() ? "star" : "barycenter",
        };

        if (IsStar())
        {
            data["star_id"] = StarId;
        }
        else
        {
            Array<Dictionary> children = new();
            foreach (HierarchyNode child in Children)
            {
                children.Add(child.ToDictionary());
            }

            data["children"] = children;
            data["separation_m"] = SeparationM;
            data["eccentricity"] = Eccentricity;
            data["orbital_period_s"] = OrbitalPeriodS;
        }

        return data;
    }

    /// <summary>
    /// Creates a hierarchy node from a dictionary payload.
    /// </summary>
    public static HierarchyNode? FromDictionary(Dictionary data)
    {
        if (data.Count == 0)
        {
            return null;
        }

        string typeName = GetString(data, "node_type", "star");
        NodeType type = typeName == "star" ? NodeType.Star : NodeType.Barycenter;
        HierarchyNode node = new(GetString(data, "id", string.Empty), type);
        if (node.IsStar())
        {
            node.StarId = GetString(data, "star_id", string.Empty);
        }
        else
        {
            node.SeparationM = GetDouble(data, "separation_m", 0.0);
            node.Eccentricity = GetDouble(data, "eccentricity", 0.0);
            node.OrbitalPeriodS = GetDouble(data, "orbital_period_s", 0.0);
            if (data.ContainsKey("children") && data["children"].VariantType == Variant.Type.Array)
            {
                foreach (Variant value in (Array)data["children"])
                {
                    if (value.VariantType == Variant.Type.Dictionary)
                    {
                        HierarchyNode? child = FromDictionary((Dictionary)value);
                        if (child != null)
                        {
                            node.Children.Add(child);
                        }
                    }
                }
            }
        }

        return node;
    }

    /// <summary>
    /// Reads a string value from a dictionary.
    /// </summary>
    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType == Variant.Type.String ? (string)value : fallback;
    }

    /// <summary>
    /// Reads a floating-point value from a dictionary.
    /// </summary>
    private static double GetDouble(Dictionary data, string key, double fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Float => (double)value,
            Variant.Type.Int => (int)value,
            Variant.Type.String => double.TryParse((string)value, out double parsed) ? parsed : fallback,
            _ => fallback,
        };
    }
}
