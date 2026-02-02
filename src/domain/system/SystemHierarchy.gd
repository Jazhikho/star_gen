## Manages the hierarchical arrangement of stars in a solar system.
## Supports arbitrary nesting depth for multi-star systems.
class_name SystemHierarchy
extends RefCounted

const _hierarchy_node: GDScript = preload("res://src/domain/system/HierarchyNode.gd")


## The root node of the hierarchy.
var root: HierarchyNode


## Creates a new SystemHierarchy.
## @param p_root: The root node (optional, can be set later).
func _init(p_root: HierarchyNode = null) -> void:
	root = p_root


## Returns true if the hierarchy has been initialized.
## @return: True if root is not null.
func is_valid() -> bool:
	return root != null


## Returns the total number of stars in the system.
## @return: Star count.
func get_star_count() -> int:
	if root == null:
		return 0
	return root.get_star_count()


## Returns all star IDs in the system.
## @return: Array of star IDs.
func get_all_star_ids() -> Array[String]:
	if root == null:
		return []
	return root.get_all_star_ids()


## Returns the maximum hierarchy depth.
## @return: Depth (1 = single star, 2 = simple binary, etc.).
func get_depth() -> int:
	if root == null:
		return 0
	return root.get_depth()


## Finds a node by ID anywhere in the hierarchy.
## @param node_id: The node ID to find.
## @return: The node, or null if not found.
func find_node(node_id: String) -> HierarchyNode:
	if root == null:
		return null
	return root.find_node(node_id)


## Returns all nodes in the hierarchy (flattened).
## @return: Array of all nodes.
func get_all_nodes() -> Array[HierarchyNode]:
	var result: Array[HierarchyNode] = []
	if root != null:
		_collect_nodes(root, result)
	return result


## Recursively collects all nodes.
## @param node: Current node.
## @param result: Array to populate.
func _collect_nodes(node: HierarchyNode, result: Array[HierarchyNode]) -> void:
	result.append(node)
	if node.is_barycenter():
		for child in node.children:
			_collect_nodes(child, result)


## Returns all barycenter (non-leaf) nodes.
## @return: Array of barycenter nodes.
func get_all_barycenters() -> Array[HierarchyNode]:
	var result: Array[HierarchyNode] = []
	for node in get_all_nodes():
		if node.is_barycenter():
			result.append(node)
	return result


## Returns all star (leaf) nodes.
## @return: Array of star nodes.
func get_all_star_nodes() -> Array[HierarchyNode]:
	var result: Array[HierarchyNode] = []
	for node in get_all_nodes():
		if node.is_star():
			result.append(node)
	return result


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var data: Dictionary = {}
	if root != null:
		data["root"] = root.to_dict()
	return data


## Creates a SystemHierarchy from a dictionary.
## @param data: Dictionary to parse.
## @return: A new SystemHierarchy.
static func from_dict(data: Dictionary) -> SystemHierarchy:
	var hierarchy: SystemHierarchy = SystemHierarchy.new()
	if data.has("root"):
		hierarchy.root = HierarchyNode.from_dict(data["root"] as Dictionary)
	return hierarchy
