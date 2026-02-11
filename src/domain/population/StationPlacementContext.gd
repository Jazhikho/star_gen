## Context describing why/where a station is placed in a system.
## Used by placement rules to determine appropriate station characteristics.
class_name StationPlacementContext
extends RefCounted


## Placement context types.
enum Context {
	BRIDGE_SYSTEM, ## System serves as waypoint/bridge between regions
	COLONY_WORLD, ## Orbiting or supporting a colony world
	NATIVE_WORLD, ## Orbiting a world with spacefaring natives
	RESOURCE_SYSTEM, ## System rich in resources but no habitable worlds
	STRATEGIC, ## Military/strategic importance
	SCIENTIFIC, ## Scientific interest (anomaly, research target)
	OTHER, ## General/miscellaneous
}


## Converts a context to a display string.
## @param context: The context enum value.
## @return: Human-readable string.
static func to_string_name(context: Context) -> String:
	match context:
		Context.BRIDGE_SYSTEM:
			return "Bridge System"
		Context.COLONY_WORLD:
			return "Colony World"
		Context.NATIVE_WORLD:
			return "Native World"
		Context.RESOURCE_SYSTEM:
			return "Resource System"
		Context.STRATEGIC:
			return "Strategic"
		Context.SCIENTIFIC:
			return "Scientific"
		Context.OTHER:
			return "Other"
		_:
			return "Unknown"


## Converts a string to a context enum.
## @param name: The string name (case-insensitive).
## @return: The context, or Context.OTHER if not found.
static func from_string(name: String) -> Context:
	match name.to_lower().replace(" ", "_").strip_edges():
		"bridge_system", "bridge":
			return Context.BRIDGE_SYSTEM
		"colony_world", "colony":
			return Context.COLONY_WORLD
		"native_world", "native":
			return Context.NATIVE_WORLD
		"resource_system", "resource":
			return Context.RESOURCE_SYSTEM
		"strategic":
			return Context.STRATEGIC
		"scientific", "science":
			return Context.SCIENTIFIC
		"other":
			return Context.OTHER
		_:
			return Context.OTHER


## Returns whether this context typically has small utility stations.
## @param context: The context to check.
## @return: True if U-class stations are common.
static func favors_utility_stations(context: Context) -> bool:
	return context == Context.BRIDGE_SYSTEM


## Returns whether this context can support large populations.
## @param context: The context to check.
## @return: True if B/A/S class stations are possible.
static func can_support_large_stations(context: Context) -> bool:
	return context in [Context.COLONY_WORLD, Context.NATIVE_WORLD, Context.RESOURCE_SYSTEM]


## Returns whether this context requires spacefaring natives.
## @param context: The context to check.
## @return: True if native tech level matters.
static func requires_spacefaring_natives(context: Context) -> bool:
	return context == Context.NATIVE_WORLD


## Returns the number of contexts.
## @return: Count of Context enum values.
static func count() -> int:
	return 7
