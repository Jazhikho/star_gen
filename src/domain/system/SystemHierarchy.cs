using Godot;
using Godot.Collections;

namespace StarGen.Domain.Systems;

/// <summary>
/// Hierarchical arrangement of stars in a solar system.
/// </summary>
public partial class SystemHierarchy : RefCounted
{
    /// <summary>
    /// Root node of the hierarchy.
    /// </summary>
    public HierarchyNode? Root;

    /// <summary>
    /// Creates a new system hierarchy.
    /// </summary>
    public SystemHierarchy(HierarchyNode? root = null)
    {
        Root = root;
    }

    /// <summary>
    /// Returns whether the hierarchy has a root node.
    /// </summary>
    public bool IsValid()
    {
        return Root != null;
    }

    /// <summary>
    /// Returns the total star count.
    /// </summary>
    public int GetStarCount()
    {
        return Root != null ? Root.GetStarCount() : 0;
    }

    /// <summary>
    /// Returns all star identifiers in the hierarchy.
    /// </summary>
    public Array<string> GetAllStarIds()
    {
        return Root != null ? Root.GetAllStarIds() : new Array<string>();
    }

    /// <summary>
    /// Returns the maximum hierarchy depth.
    /// </summary>
    public int GetDepth()
    {
        return Root != null ? Root.GetDepth() : 0;
    }

    /// <summary>
    /// Finds a node anywhere in the hierarchy by identifier.
    /// </summary>
    public HierarchyNode? FindNode(string nodeId)
    {
        return Root != null ? Root.FindNode(nodeId) : null;
    }

    /// <summary>
    /// Returns all nodes in the hierarchy as a flat list.
    /// </summary>
    public Array<HierarchyNode> GetAllNodes()
    {
        Array<HierarchyNode> nodes = new();
        if (Root != null)
        {
            CollectNodes(Root, nodes);
        }

        return nodes;
    }

    /// <summary>
    /// Returns all barycenter nodes.
    /// </summary>
    public Array<HierarchyNode> GetAllBarycenters()
    {
        Array<HierarchyNode> nodes = new();
        foreach (HierarchyNode node in GetAllNodes())
        {
            if (node.IsBarycenter())
            {
                nodes.Add(node);
            }
        }

        return nodes;
    }

    /// <summary>
    /// Returns all star leaf nodes.
    /// </summary>
    public Array<HierarchyNode> GetAllStarNodes()
    {
        Array<HierarchyNode> nodes = new();
        foreach (HierarchyNode node in GetAllNodes())
        {
            if (node.IsStar())
            {
                nodes.Add(node);
            }
        }

        return nodes;
    }

    /// <summary>
    /// Converts the hierarchy to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Dictionary data = new();
        if (Root != null)
        {
            data["root"] = Root.ToDictionary();
        }

        return data;
    }

    /// <summary>
    /// Creates a hierarchy from a dictionary payload.
    /// </summary>
    public static SystemHierarchy FromDictionary(Dictionary data)
    {
        SystemHierarchy hierarchy = new();
        if (data.ContainsKey("root") && data["root"].VariantType == Variant.Type.Dictionary)
        {
            hierarchy.Root = HierarchyNode.FromDictionary((Dictionary)data["root"]);
        }

        return hierarchy;
    }

    /// <summary>
    /// Recursively collects nodes from a subtree.
    /// </summary>
    private void CollectNodes(HierarchyNode node, Array<HierarchyNode> result)
    {
        result.Add(node);
        if (node.IsBarycenter())
        {
            foreach (HierarchyNode child in node.Children)
            {
                CollectNodes(child, result);
            }
        }
    }
}
