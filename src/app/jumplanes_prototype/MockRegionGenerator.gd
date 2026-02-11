## Generates mock regions with systems for testing jump lanes.
class_name MockRegionGenerator
extends RefCounted


## Creates a simple test region with a few systems.
## @param seed_value: RNG seed for reproducibility.
## @return: JumpLaneRegion with test systems.
static func create_simple_region(seed_value: int = 12345) -> JumpLaneRegion:
	var region: JumpLaneRegion = JumpLaneRegion.new(
		JumpLaneRegion.RegionScope.SUBSECTOR,
		"Test Subsector"
	)

	region.add_system(JumpLaneSystem.new("alpha", Vector3(0, 0, 0), 5000))
	region.add_system(JumpLaneSystem.new("beta", Vector3(2.5, 0, 1), 15000))
	region.add_system(JumpLaneSystem.new("gamma", Vector3(6, 0, -1), 25000))
	region.add_system(JumpLaneSystem.new("delta", Vector3(13, 0, 0), 35000))
	region.add_system(JumpLaneSystem.new("epsilon", Vector3(21, 0, 1), 50000))
	region.add_system(JumpLaneSystem.new("bridge_1", Vector3(17, 0, 0), 0))
	region.add_system(JumpLaneSystem.new("orphan", Vector3(-15, 0, 5), 8000))

	return region


## Creates a random region with specified parameters.
## @param seed_value: RNG seed for reproducibility.
## @param system_count: Number of systems to generate.
## @param region_size: Size of region in parsecs.
## @param populated_ratio: Ratio of populated systems (0.0-1.0).
## @return: JumpLaneRegion with random systems.
static func create_random_region(
	seed_value: int,
	system_count: int = 20,
	region_size: float = 30.0,
	populated_ratio: float = 0.7
) -> JumpLaneRegion:
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = seed_value

	var region: JumpLaneRegion = JumpLaneRegion.new(
		JumpLaneRegion.RegionScope.SUBSECTOR,
		"Random Subsector %d" % seed_value
	)

	for i in range(system_count):
		var pos: Vector3 = Vector3(
			rng.randf_range(-region_size / 2, region_size / 2),
			rng.randf_range(-2, 2),
			rng.randf_range(-region_size / 2, region_size / 2)
		)

		var pop: int = 0
		if rng.randf() < populated_ratio:
			pop = rng.randi_range(1000, 100000)

		var system_id: String = "sys_%03d" % i
		region.add_system(JumpLaneSystem.new(system_id, pos, pop))

	return region


## Creates a clustered region with groups of systems.
## @param seed_value: RNG seed for reproducibility.
## @return: JumpLaneRegion with clustered systems.
static func create_clustered_region(seed_value: int = 54321) -> JumpLaneRegion:
	var rng: RandomNumberGenerator = RandomNumberGenerator.new()
	rng.seed = seed_value

	var region: JumpLaneRegion = JumpLaneRegion.new(
		JumpLaneRegion.RegionScope.SUBSECTOR,
		"Clustered Subsector"
	)

	var clusters: Array[Vector3] = [
		Vector3(0, 0, 0),
		Vector3(15, 0, 10),
		Vector3(-12, 0, -8),
	]

	var system_index: int = 0
	for cluster_center in clusters:
		var cluster_size: int = rng.randi_range(4, 7)
		for i in range(cluster_size):
			var offset: Vector3 = Vector3(
				rng.randf_range(-4, 4),
				rng.randf_range(-1, 1),
				rng.randf_range(-4, 4)
			)
			var pos: Vector3 = cluster_center + offset
			var pop: int = 0
			if rng.randf() > 0.2:
				pop = rng.randi_range(5000, 80000)

			region.add_system(JumpLaneSystem.new("sys_%03d" % system_index, pos, pop))
			system_index += 1

	return region
