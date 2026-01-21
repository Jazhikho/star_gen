## A node in the stellar hierarchy tree.
## Can be either a single star (STAR type) or a gravitationally bound subsystem (BARYCENTER type).
class_name HierarchyNode
extends RefCounted


## Node type enumeration.
enum NodeType {
	STAR,      ## Leaf node containing a single star
	BARYCENTER ## Internal node representing bound subsystem (binary, etc.)
}


## Unique identifier for this node.
var id: String

## Type of this node.
var node_type: NodeType

## For STAR nodes: the ID of the star body.
var star_id: String

## For BARYCENTER nodes: the child nodes (always exactly 2).
var children: Array[HierarchyNode]

## For BARYCENTER nodes: separation between children in meters.
var separation_m: float

## For BARYCENTER nodes: eccentricity of the binary orbit.
var eccentricity: float

## For BARYCENTER nodes: orbital period of the pair in seconds.
var orbital_period_s: float


## Creates a new HierarchyNode.
## @param p_id: Unique node identifier.
## @param p_node_type: Type of node (STAR or BARYCENTER).
func _init(
	p_id: String = "",
	p_node_type: NodeType = NodeType.STAR
) -> void:
	id = p_id
	node_type = p_node_type
	star_id = ""
	children = []
	separation_m = 0.0
	eccentricity = 0.0
	orbital_period_s = 0.0


## Creates a star (leaf) node.
## @param p_id: Node identifier.
## @param p_star_id: ID of the star body.
## @return: A new star node.
static func create_star(p_id: String, p_star_id: String) -> HierarchyNode:
	var node: HierarchyNode = HierarchyNode.new(p_id, NodeType.STAR)
	node.star_id = p_star_id
	return node


## Creates a barycenter (pair) node.
## @param p_id: Node identifier.
## @param p_child_a: First child node.
## @param p_child_b: Second child node.
## @param p_separation_m: Separation between children in meters.
## @param p_eccentricity: Binary orbit eccentricity.
## @return: A new barycenter node.
static func create_barycenter(
	p_id: String,
	p_child_a: HierarchyNode,
	p_child_b: HierarchyNode,
	p_separation_m: float,
	p_eccentricity: float = 0.0
) -> HierarchyNode:
	var node: HierarchyNode = HierarchyNode.new(p_id, NodeType.BARYCENTER)
	node.children = [p_child_a, p_child_b]
	node.separation_m = p_separation_m
	node.eccentricity = p_eccentricity
	return node


## Returns true if this is a star (leaf) node.
## @return: True if STAR type.
func is_star() -> bool:
	return node_type == NodeType.STAR


## Returns true if this is a barycenter (internal) node.
## @return: True if BARYCENTER type.
func is_barycenter() -> bool:
	return node_type == NodeType.BARYCENTER


## Returns all star IDs contained within this node (recursively).
## @return: Array of star IDs.
func get_all_star_ids() -> Array[String]:
	var result: Array[String] = []
	if is_star():
		result.append(star_id)
	else:
		for child in children:
			result.append_array(child.get_all_star_ids())
	return result


## Returns the count of stars within this node.
## @return: Number of stars.
func get_star_count() -> int:
	if is_star():
		return 1
	var count: int = 0
	for child in children:
		count += child.get_star_count()
	return count


## Returns the depth of this subtree.
## @return: Maximum depth (1 for leaf, more for nested structures).
func get_depth() -> int:
	if is_star():
		return 1
	var max_child_depth: int = 0
	for child in children:
		max_child_depth = maxi(max_child_depth, child.get_depth())
	return max_child_depth + 1


## Finds a node by ID within this subtree.
## @param target_id: The ID to search for.
## @return: The node with that ID, or null if not found.
func find_node(target_id: String) -> HierarchyNode:
	if id == target_id:
		return self
	if is_barycenter():
		for child in children:
			var found: HierarchyNode = child.find_node(target_id)
			if found != null:
				return found
	return null


## Converts to dictionary for serialization.
## @return: Dictionary representation.
func to_dict() -> Dictionary:
	var data: Dictionary = {
		"id": id,
		"node_type": "star" if node_type == NodeType.STAR else "barycenter",
	}
	
	if is_star():
		data["star_id"] = star_id
	else:
		var child_dicts: Array[Dictionary] = []
		for child in children:
			child_dicts.append(child.to_dict())
		data["children"] = child_dicts
		data["separation_m"] = separation_m
		data["eccentricity"] = eccentricity
		data["orbital_period_s"] = orbital_period_s
	
	return data


## Creates a HierarchyNode from a dictionary.
## @param data: Dictionary to parse.
## @return: A new HierarchyNode, or null if invalid.
static func from_dict(data: Dictionary) -> HierarchyNode:
	if data.is_empty():
		return null
	
	var type_str: String = data.get("node_type", "star") as String
	var p_node_type: NodeType = NodeType.STAR if type_str == "star" else NodeType.BARYCENTER
	
	var node: HierarchyNode = HierarchyNode.new(
		data.get("id", "") as String,
		p_node_type
	)
	
	if node.is_star():
		node.star_id = data.get("star_id", "") as String
	else:
		node.separation_m = data.get("separation_m", 0.0) as float
		node.eccentricity = data.get("eccentricity", 0.0) as float
		node.orbital_period_s = data.get("orbital_period_s", 0.0) as float
		
		var child_dicts: Array = data.get("children", []) as Array
		for child_data in child_dicts:
			var child: HierarchyNode = HierarchyNode.from_dict(child_data as Dictionary)
			if child != null:
				node.children.append(child)
	
	return node
