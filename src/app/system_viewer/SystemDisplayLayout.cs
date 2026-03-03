using Godot;
using StarGen.Domain.Celestial;
using StarGen.Domain.Math;
using StarGen.Domain.Systems;
using System.Collections.Generic;

namespace StarGen.App.SystemViewer;

/// <summary>
/// Layout calculations for the system viewer.
/// </summary>
public static class SystemDisplayLayout
{
    /// <summary>
    /// Base display radius for a one-solar-radius star.
    /// </summary>
    public const float StarBaseRadius = 3.0f;

    /// <summary>
    /// Minimum star display radius.
    /// </summary>
    public const float StarMinRadius = 1.0f;

    /// <summary>
    /// Maximum star display radius.
    /// </summary>
    public const float StarMaxRadius = 9.0f;

    /// <summary>
    /// Minimum planet display radius.
    /// </summary>
    public const float PlanetMinRadius = 0.25f;

    /// <summary>
    /// Maximum planet display radius.
    /// </summary>
    public const float PlanetMaxRadius = 2.0f;

    /// <summary>
    /// Half-width of an asteroid-belt display band.
    /// </summary>
    public const float BeltDisplayHalfWidth = 0.5f;

    /// <summary>
    /// Minimum surface-to-surface gap for the first orbit around a star.
    /// </summary>
    public const float FirstOrbitSurfaceGap = 4.0f;

    /// <summary>
    /// Distance between subsequent orbit slots.
    /// </summary>
    public const float OrbitSpacing = 6.0f;

    /// <summary>
    /// Minimum binary-gap padding after accounting for orbital extents.
    /// </summary>
    public const float BinaryBufferGap = 8.0f;

    /// <summary>
    /// Minimum circum-binary gap after the inner-system extent.
    /// </summary>
    public const float PTypeBufferGap = 6.0f;

    /// <summary>
    /// Visual base orbital period in seconds.
    /// </summary>
    public const float BaseOrbitalPeriod = 20.0f;

    /// <summary>
    /// Visual Kepler-like period exponent.
    /// </summary>
    public const float OrbitalPeriodExponent = 0.8f;

    /// <summary>
    /// Calculates display radius for a star using logarithmic scaling.
    /// </summary>
    public static float CalculateStarDisplayRadius(double radiusM)
    {
        float solarRadii = (float)(radiusM / Units.SolarRadiusMeters);
        solarRadii = Mathf.Max(solarRadii, 0.01f);

        float displayRadius = StarBaseRadius + (Mathf.Log(solarRadii) / Mathf.Log(10.0f));
        return Mathf.Clamp(displayRadius, StarMinRadius, StarMaxRadius);
    }

    /// <summary>
    /// Returns the absolute log10 adjustment used by star-size scaling.
    /// </summary>
    public static float GetStarLogAdjustment(double radiusM)
    {
        float solarRadii = (float)(radiusM / Units.SolarRadiusMeters);
        solarRadii = Mathf.Max(solarRadii, 0.01f);
        return Mathf.Abs(Mathf.Log(solarRadii) / Mathf.Log(10.0f));
    }

    /// <summary>
    /// Calculates display radius for a planet using logarithmic scaling.
    /// </summary>
    public static float CalculatePlanetDisplayRadius(double radiusM)
    {
        float earthRadii = (float)(radiusM / Units.EarthRadiusMeters);
        earthRadii = Mathf.Max(earthRadii, 0.01f);

        float logRadius = Mathf.Log(earthRadii) / Mathf.Log(10.0f);
        float t = (logRadius + 1.0f) / 2.5f;
        float displayRadius = Mathf.Lerp(PlanetMinRadius, PlanetMaxRadius, t);
        return Mathf.Clamp(displayRadius, PlanetMinRadius, PlanetMaxRadius);
    }

    /// <summary>
    /// Calculates the first orbit radius around a star.
    /// </summary>
    public static float CalculateFirstOrbitRadiusForStar(
        float starDisplayRadius,
        float maxPlanetRadius,
        float logAdjustment)
    {
        return starDisplayRadius + maxPlanetRadius + FirstOrbitSurfaceGap + logAdjustment;
    }

    /// <summary>
    /// Calculates the radius of an indexed orbit slot.
    /// </summary>
    public static float CalculateNthOrbitRadius(float firstOrbitRadius, int orbitIndex)
    {
        return firstOrbitRadius + (orbitIndex * OrbitSpacing);
    }

    /// <summary>
    /// Calculates a visual orbital period from display radius.
    /// </summary>
    public static float CalculateOrbitalPeriod(float orbitRadius)
    {
        if (orbitRadius <= 0.0f)
        {
            return BaseOrbitalPeriod;
        }

        float normalizedRadius = orbitRadius / 10.0f;
        return BaseOrbitalPeriod * Mathf.Pow(normalizedRadius, OrbitalPeriodExponent);
    }

    /// <summary>
    /// Calculates the maximum rendered belt inclination from orbit distance.
    /// </summary>
    public static float CalculateBeltMaxInclinationDeg(float orbitRadius)
    {
        const float minInclination = 2.0f;
        const float maxInclination = 24.0f;
        float t = Mathf.Clamp((orbitRadius - 10.0f) / 40.0f, 0.0f, 1.0f);
        return Mathf.Lerp(minInclination, maxInclination, t);
    }

    /// <summary>
    /// Calculates the complete viewer layout for a solar system.
    /// </summary>
    public static SystemLayout CalculateLayout(SolarSystem? system)
    {
        SystemLayout layout = new();
        if (system == null || system.Hierarchy == null || !system.Hierarchy.IsValid() || system.Hierarchy.Root == null)
        {
            return layout;
        }

        global::System.Collections.Generic.Dictionary<string, Godot.Collections.Array<CelestialBody>> planetsByHost = GroupPlanetsByHost(system);
        global::System.Collections.Generic.Dictionary<string, Godot.Collections.Array<AsteroidBelt>> beltsByHost = GroupBeltsByHost(system);
        global::System.Collections.Generic.Dictionary<string, int> hostContentCounts = CalculateHostContentCounts(planetsByHost, beltsByHost);
        global::System.Collections.Generic.Dictionary<string, float> maxPlanetRadii = CalculateMaxPlanetRadii(planetsByHost);

        CalculateNodeExtents(system.Hierarchy.Root, system, hostContentCounts, maxPlanetRadii, layout);
        PositionHierarchyNode(system.Hierarchy.Root, Vector3.Zero, null, system, layout);
        PositionOrbitContent(system, planetsByHost, beltsByHost, layout);
        CalculateTotalExtent(layout);
        return layout;
    }

    /// <summary>
    /// Updates all animated orbital positions in an existing layout.
    /// </summary>
    public static void UpdateOrbits(SystemLayout? layout, float delta)
    {
        if (layout == null)
        {
            return;
        }

        foreach (string starId in layout.StarOrbits.Keys)
        {
            BodyLayout orbitLayout = layout.StarOrbits[starId];
            if (!orbitLayout.IsOrbiting || orbitLayout.OrbitalPeriod <= 0.0f)
            {
                continue;
            }

            float angularVelocity = Mathf.Tau / orbitLayout.OrbitalPeriod;
            orbitLayout.OrbitalAngle += angularVelocity * delta;
            orbitLayout.UpdatePositionFromAngle();

            if (layout.BodyLayouts.ContainsKey(starId))
            {
                BodyLayout starBody = layout.BodyLayouts[starId];
                starBody.Position = orbitLayout.Position;
                starBody.OrbitalAngle = orbitLayout.OrbitalAngle;
            }

            if (layout.StarToNode.ContainsKey(starId))
            {
                string nodeId = layout.StarToNode[starId];
                layout.HostPositions[nodeId] = orbitLayout.Position;
                if (layout.NodeExtents.ContainsKey(nodeId))
                {
                    layout.NodeExtents[nodeId].CenterPosition = orbitLayout.Position;
                }
            }
        }

        foreach (string bodyId in layout.BodyLayouts.Keys)
        {
            if (layout.StarOrbits.ContainsKey(bodyId))
            {
                continue;
            }

            BodyLayout bodyLayout = layout.BodyLayouts[bodyId];
            if (!bodyLayout.IsOrbiting || bodyLayout.OrbitalPeriod <= 0.0f)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(bodyLayout.OrbitParentId))
            {
                bodyLayout.OrbitCenter = layout.GetHostPosition(bodyLayout.OrbitParentId);
            }

            float angularVelocity = Mathf.Tau / bodyLayout.OrbitalPeriod;
            bodyLayout.OrbitalAngle += angularVelocity * delta;
            bodyLayout.UpdatePositionFromAngle();
        }
    }

    /// <summary>
    /// Groups planets by orbit host identifier.
    /// </summary>
    private static global::System.Collections.Generic.Dictionary<string, Godot.Collections.Array<CelestialBody>> GroupPlanetsByHost(SolarSystem system)
    {
        global::System.Collections.Generic.Dictionary<string, List<CelestialBody>> grouped = new();
        foreach (CelestialBody planet in system.GetPlanets())
        {
            string hostId = FindPlanetHostId(planet, system);
            if (!grouped.ContainsKey(hostId))
            {
                grouped[hostId] = new List<CelestialBody>();
            }

            grouped[hostId].Add(planet);
        }

        global::System.Collections.Generic.Dictionary<string, Godot.Collections.Array<CelestialBody>> result = new();
        foreach (string hostId in grouped.Keys)
        {
            List<CelestialBody> planets = grouped[hostId];
            planets.Sort(static (a, b) =>
            {
                double distanceA = a.HasOrbital() ? a.Orbital!.SemiMajorAxisM : 0.0;
                double distanceB = b.HasOrbital() ? b.Orbital!.SemiMajorAxisM : 0.0;
                return distanceA.CompareTo(distanceB);
            });

            Godot.Collections.Array<CelestialBody> array = new();
            foreach (CelestialBody planet in planets)
            {
                array.Add(planet);
            }

            result[hostId] = array;
        }

        return result;
    }

    /// <summary>
    /// Groups asteroid belts by host identifier.
    /// </summary>
    private static global::System.Collections.Generic.Dictionary<string, Godot.Collections.Array<AsteroidBelt>> GroupBeltsByHost(SolarSystem system)
    {
        global::System.Collections.Generic.Dictionary<string, List<AsteroidBelt>> grouped = new();
        foreach (AsteroidBelt belt in system.AsteroidBelts)
        {
            string hostId = belt.OrbitHostId;
            if (!grouped.ContainsKey(hostId))
            {
                grouped[hostId] = new List<AsteroidBelt>();
            }

            grouped[hostId].Add(belt);
        }

        global::System.Collections.Generic.Dictionary<string, Godot.Collections.Array<AsteroidBelt>> result = new();
        foreach (string hostId in grouped.Keys)
        {
            List<AsteroidBelt> belts = grouped[hostId];
            belts.Sort(static (a, b) => a.GetCenterM().CompareTo(b.GetCenterM()));

            Godot.Collections.Array<AsteroidBelt> array = new();
            foreach (AsteroidBelt belt in belts)
            {
                array.Add(belt);
            }

            result[hostId] = array;
        }

        return result;
    }

    /// <summary>
    /// Calculates the total orbit-content count per host.
    /// </summary>
    private static global::System.Collections.Generic.Dictionary<string, int> CalculateHostContentCounts(
        global::System.Collections.Generic.Dictionary<string, Godot.Collections.Array<CelestialBody>> planetsByHost,
        global::System.Collections.Generic.Dictionary<string, Godot.Collections.Array<AsteroidBelt>> beltsByHost)
    {
        global::System.Collections.Generic.Dictionary<string, int> counts = new();
        foreach (string hostId in planetsByHost.Keys)
        {
            int total = planetsByHost[hostId].Count;
            counts[hostId] = counts.ContainsKey(hostId) ? counts[hostId] + total : total;
        }

        foreach (string hostId in beltsByHost.Keys)
        {
            int total = beltsByHost[hostId].Count;
            counts[hostId] = counts.ContainsKey(hostId) ? counts[hostId] + total : total;
        }

        return counts;
    }

    /// <summary>
    /// Finds the orbit host identifier for a planet.
    /// </summary>
    private static string FindPlanetHostId(CelestialBody planet, SolarSystem system)
    {
        if (planet.HasOrbital() && !string.IsNullOrEmpty(planet.Orbital!.ParentId))
        {
            return planet.Orbital.ParentId;
        }

        if (system.OrbitHosts.Count > 0)
        {
            return system.OrbitHosts[0].NodeId;
        }

        if (system.Hierarchy?.Root != null)
        {
            return system.Hierarchy.Root.Id;
        }

        return string.Empty;
    }

    /// <summary>
    /// Calculates the largest planet display radius per host.
    /// </summary>
    private static global::System.Collections.Generic.Dictionary<string, float> CalculateMaxPlanetRadii(
        global::System.Collections.Generic.Dictionary<string, Godot.Collections.Array<CelestialBody>> planetsByHost)
    {
        global::System.Collections.Generic.Dictionary<string, float> result = new();
        foreach (string hostId in planetsByHost.Keys)
        {
            float maxRadius = PlanetMinRadius;
            foreach (CelestialBody planet in planetsByHost[hostId])
            {
                float displayRadius = CalculatePlanetDisplayRadius(planet.Physical.RadiusM);
                maxRadius = Mathf.Max(maxRadius, displayRadius);
            }

            result[hostId] = maxRadius;
        }

        return result;
    }

    /// <summary>
    /// Recursively calculates the extent radius for a hierarchy node.
    /// </summary>
    private static float CalculateNodeExtents(
        HierarchyNode? node,
        SolarSystem system,
        global::System.Collections.Generic.Dictionary<string, int> hostContentCounts,
        global::System.Collections.Generic.Dictionary<string, float> maxPlanetRadii,
        SystemLayout layout)
    {
        if (node == null)
        {
            return 0.0f;
        }

        NodeExtent extent = new(node.Id);
        extent.MaxPlanetRadius = maxPlanetRadii.ContainsKey(node.Id) ? maxPlanetRadii[node.Id] : PlanetMaxRadius;

        if (node.IsStar())
        {
            extent.StarBodyId = node.StarId;
            CelestialBody? star = system.GetBody(node.StarId);
            if (star == null)
            {
                extent.StarDisplayRadius = StarBaseRadius;
                extent.FirstOrbitRadius = StarBaseRadius + PlanetMaxRadius + FirstOrbitSurfaceGap;
                extent.ExtentRadius = StarBaseRadius;
                extent.InnerExtentRadius = StarBaseRadius;
            }
            else
            {
                float starDisplay = CalculateStarDisplayRadius(star.Physical.RadiusM);
                float logAdjustment = GetStarLogAdjustment(star.Physical.RadiusM);
                float firstOrbit = CalculateFirstOrbitRadiusForStar(starDisplay, extent.MaxPlanetRadius, logAdjustment);

                extent.StarDisplayRadius = starDisplay;
                extent.FirstOrbitRadius = firstOrbit;
                extent.STypePlanetCount = hostContentCounts.ContainsKey(node.Id) ? hostContentCounts[node.Id] : 0;

                if (extent.STypePlanetCount > 0)
                {
                    float outermostOrbit = CalculateNthOrbitRadius(firstOrbit, extent.STypePlanetCount - 1);
                    extent.ExtentRadius = outermostOrbit + extent.MaxPlanetRadius;
                    extent.InnerExtentRadius = extent.ExtentRadius;
                }
                else
                {
                    extent.ExtentRadius = starDisplay;
                    extent.InnerExtentRadius = starDisplay;
                }
            }

            extent.MaxSweepRadius = extent.ExtentRadius;
            layout.NodeExtents[node.Id] = extent;
            return extent.ExtentRadius;
        }

        List<float> childExtentValues = new();
        List<NodeExtent> childExtentObjects = new();
        List<float> masses = new();
        float totalMass = 0.0f;

        foreach (HierarchyNode child in node.Children)
        {
            float childExtentValue = CalculateNodeExtents(child, system, hostContentCounts, maxPlanetRadii, layout);
            childExtentValues.Add(childExtentValue);
            if (layout.NodeExtents.ContainsKey(child.Id))
            {
                childExtentObjects.Add(layout.NodeExtents[child.Id]);
            }

            float mass = GetNodeMass(child, system);
            masses.Add(mass);
            totalMass += mass;
        }

        float separation = BinaryBufferGap;
        foreach (float childExtent in childExtentValues)
        {
            separation += childExtent;
        }

        extent.BinarySeparation = separation;
        float maxDistanceToEdge = 0.0f;

        if (node.Children.Count >= 2 && totalMass > 0.0f && childExtentObjects.Count >= 2)
        {
            float massA = masses[0];
            float massB = masses[1];
            NodeExtent extentA = childExtentObjects[0];
            NodeExtent extentB = childExtentObjects[1];
            float childExtentA = childExtentValues[0];
            float childExtentB = childExtentValues[1];

            float orbitRadiusA = separation * massB / totalMass;
            float orbitRadiusB = separation * massA / totalMass;
            extentA.OrbitRadiusAroundParent = orbitRadiusA;
            extentA.MaxSweepRadius = childExtentA + orbitRadiusA;
            extentB.OrbitRadiusAroundParent = orbitRadiusB;
            extentB.MaxSweepRadius = childExtentB + orbitRadiusB;

            float requiredSeparation = extentA.MaxSweepRadius + extentB.MaxSweepRadius + BinaryBufferGap;
            if (requiredSeparation > separation)
            {
                extent.BinarySeparation = requiredSeparation;
                separation = requiredSeparation;
                orbitRadiusA = separation * massB / totalMass;
                orbitRadiusB = separation * massA / totalMass;
                extentA.OrbitRadiusAroundParent = orbitRadiusA;
                extentA.MaxSweepRadius = childExtentA + orbitRadiusA;
                extentB.OrbitRadiusAroundParent = orbitRadiusB;
                extentB.MaxSweepRadius = childExtentB + orbitRadiusB;
            }

            float offsetA = separation * massB / totalMass;
            float offsetB = separation * massA / totalMass;
            maxDistanceToEdge = Mathf.Max(offsetA + extentA.MaxSweepRadius, offsetB + extentB.MaxSweepRadius);
            extent.InnerExtentRadius = Mathf.Max(extentA.MaxSweepRadius, extentB.MaxSweepRadius);
        }
        else if (node.Children.Count == 1 && childExtentObjects.Count >= 1)
        {
            NodeExtent singleExtent = childExtentObjects[0];
            singleExtent.OrbitRadiusAroundParent = 0.0f;
            singleExtent.MaxSweepRadius = childExtentValues[0];
            maxDistanceToEdge = childExtentValues[0];
            extent.InnerExtentRadius = maxDistanceToEdge;
        }
        else
        {
            foreach (float childExtent in childExtentValues)
            {
                maxDistanceToEdge = Mathf.Max(maxDistanceToEdge, childExtent + (separation * 0.5f));
            }

            extent.InnerExtentRadius = maxDistanceToEdge;
        }

        extent.PTypePlanetCount = hostContentCounts.ContainsKey(node.Id) ? hostContentCounts[node.Id] : 0;
        extent.FirstOrbitRadius = extent.InnerExtentRadius + extent.MaxPlanetRadius + PTypeBufferGap;
        if (extent.PTypePlanetCount > 0)
        {
            float outermostOrbit = CalculateNthOrbitRadius(extent.FirstOrbitRadius, extent.PTypePlanetCount - 1);
            extent.ExtentRadius = outermostOrbit + extent.MaxPlanetRadius;
        }
        else
        {
            extent.ExtentRadius = extent.InnerExtentRadius;
        }

        extent.MaxSweepRadius = extent.ExtentRadius;
        layout.NodeExtents[node.Id] = extent;
        return extent.ExtentRadius;
    }

    /// <summary>
    /// Recursively positions hierarchy nodes.
    /// </summary>
    private static void PositionHierarchyNode(
        HierarchyNode? node,
        Vector3 center,
        string? parentBarycenterId,
        SolarSystem system,
        SystemLayout layout)
    {
        if (node == null)
        {
            return;
        }

        layout.HostPositions[node.Id] = center;
        if (layout.NodeExtents.ContainsKey(node.Id))
        {
            layout.NodeExtents[node.Id].CenterPosition = center;
        }

        if (node.IsStar())
        {
            CelestialBody? star = system.GetBody(node.StarId);
            if (star == null)
            {
                return;
            }

            float starDisplayRadius = layout.NodeExtents.ContainsKey(node.Id) ? layout.NodeExtents[node.Id].StarDisplayRadius : StarBaseRadius;
            BodyLayout starLayout = new(star.Id)
            {
                Position = center,
                DisplayRadius = starDisplayRadius,
                OrbitRadius = 0.0f,
                IsOrbiting = false,
            };
            layout.BodyLayouts[star.Id] = starLayout;
            layout.StarToNode[star.Id] = node.Id;

            if (!string.IsNullOrEmpty(parentBarycenterId))
            {
                Vector3 parentCenter = layout.GetHostPosition(parentBarycenterId);
                float orbitRadius = center.DistanceTo(parentCenter);
                if (orbitRadius > 0.01f)
                {
                    float orbitAngle = Mathf.Atan2(center.Z - parentCenter.Z, center.X - parentCenter.X);
                    float orbitPeriod = CalculateOrbitalPeriod(orbitRadius) * 2.0f;

                    BodyLayout starOrbit = new(star.Id)
                    {
                        OrbitRadius = orbitRadius,
                        OrbitCenter = parentCenter,
                        OrbitParentId = parentBarycenterId,
                        OrbitalAngle = orbitAngle,
                        OrbitalPeriod = orbitPeriod,
                        IsOrbiting = true,
                        DisplayRadius = starDisplayRadius,
                        Position = center,
                    };
                    layout.StarOrbits[star.Id] = starOrbit;

                    starLayout.IsOrbiting = true;
                    starLayout.OrbitRadius = orbitRadius;
                    starLayout.OrbitCenter = parentCenter;
                    starLayout.OrbitParentId = parentBarycenterId;
                    starLayout.OrbitalAngle = orbitAngle;
                    starLayout.OrbitalPeriod = orbitPeriod;
                }
            }

            return;
        }

        NodeExtent? extent = layout.GetNodeExtent(node.Id);
        if (node.Children.Count >= 2)
        {
            HierarchyNode childA = node.Children[0];
            HierarchyNode childB = node.Children[1];
            float separation = extent != null ? extent.BinarySeparation : BinaryBufferGap * 2.0f;

            float massA = GetNodeMass(childA, system);
            float massB = GetNodeMass(childB, system);
            float totalMass = massA + massB;

            float offsetA;
            float offsetB;
            if (totalMass > 0.0f)
            {
                offsetA = separation * massB / totalMass;
                offsetB = separation * massA / totalMass;
            }
            else
            {
                offsetA = separation * 0.5f;
                offsetB = separation * 0.5f;
            }

            Vector3 positionA = center + new Vector3(-offsetA, 0.0f, 0.0f);
            Vector3 positionB = center + new Vector3(offsetB, 0.0f, 0.0f);
            PositionHierarchyNode(childA, positionA, node.Id, system, layout);
            PositionHierarchyNode(childB, positionB, node.Id, system, layout);
        }
        else if (node.Children.Count == 1)
        {
            PositionHierarchyNode(node.Children[0], center, node.Id, system, layout);
        }

        if (node.Children.Count > 2)
        {
            for (int index = 2; index < node.Children.Count; index++)
            {
                float angle = (Mathf.Pi * 0.5f) + ((index - 2) * Mathf.Pi / (node.Children.Count - 1));
                float offset = (extent != null ? extent.InnerExtentRadius : 10.0f) * 0.8f;
                Vector3 childPosition = center + new Vector3(Mathf.Cos(angle) * offset, 0.0f, Mathf.Sin(angle) * offset);
                PositionHierarchyNode(node.Children[index], childPosition, node.Id, system, layout);
            }
        }
    }

    /// <summary>
    /// Returns the total mass of a hierarchy subtree.
    /// </summary>
    private static float GetNodeMass(HierarchyNode node, SolarSystem system)
    {
        if (node.IsStar())
        {
            CelestialBody? star = system.GetBody(node.StarId);
            return star != null ? (float)star.Physical.MassKg : (float)Units.SolarMassKg;
        }

        float total = 0.0f;
        foreach (HierarchyNode child in node.Children)
        {
            total += GetNodeMass(child, system);
        }

        return total;
    }

    /// <summary>
    /// Positions planets and belts around each host using shared orbit slots.
    /// </summary>
    private static void PositionOrbitContent(
        SolarSystem system,
        global::System.Collections.Generic.Dictionary<string, Godot.Collections.Array<CelestialBody>> planetsByHost,
        global::System.Collections.Generic.Dictionary<string, Godot.Collections.Array<AsteroidBelt>> beltsByHost,
        SystemLayout layout)
    {
        global::System.Collections.Generic.Dictionary<string, bool> allHosts = new();
        foreach (string hostId in planetsByHost.Keys)
        {
            allHosts[hostId] = true;
        }

        foreach (string hostId in beltsByHost.Keys)
        {
            allHosts[hostId] = true;
        }

        foreach (string hostId in allHosts.Keys)
        {
            Vector3 hostCenter = layout.GetHostPosition(hostId);
            NodeExtent? hostExtent = layout.GetNodeExtent(hostId);
            if (hostExtent == null)
            {
                GD.PushWarning($"No extent found for host: {hostId}");
                continue;
            }

            float firstOrbitRadius = hostExtent.FirstOrbitRadius;
            List<OrbitEntry> entries = new();

            if (planetsByHost.ContainsKey(hostId))
            {
                foreach (CelestialBody planet in planetsByHost[hostId])
                {
                    double distanceM = planet.HasOrbital() ? planet.Orbital!.SemiMajorAxisM : 0.0;
                    entries.Add(new OrbitEntry(OrbitEntryType.Planet, distanceM, planet, null));
                }
            }

            if (beltsByHost.ContainsKey(hostId))
            {
                foreach (AsteroidBelt belt in beltsByHost[hostId])
                {
                    entries.Add(new OrbitEntry(OrbitEntryType.Belt, belt.GetCenterM(), null, belt));
                }
            }

            entries.Sort(static (a, b) => a.DistanceM.CompareTo(b.DistanceM));
            for (int index = 0; index < entries.Count; index++)
            {
                float orbitRadius = CalculateNthOrbitRadius(firstOrbitRadius, index);
                OrbitEntry entry = entries[index];
                if (entry.Type == OrbitEntryType.Planet && entry.Planet != null)
                {
                    CelestialBody planet = entry.Planet;
                    BodyLayout planetLayout = new(planet.Id)
                    {
                        OrbitRadius = orbitRadius,
                        OrbitCenter = hostCenter,
                        OrbitParentId = hostId,
                        DisplayRadius = CalculatePlanetDisplayRadius(planet.Physical.RadiusM),
                    };

                    float angle = planet.HasOrbital()
                        ? Mathf.DegToRad((float)planet.Orbital!.MeanAnomalyDeg)
                        : (index * Mathf.Tau / Mathf.Max(entries.Count, 1));
                    planetLayout.OrbitalAngle = angle;
                    planetLayout.OrbitalPeriod = CalculateOrbitalPeriod(orbitRadius);
                    planetLayout.IsOrbiting = true;
                    planetLayout.UpdatePositionFromAngle();
                    layout.BodyLayouts[planet.Id] = planetLayout;
                }
                else if (entry.Type == OrbitEntryType.Belt && entry.Belt != null)
                {
                    AsteroidBelt belt = entry.Belt;
                    BeltLayout beltLayout = new(belt.Id)
                    {
                        HostId = hostId,
                        HostCenter = hostCenter,
                        CenterDisplayRadius = orbitRadius,
                        InnerDisplayRadius = Mathf.Max(0.1f, orbitRadius - BeltDisplayHalfWidth),
                        OuterDisplayRadius = orbitRadius + BeltDisplayHalfWidth,
                        CenterAu = (float)belt.GetCenterAu(),
                        InnerAu = (float)(belt.InnerRadiusM / Units.AuMeters),
                        OuterAu = (float)(belt.OuterRadiusM / Units.AuMeters),
                        MaxInclinationDeg = CalculateBeltMaxInclinationDeg(orbitRadius),
                    };
                    layout.BeltLayouts[belt.Id] = beltLayout;
                }
            }
        }
    }

    /// <summary>
    /// Calculates the total system extent for camera framing.
    /// </summary>
    private static void CalculateTotalExtent(SystemLayout layout)
    {
        float maxExtent = 0.0f;

        foreach (BodyLayout bodyLayout in layout.BodyLayouts.Values)
        {
            float distance = bodyLayout.Position.Length() + bodyLayout.DisplayRadius;
            maxExtent = Mathf.Max(maxExtent, distance);
            if (bodyLayout.OrbitRadius > 0.0f)
            {
                float orbitEdge = bodyLayout.OrbitCenter.Length() + bodyLayout.OrbitRadius + bodyLayout.DisplayRadius;
                maxExtent = Mathf.Max(maxExtent, orbitEdge);
            }
        }

        foreach (BodyLayout orbitLayout in layout.StarOrbits.Values)
        {
            if (orbitLayout.OrbitRadius <= 0.0f)
            {
                continue;
            }

            float orbitEdge = orbitLayout.OrbitCenter.Length() + orbitLayout.OrbitRadius + orbitLayout.DisplayRadius;
            maxExtent = Mathf.Max(maxExtent, orbitEdge);
        }

        foreach (BeltLayout beltLayout in layout.BeltLayouts.Values)
        {
            float beltEdge = beltLayout.HostCenter.Length() + beltLayout.OuterDisplayRadius;
            maxExtent = Mathf.Max(maxExtent, beltEdge);
        }

        foreach (NodeExtent extent in layout.NodeExtents.Values)
        {
            float sweep = extent.MaxSweepRadius > 0.0f ? extent.MaxSweepRadius : extent.ExtentRadius;
            float distance = extent.CenterPosition.Length() + sweep;
            maxExtent = Mathf.Max(maxExtent, distance);
        }

        layout.TotalExtent = Mathf.Max(maxExtent, 10.0f);
    }

    /// <summary>
    /// Orbit-entry categories used during slot placement.
    /// </summary>
    private enum OrbitEntryType
    {
        Planet,
        Belt,
    }

    /// <summary>
    /// Combined sortable orbit entry for a host.
    /// </summary>
    private readonly struct OrbitEntry
    {
        /// <summary>
        /// Entry type.
        /// </summary>
        public OrbitEntryType Type { get; }

        /// <summary>
        /// Physical orbit distance in meters.
        /// </summary>
        public double DistanceM { get; }

        /// <summary>
        /// Planet payload when the entry is a planet.
        /// </summary>
        public CelestialBody? Planet { get; }

        /// <summary>
        /// Belt payload when the entry is a belt.
        /// </summary>
        public AsteroidBelt? Belt { get; }

        /// <summary>
        /// Creates a new orbit entry.
        /// </summary>
        public OrbitEntry(OrbitEntryType type, double distanceM, CelestialBody? planet, AsteroidBelt? belt)
        {
            Type = type;
            DistanceM = distanceM;
            Planet = planet;
            Belt = belt;
        }
    }
}
