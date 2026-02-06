## Authority/governance type for small stations (U/O class).
## Simpler than full colony government; describes who controls the station.
class_name OutpostAuthority
extends RefCounted


## Authority types for outposts.
enum Type {
	CORPORATE, ## Owned/operated by a corporation
	MILITARY, ## Military installation
	INDEPENDENT, ## Independently owned/operated
	FRANCHISE, ## Franchise of larger organization
	COOPERATIVE, ## Worker/resident cooperative
	AUTOMATED, ## Primarily automated with minimal crew
	GOVERNMENT, ## Direct government operation
	RELIGIOUS, ## Religious organization
}


## Converts an authority type to a display string.
## @param authority: The authority enum value.
## @return: Human-readable string.
static func to_string_name(authority: Type) -> String:
	match authority:
		Type.CORPORATE:
			return "Corporate"
		Type.MILITARY:
			return "Military"
		Type.INDEPENDENT:
			return "Independent"
		Type.FRANCHISE:
			return "Franchise"
		Type.COOPERATIVE:
			return "Cooperative"
		Type.AUTOMATED:
			return "Automated"
		Type.GOVERNMENT:
			return "Government"
		Type.RELIGIOUS:
			return "Religious"
		_:
			return "Unknown"


## Converts a string to an authority type.
## @param name: The string name (case-insensitive).
## @return: The authority type, or Type.INDEPENDENT if not found.
static func from_string(name: String) -> Type:
	match name.to_lower().strip_edges():
		"corporate":
			return Type.CORPORATE
		"military":
			return Type.MILITARY
		"independent":
			return Type.INDEPENDENT
		"franchise":
			return Type.FRANCHISE
		"cooperative":
			return Type.COOPERATIVE
		"automated":
			return Type.AUTOMATED
		"government":
			return Type.GOVERNMENT
		"religious":
			return Type.RELIGIOUS
		_:
			return Type.INDEPENDENT


## Returns the typical commander title for an authority type.
## @param authority: The authority type.
## @return: Typical title string.
static func typical_commander_title(authority: Type) -> String:
	match authority:
		Type.CORPORATE:
			return "Station Manager"
		Type.MILITARY:
			return "Base Commander"
		Type.INDEPENDENT:
			return "Station Chief"
		Type.FRANCHISE:
			return "Franchise Manager"
		Type.COOPERATIVE:
			return "Station Coordinator"
		Type.AUTOMATED:
			return "System Administrator"
		Type.GOVERNMENT:
			return "Station Director"
		Type.RELIGIOUS:
			return "Station Prior"
		_:
			return "Station Chief"


## Returns whether this authority type has a parent organization.
## @param authority: The authority type.
## @return: True if typically has parent org.
static func has_parent_organization(authority: Type) -> bool:
	return authority in [Type.CORPORATE, Type.MILITARY, Type.FRANCHISE, Type.GOVERNMENT, Type.RELIGIOUS]


## Returns typical authority types for utility stations.
## @return: Array of common authority types.
static func typical_for_utility() -> Array[Type]:
	return [Type.CORPORATE, Type.FRANCHISE, Type.INDEPENDENT]


## Returns typical authority types for outposts.
## @return: Array of common authority types.
static func typical_for_outpost() -> Array[Type]:
	return [Type.CORPORATE, Type.MILITARY, Type.GOVERNMENT, Type.INDEPENDENT]


## Returns the number of authority types.
## @return: Count of Type enum values.
static func count() -> int:
	return 8
