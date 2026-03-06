#nullable enable annotations
#nullable disable warnings
using System;
using Godot.Collections;
using StarGen.Domain.Systems;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for HierarchyNode.
/// </summary>
public static class TestHierarchyNode
{
    private const double DefaultTolerance = 0.00001;

    /// <summary>
    /// Tests creating a star node.
    /// </summary>
    public static void TestCreateStarNode()
    {
        HierarchyNode node = HierarchyNode.CreateStar("node_1", "star_alpha");

        if (node.Id != "node_1")
        {
            throw new InvalidOperationException("Expected id node_1");
        }
        if (!node.IsStar())
        {
            throw new InvalidOperationException("Should be a star");
        }
        if (node.IsBarycenter())
        {
            throw new InvalidOperationException("Should not be a barycenter");
        }
        if (node.StarId != "star_alpha")
        {
            throw new InvalidOperationException("Expected star_id star_alpha");
        }
        if (node.GetStarCount() != 1)
        {
            throw new InvalidOperationException("Expected 1 star");
        }
    }

    /// <summary>
    /// Tests creating a barycenter node.
    /// </summary>
    public static void TestCreateBarycenterNode()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("node_a", "star_a");
        HierarchyNode starB = HierarchyNode.CreateStar("node_b", "star_b");

        HierarchyNode binary = HierarchyNode.CreateBarycenter(
            "binary_ab",
            starA,
            starB,
            1.0e11,
            0.5
        );

        if (binary.Id != "binary_ab")
        {
            throw new InvalidOperationException("Expected id binary_ab");
        }
        if (!binary.IsBarycenter())
        {
            throw new InvalidOperationException("Should be a barycenter");
        }
        if (binary.IsStar())
        {
            throw new InvalidOperationException("Should not be a star");
        }
        if (binary.Children.Count != 2)
        {
            throw new InvalidOperationException("Expected 2 children");
        }
        if (binary.SeparationM != 1.0e11)
        {
            throw new InvalidOperationException("Expected separation 1.0e11");
        }
        if (binary.Eccentricity != 0.5)
        {
            throw new InvalidOperationException("Expected eccentricity 0.5");
        }
        if (binary.GetStarCount() != 2)
        {
            throw new InvalidOperationException("Expected 2 stars");
        }
    }

    /// <summary>
    /// Tests get all star ids for single star.
    /// </summary>
    public static void TestGetAllStarIdsSingle()
    {
        HierarchyNode node = HierarchyNode.CreateStar("n1", "star_solo");
        Array<string> ids = node.GetAllStarIds();

        if (ids.Count != 1)
        {
            throw new InvalidOperationException("Expected 1 star id");
        }
        if (!ids.Contains("star_solo"))
        {
            throw new InvalidOperationException("Should contain star_solo");
        }
    }

    /// <summary>
    /// Tests get all star ids for binary.
    /// </summary>
    public static void TestGetAllStarIdsBinary()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("na", "star_a");
        HierarchyNode starB = HierarchyNode.CreateStar("nb", "star_b");
        HierarchyNode binary = HierarchyNode.CreateBarycenter("bin", starA, starB, 1e11, 0.0);

        Array<string> ids = binary.GetAllStarIds();

        if (ids.Count != 2)
        {
            throw new InvalidOperationException("Expected 2 star ids");
        }
        if (!ids.Contains("star_a"))
        {
            throw new InvalidOperationException("Should contain star_a");
        }
        if (!ids.Contains("star_b"))
        {
            throw new InvalidOperationException("Should contain star_b");
        }
    }

    /// <summary>
    /// Tests get all star ids for hierarchical triple.
    /// </summary>
    public static void TestGetAllStarIdsTriple()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("na", "star_a");
        HierarchyNode starB = HierarchyNode.CreateStar("nb", "star_b");
        HierarchyNode starC = HierarchyNode.CreateStar("nc", "star_c");

        HierarchyNode innerBinary = HierarchyNode.CreateBarycenter("inner", starA, starB, 1e10, 0.1);
        HierarchyNode triple = HierarchyNode.CreateBarycenter("outer", innerBinary, starC, 1e12, 0.3);

        Array<string> ids = triple.GetAllStarIds();

        if (ids.Count != 3)
        {
            throw new InvalidOperationException("Expected 3 star ids");
        }
        if (!ids.Contains("star_a"))
        {
            throw new InvalidOperationException("Should contain star_a");
        }
        if (!ids.Contains("star_b"))
        {
            throw new InvalidOperationException("Should contain star_b");
        }
        if (!ids.Contains("star_c"))
        {
            throw new InvalidOperationException("Should contain star_c");
        }
    }

    /// <summary>
    /// Tests depth calculation.
    /// </summary>
    public static void TestGetDepthSingle()
    {
        HierarchyNode node = HierarchyNode.CreateStar("n1", "star_1");
        if (node.GetDepth() != 1)
        {
            throw new InvalidOperationException("Expected depth 1");
        }
    }

    /// <summary>
    /// Tests depth for binary.
    /// </summary>
    public static void TestGetDepthBinary()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("na", "star_a");
        HierarchyNode starB = HierarchyNode.CreateStar("nb", "star_b");
        HierarchyNode binary = HierarchyNode.CreateBarycenter("bin", starA, starB, 1e11, 0.0);

        if (binary.GetDepth() != 2)
        {
            throw new InvalidOperationException("Expected depth 2");
        }
    }

    /// <summary>
    /// Tests depth for hierarchical triple.
    /// </summary>
    public static void TestGetDepthTriple()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("na", "star_a");
        HierarchyNode starB = HierarchyNode.CreateStar("nb", "star_b");
        HierarchyNode starC = HierarchyNode.CreateStar("nc", "star_c");

        HierarchyNode inner = HierarchyNode.CreateBarycenter("inner", starA, starB, 1e10, 0.0);
        HierarchyNode outer = HierarchyNode.CreateBarycenter("outer", inner, starC, 1e12, 0.0);

        if (outer.GetDepth() != 3)
        {
            throw new InvalidOperationException("Expected depth 3");
        }
    }

    /// <summary>
    /// Tests find node.
    /// </summary>
    public static void TestFindNode()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("na", "star_a");
        HierarchyNode starB = HierarchyNode.CreateStar("nb", "star_b");
        HierarchyNode binary = HierarchyNode.CreateBarycenter("bin", starA, starB, 1e11, 0.0);

        if (binary.FindNode("bin") != binary)
        {
            throw new InvalidOperationException("Should find binary");
        }
        if (binary.FindNode("na") != starA)
        {
            throw new InvalidOperationException("Should find star_a");
        }
        if (binary.FindNode("nb") != starB)
        {
            throw new InvalidOperationException("Should find star_b");
        }
        if (binary.FindNode("nonexistent") != null)
        {
            throw new InvalidOperationException("Should not find nonexistent");
        }
    }

    /// <summary>
    /// Tests serialization round-trip for star node.
    /// </summary>
    public static void TestRoundTripStar()
    {
        HierarchyNode original = HierarchyNode.CreateStar("node_1", "star_alpha");

        Godot.Collections.Dictionary data = original.ToDictionary();
        HierarchyNode restored = HierarchyNode.FromDictionary(data);

        if (restored.Id != original.Id)
        {
            throw new InvalidOperationException("ID should match");
        }
        if (restored.Type != original.Type)
        {
            throw new InvalidOperationException("Node type should match");
        }
        if (restored.StarId != original.StarId)
        {
            throw new InvalidOperationException("Star ID should match");
        }
    }

    /// <summary>
    /// Tests serialization round-trip for barycenter node.
    /// </summary>
    public static void TestRoundTripBarycenter()
    {
        HierarchyNode starA = HierarchyNode.CreateStar("na", "star_a");
        HierarchyNode starB = HierarchyNode.CreateStar("nb", "star_b");
        HierarchyNode original = HierarchyNode.CreateBarycenter("bin", starA, starB, 1.5e11, 0.3);
        original.OrbitalPeriodS = 86400.0 * 365.0;

        Godot.Collections.Dictionary data = original.ToDictionary();
        HierarchyNode restored = HierarchyNode.FromDictionary(data);

        if (restored.Id != original.Id)
        {
            throw new InvalidOperationException("ID should match");
        }
        if (restored.Type != original.Type)
        {
            throw new InvalidOperationException("Node type should match");
        }
        if (restored.Children.Count != 2)
        {
            throw new InvalidOperationException("Expected 2 children");
        }
        if (System.Math.Abs(restored.SeparationM - original.SeparationM) > DefaultTolerance)
        {
            throw new InvalidOperationException("Separation should match");
        }
        if (System.Math.Abs(restored.Eccentricity - original.Eccentricity) > DefaultTolerance)
        {
            throw new InvalidOperationException("Eccentricity should match");
        }
        if (System.Math.Abs(restored.OrbitalPeriodS - original.OrbitalPeriodS) > DefaultTolerance)
        {
            throw new InvalidOperationException("Orbital period should match");
        }

        if (restored.Children[0].StarId != "star_a")
        {
            throw new InvalidOperationException("First child should be star_a");
        }
        if (restored.Children[1].StarId != "star_b")
        {
            throw new InvalidOperationException("Second child should be star_b");
        }
    }
}
