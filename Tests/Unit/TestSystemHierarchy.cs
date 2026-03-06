#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Systems;

namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for SystemHierarchy.
/// </summary>
public static class TestSystemHierarchy
{
    /// <summary>
    /// Tests empty hierarchy.
    /// </summary>
    public static void TestEmptyHierarchy()
    {
        SystemHierarchy hierarchy = new SystemHierarchy();

        if (hierarchy.IsValid())
        {
            throw new InvalidOperationException("Empty hierarchy should not be valid");
        }
        if (hierarchy.GetStarCount() != 0)
        {
            throw new InvalidOperationException("Expected 0 stars");
        }
        if (hierarchy.GetDepth() != 0)
        {
            throw new InvalidOperationException("Expected depth 0");
        }
        if (hierarchy.GetAllStarIds().Count != 0)
        {
            throw new InvalidOperationException("Expected empty star ids");
        }
    }

    /// <summary>
    /// Tests single star hierarchy.
    /// </summary>
    public static void TestSingleStar()
    {
        HierarchyNode star = HierarchyNode.CreateStar("n1", "sol");
        SystemHierarchy hierarchy = new SystemHierarchy(star);

        if (!hierarchy.IsValid())
        {
            throw new InvalidOperationException("Single star hierarchy should be valid");
        }
        if (hierarchy.GetStarCount() != 1)
        {
            throw new InvalidOperationException("Expected 1 star");
        }
        if (hierarchy.GetDepth() != 1)
        {
            throw new InvalidOperationException("Expected depth 1");
        }
        if (hierarchy.GetAllStarIds()[0] != "sol")
        {
            throw new InvalidOperationException("Expected sol");
        }
    }

    /// <summary>
    /// Tests binary star hierarchy.
    /// </summary>
    public static void TestBinaryStars()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("na", "alpha");
        HierarchyNode starB = HierarchyNode.CreateStar("nb", "beta");
        HierarchyNode binary = HierarchyNode.CreateBarycenter("binary", starA, starB, 1e11, 0.0);
        SystemHierarchy hierarchy = new SystemHierarchy(binary);

        if (!hierarchy.IsValid())
        {
            throw new InvalidOperationException("Binary hierarchy should be valid");
        }
        if (hierarchy.GetStarCount() != 2)
        {
            throw new InvalidOperationException("Expected 2 stars");
        }
        if (hierarchy.GetDepth() != 2)
        {
            throw new InvalidOperationException("Expected depth 2");
        }
    }

    /// <summary>
    /// Tests get all nodes.
    /// </summary>
    public static void TestGetAllNodes()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("na", "alpha");
        HierarchyNode starB = HierarchyNode.CreateStar("nb", "beta");
        HierarchyNode binary = HierarchyNode.CreateBarycenter("binary", starA, starB, 1e11, 0.0);
        SystemHierarchy hierarchy = new SystemHierarchy(binary);

        Array<HierarchyNode> nodes = hierarchy.GetAllNodes();

        if (nodes.Count != 3)
        {
            throw new InvalidOperationException("Expected 3 nodes");
        }
    }

    /// <summary>
    /// Tests get all barycenters.
    /// </summary>
    public static void TestGetAllBarycenters()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("na", "alpha");
        HierarchyNode starB = HierarchyNode.CreateStar("nb", "beta");
        HierarchyNode starC = HierarchyNode.CreateStar("nc", "gamma");
        HierarchyNode inner = HierarchyNode.CreateBarycenter("inner", starA, starB, 1e10, 0.0);
        HierarchyNode outer = HierarchyNode.CreateBarycenter("outer", inner, starC, 1e12, 0.0);
        SystemHierarchy hierarchy = new SystemHierarchy(outer);

        Array<HierarchyNode> barycenters = hierarchy.GetAllBarycenters();

        if (barycenters.Count != 2)
        {
            throw new InvalidOperationException("Expected 2 barycenters");
        }
    }

    /// <summary>
    /// Tests get all star nodes.
    /// </summary>
    public static void TestGetAllStarNodes()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("na", "alpha");
        HierarchyNode starB = HierarchyNode.CreateStar("nb", "beta");
        HierarchyNode binary = HierarchyNode.CreateBarycenter("binary", starA, starB, 1e11, 0.0);
        SystemHierarchy hierarchy = new SystemHierarchy(binary);

        Array<HierarchyNode> starNodes = hierarchy.GetAllStarNodes();

        if (starNodes.Count != 2)
        {
            throw new InvalidOperationException("Expected 2 star nodes");
        }
    }

    /// <summary>
    /// Tests find node.
    /// </summary>
    public static void TestFindNode()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("na", "alpha");
        HierarchyNode starB = HierarchyNode.CreateStar("nb", "beta");
        HierarchyNode binary = HierarchyNode.CreateBarycenter("binary", starA, starB, 1e11, 0.0);
        SystemHierarchy hierarchy = new SystemHierarchy(binary);

        if (hierarchy.FindNode("binary") == null)
        {
            throw new InvalidOperationException("Should find binary");
        }
        if (hierarchy.FindNode("na") == null)
        {
            throw new InvalidOperationException("Should find na");
        }
        if (hierarchy.FindNode("nb") == null)
        {
            throw new InvalidOperationException("Should find nb");
        }
        if (hierarchy.FindNode("nonexistent") != null)
        {
            throw new InvalidOperationException("Should not find nonexistent");
        }
    }

    /// <summary>
    /// Tests serialization round-trip.
    /// </summary>
    public static void TestRoundTrip()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("na", "alpha");
        HierarchyNode starB = HierarchyNode.CreateStar("nb", "beta");
        HierarchyNode binary = HierarchyNode.CreateBarycenter("binary", starA, starB, 1e11, 0.5);
        SystemHierarchy original = new SystemHierarchy(binary);

        Godot.Collections.Dictionary data = original.ToDictionary();
        SystemHierarchy restored = SystemHierarchy.FromDictionary(data);

        if (!restored.IsValid())
        {
            throw new InvalidOperationException("Restored hierarchy should be valid");
        }
        if (restored.GetStarCount() != 2)
        {
            throw new InvalidOperationException("Expected 2 stars");
        }
        if (restored.GetDepth() != 2)
        {
            throw new InvalidOperationException("Expected depth 2");
        }
        if (restored.FindNode("binary") == null)
        {
            throw new InvalidOperationException("Should find binary node");
        }
    }
}
