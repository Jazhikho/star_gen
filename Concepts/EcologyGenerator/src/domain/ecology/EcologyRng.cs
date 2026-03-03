using System;

namespace StarGen.Domain.Ecology
{
    /// <summary>
    /// Deterministic RNG wrapper for ecology generation.
    /// All randomness must go through this class.
    /// </summary>
    public class EcologyRng
    {
        private Random _random;
        private ulong _seed;

        public ulong Seed
        {
            get { return _seed; }
        }

        public EcologyRng(ulong seed)
        {
            _seed = seed;
            _random = new Random((int)(seed % int.MaxValue));
        }

        /// <summary>
        /// Returns a random float between 0.0 and 1.0.
        /// </summary>
        public float NextFloat()
        {
            return (float)_random.NextDouble();
        }

        /// <summary>
        /// Returns a random float between min and max.
        /// </summary>
        public float NextFloatRange(float min, float max)
        {
            return min + NextFloat() * (max - min);
        }

        /// <summary>
        /// Returns a random integer between min (inclusive) and max (exclusive).
        /// </summary>
        public int NextIntRange(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Returns true with the given probability (0-1).
        /// </summary>
        public bool NextBool(float probability)
        {
            return NextFloat() < probability;
        }

        /// <summary>
        /// Picks a random element from an array.
        /// </summary>
        public T PickRandom<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                throw new ArgumentException("Array cannot be null or empty");
            }
            return array[NextIntRange(0, array.Length)];
        }

        /// <summary>
        /// Shuffles an array in place using Fisher-Yates.
        /// </summary>
        public void Shuffle<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = NextIntRange(0, i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
    }
}
