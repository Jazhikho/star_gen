using Godot.Collections;

namespace StarGen.Domain.Population;

/// <summary>
/// Services that a station can provide.
/// </summary>
public static class StationService
{
	/// <summary>
	/// Available services.
	/// </summary>
	public enum Service
	{
		Refuel,
		Repair,
		Trade,
		Medical,
		Customs,
		Entertainment,
		Lodging,
		Shipyard,
		Banking,
		Communications,
		Storage,
		Security,
	}

	/// <summary>
	/// Converts a service to a display string.
	/// </summary>
	public static string ToStringName(Service service)
	{
		return service switch
		{
			Service.Refuel => "Refuel",
			Service.Repair => "Repair",
			Service.Trade => "Trade",
			Service.Medical => "Medical",
			Service.Customs => "Customs",
			Service.Entertainment => "Entertainment",
			Service.Lodging => "Lodging",
			Service.Shipyard => "Shipyard",
			Service.Banking => "Banking",
			Service.Communications => "Communications",
			Service.Storage => "Storage",
			Service.Security => "Security",
			_ => "Unknown",
		};
	}

	/// <summary>
	/// Parses a service from a string.
	/// </summary>
	public static Service FromString(string name)
	{
		return name.ToLowerInvariant().Trim() switch
		{
			"refuel" => Service.Refuel,
			"repair" => Service.Repair,
			"trade" => Service.Trade,
			"medical" => Service.Medical,
			"customs" => Service.Customs,
			"entertainment" => Service.Entertainment,
			"lodging" => Service.Lodging,
			"shipyard" => Service.Shipyard,
			"banking" => Service.Banking,
			"communications" => Service.Communications,
			"storage" => Service.Storage,
			"security" => Service.Security,
			_ => Service.Refuel,
		};
	}

	/// <summary>
	/// Returns the basic utility-station services.
	/// </summary>
	public static Array<Service> BasicUtilityServices()
	{
		return new Array<Service> { Service.Refuel, Service.Repair, Service.Trade, Service.Lodging };
	}

	/// <summary>
	/// Returns services that require larger infrastructure.
	/// </summary>
	public static Array<Service> AdvancedServices()
	{
		return new Array<Service> { Service.Shipyard, Service.Banking, Service.Entertainment };
	}

	/// <summary>
	/// Returns common services found on most stations.
	/// </summary>
	public static Array<Service> CommonServices()
	{
		return new Array<Service> { Service.Refuel, Service.Communications };
	}

	/// <summary>
	/// Returns whether a service requires major infrastructure.
	/// </summary>
	public static bool RequiresMajorInfrastructure(Service service)
	{
		return service == Service.Shipyard
			|| service == Service.Banking
			|| service == Service.Entertainment;
	}

	/// <summary>
	/// Returns the number of services.
	/// </summary>
	public static int Count()
	{
		return 12;
	}
}
