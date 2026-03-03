using Godot;
using Godot.Collections;

namespace StarGen.Domain.Galaxy;

/// <summary>
/// Stable FNV-1a hash that does not depend on Godot's internal hash implementation.
/// </summary>
public static class StableHash
{
    private const uint FnvOffset = 2166136261;
    private const uint FnvPrime = 16777619;
    private const uint Mask32 = 0xFFFFFFFF;

    /// <summary>
    /// Hashes an array of integers using FNV-1a over their raw bytes.
    /// </summary>
    public static long HashIntegers(Array<long> values)
    {
        uint hash = FnvOffset;
        foreach (long value in values)
        {
            for (int index = 0; index < 8; index += 1)
            {
                uint byteValue = (uint)((value >> (index * 8)) & 0xFF);
                hash = (hash ^ byteValue) & Mask32;
                hash = (hash * FnvPrime) & Mask32;
            }
        }

        return hash;
    }

    /// <summary>
    /// Derives a child seed from a parent seed and 3D grid coordinates.
    /// </summary>
    public static long DeriveSeed(long parentSeed, Vector3I coords)
    {
        return HashIntegers(new Array<long> { parentSeed, coords.X, coords.Y, coords.Z });
    }

    /// <summary>
    /// Derives a child seed from a parent seed and a scalar index.
    /// </summary>
    public static long DeriveSeedIndexed(long parentSeed, long index)
    {
        return HashIntegers(new Array<long> { parentSeed, index });
    }
}
