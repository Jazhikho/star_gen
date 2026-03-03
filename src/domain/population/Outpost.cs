using Godot;
using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Small orbital or deep-space habitat with population up to 10,000.
/// </summary>
public partial class Outpost : RefCounted
{
    /// <summary>
    /// Maximum outpost population.
    /// </summary>
    public const int MaxPopulation = 10000;

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
    public StationPurpose.Purpose PrimaryPurpose = StationPurpose.Purpose.Utility;

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
    /// Governance authority.
    /// </summary>
    public OutpostAuthority.Type Authority = OutpostAuthority.Type.Independent;

    /// <summary>
    /// Parent organization identifier.
    /// </summary>
    public string ParentOrganizationId = string.Empty;

    /// <summary>
    /// Parent organization display name.
    /// </summary>
    public string ParentOrganizationName = string.Empty;

    /// <summary>
    /// Current population.
    /// </summary>
    public int Population;

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
    /// Whether the outpost is operational.
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
    /// Commander title.
    /// </summary>
    public string CommanderTitle;

    /// <summary>
    /// Commander name.
    /// </summary>
    public string CommanderName = string.Empty;

    /// <summary>
    /// Extra metadata.
    /// </summary>
    public Dictionary Metadata = new();

    /// <summary>
    /// Creates a new outpost.
    /// </summary>
    public Outpost()
    {
        CommanderTitle = StarGen.Domain.Population.OutpostAuthority.TypicalCommanderTitle(Authority);
    }

    /// <summary>
    /// Returns the age of the outpost.
    /// </summary>
    public int GetAge(int currentYear = 0)
    {
        return IsOperational ? currentYear - EstablishedYear : DecommissionedYear - EstablishedYear;
    }

    /// <summary>
    /// Returns whether this is a utility-class station.
    /// </summary>
    public bool IsUtility()
    {
        return StationClass == StarGen.Domain.Population.StationClass.Class.U;
    }

    /// <summary>
    /// Returns whether this outpost is associated with a specific body.
    /// </summary>
    public bool IsBodyAssociated()
    {
        return StarGen.Domain.Population.StationType.IsBodyAssociated(StationType) && !string.IsNullOrEmpty(OrbitingBodyId);
    }

    /// <summary>
    /// Returns whether this outpost has a parent organization.
    /// </summary>
    public bool HasParentOrganization()
    {
        return StarGen.Domain.Population.OutpostAuthority.HasParentOrganization(Authority) && !string.IsNullOrEmpty(ParentOrganizationId);
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
    /// Sets the outpost population, clamped to the valid range.
    /// </summary>
    public void SetPopulation(int newPopulation)
    {
        Population = System.Math.Clamp(newPopulation, 0, MaxPopulation);
    }

    /// <summary>
    /// Records decommissioning.
    /// </summary>
    public void RecordDecommissioning(int year, string reason)
    {
        IsOperational = false;
        DecommissionedYear = year;
        DecommissionedReason = reason;
    }

    /// <summary>
    /// Updates the commander title from the authority type.
    /// </summary>
    public void UpdateCommanderTitle()
    {
        CommanderTitle = StarGen.Domain.Population.OutpostAuthority.TypicalCommanderTitle(Authority);
    }

    /// <summary>
    /// Returns a summary of the outpost.
    /// </summary>
    public Dictionary GetSummary()
    {
        return new Dictionary
        {
            ["id"] = Id,
            ["name"] = Name,
            ["class"] = StarGen.Domain.Population.StationClass.ToLetter(StationClass),
            ["type"] = StarGen.Domain.Population.StationType.ToStringName(StationType),
            ["purpose"] = StationPurpose.ToStringName(PrimaryPurpose),
            ["authority"] = StarGen.Domain.Population.OutpostAuthority.ToStringName(Authority),
            ["population"] = Population,
            ["is_operational"] = IsOperational,
            ["age"] = GetAge(),
            ["services_count"] = Services.Count,
        };
    }

    /// <summary>
    /// Validates the outpost data and returns any errors.
    /// </summary>
    public Array<string> Validate()
    {
        Array<string> errors = new();
        if (string.IsNullOrEmpty(Id))
        {
            errors.Add("Outpost ID is required");
        }

        if (Population < 0)
        {
            errors.Add("Population cannot be negative");
        }

        if (Population > MaxPopulation)
        {
            errors.Add($"Population exceeds outpost maximum of {MaxPopulation}");
        }

        if (StationClass != StarGen.Domain.Population.StationClass.Class.U
            && StationClass != StarGen.Domain.Population.StationClass.Class.O)
        {
            errors.Add($"Outpost must be U or O class, not {StarGen.Domain.Population.StationClass.ToLetter(StationClass)}");
        }

        if (StationType == StarGen.Domain.Population.StationType.Type.Orbital && string.IsNullOrEmpty(OrbitingBodyId))
        {
            errors.Add("Orbital outpost should specify orbiting_body_id");
        }

        return errors;
    }

    /// <summary>
    /// Returns whether the outpost data is valid.
    /// </summary>
    public bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Converts the outpost to a dictionary payload.
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

        return new Dictionary
        {
            ["id"] = Id,
            ["name"] = Name,
            ["station_class"] = (int)StationClass,
            ["station_type"] = (int)StationType,
            ["primary_purpose"] = (int)PrimaryPurpose,
            ["secondary_purposes"] = secondaryPurposes,
            ["services"] = services,
            ["placement_context"] = (int)PlacementContext,
            ["authority"] = (int)Authority,
            ["parent_organization_id"] = ParentOrganizationId,
            ["parent_organization_name"] = ParentOrganizationName,
            ["population"] = Population,
            ["established_year"] = EstablishedYear,
            ["orbiting_body_id"] = OrbitingBodyId,
            ["system_id"] = SystemId,
            ["is_operational"] = IsOperational,
            ["decommissioned_year"] = DecommissionedYear,
            ["decommissioned_reason"] = DecommissionedReason,
            ["commander_title"] = CommanderTitle,
            ["commander_name"] = CommanderName,
            ["metadata"] = CloneDictionary(Metadata),
        };
    }

    /// <summary>
    /// Creates an outpost from a dictionary payload.
    /// </summary>
    public static Outpost FromDictionary(Dictionary data)
    {
        Outpost outpost = new();
        outpost.Id = GetString(data, "id", string.Empty);
        outpost.Name = GetString(data, "name", string.Empty);
        outpost.StationClass = (StarGen.Domain.Population.StationClass.Class)GetInt(data, "station_class", (int)StarGen.Domain.Population.StationClass.Class.O);
        outpost.StationType = (StarGen.Domain.Population.StationType.Type)GetInt(data, "station_type", (int)StarGen.Domain.Population.StationType.Type.Orbital);
        outpost.PrimaryPurpose = (StationPurpose.Purpose)GetInt(data, "primary_purpose", (int)StationPurpose.Purpose.Utility);
        outpost.SecondaryPurposes = GetPurposeArray(data, "secondary_purposes");
        outpost.Services = GetServiceArray(data, "services");
        outpost.PlacementContext = (StationPlacementContext.Context)GetInt(data, "placement_context", (int)StationPlacementContext.Context.Other);
        outpost.Authority = (StarGen.Domain.Population.OutpostAuthority.Type)GetInt(
            data,
            "authority",
            (int)StarGen.Domain.Population.OutpostAuthority.Type.Independent);
        outpost.ParentOrganizationId = GetString(data, "parent_organization_id", string.Empty);
        outpost.ParentOrganizationName = GetString(data, "parent_organization_name", string.Empty);
        outpost.Population = System.Math.Clamp(GetInt(data, "population", 0), 0, MaxPopulation);
        outpost.EstablishedYear = GetInt(data, "established_year", 0);
        outpost.OrbitingBodyId = GetString(data, "orbiting_body_id", string.Empty);
        outpost.SystemId = GetString(data, "system_id", string.Empty);
        outpost.IsOperational = GetBool(data, "is_operational", true);
        outpost.DecommissionedYear = GetInt(data, "decommissioned_year", 0);
        outpost.DecommissionedReason = GetString(data, "decommissioned_reason", string.Empty);
        outpost.CommanderTitle = GetString(data, "commander_title", string.Empty);
        outpost.CommanderName = GetString(data, "commander_name", string.Empty);
        outpost.Metadata = GetDictionary(data, "metadata");
        if (string.IsNullOrEmpty(outpost.CommanderTitle))
        {
            outpost.UpdateCommanderTitle();
        }

        return outpost;
    }

    /// <summary>
    /// Creates a basic utility outpost.
    /// </summary>
    public static Outpost CreateUtility(string outpostId, string outpostName, string system)
    {
        Outpost outpost = new();
        outpost.Id = outpostId;
        outpost.Name = outpostName;
        outpost.SystemId = system;
        outpost.StationClass = StarGen.Domain.Population.StationClass.Class.U;
        outpost.PrimaryPurpose = StationPurpose.Purpose.Utility;
        outpost.PlacementContext = StationPlacementContext.Context.BridgeSystem;
        outpost.Services = StationService.BasicUtilityServices();
        outpost.UpdateCommanderTitle();
        return outpost;
    }

    /// <summary>
    /// Creates a basic mining outpost.
    /// </summary>
    public static Outpost CreateMining(string outpostId, string outpostName, string system, string bodyId)
    {
        Outpost outpost = new();
        outpost.Id = outpostId;
        outpost.Name = outpostName;
        outpost.SystemId = system;
        outpost.StationClass = StarGen.Domain.Population.StationClass.Class.O;
        outpost.StationType = StarGen.Domain.Population.StationType.Type.Orbital;
        outpost.OrbitingBodyId = bodyId;
        outpost.PrimaryPurpose = StationPurpose.Purpose.Mining;
        outpost.PlacementContext = StationPlacementContext.Context.ResourceSystem;
        outpost.Authority = StarGen.Domain.Population.OutpostAuthority.Type.Corporate;
        outpost.Services = new Array<StationService.Service> { StationService.Service.Refuel, StationService.Service.Storage };
        outpost.UpdateCommanderTitle();
        return outpost;
    }

    /// <summary>
    /// Creates a basic science outpost.
    /// </summary>
    public static Outpost CreateScience(string outpostId, string outpostName, string system)
    {
        Outpost outpost = new();
        outpost.Id = outpostId;
        outpost.Name = outpostName;
        outpost.SystemId = system;
        outpost.StationClass = StarGen.Domain.Population.StationClass.Class.O;
        outpost.StationType = StarGen.Domain.Population.StationType.Type.DeepSpace;
        outpost.PrimaryPurpose = StationPurpose.Purpose.Science;
        outpost.PlacementContext = StationPlacementContext.Context.Scientific;
        outpost.Authority = StarGen.Domain.Population.OutpostAuthority.Type.Government;
        outpost.Services = new Array<StationService.Service> { StationService.Service.Communications, StationService.Service.Lodging };
        outpost.UpdateCommanderTitle();
        return outpost;
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
