using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Space station that can range from small outpost to city-sized habitat.
/// </summary>
public partial class SpaceStation : RefCounted
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public string Id = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    /// Station class.
    /// </summary>
    public StarGen.Domain.Population.StationClass.Class StationClass = StarGen.Domain.Population.StationClass.Class.O;

    /// <summary>
    /// Station type.
    /// </summary>
    public StarGen.Domain.Population.StationType.Type StationType = StarGen.Domain.Population.StationType.Type.Orbital;

    /// <summary>
    /// Primary purpose.
    /// </summary>
    public StationPurpose.Purpose PrimaryPurpose = StationPurpose.Purpose.Trade;

    /// <summary>
    /// Secondary purposes.
    /// </summary>
    public Array<StationPurpose.Purpose> SecondaryPurposes = new();

    /// <summary>
    /// Services offered.
    /// </summary>
    public Array<StationService.Service> Services = new();

    /// <summary>
    /// Placement context.
    /// </summary>
    public StationPlacementContext.Context PlacementContext = StationPlacementContext.Context.Other;

    /// <summary>
    /// Current population.
    /// </summary>
    public int Population;

    /// <summary>
    /// Peak population.
    /// </summary>
    public int PeakPopulation;

    /// <summary>
    /// Year of peak population.
    /// </summary>
    public int PeakPopulationYear;

    /// <summary>
    /// Establishment year.
    /// </summary>
    public int EstablishedYear;

    /// <summary>
    /// Orbiting body identifier.
    /// </summary>
    public string OrbitingBodyId = string.Empty;

    /// <summary>
    /// System identifier.
    /// </summary>
    public string SystemId = string.Empty;

    /// <summary>
    /// Whether the station is operational.
    /// </summary>
    public bool IsOperational = true;

    /// <summary>
    /// Decommissioned year.
    /// </summary>
    public int DecommissionedYear;

    /// <summary>
    /// Decommission reason.
    /// </summary>
    public string DecommissionedReason = string.Empty;

    /// <summary>
    /// Authority type for small stations.
    /// </summary>
    public StarGen.Domain.Population.OutpostAuthority.Type OutpostAuthority = StarGen.Domain.Population.OutpostAuthority.Type.Independent;

    /// <summary>
    /// Parent organization identifier.
    /// </summary>
    public string ParentOrganizationId = string.Empty;

    /// <summary>
    /// Parent organization display name.
    /// </summary>
    public string ParentOrganizationName = string.Empty;

    /// <summary>
    /// Commander title.
    /// </summary>
    public string CommanderTitle;

    /// <summary>
    /// Commander name.
    /// </summary>
    public string CommanderName = string.Empty;

    /// <summary>
    /// Government for larger stations.
    /// </summary>
    public Government? Government;

    /// <summary>
    /// Whether the station is independent.
    /// </summary>
    public bool IsIndependent;

    /// <summary>
    /// Independence year.
    /// </summary>
    public int IndependenceYear;

    /// <summary>
    /// Historical timeline.
    /// </summary>
    public PopulationHistory? History;

    /// <summary>
    /// Founding civilization identifier.
    /// </summary>
    public string FoundingCivilizationId = string.Empty;

    /// <summary>
    /// Founding civilization display name.
    /// </summary>
    public string FoundingCivilizationName = string.Empty;

    /// <summary>
    /// Extra metadata.
    /// </summary>
    public Dictionary Metadata = new();

    /// <summary>
    /// Creates a new station.
    /// </summary>
    public SpaceStation()
    {
        CommanderTitle = StarGen.Domain.Population.OutpostAuthority.TypicalCommanderTitle(OutpostAuthority);
    }

    /// <summary>
    /// Returns the age of the station.
    /// </summary>
    public int GetAge(int currentYear = 0)
    {
        return IsOperational ? currentYear - EstablishedYear : DecommissionedYear - EstablishedYear;
    }

    /// <summary>
    /// Updates the station class from the current population.
    /// </summary>
    public void UpdateClassFromPopulation()
    {
        bool isUtility = PrimaryPurpose == StationPurpose.Purpose.Utility;
        StationClass.Class newClass = StarGen.Domain.Population.StationClass.GetClassForPopulation(Population, isUtility);
        bool wasSmall = StarGen.Domain.Population.StationClass.UsesOutpostGovernment(StationClass);
        bool isSmall = StarGen.Domain.Population.StationClass.UsesOutpostGovernment(newClass);
        StationClass = newClass;

        if (wasSmall && !isSmall)
        {
            Government ??= new Government();
            History ??= new PopulationHistory();
        }
    }

    /// <summary>
    /// Returns whether the station uses outpost-style governance.
    /// </summary>
    public bool UsesOutpostGovernment()
    {
        return StarGen.Domain.Population.StationClass.UsesOutpostGovernment(StationClass);
    }

    /// <summary>
    /// Returns whether the station uses colony-style governance.
    /// </summary>
    public bool UsesColonyGovernment()
    {
        return StarGen.Domain.Population.StationClass.UsesColonyGovernment(StationClass);
    }

    /// <summary>
    /// Returns the current regime.
    /// </summary>
    public GovernmentType.Regime GetRegime()
    {
        return Government != null ? Government.Regime : GovernmentType.Regime.Constitutional;
    }

    /// <summary>
    /// Returns whether the station is politically stable.
    /// </summary>
    public bool IsPoliticallyStable()
    {
        return Government == null || Government.IsStable();
    }

    /// <summary>
    /// Returns whether the station is associated with a specific body.
    /// </summary>
    public bool IsBodyAssociated()
    {
        return StarGen.Domain.Population.StationType.IsBodyAssociated(StationType) && !string.IsNullOrEmpty(OrbitingBodyId);
    }

    /// <summary>
    /// Returns whether the station has a parent organization.
    /// </summary>
    public bool HasParentOrganization()
    {
        return StarGen.Domain.Population.OutpostAuthority.HasParentOrganization(OutpostAuthority)
            && !string.IsNullOrEmpty(ParentOrganizationId);
    }

    /// <summary>
    /// Returns whether a specific service is offered.
    /// </summary>
    public bool OffersService(StationService.Service service)
    {
        return Services.Contains(service);
    }

    /// <summary>
    /// Adds a service if it is not already present.
    /// </summary>
    public void AddService(StationService.Service service)
    {
        if (!Services.Contains(service))
        {
            Services.Add(service);
        }
    }

    /// <summary>
    /// Removes a service if present.
    /// </summary>
    public void RemoveService(StationService.Service service)
    {
        int index = Services.IndexOf(service);
        if (index >= 0)
        {
            Services.RemoveAt(index);
        }
    }

    /// <summary>
    /// Sets the station population and updates the class.
    /// </summary>
    public void SetPopulation(int newPopulation)
    {
        Population = System.Math.Max(0, newPopulation);
        UpdateClassFromPopulation();
    }

    /// <summary>
    /// Updates the peak population if needed.
    /// </summary>
    public void UpdatePeakPopulation(int currentYear)
    {
        if (Population > PeakPopulation)
        {
            PeakPopulation = Population;
            PeakPopulationYear = currentYear;
        }
    }

    /// <summary>
    /// Returns the current growth state.
    /// </summary>
    public string GetGrowthState()
    {
        if (!IsOperational)
        {
            return "abandoned";
        }

        if (PeakPopulation == 0)
        {
            return Population > 0 ? "growing" : "stable";
        }

        if (Population > PeakPopulation * 0.95)
        {
            return "growing";
        }

        if (Population > PeakPopulation * 0.5)
        {
            return "stable";
        }

        return "declining";
    }

    /// <summary>
    /// Records station decommissioning.
    /// </summary>
    public void RecordDecommissioning(int year, string reason)
    {
        IsOperational = false;
        DecommissionedYear = year;
        DecommissionedReason = reason;
    }

    /// <summary>
    /// Records station independence.
    /// </summary>
    public void RecordIndependence(int year)
    {
        IsIndependent = true;
        IndependenceYear = year;
    }

    /// <summary>
    /// Updates the commander title.
    /// </summary>
    public void UpdateCommanderTitle()
    {
        CommanderTitle = StarGen.Domain.Population.OutpostAuthority.TypicalCommanderTitle(OutpostAuthority);
    }

    /// <summary>
    /// Returns a summary of the station.
    /// </summary>
    public Dictionary GetSummary()
    {
        Dictionary summary = new()
        {
            ["id"] = Id,
            ["name"] = Name,
            ["class"] = StarGen.Domain.Population.StationClass.ToLetter(StationClass),
            ["class_name"] = StarGen.Domain.Population.StationClass.ToStringName(StationClass),
            ["type"] = StarGen.Domain.Population.StationType.ToStringName(StationType),
            ["purpose"] = StationPurpose.ToStringName(PrimaryPurpose),
            ["population"] = Population,
            ["is_operational"] = IsOperational,
            ["age"] = GetAge(),
            ["services_count"] = Services.Count,
            ["growth_state"] = GetGrowthState(),
        };

        if (UsesOutpostGovernment())
        {
            summary["authority"] = StarGen.Domain.Population.OutpostAuthority.ToStringName(OutpostAuthority);
        }
        else
        {
            summary["regime"] = GovernmentType.ToStringName(GetRegime());
            summary["is_independent"] = IsIndependent;
        }

        return summary;
    }

    /// <summary>
    /// Validates the station data and returns any errors.
    /// </summary>
    public Array<string> Validate()
    {
        Array<string> errors = new();
        if (string.IsNullOrEmpty(Id))
        {
            errors.Add("Station ID is required");
        }

        if (Population < 0)
        {
            errors.Add("Population cannot be negative");
        }

        if (StationType == StarGen.Domain.Population.StationType.Type.Orbital && string.IsNullOrEmpty(OrbitingBodyId))
        {
            errors.Add("Orbital station should specify orbiting_body_id");
        }

        StationClass.Class expectedClass = StarGen.Domain.Population.StationClass.GetClassForPopulation(
            Population,
            PrimaryPurpose == StationPurpose.Purpose.Utility);
        if ((StarGen.Domain.Population.StationClass.UsesColonyGovernment(StationClass)
                || StarGen.Domain.Population.StationClass.UsesColonyGovernment(expectedClass))
            && StationClass != expectedClass)
        {
            errors.Add(
                $"Station class {StarGen.Domain.Population.StationClass.ToLetter(StationClass)} does not match population {Population} (expected {StarGen.Domain.Population.StationClass.ToLetter(expectedClass)})");
        }

        if (UsesColonyGovernment() && Government == null)
        {
            errors.Add("B/A/S class station should have government");
        }

        return errors;
    }

    /// <summary>
    /// Returns whether the station data is valid.
    /// </summary>
    public bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Converts the station to a dictionary payload.
    /// </summary>
    public Dictionary ToDictionary()
    {
        Array<int> secondaryPurposes = new();
        foreach (StationPurpose.Purpose purpose in SecondaryPurposes)
        {
            secondaryPurposes.Add((int)purpose);
        }

        Array<int> services = new();
        foreach (StationService.Service service in Services)
        {
            services.Add((int)service);
        }

        Dictionary data = new()
        {
            ["id"] = Id,
            ["name"] = Name,
            ["station_class"] = (int)StationClass,
            ["station_type"] = (int)StationType,
            ["primary_purpose"] = (int)PrimaryPurpose,
            ["secondary_purposes"] = secondaryPurposes,
            ["services"] = services,
            ["placement_context"] = (int)PlacementContext,
            ["population"] = Population,
            ["peak_population"] = PeakPopulation,
            ["peak_population_year"] = PeakPopulationYear,
            ["established_year"] = EstablishedYear,
            ["orbiting_body_id"] = OrbitingBodyId,
            ["system_id"] = SystemId,
            ["is_operational"] = IsOperational,
            ["decommissioned_year"] = DecommissionedYear,
            ["decommissioned_reason"] = DecommissionedReason,
            ["outpost_authority"] = (int)OutpostAuthority,
            ["parent_organization_id"] = ParentOrganizationId,
            ["parent_organization_name"] = ParentOrganizationName,
            ["commander_title"] = CommanderTitle,
            ["commander_name"] = CommanderName,
            ["is_independent"] = IsIndependent,
            ["independence_year"] = IndependenceYear,
            ["founding_civilization_id"] = FoundingCivilizationId,
            ["founding_civilization_name"] = FoundingCivilizationName,
            ["metadata"] = CloneDictionary(Metadata),
        };

        if (Government != null)
        {
            data["government"] = Government.ToDictionary();
        }

        if (History != null)
        {
            data["history"] = History.ToDictionary();
        }

        return data;
    }

    /// <summary>
    /// Creates a station from a dictionary payload.
    /// </summary>
    public static SpaceStation FromDictionary(Dictionary data)
    {
        SpaceStation station = new();
        station.Id = GetString(data, "id", string.Empty);
        station.Name = GetString(data, "name", string.Empty);
        station.StationClass = (StarGen.Domain.Population.StationClass.Class)GetInt(data, "station_class", (int)StarGen.Domain.Population.StationClass.Class.O);
        station.StationType = (StarGen.Domain.Population.StationType.Type)GetInt(data, "station_type", (int)StarGen.Domain.Population.StationType.Type.Orbital);
        station.PrimaryPurpose = (StationPurpose.Purpose)GetInt(data, "primary_purpose", (int)StationPurpose.Purpose.Trade);
        station.SecondaryPurposes = GetPurposeArray(data, "secondary_purposes");
        station.Services = GetServiceArray(data, "services");
        station.PlacementContext = (StationPlacementContext.Context)GetInt(data, "placement_context", (int)StationPlacementContext.Context.Other);
        station.Population = System.Math.Max(0, GetInt(data, "population", 0));
        station.PeakPopulation = System.Math.Max(0, GetInt(data, "peak_population", 0));
        station.PeakPopulationYear = GetInt(data, "peak_population_year", 0);
        station.EstablishedYear = GetInt(data, "established_year", 0);
        station.OrbitingBodyId = GetString(data, "orbiting_body_id", string.Empty);
        station.SystemId = GetString(data, "system_id", string.Empty);
        station.IsOperational = GetBool(data, "is_operational", true);
        station.DecommissionedYear = GetInt(data, "decommissioned_year", 0);
        station.DecommissionedReason = GetString(data, "decommissioned_reason", string.Empty);
        station.OutpostAuthority = (StarGen.Domain.Population.OutpostAuthority.Type)GetInt(
            data,
            "outpost_authority",
            (int)StarGen.Domain.Population.OutpostAuthority.Type.Independent);
        station.ParentOrganizationId = GetString(data, "parent_organization_id", string.Empty);
        station.ParentOrganizationName = GetString(data, "parent_organization_name", string.Empty);
        station.CommanderTitle = GetString(data, "commander_title", string.Empty);
        station.CommanderName = GetString(data, "commander_name", string.Empty);
        station.IsIndependent = GetBool(data, "is_independent", false);
        station.IndependenceYear = GetInt(data, "independence_year", 0);
        station.FoundingCivilizationId = GetString(data, "founding_civilization_id", string.Empty);
        station.FoundingCivilizationName = GetString(data, "founding_civilization_name", string.Empty);
        station.Metadata = GetDictionary(data, "metadata");
        if (data.ContainsKey("government") && data["government"].VariantType == Variant.Type.Dictionary)
        {
            station.Government = Government.FromDictionary((Dictionary)data["government"]);
        }

        if (data.ContainsKey("history") && data["history"].VariantType == Variant.Type.Dictionary)
        {
            station.History = PopulationHistory.FromDictionary((Dictionary)data["history"]);
        }

        if (string.IsNullOrEmpty(station.CommanderTitle))
        {
            station.UpdateCommanderTitle();
        }

        return station;
    }

    /// <summary>
    /// Creates a basic orbital station.
    /// </summary>
    public static SpaceStation CreateOrbital(string stationId, string stationName, string system, string bodyId)
    {
        SpaceStation station = new();
        station.Id = stationId;
        station.Name = stationName;
        station.SystemId = system;
        station.OrbitingBodyId = bodyId;
        station.StationType = StarGen.Domain.Population.StationType.Type.Orbital;
        station.PlacementContext = StationPlacementContext.Context.ColonyWorld;
        station.PrimaryPurpose = StationPurpose.Purpose.Trade;
        station.Services = StationService.BasicUtilityServices();
        station.UpdateCommanderTitle();
        return station;
    }

    /// <summary>
    /// Creates a basic deep-space station.
    /// </summary>
    public static SpaceStation CreateDeepSpace(string stationId, string stationName, string system)
    {
        SpaceStation station = new();
        station.Id = stationId;
        station.Name = stationName;
        station.SystemId = system;
        station.StationType = StarGen.Domain.Population.StationType.Type.DeepSpace;
        station.PlacementContext = StationPlacementContext.Context.ResourceSystem;
        station.PrimaryPurpose = StationPurpose.Purpose.Residential;
        station.Services = StationService.BasicUtilityServices();
        station.UpdateCommanderTitle();
        return station;
    }

    /// <summary>
    /// Clones a metadata dictionary.
    /// </summary>
    private static Dictionary CloneDictionary(Dictionary source)
    {
        Dictionary clone = new();
        foreach (Variant key in source.Keys)
        {
            clone[key] = source[key];
        }

        return clone;
    }

    /// <summary>
    /// Reads a string value from a dictionary.
    /// </summary>
    private static string GetString(Dictionary data, string key, string fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType == Variant.Type.String ? (string)value : fallback;
    }

    /// <summary>
    /// Reads an integer value from a dictionary.
    /// </summary>
    private static int GetInt(Dictionary data, string key, int fallback)
    {
        if (!data.ContainsKey(key))
        {
            return fallback;
        }

        Variant value = data[key];
        return value.VariantType switch
        {
            Variant.Type.Int => (int)value,
            Variant.Type.String => int.TryParse((string)value, out int parsed) ? parsed : fallback,
            _ => fallback,
        };
    }

    /// <summary>
    /// Reads a boolean value from a dictionary.
    /// </summary>
    private static bool GetBool(Dictionary data, string key, bool fallback)
    {
        return data.ContainsKey(key) && data[key].VariantType == Variant.Type.Bool ? (bool)data[key] : fallback;
    }

    /// <summary>
    /// Reads and clones a nested dictionary value.
    /// </summary>
    private static Dictionary GetDictionary(Dictionary data, string key)
    {
        return data.ContainsKey(key) && data[key].VariantType == Variant.Type.Dictionary
            ? CloneDictionary((Dictionary)data[key])
            : new Dictionary();
    }

    /// <summary>
    /// Reads a station-purpose array from a dictionary.
    /// </summary>
    private static Array<StationPurpose.Purpose> GetPurposeArray(Dictionary data, string key)
    {
        Array<StationPurpose.Purpose> result = new();
        if (!data.ContainsKey(key) || data[key].VariantType != Variant.Type.Array)
        {
            return result;
        }

        foreach (Variant value in (Array)data[key])
        {
            if (value.VariantType == Variant.Type.Int)
            {
                result.Add((StationPurpose.Purpose)(int)value);
            }
            else if (value.VariantType == Variant.Type.String && int.TryParse((string)value, out int parsed))
            {
                result.Add((StationPurpose.Purpose)parsed);
            }
        }

        return result;
    }

    /// <summary>
    /// Reads a station-service array from a dictionary.
    /// </summary>
    private static Array<StationService.Service> GetServiceArray(Dictionary data, string key)
    {
        Array<StationService.Service> result = new();
        if (!data.ContainsKey(key) || data[key].VariantType != Variant.Type.Array)
        {
            return result;
        }

        foreach (Variant value in (Array)data[key])
        {
            if (value.VariantType == Variant.Type.Int)
            {
                result.Add((StationService.Service)(int)value);
            }
            else if (value.VariantType == Variant.Type.String && int.TryParse((string)value, out int parsed))
            {
                result.Add((StationService.Service)parsed);
            }
        }

        return result;
    }
}
