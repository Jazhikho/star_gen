using Godot;
using Godot.Collections;
using StarGen.Domain.Celestial;
using StarGen.Domain.Celestial.Components;
using StarGen.Domain.Generation.Archetypes;
using StarGen.Domain.Math;
using StarGen.Domain.Population;
using StarGen.Domain.Rng;
using StarGen.Domain.Systems;

namespace StarGen.Domain.Generation.Traveller;

/// <summary>
/// Applies Traveller mainworld takeover to a generated system while keeping non-mainworld bodies on the realistic path.
/// </summary>
public static class TravellerSystemGenerator
{
    /// <summary>
    /// Applies Traveller generation to the selected mainworld when Traveller mode is active.
    /// </summary>
    public static void ApplyTravellerMainworld(SolarSystem? system)
    {
        if (system == null)
        {
            return;
        }

        GenerationUseCaseSettings? settings = ResolveUseCaseSettings(system);
        if (settings == null || !settings.IsTravellerMode())
        {
            system.TravellerProfile = null;
            return;
        }

        TravellerMainworldSelector.SelectionResult selection = TravellerMainworldSelector.SelectForTravellerGeneration(system);
        if (!selection.HasCandidate() || selection.Body == null)
        {
            system.TravellerProfile = null;
            return;
        }

        CelestialBody mainworld = selection.Body;
        ClearExistingTravellerFlags(system);

        int baseSeed = ResolveSystemSeed(system);
        long travellerSeedLong = PopulationSeeding.GeneratePopulationSeed(mainworld.Id + ":traveller_mainworld", baseSeed);
        int travellerSeed = unchecked((int)(travellerSeedLong & 0x7FFF_FFFF));
        SeededRng rng = new(travellerSeed);

        CelestialBody? parentBody = ResolveParentBody(system, mainworld);
        ParentContext context = ResolveParentContext(system, mainworld, parentBody);
        OrbitZone.Zone orbitZone = ResolveOrbitZone(mainworld, context);

        TravellerWorldProfile worldProfile = TravellerWorldGenerator.GeneratePlanetProfile(
            requestedSizeCode: -1,
            requestedAtmosphereCode: -1,
            requestedHydrographicsCode: -1,
            requestedPopulationCode: -1,
            orbitZone,
            rng);
        TravellerTradeCodeSet tradeCodes = TravellerWorldGenerator.GenerateTradeCodes(worldProfile);
        string travelZone = TravellerWorldGenerator.DetermineTravelZone(worldProfile);
        TravellerRouteProfile routeProfile = TravellerWorldGenerator.BuildRouteProfile(worldProfile, tradeCodes);

        TravellerWorldGenerator.ApplyToBody(mainworld, worldProfile, rng);
        PlanetPopulationData populationData = BuildTravellerPopulationData(
            mainworld,
            context,
            parentBody,
            worldProfile,
            tradeCodes,
            travelZone,
            travellerSeed);
        mainworld.PopulationData = populationData;
        CopyPopulationStateToBody(mainworld, populationData);
        UpdateBodyTravellerMetadata(mainworld, worldProfile, tradeCodes, travelZone);

        TravellerSystemProfile systemProfile = new TravellerSystemProfile
        {
            MainworldBodyId = mainworld.Id,
            SelectionReason = selection.Reason,
            WorldProfile = worldProfile,
            TradeCodes = tradeCodes,
            TravelZone = travelZone,
            RouteProfile = routeProfile,
        };
        if (string.IsNullOrWhiteSpace(mainworld.Name))
        {
            systemProfile.MainworldName = mainworld.Id;
        }
        else
        {
            systemProfile.MainworldName = mainworld.Name;
        }

        system.TravellerProfile = systemProfile;
    }

    private static PlanetPopulationData BuildTravellerPopulationData(
        CelestialBody body,
        ParentContext context,
        CelestialBody? parentBody,
        TravellerWorldProfile worldProfile,
        TravellerTradeCodeSet tradeCodes,
        string travelZone,
        int generationSeed)
    {
        PlanetPopulationData data = PopulationGenerator.BuildProfileOnlyData(body, context, generationSeed, parentBody);
        data.NativePopulations.Clear();
        data.Colonies.Clear();
        data.BodyId = body.Id;
        data.GenerationSeed = generationSeed;

        if (worldProfile.PopulationCode <= 0)
        {
            return data;
        }

        Colony colony = new Colony();
        colony.Id = "traveller_mainworld";
        if (string.IsNullOrWhiteSpace(body.Name))
        {
            colony.Name = body.Id;
        }
        else
        {
            colony.Name = body.Name;
        }
        colony.BodyId = body.Id;
        colony.FoundingCivilizationId = "traveller";
        colony.FoundingCivilizationName = "Traveller Mainworld";
        colony.Population = TravellerWorldGenerator.EstimatePopulationCount(worldProfile.PopulationCode);
        colony.PeakPopulation = colony.Population;
        colony.PeakPopulationYear = 0;
        colony.TechLevel = TravellerWorldGenerator.MapTechLevelCodeToLevel(worldProfile.TechLevelCode);
        colony.Government = TravellerWorldGenerator.BuildGovernmentFromCode(worldProfile.GovernmentCode);
        colony.IsActive = true;
        colony.IsIndependent = true;
        colony.TerritorialControl = 0.85;
        colony.PrimaryIndustry = DerivePrimaryIndustry(tradeCodes);
        if (tradeCodes.ContainsAny("Ag", "Ga", "Ri", "In"))
        {
            colony.SelfSufficiency = 0.80;
        }
        else
        {
            colony.SelfSufficiency = 0.50;
        }
        colony.Metadata["traveller_starport_code"] = worldProfile.StarportCode;
        colony.Metadata["traveller_law_code"] = worldProfile.LawCode;
        colony.Metadata["traveller_trade_codes"] = tradeCodes.ToDisplayString();
        if (!string.IsNullOrEmpty(travelZone))
        {
            colony.Metadata["traveller_travel_zone"] = travelZone;
        }

        data.Colonies.Add(colony);
        data.Population = colony.Population;
        data.IsActive = true;
        data.IsIndependent = true;
        return data;
    }

    private static string DerivePrimaryIndustry(TravellerTradeCodeSet tradeCodes)
    {
        if (tradeCodes.ContainsAny("In", "Ht"))
        {
            return "industry";
        }

        if (tradeCodes.ContainsAny("Ag", "Ga", "Wa"))
        {
            return "agriculture";
        }

        if (tradeCodes.ContainsAny("As", "De", "Ic"))
        {
            return "extraction";
        }

        if (tradeCodes.Contains("Ri"))
        {
            return "commerce";
        }

        return "settlement";
    }

    private static void CopyPopulationStateToBody(CelestialBody body, PlanetPopulationData populationData)
    {
        body.EnvironmentProfile = populationData.EnvironmentProfile;
        body.Ecology = populationData.EcologyState;
        body.SpeciesEvolution = populationData.SpeciesEvolution;
        body.Sentience = populationData.SentienceAssessment;
        body.Disease = populationData.DiseaseState;
    }

    private static void UpdateBodyTravellerMetadata(
        CelestialBody body,
        TravellerWorldProfile worldProfile,
        TravellerTradeCodeSet tradeCodes,
        string travelZone)
    {
        if (body.Provenance == null)
        {
            body.Provenance = Provenance.CreateCurrent(0, new Dictionary());
        }

        body.Provenance.SpecSnapshot["traveller_world_profile"] = worldProfile.ToDictionary();
        body.Provenance.SpecSnapshot["traveller_trade_codes"] = tradeCodes.ToDictionary();
        body.Provenance.SpecSnapshot["traveller_travel_zone"] = travelZone;
        body.Provenance.SpecSnapshot["traveller_is_mainworld"] = true;
    }

    private static void ClearExistingTravellerFlags(SolarSystem system)
    {
        foreach (CelestialBody body in system.Bodies.Values)
        {
            if (body.Provenance == null)
            {
                continue;
            }

            if (body.Provenance.SpecSnapshot.ContainsKey("traveller_is_mainworld"))
            {
                body.Provenance.SpecSnapshot.Remove("traveller_is_mainworld");
            }

            if (body.Provenance.SpecSnapshot.ContainsKey("traveller_world_profile"))
            {
                body.Provenance.SpecSnapshot.Remove("traveller_world_profile");
            }

            if (body.Provenance.SpecSnapshot.ContainsKey("traveller_trade_codes"))
            {
                body.Provenance.SpecSnapshot.Remove("traveller_trade_codes");
            }

            if (body.Provenance.SpecSnapshot.ContainsKey("traveller_travel_zone"))
            {
                body.Provenance.SpecSnapshot.Remove("traveller_travel_zone");
            }
        }
    }

    private static CelestialBody? ResolveParentBody(SolarSystem system, CelestialBody body)
    {
        if (!body.HasOrbital() || body.Orbital == null || string.IsNullOrWhiteSpace(body.Orbital.ParentId))
        {
            return null;
        }

        return system.GetBody(body.Orbital.ParentId);
    }

    private static ParentContext ResolveParentContext(SolarSystem system, CelestialBody body, CelestialBody? parentBody)
    {
        if (body.Provenance != null
            && body.Provenance.SpecSnapshot.ContainsKey("context")
            && body.Provenance.SpecSnapshot["context"].VariantType == Variant.Type.Dictionary)
        {
            return ParentContext.FromDictionary((Dictionary)body.Provenance.SpecSnapshot["context"]);
        }

        Array<CelestialBody> stars = system.GetStars();
        if (stars.Count == 0)
        {
            return ParentContext.SunLike();
        }

        CelestialBody primaryStar = stars[0];
        double stellarMassKg = primaryStar.Physical.MassKg;
        double stellarLuminosityWatts = StellarProps.SolarLuminosityWatts;
        double stellarTemperatureK = 5778.0;
        double stellarAgeYears = 4.6e9;
        double orbitalDistanceM = Units.AuMeters;

        if (primaryStar.Stellar != null)
        {
            stellarLuminosityWatts = primaryStar.Stellar.LuminosityWatts;
            stellarTemperatureK = primaryStar.Stellar.EffectiveTemperatureK;
            stellarAgeYears = primaryStar.Stellar.AgeYears;
        }

        if (body.HasOrbital() && body.Orbital != null)
        {
            orbitalDistanceM = body.Orbital.SemiMajorAxisM;
        }

        if (body.Type == CelestialType.Type.Moon && parentBody != null)
        {
            double planetOrbitalDistanceM = orbitalDistanceM;
            if (parentBody.HasOrbital() && parentBody.Orbital != null)
            {
                planetOrbitalDistanceM = parentBody.Orbital.SemiMajorAxisM;
            }

            return ParentContext.ForMoon(
                stellarMassKg,
                stellarLuminosityWatts,
                stellarTemperatureK,
                stellarAgeYears,
                planetOrbitalDistanceM,
                parentBody.Physical.MassKg,
                parentBody.Physical.RadiusM,
                orbitalDistanceM);
        }

        return ParentContext.ForPlanet(
            stellarMassKg,
            stellarLuminosityWatts,
            stellarTemperatureK,
            stellarAgeYears,
            orbitalDistanceM);
    }

    private static OrbitZone.Zone ResolveOrbitZone(CelestialBody body, ParentContext context)
    {
        if (body.Provenance != null
            && body.Provenance.SpecSnapshot.ContainsKey("orbit_zone")
            && body.Provenance.SpecSnapshot["orbit_zone"].VariantType == Variant.Type.Int)
        {
            return (OrbitZone.Zone)(int)body.Provenance.SpecSnapshot["orbit_zone"];
        }

        if (body.HasOrbital() && body.Orbital != null)
        {
            return OrbitZone.FromOrbitalDistance(body.Orbital.SemiMajorAxisM, context.StellarLuminosityWatts);
        }

        return OrbitZone.Zone.Temperate;
    }

    private static int ResolveSystemSeed(SolarSystem system)
    {
        if (system.Provenance != null && system.Provenance.GenerationSeed != 0)
        {
            return unchecked((int)system.Provenance.GenerationSeed);
        }

        if (system.Provenance != null
            && system.Provenance.SpecSnapshot.ContainsKey("generation_seed")
            && system.Provenance.SpecSnapshot["generation_seed"].VariantType == Variant.Type.Int)
        {
            return (int)system.Provenance.SpecSnapshot["generation_seed"];
        }

        return 0;
    }

    private static GenerationUseCaseSettings? ResolveUseCaseSettings(SolarSystem system)
    {
        if (system.Provenance == null || system.Provenance.SpecSnapshot.Count == 0)
        {
            return null;
        }

        if (!system.Provenance.SpecSnapshot.ContainsKey("use_case_settings"))
        {
            return null;
        }

        Variant settingsVariant = system.Provenance.SpecSnapshot["use_case_settings"];
        if (settingsVariant.VariantType != Variant.Type.Dictionary)
        {
            return null;
        }

        return GenerationUseCaseSettings.FromDictionary((Dictionary)settingsVariant);
    }
}
