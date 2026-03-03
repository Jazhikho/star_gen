using System.Collections.Generic;
using Godot;

namespace StarGen.Domain.Rng;

/// <summary>
/// Deterministic RNG wrapper that centralizes random access for generation.
/// </summary>
public sealed class SeededRng
{
    private readonly long _initialSeed;
    private readonly RandomNumberGenerator _rng;

    /// <summary>
    /// Creates a new seeded RNG instance.
    /// </summary>
    public SeededRng(long seedValue)
    {
        _initialSeed = seedValue;
        _rng = new RandomNumberGenerator
        {
            Seed = unchecked((ulong)seedValue),
        };
    }

    /// <summary>
    /// Returns the seed that created this RNG.
    /// </summary>
    public long GetInitialSeed() => _initialSeed;

    /// <summary>
    /// Returns the current internal RNG state.
    /// </summary>
    public ulong GetState() => _rng.State;

    /// <summary>
    /// Restores a previously captured RNG state.
    /// </summary>
    public void SetState(ulong state) => _rng.State = state;

    /// <summary>
    /// Returns a random floating-point value in the range [0, 1).
    /// </summary>
    public float Randf() => _rng.Randf();

    /// <summary>
    /// Returns a random floating-point value in the inclusive range [from, to].
    /// </summary>
    public float RandfRange(float from, float to) => _rng.RandfRange(from, to);

    /// <summary>
    /// Returns a random unsigned integer.
    /// </summary>
    public uint Randi() => _rng.Randi();

    /// <summary>
    /// Returns a random integer in the inclusive range [from, to].
    /// </summary>
    public int RandiRange(int from, int to) => _rng.RandiRange(from, to);

    /// <summary>
    /// Returns a normally distributed random value.
    /// </summary>
    public float Randfn(float mean = 0.0f, float deviation = 1.0f) => _rng.Randfn(mean, deviation);

    /// <summary>
    /// Creates a child RNG using a seed derived from the current sequence.
    /// </summary>
    public SeededRng Fork() => new(_rng.Randi());

    /// <summary>
    /// Picks a random value from weighted options.
    /// </summary>
    public T? WeightedChoice<T>(IReadOnlyList<T> options, IReadOnlyList<float> weights)
    {
        if (options.Count == 0 || options.Count != weights.Count)
        {
            return default;
        }

        double totalWeight = 0.0;
        for (int index = 0; index < weights.Count; index += 1)
        {
            totalWeight += weights[index];
        }

        double roll = _rng.Randf() * totalWeight;
        double cumulative = 0.0;

        for (int index = 0; index < options.Count; index += 1)
        {
            cumulative += weights[index];
            if (roll < cumulative)
            {
                return options[index];
            }
        }

        return options[options.Count - 1];
    }
}
