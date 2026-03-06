using Godot;
using StarGen.Domain.Jumplanes;

namespace StarGen.App.Prototypes;

/// <summary>
/// Generates mock regions with systems for testing jump lanes.
/// </summary>
public static class MockRegionGenerator
{
    /// <summary>
    /// Creates a simple test region with a few systems.
    /// </summary>
    public static JumpLaneRegion CreateSimpleRegion(int seedValue = 12345)
    {
        var region = new JumpLaneRegion(JumpLaneRegion.RegionScope.Subsector, "Test Subsector");
        region.AddSystem(new JumpLaneSystem("alpha", Vector3.Zero, 5000));
        region.AddSystem(new JumpLaneSystem("beta", new Vector3(2.5f, 0, 1), 15000));
        region.AddSystem(new JumpLaneSystem("gamma", new Vector3(6, 0, -1), 25000));
        region.AddSystem(new JumpLaneSystem("delta", new Vector3(13, 0, 0), 35000));
        region.AddSystem(new JumpLaneSystem("epsilon", new Vector3(21, 0, 1), 50000));
        region.AddSystem(new JumpLaneSystem("bridge_1", new Vector3(17, 0, 0), 0));
        region.AddSystem(new JumpLaneSystem("orphan", new Vector3(-15, 0, 5), 8000));
        return region;
    }

    /// <summary>
    /// Creates a random region with specified parameters.
    /// </summary>
    public static JumpLaneRegion CreateRandomRegion(
        int seedValue,
        int systemCount = 20,
        float regionSize = 30.0f,
        float populatedRatio = 0.7f)
    {
        var rng = new RandomNumberGenerator();
        rng.Seed = (ulong)seedValue;

        var region = new JumpLaneRegion(JumpLaneRegion.RegionScope.Subsector, $"Random Subsector {seedValue}");

        for (int i = 0; i < systemCount; i++)
        {
            var pos = new Vector3(
                (float)rng.RandfRange(-regionSize / 2, regionSize / 2),
                (float)rng.RandfRange(-2, 2),
                (float)rng.RandfRange(-regionSize / 2, regionSize / 2));

            int pop = 0;
            if (rng.Randf() < populatedRatio)
            {
                pop = rng.RandiRange(1000, 100000);
            }

            region.AddSystem(new JumpLaneSystem($"sys_{i:D3}", pos, pop));
        }

        return region;
    }

    /// <summary>
    /// Creates a clustered region with groups of systems.
    /// </summary>
    public static JumpLaneRegion CreateClusteredRegion(int seedValue = 54321)
    {
        var rng = new RandomNumberGenerator();
        rng.Seed = (ulong)seedValue;

        var region = new JumpLaneRegion(JumpLaneRegion.RegionScope.Subsector, "Clustered Subsector");

        var clusters = new[] { Vector3.Zero, new Vector3(15, 0, 10), new Vector3(-12, 0, -8) };
        int systemIndex = 0;

        foreach (Vector3 clusterCenter in clusters)
        {
            int clusterSize = rng.RandiRange(4, 7);
            for (int i = 0; i < clusterSize; i++)
            {
                var offset = new Vector3(
                    (float)rng.RandfRange(-4, 4),
                    (float)rng.RandfRange(-1, 1),
                    (float)rng.RandfRange(-4, 4));
                Vector3 pos = clusterCenter + offset;
                int pop = 0;
                if (rng.Randf() > 0.2)
                {
                    pop = rng.RandiRange(5000, 80000);
                }

                region.AddSystem(new JumpLaneSystem($"sys_{systemIndex:D3}", pos, pop));
                systemIndex++;
            }
        }

        return region;
    }
}
