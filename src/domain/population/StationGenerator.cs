using Godot.Collections;
using StarGen.Domain.Rng;

namespace StarGen.Domain.Population;

/// <summary>
/// Generates stations and outposts for a system from placement rules.
/// </summary>
public static class StationGenerator
{
    private const int LargeStationIdOffset = 100;

    private static readonly Dictionary PopRangeU = BuildRange(50, 5000);
    private static readonly Dictionary PopRangeO = BuildRange(100, 10000);
    private static readonly Dictionary PopRangeB = BuildRange(10000, 100000);
    private static readonly Dictionary PopRangeA = BuildRange(100000, 1000000);
    private static readonly Dictionary PopRangeS = BuildRange(1000000, 10000000);

    private static readonly Dictionary NamePrefixes = new()
    {
        [(int)StationPurpose.Purpose.Utility] = new Array<string> { "Waypoint", "Rest Stop", "Junction", "Crossroads" },
        [(int)StationPurpose.Purpose.Trade] = new Array<string> { "Trade Hub", "Market", "Exchange", "Commerce" },
        [(int)StationPurpose.Purpose.Military] = new Array<string> { "Outpost", "Bastion", "Sentinel", "Watchtower" },
        [(int)StationPurpose.Purpose.Science] = new Array<string> { "Observatory", "Research", "Survey", "Lab" },
        [(int)StationPurpose.Purpose.Mining] = new Array<string> { "Excavation", "Extraction", "Drill", "Mine" },
        [(int)StationPurpose.Purpose.Residential] = new Array<string> { "Habitat", "Colony", "Settlement", "Haven" },
        [(int)StationPurpose.Purpose.Administrative] = new Array<string> { "Central", "Nexus", "Hub", "Authority" },
        [(int)StationPurpose.Purpose.Industrial] = new Array<string> { "Foundry", "Factory", "Works", "Forge" },
        [(int)StationPurpose.Purpose.Medical] = new Array<string> { "Medical", "Hospital", "Clinic", "Care" },
        [(int)StationPurpose.Purpose.Communications] = new Array<string> { "Relay", "Signal", "Beacon", "Comm" },
    };

    private static readonly string[] GreekLetters =
    {
        "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta",
        "Iota", "Kappa", "Lambda", "Mu", "Nu", "Xi", "Omicron", "Pi",
    };

    /// <summary>
    /// Generates stations for a system with an explicit RNG.
    /// </summary>
    public static StationGenerationResult Generate(
        StationSystemContext systemContext,
        StationSpec? spec,
        SeededRng rng)
    {
        StationSpec resolvedSpec = spec ?? StationSpec.Standard();
        StationGenerationResult result = new()
        {
            GenerationSeed = (int)rng.GetInitialSeed(),
        };

        if (!resolvedSpec.IsValid())
        {
            AppendStrings(result.Warnings, resolvedSpec.Validate());
            return result;
        }

        if (!resolvedSpec.GenerateStations)
        {
            return result;
        }

        StationPlacementRecommendation recommendation = resolvedSpec.ForceContext.HasValue
            ? CreateForcedRecommendation(resolvedSpec.ForceContext.Value, systemContext)
            : StationPlacementRules.EvaluateSystem(systemContext);
        result.Recommendation = recommendation;

        if (!recommendation.ShouldHaveStations && resolvedSpec.MinStations == 0)
        {
            return result;
        }

        int utilityCount = CalculateCount(recommendation.UtilityStationCount, resolvedSpec, rng);
        int outpostCount = CalculateCount(recommendation.OutpostCount, resolvedSpec, rng);
        int largeCount = CalculateCount(recommendation.LargeStationCount, resolvedSpec, rng);
        ApplyCountLimits(recommendation, resolvedSpec, ref utilityCount, ref outpostCount, ref largeCount);

        for (int index = 0; index < utilityCount; index += 1)
        {
            result.Outposts.Add(GenerateOutpost(
                systemContext,
                resolvedSpec,
                recommendation,
                StationClass.Class.U,
                index,
                rng));
        }

        for (int index = 0; index < outpostCount; index += 1)
        {
            result.Outposts.Add(GenerateOutpost(
                systemContext,
                resolvedSpec,
                recommendation,
                StationClass.Class.O,
                index + utilityCount,
                rng));
        }

        for (int index = 0; index < largeCount; index += 1)
        {
            result.Stations.Add(GenerateStation(systemContext, resolvedSpec, recommendation, index, rng));
        }

        return result;
    }

    /// <summary>
    /// Generates stations with a default spec.
    /// </summary>
    public static StationGenerationResult Generate(StationSystemContext systemContext, SeededRng rng)
    {
        return Generate(systemContext, null, rng);
    }

    /// <summary>
    /// Builds a synthetic placement recommendation when a context is forced.
    /// </summary>
    private static StationPlacementRecommendation CreateForcedRecommendation(
        StationPlacementContext.Context context,
        StationSystemContext systemContext)
    {
        StationPlacementRecommendation recommendation = new()
        {
            Context = context,
            ShouldHaveStations = true,
        };
        recommendation.Reasoning.Add("Forced context: " + StationPlacementContext.ToStringName(context));

        switch (context)
        {
            case StationPlacementContext.Context.BridgeSystem:
                recommendation.UtilityStationCount = 1;
                recommendation.AllowDeepSpace = true;
                recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Utility);
                recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Trade);
                break;
            case StationPlacementContext.Context.ColonyWorld:
                recommendation.LargeStationCount = System.Math.Max(1, systemContext.ColonyWorldCount);
                AppendStrings(recommendation.OrbitalCandidates, systemContext.ColonyPlanetIds);
                recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Trade);
                recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Residential);
                break;
            case StationPlacementContext.Context.NativeWorld:
                recommendation.LargeStationCount = System.Math.Max(1, systemContext.NativeWorldCount);
                AppendStrings(recommendation.OrbitalCandidates, systemContext.NativePlanetIds);
                recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Trade);
                recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Administrative);
                break;
            case StationPlacementContext.Context.ResourceSystem:
                recommendation.OutpostCount = 2;
                recommendation.AllowBeltStations = systemContext.AsteroidBeltCount > 0;
                recommendation.AllowDeepSpace = true;
                AppendStrings(recommendation.OrbitalCandidates, systemContext.ResourceBodyIds);
                recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Mining);
                recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Industrial);
                break;
            case StationPlacementContext.Context.Strategic:
                recommendation.OutpostCount = 1;
                recommendation.AllowDeepSpace = true;
                recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Military);
                break;
            case StationPlacementContext.Context.Scientific:
                recommendation.OutpostCount = 1;
                recommendation.AllowDeepSpace = true;
                AppendStrings(recommendation.OrbitalCandidates, systemContext.NativePlanetIds);
                recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Science);
                break;
            default:
                recommendation.OutpostCount = 1;
                recommendation.AllowDeepSpace = true;
                recommendation.RecommendedPurposes.Add(StationPurpose.Purpose.Utility);
                break;
        }

        return recommendation;
    }

    /// <summary>
    /// Applies population-density scaling and variance to a recommended count.
    /// </summary>
    private static int CalculateCount(int baseCount, StationSpec spec, SeededRng rng)
    {
        if (baseCount == 0)
        {
            return 0;
        }

        double modified = baseCount * spec.PopulationDensity;
        double variance = modified * 0.25;
        modified += rng.RandfRange((float)-variance, (float)variance);
        return System.Math.Max(0, (int)System.Math.Round(modified));
    }

    /// <summary>
    /// Enforces min/max count limits and class-allowance filters.
    /// </summary>
    private static void ApplyCountLimits(
        StationPlacementRecommendation recommendation,
        StationSpec spec,
        ref int utilityCount,
        ref int outpostCount,
        ref int largeCount)
    {
        int totalCount = utilityCount + outpostCount + largeCount;
        if (spec.MaxStations > 0 && totalCount > spec.MaxStations)
        {
            double scale = (double)spec.MaxStations / totalCount;
            utilityCount = (int)(utilityCount * scale);
            outpostCount = (int)(outpostCount * scale);
            largeCount = (int)(largeCount * scale);
            if (totalCount > 0 && utilityCount + outpostCount + largeCount == 0)
            {
                utilityCount = 1;
            }
        }

        if (spec.MinStations > 0)
        {
            totalCount = utilityCount + outpostCount + largeCount;
            if (totalCount < spec.MinStations)
            {
                int deficit = spec.MinStations - totalCount;
                if (recommendation.Context == StationPlacementContext.Context.BridgeSystem)
                {
                    utilityCount += deficit;
                }
                else if (spec.AllowLargeStations)
                {
                    largeCount += deficit;
                }
                else
                {
                    outpostCount += deficit;
                }
            }
        }

        if (!spec.AllowUtility)
        {
            outpostCount += utilityCount;
            utilityCount = 0;
        }

        if (!spec.AllowOutposts && !spec.AllowUtility)
        {
            largeCount += outpostCount;
            outpostCount = 0;
        }

        if (!spec.AllowLargeStations)
        {
            outpostCount += largeCount;
            largeCount = 0;
        }
    }

    /// <summary>
    /// Generates a single outpost payload.
    /// </summary>
    private static Outpost GenerateOutpost(
        StationSystemContext systemContext,
        StationSpec spec,
        StationPlacementRecommendation recommendation,
        StationClass.Class stationClass,
        int index,
        SeededRng rng)
    {
        Outpost outpost = new()
        {
            Id = $"{spec.IdPrefix}_{systemContext.SystemId}_{index:000}",
            StationClass = stationClass,
            PrimaryPurpose = SelectPurpose(recommendation, spec, stationClass, rng),
            SystemId = systemContext.SystemId,
            StationType = SelectStationType(recommendation, spec, rng),
            PlacementContext = recommendation.Context,
        };
        outpost.Name = GenerateName(outpost.PrimaryPurpose, index, rng);

        if (outpost.StationType == StationType.Type.Orbital)
        {
            outpost.OrbitingBodyId = SelectOrbitalBody(recommendation, rng);
        }

        outpost.Authority = SelectAuthority(outpost.PrimaryPurpose, rng);
        if (OutpostAuthority.HasParentOrganization(outpost.Authority))
        {
            outpost.ParentOrganizationId = $"org_{systemContext.SystemId}_{rng.Randi() % 1000u:000}";
            outpost.ParentOrganizationName = GenerateOrgName(outpost.Authority, rng);
        }

        Dictionary popRange = stationClass == StationClass.Class.U ? PopRangeU : PopRangeO;
        int basePopulation = rng.RandiRange((int)popRange["min"], (int)popRange["max"]);
        outpost.Population = (int)(basePopulation * spec.PopulationDensity);
        outpost.Population = System.Math.Clamp(outpost.Population, (int)popRange["min"], Outpost.MaxPopulation);
        outpost.EstablishedYear = rng.RandiRange(spec.MinEstablishedYear, spec.MaxEstablishedYear);
        outpost.Services = SelectServices(outpost.PrimaryPurpose, stationClass, rng);
        outpost.UpdateCommanderTitle();

        if (rng.Randf() < spec.DecommissionChance)
        {
            int decommissionYear = rng.RandiRange(outpost.EstablishedYear + 10, spec.MaxEstablishedYear);
            outpost.RecordDecommissioning(decommissionYear, GenerateDecommissionReason(rng));
        }

        return outpost;
    }

    /// <summary>
    /// Generates a single larger station payload.
    /// </summary>
    private static SpaceStation GenerateStation(
        StationSystemContext systemContext,
        StationSpec spec,
        StationPlacementRecommendation recommendation,
        int index,
        SeededRng rng)
    {
        SpaceStation station = new()
        {
            Id = $"{spec.IdPrefix}_{systemContext.SystemId}_{index + LargeStationIdOffset:000}",
            PrimaryPurpose = SelectPurpose(recommendation, spec, StationClass.Class.B, rng),
            SystemId = systemContext.SystemId,
            PlacementContext = recommendation.Context,
            FoundingCivilizationId = spec.FoundingCivilizationId,
            FoundingCivilizationName = spec.FoundingCivilizationName,
        };
        station.Name = GenerateName(station.PrimaryPurpose, index, rng);

        if (recommendation.OrbitalCandidates.Count > 0)
        {
            station.StationType = StationType.Type.Orbital;
            station.OrbitingBodyId = SelectOrbitalBody(recommendation, rng);
        }
        else if (recommendation.AllowDeepSpace)
        {
            station.StationType = StationType.Type.DeepSpace;
        }
        else
        {
            station.StationType = StationType.Type.Orbital;
            if (systemContext.PlanetIds.Count > 0)
            {
                station.OrbitingBodyId = systemContext.PlanetIds[rng.RandiRange(0, systemContext.PlanetIds.Count - 1)];
            }
        }

        StationClass.Class targetClass = StationPlacementRules.RecommendStationClass(
            recommendation.Context,
            IsLargePopulationContext(systemContext));
        Dictionary popRange = GetPopRange(targetClass);
        int basePopulation = rng.RandiRange((int)popRange["min"], (int)popRange["max"]);
        station.Population = (int)(basePopulation * spec.PopulationDensity);
        station.UpdateClassFromPopulation();
        station.EstablishedYear = rng.RandiRange(spec.MinEstablishedYear, spec.MaxEstablishedYear);
        station.Services = SelectServices(station.PrimaryPurpose, station.StationClass, rng);

        if (station.UsesOutpostGovernment())
        {
            station.OutpostAuthority = SelectAuthority(station.PrimaryPurpose, rng);
            if (OutpostAuthority.HasParentOrganization(station.OutpostAuthority))
            {
                station.ParentOrganizationId = $"org_{systemContext.SystemId}_{rng.Randi() % 1000u:000}";
                station.ParentOrganizationName = GenerateOrgName(station.OutpostAuthority, rng);
            }

            station.UpdateCommanderTitle();
        }
        else
        {
            station.Government ??= new Government();
            station.Government.Regime = SelectRegime(recommendation.Context, rng);
            station.Government.Legitimacy = rng.RandfRange(0.5f, 0.95f);
            station.History ??= new PopulationHistory();
            station.History.AddNewEvent(
                HistoryEvent.EventType.Founding,
                station.EstablishedYear,
                "Station Founded",
                $"{station.Name} established");
        }

        station.PeakPopulation = station.Population;
        station.PeakPopulationYear = spec.MaxEstablishedYear;
        if (rng.Randf() < spec.DecommissionChance * 0.5)
        {
            int decommissionYear = rng.RandiRange(station.EstablishedYear + 50, spec.MaxEstablishedYear);
            station.RecordDecommissioning(decommissionYear, GenerateDecommissionReason(rng));
        }

        return station;
    }

    /// <summary>
    /// Selects a station purpose within the spec constraints.
    /// </summary>
    private static StationPurpose.Purpose SelectPurpose(
        StationPlacementRecommendation recommendation,
        StationSpec spec,
        StationClass.Class stationClass,
        SeededRng rng)
    {
        Array<StationPurpose.Purpose> allowed = new();
        foreach (StationPurpose.Purpose purpose in recommendation.RecommendedPurposes)
        {
            if (spec.IsPurposeAllowed(purpose))
            {
                allowed.Add(purpose);
            }
        }

        if (allowed.Count == 0)
        {
            Array<StationPurpose.Purpose> defaults = stationClass switch
            {
                StationClass.Class.U => StationPurpose.TypicalUtilityPurposes(),
                StationClass.Class.O => StationPurpose.TypicalOutpostPurposes(),
                _ => StationPurpose.TypicalSettlementPurposes(),
            };

            foreach (StationPurpose.Purpose purpose in defaults)
            {
                if (spec.IsPurposeAllowed(purpose))
                {
                    allowed.Add(purpose);
                }
            }
        }

        if (allowed.Count == 0)
        {
            return StationPurpose.Purpose.Utility;
        }

        return allowed[rng.RandiRange(0, allowed.Count - 1)];
    }

    /// <summary>
    /// Selects a station location type from the current recommendation.
    /// </summary>
    private static StationType.Type SelectStationType(
        StationPlacementRecommendation recommendation,
        StationSpec spec,
        SeededRng rng)
    {
        Array<StationType.Type> options = new();
        if (recommendation.OrbitalCandidates.Count > 0)
        {
            options.Add(StationType.Type.Orbital);
        }

        if (recommendation.AllowDeepSpace && spec.AllowDeepSpace)
        {
            options.Add(StationType.Type.DeepSpace);
        }

        if (recommendation.AllowBeltStations && spec.AllowBeltStations)
        {
            options.Add(StationType.Type.AsteroidBelt);
        }

        if (options.Count == 0)
        {
            return StationType.Type.DeepSpace;
        }

        return options[rng.RandiRange(0, options.Count - 1)];
    }

    /// <summary>
    /// Selects an orbital target body from the available candidates.
    /// </summary>
    private static string SelectOrbitalBody(StationPlacementRecommendation recommendation, SeededRng rng)
    {
        if (recommendation.OrbitalCandidates.Count == 0)
        {
            return string.Empty;
        }

        return recommendation.OrbitalCandidates[rng.RandiRange(0, recommendation.OrbitalCandidates.Count - 1)];
    }

    /// <summary>
    /// Selects an authority type appropriate to a station purpose.
    /// </summary>
    private static OutpostAuthority.Type SelectAuthority(StationPurpose.Purpose purpose, SeededRng rng)
    {
        Array<OutpostAuthority.Type> options = purpose switch
        {
            StationPurpose.Purpose.Utility => new Array<OutpostAuthority.Type>
            {
                OutpostAuthority.Type.Corporate,
                OutpostAuthority.Type.Franchise,
                OutpostAuthority.Type.Independent,
            },
            StationPurpose.Purpose.Military => new Array<OutpostAuthority.Type>
            {
                OutpostAuthority.Type.Military,
                OutpostAuthority.Type.Government,
            },
            StationPurpose.Purpose.Science => new Array<OutpostAuthority.Type>
            {
                OutpostAuthority.Type.Government,
                OutpostAuthority.Type.Corporate,
                OutpostAuthority.Type.Cooperative,
            },
            StationPurpose.Purpose.Mining => new Array<OutpostAuthority.Type>
            {
                OutpostAuthority.Type.Corporate,
                OutpostAuthority.Type.Cooperative,
                OutpostAuthority.Type.Independent,
            },
            StationPurpose.Purpose.Trade => new Array<OutpostAuthority.Type>
            {
                OutpostAuthority.Type.Corporate,
                OutpostAuthority.Type.Franchise,
                OutpostAuthority.Type.Independent,
            },
            _ => OutpostAuthority.TypicalForOutpost(),
        };

        return options[rng.RandiRange(0, options.Count - 1)];
    }

    /// <summary>
    /// Selects a service bundle for a station.
    /// </summary>
    private static Array<StationService.Service> SelectServices(
        StationPurpose.Purpose purpose,
        StationClass.Class stationClass,
        SeededRng rng)
    {
        Array<StationService.Service> services = new()
        {
            StationService.Service.Communications,
        };

        switch (purpose)
        {
            case StationPurpose.Purpose.Utility:
                services.Add(StationService.Service.Refuel);
                services.Add(StationService.Service.Repair);
                services.Add(StationService.Service.Lodging);
                if (rng.Randf() > 0.5f)
                {
                    services.Add(StationService.Service.Trade);
                }

                break;
            case StationPurpose.Purpose.Trade:
                services.Add(StationService.Service.Trade);
                services.Add(StationService.Service.Storage);
                services.Add(StationService.Service.Customs);
                if (rng.Randf() > 0.3f)
                {
                    services.Add(StationService.Service.Refuel);
                }

                break;
            case StationPurpose.Purpose.Military:
                services.Add(StationService.Service.Security);
                services.Add(StationService.Service.Repair);
                if (rng.Randf() > 0.5f)
                {
                    services.Add(StationService.Service.Medical);
                }

                break;
            case StationPurpose.Purpose.Science:
                services.Add(StationService.Service.Lodging);
                if (rng.Randf() > 0.5f)
                {
                    services.Add(StationService.Service.Medical);
                }

                break;
            case StationPurpose.Purpose.Mining:
                services.Add(StationService.Service.Storage);
                services.Add(StationService.Service.Refuel);
                if (rng.Randf() > 0.5f)
                {
                    services.Add(StationService.Service.Repair);
                }

                break;
            case StationPurpose.Purpose.Residential:
                services.Add(StationService.Service.Lodging);
                services.Add(StationService.Service.Medical);
                services.Add(StationService.Service.Entertainment);
                break;
            case StationPurpose.Purpose.Industrial:
                services.Add(StationService.Service.Storage);
                services.Add(StationService.Service.Repair);
                break;
            default:
                services.Add(StationService.Service.Refuel);
                break;
        }

        if (StationClass.UsesColonyGovernment(stationClass))
        {
            AddServiceIfMissing(services, StationService.Service.Medical);
            if (rng.Randf() > 0.3f)
            {
                AddServiceIfMissing(services, StationService.Service.Banking);
            }

            if (rng.Randf() > 0.5f)
            {
                AddServiceIfMissing(services, StationService.Service.Entertainment);
            }

            if (stationClass >= StationClass.Class.A && rng.Randf() > 0.3f)
            {
                AddServiceIfMissing(services, StationService.Service.Shipyard);
            }
        }

        return services;
    }

    /// <summary>
    /// Selects a large-station regime based on placement context.
    /// </summary>
    private static GovernmentType.Regime SelectRegime(
        StationPlacementContext.Context context,
        SeededRng rng)
    {
        Array<GovernmentType.Regime> options = context switch
        {
            StationPlacementContext.Context.ColonyWorld => new Array<GovernmentType.Regime>
            {
                GovernmentType.Regime.Constitutional,
                GovernmentType.Regime.Corporate,
                GovernmentType.Regime.Oligarchic,
            },
            StationPlacementContext.Context.NativeWorld => new Array<GovernmentType.Regime>
            {
                GovernmentType.Regime.Constitutional,
                GovernmentType.Regime.EliteRepublic,
            },
            StationPlacementContext.Context.ResourceSystem => new Array<GovernmentType.Regime>
            {
                GovernmentType.Regime.Corporate,
                GovernmentType.Regime.Technocracy,
                GovernmentType.Regime.Oligarchic,
            },
            _ => new Array<GovernmentType.Regime>
            {
                GovernmentType.Regime.Constitutional,
                GovernmentType.Regime.Corporate,
                GovernmentType.Regime.Technocracy,
            },
        };

        return options[rng.RandiRange(0, options.Count - 1)];
    }

    /// <summary>
    /// Generates a station display name.
    /// </summary>
    private static string GenerateName(StationPurpose.Purpose purpose, int index, SeededRng rng)
    {
        Array<string> prefixes = NamePrefixes.ContainsKey((int)purpose)
            ? (Array<string>)NamePrefixes[(int)purpose]
            : new Array<string> { "Station" };
        string prefix = prefixes[rng.RandiRange(0, prefixes.Count - 1)];
        if (index < GreekLetters.Length && rng.Randf() > 0.3f)
        {
            return $"{prefix} {GreekLetters[index]}";
        }

        return $"{prefix} {index + 1}";
    }

    /// <summary>
    /// Generates a parent-organization display name.
    /// </summary>
    private static string GenerateOrgName(OutpostAuthority.Type authority, SeededRng rng)
    {
        string[] corpNames = { "Stellar", "Nova", "Cosmos", "Orbital", "Horizon", "Frontier" };
        string[] suffixes = { "Corp", "Industries", "Enterprises", "Holdings", "Group" };
        return authority switch
        {
            OutpostAuthority.Type.Corporate => $"{corpNames[rng.RandiRange(0, corpNames.Length - 1)]} {suffixes[rng.RandiRange(0, suffixes.Length - 1)]}",
            OutpostAuthority.Type.Military => $"Defense Command {rng.RandiRange(1, 100)}",
            OutpostAuthority.Type.Franchise => $"{corpNames[rng.RandiRange(0, corpNames.Length - 1)]} Services",
            OutpostAuthority.Type.Government => "Colonial Administration",
            OutpostAuthority.Type.Religious => "Order of the Stars",
            _ => "Independent Operators",
        };
    }

    /// <summary>
    /// Generates a decommission reason string.
    /// </summary>
    private static string GenerateDecommissionReason(SeededRng rng)
    {
        string[] reasons =
        {
            "Resource depletion",
            "Structural failure",
            "Economic downturn",
            "Relocated operations",
            "Political changes",
            "Natural disaster",
            "Abandonment",
            "Consolidation",
        };
        return reasons[rng.RandiRange(0, reasons.Length - 1)];
    }

    /// <summary>
    /// Returns the population range for a station class.
    /// </summary>
    private static Dictionary GetPopRange(StationClass.Class stationClass)
    {
        return stationClass switch
        {
            StationClass.Class.U => PopRangeU,
            StationClass.Class.O => PopRangeO,
            StationClass.Class.B => PopRangeB,
            StationClass.Class.A => PopRangeA,
            StationClass.Class.S => PopRangeS,
            _ => PopRangeO,
        };
    }

    /// <summary>
    /// Returns whether the placement context can sustain larger stations.
    /// </summary>
    private static bool IsLargePopulationContext(StationSystemContext context)
    {
        return context.ColonyWorldCount > 0 || context.HasSpacefaringNatives;
    }

    /// <summary>
    /// Adds a service only when it is not already present.
    /// </summary>
    private static void AddServiceIfMissing(Array<StationService.Service> services, StationService.Service service)
    {
        if (!services.Contains(service))
        {
            services.Add(service);
        }
    }

    /// <summary>
    /// Appends string values from one array to another.
    /// </summary>
    private static void AppendStrings(Array<string> destination, Array<string> source)
    {
        foreach (string value in source)
        {
            destination.Add(value);
        }
    }

    /// <summary>
    /// Builds a reusable population-range dictionary.
    /// </summary>
    private static Dictionary BuildRange(int minimum, int maximum)
    {
        return new Dictionary
        {
            ["min"] = minimum,
            ["max"] = maximum,
        };
    }
}
