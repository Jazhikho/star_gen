## Services that a station can provide to visitors.
## Stations typically offer multiple services based on their purpose and class.
class_name StationService
extends RefCounted


## Available station services.
enum Service {
	REFUEL, ## Fuel resupply
	REPAIR, ## Ship repair and maintenance
	TRADE, ## Goods trading, marketplace
	MEDICAL, ## Medical facilities
	CUSTOMS, ## Cargo inspection, legal services
	ENTERTAINMENT, ## Recreation, leisure facilities
	LODGING, ## Temporary accommodation
	SHIPYARD, ## Ship construction/major repairs
	BANKING, ## Financial services
	COMMUNICATIONS, ## Long-range comms relay
	STORAGE, ## Cargo storage facilities
	SECURITY, ## Protection services, escorts
}


## Converts a service to a display string.
## @param service: The service enum value.
## @return: Human-readable string.
static func to_string_name(service: Service) -> String:
	match service:
		Service.REFUEL:
			return "Refuel"
		Service.REPAIR:
			return "Repair"
		Service.TRADE:
			return "Trade"
		Service.MEDICAL:
			return "Medical"
		Service.CUSTOMS:
			return "Customs"
		Service.ENTERTAINMENT:
			return "Entertainment"
		Service.LODGING:
			return "Lodging"
		Service.SHIPYARD:
			return "Shipyard"
		Service.BANKING:
			return "Banking"
		Service.COMMUNICATIONS:
			return "Communications"
		Service.STORAGE:
			return "Storage"
		Service.SECURITY:
			return "Security"
		_:
			return "Unknown"


## Converts a string to a service enum.
## @param name: The string name (case-insensitive).
## @return: The service, or Service.REFUEL if not found.
static func from_string(name: String) -> Service:
	match name.to_lower().strip_edges():
		"refuel":
			return Service.REFUEL
		"repair":
			return Service.REPAIR
		"trade":
			return Service.TRADE
		"medical":
			return Service.MEDICAL
		"customs":
			return Service.CUSTOMS
		"entertainment":
			return Service.ENTERTAINMENT
		"lodging":
			return Service.LODGING
		"shipyard":
			return Service.SHIPYARD
		"banking":
			return Service.BANKING
		"communications":
			return Service.COMMUNICATIONS
		"storage":
			return Service.STORAGE
		"security":
			return Service.SECURITY
		_:
			return Service.REFUEL


## Returns basic services typically found on utility (U-class) stations.
## @return: Array of basic services.
static func basic_utility_services() -> Array[Service]:
	return [Service.REFUEL, Service.REPAIR, Service.TRADE, Service.LODGING]


## Returns services that require larger populations to support.
## @return: Array of advanced services.
static func advanced_services() -> Array[Service]:
	return [Service.SHIPYARD, Service.BANKING, Service.ENTERTAINMENT]


## Returns services typically found on all inhabited stations.
## @return: Array of common services.
static func common_services() -> Array[Service]:
	return [Service.REFUEL, Service.COMMUNICATIONS]


## Returns whether a service requires significant infrastructure.
## @param service: The service to check.
## @return: True if service requires major facilities.
static func requires_major_infrastructure(service: Service) -> bool:
	return service in [Service.SHIPYARD, Service.BANKING, Service.ENTERTAINMENT]


## Returns the number of services.
## @return: Count of Service enum values.
static func count() -> int:
	return 12
