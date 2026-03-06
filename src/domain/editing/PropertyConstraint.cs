using System;
using Godot;

namespace StarGen.Domain.Editing;

/// <summary>
/// A single property's constraint: valid range, current value, lock state.
/// Immutable after construction — create a new one via With* methods to change.
/// Pure value type. No Nodes, no file IO.
/// </summary>
public partial class PropertyConstraint : RefCounted
{
    /// <summary>The property path this constraint applies to (e.g. "physical.mass_kg").</summary>
    public string PropertyPath { get; set; } = string.Empty;

    /// <summary>Minimum allowed value in base (SI) units. NegativeInfinity if unbounded below.</summary>
    public double MinValue { get; set; }

    /// <summary>Maximum allowed value in base (SI) units. PositiveInfinity if unbounded above.</summary>
    public double MaxValue { get; set; }

    /// <summary>Current value in base (SI) units.</summary>
    public double CurrentValue { get; set; }

    /// <summary>Whether the user has locked this property.</summary>
    public bool IsLocked { get; set; }

    /// <summary>Free-text reason explaining why this range is what it is.</summary>
    public string ConstraintReason { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new PropertyConstraint.
    /// </summary>
    public PropertyConstraint(
        string propertyPath = "",
        double minValue = double.NegativeInfinity,
        double maxValue = double.PositiveInfinity,
        double currentValue = 0.0,
        bool isLocked = false,
        string constraintReason = "")
    {
        PropertyPath = propertyPath;
        MinValue = minValue;
        MaxValue = maxValue;
        CurrentValue = currentValue;
        IsLocked = isLocked;
        ConstraintReason = constraintReason;
    }

    /// <summary>Returns whether current value falls inside [min, max].</summary>
    public bool IsValueInRange()
    {
        return CurrentValue >= MinValue && CurrentValue <= MaxValue;
    }

    /// <summary>Clamps the given value into this constraint's range.</summary>
    public double ClampValue(double value)
    {
        return System.Math.Clamp(value, MinValue, MaxValue);
    }

    /// <summary>Returns whether this constraint has any finite bounds.</summary>
    public bool HasBounds()
    {
        return double.IsFinite(MinValue) || double.IsFinite(MaxValue);
    }

    /// <summary>Returns a copy with the lock flag changed.</summary>
    public PropertyConstraint WithLock(bool locked)
    {
        return new PropertyConstraint(
            PropertyPath, MinValue, MaxValue, CurrentValue, locked, ConstraintReason);
    }

    /// <summary>Returns a copy with a new current value (does NOT clamp).</summary>
    public PropertyConstraint WithValue(double newValue)
    {
        return new PropertyConstraint(
            PropertyPath, MinValue, MaxValue, newValue, IsLocked, ConstraintReason);
    }

    /// <summary>Returns a copy with a narrowed range (intersection with given bounds).</summary>
    public PropertyConstraint IntersectedWith(double newMin, double newMax, string reason = "")
    {
        string combinedReason = ConstraintReason;
        bool narrowed = newMin > MinValue || newMax < MaxValue;
        if (narrowed && !string.IsNullOrEmpty(reason))
        {
            if (string.IsNullOrEmpty(combinedReason))
            {
                combinedReason = reason;
            }
            else
            {
                combinedReason = combinedReason + "; " + reason;
            }
        }

        double nMin = System.Math.Max(MinValue, newMin);
        double nMax = System.Math.Min(MaxValue, newMax);
        return new PropertyConstraint(
            PropertyPath, nMin, nMax, CurrentValue, IsLocked, combinedReason);
    }

    /// <summary>Returns whether this constraint's range is non-empty (min &lt;= max).</summary>
    public bool IsSatisfiable()
    {
        return MinValue <= MaxValue;
    }

    /// <summary>Converts this constraint to a dictionary for snapshotting / debugging.</summary>
    public Godot.Collections.Dictionary ToDict()
    {
        return new Godot.Collections.Dictionary
        {
            ["property_path"] = PropertyPath,
            ["min_value"] = MinValue,
            ["max_value"] = MaxValue,
            ["current_value"] = CurrentValue,
            ["is_locked"] = IsLocked,
            ["constraint_reason"] = ConstraintReason,
        };
    }

    /// <summary>Compatibility alias for legacy API naming.</summary>
    public Godot.Collections.Dictionary ToDictionary()
    {
        return ToDict();
    }
}
