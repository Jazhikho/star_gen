using System;
using System.Collections.Generic;

namespace StarGen.Concepts.ReligionGenerator
{
    /// <summary>
    /// Deterministic RNG for religion generation. Uses the same LCG as the reference JS implementation
    /// (1664525, 1013904223, mod 2^32) so the same seed produces the same sequence across ports.
    /// </summary>
    public sealed class ReligionRng
    {
        private uint _state;

        public ReligionRng(int seed)
        {
            _state = unchecked((uint)seed);
        }

        /// <summary>
        /// Returns a uniform double in [0, 1).
        /// </summary>
        public double NextDouble()
        {
            _state = unchecked((uint)((long)_state * 1664525L + 1013904223L));
            return _state / 4294967296.0;
        }

        /// <summary>
        /// Returns a uniform int in [min, max] inclusive.
        /// </summary>
        public int NextInt(int min, int max)
        {
            if (min > max)
            {
                int t = min;
                min = max;
                max = t;
            }
            long range = (long)max - min + 1;
            return min + (int)(NextDouble() * range);
        }

        /// <summary>
        /// Shuffles the list in place using Fisher-Yates. Deterministic for same seed.
        /// </summary>
        public void ShuffleInPlace<T>(IList<T> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = NextInt(0, i);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        /// <summary>
        /// Weighted choice: picks one option with probability proportional to weights.
        /// If total weight is zero, returns the first option.
        /// </summary>
        public static T WeightedChoice<T>(ReligionRng rng, IReadOnlyList<T> options, IReadOnlyList<double> weights)
        {
            if (options == null || options.Count == 0)
            {
                throw new ArgumentException("Options cannot be null or empty.");
            }
            if (weights == null || weights.Count != options.Count)
            {
                throw new ArgumentException("Weights count must match options count.");
            }

            double total = 0.0;
            for (int i = 0; i < weights.Count; i++)
            {
                total += weights[i];
            }

            if (total <= 0.0)
            {
                return options[0];
            }

            double r = rng.NextDouble() * total;
            for (int i = 0; i < options.Count; i++)
            {
                if (r < weights[i])
                {
                    return options[i];
                }
                r -= weights[i];
            }
            return options[options.Count - 1];
        }

        /// <summary>
        /// Weighted sample without replacement: picks up to k items, each draw proportional to weight.
        /// Items with zero weight are excluded. Fewer than k may be returned if pool is exhausted.
        /// </summary>
        public static List<T> WeightedSampleWithoutReplacement<T>(
            ReligionRng rng,
            IReadOnlyList<T> items,
            Func<T, double> getWeight,
            int k)
        {
            if (items == null || getWeight == null || k <= 0)
            {
                return new List<T>();
            }

            List<T> pool = new List<T>();
            List<double> weights = new List<double>();
            foreach (T item in items)
            {
                double w = getWeight(item);
                if (w > 0.0)
                {
                    pool.Add(item);
                    weights.Add(w);
                }
            }

            List<T> result = new List<T>(Math.Min(k, pool.Count));
            for (int n = 0; n < k && pool.Count > 0; n++)
            {
                double total = 0.0;
                foreach (double w in weights)
                {
                    total += w;
                }
                if (total <= 0.0)
                {
                    break;
                }
                double r = rng.NextDouble() * total;
                for (int i = 0; i < pool.Count; i++)
                {
                    if (r < weights[i])
                    {
                        result.Add(pool[i]);
                        pool.RemoveAt(i);
                        weights.RemoveAt(i);
                        break;
                    }
                    r -= weights[i];
                }
            }
            return result;
        }
    }
}
