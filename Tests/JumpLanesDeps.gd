## Loads jump lanes domain scripts so class_name types are registered before test scripts.
## Preload this in RunTestsHeadless and TestScene before jump lanes tests run.
extends RefCounted

const _jump_lane_system: GDScript = preload("res://src/domain/jumplanes/JumpLaneSystem.gd")
const _jump_lane_connection: GDScript = preload("res://src/domain/jumplanes/JumpLaneConnection.gd")
const _jump_lane_region: GDScript = preload("res://src/domain/jumplanes/JumpLaneRegion.gd")
const _jump_lane_result: GDScript = preload("res://src/domain/jumplanes/JumpLaneResult.gd")
const _jump_lane_cluster_connector: GDScript = preload("res://src/domain/jumplanes/JumpLaneClusterConnector.gd")
const _jump_lane_calculator: GDScript = preload("res://src/domain/jumplanes/JumpLaneCalculator.gd")
