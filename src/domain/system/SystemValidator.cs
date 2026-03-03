using System.Collections.Generic;
using System.Globalization;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Validation;

namespace StarGen.Domain.Systems;

/// <summary>
/// Validates complete solar systems.
/// </summary>
public static class SystemValidator
{
    /// <summary>
    /// Validates a complete solar system.
    /// </summary>
    public static ValidationResult Validate(SolarSystem system)
    {
        ValidationResult result = new();
        ValidateIdentity(system, result);
        ValidateHierarchy(system, result);
        ValidateBodies(system, result);
        ValidateOrbitHosts(system, result);
        ValidateOrbitalRelationships(system, result);
        ValidateAsteroidBelts(system, result);
        return result;
    }

    /// <summary>
    /// Performs a fast validity check.
    /// </summary>
    public static bool IsValid(SolarSystem? system)
    {
        if (system == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(system.Id))
        {
            return false;
        }

        if (system.Hierarchy == null || !system.Hierarchy.IsValid())
        {
            return false;
        }

        if (system.StarIds.Count == 0)
        {
            return false;
        }

        foreach (string starId in system.StarIds)
        {
            if (!system.Bodies.ContainsKey(starId))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validates identity fields.
    /// </summary>
    private static void ValidateIdentity(SolarSystem system, ValidationResult result)
    {
        if (string.IsNullOrEmpty(system.Id))
        {
            result.AddError("id", "System ID cannot be empty");
        }

        if (string.IsNullOrEmpty(system.Name))
        {
            result.AddWarning("name", "System name is empty");
        }
    }

    /// <summary>
    /// Validates the stellar hierarchy.
    /// </summary>
    private static void ValidateHierarchy(SolarSystem system, ValidationResult result)
    {
        if (system.Hierarchy == null)
        {
            result.AddError("hierarchy", "System hierarchy is null");
            return;
        }

        if (!system.Hierarchy.IsValid())
        {
            result.AddError("hierarchy", "System hierarchy is invalid (no root)");
            return;
        }

        int hierarchyStarCount = system.Hierarchy.GetStarCount();
        int bodyStarCount = system.StarIds.Count;
        if (hierarchyStarCount != bodyStarCount)
        {
            result.AddError("hierarchy", $"Hierarchy star count ({hierarchyStarCount}) doesn't match body star count ({bodyStarCount})");
        }

        foreach (string starId in system.Hierarchy.GetAllStarIds())
        {
            if (!system.Bodies.ContainsKey(starId))
            {
                result.AddError("hierarchy", $"Hierarchy references non-existent star: {starId}");
            }
        }

        foreach (HierarchyNode barycenter in system.Hierarchy.GetAllBarycenters())
        {
            if (barycenter.SeparationM <= 0.0)
            {
                result.AddError($"hierarchy.{barycenter.Id}", "Barycenter separation must be positive");
            }

            if (barycenter.Eccentricity < 0.0 || barycenter.Eccentricity >= 1.0)
            {
                result.AddError($"hierarchy.{barycenter.Id}", "Barycenter eccentricity must be in [0, 1)");
            }

            if (barycenter.Children.Count != 2)
            {
                result.AddError($"hierarchy.{barycenter.Id}", "Barycenter must have exactly 2 children");
            }
        }
    }

    /// <summary>
    /// Validates celestial bodies and body-index consistency.
    /// </summary>
    private static void ValidateBodies(SolarSystem system, ValidationResult result)
    {
        if (system.Bodies.Count == 0)
        {
            result.AddError("bodies", "System has no bodies");
            return;
        }

        if (system.StarIds.Count == 0)
        {
            result.AddError("stars", "System has no stars");
        }

        foreach (string bodyId in system.Bodies.Keys)
        {
            CelestialBody body = system.Bodies[bodyId];
            ValidationResult bodyResult = CelestialValidator.Validate(body);
            foreach (ValidationError error in bodyResult.Errors)
            {
                string prefixedField = $"bodies.{bodyId}.{error.Field}";
                if (error.Severity == ValidationError.SeverityLevel.Error)
                {
                    result.AddError(prefixedField, error.Message);
                }
                else
                {
                    result.AddWarning(prefixedField, error.Message);
                }
            }
        }

        ValidateBodyIndex(system, system.StarIds, CelestialType.Type.Star, "star_ids", "Star", result);
        ValidateBodyIndex(system, system.PlanetIds, CelestialType.Type.Planet, "planet_ids", "Planet", result);
        ValidateBodyIndex(system, system.MoonIds, CelestialType.Type.Moon, "moon_ids", "Moon", result);
        ValidateBodyIndex(system, system.AsteroidIds, CelestialType.Type.Asteroid, "asteroid_ids", "Asteroid", result);
    }

    /// <summary>
    /// Validates orbit-host data.
    /// </summary>
    private static void ValidateOrbitHosts(SolarSystem system, ValidationResult result)
    {
        for (int index = 0; index < system.OrbitHosts.Count; index += 1)
        {
            OrbitHost host = system.OrbitHosts[index];
            string prefix = $"orbit_hosts[{index}]";

            if (string.IsNullOrEmpty(host.NodeId))
            {
                result.AddError($"{prefix}.node_id", "Orbit host node_id is empty");
            }

            if (host.CombinedMassKg <= 0.0)
            {
                result.AddError($"{prefix}.combined_mass_kg", "Orbit host mass must be positive");
            }

            if (host.InnerStabilityM < 0.0)
            {
                result.AddError($"{prefix}.inner_stability_m", "Inner stability limit cannot be negative");
            }

            if (host.OuterStabilityM <= host.InnerStabilityM)
            {
                result.AddWarning(prefix, "Orbit host has no valid stable zone (outer <= inner)");
            }
        }
    }

    /// <summary>
    /// Validates moon relationships and planet-spacing overlaps.
    /// </summary>
    private static void ValidateOrbitalRelationships(SolarSystem system, ValidationResult result)
    {
        foreach (string moonId in system.MoonIds)
        {
            CelestialBody? moon = system.GetBody(moonId);
            if (moon == null || !moon.HasOrbital())
            {
                continue;
            }

            string parentId = moon.Orbital!.ParentId;
            if (string.IsNullOrEmpty(parentId))
            {
                result.AddWarning($"bodies.{moonId}.orbital.parent_id", "Moon has no parent ID");
                continue;
            }

            CelestialBody? parent = system.GetBody(parentId);
            if (parent == null)
            {
                result.AddError($"bodies.{moonId}.orbital.parent_id", $"Moon references non-existent parent: {parentId}");
                continue;
            }

            if (parent.Type != CelestialType.Type.Planet)
            {
                result.AddWarning($"bodies.{moonId}.orbital.parent_id", $"Moon parent is not a planet: {parentId}");
            }
        }

        CheckOrbitalOverlaps(system, result);
    }

    /// <summary>
    /// Checks adjacent planets for orbital overlaps and inadequate spacing.
    /// </summary>
    private static void CheckOrbitalOverlaps(SolarSystem system, ValidationResult result)
    {
        List<CelestialBody> sortedPlanets = new();
        foreach (CelestialBody planet in system.GetPlanets())
        {
            sortedPlanets.Add(planet);
        }

        if (sortedPlanets.Count < 2)
        {
            return;
        }

        sortedPlanets.Sort((left, right) =>
        {
            double leftDistance = left.HasOrbital() ? left.Orbital!.SemiMajorAxisM : 0.0;
            double rightDistance = right.HasOrbital() ? right.Orbital!.SemiMajorAxisM : 0.0;
            return leftDistance.CompareTo(rightDistance);
        });

        Dictionary<string, OrbitHost> hostMap = new();
        foreach (OrbitHost host in system.OrbitHosts)
        {
            hostMap[host.NodeId] = host;
        }

        for (int index = 0; index < sortedPlanets.Count - 1; index += 1)
        {
            CelestialBody inner = sortedPlanets[index];
            CelestialBody outer = sortedPlanets[index + 1];
            if (!inner.HasOrbital() || !outer.HasOrbital())
            {
                continue;
            }

            double innerApoapsis = inner.Orbital!.GetApoapsisM();
            double outerPeriapsis = outer.Orbital!.GetPeriapsisM();
            if (innerApoapsis >= outerPeriapsis)
            {
                result.AddWarning("orbital_overlap", $"Potential orbital overlap between {inner.Id} and {outer.Id}");
            }

            string parentId = inner.Orbital.ParentId;
            if (string.IsNullOrEmpty(parentId) || parentId != outer.Orbital.ParentId)
            {
                continue;
            }

            if (!hostMap.ContainsKey(parentId))
            {
                continue;
            }

            OrbitHost host = hostMap[parentId];
            if (host.CombinedMassKg <= 0.0)
            {
                continue;
            }

            double requiredSpacingM = OrbitalMechanics.CalculateMinimumPlanetSpacing(
                inner.Physical.MassKg,
                outer.Physical.MassKg,
                host.CombinedMassKg,
                inner.Orbital.SemiMajorAxisM);
            double actualSpacingM = outer.Orbital.SemiMajorAxisM - inner.Orbital.SemiMajorAxisM;
            if (actualSpacingM < requiredSpacingM)
            {
                result.AddWarning(
                    "orbital_stability",
                    $"Adjacent planets {inner.Id} and {outer.Id} are closer than ~10 mutual Hill radii (gap {FormatMeters(actualSpacingM)} m, need {FormatMeters(requiredSpacingM)} m)");
            }
        }
    }

    /// <summary>
    /// Validates asteroid-belt data.
    /// </summary>
    private static void ValidateAsteroidBelts(SolarSystem system, ValidationResult result)
    {
        for (int index = 0; index < system.AsteroidBelts.Count; index += 1)
        {
            AsteroidBelt belt = system.AsteroidBelts[index];
            string prefix = $"asteroid_belts[{index}]";

            if (string.IsNullOrEmpty(belt.Id))
            {
                result.AddError($"{prefix}.id", "Belt ID is empty");
            }

            if (belt.InnerRadiusM <= 0.0)
            {
                result.AddError($"{prefix}.inner_radius_m", "Belt inner radius must be positive");
            }

            if (belt.OuterRadiusM <= belt.InnerRadiusM)
            {
                result.AddError(prefix, "Belt outer radius must be greater than inner radius");
            }

            if (belt.TotalMassKg < 0.0)
            {
                result.AddError($"{prefix}.total_mass_kg", "Belt mass cannot be negative");
            }

            foreach (string asteroidId in belt.MajorAsteroidIds)
            {
                if (!system.Bodies.ContainsKey(asteroidId))
                {
                    result.AddError($"{prefix}.major_asteroid_ids", $"Belt references non-existent asteroid: {asteroidId}");
                }
            }
        }
    }

    /// <summary>
    /// Validates a typed body id list against the main body map.
    /// </summary>
    private static void ValidateBodyIndex(
        SolarSystem system,
        Godot.Collections.Array<string> ids,
        CelestialType.Type expectedType,
        string fieldName,
        string label,
        ValidationResult result)
    {
        foreach (string bodyId in ids)
        {
            if (!system.Bodies.ContainsKey(bodyId))
            {
                result.AddError(fieldName, $"{label} ID references non-existent body: {bodyId}");
                continue;
            }

            CelestialBody body = system.Bodies[bodyId];
            if (body.Type != expectedType)
            {
                result.AddError(fieldName, $"{label} ID references non-{label.ToLowerInvariant()} body: {bodyId}");
            }
        }
    }

    /// <summary>
    /// Formats a meter distance for warning messages.
    /// </summary>
    private static string FormatMeters(double meters)
    {
        return meters.ToString(CultureInfo.InvariantCulture);
    }
}
