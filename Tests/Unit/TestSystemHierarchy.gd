## Tests for SystemHierarchy.
extends TestCase


## Tests empty hierarchy.
func test_empty_hierarchy() -> void:
	var hierarchy: SystemHierarchy = SystemHierarchy.new()
	
	assert_false(hierarchy.is_valid())
	assert_equal(hierarchy.get_star_count(), 0)
	assert_equal(hierarchy.get_depth(), 0)
	assert_equal(hierarchy.get_all_star_ids().size(), 0)


## Tests single star hierarchy.
func test_single_star() -> void:
	var star: HierarchyNode = HierarchyNode.create_star("n1", "sol")
	var hierarchy: SystemHierarchy = SystemHierarchy.new(star)
	
	assert_true(hierarchy.is_valid())
	assert_equal(hierarchy.get_star_count(), 1)
	assert_equal(hierarchy.get_depth(), 1)
	assert_equal(hierarchy.get_all_star_ids()[0], "sol")


## Tests binary star hierarchy.
func test_binary_stars() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "alpha")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "beta")
	var binary: HierarchyNode = HierarchyNode.create_barycenter("binary", star_a, star_b, 1e11, 0.0)
	var hierarchy: SystemHierarchy = SystemHierarchy.new(binary)
	
	assert_true(hierarchy.is_valid())
	assert_equal(hierarchy.get_star_count(), 2)
	assert_equal(hierarchy.get_depth(), 2)


## Tests get_all_nodes.
func test_get_all_nodes() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "alpha")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "beta")
	var binary: HierarchyNode = HierarchyNode.create_barycenter("binary", star_a, star_b, 1e11, 0.0)
	var hierarchy: SystemHierarchy = SystemHierarchy.new(binary)
	
	var nodes: Array[HierarchyNode] = hierarchy.get_all_nodes()
	
	assert_equal(nodes.size(), 3)


## Tests get_all_barycenters.
func test_get_all_barycenters() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "alpha")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "beta")
	var star_c: HierarchyNode = HierarchyNode.create_star("nc", "gamma")
	var inner: HierarchyNode = HierarchyNode.create_barycenter("inner", star_a, star_b, 1e10, 0.0)
	var outer: HierarchyNode = HierarchyNode.create_barycenter("outer", inner, star_c, 1e12, 0.0)
	var hierarchy: SystemHierarchy = SystemHierarchy.new(outer)
	
	var barycenters: Array[HierarchyNode] = hierarchy.get_all_barycenters()
	
	assert_equal(barycenters.size(), 2)


## Tests get_all_star_nodes.
func test_get_all_star_nodes() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "alpha")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "beta")
	var binary: HierarchyNode = HierarchyNode.create_barycenter("binary", star_a, star_b, 1e11, 0.0)
	var hierarchy: SystemHierarchy = SystemHierarchy.new(binary)
	
	var star_nodes: Array[HierarchyNode] = hierarchy.get_all_star_nodes()
	
	assert_equal(star_nodes.size(), 2)


## Tests find_node.
func test_find_node() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "alpha")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "beta")
	var binary: HierarchyNode = HierarchyNode.create_barycenter("binary", star_a, star_b, 1e11, 0.0)
	var hierarchy: SystemHierarchy = SystemHierarchy.new(binary)
	
	assert_not_null(hierarchy.find_node("binary"))
	assert_not_null(hierarchy.find_node("na"))
	assert_not_null(hierarchy.find_node("nb"))
	assert_null(hierarchy.find_node("nonexistent"))


## Tests serialization round-trip.
func test_round_trip() -> void:
	var star_a: HierarchyNode = HierarchyNode.create_star("na", "alpha")
	var star_b: HierarchyNode = HierarchyNode.create_star("nb", "beta")
	var binary: HierarchyNode = HierarchyNode.create_barycenter("binary", star_a, star_b, 1e11, 0.5)
	var original: SystemHierarchy = SystemHierarchy.new(binary)
	
	var data: Dictionary = original.to_dict()
	var restored: SystemHierarchy = SystemHierarchy.from_dict(data)
	
	assert_true(restored.is_valid())
	assert_equal(restored.get_star_count(), 2)
	assert_equal(restored.get_depth(), 2)
	assert_not_null(restored.find_node("binary"))
