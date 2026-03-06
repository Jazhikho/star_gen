#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Rng;

using StarGen.Domain.Math;
namespace StarGen.Tests.Unit;

/// <summary>
/// Tests for the SeededRng class.
/// Verifies determinism and correct behavior of the RNG wrapper.
/// </summary>
public static class TestSeededRng
{
    /// <summary>
    /// Tests that the same seed produces the same sequence of random numbers.
    /// </summary>
    public static void TestDeterminismSameSeedProducesSameSequence()
    {
        SeededRng rng1 = new SeededRng(12345);
        SeededRng rng2 = new SeededRng(12345);

        double[] sequence1 = new double[100];
        double[] sequence2 = new double[100];

        for (int i = 0; i < 100; i++)
        {
            sequence1[i] = rng1.Randf();
            sequence2[i] = rng2.Randf();
        }

        for (int i = 0; i < 100; i++)
        {
            if (sequence1[i] != sequence2[i])
            {
                throw new InvalidOperationException($"Sequence mismatch at index {i}");
            }
        }
    }

    /// <summary>
    /// Tests that different seeds produce different sequences.
    /// </summary>
    public static void TestDifferentSeedsProduceDifferentSequences()
    {
        SeededRng rng1 = new SeededRng(12345);
        SeededRng rng2 = new SeededRng(54321);

        int sameCount = 0;
        for (int i = 0; i < 10; i++)
        {
            if (rng1.Randf() == rng2.Randf())
            {
                sameCount += 1;
            }
        }

        if (sameCount >= 2)
        {
            throw new InvalidOperationException("Different seeds should produce different sequences");
        }
    }

    /// <summary>
    /// Tests that randf returns values in the correct range [0.0, 1.0).
    /// </summary>
    public static void TestRandfReturnsValuesInValidRange()
    {
        SeededRng rng = new SeededRng(42);

        for (int i = 0; i < 1000; i++)
        {
            double value = rng.Randf();
            if (value < 0.0)
            {
                throw new InvalidOperationException("randf should return >= 0.0");
            }
            if (value >= 1.0)
            {
                throw new InvalidOperationException("randf should return < 1.0");
            }
        }
    }

    /// <summary>
    /// Tests that randf_range returns values in the specified range.
    /// </summary>
    public static void TestRandfRangeReturnsValuesInSpecifiedRange()
    {
        SeededRng rng = new SeededRng(42);
        double minVal = 5.0;
        double maxVal = 10.0;

        for (int i = 0; i < 1000; i++)
        {
            double value = rng.RandfRange((float)minVal, (float)maxVal);
            if (value < minVal || value > maxVal)
            {
                throw new InvalidOperationException($"randf_range value out of range. Expected [{minVal}, {maxVal}], got {value}.");
            }
        }
    }

    /// <summary>
    /// Tests that randi_range returns values in the specified range.
    /// </summary>
    public static void TestRandiRangeReturnsValuesInSpecifiedRange()
    {
        SeededRng rng = new SeededRng(42);
        int minVal = 1;
        int maxVal = 6;

        for (int i = 0; i < 1000; i++)
        {
            int value = rng.RandiRange(minVal, maxVal);
            if (value < minVal || value > maxVal)
            {
                throw new InvalidOperationException($"randi_range value out of range. Expected [{minVal}, {maxVal}], got {value}.");
            }
        }
    }

    /// <summary>
    /// Tests that get_initial_seed returns the correct seed.
    /// </summary>
    public static void TestGetInitialSeedReturnsCorrectValue()
    {
        int seedValue = 99999;
        SeededRng rng = new SeededRng(seedValue);

        if (rng.GetInitialSeed() != seedValue)
        {
            throw new InvalidOperationException($"Expected seed {seedValue}, got {rng.GetInitialSeed()}");
        }

        for (int i = 0; i < 100; i++)
        {
            rng.Randf();
        }

        if (rng.GetInitialSeed() != seedValue)
        {
            throw new InvalidOperationException($"Seed should not change after generating numbers. Expected {seedValue}, got {rng.GetInitialSeed()}");
        }
    }

    /// <summary>
    /// Tests that state can be saved and restored.
    /// </summary>
    public static void TestStateSaveAndRestore()
    {
        SeededRng rng = new SeededRng(12345);

        for (int i = 0; i < 50; i++)
        {
            rng.Randf();
        }

        ulong savedState = rng.GetState();

        double[] valuesAfterSave = new double[10];
        for (int i = 0; i < 10; i++)
        {
            valuesAfterSave[i] = rng.Randf();
        }

        rng.SetState(savedState);

        for (int i = 0; i < 10; i++)
        {
            double restoredValue = rng.Randf();
            if (restoredValue != valuesAfterSave[i])
            {
                throw new InvalidOperationException($"State restore failed at index {i}");
            }
        }
    }

    /// <summary>
    /// Tests that fork creates an independent deterministic RNG.
    /// </summary>
    public static void TestForkIsDeterministic()
    {
        SeededRng parent1 = new SeededRng(12345);
        SeededRng parent2 = new SeededRng(12345);

        for (int i = 0; i < 10; i++)
        {
            parent1.Randf();
            parent2.Randf();
        }

        SeededRng child1 = parent1.Fork();
        SeededRng child2 = parent2.Fork();

        for (int i = 0; i < 100; i++)
        {
            if (child1.Randf() != child2.Randf())
            {
                throw new InvalidOperationException("Forked RNGs should be deterministic");
            }
        }
    }

    /// <summary>
    /// Tests that forking does not break parent determinism.
    /// </summary>
    public static void TestForkDoesNotAffectParentDeterminism()
    {
        SeededRng rng1 = new SeededRng(12345);
        SeededRng rng2 = new SeededRng(12345);

        for (int i = 0; i < 5; i++)
        {
            rng1.Randf();
            rng2.Randf();
        }

        SeededRng child1 = rng1.Fork();
        SeededRng child2 = rng2.Fork();

        for (int i = 0; i < 50; i++)
        {
            if (rng1.Randf() != rng2.Randf())
            {
                throw new InvalidOperationException("Parent RNGs should remain deterministic after fork");
            }
        }
    }
}
