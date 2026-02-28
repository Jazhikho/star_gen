## Session-based cache for generated solar systems.
## Maps star seeds to their generated SolarSystem instances.
## Cache is cleared when the application restarts.
class_name SystemCache
extends RefCounted


## Cached systems keyed by star seed.
var _cache: Dictionary = {}


## Retrieves a cached system by star seed.
## @param star_seed: The deterministic seed of the star system.
## @return: The cached SolarSystem, or null if not cached.
func get_system(star_seed: int) -> SolarSystem:
	var key: String = str(star_seed)
	if _cache.has(key):
		return _cache[key] as SolarSystem
	return null


## Stores a system in the cache.
## @param star_seed: The deterministic seed of the star system.
## @param system: The generated SolarSystem to cache.
func put_system(star_seed: int, system: SolarSystem) -> void:
	var key: String = str(star_seed)
	_cache[key] = system


## Checks if a system is cached.
## @param star_seed: The deterministic seed to check.
## @return: True if the system is cached.
func has_system(star_seed: int) -> bool:
	var key: String = str(star_seed)
	return _cache.has(key)


## Returns the number of cached systems.
## @return: Cache size.
func get_cache_size() -> int:
	return _cache.size()


## Removes a single cached system so the next visit re-generates (e.g. after a body edit).
## @param star_seed: The system to evict.
func evict(star_seed: int) -> void:
	var key: String = str(star_seed)
	if _cache.has(key):
		_cache.erase(key)


## Clears all cached systems.
func clear() -> void:
	_cache.clear()
