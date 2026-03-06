using System.Collections.Generic;
using Godot;

namespace StarGen.Domain.Editing;

/// <summary>
/// A set of property constraints keyed by property path.
/// Tracks which properties are locked and what ranges are valid.
/// Pure domain. No Nodes, no file IO.
/// </summary>
public partial class ConstraintSet : RefCounted
{
    private readonly Dictionary<string, PropertyConstraint> _constraints = new();

    /// <summary>
    /// Adds or replaces a constraint.
    /// </summary>
    public void SetConstraint(PropertyConstraint constraint)
    {
        _constraints[constraint.PropertyPath] = constraint;
    }

    /// <summary>
    /// Gets a constraint by property path, or null if not present.
    /// </summary>
    public PropertyConstraint? GetConstraint(string propertyPath)
    {
        if (_constraints.TryGetValue(propertyPath, out PropertyConstraint? c))
        {
            return c;
        }

        return null;
    }

    /// <summary>
    /// Returns whether a constraint exists for the given path.
    /// </summary>
    public bool HasConstraint(string propertyPath)
    {
        return _constraints.ContainsKey(propertyPath);
    }

    /// <summary>
    /// Returns all property paths currently tracked.
    /// </summary>
    public List<string> GetAllPaths()
    {
        return new List<string>(_constraints.Keys);
    }

    /// <summary>
    /// Returns property paths that are locked.
    /// </summary>
    public List<string> GetLockedPaths()
    {
        List<string> paths = new();
        foreach (KeyValuePair<string, PropertyConstraint> kv in _constraints)
        {
            if (kv.Value.IsLocked)
            {
                paths.Add(kv.Key);
            }
        }

        return paths;
    }

    /// <summary>
    /// Returns locked values as a property_path → value dictionary.
    /// </summary>
    public Godot.Collections.Dictionary GetLockedOverrides()
    {
        Godot.Collections.Dictionary result = new();
        foreach (KeyValuePair<string, PropertyConstraint> kv in _constraints)
        {
            if (kv.Value.IsLocked)
            {
                result[kv.Key] = kv.Value.CurrentValue;
            }
        }

        return result;
    }

    /// <summary>
    /// Locks a property at its current value. No-op if not tracked.
    /// </summary>
    public void Lock(string propertyPath)
    {
        PropertyConstraint? c = GetConstraint(propertyPath);
        if (c == null)
        {
            return;
        }

        _constraints[propertyPath] = c.WithLock(true);
    }

    /// <summary>
    /// Unlocks a property. No-op if not tracked.
    /// </summary>
    public void Unlock(string propertyPath)
    {
        PropertyConstraint? c = GetConstraint(propertyPath);
        if (c == null)
        {
            return;
        }

        _constraints[propertyPath] = c.WithLock(false);
    }

    /// <summary>
    /// Sets a property's current value without changing its lock state. Does NOT clamp to range.
    /// </summary>
    public void SetValue(string propertyPath, double value)
    {
        PropertyConstraint? c = GetConstraint(propertyPath);
        if (c == null)
        {
            return;
        }

        _constraints[propertyPath] = c.WithValue(value);
    }

    /// <summary>
    /// Returns whether every constraint is satisfiable and every current value is within its range.
    /// </summary>
    public bool IsConsistent()
    {
        foreach (PropertyConstraint c in _constraints.Values)
        {
            if (!c.IsSatisfiable() || !c.IsValueInRange())
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns property paths whose current value is outside their allowed range.
    /// </summary>
    public List<string> GetViolations()
    {
        List<string> violations = new();
        foreach (KeyValuePair<string, PropertyConstraint> kv in _constraints)
        {
            if (!kv.Value.IsSatisfiable() || !kv.Value.IsValueInRange())
            {
                violations.Add(kv.Key);
            }
        }

        return violations;
    }

    /// <summary>
    /// Clamps every unlocked property's current value into its allowed range.
    /// </summary>
    /// <returns>Property paths that were modified by clamping.</returns>
    public List<string> ClampUnlocked()
    {
        List<string> modified = new();
        foreach (KeyValuePair<string, PropertyConstraint> kv in _constraints)
        {
            PropertyConstraint c = kv.Value;
            if (c.IsLocked)
            {
                continue;
            }

            if (c.IsValueInRange())
            {
                continue;
            }

            double clamped = c.ClampValue(c.CurrentValue);
            _constraints[kv.Key] = c.WithValue(clamped);
            modified.Add(kv.Key);
        }

        return modified;
    }

    /// <summary>
    /// Returns the number of constraints in the set.
    /// </summary>
    public int Size()
    {
        return _constraints.Count;
    }

    /// <summary>
    /// Returns the (min, max) range for a property as Vector2, or the fallback if not tracked.
    /// </summary>
    public Vector2 GetRange(string propertyPath, Vector2 fallback)
    {
        PropertyConstraint? c = GetConstraint(propertyPath);
        if (c == null)
        {
            return fallback;
        }

        return new Vector2((float)c.MinValue, (float)c.MaxValue);
    }
}
