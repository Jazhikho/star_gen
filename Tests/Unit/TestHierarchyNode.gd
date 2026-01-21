## Tests for HierarchyNode.
extends TestCase


## Tests creating a star node.
func test_create_star_node() -> void:
	var node: HierarchyNode = HierarchyNode.create_star("node_1", "star_alpha")
	
	assert_equal(node.id, "node_1")
	assert_true(node.is_star())
	assert_false(node.is_barycenter())
	assert_equal(node.star_id, "star_alpha")
	assert_equal(node.get_star_count(), 1)


## Tests creating a barycenter node.
func test_create_barycenter_node() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("node_a", "star_a")
	var star_b: HierarchyNode = HierarchyNode.create_star("node_b", "star_b")
	
	var binary: HierarchyNode = HierarchyNode.create_barycenter(
		"binary_ab",
		star_a,
		star_b,
		1.0e11,  # ~0.67 AU
		0.5
	)
	
	assert_equal(binary.id, "binary_ab")
	assert_true(binary.is_barycenter())
	assert_false(binary.is_star())
	assert_equal(binary.children.size(), 2)
	assert_equal(binary.separation_m, 1.0e11)
	assert_equal(binary.eccentricity, 0.5)
	assert_equal(binary.get_star_count(), 2)


## Tests get_all_star_ids for single star.
func test_get_all_star_ids_single() -> void:
	var node: HierarchyNode = HierarchyNode.create_star("n1", "star_solo")
	var ids: Array[String] = node.get_all_star_ids()
	
	assert_equal(ids.size(), 1)
	assert_true(ids.has("star_solo"))


## Tests get_all_star_ids for binary.
func test_get_all_star_ids_binary() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "star_a")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "star_b")
	var binary: HierarchyNode = HierarchyNode.create_barycenter("bin", star_a, star_b, 1e11, 0.0)
	
	var ids: Array[String] = binary.get_all_star_ids()
	
	assert_equal(ids.size(), 2)
	assert_true(ids.has("star_a"))
	assert_true(ids.has("star_b"))


## Tests get_all_star_ids for hierarchical triple.
func test_get_all_star_ids_triple() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "star_a")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "star_b")
	var star_c: HierarchyNode = HierarchyNode.create_star("nc", "star_c")
	
	var inner_binary: HierarchyNode = HierarchyNode.create_barycenter("inner", star_a, star_b, 1e10, 0.1)
	var triple: HierarchyNode = HierarchyNode.create_barycenter("outer", inner_binary, star_c, 1e12, 0.3)
	
	var ids: Array[String] = triple.get_all_star_ids()
	
	assert_equal(ids.size(), 3)
	assert_true(ids.has("star_a"))
	assert_true(ids.has("star_b"))
	assert_true(ids.has("star_c"))


## Tests depth calculation.
func test_get_depth_single() -> void:
	var node: HierarchyNode = HierarchyNode.create_star("n1", "star_1")
	assert_equal(node.get_depth(), 1)


## Tests depth for binary.
func test_get_depth_binary() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "star_a")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "star_b")
	var binary: HierarchyNode = HierarchyNode.create_barycenter("bin", star_a, star_b, 1e11, 0.0)
	
	assert_equal(binary.get_depth(), 2)


## Tests depth for hierarchical triple.
func test_get_depth_triple() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "star_a")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "star_b")
	var star_c: HierarchyNode = HierarchyNode.create_star("nc", "star_c")
	
	var inner: HierarchyNode = HierarchyNode.create_barycenter("inner", star_a, star_b, 1e10, 0.0)
	var outer: HierarchyNode = HierarchyNode.create_barycenter("outer", inner, star_c, 1e12, 0.0)
	
	assert_equal(outer.get_depth(), 3)


## Tests find_node.
func test_find_node() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "star_a")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "star_b")
	var binary: HierarchyNode = HierarchyNode.create_barycenter("bin", star_a, star_b, 1e11, 0.0)
	
	assert_equal(binary.find_node("bin"), binary)
	assert_equal(binary.find_node("na"), star_a)
	assert_equal(binary.find_node("nb"), star_b)
	assert_null(binary.find_node("nonexistent"))


## Tests serialization round-trip for star node.
func test_round_trip_star() -> void:
	var original: HierarchyNode = HierarchyNode.create_star("node_1", "star_alpha")
	
	var data: Dictionary = original.to_dict()
	var restored: HierarchyNode = HierarchyNode.from_dict(data)
	
	assert_equal(restored.id, original.id)
	assert_equal(restored.node_type, original.node_type)
	assert_equal(restored.star_id, original.star_id)


## Tests serialization round-trip for barycenter node.
func test_round_trip_barycenter() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "star_a")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "star_b")
	var original: HierarchyNode = HierarchyNode.create_barycenter("bin", star_a, star_b, 1.5e11, 0.3)
	original.orbital_period_s = 86400.0 * 365.0  # ~1 year
	
	var data: Dictionary = original.to_dict()
	var restored: HierarchyNode = HierarchyNode.from_dict(data)
	
	assert_equal(restored.id, original.id)
	assert_equal(restored.node_type, original.node_type)
	assert_equal(restored.children.size(), 2)
	assert_float_equal(restored.separation_m, original.separation_m)
	assert_float_equal(restored.eccentricity, original.eccentricity)
	assert_float_equal(restored.orbital_period_s, original.orbital_period_s)
	
	# Check children
	assert_equal(restored.children[0].star_id, "star_a")
	assert_equal(restored.children[1].star_id, "star_b")
