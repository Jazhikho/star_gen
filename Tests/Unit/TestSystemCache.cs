#nullable enable annotations
#nullable disable warnings
using System;
using StarGen.Domain.Systems;
using StarGen.Services;

namespace StarGen.Tests.Unit;

/// <summary>
/// Unit tests for SystemCache.
/// </summary>
public static class TestSystemCache
{
    /// <summary>
    /// Tests cache starts empty.
    /// </summary>
    public static void TestStartsEmpty()
    {
        SystemCache cache = new SystemCache();
        if (cache.GetCacheSize() != 0)
        {
            throw new InvalidOperationException("Cache should start empty");
        }
    }

    /// <summary>
    /// Tests has system returns false for missing.
    /// </summary>
    public static void TestHasSystemReturnsFalseForMissing()
    {
        SystemCache cache = new SystemCache();
        if (cache.HasSystem(12345))
        {
            throw new InvalidOperationException("Should not have uncached system");
        }
    }

    /// <summary>
    /// Tests get system returns null for missing.
    /// </summary>
    public static void TestGetSystemReturnsNullForMissing()
    {
        SystemCache cache = new SystemCache();
        if (cache.GetSystem(12345) != null)
        {
            throw new InvalidOperationException("Should return null for uncached system");
        }
    }

    /// <summary>
    /// Tests put and get system.
    /// </summary>
    public static void TestPutAndGetSystem()
    {
        SystemCache cache = new SystemCache();
        SolarSystem system = new SolarSystem("test_1", "Test System");

        cache.PutSystem(12345, system);

        if (!cache.HasSystem(12345))
        {
            throw new InvalidOperationException("Should have cached system");
        }
        if (cache.GetSystem(12345) != system)
        {
            throw new InvalidOperationException("Should return cached system");
        }
    }

    /// <summary>
    /// Tests cache size increases.
    /// </summary>
    public static void TestCacheSizeIncreases()
    {
        SystemCache cache = new SystemCache();
        SolarSystem system1 = new SolarSystem("s1", "System One");
        SolarSystem system2 = new SolarSystem("s2", "System Two");

        cache.PutSystem(111, system1);
        if (cache.GetCacheSize() != 1)
        {
            throw new InvalidOperationException("Size should be 1");
        }

        cache.PutSystem(222, system2);
        if (cache.GetCacheSize() != 2)
        {
            throw new InvalidOperationException("Size should be 2");
        }
    }

    /// <summary>
    /// Tests overwrite same key.
    /// </summary>
    public static void TestOverwriteSameKey()
    {
        SystemCache cache = new SystemCache();
        SolarSystem system1 = new SolarSystem("first", "First");
        SolarSystem system2 = new SolarSystem("second", "Second");

        cache.PutSystem(12345, system1);
        cache.PutSystem(12345, system2);

        if (cache.GetCacheSize() != 1)
        {
            throw new InvalidOperationException("Size should still be 1");
        }
        if (cache.GetSystem(12345).Name != "Second")
        {
            throw new InvalidOperationException("Should have second system");
        }
    }

    /// <summary>
    /// Tests clear empties cache.
    /// </summary>
    public static void TestClearEmptiesCache()
    {
        SystemCache cache = new SystemCache();
        SolarSystem system = new SolarSystem("c", "Clear Test");
        cache.PutSystem(12345, system);
        cache.PutSystem(67890, system);

        cache.Clear();

        if (cache.GetCacheSize() != 0)
        {
            throw new InvalidOperationException("Cache should be empty after clear");
        }
        if (cache.HasSystem(12345))
        {
            throw new InvalidOperationException("Should not have system after clear");
        }
    }

    /// <summary>
    /// Tests different seeds are independent.
    /// </summary>
    public static void TestDifferentSeedsAreIndependent()
    {
        SystemCache cache = new SystemCache();
        SolarSystem system1 = new SolarSystem("one", "System One");
        SolarSystem system2 = new SolarSystem("two", "System Two");

        cache.PutSystem(111, system1);
        cache.PutSystem(222, system2);

        if (cache.GetSystem(111).Name != "System One")
        {
            throw new InvalidOperationException("First system correct");
        }
        if (cache.GetSystem(222).Name != "System Two")
        {
            throw new InvalidOperationException("Second system correct");
        }
    }
}
