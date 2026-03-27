using Godot.Collections;
using StarGen.Domain.Generation.Traveller;

namespace StarGen.Domain.Jumplanes;

/// <summary>
/// Traveller-specific route calculator driven by authoritative mainworld UWP data.
/// </summary>
public partial class TravellerRouteCalculator : Godot.RefCounted
{
    /// <summary>
    /// Parsecs per supported jump number.
    /// </summary>
    public const double ParsecsPerJumpNumber = 2.0;

    /// <summary>
    /// Calculates Traveller routes for all systems in a region.
    /// </summary>
    public JumpLaneResult Calculate(JumpLaneRegion region)
    {
        JumpLaneResult result = new();
        foreach (JumpLaneSystem system in region.Systems)
        {
            result.RegisterSystem(system);
        }

        for (int indexA = 0; indexA < region.Systems.Count; indexA += 1)
        {
            JumpLaneSystem systemA = region.Systems[indexA];
            for (int indexB = indexA + 1; indexB < region.Systems.Count; indexB += 1)
            {
                JumpLaneSystem systemB = region.Systems[indexB];
                if (!ShouldCreateRoute(systemA, systemB))
                {
                    continue;
                }

                double distancePc = systemA.DistanceTo(systemB);
                JumpLaneConnection.ConnectionType connectionType;
                if (distancePc <= ParsecsPerJumpNumber)
                {
                    connectionType = JumpLaneConnection.ConnectionType.Green;
                }
                else
                {
                    connectionType = JumpLaneConnection.ConnectionType.Orange;
                }

                result.AddConnection(new JumpLaneConnection(systemA.Id, systemB.Id, connectionType, distancePc));
            }
        }

        foreach (JumpLaneSystem system in region.Systems)
        {
            if (result.GetConnectionsForSystem(system.Id).Count == 0)
            {
                result.AddOrphan(system.Id);
            }
        }

        return result;
    }

    private static bool ShouldCreateRoute(JumpLaneSystem systemA, JumpLaneSystem systemB)
    {
        TravellerSystemProfile? profileA = systemA.TravellerProfile;
        TravellerSystemProfile? profileB = systemB.TravellerProfile;
        if (profileA == null || profileB == null)
        {
            return false;
        }

        if (!profileA.RouteProfile.IsRouteEligible() || !profileB.RouteProfile.IsRouteEligible())
        {
            return false;
        }

        int supportedJumpNumber = System.Math.Min(profileA.RouteProfile.MaxJumpNumber, profileB.RouteProfile.MaxJumpNumber);
        if (supportedJumpNumber <= 0)
        {
            return false;
        }

        double maxDistancePc = ParsecsPerJumpNumber * supportedJumpNumber;
        if (systemA.DistanceTo(systemB) > maxDistancePc)
        {
            return false;
        }

        if (MatchesIndustrialTradeRule(profileA.TradeCodes, profileB.TradeCodes))
        {
            return true;
        }

        if (MatchesAgriculturalTradeRule(profileA.TradeCodes, profileB.TradeCodes))
        {
            return true;
        }

        return MatchesImportanceRule(profileA.RouteProfile, profileB.RouteProfile, systemA.DistanceTo(systemB));
    }

    private static bool MatchesIndustrialTradeRule(TravellerTradeCodeSet codesA, TravellerTradeCodeSet codesB)
    {
        if (codesA.ContainsAny("In", "Ht") && codesB.ContainsAny("As", "De", "Ic", "Ni"))
        {
            return true;
        }

        if (codesB.ContainsAny("In", "Ht") && codesA.ContainsAny("As", "De", "Ic", "Ni"))
        {
            return true;
        }

        return false;
    }

    private static bool MatchesAgriculturalTradeRule(TravellerTradeCodeSet codesA, TravellerTradeCodeSet codesB)
    {
        if (codesA.ContainsAny("Hi", "Ri") && codesB.ContainsAny("Ag", "Ga", "Wa"))
        {
            return true;
        }

        if (codesB.ContainsAny("Hi", "Ri") && codesA.ContainsAny("Ag", "Ga", "Wa"))
        {
            return true;
        }

        return false;
    }

    private static bool MatchesImportanceRule(
        TravellerRouteProfile profileA,
        TravellerRouteProfile profileB,
        double distancePc)
    {
        return profileA.Importance >= 2
            && profileB.Importance >= 2
            && distancePc <= ParsecsPerJumpNumber;
    }
}
